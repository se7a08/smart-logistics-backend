using Microsoft.AspNetCore.Mvc;
using global::SmartLogistics.Application.Common.Models;
using global::SmartLogistics.Application.DTOs.Auth;
using global::SmartLogistics.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Auth;
using SmartLogistics.Application.Features.Auth.Commands;

namespace SmartLogistics.API.Controllers
{
    /// <summary>
    /// Authentication controller: register, login, token refresh, logout.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;

        /// <summary>Register a new user (Admin or Driver).</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RegisterCommand(request), ct);
            return StatusCode(201, ApiResponse<AuthResponse>.Created(result, "Registration successful."));
        }

        /// <summary>Login and obtain JWT access + refresh tokens.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new LoginCommand(request), ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
        }

        /// <summary>Exchange a refresh token for a new access + refresh token pair.</summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result));
        }

        /// <summary>Revoke the current refresh token to log out.</summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            await _mediator.Send(new LogoutCommand(request.RefreshToken), ct);
            return Ok(ApiResponse.Ok("Logged out successfully."));
        }
    }
}
