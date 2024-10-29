using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: Task
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var tasks = await _db.TaskItems
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return View(tasks);
        }

        // GET: Task/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _db.TaskItems.FindAsync(id);
            if (taskItem == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (taskItem.UserId != userId) return Forbid();

            return View(taskItem);
        }

        // GET: Task/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem taskItem, int boardId)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized("User must be logged in to create tasks.");
                }
                
                var board = await _db.Boards
                    .Include(b => b.Tasks)
                    .FirstOrDefaultAsync(b => b.Id == boardId && b.UserId == userId);

                if (board == null)
                {
                    return NotFound("Board not found or you do not have permission to add tasks to this board.");
                }

                taskItem.BoardId = board.Id;
                taskItem.UserId = userId;   // Görevi oluşturan kullanıcıyı ayarla
                _db.TaskItems.Add(taskItem);
                await _db.SaveChangesAsync();

                return RedirectToAction("Index", "Board");
            }
            return View(taskItem);
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _db.TaskItems.FindAsync(id);
            if (taskItem == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (taskItem.UserId != userId) return Forbid();

            return View(taskItem);
        }

        // POST: Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var existingTask = await _db.TaskItems.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (existingTask == null) return Forbid();

                try
                {
                    _db.TaskItems.Update(taskItem);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Index", "Board");
            }
            return View(taskItem);
        }

        // GET: Task/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _db.TaskItems.FindAsync(id);
            if (taskItem == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (taskItem.UserId != userId) return Forbid();

            return View(taskItem);
        }

        // POST: Task/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _db.TaskItems.FindAsync(id);
            if (taskItem == null) return NotFound("Task not found.");

            var userId = _userManager.GetUserId(User);
            if (taskItem.UserId != userId) return Forbid();

            _db.TaskItems.Remove(taskItem);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool TaskItemExists(int id) => _db.TaskItems.Any(e => e.Id == id);
        
        // Update Task Status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var taskItem = await _db.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound("Task not found.");
            }

            var userId = _userManager.GetUserId(User);
            if (taskItem.UserId != userId)
            {
                return Forbid();
            }

            taskItem.Status = newStatus;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}