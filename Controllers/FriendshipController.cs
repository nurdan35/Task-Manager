using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.ViewModels;

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

        // Send a friend request
        [HttpPost]
        public async Task<IActionResult> SendRequest(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId))
            {
                return BadRequest("Receiver ID cannot be null or empty.");
            }

            // Get the currently logged-in user's ID
            var userId = _userManager.GetUserId(User);
            if (userId == receiverId)
            {
                return BadRequest("You cannot send a friend request to yourself.");
            }

            // Check if the receiver exists (by ID or Email)
            var receiver = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == receiverId || u.Email == receiverId);
            if (receiver == null)
            {
                return NotFound("The specified user does not exist.");
            }

            // Check if a friendship request already exists between these users
            var existingFriendship = await _db.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == userId && f.ReceiverId == receiver.Id) ||
                    (f.RequesterId == receiver.Id && f.ReceiverId == userId));

            if (existingFriendship != null)
            {
                return BadRequest("A friend request already exists or you are already friends.");
            }

            // Create a new friendship request
            var friendship = new Friendship
            {
                RequesterId = userId,
                ReceiverId = receiver.Id,
                IsAccepted = false,
                CreatedAt = DateTime.Now
            };

            _db.Friendships.Add(friendship);
            await _db.SaveChangesAsync();

            return Ok("Friend request sent.");
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

            return RedirectToAction("GetRequests");
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

            return RedirectToAction("GetRequests");
        }

        // Get the user's friends
        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            var userId = _userManager.GetUserId(User);

            var friends = await _db.Friendships
                .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.IsAccepted)
                .Select(f => f.RequesterId == userId ? f.Receiver : f.Requester)
                .Select(user => new FriendViewModel
                {
                    Id = user.Id,
                    DisplayName = string.IsNullOrEmpty(user.Nickname) ? user.UserName : user.Nickname
                })
                .ToListAsync();

            return View(friends);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetRequests()
        {
            var userId = _userManager.GetUserId(User);

            var friendRequests = await _db.Friendships
                .Where(f => f.ReceiverId == userId && !f.IsAccepted)
                .Select(f => new FriendRequestViewModel
                {
                    RequestId = f.Id,
                    RequesterId = f.RequesterId,
                    RequesterDisplayName = !string.IsNullOrEmpty(f.Requester.Nickname) ? f.Requester.Nickname : f.Requester.UserName,
                    RequesterEmail = f.Requester.Email
                })
                .ToListAsync();

            return View(friendRequests);
        }
        
        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = _userManager.GetUserId(User);

            // İlgili arkadaşlık kaydını bulun
            var friendship = await _db.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.RequesterId == userId && f.ReceiverId == friendId) || 
                    (f.RequesterId == friendId && f.ReceiverId == userId));

            if (friendship == null)
                return NotFound("Friendship not found.");

            // Kaydı sil
            _db.Friendships.Remove(friendship);
            await _db.SaveChangesAsync();

            // Friends sayfasına dön
            return RedirectToAction("GetFriends");
        }
    }
}