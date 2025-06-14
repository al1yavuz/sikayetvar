using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sikayetvar.Data;
using sikayetvar.Models;
using System.Threading.Tasks;

namespace sikayetvar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index(bool onlyPending = false)
        {
            var query = _context.Complaints
                                .Include(c => c.User)
                                .AsQueryable();

            if (onlyPending)
                query = query.Where(c => !c.IsApproved);

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewData["OnlyPending"] = onlyPending;
            return View(complaints);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null && !complaint.IsApproved)
            {
                complaint.IsApproved = true;
                complaint.ApprovalDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { onlyPending = true });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                _context.Complaints.Remove(complaint);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
