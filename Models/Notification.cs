namespace TaskManagement.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime NotificationDate { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}

