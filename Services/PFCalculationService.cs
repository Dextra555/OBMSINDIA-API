using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace OBMS.WebAPI.Services
{
    public class PFCalculationService : IPFCalculationService
    {
        private readonly OBMSDbContext _context;

        public PFCalculationService(OBMSDbContext context)
        {
            _context = context;
        }

        public PFCalculationResult CalculatePF(decimal basicSalary, decimal da, DateTime calculationDate)
        {
            return CalculatePFWithAge(basicSalary, da, calculationDate, null);
        }

        public PFCalculationResult CalculatePFWithAge(decimal basicSalary, decimal da, DateTime calculationDate, DateTime? dateOfBirth)
        {
            var result = new PFCalculationResult
            {
                BasicSalary = basicSalary,
                DearnessAllowance = da,
                TotalWages = basicSalary + da,
                CalculationDate = calculationDate
            };

            var pfConfig = GetActivePFConfiguration(calculationDate);
            if (pfConfig == null)
            {
                return new PFCalculationResult
                {
                    BasicSalary = basicSalary,
                    DearnessAllowance = da,
                    TotalWages = basicSalary + da,
                    IsPFApplicable = false,
                    EmployeeContribution = 0,
                    EmployerContribution = 0,
                    TotalContribution = 0,
                    EmployeeContributionRate = 0,
                    EmployerContributionRate = 0,
                    CalculationDate = calculationDate,
                    PFSalary = 0,
                    PensionSalary = 0,
                    EDLISalary = 0,
                    EPF = 0,
                    EPS = 0,
                    Balance = 0,
                    NCP = 0,
                    EDLI = 0,
                    Days = 0,
                    Age = 0,
                    IsAbove55 = false
                };
            }

            // Calculate age if date of birth is provided
            int age = 0;
            bool isAbove55 = false;
            if (dateOfBirth.HasValue)
            {
                age = calculationDate.Year - dateOfBirth.Value.Year;
                if (dateOfBirth.Value > calculationDate.AddYears(-age))
                    age--;
                isAbove55 = age >= 55;
            }

            result.Age = age;
            result.IsAbove55 = isAbove55;

            result.IsPFApplicable = IsPFApplicable(basicSalary, da, calculationDate);
            result.EmployeeContributionRate = pfConfig.EmployeeRate;
            result.EmployerContributionRate = pfConfig.EmployerRate;

            // PF Wage = Basic + DA  (API method — full month, no days context)
            // Cap at BasicSalaryLimit from PFConfiguration (e.g. ₹25,000)
            // Basic+DA=24,000 → pfSalary=24,000  (below limit, no cap)
            // Basic+DA=26,000 → pfSalary=25,000  (above limit, capped)
            // Basic+DA=35,000 → pfSalary=25,000  (above limit, capped)
            decimal pfSalary = result.TotalWages > pfConfig.BasicSalaryLimit
                ? pfConfig.BasicSalaryLimit
                : result.TotalWages;
            result.PFSalary    = pfSalary;
            result.PensionSalary = pfSalary;
            result.EDLISalary  = pfSalary;

            if (result.IsPFApplicable && !isAbove55)
            {
                // Employee PF = (Basic + DA) × EmployeeRate%
                // e.g. (10,000 + 15,000) × 12% = 3,000
                decimal epf = pfSalary * pfConfig.EmployeeRate / 100m;
                result.EPF = Math.Round(epf, 0, MidpointRounding.AwayFromZero);

                // Employer PF = (Basic + DA) × EmployerRate%
                // e.g. (10,000 + 15,000) × 13% = 3,250
                decimal employerPF = pfSalary * pfConfig.EmployerRate / 100m;
                result.EPS     = Math.Round(employerPF, 0, MidpointRounding.AwayFromZero);
                result.Balance = 0;

                result.EmployeeContribution = result.EPF;
                result.EmployerContribution = result.EPS;
                result.TotalContribution    = result.EmployeeContribution + result.EmployerContribution;
                result.EligibleWages        = pfSalary;
            }
            else if (isAbove55)
            {
                result.PFSalary = 0; result.PensionSalary = 0; result.EDLISalary = 0;
                result.EPF = 0; result.EPS = 0; result.Balance = 0;
                result.EmployeeContribution = 0; result.EmployerContribution = 0;
                result.TotalContribution = 0; result.EligibleWages = 0;
            }
            else
            {
                result.EmployeeContribution = 0; result.EmployerContribution = 0;
                result.TotalContribution = 0;
                result.EPF = 0; result.EPS = 0; result.Balance = 0;
            }

            // EDLI is typically 0.5% of PF salary, capped at certain amount
            // For now, setting to 0 as per user's example
            result.EDLI = 0;
            result.NCP = 0;
            result.Days = 0;

            return result;
        }

        public bool IsPFApplicable(decimal basicSalary, decimal da, DateTime calculationDate)
        {
            var pfConfig = GetActivePFConfiguration(calculationDate);
            if (pfConfig == null) return false;

            // PF is applicable for all employees - the ceiling only caps the contribution amount
            // According to EPF rules, if Basic + DA > ₹15,000, PF is still applicable but capped at ₹1,800
            decimal totalWages = basicSalary + da;
            return totalWages > 0;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates employee PF for Indian payroll with Working Days proration + BasicSalaryLimit cap.
        ///
        /// Formula:
        ///   Step 1: pfWage  = (Basic + DA) / CalendarDays × WorkedDays   (prorate)
        ///   Step 2: pfWage  = Min(pfWage, BasicSalaryLimit)               (cap at ₹25,000)
        ///   Step 3: PF      = Round(pfWage × EmployeeRate / 100, 0)      (12%)
        ///
        /// Examples:
        ///   Basic+DA=24,000, WD=31/31 → pfWage=24,000 → PF=Round(24000×12%)=2,880
        ///   Basic+DA=26,000, WD=31/31 → pfWage=25,000 (capped) → PF=Round(25000×12%)=3,000
        ///   Basic+DA=35,000, WD=31/31 → pfWage=25,000 (capped) → PF=Round(25000×12%)=3,000
        ///   Basic+DA=14,033, WD=30/31 → pfWage=13,580 → PF=Round(13580×12%)=1,630
        /// </summary>
        public (decimal PF, decimal PFWage) CalculatePFForPayslip(
            decimal basic,
            decimal da,
            decimal otherAllowances,
            decimal calendarDays,
            decimal workedDays,
            DateTime calculationDate)
        {
            var pfConfig = GetActivePFConfiguration(calculationDate);
            if (pfConfig == null)
                return (0m, 0m);

            // Step 1 — Prorate by worked days
            // PF is only on Basic + DA (otherAllowances excluded per EPF rules)
            decimal totalBD = basic + da;
            decimal pfWage = calendarDays > 0
                ? Math.Round(totalBD / calendarDays * workedDays, 2)
                : totalBD;

            // Step 2 — Cap at BasicSalaryLimit from PFConfiguration DB (e.g. ₹25,000)
            // Basic+DA=24,000 → pfWage=24,000  (below limit, no cap)
            // Basic+DA=26,000 → pfWage=25,000  (above limit, capped)
            // Basic+DA=35,000 → pfWage=25,000  (above limit, capped)
            if (pfWage > pfConfig.BasicSalaryLimit)
                pfWage = pfConfig.BasicSalaryLimit;

            // Step 3 — Employee PF = pfWage × EmployeeRate%
            // AwayFromZero matches Crystal Report Round() behaviour
            decimal pf = Math.Round(pfWage * pfConfig.EmployeeRate / 100m, 0, MidpointRounding.AwayFromZero);

            return (pf, pfWage);
        }

        private PFConfiguration GetActivePFConfiguration(DateTime calculationDate)
        {
            return _context.Set<PFConfiguration>()
                .AsNoTracking()
                .Where(c => c.IsActive && c.EffectiveDate <= calculationDate)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefault();
        }
    }
}
