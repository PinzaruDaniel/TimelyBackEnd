using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimelyBackEnd.DTOs.Notification;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send")]
    [Authorize]
    public async Task<IActionResult> SendNotification([FromBody] CreateNotificationDto dto)
    {
        await _notificationService.SendNotificationAsync(dto);
        return Ok(new { message = "Notification scheduled." });
    }

    [HttpGet("pending")]
    [Authorize]
    public async Task<IActionResult> GetPendingNotifications()
    {
        var notifications = await _notificationService.GetPendingNotificationsAsync();
        return Ok(notifications);
    }
}