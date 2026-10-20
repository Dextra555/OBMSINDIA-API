-- ============================================================
-- EMP_ID = 319 - PaySlip table-ல் store ஆகி இருக்கும் values
-- PF, EPFDeductionAmount, PFWage, EarnedSalary எல்லாம் என்ன?
-- ============================================================
-- @Period மாற்றவும்: 'YYYY-MM' (உதா: '2026-09')
-- ============================================================

DECLARE @EmpID      INT          = 319;
DECLARE @Period     VARCHAR(7)   = '2026-09';   -- << மாதம் மாற்றவும்
DECLARE @PeriodEnd  DATE         = EOMONTH(CAST(@Period + '-01' AS DATE));

PRINT '=== PaySlip - All PF & Salary related columns for EMP_ID 319 ===';
SELECT 
    ps.ID,
    ps.EmployeeID,
    ps.Period,
    -- ── Salary basics ──
    ps.BasicSalary,
    ps.BasicSalaryRate,
    ps.BasicSalaryDays,
    ps.PerDaySalary,
    ps.EarnedSalary,
    ps.GrossPay,
    -- ── PF columns (KEY) ──
    ps.EPFDeductionAmount    AS EPFDeductionAmount_OLD,
    ps.PF                    AS PF_NEW,
    ps.PFWage                AS PFWage,
    ps.EPFEmployerContribution AS EPFEmployerContribution,
    -- ── ESI columns ──
    ps.SOCSODeductionAmount  AS SOCSODeductionAmount_OLD,
    ps.ESI                   AS ESI_NEW,
    ps.ESIWage               AS ESIWage,
    ps.SOCSOEmployerContribution AS SOCSOEmployerContribution,
    -- ── PTax ──
    ps.PTax,
    -- ── TDS ──
    ps.TDS,
    -- ── LWF ──
    ps.LWF,
    -- ── Deductions ──
    ps.DailyAdvanceRecovery,
    ps.MonthlyAdvanceRecovery,
    ps.SpecialAdvanceRecovery,
    ps.UniformIssueRecovery,
    ps.LoanRecovery,
    ps.MiscDeduction,
    ps.TotalDeduction,
    ps.NetPay,
    -- ── Other ──
    ps.AttendanceAllowance,
    ps.SpecialAllowance,
    ps.Bonus,
    ps.OverTimeSalary,
    ps.LastUpdate,
    ps.LastUpdatedBy
FROM PaySlips ps
WHERE ps.EmployeeID = @EmpID
  AND ps.Period = @PeriodEnd;

-- ============================================================
PRINT '';
PRINT '=== Compare: PaySlip stored PF vs PF Report recalculated PF ===';

SELECT 
    e.EMP_CODE,
    e.EMP_NAME,
    -- PaySlip stored values
    ps.PF                AS PaySlip_PF,
    ps.PFWage            AS PaySlip_PFWage,
    ps.EPFDeductionAmount AS PaySlip_EPF_OLD,
    ps.EPFEmployerContribution AS PaySlip_EmployerPF,
    ps.EarnedSalary      AS PaySlip_EarnedSalary,
    ps.PerDaySalary      AS PaySlip_PerDaySalary,
    ps.BasicSalaryDays   AS PaySlip_BasicSalaryDays,
    -- Employee CB values (what PF Report uses)
    e.CB_Basic           AS CB_Basic,
    e.CB_DA              AS CB_DA,
    e.CB_OtherAllowances AS CB_OtherAllowances,
    -- PF Report formula: (CB_Basic + CB_DA) / daysInMonth × daysWorked
    -- PaySlip formula:   (CB_Basic + CB_DA + CB_OtherAllowances) / daysInMonth × daysWorked
    -- NOTE: PaySlip PF includes CB_OtherAllowances! PF Report does NOT!
    (ISNULL(e.CB_Basic,0) + ISNULL(e.CB_DA,0))                          AS Report_BasicPlusDA,
    (ISNULL(e.CB_Basic,0) + ISNULL(e.CB_DA,0) + ISNULL(e.CB_OtherAllowances,0)) AS PaySlip_BasicPlusDA_Others
FROM PaySlips ps
JOIN Employees e ON e.EMP_ID = ps.EmployeeID
WHERE ps.EmployeeID = @EmpID
  AND ps.Period = @PeriodEnd;
