-- Add Voucher Filter Report screen to OBMSPermissions table
-- This script grants access to the Voucher Filter Report screen for admin and all users

-- Check if admin has access to Voucher Filter Report
IF NOT EXISTS (
    SELECT 1 FROM OBMSPermissions 
    WHERE Name = 'admin' AND ScreenName = 'Voucher Filter Report'
)
BEGIN
    -- Grant full access to admin
    INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
    VALUES ('admin', 'Voucher Filter Report', 1, 1, 1, 1, 'system')
    
    PRINT 'Admin permissions granted for Voucher Filter Report.'
END
ELSE
BEGIN
    PRINT 'Admin already has access to Voucher Filter Report.'
END

-- Check if superadmin has access to Voucher Filter Report
IF NOT EXISTS (
    SELECT 1 FROM OBMSPermissions 
    WHERE Name = 'superadmin' AND ScreenName = 'Voucher Filter Report'
)
BEGIN
    -- Grant full access to superadmin
    INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
    VALUES ('superadmin', 'Voucher Filter Report', 1, 1, 1, 1, 'system')
    
    PRINT 'Superadmin permissions granted for Voucher Filter Report.'
END
ELSE
BEGIN
    PRINT 'Superadmin already has access to Voucher Filter Report.'
END

-- Grant read access to all other users who don't have access yet
INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
SELECT u.Name, 'Voucher Filter Report', 1, 0, 0, 0, 'system'
FROM OBMSUsers u
WHERE NOT EXISTS (
    SELECT 1 FROM OBMSPermissions p 
    WHERE p.Name = u.Name AND p.ScreenName = 'Voucher Filter Report'
)

PRINT 'Voucher Filter Report screen access added to database.'
