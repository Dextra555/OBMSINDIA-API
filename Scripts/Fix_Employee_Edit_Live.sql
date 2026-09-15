-- ============================================================
-- FIX (COMPREHENSIVE v2): EmployeeById returns HTTP 500 on LIVE
-- ============================================================
-- Why the error: GetEmployeeById selects the FULL Employee,
-- EmploymentDetails and EmployeeSalaryDetails entities. If the
-- live DB is missing ANY column these models map, SQL Server
-- throws "Invalid column name 'X'" -> the API returns 500 and
-- the edit page stays blank.
--
-- This script:
--   * scans the expected columns below against the live tables
--   * ADDS every missing column (idempotent / safe to rerun)
--   * creates missing history tables (EmployeeSalaryDetailHistory,
--     EmploymentDetailsHistory) if they do not exist
--   * prints exactly what it added
-- It never drops or changes existing columns/data.
--
-- HOW TO USE:
--   1. SSMS -> connect to LIVE SQL
--   2. Change "USE [obmsindia]" if your live DB has another name
--   3. Run
--   4. Restart API (iisreset, or recycle the app pool on IIS)
--   5. Test: http://124.217.236.37:64400/api/Employee/EmployeeById?employeeId=314
--      -> must return HTTP 200
-- ============================================================

USE [obmsindia];
GO
SET NOCOUNT ON;
GO

IF OBJECT_ID('tempdb..#ExpectedCols') IS NOT NULL DROP TABLE #ExpectedCols;
CREATE TABLE #ExpectedCols (
    tbl sysname NOT NULL,
    col sysname NOT NULL,
    coltype sysname NOT NULL,
    ddlsuffix varchar(200) NULL
);

-- ========== Employee table ==========
INSERT #ExpectedCols (tbl, col, coltype, ddlsuffix) VALUES
('Employee','EMP_FATHER_NAME','VARCHAR(50)',NULL),
('Employee','DepartmentId','INT',NULL),
('Employee','DesignationId','INT',NULL),
('Employee','JoinDate','DATETIME',NULL),
('Employee','OldBranch','VARCHAR(20)',NULL),
('Employee','TransferDate','DATETIME',NULL),
('Employee','HasTransfered','BIT','NOT NULL DEFAULT 0'),
('Employee','NewSalaryStructure','CHAR(1)','NOT NULL DEFAULT ''N'''),
('Employee','SalaryStructure1000_3h','CHAR(1)',NULL),
('Employee','KDNVetting','BIT','NOT NULL DEFAULT 0'),
('Employee','LastUpdatedBy','VARCHAR(20)',NULL),
('Employee','EMP_IC_OLD','VARCHAR(20)',NULL),
('Employee','EMP_IC_NEW','VARCHAR(20)',NULL),
('Employee','EMP_IC_COLOR','VARCHAR(25)',NULL),
('Employee','EMP_PASSPORT_NO','VARCHAR(30)',NULL),
('Employee','EMP_RACE','VARCHAR(25)',NULL),
('Employee','EMP_SPOUSE_NAME','VARCHAR(50)',NULL),
('Employee','EMP_SP_IC','VARCHAR(20)',NULL),
('Employee','EMP_PER_NAME_CONTACT','VARCHAR(50)',NULL),
('Employee','EMP_CONTACT_ADDRESS1','VARCHAR(100)',NULL),
('Employee','EMP_CONTACT_ADDRESS2','VARCHAR(100)',NULL),
('Employee','EMP_CONTACT_POST_CODE','VARCHAR(5)',NULL),
('Employee','EMP_CONTACT_TOWN','VARCHAR(50)',NULL),
('Employee','EMP_CONTACT_STATE','VARCHAR(30)',NULL),
('Employee','EMP_CONTACT_TELEPHONE','VARCHAR(20)',NULL),
('Employee','EMP_SP_WORK','BIT','NOT NULL DEFAULT 0'),
('Employee','EMP_NO_CHILD','INT',NULL),
('Employee','EMP_CITIZEN','INT','NOT NULL DEFAULT 0'),
('Employee','EMP_CHECKLIST','INT','NOT NULL DEFAULT 0'),
('Employee','EMP_TOWN','VARCHAR(50)',NULL),
('Employee','EMP_STATE','VARCHAR(30)',NULL),
('Employee','EMP_NATIONAL','VARCHAR(50)',NULL),
('Employee','EMPPAY_JOIN_DATE','DATETIME',NULL),
('Employee','Aadhaar','NVARCHAR(12)',NULL),
('Employee','PAN','NVARCHAR(10)',NULL),
('Employee','PF_AccountNumber','NVARCHAR(26)',NULL),
('Employee','ESI_Number','NVARCHAR(17)',NULL),
('Employee','SalaryGroup','NVARCHAR(20)',NULL),
('Employee','SpousePAN','NVARCHAR(10)',NULL),
('Employee','SpouseAadhaar','NVARCHAR(12)',NULL),
('Employee','IndianState','NVARCHAR(50)',NULL),
('Employee','IFSC_Code','NVARCHAR(11)',NULL),
('Employee','UPIId','NVARCHAR(50)',NULL),
('Employee','BankBranch','NVARCHAR(50)',NULL),
('Employee','BankAccountNumber','NVARCHAR(34)',NULL),
('Employee','BankName','NVARCHAR(100)',NULL),
('Employee','ProfessionalTax_State','NVARCHAR(50)',NULL),
('Employee','UAN_Number','NVARCHAR(50)',NULL),
('Employee','EMP_BASIC_RATE','DECIMAL(18,2)','NOT NULL DEFAULT 0'),
('Employee','CB_Basic','DECIMAL(18,2)',NULL),
('Employee','CB_DA','DECIMAL(18,2)',NULL),
('Employee','CB_HRA','DECIMAL(18,2)',NULL),
('Employee','CB_HRAPercentage','DECIMAL(18,2)',NULL),
('Employee','CB_Leaves','DECIMAL(18,2)',NULL),
('Employee','CB_LeavesPercentage','DECIMAL(18,2)',NULL),
('Employee','CB_OtherAllowances','DECIMAL(18,2)',NULL),
('Employee','CB_NH','DECIMAL(18,2)',NULL),
('Employee','CB_NHPercentage','DECIMAL(18,2)',NULL),
('Employee','CB_AdvanceStatutoryBonus','DECIMAL(18,2)',NULL),
('Employee','CB_AdvanceStatutoryBonusPercentage','DECIMAL(18,2)',NULL),
('Employee','CB_SubTotal','DECIMAL(18,2)',NULL);

-- ========== EmploymentDetails table ==========
INSERT #ExpectedCols (tbl, col, coltype, ddlsuffix) VALUES
('EmploymentDetails','EMPPAY_BRANCHCODE','VARCHAR(20)',NULL),
('EmploymentDetails','EMPPAY_JOB_TITLE','VARCHAR(50)',NULL),
('EmploymentDetails','EMPPAY_CATEGORY','VARCHAR(50)',NULL),
('EmploymentDetails','EMPPAY_DATE_CONFIRM','DATETIME',NULL),
('EmploymentDetails','EMPPAY_DATE_PROMOTION','DATETIME',NULL),
('EmploymentDetails','EMPPAY_DATE_RESIGNED','DATETIME',NULL),
('EmploymentDetails','SALARYLAB','DECIMAL(18,2)','NOT NULL DEFAULT 0'),
('EmploymentDetails','ATTENDANCEALLOWANCE','DECIMAL(18,2)',NULL),
('EmploymentDetails','NewStructureATTENDANCEALLOWANCE','DECIMAL(18,2)','NOT NULL DEFAULT 0'),
('EmploymentDetails','SpecialAllowance','DECIMAL(18,2)','NOT NULL DEFAULT 0'),
('EmploymentDetails','LastUpdatedBy','VARCHAR(20)',NULL),
('EmploymentDetails','AttendanceAllowanceWorkingDays','DECIMAL(18,2)',NULL),
('EmploymentDetails','AttendanceAllowanceFollowCalendar','VARCHAR(1)','NOT NULL DEFAULT ''N''');

-- ========== EmployeeHistory table ==========
-- Model EmployeeHistory.cs maps JoinDate. When the API inserts a
-- history snapshot (SaveEmployee) EF writes every mapped column, so
-- a missing column here breaks SAVE as well as the 500 on edit.
INSERT #ExpectedCols (tbl, col, coltype, ddlsuffix) VALUES
('EmployeeHistory','JoinDate','DATETIME',NULL);

-- ========== EmployeeHistory table (extra columns for branch-transfer tracking) ==========
-- The save logic inserts EmployeeHistory rows. If Emp_StartDate/Emp_EndDate
-- are missing, the SAVE itself fails (not just the edit page load).
INSERT #ExpectedCols (tbl, col, coltype, ddlsuffix) VALUES
('EmployeeHistory','Emp_StartDate','DATETIME',NULL),
('EmployeeHistory','Emp_EndDate','DATETIME',NULL);

-- ========== EmployeeSalaryDetails table ==========
INSERT #ExpectedCols (tbl, col, coltype, ddlsuffix) VALUES
('EmployeeSalaryDetails','EMPFL_CODE','VARCHAR(50)',NULL),
('EmployeeSalaryDetails','EMPFL_BRANCHCODE','VARCHAR(20)',NULL),
('EmployeeSalaryDetails','EMPFL_BANK','VARCHAR(50)',NULL),
('EmployeeSalaryDetails','EMPFL_BK_ACCNO','VARCHAR(25)',NULL),
('EmployeeSalaryDetails','EMPFL_TAX_NO','VARCHAR(25)',NULL),
('EmployeeSalaryDetails','EMPFL_EPFNO','VARCHAR(25)',NULL),
('EmployeeSalaryDetails','EMPFL_EPF8Pa','BIT',NULL),
('EmployeeSalaryDetails','EMPFL_SOSCO_NO','VARCHAR(25)',NULL),
('EmployeeSalaryDetails','EPFDETECT','BIT','NOT NULL DEFAULT 0'),
('EmployeeSalaryDetails','PAYMODE','VARCHAR(20)','NOT NULL DEFAULT ''Bank'''),
('EmployeeSalaryDetails','SOCSODETECT','BIT','NOT NULL DEFAULT 0'),
('EmployeeSalaryDetails','TMPGUARD','BIT','NOT NULL DEFAULT 0'),
('EmployeeSalaryDetails','DETECTBYND55','BIT','NOT NULL DEFAULT 0'),
('EmployeeSalaryDetails','LastUpdatedBy','VARCHAR(20)',NULL),
('EmployeeSalaryDetails','INCOMETAXDETECT','BIT',NULL),
('EmployeeSalaryDetails','EMP_SP_TEL_NO','VARCHAR(20)',NULL),
('EmployeeSalaryDetails','OT','VARCHAR(10)',NULL);
GO

-- ========== Auto-add missing columns ==========
DECLARE @tbl sysname, @col sysname, @type sysname, @suffix varchar(200), @sql nvarchar(max), @already int;
DECLARE cur CURSOR LOCAL FOR
    SELECT e.tbl, e.col, e.coltype, ISNULL(e.ddlsuffix,'')
    FROM #ExpectedCols e
    WHERE NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
        WHERE c.TABLE_NAME = e.tbl AND c.COLUMN_NAME = e.col
    )
    ORDER BY e.tbl, e.col;

OPEN cur;
FETCH NEXT FROM cur INTO @tbl, @col, @type, @suffix;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'ALTER TABLE ' + QUOTENAME(@tbl) + N' ADD ' + QUOTENAME(@col) + N' ' + @type +
               (CASE WHEN LEN(@suffix) > 0 THEN N' ' + @suffix ELSE N' NULL' END);
    BEGIN TRY
        EXEC sp_executesql @sql;
        PRINT 'ADDED  ' + @tbl + '.' + @col + '  [' + @type + ']';
    END TRY
    BEGIN CATCH
        PRINT 'SKIP   ' + @tbl + '.' + @col + '  -> ' + ERROR_MESSAGE();
    END CATCH
    FETCH NEXT FROM cur INTO @tbl, @col, @type, @suffix;
END
CLOSE cur;
DEALLOCATE cur;
GO

IF OBJECT_ID('tempdb..#ExpectedCols') IS NOT NULL DROP TABLE #ExpectedCols;
GO

-- ========== Ensure Department / Designation tables exist ==========
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Department')
BEGIN
    CREATE TABLE Department (
        DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
        Dept_Code NVARCHAR(100) NOT NULL,
        Dept_Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(50) NULL,
        UpdatedDate DATETIME NULL,
        UpdatedBy NVARCHAR(50) NULL
    );
    PRINT 'CREATE Department table';
END
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Designation')
BEGIN
    CREATE TABLE Designation (
        DesignationId INT IDENTITY(1,1) PRIMARY KEY,
        Desig_Code NVARCHAR(100) NOT NULL,
        Desig_Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(50) NULL,
        UpdatedDate DATETIME NULL,
        UpdatedBy NVARCHAR(50) NULL,
        DepartmentId INT NULL
    );
    PRINT 'CREATE Designation table';
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Designation_Department' AND parent_object_id=OBJECT_ID('Designation'))
    ALTER TABLE Designation ADD CONSTRAINT FK_Designation_Department FOREIGN KEY (DepartmentId) REFERENCES Department(DepartmentId);
GO

-- ========== Ensure EmployeeSalaryDetailHistory table exists ==========
-- The save logic inserts history snapshots for branch-transfer tracking.
-- If this table is missing, SAVE fails even for non-branch-change edits.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='EmployeeSalaryDetailHistory')
BEGIN
    CREATE TABLE EmployeeSalaryDetailHistory (
        EMPFL_HISTORY_ID INT IDENTITY(1,1) PRIMARY KEY,
        EMP_ID INT NOT NULL,
        EMPFL_CODE VARCHAR(50) NULL,
        EMPFL_BRANCHCODE VARCHAR(20) NULL,
        EMPFL_BANK VARCHAR(50) NULL,
        EMPFL_BK_ACCNO VARCHAR(25) NULL,
        EMPFL_TAX_NO VARCHAR(25) NULL,
        EMPFL_EPFNO VARCHAR(25) NULL,
        EMPFL_EPF8Pa BIT NULL,
        EMPFL_SOSCO_NO VARCHAR(25) NULL,
        EPFDETECT BIT NOT NULL DEFAULT 0,
        PAYMODE VARCHAR(20) NOT NULL DEFAULT 'Bank',
        SOCSODETECT BIT NOT NULL DEFAULT 0,
        TMPGUARD BIT NOT NULL DEFAULT 0,
        DETECTBYND55 BIT NOT NULL DEFAULT 0,
        LASTUPDATE DATETIME NOT NULL DEFAULT GETDATE(),
        LastUpdatedBy VARCHAR(20) NULL,
        INCOMETAXDETECT BIT NULL,
        EMP_SP_TEL_NO VARCHAR(20) NULL,
        OT VARCHAR(10) NULL,
        Emp_StartDate DATETIME NULL,
        Emp_EndDate DATETIME NULL
    );
    PRINT 'CREATE EmployeeSalaryDetailHistory table';
END
GO

-- ========== Ensure EmploymentDetailsHistory table exists ==========
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='EmploymentDetailsHistory')
BEGIN
    CREATE TABLE EmploymentDetailsHistory (
        EMPPAY_HISTORY_ID INT IDENTITY(1,1) PRIMARY KEY,
        EMP_ID INT NOT NULL,
        EMPPAY_CODE VARCHAR(50) NULL,
        EMPPAY_BRANCHCODE VARCHAR(20) NULL,
        EMPPAY_JOB_TITLE VARCHAR(50) NULL,
        EMPPAY_CATEGORY VARCHAR(50) NULL,
        EMPPAY_DATE_JOINED DATETIME NULL,
        EMPPAY_DATE_CONFIRM DATETIME NULL,
        EMPPAY_DATE_PROMOTION DATETIME NULL,
        EMPPAY_DATE_RESIGNED DATETIME NULL,
        EMPPAY_BASIC_RATE FLOAT NOT NULL DEFAULT 0,
        SALARYLAB DECIMAL(18,2) NOT NULL DEFAULT 0,
        ATTENDANCEALLOWANCE DECIMAL(18,2) NULL,
        NewStructureATTENDANCEALLOWANCE DECIMAL(18,2) NOT NULL DEFAULT 0,
        SpecialAllowance DECIMAL(18,2) NOT NULL DEFAULT 0,
        AttendanceAllowanceWorkingDays DECIMAL(18,2) NULL,
        AttendanceAllowanceFollowCalendar VARCHAR(1) NOT NULL DEFAULT 'N',
        LASTUPDATE DATETIME NOT NULL DEFAULT GETDATE(),
        LastUpdatedBy VARCHAR(20) NULL,
        Emp_StartDate DATETIME NULL,
        Emp_EndDate DATETIME NULL
    );
    PRINT 'CREATE EmploymentDetailsHistory table';
END
GO

PRINT '';
PRINT '============================================'
PRINT 'DONE. Above ADDED lines = columns the live DB was missing.'
PRINT 'Nothing printed before DONE = everything already existed.'
PRINT 'Next: restart the API, then test:'
PRINT '  http://124.217.236.37:64400/api/Employee/EmployeeById?employeeId=314'
PRINT '  (must return HTTP 200)'
PRINT '============================================'
GO