-- Verify the join condition issue in vwPaySheet
-- The view joins PaySlip.EmployeeID with EmploymentDetails.EMPPAY_ID
-- But PaySlip.EmployeeID should join with Employee.EMP_ID

-- Check EmploymentDetails structure
SELECT TOP 5
    EMPPAY_ID,
    EMPPAY_CODE,
    EMPPAY_BRANCHCODE
FROM EmploymentDetails
WHERE EMPPAY_CODE = 'FWGG000059';

-- Check the problematic join
SELECT 
    ps.ID AS PaySlipID,
    ps.EmployeeID AS PaySlipEmployeeID,
    ed.EMPPAY_ID AS EmploymentDetailsID,
    ed.EMPPAY_CODE AS EmploymentDetailsCode,
    e.EMP_ID AS EmployeeID,
    e.EMP_CODE AS EmployeeCode
FROM PaySlip ps
JOIN Employee e ON ps.EmployeeID = e.EMP_ID
LEFT JOIN EmploymentDetails ed ON e.EMP_CODE = ed.EMPPAY_CODE
WHERE e.EMP_CODE = 'FWGG000059';

-- The issue: PaySlip.EmployeeID (59) != EmploymentDetails.EMPPAY_ID (25)
-- The view is joining on wrong field
