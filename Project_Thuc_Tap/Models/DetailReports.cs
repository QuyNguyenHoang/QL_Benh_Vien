using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Thuc_Tap.Models
{
    public class DetailReports
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int DetailId { get; set; }
        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
        public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Picture { get; set; }
        [MaxLength(50)]
        public string? Room { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }
        public DateTime? WorkingDay { get; set; }
        public string? TotalWorkHours { get; set; }
        public string? TotalOverTime { get;set; }
        public string? TotalCompensatoryLeave { get; set; }
        public string? TotalLate {  get; set; }
        public string? Notes {  get; set; }

    }
}
