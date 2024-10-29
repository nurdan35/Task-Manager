using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(string nickname, string theme)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.Nickname = nickname;
                user.Theme = theme;
                await _userManager.UpdateAsync(user);
                return RedirectToAction("Dashboard");
            }
            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> NotificationSettings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Mevcut bildirim tercihlerini döndür
            return View(new NotificationSettingsViewModel
            {
                EmailNotificationsEnabled = user.Notifications.Any(n => n.IsEmailNotification),
                SmsNotificationsEnabled = user.Notifications.Any(n => n.IsSmsNotification)
            });
        }

        [HttpPost]
        public async Task<IActionResult> NotificationSettings(NotificationSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.Notifications.Clear(); // Önce eski bildirim tercihlerini temizle

            if (model.EmailNotificationsEnabled)
            {
                user.Notifications.Add(new Notification { IsEmailNotification = true });
            }
            if (model.SmsNotificationsEnabled)
            {
                user.Notifications.Add(new Notification { IsSmsNotification = true });
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Dashboard");
        }

    }
}
