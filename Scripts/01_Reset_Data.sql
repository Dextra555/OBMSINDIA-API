-- ============================================================
-- OBMS Database Reset Script
-- Purpose: Remove all transactional & master data (schema intact)
-- Order: FK children first, parents last
-- ============================================================

USE [obms];
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '=== Starting OBMS Database Reset ==='

-- Disable FK checks temporarily
EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- ---- Step 1: Attendance & Related ----
PRINT 'Clearing Attendance data...'
IF OBJECT_ID('AttendanceDetails', 'U') IS NOT NULL DELETE FROM AttendanceDetails;
IF OBJECT_ID('Attendance', 'U') IS NOT NULL DELETE FROM Attendance;

-- ---- Step 2: Payroll & Salary ----
PRINT 'Clearing Payroll data...'
IF OBJECT_ID('PaySlip', 'U') IS NOT NULL DELETE FROM PaySlip;
IF OBJECT_ID('MiscTrans', 'U') IS NOT NULL DELETE FROM MiscTrans;
IF OBJECT_ID('SalaryAdvance', 'U') IS NOT NULL DELETE FROM SalaryAdvance;
IF OBJECT_ID('SalaryProcess', 'U') IS NOT NULL DELETE FROM SalaryProcess;
IF OBJECT_ID('AdvanceRepayment', 'U') IS NOT NULL DELETE FROM AdvanceRepayment;
IF OBJECT_ID('SalaryStructure', 'U') IS NOT NULL DELETE FROM SalaryStructure;

-- ---- Step 3: Inventory & Items ----
PRINT 'Clearing Inventory data...'
IF OBJECT_ID('EmployeeItemIssue', 'U') IS NOT NULL DELETE FROM EmployeeItemIssue;
IF OBJECT_ID('StockIssueDetail', 'U') IS NOT NULL DELETE FROM StockIssueDetail;
IF OBJECT_ID('StockIssues', 'U') IS NOT NULL DELETE FROM StockIssues;

-- ---- Step 4: Finance & Invoices ----
PRINT 'Clearing Finance data...'
IF OBJECT_ID('ClientInvoiceDetail', 'U') IS NOT NULL DELETE FROM ClientInvoiceDetail;
IF OBJECT_ID('ClientInvoice', 'U') IS NOT NULL DELETE FROM ClientInvoice;
IF OBJECT_ID('CreditorInvoiceDetails', 'U') IS NOT NULL DELETE FROM CreditorInvoiceDetails;
IF OBJECT_ID('CreditorInvoice', 'U') IS NOT NULL DELETE FROM CreditorInvoice;
IF OBJECT_ID('ReceiptDetail', 'U') IS NOT NULL DELETE FROM ReceiptDetail;
IF OBJECT_ID('Receipts', 'U') IS NOT NULL DELETE FROM Receipts;
IF OBJECT_ID('BranchPaymentDetails', 'U') IS NOT NULL DELETE FROM BranchPaymentDetails;
IF OBJECT_ID('BranchPayment', 'U') IS NOT NULL DELETE FROM BranchPayment;

-- ---- Step 5: Agreements ----
PRINT 'Clearing Agreement data...'
IF OBJECT_ID('TerminatedAgreement', 'U') IS NOT NULL DELETE FROM TerminatedAgreement;
IF OBJECT_ID('AgreementDetails', 'U') IS NOT NULL DELETE FROM AgreementDetails;
IF OBJECT_ID('Agreement', 'U') IS NOT NULL DELETE FROM Agreement;
IF OBJECT_ID('ClientLegalDemandAction', 'U') IS NOT NULL DELETE FROM ClientLegalDemandAction;

-- ---- Step 6: Employee Records ----
PRINT 'Clearing Employee data...'
IF OBJECT_ID('EmployeeHistory', 'U') IS NOT NULL DELETE FROM EmployeeHistory;
IF OBJECT_ID('EmployeeSalaryDetails', 'U') IS NOT NULL DELETE FROM EmployeeSalaryDetails;
IF OBJECT_ID('EmploymentDetails', 'U') IS NOT NULL DELETE FROM EmploymentDetails;
IF OBJECT_ID('Employee', 'U') IS NOT NULL DELETE FROM Employee;

-- ---- Step 7: Client Master ----
PRINT 'Clearing Client data...'
IF OBJECT_ID('ClientMaster', 'U') IS NOT NULL DELETE FROM ClientMaster;

-- ---- Step 8: Branch Master ----
PRINT 'Clearing Branch data...'
IF OBJECT_ID('OBMSBranches', 'U') IS NOT NULL DELETE FROM OBMSBranches;
IF OBJECT_ID('BranchMaster', 'U') IS NOT NULL DELETE FROM BranchMaster;

-- ---- Step 8.5: Statutory Configs ----
PRINT 'Clearing Statutory Config data...'
IF OBJECT_ID('ProfessionalTaxConfiguration', 'U') IS NOT NULL DELETE FROM ProfessionalTaxConfiguration;
IF OBJECT_ID('PFConfiguration', 'U') IS NOT NULL DELETE FROM PFConfiguration;
IF OBJECT_ID('ESIConfiguration', 'U') IS NOT NULL DELETE FROM ESIConfiguration;
IF OBJECT_ID('TDSSlabConfiguration', 'U') IS NOT NULL DELETE FROM TDSSlabConfiguration;
IF OBJECT_ID('GSTConfiguration', 'U') IS NOT NULL DELETE FROM GSTConfiguration;

-- ---- Step 9: Users (keep system admin) ----
-- We keep Obmsuser intact so login still works
-- IF OBJECT_ID('Obmsuser', 'U') IS NOT NULL DELETE FROM Obmsuser;

-- Re-enable FK checks
EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

-- Reset identity seeds
PRINT 'Resetting identity seeds...'
IF OBJECT_ID('AttendanceDetails', 'U') IS NOT NULL DBCC CHECKIDENT ('AttendanceDetails', RESEED, 0);
IF OBJECT_ID('Attendance', 'U') IS NOT NULL DBCC CHECKIDENT ('Attendance', RESEED, 0);
IF OBJECT_ID('PaySlip', 'U') IS NOT NULL DBCC CHECKIDENT ('PaySlip', RESEED, 0);
IF OBJECT_ID('SalaryAdvance', 'U') IS NOT NULL DBCC CHECKIDENT ('SalaryAdvance', RESEED, 0);
IF OBJECT_ID('Employee', 'U') IS NOT NULL DBCC CHECKIDENT ('Employee', RESEED, 0);
IF OBJECT_ID('EmploymentDetails', 'U') IS NOT NULL DBCC CHECKIDENT ('EmploymentDetails', RESEED, 0);
IF OBJECT_ID('EmployeeSalaryDetails', 'U') IS NOT NULL DBCC CHECKIDENT ('EmployeeSalaryDetails', RESEED, 0);
IF OBJECT_ID('ClientMaster', 'U') IS NOT NULL DBCC CHECKIDENT ('ClientMaster', RESEED, 0);
IF OBJECT_ID('BranchMaster', 'U') IS NOT NULL DBCC CHECKIDENT ('BranchMaster', RESEED, 0);
IF OBJECT_ID('Agreement', 'U') IS NOT NULL DBCC CHECKIDENT ('Agreement', RESEED, 0);

PRINT '=== Database Reset Complete ==='

-- Verification
SELECT 'BranchMaster' AS TableName, COUNT(*) AS RecordCount FROM BranchMaster
UNION ALL SELECT 'ClientMaster', COUNT(*) FROM ClientMaster
UNION ALL SELECT 'Employee', COUNT(*) FROM Employee
UNION ALL SELECT 'EmploymentDetails', COUNT(*) FROM EmploymentDetails
UNION ALL SELECT 'EmployeeSalaryDetails', COUNT(*) FROM EmployeeSalaryDetails
UNION ALL SELECT 'Attendance', COUNT(*) FROM Attendance
UNION ALL SELECT 'AttendanceDetails', COUNT(*) FROM AttendanceDetails
UNION ALL SELECT 'PaySlip', COUNT(*) FROM PaySlip
UNION ALL SELECT 'Agreement', COUNT(*) FROM Agreement;
