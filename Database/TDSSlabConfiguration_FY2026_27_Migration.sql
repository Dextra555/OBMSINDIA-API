-- ============================================================
-- Migration: Employee TDS Slab Enhancement for FY 2026-27
-- Phase 2 — TDS Slab Correction & Calculation Engine Enhancement
--
-- WARNING: This script is provided for REVIEW ONLY.
--          DO NOT execute it against any database without explicit
--          written approval. It is NOT applied automatically.
--
-- Why two batches (separated by GO):
--   SQL Server compiles an entire batch before executing it. If the
--   ALTER TABLE that adds a column and the UPDATE/INSERT that
--   references that new column live in the SAME batch, compilation
--   fails with "Invalid column name …" before anything runs.
--   So: Batch 1 = schema (DDL), Batch 2 = data (atomic, transactional).
--
-- Scope:
--   Batch 1 (DDL, idempotent):
--     1. Add Regime, Standard_Deduction, Rebate_Threshold, Rebate_Amount,
--        Effective_Date to TDSSlabConfiguration.
--     2. Create EmployeeTDSConfiguration (per employee / per FY).
--   Batch 2 (data, atomic — rolls back on any error):
--     3. Make existing FY 2024-25 slab boundaries contiguous (fix off-by-one).
--     4. Backfill FY 2024-25 rows to preserve historical behaviour
--        (Std 75000, no rebate, cess 4%, Regime = New).
--     5. Seed FY 2026-27 New Regime (7 contiguous slabs) with 87A rebate.
--     6. Seed FY 2026-27 Old Regime for regime support.
--
-- All changes are additive (new nullable columns / new table / new rows).
-- Historical FY 2024-25 slabs are NOT removed; they remain queryable by FY.
-- ============================================================

SET NOCOUNT ON;

-- ============================================================
-- BATCH 1 — Schema changes (DDL). Idempotent; safe to re-run.
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'TDSSlabConfiguration' AND COLUMN_NAME = 'Regime')
    ALTER TABLE dbo.TDSSlabConfiguration
        ADD Regime VARCHAR(10) NOT NULL CONSTRAINT DF_TDSSlab_Regime DEFAULT ('New');

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'TDSSlabConfiguration' AND COLUMN_NAME = 'Standard_Deduction')
    ALTER TABLE dbo.TDSSlabConfiguration
        ADD Standard_Deduction DECIMAL(12,2) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'TDSSlabConfiguration' AND COLUMN_NAME = 'Rebate_Threshold')
    ALTER TABLE dbo.TDSSlabConfiguration
        ADD Rebate_Threshold DECIMAL(12,2) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'TDSSlabConfiguration' AND COLUMN_NAME = 'Rebate_Amount')
    ALTER TABLE dbo.TDSSlabConfiguration
        ADD Rebate_Amount DECIMAL(12,2) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'TDSSlabConfiguration' AND COLUMN_NAME = 'Effective_Date')
    ALTER TABLE dbo.TDSSlabConfiguration
        ADD Effective_Date DATE NULL;

IF OBJECT_ID(N'dbo.EmployeeTDSConfiguration', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeTDSConfiguration
    (
        ID                        INT             IDENTITY(1,1) NOT NULL,
        EmployeeID                INT             NOT NULL,
        FinancialYear             NVARCHAR(9)     NOT NULL,
        TaxRegime                 VARCHAR(10)     NOT NULL CONSTRAINT DF_EmpTDS_Regime   DEFAULT ('New'),
        PreviousEmployerIncome    DECIMAL(12,2)   NOT NULL CONSTRAINT DF_EmpTDS_PrevInc DEFAULT (0),
        PreviousEmployerTDS       DECIMAL(12,2)   NOT NULL CONSTRAINT DF_EmpTDS_PrevTDS DEFAULT (0),
        IsActive                  BIT             NOT NULL CONSTRAINT DF_EmpTDS_Active DEFAULT (1),
        CreatedDate               DATETIME        NOT NULL CONSTRAINT DF_EmpTDS_Created DEFAULT (GETDATE()),
        LastUpdatedDate           DATETIME        NULL,
        LastUpdatedBy             NVARCHAR(50)    NULL,
        CONSTRAINT PK_EmployeeTDSConfiguration PRIMARY KEY CLUSTERED (ID ASC),
        CONSTRAINT UQ_EmpTDS_EmpFY UNIQUE (EmployeeID, FinancialYear)
    );
END
GO

-- ============================================================
-- BATCH 2 — Data changes (atomic). Rolls back on any error.
-- Compiles against the schema produced by Batch 1.
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    --------------------------------------------------------------
    -- 3. Make existing FY 2024-25 slab boundaries contiguous.
    --    Min_Income of each row = previous row's Max_Income (no gaps).
    --    The CTE is only read (joined back by Id); the UPDATE targets the
    --    base table directly, so there is no dependency on CTE updatability.
    --------------------------------------------------------------
    ;WITH ordered AS (
        SELECT Id,
               Min_Income,
               LAG(Max_Income) OVER (ORDER BY Min_Income) AS PrevMax,
               ROW_NUMBER() OVER (ORDER BY Min_Income) AS rn
        FROM dbo.TDSSlabConfiguration
        WHERE Financial_Year = '2024-2025'
    )
    UPDATE t
       SET t.Min_Income = CASE WHEN o.rn = 1 THEN 0 ELSE o.PrevMax END
      FROM dbo.TDSSlabConfiguration t
      JOIN ordered o ON o.Id = t.Id
     WHERE t.Min_Income <> CASE WHEN o.rn = 1 THEN 0 ELSE ISNULL(o.PrevMax, t.Min_Income) END;

    PRINT 'Contiguous boundaries applied to FY 2024-2025 slabs';

    --------------------------------------------------------------
    -- 4. Backfill FY 2024-25 config (preserve historical behaviour).
    --    New Regime FY24-25: Std 75000, NO rebate (Rebate_Threshold 0),
    --    cess 4%. Reproduces the prior engine exactly (no rebate then).
    --------------------------------------------------------------
    UPDATE dbo.TDSSlabConfiguration
       SET Regime             = 'New',
           Standard_Deduction = 75000.00,
           Rebate_Threshold   = 0.00,        -- rebate never triggers (preserve history)
           Rebate_Amount      = 0.00,
           Education_Cess_Rate = ISNULL(Education_Cess_Rate, 4.00),
           Effective_Date     = '2024-04-01'
     WHERE Financial_Year = '2024-2025';

    PRINT 'FY 2024-2025 config backfilled (New Regime, historical behaviour preserved)';

    --------------------------------------------------------------
    -- 5. Seed FY 2026-27 New Regime (7 contiguous slabs) + 87A rebate.
    --    Nil up to 4L; 87A rebate 60000 (nil tax up to 12L income).
    --------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM dbo.TDSSlabConfiguration
                   WHERE Financial_Year = '2026-2027' AND Regime = 'New')
    BEGIN
        INSERT INTO dbo.TDSSlabConfiguration
            (Financial_Year, Min_Income, Max_Income, Tax_Rate,
             Surcharge_Rate, Education_Cess_Rate, Higher_Education_Cess_Rate,
             Is_Active, Created_Date, Regime, Standard_Deduction,
             Rebate_Threshold, Rebate_Amount, Effective_Date)
        VALUES
            ('2026-2027', 0,        400000,   0.00, 0, 4.00, 0, 1, GETDATE(), 'New', 75000.00, 1200000.00, 60000.00, '2026-04-01'),
            ('2026-2027', 400000,   800000,   5.00, 0, 4.00, 0, 1, GETDATE(), 'New', 75000.00, 1200000.00, 60000.00, '2026-04-01'),
            ('2026-2027', 800000,   1200000, 10.00, 0, 4.00, 0, 1, GETDATE(), 'New', 75000.00, 1200000.00, 60000.00, '2026-04-01'),
            ('2026-2027', 1200000,  1600000, 15.00, 0, 4.00, 0, 1, GETDATE(), 'New', 75000.00, 1200000.00, 60000.00, '2026-04-01'),
            ('2026-2027', 1600000,  2000000, 20.00, 0, 4.00, 0, 1, GETDATE(), 'New', 75000.00, 1200000.00, 60000.00, '2026-04-01'),
            ('2026-2027', 2000000,  2400000, 25.00, 0, 4.00, 0, 1, GETDATE(), 'New', 75000.00, 1200000.00, 60000.00, '2026-04-01'),
            ('2026-2027', 2400000,  NULL,    30.00, 0, 4.00, 0, 1, GETDATE(), 'New', 75000.00, 1200000.00, 60000.00, '2026-04-01');
        PRINT 'Seeded FY 2026-2027 New Regime (7 slabs)';
    END

    --------------------------------------------------------------
    -- 6. Seed FY 2026-27 Old Regime (for regime support).
    --    Old Regime salaried std deduction 50000; 87A rebate 12500 up to 5L.
    --------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM dbo.TDSSlabConfiguration
                   WHERE Financial_Year = '2026-2027' AND Regime = 'Old')
    BEGIN
        INSERT INTO dbo.TDSSlabConfiguration
            (Financial_Year, Min_Income, Max_Income, Tax_Rate,
             Surcharge_Rate, Education_Cess_Rate, Higher_Education_Cess_Rate,
             Is_Active, Created_Date, Regime, Standard_Deduction,
             Rebate_Threshold, Rebate_Amount, Effective_Date)
        VALUES
            ('2026-2027', 0,       250000,  0.00, 0, 4.00, 0, 1, GETDATE(), 'Old', 50000.00, 500000.00, 12500.00, '2026-04-01'),
            ('2026-2027', 250000,  500000,  5.00, 0, 4.00, 0, 1, GETDATE(), 'Old', 50000.00, 500000.00, 12500.00, '2026-04-01'),
            ('2026-2027', 500000,  1000000, 20.00, 0, 4.00, 0, 1, GETDATE(), 'Old', 50000.00, 500000.00, 12500.00, '2026-04-01'),
            ('2026-2027', 1000000, NULL,    30.00, 0, 4.00, 0, 1, GETDATE(), 'Old', 50000.00, 500000.00, 12500.00, '2026-04-01');
        PRINT 'Seeded FY 2026-2027 Old Regime (4 slabs)';
    END

    COMMIT TRANSACTION;
    PRINT '=== TDS FY 2026-27 migration completed successfully ===';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT '=== TDS migration FAILED — rolled back ===';
    PRINT ERROR_MESSAGE();
    THROW;
END CATCH
GO
