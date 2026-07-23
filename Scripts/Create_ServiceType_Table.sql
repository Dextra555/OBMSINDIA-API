-- Create ServiceType table if it doesn't exist or fix its structure
USE [obmsdev_backup];
GO

PRINT '=== Checking/Creating ServiceType Table ==='

-- Drop table if it exists with wrong structure (to recreate correctly)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServiceType')
BEGIN
    PRINT 'ServiceType table exists, checking structure...'
    
    -- Check if it has the correct columns
    DECLARE @HasCorrectColumns BIT = 1
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceType' AND COLUMN_NAME = 'LastUpdatedDate')
    BEGIN
        PRINT 'Missing LastUpdatedDate column - will recreate table'
        SET @HasCorrectColumns = 0
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceType' AND COLUMN_NAME = 'LastUpdatedBy')
    BEGIN
        PRINT 'Missing LastUpdatedBy column - will recreate table'
        SET @HasCorrectColumns = 0
    END
    
    IF @HasCorrectColumns = 0
    BEGIN
        PRINT 'Dropping incorrect ServiceType table...'
        DROP TABLE ServiceType
    END
    ELSE
    BEGIN
        PRINT 'ServiceType table has correct structure'
    END
END

-- Create table if it doesn't exist or was dropped
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServiceType')
BEGIN
    PRINT 'Creating ServiceType table...'
    
    CREATE TABLE ServiceType (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ServiceName NVARCHAR(100) NOT NULL,
        ServiceCode NVARCHAR(20) NULL,
        Description NVARCHAR(200) NULL,
        HSNCode NVARCHAR(8) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
        LastUpdatedDate DATETIME NULL,
        LastUpdatedBy NVARCHAR(50) NULL
    )
    
    PRINT 'ServiceType table created successfully'
    
    -- Create indexes for performance
    CREATE INDEX IX_ServiceType_ServiceCode ON ServiceType(ServiceCode)
    CREATE INDEX IX_ServiceType_ServiceName ON ServiceType(ServiceName)
    CREATE INDEX IX_ServiceType_HSNCode ON ServiceType(HSNCode)
    CREATE INDEX IX_ServiceType_IsActive ON ServiceType(IsActive)
    
    PRINT 'Indexes created on ServiceType table'
    
    -- Insert sample data for testing
    INSERT INTO ServiceType (ServiceName, ServiceCode, Description, HSNCode, IsActive, CreatedBy)
    VALUES 
    ('Security Services', 'SEC001', 'General security services', '998311', 1, 'SYSTEM'),
    ('Guard Services', 'GRD001', 'Security guard services', '998313', 1, 'SYSTEM'),
    ('Patrol Services', 'PAT001', 'Patrol and monitoring services', '998314', 1, 'SYSTEM')
    
    PRINT 'Sample data inserted'
END
ELSE
BEGIN
    PRINT 'ServiceType table already exists with correct structure'
END

-- Verify the table structure
PRINT ''
PRINT '=== ServiceType Table Structure ==='
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ServiceType'
ORDER BY ORDINAL_POSITION

PRINT ''
PRINT '=== Sample Data ==='
SELECT TOP 5 * FROM ServiceType

PRINT ''
PRINT '=== ServiceType Table Setup Complete ==='
