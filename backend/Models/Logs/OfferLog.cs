using System;
using System.ComponentModel.DataAnnotations;

namespace CARDB_EF.Models.EF.Logs
{
    public class OfferLog
    {
        [Key] public int? LogId { get; set; }
        public int? OfferId { get; set; }
        public string ActionType { get; set; }
        public DateTime? ActionTime { get; set; }
        public string ActionDetails { get; set; }
    }
}