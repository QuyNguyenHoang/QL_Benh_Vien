using Microsoft.AspNetCore.Identity;

namespace Project_Thuc_Tap.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool? Sex { get; set; } 
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Picture { get; set; }

        // Khóa ngoại tới bảng Phong
        public int? RoomId { get; set; }
        public Room? Room { get; set; }
    }
}
