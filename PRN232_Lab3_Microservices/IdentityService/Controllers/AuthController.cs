using Microsoft.AspNetCore.Mvc;
using IdentityService.Data;
using IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IdentityDbContext _context;

        public AuthController(IdentityDbContext context)
        {
            _context = context;
        }

        // 1. API Đăng ký tài khoản (Register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return BadRequest("Tên tài khoản đã tồn tại.");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đăng ký tài khoản thành công!", userId = user.UserId });
        }

        // 2. API Đăng nhập giả lập (Login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginInfo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginInfo.Username && u.PasswordHash == loginInfo.PasswordHash);
            if (user == null)
                return Unauthorized("Sai tài khoản hoặc mật khẩu.");

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                username = user.Username,
                role = user.Role,
                token = "FAKE_JWT_TOKEN_FOR_LAB3_REQUIREMENT"
            });
        }
    }
}