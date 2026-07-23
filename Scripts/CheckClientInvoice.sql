-- Check ClientInvoice table structure
USE [obms];
GO

PRINT '=== Checking ClientInvoice Table Structure ==='

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClientInvoice')
BEGIN
    SELECT 
        COLUMN_NAME, 
        DATA_TYPE, 
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ClientInvoice'
    ORDER BY ORDINAL_POSITION;
    
    -- Check for primary key
    SELECT 
        COLUMN_NAME,
        CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
    WHERE TABLE_NAME = 'ClientInvoice'
    AND OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_NAME), 'IsPrimaryKey') = 1;
END
ELSE
BEGIN
    PRINT 'ClientInvoice table does not exist.'
END

PRINT '=== Check Complete ==='
