using System.ComponentModel.DataAnnotations;

namespace Project_Thuc_Tap.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        public string? DepartmentName { get; set; }
        [MaxLength(100)]

        public string? Status { get; set; }
    }
}
