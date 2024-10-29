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

            var viewModel = new BoardViewModel
            {
                Board = boards.FirstOrDefault(),
                Boards = boards,
                Tasks = boards.SelectMany(b => b.Tasks).ToList(),
                CurrentUser = User.Identity?.Name ?? "Guest"
            };

            return View(viewModel);
        }

        // Board Detayları
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var board = await _db.Boards
                .Include(b => b.Tasks)
                .Include(b => b.Collaborators)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

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
    }
}
