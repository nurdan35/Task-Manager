using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


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

            var userBoard = await _db.Boards.FirstOrDefaultAsync(b => b.UserId == userId);
            ViewBag.BoardId = userBoard?.Id;

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
        public async Task<IActionResult> Create(int? boardId = null)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to create tasks.";
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcının board'larını al
            var boards = await _db.Boards
                .Where(b => b.UserId == currentUserId)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Title
                }).ToListAsync();

            if (!boards.Any())
            {
                TempData["InfoMessage"] = "You need to create a board before adding tasks.";
                return RedirectToAction("Create", "Board");
            }

            // Model oluştur
            var model = new BoardViewModel
            {
                BoardSelectList = boards,
                TaskItem = new TaskItem { DueDate = DateTime.Today }
            };

            if (boardId.HasValue)
            {
                var boardExists = await _db.Boards
                    .AnyAsync(b => b.Id == boardId && b.UserId == currentUserId);

                if (!boardExists)
                {
                    TempData["ErrorMessage"] = "You do not have access to this board or it does not exist.";
                    return RedirectToAction("Index", "Board");
                }

                model.TaskItem.BoardId = boardId.Value;
            }

            return View(model);
        }


        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BoardViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to create tasks.";
                return RedirectToAction("Login", "Account");
            }

            if (model.TaskItem.BoardId <= 0)
            {
                ModelState.AddModelError("TaskItem.BoardId", "Please select a valid board.");
            }

            if (!ModelState.IsValid)
            {
                // Dropdown için board listesini yeniden yükle
                model.BoardSelectList = await _db.Boards
                    .Where(b => b.UserId == currentUserId)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.Title
                    }).ToListAsync();

                TempData["ErrorMessage"] = "Please correct the highlighted errors.";
                return View(model);
            }

            var boardExists = await _db.Boards
                .AnyAsync(b => b.Id == model.TaskItem.BoardId && b.UserId == currentUserId);

            if (!boardExists)
            {
                ModelState.AddModelError("TaskItem.BoardId", "The selected board is not valid or does not belong to you.");
                model.BoardSelectList = await _db.Boards
                    .Where(b => b.UserId == currentUserId)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.Title
                    }).ToListAsync();
                return View(model);
            }

            try
            {
                model.TaskItem.UserId = currentUserId;
                _db.TaskItems.Add(model.TaskItem);
                await _db.SaveChangesAsync();

                var notification = new Notification
                {
                    Message = $"New task created: {model.TaskItem.Title}",
                    NotificationDate = DateTime.UtcNow,
                    UserId = currentUserId,
                    Type = Notification.NotificationType.Push,
                    IsRead = false
                };

                _db.Notifications.Add(notification);
                await _db.SaveChangesAsync();
                
                return RedirectToAction("Index", "Task");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the task: {ex.Message}";
                return View(model);
            }
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
                    taskItem.UserId = userId!;
                    taskItem.BoardId = existingTask.BoardId;
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