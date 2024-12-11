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
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(string nickname, string theme, IFormFile profileImage, string selectedProfilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.Nickname = nickname;
                user.Theme = theme;
                
                // Handles profile pictures from user
                if (profileImage != null && profileImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    // deleting the old profile picture, if there is one (only those uploaded by user)
                    if (!string.IsNullOrEmpty(user.ProfilePicturePath) && user.ProfilePicturePath.StartsWith("/images/profiles/"))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    
                    // save the new profile picture
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + profileImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(fileStream);
                    }
                    user.ProfilePicturePath = "/images/profiles/" + uniqueFileName;
                }
                
                // Handles selected example profile picture
                else if (!string.IsNullOrEmpty(selectedProfilePicture))
                {
                    // delete the existing profile picture if it's a user upload
                    if (!string.IsNullOrEmpty(user.ProfilePicturePath) && !user.ProfilePicturePath.StartsWith("/images/profileExamples"))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    // set the selected example profile picture
                    user.ProfilePicturePath = selectedProfilePicture;
                }
                
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