using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project_Thuc_Tap.Models
{
  
        public class Report
        {
            [Key]
            public int ReportId { get; set; }

            [Required]
            public string? ReportDate { get; set; }

            [Required]
            public string? UserId { get; set; }

            [ForeignKey("UserId")]
            public User? User { get; set; }

            public string? TotalWorkHours { get; set; }
            public string? TotalOverTime { get; set; }
            public int TotalCompensatoryDays { get; set; }

            public string? TotalLateHours { get; set; }

            [StringLength(255)]
            public string? Notes { get; set; }
        }
    
}
