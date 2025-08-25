using System;
using System.ComponentModel.DataAnnotations;

namespace CARDB_EF.Models.EF
{
    public class Offer
    {
        [Key] public int? OfferId { get; set; }
        public int? CarId { get; set; }
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public decimal? OfferPrice { get; set; }
        public DateTime? OfferTime { get; set; }
        public string OfferStatus { get; set; }
    }
}