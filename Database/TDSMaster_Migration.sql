-- ============================================================
-- Migration: Create TDSMaster table for vendor/contractor TDS rate lookup
-- Date: 2026-10-28
-- Description: Adds a TDSMaster lookup table for managing Indian TDS
--              (Tax Deducted at Source) category names and percentages
--              applicable to vendor/contractor payments.
--
-- This table is architecturally distinct from TDSSlabConfiguration,
-- which is for employee income-tax (Section 192) payroll slabs.
-- TDSMaster covers vendor TDS sections: 194C, 194H, 194I, 194J, 194A, 194B.
--
-- IMPORTANT: The seed rates below are used as configurable defaults.
-- Validate applicable rates, thresholds, and PAN rules (Sec 206AA) against
-- the current Finance Act and CBDT circulars before using in production.
-- Notable: Commission/Brokerage seeded at 2% per user requirement;
-- statutory rate under Sec 194H (FY 2024-25) is 5% — confirm and update.
-- ============================================================

-- Step 1: Create TDSMaster table (idempotent guard)
IF OBJECT_ID(N'dbo.TDSMaster', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TDSMaster
    (
        ID              INT             IDENTITY(1,1)   NOT NULL,
        TDSName         VARCHAR(100)                    NOT NULL,
        TDSPercentage   DECIMAL(5,2)                    NOT NULL,
        IsActive        BIT             NOT NULL        CONSTRAINT DF_TDSMaster_IsActive DEFAULT (1),

        CONSTRAINT PK_TDSMaster PRIMARY KEY CLUSTERED (ID ASC)
    );

    PRINT 'Table TDSMaster created successfully.';
END
ELSE
BEGIN
    PRINT 'Table TDSMaster already exists. Skipped creation.';
END
GO

-- Step 2: Seed initial TDS categories (only if table is empty)
IF NOT EXISTS (SELECT 1 FROM dbo.TDSMaster)
BEGIN
    -- Sec 194C  – Payments to Contractors (Individual / HUF)
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Contractor - Individual / HUF (Sec 194C)', 1.00, 1);

    -- Sec 194C  – Payments to Contractors (Other than Individual / HUF)
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Contractor - Other Entities (Sec 194C)', 2.00, 1);

    -- Sec 194H  – Commission or Brokerage
    -- NOTE: Statutory rate under Sec 194H (FY 2024-25) is 5%.
    --       This row is seeded at 2% per user configuration; update as applicable.
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Commission / Brokerage (Sec 194H)', 2.00, 1);

    -- Sec 194I(b) – Rent of Plant, Machinery or Equipment
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Rent - Plant and Machinery (Sec 194I(b))', 2.00, 1);

    -- Sec 194I(a) – Rent of Land, Building or Furniture
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Rent - Land and Building (Sec 194I(a))', 10.00, 1);

    -- Sec 194J  – Professional Fees
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Professional Fees (Sec 194J)', 10.00, 1);

    -- Sec 194J  – Technical Services / Royalty / Call Centre
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Technical Services (Sec 194J)', 2.00, 1);

    -- Sec 194A  – Interest (other than interest on securities)
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Interest - Other (Sec 194A)', 10.00, 1);

    -- Sec 194    – Dividend
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Dividend (Sec 194)', 10.00, 1);

    -- Sec 194B  – Winnings from Lottery / Crossword Puzzle
    INSERT INTO dbo.TDSMaster (TDSName, TDSPercentage, IsActive)
    VALUES (N'Lottery Winnings (Sec 194B)', 30.00, 1);

    PRINT '10 seed TDS categories inserted into TDSMaster.';
END
ELSE
BEGIN
    PRINT 'TDSMaster already has data. Seed rows skipped.';
END
GO

-- Step 3: Verify
SELECT ID, TDSName, TDSPercentage, IsActive
FROM dbo.TDSMaster
ORDER BY ID;
GO
