-- ============================================================
-- VERIFY: Kani (or any employee) EmploymentDetailsHistory
-- Scenario: Join 1-Jan-2026 Chennai, Transfer 1-Mar-2026 Coimbatore
-- Run this in SQL Server to check if history rows are correct
-- ============================================================

-- STEP 1: Find Kani's Employee details
SELECT 
    EMP_ID,
    EMP_CODE,
    EMP_NAME,
    EMP_BRANCH_CODE     AS CurrentBranch,
    HasTransfered,
    TransferDate,
    OldBranch
FROM Employee
WHERE EMP_NAME LIKE '%Kani%';

-- ============================================================
-- STEP 2: Check EmploymentDetailsHistory rows for Kani
-- Expected result:
--   Row 1: EMPPAY_BRANCHCODE = Chennai,     Emp_StartDate = 2026-01-01, Emp_EndDate = 2026-02-28
--   Row 2: EMPPAY_BRANCHCODE = Coimbatore,  Emp_StartDate = 2026-03-01, Emp_EndDate = NULL
-- ============================================================
SELECT 
    edh.EMPPAY_HISTORY_ID,
    e.EMP_CODE,
    e.EMP_NAME,
    edh.EMPPAY_BRANCHCODE,
    edh.Emp_StartDate,
    edh.Emp_EndDate,
    CASE 
        WHEN edh.Emp_EndDate IS NULL THEN 'ACTIVE (current branch)'
        ELSE 'CLOSED'
    END AS RowStatus
FROM EmploymentDetailsHistory edh
INNER JOIN Employee e ON e.EMP_ID = edh.EMP_ID
WHERE e.EMP_NAME LIKE '%Kani%'
ORDER BY edh.Emp_StartDate;

-- ============================================================
-- STEP 3: Simulate getListByEmployee for Jan 2026 + Chennai
-- Expected: Kani வரணும் (Chennai row covers Jan)
-- ============================================================
DECLARE @StartPeriod DATE = '2026-01-01';  -- FirstDayOfMonth
DECLARE @EndPeriod   DATE = '2026-01-31';  -- LastDayOfMonth
DECLARE @Branch      NVARCHAR(50) = 'FWG001';  -- Change to actual Chennai branch code

SELECT DISTINCT
    e.EMP_CODE,
    e.EMP_NAME,
    e.EMP_BRANCH_CODE   AS CurrentBranch,
    edh.EMPPAY_BRANCHCODE AS HistoryBranch,
    ISNULL(edh.EMPPAY_BRANCHCODE, e.EMP_BRANCH_CODE) AS EffectiveBranch,
    edh.Emp_StartDate,
    edh.Emp_EndDate
FROM Employee e
INNER JOIN EmploymentDetails ed   ON e.EMP_CODE = ed.EMPPAY_CODE
INNER JOIN EmployeeSalaryDetails esd ON e.EMP_CODE = esd.EMPFL_CODE
LEFT JOIN EmploymentDetailsHistory edh 
    ON edh.EMP_ID = e.EMP_ID
    AND edh.Emp_StartDate <= @EndPeriod
    AND (edh.Emp_EndDate IS NULL OR edh.Emp_EndDate >= @StartPeriod)
WHERE e.EMP_NAME LIKE '%Kani%'
AND ISNULL(edh.EMPPAY_BRANCHCODE, e.EMP_BRANCH_CODE) = @Branch;

-- ============================================================
-- STEP 4: Simulate getListByEmployee for Feb 2026 + Chennai
-- Expected: Kani வரணும் (Chennai row covers Feb too)
-- ============================================================
DECLARE @StartPeriodFeb DATE = '2026-02-01';
DECLARE @EndPeriodFeb   DATE = '2026-02-28';

SELECT DISTINCT
    e.EMP_CODE,
    e.EMP_NAME,
    ISNULL(edh.EMPPAY_BRANCHCODE, e.EMP_BRANCH_CODE) AS EffectiveBranch,
    edh.Emp_StartDate,
    edh.Emp_EndDate
FROM Employee e
INNER JOIN EmploymentDetails ed   ON e.EMP_CODE = ed.EMPPAY_CODE
INNER JOIN EmployeeSalaryDetails esd ON e.EMP_CODE = esd.EMPFL_CODE
LEFT JOIN EmploymentDetailsHistory edh 
    ON edh.EMP_ID = e.EMP_ID
    AND edh.Emp_StartDate <= @EndPeriodFeb
    AND (edh.Emp_EndDate IS NULL OR edh.Emp_EndDate >= @StartPeriodFeb)
WHERE e.EMP_NAME LIKE '%Kani%'
AND ISNULL(edh.EMPPAY_BRANCHCODE, e.EMP_BRANCH_CODE) = @Branch;

-- ============================================================
-- STEP 5: Simulate getListByEmployee for Jan/Feb 2026 + Coimbatore
-- Expected: Kani வரக்கூடாது ❌
-- ============================================================
DECLARE @CoimbBranch NVARCHAR(50) = 'FWG003';  -- Change to actual Coimbatore code

SELECT DISTINCT
    e.EMP_CODE,
    e.EMP_NAME,
    ISNULL(edh.EMPPAY_BRANCHCODE, e.EMP_BRANCH_CODE) AS EffectiveBranch,
    edh.Emp_StartDate,
    edh.Emp_EndDate,
    'SHOULD BE EMPTY - Jan/Feb Coimbatore' AS ExpectedResult
FROM Employee e
INNER JOIN EmploymentDetails ed   ON e.EMP_CODE = ed.EMPPAY_CODE
INNER JOIN EmployeeSalaryDetails esd ON e.EMP_CODE = esd.EMPFL_CODE
LEFT JOIN EmploymentDetailsHistory edh 
    ON edh.EMP_ID = e.EMP_ID
    AND edh.Emp_StartDate <= @EndPeriod   -- Jan EndPeriod
    AND (edh.Emp_EndDate IS NULL OR edh.Emp_EndDate >= @StartPeriod)  -- Jan StartPeriod
WHERE e.EMP_NAME LIKE '%Kani%'
AND ISNULL(edh.EMPPAY_BRANCHCODE, e.EMP_BRANCH_CODE) = @CoimbBranch;

-- ============================================================
-- STEP 6: Simulate getListByEmployee for Mar 2026 + Coimbatore
-- Expected: Kani வரணும் ✅
-- ============================================================
DECLARE @StartPeriodMar DATE = '2026-03-01';
DECLARE @EndPeriodMar   DATE = '2026-03-31';

SELECT DISTINCT
    e.EMP_CODE,
    e.EMP_NAME,
    ISNULL(edh.EMPPAY_BRANCHCODE, e.EMP_BRANCH_CODE) AS EffectiveBranch,
    edh.Emp_StartDate,
    edh.Emp_EndDate,
    'SHOULD SHOW Kani - Mar Coimbatore' AS ExpectedResult
FROM Employee e
INNER JOIN EmploymentDetails ed   ON e.EMP_CODE = ed.EMPPAY_CODE
INNER JOIN EmployeeSalaryDetails esd ON e.EMP_CODE = esd.EMPFL_CODE
LEFT JOIN EmploymentDetailsHistory edh 
    ON edh.EMP_ID = e.EMP_ID
    AND edh.Emp_StartDate <= @EndPeriodMar
    AND (edh.Emp_EndDate IS NULL OR edh.Emp_EndDate >= @StartPeriodMar)
WHERE e.EMP_NAME LIKE '%Kani%'
AND ISNULL(edh.EMPPAY_BRANCHCODE, e.EMP_BRANCH_CODE) = @CoimbBranch;
