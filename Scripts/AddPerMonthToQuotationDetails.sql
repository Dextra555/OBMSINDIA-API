-- Add PerDay and PerMonth columns to QuotationDetails table
USE [obmsdev_backup]
GO

PRINT '=== Adding PerDay Column to QuotationDetails Table ==='

-- Check if PerDay column exists, if not add it
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuotationDetails' AND COLUMN_NAME = 'PerDay')
BEGIN
    PRINT 'Adding PerDay column to QuotationDetails table...'
    ALTER TABLE QuotationDetails ADD PerDay DECIMAL(18,2) NOT NULL DEFAULT 0
    PRINT 'PerDay column added successfully.'
END
ELSE
BEGIN
    PRINT 'PerDay column already exists in QuotationDetails table.'
END

PRINT '=== Adding PerMonth Column to QuotationDetails Table ==='

-- Check if PerMonth column exists, if not add it
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuotationDetails' AND COLUMN_NAME = 'PerMonth')
BEGIN
    PRINT 'Adding PerMonth column to QuotationDetails table...'
    ALTER TABLE QuotationDetails ADD PerMonth DECIMAL(18,2) NOT NULL DEFAULT 0
    PRINT 'PerMonth column added successfully.'
END
ELSE
BEGIN
    PRINT 'PerMonth column already exists in QuotationDetails table.'
END

PRINT '=== Columns Addition Complete ==='

-- Show table structure to verify
PRINT '=== QuotationDetails Table Structure (Partial) ==='
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'QuotationDetails' 
AND COLUMN_NAME IN ('PerDay', 'PerMonth', 'Rate', 'NoOfHours')
ORDER BY ORDINAL_POSITION;
