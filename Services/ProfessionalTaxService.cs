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

            var taxConfig = GetApplicableTaxSlab(grossSalary, state, calculationDate);
            
            if (taxConfig != null)
            {
                result.TaxAmount = taxConfig.TaxAmount;
                result.IsApplicable = true;
                result.TaxSlab = $"{taxConfig.MinSalary:N0} - {(taxConfig.MaxSalary.HasValue ? taxConfig.MaxSalary.Value.ToString("N0") : "Above")}";
                
                // Convert tax amount to monthly based on TaxPeriod
                switch (taxConfig.TaxPeriod?.ToLower())
                {
                    case "semiannual":
                        // 6-month period, divide by 6 to get monthly amount
                        result.TaxAmount = Math.Round(result.TaxAmount / 6, 2);
                        result.TaxSlab += " (Monthly: 6-month amount ÷ 6)";
                        break;
                    case "annual":
                        // Annual period, divide by 12 to get monthly amount
                        result.TaxAmount = Math.Round(result.TaxAmount / 12, 2);
                        result.TaxSlab += " (Monthly: Annual amount ÷ 12)";
                        break;
                    case "monthly":
                    default:
                        // Already monthly, no conversion needed
                        result.TaxSlab += " (Monthly amount)";
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
            var taxConfig = GetApplicableTaxSlab(grossSalary, state, calculationDate);
            var taxAmount = taxConfig?.TaxAmount ?? 0;
            
            // Convert tax amount to monthly based on TaxPeriod
            switch (taxConfig?.TaxPeriod?.ToLower())
            {
                case "semiannual":
                    // 6-month period, divide by 6 to get monthly amount
                    taxAmount = Math.Round(taxAmount / 6, 2);
                    break;
                case "annual":
                    // Annual period, divide by 12 to get monthly amount
                    taxAmount = Math.Round(taxAmount / 12, 2);
                    break;
                case "monthly":
                default:
                    // Already monthly, no conversion needed
                    break;
            }
            
            return taxAmount;
        }

        private ProfessionalTaxConfiguration GetApplicableTaxSlab(decimal grossSalary, string state, DateTime calculationDate)
        {
            if (string.IsNullOrEmpty(state))
                return null;
                
            return _context.Set<ProfessionalTaxConfiguration>()
                .Where(c => c.IsActive && 
                           c.State.ToLower() == state.ToLower() && 
                           c.EffectiveDate <= calculationDate &&
                           grossSalary >= c.MinSalary &&
                           (!c.MaxSalary.HasValue || grossSalary <= c.MaxSalary.Value))
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefault();
        }
    }
}
