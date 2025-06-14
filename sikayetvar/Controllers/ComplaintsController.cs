using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sikayetvar.Data;
using System.Security.Claims;
using sikayetvar.Models;
using System.Threading.Tasks;

namespace sikayetvar.Controllers
{
    [Authorize]
    public class ComplaintsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComplaintsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var approvedComplaints = await _context.Complaints
                .Where(c => c.IsApproved)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(approvedComplaints);
        }

        public async Task<IActionResult> MyComplaints()
        {
            var user = await _userManager.GetUserAsync(User);
            var complaints = await _context.Complaints
                .Where(c => c.UserId == user.Id)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Complaint complaint, IFormFile? imageFile, string? imageUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bilgisi alınamadı.");
                return View(complaint);
            }

            if ((imageFile == null || imageFile.Length == 0) && string.IsNullOrWhiteSpace(imageUrl))
            {
                ModelState.AddModelError("", "Lütfen bir resim yükleyin ya da bir resim URL'si girin.");
                return View(complaint);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                complaint.ImagePath = "/uploads/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                complaint.ImagePath = imageUrl;
            }

            complaint.UserId = user.Id;
            complaint.CreatedAt = DateTime.Now;
            complaint.IsApproved = false;

            if (ModelState.IsValid)
            {
                _context.Add(complaint);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Şikayet başarıyla gönderildi.";
                return RedirectToAction(nameof(MyComplaints));
            }

            return View(complaint);
        }

        public async Task<IActionResult> Details(int id)
        {
            var complaint = await _context.Complaints
                .Include(c => c.User)
                .Include(c => c.Comments)
                    .ThenInclude(comment => comment.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
                return NotFound();

            ViewBag.NewComment = new Comment { ComplaintId = complaint.Id };
            return View(complaint);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (!string.IsNullOrWhiteSpace(comment.Text))
            {
                comment.UserId = user.Id;
                comment.CreatedAt = DateTime.Now;

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = comment.ComplaintId });
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("Index");

            var results = _context.Complaints
                .Include(c => c.User)
                .Where(c => c.IsApproved &&
                           (c.Title.Contains(query) || c.Description.Contains(query)))
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View("SearchResults", results);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (complaint.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            return View(complaint);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Complaint complaint, IFormFile imageFile, string imageUrl)
        {
            if (id != complaint.Id) return NotFound();

            var original = await _context.Complaints.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (original == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (original.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            complaint.UserId = original.UserId;
            complaint.CreatedAt = original.CreatedAt;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                complaint.ImagePath = "/uploads/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                complaint.ImagePath = imageUrl;
            }
            else
            {
                complaint.ImagePath = original.ImagePath;
            }

            if (ModelState.IsValid)
            {
                _context.Update(complaint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyComplaints));
            }

            return View(complaint);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isOwner = complaint.UserId == user.Id;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (!isOwner && !isAdmin)
                return Forbid();

            return View(complaint);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isOwner = complaint.UserId == user.Id;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (!isOwner && !isAdmin)
                return Forbid();

            _context.Complaints.Remove(complaint);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyComplaints));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleResolved(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isOwner = complaint.UserId == user.Id;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (!isOwner && !isAdmin) return Forbid();

            complaint.IsResolved = !complaint.IsResolved;
            if (complaint.IsResolved)
                complaint.ResolvedAt = DateTime.Now;
            else
                complaint.ResolvedAt = null;

            _context.Update(complaint);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

    }
}
