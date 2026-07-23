-- Check if simplified CB columns exist in Employee table
USE OBMS;
GO

-- Check CB_Basic column
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_Basic'
)
BEGIN
    PRINT 'Column CB_Basic EXISTS in Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_Basic DOES NOT EXIST in Employee table.';
END
GO

-- Check CB_DA column
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_DA'
)
BEGIN
    PRINT 'Column CB_DA EXISTS in Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_DA DOES NOT EXIST in Employee table.';
END
GO

-- Check CB_HRA column
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_HRA'
)
BEGIN
    PRINT 'Column CB_HRA EXISTS in Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_HRA DOES NOT EXIST in Employee table.';
END
GO

-- Check CB_HRAPercentage column
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_HRAPercentage'
)
BEGIN
    PRINT 'Column CB_HRAPercentage EXISTS in Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_HRAPercentage DOES NOT EXIST in Employee table.';
END
GO

-- Check CB_OtherAllowances column
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_OtherAllowances'
)
BEGIN
    PRINT 'Column CB_OtherAllowances EXISTS in Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_OtherAllowances DOES NOT EXIST in Employee table.';
END
GO

-- Check CB_NH column
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_NH'
)
BEGIN
    PRINT 'Column CB_NH EXISTS in Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_NH DOES NOT EXIST in Employee table.';
END
GO

-- Check CB_NHPercentage column
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_NHPercentage'
)
BEGIN
    PRINT 'Column CB_NHPercentage EXISTS in Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_NHPercentage DOES NOT EXIST in Employee table.';
END
GO

-- Check CB_SubTotal column
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee'
    AND COLUMN_NAME = 'CB_SubTotal'
)
BEGIN
    PRINT 'Column CB_SubTotal EXISTS in Employee table.';
END
ELSE
BEGIN
    PRINT 'Column CB_SubTotal DOES NOT EXIST in Employee table.';
END
GO

PRINT '';
PRINT '============================================';
PRINT 'If any columns are missing, create them manually';
PRINT '============================================';
