using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecentraCloud.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public AdminNotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var notifications = await _notificationRepository.GetNotifications();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("resolve/{id}")]
        public async Task<IActionResult> MarkAsResolved(string id)
        {
            try
            {
                var notification = await _notificationRepository.GetNotificationById(id);
                if (notification == null)
                {
                    return NotFound("Notification not found.");
                }

                if (notification.IsResolved)
                {
                    return BadRequest("Notification is already resolved.");
                }

                notification.IsResolved = true;
                notification.ResolvedAt = DateTime.UtcNow;

                await _notificationRepository.UpdateNotification(notification);

                return Ok(new { message = "Notification marked as resolved." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            try
            {
                var notification = await _notificationRepository.GetNotificationById(id);
                if (notification == null)
                {
                    return NotFound("Notification not found.");
                }

                await _notificationRepository.DeleteNotification(id);

                return Ok(new { message = "Notification deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
