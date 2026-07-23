-- =============================================
-- Script to delete data from all tables except master tables
-- Keeps: Employee, ClientMaster, BranchMaster, Department, Designation
-- Keeps: System configuration tables
-- =============================================

USE obmsdev_backup;
GO

-- Disable foreign key constraints
ALTER TABLE NOCHECK CONSTRAINT ALL;
GO

-- Delete data from child tables first (in order of dependencies)

-- Attendance and related
DELETE FROM AttendanceDetails;
DELETE FROM Attendance;

-- Payroll and salary related
DELETE FROM SalaryAdvance;
DELETE FROM PaySlipAudit;
DELETE FROM PaySlip;
DELETE FROM SalaryProcessAudit;
DELETE FROM SalaryProcess;

-- Agreement related
DELETE FROM AgreementDetails;
DELETE FROM Agreement;
DELETE FROM TerminatedAgreements;

-- Quotation related
DELETE FROM QuotationDetails;
DELETE FROM Quotation;

-- Invoice related
DELETE FROM ClientInvoiceDetails;
DELETE FROM ClientInvoice;
DELETE FROM CreditorInvoiceDetails;
DELETE FROM CreditorInvoice;

-- Receipt related
DELETE FROM ReceiptDetails;
DELETE FROM Receipts;
DELETE FROM RECEIPIENT;

-- Branch payment related
DELETE FROM BranchPaymentDetails;
DELETE FROM BranchPayments;

-- Employee related (except Employee table)
DELETE FROM EmployeeItemIssue;
DELETE FROM EmployeeHistory;
DELETE FROM Employee_CB_Backup_20260415;
DELETE FROM EmploymentDetails;
DELETE FROM EmployeeSalaryDetails;

-- Inventory and stock
DELETE FROM StockIssueDetail;
DELETE FROM StockIssues;
DELETE FROM ItemMaster;
DELETE FROM InventoryCategory;
DELETE FROM AssetMaster;

-- Leave system
DELETE FROM LeaveSystem;

-- Miscellaneous transactions
DELETE FROM MiscTrans;

-- Supplier related
DELETE FROM Suppliers;

-- Tax and configuration tables (delete data but keep structure)
DELETE FROM AccountCategory;
DELETE FROM Acct_GLReport;
DELETE FROM AdvanceRepayment;
DELETE FROM AllDayMonth;
DELETE FROM BankList;
DELETE FROM BankMaster;
DELETE FROM ChequeMaster;
DELETE FROM CommercialBreakdown;
DELETE FROM Configuration;
DELETE FROM EPF;
DELETE FROM ESIConfiguration;
DELETE FROM GSTConfiguration;
DELETE FROM IncomeTax;
DELETE FROM IndianBanks;
DELETE FROM IndianStates;
DELETE FROM OBMSBanks;
DELETE FROM OBMSBranches;
DELETE FROM obmsdev_backupBanks;
DELETE FROM obmsdev_backupBranches;
DELETE FROM obmsdev_backupPermissions;
DELETE FROM obmsdev_backupUsers;
DELETE FROM PFConfiguration;
DELETE FROM ProfessionalTaxConfiguration;
DELETE FROM PayrollConfiguration;
DELETE FROM SIP;
DELETE FROM SOCSO;
DELETE FROM SOCSO_backup;
DELETE FROM TDSSlabConfiguration;
DELETE FROM tblCompanyRegistration;

-- Client legal demand
DELETE FROM ClientLegalDemandAction;

-- Report tables
DELETE FROM rClientStatement;
DELETE FROM rSupplierStatement;

-- Salary structure (keep the table, delete data)
DELETE FROM SalaryStructure;
DELETE FROM oSalaryStructure;

-- Service types
DELETE FROM ServiceType;

-- Screen list
DELETE FROM ScreenList;

-- Re-enable foreign key constraints
ALTER TABLE CHECK CONSTRAINT ALL;
GO

-- Verify deletion
SELECT 
    TABLE_NAME,
    CASE 
        WHEN TABLE_NAME IN ('Employee', 'ClientMaster', 'BranchMaster', 'Department', 'Designation',
                           '__EFMigrationsHistory', 'OBMSUsers', 'OBMSPermissions', 'OBMSBranches', 'OBMSBanks')
        THEN 'KEPT - Master/System Table'
        ELSE 'DATA DELETED'
    END as Status
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

PRINT 'Data deletion completed successfully.';
PRINT 'Master tables (Employee, ClientMaster, BranchMaster, Department, Designation) preserved.';
PRINT 'System configuration tables preserved.';
GO
