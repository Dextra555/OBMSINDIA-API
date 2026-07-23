-- Fix FatherName column issue
-- This script ensures the EMP_FATHER_NAME column exists and has the correct name

USE [obmsdev_backup];
GO

PRINT 'Checking EMP_FATHER_NAME column...'

-- Check if the lowercase 'fathername' column exists (incorrect name)
IF EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'fathername'
)
BEGIN
    PRINT 'Found incorrect column name "fathername". Renaming to "EMP_FATHER_NAME"...'
    
    -- Rename the column to the correct name
    EXEC sp_rename 'Employee.fathername', 'EMP_FATHER_NAME', 'COLUMN';
    
    PRINT 'Column renamed successfully.'
END
-- Check if the correct column already exists
ELSE IF EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'EMP_FATHER_NAME'
)
BEGIN
    PRINT 'EMP_FATHER_NAME column already exists with correct name.'
END
-- If neither exists, add the column
ELSE
BEGIN
    PRINT 'EMP_FATHER_NAME column does not exist. Adding it...'
    
    ALTER TABLE Employee
    ADD EMP_FATHER_NAME VARCHAR(50) NULL;
    
    PRINT 'EMP_FATHER_NAME column added successfully.'
END
GO

-- Verify the column exists
PRINT 'Verifying column exists...'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Employee' 
AND COLUMN_NAME = 'EMP_FATHER_NAME';
GO

PRINT 'Script completed successfully.'
GO
