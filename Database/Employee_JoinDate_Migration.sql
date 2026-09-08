-- =============================================
-- Employee JoinDate Column Migration
-- Date: 2026-09-02
-- Description: Adds JoinDate column to Employee and EmployeeHistory tables
--              to store the original date when employee joined the company
-- =============================================

-- Add JoinDate column to Employee table
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Employee]') 
               AND name = 'JoinDate')
BEGIN
    ALTER TABLE [dbo].[Employee]
    ADD [JoinDate] DATETIME NULL;
    
    PRINT 'JoinDate column added to Employee table';
END
ELSE
BEGIN
    PRINT 'JoinDate column already exists in Employee table';
END
GO

-- Add JoinDate column to EmployeeHistory table
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[EmployeeHistory]') 
               AND name = 'JoinDate')
BEGIN
    ALTER TABLE [dbo].[EmployeeHistory]
    ADD [JoinDate] DATETIME NULL;
    
    PRINT 'JoinDate column added to EmployeeHistory table';
END
ELSE
BEGIN
    PRINT 'JoinDate column already exists in EmployeeHistory table';
END
GO

-- Populate JoinDate for existing employees from EmploymentDetails.EMPPAY_DATE_JOINED
UPDATE e
SET e.JoinDate = ed.EMPPAY_DATE_JOINED
FROM [dbo].[Employee] e
INNER JOIN [dbo].[EmploymentDetails] ed ON e.EMP_CODE = ed.EMPPAY_CODE
WHERE e.JoinDate IS NULL
  AND ed.EMPPAY_DATE_JOINED IS NOT NULL;

PRINT 'JoinDate populated for existing employees from EmploymentDetails';
GO

-- Populate JoinDate for existing EmployeeHistory records from Employee.JoinDate
UPDATE eh
SET eh.JoinDate = e.JoinDate
FROM [dbo].[EmployeeHistory] eh
INNER JOIN [dbo].[Employee] e ON eh.EMP_ID = e.EMP_ID
WHERE eh.JoinDate IS NULL
  AND e.JoinDate IS NOT NULL;

PRINT 'JoinDate populated for existing EmployeeHistory records from Employee';
GO

-- Verify the migration
SELECT 
    'Employee' AS TableName,
    COUNT(*) AS TotalRecords,
    SUM(CASE WHEN JoinDate IS NOT NULL THEN 1 ELSE 0 END) AS RecordsWithJoinDate,
    SUM(CASE WHEN JoinDate IS NULL THEN 1 ELSE 0 END) AS RecordsWithoutJoinDate
FROM [dbo].[Employee]

UNION ALL

SELECT 
    'EmployeeHistory' AS TableName,
    COUNT(*) AS TotalRecords,
    SUM(CASE WHEN JoinDate IS NOT NULL THEN 1 ELSE 0 END) AS RecordsWithJoinDate,
    SUM(CASE WHEN JoinDate IS NULL THEN 1 ELSE 0 END) AS RecordsWithoutJoinDate
FROM [dbo].[EmployeeHistory];

PRINT 'Migration completed successfully!';
GO
