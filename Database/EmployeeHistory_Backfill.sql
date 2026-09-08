-- ============================================================
-- EmployeeHistory — Back-fill Script (India)
-- Purpose : Insert the FIRST EmployeeHistory row for ALL existing
--           employees who do not yet have a history record.
--
-- Run this ONCE after EmployeeHistory_BranchPeriod_Migration.sql.
--
-- Logic (mirrors FWG-Malaysia-New\Database\EmployeeTransfer_History_Backfill.sql):
--   Emp_StartDate = EMPPAY_DATE_JOINED (join date from the employee tables)
--   Emp_EndDate   = NULL               (currently active)
--
-- Every later branch-change / transfer writes new rows with
-- Emp_StartDate = Effective Start Date, so the first record in the
-- database always carries the join date.
-- ============================================================

INSERT INTO dbo.EmployeeHistory (
    EMP_ID,
    EMP_ROLE,
    EMP_CODE,
    EMP_NAME,
    EMP_ADDRESS1,
    EMP_ADDRESS2,
    EMP_POST_CODE,
    EMP_TOWN,
    EMP_STATE,
    EMP_NATIONAL,
    EMP_PHONE,
    EMP_HGH_EDU,
    EM_WORK_EXP,
    EMP_DATE_OF_BIRTH,
    EMP_IC_OLD,
    EMP_IC_NEW,
    EMP_IC_COLOR,
    EMP_PASSPORT_NO,
    EMP_SEX,
    EMP_RACE,
    EMP_MARTIAL_STATUS,
    EMP_SPOUSE_NAME,
    EMP_SP_IC,
    EMP_NO_CHILD,
    EMP_SP_WORK,
    EMP_PER_NAME_CONTACT,
    EMP_CONTACT_ADDRESS1,
    EMP_CONTACT_ADDRESS2,
    EMP_CONTACT_POST_CODE,
    EMP_CONTACT_TOWN,
    EMP_CONTACT_STATE,
    EMP_CONTACT_TELEPHONE,
    EMP_BRANCH_CODE,
    OldBranch,
    TransferDate,
    HasTransfered,
    EMP_MOBILEPHONE,
    EMP_CITIZEN,
    EMP_CHECKLIST,
    EMP_CLIENT,
    NewSalaryStructure,
    KDNVetting,
    SalaryStructure1000_3h,
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
    INCOMETAXDETECT,
    LASTUPDATE,
    LastUpdatedBy,
    Emp_StartDate,
    Emp_EndDate
)
SELECT
    e.EMP_ID,
    e.EMP_ROLE,
    e.EMP_CODE,
    e.EMP_NAME,
    e.EMP_ADDRESS1,
    e.EMP_ADDRESS2,
    e.EMP_POST_CODE,
    e.EMP_TOWN,
    e.EMP_STATE,
    e.EMP_NATIONAL,
    e.EMP_PHONE,
    e.EMP_HGH_EDU,
    e.EM_WORK_EXP,
    e.EMP_DATE_OF_BIRTH,
    e.EMP_IC_OLD,
    e.EMP_IC_NEW,
    e.EMP_IC_COLOR,
    e.EMP_PASSPORT_NO,
    e.EMP_SEX,
    e.EMP_RACE,
    e.EMP_MARTIAL_STATUS,
    e.EMP_SPOUSE_NAME,
    e.EMP_SP_IC,
    e.EMP_NO_CHILD,
    e.EMP_SP_WORK,
    e.EMP_PER_NAME_CONTACT,
    e.EMP_CONTACT_ADDRESS1,
    e.EMP_CONTACT_ADDRESS2,
    e.EMP_CONTACT_POST_CODE,
    e.EMP_CONTACT_TOWN,
    e.EMP_CONTACT_STATE,
    e.EMP_CONTACT_TELEPHONE,
    e.EMP_BRANCH_CODE,
    e.OldBranch,
    e.TransferDate,
    e.HasTransfered,
    e.EMP_MOBILEPHONE,
    e.EMP_CITIZEN,
    e.EMP_CHECKLIST,
    e.EMP_CLIENT,
    e.NewSalaryStructure,
    e.KDNVetting,
    e.SalaryStructure1000_3h,
    -- EmploymentDetails columns
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
    -- EmployeeSalaryDetails columns
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
    sd.INCOMETAXDETECT,
    e.LASTUPDATE,
    e.LastUpdatedBy,
    -- Emp_StartDate = join date (when available), else today
    CAST(ISNULL(ed.EMPPAY_DATE_JOINED, GETDATE()) AS DATE),
    NULL   -- Emp_EndDate NULL = currently active
FROM dbo.Employee e
INNER JOIN dbo.EmploymentDetails ed
    ON ed.EMPPAY_CODE = e.EMP_CODE
INNER JOIN dbo.EmployeeSalaryDetails sd
    ON sd.EMPFL_CODE = e.EMP_CODE
WHERE NOT EXISTS (
    SELECT 1
    FROM   dbo.EmployeeHistory h
    WHERE  h.EMP_ID = e.EMP_ID
);

PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' rows inserted into EmployeeHistory.';
GO

-- -------------------------------------------------------
-- Verify counts
-- -------------------------------------------------------
SELECT
    'Employee'      AS TableName,
    COUNT(*)        AS TotalRows
FROM dbo.Employee
UNION ALL
SELECT 'EmployeeHistory', COUNT(*) FROM dbo.EmployeeHistory;
GO