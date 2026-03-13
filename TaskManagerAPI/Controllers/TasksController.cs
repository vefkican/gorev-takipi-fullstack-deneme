using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: api/tasks?search=&isCompleted=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks([FromQuery] string? search, [FromQuery] bool? isCompleted)
        {
            var userId = GetUserId();
            var query = _context.Tasks
                .Where(t => t.UserId == userId && t.DeletedAt == null); // Silinmişleri getirme

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Title.Contains(search) ||
                    (t.Description != null && t.Description.Contains(search)));

            if (isCompleted.HasValue)
                query = query.Where(t => t.IsCompleted == isCompleted.Value);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        // GET: api/tasks/1
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var userId = GetUserId();
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && t.DeletedAt == null);
            if (task == null) return NotFound();
            return task;
        }

        // POST: api/tasks
        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(CreateTaskDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate.HasValue
                    ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
                    : null,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UserId = GetUserId()
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // PUT: api/tasks/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem task)
        {
            var userId = GetUserId();
            var existing = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && t.DeletedAt == null);
            if (existing == null) return NotFound();

            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.IsCompleted = task.IsCompleted;
            existing.DueDate = task.DueDate.HasValue
                ? DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc)
                : null;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tasks/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetUserId();
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && t.DeletedAt == null);
            if (task == null) return NotFound();

            task.DeletedAt = DateTime.UtcNow; // Silmek yerine tarih atıyoruz
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}