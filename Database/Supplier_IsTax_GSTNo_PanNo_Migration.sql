-- ============================================================
-- Migration: Add IsTax, GSTNo, PanNo columns to Suppliers table
-- Date: 2026-10-28
-- Description: Supports "Is Tax" checkbox on Add/Edit Supplier page.
--              When IsTax = 1, GST No and PAN No are captured.
-- ============================================================

-- Step 1: Add IsTax column (default 0 = not taxable)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'IsTax'
)
BEGIN
    ALTER TABLE Suppliers
    ADD IsTax BIT NOT NULL DEFAULT 0;
    PRINT 'Column IsTax added to Suppliers table.';
END
ELSE
BEGIN
    PRINT 'Column IsTax already exists in Suppliers table. Skipped.';
END

-- Step 2: Add GSTNo column
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'GSTNo'
)
BEGIN
    ALTER TABLE Suppliers
    ADD GSTNo NVARCHAR(20) NULL;
    PRINT 'Column GSTNo added to Suppliers table.';
END
ELSE
BEGIN
    PRINT 'Column GSTNo already exists in Suppliers table. Skipped.';
END

-- Step 3: Add PanNo column
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'PanNo'
)
BEGIN
    ALTER TABLE Suppliers
    ADD PanNo NVARCHAR(20) NULL;
    PRINT 'Column PanNo added to Suppliers table.';
END
ELSE
BEGIN
    PRINT 'Column PanNo already exists in Suppliers table. Skipped.';
END

-- Verify
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Suppliers'
  AND COLUMN_NAME IN ('IsTax', 'GSTNo', 'PanNo');
