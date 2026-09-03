using System.ComponentModel.DataAnnotations;

namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeUpdateDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public int EMP_ID { get; set; }

        [Required(ErrorMessage = "Employee code is required")]
        [StringLength(20, ErrorMessage = "Employee code cannot exceed 20 characters")]
        public string EMP_CODE { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        [StringLength(100, ErrorMessage = "Employee name cannot exceed 100 characters")]
        public string EMP_NAME { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string EMP_ROLE { get; set; }

        public string EMP_ADDRESS1 { get; set; }
        public string EMP_ADDRESS2 { get; set; }

        [Required(ErrorMessage = "PIN code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN code must be 6 digits")]
        public string EMP_POST_CODE { get; set; }

        public string EMP_TOWN { get; set; }
        
        [Required(ErrorMessage = "State is required")]
        public string EMP_STATE { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\+91[6-9]\d{9}$", ErrorMessage = "Phone number must be in +91 format followed by 10 digits starting with 6-9")]
        public string EMP_PHONE { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\+91[6-9]\d{9}$", ErrorMessage = "Mobile number must be in +91 format followed by 10 digits starting with 6-9")]
        public string EMP_MOBILEPHONE { get; set; }

        public string EMP_HGH_EDU { get; set; }
        public string EM_WORK_EXP { get; set; }
        public DateTime? EMP_DATE_OF_BIRTH { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string EMP_SEX { get; set; }

        public string EMP_MARTIAL_STATUS { get; set; }
        public string? EMP_SPOUSE_NAME { get; set; }
        public string? EMP_FATHER_NAME { get; set; }
        public int? EMP_NO_CHILD { get; set; }
        public bool EMP_SP_WORK { get; set; }
        public string? EMP_PER_NAME_CONTACT { get; set; }
        public string EMP_CONTACT_ADDRESS1 { get; set; }
        public string EMP_CONTACT_ADDRESS2 { get; set; }
        public string EMP_CONTACT_POST_CODE { get; set; }
        public string EMP_CONTACT_TOWN { get; set; }
        public string EMP_CONTACT_STATE { get; set; }
        public string EMP_CONTACT_TELEPHONE { get; set; }

        [Required(ErrorMessage = "Branch code is required")]
        public string EMP_BRANCH_CODE { get; set; }

        public string OldBranch { get; set; }
        public DateTime? TransferDate { get; set; }
        public bool HasTransfered { get; set; }

        /// <summary>True when the branch is being changed while editing this employee.</summary>
        public bool IsBranchChanged { get; set; }

        /// <summary>Effective Start Date at the new branch — required when IsBranchChanged = true.
        /// It closes the previous open EmployeeHistory row (Emp_EndDate) and becomes the
        /// Emp_StartDate of the new branch-change row.</summary>
        public DateTime? BranchStartDate { get; set; }

        public int EMP_CITIZEN { get; set; }
        public int EMP_CHECKLIST { get; set; }
        public string? EMP_CLIENT { get; set; }
        public char? NewSalaryStructure { get; set; }
        public bool KDNVetting { get; set; }
        public char? SalaryStructure1000_3h { get; set; }

        // Indian Compliance Fields
        [RegularExpression(@"^[2-9]\d{11}$", ErrorMessage = "Aadhaar must be 12 digits starting with 2-9")]
        public string? AadhaarNumber { get; set; }

        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "PAN must be in format ABCDE1234F")]
        public string? PANNumber { get; set; }

        public string? PFAccountNumber { get; set; }
        public string? ESINumber { get; set; }

        [RegularExpression(@"^(None|8 Hours|10 Hours)$", ErrorMessage = "Salary group must be None, 8 Hours, or 10 Hours")]
        public string? SalaryGroup { get; set; }

        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Spouse PAN must be in format ABCDE1234F")]
        public string? SpousePAN { get; set; }

        [RegularExpression(@"^[2-9]\d{11}$", ErrorMessage = "Spouse Aadhaar must be 12 digits starting with 2-9")]
        public string? SpouseAadhaar { get; set; }
        
        public string? IndianState { get; set; }

        [RegularExpression(@"^\d{10,18}$", ErrorMessage = "Bank account number must be 10-18 digits")]
        public string? BankAccountNumber { get; set; }

        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "IFSC must be in format ABCD0XXXXXX")]
        public string? BankIFSC { get; set; }

        public string? BankName { get; set; }
        public string? UPIId { get; set; }

        // Employment Details
        public int EMPPAY_ID { get; set; }
        public string EMPPAY_JOB_TITLE { get; set; }
        public string EMPPAY_CATEGORY { get; set; }
        public DateTime? EMPPAY_DATE_JOINED { get; set; }
        public DateTime? EMPPAY_DATE_CONFIRM { get; set; }
        public DateTime? EMPPAY_DATE_RESIGNED { get; set; }
        public double EMPPAY_BASIC_RATE { get; set; }
        public decimal SALARYLAB { get; set; }
        public decimal? ATTENDANCEALLOWANCE { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal? AttendanceAllowanceWorkingDays { get; set; }
        public string? AttendanceAllowanceFollowCalendar { get; set; } = "N";

        // Commercial Breakdown (CB) Fields
        public decimal? CB_Basic { get; set; }
        public decimal? CB_DA { get; set; }
        public decimal? CB_HRA { get; set; }
        public decimal? CB_HRAPercentage { get; set; }
        public decimal? CB_Leaves { get; set; }
        public decimal? CB_LeavesPercentage { get; set; }
        public decimal? CB_OtherAllowances { get; set; }
        public decimal? CB_NH { get; set; }
        public decimal? CB_NHPercentage { get; set; }
        public decimal? CB_SubTotal { get; set; }

        // Salary Details
        public int EMPFL_ID { get; set; }
        public string? EMPFL_BANK { get; set; }
        public string? EMPFL_BK_ACCNO { get; set; }
        public string? EMPFL_TAX_NO { get; set; }
        public bool EPFDETECT { get; set; }
        public string PAYMODE { get; set; }
        public bool SOCSODETECT { get; set; }
        public bool TMPGUARD { get; set; }
        public bool DETECTBYND55 { get; set; }
        public bool? INCOMETAXDETECT { get; set; }
        public string? EMP_SP_TEL_NO { get; set; }
        public string? OT { get; set; }

        public DateTime LASTUPDATE { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
