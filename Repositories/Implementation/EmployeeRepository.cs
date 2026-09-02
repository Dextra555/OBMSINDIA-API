using Microsoft.EntityFrameworkCore;

using OBMS.WebAPI.Models;

using OBMS.WebAPI.Models.Domain;

using OBMS.WebAPI.Models.DTO;

using OBMS.WebAPI.Repositories.Interface;



namespace OBMS.WebAPI.Repositories.Implementation

{

    public class EmployeeRepository : IEmployeeRepository

    {

        private readonly OBMSDbContext _oBMSDbContext;



        public EmployeeRepository(OBMSDbContext oBMSDbContext)

        {

            _oBMSDbContext = oBMSDbContext;



        }

        public async Task<Dictionary<string, string>> GetEmployeeNoByBranchID(string clientId)

        {

            var results = new Dictionary<string, string>();



            try

            {

                var client = await _oBMSDbContext.ClientMasters.FirstOrDefaultAsync(x => x.Code == clientId);

                if (client == null) return null;



                string shortName = client.Shortname?.Trim() ?? "EMP";



                var lastEmployee = await _oBMSDbContext.Employees

                    .Where(e => e.EMP_CLIENT == clientId)

                    .OrderByDescending(e => e.EMP_ID)

                    .FirstOrDefaultAsync();



                int nextNumber = 1;

                if (lastEmployee != null && !string.IsNullOrEmpty(lastEmployee.EMP_CODE))

                {

                    try

                    {

                        // Try to extract the numeric part from the end (last 6 digits)

                        string code = lastEmployee.EMP_CODE;

                        if (code.Length >= 6)

                        {

                            string numericPart = code.Substring(code.Length - 6);

                            if (int.TryParse(numericPart, out int lastNum))

                            {

                                nextNumber = lastNum + 1;

                            }

                        }

                    }

                    catch

                    {

                        // Fallback to ID-based if parsing fails

                        nextNumber = lastEmployee.EMP_ID + 1;

                    }

                }



                results.Add("ShortName", shortName);

                results.Add("Code", nextNumber.ToString("D6"));

                return results;

            }

            catch (Exception ex)

            {

                throw new Exception("Error generating employee code for client: " + ex.Message);

            }

        }





        public async Task<Dictionary<string, string>> GetEmployeeNo()

        {

            var results = new Dictionary<string, string>();



            try

            {

                // Get the highest employee code

                string nextEmployeeCode = _oBMSDbContext.Employees

                        .OrderByDescending(emp => emp.EMP_ID)

                        .Select(emp => emp.EMP_CODE)

                        .Where(empCode => empCode != null)

                        .FirstOrDefault();



                var company = Utility.Utility.GetCompnay();

                string shortName = company["ShortName"];



                if (!string.IsNullOrEmpty(nextEmployeeCode))

                {

                    try

                    {

                        // Employee code format: FWGS000018 (FWG = company, S = role, 000018 = sequence)

                        // Extract the last 6 characters as the numeric sequence

                        if (nextEmployeeCode.Length >= 6)

                        {

                            string numericString = nextEmployeeCode.Substring(nextEmployeeCode.Length - 6);

                            int numericPart = int.Parse(numericString) + 1;



                            results.Add("ShortName", shortName);

                            results.Add("Code", numericPart.ToString("D6"));

                            return results;

                        }

                        else

                        {

                            // Invalid format - calculate based on highest employee ID

                            int maxEmpId = _oBMSDbContext.Employees.Max(emp => emp.EMP_ID);

                            results.Add("ShortName", shortName);

                            results.Add("Code", (maxEmpId + 1).ToString("D6"));

                            return results;

                        }

                    }

                    catch (Exception ex)

                    {

                        // Parsing error - calculate based on highest employee ID

                        int maxEmpId = _oBMSDbContext.Employees.Max(emp => emp.EMP_ID);

                        results.Add("ShortName", shortName);

                        results.Add("Code", (maxEmpId + 1).ToString("D6"));

                        return results;

                    }

                }

                else

                {

                    // No employees found - start with 1

                    int maxEmpId = _oBMSDbContext.Employees.Select(e => (int?)e.EMP_ID).Max() ?? 0;

                    results.Add("ShortName", shortName);

                    results.Add("Code", (maxEmpId + 1).ToString("D6"));

                    return results;

                }

            }

            catch (Exception ex)

            {

                // Database access error - throw exception instead of returning static values

                throw new Exception("Failed to generate employee code: " + ex.Message);

            }

        }



        #region Employee Master Module



        public async Task<Dictionary<string, Object>> GetEmployeeById(int employeeId)

        {



            var results = new Dictionary<string, Object>();



            if (employeeId != null)

            {



                var employee = _oBMSDbContext.Employees.Where(x => x.EMP_ID == employeeId).FirstOrDefault();



                results.Add("employee", employee);



                var employment = _oBMSDbContext.EmploymentDetails.Where(x => x.EMPPAY_CODE == employee.EMP_CODE).FirstOrDefault();

                results.Add("employment", employment);



                var salaryDetail = _oBMSDbContext.EmployeeSalaryDetails.Where(x => x.EMPFL_CODE == employee.EMP_CODE).FirstOrDefault();



                // Create a combined object with CB properties from employee

                var salaryDetailWithCB = new EmployeeSalaryDetailWithCBDto

                {

                    // Original salaryDetail properties

                    EMPFL_ID = salaryDetail?.EMPFL_ID ?? 0,

                    EMPFL_CODE = salaryDetail?.EMPFL_CODE ?? string.Empty,

                    EMPFL_BRANCHCODE = salaryDetail?.EMPFL_BRANCHCODE ?? string.Empty,

                    EMPFL_BANK = salaryDetail?.EMPFL_BANK,

                    EMPFL_BK_ACCNO = salaryDetail?.EMPFL_BK_ACCNO,

                    EMPFL_TAX_NO = salaryDetail?.EMPFL_TAX_NO,

                    EMPFL_EPFNO = salaryDetail?.EMPFL_EPFNO,

                    EMPFL_EPF8Pa = salaryDetail?.EMPFL_EPF8Pa,

                    EMPFL_SOSCO_NO = salaryDetail?.EMPFL_SOSCO_NO,

                    EPFDETECT = salaryDetail?.EPFDETECT ?? false,

                    PAYMODE = salaryDetail?.PAYMODE ?? string.Empty,

                    SOCSODETECT = salaryDetail?.SOCSODETECT ?? false,

                    TMPGUARD = salaryDetail?.TMPGUARD ?? false,

                    DETECTBYND55 = salaryDetail?.DETECTBYND55 ?? false,

                    INCOMETAXDETECT = salaryDetail?.INCOMETAXDETECT ?? false,

                    EMP_SP_TEL_NO = salaryDetail?.EMP_SP_TEL_NO,

                    OT = salaryDetail?.OT ?? "No",

                    LASTUPDATE = salaryDetail?.LASTUPDATE ?? DateTime.Now,

                    LastUpdatedBy = salaryDetail?.LastUpdatedBy ?? string.Empty,



                    // Employment Details properties

                    AttendanceAllowanceWorkingDays = employment?.AttendanceAllowanceWorkingDays,

                    AttendanceAllowanceFollowCalendar = employment?.AttendanceAllowanceFollowCalendar,



                    // CB properties from employee - Simplified

                    CB_Basic = employee.CB_Basic,

                    CB_DA = employee.CB_DA,

                    CB_HRA = employee.CB_HRA,

                    CB_HRAPercentage = employee.CB_HRAPercentage,

                    CB_Leaves = employee.CB_Leaves,

                    CB_LeavesPercentage = employee.CB_LeavesPercentage,

                    CB_OtherAllowances = employee.CB_OtherAllowances,

                    CB_NH = employee.CB_NH,

                    CB_NHPercentage = employee.CB_NHPercentage,

                    CB_AdvanceStatutoryBonus = employee.CB_AdvanceStatutoryBonus,

                    CB_AdvanceStatutoryBonusPercentage = employee.CB_AdvanceStatutoryBonusPercentage,

                    CB_SubTotal = employee.CB_SubTotal

                };



                // DEBUG: Log CB values being returned

                Console.WriteLine("=== CB VALUES IN GETEMPLOYEEBYID ===");

                Console.WriteLine($"Employee ID: {employeeId}");

                Console.WriteLine($"CB_Basic: {employee.CB_Basic}");

                Console.WriteLine($"CB_DA: {employee.CB_DA}");

                Console.WriteLine($"CB_HRA: {employee.CB_HRA}");

                Console.WriteLine($"CB_Leaves: {employee.CB_Leaves}");

                Console.WriteLine($"CB_LeavesPercentage: {employee.CB_LeavesPercentage}");

                Console.WriteLine($"CB_SubTotal: {employee.CB_SubTotal}");

                Console.WriteLine($"AttendanceAllowanceWorkingDays: {employment?.AttendanceAllowanceWorkingDays}");

                Console.WriteLine("================================");



                results.Add("salaryDetail", salaryDetailWithCB);



            }



            return results;

        }

        public async Task<Dictionary<string, Object>> GetEmployeeMasterList(string userID)

        {



            var results = new Dictionary<string, Object>();





            var branchs = await _oBMSDbContext.BranchMasters

                .Join(_oBMSDbContext.OBMSBranches,

                    branchmaster => branchmaster.Code,

                    branches => branches.BranchCode,

                    (branchmaster, branches) => new { branchmaster, branches })

                .Where(joinResult => joinResult.branches.Name == userID && joinResult.branches.IsAllowed == true)

                .Select(joinResult => new BranchMaster

                {

                    ID = joinResult.branchmaster.ID,

                    Code = joinResult.branchmaster.Code,

                    Name = joinResult.branchmaster.Name,

                    Address1 = joinResult.branchmaster.Address1,

                    Address2 = joinResult.branchmaster.Address2,

                    PostCode = joinResult.branchmaster.PostCode,

                    City = joinResult.branchmaster.City,

                    State = joinResult.branchmaster.State,

                    Phone = joinResult.branchmaster.Phone,

                    Fax = joinResult.branchmaster.Fax,

                    BankName = joinResult.branchmaster.BankName,

                    BankBranch = joinResult.branchmaster.BankBranch,

                    BankAccount = joinResult.branchmaster.BankAccount,

                    PersonIncharge = joinResult.branchmaster.PersonIncharge,

                    Email = joinResult.branchmaster.Email,

                    Description = joinResult.branchmaster.Description,

                    ShortName = joinResult.branchmaster.ShortName,

                    IsHeadQuarters = joinResult.branchmaster.IsHeadQuarters,

                    UbsCode = joinResult.branchmaster.UbsCode,

                    LastUpdate = joinResult.branchmaster.LastUpdate,

                    LastUpdatedBy = joinResult.branchmaster.LastUpdatedBy,

                    ParentBranch = joinResult.branchmaster.ParentBranch

                })

                .ToListAsync();



            var bankList = _oBMSDbContext.BankLists?.ToList();



            var clientList = _oBMSDbContext.ClientMasters.Where(obj => obj.Status == "Active")?.ToList();



            var salaryStructureList = _oBMSDbContext.SalaryStructure?.ToList();







            results.Add("branchList", branchs);

            results.Add("clientList", clientList);

            results.Add("bankList", bankList);

            results.Add("stateList", Utility.Utility.GetStateList());

            results.Add("icColorList", Utility.Utility.GetICColorList());

            results.Add("nationalityList", Utility.Utility.GetNationalityList());

            results.Add("raceList", Utility.Utility.GetRaceList());

            results.Add("salaryStructureList", salaryStructureList);

            results.Add("emp", GetEmployeeNo());



            return results;

        }





        public async Task<List<Object>> GetClientsFromBranchId(string branchId)

        {

            var ret = _oBMSDbContext.ClientMasters.Where(x => x.Branch == branchId).Where(x => x.Status == "Active").ToList();

            return new List<Object>(ret);

        }



        public async Task<Employee> saveAndUpdateEmployee(Employee employee, EmploymentDetails employment, EmployeeSalaryDetails salaryDetails, bool isBranchChanged = false, DateTime? branchStartDate = null)

        {
            bool isNewEmployee = employee.EMP_ID == 0;

            // DEBUG: Log CB values in repository - Simplified

            Console.WriteLine("=== CB VALUES IN REPOSITORY ===");

            Console.WriteLine($"Employee ID: {employee.EMP_ID}");

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

            Console.WriteLine("================================");



            if (isNewEmployee)

            {

                _oBMSDbContext.Employees.Add(employee);

            }

            else

            {

                _oBMSDbContext.Employees.Update(employee);

            }



            try

            {

                await _oBMSDbContext.SaveChangesAsync();

                Console.WriteLine("=== DATABASE SAVE SUCCESS ===");

                Console.WriteLine("Employee saved to database successfully");

                Console.WriteLine("=============================");

            }

            catch (Exception dbEx)

            {

                Console.WriteLine("=== DATABASE SAVE ERROR ===");

                Console.WriteLine($"Error saving to database: {dbEx.Message}");

                Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");

                Console.WriteLine("=============================");

                throw;

            }

            employment.EMPPAY_CODE = employee.EMP_CODE;

            var _employment = await saveAndUpdateEmploymentDetails(employment);

            salaryDetails.EMPFL_CODE = employee.EMP_CODE;

            var _salaryDetails = await saveAndUpdateEmployeeSalaryDetails(salaryDetails);

            // ── History logic ─────────────────────────────────────────────────
            if (isNewEmployee)
            {
                // NEW EMPLOYEE: write initial EmployeeHistory row
                DateTime startDate = employment.EMPPAY_DATE_JOINED.Date;
                await InsertEmployeeHistoryRow(employee, employment, salaryDetails,
                    empStartDate: startDate,
                    empEndDate: null);
            }
            else if (isBranchChanged && branchStartDate.HasValue)
            {
                // BRANCH CHANGE: close the previous open history row and open a new one
                DateTime newStartDate = branchStartDate.Value.Date;
                DateTime prevEndDate  = newStartDate.AddDays(-1);

                var openHistory = await _oBMSDbContext.EmployeeHistories
                    .Where(h => h.EMP_ID == employee.EMP_ID && h.Emp_EndDate == null)
                    .OrderByDescending(h => h.EMP_HISTORY_ID)
                    .FirstOrDefaultAsync();

                if (openHistory != null)
                {
                    openHistory.Emp_EndDate = prevEndDate;
                    _oBMSDbContext.EmployeeHistories.Update(openHistory);
                    await _oBMSDbContext.SaveChangesAsync();
                }

                await InsertEmployeeHistoryRow(employee, employment, salaryDetails,
                    empStartDate: newStartDate,
                    empEndDate: null);
            }

            return employee;

        }



        public async Task<EmploymentDetails> saveAndUpdateEmploymentDetails(EmploymentDetails employment)

        {

            if (employment.EMPPAY_ID == 0)

            {

                _oBMSDbContext.EmploymentDetails.Add(employment);

            }

            else

            {

                _oBMSDbContext.EmploymentDetails.Update(employment);

            }



            await _oBMSDbContext.SaveChangesAsync();



            return employment;

            throw new NotImplementedException();

        }



        public async Task<EmployeeSalaryDetails> saveAndUpdateEmployeeSalaryDetails(EmployeeSalaryDetails salaryDetails)

        {

            if (salaryDetails.EMPFL_ID == 0)

            {

                _oBMSDbContext.EmployeeSalaryDetails.Add(salaryDetails);

            }

            else

            {

                _oBMSDbContext.EmployeeSalaryDetails.Update(salaryDetails);

            }



            await _oBMSDbContext.SaveChangesAsync();



            return salaryDetails;

            throw new NotImplementedException();

        }

        // ─────────────────────────────────────────────────────────────────────────
        // Private history insertion helper
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Inserts a single row into EmployeeHistory as a combined snapshot.
        /// Emp_StartDate marks the start of service at this branch;
        /// Emp_EndDate = null means the row is currently active.
        /// </summary>
        private async Task InsertEmployeeHistoryRow(
            Employee employee,
            EmploymentDetails employment,
            EmployeeSalaryDetails salaryDetails,
            DateTime empStartDate,
            DateTime? empEndDate)
        {
            var historyRow = new EmployeeHistory
            {
                EMP_ID                      = employee.EMP_ID,
                EMP_ROLE                    = employee.EMP_ROLE,
                EMP_CODE                    = employee.EMP_CODE,
                EMP_NAME                    = employee.EMP_NAME,
                EMP_ADDRESS1                = employee.EMP_ADDRESS1 ?? "",
                EMP_ADDRESS2                = employee.EMP_ADDRESS2 ?? "",
                EMP_POST_CODE               = employee.EMP_POST_CODE ?? "",
                EMP_TOWN                    = employee.EMP_TOWN ?? "",
                EMP_STATE                   = employee.EMP_STATE ?? "",
                EMP_NATIONAL                = employee.EMP_NATIONAL ?? "",
                EMP_PHONE                   = employee.EMP_PHONE ?? "",
                EMP_HGH_EDU                 = employee.EMP_HGH_EDU ?? "",
                EM_WORK_EXP                 = employee.EM_WORK_EXP ?? "",
                EMP_DATE_OF_BIRTH           = employee.EMP_DATE_OF_BIRTH,
                EMP_IC_OLD                  = employee.EMP_IC_OLD ?? "",
                EMP_IC_NEW                  = employee.EMP_IC_NEW ?? "",
                EMP_IC_COLOR                = employee.EMP_IC_COLOR ?? "",
                EMP_PASSPORT_NO             = employee.EMP_PASSPORT_NO ?? "",
                EMP_SEX                     = employee.EMP_SEX ?? "",
                EMP_RACE                    = employee.EMP_RACE ?? "",
                EMP_MARTIAL_STATUS          = employee.EMP_MARTIAL_STATUS ?? "",
                EMP_SPOUSE_NAME             = employee.EMP_SPOUSE_NAME ?? "",
                EMP_SP_IC                   = employee.EMP_SP_IC ?? "",
                EMP_NO_CHILD                = employee.EMP_NO_CHILD,
                EMP_SP_WORK                 = employee.EMP_SP_WORK,
                EMP_PER_NAME_CONTACT        = employee.EMP_PER_NAME_CONTACT ?? "",
                EMP_CONTACT_ADDRESS1        = employee.EMP_CONTACT_ADDRESS1 ?? "",
                EMP_CONTACT_ADDRESS2        = employee.EMP_CONTACT_ADDRESS2 ?? "",
                EMP_CONTACT_POST_CODE       = employee.EMP_CONTACT_POST_CODE ?? "",
                EMP_CONTACT_TOWN            = employee.EMP_CONTACT_TOWN ?? "",
                EMP_CONTACT_STATE           = employee.EMP_CONTACT_STATE ?? "",
                EMP_CONTACT_TELEPHONE       = employee.EMP_CONTACT_TELEPHONE ?? "",
                EMP_BRANCH_CODE             = employee.EMP_BRANCH_CODE ?? "",
                OldBranch                   = employee.OldBranch,
                TransferDate                = employee.TransferDate,
                HasTransfered               = employee.HasTransfered,
                EMP_MOBILEPHONE             = employee.EMP_MOBILEPHONE ?? "",
                EMP_CITIZEN                 = employee.EMP_CITIZEN,
                EMP_CHECKLIST               = employee.EMP_CHECKLIST,
                EMP_CLIENT                  = employee.EMP_CLIENT ?? "",
                KDNVetting                  = employee.KDNVetting,
                NewSalaryStructure          = employee.NewSalaryStructure,
                SalaryStructure1000_3h      = employee.SalaryStructure1000_3h,
                EMPPAY_JOB_TITLE            = employment.EMPPAY_JOB_TITLE ?? "",
                EMPPAY_CATEGORY             = employment.EMPPAY_CATEGORY ?? "",
                EMPPAY_DATE_JOINED          = employment.EMPPAY_DATE_JOINED,
                EMPPAY_DATE_CONFIRM         = employment.EMPPAY_DATE_CONFIRM,
                EMPPAY_DATE_PROMOTION       = employment.EMPPAY_DATE_PROMOTION,
                EMPPAY_DATE_RESIGNED        = employment.EMPPAY_DATE_RESIGNED,
                EMPPAY_BASIC_RATE           = employment.EMPPAY_BASIC_RATE,
                SALARYLAB                   = employment.SALARYLAB,
                ATTENDANCEALLOWANCE         = employment.ATTENDANCEALLOWANCE,
                NewStructureATTENDANCEALLOWANCE = employment.NewStructureATTENDANCEALLOWANCE,
                SpecialAllowance            = employment.SpecialAllowance,
                AttendanceAllowanceWorkingDays = employment.AttendanceAllowanceWorkingDays,
                AttendanceAllowanceFollowCalendar = employment.AttendanceAllowanceFollowCalendar,
                EMPFL_BANK                  = salaryDetails?.EMPFL_BANK,
                EMPFL_BK_ACCNO              = salaryDetails?.EMPFL_BK_ACCNO,
                EMPFL_TAX_NO                = salaryDetails?.EMPFL_TAX_NO,
                EMPFL_EPFNO                 = salaryDetails?.EMPFL_EPFNO,
                EMPFL_EPF8Pa                = salaryDetails?.EMPFL_EPF8Pa,
                EMPFL_SOSCO_NO              = salaryDetails?.EMPFL_SOSCO_NO,
                EPFDETECT                   = salaryDetails?.EPFDETECT ?? false,
                PAYMODE                     = salaryDetails?.PAYMODE ?? "Bank",
                SOCSODETECT                 = salaryDetails?.SOCSODETECT ?? false,
                TMPGUARD                    = salaryDetails?.TMPGUARD ?? false,
                DETECTBYND55                = salaryDetails?.DETECTBYND55 ?? false,
                INCOMETAXDETECT             = salaryDetails?.INCOMETAXDETECT,
                LASTUPDATE                  = employee.LASTUPDATE,
                LastUpdatedBy               = employee.LastUpdatedBy ?? "admin",
                Emp_StartDate               = empStartDate,
                Emp_EndDate                 = empEndDate
            };

            _oBMSDbContext.EmployeeHistories.Add(historyRow);
            await _oBMSDbContext.SaveChangesAsync();
        }


        public async Task<Employee> UpdateEmployeeTransfer(EmployeeTransferDto employeeTransferDto)

        {

            var localBranchCode = "";

            var employee = _oBMSDbContext.Employees.Where(x => x.EMP_ID == employeeTransferDto.EMP_ID).FirstOrDefault();



            localBranchCode = employee.EMP_BRANCH_CODE;

            employee.EMP_BRANCH_CODE = employeeTransferDto.TO_BRANCH_ID;

            employee.OldBranch = localBranchCode;

            employee.HasTransfered = true;

            employee.TransferDate = employeeTransferDto.TRANSFER_DATE;



            var employment = _oBMSDbContext.EmploymentDetails.Where(x => x.EMPPAY_CODE == employee.EMP_CODE).FirstOrDefault();



            var salaryDetails = _oBMSDbContext.EmployeeSalaryDetails.Where(x => x.EMPFL_CODE == employee.EMP_CODE).FirstOrDefault();



            _oBMSDbContext.Employees.Update(employee);



            // Update employment details if they exist

            if (employment != null)

            {

                employment.EMPPAY_CODE = employee.EMP_CODE;

                _oBMSDbContext.EmploymentDetails.Update(employment);

            }



            // Update salary details if they exist  

            if (salaryDetails != null)

            {

                salaryDetails.EMPFL_CODE = employee.EMP_CODE;

                _oBMSDbContext.EmployeeSalaryDetails.Update(salaryDetails);

            }



            await _oBMSDbContext.SaveChangesAsync();

            // Close the currently open EmployeeHistory row before inserting the new one
            var openHistory = await _oBMSDbContext.EmployeeHistories
                .Where(h => h.EMP_ID == employee.EMP_ID && h.Emp_EndDate == null)
                .OrderByDescending(h => h.EMP_HISTORY_ID)
                .FirstOrDefaultAsync();

            if (openHistory != null)
            {
                openHistory.Emp_EndDate = employeeTransferDto.TRANSFER_DATE.AddDays(-1);
                _oBMSDbContext.EmployeeHistories.Update(openHistory);
                await _oBMSDbContext.SaveChangesAsync();
            }

            var employeeHistory = new EmployeeHistory()

            {



                EMP_HISTORY_ID = 0,

                EMP_ID = employee.EMP_ID,

                EMP_ROLE = employee.EMP_ROLE,

                EMP_CODE = employee.EMP_CODE,

                EMP_NAME = employee.EMP_NAME,

                EMP_ADDRESS1 = employee.EMP_ADDRESS1,

                EMP_ADDRESS2 = employee.EMP_ADDRESS2,

                EMP_POST_CODE = employee.EMP_POST_CODE,

                EMP_TOWN = employee.EMP_TOWN,

                EMP_STATE = employee.EMP_STATE,

                EMP_NATIONAL = employee.EMP_NATIONAL,

                EMP_PHONE = employee.EMP_PHONE,

                EMP_HGH_EDU = employee.EMP_HGH_EDU,

                EM_WORK_EXP = employee.EM_WORK_EXP,

                EMP_DATE_OF_BIRTH = employee.EMP_DATE_OF_BIRTH,

                EMP_IC_OLD = employee.EMP_IC_OLD,

                EMP_IC_NEW = employee.EMP_IC_NEW,

                EMP_IC_COLOR = employee.EMP_IC_COLOR,

                EMP_PASSPORT_NO = employee.EMP_PASSPORT_NO,

                EMP_SEX = employee.EMP_SEX,

                EMP_RACE = employee.EMP_RACE,

                EMP_MARTIAL_STATUS = employee.EMP_MARTIAL_STATUS,

                EMP_SPOUSE_NAME = employee.EMP_SPOUSE_NAME,

                EMP_SP_IC = employee.EMP_SP_IC,

                EMP_NO_CHILD = employee.EMP_NO_CHILD,

                EMP_SP_WORK = employee.EMP_SP_WORK,

                EMP_PER_NAME_CONTACT = employee.EMP_PER_NAME_CONTACT,

                EMP_CONTACT_ADDRESS1 = employee.EMP_CONTACT_ADDRESS1,

                EMP_CONTACT_ADDRESS2 = employee.EMP_CONTACT_ADDRESS2,

                EMP_CONTACT_POST_CODE = employee.EMP_CONTACT_POST_CODE,

                EMP_CONTACT_TOWN = employee.EMP_CONTACT_TOWN,

                EMP_CONTACT_STATE = employee.EMP_CONTACT_STATE,

                EMP_CONTACT_TELEPHONE = employee.EMP_CONTACT_TELEPHONE,

                EMP_BRANCH_CODE = employeeTransferDto.TO_BRANCH_ID,

                OldBranch = localBranchCode,

                TransferDate = null,

                HasTransfered = false,

                LASTUPDATE = employee.LASTUPDATE,

                LastUpdatedBy = employee.LastUpdatedBy ?? "",

                EMP_MOBILEPHONE = employee.EMP_MOBILEPHONE,

                EMP_CITIZEN = employee.EMP_CITIZEN,

                EMP_CHECKLIST = employee.EMP_CHECKLIST,

                EMP_CLIENT = employee.EMP_CLIENT,

                NewSalaryStructure = employee.NewSalaryStructure,

                KDNVetting = employee.KDNVetting,

                SalaryStructure1000_3h = employee.SalaryStructure1000_3h,



                EMPPAY_JOB_TITLE = employment?.EMPPAY_JOB_TITLE ?? "",

                EMPPAY_CATEGORY = employment?.EMPPAY_CATEGORY ?? "",

                EMPPAY_DATE_JOINED = employment?.EMPPAY_DATE_JOINED,

                EMPPAY_DATE_CONFIRM = employment?.EMPPAY_DATE_CONFIRM,

                EMPPAY_DATE_RESIGNED = employment?.EMPPAY_DATE_RESIGNED,

                EMPPAY_BASIC_RATE = employment?.EMPPAY_BASIC_RATE ?? 0.0,

                SALARYLAB = employment?.SALARYLAB ?? 0,

                ATTENDANCEALLOWANCE = employment?.ATTENDANCEALLOWANCE,

                NewStructureATTENDANCEALLOWANCE = 0,

                SpecialAllowance = employment?.SpecialAllowance ?? 0,

                AttendanceAllowanceWorkingDays = employment?.AttendanceAllowanceWorkingDays,

                AttendanceAllowanceFollowCalendar = "N",



                EMPFL_BANK = salaryDetails?.EMPFL_BANK ?? "",

                EMPFL_BK_ACCNO = salaryDetails?.EMPFL_BK_ACCNO ?? "",

                EMPFL_TAX_NO = salaryDetails?.EMPFL_TAX_NO,

                EMPFL_EPFNO = salaryDetails?.EMPFL_EPFNO,

                EMPFL_EPF8Pa = salaryDetails?.EMPFL_EPF8Pa,

                EMPFL_SOSCO_NO = salaryDetails?.EMPFL_SOSCO_NO,

                EPFDETECT = salaryDetails?.EPFDETECT ?? false,

                PAYMODE = salaryDetails?.PAYMODE ?? "",

                SOCSODETECT = salaryDetails?.SOCSODETECT ?? false,

                TMPGUARD = salaryDetails?.TMPGUARD ?? false,

                DETECTBYND55 = salaryDetails?.DETECTBYND55 ?? false,

                INCOMETAXDETECT = salaryDetails?.INCOMETAXDETECT,

                // Set Emp_StartDate to the transfer date so this row has a valid period start
                Emp_StartDate   = employeeTransferDto.TRANSFER_DATE,
                Emp_EndDate     = null

            };



            _oBMSDbContext.EmployeeHistories.Add(employeeHistory);

            await _oBMSDbContext.SaveChangesAsync();



            return employee;



            throw new NotImplementedException();

        }



        #endregion





        public async Task<Object> CheckEmployeeInfo(string from, string data)

        {



            var results = new Dictionary<string, Object>();



            if (from == "NewIC")

            {





                var employeeId = _oBMSDbContext.Employees

    .Join(

        _oBMSDbContext.EmployeeSalaryDetails,

        employee => employee.EMP_CODE,

        salaryDetails => salaryDetails.EMPFL_CODE,

        (employee, salaryDetails) => new { Employee = employee, SalaryDetails = salaryDetails }

    )

    .Join(

        _oBMSDbContext.EmploymentDetails,

        data => data.SalaryDetails.EMPFL_CODE,

        employmentDetails => employmentDetails.EMPPAY_CODE,

        (data, employmentDetails) => new { Data = data, EmploymentDetails = employmentDetails }

    )

    .Where(

        result => result.Data.Employee.EMP_IC_NEW == data &&

                  result.Data.Employee.HasTransfered == false &&

                  result.EmploymentDetails.EMPPAY_DATE_RESIGNED == null

    )

    .Select(result => result.Data.Employee.EMP_ID)

    .FirstOrDefault();



                results.Add("EMP_ID", employeeId);

                results.Add("FROM", from);

                results.Add("MESSAGE", employeeId == 0 ? "success" : "Employee with the same New IC already exists in the database.");

                return results;



            }

            else if (from == "OldIC")

            {



                var employeeId = _oBMSDbContext.Employees

    .Join(

        _oBMSDbContext.EmployeeSalaryDetails,

        employee => employee.EMP_CODE,

        salaryDetails => salaryDetails.EMPFL_CODE,

        (employee, salaryDetails) => new { Employee = employee, SalaryDetails = salaryDetails }

    )

    .Join(

        _oBMSDbContext.EmploymentDetails,

        data => data.SalaryDetails.EMPFL_CODE,

        employmentDetails => employmentDetails.EMPPAY_CODE,

        (data, employmentDetails) => new { Data = data, EmploymentDetails = employmentDetails }

    )

    .Where(

        result => result.Data.Employee.EMP_IC_OLD == data &&

                  result.Data.Employee.HasTransfered == false &&

                  result.EmploymentDetails.EMPPAY_DATE_RESIGNED == null

    )

    .Select(result => result.Data.Employee.EMP_ID)

    .FirstOrDefault();



                results.Add("EMP_ID", employeeId);

                results.Add("FROM", from);

                results.Add("MESSAGE", employeeId == 0 ? "success" : "Employee with the same Old IC already exists in the database.");

                return results;

            }

            else if (from == "Passport")

            {

                var employeeId = _oBMSDbContext.Employees

    .Join(

        _oBMSDbContext.EmployeeSalaryDetails,

        employee => employee.EMP_CODE,

        salaryDetails => salaryDetails.EMPFL_CODE,

        (employee, salaryDetails) => new { Employee = employee, SalaryDetails = salaryDetails }

    )

    .Join(

        _oBMSDbContext.EmploymentDetails,

        data => data.SalaryDetails.EMPFL_CODE,

        employmentDetails => employmentDetails.EMPPAY_CODE,

        (data, employmentDetails) => new { Data = data, EmploymentDetails = employmentDetails }

    )

    .Where(

        result => result.Data.Employee.EMP_PASSPORT_NO == data &&

                  result.Data.Employee.HasTransfered == false &&

                  result.EmploymentDetails.EMPPAY_DATE_RESIGNED == null

    )

    .Select(result => result.Data.Employee.EMP_ID)

    .FirstOrDefault();

                results.Add("EMP_ID", employeeId);

                results.Add("FROM", from);

                results.Add("MESSAGE", employeeId == 0 ? "success" : "Employee with the same Passport already exists in the database.");

                return results;

            }

            else if (from == "EPFNo")

            {

                var employeeId = _oBMSDbContext.Employees

    .Join(

        _oBMSDbContext.EmployeeSalaryDetails,

        employee => employee.EMP_CODE,

        salaryDetails => salaryDetails.EMPFL_CODE,

        (employee, salaryDetails) => new { Employee = employee, SalaryDetails = salaryDetails }

    )

    .Join(

        _oBMSDbContext.EmploymentDetails,

        data => data.SalaryDetails.EMPFL_CODE,

        employmentDetails => employmentDetails.EMPPAY_CODE,

        (data, employmentDetails) => new { Data = data, EmploymentDetails = employmentDetails }

    )

    .Where(

        result => result.Data.SalaryDetails.EMPFL_EPFNO == data &&

                  result.Data.Employee.HasTransfered == false &&

                  result.EmploymentDetails.EMPPAY_DATE_RESIGNED == null

    )

    .Select(result => result.Data.Employee.EMP_ID)

    .FirstOrDefault();

                results.Add("EMP_ID", employeeId);

                results.Add("FROM", from);

                results.Add("MESSAGE", employeeId == 0 ? "success" : "Employee with the same EPF No already exists in the database.");

                return results;

            }

            else if (from == "SOCSONo")

            {

                var employeeId = _oBMSDbContext.Employees

    .Join(

        _oBMSDbContext.EmployeeSalaryDetails,

        employee => employee.EMP_CODE,

        salaryDetails => salaryDetails.EMPFL_CODE,

        (employee, salaryDetails) => new { Employee = employee, SalaryDetails = salaryDetails }

    )

    .Join(

        _oBMSDbContext.EmploymentDetails,

        data => data.SalaryDetails.EMPFL_CODE,

        employmentDetails => employmentDetails.EMPPAY_CODE,

        (data, employmentDetails) => new { Data = data, EmploymentDetails = employmentDetails }

    )

    .Where(

        result => result.Data.SalaryDetails.EMPFL_SOSCO_NO == data &&

                  result.Data.Employee.HasTransfered == false &&

                  result.EmploymentDetails.EMPPAY_DATE_RESIGNED == null

    )

    .Select(result => result.Data.Employee.EMP_ID)

    .FirstOrDefault();



                results.Add("EMP_ID", employeeId);

                results.Add("FROM", from);

                results.Add("MESSAGE", employeeId == 0 ? "success" : "Employee with the same SOCSO No already exists in the database.");

                return results;

            }

            else if (from == "BankAccount")

            {



                string[] bank = data.Split(new string[] { "//" }, StringSplitOptions.None);



                var employeeId = _oBMSDbContext.Employees

    .Join(

        _oBMSDbContext.EmployeeSalaryDetails,

        employee => employee.EMP_CODE,

        salaryDetails => salaryDetails.EMPFL_CODE,

        (employee, salaryDetails) => new { Employee = employee, SalaryDetails = salaryDetails }

    )

    .Join(

        _oBMSDbContext.EmploymentDetails,

        data => data.SalaryDetails.EMPFL_CODE,

        employmentDetails => employmentDetails.EMPPAY_CODE,

        (data, employmentDetails) => new { Data = data, EmploymentDetails = employmentDetails }

    )

    .Where(

        result => result.Data.SalaryDetails.EMPFL_BANK == bank[0] &&

                  result.Data.SalaryDetails.EMPFL_BK_ACCNO == bank[1] &&

                  result.Data.Employee.HasTransfered == false &&

                  result.EmploymentDetails.EMPPAY_DATE_RESIGNED == null

    )

    .Select(result => result.Data.Employee.EMP_ID)

    .FirstOrDefault();





                results.Add("EMP_ID", employeeId);

                results.Add("FROM", from);

                results.Add("MESSAGE", employeeId == 0 ? "success" : "Employee with the same Bank Account already exists in the database.");

                return results;

            }



            return null;



        }



        public async Task<List<EmployeeHistoryDto>> GetAllEmployeesWithHistory(string? branch = null)

        {

            var query = from h in _oBMSDbContext.EmployeeHistories

                        join e in _oBMSDbContext.Employees on h.EMP_ID equals e.EMP_ID

                        select new EmployeeHistoryDto

                        {

                            EMP_ID = e.EMP_ID,

                            EMP_NAME = e.EMP_NAME,

                            EMP_CODE = e.EMP_CODE,

                            HasTransfered = e.HasTransfered,

                            TransferDate = e.TransferDate,

                            EMP_HISTORY_ID = h.EMP_HISTORY_ID,

                            EMP_ROLE = h.EMP_ROLE,

                            EMPPAY_DATE_JOINED = h.EMPPAY_DATE_JOINED,

                            EMPPAY_DATE_RESIGNED = h.EMPPAY_DATE_RESIGNED,

                            EMP_BRANCH_CODE = h.EMP_BRANCH_CODE,

                            OldBranch = h.OldBranch

                        };



            if (!string.IsNullOrEmpty(branch))

            {

                query = query.Where(x => x.EMP_BRANCH_CODE == branch);

            }



            return await query.ToListAsync();

        }



        // Indian Compliance Methods Implementation

        public async Task<Dictionary<string, object>> CreateEmployee(EmployeeCreateDto employeeDto)

        {

            var result = new Dictionary<string, object>();



            try

            {

                // Check for existing PAN

                if (!string.IsNullOrWhiteSpace(employeeDto.PANNumber))

                {

                    var existingPAN = await _oBMSDbContext.Employees

                        .Where(x => x.PANNumber == employeeDto.PANNumber)

                        .FirstOrDefaultAsync();



                    if (existingPAN != null)

                    {

                        result.Add("Success", "Error");

                        result.Add("Message", "PAN number already exists");

                        return result;

                    }

                }



                // Check for existing Aadhaar

                if (!string.IsNullOrWhiteSpace(employeeDto.AadhaarNumber))

                {

                    var existingAadhaar = await _oBMSDbContext.Employees

                        .Where(x => x.AadhaarNumber == employeeDto.AadhaarNumber)

                        .FirstOrDefaultAsync();



                    if (existingAadhaar != null)

                    {

                        result.Add("Success", "Error");

                        result.Add("Message", "Aadhaar number already exists");

                        return result;

                    }

                }



                // Create Employee entity

                var employee = new Employee

                {

                    EMP_CODE = employeeDto.EMP_CODE,

                    EMP_NAME = employeeDto.EMP_NAME,

                    EMP_ROLE = employeeDto.EMP_ROLE,

                    EMP_ADDRESS1 = employeeDto.EMP_ADDRESS1,

                    EMP_ADDRESS2 = employeeDto.EMP_ADDRESS2,

                    EMP_POST_CODE = employeeDto.EMP_POST_CODE,

                    EMP_TOWN = employeeDto.EMP_TOWN ?? "Mumbai",

                    EMP_STATE = employeeDto.EMP_STATE,

                    EMP_PHONE = employeeDto.EMP_PHONE,

                    EMP_MOBILEPHONE = employeeDto.EMP_MOBILEPHONE,

                    EMP_HGH_EDU = employeeDto.EMP_HGH_EDU ?? "Graduate",

                    EM_WORK_EXP = employeeDto.EM_WORK_EXP ?? "0",

                    EMP_DATE_OF_BIRTH = employeeDto.EMP_DATE_OF_BIRTH,

                    EMP_SEX = employeeDto.EMP_SEX,

                    EMP_MARTIAL_STATUS = employeeDto.EMP_MARTIAL_STATUS ?? "Single",

                    EMP_SPOUSE_NAME = employeeDto.EMP_SPOUSE_NAME ?? "",

                    EMP_FATHER_NAME = employeeDto.EMP_FATHER_NAME,

                    EMP_NO_CHILD = employeeDto.EMP_NO_CHILD ?? 0,

                    EMP_SP_WORK = employeeDto.EMP_SP_WORK,

                    EMP_PER_NAME_CONTACT = employeeDto.EMP_MOBILEPHONE,

                    EMP_CONTACT_ADDRESS1 = employeeDto.EMP_CONTACT_ADDRESS1,

                    EMP_CONTACT_ADDRESS2 = employeeDto.EMP_CONTACT_ADDRESS2 ?? "",

                    EMP_CONTACT_POST_CODE = employeeDto.EMP_POST_CODE,

                    EMP_CONTACT_TOWN = employeeDto.EMP_TOWN ?? "Mumbai",

                    EMP_CONTACT_STATE = employeeDto.EMP_STATE,

                    EMP_CONTACT_TELEPHONE = employeeDto.EMP_PHONE,

                    EMP_BRANCH_CODE = employeeDto.EMP_BRANCH_CODE,

                    EMP_CITIZEN = employeeDto.EMP_CITIZEN,

                    EMP_CHECKLIST = employeeDto.EMP_CHECKLIST,

                    EMP_CLIENT = employeeDto.EMP_CLIENT ?? "",

                    NewSalaryStructure = employeeDto.NewSalaryStructure ?? 'N',

                    KDNVetting = employeeDto.KDNVetting,

                    SalaryStructure1000_3h = employeeDto.SalaryStructure1000_3h ?? 'N',

                    LASTUPDATE = DateTime.Now,

                    LastUpdatedBy = employeeDto.LastUpdatedBy ?? "admin",



                    // Indian Compliance Fields

                    AadhaarNumber = employeeDto.AadhaarNumber,

                    PANNumber = employeeDto.PANNumber,

                    PFAccountNumber = employeeDto.PFAccountNumber ?? "",

                    ESINumber = employeeDto.ESINumber ?? "",

                    SalaryGroup = employeeDto.SalaryGroup,

                    SpousePAN = employeeDto.SpousePAN ?? "",

                    SpouseAadhaar = employeeDto.SpouseAadhaar ?? "",

                    IndianState = employeeDto.IndianState,

                    BankAccountNumber = employeeDto.BankAccountNumber,

                    BankIFSC = employeeDto.BankIFSC,

                    BankName = employeeDto.BankName,

                    UPIId = employeeDto.UPIId ?? ""

                };



                // Create Employment Details

                var employment = new EmploymentDetails

                {

                    EMPPAY_CODE = employeeDto.EMP_CODE,

                    EMPPAY_BRANCHCODE = employeeDto.EMP_BRANCH_CODE,

                    EMPPAY_JOB_TITLE = employeeDto.EMPPAY_JOB_TITLE ?? "Security Guard",

                    EMPPAY_CATEGORY = employeeDto.EMPPAY_CATEGORY ?? "Guard",

                    EMPPAY_DATE_JOINED = employeeDto.EMPPAY_DATE_JOINED ?? DateTime.Now,

                    EMPPAY_DATE_CONFIRM = employeeDto.EMPPAY_DATE_CONFIRM ?? (DateTime?)null,

                    EMPPAY_DATE_RESIGNED = employeeDto.EMPPAY_DATE_RESIGNED ?? (DateTime?)null,

                    EMPPAY_BASIC_RATE = employeeDto.EMPPAY_BASIC_RATE > 0 ? employeeDto.EMPPAY_BASIC_RATE : 20000.0,

                    SALARYLAB = 8.0m, // Default 8 hours

                    ATTENDANCEALLOWANCE = employeeDto.ATTENDANCEALLOWANCE ?? 2000.0m,

                    NewStructureATTENDANCEALLOWANCE = 0,

                    SpecialAllowance = employeeDto.SpecialAllowance > 0 ? employeeDto.SpecialAllowance : 1000.0m,

                    AttendanceAllowanceWorkingDays = employeeDto.AttendanceAllowanceWorkingDays ?? 26.0m,

                    AttendanceAllowanceFollowCalendar = "N",

                    LASTUPDATE = DateTime.Now,

                    LastUpdatedBy = employeeDto.LastUpdatedBy ?? "admin"

                };



                // Create Salary Details

                var salaryDetails = new EmployeeSalaryDetails

                {

                    EMPFL_CODE = employeeDto.EMP_CODE,

                    EMPFL_BRANCHCODE = employeeDto.EMP_BRANCH_CODE,

                    EMPFL_BANK = employeeDto.BankName,

                    EMPFL_BK_ACCNO = employeeDto.BankAccountNumber,

                    EMPFL_TAX_NO = employeeDto.PANNumber,

                    EMPFL_EPFNO = employeeDto.PFAccountNumber,

                    EMPFL_EPF8Pa = false, // Default value

                    EMPFL_SOSCO_NO = employeeDto.ESINumber,

                    EPFDETECT = false, // Default value

                    PAYMODE = "Bank Transfer", // Default value

                    SOCSODETECT = false, // Default value

                    TMPGUARD = false, // Default value

                    DETECTBYND55 = false, // Default value

                    INCOMETAXDETECT = false, // Default value

                    EMP_SP_TEL_NO = employeeDto.EMP_MOBILEPHONE,

                    OT = employeeDto.OT ?? "No",

                    LASTUPDATE = DateTime.Now,

                    LastUpdatedBy = employeeDto.LastUpdatedBy ?? "admin"

                };



                await _oBMSDbContext.Employees.AddAsync(employee);

                await _oBMSDbContext.EmploymentDetails.AddAsync(employment);

                await _oBMSDbContext.EmployeeSalaryDetails.AddAsync(salaryDetails);



                try

                {

                    await _oBMSDbContext.SaveChangesAsync();

                }

                catch (DbUpdateException dbEx)

                {

                    var innerException = dbEx.InnerException?.InnerException?.Message ?? dbEx.InnerException?.Message ?? dbEx.Message;

                    result.Add("Success", "Error");

                    result.Add("Message", $"Database error: {innerException}");

                    return result;

                }



                result.Add("Success", "Success");

                result.Add("Employee", employee);

                result.Add("Message", "Employee created successfully");



                return result;

            }

            catch (Exception ex)

            {

                result.Add("Success", "Error");

                result.Add("Message", $"Error creating employee: {ex.Message}");

                return result;

            }

        }



        public async Task<Dictionary<string, object>> UpdateEmployee(EmployeeUpdateDto employeeDto)

        {

            var result = new Dictionary<string, object>();



            try

            {

                var employee = await _oBMSDbContext.Employees

                    .Where(x => x.EMP_ID == employeeDto.EMP_ID)

                    .FirstOrDefaultAsync();



                if (employee == null)

                {

                    result.Add("Success", "Error");

                    result.Add("Message", "Employee not found");

                    return result;

                }



                // Check for existing PAN (excluding current employee)

                if (!string.IsNullOrWhiteSpace(employeeDto.PANNumber))

                {

                    var existingPAN = await _oBMSDbContext.Employees

                        .Where(x => x.PANNumber == employeeDto.PANNumber && x.EMP_ID != employeeDto.EMP_ID)

                        .FirstOrDefaultAsync();



                    if (existingPAN != null)

                    {

                        result.Add("Success", "Error");

                        result.Add("Message", "PAN number already exists");

                        return result;

                    }

                }



                // Check for existing Aadhaar (excluding current employee)

                if (!string.IsNullOrWhiteSpace(employeeDto.AadhaarNumber))

                {

                    var existingAadhaar = await _oBMSDbContext.Employees

                        .Where(x => x.AadhaarNumber == employeeDto.AadhaarNumber && x.EMP_ID != employeeDto.EMP_ID)

                        .FirstOrDefaultAsync();



                    if (existingAadhaar != null)

                    {

                        result.Add("Success", "Error");

                        result.Add("Message", "Aadhaar number already exists");

                        return result;

                    }

                }



                // Update employee fields

                employee.EMP_NAME = employeeDto.EMP_NAME;

                employee.EMP_ROLE = employeeDto.EMP_ROLE;

                employee.EMP_ADDRESS1 = employeeDto.EMP_ADDRESS1;

                employee.EMP_ADDRESS2 = employeeDto.EMP_ADDRESS2;

                employee.EMP_POST_CODE = employeeDto.EMP_POST_CODE;

                employee.EMP_TOWN = employeeDto.EMP_TOWN;

                employee.EMP_STATE = employeeDto.EMP_STATE;

                employee.EMP_PHONE = employeeDto.EMP_PHONE;

                employee.EMP_MOBILEPHONE = employeeDto.EMP_MOBILEPHONE;

                employee.EMP_HGH_EDU = employeeDto.EMP_HGH_EDU;

                employee.EM_WORK_EXP = employeeDto.EM_WORK_EXP;

                employee.EMP_DATE_OF_BIRTH = employeeDto.EMP_DATE_OF_BIRTH;

                employee.EMP_SEX = employeeDto.EMP_SEX;

                employee.EMP_MARTIAL_STATUS = employeeDto.EMP_MARTIAL_STATUS;

                employee.EMP_SPOUSE_NAME = employeeDto.EMP_SPOUSE_NAME;

                employee.EMP_FATHER_NAME = employeeDto.EMP_FATHER_NAME;

                employee.EMP_NO_CHILD = employeeDto.EMP_NO_CHILD;

                employee.EMP_SP_WORK = employeeDto.EMP_SP_WORK;

                employee.EMP_PER_NAME_CONTACT = employeeDto.EMP_PER_NAME_CONTACT;

                employee.EMP_CONTACT_ADDRESS1 = employeeDto.EMP_CONTACT_ADDRESS1;

                employee.EMP_CONTACT_ADDRESS2 = employeeDto.EMP_CONTACT_ADDRESS2;

                employee.EMP_CONTACT_POST_CODE = employeeDto.EMP_CONTACT_POST_CODE;

                employee.EMP_CONTACT_TOWN = employeeDto.EMP_CONTACT_TOWN;

                employee.EMP_CONTACT_STATE = employeeDto.EMP_CONTACT_STATE;

                employee.EMP_CONTACT_TELEPHONE = employeeDto.EMP_CONTACT_TELEPHONE;

                employee.EMP_BRANCH_CODE = employeeDto.EMP_BRANCH_CODE;

                employee.OldBranch = employeeDto.OldBranch;

                employee.TransferDate = employeeDto.TransferDate;

                employee.HasTransfered = employeeDto.HasTransfered;

                employee.EMP_CITIZEN = employeeDto.EMP_CITIZEN;

                employee.EMP_CHECKLIST = employeeDto.EMP_CHECKLIST;

                employee.EMP_CLIENT = employeeDto.EMP_CLIENT;

                employee.NewSalaryStructure = employeeDto.NewSalaryStructure;

                employee.KDNVetting = employeeDto.KDNVetting;

                employee.SalaryStructure1000_3h = employeeDto.SalaryStructure1000_3h;

                employee.LASTUPDATE = employeeDto.LASTUPDATE;

                employee.LastUpdatedBy = employeeDto.LastUpdatedBy;



                // Update Indian Compliance Fields

                employee.AadhaarNumber = employeeDto.AadhaarNumber;

                employee.PANNumber = employeeDto.PANNumber;

                employee.PFAccountNumber = employeeDto.PFAccountNumber;

                employee.ESINumber = employeeDto.ESINumber;

                employee.SalaryGroup = employeeDto.SalaryGroup;

                employee.SpousePAN = employeeDto.SpousePAN;

                employee.SpouseAadhaar = employeeDto.SpouseAadhaar;

                employee.IndianState = employeeDto.IndianState;

                employee.BankAccountNumber = employeeDto.BankAccountNumber;

                employee.BankIFSC = employeeDto.BankIFSC;

                employee.BankName = employeeDto.BankName;

                employee.UPIId = employeeDto.UPIId;



                // Update Commercial Breakdown (CB) fields

                employee.CB_Basic = employeeDto.CB_Basic;

                employee.CB_DA = employeeDto.CB_DA;

                employee.CB_HRA = employeeDto.CB_HRA;

                employee.CB_HRAPercentage = employeeDto.CB_HRAPercentage;

                employee.CB_Leaves = employeeDto.CB_Leaves;

                employee.CB_LeavesPercentage = employeeDto.CB_LeavesPercentage;

                employee.CB_OtherAllowances = employeeDto.CB_OtherAllowances;

                employee.CB_NH = employeeDto.CB_NH;

                employee.CB_NHPercentage = employeeDto.CB_NHPercentage;

                employee.CB_SubTotal = employeeDto.CB_SubTotal;



                // Update EmploymentDetails if provided

                var employment = await _oBMSDbContext.EmploymentDetails

                    .Where(x => x.EMPPAY_CODE == employee.EMP_CODE)

                    .FirstOrDefaultAsync();



                if (employment != null)

                {

                    // If Commercial Breakdown is used (CB_Basic > 0), use CB_Basic as EMPPAY_BASIC_RATE

                    // This ensures backward compatibility with existing system

                    if (employeeDto.CB_Basic.HasValue && employeeDto.CB_Basic > 0)

                    {

                        employment.EMPPAY_BASIC_RATE = (double)employeeDto.CB_Basic;

                    }

                    else

                    {

                        // Otherwise use the provided EMPPAY_BASIC_RATE (from salary slab)

                        employment.EMPPAY_BASIC_RATE = employeeDto.EMPPAY_BASIC_RATE;

                    }



                    employment.EMPPAY_JOB_TITLE = employeeDto.EMPPAY_JOB_TITLE;

                    employment.EMPPAY_CATEGORY = employeeDto.EMPPAY_CATEGORY;

                    employment.EMPPAY_DATE_JOINED = employeeDto.EMPPAY_DATE_JOINED ?? employment.EMPPAY_DATE_JOINED;

                    employment.EMPPAY_DATE_CONFIRM = employeeDto.EMPPAY_DATE_CONFIRM;

                    employment.EMPPAY_DATE_RESIGNED = employeeDto.EMPPAY_DATE_RESIGNED;

                    employment.SALARYLAB = employeeDto.SALARYLAB;

                    employment.ATTENDANCEALLOWANCE = employeeDto.ATTENDANCEALLOWANCE;

                    employment.SpecialAllowance = employeeDto.SpecialAllowance;

                    employment.AttendanceAllowanceWorkingDays = employeeDto.AttendanceAllowanceWorkingDays;

                    employment.AttendanceAllowanceFollowCalendar = "N";

                    employment.LASTUPDATE = employeeDto.LASTUPDATE;

                    employment.LastUpdatedBy = employeeDto.LastUpdatedBy;



                    _oBMSDbContext.EmploymentDetails.Update(employment);

                }



                // Update EmployeeSalaryDetails if provided

                var salaryDetails = await _oBMSDbContext.EmployeeSalaryDetails

                    .Where(x => x.EMPFL_CODE == employee.EMP_CODE)

                    .FirstOrDefaultAsync();



                if (salaryDetails != null)

                {

                    salaryDetails.EMPFL_BANK = employeeDto.EMPFL_BANK;

                    salaryDetails.EMPFL_BK_ACCNO = employeeDto.EMPFL_BK_ACCNO;

                    salaryDetails.EMPFL_TAX_NO = employeeDto.EMPFL_TAX_NO;

                    salaryDetails.EPFDETECT = employeeDto.EPFDETECT;

                    salaryDetails.PAYMODE = employeeDto.PAYMODE;

                    salaryDetails.SOCSODETECT = employeeDto.SOCSODETECT;

                    salaryDetails.TMPGUARD = employeeDto.TMPGUARD;

                    salaryDetails.DETECTBYND55 = employeeDto.DETECTBYND55;

                    salaryDetails.INCOMETAXDETECT = employeeDto.INCOMETAXDETECT;

                    salaryDetails.EMP_SP_TEL_NO = employeeDto.EMP_SP_TEL_NO;

                    salaryDetails.OT = employeeDto.OT ?? "No";

                    salaryDetails.LASTUPDATE = employeeDto.LASTUPDATE;

                    salaryDetails.LastUpdatedBy = employeeDto.LastUpdatedBy;



                    _oBMSDbContext.EmployeeSalaryDetails.Update(salaryDetails);

                }



                await _oBMSDbContext.SaveChangesAsync();



                result.Add("Success", "Success");

                result.Add("Employee", employee);

                result.Add("Message", "Employee updated successfully");



                return result;

            }

            catch (Exception ex)

            {

                result.Add("Success", "Error");

                result.Add("Message", $"Error updating employee: {ex.Message}");

                return result;

            }

        }



        public async Task<List<EmployeeSearchResultDto>> SearchEmployees(EmployeeSearchDto searchDto)

        {

            var query = from emp in _oBMSDbContext.Employees

                        join empDetails in _oBMSDbContext.EmploymentDetails

                            on emp.EMP_CODE equals empDetails.EMPPAY_CODE into empGroup

                        from details in empGroup.DefaultIfEmpty()

                        where searchDto.EmployeeId == null || emp.EMP_ID == searchDto.EmployeeId

                        where string.IsNullOrEmpty(searchDto.EmployeeCode) || emp.EMP_CODE.Contains(searchDto.EmployeeCode)

                        where string.IsNullOrEmpty(searchDto.EmployeeName) || emp.EMP_NAME.Contains(searchDto.EmployeeName)

                        where string.IsNullOrEmpty(searchDto.AadhaarNumber) || emp.AadhaarNumber == searchDto.AadhaarNumber

                        where string.IsNullOrEmpty(searchDto.PANNumber) || emp.PANNumber == searchDto.PANNumber

                        where string.IsNullOrEmpty(searchDto.MobileNumber) || emp.EMP_MOBILEPHONE == searchDto.MobileNumber

                        where string.IsNullOrEmpty(searchDto.BranchCode) || emp.EMP_BRANCH_CODE == searchDto.BranchCode

                        where string.IsNullOrEmpty(searchDto.State) || emp.EMP_STATE == searchDto.State

                        where string.IsNullOrEmpty(searchDto.SalaryGroup) || emp.SalaryGroup == searchDto.SalaryGroup

                        orderby emp.EMP_ID descending

                        select new EmployeeSearchResultDto

                        {

                            EMP_ID = emp.EMP_ID,

                            EMP_CODE = emp.EMP_CODE,

                            EMP_NAME = emp.EMP_NAME,

                            EMP_SEX = emp.EMP_SEX,

                            AadhaarNumber = emp.AadhaarNumber,

                            PANNumber = emp.PANNumber,

                            PFAccountNumber = emp.PFAccountNumber,

                            ESINumber = emp.ESINumber,

                            SalaryGroup = emp.SalaryGroup,

                            EMP_PHONE = emp.EMP_PHONE,

                            EMP_MOBILEPHONE = emp.EMP_MOBILEPHONE,

                            EMP_STATE = emp.EMP_STATE,

                            EMP_BRANCH_CODE = emp.EMP_BRANCH_CODE,

                            EMPPAY_DATE_JOINED = details != null ? details.EMPPAY_DATE_JOINED : null,

                            EMPPAY_CATEGORY = details != null ? details.EMPPAY_CATEGORY : null,

                            EMP_ROLE = emp.EMP_ROLE,

                            IsActive = details == null || details.EMPPAY_DATE_RESIGNED == null

                        };



            return await query.ToListAsync();

        }



        public async Task<Dictionary<string, object>> ValidateEmployeeField(EmployeeValidationDto validationDto)

        {

            var result = new Dictionary<string, object>();



            try

            {

                bool exists = false;

                string message = "";



                switch (validationDto.FieldType.ToLower())

                {

                    case "pan":

                        if (!string.IsNullOrWhiteSpace(validationDto.FieldValue))

                        {

                            exists = await _oBMSDbContext.Employees

                                .Where(x => x.PANNumber == validationDto.FieldValue &&

                                           (validationDto.ExcludeEmployeeId == null || x.EMP_ID != validationDto.ExcludeEmployeeId))

                                .AnyAsync();

                            message = exists ? "PAN number already exists" : "PAN number is available";

                        }

                        break;



                    case "aadhaar":

                        if (!string.IsNullOrWhiteSpace(validationDto.FieldValue))

                        {

                            exists = await _oBMSDbContext.Employees

                                .Where(x => x.AadhaarNumber == validationDto.FieldValue &&

                                           (validationDto.ExcludeEmployeeId == null || x.EMP_ID != validationDto.ExcludeEmployeeId))

                                .AnyAsync();

                            message = exists ? "Aadhaar number already exists" : "Aadhaar number is available";

                        }

                        break;



                    case "ifsc":

                        // Basic IFSC format validation

                        exists = !System.Text.RegularExpressions.Regex.IsMatch(validationDto.FieldValue, @"^[A-Z]{4}0[A-Z0-9]{6}$");

                        message = exists ? "Invalid IFSC format" : "Valid IFSC format";

                        break;



                    case "phone":

                        // Basic Indian phone format validation

                        exists = !System.Text.RegularExpressions.Regex.IsMatch(validationDto.FieldValue, @"^\+91[6-9]\d{9}$");

                        message = exists ? "Invalid phone format" : "Valid phone format";

                        break;



                    default:

                        message = "Unknown field type";

                        break;

                }



                result.Add("Exists", exists);

                result.Add("Message", message);

                result.Add("FieldType", validationDto.FieldType);

                result.Add("FieldValue", validationDto.FieldValue);



                return result;

            }

            catch (Exception ex)

            {

                result.Add("Exists", true);

                result.Add("Message", $"Validation error: {ex.Message}");

                return result;

            }

        }



        public async Task<object> GetEmployeeByPAN(string pan)

        {

            var employee = await _oBMSDbContext.Employees

                .Where(x => x.PANNumber == pan)

                .Select(emp => new

                {

                    emp.EMP_ID,

                    emp.EMP_CODE,

                    emp.EMP_NAME,

                    emp.AadhaarNumber,

                    emp.PANNumber,

                    emp.EMP_PHONE,

                    emp.EMP_MOBILEPHONE,

                    emp.EMP_STATE,

                    emp.EMP_BRANCH_CODE

                })

                .FirstOrDefaultAsync();



            return employee;

        }



        public async Task<object> GetEmployeeByAadhaar(string aadhaar)

        {

            var employee = await _oBMSDbContext.Employees

                .Where(x => x.AadhaarNumber == aadhaar)

                .Select(emp => new

                {

                    emp.EMP_ID,

                    emp.EMP_CODE,

                    emp.EMP_NAME,

                    emp.AadhaarNumber,

                    emp.PANNumber,

                    emp.EMP_PHONE,

                    emp.EMP_MOBILEPHONE,

                    emp.EMP_STATE,

                    emp.EMP_BRANCH_CODE

                })

                .FirstOrDefaultAsync();



            return employee;

        }





        public async Task<bool> DeleteEmployee(int id)

        {

            var employee = await _oBMSDbContext.Employees.FirstOrDefaultAsync(x => x.EMP_ID == id);

            if (employee == null) return false;



            // Delete related details based on EMP_CODE

            var employmentDetails = await _oBMSDbContext.EmploymentDetails.Where(x => x.EMPPAY_CODE == employee.EMP_CODE).ToListAsync();

            if (employmentDetails.Any())

            {

                _oBMSDbContext.EmploymentDetails.RemoveRange(employmentDetails);

            }



            var salaryDetails = await _oBMSDbContext.EmployeeSalaryDetails.Where(x => x.EMPFL_CODE == employee.EMP_CODE).ToListAsync();

            if (salaryDetails.Any())

            {

                _oBMSDbContext.EmployeeSalaryDetails.RemoveRange(salaryDetails);

            }



            // Finally delete the employee

            _oBMSDbContext.Employees.Remove(employee);



            await _oBMSDbContext.SaveChangesAsync();

            return true;

        }



        #region Excel Import Methods



        public async Task<EmployeeImportPreviewDto> PreviewExcelImportAsync(IFormFile file)

        {

            var result = new EmployeeImportPreviewDto();

            var validRecords = new List<EmployeeImportPreviewDto>();

            var validationErrors = new List<EmployeeImportError>();



            using (var stream = new MemoryStream())

            {

                await file.CopyToAsync(stream);

                stream.Position = 0;



                using (var workbook = new ClosedXML.Excel.XLWorkbook(stream))

                {

                    var worksheet = workbook.Worksheet(1);

                    var rows = worksheet.RowsUsed().Skip(1); // Skip header row



                    Console.WriteLine($"=== EXCEL IMPORT DEBUG ===");

                    Console.WriteLine($"File: {file.FileName}");

                    Console.WriteLine($"Total rows found (excluding header): {rows.Count()}");

                    Console.WriteLine($"===========================");



                    int rowNumber = 2;

                    foreach (var row in rows)

                    {

                        try

                        {

                            var employeeDto = ParseExcelRow(row, rowNumber);



                            // Auto-generate EMP_CODE if not provided (for preview)

                            if (string.IsNullOrWhiteSpace(employeeDto.EMP_CODE))

                            {

                                var empNo = await GetEmployeeNo();

                                string shortName = empNo["ShortName"];

                                string code = empNo["Code"];

                                employeeDto.EMP_CODE = $"{shortName}{employeeDto.EMP_ROLE?.FirstOrDefault() ?? 'E'}{code}";

                            }



                            Console.WriteLine($"Row {rowNumber}: EMP_CODE='{employeeDto.EMP_CODE}', EMP_NAME='{employeeDto.EMP_NAME}', EMP_ROLE='{employeeDto.EMP_ROLE}', EMP_BRANCH_CODE='{employeeDto.EMP_BRANCH_CODE}', EMP_SEX='{employeeDto.EMP_SEX}'");



                            // Validate the record

                            var validationResults = await ValidateEmployeeImportDto(employeeDto);



                            if (validationResults.Any())

                            {

                                Console.WriteLine($"Row {rowNumber} VALIDATION FAILED: {string.Join(", ", validationResults.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

                                foreach (var error in validationResults)

                                {

                                    validationErrors.Add(new EmployeeImportError

                                    {

                                        RowNumber = rowNumber,

                                        EmployeeCode = employeeDto.EMP_CODE ?? "Unknown",

                                        EmployeeName = employeeDto.EMP_NAME ?? "Unknown",

                                        FieldName = error.Key,

                                        ErrorMessage = error.Value

                                    });

                                }

                            }

                            else

                            {

                                Console.WriteLine($"Row {rowNumber} VALIDATION PASSED");

                                validRecords.Add(new EmployeeImportPreviewDto

                                {

                                    Employee = MapEmployeeDtoToDomain(employeeDto),

                                    ValidationMessage = "Valid record",

                                    IsValid = true,

                                    RowNumber = rowNumber

                                });

                            }

                        }

                        catch (Exception ex)

                        {

                            Console.WriteLine($"Row {rowNumber} PARSE ERROR: {ex.Message}");

                            validationErrors.Add(new EmployeeImportError

                            {

                                RowNumber = rowNumber,

                                EmployeeCode = "Parse Error",

                                EmployeeName = "Parse Error",

                                FieldName = "General",

                                ErrorMessage = $"Error parsing row: {ex.Message}"

                            });

                        }

                        rowNumber++;

                    }



                    Console.WriteLine($"=== IMPORT SUMMARY ===");

                    Console.WriteLine($"Valid records: {validRecords.Count}");

                    Console.WriteLine($"Invalid records: {validationErrors.Count}");

                    Console.WriteLine($"====================");

                }

            }



            result.ValidRecords = validRecords;

            result.ValidationErrors = validationErrors;

            result.TotalRecords = validRecords.Count + validationErrors.Count;

            result.ValidCount = validRecords.Count;

            result.InvalidCount = validationErrors.Count;



            return result;

        }



        public async Task<EmployeeImportResultDto> BulkImportEmployeesAsync(EmployeeBulkImportRequestDto request)

        {

            Console.WriteLine("=== BULK IMPORT START ===");

            Console.WriteLine($"Total employees: {request.Employees?.Count ?? 0}");

            Console.WriteLine($"UpdateExisting: {request.UpdateExisting}");

            Console.WriteLine($"ImportedBy: {request.ImportedBy}");

            Console.WriteLine("========================");



            // Suspend execution strategy for manual transaction

            var strategy = _oBMSDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>

            {

                var result = new EmployeeImportResultDto();

                var errors = new List<EmployeeImportError>();

                var successfulImports = new List<EmployeeImportSuccess>();



                using var transaction = await _oBMSDbContext.Database.BeginTransactionAsync();

                try

                {



                    foreach (var previewRecord in request.Employees)

                    {

                        try

                        {

                            // Extract employee data from preview record

                            var employee = previewRecord.Employee;

                            if (employee == null)

                            {

                                errors.Add(new EmployeeImportError

                                {

                                    RowNumber = previewRecord.RowNumber,

                                    EmployeeCode = "Unknown",

                                    EmployeeName = "Unknown",

                                    FieldName = "Employee",

                                    ErrorMessage = "Employee data is null in preview record"

                                });

                                continue;

                            }



                            // Collect all validation errors for this employee

                            var employeeErrors = new List<EmployeeImportError>();



                            // Check if employee exists by code

                            var existingEmployee = await _oBMSDbContext.Employees

                                .FirstOrDefaultAsync(x => x.EMP_CODE == employee.EMP_CODE);



                            if (existingEmployee != null && (request.UpdateExisting == false))

                            {

                                var errorMessage = "Employee code already exists. Enable 'Update Existing' to update.";

                                Console.WriteLine($"=== IMPORT ERROR ===");

                                Console.WriteLine($"Row {previewRecord.RowNumber}: {errorMessage}");

                                Console.WriteLine($"Employee Code: {employee.EMP_CODE}");

                                Console.WriteLine("==================");



                                employeeErrors.Add(new EmployeeImportError

                                {

                                    RowNumber = previewRecord.RowNumber,

                                    EmployeeCode = employee.EMP_CODE,

                                    EmployeeName = employee.EMP_NAME,

                                    FieldName = "EMP_CODE",

                                    ErrorMessage = errorMessage

                                });

                            }



                            // Check for duplicate Aadhaar

                            if (!string.IsNullOrWhiteSpace(employee.AadhaarNumber))

                            {

                                var existingAadhaar = await _oBMSDbContext.Employees

                                    .Where(x => x.AadhaarNumber == employee.AadhaarNumber)

                                    .Where(x => existingEmployee == null || x.EMP_ID != existingEmployee.EMP_ID)

                                    .FirstOrDefaultAsync();



                                if (existingAadhaar != null)

                                {

                                    var errorMessage = $"Aadhaar number already exists for employee: {existingAadhaar.EMP_CODE}";

                                    Console.WriteLine($"=== IMPORT ERROR ===");

                                    Console.WriteLine($"Row {previewRecord.RowNumber}: {errorMessage}");

                                    Console.WriteLine($"Aadhaar: {employee.AadhaarNumber}");

                                    Console.WriteLine("==================");



                                    employeeErrors.Add(new EmployeeImportError

                                    {

                                        RowNumber = previewRecord.RowNumber,

                                        EmployeeCode = employee.EMP_CODE,

                                        EmployeeName = employee.EMP_NAME,

                                        FieldName = "AadhaarNumber",

                                        ErrorMessage = errorMessage

                                    });

                                }

                            }



                            // Check for duplicate PAN

                            if (!string.IsNullOrWhiteSpace(employee.PANNumber))

                            {

                                var existingPAN = await _oBMSDbContext.Employees

                                    .Where(x => x.PANNumber == employee.PANNumber)

                                    .Where(x => existingEmployee == null || x.EMP_ID != existingEmployee.EMP_ID)

                                    .FirstOrDefaultAsync();



                                if (existingPAN != null)

                                {

                                    var errorMessage = $"PAN number already exists for employee: {existingPAN.EMP_CODE}";

                                    Console.WriteLine($"=== IMPORT ERROR ===");

                                    Console.WriteLine($"Row {previewRecord.RowNumber}: {errorMessage}");

                                    Console.WriteLine($"PAN: {employee.PANNumber}");

                                    Console.WriteLine("==================");



                                    employeeErrors.Add(new EmployeeImportError

                                    {

                                        RowNumber = previewRecord.RowNumber,

                                        EmployeeCode = employee.EMP_CODE,

                                        EmployeeName = employee.EMP_NAME,

                                        FieldName = "PANNumber",

                                        ErrorMessage = errorMessage

                                    });

                                }

                            }



                            // If there are any validation errors, add them all and skip this employee

                            if (employeeErrors.Any())

                            {

                                errors.AddRange(employeeErrors);

                                continue;

                            }



                            // Use resolved DepartmentId and DesignationId from validation step

                            int? departmentId = employee.DepartmentId;

                            int? designationId = employee.DesignationId;



                            Employee employeeToSave;

                            string action;



                            if (existingEmployee != null)

                            {

                                // Update existing employee

                                employeeToSave = existingEmployee;

                                action = "Updated";

                            }

                            else

                            {

                                // Create new employee

                                employeeToSave = new Employee();

                                employeeToSave.EMP_CODE = employee.EMP_CODE;

                                action = "Created";

                            }



                            // Map basic fields from the employee object

                            employeeToSave.EMP_NAME = employee.EMP_NAME ?? "";

                            employeeToSave.EMP_ROLE = employee.EMP_ROLE ?? "";

                            employeeToSave.EMP_BRANCH_CODE = employee.EMP_BRANCH_CODE ?? "";

                            employeeToSave.EMP_ADDRESS1 = employee.EMP_ADDRESS1 ?? "";

                            employeeToSave.EMP_ADDRESS2 = employee.EMP_ADDRESS2 ?? "";

                            employeeToSave.EMP_POST_CODE = employee.EMP_POST_CODE ?? "";

                            employeeToSave.EMP_TOWN = employee.EMP_TOWN ?? "";

                            employeeToSave.EMP_STATE = employee.EMP_STATE ?? "";

                            employeeToSave.EMP_NATIONAL = employee.EMP_NATIONAL ?? "";

                            employeeToSave.EMP_PHONE = employee.EMP_PHONE ?? "";

                            employeeToSave.EMP_MOBILEPHONE = employee.EMP_MOBILEPHONE ?? "";

                            employeeToSave.EMP_HGH_EDU = employee.EMP_HGH_EDU ?? "";

                            employeeToSave.EM_WORK_EXP = employee.EM_WORK_EXP ?? "";
                            employeeToSave.EMP_DATE_OF_BIRTH = employee.EMP_DATE_OF_BIRTH;

                            employeeToSave.EMP_SEX = employee.EMP_SEX ?? "";

                            employeeToSave.EMP_PASSPORT_NO = employee.EMP_PASSPORT_NO ?? "";

                            employeeToSave.EMP_RACE = employee.EMP_RACE; // Use EMP_RACE from Employee domain model

                            employeeToSave.EMP_MARTIAL_STATUS = employee.EMP_MARTIAL_STATUS ?? "";

                            employeeToSave.EMP_SPOUSE_NAME = employee.EMP_SPOUSE_NAME ?? "";

                            // ... (rest of the code remains the same)
                            employeeToSave.EMP_FATHER_NAME = employee.EMP_FATHER_NAME ?? "";

                            employeeToSave.EMP_NO_CHILD = employee.EMP_NO_CHILD ?? 0;

                            employeeToSave.EMP_PER_NAME_CONTACT = employee.EMP_MOBILEPHONE ?? "";

                            employeeToSave.EMP_CONTACT_ADDRESS1 = employee.EMP_ADDRESS1 ?? "";

                            employeeToSave.EMP_CONTACT_ADDRESS2 = employee.EMP_ADDRESS2 ?? "";

                            employeeToSave.EMP_CONTACT_POST_CODE = employee.EMP_POST_CODE ?? "";

                            employeeToSave.EMP_CONTACT_TOWN = employee.EMP_TOWN ?? "";

                            employeeToSave.EMP_CONTACT_STATE = employee.EMP_STATE ?? "";

                            employeeToSave.EMP_CONTACT_TELEPHONE = employee.EMP_PHONE ?? "";

                            employeeToSave.EMP_IC_OLD = employee.EMP_IC_OLD ?? "";

                            employeeToSave.EMP_IC_NEW = employee.AadhaarNumber ?? "";

                            employeeToSave.EMP_IC_COLOR = employee.EMP_IC_COLOR ?? "";

                            employeeToSave.EMP_CITIZEN = employee.EMP_CITIZEN;

                            employeeToSave.EMP_CHECKLIST = employee.EMP_CHECKLIST;

                            employeeToSave.KDNVetting = employee.KDNVetting;

                            employeeToSave.HasTransfered = employee.HasTransfered;

                            employeeToSave.EMP_SP_WORK = employee.EMP_SP_WORK;

                            employeeToSave.EMP_CLIENT = employee.EMP_CLIENT ?? "";

                            employeeToSave.EMP_BASIC_RATE = employee.EMP_BASIC_RATE;

                            employeeToSave.LASTUPDATE = DateTime.Now;

                            employeeToSave.LastUpdatedBy = request.ImportedBy;



                            // Indian Compliance Fields

                            employeeToSave.AadhaarNumber = employee.AadhaarNumber;

                            employeeToSave.PANNumber = employee.PANNumber;

                            employeeToSave.PFAccountNumber = employee.PFAccountNumber;

                            employeeToSave.ESINumber = employee.ESINumber;

                            employeeToSave.SalaryGroup = employee.SalaryGroup ?? "None";

                            employeeToSave.IndianState = employee.IndianState;

                            employeeToSave.BankAccountNumber = employee.BankAccountNumber;

                            employeeToSave.BankIFSC = employee.BankIFSC;

                            employeeToSave.BankName = employee.BankName;

                            employeeToSave.UPIId = employee.UPIId;



                            // CB (Cost Breakdown) Fields - Only 7 essential fields for salary breakdown

                            employeeToSave.CB_Basic = employee.CB_Basic;  // Basic Salary

                            employeeToSave.CB_DA = employee.CB_DA;  // Dearness Allowance

                            employeeToSave.CB_HRA = employee.CB_HRA;  // House Rent Allowance

                            employeeToSave.CB_Leaves = employee.CB_Leaves;  // Leave Wages

                            employeeToSave.CB_NH = employee.CB_NH;  // National Festival Allowance (NFH)

                            employeeToSave.CB_AdvanceStatutoryBonus = employee.CB_AdvanceStatutoryBonus;  // Advance Statutory Bonus

                            employeeToSave.CB_OtherAllowances = employee.CB_OtherAllowances;  // Other Allowances

                            employeeToSave.CB_SubTotal = employee.CB_SubTotal;  // Sub Total (calculated)



                            // Department and Designation

                            employeeToSave.DepartmentId = departmentId;

                            employeeToSave.DesignationId = designationId;



                            if (existingEmployee == null)

                            {

                                _oBMSDbContext.Employees.Add(employeeToSave);

                            }

                            else

                            {

                                _oBMSDbContext.Employees.Update(employeeToSave);

                            }



                            await _oBMSDbContext.SaveChangesAsync();



                            // Handle Employment Details

                            var employment = await _oBMSDbContext.EmploymentDetails

                                .FirstOrDefaultAsync(x => x.EMPPAY_CODE == employeeToSave.EMP_CODE);



                            if (employment == null)

                            {

                                employment = new EmploymentDetails();

                                employment.EMPPAY_CODE = employeeToSave.EMP_CODE;

                                employment.EMPPAY_BRANCHCODE = employeeToSave.EMP_BRANCH_CODE;

                            }



                            // Note: Employment details fields like EMPPAY_JOB_TITLE, EMPPAY_CATEGORY, etc.

                            // are not available in the Employee domain model. They need to be passed separately.

                            // For now, we'll set default values

                            employment.EMPPAY_JOB_TITLE = employee.EMP_ROLE ?? "";

                            employment.EMPPAY_CATEGORY = "General";

                            employment.EMPPAY_DATE_JOINED = DateTime.Now;

                            employment.EMPPAY_DATE_CONFIRM = null;

                            employment.EMPPAY_BASIC_RATE = 0;

                            employment.LASTUPDATE = DateTime.Now;

                            employment.LastUpdatedBy = request.ImportedBy;



                            if (employment.EMPPAY_ID == 0)

                            {

                                _oBMSDbContext.EmploymentDetails.Add(employment);

                            }

                            else

                            {

                                _oBMSDbContext.EmploymentDetails.Update(employment);

                            }



                            await _oBMSDbContext.SaveChangesAsync();



                            // Handle Salary Details

                            var salaryDetails = await _oBMSDbContext.EmployeeSalaryDetails

                                .FirstOrDefaultAsync(x => x.EMPFL_CODE == employeeToSave.EMP_CODE);



                            if (salaryDetails == null)

                            {

                                salaryDetails = new EmployeeSalaryDetails();

                                salaryDetails.EMPFL_CODE = employeeToSave.EMP_CODE;

                                salaryDetails.EMPFL_BRANCHCODE = employeeToSave.EMP_BRANCH_CODE;

                            }



                            salaryDetails.EMPFL_BANK = employee.BankName ?? "";

                            salaryDetails.EMPFL_BK_ACCNO = employee.BankAccountNumber ?? "";

                            salaryDetails.EMPFL_TAX_NO = employee.PANNumber;

                            salaryDetails.PAYMODE = "BANK";

                            salaryDetails.EPFDETECT = false;

                            salaryDetails.SOCSODETECT = false;

                            salaryDetails.TMPGUARD = false;

                            salaryDetails.DETECTBYND55 = false;

                            salaryDetails.LASTUPDATE = DateTime.Now;

                            salaryDetails.LastUpdatedBy = request.ImportedBy;



                            if (salaryDetails.EMPFL_ID == 0)

                            {

                                _oBMSDbContext.EmployeeSalaryDetails.Add(salaryDetails);

                            }

                            else

                            {

                                _oBMSDbContext.EmployeeSalaryDetails.Update(salaryDetails);

                            }



                            await _oBMSDbContext.SaveChangesAsync();



                            successfulImports.Add(new EmployeeImportSuccess

                            {

                                RowNumber = previewRecord.RowNumber,

                                EmployeeCode = employeeToSave.EMP_CODE,

                                EmployeeName = employeeToSave.EMP_NAME,

                                EmployeeId = employeeToSave.EMP_ID,

                                Action = action

                            });

                        }

                        catch (Exception ex)

                        {

                            errors.Add(new EmployeeImportError

                            {

                                RowNumber = previewRecord.RowNumber,

                                EmployeeCode = previewRecord.Employee?.EMP_CODE ?? "Unknown",

                                EmployeeName = previewRecord.Employee?.EMP_NAME ?? "Unknown",

                                FieldName = "General",

                                ErrorMessage = $"Import error: {ex.Message}"

                            });

                        }

                    }



                    await _oBMSDbContext.SaveChangesAsync();

                    await transaction.CommitAsync();



                    result.Success = successfulImports.Count > 0;

                    result.Message = $"Import completed. {successfulImports.Count} successful, {errors.Count} failed.";

                    result.TotalRecords = request.Employees.Count;

                    result.SuccessfulImports = successfulImports.Count;

                    result.FailedImports = errors.Count;

                    result.Errors = errors;

                    result.SuccessfulEmployees = successfulImports.Select(s => new EmployeeImportPreviewDto

                    {

                        Employee = null, // Would need to be populated if needed

                        ValidationMessage = s.Message,

                        IsValid = s.Success,

                        RowNumber = s.RowNumber

                    }).ToList();



                    Console.WriteLine("=== BULK IMPORT END ===");

                    Console.WriteLine($"Success: {result.SuccessfulImports}");

                    Console.WriteLine($"Failed: {result.FailedImports}");

                    Console.WriteLine($"Total: {result.TotalRecords}");

                    Console.WriteLine("====================");



                    return result;

                }

                catch (Exception ex)

                {

                    await transaction.RollbackAsync();

                    Console.WriteLine("=== BULK IMPORT TRANSACTION ROLLBACK ===");

                    Console.WriteLine($"Error: {ex.Message}");

                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");



                    // Log inner exception details

                    if (ex.InnerException != null)

                    {

                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

                        Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");



                        if (ex.InnerException.InnerException != null)

                        {

                            Console.WriteLine($"Inner Inner Exception: {ex.InnerException.InnerException.Message}");

                        }

                    }

                    Console.WriteLine("======================================");

                    throw;

                }

            });

        }



        public byte[] GenerateImportTemplate()

        {

            using (var workbook = new ClosedXML.Excel.XLWorkbook())

            {

                var worksheet = workbook.Worksheets.Add("Employee Template");



                // Headers

                var headers = new[]

                {

                    "EMP_CODE", "EMP_NAME*", "EMP_ROLE*", "EMP_BRANCH_CODE*", "DepartmentId", "DesignationId", "ClientCode",

                    "EMP_SEX*", "EMP_Race", "EMP_FATHER_NAME", "EMP_ADDRESS1", "EMP_ADDRESS2", "EMP_POST_CODE", "EMP_TOWN", "EMP_STATE",

                    "EMP_NATIONAL", "EMP_PHONE", "EMP_MOBILEPHONE", "EMP_HGH_EDU", "EMP_DATE_OF_BIRTH",

                    "EMP_MARTIAL_STATUS", "EMP_SPOUSE_NAME", "EMP_NO_CHILD",

                    "AadhaarNumber", "PANNumber", "PFAccountNumber", "ESINumber", "SalaryGroup",

                    "IndianState", "BankAccountNumber", "BankIFSC", "BankName", "UPIId",

                    "EMPPAY_JOB_TITLE", "EMPPAY_CATEGORY", "EMPPAY_DATE_JOINED", "EMPPAY_DATE_CONFIRM", "EMPPAY_BASIC_RATE",

                    // CB (Cost Breakdown) Fields - Only 7 essential fields for salary breakdown

                    "CB_Basic", "CB_DA", "CB_HRA", "CB_Leaves", "CB_NH", "CB_AdvanceStatutoryBonus", "CB_OtherAllowances", "CB_SubTotal"

                };



                for (int i = 0; i < headers.Length; i++)

                {

                    worksheet.Cell(1, i + 1).Value = headers[i];

                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;

                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

                }



                // Add sample data for all 43 columns

                worksheet.Cell(2, 1).Value = "EMP001";  // EMP_CODE

                worksheet.Cell(2, 2).Value = "John Doe";  // EMP_NAME*

                worksheet.Cell(2, 3).Value = "GUARD";  // EMP_ROLE*

                worksheet.Cell(2, 4).Value = "BR001";  // EMP_BRANCH_CODE*

                worksheet.Cell(2, 5).Value = "1";  // DepartmentId

                worksheet.Cell(2, 6).Value = "1";  // DesignationId

                worksheet.Cell(2, 7).Value = "CLIENT001";  // ClientCode

                worksheet.Cell(2, 8).Value = "Male";  // EMP_SEX*

                worksheet.Cell(2, 9).Value = "Muslim";  // EMP_Race (Religion)

                worksheet.Cell(2, 10).Value = "John Doe Sr.";  // EMP_FATHER_NAME

                worksheet.Cell(2, 11).Value = "123 Main Street";  // EMP_ADDRESS1

                worksheet.Cell(2, 12).Value = "Apartment 4B";  // EMP_ADDRESS2

                worksheet.Cell(2, 13).Value = "400001";  // EMP_POST_CODE

                worksheet.Cell(2, 14).Value = "Mumbai";  // EMP_TOWN

                worksheet.Cell(2, 15).Value = "Maharashtra";  // EMP_STATE

                worksheet.Cell(2, 16).Value = "Indian";  // EMP_NATIONAL

                worksheet.Cell(2, 17).Value = "+919876543210";  // EMP_PHONE

                worksheet.Cell(2, 18).Value = "+919876543211";  // EMP_MOBILEPHONE

                worksheet.Cell(2, 19).Value = "Bachelor";  // EMP_HGH_EDU

                worksheet.Cell(2, 20).Value = "1990-01-15";  // EMP_DATE_OF_BIRTH

                worksheet.Cell(2, 21).Value = "Married";  // EMP_MARTIAL_STATUS

                worksheet.Cell(2, 22).Value = "Jane Doe";  // EMP_SPOUSE_NAME

                worksheet.Cell(2, 23).Value = 2;  // EMP_NO_CHILD

                worksheet.Cell(2, 24).Value = "234567890123";  // AadhaarNumber

                worksheet.Cell(2, 25).Value = "ABCDE1234F";  // PANNumber

                worksheet.Cell(2, 26).Value = "MH123456789";  // PFAccountNumber

                worksheet.Cell(2, 27).Value = "ESI12345678";  // ESINumber

                worksheet.Cell(2, 28).Value = "A";  // SalaryGroup

                worksheet.Cell(2, 29).Value = "Maharashtra";  // IndianState

                worksheet.Cell(2, 30).Value = "12345678901";  // BankAccountNumber

                worksheet.Cell(2, 31).Value = "HDFC0123456";  // BankIFSC

                worksheet.Cell(2, 32).Value = "HDFC Bank";  // BankName

                worksheet.Cell(2, 33).Value = "john@upi";  // UPIId

                worksheet.Cell(2, 34).Value = "Security Guard";  // EMPPAY_JOB_TITLE*

                worksheet.Cell(2, 35).Value = "General";  // EMPPAY_CATEGORY*

                worksheet.Cell(2, 36).Value = DateTime.Now.ToString("yyyy-MM-dd");  // EMPPAY_DATE_JOINED

                worksheet.Cell(2, 37).Value = "";  // EMPPAY_DATE_CONFIRM

                worksheet.Cell(2, 38).Value = 15000;  // EMPPAY_BASIC_RATE

                worksheet.Cell(2, 39).Value = 5500;  // CB_Basic (Basic Salary)

                worksheet.Cell(2, 40).Value = 4500;  // CB_DA (Dearness Allowance)

                worksheet.Cell(2, 41).Value = 2000;  // CB_HRA (House Rent Allowance)

                worksheet.Cell(2, 42).Value = 1000;  // CB_Leaves (Leave Wages)

                worksheet.Cell(2, 43).Value = 1000;  // CB_NH (National Festival Allowance)

                worksheet.Cell(2, 44).Value = 0;  // CB_AdvanceStatutoryBonus (Advance Statutory Bonus)

                worksheet.Cell(2, 45).Value = 0;  // CB_OtherAllowances (Other Allowances)

                worksheet.Cell(2, 46).Value = 14000;  // CB_SubTotal (calculated)



                // Auto-fit columns

                worksheet.Columns().AdjustToContents();



                // Add instructions

                var instructionSheet = workbook.Worksheets.Add("Instructions");

                instructionSheet.Cell(1, 1).Value = "Employee Import Template Instructions";

                instructionSheet.Cell(1, 1).Style.Font.Bold = true;

                instructionSheet.Cell(1, 1).Style.Font.FontSize = 14;



                var instructions = new[]

                {

                    "1. Fields marked with * are mandatory",

                    "2. EMP_CODE is optional - if not provided, it will be auto-generated",

                    "3. EMP_SEX must be: Male, Female, or Other",

                    "4. EMP_DATE_OF_BIRTH, EMPPAY_DATE_JOINED, EMPPAY_DATE_CONFIRM should be in YYYY-MM-DD format",

                    "5. AadhaarNumber must be 12 digits starting with 2-9",

                    "6. PANNumber must be in format ABCDE1234F",

                    "7. BankIFSC must be in format ABCD0XXXXXX",

                    "8. DepartmentId and DesignationId must be valid numeric IDs from the system",

                    "9. DepartmentId: Use the ID from Department master",

                    "10. DesignationId: Use the ID from Designation master",

                    "11. ClientCode: Use the Code from Client master",

                    "12. EMP_Race: Enter the employee's religion (e.g., Muslim, Hindu, Christian) (optional)",

                    "13. EMP_FATHER_NAME: Enter the employee's father's name (optional)",

                    "14. For new employees, do not include EMP_ID column",

                    "15. Save the file as .xlsx format before uploading"

                };



                for (int i = 0; i < instructions.Length; i++)

                {

                    instructionSheet.Cell(i + 3, 1).Value = instructions[i];

                }



                instructionSheet.Columns().AdjustToContents();



                using (var stream = new MemoryStream())

                {

                    workbook.SaveAs(stream);

                    return stream.ToArray();

                }

            }

        }



        private EmployeeExcelImportDto ParseExcelRow(ClosedXML.Excel.IXLRow row, int rowNumber)

        {

            var dto = new EmployeeExcelImportDto();

            dto.RowNumber = rowNumber;



            // Debug: Show raw cell values

            Console.WriteLine($"Row {rowNumber} Raw values: Col1='{GetCellValue(row, 1)}', Col2='{GetCellValue(row, 2)}', Col3='{GetCellValue(row, 3)}', Col4='{GetCellValue(row, 4)}', Col5='{GetCellValue(row, 5)}', Col6='{GetCellValue(row, 6)}', Col7='{GetCellValue(row, 7)}'");



            dto.EMP_CODE = GetCellValue(row, 1)?.Trim() ?? "";

            dto.EMP_NAME = GetCellValue(row, 2)?.Trim() ?? "";

            dto.EMP_ROLE = ConvertRole(GetCellValue(row, 3)) ?? "";

            dto.EMP_BRANCH_CODE = GetCellValue(row, 4)?.Trim() ?? "";

            dto.DepartmentId = ParseInt(GetCellValue(row, 5));

            dto.DesignationId = ParseInt(GetCellValue(row, 6));

            dto.ClientCode = GetCellValue(row, 7)?.Trim();

            dto.EMP_SEX = GetCellValue(row, 8)?.Trim() ?? "";

            dto.EMP_Race = GetCellValue(row, 9)?.Trim();

            dto.EMP_FATHER_NAME = GetCellValue(row, 10)?.Trim();

            dto.EMP_ADDRESS1 = GetCellValue(row, 11)?.Trim();

            dto.EMP_ADDRESS2 = GetCellValue(row, 12)?.Trim();

            dto.EMP_POST_CODE = GetCellValue(row, 13)?.Trim();

            dto.EMP_TOWN = GetCellValue(row, 14)?.Trim();

            dto.EMP_STATE = GetCellValue(row, 15)?.Trim();

            dto.EMP_NATIONAL = GetCellValue(row, 16)?.Trim();

            dto.EMP_PHONE = GetCellValue(row, 17)?.Trim();

            dto.EMP_MOBILEPHONE = GetCellValue(row, 18)?.Trim();

            dto.EMP_HGH_EDU = GetCellValue(row, 19)?.Trim();

            dto.EMP_DATE_OF_BIRTH = ParseDate(GetCellValue(row, 20));

            dto.EMP_MARTIAL_STATUS = GetCellValue(row, 21)?.Trim();

            dto.EMP_SPOUSE_NAME = GetCellValue(row, 22)?.Trim();

            dto.EMP_NO_CHILD = ParseInt(GetCellValue(row, 23));

            dto.AadhaarNumber = GetCellValue(row, 24)?.Trim();

            dto.PANNumber = GetCellValue(row, 25)?.Trim()?.ToUpper();

            dto.PFAccountNumber = GetCellValue(row, 26)?.Trim();

            dto.ESINumber = GetCellValue(row, 27)?.Trim();

            dto.SalaryGroup = GetCellValue(row, 28)?.Trim();

            dto.IndianState = GetCellValue(row, 29)?.Trim();

            dto.BankAccountNumber = GetCellValue(row, 30)?.Trim();

            dto.BankIFSC = GetCellValue(row, 31)?.Trim()?.ToUpper();

            dto.BankName = GetCellValue(row, 32)?.Trim();

            dto.UPIId = GetCellValue(row, 33)?.Trim();

            dto.EMPPAY_JOB_TITLE = GetCellValue(row, 34)?.Trim();

            dto.EMPPAY_CATEGORY = GetCellValue(row, 35)?.Trim();

            dto.EMPPAY_DATE_JOINED = ParseDate(GetCellValue(row, 36));

            dto.EMPPAY_DATE_CONFIRM = ParseDate(GetCellValue(row, 37));

            dto.EMPPAY_BASIC_RATE = ParseDouble(GetCellValue(row, 38));



            // CB (Cost Breakdown) Fields - Only 7 essential fields (Columns 39-46)

            dto.CB_Basic = ParseDecimal(GetCellValue(row, 39));  // Basic Salary

            dto.CB_DA = ParseDecimal(GetCellValue(row, 40));  // Dearness Allowance

            dto.CB_HRA = ParseDecimal(GetCellValue(row, 41));  // House Rent Allowance

            dto.CB_Leaves = ParseDecimal(GetCellValue(row, 42));  // Leave Wages

            dto.CB_NH = ParseDecimal(GetCellValue(row, 43));  // National Festival Allowance (NFH)

            dto.CB_AdvanceStatutoryBonus = ParseDecimal(GetCellValue(row, 44));  // Advance Statutory Bonus

            dto.CB_OtherAllowances = ParseDecimal(GetCellValue(row, 45));  // Other Allowances

            dto.CB_SubTotal = ParseDecimal(GetCellValue(row, 46));  // Sub Total (calculated)



            // Employment Details Fields - Columns 47-50

            dto.ATTENDANCEALLOWANCE = ParseDecimal(GetCellValue(row, 47));

            dto.SpecialAllowance = ParseDecimal(GetCellValue(row, 48));

            dto.AttendanceAllowanceWorkingDays = ParseDecimal(GetCellValue(row, 49));

            dto.AttendanceAllowanceFollowCalendar = "N";



            // Employee Salary Details Fields - Columns 51-60

            dto.EMPFL_BANK = GetCellValue(row, 51)?.Trim();

            dto.EMPFL_BK_ACCNO = GetCellValue(row, 52)?.Trim();

            dto.EMPFL_EPFNO = GetCellValue(row, 53)?.Trim();

            dto.EMPFL_SOSCO_NO = GetCellValue(row, 54)?.Trim();

            dto.PAYMODE = GetCellValue(row, 55)?.Trim();

            dto.EPFDETECT = ParseBool(GetCellValue(row, 56));

            dto.SOCSODETECT = ParseBool(GetCellValue(row, 57));

            dto.TMPGUARD = ParseBool(GetCellValue(row, 58));

            dto.DETECTBYND55 = ParseBool(GetCellValue(row, 59));

            dto.INCOMETAXDETECT = ParseBool(GetCellValue(row, 60));



            // Employee Flags - Columns 61-68

            dto.NewSalaryStructure = GetCellValue(row, 61)?.Trim();

            dto.SalaryStructure1000_3h = GetCellValue(row, 62)?.Trim();

            dto.EMP_CHECKLIST = ParseInt(GetCellValue(row, 63));

            dto.KDNVetting = ParseBool(GetCellValue(row, 64));

            dto.HasTransfered = ParseBool(GetCellValue(row, 65));

            dto.EMP_SP_WORK = GetCellValue(row, 66)?.Trim();

            dto.EMP_CLIENT = GetCellValue(row, 67)?.Trim();

            dto.EMP_SP_TEL_NO = GetCellValue(row, 68)?.Trim();



            return dto;

        }



        private string? GetCellValue(ClosedXML.Excel.IXLRow row, int column)

        {

            var cell = row.Cell(column);

            if (cell == null || cell.IsEmpty())

                return null;

            return cell.GetValue<string>();

        }



        private DateTime? ParseDate(string? value)

        {

            if (string.IsNullOrWhiteSpace(value))

                return null;



            if (DateTime.TryParse(value, out var date))

                return date;



            // Try Excel date format

            if (double.TryParse(value, out var excelDate))

            {

                try

                {

                    return DateTime.FromOADate(excelDate);

                }

                catch { }

            }



            return null;

        }



        private int? ParseInt(string? value)

        {

            if (string.IsNullOrWhiteSpace(value))

                return null;



            if (int.TryParse(value, out var result))

                return result;



            return null;

        }



        private double? ParseDouble(string? value)

        {

            if (string.IsNullOrWhiteSpace(value))

                return null;



            if (double.TryParse(value, out var result))

                return result;



            return null;

        }



        private decimal? ParseDecimal(string? value)

        {

            if (string.IsNullOrWhiteSpace(value))

                return null;



            if (decimal.TryParse(value, out var result))

                return result;



            return null;

        }



        private bool? ParseBool(string? value)

        {

            if (string.IsNullOrWhiteSpace(value))

                return null;



            if (bool.TryParse(value, out var result))

                return result;



            // Handle "Y"/"N" or "1"/"0" as boolean

            if (value.Equals("Y", StringComparison.OrdinalIgnoreCase) || value == "1")

                return true;

            if (value.Equals("N", StringComparison.OrdinalIgnoreCase) || value == "0")

                return false;



            return null;

        }



        private string? ConvertRole(string? role)

        {

            if (string.IsNullOrWhiteSpace(role))

                return null;



            role = role.Trim();



            // Convert numeric codes to text

            if (role == "1")

                return "Guard";

            if (role == "2")

                return "Staff";



            // Apply proper capitalization (first letter capital, rest lowercase)

            if (role.Length > 0)

            {

                return char.ToUpper(role[0]) + role.Substring(1).ToLower();

            }



            return role;

        }



        private async Task<Dictionary<string, string>> ValidateEmployeeImportDto(EmployeeExcelImportDto dto)

        {

            var errors = new Dictionary<string, string>();



            // Required fields (EMP_CODE is now optional - will be auto-generated)

            if (string.IsNullOrWhiteSpace(dto.EMP_NAME))

                errors.Add("EMP_NAME", "Employee Name is required");



            if (string.IsNullOrWhiteSpace(dto.EMP_ROLE))

                errors.Add("EMP_ROLE", "Employee Role is required");



            if (string.IsNullOrWhiteSpace(dto.EMP_BRANCH_CODE))

                errors.Add("EMP_BRANCH_CODE", "Branch Code is required");

            else

            {

                var branchExists = await _oBMSDbContext.BranchMasters

                    .AnyAsync(b => b.Code == dto.EMP_BRANCH_CODE);

                if (!branchExists)

                {

                    errors.Add("EMP_BRANCH_CODE", $"Branch Code '{dto.EMP_BRANCH_CODE}' does not exist in database");

                }

            }



            if (string.IsNullOrWhiteSpace(dto.EMP_SEX))

                errors.Add("EMP_SEX", "Gender is required");

            else if (!new[] { "Male", "Female", "Other" }.Contains(dto.EMP_SEX, StringComparer.OrdinalIgnoreCase))

                errors.Add("EMP_SEX", "Gender must be Male, Female, or Other");



            // Aadhaar validation

            if (!string.IsNullOrWhiteSpace(dto.AadhaarNumber))

            {

                if (!System.Text.RegularExpressions.Regex.IsMatch(dto.AadhaarNumber, @"^[2-9][0-9]{11}$"))

                    errors.Add("AadhaarNumber", "Aadhaar must be 12 digits starting with 2-9");

            }



            // PAN validation

            if (!string.IsNullOrWhiteSpace(dto.PANNumber))

            {

                if (!System.Text.RegularExpressions.Regex.IsMatch(dto.PANNumber, @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$"))

                    errors.Add("PANNumber", "PAN must be in format ABCDE1234F");

            }



            // IFSC validation

            if (!string.IsNullOrWhiteSpace(dto.BankIFSC))

            {

                if (!System.Text.RegularExpressions.Regex.IsMatch(dto.BankIFSC, @"^[A-Z]{4}0[A-Z0-9]{6}$"))

                    errors.Add("BankIFSC", "IFSC must be in format ABCD0XXXXXX");

            }



            // Department Id validation

            if (dto.DepartmentId.HasValue)

            {

                var department = await _oBMSDbContext.Departments

                    .FirstOrDefaultAsync(d => d.DepartmentId == dto.DepartmentId.Value);

                if (department == null)

                {

                    errors.Add("DepartmentId", $"Department Id '{dto.DepartmentId}' does not exist in database");

                }

                else

                {

                    dto.DepartmentName = department.Dept_Name;

                }

            }



            // Designation Id validation

            if (dto.DesignationId.HasValue)

            {

                var designation = await _oBMSDbContext.Designations

                    .FirstOrDefaultAsync(d => d.DesignationId == dto.DesignationId.Value);

                if (designation == null)

                {

                    errors.Add("DesignationId", $"Designation Id '{dto.DesignationId}' does not exist in database");

                }

                else

                {

                    dto.DesignationName = designation.Desig_Name;

                }

            }



            // Client Code validation

            if (!string.IsNullOrWhiteSpace(dto.ClientCode))

            {

                var client = await _oBMSDbContext.ClientMasters

                    .FirstOrDefaultAsync(c => c.Code == dto.ClientCode);

                if (client == null)

                {

                    errors.Add("ClientCode", $"Client Code '{dto.ClientCode}' does not exist in database");

                }

                else

                {

                    // Store the client CODE, not the name

                    dto.EMP_CLIENT = dto.ClientCode;

                }

            }



            return errors;

        }



        private Employee MapEmployeeDtoToDomain(EmployeeExcelImportDto dto)

        {

            return new Employee

            {

                EMP_CODE = dto.EMP_CODE ?? "",

                EMP_NAME = dto.EMP_NAME ?? "",

                EMP_ROLE = dto.EMP_ROLE ?? "",

                EMP_BRANCH_CODE = dto.EMP_BRANCH_CODE ?? "",

                EMP_SEX = dto.EMP_SEX ?? "",

                EMP_RACE = dto.EMP_Race ?? "",

                EMP_FATHER_NAME = dto.EMP_FATHER_NAME ?? "",

                EMP_ADDRESS1 = dto.EMP_ADDRESS1 ?? "",

                EMP_ADDRESS2 = dto.EMP_ADDRESS2 ?? "",

                EMP_POST_CODE = dto.EMP_POST_CODE ?? "",

                EMP_TOWN = dto.EMP_TOWN ?? "",

                EMP_STATE = dto.EMP_STATE ?? "",

                EMP_NATIONAL = dto.EMP_NATIONAL ?? "",

                EMP_PHONE = dto.EMP_PHONE ?? "",

                EMP_MOBILEPHONE = dto.EMP_MOBILEPHONE ?? "",

                EMP_HGH_EDU = dto.EMP_HGH_EDU ?? "",

                EMP_DATE_OF_BIRTH = dto.EMP_DATE_OF_BIRTH,

                EMP_MARTIAL_STATUS = dto.EMP_MARTIAL_STATUS ?? "",

                EMP_SPOUSE_NAME = dto.EMP_SPOUSE_NAME ?? "",

                EMP_NO_CHILD = dto.EMP_NO_CHILD ?? 0,

                AadhaarNumber = dto.AadhaarNumber,

                PANNumber = dto.PANNumber,

                PFAccountNumber = dto.PFAccountNumber,

                ESINumber = dto.ESINumber,

                SalaryGroup = dto.SalaryGroup ?? "None",

                IndianState = dto.IndianState,

                BankAccountNumber = dto.BankAccountNumber,

                BankIFSC = dto.BankIFSC,

                BankName = dto.BankName,

                UPIId = dto.UPIId,

                DepartmentId = dto.DepartmentId,

                DesignationId = dto.DesignationId,

                DepartmentName = dto.DepartmentName,

                DesignationName = dto.DesignationName,



                // CB (Cost Breakdown) Fields - Only 7 essential fields for salary breakdown

                CB_Basic = dto.CB_Basic,  // Basic Salary

                CB_DA = dto.CB_DA,  // Dearness Allowance

                CB_HRA = dto.CB_HRA,  // House Rent Allowance

                CB_Leaves = dto.CB_Leaves,  // Leave Wages

                CB_NH = dto.CB_NH,  // National Festival Allowance (NFH)

                CB_AdvanceStatutoryBonus = dto.CB_AdvanceStatutoryBonus,  // Advance Statutory Bonus

                CB_OtherAllowances = dto.CB_OtherAllowances,  // Other Allowances

                CB_SubTotal = dto.CB_SubTotal,  // Sub Total (calculated)



                // Employee Flags

                NewSalaryStructure = !string.IsNullOrEmpty(dto.NewSalaryStructure) ? dto.NewSalaryStructure[0] : 'N',

                SalaryStructure1000_3h = !string.IsNullOrEmpty(dto.SalaryStructure1000_3h) ? dto.SalaryStructure1000_3h[0] : null,

                EMP_CHECKLIST = dto.EMP_CHECKLIST ?? 0,

                KDNVetting = dto.KDNVetting ?? false,

                HasTransfered = dto.HasTransfered ?? false,

                EMP_SP_WORK = dto.EMP_SP_WORK == "1" || dto.EMP_SP_WORK == "true",

                EMP_CLIENT = dto.EMP_CLIENT ?? ""

            };

        }



        #endregion

    }

}



[Keyless]

public class CheckEmployeeInfoResult

{

    public string? EMP_ID { get; set; }

}

