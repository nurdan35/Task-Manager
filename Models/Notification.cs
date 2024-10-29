using System.ComponentModel.DataAnnotations;
namespace TaskManagement.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime NotificationDate { get; set; }
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = new ApplicationUser();
        
        public bool IsEmailNotification { get; set; } // For email notifications
        public bool IsSmsNotification { get; set; } // For SMS notifications
    }
}

