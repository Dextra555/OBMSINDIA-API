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

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates employee ESI for Indian payroll — matches Crystal Report formula exactly.
        ///
        /// Crystal formula:
        ///   gross   = EarnedSalary − CB_AdvanceStatutoryBonus
        ///   if gross &lt; BasicSalaryLimit (₹21,000)
        ///       ESI = Round(gross × EmployeeRate / 100, 0)   [0.75%]
        ///   else
        ///       ESI = 0
        /// </summary>
        public (decimal ESI, decimal ESIWage) CalculateESIForPayslip(
            decimal earnedSalary,
            decimal cbAdvanceStatutoryBonus,
            DateTime calculationDate)
        {
            var esiConfig = GetActiveESIConfiguration(calculationDate);
            if (esiConfig == null)
                return (0m, 0m);

            // Step 1 — ESI wage = EarnedSalary minus Advance Statutory Bonus
            decimal esiWage = Math.Round(earnedSalary - cbAdvanceStatutoryBonus, 2);
            if (esiWage < 0) esiWage = 0;

            // Step 2 — only applicable if esiWage < BasicSalaryLimit (₹21,000)
            if (esiWage >= esiConfig.BasicSalaryLimit)
                return (0m, esiWage);

            // Step 3 — ESI = Round(esiWage × EmployeeRate / 100, 0)
            // Use MidpointRounding.AwayFromZero to match Crystal Report Round() behaviour
            // (Crystal rounds 0.5 UP, C# default banker's rounding rounds 0.5 to nearest even)
            decimal esi = Math.Round(esiWage * esiConfig.EmployeeRate / 100m, 0, MidpointRounding.AwayFromZero);

            return (esi, esiWage);
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
