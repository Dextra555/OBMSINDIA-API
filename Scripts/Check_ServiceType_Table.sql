-- Check if ServiceType table exists and its structure
USE [obms];
GO

PRINT '=== Checking ServiceType Table ==='

-- Check if table exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServiceType')
BEGIN
    PRINT 'ServiceType table EXISTS'
    
    -- Show table structure
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ServiceType'
    ORDER BY ORDINAL_POSITION
    
    -- Show sample data if any
    PRINT ''
    PRINT 'Sample data (if any):'
    SELECT TOP 5 * FROM ServiceType
END
ELSE
BEGIN
    PRINT 'ServiceType table DOES NOT EXIST - Creating table...'
    
    -- Create ServiceType table
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
    
    -- Create indexes
    CREATE INDEX IX_ServiceType_ServiceCode ON ServiceType(ServiceCode)
    CREATE INDEX IX_ServiceType_ServiceName ON ServiceType(ServiceName)
    CREATE INDEX IX_ServiceType_HSNCode ON ServiceType(HSNCode)
    
    PRINT 'Indexes created on ServiceType table'
END
GO

PRINT '=== Check Complete ==='
