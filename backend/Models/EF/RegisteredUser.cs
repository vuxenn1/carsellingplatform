using System;
using System.ComponentModel.DataAnnotations;

namespace CARDB_EF.Models.EF
{
    public class RegisteredUser
    {
        [Key] public int? UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Mail { get; set; }
        public string Phone { get; set; }
        public string UserLocation { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? RecordTime { get; set; }
    }
}
