-- Create ClientInvoiceDetail table if it doesn't exist
USE [obms];
GO

PRINT '=== Creating ClientInvoiceDetail Table ==='

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClientInvoiceDetail')
BEGIN
    PRINT 'Creating ClientInvoiceDetail table...'
    
    -- Based on typical invoice detail structure and the error context
    CREATE TABLE ClientInvoiceDetail (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceID INT NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        Quantity DECIMAL(18,2) NOT NULL DEFAULT 1,
        Rate DECIMAL(18,2) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(18,2) NOT NULL,
        HSNCode NVARCHAR(20) NULL,
        ServiceType NVARCHAR(100) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        LastUpdated DATETIME NOT NULL DEFAULT GETDATE(),
        LastUpdatedBy NVARCHAR(50) NULL
    );
    
    -- Add foreign key constraint if ClientInvoice table exists
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClientInvoice')
    BEGIN
        ALTER TABLE ClientInvoiceDetail ADD CONSTRAINT FK_ClientInvoiceDetail_ClientInvoice 
            FOREIGN KEY (InvoiceID) REFERENCES ClientInvoice(ID) ON DELETE CASCADE;
        PRINT 'Foreign key constraint added to ClientInvoiceDetail.'
    END
    
    PRINT 'ClientInvoiceDetail table created successfully.'
END
ELSE
BEGIN
    PRINT 'ClientInvoiceDetail table already exists.'
    
    -- Check for HSNCode column specifically (since it was mentioned in the error)
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientInvoiceDetail' AND COLUMN_NAME = 'HSNCode')
    BEGIN
        ALTER TABLE ClientInvoiceDetail ADD HSNCode NVARCHAR(20) NULL
        PRINT 'Added HSNCode column to ClientInvoiceDetail table.'
    END
END

PRINT '=== ClientInvoiceDetail Creation Complete ==='

-- Show table structure
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClientInvoiceDetail')
BEGIN
    PRINT '=== ClientInvoiceDetail Table Structure ==='
    SELECT 
        COLUMN_NAME, 
        DATA_TYPE, 
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ClientInvoiceDetail'
    ORDER BY ORDINAL_POSITION;
END
