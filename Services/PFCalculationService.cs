using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;

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

            // PF Salary is capped at the basic salary limit (usually 15,000)
            decimal pfSalary = Math.Min(result.TotalWages, pfConfig.BasicSalaryLimit);
            result.PFSalary = pfSalary;
            result.PensionSalary = pfSalary;
            result.EDLISalary = pfSalary;

            if (result.IsPFApplicable && !isAbove55)
            {
                // Employee PF (12%)
                decimal epf = pfSalary * pfConfig.EmployeeRate / 100;
                result.EPF = Math.Round(epf, 2);

                // Employer EPS (8.33%)
                decimal eps = pfSalary * pfConfig.EmployerEPSRate / 100;
                result.EPS = Math.Round(eps, 2);

                // Employer PF Balance (12% - 8.33% = 3.67%)
                decimal balance = result.EPF - result.EPS;
                result.Balance = Math.Round(balance, 2);

                result.EmployeeContribution = result.EPF;
                result.EmployerContribution = result.EPS + result.Balance;
                result.TotalContribution = result.EmployeeContribution + result.EmployerContribution;
                result.EligibleWages = pfSalary;
            }
            else if (isAbove55)
            {
                // If age > 55, EPS should be 0 and PF salary should be 0
                result.PFSalary = 0;
                result.PensionSalary = 0;
                result.EDLISalary = 0;
                result.EPF = 0;
                result.EPS = 0;
                result.Balance = 0;
                result.EmployeeContribution = 0;
                result.EmployerContribution = 0;
                result.TotalContribution = 0;
                result.EligibleWages = 0;
            }
            else
            {
                result.EmployeeContribution = 0;
                result.EmployerContribution = 0;
                result.TotalContribution = 0;
                result.EPF = 0;
                result.EPS = 0;
                result.Balance = 0;
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

        private PFConfiguration GetActivePFConfiguration(DateTime calculationDate)
        {
            return _context.Set<PFConfiguration>()
                .Where(c => c.IsActive && c.EffectiveDate <= calculationDate)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefault();
        }
    }
}
