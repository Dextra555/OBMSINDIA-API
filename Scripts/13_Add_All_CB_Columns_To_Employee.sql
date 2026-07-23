-- Add all 7 simplified CB (Commercial Breakdown) columns to Employee table
-- This script creates the necessary columns for storing Commercial Breakdown data
-- Run this script against your OBMS database

USE OBMS;
GO

PRINT '============================================';
PRINT 'Adding CB Columns to Employee Table';
PRINT '============================================';
PRINT '';

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
    PRINT '✓ Column CB_Basic added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column CB_Basic already exists';
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
    PRINT '✓ Column CB_DA added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column CB_DA already exists';
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
    PRINT '✓ Column CB_HRA added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column CB_HRA already exists';
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
    PRINT '✓ Column CB_HRAPercentage added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column CB_HRAPercentage already exists';
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
    PRINT '✓ Column CB_OtherAllowances added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column CB_OtherAllowances already exists';
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
    PRINT '✓ Column CB_NH added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column CB_NH already exists';
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
    PRINT '✓ Column CB_NHPercentage added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column CB_NHPercentage already exists';
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
    PRINT '✓ Column CB_SubTotal added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column CB_SubTotal already exists';
END
GO

PRINT '';
PRINT '============================================';
PRINT 'All CB columns added/verified successfully';
PRINT '============================================';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Restart the API application';
PRINT '2. Test employee creation with Commercial Breakdown';
PRINT '3. Verify data is saved and retrieved correctly';
PRINT '============================================';
