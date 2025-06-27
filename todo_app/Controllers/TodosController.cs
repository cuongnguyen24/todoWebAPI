using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using todo_app.Models;

namespace todo_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TodosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodoModel model)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { message = "You must be logged in to create a todo." });
            }
            // Lấy userId từ token
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Kiểm tra xem List có tồn tại và thuộc về người dùng không
            var list = await _context.Lists
                .FirstOrDefaultAsync(l => l.Id == model.ListId && l.UserId == userId);
            if (list == null)
            {
                return BadRequest("List not found or you do not have access.");
            }

            // Tạo mới Todo
            var todo = new Todo
            {
                UserId = userId,
                ListId = model.ListId,
                Title = model.Title,
                Description = model.Description,
                IsCompleted = false, // Mặc định là chưa hoàn thành
                DueDate = model.DueDate,
                Priority = model.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Todo created successfully", todoId = todo.Id });
        }

    }
}
