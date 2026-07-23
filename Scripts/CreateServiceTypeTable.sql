-- Create ServiceType table if it doesn't exist
USE [obms];
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServiceType')
BEGIN
    CREATE TABLE ServiceType (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ServiceName NVARCHAR(100) NOT NULL,
        ServiceCode NVARCHAR(20) NOT NULL,
        Description NVARCHAR(200) NULL,
        HSNCode NVARCHAR(8) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
        UpdatedDate DATETIME NULL,
        UpdatedBy NVARCHAR(50) NULL
    );
    
    PRINT 'ServiceType table created successfully.'
END
ELSE
BEGIN
    PRINT 'ServiceType table already exists.'
END
GO

-- Seed initial service types if table is empty
IF NOT EXISTS (SELECT 1 FROM ServiceType)
BEGIN
    INSERT INTO ServiceType (ServiceName, ServiceCode, Description, HSNCode, IsActive, CreatedBy, CreatedDate)
    VALUES 
        (N'Manpower Supply', N'MPS', N'Provision of skilled and unskilled manpower', N'998511', 1, 'SYSTEM', GETDATE()),
        (N'Security Services', N'SEC', N'Security guard and surveillance services', N'998813', 1, 'SYSTEM', GETDATE()),
        (N'Housekeeping Services', N'HKS', N'Cleaning and maintenance services', N'998813', 1, 'SYSTEM', GETDATE()),
        (N'Administrative Support', N'ADM', N'Office administration and support services', N'998811', 1, 'SYSTEM', GETDATE()),
        (N'Technical Services', N'TEC', N'Technical and maintenance support', N'998821', 1, 'SYSTEM', GETDATE()),
        (N'Consulting Services', N'CON', N'Business and management consulting', N'998311', 1, 'SYSTEM', GETDATE());
    
    PRINT 'Initial service types seeded successfully.'
END
ELSE
BEGIN
    PRINT 'ServiceType table already has data.'
END
GO

-- Verify table structure
PRINT '=== ServiceType Table Structure ==='
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ServiceType'
ORDER BY ORDINAL_POSITION;
GO

-- Verify data
PRINT '=== ServiceType Data ==='
SELECT * FROM ServiceType;
GO

PRINT '=== ServiceType Setup Complete ==='
