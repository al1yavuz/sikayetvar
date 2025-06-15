using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sikayetvar.Data;
using sikayetvar.Models;
using System.Linq;
using System.Threading.Tasks;

namespace sikayetvar.Controllers
{
    [Authorize]
    public class NotificationsController : BaseController
    {
        public NotificationsController(AppDbContext context, UserManager<ApplicationUser> userManager)
            : base(userManager, context)
        {
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                if (!notification.IsRead)
                    notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return View(notifications);
        }




    }
}
