using Microsoft.AspNetCore.Mvc;
using OBMS.WebAPI.Models.DTO;
using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;

namespace OBMS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly OBMSDbContext _context;
        private readonly IConfiguration _configuration;

        public ReportController(OBMSDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("GetReportData")]
        public async Task<ActionResult<object>> GetReportData([FromBody] ReportRequest request)
        {
            try
            {
                switch (request.ReportName.ToLower())
                {
                    case "indianpayslip":
                        return await GetIndianPayslipData(request.Parameters);
                    case "form16":
                        return await GetForm16Data(request.Parameters);
                    case "pfchallan":
                        return await GetPFChallanData(request.Parameters);
                    case "esichallan":
                        return await GetESIChallanData(request.Parameters);
                    case "employeelistindian":
                        return await GetEmployeeListIndianData(request.Parameters);
                    case "formxvii":
                        return await GetFormXVIIData(request.Parameters);
                    case "formxxvi":
                        return await GetFormXXVIData(request.Parameters);
                    case "formxxvii":
                        return await GetFormXXVIIData(request.Parameters);
                    case "formxxviii":
                        return await GetFormXXVIIIData(request.Parameters);
                    case "formxxix":
                        return await GetFormXXIXData(request.Parameters);
                    case "overtime":
                        return await GetOvertimeData(request.Parameters);
                    default:
                        return BadRequest("Unknown report name");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error generating report data", Details = ex.Message });
            }
        }

        private async Task<ActionResult<object>> GetIndianPayslipData(Dictionary<string, object> parameters)
        {
            var employeeId = parameters.ContainsKey("employeeId") ? Convert.ToInt32(parameters["employeeId"]) : 0;
            var month = parameters.ContainsKey("month") ? parameters["month"].ToString() : "";
            var year = parameters.ContainsKey("year") ? Convert.ToInt32(parameters["year"]) : 0;

            if (employeeId == 0 || string.IsNullOrEmpty(month) || year == 0)
            {
                return BadRequest("Employee ID, month, and year are required");
            }

            // Get employee details
            var employee = await _context.Employees
                .Where(e => e.EMP_ID == employeeId)
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            // Get employment details
            var employment = await _context.EmploymentDetails
                .Where(e => e.EMPPAY_CODE == employee.EMP_CODE)
                .FirstOrDefaultAsync();

            // Mock salary calculation - replace with actual payroll logic
            var basicSalary = employment?.EMPPAY_BASIC_RATE ?? 25000;
            var hra = basicSalary * 0.4; // 40% of basic
            var specialAllowance = 5000;
            var grossSalary = basicSalary + hra + specialAllowance;

            // PF Calculation (12% of basic, capped at ₹15,000)
            var pfBasic = Math.Min(basicSalary, 15000);
            var employeePF = pfBasic * 0.12;
            var employerPF = pfBasic * 0.12;

            // ESI Calculation (0.75% employee, 3.25% employer on gross up to ₹21,000)
            var esiGross = Math.Min(grossSalary, 21000);
            var employeeESI = esiGross * 0.0075;
            var employerESI = esiGross * 0.0325;

            // Professional Tax (varies by state)
            var professionalTax = GetProfessionalTaxByState(employee.IndianState ?? "Maharashtra");

            // TDS Calculation (simplified)
            var tdsDeducted = CalculateTDS(grossSalary - employeePF);

            var totalDeductions = employeePF + employeeESI + professionalTax + tdsDeducted;
            var netSalary = grossSalary - totalDeductions;

            var reportData = new
            {
                headers = new[]
                {
                    new { name = "component", title = "Component" },
                    new { name = "amount", title = "Amount (₹)" }
                },
                details = new[]
                {
                    new { component = "Basic Salary", amount = basicSalary.ToString("F2") },
                    new { component = "House Rent Allowance (HRA)", amount = hra.ToString("F2") },
                    new { component = "Special Allowance", amount = specialAllowance.ToString("F2") }
                },
                summary = new
                {
                    grossSalary = grossSalary.ToString("F2"),
                    pfDeduction = employeePF.ToString("F2"),
                    esiDeduction = employeeESI.ToString("F2"),
                    professionalTax = professionalTax.ToString("F2"),
                    tdsDeduction = tdsDeducted.ToString("F2"),
                    totalDeductions = totalDeductions.ToString("F2"),
                    netSalary = netSalary.ToString("F2"),
                    // Employee details for template
                    EMP_CODE = employee.EMP_CODE,
                    EMP_NAME = employee.EMP_NAME,
                    AadhaarNumber = employee.AadhaarNumber,
                    PANNumber = employee.PANNumber,
                    BankAccountNumber = employee.BankAccountNumber,
                    BankIFSC = employee.BankIFSC,
                    month = month,
                    year = year.ToString(),
                    companyName = "OBMS INDIA PVT. LTD."
                },
                parameters = parameters
            };

            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetForm16Data(Dictionary<string, object> parameters)
        {
            var employeeId = parameters.ContainsKey("employeeId") ? Convert.ToInt32(parameters["employeeId"]) : 0;
            var financialYear = parameters.ContainsKey("financialYear") ? parameters["financialYear"].ToString() : "";

            var employee = await _context.Employees
                .Where(e => e.EMP_ID == employeeId)
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            // Mock Form 16 data - replace with actual calculations
            var grossAnnualSalary = 600000; // Mock annual salary
            var section80CDeductions = 150000; // Standard deductions
            var taxableIncome = Math.Max(0, grossAnnualSalary - section80CDeductions);
            var taxPayable = CalculateAnnualTax(taxableIncome);

            var reportData = new
            {
                headers = new[]
                {
                    new { name = "component", title = "Component" },
                    new { name = "amount", title = "Amount (₹)" }
                },
                details = new[]
                {
                    new { component = "Gross Annual Salary", amount = grossAnnualSalary.ToString("F2") },
                    new { component = "Section 80C Deductions", amount = section80CDeductions.ToString("F2") },
                    new { component = "Taxable Income", amount = taxableIncome.ToString("F2") },
                    new { component = "Tax Payable", amount = taxPayable.ToString("F2") }
                },
                summary = new
                {
                    grossSalary = grossAnnualSalary.ToString("F2"),
                    section80C = section80CDeductions.ToString("F2"),
                    taxableIncome = taxableIncome.ToString("F2"),
                    taxPayable = taxPayable.ToString("F2"),
                    EMP_NAME = employee.EMP_NAME,
                    PANNumber = employee.PANNumber,
                    AadhaarNumber = employee.AadhaarNumber,
                    financialYear = financialYear
                },
                parameters = parameters
            };

            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetPFChallanData(Dictionary<string, object> parameters)
        {
            var month = parameters.ContainsKey("month") ? parameters["month"].ToString() : "";
            var year = parameters.ContainsKey("year") ? Convert.ToInt32(parameters["year"]) : 0;
            var establishmentId = parameters.ContainsKey("establishmentId") ? parameters["establishmentId"].ToString() : "";

            // Mock PF challan data - replace with actual data
            var reportData = new
            {
                headers = new[]
                {
                    new { name = "employeeCode", title = "Employee Code" },
                    new { name = "employeeName", title = "Employee Name" },
                    new { name = "basicSalary", title = "Basic Salary" },
                    new { name = "employeeContribution", title = "Employee PF" },
                    new { name = "employerContribution", title = "Employer PF" }
                },
                details = new[]
                {
                    new { employeeCode = "EMP001", employeeName = "John Doe", basicSalary = "25000.00", employeeContribution = "3000.00", employerContribution = "3000.00" },
                    new { employeeCode = "EMP002", employeeName = "Jane Smith", basicSalary = "30000.00", employeeContribution = "3600.00", employerContribution = "3600.00" }
                },
                summary = new
                {
                    month = month,
                    year = year.ToString(),
                    establishmentId = establishmentId,
                    totalEmployeeContribution = "6600.00",
                    totalEmployerContribution = "6600.00",
                    totalContribution = "13200.00"
                },
                parameters = parameters
            };

            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetESIChallanData(Dictionary<string, object> parameters)
        {
            var month = parameters.ContainsKey("month") ? parameters["month"].ToString() : "";
            var year = parameters.ContainsKey("year") ? Convert.ToInt32(parameters["year"]) : 0;
            var establishmentId = parameters.ContainsKey("establishmentId") ? parameters["establishmentId"].ToString() : "";

            // Mock ESI challan data - replace with actual data
            var reportData = new
            {
                headers = new[]
                {
                    new { name = "employeeCode", title = "Employee Code" },
                    new { name = "employeeName", title = "Employee Name" },
                    new { name = "grossSalary", title = "Gross Salary" },
                    new { name = "employeeContribution", title = "Employee ESI" },
                    new { name = "employerContribution", title = "Employer ESI" }
                },
                details = new[]
                {
                    new { employeeCode = "EMP001", employeeName = "John Doe", grossSalary = "35000.00", employeeContribution = "262.50", employerContribution = "1137.50" },
                    new { employeeCode = "EMP002", employeeName = "Jane Smith", grossSalary = "40000.00", employeeContribution = "300.00", employerContribution = "1300.00" }
                },
                summary = new
                {
                    month = month,
                    year = year.ToString(),
                    establishmentId = establishmentId,
                    totalEmployeeContribution = "562.50",
                    totalEmployerContribution = "2437.50",
                    totalContribution = "3000.00"
                },
                parameters = parameters
            };

            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetEmployeeListIndianData(Dictionary<string, object> parameters)
        {
            var branchCode = parameters.ContainsKey("branchCode") ? parameters["branchCode"].ToString() : "";
            var department = parameters.ContainsKey("department") ? parameters["department"].ToString() : "";
            var status = parameters.ContainsKey("status") ? parameters["status"].ToString() : "Active";

            var query = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(branchCode))
            {
                query = query.Where(e => e.EMP_BRANCH_CODE == branchCode);
            }

            // Get employees with Indian compliance fields
            var employees = await query
                .Select(e => new
                {
                    e.EMP_CODE,
                    e.EMP_NAME,
                    e.AadhaarNumber,
                    e.PANNumber,
                    e.BankAccountNumber,
                    e.BankIFSC,
                    e.IndianState,
                    e.EMP_MOBILEPHONE
                })
                .ToListAsync();

            var reportData = new
            {
                headers = new[]
                {
                    new { name = "EMP_CODE", title = "Code" },
                    new { name = "EMP_NAME", title = "Name" },
                    new { name = "AadhaarNumber", title = "Aadhaar" },
                    new { name = "PANNumber", title = "PAN" },
                    new { name = "BankAccountNumber", title = "Bank Account" },
                    new { name = "BankIFSC", title = "IFSC" },
                    new { name = "IndianState", title = "State" },
                    new { name = "EMP_MOBILEPHONE", title = "Mobile" }
                },
                details = employees,
                summary = new
                {
                    totalEmployees = employees.Count,
                    branchCode = branchCode,
                    status = status,
                    generatedDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")
                },
                parameters = parameters
            };

            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetFormXVIIData(Dictionary<string, object> parameters)
        {
            var branch = parameters.ContainsKey("branch") ? parameters["branch"].ToString() : "";
            var client = parameters.ContainsKey("client") ? parameters["client"].ToString() : "";
            var period = parameters.ContainsKey("period") ? parameters["period"].ToString() : DateTime.Now.ToString("yyyy-MM");
            var unitName = parameters.ContainsKey("unitName") ? parameters["unitName"].ToString() : "N/A";

            var rows = new[]
            {
                new { sno=1, name="Surendar Kumar", designation="Elec Supervisor", basic=10399, da=7313, others=11408, hra=0, leaveWages=0, nh=0, advBonus=1475, actualDays=31, workingDays=31, earnedBasic=10399, earnedDA=7313, earnedOthers=11408, earnedHRA=0, earnedLeaveWages=0, earnedNH=0, earnedAdvBonus=1475, ot=0, otAmount=0, pf=1800, esiWages=0, esi=0, pt=208, uniform=0, lwf=0, mobile=0, arrear=0, net=28587 },
                new { sno=2, name="Vignesh K", designation="MST", basic=8602, da=7313, others=8691, hra=0, leaveWages=0, nh=0, advBonus=1326, actualDays=31, workingDays=31, earnedBasic=8602, earnedDA=7313, earnedOthers=8691, earnedHRA=0, earnedLeaveWages=0, earnedNH=0, earnedAdvBonus=1326, ot=0, otAmount=0, pf=1800, esiWages=0, esi=0, pt=208, uniform=0, lwf=0, mobile=0, arrear=0, net=23924 },
                new { sno=3, name="Sudhakar V", designation="MST", basic=8602, da=7313, others=8691, hra=0, leaveWages=0, nh=0, advBonus=1326, actualDays=31, workingDays=31, earnedBasic=8602, earnedDA=7313, earnedOthers=8691, earnedHRA=0, earnedLeaveWages=0, earnedNH=0, earnedAdvBonus=1326, ot=0, otAmount=0, pf=1800, esiWages=0, esi=0, pt=208, uniform=0, lwf=0, mobile=0, arrear=0, net=23924 }
            };

            var reportData = new
            {
                title = "Form XVII - Register of Wages",
                branch = branch,
                client = client,
                period = period,
                unitName = unitName,
                rows = rows,
                totals = new
                {
                    totalBasic = 27603,
                    totalDA = 21939,
                    totalOthers = 28790,
                    totalHRA = 0,
                    totalAdvBonus = 4127,
                    totalGross = 80459,
                    totalPF = 5400,
                    totalPT = 624,
                    totalDeductions = 6024,
                    totalNet = 74435
                }
            };

            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetFormXXVIData(Dictionary<string, object> parameters)
        {
            // Provide dynamic sample data (or DB query) for Form XXVI
            var branch = parameters.ContainsKey("branch") ? parameters["branch"].ToString() : "";
            var client = parameters.ContainsKey("client") ? parameters["client"].ToString() : "";
            var period = parameters.ContainsKey("period") ? parameters["period"].ToString() : DateTime.Now.ToString("yyyy-MM");

            var reportData = new
            {
                title = "Form XXVI - Muster Roll",
                branch = branch,
                client = client,
                period = period,
                rows = new[]
                {
                    new { sno=1, name="Surendar Kumar", empId="10191802", sex="M", uan="10191802", designation="Guard", day1="P", day2="P" }
                },
                totals = new { totalDays=28 }
            };
            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetFormXXVIIData(Dictionary<string, object> parameters)
        {
            var branch = parameters.ContainsKey("branch") ? parameters["branch"].ToString() : "";
            var client = parameters.ContainsKey("client") ? parameters["client"].ToString() : "";
            var period = parameters.ContainsKey("period") ? parameters["period"].ToString() : DateTime.Now.ToString("yyyy-MM");

            var reportData = new
            {
                title = "Form XXVII - Register of Wages",
                branch = branch,
                client = client,
                period = period,
                rows = new[]
                {
                    new { sno=1, name="Surendar Kumar", empId="101918", sex="M", basic=9031, da=10192, hra=3615, others=0, bonus=1601, pf=1800, pt=208, net=22431 }
                },
                totals = new { totalNet=104115 }
            };
            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetFormXXVIIIData(Dictionary<string, object> parameters)
        {
            var period = parameters.ContainsKey("period") ? parameters["period"].ToString() : DateTime.Now.ToString("yyyy-MM");

            var reportData = new
            {
                title = "Form XXVIII - Wage Slips",
                period = period,
                employee = new { name="Surendar Kumar", code="FWGR001", uan="101918025472", designation="Security Guard" },
                components = new[]
                {
                    new { label="Basic Salary", amount="9031" },
                    new { label="Dearness Allowance", amount="10192" },
                },
                deductions = new[]
                {
                    new { label="EPF 12%", amount="1800" },
                    new { label="PT", amount="208" }
                },
                totalEarnings="24439",
                totalDeductions="2008",
                netAmount="22431"
            };
            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetFormXXIXData(Dictionary<string, object> parameters)
        {
            var reportData = new
            {
                title = "Form XXIX - Advances, Deductions & Fines",
                period = parameters.ContainsKey("period") ? parameters["period"].ToString() : DateTime.Now.ToString("yyyy-MM"),
                rows = new[]
                {
                    new { sno=1, name="Surendar Kumar", empNo="FWGR001", amount=0, datePaid="NIL", installment="NIL", remarks="NIL" }
                },
                summary = "Part B: NIL; Part C: NIL"
            };
            return Ok(reportData);
        }

        private async Task<ActionResult<object>> GetOvertimeData(Dictionary<string, object> parameters)
        {
            var reportData = new
            {
                title = "Overtime Register",
                period = parameters.ContainsKey("period") ? parameters["period"].ToString() : DateTime.Now.ToString("yyyy-MM"),
                rows = new[]
                {
                    new { sno=1, name="Surendar Kumar", designation="Guard", otHours=0, otRate=0, total=0 }
                },
            };
            return Ok(reportData);
        }

        // Helper methods
        private double GetProfessionalTaxByState(string state)
        {
            var ptRates = new Dictionary<string, double>
            {
                { "Maharashtra", 200 },
                { "Karnataka", 200 },
                { "Tamil Nadu", 150 },
                { "Delhi", 0 },
                { "Gujarat", 200 },
                { "West Bengal", 130 },
                { "Uttar Pradesh", 0 },
                { "Rajasthan", 100 },
                { "Madhya Pradesh", 100 },
                { "Punjab", 200 },
                { "Haryana", 200 }
            };
            return ptRates.ContainsKey(state) ? ptRates[state] : 100;
        }

        private double CalculateTDS(double monthlyTaxableIncome)
        {
            var annualTaxableIncome = monthlyTaxableIncome * 12;
            var tax = CalculateAnnualTax(annualTaxableIncome);
            return Math.Round(tax / 12, 2); // Monthly TDS
        }

        private double CalculateAnnualTax(double annualTaxableIncome)
        {
            // Simplified tax calculation for FY 2023-24
            if (annualTaxableIncome <= 250000) return 0;
            if (annualTaxableIncome <= 500000) return (annualTaxableIncome - 250000) * 0.05;
            if (annualTaxableIncome <= 1000000) return 12500 + (annualTaxableIncome - 500000) * 0.2;
            return 112500 + (annualTaxableIncome - 1000000) * 0.3;
        }
    }

    public class ReportRequest
    {
        public string ReportName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
