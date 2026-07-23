using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("GSTConfiguration")]
    public class GSTConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal GSTRate { get; set; }

        [Required]
        [StringLength(8)]
        public string HSNCode { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? ApplicableFromDate { get; set; }

        public DateTime? ApplicableTillDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        [StringLength(50)]
        public string? LastUpdatedBy { get; set; }
    }
}
