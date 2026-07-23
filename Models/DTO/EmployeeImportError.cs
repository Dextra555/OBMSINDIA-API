namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeImportError
    {
        public int RowNumber { get; set; }
        public string? ErrorMessage { get; set; }
        public string? EmployeeCode { get; set; }
        public bool IsSuccess { get; set; }
        public string? EmployeeName { get; set; }
        public string? FieldName { get; set; }
    }
}
