-- Check AgreementDetails table structure
USE [obms];
GO

PRINT '=== Checking AgreementDetails Table Structure ==='

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AgreementDetails')
BEGIN
    PRINT 'AgreementDetails table exists. Checking columns...'
    SELECT TOP 15
        COLUMN_NAME, 
        DATA_TYPE, 
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AgreementDetails'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'AgreementDetails table does not exist.'
END

PRINT '=== Check Complete ==='
