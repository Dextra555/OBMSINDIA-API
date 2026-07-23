-- ============================================================
-- Add Father's Name Column to Employee Table
-- ============================================================

USE [obms];
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT 'Adding EMP_FATHER_NAME column to Employee table...'

-- Check if column already exists
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'EMP_FATHER_NAME'
)
BEGIN
    ALTER TABLE Employee
    ADD EMP_FATHER_NAME VARCHAR(50) NULL;
    
    PRINT 'EMP_FATHER_NAME column added successfully.'
END
ELSE
BEGIN
    PRINT 'EMP_FATHER_NAME column already exists. Skipping...'
END
GO

PRINT 'Script completed.'
GO
