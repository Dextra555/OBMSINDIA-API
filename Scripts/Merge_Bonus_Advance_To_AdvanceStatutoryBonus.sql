-- Migration Script: Merge CB_Bonus and CB_Advance into CB_AdvanceStatutoryBonus
-- This script merges the separate Statutory Bonus and Advance Payment fields into a single Advance Statutory Bonus field
-- as per the business requirement to simplify the Commercial Breakdown structure.

-- Step 1: Add new columns for Advance Statutory Bonus
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Employee') AND name = 'CB_AdvanceStatutoryBonus')
BEGIN
    ALTER TABLE Employee ADD CB_AdvanceStatutoryBonus DECIMAL(18, 2) NULL;
    PRINT 'Added column: CB_AdvanceStatutoryBonus';
END
ELSE
BEGIN
    PRINT 'Column CB_AdvanceStatutoryBonus already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Employee') AND name = 'CB_AdvanceStatutoryBonusPercentage')
BEGIN
    ALTER TABLE Employee ADD CB_AdvanceStatutoryBonusPercentage DECIMAL(18, 2) NULL;
    PRINT 'Added column: CB_AdvanceStatutoryBonusPercentage';
END
ELSE
BEGIN
    PRINT 'Column CB_AdvanceStatutoryBonusPercentage already exists';
END
GO

PRINT 'Step 1 completed: Columns added (if needed)';
PRINT 'Step 2: Migrating data...';

-- Step 2: Migrate data - Sum CB_Bonus and CB_Advance into CB_AdvanceStatutoryBonus
-- Use the percentage from CB_BonusPercentage if available, otherwise use CB_AdvancePercentage
UPDATE Employee
SET 
    CB_AdvanceStatutoryBonus = ISNULL(CB_Bonus, 0) + ISNULL(CB_Advance, 0),
    CB_AdvanceStatutoryBonusPercentage = CASE 
        WHEN CB_BonusPercentage > 0 THEN CB_BonusPercentage 
        WHEN CB_AdvancePercentage > 0 THEN CB_AdvancePercentage 
        ELSE 0 
    END
WHERE CB_AdvanceStatutoryBonus IS NULL OR CB_AdvanceStatutoryBonusPercentage IS NULL;

PRINT 'Step 2 completed: Data migrated from CB_Bonus and CB_Advance to CB_AdvanceStatutoryBonus';

-- Step 3: Drop old columns (after verifying data migration)
-- Uncomment these lines after verifying the data migration is successful
/*
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Employee') AND name = 'CB_Bonus')
BEGIN
    ALTER TABLE Employee DROP COLUMN CB_Bonus;
    PRINT 'Dropped column: CB_Bonus';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Employee') AND name = 'CB_BonusPercentage')
BEGIN
    ALTER TABLE Employee DROP COLUMN CB_BonusPercentage;
    PRINT 'Dropped column: CB_BonusPercentage';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Employee') AND name = 'CB_Advance')
BEGIN
    ALTER TABLE Employee DROP COLUMN CB_Advance;
    PRINT 'Dropped column: CB_Advance';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Employee') AND name = 'CB_AdvancePercentage')
BEGIN
    ALTER TABLE Employee DROP COLUMN CB_AdvancePercentage;
    PRINT 'Dropped column: CB_AdvancePercentage';
END
*/

PRINT 'Migration script completed successfully.';
PRINT 'IMPORTANT: Verify data migration before uncommenting and running the DROP COLUMN statements.';
