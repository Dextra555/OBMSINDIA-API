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

            // PF Wage = Min(Basic + DA, BasicSalaryLimit)
            // BasicSalaryLimit = ₹25,000 from PFConfiguration (Effective 01-Oct-2026)
            // e.g. Basic+DA=24,000 → pfSalary=24,000  → PF=24,000×12%=2,880
            //      Basic+DA=26,000 → pfSalary=25,000  → PF=25,000×12%=3,000 (capped)
            //      Basic+DA=35,000 → pfSalary=25,000  → PF=25,000×12%=3,000 (capped)
            decimal pfSalary = Math.Min(result.TotalWages, pfConfig.BasicSalaryLimit);
            result.PFSalary      = pfSalary;
            result.PensionSalary = pfSalary;
            result.EDLISalary    = pfSalary;

            if (result.IsPFApplicable && !isAbove55)
            {
                // Employee PF = Min(Basic+DA, BasicSalaryLimit) × EmployeeRate%
                decimal epf = pfSalary * pfConfig.EmployeeRate / 100m;
                result.EPF = Math.Round(epf, 0, MidpointRounding.AwayFromZero);

                // Employer PF = Min(Basic+DA, BasicSalaryLimit) × EmployerRate%
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
        /// Calculates employee PF for Indian payroll (Salary Processing).
        ///
        /// Formula:
        ///   Step 1: pfWage    = (Basic + DA) / CalendarDays × WorkedDays  ← prorate by worked days
        ///   Step 2: pfCapped  = Min(pfWage, BasicSalaryLimit)              ← cap at ₹25,000 (from DB)
        ///   Step 3: PF        = Round(pfCapped × EmployeeRate / 100, 0)   ← 12% from DB
        ///
        /// Examples (CalDays=31, WorkedDays=31 full month):
        ///   Basic+DA = 24,000 → pfWage=24,000 → capped=24,000 → PF = 24,000×12% = 2,880
        ///   Basic+DA = 26,000 → pfWage=26,000 → capped=25,000 → PF = 25,000×12% = 3,000
        ///   Basic+DA = 35,000 → pfWage=35,000 → capped=25,000 → PF = 25,000×12% = 3,000
        ///
        /// Examples (CalDays=31, WorkedDays=30):
        ///   Basic+DA = 24,000 → pfWage=24,000/31×30=23,226 → capped=23,226 → PF=23,226×12%=2,787
        ///   Basic+DA = 26,000 → pfWage=26,000/31×30=25,161 → capped=25,000 → PF=25,000×12%=3,000
        ///
        /// BasicSalaryLimit and EmployeeRate read from active PFConfiguration in DB.
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

            // Step 1 — Prorate Basic+DA by worked days
            decimal totalBD = basic + da;
            decimal pfWage = calendarDays > 0
                ? Math.Round(totalBD / calendarDays * workedDays, 2)
                : totalBD;

            // Step 2 — Cap at BasicSalaryLimit from PFConfiguration (e.g. ₹25,000)
            decimal pfCapped = pfWage > pfConfig.BasicSalaryLimit
                ? pfConfig.BasicSalaryLimit
                : pfWage;

            // Step 3 — Employee PF = pfCapped × EmployeeRate%
            decimal pf = Math.Round(pfCapped * pfConfig.EmployeeRate / 100m, 0, MidpointRounding.AwayFromZero);

            return (pf, pfCapped);
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
