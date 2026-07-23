-- Complete Professional Tax Data from Excel Sheet
-- This script inserts professional tax data for multiple states

-- Clear existing data to avoid duplicates
DELETE FROM ProfessionalTaxConfiguration;

-- Tamil Nadu Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Tamil Nadu', 0, 21000, 0, '2024-04-01', 'ADMIN', 'Up to ₹21,000 (6 months)'),
('Tamil Nadu', 21000.01, 30000, 100, '2024-04-01', 'ADMIN', '₹21,001 – ₹30,000'),
('Tamil Nadu', 30000.01, 45000, 235, '2024-04-01', 'ADMIN', '₹30,001 – ₹45,000'),
('Tamil Nadu', 45000.01, 60000, 510, '2024-04-01', 'ADMIN', '₹45,001 – ₹60,000'),
('Tamil Nadu', 60000.01, 75000, 760, '2024-04-01', 'ADMIN', '₹60,001 – ₹75,000'),
('Tamil Nadu', 75000.01, NULL, 1095, '2024-04-01', 'ADMIN', 'Above ₹75,000');

-- Karnataka Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Karnataka', 0, 15000, 0, '2024-04-01', 'ADMIN', 'Up to ₹15,000/month'),
('Karnataka', 15000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹15,000');

-- Maharashtra Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Maharashtra', 0, 7500, 0, '2024-04-01', 'ADMIN', 'Up to ₹7,500 (M)'),
('Maharashtra', 7500.01, 10000, 175, '2024-04-01', 'ADMIN', '₹7,501 – ₹10,000'),
('Maharashtra', 10000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹10,000');

-- Telangana Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Telangana', 0, 15000, 0, '2024-04-01', 'ADMIN', 'Up to ₹15,000'),
('Telangana', 15000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹15,000');

-- Andhra Pradesh Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Andhra Pradesh', 0, 15000, 0, '2024-04-01', 'ADMIN', 'Up to ₹15,000'),
('Andhra Pradesh', 15000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹15,000');

-- Gujarat Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Gujarat', 0, NULL, 200, '2024-04-01', 'ADMIN', 'General - Max ₹2,400/year');

-- West Bengal Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('West Bengal', 0, 10000, 110, '2024-04-01', 'ADMIN', 'Up to ₹10,000 - Approx'),
('West Bengal', 10000.01, 15000, 130, '2024-04-01', 'ADMIN', '₹10,001 – ₹15,000'),
('West Bengal', 15000.01, 25000, 150, '2024-04-01', 'ADMIN', '₹15,001 – ₹25,000'),
('West Bengal', 25000.01, 40000, 180, '2024-04-01', 'ADMIN', '₹25,001 – ₹40,000'),
('West Bengal', 40000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹40,000');

-- Kerala Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Kerala', 20, 200, 0, '2024-04-01', 'ADMIN', 'Varies (by local body) - ₹20 – ₹200'),
('Kerala', 200.01, NULL, 125, '2024-04-01', 'ADMIN', 'Half-yearly - Municipality-based');

-- Madhya Pradesh Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Madhya Pradesh', 0, 18750, 0, '2024-04-01', 'ADMIN', 'Up to ₹18,750'),
('Madhya Pradesh', 18750.01, 25000, 125, '2024-04-01', 'ADMIN', '₹18,751 – ₹25,000'),
('Madhya Pradesh', 25000.01, 33333, 167, '2024-04-01', 'ADMIN', '₹25,001 – ₹33,333'),
('Madhya Pradesh', 33333.01, NULL, 208, '2024-04-01', 'ADMIN', 'Above ₹33,333');

-- Odisha Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Odisha', 0, 13500, 0, '2024-04-01', 'ADMIN', 'Up to ₹13,500'),
('Odisha', 13500.01, 25000, 125, '2024-04-01', 'ADMIN', '₹13,501 – ₹25,000'),
('Odisha', 25000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹25,000');

-- Assam Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Assam', 0, 15000, 0, '2024-04-01', 'ADMIN', 'Up to ₹15,000'),
('Assam', 15000.01, 25000, 150, '2024-04-01', 'ADMIN', '₹15,001 – ₹25,000'),
('Assam', 25000.01, NULL, 208, '2024-04-01', 'ADMIN', 'Above ₹25,000');

-- Jharkhand Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Jharkhand', 0, 25000, 0, '2024-04-01', 'ADMIN', 'Up to ₹25,000'),
('Jharkhand', 25000.01, 40000, 100, '2024-04-01', 'ADMIN', '₹25,001 – ₹40,000'),
('Jharkhand', 40000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹40,000');

-- Chhattisgarh Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Chhattisgarh', 0, 15000, 0, '2024-04-01', 'ADMIN', 'Up to ₹15,000'),
('Chhattisgarh', 15000.01, 25000, 150, '2024-04-01', 'ADMIN', '₹15,001 – ₹25,000'),
('Chhattisgarh', 25000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹25,000');

-- Meghalaya Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Meghalaya', 0, NULL, 200, '2024-04-01', 'ADMIN', 'General - Slab-based');

-- Tripura Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Tripura', 0, NULL, 200, '2024-04-01', 'ADMIN', 'General - Slab-based');

-- Sikkim Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Sikkim', 0, NULL, 200, '2024-04-01', 'ADMIN', 'General - Slab-based');

-- Bihar Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Bihar', 0, 25000, 0, '2024-04-01', 'ADMIN', 'Up to ₹25,000'),
('Bihar', 25000.01, 40000, 100, '2024-04-01', 'ADMIN', '₹25,001 – ₹40,000'),
('Bihar', 40000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹40,000');

-- Create table if it doesn't exist
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
        LastUpdatedBy NVARCHAR(50) NULL,
        Notes NVARCHAR(500) NULL
    )
    
    PRINT 'ProfessionalTaxConfiguration table created successfully'
END
ELSE
BEGIN
    -- Check if Notes column exists and add it if it doesn't
    IF NOT EXISTS (SELECT * FROM syscolumns WHERE name='Notes' AND id=OBJECT_ID('ProfessionalTaxConfiguration'))
    BEGIN
        ALTER TABLE ProfessionalTaxConfiguration ADD Notes NVARCHAR(500) NULL;
        PRINT 'Notes column added to ProfessionalTaxConfiguration table'
    END
    ELSE
    BEGIN
        PRINT 'Notes column already exists in ProfessionalTaxConfiguration table'
    END
END

-- Verify insertion
SELECT 
    State,
    COUNT(*) as TotalRecords,
    CAST(MIN(MinSalary) AS NVARCHAR(20)) as MinSalaryRange,
    CASE 
        WHEN MAX(MaxSalary) IS NULL THEN 'No Limit'
        ELSE CAST(MAX(MaxSalary) AS NVARCHAR(20))
    END as MaxSalaryRange
FROM ProfessionalTaxConfiguration 
GROUP BY State
ORDER BY State;

PRINT 'Complete Professional Tax data inserted successfully!'
PRINT 'Total states: ' + CAST((SELECT COUNT(DISTINCT State) FROM ProfessionalTaxConfiguration) AS NVARCHAR(10))
PRINT 'Total records: ' + CAST((SELECT COUNT(*) FROM ProfessionalTaxConfiguration) AS NVARCHAR(10))
