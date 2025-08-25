using System;
using System.ComponentModel.DataAnnotations;

namespace CARDB_EF.Models.EF
{
    public class UserFavorite
    {
        [Key] public int? FavId { get; set; }
        public int? UserId { get; set; }
        public int? CarId { get; set; }
        public DateTime? AddTime { get; set; }
    }
}