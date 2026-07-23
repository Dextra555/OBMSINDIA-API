-- ============================================================
-- Debug Script for May Advance Data Not Showing
-- Purpose: Check if advance data exists and why it's not appearing
-- ============================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '=== Debugging May Advance Data Issue ==='

-- 1. Check if there are any PaySlip records with advance recoveries for May
PRINT ''
PRINT '=== Checking PaySlip records with advance recoveries ==='
SELECT 
    COUNT(*) as TotalRecords,
    SUM(DailyAdvanceRecovery) as TotalDailyAdvance,
    SUM(MonthlyAdvanceRecovery) as TotalMonthlyAdvance,
    SUM(SpecialAdvanceRecovery) as TotalSpecialAdvance
FROM PaySlip 
WHERE DailyAdvanceRecovery > 0 OR MonthlyAdvanceRecovery > 0 OR SpecialAdvanceRecovery > 0
AND MONTH(Period) = 5 AND YEAR(Period) = 2026  -- May 2026

-- 2. Check specific May PaySlip records
PRINT ''
PRINT '=== Sample May PaySlip records with advances ==='
SELECT TOP 10
    p.ID,
    p.EmployeeID,
    e.EMP_NAME,
    e.EMP_CODE,
    p.Period,
    p.DailyAdvanceRecovery,
    p.MonthlyAdvanceRecovery,
    p.SpecialAdvanceRecovery,
    (p.DailyAdvanceRecovery + p.MonthlyAdvanceRecovery + p.SpecialAdvanceRecovery) as TotalAdvanceRecovery
FROM PaySlip p
INNER JOIN Employee e ON p.EmployeeID = e.EMP_ID
WHERE (p.DailyAdvanceRecovery > 0 OR p.MonthlyAdvanceRecovery > 0 OR p.SpecialAdvanceRecovery > 0)
AND MONTH(p.Period) = 5 AND YEAR(p.Period) = 2025  -- May 2025
ORDER BY p.Period DESC

-- 3. Check if EmployeeSalaryDetails has the required bank account info
PRINT ''
PRINT '=== Checking EmployeeSalaryDetails for bank account info ==='
SELECT TOP 5
    e.EMP_NAME,
    e.EMP_CODE,
    esd.EMPFL_BK_ACCNO,
    esd.EMPFL_BRANCHCODE,
    p.Period,
    p.DailyAdvanceRecovery,
    p.MonthlyAdvanceRecovery,
    p.SpecialAdvanceRecovery
FROM PaySlip p
INNER JOIN Employee e ON p.EmployeeID = e.EMP_ID
INNER JOIN EmployeeSalaryDetails esd ON e.EMP_CODE = esd.EMPFL_CODE
WHERE (p.DailyAdvanceRecovery > 0 OR p.MonthlyAdvanceRecovery > 0 OR p.SpecialAdvanceRecovery > 0)
AND MONTH(p.Period) = 5 AND YEAR(p.Period) = 2025  -- May 2025
ORDER BY p.Period DESC

-- 4. Check if the RBI query would return data for May
PRINT ''
PRINT '=== Simulating RBI Bank Advance Export Query for May ==='
DECLARE @SalaryPeriod VARCHAR(20) = '2025-05-01'  -- May 2025 period
DECLARE @branch VARCHAR(50) = 'ALL'
DECLARE @employeeType VARCHAR(50) = 'ALL'

DECLARE @whereClause NVARCHAR(MAX) = 'WHERE PaySlip.Period >= @SalaryPeriod'

IF @branch != 'ALL'
BEGIN
    SET @whereClause += ' AND Employee.EMP_BRANCH_CODE = @Branch'
END

IF @employeeType != 'ALL'
BEGIN
    SET @whereClause += ' AND Employee.EMP_ROLE = @EmployeeType'
END

-- Execute the actual query used by RBI Bank Advance Export
SELECT
    Employee.EMP_CODE as BeneficiaryCode,
    EmployeeSalaryDetails.EMPFL_BK_ACCNO as BeneficiaryAccountNumber,
    Employee.EMP_NAME as BeneficiaryName,
    PaySlip.DailyAdvanceRecovery + PaySlip.MonthlyAdvanceRecovery + PaySlip.SpecialAdvanceRecovery as TransactionAmount,
    Employee.EMP_ID as CustomerReferenceNumber,
    EmployeeSalaryDetails.EMPFL_BRANCHCODE as PayerAddress1,
    Employee.IFSC_Code as IFSCCode,
    ISNULL(Employee.BankName, '') as BeneficiaryBankName,
    ISNULL(Employee.BankBranch, '') as BeneficiaryBankBranchName
FROM   (EmployeeSalaryDetails EmployeeSalaryDetails
INNER JOIN (PaySlip PaySlip INNER JOIN Employee Employee ON PaySlip.EmployeeID=Employee.EMP_ID)
ON EmployeeSalaryDetails.EMPFL_CODE=Employee.EMP_CODE)
INNER JOIN BranchMaster BankMaster ON Employee.EMP_BRANCH_CODE=BankMaster.Code 
WHERE PaySlip.Period >= @SalaryPeriod
AND (PaySlip.DailyAdvanceRecovery > 0 OR PaySlip.MonthlyAdvanceRecovery > 0 OR PaySlip.SpecialAdvanceRecovery > 0)
ORDER BY Employee.EMP_NAME

-- 5. Check SalaryAdvance table for comparison
PRINT ''
PRINT '=== Checking SalaryAdvance table for May advances ==='
SELECT TOP 10
    sa.ID,
    sa.EmployeeID,
    e.EMP_NAME,
    e.EMP_CODE,
    sa.AdvanceDate,
    sa.TransType,
    sa.Amount,
    sa.Particulars
FROM SalaryAdvance sa
INNER JOIN Employee e ON sa.EmployeeID = e.EMP_ID
WHERE MONTH(sa.AdvanceDate) = 5 AND YEAR(sa.AdvanceDate) = 2025  -- May 2025
ORDER BY sa.AdvanceDate DESC

PRINT ''
PRINT '=== Debug Complete ==='
PRINT 'If no data appears in the RBI query section, the issue might be:'
PRINT '1. No advance recoveries in PaySlip for May'
PRINT '2. Missing EmployeeSalaryDetails records'
PRINT '3. Missing Employee bank account information'
PRINT '4. Date format issues in the period parameter'
