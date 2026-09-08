-- ============================================================
-- Migration: Add EmployeeSalaryDetailHistory and
--            EmploymentDetailsHistory tables (India)
-- Purpose  : Track the salary and employment details per branch
--            period, mirroring FWG-Malaysia-New.
--            Emp_StartDate = effective date at this branch.
--            Emp_EndDate   = last day at this branch (NULL = active).
-- Run once against the OBMS India database.
-- ============================================================

-- -------------------------------------------------------
-- 1. Create EmployeeSalaryDetailHistory
-- -------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmployeeSalaryDetailHistory')
BEGIN
    CREATE TABLE dbo.EmployeeSalaryDetailHistory (
        EMPFL_HISTORY_ID    INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
        EMP_ID              INT            NOT NULL,
        EMPFL_CODE          NVARCHAR(50)   NOT NULL,
        EMPFL_BRANCHCODE    NVARCHAR(20)   NOT NULL,
        EMPFL_BANK          NVARCHAR(50)   NULL,
        EMPFL_BK_ACCNO      NVARCHAR(25)   NULL,
        EMPFL_TAX_NO        NVARCHAR(25)   NULL,
        EMPFL_EPFNO         NVARCHAR(25)   NULL,
        EMPFL_EPF8Pa        BIT            NULL,
        EMPFL_SOSCO_NO      NVARCHAR(25)   NULL,
        EPFDETECT           BIT            NOT NULL DEFAULT(0),
        PAYMODE             NVARCHAR(20)   NOT NULL,
        SOCSODETECT         BIT            NOT NULL DEFAULT(0),
        TMPGUARD            BIT            NOT NULL DEFAULT(0),
        DETECTBYND55        BIT            NOT NULL DEFAULT(0),
        LASTUPDATE          DATETIME       NOT NULL,
        LastUpdatedBy       NVARCHAR(20)   NULL,
        INCOMETAXDETECT     BIT            NULL,
        EMP_SP_TEL_NO       NVARCHAR(20)   NULL,
        OT                  NVARCHAR(10)   NULL,
        Emp_StartDate       DATETIME       NULL,
        Emp_EndDate         DATETIME       NULL
    );
    PRINT 'Table EmployeeSalaryDetailHistory created.';
END
ELSE
    PRINT 'Table EmployeeSalaryDetailHistory already exists.';
GO

-- -------------------------------------------------------
-- 2. Create EmploymentDetailsHistory
-- -------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmploymentDetailsHistory')
BEGIN
    CREATE TABLE dbo.EmploymentDetailsHistory (
        EMPPAY_HISTORY_ID                   INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
        EMP_ID                              INT             NOT NULL,
        EMPPAY_CODE                         NVARCHAR(50)    NOT NULL,
        EMPPAY_BRANCHCODE                   NVARCHAR(20)    NOT NULL,
        EMPPAY_JOB_TITLE                    NVARCHAR(50)    NULL,
        EMPPAY_CATEGORY                     NVARCHAR(50)    NULL,
        EMPPAY_DATE_JOINED                  DATETIME        NULL,
        EMPPAY_DATE_CONFIRM                 DATETIME        NULL,
        EMPPAY_DATE_PROMOTION               DATETIME        NULL,
        EMPPAY_DATE_RESIGNED                DATETIME        NULL,
        EMPPAY_BASIC_RATE                   FLOAT           NOT NULL DEFAULT(0),
        SALARYLAB                           DECIMAL(18,2)   NOT NULL DEFAULT(0),
        ATTENDANCEALLOWANCE                 DECIMAL(18,2)   NULL,
        NewStructureATTENDANCEALLOWANCE     DECIMAL(18,2)   NOT NULL DEFAULT(0),
        SpecialAllowance                    DECIMAL(18,2)   NOT NULL DEFAULT(0),
        AttendanceAllowanceWorkingDays      DECIMAL(18,2)   NULL,
        AttendanceAllowanceFollowCalendar   NVARCHAR(1)     NULL,
        LASTUPDATE                          DATETIME        NOT NULL,
        LastUpdatedBy                       NVARCHAR(20)    NULL,
        Emp_StartDate                       DATETIME        NULL,
        Emp_EndDate                         DATETIME        NULL
    );
    PRINT 'Table EmploymentDetailsHistory created.';
END
ELSE
    PRINT 'Table EmploymentDetailsHistory already exists.';
GO

SELECT 'Migration complete.';
GO
