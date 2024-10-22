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

        // GET: Task/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                taskItem.UserId = userId;
                _db.TaskItems.Add(taskItem);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", "Board");
            }
            return View(taskItem);
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _db.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (taskItem.UserId != userId)
            {
                return Forbid(); // Prevents editing tasks not owned by the user
            }

            return View(taskItem);
        }

        // POST: Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(taskItem);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Board");
            }
            return View(taskItem);
        }

        // GET: Task/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _db.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (taskItem.UserId != userId)
            {
                return Forbid(); // Prevents deleting tasks not owned by the user
            }

            return View(taskItem);
        }

        // POST: Task/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _db.TaskItems.FindAsync(id);
            _db.TaskItems.Remove(taskItem);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Board");
        }

        private bool TaskItemExists(int id)
        {
            return _db.TaskItems.Any(e => e.Id == id);
        }
    }
}
