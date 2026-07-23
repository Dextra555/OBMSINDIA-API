-- Add PerDay column to ClientInvoiceDetails table
USE [obmsdev_backup]
GO

PRINT '=== Adding PerDay Column to ClientInvoiceDetails Table ==='

-- Check if column exists, if not add it
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientInvoiceDetails' AND COLUMN_NAME = 'PerDay')
BEGIN
    PRINT 'Adding PerDay column to ClientInvoiceDetails table...'
    ALTER TABLE ClientInvoiceDetails ADD PerDay DECIMAL(18,2) NOT NULL DEFAULT 0
    PRINT 'PerDay column added successfully.'
END
ELSE
BEGIN
    PRINT 'PerDay column already exists in ClientInvoiceDetails table.'
END

PRINT '=== PerDay Column Addition Complete ==='

-- Show table structure to verify
PRINT '=== ClientInvoiceDetails Table Structure (Partial) ==='
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ClientInvoiceDetails' 
AND COLUMN_NAME IN ('PerDay', 'Rate', 'NoOfHours')
ORDER BY ORDINAL_POSITION;
