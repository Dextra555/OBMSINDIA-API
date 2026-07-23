using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface IProfessionalTaxService
    {
        ProfessionalTaxResult CalculateProfessionalTax(decimal grossSalary, string state, DateTime calculationDate);
        decimal GetProfessionalTaxAmount(decimal grossSalary, string state, DateTime calculationDate);
    }

    public class ProfessionalTaxResult
    {
        public decimal GrossSalary { get; set; }
        public string State { get; set; }
        public decimal TaxAmount { get; set; }
        public bool IsApplicable { get; set; }
        public DateTime CalculationDate { get; set; }
        public string? TaxSlab { get; set; }
    }
}
