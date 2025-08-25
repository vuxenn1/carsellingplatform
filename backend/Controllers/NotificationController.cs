using Microsoft.AspNetCore.Mvc;
using CARDB_EF.Services;
using CARDB_EF.Models.EF;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CARDB_EF.Controllers
{
    [ApiController]
    [Route("api/notification")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService notificationService;

        public NotificationController(NotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var currentUserIdString = User.FindFirstValue("userId");
            if (userId.ToString() != currentUserIdString)
            {
                return Forbid();
            }
            var notifications = await notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("user/{userId}/unread")]
        public async Task<IActionResult> GetUnreadNotificationCount(int userId)
        {
            var currentUserIdString = User.FindFirstValue("userId");
            if (userId.ToString() != currentUserIdString)
            {
                return Forbid();
            }
            var count = await notificationService.GetUnreadNotificationCountAsync(userId);
            return Ok(count);
        }

        [HttpPut("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            var currentUserIdString = User.FindFirstValue("userId");
            if (userId.ToString() != currentUserIdString)
            {
                return Forbid();
            }
            var result = await notificationService.MarkAllAsReadAsync(userId);
            if (!result.Success)
            {
                return StatusCode(500, new { message = result.Message });
            }
            return NoContent();
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] Notification not)
        {
            var result = await notificationService.SendNotificationAsync(not);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}