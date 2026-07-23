namespace OBMS.WebAPI.Models.DTO
{
    /// <summary>
    /// Enhanced bulk upload result with detailed error and success information
    /// </summary>
    public class AttendanceBulkUploadResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedRecords { get; set; }
        
        /// <summary>
        /// Summary of all errors organized by type
        /// </summary>
        public List<AttendanceBulkUploadErrorDto> Errors { get; set; } = new();
        
        /// <summary>
        /// Successful record details
        /// </summary>
        public List<AttendanceBulkUploadSuccessDto> SuccessRecords { get; set; } = new();
        
        /// <summary>
        /// Field-level validation errors
        /// </summary>
        public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();
        
        /// <summary>
        /// Row numbers that had parse errors
        /// </summary>
        public List<int> ProblematicRows { get; set; } = new();
        
        /// <summary>
        /// Overall processing statistics
        /// </summary>
        public AttendanceBulkUploadStatisticsDto Statistics { get; set; } = new();
    }

    public class AttendanceBulkUploadErrorDto
    {
        public int RowNumber { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? ErrorType { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class AttendanceBulkUploadSuccessDto
    {
        public int RowNumber { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int AttendanceID { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime ProcessedTime { get; set; } = DateTime.UtcNow;
    }

    public class AttendanceBulkUploadStatisticsDto
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public float SuccessRate { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public int ValidationErrorCount { get; set; }
        public int DatabaseErrorCount { get; set; }
        public int ParsingErrorCount { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class AttendanceValidationErrorDto
    {
        public string Field { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int Row { get; set; }
    }
}
