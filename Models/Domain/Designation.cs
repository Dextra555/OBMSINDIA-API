using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("Designation")]
    public class Designation
    {
        [Key]
        public int DesignationId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Desig_Code")]
        public string Desig_Code { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("Desig_Name")]
        public string Desig_Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [MaxLength(50)]
        public string? UpdatedBy { get; set; }

        public int? DepartmentId { get; set; }

        // Additional property for compatibility
        public string DesignationName => Desig_Name;
    }
}
