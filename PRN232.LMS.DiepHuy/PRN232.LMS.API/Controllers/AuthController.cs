using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers
{
    /// <summary>
    /// 🔴 YÊU CẦU 9: Authentication API Controller
    /// POST /api/auth/login - Login endpoint
    /// POST /api/auth/refresh-token - Refresh token endpoint
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login endpoint - returns access token and refresh token
        /// Required endpoint: POST /api/auth/login
        /// </summary>
        /// <param name="request">Login credentials (username, password)</param>
        /// <returns>JWT tokens with expiration time</returns>
        [HttpPost("login")]
        [Produces("application/json", "application/xml")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.CreateFailure("Invalid request data"));

                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    return BadRequest(ApiResponse<string>.CreateFailure("Username and password are required"));

                var result = await _authService.LoginAsync(request.Username, request.Password);
                if (result == null)
                    return Unauthorized(ApiResponse<string>.CreateFailure("Invalid username or password"));

                return Ok(ApiResponse<Services.Interfaces.LoginResponse>.CreateSuccess(result, "Login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, ApiResponse<string>.CreateFailure("An error occurred during login", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Refresh token endpoint - returns new access token
        /// Required endpoint: POST /api/auth/refresh-token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>New access token</returns>
        [HttpPost("refresh-token")]
        [Produces("application/json", "application/xml")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrEmpty(request.RefreshToken))
                    return BadRequest(ApiResponse<string>.CreateFailure("Refresh token is required"));

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                if (result == null)
                    return Unauthorized(ApiResponse<string>.CreateFailure("Invalid refresh token"));

                return Ok(ApiResponse<Services.Interfaces.LoginResponse>.CreateSuccess(result, "Token refreshed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, ApiResponse<string>.CreateFailure("An error occurred during token refresh", new List<string> { ex.Message }));
            }
        }
    }
}
