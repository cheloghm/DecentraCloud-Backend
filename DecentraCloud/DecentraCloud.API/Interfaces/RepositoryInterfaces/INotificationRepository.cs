﻿using DecentraCloud.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecentraCloud.API.Interfaces.RepositoryInterfaces
{
    public interface INotificationRepository
    {
        Task AddNotification(Notification notification);
        Task<IEnumerable<Notification>> GetNotifications();
        Task<Notification> GetNotificationById(string id);
        Task UpdateNotification(Notification notification);
        Task DeleteNotification(string id);
    }
}
