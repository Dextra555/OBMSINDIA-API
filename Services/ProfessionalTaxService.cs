using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;

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

            // First pass: find the TaxPeriod for this state so we know how to
            // scale the salary before doing the final slab match.
            var periodConfig = GetStateTaxPeriod(state, calculationDate);
            string taxPeriod = periodConfig ?? "Monthly";

            // Scale salary for slab matching
            // SemiAnnual slabs store 6-month salary boundaries (Tamil Nadu).
            // Annual slabs store annual salary boundaries.
            decimal salaryForSlabMatch = ScaleSalaryForPeriod(grossSalary, taxPeriod);

            var taxConfig = GetApplicableTaxSlab(salaryForSlabMatch, state, calculationDate);

            if (taxConfig != null)
            {
                result.TaxAmount = taxConfig.TaxAmount;
                result.IsApplicable = true;
                result.TaxSlab = $"{taxConfig.MinSalary:N0} - {(taxConfig.MaxSalary.HasValue ? taxConfig.MaxSalary.Value.ToString("N0") : "Above")}";

                // Convert slab tax amount back to a monthly figure
                switch (taxConfig.TaxPeriod?.ToLower())
                {
                    case "semiannual":
                        // DB stores the 6-month tax amount — divide by 6 → monthly
                        result.TaxAmount = Math.Round(result.TaxAmount / 6, 2);
                        result.TaxSlab += $" (semi-annual slab; monthly salary×6={salaryForSlabMatch:N0}; monthly PTax=÷6)";
                        break;
                    case "annual":
                        // DB stores the annual tax amount — divide by 12 → monthly
                        result.TaxAmount = Math.Round(result.TaxAmount / 12, 2);
                        result.TaxSlab += $" (annual slab; monthly salary×12={salaryForSlabMatch:N0}; monthly PTax=÷12)";
                        break;
                    default:
                        result.TaxSlab += " (monthly slab)";
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
            // Find the TaxPeriod for this state to scale salary before slab match
            var periodConfig = GetStateTaxPeriod(state, calculationDate);
            string taxPeriod = periodConfig ?? "Monthly";

            decimal salaryForSlabMatch = ScaleSalaryForPeriod(grossSalary, taxPeriod);

            var taxConfig = GetApplicableTaxSlab(salaryForSlabMatch, state, calculationDate);
            var taxAmount = taxConfig?.TaxAmount ?? 0;

            // Convert slab tax amount back to monthly
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
        /// Returns the TaxPeriod of the first active slab found for the given state.
        /// All slabs for one state should share the same TaxPeriod.
        /// </summary>
        private string? GetStateTaxPeriod(string state, DateTime calculationDate)
        {
            if (string.IsNullOrEmpty(state)) return null;

            return _context.Set<ProfessionalTaxConfiguration>()
                .Where(c => c.IsActive &&
                            c.State.ToLower() == state.ToLower() &&
                            c.EffectiveDate <= calculationDate)
                .OrderByDescending(c => c.EffectiveDate)
                .Select(c => c.TaxPeriod)
                .FirstOrDefault();
        }

        /// <summary>
        /// Scales the monthly salary to match how the slab boundaries are stored:
        ///   SemiAnnual → multiply by 6   (Tamil Nadu stores 6-month boundaries)
        ///   Annual     → multiply by 12
        ///   Monthly    → no change
        /// </summary>
        private static decimal ScaleSalaryForPeriod(decimal monthlySalary, string taxPeriod)
        {
            return taxPeriod?.ToLower() switch
            {
                "semiannual" => monthlySalary * 6,
                "annual"     => monthlySalary * 12,
                _            => monthlySalary
            };
        }

        private ProfessionalTaxConfiguration? GetApplicableTaxSlab(decimal salaryForMatch, string state, DateTime calculationDate)
        {
            if (string.IsNullOrEmpty(state))
                return null;

            return _context.Set<ProfessionalTaxConfiguration>()
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
