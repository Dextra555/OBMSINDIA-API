-- ============================================================
-- Add GST Slab and TDS Slab Permissions
-- Purpose: Grant permissions for GST Slab and TDS Slab screens to users
-- ============================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '=== Adding GST Slab and TDS Slab Permissions ==='

-- Add permissions for GST Slab
PRINT ''
PRINT '=== Processing GST Slab ==='

-- Add permissions for superadmin for GST Slab
IF NOT EXISTS (
    SELECT 1 FROM OBMSPermissions 
    WHERE Name = 'superadmin' AND ScreenName = 'GST Slab'
)
BEGIN
    PRINT 'Adding permissions for superadmin for GST Slab...'
    
    INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
    VALUES ('superadmin', 'GST Slab', 1, 1, 1, 1, 'system')
    
    PRINT 'Superadmin permissions added for GST Slab.'
END
ELSE
BEGIN
    PRINT 'Superadmin already has permissions for GST Slab.'
END

-- Add permissions for all existing users for GST Slab (excluding those who already have permissions)
PRINT ''
PRINT 'Adding permissions for all existing users for GST Slab...'

INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
SELECT DISTINCT u.Name, 'GST Slab', 1, 1, 1, 1, 'system'
FROM OBMSUsers u
WHERE NOT EXISTS (
    SELECT 1 FROM OBMSPermissions p 
    WHERE p.Name = u.Name AND p.ScreenName = 'GST Slab'
)

PRINT 'Permissions added for all users for GST Slab.'

-- Add permissions for TDS Slab
PRINT ''
PRINT '=== Processing TDS Slab ==='

-- Add permissions for superadmin for TDS Slab
IF NOT EXISTS (
    SELECT 1 FROM OBMSPermissions 
    WHERE Name = 'superadmin' AND ScreenName = 'TDS Slab'
)
BEGIN
    PRINT 'Adding permissions for superadmin for TDS Slab...'
    
    INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
    VALUES ('superadmin', 'TDS Slab', 1, 1, 1, 1, 'system')
    
    PRINT 'Superadmin permissions added for TDS Slab.'
END
ELSE
BEGIN
    PRINT 'Superadmin already has permissions for TDS Slab.'
END

-- Add permissions for all existing users for TDS Slab (excluding those who already have permissions)
PRINT ''
PRINT 'Adding permissions for all existing users for TDS Slab...'

INSERT INTO OBMSPermissions (Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy)
SELECT DISTINCT u.Name, 'TDS Slab', 1, 1, 1, 1, 'system'
FROM OBMSUsers u
WHERE NOT EXISTS (
    SELECT 1 FROM OBMSPermissions p 
    WHERE p.Name = u.Name AND p.ScreenName = 'TDS Slab'
)

PRINT 'Permissions added for all users for TDS Slab.'

-- Verify permissions
PRINT ''
PRINT '=== Current Permissions for GST Slab ==='
SELECT Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy
FROM OBMSPermissions
WHERE ScreenName = 'GST Slab'
ORDER BY Name

PRINT ''
PRINT '=== Current Permissions for TDS Slab ==='
SELECT Name, ScreenName, [Read], [Create], [Update], [Delete], LastUpdatedBy
FROM OBMSPermissions
WHERE ScreenName = 'TDS Slab'
ORDER BY Name

PRINT ''
PRINT '=== Script Complete ==='
