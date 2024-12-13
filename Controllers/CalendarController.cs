using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;



namespace TaskManagement.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalendarController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var tasks = await _db.TaskItems
                .Where(t => t.UserId == userId)
                .Select(t => new {
                    t.Id,
                    t.Title,
                    t.DueDate,
                    t.ReminderDate,
                    t.Status,
                    t.Priority
                })
                .ToListAsync();
            
            ViewData["AntiforgeryToken"] = HttpContext.RequestServices.GetService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>()?.GetAndStoreTokens(HttpContext).RequestToken;
            
            return View(tasks);
        }
        
        
        [HttpPost]
        public async Task<IActionResult> UpdateTaskDate(int id, DateTime newDueDate)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    
            if (task == null)
            {
                return NotFound("Task not found or you do not have permission to edit this task.");
            }

            task.DueDate = newDueDate;
            await _db.SaveChangesAsync();
    
            return Ok();
        }
    }
}