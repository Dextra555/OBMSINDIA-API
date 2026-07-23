-- Check for all tables that might be referenced in the application
USE [obms];
GO

PRINT '=== Checking All Required Tables ==='

-- List of tables that should exist based on the models
DECLARE @RequiredTables TABLE (TableName NVARCHAR(100), Description NVARCHAR(200))
INSERT INTO @RequiredTables VALUES 
('Quotation', 'Main quotation table'),
('QuotationDetails', 'Quotation line items'),
('Agreement', 'Main agreement table'),
('AgreementDetails', 'Agreement line items'),
('ClientMaster', 'Client master data'),
('BranchMaster', 'Branch master data'),
('Employee', 'Employee records'),
('Attendance', 'Attendance records'),
('AttendanceDetails', 'Attendance line items'),
('ServiceType', 'Service types for invoicing'),
('ClientInvoice', 'Client invoices'),
('ClientInvoiceDetail', 'Client invoice details')

-- Check each table
SELECT 
    rt.TableName,
    rt.Description,
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = rt.TableName) 
        THEN 'EXISTS' 
        ELSE 'MISSING' 
    END AS Status
FROM @RequiredTables rt
ORDER BY rt.TableName

PRINT '=== Check Complete ==='
