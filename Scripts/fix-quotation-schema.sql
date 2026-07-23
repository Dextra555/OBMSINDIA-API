-- Fix missing columns in Quotation table
-- This script adds AddedDate column if it doesn't exist

-- Check if AddedDate column exists in Quotation table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Quotation' 
    AND COLUMN_NAME = 'AddedDate'
)
BEGIN
    PRINT 'Adding AddedDate column to Quotation table...'
    ALTER TABLE Quotation ADD AddedDate DATETIME NULL
    PRINT 'AddedDate column added successfully.'
END
ELSE
BEGIN
    PRINT 'AddedDate column already exists in Quotation table.'
END

-- Check if QuotationID column exists in QuotationDetails table (it should based on the model)
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'QuotationDetails' 
    AND COLUMN_NAME = 'QuotationID'
)
BEGIN
    PRINT 'Adding QuotationID column to QuotationDetails table...'
    ALTER TABLE QuotationDetails ADD QuotationID INT NOT NULL DEFAULT 0
    PRINT 'QuotationID column added successfully.'
END
ELSE
BEGIN
    PRINT 'QuotationID column already exists in QuotationDetails table.'
END

-- Display current Quotation table structure
PRINT 'Current Quotation table structure:'
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Quotation'
ORDER BY ORDINAL_POSITION

-- Display current QuotationDetails table structure  
PRINT 'Current QuotationDetails table structure:'
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'QuotationDetails'
ORDER BY ORDINAL_POSITION
