-- Fix ClientInvoiceDetail table to match ClientInvoice structure
USE [obms];
GO

PRINT '=== Fixing ClientInvoiceDetail Table ==='

-- Drop the table if it exists and recreate with correct structure
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClientInvoiceDetail')
BEGIN
    PRINT 'Dropping existing ClientInvoiceDetail table...'
    DROP TABLE ClientInvoiceDetail
END

PRINT 'Creating ClientInvoiceDetail table with correct structure...'

CREATE TABLE ClientInvoiceDetail (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Branch NVARCHAR(50) NOT NULL,
    InvoiceNo NVARCHAR(50) NOT NULL,
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

-- Add foreign key constraint to match ClientInvoice composite key
ALTER TABLE ClientInvoiceDetail ADD CONSTRAINT FK_ClientInvoiceDetail_ClientInvoice 
    FOREIGN KEY (Branch, InvoiceNo) REFERENCES ClientInvoice(Branch, InvoiceNo);

PRINT 'ClientInvoiceDetail table created successfully with correct foreign key.'

PRINT '=== ClientInvoiceDetail Fix Complete ==='

-- Show table structure
PRINT '=== ClientInvoiceDetail Table Structure ==='
SELECT TOP 10
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ClientInvoiceDetail'
ORDER BY ORDINAL_POSITION;
