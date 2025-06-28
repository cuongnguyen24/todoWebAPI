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

        // Lấy danh sách list
        [HttpGet("lists")]
        public async Task<IActionResult> GetLists()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var lists = await _context.Lists
                .Where(list => list.UserId == userId)
                .Select(list => new
                {
                    list.Id,
                    list.Name,
                    list.CreatedAt
                })
                .ToListAsync();

            return Ok(lists);
        }

        // Thêm 1 list mới
        [HttpPost("lists")]
        public async Task<IActionResult> CreateList([FromBody] CreateListModel model)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { message = "You must be logged in to create a list." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var list = new List
            {
                UserId = userId,
                Name = model.Name,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "List created successfully",
                list = new
                {
                    list.Id,
                    list.Name,
                    list.CreatedAt
                }
            });
        }

        // Sửa list theo id
        [HttpPut("lists/{id}")]
        public async Task<IActionResult> UpdateList(int id, [FromBody] CreateListModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);
            if (list == null)
            {
                return NotFound(new { message = "List not found or access denied." });
            }

            list.Name = model.Name;

            await _context.SaveChangesAsync();
            return Ok(new { 
                message = "List updated successfully.",
                list = new
                { 
                    list.Id, 
                    list.Name 
                }
            });
        }

        //Xóa list theo id
        [HttpDelete("lists/{id}")]
        public async Task<IActionResult> DeleteList(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);
            if (list == null)
            {
                return NotFound(new { message = "List not found or access denied." });
            }

            // Xóa luôn các todo liên quan nếu cần
            var relatedTodos = _context.Todos.Where(t => t.ListId == id);
            _context.Todos.RemoveRange(relatedTodos);

            _context.Lists.Remove(list);
            await _context.SaveChangesAsync();

            return Ok(new { message = "List deleted successfully." });
        }

        // Lấy tất cả todo của 1 user
        [HttpGet("items")]
        public async Task<IActionResult> GetTodos()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var todos = await _context.Todos
                .Where(t => t.UserId == userId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.IsCompleted,
                    t.DueDate,
                    t.Priority,
                    t.ListId,
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();

            return Ok(todos);
        }

        // Thêm một todo
        [HttpPost("items")]
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

            return Ok(new { 
                message = "Todo created successfully",
                todo = new
                {
                    todo.Id,
                    todo.Title,
                    todo.Description,
                    todo.DueDate,
                    todo.Priority,
                    todo.IsCompleted
                }
            });
        }

        // Cập nhật todo theo id
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] CreateTodoModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null)
            {
                return NotFound(new { message = "Todo not found or access denied." });
            }

            // Optional: kiểm tra list có thuộc user không 
            var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == model.ListId && l.UserId == userId);
            if (list == null)
            {
                return BadRequest("List not found or access denied.");
            }

            todo.Title = model.Title;
            todo.Description = model.Description;
            todo.DueDate = model.DueDate;
            todo.Priority = model.Priority;
            todo.ListId = model.ListId;
            todo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Todo updated successfully." ,
                todo = new
                {
                    todo.Id,
                    todo.Title,
                    todo.Description,
                    todo.IsCompleted,
                    todo.DueDate,
                    todo.Priority,
                    todo.ListId,
                    todo.CreatedAt,
                    todo.UpdatedAt
                }
            
            });
        }


        // Xóa một todo
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null)
            {
                return NotFound(new { message = "Todo not found or access denied." });
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Todo deleted successfully." });
        }


    }
}
