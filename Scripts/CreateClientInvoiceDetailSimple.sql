-- Create ClientInvoiceDetail table without foreign key for now
USE [obms];
GO

PRINT '=== Creating ClientInvoiceDetail Table (Simple Version) ==='

-- Drop the table if it exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClientInvoiceDetail')
BEGIN
    PRINT 'Dropping existing ClientInvoiceDetail table...'
    DROP TABLE ClientInvoiceDetail
END

PRINT 'Creating ClientInvoiceDetail table...'

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

PRINT 'ClientInvoiceDetail table created successfully (without foreign key).'

PRINT '=== ClientInvoiceDetail Creation Complete ==='

-- Show table structure
PRINT '=== ClientInvoiceDetail Table Structure ==='
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ClientInvoiceDetail'
ORDER BY ORDINAL_POSITION;

PRINT '=== Final Verification ==='
SELECT 'ClientInvoiceDetail' AS TableName, 'EXISTS' AS Status
UNION ALL
SELECT 'Quotation' AS TableName, 
       CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Quotation') 
            THEN 'EXISTS' ELSE 'MISSING' END AS Status
UNION ALL
SELECT 'QuotationDetails' AS TableName, 
       CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QuotationDetails') 
            THEN 'EXISTS' ELSE 'MISSING' END AS Status;
