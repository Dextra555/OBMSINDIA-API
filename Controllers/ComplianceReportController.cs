using System;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;

using OBMS.WebAPI.Models;

using OBMS.WebAPI.Models.Domain;

using OBMS.WebAPI.Services;



namespace OBMS.WebAPI.Controllers

{

    [ApiController]

    [Route("api/[controller]")]

    public class ComplianceReportController : ControllerBase

    {

        private readonly IConfiguration _configuration;

        private readonly OBMSDbContext _context;

        private readonly IPFCalculationService _pfService;

        private readonly IESICalculationService _esiService;

        private readonly IProfessionalTaxService _ptService;

        private readonly ILogger<ComplianceReportController> _logger;



        public ComplianceReportController(

            IConfiguration configuration,

            OBMSDbContext context,

            IPFCalculationService pfService,

            IESICalculationService esiService,

            IProfessionalTaxService ptService,

            ILogger<ComplianceReportController> logger)

        {

            _configuration = configuration;

            _context = context;

            _pfService = pfService;

            _esiService = esiService;

            _ptService = ptService;

            _logger = logger;

        }



        [HttpPost("GetComplianceReportData")]

        public async Task<IActionResult> GetComplianceReportData([FromBody] System.Text.Json.JsonElement body)

        {

            try

            {

                // Parse report name

                string reportName = "";

                if (body.TryGetProperty("ReportName", out var rnProp))

                    reportName = rnProp.GetString() ?? "";



                if (string.IsNullOrEmpty(reportName))

                    return BadRequest(new { error = "Invalid request", message = "ReportName is required." });



                // Construct a standardized request object

                var request = new ComplianceReportRequest

                {

                    ReportName = reportName,

                    Parameters = new ComplianceReportParameters()

                };



                // Use the shared helper to extract period, branch, and client regardless of payload structure

                string period = "";

                string branch = "";

                string client = "";



                if (body.TryGetProperty("Parameters", out var paramObj))

                {

                    if (paramObj.TryGetProperty("Period", out var perProp) || paramObj.TryGetProperty("period", out perProp)) period = perProp.GetString() ?? "";

                    if (paramObj.TryGetProperty("Branch", out var brnProp) || paramObj.TryGetProperty("branch", out brnProp) || paramObj.TryGetProperty("BranchCode", out brnProp) || paramObj.TryGetProperty("branchCode", out brnProp)) branch = brnProp.GetString() ?? "";

                    if (paramObj.TryGetProperty("Client", out var cliProp) || paramObj.TryGetProperty("client", out cliProp) || paramObj.TryGetProperty("ClientCode", out cliProp) || paramObj.TryGetProperty("clientCode", out cliProp)) client = cliProp.GetString() ?? "";

                }

                else

                {

                    if (body.TryGetProperty("Period", out var perProp) || body.TryGetProperty("period", out perProp)) period = perProp.GetString() ?? "";

                    if (body.TryGetProperty("Branch", out var brnProp) || body.TryGetProperty("branch", out brnProp) || body.TryGetProperty("BranchCode", out brnProp) || body.TryGetProperty("branchCode", out brnProp)) branch = brnProp.GetString() ?? "";

                    if (body.TryGetProperty("Client", out var cliProp) || body.TryGetProperty("client", out cliProp) || body.TryGetProperty("ClientCode", out cliProp) || body.TryGetProperty("clientCode", out cliProp)) client = cliProp.GetString() ?? "";

                }



                request.Parameters.Period = period;

                request.Parameters.Branch = branch;

                request.Parameters.Client = client;



                // Extract specific booleans if present

                if (body.TryGetProperty("Parameters", out var pObj))

                {

                    if (pObj.TryGetProperty("ShowIndividualSlips", out var sisProp) && sisProp.ValueKind == System.Text.Json.JsonValueKind.True)

                        request.Parameters.ShowIndividualSlips = true;

                }

                else if (body.TryGetProperty("ShowIndividualSlips", out var sisPropFlat) && sisPropFlat.ValueKind == System.Text.Json.JsonValueKind.True)

                {

                    request.Parameters.ShowIndividualSlips = true;

                }



                switch (request.ReportName)

                {

                    case "FormXVII":

                        return Ok(await GenerateFormXVIIData(request));

                    case "FormXXVI":

                        return Ok(await GenerateFormXXVIData(request));

                    case "FormXXVII":

                        return Ok(await GenerateFormXVIIData(request));

                    case "FormXXVIII":

                        return Ok(await GenerateFormXXVIIIWageSlips(request));

                    case "FormXXIX":

                        return Ok(await GenerateFormXXIXData(request));

                    case "OvertimeRegister":

                        return Ok(await GenerateOvertimeData(request));

                    case "FormXXVICompliance":

                        return Ok(await GenerateFormXXVIComplianceData(request));

                    case "FormXXVIICompliance":

                        return Ok(await GenerateFormXXVIIComplianceData(request));

                    case "FormXXVIIICompliance":

                        return Ok(await GenerateFormXXVIIIComplianceData(request));

                    // AP (Andhra Pradesh) Forms - reuse existing data generation methods

                    case "AP_FormXVI":

                        return Ok(await GenerateFormXXVIData(request)); // Muster Roll (daily attendance)

                    case "AP_FormXVII":

                        return Ok(await GenerateFormXVIIData(request)); // Register of Wages

                    case "AP_FormXX":

                        return Ok(await GenerateFormXXIXData(request)); // Register of Deductions (reusing advances/deductions data)

                    case "AP_FormXXI":

                        return Ok(await GenerateFormXXIXData(request)); // Register of Fines (reusing advances/deductions data)

                    case "AP_FormXXII":

                        return Ok(await GenerateFormXXIXData(request)); // Register of Advances

                    case "AP_FormXXIII":

                        return Ok(await GenerateOvertimeData(request)); // Register of Overtime

                    case "AP_FormXXIX":

                        return Ok(await GenerateFormXXVIIIWageSlips(request)); // Wage Slips

                    default:

                        return Ok(new { rows = new object[0], totals = new { } });

                }

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }



        // ─── Helper: Parse period yyyy-MM → (year, month) ─────────────────────────────

        private static (int year, int month, DateTime periodStart, DateTime periodEnd) ParsePeriod(string period)

        {

            int year = DateTime.Now.Year;

            int month = DateTime.Now.Month;

            if (!string.IsNullOrEmpty(period) && period.Contains("-"))

            {

                var parts = period.Split('-');

                int.TryParse(parts[0], out year);

                int.TryParse(parts[1], out month);

            }

            var start = new DateTime(year, month, 1);

            var end = start.AddMonths(1).AddDays(-1);

            return (year, month, start, end);

        }



        // ─── Helper: Calculate PF (Delegated to Service) ────

        private PFCalculationResult CalculatePF(decimal basic, decimal da, DateTime date)

        {

            return _pfService.CalculatePF(basic, da, date);

        }



        private (decimal EmployerEPS, decimal EmployerEPF, decimal TotalER) CalculateEmployerPF(PFCalculationResult pfResult)

        {

            // Standard split if service doesn't provide EPS/EPF explicitly yet

            // 8.33% to EPS, 3.67% to EPF based on eligible wages

            decimal epfWage = pfResult.EligibleWages;

            decimal eps = Math.Round(epfWage * 0.0833m, 2);   // 8.33% → EPS

            decimal epf = pfResult.EmployerContribution - eps;

            return (eps, epf, pfResult.EmployerContribution);

        }



        // ─── Helper: Calculate ESI (Delegated to Service) ─────────────────

        private ESICalculationResult CalculateESI(decimal gross, DateTime date)

        {

            return _esiService.CalculateESI(gross, date);

        }



        // ─── Helper: Calculate PT per state (Delegated to Service) ────

        private decimal CalculatePT(decimal gross, string? state, DateTime date)

        {

            var result = _ptService.CalculateProfessionalTax(gross, state ?? "Telangana", date);

            return result.TaxAmount;

        }



        // ─── Helper: Calculate PT based on 6-month gross salary (current month * 6) ────

        private decimal CalculatePTBasedOnSixMonthGross(decimal currentMonthGross, string employeeCode, string? state, DateTime currentDate)

        {

            try

            {

                // Calculate 6-month gross salary by multiplying current month by 6

                decimal sixMonthGross = currentMonthGross * 6;



                // Calculate PT on 6-month gross salary

                var ptResult = _ptService.CalculateProfessionalTax(sixMonthGross, state ?? "Telangana", currentDate);

                

                // The PT service already handles semi-annual conversion (divides by 6)

                // So we get the monthly amount directly

                return ptResult.TaxAmount;

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error calculating PT based on 6-month gross salary for employee {EmployeeCode}", employeeCode);

                // Fallback to regular monthly calculation

                return CalculatePT(currentMonthGross, state, currentDate);

            }

        }



        // ─── Helper: Get real payroll rows from DB ───────────────────────────────────

        private async Task<List<dynamic>> GetPayrollRows(string branch, string client, DateTime periodStart, DateTime periodEnd)

        {

            // Resolve the client Name from the Code, since AttendanceDetails.Client sometimes stores the Name instead of Code

            string clientName = string.IsNullOrEmpty(client)

                ? ""

                : await _context.ClientMasters.Where(c => c.Code == client).Select(c => c.Name).FirstOrDefaultAsync() ?? "";



            // 1. Fetch attendance counts from AttendanceDetails as a fallback for un-processed payroll

            // Include working days (Type 1: General Working, Type 2: Off Day, Type 3: Off Day Working, Type 5: Holiday Working)

            // Include holidays (Type 4: Holiday) - employees are paid for holidays so they count as working days

            // Include paid leave days (Type 8-17: Annual Leave, Medical Leave, Maternity Leave, Paternity Leave, Hospitalization Leave, Socso, Non Schedule Off, Replacement Leave, Compensate Leave, Marriage Leave)

            // EXCLUDE Unpaid Leave (6) and Absent (7) - these should be deducted from salary and NOT counted as working days

            // REVERT: Include Client field in DISTINCT so that if employee works for multiple clients on same day, they are counted separately for each client

            var attendanceCounts = await (from att in _context.Attendances

                                          join det in _context.AttendanceDetails on att.ID equals det.AttendanceID

                                          where att.Period >= periodStart && att.Period <= periodEnd

                                          && (string.IsNullOrEmpty(client) || det.Client == client || det.Client == clientName)

                                          && new[] { 1, 2, 3, 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }.Contains(det.Type) // Working days + holidays + off days + paid leaves (excluding 6=Unpaid Leave, 7=Absent)

                                          select new { att.EmployeeID, det.AttendanceDate, det.Client })

                                         .Distinct()

                                         .GroupBy(x => x.EmployeeID)

                                         .Select(g => new { EmployeeID = g.Key, Count = g.Count() })

                                         .ToDictionaryAsync(x => x.EmployeeID, x => x.Count);



            // 2. Main query with LEFT JOIN on PaySlips, Attendances, and SalaryAdvance

            // FIX: Use a subquery to check if employee has ANY attendance for the client in the period

            var employeesWithClientAttendance = string.IsNullOrEmpty(client)

                ? null

                : await (from att in _context.Attendances

                         join det in _context.AttendanceDetails on att.ID equals det.AttendanceID

                         where att.Period >= periodStart && att.Period <= periodEnd

                         && (det.Client == client || det.Client == clientName)

                         select att.EmployeeID).Distinct().ToListAsync();



            // FIX: Aggregate SalaryAdvances per employee to prevent duplicate rows when employee has multiple advances (daily + monthly)

            var aggregatedAdvances = await (from sa in _context.SalaryAdvances

                                            where sa.AdvanceDate >= periodStart && sa.AdvanceDate <= periodEnd && !sa.IsDeleted

                                            group sa by sa.EmployeeID into g

                                            select new

                                            {

                                                EmployeeID = g.Key,

                                                TotalAmount = g.Sum(x => x.Amount),

                                                // Take the latest advance date and voucher no

                                                LatestDate = g.Max(x => x.AdvanceDate),

                                                LatestVoucherNo = g.OrderByDescending(x => x.AdvanceDate).Select(x => x.VoucherNo).FirstOrDefault()

                                            }).ToDictionaryAsync(x => x.EmployeeID, x => new { x.TotalAmount, x.LatestDate, x.LatestVoucherNo });



            var query = from emp in _context.Employees

                        join empDetail in _context.EmploymentDetails on emp.EMP_CODE equals empDetail.EMPPAY_CODE

                        join ps in _context.PaySlips on new { ID = emp.EMP_ID, P = periodEnd } equals new { ID = ps.EmployeeID, P = ps.Period } into psGroup

                        from ps in psGroup.DefaultIfEmpty()

                        join att in _context.Attendances on new { ID = emp.EMP_ID, P = periodEnd } equals new { ID = att.EmployeeID, P = att.Period } into attGroup

                        from att in attGroup.DefaultIfEmpty()

                        where (string.IsNullOrEmpty(branch) || emp.EMP_BRANCH_CODE == branch)

                             && (string.IsNullOrEmpty(client) || (employeesWithClientAttendance != null && employeesWithClientAttendance.Contains(emp.EMP_ID)))

                             && (att != null || ps != null) // Ensure there's at least some recording for the month

                        select new

                        {

                            emp.EMP_ID,

                            emp.EMP_CODE,

                            emp.EMP_NAME,

                            emp.EMP_SEX,

                            emp.EMP_ROLE,

                            emp.EMP_DATE_OF_BIRTH,

                            emp.UANNumber,

                            emp.ESINumber,

                            emp.PANNumber,

                            emp.PFAccountNumber,

                            emp.EMP_BRANCH_CODE,

                            emp.BankName,

                            emp.BankIFSC,

                            emp.BankAccountNumber,

                            emp.IndianState,

                            empDetail.EMPPAY_DATE_JOINED,

                            empDetail.EMPPAY_DATE_RESIGNED,

                            empDetail.EMPPAY_JOB_TITLE,

                            empDetail.EMPPAY_BASIC_RATE,

                            FixedHRA = empDetail.ATTENDANCEALLOWANCE,

                            FixedSpecialAllowance = empDetail.SpecialAllowance,

                            AttendanceAllowanceWorkingDays = empDetail.AttendanceAllowanceWorkingDays,

                            // CB (Cost Breakdown) fields from Employee table - Simplified

                            CB_Basic = emp.CB_Basic,

                            CB_DA = emp.CB_DA,

                            CB_HRA = emp.CB_HRA,

                            CB_HRAPercentage = emp.CB_HRAPercentage,

                            CB_Leaves = emp.CB_Leaves,

                            CB_LeavesPercentage = emp.CB_LeavesPercentage,

                            CB_OtherAllowances = emp.CB_OtherAllowances,

                            CB_NH = emp.CB_NH,

                            CB_NHPercentage = emp.CB_NHPercentage,

                            CB_AdvanceStatutoryBonus = emp.CB_AdvanceStatutoryBonus,

                            CB_AdvanceStatutoryBonusPercentage = emp.CB_AdvanceStatutoryBonusPercentage,

                            CB_SubTotal = emp.CB_SubTotal,

                            // PaySlip data (nullable)

                            ps_BasicSalary = (decimal?)ps.BasicSalary,

                            ps_BasicSalaryRate = (decimal?)ps.BasicSalaryRate,

                            ps_BasicSalaryDays = (decimal?)ps.BasicSalaryDays,

                            ps_AttendanceAllowance = (decimal?)ps.AttendanceAllowance,

                            ps_SpecialAllowance = (decimal?)ps.SpecialAllowance,

                            ps_OverTimeSalary = (decimal?)ps.OverTimeSalary,

                            ps_Shift2Salary = (decimal?)ps.Shift2Salary,

                            ps_OffDaySalary = (decimal?)ps.OffDaySalary,

                            ps_HolidaySalary = (decimal?)ps.HolidaySalary,

                            ps_Bonus = (decimal?)ps.Bonus,

                            ps_EPFDeductionAmount = (decimal?)ps.EPFDeductionAmount,

                            ps_SOCSODeductionAmount = (decimal?)ps.SOCSODeductionAmount,

                            ps_IncomeTaxDeduction = (decimal?)ps.IncomeTaxDeduction,

                            ps_MiscDeduction = (decimal?)ps.MiscDeduction,

                            ps_DailyAdvanceRecovery = (decimal?)ps.DailyAdvanceRecovery,

                            ps_SpecialAdvanceRecovery = (decimal?)ps.SpecialAdvanceRecovery,

                            ps_MonthlyAdvanceRecovery = (decimal?)ps.MonthlyAdvanceRecovery,

                            ps_UniformIssueRecovery = (decimal?)ps.UniformIssueRecovery,

                            ps_LoanRecovery = (decimal?)ps.LoanRecovery,

                            // Attendance data (fallback for OT/Bonus if PS missing)

                            att_AllowanceDeduction = (decimal?)(att != null ? att.AllowanceDeduction : 0),

                            att_SpecialAllowanceDeduction = (decimal?)(att != null ? att.SpecialAllowanceDeduction : 0),

                            att_OTAmount = (decimal?)(att != null ? att.Shift2Rate : 0),

                            att_Bonus = (decimal?)(att != null ? att.Bonus : 0),

                            MonthPeriod = periodStart

                        };



            var data = await query.ToListAsync();

            var rows = new List<dynamic>();

            int sno = 1;

            int daysInMonth = DateTime.DaysInMonth(periodEnd.Year, periodEnd.Month);



            foreach (var d in data)

            {

                bool hasPaySlip = d.ps_BasicSalary.HasValue;



                // Skip employees without salary processing (no payslip)

                if (!hasPaySlip)

                {

                    continue;

                }



                // Use actual month days (full calendar days) for compliance report calculation

                // This ensures actualDays shows the month days (30, 31) instead of configured working days (26, 4 Sundays)

                int employeeWorkingDays = daysInMonth;



                // Use attendance count for working days calculation to ensure medical leave and other paid leaves are correctly included

                // This avoids relying on PaySlip BasicSalaryDays which might have different calculation logic

                int clientDaysWorked = attendanceCounts.ContainsKey(d.EMP_ID) ? attendanceCounts[d.EMP_ID] : 0;

                int totalMonthDays = attendanceCounts.ContainsKey(d.EMP_ID) ? attendanceCounts[d.EMP_ID] : daysInMonth;



                // If client is specified, the report should only show the share for THAT client.

                // If client is empty, it shows the branch total.

                int daysWorked = !string.IsNullOrEmpty(client) ? clientDaysWorked : totalMonthDays;



                // CRITICAL FIX: Skip employees entirely if they had 0 attendance for the heavily requested filtering client.

                if (!string.IsNullOrEmpty(client) && daysWorked == 0)

                {

                    continue;

                }



                // Use CB (Cost Breakdown) fields from Employee table ONLY - Simplified

                // These are the EXACT values entered by user in Commercial Breakdown

                decimal cbBasic = d.CB_Basic ?? 0;

                decimal cbDA = d.CB_DA ?? 0;

                decimal cbHRA = d.CB_HRA ?? 0;

                decimal cbLeaves = d.CB_Leaves ?? 0;

                decimal cbOtherAllowances = d.CB_OtherAllowances ?? 0;

                decimal cbNH = d.CB_NH ?? 0;

                decimal cbAdvanceStatutoryBonus = d.CB_AdvanceStatutoryBonus ?? 0;

                decimal cbSubTotal = d.CB_SubTotal ?? 0;



                // Use SalaryAdvance data from system advance module instead of CB_Advance

                // FIX: Use aggregated advances to combine daily + monthly advances for same employee

                decimal systemAdvanceAmount = aggregatedAdvances.ContainsKey(d.EMP_ID) ? aggregatedAdvances[d.EMP_ID].TotalAmount : 0;



                // For compliance reports, use ONLY Commercial Breakdown values entered by user

                // No fallbacks - show exactly what user configured in CB

                decimal basic = cbBasic;

                decimal da = cbDA;

                decimal hra = cbHRA;

                decimal leaveWages = cbLeaves; // Use CB_Leaves from Commercial Breakdown

                decimal others = cbOtherAllowances;

                decimal nh = cbNH;

                decimal advanceStatutoryBonus = cbAdvanceStatutoryBonus; // Use CB_AdvanceStatutoryBonus from Commercial Breakdown

                decimal ot = 0;



                // Fixed salary is the sum of all fixed components from Commercial Breakdown

                // Note: advance (SalaryAdvance) is a deduction, not an earning, so it's NOT included here

                decimal fixedSalary = basic + da + hra + others + leaveWages + nh + advanceStatutoryBonus;



                // Calculate monthly gross (sum of all components)

                // Note: advance (SalaryAdvance) is a deduction, not an earning, so it's NOT included here

                decimal monthlyGross = basic + hra + da + others + ot + leaveWages + nh + advanceStatutoryBonus;



                // Calculate earned salary using formula: (Fixed Salary / ACTUAL DAYS) × No of Working Days from attendance

                // ACTUAL DAYS comes from AttendanceAllowanceWorkingDays (employee creation)

                // No of Working Days comes from actual attendance (daysWorked)

                decimal gross = daysWorked > 0 ? Math.Round((fixedSalary / employeeWorkingDays) * daysWorked, 0) : 0;



                // Deductions logic - use PaySlip data for advance recoveries

                decimal pf, esi, pt, advances, otherDed;

                decimal monthlyAdvanceRecovery = d.ps_MonthlyAdvanceRecovery ?? 0;

                decimal dailyAdvanceRecovery = d.ps_DailyAdvanceRecovery ?? 0;

                decimal loanRecovery = d.ps_LoanRecovery ?? 0;

                decimal uniformIssueRecovery = d.ps_UniformIssueRecovery ?? 0;

                decimal specialAdvanceRecovery = d.ps_SpecialAdvanceRecovery ?? 0;

                PFCalculationResult pfResult;

                ESICalculationResult esiResult;



                // Calculate PF using statutory ceiling logic (12% of Basic+DA, capped at ₹15,000 = ₹1,800 max)

                // FIX: First pro-rate basic+da based on actual days worked, then calculate PF on the pro-rated amount

                // Use employeeWorkingDays (AttendanceAllowanceWorkingDays) for pro-rating

                decimal actualBasicDA = daysWorked > 0 ? Math.Round(((basic + da) / employeeWorkingDays) * daysWorked, 0) : 0;

                pfResult = CalculatePF(actualBasicDA, 0, periodEnd);

                pf = pfResult.EmployeeContribution;



                // Calculate ESI based on actual gross salary (already pro-rated)

                esiResult = CalculateESI(gross, periodEnd);

                esi = esiResult.EmployeeContribution;



                // Calculate Professional Tax based on employee's state and period

                // PT is calculated only if employee state is available, otherwise PT = 0

                if (!string.IsNullOrEmpty(d.IndianState))

                {

                    // Tamil Nadu: Use 6-month gross calculation (current month × 6), then divide by 6

                    // Other states: Use regular monthly calculation

                    if (d.IndianState.Equals("Tamil Nadu", StringComparison.OrdinalIgnoreCase) ||

                        d.IndianState.Equals("TamilNadu", StringComparison.OrdinalIgnoreCase) ||

                        d.IndianState.Equals("TN", StringComparison.OrdinalIgnoreCase))

                    {

                        pt = CalculatePTBasedOnSixMonthGross(gross, d.EMP_CODE, d.IndianState, periodEnd);

                    }

                    else

                    {

                        // For other states, use regular monthly calculation

                        pt = CalculatePT(gross, d.IndianState, periodEnd);

                    }

                }

                else

                {

                    pt = 0;

                }



                // Total advances from PaySlip recovery fields

                advances = monthlyAdvanceRecovery + dailyAdvanceRecovery + loanRecovery + uniformIssueRecovery + specialAdvanceRecovery;

                // Use systemAdvanceAmount for deduction to ensure the displayed advance amount is actually deducted from net

                // This aligns the deduction with what's shown in the report

                advances = Math.Round(systemAdvanceAmount, 0);

                otherDed = 0;



                // LWF (Labour Welfare Fund) - Deduct 20 rupees in December month

                decimal lwf = (periodStart.Month == 12) ? 20 : 0;



                decimal totalDed = Math.Round(pf + esi + pt + advances + otherDed + lwf, 0);

                decimal net = Math.Round(gross - totalDed, 0);



                var (employerEPS, employerEPF, totalER) = CalculateEmployerPF(pfResult);



                int age = d.EMP_DATE_OF_BIRTH.HasValue

                    ? (int)((DateTime.Now - d.EMP_DATE_OF_BIRTH.Value).TotalDays / 365.25)

                    : 0;



                rows.Add(new

                {

                    sno = sno++,

                    EMP_ID = d.EMP_ID,

                    empId = d.EMP_CODE,

                    name = d.EMP_NAME,

                    sex = d.EMP_SEX?.StartsWith("M", StringComparison.OrdinalIgnoreCase) == true ? "M" : "F",

                    age = age,

                    designation = d.EMPPAY_JOB_TITLE ?? d.EMP_ROLE ?? "Security Guard",

                    uan = d.UANNumber ?? "",

                    esiNo = d.ESINumber ?? "",

                    panNo = d.PANNumber ?? "",

                    pfAccount = d.PFAccountNumber ?? "",

                    bankName = d.BankName ?? "",

                    bankIfsc = d.BankIFSC ?? "",

                    bankAccount = d.BankAccountNumber ?? "",

                    indianState = d.IndianState ?? "",

                    fatherName = "",

                    dateJoined = d.EMPPAY_DATE_JOINED.ToString("dd-MM-yyyy"),

                    // Wage components from Commercial Breakdown - rounded to whole rupees

                    basic = Math.Round(basic, 0),

                    da = Math.Round(da, 0),

                    hra = Math.Round(hra, 0),

                    others = Math.Round(others, 0),

                    bonus = Math.Round(advanceStatutoryBonus, 0),

                    advanceBonus = Math.Round(advanceStatutoryBonus, 0),

                    ot = Math.Round(ot, 0),

                    otHours = 0,

                    otAmount = Math.Round(ot, 0),

                    leaveWages = Math.Round(leaveWages, 0),

                    nh = Math.Round(nh, 0),

                    advance = Math.Round(systemAdvanceAmount, 0),

                    totalDays = daysInMonth,

                    daysWorked,

                    fixedSalary = Math.Round(fixedSalary, 0),

                    basicDA = Math.Round(basic + da, 0),

                    gross = Math.Round(gross, 0),

                    // Commercial Breakdown percentages (for reporting) - Simplified

                    cbHraPercentage = d.CB_HRAPercentage,

                    cbNHPercentage = d.CB_NHPercentage,

                    // Deductions from Commercial Breakdown - rounded to whole rupees

                    pf = Math.Round(pf, 0),

                    esi = Math.Round(esi, 0),

                    pt = Math.Round(pt, 0),

                    advanceDed = Math.Round(advances, 0),

                    monthlyAdvanceRecovery = Math.Round(monthlyAdvanceRecovery, 0),

                    dailyAdvanceRecovery = Math.Round(dailyAdvanceRecovery, 0),

                    loanRecovery = Math.Round(loanRecovery, 0),

                    uniformIssueRecovery = Math.Round(uniformIssueRecovery, 0),

                    specialAdvanceRecovery = Math.Round(specialAdvanceRecovery, 0),

                    fines = 0,

                    lwf = lwf,

                    mobile = 0,

                    arrear = Math.Round(otherDed, 0),

                    totalDeductions = Math.Round(totalDed, 0),

                    net = Math.Round(net, 0),

                    isPFApplicable = pf > 0,

                    isESIApplicable = esi > 0,

                    // Employer contributions - rounded to whole rupees

                    employerPF = Math.Round(totalER, 0),

                    employerEPS = Math.Round(employerEPS, 0),

                    employerEPF = Math.Round(employerEPF, 0),

                    // EPF/EPS wages

                    epfWages = pfResult.EligibleWages,

                    epsWages = pfResult.EligibleWages,

                    employeeEPFShare = Math.Round(pf, 0),

                    payType = "Monthly",

                    wagePeriod = d.MonthPeriod.ToString("yyyy-MM"),

                    salaryDate = d.MonthPeriod.ToString("MMM yyyy"),

                    // Aliases for different frontend templates

                    actualDays = employeeWorkingDays, // Employee creation attendance working days

                    attendance = daysWorked, // Actual worked days

                    workDays = daysWorked, // Actual worked days

                    attendanceCount = daysWorked, // Actual worked days

                    basicWages = Math.Round(basic, 0),

                    remarks = "",

                    signature = "",

                    workerSignature = "",

                    inchargeSignature = "",

                    reportDate = DateTime.Now.ToString("dd-MM-yyyy"),

                    submissionDate = DateTime.Now.AddDays(1).ToString("dd-MM-yyyy"),

                    terminationDate = ""

                });

            }



            return rows;

        }



        // ─── Form XVII – Register of Wages ───────────────────────────────────────────

        private async Task<object> GenerateFormXVIIData(ComplianceReportRequest request)

        {

            var period = request?.Parameters?.Period ?? DateTime.Now.ToString("yyyy-MM");

            var branch = request?.Parameters?.Branch ?? "";

            var client = request?.Parameters?.Client ?? "";

            var (year, month, periodStart, periodEnd) = ParsePeriod(period);



            var rows = await GetPayrollRows(branch, client, periodStart, periodEnd);



            var totals = ComputeTotalsXVII(rows);

            var metadata = BuildMetadata(request, branch, client, period, year, month);

            return new { rows, totals, metadata };

        }



        private Dictionary<string, object> ComputeTotalsXVII(List<dynamic> rows)

        {

            decimal Sum(Func<dynamic, decimal> sel) => rows.Aggregate(0m, (acc, r) => acc + sel(r));

            return new Dictionary<string, object>

            {

                ["TotalBasic"] = Math.Round(Sum(r => r.basic), 0),

                ["TotalDA"] = Math.Round(Sum(r => r.da), 0),

                ["TotalHRA"] = Math.Round(Sum(r => r.hra), 0),

                ["TotalOthers"] = Math.Round(Sum(r => r.others), 0),

                ["TotalBonus"] = Math.Round(Sum(r => r.bonus), 0),

                ["TotalLeaveWages"] = Math.Round(Sum(r => r.leaveWages), 0),

                ["TotalNH"] = Math.Round(Sum(r => r.nh), 0),

                ["TotalAdvance"] = Math.Round(Sum(r => r.advance), 0),

                ["TotalAdvanceBonus"] = Math.Round(Sum(r => r.bonus ?? 0), 0),

                ["TotalActualDays"] = Sum(r => r.actualDays),

                ["TotalFixedSalary"] = Math.Round(Sum(r => r.fixedSalary), 0),

                ["TotalWorkDays"] = Sum(r => r.workDays),

                ["TotalBasicFixed"] = Math.Round(Sum(r => r.basic), 0),

                ["TotalDAFixed"] = Math.Round(Sum(r => r.da), 0),

                ["TotalOthersFixed"] = Math.Round(Sum(r => r.others), 0),

                ["TotalHRAFixed"] = Math.Round(Sum(r => r.hra), 0),

                ["TotalLeaveWagesFixed"] = Math.Round(Sum(r => r.leaveWages), 0),

                ["TotalNHFixed"] = Math.Round(Sum(r => r.nh), 0),

                ["TotalAdvanceFixed"] = Math.Round(Sum(r => r.advance), 0),

                ["TotalBonusFixed"] = Math.Round(Sum(r => r.bonus), 0),

                ["TotalAdvanceBonusFixed"] = Math.Round(Sum(r => r.bonus ?? 0), 0),

                ["TotalOT"] = Sum(r => r.otHours),

                ["TotalOTAmount"] = Math.Round(Sum(r => r.otAmount), 0),

                ["TotalGross"] = Math.Round(Sum(r => r.gross), 0),

                ["TotalPF"] = Math.Round(Sum(r => r.pf), 0),

                ["TotalESI"] = Math.Round(Sum(r => r.esi), 0),

                ["TotalPT"] = Math.Round(Sum(r => r.pt), 0),

                ["TotalAdvanceDed"] = Math.Round(Sum(r => r.advanceDed), 0),

                ["TotalLWF"] = Math.Round(Sum(r => r.lwf), 0),

                ["TotalDeductions"] = Math.Round(Sum(r => r.totalDeductions), 0),

                ["TotalMobile"] = Math.Round(Sum(r => r.mobile), 0),

                ["TotalArrear"] = Math.Round(Sum(r => r.arrear), 0),

                ["TotalNet"] = Math.Round(Sum(r => r.net), 0)

            };

        }



        // ─── Form XXVI – Muster Roll ──────────────────────────────────────────────────

        private async Task<object> GenerateFormXXVIData(ComplianceReportRequest request)

        {

            var period = request?.Parameters?.Period ?? DateTime.Now.ToString("yyyy-MM");

            var branch = request?.Parameters?.Branch ?? "";

            var client = request?.Parameters?.Client ?? "";

            var (year, month, periodStart, periodEnd) = ParsePeriod(period);

            int daysInMonth = DateTime.DaysInMonth(year, month);



            string clientName = string.IsNullOrEmpty(client)

                ? ""

                : await _context.ClientMasters.Where(c => c.Code == client).Select(c => c.Name).FirstOrDefaultAsync() ?? "";



            var rows = await GetPayrollRows(branch, client, periodStart, periodEnd);



            // Fetch actual daily attendance statuses specifically for the selected client if provided

            // FIX: Only check Client field (not OTClient) to prevent double-counting

            var dailyDetails = await (from att in _context.Attendances

                                      join det in _context.AttendanceDetails on att.ID equals det.AttendanceID

                                      join emp in _context.Employees on att.EmployeeID equals emp.EMP_ID

                                      where att.Period >= periodStart && att.Period <= periodEnd

                                      && (string.IsNullOrEmpty(branch) || emp.EMP_BRANCH_CODE == branch)

                                      && (string.IsNullOrEmpty(client) || det.Client == client || det.Client == clientName)

                                      select new { att.EmployeeID, Day = det.AttendanceDate.Day, det.Type })

                                     .ToListAsync();



            var dailyMap = dailyDetails.GroupBy(x => x.EmployeeID)

                                       .ToDictionary(g => g.Key, g => g.GroupBy(x => x.Day).ToDictionary(x => x.Key, x => GetStatusChar(x.First().Type)));



            // Add day columns

            int sno = 1;

            var musterRows = rows.Select(row =>

            {

                int empId = (int?)row.EMP_ID ?? 0;

                var dict = new Dictionary<string, object>

                {

                    ["sno"] = sno++,

                    ["empId"] = (string)row.empId,

                    ["name"] = (string)row.name,

                    ["age"] = (int)row.age,

                    ["sex"] = (string)row.sex,

                    ["designation"] = (string)row.designation,

                    ["fatherName"] = (string)row.fatherName,

                    ["daysWorked"] = (int)row.daysWorked,

                    ["workerSignature"] = "",

                    ["reportDate"] = (string)row.reportDate,

                    ["submissionDate"] = (string)row.submissionDate,

                    ["terminationDate"] = "",

                    ["inchargeSignature"] = "",

                    // Aliases

                    ["actualDays"] = (int)row.daysWorked,

                    ["attendance"] = (int)row.daysWorked,

                    ["attendanceCount"] = (int)row.daysWorked

                };



                var empDaily = dailyMap.ContainsKey(empId) ? dailyMap[empId] : new Dictionary<int, string>();

                for (int d = 1; d <= daysInMonth; d++)

                {

                    dict[$"day{d}"] = empDaily.ContainsKey(d) ? empDaily[d] : "A";

                }

                return dict;

            }).ToList();



            var totals = new Dictionary<string, object> { ["TotalDays"] = musterRows.Sum(r => (int)r["daysWorked"]) };

            var metadata = BuildMetadata(request, branch, client, period, year, month);

            return new { rows = musterRows, totals, metadata };

        }



        private string GetStatusChar(int type)

        {

            return type switch

            {

                1 => "P",  // General Working

                2 => "O",  // Off Day

                3 => "P",  // Off Day Working

                4 => "H",  // Holiday

                5 => "P",  // Holiday Working

                6 => "L",  // Unpaid Leave

                7 => "A",  // Absent

                8 => "L",  // Annual Leave

                9 => "L",  // Casual Leave

                10 => "L", // Sick Leave

                11 => "L", // Maternity Leave

                12 => "L", // Paternity Leave

                15 => "L", // Compensatory Off

                16 => "L", // Restricted Holiday

                17 => "L", // Other Leave

                _ => "A"

            };

        }



        // ─── Form XXVII – Register of Wages (detailed) ───────────────────────────────

        private async Task<object> GenerateFormXXVIIData(ComplianceReportRequest request)

        {

            var period = request?.Parameters?.Period ?? DateTime.Now.ToString("yyyy-MM");

            var branch = request?.Parameters?.Branch ?? "";

            var client = request?.Parameters?.Client ?? "";

            var (year, month, periodStart, periodEnd) = ParsePeriod(period);



            var rows = await GetPayrollRows(branch, client, periodStart, periodEnd);



            var totals = new Dictionary<string, object>

            {

                ["TotalDays"] = rows.Sum(r => (int)r.totalDays),

                ["TotalWorkedDays"] = rows.Sum(r => (int)r.daysWorked),

                // Fixed salary totals

                ["TotalBasicFixed"] = Math.Round(rows.Sum(r => (decimal)r.basic), 2),

                ["TotalDAFixed"] = Math.Round(rows.Sum(r => (decimal)r.da), 2),

                ["TotalOthersFixed"] = Math.Round(rows.Sum(r => (decimal)r.others), 2),

                ["TotalHRAFixed"] = Math.Round(rows.Sum(r => (decimal)r.hra), 2),

                ["TotalLeaveWagesFixed"] = Math.Round(rows.Sum(r => (decimal)r.leaveWages), 2),

                ["TotalNHFixed"] = Math.Round(rows.Sum(r => (decimal)r.nh), 2),

                ["TotalAdvanceFixed"] = Math.Round(rows.Sum(r => (decimal)r.advance), 2),

                ["TotalBonusFixed"] = Math.Round(rows.Sum(r => (decimal)r.bonus), 2),

                ["TotalActualDays"] = rows.Sum(r => (int)r.daysWorked),

                ["TotalFixedSalary"] = Math.Round(rows.Sum(r => (decimal)r.fixedSalary), 2),

                ["TotalWorkDays"] = rows.Sum(r => (int)r.daysWorked),

                // Earned salary totals

                ["TotalBasic"] = Math.Round(rows.Sum(r => (decimal)r.basic), 2),

                ["TotalDA"] = Math.Round(rows.Sum(r => (decimal)r.da), 2),

                ["TotalOthers"] = Math.Round(rows.Sum(r => (decimal)r.others), 2),

                ["TotalHRA"] = Math.Round(rows.Sum(r => (decimal)r.hra), 2),

                ["TotalLeaveWages"] = Math.Round(rows.Sum(r => (decimal)r.leaveWages), 2),

                ["TotalNH"] = Math.Round(rows.Sum(r => (decimal)r.nh), 2),

                ["TotalAdvance"] = Math.Round(rows.Sum(r => (decimal)r.advance), 2),

                ["TotalBonus"] = Math.Round(rows.Sum(r => (decimal)r.bonus), 2),

                ["TotalOT"] = rows.Sum(r => (int)r.otHours),

                ["TotalOTAmount"] = Math.Round(rows.Sum(r => (decimal)r.otAmount), 2),

                ["TotalGross"] = Math.Round(rows.Sum(r => (decimal)r.gross), 2),

                // Deductions

                ["TotalPF"] = Math.Round(rows.Sum(r => (decimal)r.pf), 2),

                ["TotalESI"] = Math.Round(rows.Sum(r => (decimal)r.esi), 2),

                ["TotalPT"] = Math.Round(rows.Sum(r => (decimal)r.pt), 2),

                ["TotalAdvanceDed"] = Math.Round(rows.Sum(r => (decimal)r.advanceDed), 2),

                ["TotalLWF"] = Math.Round(rows.Sum(r => (decimal)r.lwf), 2),

                ["TotalDeductions"] = Math.Round(rows.Sum(r => (decimal)r.totalDeductions), 2),

                ["TotalMobile"] = Math.Round(rows.Sum(r => (decimal)r.mobile), 2),

                ["TotalArrear"] = Math.Round(rows.Sum(r => (decimal)r.arrear), 2),

                ["TotalNet"] = Math.Round(rows.Sum(r => (decimal)r.net), 2)

            };



            var metadata = BuildMetadata(request, branch, client, period, year, month);

            return new { rows, totals, metadata };

        }



        // ─── Form XXVIII – Individual Wage Slips ─────────────────────────────────────

        private async Task<object> GenerateFormXXVIIIWageSlips(ComplianceReportRequest request)

        {

            var period = request?.Parameters?.Period ?? DateTime.Now.ToString("yyyy-MM");

            var branch = request?.Parameters?.Branch ?? "";

            var client = request?.Parameters?.Client ?? "";

            var showIndividual = request?.Parameters?.ShowIndividualSlips ?? false;

            var (year, month, periodStart, periodEnd) = ParsePeriod(period);



            var rows = await GetPayrollRows(branch, client, periodStart, periodEnd);



            // Map to wage slip format

            var wageSlips = rows.Select(r => new

            {

                EmployeeName = (string)r.name,

                EmployeeCode = (string)r.empId,

                Designation = (string)r.designation,

                Basic = (decimal)r.basic,

                DA = (decimal)r.da,

                HRA = (decimal)r.hra,

                Others = (decimal)r.others,

                Bonus = (decimal)r.bonus,

                OT = (decimal)r.ot,

                Gross = (decimal)r.gross,

                // Commercial Breakdown percentages (for reporting) - Simplified

                cbHraPercentage = r.cbHraPercentage,

                cbNHPercentage = r.cbNHPercentage,

                // Deductions from Commercial Breakdown

                PF = (decimal)r.pf,

                ESI = (decimal)r.esi,

                PT = (decimal)r.pt,

                AdvanceDed = (decimal)r.advanceDed,

                OtherDed = 0m,

                TotalDed = (decimal)r.totalDeductions,

                Net = (decimal)r.net,

                DaysWorked = (int)r.daysWorked,

                SalaryDate = (string)r.salaryDate

            }).ToList();



            var totals = new Dictionary<string, object>

            {

                ["TotalBasic"] = Math.Round(wageSlips.Sum(r => r.Basic), 2),

                ["TotalDA"] = Math.Round(wageSlips.Sum(r => r.DA), 2),

                ["TotalHRA"] = Math.Round(wageSlips.Sum(r => r.HRA), 2),

                ["TotalBonus"] = Math.Round(wageSlips.Sum(r => r.Bonus), 2),

                ["TotalGross"] = Math.Round(wageSlips.Sum(r => r.Gross), 2),

                ["TotalDeduct"] = Math.Round(wageSlips.Sum(r => r.TotalDed), 2),

                ["TotalNet"] = Math.Round(wageSlips.Sum(r => r.Net), 2),

                ["SalaryDate"] = new DateTime(year, month, 1).ToString("MMM yyyy")

            };



            var metadata = BuildMetadata(request, branch, client, period, year, month);

            metadata["Establishment"] = metadata["ContractorName"];

            metadata["SalaryDate"] = new DateTime(year, month, 1).ToString("MMM yyyy");



            return new { rows = wageSlips, totals, metadata, showIndividual };

        }



        // ─── Form XXIX – Advances Register ───────────────────────────────────────────

        private async Task<object> GenerateFormXXIXData(ComplianceReportRequest request)

        {

            var period = request?.Parameters?.Period ?? DateTime.Now.ToString("yyyy-MM");

            var branch = request?.Parameters?.Branch ?? "";

            var client = request?.Parameters?.Client ?? "";

            var (year, month, periodStart, periodEnd) = ParsePeriod(period);



            string clientName = string.IsNullOrEmpty(client)

                ? ""

                : await _context.ClientMasters.Where(c => c.Code == client).Select(c => c.Name).FirstOrDefaultAsync() ?? "";



            // FIX: Get employees who have attendance for the client in the period

            var employeesWithClientAttendance = string.IsNullOrEmpty(client)

                ? null

                : await (from att in _context.Attendances

                         join det in _context.AttendanceDetails on att.ID equals det.AttendanceID

                         where att.Period >= periodStart && att.Period <= periodEnd

                         && (det.Client == client || det.Client == clientName)

                         select att.EmployeeID).Distinct().ToListAsync();



            // Fetch salary advances and filter by branch/client

            // FIX: Aggregate advances per employee to combine daily + monthly advances

            var advances = await (from sa in _context.SalaryAdvances

                                  join emp in _context.Employees on sa.EmployeeID equals emp.EMP_ID

                                  where sa.AdvanceDate >= periodStart && sa.AdvanceDate <= periodEnd

                                       && (string.IsNullOrEmpty(branch) || emp.EMP_BRANCH_CODE == branch)

                                       && (string.IsNullOrEmpty(client) || (employeesWithClientAttendance != null && employeesWithClientAttendance.Contains(emp.EMP_ID)))

                                       && !sa.IsDeleted

                                  group sa by new { emp.EMP_CODE, emp.EMP_NAME } into g

                                  select new

                                  {

                                      EMP_CODE = g.Key.EMP_CODE,

                                      EMP_NAME = g.Key.EMP_NAME,

                                      TotalAmount = g.Sum(x => x.Amount),

                                      LatestDate = g.Max(x => x.AdvanceDate),

                                      TransType = g.FirstOrDefault() != null ? g.FirstOrDefault().TransType : 0,

                                      Particulars = g.FirstOrDefault() != null ? g.FirstOrDefault().Particulars : ""

                                  }).ToListAsync();



            // Group by employee

            var grouped = advances

                .Select((g, idx) => new

                {

                    sno = idx + 1,

                    name = g.EMP_NAME,

                    empId = g.EMP_CODE,

                    // FIX: Show total combined amount instead of separate advances

                    advance1Date = g.LatestDate.ToString("dd-MM-yyyy"),

                    advance1Amount = g.TotalAmount,

                    advance2Date = "",

                    advance2Amount = 0,

                    advance3Date = "",

                    advance3Amount = 0,

                    deduction1Date = "",

                    deduction1Amount = 0,

                    deduction2Amount = 0,

                    fine1Date = "",

                    fine1Amount = 0,

                    fine2Amount = 0,

                    signature = ""

                }).ToList<object>();



            var totals = new Dictionary<string, object>

            {

                ["Advance1Total"] = grouped.Sum(r => { dynamic d = r; return (decimal)d.advance1Amount; }),

                ["Advance2Total"] = grouped.Sum(r => { dynamic d = r; return (decimal)d.advance2Amount; }),

                ["Advance3Total"] = grouped.Sum(r => { dynamic d = r; return (decimal)d.advance3Amount; }),

                ["Deduction1Total"] = 0,

                ["Deduction2Total"] = 0,

                ["Fine1Total"] = 0,

                ["Fine2Total"] = 0

            };



            var metadata = BuildMetadata(request, branch, client, period, year, month);

            return new { rows = grouped, totals, metadata };

        }



        // ─── Overtime Register ────────────────────────────────────────────────────────

        private async Task<object> GenerateOvertimeData(ComplianceReportRequest request)

        {

            var period = request?.Parameters?.Period ?? DateTime.Now.ToString("yyyy-MM");

            var branch = request?.Parameters?.Branch ?? "";

            var client = request?.Parameters?.Client ?? "";

            var (year, month, periodStart, periodEnd) = ParsePeriod(period);



            // Employees with overtime indicators filtered by Client/Branch

            var overtimeRows = await (from emp in _context.Employees

                                      join empD in _context.EmploymentDetails on emp.EMP_CODE equals empD.EMPPAY_CODE

                                      join att in _context.Attendances on emp.EMP_ID equals att.EmployeeID

                                      join det in _context.AttendanceDetails on att.ID equals det.AttendanceID

                                      where att.Period >= periodStart && att.Period <= periodEnd

                                           && (det.OTClient == client || (string.IsNullOrEmpty(client) && att.Shift2Rate > 0))

                                           && (string.IsNullOrEmpty(branch) || emp.EMP_BRANCH_CODE == branch)

                                      select new

                                      {

                                          emp.EMP_NAME,

                                          EMP_CODE = emp.EMP_CODE,

                                          Designation = empD.EMPPAY_JOB_TITLE ?? emp.EMP_ROLE,

                                          att.AllowanceDeduction,

                                          att.Shift2Rate,

                                          att.Period,

                                          det.AttendanceDate,

                                          det.OTTimeStart,

                                          det.OTTimeEnd,

                                          det.OTClient

                                      }).ToListAsync();



            var rows = overtimeRows.Select(r => new

            {

                employeeName = r.EMP_NAME,

                employeeCode = r.EMP_CODE,

                fathersName = "",

                sex = "",

                designation = r.Designation ?? "Security Guard",

                overtimeWorkedDate = r.Period.ToString("yyyy-MM-dd"),

                totalOvertimeHours = 0, // Hours not tracked in current schema

                normalRateOfWages = r.AllowanceDeduction,

                recoveryCompletedDate = "",

                overtimeRate = r.Shift2Rate,

                overtimeEarnings = r.Shift2Rate,

                overtimeWagePaidDate = r.Period.ToString("yyyy-MM-dd"),

                remarks = ""

            }).ToList<object>();



            var totals = new Dictionary<string, object>

            {

                ["TotalOTHours"] = 0,

                ["TotalOTAmount"] = overtimeRows.Sum(r => r.Shift2Rate)

            };



            var metadata = BuildMetadata(request, branch, client, period, year, month);

            metadata["DateRange"] = period;

            return new { rows, totals, metadata };

        }



        // ─── PF Statement ─────────────────────────────────────────────────────────────

        [HttpPost("GetPFStatement")]

        public async Task<IActionResult> GetPFStatement([FromBody] System.Text.Json.JsonElement body)

        {

            try

            {

                // Accept both { Parameters: { branch, client, period } } and flat { branch, client, period }

                string period = "", branch = "", client = "";

                ExtractParams(body, out period, out branch, out client);

                if (string.IsNullOrEmpty(period)) period = DateTime.Now.ToString("yyyy-MM");



                var (year, month, periodStart, periodEnd) = ParsePeriod(period);

                var rows = await GetPayrollRows(branch, client, periodStart, periodEnd);



                var pfRows = rows.Select((r, idx) =>

                {

                    // Use pre-calculated age from GetPayrollRows and check if above 55

                    int age = (int)(r.age ?? 0);

                    bool isAbove55 = age >= 55;



                    // Apply age-based logic: EPS = 0 and PF salary = 0 if age > 55

                    decimal epfWages = (decimal)r.epfWages;

                    decimal epsWages = (decimal)r.epsWages;

                    decimal edliWages = epfWages; // EDLI typically same as EPF wages

                    decimal epfContributionRemitted = (decimal)r.employeeEPFShare;

                    decimal epsContributionRemitted = (decimal)r.employerEPS;

                    decimal epfDifference = (decimal)r.employerEPF;

                    decimal ncpDays = 0; // TODO: Calculate from attendance

                    decimal refundOfAdvances = 0; // TODO: Get from salary advances

                    decimal numberOfDays = (int)r.daysWorked;



                    if (isAbove55)

                    {

                        epfWages = 0;

                        epsWages = 0;

                        edliWages = 0;

                        epfContributionRemitted = 0;

                        epsContributionRemitted = 0;

                        epfDifference = 0;

                    }



                    return new

                    {

                        sno = idx + 1,

                        uan = (string)r.uan,

                        memberName = (string)r.name,

                        grossWages = (decimal)r.gross,

                        epfWages = epfWages,

                        epsWages = epsWages,

                        edliWages = edliWages,

                        epfContributionRemitted = epfContributionRemitted,

                        epsContributionRemitted = epsContributionRemitted,

                        epfDifference = epfDifference,

                        ncpDays = ncpDays,

                        refundOfAdvances = refundOfAdvances,

                        numberOfDays = numberOfDays,

                        age = age,

                        isAbove55 = isAbove55

                    };

                }).ToList();



                var totals = new Dictionary<string, object>

                {

                    ["TotalGrossWages"] = pfRows.Sum(r => r.grossWages),

                    ["TotalEPFWages"] = pfRows.Sum(r => r.epfWages),

                    ["TotalEPSWages"] = pfRows.Sum(r => r.epsWages),

                    ["TotalEPFContributionRemitted"] = pfRows.Sum(r => r.epfContributionRemitted),

                    ["TotalEPSContributionRemitted"] = pfRows.Sum(r => r.epsContributionRemitted),

                    ["TotalEPFDifference"] = pfRows.Sum(r => r.epfDifference),

                    ["GeneratedDate"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm"),

                    ["ReportTitle"] = $"PF Statement - {new DateTime(year, month, 1):MMMM yyyy}",

                    ["Branch"] = branch,

                    ["Client"] = client,

                    ["Period"] = $"{new DateTime(year, month, 1):MMMM yyyy}"

                };



                return Ok(new { rows = pfRows, totals });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { error = ex.Message });

            }

        }



        // ─── ESI Statement ────────────────────────────────────────────────────────────

        [HttpPost("GetESIStatement")]

        public async Task<IActionResult> GetESIStatement([FromBody] System.Text.Json.JsonElement body)

        {

            try

            {

                string period = "", branch = "", client = "";

                ExtractParams(body, out period, out branch, out client);

                if (string.IsNullOrEmpty(period)) period = DateTime.Now.ToString("yyyy-MM");



                var (year, month, periodStart, periodEnd) = ParsePeriod(period);

                var rows = await GetPayrollRows(branch, client, periodStart, periodEnd);



                // Format ESI Statement data with required columns

                var esiRows = rows.Select((r, idx) =>

                {

                    decimal totalMonthlyWages = (decimal)r.gross;

                    int noOfDaysForWhichWagesPaid = (int)r.daysWorked;

                    string reasonCodeForZeroWorkingDays = noOfDaysForWhichWagesPaid == 0 ? "N" : "";

                    // Use terminationDate instead of resignDate (property name in GetPayrollRows)

                    string lastWorkingDay = !string.IsNullOrEmpty(r.terminationDate) ? r.terminationDate : "";



                    return new

                    {

                        sno = idx + 1,

                        ipNumber = (string)r.esiNo,

                        ipName = (string)r.name,

                        noOfDaysForWhichWagesPaid = noOfDaysForWhichWagesPaid,

                        totalMonthlyWages = totalMonthlyWages,

                        reasonCodeForZeroWorkingDays = reasonCodeForZeroWorkingDays,

                        lastWorkingDay = lastWorkingDay

                    };

                }).ToList();



                var totals = new Dictionary<string, object>

                {

                    ["TotalMonthlyWages"] = esiRows.Sum(r => r.totalMonthlyWages),

                    ["TotalDaysWorked"] = esiRows.Sum(r => r.noOfDaysForWhichWagesPaid),

                    ["GeneratedDate"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm"),

                    ["ReportTitle"] = $"ESI Statement - {new DateTime(year, month, 1):MMMM yyyy}",

                    ["Branch"] = branch,

                    ["Client"] = client,

                    ["Period"] = $"{new DateTime(year, month, 1):MMMM yyyy}"

                };



                return Ok(new { rows = esiRows, totals });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { error = ex.Message });

            }

        }



        // ─── PT Statement ─────────────────────────────────────────────────────────────

        [HttpPost("GetPTStatement")]

        public async Task<IActionResult> GetPTStatement([FromBody] System.Text.Json.JsonElement body)

        {

            try

            {

                string period = "", branch = "", client = "";

                ExtractParams(body, out period, out branch, out client);

                if (string.IsNullOrEmpty(period)) period = DateTime.Now.ToString("yyyy-MM");



                var (year, month, periodStart, periodEnd) = ParsePeriod(period);

                var rows = await GetPayrollRows(branch, client, periodStart, periodEnd);



                // Include ALL employees – mark PT applicability in each row so the report can show ineligible employees

                var ptRows = rows.Select((r, idx) =>

                {

                    decimal gross = (decimal)r.gross;

                    decimal pt = (decimal)r.pt;

                    bool isApplicable = pt > 0;

                    string state = r.indianState as string ?? "";

                    return new

                    {

                        sno = idx + 1,

                        empCode = (string)r.empId,

                        empName = (string)r.name,

                        panNo = (string)r.panNo,

                        state,

                        daysWorked = (int)r.daysWorked,

                        gross,

                        pt,

                        isApplicable

                    };

                }).ToList();



                var totals = new Dictionary<string, object>

                {

                    ["TotalGross"] = ptRows.Sum(r => r.gross),

                    ["TotalPT"] = ptRows.Sum(r => r.pt),

                    ["TotalEmployees"] = ptRows.Count,

                    ["TotalApplicable"] = ptRows.Count(r => r.isApplicable),

                    ["GeneratedDate"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm"),

                    ["ReportTitle"] = $"PROFESSIONAL TAX STATEMENT - {new DateTime(year, month, 1):MMMM yyyy}",

                    ["Branch"] = branch,

                    ["Client"] = client,

                    ["Period"] = $"{new DateTime(year, month, 1):MMMM yyyy}"

                };



                return Ok(new { rows = ptRows, totals });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { error = ex.Message });

            }

        }



        // ─── Compliance Issue Reports ─────────────────────────────────────────────────

        private async Task<object> GenerateFormXXVIComplianceData(ComplianceReportRequest request)

        {

            var period = request?.Parameters?.Period ?? DateTime.Now.ToString("yyyy-MM");

            var branch = request?.Parameters?.Branch ?? "";

            var client = request?.Parameters?.Client ?? "";

            var (year, month, periodStart, periodEnd) = ParsePeriod(period);



            // Find employees with low attendance (< 18 days in month)

            var attendanceData = await (from emp in _context.Employees

                                        join att in _context.Attendances on emp.EMP_ID equals att.EmployeeID

                                        where att.Period >= periodStart && att.Period <= periodEnd

                                             && (string.IsNullOrEmpty(branch) || emp.EMP_BRANCH_CODE == branch)

                                             && (string.IsNullOrEmpty(client) || emp.EMP_CLIENT == client || _context.AttendanceDetails.Any(ad => ad.AttendanceID == att.ID && ad.Client == client))

                                        select new

                                        {

                                            emp.EMP_CODE,

                                            emp.EMP_NAME,

                                            emp.EMP_BRANCH_CODE,

                                            att.AllowanceDeduction

                                        }).ToListAsync();



            var complianceIssues = new List<object>();

            int sno = 1;



            foreach (var emp in attendanceData)

            {

                var issues = new List<string>();

                var severity = "Low";



                if (emp.AllowanceDeduction == 0)

                {

                    issues.Add("No salary recorded for this period");

                    severity = "Critical";

                }



                if (issues.Any())

                {

                    complianceIssues.Add(new

                    {

                        sno = sno++,

                        employeeCode = emp.EMP_CODE,

                        employeeName = emp.EMP_NAME,

                        branch = emp.EMP_BRANCH_CODE,

                        client,

                        period,

                        totalDays = DateTime.DaysInMonth(year, month),

                        daysWorked = emp.AllowanceDeduction > 0 ? 26 : 0,

                        attendanceIssue = string.Join("; ", issues),

                        severity,

                        description = string.Join(". ", issues),

                        dueDate = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd"),

                        status = "Pending",

                        assignedTo = "HR Manager",

                        reportedDate = DateTime.Now.ToString("yyyy-MM-dd"),

                        penaltyAmount = severity == "Critical" ? 5000 : 1000

                    });

                }

            }



            var totals = new Dictionary<string, object>

            {

                ["TotalPenalties"] = complianceIssues.Sum(r => { dynamic d = r; return (int)d.penaltyAmount; })

            };

            return new { rows = complianceIssues, totals };

        }



        private async Task<object> GenerateFormXXVIIComplianceData(ComplianceReportRequest request)

            => await GenerateFormXXVIIIComplianceData(request);



        private async Task<object> GenerateFormXXVIIIComplianceData(ComplianceReportRequest request)

        {

            var period = request?.Parameters?.Period ?? DateTime.Now.ToString("yyyy-MM");

            var branch = request?.Parameters?.Branch ?? "";

            var client = request?.Parameters?.Client ?? "";

            var (year, month, periodStart, periodEnd) = ParsePeriod(period);



            var rows = await GetPayrollRows(branch, client, periodStart, periodEnd);



            // PF config to check minimum wage

            decimal minWage = 10000m; // Default minimum wage (Telangana/AP minimum wage for security guard)



            var complianceIssues = new List<object>();

            int sno = 1;



            foreach (dynamic row in rows)

            {

                var violations = new List<string>();

                bool minWageViolation = (decimal)row.basic < minWage;

                bool pfIssue = (decimal)row.pf <= 0 && (decimal)row.basic > 0;

                bool esiIssue = (decimal)row.gross <= 21000 && (decimal)row.esi <= 0;

                bool ptIssue = (decimal)row.gross > 15000 && (decimal)row.pt <= 0;



                if (minWageViolation) violations.Add($"Basic wages ₹{row.basic} below minimum wage ₹{minWage}");

                if (pfIssue) violations.Add("PF not deducted");

                if (esiIssue) violations.Add("ESI not deducted despite gross ≤ ₹21,000");

                if (ptIssue) violations.Add("Professional tax not deducted");



                if (violations.Any())

                {

                    var severity = minWageViolation ? "Critical" : pfIssue ? "High" : "Medium";

                    complianceIssues.Add(new

                    {

                        sno = sno++,

                        employeeCode = (string)row.empId,

                        employeeName = (string)row.name,

                        branch = (string)row.empId,

                        client,

                        period,

                        // Full salary breakdown from Commercial Breakdown

                        basic = (decimal)row.basic,

                        da = (decimal)row.da,

                        hra = (decimal)row.hra,

                        others = (decimal)row.others,

                        bonus = (decimal)row.bonus,

                        ot = (decimal)row.ot,

                        basicDA = (decimal)row.basicDA,

                        gross = (decimal)row.gross,

                        // Commercial Breakdown percentages (for reporting) - Simplified

                        cbHraPercentage = row.cbHraPercentage,

                        cbNHPercentage = row.cbNHPercentage,

                        // Deductions from Commercial Breakdown

                        pf = (decimal)row.pf,

                        esi = (decimal)row.esi,

                        pt = (decimal)row.pt,

                        advanceDed = (decimal)row.advanceDed,

                        totalDeductions = (decimal)row.totalDeductions,

                        net = (decimal)row.net,

                        daysWorked = (int)row.daysWorked,

                        totalDays = (int)row.totalDays,

                        // Compliance issues

                        minimumWageViolation = minWageViolation,

                        pfDeductionIssue = pfIssue,

                        esiDeductionIssue = esiIssue,

                        ptDeductionIssue = ptIssue,

                        salaryDelay = false,

                        severity,

                        description = string.Join(". ", violations),

                        dueDate = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd"),

                        status = "Pending",

                        assignedTo = "Payroll Manager",

                        reportedDate = DateTime.Now.ToString("yyyy-MM-dd"),

                        penaltyAmount = minWageViolation ? 3000 : 1000

                    });

                }

            }



            var totalIssues = complianceIssues.Count;

            var totals = new Dictionary<string, object>

            {

                ["TotalPenalties"] = complianceIssues.Sum(r => { dynamic d = r; return (int)d.penaltyAmount; }),

                ["TotalIssues"] = totalIssues,

                ["CriticalIssues"] = complianceIssues.Count(r => { dynamic d = r; return (string)d.severity == "Critical"; }),

                ["HighIssues"] = complianceIssues.Count(r => { dynamic d = r; return (string)d.severity == "High"; }),

                ["MediumIssues"] = complianceIssues.Count(r => { dynamic d = r; return (string)d.severity == "Medium"; }),

                ["LowIssues"] = complianceIssues.Count(r => { dynamic d = r; return (string)d.severity == "Low"; }),

                // Salary totals for compliance report footer

                ["TotalBasic"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.basic; }),

                ["TotalDA"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.da; }),

                ["TotalHRA"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.hra; }),

                ["TotalOthers"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.others; }),

                ["TotalBonus"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.bonus; }),

                ["TotalOT"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.ot; }),

                ["TotalGross"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.gross; }),

                ["TotalPF"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.pf; }),

                ["TotalESI"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.esi; }),

                ["TotalPT"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.pt; }),

                ["TotalNet"] = complianceIssues.Sum(r => { dynamic d = r; return (decimal)d.net; })

            };

            return new { rows = complianceIssues, totals };

        }



        // ─── Helper: Extract branch/client/period from JsonElement (supports nested Parameters or root-level) ──

        private static void ExtractParams(System.Text.Json.JsonElement body, out string period, out string branch, out string client)

        {

            period = ""; branch = ""; client = "";



            // Try nested Parameters first: { Parameters: { Branch, Client, Period } }

            if (body.TryGetProperty("Parameters", out var p) && p.ValueKind == System.Text.Json.JsonValueKind.Object)

            {

                period = p.TryGetProperty("period", out var pv) ? pv.GetString() ?? "" : (p.TryGetProperty("Period", out var pv2) ? pv2.GetString() ?? "" : "");

                branch = p.TryGetProperty("branch", out var bv) ? bv.GetString() ?? "" : (p.TryGetProperty("Branch", out var bv2) ? bv2.GetString() ?? "" : "");

                client = p.TryGetProperty("client", out var cv) ? cv.GetString() ?? "" : (p.TryGetProperty("Client", out var cv2) ? cv2.GetString() ?? "" : "");

            }



            // Fall back to root-level flat fields

            if (string.IsNullOrEmpty(period))

                period = body.TryGetProperty("period", out var fv) ? fv.GetString() ?? "" : (body.TryGetProperty("Period", out var fv2) ? fv2.GetString() ?? "" : "");

            if (string.IsNullOrEmpty(branch))

                branch = body.TryGetProperty("branch", out var fb) ? fb.GetString() ?? "" : (body.TryGetProperty("Branch", out var fb2) ? fb2.GetString() ?? "" : "");

            if (string.IsNullOrEmpty(client))

                client = body.TryGetProperty("client", out var fc) ? fc.GetString() ?? "" : (body.TryGetProperty("Client", out var fc2) ? fc2.GetString() ?? "" : "");

        }



        // ─── Metadata builder ─────────────────────────────────────────────────────────

        private Dictionary<string, object> BuildMetadata(ComplianceReportRequest? request, string branch, string client, string period, int year, int month)

        {

            // Try to load real branch/client info from DB

            var branchInfo = _context.BranchMasters.FirstOrDefault(b => b.Code == branch);

            var clientInfo = _context.ClientMasters.FirstOrDefault(c => c.Code == client);



            string companyName = branchInfo?.Name ?? request?.Parameters?.ContractorName ?? "Freight Watch G Security Services India Pvt. Ltd.";

            string companyAddress = branchInfo?.Address1 != null

                ? $"{branchInfo.Address1}, {branchInfo.Address2}, {branchInfo.State}"

                : "No.397 (old no.281), Anna Salai, Precision Plaza, 1st Floor, Teynampet, Chennai - 600018";

            string companyPhone = branchInfo?.Phone ?? "+91 4440050684";

            string companyEmail = branchInfo?.Email ?? "info@fwgindia.com";

            string companyCIN = "U74920TN2005PTC057775";

            string companyGST = "";



            string monthName = new DateTime(year, month, 1).ToString("MMMM");

            string clientName = clientInfo?.Name ?? client;

            string worksite = clientInfo?.Address1 != null ? $"{clientInfo.Address1}, {clientInfo.State}" : (request?.Parameters?.WorkSite ?? "");



            return new Dictionary<string, object>

            {

                ["Branch"] = branch,

                ["BranchName"] = branchInfo?.Name ?? branch,

                ["Client"] = client,

                ["ClientName"] = clientName,

                ["Period"] = $"{monthName} {year}",

                ["Year"] = year.ToString(),

                ["Month"] = month.ToString(),

                ["MonthName"] = monthName,

                ["DaysInMonth"] = DateTime.DaysInMonth(year, month),

                ["CompanyName"] = companyName,

                ["CompanyAddress"] = companyAddress,

                ["CompanyPhone"] = companyPhone,

                ["CompanyEmail"] = companyEmail,

                ["CompanyCIN"] = companyCIN,

                ["CompanyGST"] = companyGST,

                ["PrincipalEmployer"] = request?.Parameters?.PrincipalEmployer ?? clientName,

                ["ContractorName"] = request?.Parameters?.ContractorName ?? companyName,

                ["WorkSite"] = request?.Parameters?.WorkSite ?? worksite,

                ["Establishment"] = companyName,

                ["EstablishmentNameAndAddress"] = $"{companyName}, {companyAddress}",

                ["EmployerContractorNameAndAddress"] = $"{companyName}, {companyAddress}",

                ["WagePeriod"] = period,

                ["DateRange"] = period,

                ["GeneratedDate"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm"),

                ["UnitName"] = request?.Parameters?.UnitName ?? branchInfo?.Name ?? ""

            };

        }



        private string GetMonthName(string period)

        {

            if (string.IsNullOrEmpty(period) || !period.Contains("-")) return "January";

            if (int.TryParse(period.Split('-')[1], out int month))

            {

                var months = new[] { "January", "February", "March", "April", "May", "June",

                                     "July", "August", "September", "October", "November", "December" };

                return month >= 1 && month <= 12 ? months[month - 1] : "January";

            }

            return "January";

        }

        #region GST Statement Report

        [HttpPost("GetGSTStatement")]
        public async Task<IActionResult> GetGSTStatement([FromBody] System.Text.Json.JsonElement body)
        {
            try
            {
                string fromDate = "", toDate = "", branch = "", client = "";

                if (body.TryGetProperty("Parameters", out var prms))
                {
                    if (prms.TryGetProperty("fromDate", out var fd)) fromDate = fd.GetString() ?? "";
                    if (prms.TryGetProperty("toDate", out var td)) toDate = td.GetString() ?? "";
                    if (prms.TryGetProperty("branch", out var br)) branch = br.GetString() ?? "";
                    if (prms.TryGetProperty("client", out var cl)) client = cl.GetString() ?? "";
                }

                if (string.IsNullOrEmpty(fromDate)) fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
                if (string.IsNullOrEmpty(toDate)) toDate = DateTime.Now.ToString("yyyy-MM-dd");

                if (!DateTime.TryParse(fromDate, out var dtFrom)) dtFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                if (!DateTime.TryParse(toDate, out var dtTo)) dtTo = DateTime.Now;
                dtTo = dtTo.Date.AddDays(1).AddSeconds(-1);

                var query = _context.Set<ClientInvoice>()
                    .Where(i => i.IsDeleted == "N" && i.InvoiceDate >= dtFrom && i.InvoiceDate <= dtTo);

                if (!string.IsNullOrEmpty(branch))
                    query = query.Where(i => i.Branch == branch);
                if (!string.IsNullOrEmpty(client))
                    query = query.Where(i => i.Client == client);

                var invoices = await query
                    .OrderBy(i => i.InvoiceDate)
                    .ThenBy(i => i.InvoiceNo)
                    .ToListAsync();

                var clientCodes = invoices.Select(i => i.Client).Distinct().ToList();
                var clientMasters = await _context.Set<ClientMaster>()
                    .Where(c => clientCodes.Contains(c.Code))
                    .ToDictionaryAsync(c => c.Code, c => c);

                var branchCodes = invoices.Select(i => i.Branch).Distinct().ToList();
                var branchMasters = await _context.Set<BranchMaster>()
                    .Where(b => branchCodes.Contains(b.Code))
                    .ToDictionaryAsync(b => b.Code, b => b);

                var rows = new List<object>();
                var rateGroups = new Dictionary<decimal, decimal>();
                decimal grandTaxable = 0, grandCGST = 0, grandSGST = 0, grandIGST = 0, grandTotal = 0;

                foreach (var inv in invoices)
                {
                    clientMasters.TryGetValue(inv.Client, out var clientInfo);
                    branchMasters.TryGetValue(inv.Branch, out var branchInfo);

                    var branchState = branchInfo?.State?.Trim() ?? "";
                    var clientState = (clientInfo?.IndianState ?? clientInfo?.State ?? clientInfo?.BillingState ?? "").Trim();
                    var isIntraState = !string.IsNullOrEmpty(branchState) &&
                                       !string.IsNullOrEmpty(clientState) &&
                                       branchState.Equals(clientState, StringComparison.OrdinalIgnoreCase);

                    var clientGSTIN = clientInfo?.GSTIN ?? "";
                    var taxableAmount = inv.ServiceCharges - inv.Discount;
                    var taxAmount = inv.TaxAmount;

                    decimal gstRate = 0;
                    if (taxableAmount > 0 && taxAmount > 0)
                        gstRate = Math.Round(taxAmount / taxableAmount * 100, 2);

                    decimal cgst = 0, sgst = 0, igst = 0;
                    string gstRateDisplay;
                    if (isIntraState)
                    {
                        cgst = taxAmount / 2;
                        sgst = taxAmount / 2;
                        var halfRate = gstRate / 2;
                        gstRateDisplay = halfRate % 1 == 0
                            ? $"{(int)halfRate}% + {(int)halfRate}%"
                            : $"{halfRate:F1}% + {halfRate:F1}%";
                    }
                    else
                    {
                        igst = taxAmount;
                        gstRateDisplay = gstRate % 1 == 0
                            ? $"{(int)gstRate}%"
                            : $"{gstRate:F2}%";
                    }

                    grandTaxable += taxableAmount;
                    grandCGST += cgst;
                    grandSGST += sgst;
                    grandIGST += igst;
                    grandTotal += inv.ServiceCharges - inv.Discount + inv.TaxAmount;

                    if (!rateGroups.ContainsKey(gstRate))
                        rateGroups[gstRate] = 0;
                    rateGroups[gstRate] += taxableAmount;

                    rows.Add(new
                    {
                        sno = rows.Count + 1,
                        invoiceNo = inv.InvoiceNo,
                        invoiceDate = inv.InvoiceDate.ToString("dd-MM-yyyy"),
                        clientName = clientInfo?.Name ?? inv.Client,
                        clientGSTIN = clientGSTIN,
                        clientState = clientInfo?.IndianState ?? clientInfo?.State ?? "",
                        branch = inv.Branch,
                        subject = inv.Subject,
                        taxableAmount = Math.Round(taxableAmount, 2),
                        gstRate,
                        gstRateDisplay,
                        cgst = Math.Round(cgst, 2),
                        sgst = Math.Round(sgst, 2),
                        igst = Math.Round(igst, 2),
                        totalGST = Math.Round(taxAmount, 2),
                        totalAmount = Math.Round(taxableAmount + taxAmount, 2),
                        isIntraState
                    });
                }

                var rateSummary = rateGroups.Select(r => new
                {
                    gstRate = r.Key,
                    taxableAmount = Math.Round(r.Value, 2),
                    gstAmount = Math.Round(r.Key > 0 ? r.Value * r.Key / 100 : 0, 2)
                }).OrderBy(r => r.gstRate).ToList();

                var firstBranchGSTIN = branchMasters.Values.FirstOrDefault()?.GSTIN ?? _configuration.GetValue<string>("CompanyGSTIN") ?? "";

                var totals = new Dictionary<string, object>
                {
                    ["FromDate"] = dtFrom.ToString("dd-MM-yyyy"),
                    ["ToDate"] = dtTo.ToString("dd-MM-yyyy"),
                    ["Branch"] = branch,
                    ["Client"] = client,
                    ["BranchName"] = branch,
                    ["ClientName"] = client,
                    ["CompanyGSTIN"] = firstBranchGSTIN,
                    ["ReportTitle"] = $"GST TAX INVOICE SUMMARY REPORT - {dtFrom:dd-MMM-yyyy} TO {dtTo:dd-MMM-yyyy}",
                    ["Period"] = $"{dtFrom:dd-MMM-yyyy} to {dtTo:dd-MMM-yyyy}",
                    ["GeneratedDate"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm"),
                    ["TotalInvoices"] = rows.Count,
                    ["GrandTaxable"] = Math.Round(grandTaxable, 2),
                    ["GrandCGST"] = Math.Round(grandCGST, 2),
                    ["GrandSGST"] = Math.Round(grandSGST, 2),
                    ["GrandIGST"] = Math.Round(grandIGST, 2),
                    ["GrandTotalGST"] = Math.Round(grandCGST + grandSGST + grandIGST, 2),
                    ["GrandTotal"] = Math.Round(grandTotal, 2),
                    ["RateSummary"] = rateSummary
                };

                return Ok(new { rows, totals });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating GST statement");
                return StatusCode(500, new { message = "Error generating GST statement", error = ex.Message });
            }
        }

        #endregion

    }



    public class ComplianceReportRequest

    {

        public string ReportName { get; set; } = "";

        public ComplianceReportParameters? Parameters { get; set; }

    }



    public class ComplianceReportParameters

    {

        public string? Branch { get; set; }

        public string? Client { get; set; }

        public string? Period { get; set; }

        public string? UnitName { get; set; }

        public string? PrincipalEmployer { get; set; }

        public string? ContractorName { get; set; }

        public string? WorkSite { get; set; }

        public bool ShowIndividualSlips { get; set; }

    }

}

