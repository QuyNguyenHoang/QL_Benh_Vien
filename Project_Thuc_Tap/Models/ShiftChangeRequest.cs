using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project_Thuc_Tap.Models
{
    public class ShiftChangeRequest

    {
        [Key]
        public int ShiftChangeRequestId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public DateTime ShiftChangeDay { get; set; }
        public string? Reason { get; set; }
    }
}
