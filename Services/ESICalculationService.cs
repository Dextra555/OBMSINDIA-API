using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

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
        /// Main ESI payslip calculation — respects CalculationBasis from ESIConfiguration.
        ///
        /// "Gross"   → ESI Wage = EarnedSalary − CB_AdvanceStatutoryBonus
        ///             Prorated by worked days: ESIWage / CalendarDays × WorkedDays
        ///             Limit check: if ESI Wage ≤ BasicSalaryLimit (₹21,000) → apply, else 0
        ///
        /// "BasicDA" → ESI Wage = (CB_Basic + CB_DA) / CalendarDays × WorkedDays
        ///             NO limit check — always apply ESI on prorated Basic+DA
        ///             Formula: ESI = Round(ESIWage × EmployeeRate / 100, 0)
        ///
        /// Example (full month 31 days): Basic=6680, DA=7353
        ///   ESIWage = (6680+7353)/31×31 = 14,033  →  ESI = Round(14033×0.75%) = 105
        ///
        /// Example (30 days worked, 31 calendar): Basic=6680, DA=7353
        ///   ESIWage = (6680+7353)/31×30 = 13,580  →  ESI = Round(13580×0.75%) = 102
        /// </summary>
        public (decimal ESI, decimal ESIWage) CalculateESIForPayslip(
            decimal earnedSalary,
            decimal cbAdvanceStatutoryBonus,
            decimal cbBasic,
            decimal cbDA,
            DateTime calculationDate)
        {
            return CalculateESIForPayslip(
                earnedSalary, cbAdvanceStatutoryBonus, cbBasic, cbDA,
                calendarDays: 0, workedDays: 0,   // 0 = no proration (full month)
                calculationDate);
        }

        /// <summary>
        /// ESI payslip calculation with Total Working Days proration.
        /// Pass calendarDays and workedDays to prorate ESI wage.
        /// If calendarDays=0 or workedDays=calendarDays → no proration (full month).
        /// </summary>
        public (decimal ESI, decimal ESIWage) CalculateESIForPayslip(
            decimal earnedSalary,
            decimal cbAdvanceStatutoryBonus,
            decimal cbBasic,
            decimal cbDA,
            int calendarDays,
            decimal workedDays,
            DateTime calculationDate)
        {
            var esiConfig = GetActiveESIConfiguration(calculationDate);
            if (esiConfig == null)
                return (0m, 0m);

            decimal esiWage;

            if (string.Equals(esiConfig.CalculationBasis, "BasicDA", StringComparison.OrdinalIgnoreCase))
            {
                // BasicDA basis: ESI Wage = (Basic + DA) / CalendarDays × WorkedDays
                decimal fullWage = cbBasic + cbDA;
                esiWage = (calendarDays > 0 && workedDays > 0 && workedDays < calendarDays)
                    ? Math.Round(fullWage / calendarDays * workedDays, 2)
                    : fullWage;

                if (esiWage <= 0)
                    return (0m, 0m);

                // ESI = Round(ESI Wage × EmployeeRate / 100, 0)
                decimal esi = Math.Round(esiWage * esiConfig.EmployeeRate / 100m, 0, MidpointRounding.AwayFromZero);
                return (esi, esiWage);
            }
            else
            {
                // Gross basis: ESI Wage = (EarnedSalary − Bonus) / CalendarDays × WorkedDays
                decimal fullWage = Math.Round(earnedSalary - cbAdvanceStatutoryBonus, 2);
                if (fullWage < 0) fullWage = 0;

                esiWage = (calendarDays > 0 && workedDays > 0 && workedDays < calendarDays)
                    ? Math.Round(fullWage / calendarDays * workedDays, 2)
                    : fullWage;

                // Gross basis: limit check — ESI only if wage ≤ ₹21,000
                if (esiWage >= esiConfig.BasicSalaryLimit)
                    return (0m, esiWage);

                decimal esi = Math.Round(esiWage * esiConfig.EmployeeRate / 100m, 0, MidpointRounding.AwayFromZero);
                return (esi, esiWage);
            }
        }

        /// <summary>
        /// Backward-compatible overload — Gross basis, cbBasic=0, cbDA=0.
        /// </summary>
        public (decimal ESI, decimal ESIWage) CalculateESIForPayslip(
            decimal earnedSalary,
            decimal cbAdvanceStatutoryBonus,
            DateTime calculationDate)
        {
            return CalculateESIForPayslip(earnedSalary, cbAdvanceStatutoryBonus, 0m, 0m, 0, 0, calculationDate);
        }

        private ESIConfiguration GetActiveESIConfiguration(DateTime calculationDate)
        {
            return _context.Set<ESIConfiguration>()
                .AsNoTracking()
                .Where(c => c.IsActive && c.EffectiveDate <= calculationDate)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefault();
        }
    }
}
