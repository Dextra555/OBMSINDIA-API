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

-- Create PFConfiguration table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PFConfiguration' AND xtype='U')
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
    
    PRINT 'PFConfiguration table created successfully'
END
ELSE
BEGIN
    PRINT 'PFConfiguration table already exists'
END

-- Create ESIConfiguration table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ESIConfiguration' AND xtype='U')
BEGIN
    CREATE TABLE ESIConfiguration (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Employee_Rate DECIMAL(5,2) NOT NULL DEFAULT 0.75,
        Employer_Rate DECIMAL(5,2) NOT NULL DEFAULT 3.25,
        Basic_Salary_Limit DECIMAL(10,2) NOT NULL DEFAULT 21000.00,
        Effective_Date DATETIME NOT NULL DEFAULT GETDATE(),
        Is_Active BIT NOT NULL DEFAULT 1,
        Created_Date DATETIME NOT NULL DEFAULT GETDATE(),
        Financial_Year NVARCHAR(9) NOT NULL DEFAULT '2024-2025'
    )
    
    PRINT 'ESIConfiguration table created successfully'
END
ELSE
BEGIN
    PRINT 'ESIConfiguration table already exists'
END

-- Create TDSSlabConfiguration table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TDSSlabConfiguration' AND xtype='U')
BEGIN
    CREATE TABLE TDSSlabConfiguration (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Financial_Year NVARCHAR(9) NOT NULL DEFAULT '2024-2025',
        Min_Income DECIMAL(12,2) NOT NULL,
        Max_Income DECIMAL(12,2) NULL,
        Tax_Rate DECIMAL(5,2) NOT NULL,
        Surcharge_Rate DECIMAL(5,2) NULL,
        Education_Cess_Rate DECIMAL(5,2) NULL,
        Higher_Education_Cess_Rate DECIMAL(5,2) NULL,
        Is_Active BIT NOT NULL DEFAULT 1,
        Created_Date DATETIME NOT NULL DEFAULT GETDATE()
    )
    
    PRINT 'TDSSlabConfiguration table created successfully'
END
ELSE
BEGIN
    PRINT 'TDSSlabConfiguration table already exists'
END
