using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using todo_app.Models;
using todo_app.Models.Dtos.Auth;
using todo_app.Models.Entities;

namespace todo_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerOperation(Summary = "Đăng nhập")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                return Unauthorized("Thông tin xác thực không hợp lệ");
            }

            // access token
            var accesstoken = GenerateJwtToken(user);

            // refresh token
            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();

            return Ok(new
            {
                accesstoken,
                refreshToken,
                user = new
                {
                    user.Username
                }
            });

        }

        [AllowAnonymous]
        [HttpPost("register")]
        [SwaggerOperation(Summary = "Đăng ký")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Email phải có định dạng @gmail.com");
            }

            if (_context.Users.Any(u => u.Username == model.Username || u.Email == model.Email))
            {
                return BadRequest("Tên người dùng hoặc email đã tồn tại.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "Người dùng đã đăng ký thành công" });
        }


        [AllowAnonymous]
        [HttpPost("/api/token/refresh")]
        [SwaggerOperation(Summary = "Lấy access token mới bằng refresh token")]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.RefreshToken == refreshToken &&
                u.RefreshTokenExpiryTime > DateTime.UtcNow);

            if (user == null)
            {
                return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }


        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Thời gian hết hạn của accessToken
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
