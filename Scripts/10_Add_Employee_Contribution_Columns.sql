-- Add Employee Contribution columns to Employee table
-- These columns store employee PF and ESI contributions as per Indian statutory requirements

USE OBMS;
GO

-- Add Employee PF Contribution column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_EmployeePF'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_EmployeePF DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_EmployeePF added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_EmployeePF already exists in Employee table.';
END
GO

-- Add Employee PF Percentage column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_EmployeePFPercentage'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_EmployeePFPercentage DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_EmployeePFPercentage added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_EmployeePFPercentage already exists in Employee table.';
END
GO

-- Add Employee ESI Contribution column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_EmployeeESI'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_EmployeeESI DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_EmployeeESI added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_EmployeeESI already exists in Employee table.';
END
GO

-- Add Employee ESI Percentage column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_EmployeeESIPercentage'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_EmployeeESIPercentage DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_EmployeeESIPercentage added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_EmployeeESIPercentage already exists in Employee table.';
END
GO
