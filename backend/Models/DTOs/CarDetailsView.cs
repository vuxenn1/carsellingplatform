using System;

namespace CARDB_EF.Models.DTOs
{
    public class CarDetailsView
    {
        public int? CarId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int? Year { get; set; }
        public int? KM { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string CarStatus { get; set; }
        public DateTime? ListDate { get; set; }

        public int? OwnerId { get; set; }
        public string OwnerUsername { get; set; }
        public string OwnerLocation { get; set; }
    }
}