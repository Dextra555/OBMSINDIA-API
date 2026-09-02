using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;
using OBMS.WebAPI.Repositories.Implementation;
using OBMS.WebAPI.Repositories.Interface;

namespace OBMS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        HttpResponseMessage response = new HttpResponseMessage();
        private readonly OBMSDbContext _oBMSDbContext;
        private readonly IEmployeeRepository _employeeRepository;
        
        public EmployeeController(IEmployeeRepository employeeRepository, OBMSDbContext oBMSDbContext)
        {
            _employeeRepository = employeeRepository;
            _oBMSDbContext = oBMSDbContext;
        }

        [HttpGet]
        [Route("EmployeeNoByBranchID")]
        public async Task<ActionResult<Object>> GetEmployeeNoByBranchID(string branchId)
        {
            try
            {
                return Ok(await _employeeRepository.GetEmployeeNoByBranchID(branchId));
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        //[HttpGet]
        //[Route("Employees")]
        //public async Task<ActionResult<Object>> GetEmployees(string name)
        //{
        //    var employees = _oBMSDbContext.Employees.OrderByDescending(x => x.EMP_ID).ToList();


        //    var results = new Dictionary<string, Object>();


        //    if (name != "none")
        //    {


        //        var branchList = from branchMaster in _oBMSDbContext.BranchMasters
        //                     join obmsBranches in _oBMSDbContext.OBMSBranches on branchMaster.Code equals obmsBranches.BranchCode
        //                     where obmsBranches.Name == name
        //                     orderby branchMaster.Name
        //                     select new
        //                     {
        //                         branchMaster.Code,
        //                         branchMaster.Name
        //                     };
        //        results.Add("branchList", branchList);
        //    }

        //    results.Add("employees", employees);


        //    return results;
        //}

        [HttpGet]
        [Route("Employees")]
        public async Task<ActionResult<object>> GetEmployees(string name)
        {
            try
            {
                var employeeData = await (
                    from emp in _oBMSDbContext.Employees
                    join empDetails in _oBMSDbContext.EmploymentDetails
                        on emp.EMP_CODE equals empDetails.EMPPAY_CODE into empGroup
                    from details in empGroup.DefaultIfEmpty()
                    join dept in _oBMSDbContext.Departments on emp.DepartmentId equals dept.DepartmentId into deptGroup
                    from department in deptGroup.DefaultIfEmpty()
                    join desig in _oBMSDbContext.Designations on emp.DesignationId equals desig.DesignationId into desigGroup
                    from designation in desigGroup.DefaultIfEmpty()
                    orderby emp.EMP_ID descending
                    select new
                    {
                        EMP_ID = emp.EMP_ID,
                        EMP_CODE = emp.EMP_CODE,
                        EMP_NAME = emp.EMP_NAME,
                        EMP_PHONE = emp.EMP_PHONE,
                        EMP_MOBILEPHONE = emp.EMP_MOBILEPHONE,
                        EMP_BRANCH_CODE = emp.EMP_BRANCH_CODE,
                        EMP_CLIENT = emp.EMP_CLIENT,
                        DepartmentName = department != null ? department.DepartmentName : null,
                        DesignationName = designation != null ? designation.DesignationName : null,
                        LASTUPDATE = emp.LASTUPDATE,
                        // Indian Compliance Fields
                        AadhaarNumber = emp.AadhaarNumber,
                        PANNumber = emp.PANNumber,
                        IndianState = emp.IndianState,
                        BankAccountNumber = emp.BankAccountNumber,
                        BankName = emp.BankName,
                        BankIFSC = emp.BankIFSC,
                        // Basic Info
                        EMP_SEX = emp.EMP_SEX,
                        EMP_ROLE = emp.EMP_ROLE,
                        // Employment Details (only if exists)
                        EMPPAY_DATE_JOINED = details != null ? details.EMPPAY_DATE_JOINED : (DateTime?)null,
                        EMPPAY_DATE_RESIGNED = details != null && details.EMPPAY_DATE_RESIGNED.HasValue ? details.EMPPAY_DATE_RESIGNED.Value : (DateTime?)null,
                        EMPPAY_CATEGORY = details != null ? details.EMPPAY_CATEGORY : null
                    }).ToListAsync();

            var results = new Dictionary<string, object>
            {
                { "employees", employeeData }
            };

            if (name != "none")
            {
                var branchList = await (
                    from branchMaster in _oBMSDbContext.BranchMasters
                    join obmsBranches in _oBMSDbContext.OBMSBranches
                        on branchMaster.Code equals obmsBranches.BranchCode
                    where obmsBranches.Name == name
                    orderby branchMaster.Name
                    select new
                    {
                        branchMaster.Code,
                        branchMaster.Name
                    }).ToListAsync();

                results.Add("branchList", branchList);
            }

            return results;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving employees", error = ex.Message });
            }
        }



        [HttpGet]
        [Route("EmployeesByBranchId")]
        public async Task<ActionResult<object>> EmployeesByBranchId(string branchId)
        {
            var employees = await (
                from emp in _oBMSDbContext.Employees
                join empDetails in _oBMSDbContext.EmploymentDetails
                    on emp.EMP_CODE equals empDetails.EMPPAY_CODE into empGroup
                from details in empGroup.DefaultIfEmpty()
                join dept in _oBMSDbContext.Departments on emp.DepartmentId equals dept.DepartmentId into deptGroup
                from department in deptGroup.DefaultIfEmpty()
                join desig in _oBMSDbContext.Designations on emp.DesignationId equals desig.DesignationId into desigGroup
                from designation in desigGroup.DefaultIfEmpty()
                where emp.EMP_BRANCH_CODE == branchId
                orderby emp.EMP_ID descending
                select new
                {
                    emp.EMP_ID,
                    emp.EMP_CODE,
                    emp.EMP_NAME,
                    emp.EMP_SEX,
                    emp.EMP_IC_NEW,
                    emp.EMP_PASSPORT_NO,
                    dept_name = department != null ? department.DepartmentName : null,
                    desig_name = designation != null ? designation.DesignationName : null,
                    emp.EMP_TOWN,
                    emp.EMP_ROLE,
                    emp.IndianState,
                    emp.AadhaarNumber,
                    emp.PANNumber,
                    EMPPAY_DATE_JOINED = details != null ? details.EMPPAY_DATE_JOINED : (DateTime?)null,
                    EMPPAY_CATEGORY = details != null ? details.EMPPAY_CATEGORY : null
                }).ToListAsync();

            return Ok(employees);
        }



        [HttpGet]
        [Route("EmployeeById")]
        public async Task<Object> GetEmployeeById(int employeeId)
        {
            return _employeeRepository.GetEmployeeById(employeeId);
        }



        [HttpGet]
        [Route("GetEmployeeMaster")]
        public async Task<ActionResult<Object>> GetEmployeeMaster(string userId)
        {
            try
            {
                var branchList = await _employeeRepository.GetEmployeeMasterList(userId);
                return Ok(branchList);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [HttpGet]
        [Route("GetClientsFromBranchId")]
        public async Task<ActionResult<Object>> GetClientsFromBranchId(string branchId, string empType)
        {
            try
            {
                var clientList = await _employeeRepository.GetClientsFromBranchId(branchId);

                var empNo = await _employeeRepository.GetEmployeeNoByBranchID(branchId);

                var results = new Dictionary<string, Object>();

                results.Add("clientList", clientList);
                results.Add("emp", empNo);

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        [Route("GetEmployeeNo")]
        public async Task<ActionResult<Object>> GetEmployeeNo()
        {
            try
            {
                var obj = await _employeeRepository.GetEmployeeNo();           
                var results = new Dictionary<string, Object>();

                results.Add("emp", obj);

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [HttpPost]
        [Route("SaveAndUpdateEmployee")]
        public async Task<IActionResult> SaveAndUpdateEmployee(EmployeeRequestDto employeeRequestDto)
        {
            try
            {
                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                // Validate Indian compliance fields for new employees
                // Aadhaar number is now optional

                // Sync PAN and Aadhaar values between fields if one is set and the other isn't
                if (string.IsNullOrWhiteSpace(employeeRequestDto.PANNumber) && !string.IsNullOrWhiteSpace(employeeRequestDto.EMPFL_TAX_NO))
                {
                    employeeRequestDto.PANNumber = employeeRequestDto.EMPFL_TAX_NO;
                }
                else if (!string.IsNullOrWhiteSpace(employeeRequestDto.PANNumber) && string.IsNullOrWhiteSpace(employeeRequestDto.EMPFL_TAX_NO))
                {
                    employeeRequestDto.EMPFL_TAX_NO = employeeRequestDto.PANNumber;
                }

                // Check PAN uniqueness
                if (!string.IsNullOrWhiteSpace(employeeRequestDto.PANNumber))
                {
                    var existingPAN = _oBMSDbContext.Employees
                        .Where(x => x.PANNumber == employeeRequestDto.PANNumber && x.EMP_ID != employeeRequestDto.EMP_ID)
                        .Select(x => x.PANNumber)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(existingPAN))
                    {
                        dictResult.Add("Success", "Warning");
                        dictResult.Add("EmployeePAN", existingPAN);
                        dictResult.Add("Message", "Employee already has this PAN number. Please choose a different PAN number.");
                        return Ok(dictResult);
                    }
                }

                // Check Aadhaar uniqueness
                if (!string.IsNullOrWhiteSpace(employeeRequestDto.AadhaarNumber))
                {
                    var existingAadhaar = _oBMSDbContext.Employees
                        .Where(x => x.AadhaarNumber == employeeRequestDto.AadhaarNumber && x.EMP_ID != employeeRequestDto.EMP_ID)
                        .Select(x => x.AadhaarNumber)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(existingAadhaar))
                    {
                        dictResult.Add("Success", "Warning");
                        dictResult.Add("EmployeeAadhaar", existingAadhaar);
                        dictResult.Add("Message", "Employee already has this Aadhaar number. Please choose a different Aadhaar number.");
                        return Ok(dictResult);
                    }
                }

                // Validate Malaysian IC only if provided (for legacy data)
                if (!string.IsNullOrWhiteSpace(employeeRequestDto.EMP_IC_NEW))
                {
                    var existingIC = _oBMSDbContext.Employees
                        .Where(x => x.EMP_IC_NEW == employeeRequestDto.EMP_IC_NEW && x.EMP_ID != employeeRequestDto.EMP_ID)
                        .Select(x => x.EMP_IC_NEW)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(existingIC))
                    {
                        dictResult.Add("Success", "Warning");
                        dictResult.Add("EmployeeIC", existingIC);
                        dictResult.Add("Message", "Employee already has this IC number. Please choose a different IC number.");
                        return Ok(dictResult);
                    }
                }

                // Continue with rest of the flow...
                var employee = new Employee();
                if (employeeRequestDto.EMP_ID != 0)
                {
                    employee = _oBMSDbContext.Employees.Where(x => x.EMP_ID == employeeRequestDto.EMP_ID).FirstOrDefault();
                }

                employee.EMP_ID = employeeRequestDto.EMP_ID;
                employee.EMP_ROLE = employeeRequestDto.EMP_ROLE;
                employee.EMP_CODE = employeeRequestDto.EMP_CODE;
                employee.EMP_NAME = employeeRequestDto.EMP_NAME;
                employee.EMP_BRANCH_CODE = employeeRequestDto.EMP_BRANCH_CODE;
                employee.EMP_ADDRESS1 = employeeRequestDto.EMP_ADDRESS1 ?? "";
                employee.EMP_ADDRESS2 = employeeRequestDto.EMP_ADDRESS2 ?? "";
                employee.EMP_POST_CODE = employeeRequestDto.EMP_POST_CODE ?? "";
                employee.EMP_TOWN = employeeRequestDto.EMP_TOWN ?? "";
                employee.EMP_STATE = employeeRequestDto.EMP_STATE ?? "";
                employee.EMP_NATIONAL = employeeRequestDto.EMP_NATIONAL ?? "";
                employee.EMP_PHONE = employeeRequestDto.EMP_PHONE ?? "";
                employee.EMP_HGH_EDU = employeeRequestDto.EMP_HGH_EDU ?? "";
                employee.EM_WORK_EXP = employeeRequestDto.EM_WORK_EXP ?? "";
                employee.EMP_DATE_OF_BIRTH = employeeRequestDto.EMP_DATE_OF_BIRTH;
                employee.EMP_SEX = employeeRequestDto.EMP_SEX ?? "";
                employee.EMP_MARTIAL_STATUS = employeeRequestDto.EMP_MARTIAL_STATUS ?? "";
                employee.EMP_SPOUSE_NAME = employeeRequestDto.EMP_SPOUSE_NAME ?? "";
                employee.EMP_FATHER_NAME = employeeRequestDto.EMP_FATHER_NAME ?? "";
                employee.EMP_SP_IC = employeeRequestDto.EMP_SP_IC ?? "";
                employee.EMP_NO_CHILD = employeeRequestDto.EMP_NO_CHILD;
                employee.EMP_SP_WORK = employeeRequestDto.EMP_SP_WORK;
                employee.EMP_PER_NAME_CONTACT = employeeRequestDto.EMP_PER_NAME_CONTACT ?? "";
                employee.EMP_CONTACT_ADDRESS1 = employeeRequestDto.EMP_CONTACT_ADDRESS1 ?? "";
                employee.EMP_CONTACT_ADDRESS2 = employeeRequestDto.EMP_CONTACT_ADDRESS2 ?? "";
                employee.EMP_CONTACT_POST_CODE = employeeRequestDto.EMP_CONTACT_POST_CODE ?? "";
                employee.EMP_CONTACT_TOWN = employeeRequestDto.EMP_CONTACT_TOWN ?? "";
                employee.EMP_CONTACT_STATE = employeeRequestDto.EMP_CONTACT_STATE ?? "";
                employee.EMP_CONTACT_TELEPHONE = employeeRequestDto.EMP_CONTACT_TELEPHONE ?? "";
                employee.LASTUPDATE = employeeRequestDto.LASTUPDATE;
                employee.LastUpdatedBy = employeeRequestDto.LastUpdatedBy ?? "admin";
                
                // Map Malaysian fields (keep for legacy compatibility)
                employee.EMP_IC_OLD = employeeRequestDto.EMP_IC_OLD ?? "";
                employee.EMP_IC_NEW = !string.IsNullOrWhiteSpace(employeeRequestDto.EMP_IC_NEW) 
                                        ? employeeRequestDto.EMP_IC_NEW 
                                        : (employeeRequestDto.AadhaarNumber ?? "000000000000"); // Fallback for Indian Citizens
                employee.EMP_IC_COLOR = employeeRequestDto.EMP_IC_COLOR ?? "None";
                employee.EMP_PASSPORT_NO = employeeRequestDto.EMP_PASSPORT_NO ?? "";
                employee.EMP_RACE = employeeRequestDto.EMP_RACE ?? "";
                employee.EMP_NATIONAL = employeeRequestDto.EMP_NATIONAL ?? "";
                employee.EMP_SP_IC = employeeRequestDto.EMP_SP_IC ?? "";
                employee.EMP_MOBILEPHONE = employeeRequestDto.EMP_MOBILEPHONE ?? "";
                employee.EMP_CITIZEN = employeeRequestDto.EMP_CITIZEN;
                employee.EMP_CHECKLIST = employeeRequestDto.EMP_CHECKLIST;
                employee.EMP_CLIENT = employeeRequestDto.EMP_CLIENT ?? "";
                employee.KDNVetting = employeeRequestDto.KDNVetting;
                
                // Map character flags with null checks
                if (!string.IsNullOrEmpty(employeeRequestDto.NewSalaryStructure))
                {
                    employee.NewSalaryStructure = employeeRequestDto.NewSalaryStructure[0];
                }
                if (!string.IsNullOrEmpty(employeeRequestDto.SalaryStructure1000_3h))
                {
                    employee.SalaryStructure1000_3h = employeeRequestDto.SalaryStructure1000_3h[0];
                }

                // Map Indian Compliance Fields
                employee.AadhaarNumber = employeeRequestDto.AadhaarNumber;
                employee.PANNumber = employeeRequestDto.PANNumber;
                employee.PFAccountNumber = employeeRequestDto.PFAccountNumber;
                employee.ESINumber = employeeRequestDto.ESINumber;
                employee.SalaryGroup = employeeRequestDto.SalaryGroup ?? "None";
                employee.SpousePAN = employeeRequestDto.SpousePAN;
                employee.SpouseAadhaar = employeeRequestDto.SpouseAadhaar;
                employee.IndianState = employeeRequestDto.IndianState;
                employee.BankAccountNumber = employeeRequestDto.BankAccountNumber;
                employee.BankIFSC = employeeRequestDto.BankIFSC;
                employee.BankName = employeeRequestDto.BankName;
                employee.UPIId = employeeRequestDto.UPIId;

                // Map Department and Designation
                employee.DepartmentId = employeeRequestDto.DepartmentId;
                employee.DesignationId = employeeRequestDto.DesignationId;


                var employment = new EmploymentDetails();
                if (employeeRequestDto.EMPPAY_ID != 0)
                {
                    employment = _oBMSDbContext.EmploymentDetails.Where(x => x.EMPPAY_ID == employeeRequestDto.EMPPAY_ID).FirstOrDefault();
                }



                employment.EMPPAY_ID = employeeRequestDto.EMPPAY_ID;
                employment.EMPPAY_BRANCHCODE = employeeRequestDto.EMP_BRANCH_CODE;
                employment.EMPPAY_JOB_TITLE = employeeRequestDto.EMPPAY_JOB_TITLE;
                employment.EMPPAY_CATEGORY = employeeRequestDto.EMPPAY_CATEGORY;
                employment.EMPPAY_DATE_JOINED = employeeRequestDto.EMPPAY_DATE_JOINED.HasValue
                                                    ? (DateTime)employeeRequestDto.EMPPAY_DATE_JOINED.Value
                                                    : new DateTime(1753, 1, 1);
                employment.EMPPAY_DATE_CONFIRM = employeeRequestDto.EMPPAY_DATE_CONFIRM.HasValue
                                                    ? (DateTime)employeeRequestDto.EMPPAY_DATE_CONFIRM.Value
                                                    : null;
                //employment.EMPPAY_DATE_RESIGNED = (DateTime)employeeRequestDto.EMPPAY_DATE_RESIGNED;

                employment.EMPPAY_DATE_RESIGNED = employeeRequestDto.EMPPAY_DATE_RESIGNED.HasValue
                                                     ? (DateTime)employeeRequestDto.EMPPAY_DATE_RESIGNED.Value
                                                     : null;


                // Store EmploymentDetails fields separately from CB fields
                // CB fields are the source of truth for salary breakdown
                // SALARYLAB is set later based on CB auto-generation logic
                
                // If Commercial Breakdown is used (CB_Basic > 0), use CB_Basic as EMPPAY_BASIC_RATE
                // This ensures backward compatibility with existing system
                if (employeeRequestDto.CB_Basic.HasValue && employeeRequestDto.CB_Basic > 0)
                {
                    employment.EMPPAY_BASIC_RATE = (double)employeeRequestDto.CB_Basic;
                }
                else
                {
                    // Otherwise use the provided EMPPAY_BASIC_RATE (from salary slab)
                    employment.EMPPAY_BASIC_RATE = employeeRequestDto.EMPPAY_BASIC_RATE;
                }
                employment.ATTENDANCEALLOWANCE = employeeRequestDto.ATTENDANCEALLOWANCE;
                employment.NewStructureATTENDANCEALLOWANCE = 0;
                employment.SpecialAllowance = employeeRequestDto.SpecialAllowance;
                employment.LASTUPDATE = employeeRequestDto.LASTUPDATE;
                employment.LastUpdatedBy = employeeRequestDto.LastUpdatedBy;
                employment.AttendanceAllowanceWorkingDays = employeeRequestDto.AttendanceAllowanceWorkingDays;
                employment.AttendanceAllowanceFollowCalendar = employeeRequestDto.AttendanceAllowanceFollowCalendar ?? "N";


                var salaryDetails = new EmployeeSalaryDetails();
                if (employeeRequestDto.EMPFL_ID != 0)
                {
                    salaryDetails = _oBMSDbContext.EmployeeSalaryDetails.Where(x => x.EMPFL_ID == employeeRequestDto.EMPFL_ID).FirstOrDefault();
                }

                salaryDetails.EMPFL_ID = employeeRequestDto.EMPFL_ID;
                salaryDetails.EMPFL_BRANCHCODE = employeeRequestDto.EMP_BRANCH_CODE;
                salaryDetails.EMPFL_BANK = employeeRequestDto.EMPFL_BANK;
                salaryDetails.EMPFL_BK_ACCNO = employeeRequestDto.EMPFL_BK_ACCNO;
                salaryDetails.EMPFL_TAX_NO = employeeRequestDto.EMPFL_TAX_NO;
                salaryDetails.EMPFL_EPFNO = employeeRequestDto.EMPFL_EPFNO;
                salaryDetails.EMPFL_EPF8Pa = employeeRequestDto.EMPFL_EPF8Pa;
                salaryDetails.EMPFL_SOSCO_NO = employeeRequestDto.EMPFL_SOSCO_NO;
                salaryDetails.EPFDETECT = employeeRequestDto.EPFDETECT;
                salaryDetails.PAYMODE = employeeRequestDto.PAYMODE;
                salaryDetails.SOCSODETECT = employeeRequestDto.SOCSODETECT;
                salaryDetails.TMPGUARD = employeeRequestDto.TMPGUARD;
                salaryDetails.DETECTBYND55 = employeeRequestDto.DETECTBYND55;
                salaryDetails.INCOMETAXDETECT = employeeRequestDto.INCOMETAXDETECT;
                salaryDetails.EMP_SP_TEL_NO = employeeRequestDto.EMP_SP_TEL_NO;
                salaryDetails.LASTUPDATE = employeeRequestDto.LASTUPDATE;
                salaryDetails.LastUpdatedBy = employeeRequestDto.LastUpdatedBy;

                // DEBUG: Log incoming CB values from DTO
                Console.WriteLine("=== INCOMING CB VALUES FROM DTO ===");
                Console.WriteLine($"CB_Basic: {employeeRequestDto.CB_Basic}");
                Console.WriteLine($"CB_DA: {employeeRequestDto.CB_DA}");
                Console.WriteLine($"CB_HRA: {employeeRequestDto.CB_HRA}");
                Console.WriteLine($"CB_HRAPercentage: {employeeRequestDto.CB_HRAPercentage}");
                Console.WriteLine($"CB_Leaves: {employeeRequestDto.CB_Leaves}");
                Console.WriteLine($"CB_LeavesPercentage: {employeeRequestDto.CB_LeavesPercentage}");
                Console.WriteLine($"CB_OtherAllowances: {employeeRequestDto.CB_OtherAllowances}");
                Console.WriteLine($"CB_NH: {employeeRequestDto.CB_NH}");
                Console.WriteLine($"CB_NHPercentage: {employeeRequestDto.CB_NHPercentage}");
                Console.WriteLine($"CB_AdvanceStatutoryBonus: {employeeRequestDto.CB_AdvanceStatutoryBonus}");
                Console.WriteLine($"CB_AdvanceStatutoryBonusPercentage: {employeeRequestDto.CB_AdvanceStatutoryBonusPercentage}");
                Console.WriteLine($"CB_SubTotal: {employeeRequestDto.CB_SubTotal}");
                Console.WriteLine("===================================");

                // Map Commercial Breakdown Fields to employee - Simplified
                employee.CB_Basic = employeeRequestDto.CB_Basic;
                employee.CB_DA = employeeRequestDto.CB_DA;
                employee.CB_HRA = employeeRequestDto.CB_HRA;
                employee.CB_HRAPercentage = employeeRequestDto.CB_HRAPercentage;
                employee.CB_Leaves = employeeRequestDto.CB_Leaves;
                employee.CB_LeavesPercentage = employeeRequestDto.CB_LeavesPercentage;
                employee.CB_OtherAllowances = employeeRequestDto.CB_OtherAllowances;
                employee.CB_NH = employeeRequestDto.CB_NH;
                employee.CB_NHPercentage = employeeRequestDto.CB_NHPercentage;
                employee.CB_AdvanceStatutoryBonus = employeeRequestDto.CB_AdvanceStatutoryBonus;
                employee.CB_AdvanceStatutoryBonusPercentage = employeeRequestDto.CB_AdvanceStatutoryBonusPercentage;
                employee.CB_SubTotal = employeeRequestDto.CB_SubTotal;

                // Auto-generate SalaryStructure from CB fields if present
                if (employee.CB_Basic.HasValue && employee.CB_Basic > 0)
                {
                    // Calculate total monthly from CB fields
                    decimal totalMonthly = (employee.CB_Basic ?? 0) 
                                         + (employee.CB_DA ?? 0) 
                                         + (employee.CB_HRA ?? 0) 
                                         + (employee.CB_Leaves ?? 0) 
                                         + (employee.CB_NH ?? 0) 
                                         + (employee.CB_OtherAllowances ?? 0);

                    // Default values for rate calculation
                    decimal workingDays = 26m;
                    decimal workingHours = 8m;
                    decimal offDayMultiplier = 1.5m;
                    decimal holidayMultiplier = 2.0m;
                    decimal otMultiplier = 1.5m;

                    // Derive daily rate
                    decimal generalDayRate = totalMonthly / workingDays;

                    // Check if SalaryStructure already exists for this employee
                    SalaryStructure salaryStructure = null;
                    
                    // Try to find existing SalaryStructure by name pattern
                    var existingSalaryStructure = _oBMSDbContext.SalaryStructures
                        .FirstOrDefault(s => s.Name == $"CB-{employee.EMP_CODE}");

                    if (existingSalaryStructure != null)
                    {
                        // Update existing SalaryStructure
                        existingSalaryStructure.GeneralDayRate = generalDayRate;
                        existingSalaryStructure.GeneralDayHours = workingHours;
                        existingSalaryStructure.GeneralDayOTRate = otMultiplier;
                        existingSalaryStructure.OffDayRate = offDayMultiplier;
                        existingSalaryStructure.OffDayOTRate = otMultiplier;
                        existingSalaryStructure.HolidayRate = holidayMultiplier;
                        existingSalaryStructure.HolidayOTRate = holidayMultiplier;
                        existingSalaryStructure.WorkingDays = workingDays;
                        existingSalaryStructure.WorkingHours = workingHours;
                        existingSalaryStructure.SalaryBand = totalMonthly;
                        existingSalaryStructure.LastUpdatedDate = DateTime.Now;
                        
                        salaryStructure = existingSalaryStructure;
                        
                        Console.WriteLine($"Updated existing SalaryStructure for employee {employee.EMP_CODE}");
                    }
                    else
                    {
                        // Create new SalaryStructure
                        salaryStructure = new SalaryStructure
                        {
                            BranchCode = employee.EMP_BRANCH_CODE,
                            EmployeeType = "CUSTOM",
                            EmployeeNationality = employee.EMP_NATIONAL,
                            Name = $"CB-{employee.EMP_CODE}",
                            GeneralDayRate = generalDayRate,
                            GeneralDayHours = workingHours,
                            GeneralDayOTRate = otMultiplier,
                            OffDayRate = offDayMultiplier,
                            OffDayOTRate = otMultiplier,
                            HolidayRate = holidayMultiplier,
                            HolidayOTRate = holidayMultiplier,
                            WorkingDays = workingDays,
                            WorkingHours = workingHours,
                            SalaryBand = totalMonthly,
                            TravelAllowance = 0,
                            Status = "A",
                            Active = "Y",
                            LastUpdatedDate = DateTime.Now,
                            LastUpdatedBy = employee.LastUpdatedBy ?? "Auto-Generator",
                            EICC = false,
                            NonStructure = false
                        };

                        _oBMSDbContext.SalaryStructures.Add(salaryStructure);
                        await _oBMSDbContext.SaveChangesAsync();
                        
                        Console.WriteLine($"Created new SalaryStructure for employee {employee.EMP_CODE}");
                    }

                    // Link EmploymentDetails to the auto-generated SalaryStructure
                    employment.SALARYLAB = salaryStructure.SalaryId;
                    
                    Console.WriteLine($"Linked EmploymentDetails to SalaryStructure ID: {salaryStructure.SalaryId}");
                    Console.WriteLine($"Total Monthly: {totalMonthly}, Daily Rate: {generalDayRate}");
                }
                else
                {
                    // No CB fields, use existing SALARYLAB value
                    employment.SALARYLAB = employeeRequestDto.SALARYLAB;
                }

                // DEBUG: Log values mapped to employee entity before save
                Console.WriteLine("=== CB VALUES MAPPED TO EMPLOYEE ENTITY ===");
                Console.WriteLine($"CB_Basic: {employee.CB_Basic}");
                Console.WriteLine($"CB_DA: {employee.CB_DA}");
                Console.WriteLine($"CB_HRA: {employee.CB_HRA}");
                Console.WriteLine($"CB_HRAPercentage: {employee.CB_HRAPercentage}");
                Console.WriteLine($"CB_Leaves: {employee.CB_Leaves}");
                Console.WriteLine($"CB_LeavesPercentage: {employee.CB_LeavesPercentage}");
                Console.WriteLine($"CB_OtherAllowances: {employee.CB_OtherAllowances}");
                Console.WriteLine($"CB_NH: {employee.CB_NH}");
                Console.WriteLine($"CB_NHPercentage: {employee.CB_NHPercentage}");
                Console.WriteLine($"CB_SubTotal: {employee.CB_SubTotal}");
                Console.WriteLine($"EMP_ID: {employee.EMP_ID}");
                Console.WriteLine("=========================================");

                // ── Branch change validation ─────────────────────────────────
                if (employeeRequestDto.IsBranchChanged && !employeeRequestDto.BranchStartDate.HasValue)
                {
                    dictResult.Add("Success", "Warning");
                    dictResult.Add("Message", "Please provide the Effective Start Date for the new branch.");
                    return Ok(dictResult);
                }

                // When branch has changed, keep the transfer-tracking fields on the Employee row
                if (employeeRequestDto.IsBranchChanged && employeeRequestDto.EMP_ID != 0)
                {
                    var existingEmployee = _oBMSDbContext.Employees
                        .Where(x => x.EMP_ID == employeeRequestDto.EMP_ID)
                        .Select(x => x.EMP_BRANCH_CODE)
                        .FirstOrDefault();

                    employee.OldBranch      = existingEmployee ?? "";
                    employee.TransferDate   = employeeRequestDto.BranchStartDate;
                    employee.HasTransfered  = true;
                }

                await _employeeRepository.saveAndUpdateEmployee(
                    employee, employment, salaryDetails,
                    isBranchChanged: employeeRequestDto.IsBranchChanged,
                    branchStartDate: employeeRequestDto.BranchStartDate);

                // DEBUG: Log after save
                Console.WriteLine("=== AFTER SAVE OPERATION ===");
                Console.WriteLine("Save completed successfully");
                Console.WriteLine("=============================");

                dictResult.Add("Success", "Success");
                dictResult.Add("Employee", employee);
                return Ok(dictResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = "Error",
                    Message = "An error occurred while saving employee: " + (ex.InnerException?.Message ?? ex.Message)
                });
            }

        }

        [HttpPost]
        [Route("UpdateEmployeeTransfer")]
        public async Task<ActionResult<HttpResponseMessage>> UpdateEmployeeTransfer(EmployeeTransferDto employeeTransferDto)
        {
            await _employeeRepository.UpdateEmployeeTransfer(employeeTransferDto);


            Dictionary<string, object> dictResult = new Dictionary<string, object>();
            dictResult.Add("Success", "Success");

            response.Headers.Add("Success", "Successfully save & update Employee details.");

            return Ok(dictResult);
        }

        [HttpGet]
        [Route("CheckEmployeeInfo")]
        public async Task<ActionResult<Object>> CheckEmployeeInfo(string from, string data)
        {
            try
            {

                var dictResult = _employeeRepository.CheckEmployeeInfo(from,data);


                return Ok(dictResult);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [HttpGet("GetAllEmployeesWithHistory")]
        public async Task<IActionResult> GetAllEmployeesWithHistory(string? branch = null)
        {
            try
            {
                var result = await _employeeRepository.GetAllEmployeesWithHistory(branch);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving employee history.",
                    Details = ex.Message
                });
            }
        }

        // Indian Compliance Endpoints
        [HttpPost("CreateEmployee")]
        public async Task<ActionResult<object>> CreateEmployee([FromBody] EmployeeCreateDto employeeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _employeeRepository.CreateEmployee(employeeDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while creating employee.",
                    Details = ex.Message
                });
            }
        }

        [HttpPut("UpdateEmployee/{id}")]
        public async Task<ActionResult<object>> UpdateEmployee(int id, [FromBody] EmployeeUpdateDto employeeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != employeeDto.EMP_ID)
                {
                    return BadRequest("Employee ID mismatch");
                }

                var result = await _employeeRepository.UpdateEmployee(employeeDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while updating employee.",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("SearchEmployees")]
        public async Task<ActionResult<List<EmployeeSearchResultDto>>> SearchEmployees([FromBody] EmployeeSearchDto searchDto)
        {
            try
            {
                var result = await _employeeRepository.SearchEmployees(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while searching employees.",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("ValidateEmployeeField")]
        public async Task<ActionResult<object>> ValidateEmployeeField([FromBody] EmployeeValidationDto validationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _employeeRepository.ValidateEmployeeField(validationDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while validating employee field.",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("GetEmployeeByPAN/{pan}")]
        public async Task<ActionResult<object>> GetEmployeeByPAN(string pan)
        {
            try
            {
                var result = await _employeeRepository.GetEmployeeByPAN(pan);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving employee by PAN.",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("GetEmployeeByAadhaar/{aadhaar}")]
        public async Task<ActionResult<object>> GetEmployeeByAadhaar(string aadhaar)
        {
            try
            {
                var result = await _employeeRepository.GetEmployeeByAadhaar(aadhaar);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving employee by Aadhaar.",
                    Details = ex.Message
                });
            }
        }

        [HttpDelete("DeleteEmployee/{id}")]
        public async Task<ActionResult<object>> DeleteEmployee(int id)
        {
            try
            {
                var result = await _employeeRepository.DeleteEmployee(id);
                if (result)
                {
                    return Ok(new { Success = "Success", Message = "Employee deleted successfully" });
                }
                return NotFound(new { Success = "Error", Message = "Employee not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = "Error",
                    Message = "An error occurred while deleting employee.",
                    Details = ex.Message
                });
            }
        }

        // Excel Import Endpoints
        [HttpPost("PreviewExcelImport")]
        public async Task<ActionResult<EmployeeImportPreviewDto>> PreviewExcelImport(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Message = "No file uploaded" });
                }

                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { Message = "File size exceeds 5MB limit" });
                }

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
                    !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Message = "Only Excel files (.xlsx, .xls) are allowed" });
                }

                var result = await _employeeRepository.PreviewExcelImportAsync(file);
                
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while processing the Excel file.",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("BulkImportEmployees")]
        public async Task<ActionResult<EmployeeImportResultDto>> BulkImportEmployees([FromBody] EmployeeBulkImportRequestDto request)
        {
            try
            {
                
                if (request.Employees == null || request.Employees.Count == 0)
                {
                    return BadRequest(new { Message = "No employees to import" });
                }

                var result = await _employeeRepository.BulkImportEmployeesAsync(request);
                
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred during bulk import.",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("DownloadImportTemplate")]
        public IActionResult DownloadImportTemplate()
        {
            try
            {
                var templateBytes = _employeeRepository.GenerateImportTemplate();
                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employee_Import_Template.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while generating template.",
                    Details = ex.Message
                });
            }
        }

    }
}
