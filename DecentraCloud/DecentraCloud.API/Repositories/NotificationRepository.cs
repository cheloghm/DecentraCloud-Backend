using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecentraCloud.API.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationRepository(IMongoDatabase database)
        {
            _notifications = database.GetCollection<Notification>("Notifications");
        }

        public async Task AddNotification(Notification notification)
        {
            await _notifications.InsertOneAsync(notification);
        }

        public async Task<IEnumerable<Notification>> GetNotifications()
        {
            return await _notifications.Find(_ => true).ToListAsync();
        }
    }
}
