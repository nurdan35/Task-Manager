namespace TaskManagement.Models
{
    public class FriendRequestViewModel
    {
        public int RequestId { get; set; } 
        public string RequesterId { get; set; } = string.Empty;
        public string? RequesterDisplayName { get; set; }
        public string? RequesterEmail { get; set; }
    }
}