using DecentraCloud.API.Models;

namespace DecentraCloud.API.Interfaces.ServiceInterfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetNotifications();
        Task<Notification> GetNotificationById(string id);
        Task<bool> UpdateNotification(Notification notification);
        Task<bool> DeleteNotification(string id);
    }

}
