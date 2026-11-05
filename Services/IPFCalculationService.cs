using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface IPFCalculationService
    {
        PFCalculationResult CalculatePF(decimal basicSalary, decimal da, DateTime calculationDate);
        PFCalculationResult CalculatePFWithAge(decimal basicSalary, decimal da, DateTime calculationDate, DateTime? dateOfBirth);
        bool IsPFApplicable(decimal basicSalary, decimal da, DateTime calculationDate);

        /// <summary>
        /// Calculates employee PF contribution for Indian payroll.
        ///
        /// Formula:
        ///   pfWage = Basic + DA
        ///   PF     = Round(pfWage × EmployeeRate / 100, 0)
        ///
        /// e.g. Basic=10,000 + DA=15,000 = 25,000 × 12% = 3,000
        /// </summary>
        /// <param name="basic">CB_Basic</param>
        /// <param name="da">CB_DA (Dearness Allowance — do NOT pass HRA here)</param>
        /// <param name="otherAllowances">CB_OtherAllowances (not used in PF calc)</param>
        /// <param name="calendarDays">Calendar days in month (not used in simple formula)</param>
        /// <param name="workedDays">Worked days (not used in simple formula)</param>
        /// <param name="calculationDate">Salary period date (for PFConfiguration lookup)</param>
        /// <returns>Tuple of (PF employee contribution, PFWage = Basic+DA)</returns>
        (decimal PF, decimal PFWage) CalculatePFForPayslip(
            decimal basic,
            decimal da,
            decimal otherAllowances,
            decimal calendarDays,
            decimal workedDays,
            DateTime calculationDate);
    }

    public class PFCalculationResult
    {
        public decimal BasicSalary { get; set; }
        public decimal DearnessAllowance { get; set; }
        public decimal TotalWages { get; set; }
        public decimal EligibleWages { get; set; }
        public bool IsPFApplicable { get; set; }
        public decimal EmployeeContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal TotalContribution { get; set; }
        public decimal EmployeeContributionRate { get; set; }
        public decimal EmployerContributionRate { get; set; }
        public DateTime CalculationDate { get; set; }

        // PF Statement specific fields
        public decimal PFSalary { get; set; }
        public decimal PensionSalary { get; set; }
        public decimal EDLISalary { get; set; }
        public decimal EPF { get; set; }
        public decimal EPS { get; set; }
        public decimal Balance { get; set; }
        public decimal NCP { get; set; }
        public decimal EDLI { get; set; }
        public int Days { get; set; }
        public int Age { get; set; }
        public bool IsAbove55 { get; set; }
    }
}
