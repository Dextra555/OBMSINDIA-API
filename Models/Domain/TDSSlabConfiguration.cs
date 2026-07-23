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
    }
}
