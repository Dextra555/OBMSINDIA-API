-- ============================================================
-- Quick Check for Advance Data
-- Purpose: Simple test to see if advance data exists
-- ============================================================

PRINT '=== Quick Advance Data Check ==='

-- Check if there are ANY advances in May 2026
PRINT ''
PRINT '=== All advances in May 2026 ==='
SELECT COUNT(*) as TotalAdvances
FROM SalaryAdvance 
WHERE MONTH(AdvanceDate) = 5 AND YEAR(AdvanceDate) = 2026
AND IsDeleted = 0

-- Show sample advance records
PRINT ''
PRINT '=== Sample advance records for May 2026 ==='
SELECT TOP 5
    sa.ID,
    sa.EmployeeID,
    e.EMP_NAME,
    e.EMP_CODE,
    sa.AdvanceDate,
    sa.Amount,
    sa.TransType,
    sa.Particulars
FROM SalaryAdvance sa
INNER JOIN Employee e ON sa.EmployeeID = e.EMP_ID
WHERE MONTH(sa.AdvanceDate) = 5 AND YEAR(sa.AdvanceDate) = 2026
AND sa.IsDeleted = 0
ORDER BY sa.AdvanceDate DESC

-- Check if employees have bank account details
PRINT ''
PRINT '=== Check bank account details for employees with advances ==='
SELECT TOP 5
    e.EMP_NAME,
    e.EMP_CODE,
    esd.EMPFL_BK_ACCNO,
    sa.AdvanceDate,
    sa.Amount
FROM SalaryAdvance sa
INNER JOIN Employee e ON sa.EmployeeID = e.EMP_ID
LEFT JOIN EmployeeSalaryDetails esd ON e.EMP_CODE = esd.EMPFL_CODE
WHERE MONTH(sa.AdvanceDate) = 5 AND YEAR(sa.AdvanceDate) = 2026
AND sa.IsDeleted = 0
ORDER BY sa.AdvanceDate DESC

PRINT ''
PRINT '=== Check Complete ==='
