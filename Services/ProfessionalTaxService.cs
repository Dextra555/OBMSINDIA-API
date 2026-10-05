using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace OBMS.WebAPI.Services
{
    public class ProfessionalTaxService : IProfessionalTaxService
    {
        private readonly OBMSDbContext _context;

        public ProfessionalTaxService(OBMSDbContext context)
        {
            _context = context;
        }

        public ProfessionalTaxResult CalculateProfessionalTax(decimal grossSalary, string state, DateTime calculationDate)
        {
            var result = new ProfessionalTaxResult
            {
                GrossSalary = grossSalary,
                State = state,
                CalculationDate = calculationDate
            };

            var periodConfig = GetStateTaxPeriod(state, calculationDate);
            string taxPeriod = periodConfig ?? "Monthly";

            decimal salaryForSlabMatch = ScaleSalaryForPeriod(grossSalary, taxPeriod);
            var taxConfig = GetApplicableTaxSlab(salaryForSlabMatch, state, calculationDate);

            if (taxConfig != null)
            {
                result.TaxAmount = taxConfig.TaxAmount;
                result.IsApplicable = true;
                result.TaxSlab = $"{taxConfig.MinSalary:N0} - {(taxConfig.MaxSalary.HasValue ? taxConfig.MaxSalary.Value.ToString("N0") : "Above")}";

                switch (taxConfig.TaxPeriod?.ToLower())
                {
                    case "semiannual":
                        result.TaxAmount = Math.Round(result.TaxAmount / 6, 2);
                        result.TaxSlab += $" (semi-annual; salary×6={salaryForSlabMatch:N0}; ÷6)";
                        break;
                    case "annual":
                        result.TaxAmount = Math.Round(result.TaxAmount / 12, 2);
                        result.TaxSlab += $" (annual; salary×12={salaryForSlabMatch:N0}; ÷12)";
                        break;
                    default:
                        result.TaxSlab += " (monthly)";
                        break;
                }
            }
            else
            {
                result.TaxAmount = 0;
                result.IsApplicable = false;
                result.TaxSlab = "No PT applicable";
            }

            return result;
        }

        public decimal GetProfessionalTaxAmount(decimal grossSalary, string state, DateTime calculationDate)
        {
            var periodConfig = GetStateTaxPeriod(state, calculationDate);
            string taxPeriod = periodConfig ?? "Monthly";

            decimal salaryForSlabMatch = ScaleSalaryForPeriod(grossSalary, taxPeriod);
            var taxConfig = GetApplicableTaxSlab(salaryForSlabMatch, state, calculationDate);
            var taxAmount = taxConfig?.TaxAmount ?? 0;

            switch (taxConfig?.TaxPeriod?.ToLower())
            {
                case "semiannual":
                    taxAmount = Math.Round(taxAmount / 6, 2);
                    break;
                case "annual":
                    taxAmount = Math.Round(taxAmount / 12, 2);
                    break;
                default:
                    break;
            }

            return taxAmount;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns TaxPeriod for the given state.
        /// AsNoTracking prevents EF change tracker buildup in long salary processing loops.
        /// </summary>
        private string? GetStateTaxPeriod(string state, DateTime calculationDate)
        {
            if (string.IsNullOrEmpty(state)) return null;

            return _context.Set<ProfessionalTaxConfiguration>()
                .AsNoTracking()
                .Where(c => c.IsActive &&
                            c.State.ToLower() == state.ToLower() &&
                            c.EffectiveDate <= calculationDate)
                .OrderByDescending(c => c.EffectiveDate)
                .Select(c => c.TaxPeriod)
                .FirstOrDefault();
        }

        private static decimal ScaleSalaryForPeriod(decimal monthlySalary, string taxPeriod)
        {
            return taxPeriod?.ToLower() switch
            {
                "semiannual" => monthlySalary * 6,
                "annual"     => monthlySalary * 12,
                _            => monthlySalary
            };
        }

        /// <summary>
        /// Finds the matching slab for the (already scaled) salary.
        /// AsNoTracking prevents stale DbContext issues in long-running ADO.NET loops.
        /// </summary>
        private ProfessionalTaxConfiguration? GetApplicableTaxSlab(decimal salaryForMatch, string state, DateTime calculationDate)
        {
            if (string.IsNullOrEmpty(state))
                return null;

            return _context.Set<ProfessionalTaxConfiguration>()
                .AsNoTracking()
                .Where(c => c.IsActive &&
                            c.State.ToLower() == state.ToLower() &&
                            c.EffectiveDate <= calculationDate &&
                            salaryForMatch >= c.MinSalary &&
                            (!c.MaxSalary.HasValue || salaryForMatch <= c.MaxSalary.Value))
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefault();
        }
    }
}
