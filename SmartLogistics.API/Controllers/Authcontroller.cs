using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Auth;
using SmartLogistics.Application.Features.Auth.Commands;

namespace SmartLogistics.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Handles new user registration
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RegisterCommand(request), ct);

            var response = ApiResponse<AuthResponse>.Created(result, "Account created successfully.");
            return StatusCode(201, response);
        }

        // Handles user login and returns JWT tokens
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new LoginCommand(request), ct);

            var response = ApiResponse<AuthResponse>.Ok(result, "Welcome back! Login successful.");
            return Ok(response);
        }

        // Exchanges an expired access token for a new one using a refresh token
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);

            var response = ApiResponse<AuthResponse>.Ok(result, "Token refreshed successfully.");
            return Ok(response);
        }

        // Revokes the refresh token and logs the user out
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            await _mediator.Send(new LogoutCommand(request.RefreshToken), ct);

            var response = ApiResponse.Ok("Session closed. Logged out successfully.");
            return Ok(response);
        }
    }
}