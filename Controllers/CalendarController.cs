using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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
            
            // Vi trenger ikke lenger å sende med alle tasks her,
            // fordi FullCalendar vil hente dem via GetEvents-metoden.
            // Men du kan hente dem om du ønsker - vi bruker ikke Model i dette eksemplet lenger.

            ViewData["AntiforgeryToken"] = HttpContext.RequestServices
                .GetService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>()
                ?.GetAndStoreTokens(HttpContext).RequestToken;

            return View(); // Returner view uten Model, siden vi laster data via AJAX
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

        // LEGG TIL DENNE METODEN FOR Å HENTE OG FILTRERE EVENTS BASERT PÅ QUERY
        [HttpGet]
        public async Task<IActionResult> GetEvents(string query)
        {
            var userId = _userManager.GetUserId(User);
            query = query ?? "";
            var lowerQuery = query.ToLower();

            // Forsøk å tolke query som en dato i formatet yyyy-MM-dd
            bool isDate = DateTime.TryParseExact(
                query,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate
            );

            var filteredTasks = await _db.TaskItems
                .Where(t => t.UserId == userId &&
                            (string.IsNullOrEmpty(query) ||
                             t.Title.ToLower().Contains(lowerQuery) ||
                             (t.Description != null && t.Description.ToLower().Contains(lowerQuery)) ||
                             (isDate && t.DueDate.Date == parsedDate.Date)))
                .Select(t => new {
                    id = t.Id,
                    title = t.Title,
                    start = t.DueDate.ToString("yyyy-MM-dd"),
                    color = t.Status == "done" ? "green" 
                        : (t.Status == "doing" ? "blue" : "red"),
                    extendedProps = new {
                        description = t.Description,
                        priority = t.Priority,
                        status = t.Status
                    }
                })
                .ToListAsync();

            return Json(filteredTasks);
        }

    }
}
