using System;
using System.ComponentModel.DataAnnotations;

namespace CARDB_EF.Models.EF
{
    public class Notification
    {
        [Key] public int? NotificationId { get; set; }
        public string MessageText { get; set; }
        public int? ReceiverId { get; set; }
        public DateTime? SentTime { get; set; }
        public bool? IsRead { get; set; }
    }
}
