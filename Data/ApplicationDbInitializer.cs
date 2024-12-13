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
                UserName = "u@uia.no",
                Email = "u@uia.no",
                EmailConfirmed = true
            };
            if (await um.FindByNameAsync(user.UserName) == null)
            {
                var result = await um.CreateAsync(user, "Passord!1");
                if (result.Succeeded)
                {
                    await um.AddToRoleAsync(user, "User");
                }
            }
            
            // Add sample boards
            if (!db.Boards.Any())
            {
                var adminId = (await um.FindByNameAsync(admin.UserName))?.Id;
                var userId = (await um.FindByNameAsync(user.UserName))?.Id;

                if (adminId == null || userId == null)
                {
                    throw new Exception("Admin or User ID could not be found.");
                }

                var board1 = new Board
                {
                    Title = "Admin's Board",
                    UserId = adminId
                };

                var board2 = new Board
                {
                    Title = "User's Board",
                    UserId = userId
                };

                await db.Boards.AddRangeAsync(board1, board2);
                await db.SaveChangesAsync();

                // Add sample tasks associated with boards
                var task1 = new TaskItem
                {
                    Title = "Task 1: Design Plan",
                    Description = "Create a design plan for the task management system.",
                    Status = "todo",
                    DueDate = DateTime.Now.AddDays(5),
                    Tag = "Draft",
                    UserId = userId,
                    BoardId = board2.Id // Assign to user's board
                };

                var task2 = new TaskItem
                {
                    Title = "Task 2: Start Coding",
                    Description = "Set up the basic coding structure for the task management system.",
                    Status = "doing",
                    DueDate = DateTime.Now.AddDays(7),
                    Tag = "Priority",
                    IsFlagged = false,
                    UserId = adminId,
                    BoardId = board1.Id // Assign to admin's board
                };

                await db.TaskItems.AddRangeAsync(task1, task2);
                await db.SaveChangesAsync();
                
                // Add notifications
                var notification1 = new Notification
                {
                    Message = "Task 1: Design Plan has been created.",
                    NotificationDate = DateTime.UtcNow,
                    UserId = userId,
                    Type = Notification.NotificationType.Email,
                    IsRead = false
                };

                var notification2 = new Notification
                {
                    Message = "Task 2: Start Coding has been created.",
                    NotificationDate = DateTime.UtcNow,
                    UserId = adminId,
                    Type = Notification.NotificationType.Push,
                    IsRead = false
                };

                await db.Notifications.AddRangeAsync(notification1, notification2);
                await db.SaveChangesAsync();

            }

        }
    }
}