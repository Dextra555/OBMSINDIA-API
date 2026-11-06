using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("TDSSlabConfiguration")]
    public class TDSSlabConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("Financial_Year")]
        [StringLength(9)]
        public string FinancialYear { get; set; } = "2024-2025";

        [Required]
        [Column("Min_Income", TypeName = "decimal(12,2)")]
        public decimal MinIncome { get; set; }

        [Column("Max_Income", TypeName = "decimal(12,2)")]
        public decimal? MaxIncome { get; set; }

        [Required]
        [Column("Tax_Rate", TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; }

        [Column("Surcharge_Rate", TypeName = "decimal(5,2)")]
        public decimal? SurchargeRate { get; set; }

        [Column("Education_Cess_Rate", TypeName = "decimal(5,2)")]
        public decimal? EducationCessRate { get; set; }

        [Column("Higher_Education_Cess_Rate", TypeName = "decimal(5,2)")]
        public decimal? HigherEducationCessRate { get; set; }

        [Required]
        [Column("Is_Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("Created_Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ── Phase 2: FY / Regime-level tax configuration (denormalised per slab row) ──

        /// <summary>Tax regime: "New" or "Old". Defaults to "New".</summary>
        [Column("Regime", TypeName = "varchar(10)")]
        public string Regime { get; set; } = "New";

        /// <summary>
        /// Standard deduction for this FY + Regime (New Regime salaried = ₹75,000,
        /// Old Regime salaried = ₹50,000). Read by the engine from the first slab of the FY.
        /// </summary>
        [Column("Standard_Deduction", TypeName = "decimal(12,2)")]
        public decimal? StandardDeduction { get; set; }

        /// <summary>
        /// Section 87A rebate threshold: taxable income at/ below which the rebate applies.
        /// New Regime FY 2026-27 = ₹12,00,000. Set 0 to disable rebate (historical behaviour).
        /// </summary>
        [Column("Rebate_Threshold", TypeName = "decimal(12,2)")]
        public decimal? RebateThreshold { get; set; }

        /// <summary>
        /// Maximum Section 87A rebate amount. New Regime FY 2026-27 = ₹60,000.
        /// </summary>
        [Column("Rebate_Amount", TypeName = "decimal(12,2)")]
        public decimal? RebateAmount { get; set; }

        /// <summary>Effective date this slab configuration applies from.</summary>
        [Column("Effective_Date")]
        public DateTime? EffectiveDate { get; set; }
    }
}
