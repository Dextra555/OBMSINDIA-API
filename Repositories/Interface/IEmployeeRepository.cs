using Microsoft.AspNetCore.Http;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;

namespace OBMS.WebAPI.Repositories.Interface
{
    public interface IEmployeeRepository
    {
        Task<Dictionary<string, string>> GetEmployeeNoByBranchID(string branchId);
        Task<Dictionary<string, string>> GetEmployeeNo();
        Task<Dictionary<string, Object>> GetEmployeeMasterList(string userID);
        Task<List<Object>> GetClientsFromBranchId(string branchId);
        Task<Employee> saveAndUpdateEmployee(Employee employee, EmploymentDetails employment, EmployeeSalaryDetails salaryDetails);
        Task<Employee> UpdateEmployeeTransfer(EmployeeTransferDto employeeTransferDto);
        Task<Dictionary<string, Object>> GetEmployeeById(int employeeId);
        Task<Object> CheckEmployeeInfo(string from,string data);
        Task<List<EmployeeHistoryDto>> GetAllEmployeesWithHistory(string? branch = null);

        // Indian Compliance Methods
        Task<Dictionary<string, object>> CreateEmployee(EmployeeCreateDto employeeDto);
        Task<Dictionary<string, object>> UpdateEmployee(EmployeeUpdateDto employeeDto);
        Task<List<EmployeeSearchResultDto>> SearchEmployees(EmployeeSearchDto searchDto);
        Task<Dictionary<string, object>> ValidateEmployeeField(EmployeeValidationDto validationDto);
        Task<object> GetEmployeeByPAN(string pan);
        Task<object> GetEmployeeByAadhaar(string aadhaar);
        Task<bool> DeleteEmployee(int id);

        // Excel Import Methods
        Task<EmployeeImportPreviewDto> PreviewExcelImportAsync(IFormFile file);
        Task<EmployeeImportResultDto> BulkImportEmployeesAsync(EmployeeBulkImportRequestDto request);
        byte[] GenerateImportTemplate();
    }
}
