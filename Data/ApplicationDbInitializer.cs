using TaskManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace TaskManagement.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext db, UserManager<ApplicationUser> um,
            RoleManager<IdentityRole> rm)
        {
            // Reset and recreate the database (Use only for development purposes)
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            // Check roles, create them if they don't exist
            if (!await rm.RoleExistsAsync("Admin"))
            {
                var adminRole = new IdentityRole("Admin");
                await rm.CreateAsync(adminRole);
            }

            if (!await rm.RoleExistsAsync("User"))
            {
                var userRole = new IdentityRole("User");
                await rm.CreateAsync(userRole);
            }

            // Create admin user
            var admin = new ApplicationUser
            {
                Nickname = "AdminUser",
                UserName = "admin@taskmanager.com",
                Email = "admin@taskmanager.com",
                EmailConfirmed = true
            };
            if (await um.FindByNameAsync(admin.UserName) == null)
            {
                var result = await um.CreateAsync(admin, "AdminPassword1!");
                if (result.Succeeded)
                {
                    await um.AddToRoleAsync(admin, "Admin");
                }
            }

            // Create normal user
            var user = new ApplicationUser
            {
                Nickname = "NormalUser",
                UserName = "user@taskmanager.com",
                Email = "user@taskmanager.com",
                EmailConfirmed = true
            };
            if (await um.FindByNameAsync(user.UserName) == null)
            {
                var result = await um.CreateAsync(user, "UserPassword1!");
                if (result.Succeeded)
                {
                    await um.AddToRoleAsync(user, "User");
                }
            }

            // Add sample tasks
            if (!db.TaskItems.Any())
            {
                var adminId = (await um.FindByNameAsync(admin.UserName))?.Id;
                var userId = (await um.FindByNameAsync(user.UserName))?.Id;

                if (adminId == null || userId == null)
                {
                    throw new Exception("Admin or User ID could not be found.");
                }

                var task1 = new TaskItem
                {
                    Title = "Task 1: Design Plan",
                    Description = "Create a design plan for the task management system.",
                    Status = "todo",
                    DueDate = DateTime.Now.AddDays(5),
                    Tag = "Draft",
                    UserId = userId // Assigned to the normal user
                };

                var task2 = new TaskItem
                {
                    Title = "Task 2: Start Coding",
                    Description = "Set up the basic coding structure for the task management system.",
                    Status = "doing",
                    DueDate = DateTime.Now.AddDays(7),
                    Tag = "Priority",
                    IsFlagged = true,
                    UserId = adminId // Assigned to the admin user
                };

                await db.TaskItems.AddRangeAsync(task1, task2);
                await db.SaveChangesAsync();
            }
        }
    }
}