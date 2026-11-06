using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface ITDSCalculationService
    {
        /// <summary>Primary method: pass monthly gross + period month for correct FY-remaining-months TDS.</summary>
        TDSCalculationResult CalculateTDS(decimal grossMonthlyIncome, int periodMonth, int ageGroup = 1);

        /// <summary>Overload for annual income: projects over a full 12-month FY, returns monthly TDS = annual tax ÷ 12.</summary>
        TDSCalculationResult CalculateTDS(decimal annualIncome, int ageGroup = 1);

        /// <summary>Phase 2: full computation with joining date, FY, regime, previous-employer income/TDS and YTD.</summary>
        TDSCalculationResult CalculateTDS(TDSComputationInput input);

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

        // ── Phase 2: computation metadata for payslip / auditor statement ──
        public string FinancialYear { get; set; } = string.Empty;
        public string Regime { get; set; } = "New";
        public decimal StandardDeduction { get; set; }
        public decimal RebateAmount { get; set; }
        public decimal BalanceTax { get; set; }
        public int EligibleSalaryMonths { get; set; }
        public int RemainingDeductionMonths { get; set; }
        public decimal PreviousEmployerIncome { get; set; }
        public decimal PreviousEmployerTDS { get; set; }
        public decimal YtdTDS { get; set; }
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
