-- =============================================================================
-- Migration: Replace single GSTPercentage/GSTAmount with CGST + SGST split
--            on CreditorInvoice table (Expenses page)
-- Date: 2026-10-20
-- Description: Adds CGSTPercentage, CGSTAmount, SGSTPercentage, SGSTAmount.
--              Existing GSTPercentage / GSTAmount columns are kept for backward
--              compatibility (Finance payments still references GSTAmount).
--              Backfills CGST/SGST as 50/50 split of any existing GSTAmount.
-- =============================================================================

-- 1. CGSTPercentage
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'CGSTPercentage'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD CGSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Column CGSTPercentage added.';
END
ELSE
    PRINT 'Column CGSTPercentage already exists — skipped.';
GO

-- 2. CGSTAmount
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'CGSTAmount'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD CGSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Column CGSTAmount added.';
END
ELSE
    PRINT 'Column CGSTAmount already exists — skipped.';
GO

-- 3. SGSTPercentage
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'SGSTPercentage'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD SGSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Column SGSTPercentage added.';
END
ELSE
    PRINT 'Column SGSTPercentage already exists — skipped.';
GO

-- 4. SGSTAmount
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'SGSTAmount'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD SGSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Column SGSTAmount added.';
END
ELSE
    PRINT 'Column SGSTAmount already exists — skipped.';
GO

-- 5. Backfill: split existing GSTAmount 50/50 into CGST + SGST
--    and split GSTPercentage 50/50 into CGST% + SGST%
UPDATE CreditorInvoice
SET
    CGSTPercentage = GSTPercentage / 2,
    CGSTAmount     = GSTAmount / 2,
    SGSTPercentage = GSTPercentage / 2,
    SGSTAmount     = GSTAmount / 2
WHERE GSTAmount > 0
  AND CGSTAmount = 0
  AND SGSTAmount = 0;
PRINT 'Backfill of CGST/SGST from existing GSTAmount complete.';
GO

-- 6. Verify
SELECT
    name,
    TYPE_NAME(user_type_id)              AS DataType,
    is_nullable,
    object_definition(default_object_id) AS DefaultValue
FROM sys.columns
WHERE object_id = OBJECT_ID('CreditorInvoice')
  AND name IN ('GSTPercentage','GSTAmount',
               'CGSTPercentage','CGSTAmount',
               'SGSTPercentage','SGSTAmount',
               'TDSPercentage','TDSAmount');
GO

-- =============================================================================
-- Extension: Add IGST columns for inter-state transactions
-- Date: 2026-10-20
-- =============================================================================

-- 7. IGSTPercentage
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'IGSTPercentage'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD IGSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Column IGSTPercentage added.';
END
ELSE
    PRINT 'Column IGSTPercentage already exists — skipped.';
GO

-- 8. IGSTAmount
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'IGSTAmount'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD IGSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Column IGSTAmount added.';
END
ELSE
    PRINT 'Column IGSTAmount already exists — skipped.';
GO

-- 9. TransactionType — 'I' = Intra-state (CGST+SGST), 'E' = Inter-state (IGST)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'TransactionType'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD TransactionType NVARCHAR(1) NOT NULL DEFAULT 'I';
    PRINT 'Column TransactionType added.';
END
ELSE
    PRINT 'Column TransactionType already exists — skipped.';
GO

-- 10. Backfill: existing records with GSTAmount set but IGST = 0
--     → mark as Intra-state (already backfilled as CGST+SGST above)
UPDATE CreditorInvoice
SET TransactionType = 'I'
WHERE TransactionType = '' OR TransactionType IS NULL;
GO

-- Verify all GST columns
SELECT
    name,
    TYPE_NAME(user_type_id) AS DataType,
    object_definition(default_object_id) AS DefaultValue
FROM sys.columns
WHERE object_id = OBJECT_ID('CreditorInvoice')
  AND name IN ('GSTPercentage','GSTAmount',
               'CGSTPercentage','CGSTAmount',
               'SGSTPercentage','SGSTAmount',
               'IGSTPercentage','IGSTAmount',
               'TransactionType',
               'TDSPercentage','TDSAmount');
GO
