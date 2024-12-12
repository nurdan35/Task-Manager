using System.ComponentModel.DataAnnotations;
namespace TaskManagement.Models
{
    public class Notification
    {
        public enum NotificationType
        {
            Email,
            Sms,
            Push
        }

        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;
        public DateTime NotificationDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = new ApplicationUser();
        
        public NotificationType Type { get; set; }
        
        public bool IsRead { get; set; } = false; // default
        
    }
}

