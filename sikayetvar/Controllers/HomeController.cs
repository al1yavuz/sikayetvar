using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sikayetvar.Data;
using System.Linq;
using System.Threading.Tasks;

namespace sikayetvar.Controllers
{
    public class HomeController : BaseController
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context, UserManager<ApplicationUser> userManager)
            : base(userManager, context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var mostCommented = await _context.Complaints
                .Include(c => c.User)
                .Include(c => c.Comments)
                .Where(c => c.IsApproved)
                .OrderByDescending(c => c.Comments.Count)
                .Take(5)
                .ToListAsync();

            return View(mostCommented);
        }
    }
}
