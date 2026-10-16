-- =============================================================================
-- TDS Applicable Migration Script
-- Purpose : 1. Add TDSApplicable column to Employee table
--           2. Add TDS column to PaySlip and PaySlipAudit tables
-- Run on  : OBMS India Live database
-- Author  : Kiro
-- Date    : 2026-10-16
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 1 : Employee table – add TDSApplicable column
--   BIT column, NOT NULL, default 0 (No).
--   All existing employees default to No — change individual records as needed.
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Employee]') AND name = N'TDSApplicable'
)
BEGIN
    ALTER TABLE [dbo].[Employee]
        ADD [TDSApplicable] BIT NOT NULL DEFAULT 0;
    PRINT 'TDSApplicable column added to Employee table (default 0 = No).';
END
ELSE
BEGIN
    PRINT 'TDSApplicable column already exists in Employee table – skipped.';
END
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 2 : PaySlip table – add TDS column
--   Stores the monthly TDS amount deducted from the employee's salary.
--   0.00 when TDSApplicable = 0; calculated value when TDSApplicable = 1.
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'TDS'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [TDS] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'TDS column added to PaySlip table.';
END
ELSE
BEGIN
    PRINT 'TDS column already exists in PaySlip table – skipped.';
END
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 3 : PaySlipAudit table – add TDS column (mirror of PaySlip)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'TDS'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [TDS] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'TDS column added to PaySlipAudit table.';
END
ELSE
BEGIN
    PRINT 'TDS column already exists in PaySlipAudit table – skipped.';
END
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 4 : Verify results
-- ─────────────────────────────────────────────────────────────────────────────
SELECT
    c.name            AS ColumnName,
    t.name            AS DataType,
    c.max_length,
    c.is_nullable,
    c.default_object_id
FROM sys.columns c
JOIN sys.types   t ON t.user_type_id = c.user_type_id
WHERE c.object_id IN (
        OBJECT_ID(N'[dbo].[Employee]'),
        OBJECT_ID(N'[dbo].[PaySlip]'),
        OBJECT_ID(N'[dbo].[PaySlipAudit]')
      )
  AND c.name IN (N'TDSApplicable', N'TDS')
ORDER BY OBJECT_NAME(c.object_id), c.name;

PRINT '=== TDSApplicable_Migration complete. Review results above. ===';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 5 : Refresh vwPaySheet to include TDS and TDSApplicable columns
--   Run this AFTER the ALTER TABLE statements above have been applied.
--   Copy the full CREATE_vwPaySheet.sql body here so the view is self-contained.
-- ─────────────────────────────────────────────────────────────────────────────
-- NOTE: The canonical definition is in Scripts\Create_vwPaySheet.sql.
--       The snippet below re-executes it so this migration file is self-contained.

IF OBJECT_ID(N'[dbo].[vwPaySheet]', 'V') IS NOT NULL
    DROP VIEW [dbo].[vwPaySheet];
GO

-- Paste the full updated vwPaySheet definition from Scripts\Create_vwPaySheet.sql here,
-- or execute Create_vwPaySheet.sql directly after this migration.
-- The view now SELECTs PaySlip.TDS and Employee.TDSApplicable.
PRINT 'vwPaySheet must be refreshed: run Scripts\Create_vwPaySheet.sql after this migration.';
GO
