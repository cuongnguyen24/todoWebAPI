using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using todo_app.Models;
using todo_app.Models.Dtos.Tag;
using todo_app.Models.Entities;

namespace todo_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TagsController(AppDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả các tags
        [HttpGet]
        [SwaggerOperation(Summary = "Lấy tất cả các tag")]
        public async Task<IActionResult> GetTags()
        {
            var tags = await _context.Tags
                .Select(t => new { t.Id, t.Name })
                .ToListAsync();

            return Ok(tags);
        }

        // Tạo tag
        [HttpPost]
        [SwaggerOperation(Summary = "Tạo tag")]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return BadRequest(new { message = "Tên thẻ là bắt buộc" });
            }

            var exists = await _context.Tags.AnyAsync(t => t.Name == model.Name);
            if (exists)
            {
                return BadRequest(new { message = "Tên thẻ đã tồn tại" });
            }

            var tag = new Tag
            {
                Name = model.Name
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Tạo thẻ thành công", 
                tag = new
                {
                    tag.Id,
                    tag.Name
                }
            });
        }

        // Sửa thẻ
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Sửa tag")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] CreateTagModel model)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound(new { message = "Thẻ không tồn tại." });
            }

            tag.Name = model.Name;
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Cập nhật thẻ thành công",
                tag = new
                {
                    tag.Id,
                    tag.Name
                }
            });
        }

        // Xóa tag
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Xóa tag")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound(new { message = "Thẻ không tồn tại." });
            }

            // Xóa các liên kết với todo (nếu có)
            var todoTags = _context.TodoTags.Where(tt => tt.TagId == id);
            _context.TodoTags.RemoveRange(todoTags);

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa tag thành công" });
        }

    }
}
