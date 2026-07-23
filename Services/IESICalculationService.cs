using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface IESICalculationService
    {
        ESICalculationResult CalculateESI(decimal grossSalary, DateTime calculationDate);
        ESICalculationResult CalculateESIWithAttendance(decimal grossSalary, int daysWorked, int totalWorkingDays, DateTime calculationDate);
        bool IsESIApplicable(decimal grossSalary, DateTime calculationDate);
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
