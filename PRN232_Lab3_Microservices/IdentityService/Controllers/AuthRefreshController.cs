using IdentityService.Data;
using IdentityService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthRefreshController : ControllerBase
    {
        private readonly IdentityDbContext _context;

        // Must match Program.cs
        private const string JwtSecret = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForLab3";
        private const string JwtIssuer = "PRN232_Lab3";
        private const string JwtAudience = "PRN232_Lab3_Users";

        public AuthRefreshController(IdentityDbContext context)
        {
            _context = context;
        }

        public class RefreshTokenRequest
        {
            public string RefreshToken { get; set; } = null!;
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
                return BadRequest("Refresh token is required.");

            var refresh = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (refresh == null)
                return Unauthorized("Invalid refresh token.");

            if (refresh.Revoked)
                return Unauthorized("Refresh token has been revoked.");

            if (DateTime.UtcNow >= refresh.ExpiresAt)
                return Unauthorized("Refresh token expired.");

            // rotate: revoke old + issue new
            refresh.Revoked = true;

            var newRefreshToken = new RefreshToken
            {
                UserId = refresh.UserId,
                Token = GenerateRefreshToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Revoked = false,
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            var accessToken = CreateJwtToken(refresh.User);

            return Ok(new
            {
                accessToken,
                refreshToken = newRefreshToken.Token,
                expiresIn = 60 * 60
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

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}

