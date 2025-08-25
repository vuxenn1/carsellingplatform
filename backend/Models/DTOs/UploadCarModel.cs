using System.ComponentModel.DataAnnotations;

namespace CARDB_EF.Models.DTOs
{
    public class UploadCarModel
    {
        [Required] public int ownerId { get; set; }
        [Required] public string brand { get; set; }
        [Required] public string model { get; set; }
        [Required] public int year { get; set; }
        [Required] public int km { get; set; }
        [Required] public string fuelType { get; set; }
        [Required] public string transmission { get; set; }
        [Required] public decimal price { get; set; }
        public string description { get; set; }
    }
}