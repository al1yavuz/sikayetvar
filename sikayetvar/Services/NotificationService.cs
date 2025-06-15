using sikayetvar.Data;
using sikayetvar.Models;
using System;
using System.Threading.Tasks;

namespace sikayetvar.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string userId, string message, string url = null, string type = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Url = url,
                Type = type,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
