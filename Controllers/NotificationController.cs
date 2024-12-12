using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID is null or empty");
                return Unauthorized("User not found.");
            }

            try
            {
                var notifications = await _db.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.NotificationDate)
                    .ToListAsync();

                return View(notifications);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching notifications: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching notifications.");
            }
        }

        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> NotificationSettings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new NotificationSettingsViewModel
            {
                EmailNotificationsEnabled = user.IsEmailNotificationEnabled,
                SmsNotificationsEnabled = user.IsSmsNotificationEnabled,
                PushNotificationsEnabled = user.IsPushNotificationsEnabled
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> NotificationSettings(NotificationSettingsViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            user.IsEmailNotificationEnabled = model.EmailNotificationsEnabled;
            user.IsSmsNotificationEnabled = model.SmsNotificationsEnabled;
            user.IsPushNotificationsEnabled = model.PushNotificationsEnabled;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "An error occurred while saving your settings.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Your settings have been saved.";
            return RedirectToAction("NotificationSettings");
        }
    }
}
