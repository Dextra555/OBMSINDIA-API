-- Add CB_OtherAllowances column to Employee table
-- This column stores the Other Allowances value from the Salary Breakdown - Commercial Details

USE OBMS;
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_OtherAllowances'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_OtherAllowances DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_OtherAllowances added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_OtherAllowances already exists in Employee table.';
END
GO
