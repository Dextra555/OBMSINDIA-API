-- Add simplified CB (Commercial Breakdown) columns to Employee table
-- These columns store the Commercial Breakdown values from Salary Breakdown

USE OBMS;
GO

-- Add CB_Basic column
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_Basic'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_Basic DECIMAL(18, 2) NULL;

    PRINT 'Column CB_Basic added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_Basic already exists in Employee table.';
END
GO

-- Add CB_DA column
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_DA'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_DA DECIMAL(18, 2) NULL;

    PRINT 'Column CB_DA added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_DA already exists in Employee table.';
END
GO

-- Add CB_HRA column
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_HRA'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_HRA DECIMAL(18, 2) NULL;

    PRINT 'Column CB_HRA added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_HRA already exists in Employee table.';
END
GO

-- Add CB_HRAPercentage column
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_HRAPercentage'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_HRAPercentage DECIMAL(18, 2) NULL;

    PRINT 'Column CB_HRAPercentage added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_HRAPercentage already exists in Employee table.';
END
GO

-- Add CB_OtherAllowances column
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

-- Add CB_NH column (National Festival Allowance)
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_NH'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_NH DECIMAL(18, 2) NULL;

    PRINT 'Column CB_NH added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_NH already exists in Employee table.';
END
GO

-- Add CB_NHPercentage column
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_NHPercentage'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_NHPercentage DECIMAL(18, 2) NULL;

    PRINT 'Column CB_NHPercentage added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_NHPercentage already exists in Employee table.';
END
GO

-- Add CB_SubTotal column
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_SubTotal'
)
BEGIN
    ALTER TABLE Employee
    ADD CB_SubTotal DECIMAL(18, 2) NULL;

    PRINT 'Column CB_SubTotal added successfully to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_SubTotal already exists in Employee table.';
END
GO

PRINT '';
PRINT '============================================';
PRINT 'Simplified CB columns added successfully.';
PRINT '============================================';
