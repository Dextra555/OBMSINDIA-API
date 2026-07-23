-- Seed PF Configuration with statutory ceiling values
-- This ensures the ₹15,000 ceiling configuration is present for PF calculation

USE [obmsdev_backup]
GO

-- Check if PFConfiguration table exists, if not create it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='PFConfiguration')
BEGIN
    CREATE TABLE PFConfiguration (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Employee_Rate DECIMAL(5,2) NOT NULL DEFAULT 12.00,
        Employer_Rate DECIMAL(5,2) NOT NULL DEFAULT 12.00,
        Employer_EPS_Rate DECIMAL(5,2) NOT NULL DEFAULT 8.33,
        Basic_Salary_Limit DECIMAL(10,2) NOT NULL DEFAULT 15000.00,
        Effective_Date DATETIME NOT NULL DEFAULT GETDATE(),
        Is_Active BIT NOT NULL DEFAULT 1,
        Created_Date DATETIME NOT NULL DEFAULT GETDATE(),
        Financial_Year NVARCHAR(9) NOT NULL DEFAULT '2024-2025'
    )
    PRINT 'PFConfiguration table created'
END
ELSE
BEGIN
    PRINT 'PFConfiguration table already exists'
END
GO

-- Update existing active configuration to statutory rates
UPDATE PFConfiguration
SET Employee_Rate = 12.00,
    Employer_Rate = 13.00,
    Basic_Salary_Limit = 15000.00
WHERE Is_Active = 1
GO

-- Ensure statutory configuration record exists
IF NOT EXISTS (SELECT 1 FROM PFConfiguration WHERE Is_Active = 1)
BEGIN
    INSERT INTO PFConfiguration (Employee_Rate, Employer_Rate, Employer_EPS_Rate, Basic_Salary_Limit, Effective_Date, Is_Active, Created_Date, Financial_Year)
    VALUES (12.00, 13.00, 8.33, 15000.00, '2024-04-01', 1, GETDATE(), '2024-2025')
    PRINT 'PFConfiguration record inserted with statutory ceiling values'
END
ELSE
BEGIN
    PRINT 'PFConfiguration record updated with statutory ceiling values'
END
GO

-- Verify the configuration
SELECT 
    Id, 
    Employee_Rate, 
    Employer_Rate, 
    Employer_EPS_Rate, 
    Basic_Salary_Limit, 
    Effective_Date, 
    Is_Active, 
    Financial_Year 
FROM PFConfiguration 
WHERE Is_Active = 1
GO

PRINT 'PF Configuration seeding completed'
GO
