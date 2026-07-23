-- Add PerMonth column to ClientInvoiceDetails table
USE [obmsdev_backup]
GO

PRINT '=== Adding PerMonth Column to ClientInvoiceDetails Table ==='

-- Check if column exists, if not add it
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientInvoiceDetails' AND COLUMN_NAME = 'PerMonth')
BEGIN
    PRINT 'Adding PerMonth column to ClientInvoiceDetails table...'
    ALTER TABLE ClientInvoiceDetails ADD PerMonth DECIMAL(18,2) NOT NULL DEFAULT 0
    PRINT 'PerMonth column added successfully.'
END
ELSE
BEGIN
    PRINT 'PerMonth column already exists in ClientInvoiceDetails table.'
END

PRINT '=== PerMonth Column Addition Complete ==='

-- Show table structure to verify
PRINT '=== ClientInvoiceDetails Table Structure (Partial) ==='
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ClientInvoiceDetails' 
AND COLUMN_NAME IN ('PerDay', 'PerMonth', 'Rate', 'NoOfHours')
ORDER BY ORDINAL_POSITION;
