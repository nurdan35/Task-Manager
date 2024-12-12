using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Nickname is required.")]
        [MaxLength(50, ErrorMessage = "Nickname cannot exceed 50 characters.")]
        public string Nickname { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string? ProfileImage { get; set; } // GNOS-13: Profil resmi
        
        [MaxLength(20)]
        public string Theme { get; set; } = "light"; // Default theme is 'light' // GNOS-14: Tema değiştirme
        
        public string? ProfilePicturePath { get; set; } // Path to profile picture
        
        // Relationships
        public ICollection<Board> Boards { get; set; } = new List<Board>();
        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        
        public bool IsEmailNotificationEnabled { get; set; } = false;
        public bool IsSmsNotificationEnabled { get; set; } = false;
        public bool IsPushNotificationsEnabled { get; set; } = false; // Default
        
        public ICollection<BoardShare> SharedBoards { get; set; } = new List<BoardShare>();
    }
}