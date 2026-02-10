using System.Security.Claims;
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

    [HttpPost("register-token")]
    [Authorize]
    public async Task<IActionResult> RegisterToken([FromBody] string token)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.RegisterFcmTokenAsync(userId, token);
        return Ok(new { message = "FCM token registered successfully." });
    }

    [HttpPost("chat-notification")]
    [Authorize]
    public async Task<IActionResult> SendChatNotification([FromBody] ChatNotificationDto dto)
    {
        var senderId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.SendChatNotificationAsync(senderId, dto.RecipientId, dto.Message);
        return Ok(new { message = "Chat notification sent." });
    }

    [HttpPost("group-chat-notification")]
    [Authorize]
    public async Task<IActionResult> SendGroupChatNotification([FromBody] GroupChatNotificationDto dto)
    {
        var senderId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.SendGroupChatNotificationAsync(senderId, dto.GroupId, dto.Message);
        return Ok(new { message = "Group chat notification sent." });
    }
}

public class ChatNotificationDto
{
    public Guid RecipientId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class GroupChatNotificationDto
{
    public Guid GroupId { get; set; }
    public string Message { get; set; } = string.Empty;
}