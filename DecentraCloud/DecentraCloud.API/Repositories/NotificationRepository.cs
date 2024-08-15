using DecentraCloud.API.Data;
using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecentraCloud.API.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DecentraCloudContext _context;

        public NotificationRepository(DecentraCloudContext context)
        {
            _context = context;
        }

        public async Task AddNotification(Notification notification)
        {
            await _context.Notifications.InsertOneAsync(notification);
        }

        public async Task<IEnumerable<Notification>> GetNotifications()
        {
            return await _context.Notifications.Find(_ => true).ToListAsync();
        }

        public async Task<Notification> GetNotificationById(string id)
        {
            return await _context.Notifications.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateNotification(Notification notification)
        {
            await _context.Notifications.ReplaceOneAsync(n => n.Id == notification.Id, notification);
        }

        public async Task DeleteNotification(string id)
        {
            await _context.Notifications.DeleteOneAsync(n => n.Id == id);
        }
    }
}
