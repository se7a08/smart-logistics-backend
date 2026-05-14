namespace SmartLogistics.Application.DTOs.Auth
{
    // The data returned to the client after a successful authentication
    public record AuthResponse(
        Guid UserId,
        string FullName,
        string Email,
        string Role,
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiry
    );
}