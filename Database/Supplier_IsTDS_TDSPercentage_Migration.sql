-- ============================================================
-- Migration: Add IsTDS, TDSPercentage columns to Suppliers table
-- Date: 2026-10-28
-- Description: Supports "Is TDS" checkbox on Add/Edit Supplier page.
--              When IsTDS = 1, TDSPercentage stores the selected rate
--              from TDS Master (e.g. 1.00, 2.00, 10.00).
-- ============================================================

-- Step 1: Add IsTDS column (default 0 = TDS not applicable)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'IsTDS'
)
BEGIN
    ALTER TABLE Suppliers
    ADD IsTDS BIT NOT NULL DEFAULT 0;
    PRINT 'Column IsTDS added to Suppliers table.';
END
ELSE
BEGIN
    PRINT 'Column IsTDS already exists in Suppliers table. Skipped.';
END

-- Step 2: Add TDSPercentage column (nullable decimal)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'TDSPercentage'
)
BEGIN
    ALTER TABLE Suppliers
    ADD TDSPercentage DECIMAL(5, 2) NULL;
    PRINT 'Column TDSPercentage added to Suppliers table.';
END
ELSE
BEGIN
    PRINT 'Column TDSPercentage already exists in Suppliers table. Skipped.';
END

-- Verify all new columns together
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Suppliers'
  AND COLUMN_NAME IN ('IsTax', 'GSTNo', 'PanNo', 'IsTDS', 'TDSPercentage');
