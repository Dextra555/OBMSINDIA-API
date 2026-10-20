-- ============================================================
-- EMP_ID = 319 - EPF Contribution Remitted calculation trace
-- எப்படி value வருது என்று step-by-step check பண்ணும் query
-- ============================================================
-- @Period மாற்றவும்: 'YYYY-MM' (உதா: '2026-09')
-- ============================================================

DECLARE @EmpID      INT          = 319;
DECLARE @Period     VARCHAR(7)   = '2026-09';   -- << மாதம் மாற்றவும்
DECLARE @PeriodStart DATE        = CAST(@Period + '-01' AS DATE);
DECLARE @PeriodEnd   DATE        = EOMONTH(CAST(@Period + '-01' AS DATE));
DECLARE @DaysInMonth INT         = DAY(@PeriodEnd);

PRINT '=== STEP 1: Employee Basic Info + CB values ===';
SELECT 
    e.EMP_ID,
    e.EMP_CODE,
    e.EMP_NAME,
    e.EMP_DATE_OF_BIRTH,
    DATEDIFF(YEAR, e.EMP_DATE_OF_BIRTH, GETDATE()) AS Age,
    CASE WHEN DATEDIFF(YEAR, e.EMP_DATE_OF_BIRTH, GETDATE()) >= 55 
         THEN 'YES - Age >= 55' ELSE 'OK - Age < 55' END AS AgeCheck,
    e.EMP_BRANCH_CODE,
    e.IndianState,
    e.UANNumber,
    e.PFAccountNumber,
    -- CB values (Commercial Breakdown)
    ISNULL(e.CB_Basic, 0)              AS CB_Basic,
    ISNULL(e.CB_DA, 0)                 AS CB_DA,
    ISNULL(e.CB_HRA, 0)                AS CB_HRA,
    ISNULL(e.CB_OtherAllowances, 0)    AS CB_OtherAllowances,
    ISNULL(e.CB_Leaves, 0)             AS CB_Leaves,
    ISNULL(e.CB_NH, 0)                 AS CB_NH,
    ISNULL(e.CB_AdvanceStatutoryBonus, 0) AS CB_AdvanceStatutoryBonus,
    ISNULL(e.CB_SubTotal, 0)           AS CB_SubTotal,
    -- Basic + DA (PF calculation base)
    ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) AS BasicPlusDA
FROM Employees e
WHERE e.EMP_ID = @EmpID;

-- ============================================================
PRINT '';
PRINT '=== STEP 2: EPFDETECT Check (PF deduction enabled?) ===';
SELECT 
    esd.EMPFL_CODE,
    esd.EPFDETECT,
    CASE WHEN esd.EPFDETECT = 1 THEN 'PASS - PF Eligible' 
         ELSE 'FAIL - EPFDETECT = false, report-ல் வராது!' 
    END AS PFEligibleStatus
FROM EmployeeSalaryDetails esd
WHERE esd.EMPFL_CODE = (SELECT EMP_CODE FROM Employees WHERE EMP_ID = @EmpID);

-- ============================================================
PRINT '';
PRINT '=== STEP 3: PaySlip Check (Salary processed for period?) ===';
SELECT 
    ps.EmployeeID,
    ps.Period,
    ps.BasicSalary,
    ps.BasicSalaryDays,
    ps.EPFDeductionAmount,
    CASE WHEN ps.BasicSalary IS NOT NULL AND ps.BasicSalary > 0 
         THEN 'PASS' ELSE 'FAIL - No PaySlip or BasicSalary = 0' END AS PaySlipCheck
FROM PaySlips ps
WHERE ps.EmployeeID = @EmpID
  AND ps.Period = @PeriodEnd;

-- ============================================================
PRINT '';
PRINT '=== STEP 4: Attendance Count (daysWorked) ===';
-- Code-ல் Type IN (1,2,3,4,5,8,9,10,11,12,13,14,15,16,17) count ஆகுது
SELECT 
    att.EmployeeID,
    att.Period,
    COUNT(DISTINCT det.AttendanceDate) AS DaysWorked,
    @DaysInMonth AS DaysInMonth,
    -- Working day types
    SUM(CASE WHEN det.Type IN (1,2,3,4,5,8,9,10,11,12,13,14,15,16,17) 
             THEN 1 ELSE 0 END) AS WorkingDayDetails,
    SUM(CASE WHEN det.Type = 6 THEN 1 ELSE 0 END) AS UnpaidLeave,
    SUM(CASE WHEN det.Type = 7 THEN 1 ELSE 0 END) AS Absent
FROM Attendances att
JOIN AttendanceDetails det ON att.ID = det.AttendanceID
WHERE att.EmployeeID = @EmpID
  AND att.Period = @PeriodEnd
GROUP BY att.EmployeeID, att.Period;

-- ============================================================
PRINT '';
PRINT '=== STEP 5: FULL CALCULATION TRACE - EPF Contribution Remitted ===';
-- இதுதான் முக்கிய query - ஒவ்வொரு step-ஆக value show ஆகும்

DECLARE @CB_Basic      DECIMAL(18,2) = 0;
DECLARE @CB_DA         DECIMAL(18,2) = 0;
DECLARE @DaysWorked    INT           = 0;
DECLARE @EPFDETECT     BIT           = 0;
DECLARE @HasPaySlip    BIT           = 0;
DECLARE @EPFDeductionAmount DECIMAL(18,2) = 0;

-- Get CB values
SELECT @CB_Basic = ISNULL(CB_Basic, 0), @CB_DA = ISNULL(CB_DA, 0)
FROM Employees WHERE EMP_ID = @EmpID;

-- Get daysWorked (distinct attendance dates with working day types)
SELECT @DaysWorked = COUNT(DISTINCT det.AttendanceDate)
FROM Attendances att
JOIN AttendanceDetails det ON att.ID = det.AttendanceID
WHERE att.EmployeeID = @EmpID
  AND att.Period = @PeriodEnd
  AND det.Type IN (1,2,3,4,5,8,9,10,11,12,13,14,15,16,17);

-- Get EPFDETECT
SELECT @EPFDETECT = ISNULL(esd.EPFDETECT, 0)
FROM EmployeeSalaryDetails esd
WHERE esd.EMPFL_CODE = (SELECT EMP_CODE FROM Employees WHERE EMP_ID = @EmpID);

-- Get PaySlip
SELECT @HasPaySlip = CASE WHEN ps.BasicSalary IS NOT NULL AND ps.BasicSalary > 0 THEN 1 ELSE 0 END,
       @EPFDeductionAmount = ISNULL(ps.EPFDeductionAmount, 0)
FROM PaySlips ps
WHERE ps.EmployeeID = @EmpID AND ps.Period = @PeriodEnd;

-- Final calculation
DECLARE @BasicPlusDA      DECIMAL(18,2) = @CB_Basic + @CB_DA;
DECLARE @ActualBasicDA    DECIMAL(18,2) = 0;
DECLARE @PFSalary         DECIMAL(18,2) = 0;
DECLARE @EPF              DECIMAL(18,2) = 0;
DECLARE @EPFRounded       DECIMAL(18,0) = 0;

-- Step A: Pro-rate Basic+DA
IF @DaysWorked > 0
    SET @ActualBasicDA = ROUND((@BasicPlusDA / @DaysInMonth) * @DaysWorked, 0);

-- Step B: Cap at 15000 (ceiling)
SET @PFSalary = CASE WHEN @ActualBasicDA > 15000 THEN 15000 ELSE @ActualBasicDA END;

-- Step C: EPF = PFSalary × 12%
IF @EPFDETECT = 1 AND @HasPaySlip = 1
    SET @EPF = @PFSalary * 12.0 / 100.0;

-- Step D: Round to whole rupees
SET @EPFRounded = ROUND(@EPF, 0);

-- Final Result
SELECT 
    @EmpID                                    AS EMP_ID,
    @Period                                    AS Period,
    -- Step 1: CB values
    @CB_Basic                                  AS CB_Basic,
    @CB_DA                                     AS CB_DA,
    @BasicPlusDA                               AS BasicPlusDA,
    -- Step 2: Pro-rate
    @DaysInMonth                               AS DaysInMonth_CalendarDays,
    @DaysWorked                                AS DaysWorked_Attendance,
    @ActualBasicDA                             AS ActualBasicDA_ProRated,
    -- Step 3: Ceiling cap
    @PFSalary                                  AS PFSalary_CappedAt15000,
    -- Step 4: 12% calculation
    @EPF                                       AS EPF_12Percent,
    @EPFRounded                                AS EPF_Contribution_Remitted_Final,
    -- Checks
    @EPFDETECT                                 AS EPFDETECT,
    CASE WHEN @EPFDETECT = 1 THEN 'PASS' ELSE 'FAIL' END AS Check1_EPFDETECT,
    @HasPaySlip                                AS HasPaySlip,
    CASE WHEN @HasPaySlip = 1 THEN 'PASS' ELSE 'FAIL' END AS Check2_PaySlip,
    CASE WHEN @DaysWorked > 0 THEN 'PASS' ELSE 'FAIL' END AS Check3_Attendance,
    -- PaySlip EPF for comparison
    @EPFDeductionAmount                        AS PaySlip_EPFDeductionAmount,
    -- Summary
    CASE 
        WHEN @EPFDETECT = 0 THEN 'EPFDETECT = false → Report-ல் வராது'
        WHEN @HasPaySlip = 0 THEN 'PaySlip இல்லை → Report-ல் வராது'
        WHEN @DaysWorked = 0 THEN 'Attendance இல்லை → Report-ல் வராது'
        WHEN @CB_Basic = 0 AND @CB_DA = 0 THEN 'CB_Basic + CB_DA = 0 → EPF = 0'
        ELSE 'OK - EPF Contribution Remitted = ' + CAST(@EPFRounded AS VARCHAR(20))
    END AS FinalStatus
;

PRINT '';
PRINT '=== Done. Final EPF Contribution Remitted value மேலே உள்ளது ===';
