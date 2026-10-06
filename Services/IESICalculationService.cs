using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface IESICalculationService
    {
        ESICalculationResult CalculateESI(decimal grossSalary, DateTime calculationDate);
        ESICalculationResult CalculateESIWithAttendance(decimal grossSalary, int daysWorked, int totalWorkingDays, DateTime calculationDate);
        bool IsESIApplicable(decimal grossSalary, DateTime calculationDate);

        /// <summary>
        /// Calculates employee ESI contribution for Indian payroll.
        /// Matches Crystal Report formula exactly:
        ///   esiWage = EarnedSalary − CB_AdvanceStatutoryBonus
        ///   if esiWage &lt; BasicSalaryLimit (₹21,000 from ESIConfiguration)
        ///       ESI = Round(esiWage × EmployeeRate / 100, 0)   [0.75%]
        ///   else
        ///       ESI = 0
        /// </summary>
        /// <param name="earnedSalary">Pre-calculated EarnedSalary (BasicSalaryDays × PerDaySalary)</param>
        /// <param name="cbAdvanceStatutoryBonus">CB_AdvanceStatutoryBonus to deduct from wage</param>
        /// <param name="calculationDate">Salary period date (for ESIConfiguration lookup)</param>
        /// <returns>Tuple of (ESI employee contribution, ESIWage used)</returns>
        (decimal ESI, decimal ESIWage) CalculateESIForPayslip(
            decimal earnedSalary,
            decimal cbAdvanceStatutoryBonus,
            DateTime calculationDate);
    }

    public class ESICalculationResult
    {
        public decimal GrossSalary { get; set; }
        public bool IsESIApplicable { get; set; }
        public decimal EmployeeContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal TotalContribution { get; set; }
        public decimal EmployeeContributionRate { get; set; }
        public decimal EmployerContributionRate { get; set; }
        public DateTime CalculationDate { get; set; }

        // ESI Wage Report specific fields
        public int DaysWorked { get; set; }
        public int TotalWorkingDays { get; set; }
        public decimal ESIWage { get; set; }
        public bool HasZeroWorkingDays { get; set; }
        public bool IsPartialMonth { get; set; }
    }
}
