-- ============================================================
-- Create Quotation and QuotationDetails Tables
-- Based on the Entity Framework models
-- ============================================================

USE [obms];
GO

PRINT '=== Creating Quotation Tables ==='

-- Create Quotation table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Quotation')
BEGIN
    PRINT 'Creating Quotation table...'
    CREATE TABLE Quotation (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        Branch NVARCHAR(20) NOT NULL,
        Client NVARCHAR(20) NOT NULL,
        WorkPlace NVARCHAR(2000) NOT NULL,
        QuotationDate DATETIME NOT NULL DEFAULT GETDATE(),
        QuotationEndDate DATETIME NOT NULL DEFAULT DATEADD(MONTH, 1, GETDATE()),
        Note NVARCHAR(4000) NULL,
        AddedDate DATETIME NULL DEFAULT GETDATE(),
        LASTUPDATE DATETIME NOT NULL DEFAULT GETDATE(),
        LastUpdatedBy NVARCHAR(50) NULL,
        IsValid BIT NULL,
        Status NVARCHAR(50) NULL
    );
    PRINT 'Quotation table created successfully.'
END
ELSE
BEGIN
    PRINT 'Quotation table already exists. Checking for missing columns...'
    
    -- Check and add missing columns if they exist
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Quotation' AND COLUMN_NAME = 'AddedDate')
    BEGIN
        ALTER TABLE Quotation ADD AddedDate DATETIME NULL DEFAULT GETDATE()
        PRINT 'Added AddedDate column to Quotation table.'
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Quotation' AND COLUMN_NAME = 'LASTUPDATE')
    BEGIN
        ALTER TABLE Quotation ADD LASTUPDATE DATETIME NOT NULL DEFAULT GETDATE()
        PRINT 'Added LASTUPDATE column to Quotation table.'
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Quotation' AND COLUMN_NAME = 'LastUpdatedBy')
    BEGIN
        ALTER TABLE Quotation ADD LastUpdatedBy NVARCHAR(50) NULL
        PRINT 'Added LastUpdatedBy column to Quotation table.'
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Quotation' AND COLUMN_NAME = 'IsValid')
    BEGIN
        ALTER TABLE Quotation ADD IsValid BIT NULL
        PRINT 'Added IsValid column to Quotation table.'
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Quotation' AND COLUMN_NAME = 'Status')
    BEGIN
        ALTER TABLE Quotation ADD Status NVARCHAR(50) NULL
        PRINT 'Added Status column to Quotation table.'
    END
END

-- Create QuotationDetails table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QuotationDetails')
BEGIN
    PRINT 'Creating QuotationDetails table...'
    CREATE TABLE QuotationDetails (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        QuotationID INT NOT NULL,
        QuotationDate DATETIME NOT NULL DEFAULT GETDATE(),
        Client NVARCHAR(20) NOT NULL,
        Branch NVARCHAR(20) NOT NULL,
        Description NVARCHAR(200) NOT NULL,
        NoOfGuards INT NOT NULL,
        Rate DECIMAL(18,2) NOT NULL,
        NoOfHours DECIMAL(18,2) NOT NULL,
        NoOfDays DECIMAL(18,2) NOT NULL,
        FollowCalender BIT NOT NULL DEFAULT 0,
        HasDiscount BIT NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountHour INT NOT NULL DEFAULT 0,
        IsTaxable BIT NOT NULL DEFAULT 0,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        MonthTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
        LASTUPDATE DATETIME NOT NULL DEFAULT GETDATE(),
        LastUpdatedBy NVARCHAR(50) NULL,
        Category NVARCHAR(50) NOT NULL,
        Reason NVARCHAR(250) NOT NULL,
        Basic DECIMAL(18,2) NOT NULL DEFAULT 0,
        DA DECIMAL(18,2) NOT NULL DEFAULT 0,
        Leaves DECIMAL(18,2) NOT NULL DEFAULT 0,
        LeavesPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        Allowance DECIMAL(18,2) NOT NULL DEFAULT 0,
        Bonus DECIMAL(18,2) NOT NULL DEFAULT 0,
        BonusPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        NFH DECIMAL(18,2) NOT NULL DEFAULT 0,
        PF DECIMAL(18,2) NOT NULL DEFAULT 0,
        PFPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        ESI DECIMAL(18,2) NOT NULL DEFAULT 0,
        ESIPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        Uniform DECIMAL(18,2) NOT NULL DEFAULT 0,
        ServiceFee DECIMAL(18,2) NOT NULL DEFAULT 0,
        HRA DECIMAL(18,2) NOT NULL DEFAULT 0,
        HRAPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        ProfessionalTax DECIMAL(18,2) NOT NULL DEFAULT 0,
        RelieverCharges DECIMAL(18,2) NOT NULL DEFAULT 0,
        RelieverChargesPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        Others DECIMAL(18,2) NOT NULL DEFAULT 0,
        OthersPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        AdministrationCharges DECIMAL(18,2) NOT NULL DEFAULT 0,
        AdministrationChargesPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        ManagementFee DECIMAL(18,2) NOT NULL DEFAULT 0,
        ManagementFeePercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalPlusStatutory DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalDirectCost DECIMAL(18,2) NOT NULL DEFAULT 0,
        MonthlyChargedCost DECIMAL(18,2) NOT NULL DEFAULT 0
    );
    
    -- Create foreign key relationship
    ALTER TABLE QuotationDetails ADD CONSTRAINT FK_QuotationDetails_Quotation 
        FOREIGN KEY (QuotationID) REFERENCES Quotation(ID) ON DELETE CASCADE;
    
    PRINT 'QuotationDetails table created successfully.'
END
ELSE
BEGIN
    PRINT 'QuotationDetails table already exists. Checking for missing columns...'
    
    -- Check and add QuotationID column if missing
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuotationDetails' AND COLUMN_NAME = 'QuotationID')
    BEGIN
        ALTER TABLE QuotationDetails ADD QuotationID INT NOT NULL DEFAULT 0
        PRINT 'Added QuotationID column to QuotationDetails table.'
    END
END

-- Display final table structures
PRINT '=== Final Quotation Table Structure ==='
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Quotation'
ORDER BY ORDINAL_POSITION

PRINT '=== Final QuotationDetails Table Structure ==='
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'QuotationDetails'
ORDER BY ORDINAL_POSITION

PRINT '=== Quotation Tables Creation Complete ==='
