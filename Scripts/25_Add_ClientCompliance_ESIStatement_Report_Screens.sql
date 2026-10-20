-- ============================================================
-- Script  : 25_Add_ClientCompliance_ESIStatement_Report_Screens.sql
-- Purpose : Add the missing "Client Compliance Report" and
--           "ESI Statement Report" screens to ScreenList and grant
--           read/view permission so the payroll user (and all
--           existing users) can open these report pages.
--
--  Background:
--    Frontend components call getUserAccessRights(currentUser, <screen>):
--      - report/payroll/client-compliance-report -> 'Client Compliance Report'
--      - report/payroll/esi-statement-report     -> 'ESI Statement Report'
--    Backend GetUserAccessRights queries OBMSPermissions by Name + ScreenName.
--    If no row (or Read=0) exists for the user, the page is blocked with
--    "You do not have permissions to view this page."
--    These two screens were never seeded into ScreenList, so they could not
--    be assigned through the User Access Rights admin UI either.
--
--  This script:
--    1. Inserts both screens into ScreenList under the 'Payroll' category
--       (idempotent - safe to run multiple times).
--    2. Grants admin & superadmin full access (Create/Read/Update/Delete).
--    3. Grants the 'payroll' user Read (View) access explicitly.
--    4. Grants all existing non-deleted OBMSUsers Read access so no user is
--       accidentally locked out.
--
--  Note: ScreenList PK = ScreenName only (not composite), so each screen
--        name is globally unique and can only belong to one category.
-- ============================================================

SET NOCOUNT ON;
GO

PRINT '============================================================';
PRINT '  25_Add_ClientCompliance_ESIStatement_Report_Screens.sql';
PRINT '============================================================';
PRINT '';

-- ============================================================
-- STEP 1: Add screens to ScreenList (Payroll category)
-- ============================================================

PRINT '--- STEP 1: Adding screens to ScreenList (Payroll) ---';
PRINT '';

-- 1a. Client Compliance Report
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Client Compliance Report')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate, LastUpdatedBy)
    VALUES ('Client Compliance Report', 'Payroll', GETDATE(), 'system');
    PRINT 'Added screen: Client Compliance Report (Payroll)';
END
ELSE
BEGIN
    PRINT 'Screen already exists: Client Compliance Report -> '
        + ISNULL((SELECT Category FROM ScreenList WHERE ScreenName = 'Client Compliance Report'), '(null)');
END

-- 1b. ESI Statement Report
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'ESI Statement Report')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate, LastUpdatedBy)
    VALUES ('ESI Statement Report', 'Payroll', GETDATE(), 'system');
    PRINT 'Added screen: ESI Statement Report (Payroll)';
END
ELSE
BEGIN
    PRINT 'Screen already exists: ESI Statement Report -> '
        + ISNULL((SELECT Category FROM ScreenList WHERE ScreenName = 'ESI Statement Report'), '(null)');
END

PRINT '';

-- ============================================================
-- STEP 2: Helper - grant a single user access to a single screen
--   @access = 1 -> full (C/R/U/D), 0 -> Read only
-- ============================================================

PRINT '--- STEP 2: Granting permissions ---';
PRINT '';

DECLARE @ScreenName NVARCHAR(200);
DECLARE @UserName   NVARCHAR(200);

-- ------------------------------------------------------------
-- 2a. admin & superadmin -> FULL access to both screens
-- ------------------------------------------------------------

DECLARE admin_cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT v.ScreenName, v.UserName
    FROM (VALUES
        ('Client Compliance Report', 'admin'),
        ('Client Compliance Report', 'superadmin'),
        ('ESI Statement Report',     'admin'),
        ('ESI Statement Report',     'superadmin')
    ) v(ScreenName, UserName);

OPEN admin_cur;
FETCH NEXT FROM admin_cur INTO @ScreenName, @UserName;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM OBMSPermissions WHERE Name = @UserName AND ScreenName = @ScreenName)
    BEGIN
        INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
        VALUES (@UserName, @ScreenName, 1, 1, 1, 1, 'system');
        PRINT 'Granted FULL access: ' + @UserName + ' -> ' + @ScreenName;
    END
    ELSE
    BEGIN
        UPDATE OBMSPermissions
           SET [Read] = 1, [Create] = 1, [Update] = 1, [Delete] = 1, LastUpdatedBy = 'system'
         WHERE Name = @UserName AND ScreenName = @ScreenName
           AND NOT ([Read] = 1 AND [Create] = 1 AND [Update] = 1 AND [Delete] = 1);
        PRINT 'Verified FULL access: ' + @UserName + ' -> ' + @ScreenName;
    END
    FETCH NEXT FROM admin_cur INTO @ScreenName, @UserName;
END
CLOSE admin_cur;
DEALLOCATE admin_cur;

-- ------------------------------------------------------------
-- 2b. 'payroll' user -> Read (View) access to both screens
--     (explicit, as requested - works even if the exact login
--      name differs in casing)
-- ------------------------------------------------------------

DECLARE payroll_cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT ScreenName FROM (VALUES
        ('Client Compliance Report'),
        ('ESI Statement Report')
    ) v(ScreenName);

OPEN payroll_cur;
FETCH NEXT FROM payroll_cur INTO @ScreenName;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM OBMSPermissions
        WHERE Name = 'payroll' AND ScreenName = @ScreenName
    )
    BEGIN
        INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
        VALUES ('payroll', @ScreenName, 1, 0, 0, 0, 'system');
        PRINT 'Granted READ access: payroll -> ' + @ScreenName;
    END
    ELSE
    BEGIN
        UPDATE OBMSPermissions SET [Read] = 1, LastUpdatedBy = 'system'
         WHERE Name = 'payroll' AND ScreenName = @ScreenName AND [Read] <> 1;
        PRINT 'Verified READ access: payroll -> ' + @ScreenName;
    END
    FETCH NEXT FROM payroll_cur INTO @ScreenName;
END
CLOSE payroll_cur;
DEALLOCATE payroll_cur;

-- ------------------------------------------------------------
-- 2c. All other existing (non-deleted) users -> Read access
--     Ensures no registered user is locked out of these reports.
-- ------------------------------------------------------------

INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
SELECT u.Name, s.ScreenName, 1, 0, 0, 0, 'system'
FROM OBMSUsers u
CROSS JOIN (VALUES
    ('Client Compliance Report'),
    ('ESI Statement Report')
) s(ScreenName)
WHERE u.IsDeleted = 0
  AND NOT EXISTS (
      SELECT 1 FROM OBMSPermissions p
      WHERE p.Name = u.Name AND p.ScreenName = s.ScreenName
  );

PRINT 'Granted READ access to all remaining active users for both report screens.';
PRINT '';

-- ============================================================
-- STEP 3: Verification
-- ============================================================

PRINT '--- STEP 3: Verification ---';
PRINT '';

PRINT '=== ScreenList entries ===';
SELECT ScreenName, Category, LastUpdate
FROM ScreenList
WHERE ScreenName IN ('Client Compliance Report', 'ESI Statement Report')
ORDER BY ScreenName;

PRINT '';
PRINT '=== OBMSPermissions for Client Compliance Report ===';
SELECT Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy
FROM OBMSPermissions
WHERE ScreenName = 'Client Compliance Report'
ORDER BY Name;

PRINT '';
PRINT '=== OBMSPermissions for ESI Statement Report ===';
SELECT Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy
FROM OBMSPermissions
WHERE ScreenName = 'ESI Statement Report'
ORDER BY Name;

PRINT '';
PRINT '=== Payroll user access check ===';
SELECT Name, ScreenName, [Read] AS CanView
FROM OBMSPermissions
WHERE Name = 'payroll'
  AND ScreenName IN ('Client Compliance Report', 'ESI Statement Report')
ORDER BY ScreenName;

PRINT '';
PRINT '============================================================';
PRINT '  Script 25 complete.';
PRINT '  Payroll user can now open:';
PRINT '    - Report / Client Compliance Report';
PRINT '    - Report / ESI Statement Report';
PRINT '============================================================';
GO
