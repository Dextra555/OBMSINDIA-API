-- Add OT (Overtime) column to EmployeeSalaryDetails table
-- Date: 2026-04-27
-- Description: This script adds the OT field to track whether an employee is eligible for overtime

USE OBMSDB;
GO

-- Check if column already exists
IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('EmployeeSalaryDetails') 
    AND name = 'OT'
)
BEGIN
    ALTER TABLE EmployeeSalaryDetails
    ADD OT NVARCHAR(10) NULL DEFAULT 'No';
    
    PRINT 'Column OT added successfully to EmployeeSalaryDetails table.';
END
ELSE
BEGIN
    PRINT 'Column OT already exists in EmployeeSalaryDetails table.';
END
GO

-- Update existing records to have default value 'No'
UPDATE EmployeeSalaryDetails
SET OT = 'No'
WHERE OT IS NULL;
GO

PRINT 'Existing records updated with default OT value.';
GO
