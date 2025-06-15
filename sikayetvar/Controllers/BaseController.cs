using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using sikayetvar.Models;
using sikayetvar.Data;
using System.Threading.Tasks;
using System.Linq;

public class BaseController : Controller
{
    protected readonly UserManager<ApplicationUser> _userManager;
    protected readonly AppDbContext _context;

    public BaseController(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        if (User.Identity.IsAuthenticated)
        {
            var userId = _userManager.GetUserId(User);
            var unreadCount = _context.Notifications.Count(n => n.UserId == userId && !n.IsRead);
            ViewBag.UnreadNotifications = unreadCount;
        }
        base.OnActionExecuting(context);
    }
}
