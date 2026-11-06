using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    /// <summary>
    /// Per-employee / per-Financial-Year TDS configuration.
    /// Captures the employee's chosen tax regime (Old / New) for the FY and any
    /// previous-employer salary income &amp; TDS already deducted (Section 192(2) / Form 12B).
    /// Used by the TDS engine to compute projected annual taxable income and the
    /// remaining monthly TDS deduction.
    /// </summary>
    [Table("EmployeeTDSConfiguration")]
    public class EmployeeTDSConfiguration
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        [Required]
        [StringLength(9)]
        public string FinancialYear { get; set; } = string.Empty;

        /// <summary>"New" or "Old". Defaults to "New" (the default regime in India).</summary>
        [Required]
        [StringLength(10)]
        public string TaxRegime { get; set; } = "New";

        /// <summary>Gross income from a previous employer in the same FY (if any).</summary>
        [Column(TypeName = "decimal(12,2)")]
        public decimal PreviousEmployerIncome { get; set; } = 0m;

        /// <summary>TDS already deducted by a previous employer in the same FY (if any).</summary>
        [Column(TypeName = "decimal(12,2)")]
        public decimal PreviousEmployerTDS { get; set; } = 0m;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedDate { get; set; }
        [StringLength(50)]
        public string? LastUpdatedBy { get; set; }
    }
}
