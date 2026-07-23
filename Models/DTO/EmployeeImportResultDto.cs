namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeImportResultDto
    {
        public int TotalRecords { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public bool IsSuccess { get; set; }
        public List<EmployeeImportPreviewDto> SuccessfulEmployees { get; set; } = new List<EmployeeImportPreviewDto>();
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<EmployeeImportError> Errors { get; set; } = new List<EmployeeImportError>();
    }
}
