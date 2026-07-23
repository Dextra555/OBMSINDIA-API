using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;

namespace OBMS.WebAPI.Services
{
    public class ESICalculationService : IESICalculationService
    {
        private readonly OBMSDbContext _context;

        public ESICalculationService(OBMSDbContext context)
        {
            _context = context;
        }

        public ESICalculationResult CalculateESI(decimal grossSalary, DateTime calculationDate)
        {
            return CalculateESIWithAttendance(grossSalary, 30, 30, calculationDate);
        }

        public ESICalculationResult CalculateESIWithAttendance(decimal grossSalary, int daysWorked, int totalWorkingDays, DateTime calculationDate)
        {
            var result = new ESICalculationResult
            {
                GrossSalary = grossSalary,
                DaysWorked = daysWorked,
                TotalWorkingDays = totalWorkingDays,
                CalculationDate = calculationDate
            };

            var esiConfig = GetActiveESIConfiguration(calculationDate);
            if (esiConfig == null)
            {
                return new ESICalculationResult
                {
                    GrossSalary = grossSalary,
                    IsESIApplicable = false,
                    EmployeeContribution = 0,
                    EmployerContribution = 0,
                    TotalContribution = 0,
                    EmployeeContributionRate = 0,
                    EmployerContributionRate = 0,
                    CalculationDate = calculationDate,
                    DaysWorked = daysWorked,
                    TotalWorkingDays = totalWorkingDays,
                    ESIWage = 0,
                    HasZeroWorkingDays = daysWorked == 0,
                    IsPartialMonth = daysWorked < totalWorkingDays
                };
            }

            result.HasZeroWorkingDays = daysWorked == 0;
            result.IsPartialMonth = daysWorked < totalWorkingDays;

            // If zero working days, no ESI contribution
            if (result.HasZeroWorkingDays)
            {
                result.IsESIApplicable = false;
                result.EmployeeContribution = 0;
                result.EmployerContribution = 0;
                result.TotalContribution = 0;
                result.ESIWage = 0;
                return result;
            }

            // For partial month, calculate ESI wage proportionally
            decimal esiWage = grossSalary;
            if (result.IsPartialMonth)
            {
                esiWage = grossSalary * daysWorked / totalWorkingDays;
            }

            // ESI is only applicable if gross salary is below the salary limit
            result.IsESIApplicable = IsESIApplicable(esiWage, calculationDate);
            
            if (result.IsESIApplicable)
            {
                result.ESIWage = esiWage;
            }
            else
            {
                result.ESIWage = 0;
            }
            result.EmployeeContributionRate = esiConfig.EmployeeRate;
            result.EmployerContributionRate = esiConfig.EmployerRate;

            if (result.IsESIApplicable)
            {
                result.EmployeeContribution = result.ESIWage * esiConfig.EmployeeRate / 100;
                result.EmployerContribution = result.ESIWage * esiConfig.EmployerRate / 100;
                result.TotalContribution = result.EmployeeContribution + result.EmployerContribution;
            }
            else
            {
                result.EmployeeContribution = 0;
                result.EmployerContribution = 0;
                result.TotalContribution = 0;
            }

            return result;
        }

        public bool IsESIApplicable(decimal grossSalary, DateTime calculationDate)
        {
            var esiConfig = GetActiveESIConfiguration(calculationDate);
            if (esiConfig == null) return false;

            return grossSalary <= esiConfig.BasicSalaryLimit;
        }

        private ESIConfiguration GetActiveESIConfiguration(DateTime calculationDate)
        {
            return _context.Set<ESIConfiguration>()
                .Where(c => c.IsActive && c.EffectiveDate <= calculationDate)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefault();
        }
    }
}
