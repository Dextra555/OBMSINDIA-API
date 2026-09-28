-- ============================================================
-- Migration: Change AgreementDetails.DiscountHour from INT to DECIMAL(10,2)
-- Reason:    Supports fractional discount days (e.g. 0.5)
-- Date:      2026-09-24
-- ============================================================

-- Step 1: Check current column type (informational)
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    NUMERIC_PRECISION,
    NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AgreementDetails'
  AND COLUMN_NAME = 'DiscountHour';

-- Step 2: Alter the column
-- Safe to run: INT values (e.g. 0, 1, 2) convert cleanly to DECIMAL(10,2)
ALTER TABLE AgreementDetails
    ALTER COLUMN DiscountHour DECIMAL(10, 2) NOT NULL;

-- Step 3: Verify the change
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    NUMERIC_PRECISION,
    NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AgreementDetails'
  AND COLUMN_NAME = 'DiscountHour';
