using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.DTOs.Auth;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Auth.Commands
{
    // --- Register Command ---
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
            var userRepo = _uow.Repository<User>();

            // Ensure email uniqueness
            if (await userRepo.AnyAsync(u => u.Email == req.Email.ToLower(), ct))
            {
                throw new BusinessRuleException("The provided email address is already in use.");
            }

            var newUser = new User
            {
                FullName = req.FullName,
                Email = req.Email.ToLower(),
                PasswordHash = _hasher.Hash(req.Password),
                PhoneNumber = req.PhoneNumber,
                Role = req.Role,
                LicenseNumber = req.LicenseNumber,
                VehiclePlate = req.VehiclePlate,
                IsActive = true
            };

            await userRepo.AddAsync(newUser, ct);

            // Setup the initial refresh token for the new user
            var refreshToken = CreateNewRefreshToken(newUser.Id);
            await _uow.Repository<RefreshToken>().AddAsync(refreshToken, ct);
            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.GenerateAccessToken(newUser.Id, newUser.Email, newUser.Role.ToString());

            return new AuthResponse(
                newUser.Id,
                newUser.FullName,
                newUser.Email,
                newUser.Role.ToString(),
                accessToken,
                refreshToken.Token,
                DateTime.UtcNow.AddHours(1)
            );
        }

        private static RefreshToken CreateNewRefreshToken(Guid userId) => new()
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
    }

    // --- Login Command ---
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
                .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower() && !u.IsDeleted, ct);

            if (user == null || !_hasher.Verify(req.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Incorrect email or password.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedException("This account has been deactivated. Please contact support.");
            }

            // Sync FCM Token for mobile notifications if provided
            if (!string.IsNullOrEmpty(req.FcmToken) && user.FcmToken != req.FcmToken)
            {
                user.FcmToken = req.FcmToken;
                _uow.Repository<User>().Update(user);
            }

            // Create a new session token
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            await _uow.Repository<RefreshToken>().AddAsync(refreshToken, ct);
            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());

            return new AuthResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Role.ToString(),
                accessToken,
                refreshToken.Token,
                DateTime.UtcNow.AddHours(1)
            );
        }
    }

    // --- Refresh Token Command ---
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
            var existingToken = await _uow.Repository<RefreshToken>()
                .FirstOrDefaultAsync(t => t.Token == command.Token, ct);

            if (existingToken == null || !existingToken.IsActive)
            {
                throw new UnauthorizedException("Session expired or invalid token.");
            }

            var user = await _uow.Repository<User>().GetByIdAsync(existingToken.UserId, ct);
            if (user == null) throw new UnauthorizedException("User no longer exists.");

            // Token Rotation: Invalidate old token and issue a new one
            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.ReplacedByToken = newRefreshToken.Token;
            existingToken.RevokedReason = "Token rotated via refresh request";

            _uow.Repository<RefreshToken>().Update(existingToken);
            await _uow.Repository<RefreshToken>().AddAsync(newRefreshToken, ct);
            await _uow.SaveChangesAsync(ct);

            var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());

            return new AuthResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Role.ToString(),
                accessToken,
                newRefreshToken.Token,
                DateTime.UtcNow.AddHours(1)
            );
        }
    }

    // --- Logout Command ---
    public record LogoutCommand(string RefreshToken) : IRequest<bool>;

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly IUnitOfWork _uow;

        public LogoutCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<bool> Handle(LogoutCommand command, CancellationToken ct)
        {
            var token = await _uow.Repository<RefreshToken>()
                .FirstOrDefaultAsync(t => t.Token == command.RefreshToken, ct);

            if (token == null || !token.IsActive) return false;

            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Manual logout by user";

            _uow.Repository<RefreshToken>().Update(token);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}