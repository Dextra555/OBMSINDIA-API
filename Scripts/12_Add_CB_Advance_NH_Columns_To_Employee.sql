-- Add CB_NFH column to Employee table
-- This script adds the CB_NFH column (National Festival Holiday allowance)

USE OBMS;
GO

-- Add CB_NFH column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_NFH'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_NFH DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_NFH added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_NFH already exists in Employee table.';
END
GO

PRINT 'CB_NFH column script completed.';
