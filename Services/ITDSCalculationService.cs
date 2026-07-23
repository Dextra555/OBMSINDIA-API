using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface ITDSCalculationService
    {
        TDSCalculationResult CalculateTDS(decimal annualIncome, int ageGroup = 1);
        decimal GetTDSDeduction(decimal annualIncome, int ageGroup = 1);
    }

    public class TDSCalculationResult
    {
        public decimal AnnualIncome { get; set; }
        public int AgeGroup { get; set; } // 1: <60, 2: 60-80, 3: >80
        public decimal TaxableIncome { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Surcharge { get; set; }
        public decimal EducationCess { get; set; }
        public decimal TotalTax { get; set; }
        public decimal MonthlyTDS { get; set; }
        public bool IsApplicable { get; set; }
        public List<TDSSlabDetail> SlabDetails { get; set; } = new List<TDSSlabDetail>();
    }

    public class TDSSlabDetail
    {
        public decimal MinIncome { get; set; }
        public decimal? MaxIncome { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxableAmount { get; set; }
    }
}
