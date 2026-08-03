using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskmanager.DTOs;
using taskmanager.Extensions;
using taskmanager.Services;

namespace taskmanager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDtoResponse>>> GetNotifications([FromQuery] bool unreadOnly = false)
            => Ok(await _notificationService.GetMyNotificationsAsync(User.GetUserId(), unreadOnly));

        [HttpPatch("{id}/read")]
        public async Task<ActionResult<NotificationDtoResponse>> MarkAsRead(Guid id)
            => Ok(await _notificationService.MarkAsReadAsync(id, User.GetUserId()));
    }
}
