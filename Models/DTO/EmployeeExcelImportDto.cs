using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeExcelImportDto
    {
        public string? EmployeeCode { get; set; }
        public string? Name { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? Category { get; set; }
        public DateTime? DateJoined { get; set; }
        public string? Status { get; set; }
        public decimal? BasicSalary { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? NRIC { get; set; }
        public string? EPFNo { get; set; }
        public string? SOCSONo { get; set; }
        public string? TaxNo { get; set; }
        public string? EMP_CODE { get; set; }
        public string? EMP_NAME { get; set; }
        public DateTime? EMPPAY_DATE_CONFIRM { get; set; }
        public double? EMPPAY_BASIC_RATE { get; set; }
        public string? EMPPAY_JOB_TITLE { get; set; }
        public string? UPIId { get; set; }
        public string? SalaryGroup { get; set; }
        public string? IndianState { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? EMPPAY_CATEGORY { get; set; }
        public DateTime? EMPPAY_DATE_JOINED { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
        public string? BankIFSC { get; set; }
        public string? PANNumber { get; set; }
        public string? AadhaarNumber { get; set; }
        public string? EMP_ROLE { get; set; }
        public string? EMP_BRANCH_CODE { get; set; }
        public string? EMP_SEX { get; set; }
        public string? EMP_Race { get; set; }
        public string? EMP_FATHER_NAME { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string? EMP_ADDRESS1 { get; set; }
        public string? EMP_ADDRESS2 { get; set; }
        public string? EMP_POST_CODE { get; set; }
        public string? EMP_TOWN { get; set; }
        public string? EMP_STATE { get; set; }
        public string? EMP_NATIONAL { get; set; }
        public string? EMP_PHONE { get; set; }
        public string? EMP_MOBILEPHONE { get; set; }
        public string? EMP_HGH_EDU { get; set; }
        public DateTime? EMP_DATE_OF_BIRTH { get; set; }
        public string? EMP_MARTIAL_STATUS { get; set; }
        public string? EMP_SPOUSE_NAME { get; set; }
        public int? EMP_NO_CHILD { get; set; }
        public string? PFAccountNumber { get; set; }
        public string? ESINumber { get; set; }
        public string? Branch { get; set; }
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }

        // CB (Cost Breakdown) Fields - Only 7 essential fields for salary breakdown
        public decimal? CB_Basic { get; set; }  // Basic Salary
        public decimal? CB_DA { get; set; }  // Dearness Allowance
        public decimal? CB_HRA { get; set; }  // House Rent Allowance
        public decimal? CB_Leaves { get; set; }  // Leave Wages
        public decimal? CB_NH { get; set; }  // National Festival Allowance (NFH)
        public decimal? CB_AdvanceStatutoryBonus { get; set; }  // Advance Statutory Bonus
        public decimal? CB_OtherAllowances { get; set; }  // Other Allowances
        public decimal? CB_SubTotal { get; set; }  // Sub Total (calculated)

        // Employment Details Fields
        public decimal? ATTENDANCEALLOWANCE { get; set; }
        public decimal? SpecialAllowance { get; set; }
        public decimal? AttendanceAllowanceWorkingDays { get; set; }
        public string? AttendanceAllowanceFollowCalendar { get; set; } = "N";

        // Employee Salary Details Fields
        public string? EMPFL_BANK { get; set; }
        public string? EMPFL_BK_ACCNO { get; set; }
        public string? EMPFL_EPFNO { get; set; }
        public string? EMPFL_SOSCO_NO { get; set; }
        public string? PAYMODE { get; set; }
        public bool? EPFDETECT { get; set; }
        public bool? SOCSODETECT { get; set; }
        public bool? TMPGUARD { get; set; }
        public bool? DETECTBYND55 { get; set; }
        public bool? INCOMETAXDETECT { get; set; }

        // Employee Flags
        public string? NewSalaryStructure { get; set; }
        public string? SalaryStructure1000_3h { get; set; }
        public int? EMP_CHECKLIST { get; set; }
        public bool? KDNVetting { get; set; }
        public bool? HasTransfered { get; set; }
        public string? EMP_SP_WORK { get; set; }
        public string? EMP_CLIENT { get; set; }
        public string? EMP_SP_TEL_NO { get; set; }

        // Department and Designation Codes (for bulk upload)
        public string? DepartmentCode { get; set; }
        public string? DesignationCode { get; set; }
        public string? ClientCode { get; set; }

        // Department and Designation IDs (resolved from codes)
        public int? DepartmentId { get; set; }
        public int? DesignationId { get; set; }
    }
}
