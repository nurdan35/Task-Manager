namespace TaskManagement.ViewModels
{
    public class FriendRequestViewModel
    {
        public int RequestId { get; set; }
        public string RequesterId { get; set; }
        public string RequesterDisplayName { get; set; }
        public string RequesterEmail { get; set; }
    }
}