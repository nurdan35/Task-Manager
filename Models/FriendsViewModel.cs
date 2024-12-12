namespace TaskManagement.Models
{
    public class FriendsViewModel
    {
        public IEnumerable<FriendViewModel> Friends { get; set; } = new List<FriendViewModel>();
        public IEnumerable<FriendRequestViewModel> FriendRequests { get; set; } = new List<FriendRequestViewModel>();
    }
}