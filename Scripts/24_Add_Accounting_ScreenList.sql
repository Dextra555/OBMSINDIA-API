-- ============================================================
-- Script  : 24_Add_Accounting_ScreenList.sql
-- Purpose : Insert all missing Accounting category screens
--
--  ANALYSIS (from actual Angular component getUserAccessRights calls):
--  ─────────────────────────────────────────────────────────────────
--  ACCOUNTING TRANSACTIONS (modules/Accounting):
--    Account GLDataList     → acc-gl-data-list.component
--
--  ACCOUNTING REPORTS (modules/report/accounting):
--    Accounting Report      → trail-balance-report, balance-sheet-report,
--                             bank-reconciliation-report, general-ledger-report
--    Profit And Loss Statement → accounting-profit-and-loss, profit-and-loss-without-contra
--    P&L Year Report        → pl-year-report
--
--  ⚠️  IMPORTANT NOTE on PK constraint:
--    ScreenList PK = ScreenName only (not composite).
--    So these screen names must be globally unique.
--    The script uses UPDATE if exists in wrong category,
--    or INSERT if not exists at all.
-- ============================================================

USE OBMSINDIAUAT;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '============================================================';
PRINT '  24_Add_Accounting_ScreenList.sql';
PRINT '  Database: OBMSINDIAUAT';
PRINT '============================================================';
PRINT '';

-- ============================================================
-- STEP 1: Diagnose - show current Accounting screens
-- ============================================================

PRINT '--- Current Accounting screens in ScreenList ---';
SELECT ScreenName, Category, LastUpdate
FROM ScreenList
WHERE Category = 'Accounting'
ORDER BY ScreenName;

PRINT '';
PRINT '--- Checking if any Accounting screens exist under wrong category ---';
SELECT ScreenName, Category, LastUpdate
FROM ScreenList
WHERE ScreenName IN (
    'Account GLDataList',
    'Accounting Report',
    'Profit And Loss Statement',
    'Profit And Lost',
    'P&L Year Report'
)
ORDER BY ScreenName;

PRINT '';

-- ============================================================
-- STEP 2: Helper macro - safe upsert for each screen
--   If exists (any category) → UPDATE to Accounting
--   If not exists            → INSERT as Accounting
-- ============================================================

PRINT '--- Adding Accounting screens ---';
PRINT '';

-- ── 1. Account GLDataList (acc-gl-data-list.component)
IF EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Account GLDataList')
BEGIN
    UPDATE ScreenList SET Category = 'Accounting', LastUpdate = GETDATE()
    WHERE ScreenName = 'Account GLDataList';
    PRINT 'Updated: Account GLDataList → Accounting';
END
ELSE
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Account GLDataList', 'Accounting', GETDATE());
    PRINT 'Added: Account GLDataList (Accounting)';
END

-- ── 2. Accounting Report
--    Used by: trail-balance-report, balance-sheet-report,
--             bank-reconciliation-report, general-ledger-report
IF EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Accounting Report')
BEGIN
    UPDATE ScreenList SET Category = 'Accounting', LastUpdate = GETDATE()
    WHERE ScreenName = 'Accounting Report';
    PRINT 'Updated: Accounting Report → Accounting';
END
ELSE
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Accounting Report', 'Accounting', GETDATE());
    PRINT 'Added: Accounting Report (Accounting)';
END

-- ── 3. Profit And Loss Statement
--    Used by: accounting-profit-and-loss, profit-and-loss-without-contra
IF EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Profit And Lost')
BEGIN
    -- Rename old wrong entry to correct name
    UPDATE ScreenList SET ScreenName = 'Profit And Loss Statement', Category = 'Accounting', LastUpdate = GETDATE()
    WHERE ScreenName = 'Profit And Lost';
    PRINT 'Renamed + Updated: Profit And Lost → Profit And Loss Statement (Accounting)';
END
ELSE IF EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Profit And Loss Statement')
BEGIN
    UPDATE ScreenList SET Category = 'Accounting', LastUpdate = GETDATE()
    WHERE ScreenName = 'Profit And Loss Statement';
    PRINT 'Updated: Profit And Loss Statement → Accounting';
END
ELSE
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Profit And Loss Statement', 'Accounting', GETDATE());
    PRINT 'Added: Profit And Loss Statement (Accounting)';
END

-- ── 4. P&L Year Report  (pl-year-report.component)
IF EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'P&L Year Report')
BEGIN
    UPDATE ScreenList SET Category = 'Accounting', LastUpdate = GETDATE()
    WHERE ScreenName = 'P&L Year Report';
    PRINT 'Updated: P&L Year Report → Accounting';
END
ELSE
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('P&L Year Report', 'Accounting', GETDATE());
    PRINT 'Added: P&L Year Report (Accounting)';
END

PRINT '';

-- ============================================================
-- STEP 3: Verification
-- ============================================================

PRINT '--- STEP 3: Verification ---';
PRINT '';

PRINT '=== All Accounting Screens ===';
SELECT ScreenName, Category, LastUpdate
FROM ScreenList
WHERE Category = 'Accounting'
ORDER BY ScreenName;

PRINT '';
PRINT '=== Total Accounting screen count ===';
SELECT COUNT(*) AS AccountingScreens
FROM ScreenList
WHERE Category = 'Accounting';

PRINT '';
PRINT '=== All categories summary ===';
SELECT Category, COUNT(*) AS TotalScreens
FROM ScreenList
GROUP BY Category
ORDER BY Category;

PRINT '';
PRINT '============================================================';
PRINT '  Script 24 complete.';
PRINT '  Accounting ScreenList is now fully populated.';
PRINT '============================================================';
GO
