namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeBulkImportRequestDto
    {
        public string FilePath { get; set; }
        public string? Branch { get; set; }
        public string? CreatedBy { get; set; }
        public string? ImportedBy { get; set; }
        public bool? UpdateExisting { get; set; }
        public List<EmployeeImportPreviewDto> Employees { get; set; } = new List<EmployeeImportPreviewDto>();
    }
}
