-- ============================================================
-- Add RBI Bank Salary Process Screen to ScreenList
-- Purpose: Add the missing screen and configure permissions
-- ============================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '=== Adding RBI Bank Salary Process Screen ==='

-- Check if screen already exists
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'RBI Bank Salary Process')
BEGIN
    PRINT 'Adding RBI Bank Salary Process to ScreenList...'
    
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('RBI Bank Salary Process', 'Payroll', GETDATE())
    
    PRINT 'Screen added successfully.'
END
ELSE
BEGIN
    PRINT 'Screen already exists in ScreenList.'
END

-- Add permissions for superadmin
IF NOT EXISTS (
    SELECT 1 FROM OBMSPermissions 
    WHERE Name = 'superadmin' AND ScreenName = 'RBI Bank Salary Process'
)
BEGIN
    PRINT 'Adding permissions for superadmin...'
    
    INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
    VALUES ('superadmin', 'RBI Bank Salary Process', 1, 1, 1, 1, 'system')
    
    PRINT 'Superadmin permissions added successfully.'
END
ELSE
BEGIN
    PRINT 'Superadmin already has permissions for this screen.'
END

-- Add permissions for all existing users (excluding those who already have permissions)
PRINT ''
PRINT 'Adding permissions for all existing users...'

INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
SELECT DISTINCT u.Name, 'RBI Bank Salary Process', 1, 1, 1, 1, 'system'
FROM OBMSUsers u
WHERE NOT EXISTS (
    SELECT 1 FROM OBMSPermissions p 
    WHERE p.Name = u.Name AND p.ScreenName = 'RBI Bank Salary Process'
)

PRINT 'Permissions added for all users.'

-- List all current screens in Payroll category for verification
PRINT ''
PRINT '=== Current Screens in Payroll Category ==='
SELECT ScreenName, Category, LastUpdate 
FROM ScreenList 
WHERE Category = 'Payroll'
ORDER BY ScreenName

PRINT ''
PRINT '=== Current Permissions for RBI Bank Salary Process ==='
SELECT Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy
FROM OBMSPermissions
WHERE ScreenName = 'RBI Bank Salary Process'

PRINT ''
PRINT '=== Script Complete ==='
