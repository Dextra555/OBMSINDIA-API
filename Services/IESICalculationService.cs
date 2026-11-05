using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface IESICalculationService
    {
        ESICalculationResult CalculateESI(decimal grossSalary, DateTime calculationDate);
        ESICalculationResult CalculateESIWithAttendance(decimal grossSalary, int daysWorked, int totalWorkingDays, DateTime calculationDate);
        bool IsESIApplicable(decimal grossSalary, DateTime calculationDate);

        /// <summary>
        /// Calculates employee ESI contribution for Indian payroll with Working Days proration.
        /// Basis is determined by ESIConfiguration.CalculationBasis:
        ///
        ///   "BasicDA" → ESI Wage = (CB_Basic + CB_DA) / CalendarDays × WorkedDays
        ///               ESI = Round(ESIWage × EmployeeRate / 100, 0)
        ///
        ///   "Gross"   → ESI Wage = (EarnedSalary − Bonus) / CalendarDays × WorkedDays
        ///               if ESI Wage ≤ BasicSalaryLimit (₹21,000) → apply, else 0
        ///
        /// Example (Basic=6680, DA=7353, WD=30, CalDays=31):
        ///   ESIWage = (6680+7353)/31×30 = 13,580  →  ESI = Round(13580×0.75%) = 102
        /// </summary>
        (decimal ESI, decimal ESIWage) CalculateESIForPayslip(
            decimal earnedSalary,
            decimal cbAdvanceStatutoryBonus,
            decimal cbBasic,
            decimal cbDA,
            int calendarDays,
            decimal workedDays,
            DateTime calculationDate);

        /// <summary>
        /// Overload without proration (full month) — used by API payslip methods.
        /// </summary>
        (decimal ESI, decimal ESIWage) CalculateESIForPayslip(
            decimal earnedSalary,
            decimal cbAdvanceStatutoryBonus,
            decimal cbBasic,
            decimal cbDA,
            DateTime calculationDate);

        /// <summary>
        /// Backward-compatible overload — Gross basis, cbBasic=0, cbDA=0.
        /// </summary>
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
