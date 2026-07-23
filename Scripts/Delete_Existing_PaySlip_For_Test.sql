-- Delete existing PaySlip and related records for employee 106, March 2026
-- This allows re-running salary process with the fixed code
USE [obmsdev_backup]
GO

-- Delete AdvanceRepayment records for the PaySlip
DELETE FROM AdvanceRepayment 
WHERE PaySlipID IN (SELECT ID FROM PaySlip WHERE EmployeeID=106 AND Period='2026-03-31')
PRINT 'Deleted AdvanceRepayment records for employee 106, March 2026'
GO

-- Delete PaySlipAudit records
DELETE FROM PaySlipAudit 
WHERE EmployeeID=106 AND Period='2026-03-31'
PRINT 'Deleted PaySlipAudit records for employee 106, March 2026'
GO

-- Delete PaySlip record
DELETE FROM PaySlip 
WHERE EmployeeID=106 AND Period='2026-03-31'
PRINT 'Deleted PaySlip record for employee 106, March 2026'
GO

-- Verify deletion
SELECT COUNT(*) AS RemainingPaySlips FROM PaySlip WHERE EmployeeID=106 AND Period='2026-03-31'
SELECT COUNT(*) AS RemainingAdvanceRepayments FROM AdvanceRepayment WHERE PaySlipID IN (SELECT ID FROM PaySlip WHERE EmployeeID=106 AND Period='2026-03-31')
GO

PRINT 'Cleanup completed. Now you can run salary process again for employee 106, March 2026'
GO
