using System;
using System.ComponentModel.DataAnnotations;

namespace CARDB_EF
{
    public class CarImage
    {
        [Key] public int ImageId { get; set; }

        public string ImageUrl { get; set; }

        public int CarId { get; set; }

        public string AltText { get; set; }

        public DateTime UploadTime { get; set; }
    }
}