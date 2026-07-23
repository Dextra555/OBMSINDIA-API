using System.ComponentModel.DataAnnotations;

namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeSearchDto
    {
        public int? EmployeeId { get; set; }
        
        [StringLength(20, ErrorMessage = "Employee code cannot exceed 20 characters")]
        public string? EmployeeCode { get; set; }
        
        [StringLength(100, ErrorMessage = "Employee name cannot exceed 100 characters")]
        public string? EmployeeName { get; set; }

        [RegularExpression(@"^[2-9]\d{11}$", ErrorMessage = "Aadhaar must be 12 digits starting with 2-9")]
        public string? AadhaarNumber { get; set; }

        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "PAN must be in format ABCDE1234F")]
        public string? PANNumber { get; set; }

        [RegularExpression(@"^\+91[6-9]\d{9}$", ErrorMessage = "Mobile number must be in +91 format followed by 10 digits starting with 6-9")]
        public string? MobileNumber { get; set; }

        public string? BranchCode { get; set; }
        public string? State { get; set; }
        public string? SalaryGroup { get; set; }
        public bool? IsActive { get; set; }
    }

    public class EmployeeValidationDto
    {
        [Required(ErrorMessage = "Field type is required")]
        public string FieldType { get; set; } // "PAN", "Aadhaar", "IFSC", "Phone"

        [Required(ErrorMessage = "Field value is required")]
        public string FieldValue { get; set; }

        public int? ExcludeEmployeeId { get; set; } // For uniqueness validation during updates
    }

    public class EmployeeSearchResultDto
    {
        public int EMP_ID { get; set; }
        public string EMP_CODE { get; set; }
        public string EMP_NAME { get; set; }
        public string EMP_SEX { get; set; }
        public string? AadhaarNumber { get; set; }
        public string? PANNumber { get; set; }
        public string? PFAccountNumber { get; set; }
        public string? ESINumber { get; set; }
        public string? SalaryGroup { get; set; }
        public string EMP_PHONE { get; set; }
        public string EMP_MOBILEPHONE { get; set; }
        public string EMP_STATE { get; set; }
        public string EMP_BRANCH_CODE { get; set; }
        public DateTime? EMPPAY_DATE_JOINED { get; set; }
        public string EMPPAY_CATEGORY { get; set; }
        public string EMP_ROLE { get; set; }
        public bool IsActive { get; set; }
    }
}
