using System.ComponentModel.DataAnnotations;

namespace OBMS.WebAPI.Models.DTO
{
    public class DepartmentDTO
    {
        public int DepartmentId { get; set; }

        [MaxLength(100)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [MaxLength(50)]
        public string? UpdatedBy { get; set; }
    }
}
