using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    /// <summary>
    /// Inputs for a single TDS computation (one employee, one pay period).
    /// </summary>
    public class TDSComputationInput
    {
        /// <summary>Gross salary for the current month (monthly amount).</summary>
        public decimal GrossMonthlyIncome { get; set; }

        /// <summary>Calendar month of the current pay period (1=Jan … 12=Dec).</summary>
        public int PeriodMonth { get; set; }

        /// <summary>Calendar year of the current pay period.</summary>
        public int PeriodYear { get; set; }

        /// <summary>Employee joining date (drives eligible salary months within the FY).</summary>
        public DateTime? JoiningDate { get; set; }

        /// <summary>Employee resignation/leaving date (caps eligible months).</summary>
        public DateTime? ResignDate { get; set; }

        /// <summary>Financial Year start year, e.g. 2026 for FY 2026-2027.</summary>
        public int FinancialYearStart { get; set; }

        /// <summary>"New" or "Old".</summary>
        public string Regime { get; set; } = "New";

        /// <summary>Gross income earned with a previous employer in the same FY (if declared).</summary>
        public decimal PreviousEmployerIncome { get; set; }

        /// <summary>TDS already deducted by a previous employer in the same FY (if declared).</summary>
        public decimal PreviousEmployerTDS { get; set; }

        /// <summary>TDS already deducted by THIS employer in the FY, for periods before the current one.</summary>
        public decimal YtdTDS { get; set; }

        /// <summary>Age group: 1 = &lt;60, 2 = 60–80, 3 = &gt;80 (used only under Old Regime).</summary>
        public int AgeGroup { get; set; } = 1;
    }

    /// <summary>
    /// FY + Regime-level tax configuration values (read from DB, not hardcoded).
    /// </summary>
    public class TDSFYConfig
    {
        public decimal StandardDeduction { get; set; }
        public decimal RebateThreshold { get; set; }
        public decimal RebateAmount { get; set; }
        /// <summary>Health &amp; Education Cess rate as a percent, e.g. 4 for 4%.</summary>
        public decimal EducationCessRate { get; set; }
        public bool ApplyMarginalRelief { get; set; }
    }

    /// <summary>
    /// Pure, database-independent TDS math. The single source of truth for the
    /// tax computation. <see cref="TDSCalculationService"/> loads config + slabs
    /// from the database and delegates the actual arithmetic here, which keeps
    /// the engine fully unit-testable without EF/SQL.
    /// </summary>
    public static class TdsTaxMath
    {
        /// <summary>
        /// Months remaining in the Indian FY (Apr–Mar) from the given period month,
        /// inclusive of the period month. Apr=12, May=11 … Jan=3, Feb=2, Mar=1.
        /// Used to spread remaining tax over the remaining deduction months.
        /// </summary>
        public static int RemainingDeductionMonths(int periodMonth)
            => ((3 - periodMonth + 12) % 12) + 1;

        /// <summary>Full months between two dates inclusive of both end months.</summary>
        public static int MonthsBetweenInclusive(DateTime start, DateTime end)
            => (end.Year - start.Year) * 12 + (end.Month - start.Month) + 1;

        /// <summary>
        /// Eligible salary months within the FY, from joining (or FY start) to
        /// resignation (or FY end). Capped 0..12.
        /// </summary>
        public static int EligibleSalaryMonths(DateTime? joiningDate, DateTime? resignDate, int fyStartYear)
        {
            var fyStart = new DateTime(fyStartYear, 4, 1);
            var fyEnd = new DateTime(fyStartYear + 1, 3, 31);

            if (joiningDate.HasValue && joiningDate.Value > fyEnd)
                return 0; // joins after this FY ends

            DateTime periodStart = (joiningDate.HasValue && joiningDate.Value > fyStart)
                ? joiningDate.Value
                : fyStart;

            if (resignDate.HasValue && resignDate.Value >= fyStart && resignDate.Value <= fyEnd)
                return Math.Max(0, Math.Min(12, MonthsBetweenInclusive(periodStart, resignDate.Value)));

            return Math.Max(0, Math.Min(12, MonthsBetweenInclusive(periodStart, fyEnd)));
        }

        /// <summary>
        /// Slab-wise tax using contiguous slabs (Min of each slab = Max of previous).
        /// Returns the total slab tax and fills per-slab details.
        /// </summary>
        public static decimal SlabTax(decimal taxableIncome, List<TDSSlabConfiguration> slabs, List<TDSSlabDetail> slabDetails)
        {
            decimal totalTax = 0;
            decimal remainingIncome = taxableIncome;

            foreach (var slab in slabs.OrderBy(s => s.MinIncome))
            {
                if (remainingIncome <= 0) break;

                decimal slabIncome;
                if (!slab.MaxIncome.HasValue)
                {
                    slabIncome = remainingIncome;
                }
                else
                {
                    decimal slabWidth = slab.MaxIncome.Value - slab.MinIncome;
                    slabIncome = Math.Min(remainingIncome, slabWidth);
                }

                decimal slabTaxAmount = slabIncome * slab.TaxRate / 100m;
                totalTax += slabTaxAmount;

                slabDetails?.Add(new TDSSlabDetail
                {
                    MinIncome = slab.MinIncome,
                    MaxIncome = slab.MaxIncome,
                    TaxRate = slab.TaxRate,
                    TaxableAmount = slabIncome,
                    TaxAmount = slabTaxAmount
                });

                remainingIncome -= slabIncome;
            }

            return totalTax;
        }

        /// <summary>
        /// Section 87A rebate with optional marginal relief (New Regime).
        /// </summary>
        public static decimal Rebate87A(
            decimal taxableIncome, decimal taxAfterSlabs,
            decimal rebateThreshold, decimal rebateAmount, bool applyMarginalRelief)
        {
            if (taxAfterSlabs <= 0) return 0;

            if (taxableIncome <= rebateThreshold)
                return Math.Min(taxAfterSlabs, rebateAmount);

            if (applyMarginalRelief)
            {
                // Just above threshold: cap tax at the excess over threshold.
                decimal excess = taxableIncome - rebateThreshold;
                if (taxAfterSlabs > excess)
                    return Math.Max(0, taxAfterSlabs - excess);
            }

            return 0;
        }

        /// <summary>
        /// Full TDS computation. Pure: no side effects, no DB access.
        /// </summary>
        public static TDSCalculationResult Compute(
            TDSComputationInput input, TDSFYConfig fyConfig, List<TDSSlabConfiguration> slabs)
        {
            int eligibleMonths = EligibleSalaryMonths(input.JoiningDate, input.ResignDate, input.FinancialYearStart);
            if (eligibleMonths <= 0)
                return ZeroResult(input, fyConfig);

            // Project annual income = monthly gross × eligible months + previous employer income.
            decimal projectedAnnualIncome =
                input.GrossMonthlyIncome * eligibleMonths + input.PreviousEmployerIncome;

            decimal standardDeduction = Math.Min(fyConfig.StandardDeduction, projectedAnnualIncome);
            decimal taxableIncome = Math.Max(0m, projectedAnnualIncome - standardDeduction);

            var result = new TDSCalculationResult
            {
                AnnualIncome = projectedAnnualIncome,
                AgeGroup = input.AgeGroup,
                TaxableIncome = taxableIncome,
                IsApplicable = taxableIncome > 0
            };

            if (!result.IsApplicable)
                return ZeroResult(input, fyConfig, projectedAnnualIncome, eligibleMonths);

            // Slab tax
            result.TaxAmount = SlabTax(taxableIncome, slabs, result.SlabDetails);

            // 87A rebate (with marginal relief for New Regime)
            decimal rebate = Rebate87A(taxableIncome, result.TaxAmount,
                fyConfig.RebateThreshold, fyConfig.RebateAmount, fyConfig.ApplyMarginalRelief);
            result.RebateAmount = rebate;

            decimal taxAfterRebate = Math.Max(0m, result.TaxAmount - rebate);

            // Surcharge not applied for typical salary income
            result.Surcharge = 0m;

            // Cess on tax after rebate (rate from DB config)
            decimal cessRate = fyConfig.EducationCessRate;
            result.EducationCess = taxAfterRebate * cessRate / 100m;

            decimal totalTax = taxAfterRebate + result.EducationCess;
            result.TotalTax = totalTax;

            // Remaining tax = total − previous employer TDS − YTD TDS already deducted here
            decimal remainingTax = totalTax - input.PreviousEmployerTDS - input.YtdTDS;
            result.BalanceTax = Math.Max(0m, remainingTax);

            int remainingMonths = RemainingDeductionMonths(input.PeriodMonth);
            result.MonthlyTDS = remainingMonths > 0
                ? Math.Round(result.BalanceTax / remainingMonths, 2, MidpointRounding.AwayFromZero)
                : 0m;

            // Computation metadata (for UI / auditor statement)
            result.FinancialYear = $"{input.FinancialYearStart}-{input.FinancialYearStart + 1}";
            result.Regime = input.Regime;
            result.EligibleSalaryMonths = eligibleMonths;
            result.RemainingDeductionMonths = remainingMonths;
            result.PreviousEmployerIncome = input.PreviousEmployerIncome;
            result.PreviousEmployerTDS = input.PreviousEmployerTDS;
            result.YtdTDS = input.YtdTDS;
            result.StandardDeduction = standardDeduction;

            return result;
        }

        private static TDSCalculationResult ZeroResult(TDSComputationInput input, TDSFYConfig fyConfig,
            decimal projectedAnnualIncome = 0m, int eligibleMonths = 0)
        {
            return new TDSCalculationResult
            {
                AnnualIncome = projectedAnnualIncome,
                AgeGroup = input.AgeGroup,
                TaxableIncome = 0,
                TaxAmount = 0,
                Surcharge = 0,
                EducationCess = 0,
                TotalTax = 0,
                RebateAmount = 0,
                BalanceTax = 0,
                MonthlyTDS = 0,
                IsApplicable = false,
                FinancialYear = $"{input.FinancialYearStart}-{input.FinancialYearStart + 1}",
                Regime = input.Regime,
                EligibleSalaryMonths = eligibleMonths,
                RemainingDeductionMonths = RemainingDeductionMonths(input.PeriodMonth),
                PreviousEmployerIncome = input.PreviousEmployerIncome,
                PreviousEmployerTDS = input.PreviousEmployerTDS,
                YtdTDS = input.YtdTDS,
                StandardDeduction = Math.Min(fyConfig.StandardDeduction, projectedAnnualIncome)
            };
        }

        /// <summary>Indian FY start year for a given date (FY Apr–Mar).</summary>
        public static int FinancialYearStart(DateTime date) => date.Month >= 4 ? date.Year : date.Year - 1;

        /// <summary>Financial Year string e.g. "2026-2027" for a given date.</summary>
        public static string FinancialYearString(DateTime date)
        {
            int s = FinancialYearStart(date);
            return $"{s}-{s + 1}";
        }
    }
}
