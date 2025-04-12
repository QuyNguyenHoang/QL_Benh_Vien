using Microsoft.Build.Construction;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QRCoder;

namespace Project_Thuc_Tap.Models
{
    public class TimeKeeping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TimeKeepingId { get; set; }

        
        [ForeignKey("User")]
        public string? Id { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        public string? TimeIn { get; set; }
        public string? TimeOut { get; set; }
        public string? TimeLate { get; set; }
        public string? Shift { get; set; }
        public bool? IsOverTime { get; set; }
        public string? Description { get; set; }

        
        public User? User { get; set; }
    }
}
