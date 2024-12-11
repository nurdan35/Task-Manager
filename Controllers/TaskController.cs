using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using System.Globalization;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAntiforgery _antiforgery;

        public TaskController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            IAntiforgery antiforgery)
        {
            _db = db;
            _userManager = userManager;
            _antiforgery = antiforgery;
        }

        // GET: Task
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var tasks = await _db.TaskItems
                .Where(t => t.UserId == userId)
                .ToListAsync();
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            ViewData["RequestVerificationToken"] = tokens.RequestToken;

            return View(tasks);
        }
        [HttpGet]
        public async Task<IActionResult> SearchTasks(string query)
        {
            var userId = _userManager.GetUserId(User);

            // Sett query til tom streng om den er null, for enkelhets skyld
            if (string.IsNullOrWhiteSpace(query))
                query = "";

            // Gjør søket case-insensitive ved å bruke ToLower()
            string lowerQuery = query.ToLower();

            // Prøv å parse dato i formatet yyyy-MM-dd (f.eks. 2023-12-10)
            bool isDate = DateTime.TryParseExact(
                query, 
                "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out DateTime parsedDate
            );

            var filteredTasks = await _db.TaskItems
                .Where(t => t.UserId == userId &&
                            (lowerQuery == "" ||
                             t.Title.ToLower().Contains(lowerQuery) ||
                             t.Description.ToLower().Contains(lowerQuery) ||
                             (isDate && t.DueDate.Date == parsedDate.Date)))
                .Select(t => new {
                    id = t.Id,
                    title = t.Title,
                    description = t.Description,
                    dueDate = t.DueDate,
                    status = t.Status
                })
                .ToListAsync();

            return Json(filteredTasks);
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
        public IActionResult Create(int boardId)
        {
            ViewBag.BoardId = boardId;
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
                taskItem.UserId = userId; // Görevi oluşturan kullanıcıyı ayarla
                _db.TaskItems.Add(taskItem);
                await _db.SaveChangesAsync();

                return RedirectToAction("Details", "Board", new { id = boardId });
            }

            ViewBag.BoardId = boardId;
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
                var existingTask = await _db.TaskItems.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (existingTask == null) return Forbid();

                try
                {
                    taskItem.UserId = userId!; // Kullanıcıyı ayarla
                    taskItem.BoardId = existingTask.BoardId; // BoardId'yi koru
                    _db.TaskItems.Update(taskItem);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction("Details", "Board", new { id = taskItem.BoardId });
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
            
            // Check if the new status is a valid value
            var validStatuses = new List<string> { "todo", "doing", "done" };
            if (!validStatuses.Contains(newStatus))
            {
                return BadRequest("Invalid status value.");
            }

            taskItem.Status = newStatus;
            _db.TaskItems.Update(taskItem);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}