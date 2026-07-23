-- Add CB_Bonus, CB_BonusPercentage, CB_Advance, CB_AdvancePercentage columns to Employee table
-- This script adds the advance statutory bonus and advance payment columns to the Commercial Breakdown structure

USE OBMS;
GO

-- Add CB_Bonus column (Statutory Bonus amount)
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_Bonus'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_Bonus DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_Bonus added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_Bonus already exists in Employee table.';
END
GO

-- Add CB_BonusPercentage column (Statutory Bonus percentage)
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_BonusPercentage'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_BonusPercentage DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_BonusPercentage added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_BonusPercentage already exists in Employee table.';
END
GO

-- Add CB_Advance column (Advance Payment amount)
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_Advance'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_Advance DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_Advance added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_Advance already exists in Employee table.';
END
GO

-- Add CB_AdvancePercentage column (Advance Payment percentage)
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_AdvancePercentage'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_AdvancePercentage DECIMAL(18, 2) NULL;
    
    PRINT 'Column CB_AdvancePercentage added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_AdvancePercentage already exists in Employee table.';
END
GO

PRINT 'CB_Bonus and CB_Advance columns script completed.';
