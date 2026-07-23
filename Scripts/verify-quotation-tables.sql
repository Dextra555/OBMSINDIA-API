-- Verify Quotation tables exist and show their structure
USE [obms];
GO

PRINT '=== Checking for Quotation Tables ==='

-- Check if tables exist
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Quotation') 
        THEN 'Quotation table EXISTS' 
        ELSE 'Quotation table MISSING' 
    END AS QuotationStatus,
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QuotationDetails') 
        THEN 'QuotationDetails table EXISTS' 
        ELSE 'QuotationDetails table MISSING' 
    END AS QuotationDetailsStatus;

-- Show Quotation table structure if it exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Quotation')
BEGIN
    PRINT '=== Quotation Table Structure ==='
    SELECT 
        COLUMN_NAME, 
        DATA_TYPE, 
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Quotation'
    ORDER BY ORDINAL_POSITION;
END

-- Show QuotationDetails table structure if it exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QuotationDetails')
BEGIN
    PRINT '=== QuotationDetails Table Structure ==='
    SELECT TOP 10
        COLUMN_NAME, 
        DATA_TYPE, 
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'QuotationDetails'
    ORDER BY ORDINAL_POSITION;
END

PRINT '=== Verification Complete ==='
