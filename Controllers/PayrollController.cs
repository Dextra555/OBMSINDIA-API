using Microsoft.AspNetCore.Mvc;

using OBMS.WebAPI.Models.Domain;

using OBMS.WebAPI.Models;

using OBMS.WebAPI.Models.DTO;

using OBMS.WebAPI.Repositories.Interface;

using Microsoft.EntityFrameworkCore;

using OBMS.WebAPI.BusinessObjects;

using OBMS.WebAPI.Services;



namespace OBMS.WebAPI.Controllers

{

    [Route("api/[controller]")]

    [ApiController]

    public class PayrollController : ControllerBase

    {

        HttpResponseMessage response = new HttpResponseMessage();

        private readonly IPayrollRepository _payrollRepository;

        private readonly OBMSDbContext _oBMSDbContext;

        private readonly ISalaryProcess _salaryProcess;

        private readonly IProfessionalTaxService _professionalTaxService;

        private readonly IAttendanceExcelService _attendanceExcelService;

        private readonly IAttendancePeriodService _attendancePeriodService;



        public PayrollController(IPayrollRepository payrollRepository, OBMSDbContext oBMSDbContext, ISalaryProcess salaryProcess, IProfessionalTaxService professionalTaxService, IAttendanceExcelService attendanceExcelService, IAttendancePeriodService attendancePeriodService)

        {

            _payrollRepository = payrollRepository;

            _oBMSDbContext = oBMSDbContext;

            _salaryProcess = salaryProcess;

            _professionalTaxService = professionalTaxService;

            _attendanceExcelService = attendanceExcelService;

            _attendancePeriodService = attendancePeriodService;

        }



        [HttpGet]

        [Route("GetEmployeeList")]

        public async Task<ActionResult<SalaryAdvanceRequestDto>> GetEmployeeList()

        {

            try

            {

                var employeeList = await _payrollRepository.GetEmployeeList();

                return Ok(employeeList);

            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetEmployeeListBySalaryAdvance")]

        public async Task<ActionResult<SalaryAdvanceRequestDto>> GetEmployeeListBySalaryAdvance()

        {

            try

            {

                var employeeList = await _payrollRepository.GetEmployeeListBySalaryAdvance();

                return Ok(employeeList);

            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetEmployeeListByBranchCode")]

        public async Task<ActionResult<SalaryAdvanceRequestDto>> GetEmployeeListByBranchCode(string branchCode)

        {

            try

            {

                var employeeList = await _payrollRepository.GetEmployeeListByBranchCode(branchCode);

                return Ok(employeeList);

            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetEmployeeListByAdvanceID")]

        public async Task<ActionResult<SalaryAdvanceRequestDto>> GetEmployeeListByAdvanceID(int Id)

        {

            try

            {

                var employeeList = await _payrollRepository.GetEmployeeListByAdvanceID(Id);

                return Ok(employeeList);

            }

            catch

            {

                throw;

            }



        }

        [HttpGet]

        [Route("GetListByEmplyeeType")]

        public async Task<ActionResult<SalaryAdvanceRequestDto>> GetListByEmplyeeType(DateTime advanceDate, string branch, string employeeType, int transType, decimal advanceAmount, string race)

        {

            try

            {
                Console.WriteLine($"[PayrollController] GetListByEmplyeeType called with: advanceDate={advanceDate}, branch={branch}, employeeType={employeeType}, transType={transType}, advanceAmount={advanceAmount}, race={race}");
                var employeeList = await _payrollRepository.GetListByEmplyeeType(advanceDate, branch, employeeType, transType, advanceAmount, race);
                Console.WriteLine($"[PayrollController] Employee list count: {employeeList?.Count ?? 0}");
                return Ok(employeeList);

            }

            catch (Exception ex)

            {
                Console.WriteLine($"[PayrollController] ERROR in GetListByEmplyeeType: {ex.Message}");
                Console.WriteLine($"[PayrollController] Stack trace: {ex.StackTrace}");
                throw;

            }



        }



        [HttpGet]

        [Route("GetInventoryCategories")]

        public async Task<ActionResult<SalaryAdvanceRequestDto>> GetInventoryCategories()

        {

            try

            {

                var inventoryCategories = await _payrollRepository.GetInventoryCategories();

                return Ok(inventoryCategories);

            }

            catch

            {

                throw;

            }



        }



        [HttpPost]

        [Route("SaveAndUpdateSalaryMonthlyAdvance")]

        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateSalaryMonthlyAdvance(SalaryAdvanceRequestDto salaryAdvanceRequestDto)

        {

            try

            {

                var salaryAdvanceDetails = new SalaryAdvance()

                {

                    ID = salaryAdvanceRequestDto.ID,

                    EmployeeID = salaryAdvanceRequestDto.EmployeeID,

                    AdvanceTakenDate = salaryAdvanceRequestDto.AdvanceTakenDate,

                    AdvanceDate = salaryAdvanceRequestDto.AdvanceDate,

                    VoucherNo = salaryAdvanceRequestDto.VoucherNo,

                    Amount = salaryAdvanceRequestDto.Amount,

                    NoOfInstallments = salaryAdvanceRequestDto.NoOfInstallments,

                    PaymentType = salaryAdvanceRequestDto.PaymentType,

                    Particulars = salaryAdvanceRequestDto.Particulars,

                    TransType = salaryAdvanceRequestDto.TransType,

                    IsDeleted = salaryAdvanceRequestDto.IsDeleted,

                    LastUpdate = DateTime.Now,

                    LastUpdatedBy = salaryAdvanceRequestDto.LastUpdatedBy,

                };

                await _payrollRepository.SaveAndUpdateSalaryMonthlyAdvance(salaryAdvanceDetails, salaryAdvanceRequestDto.Items);



                if (salaryAdvanceDetails != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("SalaryAdvance", salaryAdvanceDetails);

                    dictResult.Add("Message", "Successfully save & update salary advance details.");

                    return Ok(dictResult);

                }

                else

                {

                    response.Headers.Add("Failure", "Branch save & update failed please check your key-in details....");

                }

                return response;

            }

            catch (Exception ex)

            {



                throw;

            }

        }



        [HttpGet]

        [Route("GetSalaryAdvanceById")]

        public async Task<ActionResult<SalaryAdvanceRequestDto>> GetSalaryAdvanceById(int employeeId)

        {

            try

            {

                var employeeList = await _payrollRepository.GetSalaryAdvanceById(employeeId);

                return Ok(employeeList);

            }

            catch

            {

                throw;

            }



        }

        [HttpGet]

        [Route("GetEmployeeById")]

        public async Task<ActionResult<EmployeeRequestDto>> GetEmployeeById(int employeeId)

        {

            try

            {

                var employeeList = await _payrollRepository.GetEmployeeById(employeeId);

                return Ok(employeeList);

            }

            catch

            {

                throw;

            }



        }



        [HttpPost]

        [Route("GetSalaryAdvanceByDateAndEmployee")]

        public async Task<ActionResult<HttpResponseMessage>> GetSalaryAdvanceByDateAndEmployee(SalaryAdvance salaryAdvance)

        {

            try

            {
                Console.WriteLine($"[PayrollController] GetSalaryAdvanceByDateAndEmployee called with EmployeeID: {salaryAdvance?.EmployeeID}, AdvanceDate: {salaryAdvance?.AdvanceDate}, Amount: {salaryAdvance?.Amount}");
                Console.WriteLine($"[PayrollController] Full SalaryAdvance object: {System.Text.Json.JsonSerializer.Serialize(salaryAdvance)}");

                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                if (salaryAdvance != null)

                {

                    Console.WriteLine($"[PayrollController] Checking salary process date for EmployeeID: {salaryAdvance.EmployeeID}, Year: {salaryAdvance.AdvanceDate.Year}, Month: {salaryAdvance.AdvanceDate.Month + 1}");
                    var result = _payrollRepository.GetSalaryProcessDateByEmployeeID(salaryAdvance.EmployeeID, salaryAdvance.AdvanceDate.Year, salaryAdvance.AdvanceDate.Month + 1);
                    Console.WriteLine($"[PayrollController] Salary process date check result: {result}");

                    if (result == true && salaryAdvance.LastUpdatedBy?.ToLower() != "superadmin")

                    {



                        dictResult.Add("True", "True");

                        dictResult.Add("SalaryAdvance", salaryAdvance);

                        dictResult.Add("Message", "Salary already Process for this Guard/Satff.You Not have Right To Update or Save. Please Contact HQ for More Information..");

                        Console.WriteLine("[PayrollController] Returning error: Salary already processed");
                        return Ok(dictResult);

                    }

                    else

                    {

                        Console.WriteLine($"[PayrollController] Getting resign date for EmployeeID: {salaryAdvance.EmployeeID}");
                        var resgindate = _payrollRepository.GetResignDateByEmployeeID(salaryAdvance.EmployeeID);
                        Console.WriteLine($"[PayrollController] Resign date: {resgindate}");

                        if (resgindate != new DateTime(1900, 1, 1))

                        {

                            if (salaryAdvance.AdvanceDate > resgindate)

                            {

                                dictResult.Add("ResignDate", "ResignDate");

                                dictResult.Add("SalaryAdvance", salaryAdvance);

                                dictResult.Add("Message", "Advance Date Cannot Over than Guard/Staff Resign Date. Please Contact HQ for More Information.");

                                Console.WriteLine("[PayrollController] Returning error: Advance date after resign date");
                                return Ok(dictResult);

                            }

                            else

                            {

                                Console.WriteLine("[PayrollController] Calling SaveAndUpdateSalaryMonthlyAdvance");
                                await _payrollRepository.SaveAndUpdateSalaryMonthlyAdvance(salaryAdvance, null);

                                dictResult.Add("Success", "Success");

                                dictResult.Add("SalaryAdvance", salaryAdvance);

                                dictResult.Add("Message", "Successfully save & update salary advance details.");

                                Console.WriteLine("[PayrollController] Successfully saved salary advance");
                                return Ok(dictResult);

                            }

                        }

                        else

                        {
                            Console.WriteLine("[PayrollController] No resign date, proceeding to save salary advance");
                            await _payrollRepository.SaveAndUpdateSalaryMonthlyAdvance(salaryAdvance, null);

                            dictResult.Add("Success", "Success");

                            dictResult.Add("SalaryAdvance", salaryAdvance);

                            dictResult.Add("Message", "Successfully save & update salary advance details.");

                            Console.WriteLine("[PayrollController] Successfully saved salary advance (no resign date)");
                            return Ok(dictResult);

                        }



                    }



                }



                //var employeeList = await _payrollRepository.GetSalaryAdvanceByDateAndEmployee(salaryAdvance);

                //if (employeeList != null && employeeList.Count > 0)

                //{

                //    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                //    dictResult.Add("Exists", "Exists");

                //    dictResult.Add("SalaryAdvance", employeeList);

                //    dictResult.Add("Message", "Record Exists.Please check");

                //    //response.Headers.Add("Message", "Record Exists.Please check");



                //    return Ok(dictResult);

                //}

                //else

                //{

                //    await _payrollRepository.SaveAndUpdateSalaryMonthlyAdvance(salaryAdvance, null);

                //    if (salaryAdvance != null)

                //    {

                //        Dictionary<string, object> dictResult = new Dictionary<string, object>();

                //        dictResult.Add("Success", "Success");

                //        dictResult.Add("SalaryAdvance", salaryAdvance);

                //        dictResult.Add("Message", "Successfully save & update salary advance details.");

                //        //response.Headers.Add("Success", "Successfully save & update salary advance details.");



                //        return Ok(dictResult);

                //    }

                //}

                else
                {
                    Console.WriteLine("[PayrollController] ERROR: salaryAdvance is null");
                    dictResult.Add("Error", "Error");
                    dictResult.Add("Message", "Salary advance data is null");
                    return BadRequest(dictResult);
                }

                return Ok(dictResult);

            }

            catch (Exception ex)

            {
                Console.WriteLine($"[PayrollController] ERROR in GetSalaryAdvanceByDateAndEmployee: {ex.Message}");
                Console.WriteLine($"[PayrollController] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[PayrollController] Inner exception: {ex.InnerException?.Message}");
                Dictionary<string, object> errorResult = new Dictionary<string, object>();
                errorResult.Add("Error", "Error");
                errorResult.Add("Message", $"An error occurred: {ex.Message}");
                return StatusCode(500, errorResult);
            }



        }



        [HttpGet]

        [Route("GetNewVoucherNumberAsync")]

        public IActionResult GetNewVoucherNumberAsync(int transType)

        {

            try

            {

                string voucherNumber = _payrollRepository.GetNewVoucherNumberAsync(transType);

                if (voucherNumber != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("VoucherNumber", voucherNumber);

                    return Ok(dictResult);

                }

                return Ok(voucherNumber);



            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetUniformItemRows")]

        public async Task<ActionResult<List<ItemMasterDto>>> GetUniformItemRows(int AdvanceID, int Category)

        {

            try

            {

                var uniformItemRows = _payrollRepository.GetUniformItemRows(AdvanceID, Category);

                return Ok(uniformItemRows);



            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetSalaryProcessDateByEmployeeID")]

        public bool GetSalaryProcessDateByEmployeeID(int employeeID, int year, int month)

        {

            try

            {

                return _payrollRepository.GetSalaryProcessDateByEmployeeID(employeeID, year, month);

            }

            catch (Exception ex)

            {



                throw;

            }

        }



        [HttpGet]

        [Route("GetResignDate")]

        public IActionResult GetResignDate(int employeeID)

        {

            try

            {

                var resignDate = _payrollRepository.GetResignDateByEmployeeID(employeeID);

                return Ok(resignDate);

            }

            catch (Exception ex) {

                return StatusCode(500, $"Internal Server Error: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("GetMiscTrans")]

        public async Task<ActionResult<IEnumerable<MiscTransDto>>> GetMiscTrans()

        {

            try

            {

                var miscTrans = await _payrollRepository.GetMiscTrans();

                return Ok(miscTrans);

            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetMiscTransById")]

        public async Task<ActionResult<MiscTransDto>> GetMiscTransById(int id)

        {

            try

            {

                var miscTrans = await _payrollRepository.GetMiscTransById(id);

                if (miscTrans == null)

                {

                    return NotFound();

                }



                return Ok(miscTrans);

            }

            catch (Exception ex)

            {



                throw;

            }



        }



        [HttpPost]

        [Route("SaveAndUpdateMiscTrans")]

        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateMiscTrans(MiscTransDto miscTrans)

        {

            try

            {

                var miscTransdetails = new MiscTrans()

                {

                    ID = miscTrans.ID,

                    TransDate = miscTrans.TransDate,

                    EmployeeID = miscTrans.EmployeeID,

                    TransType = miscTrans.TransType,

                    Amount = miscTrans.Amount,

                    Particulars = miscTrans.Particulars,

                    LastUpdate = DateTime.Now,

                    LastUpdatedBy = miscTrans.LastUpdatedBy,

                };

                await _payrollRepository.SaveAndUpdateMiscTrans(miscTransdetails);

                if (miscTransdetails != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("MiscTrans", miscTransdetails);

                    dictResult.Add("Message", "Successfully save & update misc trans details.");

                    return Ok(dictResult);

                }

                else

                {

                    response.Headers.Add("Failure", "Misc Trans save & update failed please check your key-in deatais....");

                }

                return response;

            }

            catch (Exception ex)

            {



                throw;

            }

        }



        //not yet  implemented

        [HttpDelete("DeleteMiscTransById")]

        public async Task<ActionResult> DeleteMiscTransById(int id)

        {

            try

            {

                await _payrollRepository.DeleteMiscTransById(id);

                return NoContent();

            }

            catch (Exception ex)

            {



                throw;

            }



        }



        [HttpPost]

        [Route("SaveAndUpdateSalaryDailyAdvances")]

        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateSalaryDailyAdvances(List<SalaryAdvanceRequestDto> salaryAdvanceRequestDto)

        {

            try

            {

                List<SalaryAdvance> updatedRecords = new List<SalaryAdvance>();



                foreach (var salaryAdvanceReques in salaryAdvanceRequestDto)

                {

                    var salaryAdvanceDetails = new SalaryAdvance()

                    {

                        ID = salaryAdvanceReques.ID,

                        EmployeeID = salaryAdvanceReques.EmployeeID,

                        AdvanceTakenDate = salaryAdvanceReques.AdvanceTakenDate,

                        AdvanceDate = salaryAdvanceReques.AdvanceDate,

                        VoucherNo = salaryAdvanceReques.VoucherNo,

                        Amount = salaryAdvanceReques.Amount,

                        NoOfInstallments = salaryAdvanceReques.NoOfInstallments,

                        PaymentType = salaryAdvanceReques.PaymentType,

                        Particulars = salaryAdvanceReques.Particulars,

                        TransType = salaryAdvanceReques.TransType,

                        IsDeleted = salaryAdvanceReques.IsDeleted,

                        LastUpdate = DateTime.Now,

                        LastUpdatedBy = salaryAdvanceReques.LastUpdatedBy,

                    };



                    // Add the current salaryAdvanceDetails to the updatedRecords list

                    updatedRecords.Add(salaryAdvanceDetails);

                }



                // Pass the updatedRecords list to the repository method

                await _payrollRepository.SaveAndUpdateSalaryDailyAdvances(updatedRecords);



                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                dictResult.Add("Success", "Success");

                dictResult.Add("SalaryAdvance", updatedRecords);

                dictResult.Add("Message", "Successfully save & update daily advance details");

                return Ok(dictResult);

            }

            catch (Exception ex)

            {

                // Handle exceptions appropriately

                return StatusCode(500, "An error occurred while processing the request.");

            }

        }



        [HttpGet]

        [Route("GetDailyAdvanceList")]

        public List<EmployeeDailyAdvanceRow> GetDailyAdvanceList(DateTime advanceDate, int employeeID, int advanceType)

        {

            try

            {

                return _payrollRepository.GetDailyAdvanceList(advanceDate, employeeID, advanceType);



            }

            catch (Exception ex)

            {



                throw;

            }

        }

        [HttpGet("GetEmployeeNo/{employeeId}")]

        public async Task<IActionResult> GetEmployeeNo(int employeeId)

        {

            var employeeNo = await _payrollRepository.GetEmployeeNoAsync(employeeId);

            if (employeeNo == null)

            {

                return NotFound(new { Message = "Employee not found" });

            }

            return Ok(new { EmployeeNo = employeeNo });

        }

        #region Attendance

        // ── GetAttendancePeriod (client-wise custom period) ──────────────────
        [HttpGet]
        [Route("GetAttendancePeriod")]
        public async Task<ActionResult<AttendancePeriodDto>> GetAttendancePeriod(
            string clientCode, int year, int month)
        {
            try
            {
                var result = await _attendancePeriodService.GetAttendancePeriodAsync(clientCode, year, month);
                return Ok(new AttendancePeriodDto
                {
                    StartDate = result.StartDate,
                    EndDate   = result.EndDate,
                    PeriodKey = result.PeriodKey,
                    IsCustom  = result.IsCustom,
                    TotalDays = result.TotalDays,
                    Label     = $"{result.StartDate:dd-MMM-yyyy} to {result.EndDate:dd-MMM-yyyy}"
                });
            }
            catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
        }

        [HttpGet]

        [Route("GetClients")]

        public async Task<ActionResult<AttendanceDto>> GetClients(DateTime period, string branchCode)

        {

            try

            {

                var clients = _payrollRepository.GetClients(period, branchCode);

                return Ok(clients);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }



        }



        // TEMPORARY: Get clients by branch only (barcode-based, without agreement filter)

        [HttpGet]

        [Route("GetClientsByBranchOnly")]

        public async Task<ActionResult<ClientMaster>> GetClientsByBranchOnly(string branchCode)

        {

            try

            {

                var clients = _oBMSDbContext.ClientMasters

                    .Where(x => x.Branch == branchCode && (x.Status == "Active" || x.Status == "A"))

                    .OrderBy(x => x.Name)

                    .ToList();

                return Ok(clients);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("AttendanceByEmployeeID")]

        public async Task<ActionResult<AttendanceDto>> AttendanceByEmployeeID(DateTime Period, int employeeID)

        {

            try

            {

                var attendance = _payrollRepository.AttendanceByEmployeeID(Period, employeeID);

                return Ok(attendance);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }



        }



        [HttpGet]

        [Route("AttendanceDetailsByID")]

        public async Task<ActionResult<List<AttendanceDetailsDto>>> AttendanceDetailsByID(int Id)

        {

            try

            {

                var attendanceDetails = await _payrollRepository.AttendanceDetailsByID(Id);

                return Ok(attendanceDetails);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }



        }



        [HttpGet]

        [Route("GetAttendanceDetailsList")]

        public async Task<ActionResult<List<AttendanceDetailsDto>>> GetAttendanceDetailsList(int attendanceID)

        {

            try

            {

                var attendanceDetails = await _payrollRepository.GetAttendanceDetailsList(attendanceID);

                return Ok(attendanceDetails);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}"); ;

            }



        }



        [HttpGet]

        [Route("GetEmployeeDetails")]

        public async Task<ActionResult<List<SalaryAttendenceDto>>> GetEmployeeDetails(string branchCode, string employeeNo)

        {

            try

            {

                var employeeList = await _payrollRepository.GetEmployeeDetails(branchCode, employeeNo);

                return Ok(employeeList);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}"); ;

            }



        }



        [HttpGet]

        [Route("IsSalaryProcessDoneForCurrentPeriod")]

        public IActionResult IsSalaryProcessDoneForCurrentPeriod(string branch, string employeeType, DateTime dtPeriod)

        {

            try

            {

                var isSalaryProcess = _payrollRepository.IsSalaryProcessDoneForCurrentPeriod(branch, employeeType, dtPeriod);

                return Ok(isSalaryProcess);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("GetEmployeeAttendanceList")]

        public ActionResult<List<string>> GetEmployeeAttendanceList(DateTime period, string branch)

        {

            try

            {

                var result = _payrollRepository.GetEmployeeAttendanceList(period, branch);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("GetTodayAttendanceList")]

        public async Task<ActionResult<List<object>>> GetTodayAttendanceList(string branch = "ALL")

        {

            try

            {

                var today = DateTime.Today;

                // Debug: Log what we're searching for
                Console.WriteLine($"Searching for attendance for today: {today:yyyy-MM-dd}, branch: {branch}");

                var attendanceData = await _payrollRepository.GetTodayAttendanceList(branch);

                // Debug: Log results
                Console.WriteLine($"Found {attendanceData.Count} attendance records for today");

                return Ok(attendanceData);

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Error in GetTodayAttendanceList: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("GetAttendanceByDate")]

        public async Task<ActionResult<List<object>>> GetAttendanceByDate(DateTime attendanceDate, string branch = "ALL")

        {

            try

            {

                // Debug: Log what we're searching for
                Console.WriteLine($"Searching for attendance for date: {attendanceDate:yyyy-MM-dd}, branch: {branch}");

                var attendanceData = await _payrollRepository.GetAttendanceByDate(attendanceDate, branch);

                // Debug: Log results
                Console.WriteLine($"Found {attendanceData.Count} attendance records for date: {attendanceDate:yyyy-MM-dd}");

                return Ok(attendanceData);

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Error in GetAttendanceByDate: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("IsTemporaryEmployee")]

        public IActionResult IsTemporaryEmployee(string employeeCode)

        {

            try

            {

                var isTemporary = _payrollRepository.IsTemporaryEmployee(employeeCode);

                return Ok(isTemporary);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("GetAnnualLeave")]

        public async Task<Dictionary<string, Object>> GetAnnualLeave(int employeeID, DateTime Period)

        {

            var results = new Dictionary<string, Object>();

            try

            {

                var sqlQuery = @"

                            SELECT CAST(SUM(LeaveTaken) AS INT) as LeaveTaken, 

                                   CAST(SUM(LeaveAvailable) AS INT) as LeaveAvailable

                            FROM (

                            SELECT COUNT(TYPE) as LeaveTaken, 0 as LeaveAvailable FROM AttendanceDetails

                            INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID

                            INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID

                            INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                            WHERE AttendanceDetails.AttendanceDate < @Period AND Year(AttendanceDetails.AttendanceDate) = Year(@Period) AND Type = 8 AND Attendance.EmployeeID = @employeeID

                            UNION

                            SELECT 0 as LeaveTaken, 

                                CASE

                                WHEN DateDiff(day, EMPPAY_DATE_JOINED, @Period) / 365 < 1 THEN CAST(AL0To1 * DateDiff(Month, EMPPAY_DATE_JOINED, @Period) / 12 as int)

                                WHEN DateDiff(day, EMPPAY_DATE_JOINED, @Period) / 365 BETWEEN 1 AND 2 THEN CAST(AL1To2 as int)

                                WHEN DateDiff(day, EMPPAY_DATE_JOINED, @Period) / 365 BETWEEN 3 AND 5 THEN CAST(AL2To5 as int)

                                ELSE CAST(Al6 as int) END as LeaveAvailable

                            FROM leaveSystem, Employee

                            INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                            WHERE Employee.EMP_ID = @employeeID

                            ) ANNUALLEAVE";



                var parameters = new[]

                                {

                                 new Microsoft.Data.SqlClient.SqlParameter("@employeeID", employeeID),

                                 new Microsoft.Data.SqlClient.SqlParameter("@Period", Period),

                                };





                var result = await _oBMSDbContext.LeaveClassResults

                       .FromSqlRaw(sqlQuery, parameters)

                .FirstOrDefaultAsync();

                results.Add("LeaveTaken", result.LeaveTaken);

                results.Add("LeaveAvailable", result.LeaveAvailable);

            }

            catch (Exception)

            {



                throw;

            }



            return results;

        }



        [HttpGet]

        [Route("GetMedicalLeave")]

        public async Task<Dictionary<string, Object>> GetMedicalLeave(int employeeID, DateTime Period)

        {

            try

            {

                var results = new Dictionary<string, Object>();

                var sqlQuery = @"

                    SELECT CAST(SUM(LeaveTaken) AS INT) as LeaveTaken, 

                           CAST(SUM(LeaveAvailable) AS INT) as LeaveAvailable

                    FROM (

                        SELECT COUNT(TYPE) as LeaveTaken, 0 as LeaveAvailable

                        FROM AttendanceDetails

                        INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID

                        INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID

                        INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                        WHERE AttendanceDetails.AttendanceDate < @Period

                            AND Year(AttendanceDetails.AttendanceDate) = Year(@Period)

                            AND Type = 9

                            AND Attendance.EmployeeID = @employeeID

                        UNION

                        SELECT 0 as LeaveTaken,

                            CASE

                                WHEN DateDiff(day, EMPPAY_DATE_JOINED, @Period) / 365 < 2 THEN ML0To2

                                WHEN DateDiff(day, EMPPAY_DATE_JOINED, @Period) / 365 BETWEEN 2 AND 5 THEN ML2To5

                                ELSE Ml6

                            END as LeaveAvailable

                        FROM leaveSystem

                        INNER JOIN Employee ON Employee.EMP_ID = @employeeID

                        INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                    ) ANNUALLEAVE";





                var parameters = new[]

                {

                                 new Microsoft.Data.SqlClient.SqlParameter("@employeeID", employeeID),

                                new Microsoft.Data.SqlClient.SqlParameter("@Period", Period),

                    };





                var result = await _oBMSDbContext.LeaveClassResults

                       .FromSqlRaw(sqlQuery, parameters)

                .FirstOrDefaultAsync();

                results.Add("LeaveTaken", result.LeaveTaken);

                results.Add("LeaveAvailable", result.LeaveAvailable);



                return results;

            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetMaternityLeave")]

        public async Task<Dictionary<string, Object>> GetMaternityLeave(int employeeID, DateTime Period)

        {

            try

            {

                var results = new Dictionary<string, Object>();

                var sqlQuery = @"

                            SELECT CAST(SUM(LeaveTaken) AS INT) as LeaveTaken, 

                                   CAST(SUM(LeaveAvailable) AS INT) as LeaveAvailable

                            FROM (

                            SELECT COUNT(TYPE) as LeaveTaken, 0 as LeaveAvailable

                            FROM AttendanceDetails

                            INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID

                            INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID

                            INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                            WHERE AttendanceDetails.AttendanceDate < @Period AND Year(AttendanceDetails.AttendanceDate) = Year(@Period) AND Type = 10 AND Attendance.EmployeeID = @employeeID

                            UNION

                            SELECT 0 as LeaveTaken, MtnyL as LeaveAvailable

                            FROM leaveSystem, Employee

                            INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                            WHERE Employee.EMP_ID = @employeeID

                            ) ANNUALLEAVE";



                var parameters = new[]

                {

                                 new Microsoft.Data.SqlClient.SqlParameter("@employeeID", employeeID),

                                new Microsoft.Data.SqlClient.SqlParameter("@Period", Period),

                    };





                var result = await _oBMSDbContext.LeaveClassResults

                       .FromSqlRaw(sqlQuery, parameters)

                .FirstOrDefaultAsync();

                results.Add("LeaveTaken", result.LeaveTaken);

                results.Add("LeaveAvailable", result.LeaveAvailable);



                return results;

            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetPaternityLeave")]

        public async Task<Dictionary<string, Object>> GetPaternityLeave(int employeeID, DateTime Period)

        {

            try

            {

                var results = new Dictionary<string, Object>();

                var sqlQuery = @"

                             SELECT CAST(SUM(LeaveTaken) AS INT) as LeaveTaken, 

                                    CAST(SUM(LeaveAvailable) AS INT) as LeaveAvailable

                            FROM (

                            SELECT COUNT(TYPE) as LeaveTaken, 0 as LeaveAvailable FROM AttendanceDetails

                            INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID

                            INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID

                            INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                            WHERE AttendanceDetails.AttendanceDate < @Period AND Year(AttendanceDetails.AttendanceDate) = Year(@Period) AND Type = 11 AND Attendance.EmployeeID = @employeeID

                            UNION

                            SELECT 0 as LeaveTaken, PtnyL as LeaveAvailable

                            FROM leaveSystem, Employee

                            INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                            WHERE Employee.EMP_ID = @employeeID

                            ) ANNUALLEAVE";



                var parameters = new[]

                {

                                 new Microsoft.Data.SqlClient.SqlParameter("@employeeID", employeeID),

                                new Microsoft.Data.SqlClient.SqlParameter("@Period", Period),

                    };





                var result = await _oBMSDbContext.LeaveClassResults

                       .FromSqlRaw(sqlQuery, parameters)

                .FirstOrDefaultAsync();

                results.Add("LeaveTaken", result.LeaveTaken);

                results.Add("LeaveAvailable", result.LeaveAvailable);



                return results;

            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetHospitalizationLeave")]

        public async Task<Dictionary<string, Object>> GetHospitalizationLeave(int employeeID, DateTime Period)

        {

            try

            {

                var results = new Dictionary<string, Object>();

                var sqlQuery = @"

                             SELECT CAST(SUM(LeaveTaken) AS INT) as LeaveTaken, 

                                    CAST(SUM(LeaveAvailable) AS INT) as LeaveAvailable

                             FROM (

                            SELECT COUNT(TYPE) as LeaveTaken, 0 as LeaveAvailable FROM AttendanceDetails

                                INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID

                                INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID

                                INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                                WHERE AttendanceDetails.AttendanceDate < @Period AND Year(AttendanceDetails.AttendanceDate) = Year(@Period) AND Type = 12 AND Attendance.EmployeeID = @employeeID

                            UNION

                            SELECT 0 as LeaveTaken, HL as LeaveAvailable

                                FROM leaveSystem, Employee

                                INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE

                                WHERE Employee.EMP_ID = @employeeID

                            ) ANNUALLEAVE";



                var parameters = new[]

                {

                                 new Microsoft.Data.SqlClient.SqlParameter("@employeeID", employeeID),

                                new Microsoft.Data.SqlClient.SqlParameter("@Period", Period),

                    };





                var result = await _oBMSDbContext.LeaveClassResults

                       .FromSqlRaw(sqlQuery, parameters)

                .FirstOrDefaultAsync();

                results.Add("LeaveTaken", result.LeaveTaken);

                results.Add("LeaveAvailable", result.LeaveAvailable);



                return results;

            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("CalculateAge")]

        public int CalculateAge(DateTime birthDate)

        {

            int age = _payrollRepository.CalculateAge(birthDate);



            return age;

        }



        [HttpPost]

        [Route("CalculateProfessionalTax")]

        public ActionResult<ProfessionalTaxResult> CalculateProfessionalTax([FromBody] ProfessionalTaxRequestDto request)

        {

            try

            {

                var result = _professionalTaxService.CalculateProfessionalTax(

                    request.GrossSalary, 

                    request.State, 

                    request.CalculationDate

                );

                

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "Error calculating professional tax", error = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetProfessionalTaxForEmployee")]

        public ActionResult<decimal> GetProfessionalTaxForEmployee(int employeeId, DateTime calculationDate)

        {

            try

            {

                // Get employee details including state

                var employee = _oBMSDbContext.Employees

                    .Where(e => e.EMP_ID == employeeId)

                    .Select(e => new { 

                        State = e.ProfessionalTaxState ?? e.EMP_STATE ?? "Maharashtra", 

                        EMP_ID = e.EMP_ID,

                        EMP_CODE = e.EMP_CODE

                    })

                    .FirstOrDefault();



                if (employee == null)

                {

                    return NotFound(new { message = "Employee not found" });

                }



                // Get gross salary from salary details - this would need to be implemented based on your salary structure

                // For now, using a basic approach - you may need to adjust this based on your actual salary calculation logic

                var grossSalary = GetEmployeeGrossSalary(employee.EMP_ID, calculationDate);



                var ptAmount = _professionalTaxService.GetProfessionalTaxAmount(

                    grossSalary, 

                    employee.State, 

                    calculationDate

                );



                return Ok(ptAmount);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "Error getting professional tax for employee", error = ex.Message });

            }

        }



        private decimal GetEmployeeGrossSalary(int employeeId, DateTime calculationDate)

        {

            // This is a placeholder implementation

            // You would need to implement the actual logic to calculate/get gross salary

            // based on your payroll system's salary structure

            

            // For now, return a default value - you should replace this with actual salary calculation

            return 15000; // Default monthly salary

            

            // Example implementation might look like:

            // var salaryDetails = _oBMSDbContext.EmployeeSalaryDetails

            //     .Where(s => s.EMP_ID == employeeId && s.Month == calculationDate.Month && s.Year == calculationDate.Year)

            //     .FirstOrDefault();

            // return salaryDetails?.GrossSalary ?? 0;

        }



        [HttpPost("SaveAndUpdateAttendance")]

        public async Task<ActionResult> SaveAndUpdateAttendance(AttendanceModel attendance)

        {

            try

            {

                // API-only processing - doesn't affect UI storage
                if (attendance.attendanceDetails != null)
                {
                    foreach (var detail in attendance.attendanceDetails)
                    {
                        // Convert work type names to IDs (like UI getWorkTypeByName)
                        if (detail.Type != null && detail.Type.GetType() == typeof(string))
                        {
                            detail.Type = GetWorkTypeIdByName(detail.Type.ToString());
                        }
                    }
                }

                // Resolve period key via AttendancePeriodService (custom or calendar month fallback)
                var refDate    = attendance.attendanceModel!.Period;
                var periodInfo = await _attendancePeriodService.GetAttendancePeriodAsync(
                    attendance.ClientCode ?? string.Empty,
                    refDate.Year,
                    refDate.Month);
                var period = periodInfo.PeriodKey;

                var attendanceModel = new Attendance()

                {

                    ID = attendance.attendanceModel.ID,

                    Period = period, // Use calculated period

                    EmployeeID = attendance.attendanceModel.EmployeeID,

                    Branch = attendance.attendanceModel.Branch,

                    Shift2Type = attendance.attendanceModel.Shift2Type,

                    Shift2Rate = attendance.attendanceModel.Shift2Rate,

                    AllowanceDeduction = attendance.attendanceModel.AllowanceDeduction,

                    SpecialAllowanceDeduction = attendance.attendanceModel.SpecialAllowanceDeduction,

                    Bonus = attendance.attendanceModel.Bonus,

                    LastUpdate = DateTime.Now,

                    LastUpdatedBy = attendance.attendanceModel.LastUpdatedBy,

                };

                await _payrollRepository.SaveAndUpdateAttendance(attendanceModel, attendance.attendanceDetails);

                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                dictResult.Add("Success", "Success");

                dictResult.Add("Message", "Attendance saved / updated successfully");

                return Ok(dictResult);

            }

            catch (Exception ex)

            {

                return BadRequest($"Error: {ex.Message}");

            }

        }


        [HttpPost("GenerateSimplifiedAttendanceTemplate")]

        public async Task<IActionResult> GenerateSimplifiedAttendanceTemplate(DateTime period, string branchCode)
        {
            try
            {
                var templateBytes = _attendanceExcelService.GenerateSimplifiedAttendanceTemplate(period, branchCode);
                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Attendance_Template_{period:MMM-yy}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating template: {ex.Message}");
            }
        }

        [HttpPost("UploadSimplifiedAttendance")]
        public async Task<IActionResult> UploadSimplifiedAttendance([FromForm] IFormFile file, [FromForm] string currentUser)
        {
            var startTime = DateTime.UtcNow;
            var result = new AttendanceBulkUploadResultDto
            {
                TotalRecords = 0,
                Success = false,
                Message = "Upload not started"
            };

            try
            {
                // Input validation
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new AttendanceBulkUploadResultDto
                    {
                        Success = false,
                        Message = "No file was uploaded. Please select a valid Excel file.",
                        Errors = new List<AttendanceBulkUploadErrorDto>
                        {
                            new()
                            {
                                ErrorMessage = "File upload failed - Empty or null file provided",
                                ErrorType = "FileValidation"
                            }
                        }
                    });
                }

                // Validate file type
                var allowedExtensions = new[] { ".xlsx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new AttendanceBulkUploadResultDto
                    {
                        Success = false,
                        Message = $"Invalid file format. Only Excel files (.xlsx) are accepted. Received: {fileExtension}",
                        Errors = new List<AttendanceBulkUploadErrorDto>
                        {
                            new()
                            {
                                ErrorMessage = $"File extension {fileExtension} is not supported",
                                ErrorType = "FileFormat"
                            }
                        }
                    });
                }

                // Validate file size (max 5MB)
                const long maxFileSize = 5 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new AttendanceBulkUploadResultDto
                    {
                        Success = false,
                        Message = $"File size exceeds maximum allowed size of 5MB. Your file: {file.Length / (1024 * 1024)}MB",
                        Errors = new List<AttendanceBulkUploadErrorDto>
                        {
                            new()
                            {
                                ErrorMessage = $"File size {file.Length} bytes exceeds maximum {maxFileSize} bytes",
                                ErrorType = "FileSize"
                            }
                        }
                    });
                }

                using var stream = file.OpenReadStream();
                
                // Parse Excel file
                List<AttendanceBulkUploadDto> simplifiedData;
                try
                {
                    simplifiedData = _attendanceExcelService.ParseSimplifiedAttendanceExcel(stream);
                    result.TotalRecords = simplifiedData.Count;
                }
                catch (InvalidOperationException parseEx)
                {
                    return BadRequest(new AttendanceBulkUploadResultDto
                    {
                        Success = false,
                        Message = $"Error parsing Excel file: {parseEx.Message}",
                        ValidationErrors = new Dictionary<string, List<string>>
                        {
                            { "ParseError", new List<string> { parseEx.Message } }
                        }
                    });
                }
                catch (Exception parseEx)
                {
                    return BadRequest(new AttendanceBulkUploadResultDto
                    {
                        Success = false,
                        Message = "Unexpected error while parsing Excel file. Please ensure it's a valid Excel file.",
                        Errors = new List<AttendanceBulkUploadErrorDto>
                        {
                            new()
                            {
                                ErrorMessage = parseEx.Message,
                                ErrorType = "ParsingException",
                                StackTrace = parseEx.StackTrace
                            }
                        }
                    });
                }

                // Convert simplified format to detailed format
                var detailedData = new List<AttendanceBulkUploadDto>();
                var conversionErrors = new List<AttendanceBulkUploadErrorDto>();

                foreach (var simplifiedDto in simplifiedData)
                {
                    try
                    {
                        var detailedDto = _attendanceExcelService.ConvertSimplifiedToDetailed(simplifiedDto);
                        detailedData.Add(detailedDto);
                    }
                    catch (InvalidOperationException convEx)
                    {
                        conversionErrors.Add(new AttendanceBulkUploadErrorDto
                        {
                            EmployeeCode = simplifiedDto.EmployeeCode,
                            EmployeeName = simplifiedDto.EmployeeName,
                            ErrorMessage = $"Data conversion failed: {convEx.Message}",
                            ErrorType = "ConversionError"
                        });
                    }
                }

                if (conversionErrors.Any())
                {
                    return Ok(new AttendanceBulkUploadResultDto
                    {
                        Success = false,
                        TotalRecords = simplifiedData.Count,
                        FailedRecords = conversionErrors.Count,
                        Message = $"Failed to convert {conversionErrors.Count} records. See error details below.",
                        Errors = conversionErrors
                    });
                }

                // Save to database
                var dbResult = await _payrollRepository.SaveSimplifiedBulkAttendance(detailedData, currentUser);
                
                // Add processing time
                var processingTime = DateTime.UtcNow - startTime;
                dbResult.Statistics.ProcessingTime = processingTime;

                // Enhance response
                if (!dbResult.Success)
                {
                    return Ok(dbResult);
                }

                return Ok(dbResult);
            }
            catch (Exception ex)
            {
                // Log the exception
                var errorDetail = new AttendanceBulkUploadResultDto
                {
                    Success = false,
                    TotalRecords = result.TotalRecords,
                    Message = "An unexpected error occurred during file upload. Please contact system administrator.",
                    Errors = new List<AttendanceBulkUploadErrorDto>
                    {
                        new()
                        {
                            ErrorMessage = ex.Message,
                            ErrorType = "SystemError",
                            StackTrace = ex.StackTrace,
                            Timestamp = DateTime.UtcNow
                        }
                    }
                };

                return StatusCode(500, errorDetail);
            }
        }



        [HttpDelete("DeleteAttendance")]

        public async Task<IActionResult> DeleteAttendance(int id)

        {

            Dictionary<string, object> dictResult = new Dictionary<string, object>();

            try

            {

                var result = await _payrollRepository.DeleteAttendanceAsync(id);



                if (result)

                {

                   

                    dictResult.Add("Success", "Success");

                    dictResult.Add("Message", "Attendance deleted successfully");

                    return Ok(dictResult);

                }



                dictResult.Add("NotFound", "NotFound");

                dictResult.Add("Message", "Record Not Found");

                return Ok(dictResult);

            }

            catch (Exception ex)

            {

                // Log the exception here                

                dictResult.Add("Error", "Error");

                dictResult.Add("Message", "An error occurred: " + ex.Message);

                return Ok(dictResult);

            }

        }



        [HttpGet]

        [Route("GetList")]

        public async Task<ActionResult<SalaryAdvanceRequestDto>> GetList(string branch, string employeeType, DateTime resignedDate, DateTime joinDate, DateTime attendancePeriod, string status)

        {

            try

            {

                var employeeList = _payrollRepository.GetList(branch, employeeType, resignedDate, joinDate, attendancePeriod, status);

                return Ok(employeeList);

            }

            catch

            {

                throw;

            }



        }



        [HttpGet]

        [Route("getListByEmployee")]

        public async Task<ActionResult<SalaryAdvanceRequestDto>> getListByEmployee(string branch, string employeeType, DateTime resignedDate, DateTime joinDate, string status)

        {

            try

            {

                var employeeList = _payrollRepository.getListByEmployee(branch, employeeType, resignedDate, joinDate, status);

                return Ok(employeeList);

            }

            catch

            {

                throw;

            }



        }





        //[HttpGet]

        //[Route("GetAnnualLeave")]

        //public int GetAnnualLeave(int employeeID, DateTime period)

        //{

        //    var leaveQuery = from attendanceDetail in _oBMSDbContext.AttendanceDetails

        //                     join attendance in _oBMSDbContext.Attendances on attendanceDetail.AttendanceID equals attendance.ID

        //                     join employee in _oBMSDbContext.Employees on attendance.EmployeeID equals employee.EMP_ID

        //                     join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                     where attendanceDetail.AttendanceDate < period && attendanceDetail.Type == 8 && employee.EMP_ID == employeeID

        //                     select new

        //                     {

        //                         LeaveTaken = 0, // Assuming 1 for LeaveTaken

        //                         LeaveAvailable = 0

        //                     };



        //    var leaveAvailableQuery = from employee in _oBMSDbContext.Employees

        //                              from leaveSystem in _oBMSDbContext.LeaveSystems

        //                              join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                              where employee.EMP_ID == employeeID

        //                              select new

        //                              {

        //                                  LeaveTaken = 0,

        //                                  //LeaveAvailable = leaveSystem.AL6 * ((period.Year - employmentDetail.EMPPAY_DATE_JOINED.Year == 0) ? DateDiffInMonths(employmentDetail.EMPPAY_DATE_JOINED, period) / 12 : 0)

        //                                  LeaveAvailable = leaveSystem.AL6

        //                                  //LeaveAvailable = leaveSystem.AL6 * (decimal)((period - employmentDetail.EMPPAY_DATE_JOINED).TotalDays / 365) / 12

        //                              };



        //    return (int)leaveAvailableQuery.AsEnumerable().Sum(x => x.LeaveAvailable);



        //}



        //[HttpGet]

        //[Route("GetMedicalLeave")]

        //public int GetMedicalLeave(int employeeID, DateTime period)

        //{

        //    var leaveQuery = from attendanceDetail in _oBMSDbContext.AttendanceDetails

        //                     join attendance in _oBMSDbContext.Attendances on attendanceDetail.AttendanceID equals attendance.ID

        //                     join employee in _oBMSDbContext.Employees on attendance.EmployeeID equals employee.EMP_ID

        //                     join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                     where attendanceDetail.AttendanceDate < period && attendanceDetail.Type == 9 && employee.EMP_ID == employeeID

        //                     select new

        //                     {

        //                         LeaveTaken = 0, // Assuming 1 for LeaveTaken

        //                         LeaveAvailable = 0

        //                     };



        //    var leaveAvailableQuery = from employee in _oBMSDbContext.Employees

        //                              from leaveSystem in _oBMSDbContext.LeaveSystems

        //                              join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                              where employee.EMP_ID == employeeID

        //                              select new

        //                              {

        //                                  LeaveTaken = 0,

        //                                  //LeaveAvailable = leaveSystem.AL6 * ((period.Year - employmentDetail.EMPPAY_DATE_JOINED.Year == 0) ? DateDiffInMonths(employmentDetail.EMPPAY_DATE_JOINED, period) / 12 : 0)

        //                                  LeaveAvailable = leaveSystem.ML6

        //                                  //LeaveAvailable = leaveSystem.AL6 * (decimal)((period - employmentDetail.EMPPAY_DATE_JOINED).TotalDays / 365) / 12

        //                              };



        //    return (int)leaveAvailableQuery.AsEnumerable().Sum(x => x.LeaveAvailable);



        //}



        //[HttpGet]

        //[Route("GetPaternityLeave")]

        //public int GetPaternityLeave(int employeeID, DateTime period)

        //{

        //    var leaveQuery = from attendanceDetail in _oBMSDbContext.AttendanceDetails

        //                     join attendance in _oBMSDbContext.Attendances on attendanceDetail.AttendanceID equals attendance.ID

        //                     join employee in _oBMSDbContext.Employees on attendance.EmployeeID equals employee.EMP_ID

        //                     join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                     where attendanceDetail.AttendanceDate < period && attendanceDetail.Type == 9 && employee.EMP_ID == employeeID

        //                     select new

        //                     {

        //                         LeaveTaken = 0, // Assuming 1 for LeaveTaken

        //                         LeaveAvailable = 0

        //                     };



        //    var leaveAvailableQuery = from employee in _oBMSDbContext.Employees

        //                              from leaveSystem in _oBMSDbContext.LeaveSystems

        //                              join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                              where employee.EMP_ID == employeeID

        //                              select new

        //                              {

        //                                  LeaveTaken = 0,

        //                                  //LeaveAvailable = leaveSystem.AL6 * ((period.Year - employmentDetail.EMPPAY_DATE_JOINED.Year == 0) ? DateDiffInMonths(employmentDetail.EMPPAY_DATE_JOINED, period) / 12 : 0)

        //                                  LeaveAvailable = leaveSystem.PtnyL

        //                                  //LeaveAvailable = leaveSystem.AL6 * (decimal)((period - employmentDetail.EMPPAY_DATE_JOINED).TotalDays / 365) / 12

        //                              };



        //    return (int)leaveAvailableQuery.AsEnumerable().Sum(x => x.LeaveAvailable);



        //}



        //[HttpGet]

        //[Route("GetMaternityLeave")]

        //public int GetMaternityLeave(int employeeID, DateTime period)

        //{

        //    var leaveQuery = from attendanceDetail in _oBMSDbContext.AttendanceDetails

        //                     join attendance in _oBMSDbContext.Attendances on attendanceDetail.AttendanceID equals attendance.ID

        //                     join employee in _oBMSDbContext.Employees on attendance.EmployeeID equals employee.EMP_ID

        //                     join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                     where attendanceDetail.AttendanceDate < period && attendanceDetail.Type == 10 && employee.EMP_ID == employeeID

        //                     select new

        //                     {

        //                         LeaveTaken = 0, // Assuming 1 for LeaveTaken

        //                         LeaveAvailable = 0

        //                     };



        //    var leaveAvailableQuery = from employee in _oBMSDbContext.Employees

        //                              from leaveSystem in _oBMSDbContext.LeaveSystems

        //                              join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                              where employee.EMP_ID == employeeID

        //                              select new

        //                              {

        //                                  LeaveTaken = 0,

        //                                  //LeaveAvailable = leaveSystem.AL6 * ((period.Year - employmentDetail.EMPPAY_DATE_JOINED.Year == 0) ? DateDiffInMonths(employmentDetail.EMPPAY_DATE_JOINED, period) / 12 : 0)

        //                                  LeaveAvailable = leaveSystem.MtnyL

        //                                  //LeaveAvailable = leaveSystem.AL6 * (decimal)((period - employmentDetail.EMPPAY_DATE_JOINED).TotalDays / 365) / 12

        //                              };



        //    return (int)leaveAvailableQuery.AsEnumerable().Sum(x => x.LeaveAvailable);



        //}



        //[HttpGet]

        //[Route("GetHospitalizationLeave")]

        //public int GetHospitalizationLeave(int employeeID, DateTime period)

        //{

        //    var leaveQuery = from attendanceDetail in _oBMSDbContext.AttendanceDetails

        //                     join attendance in _oBMSDbContext.Attendances on attendanceDetail.AttendanceID equals attendance.ID

        //                     join employee in _oBMSDbContext.Employees on attendance.EmployeeID equals employee.EMP_ID

        //                     join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                     where attendanceDetail.AttendanceDate < period && attendanceDetail.Type == 9 && employee.EMP_ID == employeeID

        //                     select new

        //                     {

        //                         LeaveTaken = 0, // Assuming 1 for LeaveTaken

        //                         LeaveAvailable = 0

        //                     };



        //    var leaveAvailableQuery = from employee in _oBMSDbContext.Employees

        //                              from leaveSystem in _oBMSDbContext.LeaveSystems

        //                              join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

        //                              where employee.EMP_ID == employeeID

        //                              select new

        //                              {

        //                                  LeaveTaken = 0,

        //                                  //LeaveAvailable = leaveSystem.AL6 * ((period.Year - employmentDetail.EMPPAY_DATE_JOINED.Year == 0) ? DateDiffInMonths(employmentDetail.EMPPAY_DATE_JOINED, period) / 12 : 0)

        //                                  LeaveAvailable = leaveSystem.HL

        //                                  //LeaveAvailable = leaveSystem.AL6 * (decimal)((period - employmentDetail.EMPPAY_DATE_JOINED).TotalDays / 365) / 12

        //                              };



        //    return (int)leaveAvailableQuery.AsEnumerable().Sum(x => x.LeaveAvailable);



        //}



        //[HttpGet]

        //[Route("GetAnnualLeaveAPI1")]

        //public int GetAnnualLeaveAPI1(decimal employeeID, DateTime period)

        //{

        //    try

        //    {

        //        var leaveTakenQuery = from ad in _oBMSDbContext.AttendanceDetails

        //                              join a in _oBMSDbContext.Attendances on ad.AttendanceID equals a.ID

        //                              where ad.AttendanceDate < period && ad.Type == 8 && a.EmployeeID == employeeID

        //                              select ad;



        //        int leaveTaken = leaveTakenQuery.Count();



        //        var employmentDetails = _oBMSDbContext.EmploymentDetails.FirstOrDefault(ed => ed.EMPPAY_ID == employeeID);

        //        var leaveSystem = _oBMSDbContext.LeaveSystems.SingleOrDefault();

        //        if (employmentDetails == null || leaveSystem == null)

        //            return 0;



        //        int yearsWorked = (int)((period - employmentDetails.EMPPAY_DATE_JOINED).TotalDays / 365);



        //        int leaveAvailable = yearsWorked switch

        //        {

        //            int n when n < 1 => (int)(leaveSystem.al0to1 * (decimal)((period - employmentDetails.EMPPAY_DATE_JOINED).TotalDays / 365) / 12),

        //            int n when n >= 1 && n < 2 => (int)leaveSystem.AL1to2,

        //            int n when n >= 3 && n <= 5 => (int)leaveSystem.AL2to5,

        //            _ => (int)leaveSystem.AL6

        //        };



        //        return leaveAvailable - leaveTaken;

        //    }

        //    catch

        //    {

        //        throw;

        //    }

        //}







        #endregion



        #region Salary Processing



        [HttpGet]

        [Route("LastprocessedDate")]

        public async Task<Dictionary<string, Object>> LastprocessedDate(string branchCode, string employeeType)

        {

            try

            {

                var results = new Dictionary<string, Object>();

                var sqlQuery = @"

                            SELECT COALESCE(MAX(Period), GETDATE()) AS ProcessedDate

                            FROM SalaryProcess

                            WHERE Branch = @branchCode

                            AND EmployeeType = @employeeType";



                var parameters = new[]

                {

                     new Microsoft.Data.SqlClient.SqlParameter("@branchCode", branchCode),

                     new Microsoft.Data.SqlClient.SqlParameter("@employeeType", employeeType),

                };





                var result = await _oBMSDbContext.LastprocessedDates

                .FromSqlRaw(sqlQuery, parameters)

                .FirstOrDefaultAsync();



                if (result != null)

                {

                    if (result.ProcessedDate != null)

                    {

                        results.Add("LastProcessedDate", (DateTime)result.ProcessedDate);

                    }

                    else

                    {

                        results.Add("LastProcessedDate", DateTime.Now);

                    }

                }

                else

                {

                    results.Add("LastProcessedDate", DateTime.Now);

                }



                return results;

            }

            catch (Exception ex)

            {



                throw;

            }



        }



        [HttpGet]

        [Route("LastSalaryProcessRemarks")]

        public IActionResult LastSalaryProcessRemarks(DateTime period, string branchCode, string employeeType)

        {

            try

            {

                var lastProcessedRemarks = _payrollRepository.LastSalaryProcessRemarks(period, branchCode, employeeType);



                return Ok(new { remarks = lastProcessedRemarks });

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal Server Error: {ex.Message}");

            }

        }



        [HttpGet("Process")]

        public async Task<IActionResult> Process(string branch, string employeeType, string remarks, DateTime period, bool lockProcess, string currentUser, string companyCode)

        {

            var results = new Dictionary<string, object>();



            try

            {

                //var resultMessage = _payrollRepository.Process(branch, employeeType, remarks, period, lockProcess, currentUser, companyCode);

                var resultMessage = _salaryProcess.Process(branch, employeeType, remarks, period, lockProcess, currentUser, companyCode);

                var result = new ProcessResult

                {

                    Message = resultMessage

                };

                results.Add("result", result);



                // Return the result wrapped in a Task as HTTP 200 OK

                return Ok(await Task.FromResult(results));

            }

            catch (Exception ex)

            {

                // Handle exceptions and return an HTTP 500 error with exception details

                return StatusCode(500, new { message = "An error occurred while processing payroll", error = ex.Message });

            }

        }

        #endregion



        #region Payroll report sections

        [HttpGet]

        [Route("WithBlankRow")]

        public IActionResult GetListWithBlankRow(string dtSalaryPeriod)

        {

            try

            {

                var result = BankStatementExcel.GetListWithBlankRow(dtSalaryPeriod);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("WithBlankRowByBranch")]

        public IActionResult GetListWithBlankRowByBranch(string dtSalaryPeriod, string branch)

        {

            try

            {

                var result = BankStatementExcel.GetListWithBlankRow(dtSalaryPeriod, branch);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("WithBlankRowByBranchAndEmployeeType")]

        public IActionResult GetListWithBlankRowByBranchAndEmployeeType(string dtSalaryPeriod, string branch, string employeeType)

        {

            try

            {

                var result = BankStatementExcel.GetListWithBlankRow(dtSalaryPeriod, branch, employeeType);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("WithBlankRowByBranchEmployeeTypeAndBank")]

        public IActionResult GetListWithBlankRowByBranchEmployeeTypeAndBank(string dtSalaryPeriod, string branch, string employeeType, string bank)

        {

            try

            {

                var result = BankStatementExcel.GetListWithBlankRow(dtSalaryPeriod, branch, employeeType, bank);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("GetEPFToExcel")]

        public IActionResult GetEPFToExcel(string branch, DateTime dtSalaryPeriod,  string employeeType)

        {

            try

            {

                var result = BankStatementExcel.GetEPFToExcel( branch,dtSalaryPeriod, employeeType);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("GetRbiBankSalaryExport")]

        public IActionResult GetRbiBankSalaryExport(string dtSalaryPeriod, string branch = "ALL", string employeeType = "ALL")

        {

            try

            {

                var result = _payrollRepository.GetRbiBankSalaryExportData(dtSalaryPeriod, branch, employeeType);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("GetRbiBankSalaryExportCsv")]

        public IActionResult GetRbiBankSalaryExportCsv(string dtSalaryPeriod, string branch = "ALL", string employeeType = "ALL")

        {

            try

            {

                var exportData = _payrollRepository.GetRbiBankSalaryExportData(dtSalaryPeriod, branch, employeeType);

                var csvBytes = RbiBankSalaryExport.GenerateCsvExportBytes(exportData);

                var fileName = $"RBI_Bank_Salary_Export_{dtSalaryPeriod}_{branch}_{employeeType}.csv";

                return File(csvBytes, "text/csv", fileName);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("GetRbiBankAdvanceExport")]

        public IActionResult GetRbiBankAdvanceExport(string dtSalaryPeriod, string branch = "ALL", string employeeType = "ALL")

        {

            try

            {

                var result = _payrollRepository.GetRbiBankAdvanceExportData(dtSalaryPeriod, branch, employeeType);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("GetRbiBankAdvanceExportCsv")]

        public IActionResult GetRbiBankAdvanceExportCsv(string dtSalaryPeriod, string branch = "ALL", string employeeType = "ALL")

        {

            try

            {

                var exportData = _payrollRepository.GetRbiBankAdvanceExportData(dtSalaryPeriod, branch, employeeType);

                var csvBytes = RbiBankAdvanceExport.GenerateCsvExportBytes(exportData);

                var fileName = $"RBI_Bank_Advance_Export_{dtSalaryPeriod}_{branch}_{employeeType}.csv";

                return File(csvBytes, "text/csv", fileName);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("GetEmployeeSocsoList")]

        public IActionResult GetEmployeeSocsoList(DateTime dtSalaryPeriod, string branch)

        {

            try

            {

                var result = BankStatementExcel.GetEmployeeSocsoList(dtSalaryPeriod, branch);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        [HttpGet]

        [Route("GetEmployeeSIPList")]

        public IActionResult GetEmployeeSIPList(string CompanyCode, string SSM, DateTime dtSalaryPeriod, string branch)

        {

            try

            {

                var result = BankStatementExcel.GetEmployeeSIPList(CompanyCode, SSM,dtSalaryPeriod, branch);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("GetConfig")]

        public IActionResult GetConfig(string KeyValue, string Branch)

        {

            try

            {

                var result = UtilityMain.GetConfig(KeyValue, Branch);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetEPFToCIMBList")]

        public IActionResult GetEPFToCIMBList(string Branch,DateTime Period,string EmployeeType,string CompanyEPF,string CompanyPIC,string CompanyPICContact)

        {

            try

            {

                var result = UtilityMain.GetEPFToCIMBList(Branch, Period, EmployeeType, CompanyEPF, CompanyPIC, CompanyPICContact);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        // Endpoint for GetSocsoToCIMBList

        [HttpGet]

        [Route("GetSocsoToCIMBList")]

        public IActionResult GetSocsoToCIMBList(string CompanyRegNumber,string SocsoCompanyCode,string Branch,DateTime Period,string EmployeeType, string EmpTempType)

        {

            try

            {

                var result = UtilityMain.GetSocsoToCIMBList(CompanyRegNumber, SocsoCompanyCode, Branch, Period, EmployeeType, EmpTempType);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        // Endpoint for GetSIPToCIMBList

        [HttpGet]

        [Route("GetSIPToCIMBList")]

        public IActionResult GetSIPToCIMBList(string CompanyRegNumber,string SIPCompanyCode,string Branch, DateTime Period, string EmployeeType)

        {

            try

            {

                var result = UtilityMain.GetSIPToCIMBList(CompanyRegNumber, SIPCompanyCode, Branch, Period, EmployeeType);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetEmployeeSalarynAdvanceList")]

        public IActionResult GetEmployeeSalarynAdvanceList(DateTime period,string employeeType,string bankCode,string type,string company,string source)

        {

            try

            {

                List<string> result = UtilityMain.GetEmployeeSalarynAdvanceList(period, employeeType, bankCode, type, company, source);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message }); 

            }

            

        }



        [HttpGet]

        [Route("GetEmployeeSalarynAdvanceListWithBranch")]

        public IActionResult GetEmployeeSalarynAdvanceListWithBranch(string branch,DateTime period,string employeeType,string bankCode,string type,string company,string source)

        {

            try

            {

                List<string> result = UtilityMain.GetEmployeeSalarynAdvanceList(branch, period, employeeType, bankCode, type, company, source);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

           

        }



        [HttpGet]

        [Route("GetEmployeeSalarynAdvanceTotalList")]

        public IActionResult GetEmployeeSalarynAdvanceTotalList(DateTime period,string employeeType,string bankCode,string type,string company,string source)

        {

            try

            {

                List<string> result = UtilityMain.GetEmployeeSalarynAdvanceTotalList(period, employeeType, bankCode, type, company, source);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetEmployeeSalarynAdvanceTotalListWithBranch")]

        public IActionResult GetEmployeeSalarynAdvanceTotalListWithBranch(string branch,DateTime period,string employeeType,string bankCode,string type,string company,string source)

        {

            try

            {

                List<string> result = UtilityMain.GetEmployeeSalarynAdvanceTotalList(branch, period, employeeType, bankCode, type, company, source);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetEmployeeSalarynAdvanceHashTotalList")]

        public IActionResult GetEmployeeSalarynAdvanceHashTotalList(DateTime period,string employeeType,string bankCode,string type,string company,string source)

        {

            try

            {

                List<string> result = UtilityMain.GetEmployeeSalarynAdvanceHashTotalList(period, employeeType, bankCode, type, company, source);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetEmployeeSalarynAdvanceHashTotalListWithBranch")]

        public IActionResult GetEmployeeSalarynAdvanceHashTotalListWithBranch(string branch,DateTime period,string employeeType,string bankCode,string type,string company,string source)

        {

            try

            {

                List<string> result = UtilityMain.GetEmployeeSalarynAdvanceHashTotalList(branch, period, employeeType, bankCode, type, company, source);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }



        }



        [HttpGet("ClientInvoiceCalculation")]

        public IActionResult ClientInvoiceCalculation(string branch, string client, DateTime agreementPeriod)

        {

            try

            {

                // Create an instance of ClientInvoiceCalculation

                var invoiceCalculation = new ClientInvoiceCalculation(branch, client, agreementPeriod);



                // Prepare the response

                var result = new

                {

                    ServiceCharges = invoiceCalculation.ServiceCharges,

                    Discount = invoiceCalculation.Discount,

                    TaxAmount = invoiceCalculation.TaxAmount,

                    NoOfDays = invoiceCalculation.NoOfDays,

                    NoOfHours = invoiceCalculation.NoOfHours,

                    Total = invoiceCalculation.Total

                };



                // Return the result

                return Ok(result);

            }

            catch (Exception ex)

            {

                // Handle any exceptions

                return StatusCode(500, new { Message = "An error occurred while calculating the invoice.", Error = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetMonthlyInvoiceStatusList")]

        public IActionResult GetMonthlyInvoiceStatusList(string Start, string End, string Branch)

        {

            try

            {

                var result = BankStatementExcel.GetMonthlyInvoiceStatusList(Start, End, Branch);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("CheckExistAdvance")]

        public IActionResult CheckExistAdvance(int EmployeeID, DateTime AdvanceDate, int LoanType)

        {

            try

            {

                var result = UtilityMain.CheckExistAdvance(EmployeeID, AdvanceDate, LoanType);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("NewAdvanceVoucherNo")]

        public IActionResult NewAdvanceVoucherNo(string Branch, int TransType)

        {

            try

            {

                var result = UtilityMain.NewAdvanceVoucherNo(Branch, TransType);

                return Ok(new { VoucherNo = result }); // Return as JSON object

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet("GetSalaryAdvances")]

        public async Task<IActionResult> GetSalaryAdvances(DateTime advanceDate, int employeeId, int transType)

        {

            try

            {

                var result = await _payrollRepository.GetSalaryAdvancesAsync(advanceDate, employeeId, transType);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpDelete("Delete/{id}")]

        public async Task<IActionResult> Delete(int id, [FromQuery] string currentUser)

        {

            try

            {

                var result = await _payrollRepository.DeleteSalaryAdvanceAsync(id, currentUser);

                if (result)

                    return Ok(new { success = true, message = "Salary advance deleted successfully." });



                return NotFound(new { success = false, message = "Salary advance not found." });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { success = false, message = "An error occurred.", error = ex.Message });

            }

        }



        [HttpGet("latest-period")]

        public async Task<IActionResult> GetLatestPeriod(int employeeId, int year, int month)

        {

            try

            {

                var period = await _payrollRepository.GetLatestAttendancePeriodAsync(employeeId, year, month);

                return Ok(period);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("GetEmployeeLoanList")]

        public IActionResult GetEmployeeLoanList(DateTime Period, string Branch, string EmployeeType, int TransType)

        {

            try

            {

                var result = UtilityMain.GetEmployeeLoanList(Period, Branch, EmployeeType, TransType);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        #endregion



        #region Dynamic Payslip Data API Endpoints



        [HttpGet]

        [Route("GetPayslipData")]

        public async Task<IActionResult> GetPayslipData(string LoginID, string Branch, string Period, string EmployeeType, string Employee, string Lang)

        {

            try

            {

                var data = await _payrollRepository.GetPayslipData(LoginID, Branch, Period, EmployeeType, Employee, Lang);

                return Ok(data);

            }

            catch (Exception ex) {

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetPayslip2Data")]

        public async Task<IActionResult> GetPayslip2Data(string LoginID, string Branch, string Period, string EmployeeType, string Employee, string Lang)

        {

            try

            {

                var data = await _payrollRepository.GetPayslip2Data(LoginID, Branch, Period, EmployeeType, Employee, Lang);

                return Ok(data);

            }

            catch (Exception ex) {

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetNewPayslipData")]

        public async Task<IActionResult> GetNewPayslipData(string LoginID, string Branch, string Period, string EmployeeType, string Employee, string Lang)

        {

            try

            {

                var data = await _payrollRepository.GetNewPayslipData(LoginID, Branch, Period, EmployeeType, Employee, Lang);

                return Ok(data);

            }

            catch (Exception ex) {

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetTimeSheetData")]

        public async Task<IActionResult> GetTimeSheetData(string LoginID, string Branch, string Period, string EmployeeType, string Employee, string Lang)

        {

            try

            {

                var data = await _payrollRepository.GetTimeSheetData(LoginID, Branch, Period, EmployeeType, Employee, Lang);

                return Ok(data);

            }

            catch (Exception ex) {

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetGuard1PayslipData")]

        public async Task<IActionResult> GetGuard1PayslipData(string LoginID, string Branch, string Period, string EmployeeType, string Employee, string Lang)

        {

            try

            {

                var data = await _payrollRepository.GetGuard1PayslipData(LoginID, Branch, Period, EmployeeType, Employee, Lang);

                return Ok(data);

            }

            catch (Exception ex) {

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetGuard2PayslipData")]

        public async Task<IActionResult> GetGuard2PayslipData(string LoginID, string Branch, string Period, string EmployeeType, string Employee, string Lang)

        {

            try

            {

                var data = await _payrollRepository.GetGuard2PayslipData(LoginID, Branch, Period, EmployeeType, Employee, Lang);

                return Ok(data);

            }

            catch (Exception ex) {

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetRBAPayslipData")]

        public async Task<IActionResult> GetRBAPayslipData(string LoginID, string Branch, string Period, string EmployeeType, string Employee, string Lang)

        {

            try

            {

                var data = await _payrollRepository.GetRBAPayslipData(LoginID, Branch, Period, EmployeeType, Employee, Lang);

                return Ok(data);

            }

            catch (Exception ex) {

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }

        #endregion

        // Helper method - reverse of UI getWorkTypeByName
        private int GetWorkTypeIdByName(string typeName)
        {
            return typeName switch
            {
                "General Working" => 1,
                "Off Day" => 2,
                "Off Day Working" => 3,
                "Holiday" => 4,
                "Holiday Working" => 5,
                "Unpaid Leave" => 6,
                "Absent" => 7,
                "Annual Leave" => 8,
                "Medical Leave" => 9,
                "Maternity Leave" => 10,
                "Paternity Leave" => 11,
                "Hospitalization Leave" => 12,
                "Socso" => 13,
                "Non Schedule Off" => 14,
                "Replacement Leave" => 15,
                "Compensanate Leave" => 16,
                "Marriage Leave" => 17,
                _ => 1 // Default to General Working
            };
        }

        #region Voucher Filter Report
        [HttpGet]
        [Route("GetVoucherDetailsByFilter")]
        public async Task<IActionResult> GetVoucherDetailsByFilter(string branch, DateTime period, string employeeType, string bankCode, string paymentType, string voucherType)
        {
            try
            {
                var result = await _payrollRepository.GetVoucherDetailsByFilter(branch, period, employeeType, bankCode, paymentType, voucherType);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        #endregion

    }

}



[Keyless]

public class LeaveClassResults

{

    public int LeaveTaken { get; set; }

    public int LeaveAvailable { get; set; }

}

[Keyless]

public class LastprocessedDates

{

    public DateTime ProcessedDate { get; set; }

}



public class ProcessResult

{

    public string? Message { get; set; }

}



