using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using todo_app.Models;
using todo_app.Models.Dtos.Todo;
using todo_app.Models.Entities;

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
        [SwaggerOperation(Summary = "Lấy danh sách list")]
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
        [SwaggerOperation(Summary = "Thêm list mới")]
        public async Task<IActionResult> CreateList([FromBody] CreateListModel model)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { message = "Bạn phải đăng nhập để tạo danh sách." });
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
                message = "Danh sách được tạo thành công",
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
        [SwaggerOperation(Summary = "Sửa list")]
        public async Task<IActionResult> UpdateList(int id, [FromBody] CreateListModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);
            if (list == null)
            {
                return NotFound(new { 
                    message = "Không tìm thấy danh sách hoặc quyền truy cập bị từ chối." 
                });
            }

            list.Name = model.Name;

            await _context.SaveChangesAsync();
            return Ok(new { 
                message = "Danh sách được cập nhật thành công.",
                list = new
                { 
                    list.Id, 
                    list.Name 
                }
            });
        }

        //Xóa list theo id
        [HttpDelete("lists/{id}")]
        [SwaggerOperation(Summary = "Xóa list")]
        public async Task<IActionResult> DeleteList(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);
            if (list == null)
            {
                return NotFound(new { 
                    message = "Không tìm thấy danh sách hoặc quyền truy cập bị từ chối."
                });
            }

            // Xóa luôn các todo liên quan nếu cần
            var relatedTodos = _context.Todos.Where(t => t.ListId == id);
            _context.Todos.RemoveRange(relatedTodos);

            _context.Lists.Remove(list);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Danh sách đã được xóa thành công." });
        }

        // Lấy tất cả todo của 1 user
        [HttpGet("items")]
        [SwaggerOperation(Summary = "Lấy tất cả todo")]
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
        [SwaggerOperation(Summary = "Thêm một todo")]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodoModel model)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { message = "Bạn phải đăng nhập để tạo việc cần làm." });
            }
            // Lấy userId từ token
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Kiểm tra xem List có tồn tại và thuộc về người dùng không
            var list = await _context.Lists
                .FirstOrDefaultAsync(l => l.Id == model.ListId && l.UserId == userId);
            if (list == null)
            {
                return BadRequest("Không tìm thấy danh sách hoặc bạn không có quyền truy cập.");
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
                message = "Todo được tạo thành công",
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
        [SwaggerOperation(Summary = "Cập nhật todo")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] CreateTodoModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null)
            {
                return NotFound(new { message = "Không tìm thấy Todo hoặc quyền truy cập bị từ chối." });
            }

            // Optional: kiểm tra list có thuộc user không 
            var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == model.ListId && l.UserId == userId);
            if (list == null)
            {
                return BadRequest("\r\nKhông tìm thấy danh sách hoặc quyền truy cập bị từ chối.");
            }

            todo.Title = model.Title;
            todo.Description = model.Description;
            todo.DueDate = model.DueDate;
            todo.Priority = model.Priority;
            todo.ListId = model.ListId;
            todo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Todo đã cập nhật thành công.",
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
        [SwaggerOperation(Summary = "Xóa một todo")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null)
            {
                return NotFound(new { message = "Không tìm thấy Todo hoặc quyền truy cập bị từ chối." });
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Todo đã xóa thành công." });
        }


        // Lấy danh sách thẻ của 1 todo
        [HttpGet("{id}/tags")]
        [SwaggerOperation(Summary = "Lấy danh sách thẻ của todo")]
        public async Task<IActionResult> GetTagsForTodo(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null)
            {
                return NotFound(new { message = "Không tìm thấy todo hoặc quyền truy cập bị từ chối" });
            }

            var tags = await _context.TodoTags
                .Where(tt => tt.TodoId == id)
                .Select(tt => tt.Tag)
                .ToListAsync();

            return Ok(tags);
        }


        // Gán tag vào todo
        [HttpPost("assign-tags")]
        [SwaggerOperation(Summary = "Gán tag vào todo")]
        public async Task<IActionResult> AssignTagsToTodo([FromBody] AssignTagsModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Kiểm tra todo thuộc user không
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == model.TodoId && t.UserId == userId);
            if (todo == null)
            {
                return NotFound(new { message = "Không tìm thấy Todo hoặc quyền truy cập bị từ chối." });
            }

            // Gán tag mới (nếu chưa tồn tại)
            foreach (var tagId in model.TagIds.Distinct())
            {
                // Kiểm tra tag có tồn tại không
                var tagExists = await _context.Tags.AnyAsync(t => t.Id == tagId);
                var alreadyTagged = await _context.TodoTags.AnyAsync(tt => tt.TodoId == model.TodoId && tt.TagId == tagId);

                if (tagExists && !alreadyTagged)
                {
                    _context.TodoTags.Add(new TodoTag
                    {
                        TodoId = model.TodoId,
                        TagId = tagId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { 
                message = "Đã gán thẻ cho Todo thành công." ,
            });
        }

        [HttpDelete("{todoId}/tags/{tagId}")]
        [SwaggerOperation(Summary = "Xóa tag ra khỏi Todo")]
        public async Task<IActionResult> RemoveTagFromTodo(int todoId, int tagId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Kiểm tra xem todo có thuộc về user không
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);
            if (todo == null)
            {
                return NotFound(new { 
                    message = "Không tìm thấy Todo hoặc quyền truy cập bị từ chối." 
                });
            }

            // Tìm quan hệ tag
            var todoTag = await _context.TodoTags
                .FirstOrDefaultAsync(tt => tt.TodoId == todoId && tt.TagId == tagId);

            if (todoTag == null)
            {
                return NotFound(new { 
                    message = "Thẻ chưa được gán cho Todo này." 
                });
            }

            _context.TodoTags.Remove(todoTag);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Đã xóa thẻ khỏi Todo thành công." 
            });
        }

    }
}
