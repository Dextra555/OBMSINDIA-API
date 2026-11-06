using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;

namespace OBMS.WebAPI.Services
{
    public class TDSCalculationService : ITDSCalculationService
    {
        private readonly OBMSDbContext _context;

        public TDSCalculationService(OBMSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Phase 2 primary computation: monthly gross + period month.
        /// Uses the FY/Regime derived from the period date (New Regime by default),
        /// full-FY eligible months (joining date not supplied), no previous-employer
        /// income and no YTD. Backwards compatible with existing callers.
        /// </summary>
        public TDSCalculationResult CalculateTDS(decimal grossMonthlyIncome, int periodMonth, int ageGroup = 1)
        {
            // Derive FY from the current month. periodMonth alone doesn't give the year,
            // so use today's date for the FY (the payslip path supplies the full input).
            int fyStart = TdsTaxMath.FinancialYearStart(DateTime.Now);

            var input = new TDSComputationInput
            {
                GrossMonthlyIncome = grossMonthlyIncome,
                PeriodMonth = periodMonth,
                PeriodYear = DateTime.Now.Year,
                FinancialYearStart = fyStart,
                Regime = "New",
                AgeGroup = ageGroup
            };
            return CalculateTDS(input);
        }

        /// <summary>Overload for annual income: projects over a full 12-month FY
        /// (April → remainingFYMonths = 12) so projectedAnnualIncome = annualIncome and MonthlyTDS = annual tax ÷ 12.</summary>
        public TDSCalculationResult CalculateTDS(decimal annualIncome, int ageGroup = 1)
            => CalculateTDS(annualIncome / 12m, 4, ageGroup); // April → remainingFYMonths=12 → projectedAnnual = annualIncome, MonthlyTDS = annualTax/12

        /// <summary>
        /// Phase 2 full computation. The single entry point used by the payroll engine
        /// and the TDS computation/preview API. All tax values are read from the database
        /// for the requested Financial Year + Regime; nothing is hardcoded.
        /// </summary>
        public TDSCalculationResult CalculateTDS(TDSComputationInput input)
        {
            string regime = string.IsNullOrWhiteSpace(input.Regime) ? "New" : input.Regime;
            string fyString = $"{input.FinancialYearStart}-{input.FinancialYearStart + 1}";

            var slabs = GetSlabs(fyString, regime);
            var fyConfig = BuildFYConfig(slabs, regime);

            // No slabs configured for this FY/Regime → tax not applicable (safeguard).
            if (slabs.Count == 0)
                return new TDSCalculationResult
                {
                    FinancialYear = fyString,
                    Regime = regime,
                    IsApplicable = false
                };

            return TdsTaxMath.Compute(input, fyConfig, slabs);
        }

        public decimal GetTDSDeduction(decimal annualIncome, int ageGroup = 1)
        {
            var result = CalculateTDS(annualIncome, ageGroup);
            return result.MonthlyTDS;
        }

        // ── Database access: slabs + FY/regime config (nothing hardcoded) ──

        private List<TDSSlabConfiguration> GetSlabs(string financialYear, string regime)
        {
            return _context.Set<TDSSlabConfiguration>()
                .Where(c => c.IsActive
                         && c.FinancialYear == financialYear
                         && c.Regime == regime)
                .OrderBy(c => c.MinIncome)
                .ToList();
        }

        /// <summary>
        /// Builds the FY + Regime config from the first slab row (denormalised columns:
        /// Standard_Deduction, Rebate_Threshold, Rebate_Amount, Education_Cess_Rate).
        /// Falls back to New-Regime-safe defaults if columns are null (legacy rows).
        /// </summary>
        private TDSFYConfig BuildFYConfig(List<TDSSlabConfiguration> slabs, string regime)
        {
            var first = slabs.FirstOrDefault();
            if (first == null)
                return new TDSFYConfig();

            return new TDSFYConfig
            {
                StandardDeduction = first.StandardDeduction ?? 75000m,
                RebateThreshold = first.RebateThreshold ?? 0m,
                RebateAmount = first.RebateAmount ?? 0m,
                EducationCessRate = first.EducationCessRate ?? 4m,
                ApplyMarginalRelief = string.Equals(regime, "New", StringComparison.OrdinalIgnoreCase)
            };
        }
    }
}
