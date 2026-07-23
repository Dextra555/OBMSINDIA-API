-- Fix Script: Handle NULL values in CB_AdvanceStatutoryBonus for EmployeeID 59 and 60
-- This script ensures all employees have valid CB_AdvanceStatutoryBonus values

USE OBMS;
GO

PRINT 'Checking current state of CB columns...';

-- Check which columns exist
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Employee'
AND COLUMN_NAME IN ('CB_Bonus', 'CB_BonusPercentage', 'CB_Advance', 'CB_AdvancePercentage', 'CB_AdvanceStatutoryBonus', 'CB_AdvanceStatutoryBonusPercentage')
ORDER BY COLUMN_NAME;
GO

PRINT 'Checking EmployeeID 59 and 60 data...';

-- Check current values for EmployeeID 59 and 60
SELECT 
    EMP_ID,
    EMP_NAME,
    CB_Bonus,
    CB_BonusPercentage,
    CB_Advance,
    CB_AdvancePercentage,
    CB_AdvanceStatutoryBonus,
    CB_AdvanceStatutoryBonusPercentage
FROM Employee
WHERE EMP_ID IN (59, 60);
GO

PRINT 'Fixing NULL values in CB_AdvanceStatutoryBonus...';

-- Fix: Migrate data from old columns to new columns for all employees where new columns are NULL
UPDATE Employee
SET 
    CB_AdvanceStatutoryBonus = ISNULL(CB_Bonus, 0) + ISNULL(CB_Advance, 0),
    CB_AdvanceStatutoryBonusPercentage = CASE 
        WHEN CB_BonusPercentage > 0 THEN CB_BonusPercentage 
        WHEN CB_AdvancePercentage > 0 THEN CB_AdvancePercentage 
        ELSE 0 
    END
WHERE CB_AdvanceStatutoryBonus IS NULL OR CB_AdvanceStatutoryBonusPercentage IS NULL;
GO

PRINT 'Verifying fix for EmployeeID 59 and 60...';

-- Verify the fix
SELECT 
    EMP_ID,
    EMP_NAME,
    CB_AdvanceStatutoryBonus,
    CB_AdvanceStatutoryBonusPercentage
FROM Employee
WHERE EMP_ID IN (59, 60);
GO

PRINT 'Fix completed successfully.';
PRINT 'NOTE: If old columns (CB_Bonus, CB_Advance) still exist, consider dropping them after verification.';
