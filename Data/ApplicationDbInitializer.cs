using TaskManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace TaskManagement.Data
{
    public static class ApplicationDbInitializer
    {
        public static void Initialize(ApplicationDbContext db, UserManager<ApplicationUser> um,
            RoleManager<IdentityRole> rm)
        {
            // Reset and recreate the database
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Check roles, create them if they don't exist
            if (!rm.RoleExistsAsync("Admin").Result)
            {
                var adminRole = new IdentityRole("Admin");
                rm.CreateAsync(adminRole).Wait();
            }

            if (!rm.RoleExistsAsync("User").Result)
            {
                var userRole = new IdentityRole("User");
                rm.CreateAsync(userRole).Wait();
            }

            // Create admin user
            var admin = new ApplicationUser
            {
                Nickname = "AdminUser",
                UserName = "admin@taskmanager.com",
                Email = "admin@taskmanager.com",
                EmailConfirmed = true
            };
            if (um.FindByNameAsync(admin.UserName).Result == null)
            {
                um.CreateAsync(admin, "AdminPassword1!").Wait();
                um.AddToRoleAsync(admin, "Admin").Wait();
            }

            // Create normal user
            var user = new ApplicationUser
            {
                Nickname = "NormalUser",
                UserName = "user@taskmanager.com",
                Email = "user@taskmanager.com",
                EmailConfirmed = true
            };
            if (um.FindByNameAsync(user.UserName).Result == null)
            {
                um.CreateAsync(user, "UserPassword1!").Wait();
                um.AddToRoleAsync(user, "User").Wait();
            }

            // Add sample tasks
            if (!db.TaskItems.Any())
            {
                var task1 = new TaskItem
                {
                    Title = "Task 1: Design Plan",
                    Description = "Create a design plan for the task management system.",
                    Status = "todo",
                    DueDate = DateTime.Now.AddDays(5),
                    Tag = "Draft",
                    IsFlagged = false,
                    UserId = user.Id // Assigned to the normal user
                };

                var task2 = new TaskItem
                {
                    Title = "Task 2: Start Coding",
                    Description = "Set up the basic coding structure for the task management system.",
                    Status = "doing",
                    DueDate = DateTime.Now.AddDays(7),
                    Tag = "Priority",
                    IsFlagged = true,
                    UserId = admin.Id // Assigned to the admin user
                };

                db.TaskItems.AddRange(task1, task2);
                db.SaveChanges();

            }
        }
    }
}
