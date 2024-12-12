using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class BoardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public BoardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Boardları Listeleme
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var boards = await _db.Boards
                .Where(b => b.UserId == userId)
                .Include(b => b.Tasks)
                .ToListAsync();
            
            // Kullanıcının paylaşılan boardlarını getir
            var sharedBoards = await _db.BoardShares
                .Where(bs => bs.SharedWithUserId == userId)
                .Include(bs => bs.Board)
                .ThenInclude(b => b.Tasks)
                .Select(bs => bs.Board)
                .Where(b => b != null)
                .ToListAsync();
            

            // Kullanıcının paylaşılan boardlarını getir
            var sharedBoards = await _db.BoardShares
                .Where(bs => bs.SharedWithUserId == userId)
                .Include(bs => bs.Board)
                .ThenInclude(b => b.Tasks)
                .Select(bs => bs.Board)
                .Where(b => b != null)
                .ToListAsync();
            
            
            var viewModel = new BoardViewModel
            {
                Board = boards.FirstOrDefault(),
                Boards = boards,
                SharedBoards = sharedBoards.Where(sb => sb != null).ToList()!,
                Tasks = boards.SelectMany(b => b.Tasks).ToList(),
                CurrentUser = User.Identity?.Name ?? "Guest"
            };

            return View(viewModel);
        }
        
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var board = await _db.Boards
                .Include(b => b.Tasks)
                .Include(b => b.BoardShares)
                .FirstOrDefaultAsync(b => b.Id == id 
                                          && (b.UserId == userId || b.BoardShares.Any(bs => bs.SharedWithUserId == userId)));

            if (board == null)
            {
                return NotFound("Board not found or you do not have access.");
            }

            return View(board);
        }

        // Yeni Board Oluşturma - GET
        public IActionResult Create()
        {
            return View();
        }

        // Yeni Board Oluşturma - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Board board)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized("Kullanıcının oturum açması gerekiyor.");
                }
                
                board.UserId = userId;  // Sahip olarak mevcut kullanıcıyı ayarla
                _db.Boards.Add(board);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(board);
        }

        // Board Düzenleme - GET
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var board = await _db.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (board == null)
            {
                return NotFound("Board not found or you do not have permission to edit this board.");
            }

            return View(board);
        }

        // Board Düzenleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Board board)
        {
            if (id != board.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var existingBoard = await _db.Boards.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (existingBoard == null)
                {
                    return Forbid("You do not have permission to edit this board.");
                }
                board.UserId = userId!;
                _db.Boards.Update(board);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(board);
        }

        // Board Silme - GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var board = await _db.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (board == null)
            {
                return NotFound("Board not found or you do not have permission to delete this board.");
            }

            return View(board);
        }

        // Board Silme - POST
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var board = await _db.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (board == null)
            {
                return NotFound("Board not found or you do not have permission to delete this board.");
            }

            _db.Boards.Remove(board);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShareBoard(int boardId, string sharedWithUserId, string email)
        {
            var userId = _userManager.GetUserId(User);

            // Board'un sahibi olup olmadığını kontrol edin
            var board = await _db.Boards.FirstOrDefaultAsync(b => b.Id == boardId && b.UserId == userId);
            if (board == null)
                return Json(new { success = false, message ="Board not found or you do not have permission to share it." });

            // Kullanıcıyı e-posta ile bulun
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Daha önce paylaşılmış mı kontrol edin
            var existingShare = await _db.BoardShares
                .FirstOrDefaultAsync(bs => bs.BoardId == boardId && bs.SharedWithUserId == sharedWithUserId);
            if (existingShare != null)
                return Json(new { success = false, message = "This board is already shared with this user." });

            // Paylaşımı ekle
            var boardShare = new BoardShare
            {
                BoardId = boardId,
                SharedWithUserId = user.Id, // Paylaşılacak kişinin UserId'si
                SharedWithUserEmail = user.Email // Alternatif olarak Email
            };
            _db.BoardShares.Add(boardShare);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Board shared successfully." });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSharedBoard(int boardId)
        {
            Console.WriteLine($"DeleteSharedBoard: Received boardId: {boardId}");
            
            if (boardId <= 0)
            {
                return BadRequest("Invalid board ID.");
            }
            var userId = _userManager.GetUserId(User);

            // Kontrol: Kullanıcı oturum açmış mı
            if (userId == null)
            {
                return Unauthorized("You must be logged in to delete shared boards.");
            }

            // Paylaşılan board'u kontrol et
            var boardShare = await _db.BoardShares
                .FirstOrDefaultAsync(bs => bs.BoardId == boardId && bs.SharedWithUserId == userId);

            if (boardShare == null)
            {
                Console.WriteLine($"DeleteSharedBoard: No matching record found for BoardId: {boardId}, SharedWithUserId: {userId}");
                return NotFound("Shared board not found or you do not have permission to delete it.");
            }

            // Paylaşımı sil
            _db.BoardShares.Remove(boardShare);
            await _db.SaveChangesAsync();
            
            // Kullanıcıyı Index sayfasına yönlendir
            return RedirectToAction(nameof(Index));
        }
    }
}