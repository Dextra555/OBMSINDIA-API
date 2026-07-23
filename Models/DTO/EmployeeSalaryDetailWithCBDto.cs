namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeSalaryDetailWithCBDto
    {
        // Original EmployeeSalaryDetails properties
        public int EMPFL_ID { get; set; }
        public string EMPFL_CODE { get; set; }
        public string EMPFL_BRANCHCODE { get; set; }
        public string? EMPFL_BANK { get; set; }
        public string? EMPFL_BK_ACCNO { get; set; }
        public string? EMPFL_TAX_NO { get; set; }
        public string? EMPFL_EPFNO { get; set; }
        public bool? EMPFL_EPF8Pa { get; set; }
        public string? EMPFL_SOSCO_NO { get; set; }
        public bool EPFDETECT { get; set; }
        public string PAYMODE { get; set; }
        public bool SOCSODETECT { get; set; }
        public bool TMPGUARD { get; set; }
        public bool DETECTBYND55 { get; set; }
        public bool? INCOMETAXDETECT { get; set; }
        public string? EMP_SP_TEL_NO { get; set; }
        public DateTime LASTUPDATE { get; set; }
        public string LastUpdatedBy { get; set; }
        public string? OT { get; set; }

        // Employment Details properties
        public decimal? AttendanceAllowanceWorkingDays { get; set; }
        public string? AttendanceAllowanceFollowCalendar { get; set; } = "N";

        // CB properties from Employee - Simplified
        public decimal? CB_Basic { get; set; }
        public decimal? CB_DA { get; set; }
        public decimal? CB_HRA { get; set; }
        public decimal? CB_HRAPercentage { get; set; }
        public decimal? CB_Leaves { get; set; }
        public decimal? CB_LeavesPercentage { get; set; }
        public decimal? CB_OtherAllowances { get; set; }
        public decimal? CB_NH { get; set; }
        public decimal? CB_NHPercentage { get; set; }
        public decimal? CB_AdvanceStatutoryBonus { get; set; }
        public decimal? CB_AdvanceStatutoryBonusPercentage { get; set; }
        public decimal? CB_SubTotal { get; set; }
    }
}
