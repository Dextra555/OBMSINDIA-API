using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeImportPreviewDto
    {
        public Employee Employee { get; set; }
        public string? ValidationMessage { get; set; }
        public bool IsValid { get; set; }
        public int RowNumber { get; set; }
        public List<EmployeeImportPreviewDto> ValidRecords { get; set; } = new List<EmployeeImportPreviewDto>();
        public List<EmployeeImportError> ValidationErrors { get; set; } = new List<EmployeeImportError>();
        public int TotalRecords { get; set; }
        public int ValidCount { get; set; }
        public int InvalidCount { get; set; }
    }
}
