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
GO

-- Step 2: Branch-change rows carry the transfer / effective date.
--         (Rows snapshotted after a transfer have HasTransfered = 1 and the
--          transfer (Effective Start Date) in TransferDate.)
UPDATE EmployeeHistory
SET    Emp_StartDate = TransferDate
WHERE  HasTransfered = 1
  AND  TransferDate IS NOT NULL
  AND  Emp_StartDate IS NULL;

PRINT 'Set Emp_StartDate = TransferDate (Effective Start Date) for branch-change rows.';

-- Step 3: Remaining rows use the join date as the branch start.
--         (First history row per employee, or transfer rows with no date.)
UPDATE EmployeeHistory
SET    Emp_StartDate = EMPPAY_DATE_JOINED
WHERE  Emp_StartDate IS NULL
  AND  EMPPAY_DATE_JOINED IS NOT NULL;

PRINT 'Back-filled remaining Emp_StartDate from EMPPAY_DATE_JOINED.';

-- Step 4: Close each previous row with the next row's Effective Start Date,
--         keeping the latest (active) row per employee NULL.
;WITH cte AS
(
    SELECT EMP_HISTORY_ID,
           EMP_ID,
           Emp_StartDate,
           ROW_NUMBER() OVER (PARTITION BY EMP_ID ORDER BY EMP_HISTORY_ID) AS Seq
    FROM   EmployeeHistory
)
UPDATE h
SET    h.Emp_EndDate = nxt.Emp_StartDate
FROM   EmployeeHistory h
JOIN   cte cur ON cur.EMP_HISTORY_ID = h.EMP_HISTORY_ID
JOIN   cte nxt ON nxt.EMP_ID = cur.EMP_ID AND nxt.Seq = cur.Seq + 1
WHERE  h.Emp_EndDate IS NULL
  AND  nxt.Emp_StartDate IS NOT NULL;

PRINT 'Closed previous rows with the next row''s Effective Start Date.';

SELECT 'Migration complete.';
