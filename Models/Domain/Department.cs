using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("Department")]
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Dept_Code")]
        public string Dept_Code { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("Dept_Name")]
        public string Dept_Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [MaxLength(50)]
        public string? UpdatedBy { get; set; }

        // Additional property for compatibility
        public string DepartmentName => Dept_Name;
    }
}
