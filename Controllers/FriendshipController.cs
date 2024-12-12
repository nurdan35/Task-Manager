using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class FriendshipController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FriendshipController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Get all friends and friend requests
        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            var userId = _userManager.GetUserId(User);
            // Get accepted friends
            var friends = await _db.Friendships
                .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.IsAccepted)
                .Select(f => f.RequesterId == userId ? f.Receiver : f.Requester)
                .Select(user => new FriendViewModel
                {
                    Id = user.Id,
                    DisplayName = string.IsNullOrEmpty(user.Nickname) ? user.UserName : user.Nickname
                })
                .ToListAsync();
            // Get pending friend requests
            var friendRequests = await _db.Friendships
                .Where(f => f.ReceiverId == userId && !f.IsAccepted)
                .Select(f => new FriendRequestViewModel
                {
                    RequestId = f.Id,
                    RequesterId = f.RequesterId,
                    RequesterDisplayName = !string.IsNullOrEmpty(f.Requester.Nickname)
                        ? f.Requester.Nickname
                        : f.Requester.UserName,
                    RequesterEmail = f.Requester.Email
                })
                .ToListAsync();
            var viewModel = new FriendsViewModel
            {
                Friends = friends,
                FriendRequests = friendRequests
            };
            return View(viewModel);
        }

        // Send a friend request
        [HttpPost]
        public async Task<IActionResult> SendRequest(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId))
            {
                return Json(new { success = false, message = "Receiver ID cannot be null or empty." });
            }

            var userId = _userManager.GetUserId(User);
            if (userId == receiverId)
            {
                return Json(new { success = false, message = "You cannot send a friend request to yourself." });
            }

            var receiver =
                await _userManager.Users.FirstOrDefaultAsync(u => u.Id == receiverId || u.Email == receiverId);
            if (receiver == null)
            {
                return Json(new { success = false, message = "The specified user does not exist." });
            }

            var existingFriendship = await _db.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == userId && f.ReceiverId == receiver.Id) ||
                    (f.RequesterId == receiver.Id && f.ReceiverId == userId));

            if (existingFriendship != null)
            {
                return Json(new
                    { success = false, message = "A friend request already exists or you are already friends." });
            }

            var friendship = new Friendship
            {
                RequesterId = userId,
                ReceiverId = receiver.Id,
                IsAccepted = false,
                CreatedAt = DateTime.Now
            };

            _db.Friendships.Add(friendship);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Friend request sent successfully." });
        }

        // Accept a friend request
        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var userId = _userManager.GetUserId(User);

            var friendship = await _db.Friendships
                .FirstOrDefaultAsync(f => f.Id == requestId && f.ReceiverId == userId);

            if (friendship == null)
                return NotFound("Friend request not found.");

            friendship.IsAccepted = true;
            await _db.SaveChangesAsync();

            return RedirectToAction("GetFriends");
        }

        // Reject a friend request
        [HttpPost]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            var userId = _userManager.GetUserId(User);

            var friendship = await _db.Friendships
                .FirstOrDefaultAsync(f => f.Id == requestId && f.ReceiverId == userId);

            if (friendship == null)
                return NotFound("Friend request not found.");

            _db.Friendships.Remove(friendship);
            await _db.SaveChangesAsync();
            return RedirectToAction("GetFriends");
        }

        // Remove an existing friend
        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = _userManager.GetUserId(User);

            var friendship = await _db.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == userId && f.ReceiverId == friendId) ||
                    (f.RequesterId == friendId && f.ReceiverId == userId));

            if (friendship == null)
                return NotFound("Friendship not found.");

            _db.Friendships.Remove(friendship);
            await _db.SaveChangesAsync();
            return RedirectToAction("GetFriends");
        }
    }
}