using System.ComponentModel.DataAnnotations;

namespace OBMS.WebAPI.Models.DTO
{
    public class AttendanceBulkUploadDto
    {
        [Required]
        public string EmployeeCode { get; set; } = string.Empty;
        
        [Required]
        public string EmployeeName { get; set; } = string.Empty;
        
        [Required]
        public string BranchCode { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Period { get; set; }
        
        // Attendance Header Fields
        public int Shift2Type { get; set; } = 0;
        public decimal Shift2Rate { get; set; } = 0;
        public decimal AllowanceDeduction { get; set; } = 0;
        public decimal SpecialAllowanceDeduction { get; set; } = 0;
        public decimal Bonus { get; set; } = 0;
        
        // Daily Attendance Details (for each day of the month)
        public List<AttendanceDayDetailDto> DailyAttendance { get; set; } = new List<AttendanceDayDetailDto>();
        
        // Simplified format fields (for PDF-style bulk upload with P, W/O, H, L, NH codes)
        public string? Unit { get; set; }
        public string? Designation { get; set; }
        public Dictionary<int, string> DailyAttendanceCodes { get; set; } = new Dictionary<int, string>();
        
        // Summary fields (calculated from daily codes)
        public int Duty { get; set; } = 0;
        public int WeeklyOff { get; set; } = 0;
        public int HolidayOrNationalHoliday { get; set; } = 0;
        public int Leave { get; set; } = 0;
        public int Total { get; set; } = 0;
    }

    public class AttendanceDayDetailDto
    {
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime AttendanceDate { get; set; }
        
        public string? Client { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? TimeStart { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? TimeEnd { get; set; }
        
        public string? OTClient { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? OTTimeStart { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? OTTimeEnd { get; set; }
        
        [Required]
        public string WorkType { get; set; } = "General Working";
        
        /// <summary>
        /// Simplified attendance code (P, W/O, H, L, NH)
        /// IMPORTANT: This field stores the original code from the input
        /// </summary>
        [Required]
        public string AttendanceCode { get; set; } = "P";
        
        /// <summary>
        /// Numeric type mapping (1=General Working, 2=Off Day, 4=Holiday, 8=Leave, etc.)
        /// IMPORTANT: This field explicitly stores the Type ID for database storage
        /// </summary>
        public int TypeID { get; set; } = 0;
        
        // Calculated fields
        public decimal WorkingHours { get; set; } = 0;
        public decimal OvertimeHours { get; set; } = 0;
        public string? Remarks { get; set; }
        
        public string ToDomainWorkType()
        {
            return WorkType ?? "General Working";
        }
    }

    public class AttendanceTemplateDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public DateTime Period { get; set; }
        public int Shift2Type { get; set; } = 0;
        public decimal Shift2Rate { get; set; } = 0;
        public decimal AllowanceDeduction { get; set; } = 0;
        public decimal SpecialAllowanceDeduction { get; set; } = 0;
        public decimal Bonus { get; set; } = 0;
    }

    /// <summary>
    /// Code mapping for simplified attendance system (P, W/O, H, L, NH)
    /// IMPORTANT: This mapping is the single source of truth for code-to-type conversions
    /// </summary>
    public static class AttendanceCodeMapping
    {
        public static readonly Dictionary<string, int> CodeToTypeMap = new Dictionary<string, int>
        {
            { "P", 1 },        // Present → General Working (Type 1)
            { "W/O", 2 },      // Weekly Off → Off Day (Type 2)
            { "H", 4 },        // Holiday → Holiday (Type 4)
            { "L", 7 },        // Leave (Absent) → Absent (Type 7)
            { "NH", 5 }        // National Holiday → Holiday Working (Type 5)
        };

        public static readonly Dictionary<int, string> TypeToCodeMap = new Dictionary<int, string>
        {
            { 1, "P" },        // General Working → Present
            { 2, "W/O" },      // Off Day → Weekly Off
            { 3, "P" },        // Off Day Working → Present
            { 4, "H" },        // Holiday → Holiday
            { 5, "NH" },       // Holiday Working → National Holiday
            { 7, "L" },        // Absent → L
            { 8, "L" },        // Annual Leave → L
            { 9, "L" },        // Medical Leave → L
            { 10, "L" },       // Maternity Leave → L
            { 11, "L" },       // Paternity Leave → L
            { 12, "L" },       // Hospitalization Leave → L
            { 13, "W/O" },     // Rest Day → Weekly Off
            { 14, "L" },       // Unpaid Leave → L
            { 17, "L" }        // Marriage Leave → L
        };

        /// <summary>
        /// Get numeric type from attendance code
        /// </summary>
        public static int GetAttendanceType(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return 1; // Default to General Working

            var upperCode = code.ToUpper().Trim();
            return CodeToTypeMap.TryGetValue(upperCode, out var type) ? type : 1;
        }

        /// <summary>
        /// Get attendance code from numeric type
        /// </summary>
        public static string GetAttendanceCode(int type)
        {
            return TypeToCodeMap.TryGetValue(type, out var code) ? code : "P";
        }

        /// <summary>
        /// Get user-friendly description of attendance code
        /// </summary>
        public static string GetCodeDescription(string code)
        {
            return code?.ToUpper() switch
            {
                "P" => "Present - Regular working day",
                "W/O" => "Weekly Off - Non-working day",
                "H" => "Holiday - Public holiday",
                "L" => "Absent - No pay deduction",
                "NH" => "National Holiday - National festival",
                _ => "Unknown"
            };
        }
    }
}
