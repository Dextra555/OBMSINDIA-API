namespace OBMS.WebAPI.Models.DTO
{
    public class SalaryAttendenceDto
    {
        public int ID { get; set; }
        public int EMP_ID { get; set; }
        public string? EMP_NAME { get; set; }
        public string? EMP_CODE { get; set; }
        
        // Indian Compliance Fields
        public string? AadhaarNumber { get; set; }
        public string? PANNumber { get; set; }
        public string? PFAccountNumber { get; set; }
        public string? ESINumber { get; set; }
        public string? UANNumber { get; set; }
        public bool? PFDETECT { get; set; }
        public bool? ESIDETECT { get; set; }
        public bool? INCOMETAXDETECT { get; set; }
        
        // Bank Details
        public string? EMPFL_BANK { get; set; }
        public string? EMPFL_BK_ACCNO { get; set; }
        public string? BankIFSC { get; set; }
        public string? BankName { get; set; }
        public string? PAYMODE { get; set; }
        
        // Other fields
        public decimal Amount { get; set; }
        public string? Particulars { get; set; }
        public DateTime? EMP_DATE_OF_BIRTH { get; set; }
        public DateTime? EMPPAY_DATE_JOINED { get; set; }
        public DateTime? EMPPAY_DATE_RESIGNED { get; set; }
        public decimal? ATTENDANCEALLOWANCE { get; set; }
        public decimal? SpecialAllowance { get; set; }
        public double? EMPPAY_BASIC_RATE { get; set; }
        public string? Name { get; set; }
        public string? SalaryStructure { get; set; }
        
        // Working Days and Allowance Configuration
        public decimal? AttendanceAllowanceWorkingDays { get; set; }
        public decimal? WorkingDays { get; set; }
        public string? AttendanceAllowanceFollowCalendar { get; set; } = "N";
    }
}
