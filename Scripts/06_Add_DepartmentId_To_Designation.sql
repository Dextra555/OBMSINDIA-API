-- ============================================================
-- Designation: Add DepartmentId for Department Mapping
-- ============================================================

USE [obms];
GO

PRINT '=== Adding DepartmentId to Designation table ==='

-- Add DepartmentId column to Designation table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Designation' AND COLUMN_NAME='DepartmentId')
BEGIN
    ALTER TABLE Designation ADD DepartmentId INT NULL;
    PRINT 'DepartmentId column added to Designation table';
END
ELSE
BEGIN
    PRINT 'DepartmentId column already exists in Designation table';
END

-- Add foreign key constraint if Department table exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Department')
BEGIN
    -- Check if foreign key already exists
    IF NOT EXISTS (
        SELECT 1 
        FROM sys.foreign_keys 
        WHERE name = 'FK_Designation_Department' 
        AND parent_object_id = OBJECT_ID('Designation')
    )
    BEGIN
        -- Add foreign key constraint
        ALTER TABLE Designation 
        ADD CONSTRAINT FK_Designation_Department 
        FOREIGN KEY (DepartmentId) REFERENCES Department(DepartmentId);
        PRINT 'Foreign key constraint FK_Designation_Department added';
    END
    ELSE
    BEGIN
        PRINT 'Foreign key constraint FK_Designation_Department already exists';
    END
END
ELSE
BEGIN
    PRINT 'Department table does not exist, skipping foreign key constraint';
END

PRINT '=== Migration Complete ==='

-- Verify the changes
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Designation' 
  AND COLUMN_NAME = 'DepartmentId'

-- Check for foreign key constraint
SELECT 
    name AS ForeignKeyName,
    OBJECT_NAME(parent_object_id) AS TableName,
    OBJECT_NAME(referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('Designation')
  AND name = 'FK_Designation_Department';

PRINT '=== Verification Complete ==='
