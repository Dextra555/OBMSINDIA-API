-- Verify and add EMP_FATHER_NAME column if it doesn't exist
USE [obmsdev_backup];
GO

-- Check if column exists
IF EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'EMP_FATHER_NAME'
)
BEGIN
    PRINT 'EMP_FATHER_NAME column exists.'
    
    -- Show column details
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'EMP_FATHER_NAME'
END
ELSE
BEGIN
    PRINT 'EMP_FATHER_NAME column does not exist. Adding it...'
    
    -- Add the column
    ALTER TABLE Employee
    ADD EMP_FATHER_NAME VARCHAR(50) NULL;
    
    PRINT 'EMP_FATHER_NAME column added successfully.'
END
GO

-- Test query to verify
SELECT TOP 5 EMP_ID, EMP_NAME, EMP_FATHER_NAME 
FROM Employee 
WHERE EMP_FATHER_NAME IS NOT NULL
ORDER BY EMP_ID;
GO
