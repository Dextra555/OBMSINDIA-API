-- ============================================================
-- Migration: Add Emp_StartDate and Emp_EndDate to EmployeeHistory
-- Purpose  : Track the branch period for each history row.
--            Emp_StartDate = when the employee started at this branch.
--            Emp_EndDate   = when they left (NULL = currently active).
-- Run once against the OBMS India database.
-- ============================================================

-- Step 1: Add the columns (nullable so existing rows don't break)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'EmployeeHistory' AND COLUMN_NAME = 'Emp_StartDate'
)
BEGIN
    ALTER TABLE EmployeeHistory ADD Emp_StartDate DATETIME NULL;
    PRINT 'Column Emp_StartDate added to EmployeeHistory.';
END
ELSE
BEGIN
    PRINT 'Column Emp_StartDate already exists — skipped.';
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'EmployeeHistory' AND COLUMN_NAME = 'Emp_EndDate'
)
BEGIN
    ALTER TABLE EmployeeHistory ADD Emp_EndDate DATETIME NULL;
    PRINT 'Column Emp_EndDate added to EmployeeHistory.';
END
ELSE
BEGIN
    PRINT 'Column Emp_EndDate already exists — skipped.';
END

-- Step 2: Back-fill Emp_StartDate for existing rows using EMPPAY_DATE_JOINED
--         (the best available proxy for when the employee started at that branch)
UPDATE EmployeeHistory
SET    Emp_StartDate = EMPPAY_DATE_JOINED
WHERE  Emp_StartDate IS NULL
  AND  EMPPAY_DATE_JOINED IS NOT NULL;

PRINT 'Back-filled Emp_StartDate from EMPPAY_DATE_JOINED for existing rows.';

-- Step 3: For the latest active row per employee, leave Emp_EndDate as NULL
--         (already NULL — nothing to do). Older rows that were created via
--         UpdateEmployeeTransfer will have Emp_EndDate still NULL; those can
--         be cleaned up manually if needed since we now close them properly
--         going forward.

SELECT 'Migration complete. Rows updated: ' + CAST(@@ROWCOUNT AS VARCHAR);
