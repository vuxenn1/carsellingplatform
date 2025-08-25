namespace CARDB_EF.Models.DTOs
{
    public class UserUpdateDto
    {
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string Mail { get; set; }
        public string Phone { get; set; }
        public string UserLocation { get; set; }
    }
}