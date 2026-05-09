using SmartLogistics.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.DTOs.Auth
{
    public record RegisterRequest(
        string FullName,
        string Email,
        string Password,
        string PhoneNumber,
        UserRole Role,
        string? LicenseNumber,
        string? VehiclePlate
    );

    public record LoginRequest(
        string Email,
        string Password,
        string? FcmToken
    );

    public record RefreshTokenRequest(string RefreshToken);

    public record AuthResponse(
        Guid UserId,
        string FullName,
        string Email,
        string Role,
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiry
    );

    public record UserDto(
        Guid Id,
        string FullName,
        string Email,
        string PhoneNumber,
        string Role,
        bool IsActive,
        string? LicenseNumber,
        string? VehiclePlate,
        DateTime CreatedAt
    );
}

