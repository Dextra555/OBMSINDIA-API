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
        /// Calculate TDS per India Act FY 2026-27 New Regime.
        /// </summary>
        /// <param name="grossMonthlyIncome">Employee gross pay for the current month.</param>
        /// <param name="periodMonth">Calendar month of the salary period (1=Jan … 12=Dec).</param>
        /// <param name="ageGroup">Retained for signature compatibility; not used under New Regime.</param>
        public TDSCalculationResult CalculateTDS(decimal grossMonthlyIncome, int periodMonth, int ageGroup = 1)
        {
            // ── Step 1: Remaining FY months (Indian FY: Apr–Mar) ─────────────
            // Apr=12, May=11, Jun=10, Jul=9, Aug=8, Sep=7, Oct=6, Nov=5,
            // Dec=4,  Jan=3,  Feb=2,  Mar=1
            int remainingFYMonths = ((3 - periodMonth + 12) % 12) + 1;

            // ── Step 2: Projected income for remaining FY months ─────────────
            decimal projectedAnnualIncome = grossMonthlyIncome * remainingFYMonths;

            var result = new TDSCalculationResult
            {
                AnnualIncome = projectedAnnualIncome,
                AgeGroup = ageGroup
            };

            // ── Step 3: Standard deduction ₹75,000 (New Regime, Budget 2024) ─
            decimal standardDeduction = Math.Min(75000m, projectedAnnualIncome);

            // ── Step 4: Taxable income (New Regime — 0% slab in DB covers 4L) ─
            result.TaxableIncome = Math.Max(0, projectedAnnualIncome - standardDeduction);
            result.IsApplicable = result.TaxableIncome > 0;

            if (result.IsApplicable)
            {
                // ── Step 5: Slab tax (active slabs = 2026-27 New Regime from DB) ─
                var taxSlabs = GetActiveTDSSlabs();
                result.TaxAmount = CalculateTaxBySlabs(result.TaxableIncome, taxSlabs, result.SlabDetails);

                // ── Step 6: No surcharge for typical salary income ────────────
                result.Surcharge = 0;

                // ── Step 7: 4% Health & Education Cess ───────────────────────
                result.EducationCess = result.TaxAmount * 4m / 100m;

                result.TotalTax = result.TaxAmount + result.EducationCess;

                // ── Step 8: Monthly TDS = FY tax / remaining months ───────────
                result.MonthlyTDS = result.TotalTax / remainingFYMonths;
            }
            else
            {
                result.TaxAmount = 0;
                result.Surcharge = 0;
                result.EducationCess = 0;
                result.TotalTax = 0;
                result.MonthlyTDS = 0;
            }

            return result;
        }

        /// <summary>Overload kept for callers that pass only annualIncome (assumes full year / March period).</summary>
        public TDSCalculationResult CalculateTDS(decimal annualIncome, int ageGroup = 1)
            => CalculateTDS(annualIncome / 12m, 3, ageGroup); // treat as full-year: period=Mar → remainingMonths=1 ×12 = annualIncome

        public decimal GetTDSDeduction(decimal annualIncome, int ageGroup = 1)
        {
            var result = CalculateTDS(annualIncome, ageGroup);
            return result.MonthlyTDS;
        }

        // GetBasicExemptionLimit retained but not used under New Regime
        // (the 0% slab in TDSSlabConfiguration covers the first ₹4,00,000)
        private decimal GetBasicExemptionLimit(int ageGroup)
        {
            return ageGroup switch
            {
                1 => 250000, // Below 60 years (Old Regime — kept for reference)
                2 => 300000,
                3 => 500000,
                _ => 250000
            };
        }

        private decimal CalculateTaxBySlabs(decimal taxableIncome, List<TDSSlabConfiguration> slabs, List<TDSSlabDetail> slabDetails)
        {
            decimal totalTax = 0;
            decimal remainingIncome = taxableIncome;

            foreach (var slab in slabs.OrderBy(s => s.MinIncome))
            {
                if (remainingIncome <= 0) break;

                decimal slabIncome = 0;
                if (!slab.MaxIncome.HasValue)
                {
                    slabIncome = remainingIncome;
                }
                else
                {
                    decimal slabMaxIncome = slab.MaxIncome.Value - slab.MinIncome;
                    slabIncome = Math.Min(remainingIncome, slabMaxIncome);
                }

                decimal slabTax = slabIncome * slab.TaxRate / 100;
                totalTax += slabTax;

                slabDetails.Add(new TDSSlabDetail
                {
                    MinIncome = slab.MinIncome,
                    MaxIncome = slab.MaxIncome,
                    TaxRate = slab.TaxRate,
                    TaxableAmount = slabIncome,
                    TaxAmount = slabTax
                });

                remainingIncome -= slabIncome;
            }

            return totalTax;
        }

        private decimal CalculateSurcharge(decimal annualIncome)
        {
            // Surcharge rates (example - can be configured)
            if (annualIncome > 50000000) // 5 crore
            {
                return annualIncome * 15 / 100;
            }
            else if (annualIncome > 20000000) // 2 crore
            {
                return annualIncome * 25 / 100;
            }
            else if (annualIncome > 10000000) // 1 crore
            {
                return annualIncome * 10 / 100;
            }
            
            return 0;
        }

        private List<TDSSlabConfiguration> GetActiveTDSSlabs()
        {
            return _context.Set<TDSSlabConfiguration>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.MinIncome)
                .ToList();
        }
    }
}
