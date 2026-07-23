-- Check Agreement table structure
USE [obms];
GO

PRINT '=== Checking Agreement Table Structure ==='

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Agreement')
BEGIN
    PRINT 'Agreement table exists. Checking columns...'
    SELECT 
        COLUMN_NAME, 
        DATA_TYPE, 
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Agreement'
    ORDER BY ORDINAL_POSITION;
    
    -- Check for the missing columns specifically
    SELECT 
        'AddedDate' AS ColumnName,
        CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Agreement' AND COLUMN_NAME = 'AddedDate')
             THEN 'EXISTS' ELSE 'MISSING' END AS Status
    UNION ALL
    SELECT 
        'QuotationID' AS ColumnName,
        CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Agreement' AND COLUMN_NAME = 'QuotationID')
             THEN 'EXISTS' ELSE 'MISSING' END AS Status;
END
ELSE
BEGIN
    PRINT 'Agreement table does not exist.'
END

PRINT '=== Check Complete ==='
