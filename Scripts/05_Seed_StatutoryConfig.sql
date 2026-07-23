-- ============================================================
-- OBMS Statutory Configuration Seed Data
-- ============================================================
USE [obms];
GO

PRINT '=== Seeding Statutory Configuration Data ==='

-- 1. PF Configuration
IF NOT EXISTS (SELECT 1 FROM PFConfiguration)
BEGIN
    PRINT 'Inserting PFConfiguration...'
    INSERT INTO PFConfiguration (Employee_Rate, Employer_Rate, Employer_EPS_Rate, Basic_Salary_Limit, Effective_Date, Is_Active, Created_Date, Financial_Year)
    VALUES (12.00, 12.00, 8.33, 15000.00, '2024-04-01', 1, GETDATE(), '2024-2025');
END

-- 2. ESI Configuration
IF NOT EXISTS (SELECT 1 FROM ESIConfiguration)
BEGIN
    PRINT 'Inserting ESIConfiguration...'
    INSERT INTO ESIConfiguration (Employee_Rate, Employer_Rate, Gross_Salary_Limit, Effective_Date, Is_Active, Created_Date, Financial_Year)
    VALUES (0.75, 3.25, 21000.00, '2024-04-01', 1, GETDATE(), '2024-2025');
END

-- 3. Professional Tax Configuration
IF NOT EXISTS (SELECT 1 FROM ProfessionalTaxConfiguration)
BEGIN
    PRINT 'Inserting ProfessionalTaxConfiguration...'
    INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date)
    VALUES 
    ('35', 'Telangana', 15000, 0, '2024-04-01', 1, GETDATE()),
    ('35', 'Telangana', 20000, 150, '2024-04-01', 1, GETDATE()),
    ('35', 'Telangana', 999999, 200, '2024-04-01', 1, GETDATE()),
    
    ('27', 'Maharashtra', 7500, 0, '2024-04-01', 1, GETDATE()),
    ('27', 'Maharashtra', 10000, 175, '2024-04-01', 1, GETDATE()),
    ('27', 'Maharashtra', 999999, 200, '2024-04-01', 1, GETDATE()),
    
    ('28', 'Karnataka', 14999, 0, '2024-04-01', 1, GETDATE()),
    ('28', 'Karnataka', 999999, 200, '2024-04-01', 1, GETDATE()),
    
    ('07', 'Delhi', 999999, 0, '2024-04-01', 1, GETDATE()),
    
    ('32', 'Tamil Nadu', 21000, 0, '2024-04-01', 1, GETDATE()),
    ('32', 'Tamil Nadu', 31500, 135, '2024-04-01', 1, GETDATE()),
    ('32', 'Tamil Nadu', 47500, 315, '2024-04-01', 1, GETDATE()),
    ('32', 'Tamil Nadu', 63000, 630, '2024-04-01', 1, GETDATE()),
    ('32', 'Tamil Nadu', 999999, 1045, '2024-04-01', 1, GETDATE());
END

-- 4. TDS Slab Configuration (New Regime FY 2024-25)
IF NOT EXISTS (SELECT 1 FROM TDSSlabConfiguration)
BEGIN
    PRINT 'Inserting TDSSlabConfiguration...'
    INSERT INTO TDSSlabConfiguration (Financial_Year, Min_Income, Max_Income, Tax_Rate, Surcharge_Rate, Education_Cess_Rate, Higher_Education_Cess_Rate, Is_Active, Created_Date)
    VALUES
    ('2024-2025', 0, 300000, 0.00, 0, 4.00, 0, 1, GETDATE()),
    ('2024-2025', 300001, 600000, 5.00, 0, 4.00, 0, 1, GETDATE()),
    ('2024-2025', 600001, 900000, 10.00, 0, 4.00, 0, 1, GETDATE()),
    ('2024-2025', 900001, 1200000, 15.00, 0, 4.00, 0, 1, GETDATE()),
    ('2024-2025', 1200001, 1500000, 20.00, 0, 4.00, 0, 1, GETDATE()),
    ('2024-2025', 1500001, 99999999, 30.00, 0, 4.00, 0, 1, GETDATE());
END

PRINT '=== Statutory Config Seeding Completed ==='
