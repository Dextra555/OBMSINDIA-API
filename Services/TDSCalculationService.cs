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

        public TDSCalculationResult CalculateTDS(decimal annualIncome, int ageGroup = 1)
        {
            var result = new TDSCalculationResult
            {
                AnnualIncome = annualIncome,
                AgeGroup = ageGroup
            };

            // Get basic exemption limit based on age group
            decimal basicExemption = GetBasicExemptionLimit(ageGroup);
            result.TaxableIncome = Math.Max(0, annualIncome - basicExemption);
            
            result.IsApplicable = result.TaxableIncome > 0;

            if (result.IsApplicable)
            {
                var taxSlabs = GetActiveTDSSlabs();
                result.TaxAmount = CalculateTaxBySlabs(result.TaxableIncome, taxSlabs, result.SlabDetails);
                
                // Calculate surcharge (if applicable)
                result.Surcharge = CalculateSurcharge(annualIncome);
                
                // Calculate education cess (fixed at 4%)
                var cessRate = 4;
                result.EducationCess = (result.TaxAmount + result.Surcharge) * cessRate / 100;
                
                result.TotalTax = result.TaxAmount + result.Surcharge + result.EducationCess;
                result.MonthlyTDS = result.TotalTax / 12;
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

        public decimal GetTDSDeduction(decimal annualIncome, int ageGroup = 1)
        {
            var result = CalculateTDS(annualIncome, ageGroup);
            return result.MonthlyTDS;
        }

        private decimal GetBasicExemptionLimit(int ageGroup)
        {
            return ageGroup switch
            {
                1 => 250000, // Below 60 years
                2 => 300000, // 60 to 80 years
                3 => 500000, // Above 80 years
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
