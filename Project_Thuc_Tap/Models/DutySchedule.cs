using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Thuc_Tap.Models
{
    public class DutySchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DutyScheduleId { get; set; }


        [Required(ErrorMessage = "Vui lòng chọn nhân viên ")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required(ErrorMessage = "Tạo lịch trực phải chọn ngày trực chứ ný")]
        public DateTime? DutyDays { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ca trực!")]
        [StringLength(10)]
        public string? Shift { get; set; }

        public bool? IsOverTime { get; set; }


        public bool? Status { get; set; } = null;


        [StringLength(255)]
        public string? Description { get; set; }
    }
}
