using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.DTOs.Auth;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.Features.Auth.Commands
{
    // ─── Register ──────────────────────────────────────────────────────────────

    public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtService _jwt;

        public RegisterCommandHandler(IUnitOfWork uow, IPasswordHasher hasher, IJwtService jwt)
        {
            _uow = uow;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken ct)
        {
            var req = command.Request;
            var repo = _uow.Repository<User>();

            if (await repo.AnyAsync(u => u.Email == req.Email, ct))
                throw new BusinessRuleException("Email is already registered.");

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email.ToLower(),
                PasswordHash = _hasher.Hash(req.Password),
                PhoneNumber = req.PhoneNumber,
                Role = req.Role,
                LicenseNumber = req.LicenseNumber,
                VehiclePlate = req.VehiclePlate
            };

            await repo.AddAsync(user, ct);

            // Generate refresh token
            var refreshToken = CreateRefreshToken(user.Id);
            await _uow.Repository<RefreshToken>().AddAsync(refreshToken, ct);
            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());

            return new AuthResponse(user.Id, user.FullName, user.Email, user.Role.ToString(),
                accessToken, refreshToken.Token, DateTime.UtcNow.AddHours(1));
        }

        private static RefreshToken CreateRefreshToken(Guid userId) => new()
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
    }

    // ─── Login ─────────────────────────────────────────────────────────────────

    public record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtService _jwt;

        public LoginCommandHandler(IUnitOfWork uow, IPasswordHasher hasher, IJwtService jwt)
        {
            _uow = uow;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken ct)
        {
            var req = command.Request;
            var user = await _uow.Repository<User>()
                .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower() && !u.IsDeleted, ct)
                ?? throw new UnauthorizedException("Invalid email or password.");

            if (!_hasher.Verify(req.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedException("Account is deactivated.");

            // Update FCM token if provided
            if (!string.IsNullOrEmpty(req.FcmToken) && user.FcmToken != req.FcmToken)
            {
                user.FcmToken = req.FcmToken;
                _uow.Repository<User>().Update(user);
            }

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            await _uow.Repository<RefreshToken>().AddAsync(refreshToken, ct);
            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());

            return new AuthResponse(user.Id, user.FullName, user.Email, user.Role.ToString(),
                accessToken, refreshToken.Token, DateTime.UtcNow.AddHours(1));
        }
    }

    // ─── Refresh Token ─────────────────────────────────────────────────────────

    public record RefreshTokenCommand(string Token) : IRequest<AuthResponse>;

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly IJwtService _jwt;

        public RefreshTokenCommandHandler(IUnitOfWork uow, IJwtService jwt)
        {
            _uow = uow;
            _jwt = jwt;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken ct)
        {
            var existing = await _uow.Repository<RefreshToken>()
                .FirstOrDefaultAsync(t => t.Token == command.Token, ct)
                ?? throw new UnauthorizedException("Invalid refresh token.");

            if (!existing.IsActive)
                throw new UnauthorizedException("Refresh token is expired or revoked.");

            var user = await _uow.Repository<User>()
                .GetByIdAsync(existing.UserId, ct)
                ?? throw new UnauthorizedException("User not found.");

            // Rotate refresh token
            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            existing.RevokedAt = DateTime.UtcNow;
            existing.ReplacedByToken = newRefreshToken.Token;
            existing.RevokedReason = "Replaced by new token";

            _uow.Repository<RefreshToken>().Update(existing);
            await _uow.Repository<RefreshToken>().AddAsync(newRefreshToken, ct);
            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());

            return new AuthResponse(user.Id, user.FullName, user.Email, user.Role.ToString(),
                accessToken, newRefreshToken.Token, DateTime.UtcNow.AddHours(1));
        }
    }

    // ─── Logout ────────────────────────────────────────────────────────────────

    public record LogoutCommand(string RefreshToken) : IRequest<bool>;

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly IUnitOfWork _uow;

        public LogoutCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<bool> Handle(LogoutCommand command, CancellationToken ct)
        {
            var token = await _uow.Repository<RefreshToken>()
                .FirstOrDefaultAsync(t => t.Token == command.RefreshToken, ct);

            if (token is null || !token.IsActive)
                return false;

            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "User logged out";
            _uow.Repository<RefreshToken>().Update(token);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}

