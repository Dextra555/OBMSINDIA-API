-- Professional Tax Setup Script
-- Run this script in your SQL Server Management Studio to set up the database

-- Create ProfessionalTaxConfiguration table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProfessionalTaxConfiguration' AND xtype='U')
BEGIN
    CREATE TABLE ProfessionalTaxConfiguration (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        State NVARCHAR(50) NOT NULL,
        MinSalary DECIMAL(10,2) NOT NULL,
        MaxSalary DECIMAL(10,2) NULL,
        TaxAmount DECIMAL(10,2) NOT NULL,
        EffectiveDate DATETIME NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(50) NOT NULL,
        LastUpdatedDate DATETIME NULL,
        LastUpdatedBy NVARCHAR(50) NULL
    )
    
    PRINT 'ProfessionalTaxConfiguration table created successfully'
END
ELSE
BEGIN
    PRINT 'ProfessionalTaxConfiguration table already exists'
END

-- Insert sample data for Maharashtra
IF NOT EXISTS (SELECT 1 FROM ProfessionalTaxConfiguration WHERE State = 'Maharashtra')
BEGIN
    INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy) VALUES
    ('Maharashtra', 0, 7500, 0, '2024-04-01', 'ADMIN'),
    ('Maharashtra', 7500.01, 10000, 175, '2024-04-01', 'ADMIN'),
    ('Maharashtra', 10000.01, 15000, 200, '2024-04-01', 'ADMIN'),
    ('Maharashtra', 15000.01, 20000, 300, '2024-04-01', 'ADMIN'),
    ('Maharashtra', 20000.01, NULL, 400, '2024-04-01', 'ADMIN')
    
    PRINT 'Maharashtra professional tax data inserted successfully'
END

-- Insert sample data for Karnataka
IF NOT EXISTS (SELECT 1 FROM ProfessionalTaxConfiguration WHERE State = 'Karnataka')
BEGIN
    INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy) VALUES
    ('Karnataka', 0, 14999, 0, '2024-04-01', 'ADMIN'),
    ('Karnataka', 15000, 19999, 150, '2024-04-01', 'ADMIN'),
    ('Karnataka', 20000, NULL, 200, '2024-04-01', 'ADMIN')
    
    PRINT 'Karnataka professional tax data inserted successfully'
END

-- Verify data
SELECT COUNT(*) as TotalRecords FROM ProfessionalTaxConfiguration
PRINT 'Setup completed. Total records: ' + CAST((SELECT COUNT(*) FROM ProfessionalTaxConfiguration) AS NVARCHAR(10))
