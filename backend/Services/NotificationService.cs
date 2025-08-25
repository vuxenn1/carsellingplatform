using CARDB_EF.Data;
using CARDB_EF.Models.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CARDB_EF.Services
{
    /// <summary>
    /// Manages creating and retrieving user notifications.
    /// </summary>
    public class NotificationService
    {
        private readonly CarDbContext cdb;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/>.
        /// </summary>
        /// <param name="cdb">The database context.</param>
        public NotificationService(CarDbContext cdb)
        {
            this.cdb = cdb;
        }

        /// <summary>
        /// Sends a notification to a user asynchronously.
        /// </summary>
        /// <param name="not">The notification object to send. It must contain a valid ReceiverId and MessageText.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> SendNotificationAsync(Notification not)
        {
            if (not == null || not.ReceiverId <= 0 || string.IsNullOrEmpty(not.MessageText))
                return (false, "Notification data is invalid.");

            not.SentTime = DateTime.UtcNow;
            not.IsRead = false;

            await cdb.Notifications.AddAsync(not);
            await cdb.SaveChangesAsync();

            return (true, $"Notification sent to user {not.ReceiverId}.");
        }

        /// <summary>
        /// Retrieves all notifications for a specific user, ordered by the most recent.
        /// </summary>
        /// <param name="userId">The ID of the user whose notifications are to be retrieved.</param>
        /// <returns>
        /// A list of <see cref="Notification"/> objects. Returns an empty list if the user ID is invalid or no notifications are found.
        /// Throws an exception on a database error.
        /// </returns>
        public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
        {
            if (userId <= 0)
                return new List<Notification>();

            return await cdb.Notifications
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.SentTime)
                .ToListAsync();
        }

        /// <summary>
        /// Gets the count of unread notifications for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// The integer count of unread notifications. Returns 0 if the user ID is invalid.
        /// Throws an exception on a database error.
        /// </returns>
        public async Task<int> GetUnreadNotificationCountAsync(int userId)
        {
            if (userId <= 0)
                return 0;

            return await cdb.Notifications.CountAsync(n => n.ReceiverId == userId && n.IsRead == false);
        }

        /// <summary>
        /// Marks all unread notifications for a user as read asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user whose notifications will be marked as read.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> MarkAllAsReadAsync(int userId)
        {
            if (userId <= 0)
                return (false, "Invalid User ID.");

            var unreadNotifications = await cdb.Notifications
                .Where(n => n.ReceiverId == userId && n.IsRead == false)
                .ToListAsync();

            if (!unreadNotifications.Any())
                return (true, "No unread notifications to mark as read.");

            foreach (var notification in unreadNotifications)
                notification.IsRead = true;

            await cdb.SaveChangesAsync();
            return (true, "All notifications marked as read.");
        }
    }
}