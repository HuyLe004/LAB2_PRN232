using PRN232.LMS.API.Models;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces
{
    /// <summary>
    /// 🔴 YÊU CẦU 9: Authentication Service Interface
    /// Xử lý JWT token generation, validation, và refresh token
    /// </summary>
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string username, string password);
        Task<LoginResponse?> RefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto?> GetUserFromTokenAsync(string token);
    }
}
