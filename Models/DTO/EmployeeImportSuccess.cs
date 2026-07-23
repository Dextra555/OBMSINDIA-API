namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeImportSuccess
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public int RowNumber { get; set; }
        public string Action { get; set; }
    }
}
