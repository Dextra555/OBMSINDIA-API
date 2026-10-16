-- ============================================================
-- FWGG000368 - PF Statement Report இல் வரவில்லை - Debug Query
-- ============================================================
-- இந்த queries-ஐ ஒவ்வொன்றாக SSMS-ல் run பண்ணி காரணம் கண்டுபிடிக்கலாம்
-- Period மாற்ற வேண்டும்: @Period = 'YYYY-MM' (உதா: '2026-09')
-- ============================================================

DECLARE @EmpCode     VARCHAR(50)  = 'FWGG000368';
DECLARE @Period      VARCHAR(7)   = '2026-09';  -- << இங்கே மாதம் மாற்றவும்
DECLARE @PeriodStart DATE         = CAST(@Period + '-01' AS DATE);
DECLARE @PeriodEnd   DATE         = EOMONTH(CAST(@Period + '-01' AS DATE));

PRINT '=== STEP 1: Employee Basic Info ===';
SELECT 
    e.EMP_ID,
    e.EMP_CODE,
    e.EMP_NAME,
    e.EMP_DATE_OF_BIRTH,
    DATEDIFF(YEAR, e.EMP_DATE_OF_BIRTH, GETDATE()) AS CurrentAge,
    CASE WHEN DATEDIFF(YEAR, e.EMP_DATE_OF_BIRTH, GETDATE()) >= 55 
         THEN 'YES - Age > 55, PF will be ZERO in report!' 
         ELSE 'OK - Age < 55' END AS AgeCheck,
    e.UANNumber,
    e.PFAccountNumber,
    e.EMP_BRANCH_CODE,
    e.IndianState
FROM Employees e
WHERE e.EMP_CODE = @EmpCode;

-- ============================================================
PRINT '';
PRINT '=== STEP 2: EPFDETECT Check (PF Deduction enabled?) ===';
-- EPFDETECT = false இருந்தால் PF report-ல் வராது!
SELECT 
    esd.EMPFL_CODE,
    esd.EPFDETECT,
    CASE WHEN esd.EPFDETECT = 1 THEN 'YES - PF Eligible' 
         ELSE 'NO - EPFDETECT = false, இதனால் PF report-ல் வராது!' 
    END AS PFEligibleStatus
FROM EmployeeSalaryDetails esd
WHERE esd.EMPFL_CODE = @EmpCode;

-- ============================================================
PRINT '';
PRINT '=== STEP 3: PaySlip Check (Salary processed?) ===';
-- PaySlip இல்லாவிட்டால் skip ஆகும்!
SELECT 
    ps.EmployeeID,
    ps.Period,
    ps.BasicSalary,
    ps.EPFDeductionAmount,
    CASE WHEN ps.BasicSalary IS NULL OR ps.BasicSalary = 0 
         THEN 'WARNING - BasicSalary = 0 or NULL' 
         ELSE 'OK' END AS BasicSalaryCheck
FROM PaySlips ps
JOIN Employees e ON e.EMP_ID = ps.EmployeeID
WHERE e.EMP_CODE = @EmpCode
  AND ps.Period = @PeriodEnd
ORDER BY ps.Period DESC;

-- ============================================================
PRINT '';
PRINT '=== STEP 4: Attendance Check (Attendance record exists?) ===';
-- Attendance + PaySlip இரண்டுமே இல்லாவிட்டால் skip ஆகும்!
SELECT 
    att.EmployeeID,
    att.Period,
    COUNT(det.ID) AS AttendanceDetailCount,
    SUM(CASE WHEN det.Type IN (1,2,3,4,5,8,9,10,11,12,13,14,15,16,17) 
             THEN 1 ELSE 0 END) AS WorkingDaysCount,
    SUM(CASE WHEN det.Type = 6 THEN 1 ELSE 0 END) AS UnpaidLeaveCount,
    SUM(CASE WHEN det.Type = 7 THEN 1 ELSE 0 END) AS AbsentCount
FROM Attendances att
JOIN AttendanceDetails det ON att.ID = det.AttendanceID
JOIN Employees e ON e.EMP_ID = att.EmployeeID
WHERE e.EMP_CODE = @EmpCode
  AND att.Period = @PeriodEnd
GROUP BY att.EmployeeID, att.Period;

-- ============================================================
PRINT '';
PRINT '=== STEP 5: Commercial Breakdown (CB values) ===';
-- CB_Basic = 0 இருந்தால் PF = 0 வரும்!
SELECT 
    e.EMP_CODE,
    e.EMP_NAME,
    e.CB_Basic,
    e.CB_DA,
    e.CB_HRA,
    e.CB_Leaves,
    e.CB_OtherAllowances,
    e.CB_NH,
    e.CB_AdvanceStatutoryBonus,
    e.CB_SubTotal,
    (ISNULL(e.CB_Basic,0) + ISNULL(e.CB_DA,0)) AS BasicPlusDA,
    -- PF = 12% of Basic+DA, max ceiling ₹15000
    CASE WHEN (ISNULL(e.CB_Basic,0) + ISNULL(e.CB_DA,0)) > 15000 
         THEN 1800  -- ceiling
         ELSE ROUND((ISNULL(e.CB_Basic,0) + ISNULL(e.CB_DA,0)) * 0.12, 0)
    END AS ExpectedPF
FROM Employees e
WHERE e.EMP_CODE = @EmpCode;

-- ============================================================
PRINT '';
PRINT '=== STEP 6: EmploymentDetails ===';
SELECT 
    ed.EMPPAY_CODE,
    ed.EMPPAY_DATE_JOINED,
    ed.EMPPAY_DATE_RESIGNED,
    ed.EMPPAY_BASIC_RATE,
    ed.ATTENDANCEALLOWANCE,
    ed.SpecialAllowance,
    ed.AttendanceAllowanceWorkingDays
FROM EmploymentDetails ed
WHERE ed.EMPPAY_CODE = @EmpCode;

-- ============================================================
PRINT '';
PRINT '=== STEP 7: Overall Summary - PF Report ல் வராத காரணம் ===';
SELECT 
    e.EMP_CODE,
    e.EMP_NAME,
    -- Check 1: EPFDETECT
    CASE WHEN esd.EPFDETECT = 1 THEN 'PASS' ELSE 'FAIL - EPFDETECT false' END AS Check1_EPFDETECT,
    -- Check 2: Age
    CASE WHEN DATEDIFF(YEAR, e.EMP_DATE_OF_BIRTH, GETDATE()) >= 55 
         THEN 'WARN - Age >= 55 (PF = 0)' ELSE 'PASS' END AS Check2_Age,
    -- Check 3: PaySlip exists
    CASE WHEN ps.EmployeeID IS NOT NULL THEN 'PASS' ELSE 'FAIL - No PaySlip for period' END AS Check3_PaySlip,
    -- Check 4: Attendance exists
    CASE WHEN att.EmployeeID IS NOT NULL THEN 'PASS' ELSE 'FAIL - No Attendance for period' END AS Check4_Attendance,
    -- Check 5: CB_Basic > 0
    CASE WHEN ISNULL(e.CB_Basic,0) > 0 THEN 'PASS' ELSE 'FAIL - CB_Basic = 0' END AS Check5_CBBasic,
    -- Values
    DATEDIFF(YEAR, e.EMP_DATE_OF_BIRTH, GETDATE()) AS Age,
    esd.EPFDETECT,
    e.CB_Basic,
    e.CB_DA,
    ps.EPFDeductionAmount AS PaySlip_EPF
FROM Employees e
LEFT JOIN EmployeeSalaryDetails esd ON esd.EMPFL_CODE = e.EMP_CODE
LEFT JOIN PaySlips ps ON ps.EmployeeID = e.EMP_ID AND ps.Period = @PeriodEnd
LEFT JOIN Attendances att ON att.EmployeeID = e.EMP_ID AND att.Period = @PeriodEnd
WHERE e.EMP_CODE = @EmpCode;
