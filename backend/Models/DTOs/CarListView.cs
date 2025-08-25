using System;

namespace CARDB_EF.Models.DTOs
{
    public class CarListView
    {
        public int CarId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int KM { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime? ListDate { get; set; }

        public int OwnerId { get; set; }
        public string OwnerUsername { get; set; }
        public string OwnerLocation { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}