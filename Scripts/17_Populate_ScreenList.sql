-- ============================================================
-- Populate ScreenList with all necessary screens
-- Purpose: Ensure ScreenList table has data for all categories
-- ============================================================

USE OBMSDB;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '=== Populating ScreenList Table ==='

-- Clear existing data (optional - comment out if you want to preserve existing data)
-- DELETE FROM ScreenList
-- GO

-- Accounting Category Screens
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Trial Balance' AND Category = 'Accounting')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Trial Balance', 'Accounting', GETDATE())
    PRINT 'Added: Trial Balance (Accounting)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'General Ledger' AND Category = 'Accounting')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('General Ledger', 'Accounting', GETDATE())
    PRINT 'Added: General Ledger (Accounting)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Balance Sheet' AND Category = 'Accounting')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Balance Sheet', 'Accounting', GETDATE())
    PRINT 'Added: Balance Sheet (Accounting)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Bank Reconciliation' AND Category = 'Accounting')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Bank Reconciliation', 'Accounting', GETDATE())
    PRINT 'Added: Bank Reconciliation (Accounting)'
END

-- Administration Category Screens
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'User Access Rights' AND Category = 'Administration')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('User Access Rights', 'Administration', GETDATE())
    PRINT 'Added: User Access Rights (Administration)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'User Registration' AND Category = 'Administration')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('User Registration', 'Administration', GETDATE())
    PRINT 'Added: User Registration (Administration)'
END

-- Master Category Screens
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Branch Master' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Branch Master', 'Master', GETDATE())
    PRINT 'Added: Branch Master (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Client Master' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Client Master', 'Master', GETDATE())
    PRINT 'Added: Client Master (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Employee Master' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Employee Master', 'Master', GETDATE())
    PRINT 'Added: Employee Master (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Service Type' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Service Type', 'Master', GETDATE())
    PRINT 'Added: Service Type (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Designation Master' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Designation Master', 'Master', GETDATE())
    PRINT 'Added: Designation Master (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Department Master' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Department Master', 'Master', GETDATE())
    PRINT 'Added: Department Master (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'State Master' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('State Master', 'Master', GETDATE())
    PRINT 'Added: State Master (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'GST Slab' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('GST Slab', 'Master', GETDATE())
    PRINT 'Added: GST Slab (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Salary Slab' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Salary Slab', 'Master', GETDATE())
    PRINT 'Added: Salary Slab (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'EPF Slab' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('EPF Slab', 'Master', GETDATE())
    PRINT 'Added: EPF Slab (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'ESI Slab' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('ESI Slab', 'Master', GETDATE())
    PRINT 'Added: ESI Slab (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'SIP Slab' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('SIP Slab', 'Master', GETDATE())
    PRINT 'Added: SIP Slab (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'SOCSO Slab' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('SOCSO Slab', 'Master', GETDATE())
    PRINT 'Added: SOCSO Slab (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Income Tax Slab' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Income Tax Slab', 'Master', GETDATE())
    PRINT 'Added: Income Tax Slab (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Leave Slab' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Leave Slab', 'Master', GETDATE())
    PRINT 'Added: Leave Slab (Master)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'TDS Slab' AND Category = 'Master')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('TDS Slab', 'Master', GETDATE())
    PRINT 'Added: TDS Slab (Master)'
END

-- Quotation & Agreement Category Screens
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Quotations' AND Category = 'Quotation & Agreement')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Quotations', 'Quotation & Agreement', GETDATE())
    PRINT 'Added: Quotations (Quotation & Agreement)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Agreements' AND Category = 'Quotation & Agreement')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Agreements', 'Quotation & Agreement', GETDATE())
    PRINT 'Added: Agreements (Quotation & Agreement)'
END

-- Inventory Category Screens
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Item Master' AND Category = 'Inventory')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Item Master', 'Inventory', GETDATE())
    PRINT 'Added: Item Master (Inventory)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Category Master' AND Category = 'Inventory')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Category Master', 'Inventory', GETDATE())
    PRINT 'Added: Category Master (Inventory)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Asset Master' AND Category = 'Inventory')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Asset Master', 'Inventory', GETDATE())
    PRINT 'Added: Asset Master (Inventory)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Supplier Master' AND Category = 'Inventory')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Supplier Master', 'Inventory', GETDATE())
    PRINT 'Added: Supplier Master (Inventory)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Recipient Master' AND Category = 'Inventory')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Recipient Master', 'Inventory', GETDATE())
    PRINT 'Added: Recipient Master (Inventory)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Purchase Bills' AND Category = 'Inventory')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Purchase Bills', 'Inventory', GETDATE())
    PRINT 'Added: Purchase Bills (Inventory)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Material Issue' AND Category = 'Inventory')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Material Issue', 'Inventory', GETDATE())
    PRINT 'Added: Material Issue (Inventory)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Utility Bills' AND Category = 'Inventory')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Utility Bills', 'Inventory', GETDATE())
    PRINT 'Added: Utility Bills (Inventory)'
END

-- Finance Category Screens
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Invoice' AND Category = 'Finance')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Invoice', 'Finance', GETDATE())
    PRINT 'Added: Invoice (Finance)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Receipts' AND Category = 'Finance')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Receipts', 'Finance', GETDATE())
    PRINT 'Added: Receipts (Finance)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Payments' AND Category = 'Finance')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Payments', 'Finance', GETDATE())
    PRINT 'Added: Payments (Finance)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Credit Note' AND Category = 'Finance')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Credit Note', 'Finance', GETDATE())
    PRINT 'Added: Credit Note (Finance)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Legal Demand Action' AND Category = 'Finance')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Legal Demand Action', 'Finance', GETDATE())
    PRINT 'Added: Legal Demand Action (Finance)'
END

-- Payroll Category Screens
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Salary Processing' AND Category = 'Payroll')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Salary Processing', 'Payroll', GETDATE())
    PRINT 'Added: Salary Processing (Payroll)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Attendance' AND Category = 'Payroll')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Attendance', 'Payroll', GETDATE())
    PRINT 'Added: Attendance (Payroll)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Employee Loan' AND Category = 'Payroll')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Employee Loan', 'Payroll', GETDATE())
    PRINT 'Added: Employee Loan (Payroll)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Employee Monthly Advance' AND Category = 'Payroll')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Employee Monthly Advance', 'Payroll', GETDATE())
    PRINT 'Added: Employee Monthly Advance (Payroll)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Employee Uniform Loan' AND Category = 'Payroll')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Employee Uniform Loan', 'Payroll', GETDATE())
    PRINT 'Added: Employee Uniform Loan (Payroll)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Misc Transactions' AND Category = 'Payroll')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Misc Transactions', 'Payroll', GETDATE())
    PRINT 'Added: Misc Transactions (Payroll)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Employee Transfer' AND Category = 'Payroll')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Employee Transfer', 'Payroll', GETDATE())
    PRINT 'Added: Employee Transfer (Payroll)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'Employee History' AND Category = 'Payroll')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('Employee History', 'Payroll', GETDATE())
    PRINT 'Added: Employee History (Payroll)'
END

IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'RBI Bank Salary Process' AND Category = 'Payroll')
BEGIN
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('RBI Bank Salary Process', 'Payroll', GETDATE())
    PRINT 'Added: RBI Bank Salary Process (Payroll)'
END

PRINT ''
PRINT '=== Displaying All Screens in ScreenList ==='
SELECT Category, ScreenName, LastUpdate
FROM ScreenList
ORDER BY Category, ScreenName

PRINT ''
PRINT '=== Script Complete ==='
