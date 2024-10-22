using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;


namespace TaskManagement.Controllers
{
    [Authorize]
    public class BoardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public BoardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var tasks = await _db.TaskItems
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            ViewBag.TodoTasks = tasks.Where(t => t.Status == "todo").ToList();
            ViewBag.DoingTasks = tasks.Where(t => t.Status == "doing").ToList();
            ViewBag.DoneTasks = tasks.Where(t => t.Status == "done").ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int id, string newStatus)
        {
            var task = await _db.TaskItems.FindAsync(id);
            if (task == null) return NotFound();
            task.Status = newStatus;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
