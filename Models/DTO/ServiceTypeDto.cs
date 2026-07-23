using System.ComponentModel.DataAnnotations;

namespace OBMS.WebAPI.Models.DTO
{
    public class ServiceTypeDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Service Name is required")]
        [StringLength(100, ErrorMessage = "Service Name cannot exceed 100 characters")]
        public string ServiceName { get; set; }

        [StringLength(20, ErrorMessage = "Service Code cannot exceed 20 characters")]
        public string ServiceCode { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "HSN Code is required")]
        [StringLength(8, ErrorMessage = "HSN Code cannot exceed 8 characters")]
        public string HSNCode { get; set; }

        [StringLength(50, ErrorMessage = "Pricing Model cannot exceed 50 characters")]
        public string PricingModel { get; set; } = "Standard";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public string LastUpdatedBy { get; set; }
    }

    public class ServiceTypeCreateDto
    {
        [Required(ErrorMessage = "Service Name is required")]
        [StringLength(100, ErrorMessage = "Service Name cannot exceed 100 characters")]
        public string ServiceName { get; set; }

        [StringLength(20, ErrorMessage = "Service Code cannot exceed 20 characters")]
        public string ServiceCode { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "HSN Code is required")]
        [StringLength(8, ErrorMessage = "HSN Code cannot exceed 8 characters")]
        public string HSNCode { get; set; }

        [StringLength(50, ErrorMessage = "Pricing Model cannot exceed 50 characters")]
        public string PricingModel { get; set; } = "Standard";
    }

    public class ServiceTypeUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Service Name is required")]
        [StringLength(100, ErrorMessage = "Service Name cannot exceed 100 characters")]
        public string ServiceName { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "HSN Code is required")]
        [StringLength(8, ErrorMessage = "HSN Code cannot exceed 8 characters")]
        public string HSNCode { get; set; }

        [StringLength(50, ErrorMessage = "Pricing Model cannot exceed 50 characters")]
        public string PricingModel { get; set; }

        public bool IsActive { get; set; }
    }
}
