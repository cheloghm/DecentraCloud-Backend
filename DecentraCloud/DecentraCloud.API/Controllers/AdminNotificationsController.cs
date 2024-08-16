using DecentraCloud.API.Interfaces.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminNotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public AdminNotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var notifications = await _notificationService.GetNotifications(pageNumber, pageSize);
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
            var notification = await _notificationService.GetNotificationById(id);
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

            var success = await _notificationService.UpdateNotification(notification);
            if (!success)
            {
                return StatusCode(500, "Failed to update notification.");
            }

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
            var notification = await _notificationService.GetNotificationById(id);
            if (notification == null)
            {
                return NotFound("Notification not found.");
            }

            var success = await _notificationService.DeleteNotification(id);
            if (!success)
            {
                return StatusCode(500, "Failed to delete notification.");
            }

            return Ok(new { message = "Notification deleted." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
