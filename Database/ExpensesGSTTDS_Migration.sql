-- =============================================================================
-- Migration: Add GST and TDS percentage + amount fields to CreditorInvoice
-- Date: 2026-10-17
-- Description: Stores GST% and TDS% on the expense header (CreditorInvoice).
--              Amount is computed from % × Total and stored for audit.
--              GSTApplicable / TDSApplicable bit columns are NOT needed;
--              a non-zero percentage implies applicability.
-- =============================================================================

-- 1. GSTPercentage — GST rate applied to the invoice (e.g. 18.00)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'GSTPercentage'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Column GSTPercentage added.';
END
ELSE
    PRINT 'Column GSTPercentage already exists — skipped.';
GO

-- 2. GSTAmount — computed/manually-overridden GST value (in INR)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'GSTAmount'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD GSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Column GSTAmount added.';
END
ELSE
    PRINT 'Column GSTAmount already exists — skipped.';
GO

-- 3. TDSPercentage — TDS rate applied to the invoice (e.g. 2.00)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'TDSPercentage'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD TDSPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Column TDSPercentage added.';
END
ELSE
    PRINT 'Column TDSPercentage already exists — skipped.';
GO

-- 4. TDSAmount — computed/manually-overridden TDS value (in INR)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'TDSAmount'
)
BEGIN
    ALTER TABLE CreditorInvoice
    ADD TDSAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Column TDSAmount added.';
END
ELSE
    PRINT 'Column TDSAmount already exists — skipped.';
GO

-- Clean up earlier bit columns — must drop default constraint first
DECLARE @sql NVARCHAR(200);

-- Drop default constraint on GSTApplicable
SELECT @sql = 'ALTER TABLE CreditorInvoice DROP CONSTRAINT ' + dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE c.object_id = OBJECT_ID('CreditorInvoice') AND c.name = 'GSTApplicable';
IF @sql IS NOT NULL BEGIN EXEC sp_executesql @sql; PRINT 'Default constraint on GSTApplicable dropped.'; END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'GSTApplicable')
BEGIN
    ALTER TABLE CreditorInvoice DROP COLUMN GSTApplicable;
    PRINT 'Column GSTApplicable dropped.';
END
GO

-- Drop default constraint on TDSApplicable
DECLARE @sql2 NVARCHAR(200);
SELECT @sql2 = 'ALTER TABLE CreditorInvoice DROP CONSTRAINT ' + dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE c.object_id = OBJECT_ID('CreditorInvoice') AND c.name = 'TDSApplicable';
IF @sql2 IS NOT NULL BEGIN EXEC sp_executesql @sql2; PRINT 'Default constraint on TDSApplicable dropped.'; END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CreditorInvoice') AND name = 'TDSApplicable')
BEGIN
    ALTER TABLE CreditorInvoice DROP COLUMN TDSApplicable;
    PRINT 'Column TDSApplicable dropped.';
END
GO

-- Verify
SELECT
    name,
    TYPE_NAME(user_type_id)              AS DataType,
    is_nullable,
    object_definition(default_object_id) AS DefaultValue
FROM sys.columns
WHERE object_id = OBJECT_ID('CreditorInvoice')
  AND name IN ('GSTPercentage','GSTAmount','TDSPercentage','TDSAmount');
GO
