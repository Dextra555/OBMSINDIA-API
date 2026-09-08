-- =============================================
-- EmployeeHistory Duplicate Active Records Cleanup
-- Date: 2026-09-02
-- Description: Closes duplicate active records (Emp_EndDate = NULL) 
--              keeping only the most recent one for each employee
-- =============================================

-- Step 1: Identify employees with multiple active history records
SELECT 
    EMP_ID,
    EMP_CODE,
    EMP_NAME,
    COUNT(*) AS ActiveRecordCount
FROM [dbo].[EmployeeHistory]
WHERE Emp_EndDate IS NULL
GROUP BY EMP_ID, EMP_CODE, EMP_NAME
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

PRINT '--- Employees with multiple active records identified ---';
GO

-- Step 2: Close all but the most recent active record for each employee
-- Keep the record with the highest EMP_HISTORY_ID (most recent)

WITH RankedHistory AS (
    SELECT 
        EMP_HISTORY_ID,
        EMP_ID,
        EMP_CODE,
        Emp_StartDate,
        ROW_NUMBER() OVER (
            PARTITION BY EMP_ID 
            ORDER BY EMP_HISTORY_ID DESC
        ) AS RowNum
    FROM [dbo].[EmployeeHistory]
    WHERE Emp_EndDate IS NULL
)
UPDATE eh
SET eh.Emp_EndDate = DATEADD(DAY, -1, ISNULL(rh.Emp_StartDate, GETDATE()))
FROM [dbo].[EmployeeHistory] eh
INNER JOIN RankedHistory rh ON eh.EMP_HISTORY_ID = rh.EMP_HISTORY_ID
WHERE rh.RowNum > 1  -- Close all except the most recent (RowNum = 1)
  AND eh.Emp_EndDate IS NULL;

PRINT '--- Duplicate active records closed ---';
GO

-- Step 3: Verify cleanup - Should return no rows
SELECT 
    EMP_ID,
    EMP_CODE,
    EMP_NAME,
    COUNT(*) AS ActiveRecordCount
FROM [dbo].[EmployeeHistory]
WHERE Emp_EndDate IS NULL
GROUP BY EMP_ID, EMP_CODE, EMP_NAME
HAVING COUNT(*) > 1;

PRINT '--- Verification complete (no results = success) ---';
GO

-- Step 4: Show final state - Each employee should have exactly one active record
SELECT 
    EMP_ID,
    COUNT(*) AS TotalRecords,
    SUM(CASE WHEN Emp_EndDate IS NULL THEN 1 ELSE 0 END) AS ActiveRecords,
    SUM(CASE WHEN Emp_EndDate IS NOT NULL THEN 1 ELSE 0 END) AS ClosedRecords
FROM [dbo].[EmployeeHistory]
GROUP BY EMP_ID
ORDER BY TotalRecords DESC;

PRINT '--- Final state summary ---';
GO
