using OBMS.WebAPI.Models.DTO;

namespace OBMS.WebAPI.Services
{
    public interface IAttendanceErrorLogger
    {
        void LogParsingError(int row, string employeeCode, string message, Exception? ex = null);
        void LogValidationError(int row, string employeeCode, string field, string message);
        void LogDatabaseError(string employeeCode, string operation, Exception ex);
        void LogBulkUploadStart(string userId, int recordCount);
        void LogBulkUploadCompletion(string userId, int successCount, int failureCount);
        List<AttendanceErrorLogDto> GetRecentErrors(int limit = 100);
    }

    public class AttendanceErrorLogger : IAttendanceErrorLogger
    {
        private readonly ILogger<AttendanceErrorLogger> _logger;
        private readonly List<AttendanceErrorLogDto> _errorLog = new();

        public AttendanceErrorLogger(ILogger<AttendanceErrorLogger> logger)
        {
            _logger = logger;
        }

        public void LogParsingError(int row, string employeeCode, string message, Exception? ex = null)
        {
            var errorMsg = $"[Row {row}] Employee: {employeeCode} - {message}";
            
            if (ex != null)
            {
                _logger.LogError(ex, errorMsg);
            }
            else
            {
                _logger.LogWarning(errorMsg);
            }

            _errorLog.Add(new AttendanceErrorLogDto
            {
                Timestamp = DateTime.UtcNow,
                ErrorType = "ParsingError",
                Row = row,
                EmployeeCode = employeeCode,
                Message = message,
                Exception = ex?.ToString()
            });
        }

        public void LogValidationError(int row, string employeeCode, string field, string message)
        {
            var errorMsg = $"[Row {row}] Employee: {employeeCode} - Field: {field} - {message}";
            _logger.LogWarning(errorMsg);

            _errorLog.Add(new AttendanceErrorLogDto
            {
                Timestamp = DateTime.UtcNow,
                ErrorType = "ValidationError",
                Row = row,
                EmployeeCode = employeeCode,
                Field = field,
                Message = message
            });
        }

        public void LogDatabaseError(string employeeCode, string operation, Exception ex)
        {
            var errorMsg = $"Database Error - Employee: {employeeCode} - Operation: {operation}";
            _logger.LogError(ex, errorMsg);

            _errorLog.Add(new AttendanceErrorLogDto
            {
                Timestamp = DateTime.UtcNow,
                ErrorType = "DatabaseError",
                EmployeeCode = employeeCode,
                Message = operation,
                Exception = ex.Message
            });
        }

        public void LogBulkUploadStart(string userId, int recordCount)
        {
            _logger.LogInformation($"Bulk Attendance Upload Started - User: {userId}, Records: {recordCount}");
        }

        public void LogBulkUploadCompletion(string userId, int successCount, int failureCount)
        {
            _logger.LogInformation(
                $"Bulk Attendance Upload Completed - User: {userId}, Success: {successCount}, Failed: {failureCount}");
        }

        public List<AttendanceErrorLogDto> GetRecentErrors(int limit = 100)
        {
            return _errorLog.TakeLast(limit).ToList();
        }
    }

    public class AttendanceErrorLogDto
    {
        public DateTime Timestamp { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public int Row { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Field { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
    }
}
