-- ============================================================
-- Drop and Recreate Designation and Department Tables
-- ============================================================

-- Note: Change database name if needed
-- USE [obms];
-- GO

PRINT '=== Dropping foreign key constraints referencing Designation ==='

-- Drop all foreign key constraints that reference Designation
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER TABLE ' + OBJECT_NAME(parent_object_id) + ' DROP CONSTRAINT ' + name + ';' + CHAR(13)
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('Designation');

IF LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql;
    PRINT 'Foreign key constraints referencing Designation dropped';
END
ELSE
BEGIN
    PRINT 'No foreign key constraints found referencing Designation';
END

PRINT '=== Dropping foreign key constraints referencing Department ==='

-- Drop all foreign key constraints that reference Department
SET @sql = '';
SELECT @sql = @sql + 'ALTER TABLE ' + OBJECT_NAME(parent_object_id) + ' DROP CONSTRAINT ' + name + ';' + CHAR(13)
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('Department');

IF LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql;
    PRINT 'Foreign key constraints referencing Department dropped';
END
ELSE
BEGIN
    PRINT 'No foreign key constraints found referencing Department';
END

PRINT '=== Dropping Designation table ==='

-- Drop Designation table
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Designation')
BEGIN
    DROP TABLE Designation;
    PRINT 'Designation table dropped';
END
ELSE
BEGIN
    PRINT 'Designation table does not exist';
END

PRINT '=== Dropping Department table ==='

-- Drop Department table
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Department')
BEGIN
    DROP TABLE Department;
    PRINT 'Department table dropped';
END
ELSE
BEGIN
    PRINT 'Department table does not exist';
END

PRINT '=== Creating Department table ==='

-- Create Department table
CREATE TABLE Department (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    Dept_Code NVARCHAR(100) NOT NULL,
    Dept_Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedDate DATETIME NULL,
    UpdatedBy NVARCHAR(50) NULL
);

PRINT 'Department table created successfully';

PRINT '=== Creating Designation table ==='

-- Create Designation table with DepartmentId
CREATE TABLE Designation (
    DesignationId INT IDENTITY(1,1) PRIMARY KEY,
    Desig_Code NVARCHAR(100) NOT NULL,
    Desig_Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedDate DATETIME NULL,
    UpdatedBy NVARCHAR(50) NULL,
    DepartmentId INT NULL
);

PRINT 'Designation table created successfully';

PRINT '=== Adding foreign key constraint ==='

-- Add foreign key constraint
ALTER TABLE Designation 
ADD CONSTRAINT FK_Designation_Department 
FOREIGN KEY (DepartmentId) REFERENCES Department(DepartmentId);

PRINT 'Foreign key constraint FK_Designation_Department added';

PRINT '=== Creating indexes ==='

-- Create index on DepartmentId for better query performance
CREATE INDEX IX_Designation_DepartmentId ON Designation(DepartmentId);

PRINT 'Index IX_Designation_DepartmentId created';

PRINT '=== Tables recreation complete ==='

-- Verify the tables
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('Department', 'Designation')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- Verify foreign key
SELECT 
    name AS ForeignKeyName,
    OBJECT_NAME(parent_object_id) AS TableName,
    OBJECT_NAME(referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys
WHERE name = 'FK_Designation_Department';

PRINT '=== Verification complete ==='
PRINT 'Tables are ready for use. You can now insert sample data.'
