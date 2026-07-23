using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("ProfessionalTaxConfiguration")]
    public class ProfessionalTaxConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string State { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MinSalary { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string TaxPeriod { get; set; } = "Monthly"; // Monthly, SemiAnnual (6 months), Annual

        [Required]
        public DateTime EffectiveDate { get; set; } = DateTime.Now;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        [StringLength(50)]
        public string? LastUpdatedBy { get; set; }
    }
}
