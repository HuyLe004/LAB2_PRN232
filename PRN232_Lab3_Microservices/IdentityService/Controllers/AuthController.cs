using IdentityService.Data;
using IdentityService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IdentityDbContext _context;

        // Must match the values in Program.cs
        private const string JwtSecret = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForLab3";
        private const string JwtIssuer = "PRN232_Lab3";
        private const string JwtAudience = "PRN232_Lab3_Users";

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

        // 2. API Đăng nhập (Login) - generate JWT thật
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginInfo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                u => u.Username == loginInfo.Username && u.PasswordHash == loginInfo.PasswordHash);

            if (user == null)
                return Unauthorized("Sai tài khoản hoặc mật khẩu.");

            var token = CreateJwtToken(user);

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                username = user.Username,
                role = user.Role,
                token
            });
        }

        private static string CreateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(60);

            var token = new JwtSecurityToken(
                issuer: JwtIssuer,
                audience: JwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
