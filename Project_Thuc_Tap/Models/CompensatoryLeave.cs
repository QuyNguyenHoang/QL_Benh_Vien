using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project_Thuc_Tap.Models
{
    public class CompensatoryLeave
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompensatoryLeaveId { get; set; }

        // Khóa ngoại trỏ đến AspNetUsers
        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
        public DateTime? CompensatoryDays { get; set; }
        public string? Shift { get; set; }

        public bool? Status { get; set; } = false;
    }
}
