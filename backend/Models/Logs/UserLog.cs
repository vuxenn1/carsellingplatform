using System;
using System.ComponentModel.DataAnnotations;

namespace CARDB_EF.Models.EF.Logs
{
    public class UserLog
    {
        [Key] public int? LogId { get; set; }
        public int? UserId { get; set; }
        public string ActionType { get; set; }
        public DateTime? ActionTime { get; set; }
        public string ActionDetails { get; set; }
    }
}