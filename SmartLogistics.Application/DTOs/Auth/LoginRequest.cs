namespace SmartLogistics.Application.DTOs.Auth
{
    // Data required for the user to log in
    public record LoginRequest(
        string Email,
        string Password,
        string? FcmToken // Optional: Used for Push Notifications (Firebase)
    );
}