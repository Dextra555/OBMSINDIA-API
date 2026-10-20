-- EMP_ID = 319 - PaySlip table-ல் store ஆகி இருக்கும் values நேரடியா
-- @Period மாற்றவும்

DECLARE @EmpID     INT = 319;
DECLARE @PeriodEnd DATE = EOMONTH('2026-09-01');  -- << மாதம் மாற்றவும்

SELECT 
    ps.EmployeeID,
    e.EMP_CODE,
    e.EMP_NAME,
    ps.Period,
    -- Salary
    ps.BasicSalary,
    ps.BasicSalaryDays,
    ps.BasicSalaryRate,
    ps.PerDaySalary,
    ps.EarnedSalary,
    ps.GrossPay,
    -- PF stored values
    ps.PF,
    ps.PFWage,
    ps.EPFDeductionAmount,
    ps.EPFEmployerContribution,
    -- ESI
    ps.ESI,
    ps.ESIWage,
    ps.SOCSODeductionAmount,
    -- PTax, TDS, LWF
    ps.PTax,
    ps.TDS,
    ps.LWF,
    -- Deductions
    ps.DailyAdvanceRecovery,
    ps.MonthlyAdvanceRecovery,
    ps.LoanRecovery,
    ps.TotalDeduction,
    ps.NetPay,
    ps.LastUpdate,
    ps.LastUpdatedBy
FROM PaySlips ps
JOIN Employees e ON e.EMP_ID = ps.EmployeeID
WHERE ps.EmployeeID = @EmpID
  AND ps.Period = @PeriodEnd;
