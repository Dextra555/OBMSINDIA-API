using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface IPFCalculationService
    {
        PFCalculationResult CalculatePF(decimal basicSalary, decimal da, DateTime calculationDate);
        PFCalculationResult CalculatePFWithAge(decimal basicSalary, decimal da, DateTime calculationDate, DateTime? dateOfBirth);
        bool IsPFApplicable(decimal basicSalary, decimal da, DateTime calculationDate);
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
