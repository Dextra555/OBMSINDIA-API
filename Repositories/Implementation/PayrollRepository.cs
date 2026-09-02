using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;

using OBMS.WebAPI.Models;

using OBMS.WebAPI.Models.Domain;

using OBMS.WebAPI.Models.DTO;

using OBMS.WebAPI.Repositories.Interface;

using OBMS.WebAPI.BusinessObjects;

using SkiaSharp;

using Syncfusion.XlsIO.Implementation.Security;

using System.Diagnostics;

using System.Globalization;

using System.Linq;

using System.Xml;



using OBMS.WebAPI.Services;



namespace OBMS.WebAPI.Repositories.Implementation

{

    public class PayrollRepository : IPayrollRepository

    {

        private readonly OBMSDbContext _oBMSDbContext;

        private readonly IPFCalculationService _pfCalculationService;

        private readonly IESICalculationService _esiCalculationService;

        private readonly ITDSCalculationService _tdsCalculationService;

        private readonly IProfessionalTaxService _professionalTaxService;

        private readonly IAttendancePeriodService _attendancePeriodService;



        public PayrollRepository(

            OBMSDbContext oBMSDbContext,

            IPFCalculationService pfCalculationService,

            IESICalculationService esiCalculationService,

            ITDSCalculationService tdsCalculationService,

            IProfessionalTaxService professionalTaxService,

            IAttendancePeriodService attendancePeriodService)

        {

            _oBMSDbContext = oBMSDbContext;

            _pfCalculationService = pfCalculationService;

            _esiCalculationService = esiCalculationService;

            _tdsCalculationService = tdsCalculationService;

            _professionalTaxService = professionalTaxService;

            _attendancePeriodService = attendancePeriodService;

        }



        #region Monthly Salary Advance

        public async Task<List<SalaryAdvance>> GetSalaryAdvanceById(int employeeId)

        {

            var result = await _oBMSDbContext.SalaryAdvances.Where(sa => sa.EmployeeID == employeeId).ToListAsync();

            return new List<SalaryAdvance>(result);

        }

        public async Task<List<Employee>> GetEmployeeById(int employeeId)

        {

            var result = await _oBMSDbContext.Employees.Where(sa => sa.EMP_ID == employeeId).ToListAsync();

            return new List<Employee>(result);

        }

        public async Task<List<SalaryAdvance>> GetSalaryAdvanceByDateAndEmployee(SalaryAdvance salaryAdvance)

        {

            var result = await _oBMSDbContext.SalaryAdvances

                .Where(sa => sa.AdvanceDate == salaryAdvance.AdvanceDate &&

                              sa.EmployeeID == salaryAdvance.EmployeeID &&

                              sa.TransType == salaryAdvance.TransType &&

                              !sa.IsDeleted) // Assuming IsDeleted is a boolean property

                .ToListAsync();



            return result;

        }

        public async Task<List<SalaryAdvanceDto>> GetEmployeeList()

        {

            var result = (from employee in _oBMSDbContext.Employees

                          join salaryDetails in _oBMSDbContext.EmployeeSalaryDetails on employee.EMP_CODE equals salaryDetails.EMPFL_CODE

                          join employeement in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employeement.EMPPAY_CODE

                          select new

                          {

                              employee.EMP_ID,

                              employee.EMP_NAME,

                              employee.EMP_IC_NEW,

                              employee.EMP_IC_OLD,

                              employee.EMP_PASSPORT_NO,

                              salaryDetails.EMPFL_BANK,

                              salaryDetails.EMPFL_BK_ACCNO,

                              salaryDetails.PAYMODE

                          }).ToList();



            var salaryAdvanceList = result.Select(x => new SalaryAdvanceDto

            {

                EMP_ID = x.EMP_ID,

                EMP_NAME = x.EMP_NAME,

                EMP_IC_NEW = x.EMP_IC_NEW,

                EMP_IC_OLD = x.EMP_IC_OLD,

                EMP_PASSPORT_NO = x.EMP_PASSPORT_NO,

                EMPFL_BANK = x.EMPFL_BANK,

                EMPFL_BK_ACCNO = x.EMPFL_BK_ACCNO,

                PAYMODE = x.PAYMODE,

            }).ToList();



            return new List<SalaryAdvanceDto>(salaryAdvanceList);

        }

        public async Task<List<SalaryAdvanceDto>> GetEmployeeListBySalaryAdvance()

        {

            var result = await (from employee in _oBMSDbContext.Employees

                                join salaryDetails in _oBMSDbContext.EmployeeSalaryDetails on employee.EMP_CODE equals salaryDetails.EMPFL_CODE

                                join employeement in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employeement.EMPPAY_CODE

                                ////join salaryAdvance in _oBMSDbContext.SalaryAdvances on employee.EMP_ID equals salaryAdvance.EmployeeID into salaryAdvanceGroup

                                ////  from salaryAdvances in salaryAdvanceGroup.DefaultIfEmpty()

                                join salaryAdvance in _oBMSDbContext.SalaryAdvances on employee.EMP_ID equals salaryAdvance.EmployeeID

                                select new

                                {

                                    salaryAdvance.ID,

                                    employee.EMP_ID,

                                    employee.EMP_NAME,

                                    employee.EMP_IC_NEW,

                                    employee.EMP_IC_OLD,

                                    employee.EMP_PASSPORT_NO,

                                    salaryDetails.EMPFL_BANK,

                                    salaryDetails.EMPFL_BK_ACCNO,

                                    salaryDetails.PAYMODE,

                                    salaryAdvance.Amount,

                                    salaryAdvance.Particulars

                                }).ToListAsync();



            var salaryAdvanceList = result.Select(x => new SalaryAdvanceDto

            {

                ID = x.ID,

                EMP_ID = x.EMP_ID,

                EMP_NAME = x.EMP_NAME,

                EMP_IC_NEW = x.EMP_IC_NEW,

                EMP_IC_OLD = x.EMP_IC_OLD,

                EMP_PASSPORT_NO = x.EMP_PASSPORT_NO,

                EMPFL_BANK = x.EMPFL_BANK,

                EMPFL_BK_ACCNO = x.EMPFL_BK_ACCNO,

                PAYMODE = x.PAYMODE,

                Amount = x.Amount,

                Particulars = x.Particulars,

            }).ToList();



            return new List<SalaryAdvanceDto>(salaryAdvanceList);

        }

        public async Task<List<SalaryAdvanceDto>> GetEmployeeListByBranchCode(string branchCode)

        {

            var result = await (from employee in _oBMSDbContext.Employees

                                join salaryDetails in _oBMSDbContext.EmployeeSalaryDetails on employee.EMP_CODE equals salaryDetails.EMPFL_CODE

                                join employeement in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employeement.EMPPAY_CODE

                                join salaryAdvance in _oBMSDbContext.SalaryAdvances on employee.EMP_ID equals salaryAdvance.EmployeeID into advancesGroup

                                from salaryAdvance in advancesGroup.DefaultIfEmpty()

                                where employee.EMP_BRANCH_CODE == branchCode

                                select new SalaryAdvanceDto

                                {

                                    ID = (salaryAdvance != null) ? salaryAdvance.ID : 0,

                                    EMP_ID = employee.EMP_ID,

                                    EMP_NAME = employee.EMP_NAME,

                                    EMP_CODE = employee.EMP_CODE,

                                    EMP_IC_NEW = employee.EMP_IC_NEW,

                                    EMP_IC_OLD = employee.EMP_IC_OLD,

                                    EMP_PASSPORT_NO = employee.EMP_PASSPORT_NO,

                                    EMPFL_BANK = salaryDetails.EMPFL_BANK,

                                    EMPFL_BK_ACCNO = salaryDetails.EMPFL_BK_ACCNO,

                                    PAYMODE = salaryDetails.PAYMODE,

                                    Amount = (salaryAdvance != null) ? salaryAdvance.Amount : 0,

                                    Particulars = (salaryAdvance != null) ? salaryAdvance.Particulars : ""

                                })

                    .GroupBy(dto => dto.EMP_ID) // Group by employee ID

                    .Select(group => group.First()) // Select the first element of each group

                    .ToListAsync();





            return new List<SalaryAdvanceDto>(result);

        }

        public async Task<List<SalaryAdvanceDto>> GetEmployeeListByAdvanceID(int Id)

        {

            var result = await (from employee in _oBMSDbContext.Employees

                                join salaryDetails in _oBMSDbContext.EmployeeSalaryDetails on employee.EMP_CODE equals salaryDetails.EMPFL_CODE

                                join employeement in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employeement.EMPPAY_CODE

                                join salaryAdvance in _oBMSDbContext.SalaryAdvances on employee.EMP_ID equals salaryAdvance.EmployeeID into advancesGroup

                                from salaryAdvance in advancesGroup.DefaultIfEmpty()

                                where salaryAdvance.ID == Id

                                select new SalaryAdvanceDto

                                {

                                    ID = (salaryAdvance != null) ? salaryAdvance.ID : 0,

                                    EMP_ID = employee.EMP_ID,

                                    EMP_NAME = employee.EMP_NAME,

                                    EMP_IC_NEW = employee.EMP_IC_NEW,

                                    EMP_IC_OLD = employee.EMP_IC_OLD,

                                    EMP_PASSPORT_NO = employee.EMP_PASSPORT_NO,

                                    EMPFL_BANK = salaryDetails.EMPFL_BANK,

                                    EMPFL_BK_ACCNO = salaryDetails.EMPFL_BK_ACCNO,

                                    PAYMODE = salaryDetails.PAYMODE,

                                    Amount = (salaryAdvance != null) ? salaryAdvance.Amount : 0,

                                    Particulars = (salaryAdvance != null) ? salaryAdvance.Particulars : ""

                                }).ToListAsync();



            return new List<SalaryAdvanceDto>(result);

        }
        public async Task<SalaryAdvance> SaveAndUpdateSalaryMonthlyAdvance(SalaryAdvance salaryAdvance, List<EmployeeItemIssueDto>? items = null)
        {
            Console.WriteLine($"[PayrollRepository] SaveAndUpdateSalaryMonthlyAdvance called with ID: {salaryAdvance?.ID}, EmployeeID: {salaryAdvance?.EmployeeID}, Amount: {salaryAdvance?.Amount}, Items: {items?.Count ?? 0}");
            
            var strategy = _oBMSDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _oBMSDbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingRecord = await _oBMSDbContext.SalaryAdvances
                        .FirstOrDefaultAsync(s => s.ID == salaryAdvance.ID);

                    if (existingRecord != null)
                    {
                        Console.WriteLine($"[PayrollRepository] Updating existing record with ID: {existingRecord.ID}");
                        // Update properties
                        existingRecord.EmployeeID = salaryAdvance.EmployeeID;
                        existingRecord.AdvanceTakenDate = salaryAdvance.AdvanceTakenDate;
                        existingRecord.AdvanceDate = salaryAdvance.AdvanceDate;
                        existingRecord.VoucherNo = salaryAdvance.VoucherNo;
                        existingRecord.Amount = salaryAdvance.Amount;
                        existingRecord.NoOfInstallments = salaryAdvance.NoOfInstallments;
                        existingRecord.PaymentType = salaryAdvance.PaymentType;
                        existingRecord.Particulars = salaryAdvance.Particulars;
                        existingRecord.TransType = salaryAdvance.TransType;
                        existingRecord.IsDeleted = salaryAdvance.IsDeleted;
                        existingRecord.LastUpdate = DateTime.Now;
                        existingRecord.LastUpdatedBy = salaryAdvance.LastUpdatedBy;
                        _oBMSDbContext.Update(existingRecord);
                        await _oBMSDbContext.SaveChangesAsync();
                        Console.WriteLine($"[PayrollRepository] Successfully updated record with ID: {existingRecord.ID}");
                        
                        // Handle EmployeeItemIssue records for update
                        await SaveEmployeeItemIssues(existingRecord.ID, items);
                        
                        await transaction.CommitAsync();
                        return existingRecord;
                    }
                    else
                    {
                        Console.WriteLine("[PayrollRepository] Creating new salary advance record");
                        _oBMSDbContext.Add(salaryAdvance);
                        await _oBMSDbContext.SaveChangesAsync();
                        Console.WriteLine($"[PayrollRepository] Successfully created new record with ID: {salaryAdvance.ID}");
                        
                        // Handle EmployeeItemIssue records for new record
                        await SaveEmployeeItemIssues(salaryAdvance.ID, items);
                        
                        await transaction.CommitAsync();
                        return salaryAdvance;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PayrollRepository] Error in SaveAndUpdateSalaryMonthlyAdvance: {ex.Message}");
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private async Task SaveEmployeeItemIssues(int advanceId, List<EmployeeItemIssueDto>? items)
        {
            Console.WriteLine($"[PayrollRepository] SaveEmployeeItemIssues called with AdvanceID: {advanceId}, Items: {items?.Count ?? 0}");
            
            if (items == null || !items.Any())
            {
                Console.WriteLine("[PayrollRepository] No items to save");
                return;
            }

            // Remove existing EmployeeItemIssue records for this AdvanceID
            var existingItems = await _oBMSDbContext.EmployeeItemIssues
                .Where(eii => eii.AdvanceID == advanceId)
                .ToListAsync();
            
            if (existingItems.Any())
            {
                Console.WriteLine($"[PayrollRepository] Removing {existingItems.Count} existing EmployeeItemIssue records");
                _oBMSDbContext.EmployeeItemIssues.RemoveRange(existingItems);
                await _oBMSDbContext.SaveChangesAsync();
            }

// Add new EmployeeItemIssue records
            var employeeItemIssues = items.Select(item => new EmployeeItemIssue
            {
                AdvanceID = advanceId,
                ItemID = item.ItemID,
                Price = item.Price,
                Quantity = item.Quantity,
                LastUpdate = DateTime.Now,
                LastUpdatedBy = item.LastUpdatedBy ?? "Admin"
            }).ToList();

            Console.WriteLine($"[PayrollRepository] Adding {employeeItemIssues.Count} new EmployeeItemIssue records");
            await _oBMSDbContext.EmployeeItemIssues.AddRangeAsync(employeeItemIssues);
            await _oBMSDbContext.SaveChangesAsync();
            
            Console.WriteLine($"[PayrollRepository] Successfully saved EmployeeItemIssue records for AdvanceID: {advanceId}");
        }

        #endregion

        public async Task<List<InventoryCategory>> GetInventoryCategories()

        {

            var query = _oBMSDbContext.InventoryCategories

                .OrderBy(ic => ic.Name)

                .Select(ic => new InventoryCategory

                {

                    ID = ic.ID,

                    Name = $"{ic.Name}({(ic.Cat == "P" ? "Purchase" : "Utility")})",

                    Cat = ic.Cat,

                    AssetType = ic.AssetType

                });



            return await query.ToListAsync();

        }



        public string GetNewVoucherNumberAsync(int transType)

        {

            var result = _oBMSDbContext.SalaryAdvances

                .Where(s => s.TransType == transType && s.VoucherNo != null)

                .AsEnumerable()

                .Select(s =>

                {

                    // Remove any non-numeric characters and parse

                    var cleanVoucherNo = System.Text.RegularExpressions.Regex.Replace(s.VoucherNo ?? "", "[^0-9]", "");

                    return int.TryParse(cleanVoucherNo, out var voucherNo) ? voucherNo : (int?)null;

                })

                .Where(v => v.HasValue) // Exclude null values

                .Max() ?? 0; // Get the highest valid VoucherNo or default to 0

            var newVoucherNo = result + 1;

            return newVoucherNo.ToString("000000");

        }







        public List<ItemMasterDto> GetUniformItemRows(int AdvanceID, int Category)

        {

            var query = from itemMaster in _oBMSDbContext.ItemMasters

                        join employeeItemIssue in _oBMSDbContext.EmployeeItemIssues

                            on new { ItemID = itemMaster.ID, AdvanceID }

                            equals new { ItemID = employeeItemIssue.ItemID, AdvanceID = employeeItemIssue.AdvanceID }

                            into itemIssueGroup

                        from employeeItemIssue in itemIssueGroup.DefaultIfEmpty()

                        where itemMaster.CategoryID == Category

                        select new ItemMasterDto

                        {

                            ID = employeeItemIssue != null ? employeeItemIssue.ID : 0,

                            AdvanceID = employeeItemIssue != null ? employeeItemIssue.AdvanceID : 0,

                            ItemID = itemMaster.ID,

                            Name = itemMaster.Name,

                            Price = itemMaster.SellPrice ?? 0,

                            Quantity = employeeItemIssue != null ? employeeItemIssue.Quantity: 0

                            

                        };



            return query.ToList();

        }





        public bool GetSalaryProcessDateByEmployeeID(int employeeID, int year, int month)

        {

            var payslipExists = _oBMSDbContext.PaySlips

                .Where(p => p.EmployeeID == employeeID && p.Period.Year == year && p.Period.Month == month)

                .OrderByDescending(p => p.Period)

                .Take(1)

                .Any();



            return payslipExists;



        }



        public DateTime GetResignDateByEmployeeID(int employeeID)

        {

            var resignDate = _oBMSDbContext.Employees

                .Join(

                    _oBMSDbContext.EmploymentDetails,

                    employee => employee.EMP_CODE,

                    employmentDetails => employmentDetails.EMPPAY_CODE,

                    (employee, employmentDetails) => new { employee, employmentDetails }

                )

                .Where(joinResult => !joinResult.employee.HasTransfered && joinResult.employee.EMP_ID == employeeID)

                .Select(joinResult => joinResult.employmentDetails.EMPPAY_DATE_RESIGNED)

                .FirstOrDefault();

            return resignDate ?? new DateTime(1900, 1, 1);



            //return (DateTime)(resignDate != default(DateTime) ? resignDate : new DateTime(1900, 1, 1));



        }





        #region MiscTransaction Region

        public async Task<IEnumerable<MiscTrans>> GetMiscTrans()

        {

            return await _oBMSDbContext.MiscTrans.ToListAsync();

        }



        public async Task<List<MiscTrans>> GetMiscTransById(int id)

        {

            var result = await _oBMSDbContext.MiscTrans.Where(m => m.ID == id).ToListAsync();

            return new List<MiscTrans>(result);

        }

        public async Task<MiscTrans> SaveAndUpdateMiscTrans(MiscTrans miscTrans)

        {

            var existingMiscTrans = await _oBMSDbContext.MiscTrans.FirstOrDefaultAsync(mt => mt.ID == miscTrans.ID);

            if (existingMiscTrans != null)

            {

                existingMiscTrans.TransDate = miscTrans.TransDate;

                existingMiscTrans.EmployeeID = miscTrans.EmployeeID;

                existingMiscTrans.TransType = miscTrans.TransType;

                existingMiscTrans.Amount = miscTrans.Amount;

                existingMiscTrans.Particulars = miscTrans.Particulars;

                existingMiscTrans.LastUpdate = DateTime.Now;

                existingMiscTrans.LastUpdatedBy = miscTrans.LastUpdatedBy;

                _oBMSDbContext.Update(existingMiscTrans);

                await _oBMSDbContext.SaveChangesAsync();

                return existingMiscTrans;

            }

            else

            {

                miscTrans.LastUpdate = DateTime.Now;

                _oBMSDbContext.MiscTrans.Add(miscTrans);

                await _oBMSDbContext.SaveChangesAsync();

                return miscTrans;

            }



        }



        public async Task DeleteMiscTransById(int id)

        {

            var miscTrans = await _oBMSDbContext.MiscTrans.FindAsync(id);

            _oBMSDbContext.MiscTrans.Remove(miscTrans);

            await _oBMSDbContext.SaveChangesAsync();

        }



        #endregion

        public async Task<List<SalaryAdvance>> SaveAndUpdateSalaryDailyAdvances(List<SalaryAdvance> salaryAdvances)

        {

            List<SalaryAdvance> updatedRecords = new List<SalaryAdvance>();



            foreach (var salaryAdvance in salaryAdvances)

            {

                var existingRecord = await _oBMSDbContext.SalaryAdvances

                    .FirstOrDefaultAsync(s => s.ID == salaryAdvance.ID);



                if (existingRecord != null)

                {

                    // Update properties

                    existingRecord.EmployeeID = salaryAdvance.EmployeeID;

                    existingRecord.AdvanceTakenDate = salaryAdvance.AdvanceTakenDate;

                    existingRecord.AdvanceDate = salaryAdvance.AdvanceDate;

                    existingRecord.VoucherNo = salaryAdvance.VoucherNo;

                    existingRecord.Amount = salaryAdvance.Amount;

                    existingRecord.NoOfInstallments = salaryAdvance.NoOfInstallments;

                    existingRecord.PaymentType = salaryAdvance.PaymentType;

                    existingRecord.Particulars = salaryAdvance.Particulars;

                    existingRecord.TransType = salaryAdvance.TransType;

                    existingRecord.IsDeleted = salaryAdvance.IsDeleted;

                    existingRecord.LastUpdate = DateTime.Now;

                    existingRecord.LastUpdatedBy = salaryAdvance.LastUpdatedBy;



                    _oBMSDbContext.Update(existingRecord);

                    updatedRecords.Add(existingRecord);

                }

                else

                {

                    _oBMSDbContext.Add(salaryAdvance);

                    updatedRecords.Add(salaryAdvance);

                }

            }



            await _oBMSDbContext.SaveChangesAsync();

            return updatedRecords;

        }



        public List<EmployeeDailyAdvanceRow> GetDailyAdvanceList(DateTime advanceDate, int employeeID, int advanceType)

        {

            if (employeeID > 0)

            {



                int noOfDays = DateTime.DaysInMonth(advanceDate.Year, advanceDate.Month);

                List<EmployeeDailyAdvanceRow> employeeDailyAdvanceRowList = new List<EmployeeDailyAdvanceRow>();

                int startDay = 1;



                var employee = _oBMSDbContext.Employees.Where(e => e.EMP_ID == employeeID).SingleOrDefault().EMP_CODE;



                if (employee != null)

                {

                    var employment = Get(employee);

                    if (employment.EMPPAY_DATE_RESIGNED?.Year != 1)

                    {

                        if (advanceDate > employment.EMPPAY_DATE_RESIGNED)

                        {

                            startDay = 0;

                            //noOfDays = 0;

                        }

                        else if ((advanceDate.Month == employment.EMPPAY_DATE_RESIGNED?.Month) && (advanceDate.Year == employment.EMPPAY_DATE_RESIGNED?.Year))

                        {

                            noOfDays = (int)(employment.EMPPAY_DATE_RESIGNED?.Day);

                        }

                    }



                    if ((advanceDate.Month == employment.EMPPAY_DATE_JOINED.Month) && (advanceDate.Year == employment.EMPPAY_DATE_JOINED.Year))

                    {

                        startDay = employment.EMPPAY_DATE_JOINED.Day;

                    }



                    for (int i = startDay; i <= noOfDays; i++)

                    {

                        employeeDailyAdvanceRowList.Add(new EmployeeDailyAdvanceRow(0, 0, i, 0, "", 0, "", "", 0, false, "", DateTime.MinValue, DateTime.MinValue));

                    }



                    var salaryAdvances = _oBMSDbContext.SalaryAdvances

                        .Where(sa => sa.TransType == advanceType &&

                                     sa.AdvanceDate.Month == advanceDate.Month &&

                                     sa.AdvanceDate.Year == advanceDate.Year &&

                                     sa.EmployeeID == employeeID &&

                                     !sa.IsDeleted)

                        .ToList();



                    foreach (var salaryAdvance in salaryAdvances)

                    {

                        int dayIndex = salaryAdvance.AdvanceDate.Day - 1; // Subtract 1 to convert day number to zero-based index

                        employeeDailyAdvanceRowList[dayIndex].ID = salaryAdvance.ID;

                        employeeDailyAdvanceRowList[dayIndex].Day = salaryAdvance.AdvanceDate.Day;

                        employeeDailyAdvanceRowList[dayIndex].Amount = salaryAdvance.Amount;

                        employeeDailyAdvanceRowList[dayIndex].EmployeeID = salaryAdvance.EmployeeID;

                        employeeDailyAdvanceRowList[dayIndex].TransType = salaryAdvance.TransType;

                        employeeDailyAdvanceRowList[dayIndex].Particulars = salaryAdvance.Particulars;

                        employeeDailyAdvanceRowList[dayIndex].IsDeleted = salaryAdvance.IsDeleted;

                        employeeDailyAdvanceRowList[dayIndex].LastUpdatedBy = salaryAdvance.LastUpdatedBy;

                        employeeDailyAdvanceRowList[dayIndex].VoucherNo = salaryAdvance.VoucherNo;

                        employeeDailyAdvanceRowList[dayIndex].PaymentType = salaryAdvance.PaymentType;

                        employeeDailyAdvanceRowList[dayIndex].NoOfInstallments = salaryAdvance.NoOfInstallments;

                        employeeDailyAdvanceRowList[dayIndex].AdvanceDate = salaryAdvance.AdvanceDate;

                        employeeDailyAdvanceRowList[dayIndex].AdvanceTakenDate = salaryAdvance.AdvanceTakenDate;

                    }

                    return employeeDailyAdvanceRowList;

                }

            }



            return new List<EmployeeDailyAdvanceRow>(); // Return empty list if employee is not found



        }



        public EmploymentDetails Get(string employeeNo)

        {

            var employmentDetails = _oBMSDbContext.EmploymentDetails.FirstOrDefault(ed => ed.EMPPAY_CODE == employeeNo);



            if (employmentDetails != null)

            {

                return employmentDetails;

            }

            return employmentDetails;

        }

        public async Task<string?> GetEmployeeNoAsync(int employeeId)

        {

            var employee = await _oBMSDbContext.Employees

                .Where(e => e.EMP_ID == employeeId)

                .Select(e => e.EMP_CODE)

                .FirstOrDefaultAsync();



            return employee;

        }



        public async Task<IEnumerable<SalaryAdvance>> GetSalaryAdvancesAsync(DateTime advanceDate, int employeeId, int transType)

        {

            return await _oBMSDbContext.SalaryAdvances

                .Where(sa => sa.AdvanceDate == advanceDate &&

                             sa.EmployeeID == employeeId &&

                             sa.TransType == transType &&

                             !sa.IsDeleted)

                .ToListAsync();

        }

        public async Task<bool> DeleteSalaryAdvanceAsync(int salaryAdvanceID, string currentUser)

        {

            try

            {

                var salaryAdvance = await _oBMSDbContext.SalaryAdvances.FirstOrDefaultAsync(x => x.ID == salaryAdvanceID && !x.IsDeleted);

                if (salaryAdvance == null)

                    return false;



                salaryAdvance.IsDeleted = true;

                salaryAdvance.LastUpdatedBy = currentUser;

                salaryAdvance.LastUpdate = DateTime.Now;



                await _oBMSDbContext.SaveChangesAsync();

                return true;

            }

            catch

            {

                throw;

            }

        }

        #region Attendance



        public ActionResult<IEnumerable<ClientMaster>> GetClients(DateTime period, string branchCode)
        {

            IQueryable<ClientMaster> query = _oBMSDbContext.ClientMasters;



            if (!string.IsNullOrEmpty(branchCode))
            {

                query = query.Where(c => c.Branch == branchCode);

            }



            query = query.Where(c => c.SuperClientCode != null);

            if (period != null)
            {
                // Get the first day of the attendance period month
                var periodFirstDay = new DateTime(period.Year, period.Month, 1);
                var periodLastDay = new DateTime(period.Year, period.Month, DateTime.DaysInMonth(period.Year, period.Month)); // kept as calendar-month boundary for agreement filter

                // Include Active clients, plus Inactive clients whose termination falls within the selected period month
                query = query.Where(c => c.Status == "Active" ||
                                         (c.Status == "Inactive" &&
                                          _oBMSDbContext.TerminatedAgreements.Any(ta => ta.Client == c.Code &&
                                              ta.TerminationDate >= periodFirstDay && ta.TerminationDate <= periodLastDay)));

                query = query.Where(c => _oBMSDbContext.Agreements.Any(a => a.Client == c.Code && a.AgreementDate <= period) &&
                                          !_oBMSDbContext.TerminatedAgreements.Any(ta => ta.Client == c.Code && ta.TerminationDate < periodFirstDay));

            }
            else
            {
                query = query.Where(c => c.Status == "Active");
            }

            query = query.OrderBy(c => c.Name);

            List<ClientMaster> clients = query.ToList();

            return clients;

        }



        public async Task<ClientMaster> GetClientByCode(string clientCode)

        {

            return await _oBMSDbContext.ClientMasters

                .Where(c => c.Code == clientCode)

                .FirstOrDefaultAsync();

        }

        public async Task<List<ClientMaster>> GetClientsByBranch(string branchCode)
        {
            return await _oBMSDbContext.ClientMasters
                .Where(c => c.Branch == branchCode && c.Status == "Active")
                .ToListAsync();
        }



        public Attendance AttendanceByEmployeeID(DateTime Period, int employeeID)

        {

            try

            {

                var attendance = _oBMSDbContext.Attendances

                    .Where(e => e.EmployeeID == employeeID && e.Period == Period)

                    .FirstOrDefault();



                if (attendance != null)

                {

                    return attendance;

                }

                return attendance;

            }

            catch

            {

                throw;

            }

        }



        public async Task<List<AttendanceDetails>> AttendanceDetailsByID(int Id)

        {

            var result = _oBMSDbContext.AttendanceDetails

                .Where(e => e.AttendanceID == Id).ToList();



            return new List<AttendanceDetails>(result);





        }

        public async Task<List<AttendanceDetails>> GetAttendanceDetailsList(int AttendanceID)

        {

            try

            {

                var attendancedetailsfactoryList = (

                    from ad in _oBMSDbContext.AttendanceDetails

                    join a in _oBMSDbContext.Attendances on ad.AttendanceID equals a.ID

                    join c1 in _oBMSDbContext.ClientMasters on new { a.Branch, Code = ad.Client } equals new { c1.Branch, c1.Code } into clientJoin

                    from c1 in clientJoin.DefaultIfEmpty()

                    join c2 in _oBMSDbContext.ClientMasters on new { a.Branch, Code = ad.OTClient } equals new { c2.Branch, c2.Code } into otClientJoin

                    from c2 in otClientJoin.DefaultIfEmpty()

                    where ad.AttendanceID == AttendanceID

                    select new AttendanceDetails

                    {

                        ID = ad.ID,

                        AttendanceID = ad.AttendanceID,

                        AttendanceDate = ad.AttendanceDate,

                        Client = ad.Client ?? "",

                        TimeStart = ad.TimeStart ?? DateTime.MinValue,

                        TimeEnd = ad.TimeEnd ?? DateTime.MinValue,

                        OTClient = ad.OTClient ?? "",

                        OTTimeStart = ad.OTTimeStart ?? DateTime.MinValue,

                        OTTimeEnd = ad.OTTimeEnd ?? DateTime.MinValue,

                        Type = ad.Type,

                        LastUpdate = ad.LastUpdate

                    }).ToList();



                return attendancedetailsfactoryList;



            }

            catch

            {

                throw;

            }

        }



        public async Task<List<SalaryAttendenceDto>> GetEmployeeDetails(string branchCode, string employeeNo)

        {

            //var employee = _oBMSDbContext.Employees

            //    .Where(e => e.EMP_CODE == employeeNo && e.EMP_BRANCH_CODE == branchCode)

            //    .FirstOrDefault();

            //return employee;



            var result = await (from employee in _oBMSDbContext.Employees

                                join salaryDetails in _oBMSDbContext.EmployeeSalaryDetails on employee.EMP_CODE equals salaryDetails.EMPFL_CODE

                                join employeement in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employeement.EMPPAY_CODE

                                join salarystructure in _oBMSDbContext.SalaryStructures on employeement.SALARYLAB equals salarystructure.SalaryId into salaryStructGroup

                                from salarystructure in salaryStructGroup.DefaultIfEmpty()

                                join salaryAdvance in _oBMSDbContext.SalaryAdvances on employee.EMP_ID equals salaryAdvance.EmployeeID into advancesGroup

                                from salaryAdvance in advancesGroup.DefaultIfEmpty()

                                where employee.EMP_CODE == employeeNo && employee.EMP_BRANCH_CODE == branchCode

                                select new SalaryAttendenceDto

                                {

                                    ID = (salaryAdvance != null) ? salaryAdvance.ID : 0,

                                    EMP_ID = employee.EMP_ID,

                                    EMP_NAME = employee.EMP_NAME,

                                    EMP_CODE = employee.EMP_CODE,

                                    // Indian Compliance Fields

                                    AadhaarNumber = employee.AadhaarNumber,

                                    PANNumber = employee.PANNumber,

                                    PFAccountNumber = employee.PFAccountNumber,

                                    ESINumber = employee.ESINumber,

                                    UANNumber = employee.UANNumber,

                                    PFDETECT = salaryDetails.EPFDETECT,

                                    ESIDETECT = salaryDetails.SOCSODETECT,

                                    INCOMETAXDETECT = salaryDetails.INCOMETAXDETECT,

                                    // Bank Details

                                    EMPFL_BANK = salaryDetails.EMPFL_BANK,

                                    EMPFL_BK_ACCNO = salaryDetails.EMPFL_BK_ACCNO,

                                    BankIFSC = employee.BankIFSC,

                                    BankName = employee.BankName,

                                    PAYMODE = salaryDetails.PAYMODE,

                                    // Other fields

                                    EMP_DATE_OF_BIRTH = employee.EMP_DATE_OF_BIRTH,

                                    SalaryStructure = employee.NewSalaryStructure.ToString(),

                                    Amount = (salaryAdvance != null) ? salaryAdvance.Amount : 0,

                                    Particulars = (salaryAdvance != null) ? salaryAdvance.Particulars : "",

                                    EMPPAY_DATE_JOINED = employeement.EMPPAY_DATE_JOINED,

                                    EMPPAY_DATE_RESIGNED = employeement.EMPPAY_DATE_RESIGNED,

                                    ATTENDANCEALLOWANCE = employeement.ATTENDANCEALLOWANCE,

                                    SpecialAllowance = employeement.SpecialAllowance,

                                    EMPPAY_BASIC_RATE = employeement.EMPPAY_BASIC_RATE,

                                    Name = salarystructure != null ? salarystructure.Name : null,

                                    // Working Days and Allowance Configuration

                                    AttendanceAllowanceWorkingDays = employeement.AttendanceAllowanceWorkingDays,

                                    WorkingDays = salarystructure != null ? salarystructure.WorkingDays : 26,

                                    AttendanceAllowanceFollowCalendar = employeement.AttendanceAllowanceFollowCalendar,

                                }).ToListAsync();



            return new List<SalaryAttendenceDto>(result);





        }



        public int CalculateAge(DateTime birthDate)

        {

            int age = DateTime.Now.Year - birthDate.Year;

            int num;



            if (DateTime.Now.Month >= birthDate.Month)

            {

                DateTime now = DateTime.Now;



                if (now.Month == birthDate.Month)

                {

                    now = DateTime.Now;

                    num = now.Day >= birthDate.Day ? 1 : 0;

                }

                else

                {

                    num = 1;

                }

            }

            else

            {

                num = 0;

            }



            if (num == 0)

            {

                --age;

            }



            return age;

        }

        public int GetAnnualLeave(int employeeID, DateTime period)

        {

            try

            {

                var leaveQuery = from attendanceDetail in _oBMSDbContext.AttendanceDetails

                                 join attendance in _oBMSDbContext.Attendances on attendanceDetail.AttendanceID equals attendance.ID

                                 join employee in _oBMSDbContext.Employees on attendance.EmployeeID equals employee.EMP_ID

                                 join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

                                 where attendanceDetail.AttendanceDate < period && attendanceDetail.Type == 8 && employee.EMP_ID == employeeID

                                 select new

                                 {

                                     LeaveTaken = 1, // Assuming 1 for LeaveTaken

                                     LeaveAvailable = 0

                                 };



                var leaveAvailableQuery = from leaveSystem in _oBMSDbContext.LeaveSystems

                                          join employee in _oBMSDbContext.Employees on leaveSystem.LS_ID equals employee.EMP_ID

                                          join employmentDetail in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetail.EMPPAY_CODE

                                          where employee.EMP_ID == employeeID

                                          select new

                                          {

                                              LeaveTaken = 0,

                                              LeaveAvailable = leaveSystem.al0to1 * ((period.Year - employmentDetail.EMPPAY_DATE_JOINED.Year == 0) ? DateDiffInMonths(employmentDetail.EMPPAY_DATE_JOINED, period) / 12 : 0)

                                              // Add similar conditions for other cases

                                          };



                //var result = leaveQuery.Union(leaveAvailableQuery).GroupBy(x => 1)

                //                          .Select(g => new

                //                          {

                //                              LeaveTaken = g.Sum(x => x.LeaveTaken),

                //                              LeaveAvailable = g.Sum(x => x.LeaveAvailable)

                //                          })

                //                          .FirstOrDefault();



                var result = new

                {

                    LeaveTaken = leaveQuery.Sum(x => x.LeaveTaken),

                    LeaveAvailable = leaveAvailableQuery.Sum(x => x.LeaveAvailable)

                };



                //return (int)result.LeaveAvailable - result.LeaveTaken;



                return result != null ? ((int)result.LeaveAvailable - result.LeaveTaken) : 0;



            }

            catch

            {

                throw;

            }

        }



        public int DateDiffInMonths(DateTime startDate, DateTime endDate)

        {

            return (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

        }



        public async Task<ActionResult> SaveAndUpdateAttendance(Attendance attendanceModel, List<AttendanceDetails> attendanceDetails)

        {

            try

            {

                if (attendanceModel != null)

                {

                    var existingattendance = _oBMSDbContext.Attendances.Where(a => a.ID == attendanceModel.ID).SingleOrDefault();

                    if (existingattendance != null)

                    {

                        existingattendance.EmployeeID = attendanceModel.EmployeeID;

                        existingattendance.Period = attendanceModel.Period;

                        existingattendance.Branch = attendanceModel.Branch;

                        existingattendance.Shift2Type = attendanceModel.Shift2Type;

                        existingattendance.Shift2Rate = attendanceModel.Shift2Rate;

                        existingattendance.Bonus = attendanceModel.Bonus;

                        existingattendance.AllowanceDeduction = attendanceModel.AllowanceDeduction;

                        existingattendance.SpecialAllowanceDeduction = attendanceModel.SpecialAllowanceDeduction;

                        existingattendance.LastUpdate = attendanceModel.LastUpdate;

                        existingattendance.LastUpdatedBy = attendanceModel.LastUpdatedBy;



                        _oBMSDbContext.Update(existingattendance);

                        await _oBMSDbContext.SaveChangesAsync();



                        List<AttendanceDetails> updatedRecords = new List<AttendanceDetails>();

                        foreach (var attendanceDetail in attendanceDetails)

                        {

                            var existingRecord = await _oBMSDbContext.AttendanceDetails

                                .FirstOrDefaultAsync(s => s.ID == attendanceDetail.ID);



                            if (existingRecord != null)

                            {

                                // Update properties

                                existingRecord.AttendanceID = attendanceDetail.AttendanceID;

                                existingRecord.AttendanceDate = attendanceDetail.AttendanceDate;

                                existingRecord.Client = attendanceDetail.Client;

                                existingRecord.TimeStart = attendanceDetail.TimeStart;

                                existingRecord.TimeEnd = attendanceDetail.TimeEnd;

                                existingRecord.OTClient = attendanceDetail.OTClient;

                                existingRecord.OTTimeStart = attendanceDetail.OTTimeStart;

                                existingRecord.OTTimeEnd = attendanceDetail.OTTimeEnd;

                                existingRecord.Type = attendanceDetail.Type;

                                existingRecord.LastUpdate = DateTime.Now;

                                existingRecord.LastUpdatedBy = attendanceDetail.LastUpdatedBy;



                                _oBMSDbContext.Update(existingRecord);

                                updatedRecords.Add(existingRecord);

                            }

                            else

                            {

                                attendanceDetail.AttendanceID = existingattendance.ID;

                                _oBMSDbContext.Add(attendanceDetail);

                                updatedRecords.Add(attendanceDetail);

                            }

                        }

                        await _oBMSDbContext.SaveChangesAsync();

                    }

                    else

                    {

                        _oBMSDbContext.Add(attendanceModel);

                        await _oBMSDbContext.SaveChangesAsync();



                        List<AttendanceDetails> updatedRecords = new List<AttendanceDetails>();

                        foreach (var attendanceDetail in attendanceDetails)

                        {

                            var existingRecord = await _oBMSDbContext.AttendanceDetails

                                .FirstOrDefaultAsync(s => s.ID == attendanceDetail.ID);



                            if (existingRecord != null)

                            {

                                // Update properties

                                existingRecord.AttendanceID = attendanceDetail.AttendanceID;

                                existingRecord.AttendanceDate = attendanceDetail.AttendanceDate;

                                existingRecord.Client = attendanceDetail.Client;

                                existingRecord.TimeStart = attendanceDetail.TimeStart;

                                existingRecord.TimeEnd = attendanceDetail.TimeEnd;

                                existingRecord.OTClient = attendanceDetail.OTClient;

                                existingRecord.OTTimeStart = attendanceDetail.OTTimeStart;

                                existingRecord.OTTimeEnd = attendanceDetail.OTTimeEnd;

                                existingRecord.Type = attendanceDetail.Type;

                                existingRecord.LastUpdate = DateTime.Now;

                                existingRecord.LastUpdatedBy = attendanceDetail.LastUpdatedBy;



                                _oBMSDbContext.Update(existingRecord);

                                updatedRecords.Add(existingRecord);

                            }

                            else

                            {

                                attendanceDetail.AttendanceID = attendanceModel.ID;

                                _oBMSDbContext.Add(attendanceDetail);

                                updatedRecords.Add(attendanceDetail);

                            }

                        }

                        await _oBMSDbContext.SaveChangesAsync();

                    }



                }

            }

            catch (Exception ex)

            {

                return new BadRequestObjectResult($"Error: {ex.Message}");

            }



            return new OkObjectResult(new { Success = "Success", Message = "Attendance saved / updated successfully" });

        }

        public async Task<AttendanceBulkUploadResultDto> SaveSimplifiedBulkAttendance(List<AttendanceBulkUploadDto> attendanceData, string currentUser)
        {
            var startTime = DateTime.UtcNow;
            var result = new AttendanceBulkUploadResultDto
            {
                TotalRecords = attendanceData.Count,
                Success = true,
                Message = string.Empty,
                Statistics = new AttendanceBulkUploadStatisticsDto
                {
                    TotalProcessed = attendanceData.Count,
                    ProcessedAt = startTime
                }
            };

            var strategy = _oBMSDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _oBMSDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Pre-validation: Check for duplicate employees in upload
                    var duplicateEmployees = attendanceData
                        .GroupBy(a => new { a.EmployeeCode, a.EmployeeName })
                        .Where(g => g.Count() > 1)
                        .ToList();

                    if (duplicateEmployees.Any())
                    {
                        result.Success = false;
                        var duplicateList = duplicateEmployees
                            .Select(g => $"{g.Key.EmployeeCode} ({g.Key.EmployeeName})")
                            .ToList();
                        
                        result.Message = $"Upload aborted: Duplicate employee entries found. Each employee should appear only once.";
                        result.Errors.Add(new AttendanceBulkUploadErrorDto
                        {
                            ErrorMessage = string.Join(", ", duplicateList),
                            ErrorType = "DuplicateEmployee"
                        });
                        
                        await transaction.RollbackAsync();
                        return result;
                    }

                    // Process each attendance record
                    foreach (var attendanceDto in attendanceData)
                    {
                        try
                        {
                            // Get employee ID from employee code
                            var employee = await _oBMSDbContext.Employees
                                .FirstOrDefaultAsync(e => e.EMP_CODE == attendanceDto.EmployeeCode);

                            if (employee == null)
                            {
                                result.FailedRecords++;
                                result.Statistics.DatabaseErrorCount++;
                                result.Errors.Add(new AttendanceBulkUploadErrorDto
                                {
                                    RowNumber = result.SuccessfulRecords + result.FailedRecords,
                                    EmployeeCode = attendanceDto.EmployeeCode,
                                    EmployeeName = attendanceDto.EmployeeName,
                                    ErrorMessage = $"Employee code '{attendanceDto.EmployeeCode}' not found in system. " +
                                        "Please ensure the employee code is correct and the employee exists in the system.",
                                    ErrorType = "EmployeeNotFound",
                                    Timestamp = DateTime.UtcNow
                                });
                                result.ProblematicRows.Add(result.SuccessfulRecords + result.FailedRecords);
                                continue;
                            }

                            // Auto-map employee's assigned client using Shortname (EMP_CLIENT holds the client Code)
                            var employeeClientCode = employee.EMP_CLIENT ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(employeeClientCode))
                            {
                                var clientRecord = await _oBMSDbContext.ClientMasters
                                    .FirstOrDefaultAsync(c => c.Code == employeeClientCode);
                                var employeeClientShortname = !string.IsNullOrWhiteSpace(clientRecord?.Shortname)
                                    ? clientRecord.Shortname
                                    : employeeClientCode;

                                foreach (var dayDetail in attendanceDto.DailyAttendance)
                                {
                                    if (string.IsNullOrWhiteSpace(dayDetail.Client))
                                        dayDetail.Client = employeeClientShortname;
                                }
                            }

                            // Resolve period key via AttendancePeriodService (custom or calendar month)
                            var periodInfo = await _attendancePeriodService.GetAttendancePeriodAsync(
                                attendanceDto.ClientCode ?? employeeClientCode,
                                attendanceDto.Period.Year,
                                attendanceDto.Period.Month);
                            var period = periodInfo.PeriodKey;

                            // Check if attendance record exists for this employee and period (year+month match)
                            var existingAttendance = await _oBMSDbContext.Attendances
                                .FirstOrDefaultAsync(a => a.EmployeeID == employee.EMP_ID &&
                                                          a.Period.Year == period.Year &&
                                                          a.Period.Month == period.Month);

                            if (existingAttendance != null)
                            {
                                // Update existing attendance record
                                try
                                {
                                    existingAttendance.Branch = attendanceDto.BranchCode;
                                    existingAttendance.Shift2Type = attendanceDto.Shift2Type;
                                    existingAttendance.Shift2Rate = attendanceDto.Shift2Rate;
                                    existingAttendance.Bonus = attendanceDto.Bonus;
                                    existingAttendance.AllowanceDeduction = attendanceDto.AllowanceDeduction;
                                    existingAttendance.SpecialAllowanceDeduction = attendanceDto.SpecialAllowanceDeduction;
                                    existingAttendance.LastUpdate = DateTime.Now;
                                    existingAttendance.LastUpdatedBy = currentUser;

                                    _oBMSDbContext.Update(existingAttendance);
                                    await _oBMSDbContext.SaveChangesAsync();

                                    // Remove existing attendance details
                                    var existingDetails = await _oBMSDbContext.AttendanceDetails
                                        .Where(ad => ad.AttendanceID == existingAttendance.ID)
                                        .ToListAsync();

                                    if (existingDetails.Any())
                                    {
                                        _oBMSDbContext.AttendanceDetails.RemoveRange(existingDetails);
                                        await _oBMSDbContext.SaveChangesAsync();
                                    }

                                    // Add new attendance details
                                    int detailsAdded = 0;
                                    foreach (var dayDetail in attendanceDto.DailyAttendance)
                                    {
                                        try
                                        {
                                            var attendanceDetail = new AttendanceDetails
                                            {
                                                AttendanceID = existingAttendance.ID,
                                                AttendanceDate = dayDetail.AttendanceDate,
                                                Client = dayDetail.Client ?? string.Empty,
                                                TimeStart = dayDetail.TimeStart,
                                                TimeEnd = dayDetail.TimeEnd,
                                                OTClient = dayDetail.OTClient ?? string.Empty,
                                                OTTimeStart = dayDetail.OTTimeStart,
                                                OTTimeEnd = dayDetail.OTTimeEnd,
                                                Type = dayDetail.TypeID > 0 
                                                    ? dayDetail.TypeID 
                                                    : (AttendanceCodeMapping.GetAttendanceType(dayDetail.Remarks ?? "P")),
                                                LastUpdate = DateTime.Now,
                                                LastUpdatedBy = currentUser
                                            };

                                            _oBMSDbContext.Add(attendanceDetail);
                                            detailsAdded++;
                                        }
                                        catch (Exception detEx)
                                        {
                                            throw new InvalidOperationException(
                                                $"Failed to add attendance detail for {dayDetail.AttendanceDate:yyyy-MM-dd}: {detEx.Message}", 
                                                detEx);
                                        }
                                    }

                                    await _oBMSDbContext.SaveChangesAsync();

                                    result.SuccessfulRecords++;
                                    result.SuccessRecords.Add(new AttendanceBulkUploadSuccessDto
                                    {
                                        RowNumber = result.SuccessfulRecords,
                                        EmployeeCode = attendanceDto.EmployeeCode,
                                        EmployeeName = attendanceDto.EmployeeName,
                                        AttendanceID = existingAttendance.ID,
                                        Message = $"Attendance record updated with {detailsAdded} day details"
                                    });
                                }
                                catch (DbUpdateException dbEx)
                                {
                                    result.FailedRecords++;
                                    result.Statistics.DatabaseErrorCount++;
                                    result.Errors.Add(new AttendanceBulkUploadErrorDto
                                    {
                                        EmployeeCode = attendanceDto.EmployeeCode,
                                        EmployeeName = attendanceDto.EmployeeName,
                                        ErrorMessage = $"Database error while updating attendance: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                        ErrorType = "DatabaseUpdate",
                                        Timestamp = DateTime.UtcNow
                                    });
                                }
                            }
                            else
                            {
                                // Create new attendance record
                                try
                                {
                                    var newAttendance = new Attendance
                                    {
                                        EmployeeID = employee.EMP_ID,
                                        Period = period,
                                        Branch = attendanceDto.BranchCode,
                                        Shift2Type = attendanceDto.Shift2Type,
                                        Shift2Rate = attendanceDto.Shift2Rate,
                                        Bonus = attendanceDto.Bonus,
                                        AllowanceDeduction = attendanceDto.AllowanceDeduction,
                                        SpecialAllowanceDeduction = attendanceDto.SpecialAllowanceDeduction,
                                        LastUpdate = DateTime.Now,
                                        LastUpdatedBy = currentUser
                                    };

                                    _oBMSDbContext.Add(newAttendance);
                                    await _oBMSDbContext.SaveChangesAsync();

                                    // Add attendance details
                                    int detailsAdded = 0;
                                    foreach (var dayDetail in attendanceDto.DailyAttendance)
                                    {
                                        try
                                        {
                                            var attendanceDetail = new AttendanceDetails
                                            {
                                                AttendanceID = newAttendance.ID,
                                                AttendanceDate = dayDetail.AttendanceDate,
                                                Client = dayDetail.Client ?? string.Empty,
                                                TimeStart = dayDetail.TimeStart,
                                                TimeEnd = dayDetail.TimeEnd,
                                                OTClient = dayDetail.OTClient ?? string.Empty,
                                                OTTimeStart = dayDetail.OTTimeStart,
                                                OTTimeEnd = dayDetail.OTTimeEnd,
                                                Type = dayDetail.TypeID > 0 
                                                    ? dayDetail.TypeID 
                                                    : (AttendanceCodeMapping.GetAttendanceType(dayDetail.Remarks ?? "P")),
                                                LastUpdate = DateTime.Now,
                                                LastUpdatedBy = currentUser
                                            };

                                            _oBMSDbContext.Add(attendanceDetail);
                                            detailsAdded++;
                                        }
                                        catch (Exception detEx)
                                        {
                                            throw new InvalidOperationException(
                                                $"Failed to add attendance detail for {dayDetail.AttendanceDate:yyyy-MM-dd}: {detEx.Message}",
                                                detEx);
                                        }
                                    }

                                    await _oBMSDbContext.SaveChangesAsync();

                                    result.SuccessfulRecords++;
                                    result.SuccessRecords.Add(new AttendanceBulkUploadSuccessDto
                                    {
                                        RowNumber = result.SuccessfulRecords,
                                        EmployeeCode = attendanceDto.EmployeeCode,
                                        EmployeeName = attendanceDto.EmployeeName,
                                        AttendanceID = newAttendance.ID,
                                        Message = $"New attendance record created with {detailsAdded} day details"
                                    });
                                }
                                catch (DbUpdateException dbEx)
                                {
                                    result.FailedRecords++;
                                    result.Statistics.DatabaseErrorCount++;
                                    result.Errors.Add(new AttendanceBulkUploadErrorDto
                                    {
                                        EmployeeCode = attendanceDto.EmployeeCode,
                                        EmployeeName = attendanceDto.EmployeeName,
                                        ErrorMessage = $"Database error while creating attendance: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                        ErrorType = "DatabaseInsert",
                                        Timestamp = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            result.FailedRecords++;
                            result.Statistics.ParsingErrorCount++;
                            result.Errors.Add(new AttendanceBulkUploadErrorDto
                            {
                                RowNumber = result.SuccessfulRecords + result.FailedRecords,
                                EmployeeCode = attendanceDto.EmployeeCode,
                                EmployeeName = attendanceDto.EmployeeName,
                                ErrorMessage = $"Unexpected error processing attendance: {ex.Message}",
                                ErrorType = "ProcessingError",
                                StackTrace = ex.StackTrace,
                                Timestamp = DateTime.UtcNow
                            });
                            result.ProblematicRows.Add(result.SuccessfulRecords + result.FailedRecords);
                        }
                    }

                    await transaction.CommitAsync();
                    
                    // Calculate statistics
                    result.Statistics.SuccessCount = result.SuccessfulRecords;
                    result.Statistics.FailureCount = result.FailedRecords;
                    result.Statistics.ValidationErrorCount = result.Errors.Count(e => e.ErrorType == "ValidationError");
                    result.Statistics.SuccessRate = result.TotalRecords > 0 
                        ? (float)result.SuccessfulRecords / result.TotalRecords * 100 
                        : 0;
                    result.Statistics.ProcessingTime = DateTime.UtcNow - startTime;

                    result.Message = result.FailedRecords == 0
                        ? $"✓ Successfully processed all {result.SuccessfulRecords} records in {result.Statistics.ProcessingTime.TotalSeconds:F2} seconds"
                        : $"Partial success: {result.SuccessfulRecords} succeeded, {result.FailedRecords} failed out of {result.TotalRecords} records";
                    
                    result.Success = result.FailedRecords == 0;
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Success = false;
                    result.FailedRecords = result.TotalRecords;
                    result.Message = $"Bulk upload failed and rolled back. Error: {ex.Message}";
                    
                    result.Errors.Add(new AttendanceBulkUploadErrorDto
                    {
                        ErrorMessage = $"Transaction rolled back due to error: {ex.Message}",
                        ErrorType = "TransactionError",
                        StackTrace = ex.StackTrace,
                        Timestamp = DateTime.UtcNow
                    });

                    // Log error for audit
                    try
                    {
                        // You could add structured logging here if available
                        System.Diagnostics.Debug.WriteLine(
                            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: Bulk attendance upload failed for user {currentUser}. Error: {ex.Message}");
                    }
                    catch { /* Ignore logging errors */ }

                    return result;
                }
            });
        }

        public async Task<bool> DeleteAttendanceAsync(int dID)

        {

            try

            {

                // Fetch attendance details and delete

                var attendanceDetails = await _oBMSDbContext.AttendanceDetails

                    .Where(ad => ad.AttendanceID == dID)

                    .ToListAsync();



                if (attendanceDetails.Any())

                {

                    _oBMSDbContext.AttendanceDetails.RemoveRange(attendanceDetails);

                }



                // Fetch attendance and delete

                var attendance = await _oBMSDbContext.Attendances

                    .FirstOrDefaultAsync(a => a.ID == dID);



                if (attendance != null)

                {

                    // Extract employeeId and period from attendance

                    var employeeId = attendance.EmployeeID; // Replace with actual property name

                    var period = attendance.Period;         // Replace with actual property name



                    _oBMSDbContext.Attendances.Remove(attendance);



                    // Delete from AdvanceRepayment

                    var paySlipId = await _oBMSDbContext.PaySlips

                        .Where(p => p.EmployeeID == employeeId && p.Period == period)

                        .Select(p => p.ID)

                        .FirstOrDefaultAsync();



                    if (paySlipId != 0)

                    {

                        _oBMSDbContext.AdvanceRepayments.RemoveRange(

                            _oBMSDbContext.AdvanceRepayments.Where(ar => ar.PaySlipID == paySlipId)

                        );

                    }



                    // Delete from PaySlip

                    _oBMSDbContext.PaySlips.RemoveRange(

                        _oBMSDbContext.PaySlips.Where(p => p.EmployeeID == employeeId && p.Period == period)

                    );

                }



                // Save all changes in a single transaction

                await _oBMSDbContext.SaveChangesAsync();



                return true;

            }

            catch (Exception)

            {

                throw; // Properly log exceptions in production code

            }

        }



        public List<EmployeeDto> GetList(string branch, string employeeType, DateTime resignedDate, DateTime joinDate, DateTime attendancePeriod, string status)

        {

            var query = (from emp in _oBMSDbContext.Employees

                         join empDetails in _oBMSDbContext.EmploymentDetails on emp.EMP_CODE equals empDetails.EMPPAY_CODE

                         join salaryDetails in _oBMSDbContext.EmployeeSalaryDetails on emp.EMP_CODE equals salaryDetails.EMPFL_CODE

                         join attendance in _oBMSDbContext.Attendances on emp.EMP_ID equals attendance.EmployeeID

                         join attendanceDetails in _oBMSDbContext.AttendanceDetails on attendance.ID equals attendanceDetails.AttendanceID

                         where emp.HasTransfered == false

                         select new EmployeeDto

                         {

                             EMP_ID = emp.EMP_ID,

                             EMP_ROLE = emp.EMP_ROLE,

                             EMP_CODE = emp.EMP_CODE,

                             EMP_NAME = emp.EMP_NAME,

                             EMP_CLIENT = emp.EMP_CLIENT,

                             EMP_ADDRESS1 = emp.EMP_ADDRESS1,

                             EMP_ADDRESS2 = emp.EMP_ADDRESS2,

                             EMP_POST_CODE = emp.EMP_POST_CODE,

                             EMP_TOWN = emp.EMP_TOWN,

                             EMP_STATE = emp.EMP_STATE,

                             EMP_CITIZEN = emp.EMP_CITIZEN,

                             EMP_CHECKLIST = emp.EMP_CHECKLIST,

                             EMP_NATIONAL = emp.EMP_NATIONAL,

                             EMP_PHONE = emp.EMP_PHONE,

                             EMP_MOBILEPHONE = emp.EMP_MOBILEPHONE,

                             EMP_HGH_EDU = emp.EMP_HGH_EDU,

                             EMP_DATE_OF_BIRTH = emp.EMP_DATE_OF_BIRTH,

                             EMP_IC_OLD = emp.EMP_IC_OLD,

                             EMP_IC_NEW = emp.EMP_IC_NEW,

                             EMP_IC_COLOR = emp.EMP_IC_COLOR,

                             EMP_PASSPORT_NO = emp.EMP_PASSPORT_NO,

                             EMP_SEX = emp.EMP_SEX,

                             EMP_RACE = emp.EMP_RACE,

                             EMP_MARTIAL_STATUS = emp.EMP_MARTIAL_STATUS,

                             EMP_SPOUSE_NAME = emp.EMP_SPOUSE_NAME,

                             EMP_SP_IC = emp.EMP_SP_IC,

                             EMP_NO_CHILD = emp.EMP_NO_CHILD,

                             EMP_SP_WORK = emp.EMP_SP_WORK,

                             EMP_PER_NAME_CONTACT = emp.EMP_PER_NAME_CONTACT,

                             EMP_CONTACT_ADDRESS1 = emp.EMP_CONTACT_ADDRESS1,

                             EMP_CONTACT_ADDRESS2 = emp.EMP_CONTACT_ADDRESS2,

                             EMP_CONTACT_POST_CODE = emp.EMP_CONTACT_POST_CODE,

                             EMP_CONTACT_TOWN = emp.EMP_CONTACT_TOWN,

                             EMP_CONTACT_STATE = emp.EMP_CONTACT_STATE,

                             EMP_CONTACT_TELEPHONE = emp.EMP_CONTACT_TELEPHONE,

                             EMP_BRANCH_CODE = emp.EMP_BRANCH_CODE,

                             OldBranch = emp.OldBranch,

                             TransferDate = emp.TransferDate,

                             LASTUPDATE = emp.LASTUPDATE,

                             NewSalaryStructure = emp.NewSalaryStructure,

                             SalaryStructure1000_3h = emp.SalaryStructure1000_3h,

                             EMPPAY_DATE_RESIGNED = empDetails.EMPPAY_DATE_RESIGNED,

                             EMPPAY_DATE_JOINED = empDetails.EMPPAY_DATE_JOINED,

                             TMPGUARD = salaryDetails.TMPGUARD,

                             Period = attendance.Period



                         }).Distinct();



            // Apply filters based on parameters

            if (!string.IsNullOrEmpty(branch))

                query = query.Where(emp => emp.EMP_BRANCH_CODE == branch);



            if (!string.IsNullOrEmpty(employeeType))

            {

                if (employeeType.Contains("TEMPORARY"))

                    query = query.Where(emp => emp.TMPGUARD == false);

                if (employeeType == "TEMPORARYSTAFF")

                    query = query.Where(emp => emp.EMP_ROLE.Contains("STAFF"));

                else if (employeeType == "TEMPORARYGUARD")

                    query = query.Where(emp => emp.EMP_ROLE.Contains("GUARD"));

                else

                    query = query.Where(emp => emp.EMP_ROLE.Contains(employeeType));

            }



            if (resignedDate.Year != 1)

                //query = query.Where(empDetails => (empDetails.EMPPAY_DATE_RESIGNED ?? new DateTime(2100, 1, 1)) >= resignedDate);

                if (joinDate.Year != 1)

                    query = query.Where(emp => emp.EMPPAY_DATE_JOINED <= joinDate);

            if (attendancePeriod.Year != 1)

                query = query.Where(emp => emp.Period.Year == attendancePeriod.Year && emp.Period.Month == attendancePeriod.Month);



            query = query.OrderBy(emp => emp.EMP_NAME);



            return query.ToList();

        }

        //public List<EmployeeDto> getListByEmployee(string branch, string employeeType, DateTime resignedDate, DateTime joinDate, string status)

        //{

        //    var defaultResignedDate = new DateTime(2100, 1, 1);



        //    var query = from e in _oBMSDbContext.Employees

        //                     join ed in _oBMSDbContext.EmploymentDetails on e.EMP_CODE equals ed.EMPPAY_CODE

        //                     join esd in _oBMSDbContext.EmployeeSalaryDetails on e.EMP_CODE equals esd.EMPFL_CODE

        //                     where

        //                           (e.HasTransfered == false ||

        //                            (e.HasTransfered == true && e.TransferDate >= resignedDate))

        //                     select new EmployeeDto

        //                     {

        //                         EMP_ID = e.EMP_ID,

        //                         EMP_ROLE = e.EMP_ROLE,

        //                         EMP_CODE = e.EMP_CODE,

        //                         EMP_NAME = e.EMP_NAME,

        //                         EMP_CLIENT = e.EMP_CLIENT,

        //                         EMP_ADDRESS1 = e.EMP_ADDRESS1,

        //                         EMP_ADDRESS2 = e.EMP_ADDRESS2,

        //                         EMP_POST_CODE = e.EMP_POST_CODE,

        //                         EMP_TOWN = e.EMP_TOWN,

        //                         EMP_STATE = e.EMP_STATE,

        //                         EMP_NATIONAL = e.EMP_NATIONAL,

        //                         EMP_PHONE = e.EMP_PHONE,

        //                         EMP_MOBILEPHONE = e.EMP_MOBILEPHONE,

        //                         EMP_CITIZEN = e.EMP_CITIZEN,

        //                         EMP_CHECKLIST = e.EMP_CHECKLIST,

        //                         EMP_HGH_EDU = e.EMP_HGH_EDU,

        //                         EM_WORK_EXP = e.EM_WORK_EXP,

        //                         EMP_DATE_OF_BIRTH = e.EMP_DATE_OF_BIRTH,

        //                         EMP_IC_OLD = e.EMP_IC_OLD,

        //                         EMP_IC_NEW = e.EMP_IC_NEW,

        //                         EMP_IC_COLOR = e.EMP_IC_COLOR,

        //                         EMP_PASSPORT_NO = e.EMP_PASSPORT_NO,

        //                         EMP_SEX = e.EMP_SEX,

        //                         EMP_RACE = e.EMP_RACE,

        //                         EMP_MARTIAL_STATUS = e.EMP_MARTIAL_STATUS,

        //                         EMP_SPOUSE_NAME = e.EMP_SPOUSE_NAME,

        //                         EMP_SP_IC = e.EMP_SP_IC,

        //                         EMP_NO_CHILD = e.EMP_NO_CHILD,

        //                         EMP_SP_WORK = e.EMP_SP_WORK,

        //                         EMP_PER_NAME_CONTACT = e.EMP_PER_NAME_CONTACT,

        //                         EMP_CONTACT_ADDRESS1 = e.EMP_CONTACT_ADDRESS1,

        //                         EMP_CONTACT_ADDRESS2 = e.EMP_CONTACT_ADDRESS2,

        //                         EMP_CONTACT_POST_CODE = e.EMP_CONTACT_POST_CODE,

        //                         EMP_CONTACT_TOWN = e.EMP_CONTACT_TOWN,

        //                         EMP_CONTACT_STATE = e.EMP_CONTACT_STATE,

        //                         EMP_CONTACT_TELEPHONE = e.EMP_CONTACT_TELEPHONE,

        //                         EMP_BRANCH_CODE = e.EMP_BRANCH_CODE,

        //                         OldBranch = e.OldBranch,

        //                         TransferDate = e.TransferDate,

        //                         LASTUPDATE = e.LASTUPDATE,

        //                         NewSalaryStructure = e.NewSalaryStructure,

        //                         SalaryStructure1000_3h = e.SalaryStructure1000_3h

        //                     };

        //    // Apply filters based on parameters

        //    //if (!string.IsNullOrEmpty(status))

        //    //{

        //    //    if (status == "Active")

        //    //    {

        //    //        if (resignedDate.Month == 12)

        //    //        {

        //    //            query = query.Where(ed =>

        //    //                // Case for Active status with ResignedDate in December

        //    //                ed.EMPPAY_DATE_RESIGNED == null ||

        //    //                (ed.EMPPAY_DATE_RESIGNED.Value.Year >= resignedDate.Year - 1 &&

        //    //                 ed.EMPPAY_DATE_RESIGNED.Value.Month >= resignedDate.Month - 1)

        //    //            );

        //    //        }

        //    //        else

        //    //        {

        //    //            query = query.Where(ed =>

        //    //                // Case for Active status with ResignedDate not in December

        //    //                ed.EMPPAY_DATE_RESIGNED == null ||

        //    //                (ed.EMPPAY_DATE_RESIGNED.Value.Year >= resignedDate.Year &&

        //    //                 ed.EMPPAY_DATE_RESIGNED.Value.Month >= resignedDate.Month - 1)

        //    //            );

        //    //        }

        //    //    }

        //    //    else if (status == "Inactive")

        //    //    {

        //    //        // Case for Inactive status where EMPPAY_DATE_RESIGNED is not null

        //    //        query = query.Where(ed => ed.EMPPAY_DATE_RESIGNED != null);

        //    //    }

        //    //}



        //    if (!string.IsNullOrEmpty(employeeType))

        //        query = query.Where(e => e.EMP_ROLE.Contains(employeeType));



        //    if (!string.IsNullOrEmpty(branch))

        //        query = query.Where(e => e.EMP_BRANCH_CODE == branch);



        //    if (resignedDate.Year != 1)

        //        query = query.Where(ed =>

        //                (ed.EMPPAY_DATE_RESIGNED != null && ed.EMPPAY_DATE_RESIGNED >= resignedDate) ||

        //                defaultResignedDate >= resignedDate);



        //    if (joinDate.Year != 1)

        //        query = query.Where(ed => ed.EMPPAY_DATE_JOINED <= joinDate);



        //    query = query.OrderBy(emp => emp.EMP_NAME);



        //    return query.ToList();

        //}



        public List<EmployeeDto> getListByEmployee(string branch, string employeeType, DateTime resignedDate, DateTime joinDate, string status)

        {

            var defaultResignedDate = new DateTime(2100, 1, 1);

            var result = new List<EmployeeDto>();

            // Base query

            if (joinDate.Year != 1)

            {

                var query = from e in _oBMSDbContext.Employees

                            join ed in _oBMSDbContext.EmploymentDetails on e.EMP_CODE equals ed.EMPPAY_CODE

                            join esd in _oBMSDbContext.EmployeeSalaryDetails on e.EMP_CODE equals esd.EMPFL_CODE

                            where (e.HasTransfered == false ||

                                  (e.HasTransfered == true && e.TransferDate >= resignedDate))

                            select new

                            {

                                Employee = e,

                                EmploymentDetail = ed

                            };



                // Apply filters before projection



                if (!string.IsNullOrEmpty(status))

                {

                    if (status == "Active")

                    {

                        if (resignedDate.Month == 12)

                        {

                            query = query.Where(q =>

                           q.EmploymentDetail.EMPPAY_DATE_RESIGNED == null ||

                           (q.EmploymentDetail.EMPPAY_DATE_RESIGNED != null &&

                            q.EmploymentDetail.EMPPAY_DATE_RESIGNED.Value.Year >= resignedDate.Year - 1 &&

                            q.EmploymentDetail.EMPPAY_DATE_RESIGNED.Value.Month >= resignedDate.Month - 1));

                        }

                        else

                        {

                            query = query.Where(q =>

                            q.EmploymentDetail.EMPPAY_DATE_RESIGNED == null ||

                            (q.EmploymentDetail.EMPPAY_DATE_RESIGNED != null &&

                             q.EmploymentDetail.EMPPAY_DATE_RESIGNED.Value.Year >= resignedDate.Year &&

                             q.EmploymentDetail.EMPPAY_DATE_RESIGNED.Value.Month >= resignedDate.Month - 1));

                        }

                    }

                    else if (status == "Inactive")

                    {

                        query = query.Where(q => q.EmploymentDetail.EMPPAY_DATE_RESIGNED != null);

                    }

                }



                if (!string.IsNullOrEmpty(employeeType))

                    query = query.Where(q => q.Employee.EMP_ROLE == employeeType);



                if (!string.IsNullOrEmpty(branch))

                    query = query.Where(q => q.Employee.EMP_BRANCH_CODE == branch);



                if (resignedDate.Year != 1)

                    query = query.Where(q =>

                        q.EmploymentDetail.EMPPAY_DATE_RESIGNED == null || q.EmploymentDetail.EMPPAY_DATE_RESIGNED >= resignedDate);



                if (joinDate.Year != 1)

                    query = query.Where(q => q.EmploymentDetail.EMPPAY_DATE_JOINED <= joinDate);



                query = query.OrderBy(emp => emp.Employee.EMP_NAME);



                // Project the results into EmployeeDto

                result = query

                    .OrderBy(q => q.Employee.EMP_NAME)

                    .Select(q => new EmployeeDto

                    {

                        EMP_ID = q.Employee.EMP_ID,

                        EMP_ROLE = q.Employee.EMP_ROLE,

                        EMP_CODE = q.Employee.EMP_CODE,

                        EMP_NAME = q.Employee.EMP_NAME,

                        EMP_CLIENT = q.Employee.EMP_CLIENT,

                        EMP_ADDRESS1 = q.Employee.EMP_ADDRESS1,

                        EMP_ADDRESS2 = q.Employee.EMP_ADDRESS2,

                        EMP_POST_CODE = q.Employee.EMP_POST_CODE,

                        EMP_TOWN = q.Employee.EMP_TOWN,

                        EMP_STATE = q.Employee.EMP_STATE,

                        EMP_NATIONAL = q.Employee.EMP_NATIONAL,

                        EMP_PHONE = q.Employee.EMP_PHONE,

                        EMP_MOBILEPHONE = q.Employee.EMP_MOBILEPHONE,

                        EMP_CITIZEN = q.Employee.EMP_CITIZEN,

                        EMP_CHECKLIST = q.Employee.EMP_CHECKLIST,

                        EMP_HGH_EDU = q.Employee.EMP_HGH_EDU,

                        EM_WORK_EXP = q.Employee.EM_WORK_EXP,

                        EMP_DATE_OF_BIRTH = q.Employee.EMP_DATE_OF_BIRTH,

                        EMP_IC_OLD = q.Employee.EMP_IC_OLD,

                        EMP_IC_NEW = q.Employee.EMP_IC_NEW,

                        EMP_IC_COLOR = q.Employee.EMP_IC_COLOR,

                        EMP_PASSPORT_NO = q.Employee.EMP_PASSPORT_NO,

                        EMP_SEX = q.Employee.EMP_SEX,

                        EMP_RACE = q.Employee.EMP_RACE,

                        EMP_MARTIAL_STATUS = q.Employee.EMP_MARTIAL_STATUS,

                        EMP_SPOUSE_NAME = q.Employee.EMP_SPOUSE_NAME,

                        EMP_SP_IC = q.Employee.EMP_SP_IC,

                        EMP_NO_CHILD = q.Employee.EMP_NO_CHILD,

                        EMP_SP_WORK = q.Employee.EMP_SP_WORK,

                        EMP_PER_NAME_CONTACT = q.Employee.EMP_PER_NAME_CONTACT,

                        EMP_CONTACT_ADDRESS1 = q.Employee.EMP_CONTACT_ADDRESS1,

                        EMP_CONTACT_ADDRESS2 = q.Employee.EMP_CONTACT_ADDRESS2,

                        EMP_CONTACT_POST_CODE = q.Employee.EMP_CONTACT_POST_CODE,

                        EMP_CONTACT_TOWN = q.Employee.EMP_CONTACT_TOWN,

                        EMP_CONTACT_STATE = q.Employee.EMP_CONTACT_STATE,

                        EMP_CONTACT_TELEPHONE = q.Employee.EMP_CONTACT_TELEPHONE,

                        EMP_BRANCH_CODE = q.Employee.EMP_BRANCH_CODE,

                        OldBranch = q.Employee.OldBranch,

                        TransferDate = q.Employee.TransferDate,

                        LASTUPDATE = q.Employee.LASTUPDATE,

                        NewSalaryStructure = q.Employee.NewSalaryStructure,

                        SalaryStructure1000_3h = q.Employee.SalaryStructure1000_3h

                    })

                    .ToList();

            }

            else

            {

                var query = from e in _oBMSDbContext.Employees

                            join ed in _oBMSDbContext.EmploymentDetails on e.EMP_CODE equals ed.EMPPAY_CODE

                            join esd in _oBMSDbContext.EmployeeSalaryDetails on e.EMP_CODE equals esd.EMPFL_CODE

                            where (e.HasTransfered == false)

                            select new

                            {

                                Employee = e,

                                EmploymentDetail = ed

                            };



                // Apply filters before projection

                if (!string.IsNullOrEmpty(status))

                {

                    if (status == "Active")

                    {

                        if (resignedDate.Month == 12)

                        {

                            query = query.Where(q =>

                           q.EmploymentDetail.EMPPAY_DATE_RESIGNED == null ||

                           (q.EmploymentDetail.EMPPAY_DATE_RESIGNED != null &&

                            q.EmploymentDetail.EMPPAY_DATE_RESIGNED.Value.Year >= resignedDate.Year - 1 &&

                            q.EmploymentDetail.EMPPAY_DATE_RESIGNED.Value.Month >= resignedDate.Month - 1));

                        }

                        else

                        {

                            query = query.Where(q =>

                            q.EmploymentDetail.EMPPAY_DATE_RESIGNED == null ||

                            (q.EmploymentDetail.EMPPAY_DATE_RESIGNED != null &&

                             q.EmploymentDetail.EMPPAY_DATE_RESIGNED.Value.Year >= resignedDate.Year &&

                             q.EmploymentDetail.EMPPAY_DATE_RESIGNED.Value.Month >= resignedDate.Month - 1));

                        }

                    }

                    else if (status == "Inactive")

                    {

                        query = query.Where(q => q.EmploymentDetail.EMPPAY_DATE_RESIGNED != null);

                    }

                }



                if (!string.IsNullOrEmpty(employeeType))

                    query = query.Where(q => q.Employee.EMP_ROLE.Contains(employeeType));



                if (!string.IsNullOrEmpty(branch))

                    query = query.Where(q => q.Employee.EMP_BRANCH_CODE == branch);



                if (resignedDate.Year != 1)

                    query = query.Where(q =>

                        q.EmploymentDetail.EMPPAY_DATE_RESIGNED == null || q.EmploymentDetail.EMPPAY_DATE_RESIGNED >= resignedDate);



                if (joinDate.Year != 1)

                    query = query.Where(q => q.EmploymentDetail.EMPPAY_DATE_JOINED <= joinDate);



                query = query.OrderBy(emp => emp.Employee.EMP_NAME);



                // Project the results into EmployeeDto

                result = query

                    .OrderBy(q => q.Employee.EMP_NAME)

                    .Select(q => new EmployeeDto

                    {

                        EMP_ID = q.Employee.EMP_ID,

                        EMP_ROLE = q.Employee.EMP_ROLE,

                        EMP_CODE = q.Employee.EMP_CODE,

                        EMP_NAME = q.Employee.EMP_NAME,

                        EMP_CLIENT = q.Employee.EMP_CLIENT,

                        EMP_ADDRESS1 = q.Employee.EMP_ADDRESS1,

                        EMP_ADDRESS2 = q.Employee.EMP_ADDRESS2,

                        EMP_POST_CODE = q.Employee.EMP_POST_CODE,

                        EMP_TOWN = q.Employee.EMP_TOWN,

                        EMP_STATE = q.Employee.EMP_STATE,

                        EMP_NATIONAL = q.Employee.EMP_NATIONAL,

                        EMP_PHONE = q.Employee.EMP_PHONE,

                        EMP_MOBILEPHONE = q.Employee.EMP_MOBILEPHONE,

                        EMP_CITIZEN = q.Employee.EMP_CITIZEN,

                        EMP_CHECKLIST = q.Employee.EMP_CHECKLIST,

                        EMP_HGH_EDU = q.Employee.EMP_HGH_EDU,

                        EM_WORK_EXP = q.Employee.EM_WORK_EXP,

                        EMP_DATE_OF_BIRTH = q.Employee.EMP_DATE_OF_BIRTH,

                        EMP_IC_OLD = q.Employee.EMP_IC_OLD,

                        EMP_IC_NEW = q.Employee.EMP_IC_NEW,

                        EMP_IC_COLOR = q.Employee.EMP_IC_COLOR,

                        EMP_PASSPORT_NO = q.Employee.EMP_PASSPORT_NO,

                        EMP_SEX = q.Employee.EMP_SEX,

                        EMP_RACE = q.Employee.EMP_RACE,

                        EMP_MARTIAL_STATUS = q.Employee.EMP_MARTIAL_STATUS,

                        EMP_SPOUSE_NAME = q.Employee.EMP_SPOUSE_NAME,

                        EMP_SP_IC = q.Employee.EMP_SP_IC,

                        EMP_NO_CHILD = q.Employee.EMP_NO_CHILD,

                        EMP_SP_WORK = q.Employee.EMP_SP_WORK,

                        EMP_PER_NAME_CONTACT = q.Employee.EMP_PER_NAME_CONTACT,

                        EMP_CONTACT_ADDRESS1 = q.Employee.EMP_CONTACT_ADDRESS1,

                        EMP_CONTACT_ADDRESS2 = q.Employee.EMP_CONTACT_ADDRESS2,

                        EMP_CONTACT_POST_CODE = q.Employee.EMP_CONTACT_POST_CODE,

                        EMP_CONTACT_TOWN = q.Employee.EMP_CONTACT_TOWN,

                        EMP_CONTACT_STATE = q.Employee.EMP_CONTACT_STATE,

                        EMP_CONTACT_TELEPHONE = q.Employee.EMP_CONTACT_TELEPHONE,

                        EMP_BRANCH_CODE = q.Employee.EMP_BRANCH_CODE,

                        OldBranch = q.Employee.OldBranch,

                        TransferDate = q.Employee.TransferDate,

                        LASTUPDATE = q.Employee.LASTUPDATE,

                        NewSalaryStructure = q.Employee.NewSalaryStructure,

                        SalaryStructure1000_3h = q.Employee.SalaryStructure1000_3h

                    })

                    .ToList();

            }

            return result;

        }



        public async Task<List<SalaryAdvanceDto>> GetListByEmplyeeType(DateTime advanceDate, string branch, string employeeType, int transType, decimal advanceAmount, string race)

        {

            Console.WriteLine($"[PayrollRepository] GetListByEmplyeeType called with: advanceDate={advanceDate}, branch={branch}, employeeType={employeeType}, transType={transType}, advanceAmount={advanceAmount}, race={race}");

            

            if (race == "All")

            {

                race = string.Empty;

            }



            Console.WriteLine($"[PayrollRepository] Processing race: '{race}'");



            //var resultList = (from employee in _oBMSDbContext.Employees

            //                  join salaryDetails in _oBMSDbContext.EmployeeSalaryDetails on employee.EMP_CODE equals salaryDetails.EMPFL_CODE

            //                  join employmentDetails in _oBMSDbContext.EmploymentDetails on employee.EMP_CODE equals employmentDetails.EMPPAY_CODE

            //                  join salaryAdvance in _oBMSDbContext.SalaryAdvances.Where(sa => sa.IsDeleted == false &&

            //                                                                             sa.TransType == transType &&

            //                                                                             sa.AdvanceDate.Month == advanceDate.Month &&

            //                                                                             sa.AdvanceDate.Year == advanceDate.Year)



            //                 on employee.EMP_ID equals salaryAdvance.EmployeeID into advancesGroup

            //                  from salaryAdvance in advancesGroup.DefaultIfEmpty()

            //                  where employee.EMP_ROLE == employeeType &&

            //                        employee.EMP_BRANCH_CODE == branch &&

            //                        employee.HasTransfered == false &&

            //                        salaryAdvance.IsDeleted == false &&

            //                        employmentDetails.EMPPAY_DATE_JOINED <= advanceDate

            //                  && (employmentDetails.EMPPAY_DATE_RESIGNED == null || employmentDetails.EMPPAY_DATE_RESIGNED >= advanceDate)

            //                  && (!string.IsNullOrEmpty(race) && employee.EMP_RACE == race)

            //                  select new SalaryAdvanceDto

            //                  {

            //                      ID = (salaryAdvance != null) ? salaryAdvance.ID : 0,

            //                      EMP_ID = employee.EMP_ID,

            //                      EMP_CODE = employee.EMP_CODE,

            //                      EMP_NAME = employee.EMP_NAME,

            //                      EMP_RACE = employee.EMP_RACE,

            //                      EMP_ROLE = employee.EMP_ROLE,

            //                      EMP_IC_NEW = employee.EMP_IC_NEW,

            //                      EMP_IC_OLD = employee.EMP_IC_OLD,

            //                      EMP_PASSPORT_NO = employee.EMP_PASSPORT_NO,

            //                      EMPFL_BANK = salaryDetails.EMPFL_BANK,

            //                      EMPFL_BK_ACCNO = salaryDetails.EMPFL_BK_ACCNO,

            //                      PAYMODE = salaryDetails.PAYMODE,

            //                      Amount = (salaryAdvance != null) ? salaryAdvance.Amount : 0,

            //                      Particulars = (salaryAdvance != null) ? salaryAdvance.Particulars : ""

            //                  }).Distinct();

            //if (!string.IsNullOrEmpty(race))

            //    resultList = resultList.Where(emp => emp.EMP_RACE == race);

            //resultList = resultList.OrderBy(emp => emp.EMP_NAME);



            var initialQuery = from employee in _oBMSDbContext.Employees

                         join salaryDetails in _oBMSDbContext.EmployeeSalaryDetails

                             on employee.EMP_CODE equals salaryDetails.EMPFL_CODE

                         join employmentDetails in _oBMSDbContext.EmploymentDetails

                             on employee.EMP_CODE equals employmentDetails.EMPPAY_CODE

                         join salaryAdvanceTemp in _oBMSDbContext.SalaryAdvances

                             .Where(sa => sa.TransType == transType

                                          && !sa.IsDeleted

                                          && sa.AdvanceDate.Month == advanceDate.Month

                                          && sa.AdvanceDate.Year == advanceDate.Year)

                             on employee.EMP_ID equals salaryAdvanceTemp.EmployeeID into salaryAdvanceGroup

                         from salaryAdvance in salaryAdvanceGroup.DefaultIfEmpty()

                         where employee.EMP_ROLE == employeeType

                              && employee.EMP_BRANCH_CODE == branch

                              && employee.HasTransfered == false

                              && (salaryAdvance == null || !salaryAdvance.IsDeleted)

                              && (string.IsNullOrEmpty(race) || employee.EMP_RACE == race)

                         orderby employee.EMP_NAME

                         select new

                         {

                             employee,

                             salaryDetails,

                             employmentDetails,

                             salaryAdvance

                         };



            Console.WriteLine($"[PayrollRepository] Executing initial query to get raw data");

            var rawData = initialQuery.ToList();

            Console.WriteLine($"[PayrollRepository] Raw data count before filtering: {rawData.Count}");



            var query = initialQuery.AsEnumerable() // <-- Forces execution in memory

             .Where(x =>

             {

                 DateTime joinedDate = Convert.ToDateTime(x.employmentDetails.EMPPAY_DATE_JOINED);

                 DateTime resignedDate = string.IsNullOrEmpty(x.employmentDetails.EMPPAY_DATE_RESIGNED.ToString())

                                         ? new DateTime(2100, 1, 1)

                                         : Convert.ToDateTime(x.employmentDetails.EMPPAY_DATE_RESIGNED);

                 DateTime compareDate = x.salaryAdvance?.AdvanceDate ?? advanceDate;



                 bool isValid = joinedDate <= compareDate && resignedDate >= compareDate;

                 Console.WriteLine($"[PayrollRepository] Filtering employee {x.employee.EMP_NAME}: joinedDate={joinedDate}, resignedDate={resignedDate}, compareDate={compareDate}, isValid={isValid}");

                 return isValid;

             })

             .Select(x => new SalaryAdvanceDto

             {

                 ID = x.salaryAdvance?.ID ?? 0,

                 EMP_ID = x.employee.EMP_ID,

                 EMP_CODE = x.employee.EMP_CODE,

                 EMP_NAME = x.employee.EMP_NAME,

                 EMP_RACE = x.employee.EMP_RACE,

                 EMP_ROLE = x.employee.EMP_ROLE,

                 EMP_IC_NEW = x.employee.EMP_IC_NEW ?? "",

                 EMP_IC_OLD = x.employee.EMP_IC_OLD ?? "",

                 EMP_PASSPORT_NO = x.employee.EMP_PASSPORT_NO ?? "",

                 EMPFL_BANK = x.salaryDetails.EMPFL_BANK ?? "",

                 EMPFL_BK_ACCNO = x.salaryDetails.EMPFL_BK_ACCNO ?? "",

                 PAYMODE = x.salaryDetails.PAYMODE ?? "",

                 Amount = x.salaryAdvance?.Amount ?? 0,

                 Particulars = x.salaryAdvance?.Particulars ?? ""

             })

             .Distinct()

             .ToList();



            Console.WriteLine($"[PayrollRepository] Final employee list count: {query.Count}");



            return new List<SalaryAdvanceDto>(query);



        }

        #endregion



        #region Salary Processing

        public string LastSalaryProcessRemarks(DateTime period, string branchCode, string employeeType)

        {

            var remarks = _oBMSDbContext.SalaryProcess

                 .Where(sp => sp.Period <= period && sp.Branch == branchCode && sp.EmployeeType == employeeType)

                 .Select(sp => sp.Remarks)

                 .FirstOrDefault();



            return remarks;

        }

        public bool IsSalaryProcessDoneForCurrentPeriod(string branch, string employeeType, DateTime dtPeriod)

        {

            var isProcessed = _oBMSDbContext.SalaryProcess

                   .Any(sp =>

                       sp.EmployeeType == employeeType &&

                       sp.Branch == branch &&

                       sp.Period.Year == dtPeriod.Year &&

                       sp.Period.Month == dtPeriod.Month);



            return isProcessed;

        }

        public List<string> GetEmployeeAttendanceList(DateTime period, string branch)

        {

            var nameList = _oBMSDbContext.Attendances

                .Where(a => a.Period.Year == period.Year && a.Period.Month == period.Month && a.Branch == branch)

                .Join(

                    _oBMSDbContext.Employees,

                    attendance => attendance.EmployeeID,

                    employee => employee.EMP_ID,

                    (attendance, employee) => employee.EMP_CODE

                )

                .ToList();



            return nameList;



        }



        public async Task<List<object>> GetTodayAttendanceList(string branch)

        {

            try

            {

                var today = DateTime.Today;

                

                // Debug: Check what attendance records exist in the database

                var allAttendance = await _oBMSDbContext.Attendances.ToListAsync();

                Console.WriteLine($"Total attendance records in database: {allAttendance.Count}");

                

                var todayAttendance = allAttendance.Where(a => a.Period.Date == today.Date).ToList();

                Console.WriteLine($"Attendance records with Period = today: {todayAttendance.Count}");

                

                var allAttendanceDetails = await _oBMSDbContext.AttendanceDetails.ToListAsync();

                var todayAttendanceDetails = allAttendanceDetails.Where(ad => ad.AttendanceDate.Date == today.Date).ToList();

                Console.WriteLine($"AttendanceDetails records with AttendanceDate = today: {todayAttendanceDetails.Count}");

                

                var attendanceData = await (

                    from a in _oBMSDbContext.Attendances

                    join e in _oBMSDbContext.Employees on a.EmployeeID equals e.EMP_ID

                    join ad in _oBMSDbContext.AttendanceDetails on a.ID equals ad.AttendanceID

                    where (ad.AttendanceDate.Date == today.Date || a.Period.Date == today.Date) && 

                          (string.IsNullOrEmpty(branch) || branch == "ALL" || a.Branch == branch)

                    select new

                    {

                        ID = a.ID,

                        Period = a.Period,

                        EmployeeID = a.EmployeeID,

                        EmployeeName = e.EMP_NAME,

                        EmployeeCode = e.EMP_CODE,

                        Branch = a.Branch,

                        AttendanceDate = ad.AttendanceDate,

                        TimeStart = ad.TimeStart,

                        TimeEnd = ad.TimeEnd,

                        Client = ad.Client,

                        OTClient = ad.OTClient,

                        OTTimeStart = ad.OTTimeStart,

                        OTTimeEnd = ad.OTTimeEnd,

                        Type = ad.Type,

                        Status = ad.Type == 8 ? "Leave" : ad.Type == 1 ? "Present" : "Absent",

                        LastUpdate = ad.LastUpdate,

                        LastUpdatedBy = ad.LastUpdatedBy,

                        PunchInTime = ad.TimeStart.HasValue ? ad.TimeStart.Value.ToString("HH:mm") : "-",

                        PunchOutTime = ad.TimeEnd.HasValue ? ad.TimeEnd.Value.ToString("HH:mm") : "-",

                        TotalHours = ad.TimeStart.HasValue && ad.TimeEnd.HasValue ? 

                            (decimal)(ad.TimeEnd.Value - ad.TimeStart.Value).TotalHours : 0,

                        OvertimeHours = ad.OTTimeStart.HasValue && ad.OTTimeEnd.HasValue ? 

                            (decimal)(ad.OTTimeEnd.Value - ad.OTTimeStart.Value).TotalHours : 0,

                        Behavior = ad.TimeStart.HasValue ? 

                            (ad.TimeStart.Value.Hour > 9 ? "Late" : "On Time") : "Absent"

                    }

                ).ToListAsync();



                Console.WriteLine($"Final query returned {attendanceData.Count} records");

                return attendanceData.Cast<object>().ToList();

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Repository error: {ex.Message}");

                throw new Exception($"Error retrieving today's attendance: {ex.Message}", ex);

            }

        }



        public async Task<List<object>> GetAttendanceByDate(DateTime attendanceDate, string branch)

        {

            try

            {

                // Debug: Check what attendance records exist in the database for the specific date

                var allAttendance = await _oBMSDbContext.Attendances.ToListAsync();

                Console.WriteLine($"Total attendance records in database: {allAttendance.Count}");

                

                var targetAttendance = allAttendance.Where(a => a.Period.Date == attendanceDate.Date).ToList();

                Console.WriteLine($"Attendance records with Period = target date: {targetAttendance.Count}");

                

                var allAttendanceDetails = await _oBMSDbContext.AttendanceDetails.ToListAsync();

                var targetAttendanceDetails = allAttendanceDetails.Where(ad => ad.AttendanceDate.Date == attendanceDate.Date).ToList();

                Console.WriteLine($"AttendanceDetails records with AttendanceDate = target date: {targetAttendanceDetails.Count}");

                

                var attendanceData = await (

                    from a in _oBMSDbContext.Attendances

                    join e in _oBMSDbContext.Employees on a.EmployeeID equals e.EMP_ID

                    join ad in _oBMSDbContext.AttendanceDetails on a.ID equals ad.AttendanceID

                    where (ad.AttendanceDate.Date == attendanceDate.Date || a.Period.Date == attendanceDate.Date) && 

                          (string.IsNullOrEmpty(branch) || branch == "ALL" || a.Branch == branch)

                    select new

                    {

                        ID = a.ID,

                        Period = a.Period,

                        EmployeeID = a.EmployeeID,

                        EmployeeName = e.EMP_NAME,

                        EmployeeCode = e.EMP_CODE,

                        Branch = a.Branch,

                        AttendanceDate = ad.AttendanceDate,

                        TimeStart = ad.TimeStart,

                        TimeEnd = ad.TimeEnd,

                        Client = ad.Client,

                        OTClient = ad.OTClient,

                        OTTimeStart = ad.OTTimeStart,

                        OTTimeEnd = ad.OTTimeEnd,

                        Type = ad.Type,

                        Status = ad.Type == 8 ? "Leave" : ad.Type == 1 ? "Present" : "Absent",

                        LastUpdate = ad.LastUpdate,

                        LastUpdatedBy = ad.LastUpdatedBy,

                        PunchInTime = ad.TimeStart.HasValue ? ad.TimeStart.Value.ToString("HH:mm") : "-",

                        PunchOutTime = ad.TimeEnd.HasValue ? ad.TimeEnd.Value.ToString("HH:mm") : "-",

                        TotalHours = ad.TimeStart.HasValue && ad.TimeEnd.HasValue ? 

                            (decimal)(ad.TimeEnd.Value - ad.TimeStart.Value).TotalHours : 0,

                        OvertimeHours = ad.OTTimeStart.HasValue && ad.OTTimeEnd.HasValue ? 

                            (decimal)(ad.OTTimeEnd.Value - ad.OTTimeStart.Value).TotalHours : 0,

                        Behavior = ad.TimeStart.HasValue ? 

                            (ad.TimeStart.Value.Hour > 9 ? "Late" : "On Time") : "Absent"

                    }

                ).ToListAsync();



                Console.WriteLine($"Final query returned {attendanceData.Count} records for date: {attendanceDate:yyyy-MM-dd}");

                return attendanceData.Cast<object>().ToList();

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Repository error: {ex.Message}");

                throw new Exception($"Error retrieving attendance for date {attendanceDate:yyyy-MM-dd}: {ex.Message}", ex);

            }

        }



        public bool IsTemporaryEmployee(string employeeCode)

        {

            try

            {

                var employee = _oBMSDbContext.EmployeeSalaryDetails

                    .Where(e => e.EMPFL_CODE == employeeCode)

                    .FirstOrDefault();



                return employee?.TMPGUARD ?? false;

            }

            catch

            {

                throw;

            }

        }



        public string Process(string branch, string employeeType, string remarks, DateTime period, bool lockProcess, string currentUser, string companyCode)

        {

            string status = string.Empty;

            var attendanceQuery = (from attendance in _oBMSDbContext.Attendances

                                   join employee in _oBMSDbContext.Employees on attendance.EmployeeID equals employee.EMP_ID

                                   where attendance.Branch == branch && employee.EMP_ROLE.Contains(employeeType) && attendance.Period.Year == period.Year && attendance.Period.Month == period.Month

                                   orderby employee.EMP_NAME

                                   select new { attendance.Branch, attendance.EmployeeID }).ToList();

            List<int> employeeIDs = attendanceQuery.Select(a => a.EmployeeID).ToList();

            var salaryProcess = _oBMSDbContext.SalaryProcess

                    .FirstOrDefault(sp => sp.Period.Year == period.Year && sp.Period.Month == period.Month && sp.Branch == branch && sp.EmployeeType.Contains(employeeType));



            if (salaryProcess != null)

            {

                if (salaryProcess.IsLocked)

                    return "Salary Processing for the month is locked. It cannot be recomputed again.";



                salaryProcess.ID = salaryProcess.ID;

                salaryProcess.LastUpdate = DateTime.Now;

                salaryProcess.LastUpdatedBy = currentUser;

                salaryProcess.IsLocked = lockProcess;

                salaryProcess.Remarks = remarks;

                _oBMSDbContext.SalaryProcess.Update(salaryProcess);

                status = "updated";

            }

            else

            {

                _oBMSDbContext.SalaryProcess.Add(new Models.Domain.SalaryProcess

                {

                    Branch = branch,

                    Period = period,

                    EmployeeType = employeeType,

                    IsLocked = lockProcess,

                    Remarks = remarks,

                    LastUpdate = DateTime.Now,

                    LastUpdatedBy = currentUser

                });

                status = "inserted";

            }



            //  _oBMSDbContext.SaveChanges();

            // The actual payload generation and compliance calculations (PF, ESI, Tax) 

            // happen dynamically in GetPayslipData. This method purely manages the SalaryProcess lock.



            return status;

        }



        public async Task<DateTime?> GetLatestAttendancePeriodAsync(int employeeId, int year, int month)

        {

            var record = await _oBMSDbContext.Attendances

                .Where(a => a.EmployeeID == employeeId &&

                            a.Period.Year == year &&

                            a.Period.Month > month)

                .OrderBy(a => a.Period)

                .FirstOrDefaultAsync();



            return record?.Period;

        }

        #endregion



        // Payslip Data Methods

        public async Task<object> GetPayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang)

        {

            try

            {

                if (!DateTime.TryParseExact(period, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime periodDate))

                    periodDate = DateTime.Now;



                var employeeData = await (from emp in _oBMSDbContext.Employees

                                       join empDetail in _oBMSDbContext.EmploymentDetails on emp.EMP_CODE equals empDetail.EMPPAY_CODE

                                       join attendance in _oBMSDbContext.Attendances on emp.EMP_ID equals attendance.EmployeeID

                                       where emp.EMP_CODE == employee && attendance.Period.Year == periodDate.Year && attendance.Period.Month == periodDate.Month

                                       select new { emp, empDetail, attendance }).FirstOrDefaultAsync();



                if (employeeData != null)

                {

                    // Use CB (Commercial Breakdown) fields from Employee table - Simplified

                    // These are the exact values entered by user in Commercial Breakdown dialog

                    decimal basicSalary = employeeData.emp.CB_Basic ?? (employeeData.attendance.AllowanceDeduction > 0 ? employeeData.attendance.AllowanceDeduction : (decimal)employeeData.empDetail.EMPPAY_BASIC_RATE);

                    decimal da = employeeData.emp.CB_DA ?? 0;

                    decimal hra = employeeData.emp.CB_HRA ?? employeeData.empDetail.ATTENDANCEALLOWANCE ?? 0;

                    decimal otherAllowances = employeeData.emp.CB_OtherAllowances ?? (decimal?)employeeData.empDetail.SpecialAllowance ?? 0m;

                    decimal nh = employeeData.emp.CB_NH ?? 0;

                    decimal leaves = employeeData.emp.CB_Leaves ?? 0;

                    decimal advanceStatutoryBonus = employeeData.emp.CB_AdvanceStatutoryBonus ?? 0;

                    decimal uniformCost = 0;

                    decimal overtime = employeeData.attendance.Shift2Rate;



                    // Calculate gross salary using CB components - Simplified

                    decimal grossSalary = basicSalary + da + hra + otherAllowances + nh + leaves + advanceStatutoryBonus + overtime;



                    var pfResult = _pfCalculationService.CalculatePF(basicSalary, hra, periodDate);

                    var esiResult = _esiCalculationService.CalculateESI(grossSalary, periodDate);

                    var ptResult = _professionalTaxService.CalculateProfessionalTax(grossSalary, "Tamil Nadu", periodDate);

                    var tdsResult = _tdsCalculationService.CalculateTDS(grossSalary * 12, 1);



                    return new

                    {

                        period = period,

                        employeeName = employeeData.emp.EMP_NAME,

                        employeeCode = employeeData.emp.EMP_CODE,

                        aadhaar = employeeData.emp.AadhaarNumber ?? "XXXX-XXXX-1234",

                        pan = employeeData.emp.PANNumber ?? "ABCDE1234F",

                        bankAccount = employeeData.emp.BankAccountNumber ?? "12345678901",

                        dateOfJoining = employeeData.empDetail.EMPPAY_DATE_JOINED.ToString("dd-MMM-yyyy"),

                        designation = employeeData.emp.EMP_ROLE ?? "Security Guard",

                        department = "Operations",

                        location = branch,

                        basicSalary = Math.Round(basicSalary, 2),

                        da = Math.Round(da, 2),

                        hra = Math.Round(hra, 2),

                        leaves = Math.Round(leaves, 2),

                        nh = Math.Round(nh, 2),

                        advanceStatutoryBonus = Math.Round(advanceStatutoryBonus, 2),

                        bonus = Math.Round(advanceStatutoryBonus, 2),

                        others = Math.Round(otherAllowances, 2),

                        uniformCost = Math.Round(uniformCost, 2),

                        specialAllowance = Math.Round(otherAllowances, 2),

                        overtime = Math.Round(overtime, 2),

                        overtimeHours = "8.00",

                        grossEarnings = Math.Round(grossSalary, 2),

                        providentFund = Math.Round(pfResult.EmployeeContribution, 2),

                        esi = Math.Round(esiResult.EmployeeContribution, 2),

                        professionalTax = Math.Round(ptResult.TaxAmount, 2),

                        incomeTax = Math.Round(tdsResult.MonthlyTDS, 2),

                        totalDeductions = Math.Round(pfResult.EmployeeContribution + esiResult.EmployeeContribution + ptResult.TaxAmount + tdsResult.MonthlyTDS, 2),

                        netSalary = Math.Round(grossSalary - (pfResult.EmployeeContribution + esiResult.EmployeeContribution + ptResult.TaxAmount + tdsResult.MonthlyTDS), 2),

                        netSalaryWords = "",

                        employerPF = Math.Round(pfResult.EmployerContribution, 2),

                        employerESI = Math.Round(esiResult.EmployerContribution, 2)

                    };

                }



                return null;

            }

            catch (Exception)

            {

                throw;

            }

        }



        public async Task<object> GetTimeSheetData(string loginId, string branch, string period, string employeeType, string employee, string lang)

        {

            return await GetPayslipData(loginId, branch, period, employeeType, employee, lang); // Placeholder

        }



        public async Task<object> GetPayslip2Data(string loginId, string branch, string period, string employeeType, string employee, string lang)

        {

            return await GetPayslipData(loginId, branch, period, employeeType, employee, lang);

        }



        public async Task<object> GetNewPayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang)

        {

            return await GetPayslipData(loginId, branch, period, employeeType, employee, lang);

        }



        public async Task<object> GetGuard1PayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang)

        {

            try

            {

                if (!DateTime.TryParseExact(period, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime periodDate))

                    periodDate = DateTime.Now;



                var guardData = await (from emp in _oBMSDbContext.Employees

                                     join empDetail in _oBMSDbContext.EmploymentDetails on emp.EMP_CODE equals empDetail.EMPPAY_CODE

                                     join attendance in _oBMSDbContext.Attendances on emp.EMP_ID equals attendance.EmployeeID

                                     where emp.EMP_CODE == employee && attendance.Period.Year == periodDate.Year && attendance.Period.Month == periodDate.Month

                                     select new { emp, empDetail, attendance }).FirstOrDefaultAsync();



                if (guardData != null)

                {

                    decimal basicSalary = guardData.attendance.AllowanceDeduction > 0 ? guardData.attendance.AllowanceDeduction : (decimal)guardData.empDetail.EMPPAY_BASIC_RATE;

                    decimal hrAllowance = guardData.empDetail.ATTENDANCEALLOWANCE ?? 0;

                    decimal grossSalary = basicSalary + hrAllowance;



                    var pfResult = _pfCalculationService.CalculatePF(basicSalary, hrAllowance, periodDate);

                    var esiResult = _esiCalculationService.CalculateESI(grossSalary, periodDate);

                    var ptResult = _professionalTaxService.CalculateProfessionalTax(grossSalary, "Tamil Nadu", periodDate);



                    return new

                    {

                        period = period,

                        employeeName = guardData.emp.EMP_NAME,

                        employeeCode = guardData.emp.EMP_CODE,

                        location = branch,

                        workingDays = 26,

                        presentDays = 26,

                        basicSalary = Math.Round(basicSalary, 2),

                        hrAllowance = Math.Round(hrAllowance, 2),

                        grossSalary = Math.Round(grossSalary, 2),

                        providentFund = Math.Round(pfResult.EmployeeContribution, 2),

                        esi = Math.Round(esiResult.EmployeeContribution, 2),

                        professionalTax = Math.Round(ptResult.TaxAmount, 2),

                        totalDeductions = Math.Round(pfResult.EmployeeContribution + esiResult.EmployeeContribution + ptResult.TaxAmount, 2),

                        netSalary = Math.Round(grossSalary - (pfResult.EmployeeContribution + esiResult.EmployeeContribution + ptResult.TaxAmount), 2),

                        netSalaryWords = ""

                    };

                }

                return null;

            }

            catch (Exception)

            {

                throw;

            }

        }



        public async Task<object> GetGuard2PayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang)

        {

            try

            {

                if (!DateTime.TryParseExact(period, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime periodDate))

                    periodDate = DateTime.Now;



                var guardData = await (from emp in _oBMSDbContext.Employees

                                     join empDetail in _oBMSDbContext.EmploymentDetails on emp.EMP_CODE equals empDetail.EMPPAY_CODE

                                     join attendance in _oBMSDbContext.Attendances on emp.EMP_ID equals attendance.EmployeeID

                                     where emp.EMP_CODE == employee && attendance.Period.Year == periodDate.Year && attendance.Period.Month == periodDate.Month

                                     select new { emp, empDetail, attendance }).FirstOrDefaultAsync();



                if (guardData != null)

                {

                    decimal basicSalary = guardData.attendance.AllowanceDeduction > 0 ? guardData.attendance.AllowanceDeduction : (decimal)guardData.empDetail.EMPPAY_BASIC_RATE;

                    decimal hrAllowance = guardData.empDetail.ATTENDANCEALLOWANCE ?? 0;

                    decimal overtime = guardData.attendance.Shift2Rate;

                    decimal grossSalary = basicSalary + hrAllowance + overtime;



                    var pfResult = _pfCalculationService.CalculatePF(basicSalary, hrAllowance, periodDate);

                    var esiResult = _esiCalculationService.CalculateESI(grossSalary, periodDate);

                    var ptResult = _professionalTaxService.CalculateProfessionalTax(grossSalary, "Tamil Nadu", periodDate);



                    return new

                    {

                        period = period,

                        employeeName = guardData.emp.EMP_NAME,

                        employeeCode = guardData.emp.EMP_CODE,

                        location = branch,

                        workingDays = 26,

                        presentDays = 26,

                        basicSalary = Math.Round(basicSalary, 2),

                        hrAllowance = Math.Round(hrAllowance, 2),

                        overtime = Math.Round(overtime, 2),

                        grossSalary = Math.Round(grossSalary, 2),

                        providentFund = Math.Round(pfResult.EmployeeContribution, 2),

                        esi = Math.Round(esiResult.EmployeeContribution, 2),

                        professionalTax = Math.Round(ptResult.TaxAmount, 2),

                        totalDeductions = Math.Round(pfResult.EmployeeContribution + esiResult.EmployeeContribution + ptResult.TaxAmount, 2),

                        netSalary = Math.Round(grossSalary - (pfResult.EmployeeContribution + esiResult.EmployeeContribution + ptResult.TaxAmount), 2),

                        netSalaryWords = ""

                    };

                }

                return null;

            }

            catch (Exception)

            {

                throw;

            }

        }



        public async Task<object> GetRBAPayslipData(string loginId, string branch, string period, string employeeType, string employee, string lang)

        {

            try

            {

                if (!DateTime.TryParseExact(period, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime periodDate))

                    periodDate = DateTime.Now;



                var rbaData = await (from emp in _oBMSDbContext.Employees

                                   join empDetail in _oBMSDbContext.EmploymentDetails on emp.EMP_CODE equals empDetail.EMPPAY_CODE

                                   join attendance in _oBMSDbContext.Attendances on emp.EMP_ID equals attendance.EmployeeID

                                   where emp.EMP_CODE == employee && attendance.Period.Year == periodDate.Year && attendance.Period.Month == periodDate.Month

                                   select new { emp, empDetail, attendance }).FirstOrDefaultAsync();



                if (rbaData != null)

                {

                    decimal basicSalary = rbaData.attendance.AllowanceDeduction > 0 ? rbaData.attendance.AllowanceDeduction : 16000;

                    decimal hrAllowance = rbaData.attendance.Shift2Rate > 0 ? rbaData.attendance.Shift2Rate : 3200;

                    decimal specialAllowance = 1500;

                    decimal grossSalary = basicSalary + hrAllowance + specialAllowance;



                    var pfResult = _pfCalculationService.CalculatePF(basicSalary, hrAllowance, periodDate);

                    var esiResult = _esiCalculationService.CalculateESI(grossSalary, periodDate);

                    var ptResult = _professionalTaxService.CalculateProfessionalTax(grossSalary, "Tamil Nadu", periodDate);

                    var tdsResult = _tdsCalculationService.CalculateTDS(grossSalary * 12, 1);



                    return new

                    {

                        period = period,

                        employeeName = rbaData.emp.EMP_NAME,

                        employeeCode = rbaData.emp.EMP_CODE,

                        location = branch,

                        workingDays = 26,

                        presentDays = 26,

                        basicSalary = Math.Round(basicSalary, 2),

                        hrAllowance = Math.Round(hrAllowance, 2),

                        specialAllowance = Math.Round(specialAllowance, 2),

                        grossSalary = Math.Round(grossSalary, 2),

                        providentFund = Math.Round(pfResult.EmployeeContribution, 2),

                        esi = Math.Round(esiResult.EmployeeContribution, 2),

                        professionalTax = Math.Round(ptResult.TaxAmount, 2),

                        incomeTax = Math.Round(tdsResult.MonthlyTDS, 2),

                        totalDeductions = Math.Round(pfResult.EmployeeContribution + esiResult.EmployeeContribution + ptResult.TaxAmount + tdsResult.MonthlyTDS, 2),

                        netSalary = Math.Round(grossSalary - (pfResult.EmployeeContribution + esiResult.EmployeeContribution + ptResult.TaxAmount + tdsResult.MonthlyTDS), 2),

                        netSalaryWords = ""

                    };

                }

                return null;

            }

            catch (Exception)

            {

                throw;

            }

        }



        #region RBI Bank Salary Export

        public List<RbiBankSalaryExport> GetRbiBankSalaryExportData(string dtSalaryPeriod, string branch, string employeeType)

        {

            try

            {

                var businessData = RbiBankSalaryExport.GetRbiBankSalaryExportData(dtSalaryPeriod, branch, employeeType);

                return businessData;

            }

            catch (Exception)

            {

                throw;

            }

        }



        public List<RbiBankAdvanceExport> GetRbiBankAdvanceExportData(string dtSalaryPeriod, string branch, string employeeType)

        {

            try

            {

                var businessData = RbiBankAdvanceExport.GetRbiBankAdvanceExportData(dtSalaryPeriod, branch, employeeType);

                return businessData;

            }

            catch (Exception)

            {

                throw;

            }

        }

        #endregion

        #region Voucher Filter Report
        public async Task<List<VoucherDetailDto>> GetVoucherDetailsByFilter(string branch, DateTime period, string employeeType, string bankCode, string paymentType, string voucherType)
        {
            try
            {
                List<VoucherDetailDto> voucherDetails = new List<VoucherDetailDto>();

                if (voucherType == "DailyAdvance")
                {
                    // Get SalaryAdvance records with employee details
                    var query = from sa in _oBMSDbContext.SalaryAdvances
                                join emp in _oBMSDbContext.Employees on sa.EmployeeID equals emp.EMP_ID
                                where sa.IsDeleted == false
                                && sa.TransType == 1 // Daily Advance
                                && sa.AdvanceDate.Month == period.Month
                                && sa.AdvanceDate.Year == period.Year
                                select new VoucherDetailDto
                                {
                                    ID = sa.ID,
                                    VoucherNo = sa.VoucherNo,
                                    Date = sa.AdvanceDate,
                                    EmployeeName = emp.EMP_NAME,
                                    Amount = sa.Amount,
                                    PaymentType = sa.PaymentType,
                                    EmployeeCode = emp.EMP_CODE
                                };

                    // Apply filters
                    if (!string.IsNullOrEmpty(branch))
                    {
                        query = query.Where(v => _oBMSDbContext.Employees
                            .Where(e => e.EMP_CODE == v.EmployeeCode)
                            .Select(e => e.EMP_BRANCH_CODE)
                            .FirstOrDefault() == branch);
                    }

                    if (!string.IsNullOrEmpty(employeeType))
                    {
                        query = query.Where(v => _oBMSDbContext.Employees
                            .Where(e => e.EMP_CODE == v.EmployeeCode)
                            .Select(e => e.EMP_ROLE)
                            .FirstOrDefault() == employeeType);
                    }

                    if (!string.IsNullOrEmpty(paymentType) && paymentType != "All")
                    {
                        query = query.Where(v => v.PaymentType == paymentType);
                    }

                    if (!string.IsNullOrEmpty(bankCode) && paymentType == "Bank")
                    {
                        query = query.Where(v => _oBMSDbContext.EmployeeSalaryDetails
                            .Where(esd => esd.EMPFL_CODE == v.EmployeeCode)
                            .Select(esd => esd.EMPFL_BANK)
                            .FirstOrDefault() == bankCode);
                    }

                    voucherDetails = await query.ToListAsync();
                }
                else if (voucherType == "Loan")
                {
                    // Get SalaryAdvance records with TransType != 1 (Loan)
                    var query = from sa in _oBMSDbContext.SalaryAdvances
                                join emp in _oBMSDbContext.Employees on sa.EmployeeID equals emp.EMP_ID
                                where sa.IsDeleted == false
                                && sa.TransType != 1 // Loan (not daily advance)
                                && sa.AdvanceDate.Month == period.Month
                                && sa.AdvanceDate.Year == period.Year
                                select new VoucherDetailDto
                                {
                                    ID = sa.ID,
                                    VoucherNo = sa.VoucherNo,
                                    Date = sa.AdvanceDate,
                                    EmployeeName = emp.EMP_NAME,
                                    Amount = sa.Amount,
                                    PaymentType = sa.PaymentType,
                                    EmployeeCode = emp.EMP_CODE
                                };

                    // Apply filters
                    if (!string.IsNullOrEmpty(branch))
                    {
                        query = query.Where(v => _oBMSDbContext.Employees
                            .Where(e => e.EMP_CODE == v.EmployeeCode)
                            .Select(e => e.EMP_BRANCH_CODE)
                            .FirstOrDefault() == branch);
                    }

                    if (!string.IsNullOrEmpty(employeeType))
                    {
                        query = query.Where(v => _oBMSDbContext.Employees
                            .Where(e => e.EMP_CODE == v.EmployeeCode)
                            .Select(e => e.EMP_ROLE)
                            .FirstOrDefault() == employeeType);
                    }

                    if (!string.IsNullOrEmpty(paymentType) && paymentType != "All")
                    {
                        int transType = paymentType == "Bank" ? 3 : paymentType == "Cash" ? 1 : 2;
                        query = query.Where(v => _oBMSDbContext.SalaryAdvances
                            .Where(sa => sa.ID == v.ID)
                            .Select(sa => sa.TransType)
                            .FirstOrDefault() == transType);
                    }

                    voucherDetails = await query.ToListAsync();
                }
                else if (voucherType == "UniformLoan")
                {
                    // Get SalaryAdvance records with TransType = 4 (Uniform Loan)
                    var query = from sa in _oBMSDbContext.SalaryAdvances
                                join emp in _oBMSDbContext.Employees on sa.EmployeeID equals emp.EMP_ID
                                where sa.IsDeleted == false
                                && sa.TransType == 4 // Uniform Loan
                                && sa.AdvanceDate.Month == period.Month
                                && sa.AdvanceDate.Year == period.Year
                                select new VoucherDetailDto
                                {
                                    ID = sa.ID,
                                    VoucherNo = sa.VoucherNo,
                                    Date = sa.AdvanceDate,
                                    EmployeeName = emp.EMP_NAME,
                                    Amount = sa.Amount,
                                    PaymentType = sa.PaymentType,
                                    EmployeeCode = emp.EMP_CODE
                                };

                    // Apply filters
                    if (!string.IsNullOrEmpty(branch))
                    {
                        query = query.Where(v => _oBMSDbContext.Employees
                            .Where(e => e.EMP_CODE == v.EmployeeCode)
                            .Select(e => e.EMP_BRANCH_CODE)
                            .FirstOrDefault() == branch);
                    }

                    if (!string.IsNullOrEmpty(employeeType))
                    {
                        query = query.Where(v => _oBMSDbContext.Employees
                            .Where(e => e.EMP_CODE == v.EmployeeCode)
                            .Select(e => e.EMP_ROLE)
                            .FirstOrDefault() == employeeType);
                    }

                    if (!string.IsNullOrEmpty(paymentType) && paymentType != "All")
                    {
                        query = query.Where(v => v.PaymentType == paymentType);
                    }

                    voucherDetails = await query.ToListAsync();
                }

                return voucherDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Bulk Attendance Upload

        public async Task<AttendanceBulkUploadResultDto> BulkUploadAttendance(List<AttendanceBulkUploadDto> attendanceData)
        {
            var result = new AttendanceBulkUploadResultDto();
            var strategy = _oBMSDbContext.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _oBMSDbContext.Database.BeginTransactionAsync();
                    try
                    {
                        result.TotalRecords = attendanceData.Count;

                        foreach (var (attendance, index) in attendanceData.Select((item, index) => (item, index)))
                        {
                            try
                            {
                                var validationResult = ValidateAttendanceRecord(attendance);
                                if (!validationResult.IsValid)
                                {
                                    result.FailedRecords++;
                                    result.Errors.Add(new AttendanceBulkUploadErrorDto
                                    {
                                        RowNumber = index + 2, // +2 because Excel rows start from 2 (after header)
                                        EmployeeCode = attendance.EmployeeCode,
                                        EmployeeName = attendance.EmployeeName,
                                        AttendanceDate = attendance.Period,
                                        ErrorMessage = validationResult.ErrorMessage,
                                        FieldName = validationResult.FieldName
                                    });
                                    continue;
                                }

                                var employee = await _oBMSDbContext.Employees
                                    .FirstOrDefaultAsync(e => e.EMP_CODE == attendance.EmployeeCode);

                                if (employee == null)
                                {
                                    result.FailedRecords++;
                                    result.Errors.Add(new AttendanceBulkUploadErrorDto
                                    {
                                        RowNumber = index + 2,
                                        EmployeeCode = attendance.EmployeeCode,
                                        EmployeeName = attendance.EmployeeName,
                                        AttendanceDate = attendance.Period,
                                        ErrorMessage = $"Employee with code {attendance.EmployeeCode} not found",
                                        FieldName = "EmployeeCode"
                                    });
                                    continue;
                                }

                                // Check if attendance already exists for this employee and period
                                var existingAttendance = await _oBMSDbContext.Attendances
                                    .FirstOrDefaultAsync(a => a.EmployeeID == employee.EMP_ID && 
                                                           a.Period.Year == attendance.Period.Year && 
                                                           a.Period.Month == attendance.Period.Month);

                                var attendanceHeader = existingAttendance ?? new Attendance
                                {
                                    EmployeeID = employee.EMP_ID,
                                    Period = _attendancePeriodService.GetAttendancePeriod(attendance.ClientCode ?? string.Empty, attendance.Period.Year, attendance.Period.Month).PeriodKey,
                                    Branch = attendance.BranchCode,
                                    Shift2Type = attendance.Shift2Type,
                                    Shift2Rate = attendance.Shift2Rate,
                                    AllowanceDeduction = attendance.AllowanceDeduction,
                                    SpecialAllowanceDeduction = attendance.SpecialAllowanceDeduction,
                                    Bonus = attendance.Bonus,
                                    LastUpdate = DateTime.Now,
                                    LastUpdatedBy = "BulkUpload"
                                };

                                if (existingAttendance == null)
                                {
                                    _oBMSDbContext.Attendances.Add(attendanceHeader);
                                    await _oBMSDbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    // Update existing attendance header
                                    existingAttendance.Branch = attendance.BranchCode;
                                    existingAttendance.Shift2Type = attendance.Shift2Type;
                                    existingAttendance.Shift2Rate = attendance.Shift2Rate;
                                    existingAttendance.AllowanceDeduction = attendance.AllowanceDeduction;
                                    existingAttendance.SpecialAllowanceDeduction = attendance.SpecialAllowanceDeduction;
                                    existingAttendance.Bonus = attendance.Bonus;
                                    existingAttendance.LastUpdate = DateTime.Now;
                                    existingAttendance.LastUpdatedBy = "BulkUpload";
                                    _oBMSDbContext.Update(existingAttendance);
                                    await _oBMSDbContext.SaveChangesAsync();
                                }

                                // Remove existing attendance details for this period
                                var existingDetails = await _oBMSDbContext.AttendanceDetails
                                    .Where(ad => ad.AttendanceID == attendanceHeader.ID)
                                    .ToListAsync();
                                if (existingDetails.Any())
                                {
                                    _oBMSDbContext.AttendanceDetails.RemoveRange(existingDetails);
                                    await _oBMSDbContext.SaveChangesAsync();
                                }

                                // Add new attendance details
                                var attendanceDetails = new List<AttendanceDetails>();
                                foreach (var dayDetail in attendance.DailyAttendance)
                                {
                                    var workTypeId = ConvertWorkTypeToInt(dayDetail.WorkType);
                                    attendanceDetails.Add(new AttendanceDetails
                                    {
                                        AttendanceID = attendanceHeader.ID,
                                        AttendanceDate = dayDetail.AttendanceDate,
                                        Client = dayDetail.Client,
                                        TimeStart = dayDetail.TimeStart,
                                        TimeEnd = dayDetail.TimeEnd,
                                        OTClient = dayDetail.OTClient,
                                        OTTimeStart = dayDetail.OTTimeStart,
                                        OTTimeEnd = dayDetail.OTTimeEnd,
                                        Type = workTypeId,
                                        LastUpdate = DateTime.Now,
                                        LastUpdatedBy = "BulkUpload"
                                    });
                                }

                                if (attendanceDetails.Any())
                                {
                                    _oBMSDbContext.AttendanceDetails.AddRange(attendanceDetails);
                                    await _oBMSDbContext.SaveChangesAsync();
                                }

                                result.SuccessfulRecords++;
                                result.SuccessRecords.Add(new AttendanceBulkUploadSuccessDto
                                {
                                    RowNumber = index + 2,
                                    EmployeeCode = attendance.EmployeeCode,
                                    EmployeeName = attendance.EmployeeName,
                                    AttendanceID = attendanceHeader.ID,
                                    Message = "Successfully processed"
                                });
                            }
                            catch (Exception ex)
                            {
                                result.FailedRecords++;
                                result.Errors.Add(new AttendanceBulkUploadErrorDto
                                {
                                    RowNumber = index + 2,
                                    EmployeeCode = attendance.EmployeeCode,
                                    EmployeeName = attendance.EmployeeName,
                                    AttendanceDate = attendance.Period,
                                    ErrorMessage = ex.Message,
                                    FieldName = "General"
                                });
                            }
                        }

                        await transaction.CommitAsync();
                        result.Success = true;
                        result.Message = $"Bulk upload completed. Success: {result.SuccessfulRecords}, Failed: {result.FailedRecords}";
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Bulk upload failed: {ex.Message}";
            }

            return result;
        }

        public async Task<List<AttendanceBulkUploadDto>> GetAttendanceDataForExport(DateTime period, string branchCode)
        {
            var query = from attendance in _oBMSDbContext.Attendances
                       join employee in _oBMSDbContext.Employees on attendance.EmployeeID equals employee.EMP_ID
                       join attendanceDetail in _oBMSDbContext.AttendanceDetails on attendance.ID equals attendanceDetail.AttendanceID into detailGroup
                       from detail in detailGroup.DefaultIfEmpty()
                       where attendance.Period.Year == period.Year && 
                             attendance.Period.Month == period.Month &&
                             (branchCode == "ALL" || attendance.Branch == branchCode)
                       select new AttendanceBulkUploadDto
                       {
                           EmployeeCode = employee.EMP_CODE,
                           EmployeeName = employee.EMP_NAME,
                           BranchCode = attendance.Branch,
                           Period = attendance.Period,
                           Shift2Type = attendance.Shift2Type,
                           Shift2Rate = attendance.Shift2Rate,
                           AllowanceDeduction = attendance.AllowanceDeduction,
                           SpecialAllowanceDeduction = attendance.SpecialAllowanceDeduction,
                           Bonus = attendance.Bonus,
                           DailyAttendance = detailGroup.Where(d => d != null).Select(d => new AttendanceDayDetailDto
                           {
                               AttendanceDate = d.AttendanceDate,
                               Client = d.Client,
                               TimeStart = d.TimeStart,
                               TimeEnd = d.TimeEnd,
                               OTClient = d.OTClient,
                               OTTimeStart = d.OTTimeStart,
                               OTTimeEnd = d.OTTimeEnd,
                               WorkType = ConvertIntToWorkType(d.Type),
                               WorkingHours = d.TimeStart.HasValue && d.TimeEnd.HasValue ? 
                                   (decimal)(d.TimeEnd.Value - d.TimeStart.Value).TotalHours : 0,
                               OvertimeHours = d.OTTimeStart.HasValue && d.OTTimeEnd.HasValue ? 
                                   (decimal)(d.OTTimeEnd.Value - d.OTTimeStart.Value).TotalHours : 0
                           }).ToList()
                       };

            var result = await query.ToListAsync();
            
            // Group by employee to consolidate daily attendance
            var groupedResult = result
                .GroupBy(a => new { a.EmployeeCode, a.EmployeeName, a.BranchCode, a.Period })
                .Select(g => new AttendanceBulkUploadDto
                {
                    EmployeeCode = g.Key.EmployeeCode,
                    EmployeeName = g.Key.EmployeeName,
                    BranchCode = g.Key.BranchCode,
                    Period = g.Key.Period,
                    Shift2Type = g.First().Shift2Type,
                    Shift2Rate = g.First().Shift2Rate,
                    AllowanceDeduction = g.First().AllowanceDeduction,
                    SpecialAllowanceDeduction = g.First().SpecialAllowanceDeduction,
                    Bonus = g.First().Bonus,
                    DailyAttendance = g.SelectMany(a => a.DailyAttendance).OrderBy(d => d.AttendanceDate).ToList()
                })
                .ToList();

            return groupedResult;
        }

        private (bool IsValid, string ErrorMessage, string FieldName) ValidateAttendanceRecord(AttendanceBulkUploadDto attendance)
        {
            if (string.IsNullOrWhiteSpace(attendance.EmployeeCode))
                return (false, "Employee code is required", "EmployeeCode");

            if (string.IsNullOrWhiteSpace(attendance.EmployeeName))
                return (false, "Employee name is required", "EmployeeName");

            if (string.IsNullOrWhiteSpace(attendance.BranchCode))
                return (false, "Branch code is required", "BranchCode");

            if (attendance.Period == default)
                return (false, "Period is required", "Period");

            if (attendance.DailyAttendance == null || !attendance.DailyAttendance.Any())
                return (false, "Daily attendance data is required", "DailyAttendance");

            // Validate each daily attendance record
            foreach (var dayDetail in attendance.DailyAttendance)
            {
                if (dayDetail.AttendanceDate == default)
                    return (false, "Attendance date is required", "AttendanceDate");

                if (string.IsNullOrWhiteSpace(dayDetail.WorkType))
                    return (false, "Work type is required", "WorkType");

                // Validate time entries
                if (dayDetail.TimeStart.HasValue && !dayDetail.TimeEnd.HasValue)
                    return (false, "Time end is required when time start is provided", "TimeEnd");

                if (!dayDetail.TimeStart.HasValue && dayDetail.TimeEnd.HasValue)
                    return (false, "Time start is required when time end is provided", "TimeStart");

                if (dayDetail.TimeStart.HasValue && dayDetail.TimeEnd.HasValue && 
                    dayDetail.TimeStart.Value >= dayDetail.TimeEnd.Value)
                    return (false, "Time start must be before time end", "TimeStart");

                // Validate overtime time entries
                if (dayDetail.OTTimeStart.HasValue && !dayDetail.OTTimeEnd.HasValue)
                    return (false, "OT time end is required when OT time start is provided", "OTTimeEnd");

                if (!dayDetail.OTTimeStart.HasValue && dayDetail.OTTimeEnd.HasValue)
                    return (false, "OT time start is required when OT time end is provided", "OTTimeStart");

                if (dayDetail.OTTimeStart.HasValue && dayDetail.OTTimeEnd.HasValue && 
                    dayDetail.OTTimeStart.Value >= dayDetail.OTTimeEnd.Value)
                    return (false, "OT time start must be before OT time end", "OTTimeStart");
            }

            return (true, string.Empty, string.Empty);
        }

        private int ConvertWorkTypeToInt(string workType)
        {
            return workType switch
            {
                "General Working" => 1,
                "Off Day" => 2,
                "Off Day Working" => 3,
                "Holiday" => 4,
                "Holiday Working" => 5,
                "Annual Leave" => 8,
                "Medical Leave" => 9,
                "Maternity Leave" => 10,
                "Paternity Leave" => 11,
                "Hospitalization Leave" => 12,
                "Rest Day" => 13,
                "Unpaid Leave" => 14,
                "Marriage Leave" => 17,
                _ => 1 // Default to General Working
            };
        }

        private string ConvertIntToWorkType(int workType)
        {
            return workType switch
            {
                1 => "General Working",
                2 => "Off Day",
                3 => "Off Day Working",
                4 => "Holiday",
                5 => "Holiday Working",
                8 => "Annual Leave",
                9 => "Medical Leave",
                10 => "Maternity Leave",
                11 => "Paternity Leave",
                12 => "Hospitalization Leave",
                13 => "Rest Day",
                14 => "Unpaid Leave",
                17 => "Marriage Leave",
                _ => "General Working"
            };
        }

        #endregion

        #endregion
    }

}


