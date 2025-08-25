namespace CARDB_EF.Models.DTOs
{
    public class CarUpdateDto
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int KM { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }
}