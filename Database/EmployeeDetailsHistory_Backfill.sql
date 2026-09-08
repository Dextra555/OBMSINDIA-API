-- ============================================================
-- EmployeeSalaryDetailHistory & EmploymentDetailsHistory
-- Back-fill Script (India)
-- Purpose : Insert the FIRST history row for ALL existing
--           employees who do not yet have a record in the
--           new per-branch detail history tables.
--
-- Run this ONCE after EmployeeDetailsHistory_Migration.sql.
--
-- Logic (mirrors FWG-Malaysia-New\Database\EmployeeTransfer_History_Backfill.sql):
--   Emp_StartDate = EMPPAY_DATE_JOINED (join date from the employee tables)
--   Emp_EndDate   = NULL               (currently active)
-- ============================================================

-- -------------------------------------------------------
-- 1. Back-fill EmploymentDetailsHistory
--    Join Employee -> EmploymentDetails where no history
--    row exists yet for that EMP_ID.
-- -------------------------------------------------------
INSERT INTO dbo.EmploymentDetailsHistory (
    EMP_ID,
    EMPPAY_CODE,
    EMPPAY_BRANCHCODE,
    EMPPAY_JOB_TITLE,
    EMPPAY_CATEGORY,
    EMPPAY_DATE_JOINED,
    EMPPAY_DATE_CONFIRM,
    EMPPAY_DATE_PROMOTION,
    EMPPAY_DATE_RESIGNED,
    EMPPAY_BASIC_RATE,
    SALARYLAB,
    ATTENDANCEALLOWANCE,
    NewStructureATTENDANCEALLOWANCE,
    SpecialAllowance,
    AttendanceAllowanceWorkingDays,
    AttendanceAllowanceFollowCalendar,
    LASTUPDATE,
    LastUpdatedBy,
    Emp_StartDate,
    Emp_EndDate
)
SELECT
    e.EMP_ID,
    ed.EMPPAY_CODE,
    ed.EMPPAY_BRANCHCODE,
    ed.EMPPAY_JOB_TITLE,
    ed.EMPPAY_CATEGORY,
    ed.EMPPAY_DATE_JOINED,
    ed.EMPPAY_DATE_CONFIRM,
    ed.EMPPAY_DATE_PROMOTION,
    ed.EMPPAY_DATE_RESIGNED,
    ed.EMPPAY_BASIC_RATE,
    ed.SALARYLAB,
    ed.ATTENDANCEALLOWANCE,
    ed.NewStructureATTENDANCEALLOWANCE,
    ed.SpecialAllowance,
    ed.AttendanceAllowanceWorkingDays,
    ed.AttendanceAllowanceFollowCalendar,
    ed.LASTUPDATE,
    ed.LastUpdatedBy,
    -- Emp_StartDate = join date (when available), else today
    CAST(ISNULL(ed.EMPPAY_DATE_JOINED, GETDATE()) AS DATE),
    NULL   -- Emp_EndDate NULL = currently active
FROM dbo.Employee e
INNER JOIN dbo.EmploymentDetails ed
    ON ed.EMPPAY_CODE = e.EMP_CODE
WHERE NOT EXISTS (
    SELECT 1
    FROM   dbo.EmploymentDetailsHistory h
    WHERE  h.EMP_ID = e.EMP_ID
);

PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' rows inserted into EmploymentDetailsHistory.';
GO

-- -------------------------------------------------------
-- 2. Back-fill EmployeeSalaryDetailHistory
--    Join Employee -> EmployeeSalaryDetails where no history
--    row exists yet for that EMP_ID.
-- -------------------------------------------------------
INSERT INTO dbo.EmployeeSalaryDetailHistory (
    EMP_ID,
    EMPFL_CODE,
    EMPFL_BRANCHCODE,
    EMPFL_BANK,
    EMPFL_BK_ACCNO,
    EMPFL_TAX_NO,
    EMPFL_EPFNO,
    EMPFL_EPF8Pa,
    EMPFL_SOSCO_NO,
    EPFDETECT,
    PAYMODE,
    SOCSODETECT,
    TMPGUARD,
    DETECTBYND55,
    LASTUPDATE,
    LastUpdatedBy,
    INCOMETAXDETECT,
    EMP_SP_TEL_NO,
    OT,
    Emp_StartDate,
    Emp_EndDate
)
SELECT
    e.EMP_ID,
    sd.EMPFL_CODE,
    sd.EMPFL_BRANCHCODE,
    sd.EMPFL_BANK,
    sd.EMPFL_BK_ACCNO,
    sd.EMPFL_TAX_NO,
    sd.EMPFL_EPFNO,
    sd.EMPFL_EPF8Pa,
    sd.EMPFL_SOSCO_NO,
    sd.EPFDETECT,
    sd.PAYMODE,
    sd.SOCSODETECT,
    sd.TMPGUARD,
    sd.DETECTBYND55,
    sd.LASTUPDATE,
    sd.LastUpdatedBy,
    sd.INCOMETAXDETECT,
    sd.EMP_SP_TEL_NO,
    sd.OT,
    -- Emp_StartDate = join date from EmploymentDetails (when available)
    CAST(ISNULL(ed.EMPPAY_DATE_JOINED, GETDATE()) AS DATE),
    NULL   -- Emp_EndDate NULL = currently active
FROM dbo.Employee e
INNER JOIN dbo.EmployeeSalaryDetails sd
    ON sd.EMPFL_CODE = e.EMP_CODE
LEFT JOIN dbo.EmploymentDetails ed
    ON ed.EMPPAY_CODE = e.EMP_CODE
WHERE NOT EXISTS (
    SELECT 1
    FROM   dbo.EmployeeSalaryDetailHistory h
    WHERE  h.EMP_ID = e.EMP_ID
);

PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' rows inserted into EmployeeSalaryDetailHistory.';
GO

-- -------------------------------------------------------
-- 3. Verify counts
-- -------------------------------------------------------
SELECT
    'Employee'                   AS TableName,
    COUNT(*)                     AS TotalRows
FROM dbo.Employee
UNION ALL
SELECT 'EmploymentDetailsHistory',   COUNT(*) FROM dbo.EmploymentDetailsHistory
UNION ALL
SELECT 'EmployeeSalaryDetailHistory', COUNT(*) FROM dbo.EmployeeSalaryDetailHistory
UNION ALL
SELECT 'EmployeeHistory',            COUNT(*) FROM dbo.EmployeeHistory;
GO
