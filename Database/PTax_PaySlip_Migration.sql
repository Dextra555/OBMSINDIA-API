-- =============================================================================
-- PTax Integration Migration Script
-- Purpose : 1. Add PTax column to PaySlip and PaySlipAudit tables
--           2. Correct slab data gaps identified in gap analysis
-- Run on  : OBMS India Live database
-- Author  : Kiro
-- Date    : 2026-10-01
-- =============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 1 : PaySlip table – add PTax column
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'PTax'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [PTax] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'PTax column added to PaySlip table.';
END
ELSE
BEGIN
    PRINT 'PTax column already exists in PaySlip table – skipped.';
END
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 2 : PaySlipAudit table – add PTax column (same structure)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'PTax'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [PTax] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'PTax column added to PaySlipAudit table.';
END
ELSE
BEGIN
    PRINT 'PTax column already exists in PaySlipAudit table – skipped.';
END
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 3 : TaxPeriod column guard (should already exist from prior migration)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[ProfessionalTaxConfiguration]')
      AND name = N'TaxPeriod'
)
BEGIN
    ALTER TABLE [dbo].[ProfessionalTaxConfiguration]
        ADD [TaxPeriod] NVARCHAR(20) NOT NULL DEFAULT 'Monthly';
    PRINT 'TaxPeriod column added to ProfessionalTaxConfiguration.';
END
ELSE
BEGIN
    PRINT 'TaxPeriod column already exists – skipped.';
END
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 4 : Slab data corrections
--
-- GAP IDENTIFIED:
--   AP / Telangana : Missing ₹150 slab for salary range 15,001 – 20,000
--   Gujarat        : Single flat Annual slab replaced with correct monthly bands
--   Delhi / UP / Haryana / Punjab : Should be zero (no PT levied in these states)
--
-- NOTE on Karnataka:
--   Crystal Report uses ₹24,999 threshold; DB uses ₹15,000.
--   The ₹15,000 threshold is the current official Karnataka PT threshold (as of 2025-26).
--   We retain the DB value (₹15,000) as the source of truth.
--   If you want to restore the Crystal formula threshold, change it to ₹24,999 here.
-- ─────────────────────────────────────────────────────────────────────────────

-- ── 4A : Andhra Pradesh – Add missing ₹150 slab (15,001 – 20,000) ──────────
-- First, fix the upper bound of the existing 0-slab so it ends at 20,000
IF EXISTS (
    SELECT 1 FROM [dbo].[ProfessionalTaxConfiguration]
    WHERE State = 'Andhra Pradesh' AND IsActive = 1 AND MinSalary = 0 AND TaxAmount = 0
)
BEGIN
    UPDATE [dbo].[ProfessionalTaxConfiguration]
    SET MaxSalary = 15000.00
    WHERE State = 'Andhra Pradesh' AND IsActive = 1 AND MinSalary = 0 AND TaxAmount = 0;
    PRINT 'Andhra Pradesh: 0-slab MaxSalary corrected to 15,000.';
END

-- Insert the missing ₹150 slab if it does not already exist
IF NOT EXISTS (
    SELECT 1 FROM [dbo].[ProfessionalTaxConfiguration]
    WHERE State = 'Andhra Pradesh' AND IsActive = 1
      AND MinSalary = 15000.01 AND TaxAmount = 150
)
BEGIN
    INSERT INTO [dbo].[ProfessionalTaxConfiguration]
        (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, IsActive, CreatedBy, CreatedDate)
    VALUES
        ('Andhra Pradesh', 15000.01, 20000.00, 150, 'Monthly', '2025-04-01', 1, 'ADMIN', GETDATE());
    PRINT 'Andhra Pradesh: Added ₹150 slab (15,001 – 20,000).';
END

-- Adjust the existing >15K slab: set MinSalary to 20,000.01
UPDATE [dbo].[ProfessionalTaxConfiguration]
SET MinSalary = 20000.01
WHERE State = 'Andhra Pradesh' AND IsActive = 1
  AND TaxAmount = 200 AND MinSalary = 15000.01;

-- ── 4B : Telangana – same three-slab fix ────────────────────────────────────
IF EXISTS (
    SELECT 1 FROM [dbo].[ProfessionalTaxConfiguration]
    WHERE State = 'Telangana' AND IsActive = 1 AND MinSalary = 0 AND TaxAmount = 0
)
BEGIN
    UPDATE [dbo].[ProfessionalTaxConfiguration]
    SET MaxSalary = 15000.00
    WHERE State = 'Telangana' AND IsActive = 1 AND MinSalary = 0 AND TaxAmount = 0;
    PRINT 'Telangana: 0-slab MaxSalary corrected to 15,000.';
END

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[ProfessionalTaxConfiguration]
    WHERE State = 'Telangana' AND IsActive = 1
      AND MinSalary = 15000.01 AND TaxAmount = 150
)
BEGIN
    INSERT INTO [dbo].[ProfessionalTaxConfiguration]
        (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, IsActive, CreatedBy, CreatedDate)
    VALUES
        ('Telangana', 15000.01, 20000.00, 150, 'Monthly', '2025-04-01', 1, 'ADMIN', GETDATE());
    PRINT 'Telangana: Added ₹150 slab (15,001 – 20,000).';
END

UPDATE [dbo].[ProfessionalTaxConfiguration]
SET MinSalary = 20000.01
WHERE State = 'Telangana' AND IsActive = 1
  AND TaxAmount = 200 AND MinSalary = 15000.01;

-- ── 4C : Gujarat – replace flat Annual slab with correct monthly bands ───────
-- Deactivate the existing incorrect flat ₹200 Annual slab
UPDATE [dbo].[ProfessionalTaxConfiguration]
SET IsActive = 0, LastUpdatedDate = GETDATE(), LastUpdatedBy = 'ADMIN'
WHERE State = 'Gujarat' AND IsActive = 1;
PRINT 'Gujarat: Deactivated existing incorrect slab(s).';

-- Insert correct monthly salary-banded slabs (matching Crystal Report formula)
INSERT INTO [dbo].[ProfessionalTaxConfiguration]
    (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, IsActive, CreatedBy, CreatedDate)
VALUES
    ('Gujarat', 0,        5999.99, 0,   'Monthly', '2025-04-01', 1, 'ADMIN', GETDATE()),
    ('Gujarat', 6000.00,  8999.99, 80,  'Monthly', '2025-04-01', 1, 'ADMIN', GETDATE()),
    ('Gujarat', 9000.00, 11999.99, 150, 'Monthly', '2025-04-01', 1, 'ADMIN', GETDATE()),
    ('Gujarat', 12000.00, NULL,    200, 'Monthly', '2025-04-01', 1, 'ADMIN', GETDATE());
PRINT 'Gujarat: Inserted 4 correct monthly salary-banded slabs.';

-- ── 4D : Delhi / UP / Haryana / Punjab – zero PT (no PT levied) ─────────────
-- Deactivate any existing non-zero slabs for these states
UPDATE [dbo].[ProfessionalTaxConfiguration]
SET IsActive = 0, LastUpdatedDate = GETDATE(), LastUpdatedBy = 'ADMIN'
WHERE State IN ('Delhi', 'Uttar Pradesh', 'Haryana', 'Punjab')
  AND IsActive = 1 AND TaxAmount > 0;
PRINT 'Delhi / UP / Haryana / Punjab: Deactivated non-zero PT slabs (no PT levied in these states).';

-- ─────────────────────────────────────────────────────────────────────────────
-- PART 5 : Verify results
-- ─────────────────────────────────────────────────────────────────────────────
SELECT State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, IsActive
FROM [dbo].[ProfessionalTaxConfiguration]
WHERE State IN ('Tamil Nadu','Karnataka','Maharashtra','Andhra Pradesh','Telangana',
                'West Bengal','Kerala','Gujarat','Delhi','Uttar Pradesh','Haryana','Punjab')
ORDER BY State, MinSalary;

PRINT '=== Migration complete. Review results above before proceeding. ===';
GO

-- =============================================================================
-- PART 6 : PaySlip table – add EarnedSalary and PerDaySalary columns
--   EarnedSalary  = BasicSalaryDays × PerDaySalary
--                   (matches Crystal {@EarnedSalary} formula exactly)
--   PerDaySalary  = (CB_Basic + CB_DA + CB_HRA + CB_OtherAllowances +
--                    CB_AdvanceStatutoryBonus + CB_Leaves + CB_NH)
--                   / CalendarDaysInMonth
--                   (matches Crystal {@PerDaySalary} formula exactly)
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'EarnedSalary'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [EarnedSalary] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'EarnedSalary column added to PaySlip table.';
END
ELSE
BEGIN
    PRINT 'EarnedSalary column already exists in PaySlip — skipped.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'PerDaySalary'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [PerDaySalary] DECIMAL(10,4) NOT NULL DEFAULT 0;
    -- 4 decimal places for accuracy (e.g. 30000 / 31 = 967.7419)
    PRINT 'PerDaySalary column added to PaySlip table.';
END
ELSE
BEGIN
    PRINT 'PerDaySalary column already exists in PaySlip — skipped.';
END
GO

-- =============================================================================
-- PART 7 : PaySlipAudit table – same two columns
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'EarnedSalary'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [EarnedSalary] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'EarnedSalary column added to PaySlipAudit table.';
END
ELSE
BEGIN
    PRINT 'EarnedSalary column already exists in PaySlipAudit — skipped.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'PerDaySalary'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [PerDaySalary] DECIMAL(10,4) NOT NULL DEFAULT 0;
    PRINT 'PerDaySalary column added to PaySlipAudit table.';
END
ELSE
BEGIN
    PRINT 'PerDaySalary column already exists in PaySlipAudit — skipped.';
END
GO

PRINT '=== Parts 6-7 complete: EarnedSalary + PerDaySalary added. ===';
GO

-- =============================================================================
-- PART 8 : PaySlip table – add PF and PFWage columns
--   PF      = Employee PF contribution (12% of capped PF wage), rounded to 0dp
--             Matches Crystal Report formula exactly.
--   PFWage  = Actual PF wage used for calculation
--             = (Basic + DA + OtherAllowances) / CalendarDays × WorkingDays
--             capped at PFConfiguration.Basic_Salary_Limit (₹15,000)
--
-- NOTE: EPFDeductionAmount already exists in PaySlip — that is the legacy
--       Malaysia-era lookup table value. PF is the new Indian statutory value
--       from PFConfiguration. Both coexist; Crystal Report should use PF.
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'PF'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [PF] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'PF column added to PaySlip table.';
END
ELSE
    PRINT 'PF column already exists in PaySlip — skipped.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'PFWage'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [PFWage] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'PFWage column added to PaySlip table.';
END
ELSE
    PRINT 'PFWage column already exists in PaySlip — skipped.';
GO

-- =============================================================================
-- PART 9 : PaySlipAudit — same two columns
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'PF'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [PF] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'PF column added to PaySlipAudit table.';
END
ELSE
    PRINT 'PF column already exists in PaySlipAudit — skipped.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'PFWage'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [PFWage] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'PFWage column added to PaySlipAudit table.';
END
ELSE
    PRINT 'PFWage column already exists in PaySlipAudit — skipped.';
GO

PRINT '=== Parts 8-9 complete: PF + PFWage columns added. ===';
GO

-- =============================================================================
-- PART 10 : PaySlip table – add ESI and ESIWage columns
--   ESI     = Employee ESI contribution (0.75% of ESI wage), rounded to 0dp
--             Matches Crystal Report formula exactly.
--   ESIWage = EarnedSalary − CB_AdvanceStatutoryBonus, capped at ₹21,000
--
-- NOTE: SOCSODeductionAmount already exists in PaySlip — that is the legacy
--       Malaysia-era SOCSO value. ESI is the new Indian statutory value
--       from ESIConfiguration. Both coexist; Crystal Report should use ESI.
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'ESI'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [ESI] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'ESI column added to PaySlip table.';
END
ELSE
    PRINT 'ESI column already exists in PaySlip — skipped.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'ESIWage'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [ESIWage] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'ESIWage column added to PaySlip table.';
END
ELSE
    PRINT 'ESIWage column already exists in PaySlip — skipped.';
GO

-- =============================================================================
-- PART 11 : PaySlipAudit — same two columns
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'ESI'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [ESI] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'ESI column added to PaySlipAudit table.';
END
ELSE
    PRINT 'ESI column already exists in PaySlipAudit — skipped.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'ESIWage'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [ESIWage] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'ESIWage column added to PaySlipAudit table.';
END
ELSE
    PRINT 'ESIWage column already exists in PaySlipAudit — skipped.';
GO

PRINT '=== Parts 10-11 complete: ESI + ESIWage columns added. ===';
GO

-- =============================================================================
-- PART 12 : PaySlip table – add GrossPay and LWF columns
--
--   GrossPay = CB_Basic + CB_DA + CB_HRA + CB_OtherAllowances +
--              CB_AdvanceStatutoryBonus + CB_Leaves + CB_NH
--              (full month CB sum — matches Crystal Report {@gross} formula)
--
--   LWF      = Labour Welfare Fund
--              If Month = December → ₹30
--              Else → 0
--              (matches Crystal Report LWF formula exactly)
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'GrossPay'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [GrossPay] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'GrossPay column added to PaySlip table.';
END
ELSE
    PRINT 'GrossPay column already exists in PaySlip — skipped.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'LWF'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [LWF] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'LWF column added to PaySlip table.';
END
ELSE
    PRINT 'LWF column already exists in PaySlip — skipped.';
GO

-- =============================================================================
-- PART 13 : PaySlipAudit — same two columns
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'GrossPay'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [GrossPay] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'GrossPay column added to PaySlipAudit table.';
END
ELSE
    PRINT 'GrossPay column already exists in PaySlipAudit — skipped.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'LWF'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [LWF] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'LWF column added to PaySlipAudit table.';
END
ELSE
    PRINT 'LWF column already exists in PaySlipAudit — skipped.';
GO

PRINT '=== Parts 12-13 complete: GrossPay + LWF columns added. ===';
GO

-- =============================================================================
-- PART 14 : PaySlip table – add TotalDeduction and NetPay columns
--
--   TotalDeduction = PF + ESI + PTax + LWF +
--                    DailyAdvanceRecovery + MonthlyAdvanceRecovery +
--                    UniformIssueRecovery + LoanRecovery +
--                    MiscDeduction +
--                    (GrossPay - EarnedSalary)  [= unpaid/absent leave deduction]
--
--   NetPay = EarnedSalary - (PF + ESI + PTax + LWF +
--             DailyAdvanceRecovery + MonthlyAdvanceRecovery +
--             UniformIssueRecovery + LoanRecovery + MiscDeduction)
--          = GrossPay - TotalDeduction
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'TotalDeduction'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [TotalDeduction] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'TotalDeduction column added to PaySlip table.';
END
ELSE
    PRINT 'TotalDeduction column already exists in PaySlip — skipped.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlip]') AND name = N'NetPay'
)
BEGIN
    ALTER TABLE [dbo].[PaySlip]
        ADD [NetPay] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'NetPay column added to PaySlip table.';
END
ELSE
    PRINT 'NetPay column already exists in PaySlip — skipped.';
GO

-- =============================================================================
-- PART 15 : PaySlipAudit — same two columns
-- =============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'TotalDeduction'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [TotalDeduction] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'TotalDeduction column added to PaySlipAudit table.';
END
ELSE
    PRINT 'TotalDeduction column already exists in PaySlipAudit — skipped.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name = N'NetPay'
)
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit]
        ADD [NetPay] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT 'NetPay column added to PaySlipAudit table.';
END
ELSE
    PRINT 'NetPay column already exists in PaySlipAudit — skipped.';
GO

PRINT '=== Parts 14-15 complete: TotalDeduction + NetPay columns added. ===';
GO
