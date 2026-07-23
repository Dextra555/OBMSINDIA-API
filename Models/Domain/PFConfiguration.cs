using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("PFConfiguration")]
    public class PFConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("Employee_Rate", TypeName = "decimal(5,2)")]
        public decimal EmployeeRate { get; set; } = 12.00m;

        [Required]
        [Column("Employer_Rate", TypeName = "decimal(5,2)")]
        public decimal EmployerRate { get; set; } = 12.00m;

        [Required]
        [Column("Employer_EPS_Rate", TypeName = "decimal(5,2)")]
        public decimal EmployerEPSRate { get; set; } = 8.33m;

        [Required]
        [Column("Basic_Salary_Limit", TypeName = "decimal(10,2)")]
        public decimal BasicSalaryLimit { get; set; } = 15000.00m;

        [Required]
        [Column("Effective_Date")]
        public DateTime EffectiveDate { get; set; } = DateTime.Now;

        [Required]
        [Column("Is_Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("Created_Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [Column("Financial_Year")]
        [StringLength(9)]
        public string FinancialYear { get; set; } = "2024-2025";
    }
}
