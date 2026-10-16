-- ============================================================
-- Script  : 23_Fix_ScreenList_PK_Conflict.sql
-- Purpose : Fix PRIMARY KEY conflicts in ScreenList table
--
--  ROOT CAUSE:
--    ScreenList table has [Key] = ScreenName only (not composite key).
--    So same ScreenName cannot exist in two different categories.
--
--  PROBLEM SCREENS:
--    1. 'Cheque Master'     → already exists in Inventory, needed in Finance
--    2. 'Employee Transfer' → already exists in some category, needed in Payroll
--
--  FIX STRATEGY:
--    → Check current category of conflicting screens
--    → UPDATE Category to the correct one (Finance / Payroll)
--    → These screens belong to Finance / Payroll in the actual app
--
-- ============================================================

USE OBMSINDIAUAT;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '============================================================';
PRINT '  23_Fix_ScreenList_PK_Conflict.sql';
PRINT '============================================================';
PRINT '';

-- ============================================================
-- STEP 1: Diagnose - show current state of conflicting screens
-- ============================================================

PRINT '--- Current state of conflicting screens ---';

SELECT ScreenName, Category, LastUpdate
FROM ScreenList
WHERE ScreenName IN ('Cheque Master', 'Employee Transfer')
ORDER BY ScreenName;

PRINT '';

-- ============================================================
-- STEP 2: Fix 'Cheque Master' → Update to Finance
-- (finance-cheque-master.component uses 'Cheque Status', 
--  but Cheque Master is in Finance left nav menu)
-- ============================================================

PRINT '--- Fixing: Cheque Master ---';

IF EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Cheque Master')
BEGIN
    UPDATE ScreenList 
    SET Category = 'Finance', LastUpdate = GETDATE()
    WHERE ScreenName = 'Cheque Master';
    PRINT 'Updated: Cheque Master → Category = Finance';
END
ELSE
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Cheque Master', 'Finance', GETDATE());
    PRINT 'Inserted: Cheque Master (Finance)';
END

-- ============================================================
-- STEP 3: Fix 'Employee Transfer' → Update to Payroll
-- (employee-transfer component uses 'Employee Transfer')
-- ============================================================

PRINT '--- Fixing: Employee Transfer ---';

IF EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Employee Transfer')
BEGIN
    UPDATE ScreenList 
    SET Category = 'Payroll', LastUpdate = GETDATE()
    WHERE ScreenName = 'Employee Transfer';
    PRINT 'Updated: Employee Transfer → Category = Payroll';
END
ELSE
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Employee Transfer', 'Payroll', GETDATE());
    PRINT 'Inserted: Employee Transfer (Payroll)';
END

PRINT '';

-- ============================================================
-- STEP 4: Verify final state
-- ============================================================

PRINT '--- Verification ---';
PRINT '';

PRINT '=== Fixed screens ===';
SELECT ScreenName, Category, LastUpdate
FROM ScreenList
WHERE ScreenName IN ('Cheque Master', 'Employee Transfer')
ORDER BY ScreenName;

PRINT '';
PRINT '=== Finance count ===';
SELECT COUNT(*) AS FinanceScreens FROM ScreenList WHERE Category = 'Finance';

PRINT '';
PRINT '=== Payroll count ===';
SELECT COUNT(*) AS PayrollScreens FROM ScreenList WHERE Category = 'Payroll';

PRINT '';
PRINT '=== Any remaining duplicates across all categories ===';
SELECT ScreenName, COUNT(*) AS [Count]
FROM ScreenList
GROUP BY ScreenName
HAVING COUNT(*) > 1;

PRINT '';
PRINT '============================================================';
PRINT '  Script 23 complete.';
PRINT '============================================================';
GO
