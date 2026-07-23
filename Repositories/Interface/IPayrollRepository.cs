using Microsoft.AspNetCore.Mvc;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;

namespace OBMS.WebAPI.Repositories.Interface
{
    public interface IPayrollRepository
    {
        Task<List<SalaryAdvanceDto>> GetEmployeeList();
        Task<List<SalaryAdvanceDto>> GetEmployeeListBySalaryAdvance();
        Task<List<SalaryAdvanceDto>> GetEmployeeListByBranchCode(string branchCode);
        Task<List<SalaryAdvanceDto>> GetEmployeeListByAdvanceID(int Id);
        Task<List<SalaryAdvanceDto>> GetListByEmplyeeType(DateTime advanceDate, string branch, string employeeType, int transType, decimal advanceAmount, string race);
        Task<SalaryAdvance> SaveAndUpdateSalaryMonthlyAdvance(SalaryAdvance salaryAdvance, List<EmployeeItemIssueDto>? items = null);
        Task<List<SalaryAdvance>> GetSalaryAdvanceById(int employeeId);
        Task<List<Employee>> GetEmployeeById(int employeeId);
        Task<List<SalaryAdvance>> GetSalaryAdvanceByDateAndEmployee(SalaryAdvance salaryAdvance);
        Task<List<InventoryCategory>> GetInventoryCategories();
        string GetNewVoucherNumberAsync(int transType);
        List<ItemMasterDto> GetUniformItemRows(int AdvanceID, int Category);
        bool GetSalaryProcessDateByEmployeeID(int employeeID, int year, int month);
        DateTime GetResignDateByEmployeeID(int employeeID);
        Task<IEnumerable<MiscTrans>> GetMiscTrans();
        Task<List<MiscTrans>> GetMiscTransById(int id);
        Task<MiscTrans> SaveAndUpdateMiscTrans(MiscTrans miscTrans);
        Task DeleteMiscTransById(int id);
        Task<List<SalaryAdvance>> SaveAndUpdateSalaryDailyAdvances(List<SalaryAdvance> salaryAdvances);
        List<EmployeeDailyAdvanceRow> GetDailyAdvanceList(DateTime advanceDate, int employeeID, int advanceType);
        EmploymentDetails Get(string employeeNo);
        Task<string?> GetEmployeeNoAsync(int employeeId);
        Task<bool> DeleteSalaryAdvanceAsync(int salaryAdvanceID, string currentUser);
        Task<IEnumerable<SalaryAdvance>> GetSalaryAdvancesAsync(DateTime advanceDate, int employeeId, int transType);
        #region Attendance
        ActionResult<IEnumerable<ClientMaster>> GetClients(DateTime period, string branchCode);
        Task<ClientMaster> GetClientByCode(string clientCode);
        Task<List<ClientMaster>> GetClientsByBranch(string branchCode);
        Attendance AttendanceByEmployeeID(DateTime Period,int employeeID);
        Task<List<AttendanceDetails>> AttendanceDetailsByID(int Id);
        Task<List<AttendanceDetails>> GetAttendanceDetailsList(int AttendanceID);
        Task<List<SalaryAttendenceDto>> GetEmployeeDetails(string branchCode, string employeeNo);
        bool IsSalaryProcessDoneForCurrentPeriod(string branch,string employeeType, DateTime dtPeriod);
        int CalculateAge(DateTime birthDate);
        List<string> GetEmployeeAttendanceList(DateTime period, string branch);
        bool IsTemporaryEmployee(string employeeCode);
        int GetAnnualLeave(int employeeId, DateTime period);
        int DateDiffInMonths(DateTime startDate, DateTime endDate);
        Task<ActionResult> SaveAndUpdateAttendance(Attendance attendanceModel, List<AttendanceDetails> attendanceDetails);
        Task<AttendanceBulkUploadResultDto> SaveSimplifiedBulkAttendance(List<AttendanceBulkUploadDto> attendanceData, string currentUser);
        Task<bool> DeleteAttendanceAsync(int dID);
        List<EmployeeDto> GetList(string branch, string employeeType, DateTime resignedDate, DateTime joinDate, DateTime attendancePeriod, string status);
        List<EmployeeDto> getListByEmployee(string branch, string employeeType, DateTime resignedDate, DateTime joinDate, string status);
        Task<DateTime?> GetLatestAttendancePeriodAsync(int employeeId, int year, int month);
        Task<List<object>> GetTodayAttendanceList(string branch);
        Task<List<object>> GetAttendanceByDate(DateTime attendanceDate, string branch);
        #endregion

        #region Salary Processing
        string LastSalaryProcessRemarks(DateTime period, string branchCode, string employeeType);
        string Process(string branch, string employeeType, string remarks, DateTime period, bool lockProcess, string currentUser, string companyCode);
        #endregion

        #region Dynamic Payslip Data
        Task<object> GetPayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang);
        Task<object> GetPayslip2Data(string loginId, string branch, string period, string employeeType, string employee, string lang);
        Task<object> GetNewPayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang);
        Task<object> GetTimeSheetData(string loginId, string branch, string period, string employeeType, string employee, string lang);
        Task<object> GetGuard1PayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang);
        Task<object> GetGuard2PayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang);
        Task<object> GetRBAPayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang);
        #endregion

        #region RBI Bank Salary Export
        List<BusinessObjects.RbiBankSalaryExport> GetRbiBankSalaryExportData(string dtSalaryPeriod, string branch, string employeeType);
        List<BusinessObjects.RbiBankAdvanceExport> GetRbiBankAdvanceExportData(string dtSalaryPeriod, string branch, string employeeType);
        #endregion

        #region Voucher Filter Report
        Task<List<VoucherDetailDto>> GetVoucherDetailsByFilter(string branch, DateTime period, string employeeType, string bankCode, string paymentType, string voucherType);
        #endregion

        #region Bulk Attendance Upload
        Task<AttendanceBulkUploadResultDto> BulkUploadAttendance(List<AttendanceBulkUploadDto> attendanceData);
        Task<List<AttendanceBulkUploadDto>> GetAttendanceDataForExport(DateTime period, string branchCode);
        #endregion

    }
}
