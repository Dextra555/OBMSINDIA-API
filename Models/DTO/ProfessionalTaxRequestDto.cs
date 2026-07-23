using System.ComponentModel.DataAnnotations;

namespace OBMS.WebAPI.Models.DTO
{
    public class ProfessionalTaxRequestDto
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Gross salary must be a positive number")]
        public decimal GrossSalary { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "State name cannot exceed 50 characters")]
        public string State { get; set; } = string.Empty;

        public DateTime CalculationDate { get; set; } = DateTime.Now;
    }
}
