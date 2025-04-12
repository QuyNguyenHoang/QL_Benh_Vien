using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project_Thuc_Tap.Models
{
    public class ShiftChangeReceiver
    {
        [Key]
        public int ShiftChangeReceiverId { get; set; }


        [ForeignKey("UserId")]
        public User? User { get; set; }
        public int ShiftChangeRequestId { get; set; }
        public string? Status { get; set; }
    }
}
