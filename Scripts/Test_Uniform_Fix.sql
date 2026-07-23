-- Test script to verify uniform loan fix
-- Run this after salary process to check if uniform details are now appearing

-- 1. Test the fixed query manually for a specific employee
DECLARE @EmployeeID INT = 1; -- Replace with actual employee ID
DECLARE @Period DATETIME = '2026-05-01'; -- Replace with current period

-- This is the fixed query from SalaryProcess.cs
SELECT SalaryAdvance.ID, SalaryAdvance.TransType, CAST((SalaryAdvance.Amount/SalaryAdvance.NoOfInstallments) AS Numeric(18,2)) AS Installment, 
       SalaryAdvance.Amount as Advance , ISNULL(Repayment.Payment,0) AS Payment 
FROM SalaryAdvance 
LEFT OUTER JOIN ( 
    SELECT AdvanceRepayment.AdvanceID , ISNULL(SUM(AdvanceRepayment.Amount),0) as Payment 
    FROM AdvanceRepayment 
    LEFT JOIN Payslip ON AdvanceRepayment.PaySlipID = PaySlip.ID 
    WHERE (PaySlip.Period<>@Period OR PaySlip.Period IS NULL) 
    GROUP BY AdvanceRepayment.AdvanceID 
) Repayment ON SalaryAdvance.ID = Repayment.AdvanceID  
WHERE SalaryAdvance.EmployeeID=@EmployeeID 
    AND SalaryAdvance.IsDeleted = 0 
    AND SalaryAdvance.AdvanceDate<= @Period 
    AND (SalaryAdvance.Amount-ISNULL(Repayment.Payment,0)>0 OR SalaryAdvance.TransType = 4)
ORDER BY SalaryAdvance.TransType;

-- 2. Check if uniform advances are being processed correctly
SELECT 
    sa.TransType,
    CASE sa.TransType
        WHEN 1 THEN 'Daily Advance'
        WHEN 2 THEN 'Monthly Advance' 
        WHEN 4 THEN 'Uniform Issue'
        WHEN 5 THEN 'Special Advance'
        ELSE 'Other'
    END as AdvanceType,
    COUNT(*) as Count,
    SUM(sa.Amount) as TotalAmount,
    SUM(ISNULL(ar.Payment, 0)) as TotalRepaid,
    SUM(sa.Amount - ISNULL(ar.Payment, 0)) as Balance
FROM SalaryAdvance sa
LEFT JOIN (
    SELECT AdvanceRepayment.AdvanceID, ISNULL(SUM(AdvanceRepayment.Amount),0) as Payment
    FROM AdvanceRepayment 
    GROUP BY AdvanceRepayment.AdvanceID
) ar ON sa.ID = ar.AdvanceID
WHERE sa.EmployeeID = @EmployeeID
    AND sa.IsDeleted = 0
    AND sa.AdvanceDate <= @Period
GROUP BY sa.TransType
ORDER BY sa.TransType;

-- 3. Check recent payslips for UniformIssueRecovery values
SELECT TOP 10
    ps.Period,
    ps.EmployeeID,
    e.Emp_Name,
    ps.UniformIssueRecovery,
    ps.BasicSalary,
    ps.DailyAdvanceRecovery,
    ps.MonthlyAdvanceRecovery,
    ps.SpecialAdvanceRecovery,
    ps.LoanRecovery,
    ps.LastUpdate
FROM PaySlip ps
INNER JOIN Employee e ON ps.EmployeeID = e.Emp_ID
WHERE ps.EmployeeID = @EmployeeID
ORDER BY ps.Period DESC;

-- 4. Debug: Check if there are any uniform advances that should be included
SELECT 
    sa.ID,
    sa.EmployeeID,
    e.Emp_Name,
    sa.TransType,
    sa.Amount,
    sa.NoOfInstallments,
    sa.AdvanceDate,
    sa.VoucherNo,
    ISNULL(ar.Payment, 0) as RepaidAmount,
    sa.Amount - ISNULL(ar.Payment, 0) as RemainingBalance,
    CASE 
        WHEN sa.Amount - ISNULL(ar.Payment, 0) > 0 THEN 'Should be included'
        WHEN sa.TransType = 4 THEN 'Uniform - Should be included regardless'
        ELSE 'Should not be included'
    END as InclusionStatus
FROM SalaryAdvance sa
INNER JOIN Employee e ON sa.EmployeeID = e.Emp_ID
LEFT JOIN (
    SELECT AdvanceRepayment.AdvanceID, ISNULL(SUM(AdvanceRepayment.Amount),0) as Payment
    FROM AdvanceRepayment 
    GROUP BY AdvanceRepayment.AdvanceID
) ar ON sa.ID = ar.AdvanceID
WHERE sa.EmployeeID = @EmployeeID
    AND sa.IsDeleted = 0
    AND sa.AdvanceDate <= @Period
ORDER BY sa.TransType, sa.AdvanceDate DESC;
