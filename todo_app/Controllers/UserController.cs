using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using todo_app.Models;
using todo_app.Models.Dtos.User;

namespace todo_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        [SwaggerOperation(Summary = "Lấy thông tin cá nhân")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                createdAt = user.CreatedAt,
                updatedAt = user.UpdatedAt
            });
        }


        [HttpPut("password")]
        [SwaggerOperation(Summary = "Đổi mật khẩu")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "Người dùng không tồn tại." });

            // So sánh mật khẩu hiện tại
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "Mật khẩu hiện tại không đúng." });

            // Mã hoá mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        [HttpPut("profile")]
        [SwaggerOperation(Summary = "Cập nhật thông tin cá nhân")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "Người dùng không tồn tại." });

            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Email phải có định dạng @gmail.com." });
            }

            // Kiểm tra email đã tồn tại ở người dùng khác
            var emailTaken = await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != userId);
            if (emailTaken)
            {
                return BadRequest(new { message = "Email đã được sử dụng bởi người khác." });
            }

            user.Email = model.Email;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật thông tin thành công.",
                user = new
                {
                    user.Username,
                    user.Email
                }
            });
        }

    }
}
