﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sikayetvar.Data;
using sikayetvar.Models;
using sikayetvar.Services;
using System.Threading.Tasks;

namespace sikayetvar.Controllers
{
    [Authorize]
    public class CommentLikeController : BaseController
    {
        private readonly NotificationService _notificationService;

        public CommentLikeController(UserManager<ApplicationUser> userManager, AppDbContext context, NotificationService notificationService)
            : base(userManager, context)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> Like(int commentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
           
            var existingLike = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.UserId == user.Id && cl.CommentId == commentId);

            if (existingLike != null)
            {
                
                _context.CommentLikes.Remove(existingLike);
            }
            else
            {
                
                var like = new CommentLike
                {
                    UserId = user.Id,
                    CommentId = commentId
                };

                _context.CommentLikes.Add(like);

                
                var comment = await _context.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == commentId);

                if (comment != null && comment.UserId != user.Id)
                {
                    await _notificationService.CreateNotificationAsync(
                        userId: comment.UserId,
                        message: $"{user.FirstName} {user.LastName} yorumunuzu beğendi.",
                        url: $"/Complaints/Details/{comment.ComplaintId}",
                        type: "begenme"
                    );
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Complaints", new { id = (await _context.Comments.FindAsync(commentId))?.ComplaintId });
        }
    }
}
