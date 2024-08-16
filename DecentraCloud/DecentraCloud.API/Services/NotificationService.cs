using DecentraCloud.API.Interfaces.ServiceInterfaces;
using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Models;

namespace DecentraCloud.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<Notification>> GetNotifications()
        {
            return await _notificationRepository.GetNotifications();
        }

        public async Task<Notification> GetNotificationById(string id)
        {
            return await _notificationRepository.GetNotificationById(id);
        }

        public async Task<bool> UpdateNotification(Notification notification)
        {
            return await _notificationRepository.UpdateNotification(notification);
        }

        public async Task<bool> DeleteNotification(string id)
        {
            return await _notificationRepository.DeleteNotification(id);
        }
    }

}
