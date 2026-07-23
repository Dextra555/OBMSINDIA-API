using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("QuotationDetails")]
    public class QuotationDetails
    {
        [Key]
        public int ID { get; set; }

        public int QuotationID { get; set; }

        [Required]
        public DateTime QuotationDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Client { get; set; }

        [Required]
        [StringLength(20)]
        public string Branch { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public int NoOfGuards { get; set; }

        public decimal PerDay { get; set; }

        public decimal PerMonth { get; set; }

        [Required]
        public decimal Rate { get; set; }

        [Required]
        public decimal NoOfHours { get; set; }

        [Required]
        public decimal NoOfDays { get; set; }

        [Required]
        public bool FollowCalender { get; set; }

        [Required]
        public bool HasDiscount { get; set; }

        [Required]
        public decimal DiscountAmount { get; set; }

        [Required]
        public int DiscountHour { get; set; }

        [Required]
        public bool IsTaxable { get; set; }

        [Required]
        public decimal TaxAmount { get; set; }

        [Required]
        public decimal MonthTotal { get; set; }

        [Required]
        public DateTime LASTUPDATE { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string? LastUpdatedBy { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(250)]
        public string Reason { get; set; }

        public decimal Basic { get; set; }
        public decimal DA { get; set; }
        public decimal Leaves { get; set; }
        public decimal LeavesPercentage { get; set; }
        public decimal Allowance { get; set; }
        public decimal Bonus { get; set; }
        public decimal BonusPercentage { get; set; }
        public decimal NFH { get; set; }
        public decimal PF { get; set; }
        public decimal PFPercentage { get; set; }
        public decimal ESI { get; set; }
        public decimal ESIPercentage { get; set; }
        public decimal Uniform { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal HRA { get; set; }
        public decimal HRAPercentage { get; set; }
        public decimal ProfessionalTax { get; set; }
        public decimal RelieverCharges { get; set; }
        public decimal RelieverChargesPercentage { get; set; }
        public decimal Others { get; set; }
        public decimal OthersPercentage { get; set; }
        public decimal AdministrationCharges { get; set; }
        public decimal AdministrationChargesPercentage { get; set; }
        public decimal ManagementFee { get; set; }
        public decimal ManagementFeePercentage { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPlusStatutory { get; set; }
        public decimal TotalDirectCost { get; set; }
        public decimal MonthlyChargedCost { get; set; }
    }
}
