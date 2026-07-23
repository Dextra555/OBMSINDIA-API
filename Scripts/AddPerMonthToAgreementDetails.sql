-- Add PerMonth column to AgreementDetails table
USE [obmsdev_backup]
GO

PRINT '=== Adding PerMonth Column to AgreementDetails Table ==='

-- Check if column exists, if not add it
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AgreementDetails' AND COLUMN_NAME = 'PerMonth')
BEGIN
    PRINT 'Adding PerMonth column to AgreementDetails table...'
    ALTER TABLE AgreementDetails ADD PerMonth DECIMAL(18,2) NOT NULL DEFAULT 0
    PRINT 'PerMonth column added successfully.'
END
ELSE
BEGIN
    PRINT 'PerMonth column already exists in AgreementDetails table.'
END

PRINT '=== PerMonth Column Addition Complete ==='

-- Show table structure to verify
PRINT '=== AgreementDetails Table Structure (Partial) ==='
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AgreementDetails' 
AND COLUMN_NAME IN ('PerDay', 'PerMonth', 'Rate', 'NoOfHours')
ORDER BY ORDINAL_POSITION;
