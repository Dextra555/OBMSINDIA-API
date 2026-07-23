-- Add PF Statement Report Screen to ScreenList
-- Purpose: Add the missing PF Statement Report screen and configure permissions

PRINT '=== Adding PF Statement Report Screen ==='

-- Check if screen already exists
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'PF Statement Report')
BEGIN
    PRINT 'Adding PF Statement Report to ScreenList...'
    
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('PF Statement Report', 'Finance', GETDATE())
    
    PRINT 'Screen added successfully.'
END
ELSE
BEGIN
    PRINT 'Screen already exists in ScreenList.'
END

-- Grant superadmin full access to this screen
IF NOT EXISTS (
    SELECT 1 FROM OBMSPermissions 
    WHERE Name = 'superadmin' AND ScreenName = 'PF Statement Report'
)
BEGIN
    PRINT 'Granting superadmin permissions for PF Statement Report...'
    
    INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
    VALUES ('superadmin', 'PF Statement Report', 1, 1, 1, 1, 'system')
    
    PRINT 'Superadmin permissions granted.'
END
ELSE
BEGIN
    PRINT 'Superadmin already has permissions for this screen.'
END

-- Grant permissions to all existing users (optional - remove if not needed)
INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
SELECT u.Name, 'PF Statement Report', 1, 0, 0, 0, 'system'
FROM Users u
WHERE NOT EXISTS (
    SELECT 1 FROM OBMSPermissions p 
    WHERE p.Name = u.Name AND p.ScreenName = 'PF Statement Report'
)

PRINT '=== Verification ==='
-- List all current screens in Finance category for verification
PRINT '=== Current Screens in Finance Category ==='
SELECT ScreenName, Category, LastUpdate 
FROM ScreenList 
WHERE Category = 'Finance'
ORDER BY ScreenName

-- Show permissions for PF Statement Report
PRINT '=== Permissions for PF Statement Report ==='
SELECT Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy
FROM OBMSPermissions
WHERE ScreenName = 'PF Statement Report'

PRINT '=== Script Complete ==='
