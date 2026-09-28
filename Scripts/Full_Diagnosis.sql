-- ============================================================
-- FULL DIAGNOSIS: Why Report is Empty for FWGG000059
-- Run each section one by one and check results
-- ============================================================

-- ============================================================
-- STEP 1: Employee basic info
-- ============================================================
SELECT 
    EMP_ID,
    EMP_CODE,
    EMP_NAME,
    EMP_ROLE,
    EMP_BRANCH_CODE     AS CurrentBranch,
    HasTransfered,
    TransferDate,
    OldBranch
FROM Employee
WHERE EMP_CODE = 'FWGG000059';

-- ============================================================
-- STEP 2: EmploymentDetails - join date, branch
-- ============================================================
SELECT 
    EMPPAY_ID,
    EMPPAY_CODE,
    EMPPAY_BRANCHCODE,
    EMPPAY_DATE_JOINED,
    EMPPAY_DATE_RESIGNED,
    EMPPAY_BASIC_RATE,
    SALARYLAB
FROM EmploymentDetails
WHERE EMPPAY_CODE = 'FWGG000059';

-- ============================================================
-- STEP 3: SalaryStructure linked - must NOT be NULL
-- ============================================================
SELECT 
    ss.SalaryId,
    ss.Name,
    ss.WorkingDays,
    ss.GeneralDayRate,
    ed.SALARYLAB
FROM EmploymentDetails ed
LEFT JOIN SalaryStructure ss ON ss.SalaryId = ed.SALARYLAB
WHERE ed.EMPPAY_CODE = 'FWGG000059';

-- ============================================================
-- STEP 4: Attendance saved for this employee?
-- ============================================================
SELECT 
    a.ID,
    a.Branch,
    a.Period,
    a.EmployeeID,
    COUNT(ad.ID) AS DayCount
FROM Attendance a
LEFT JOIN AttendanceDetails ad ON ad.AttendanceID = a.ID
WHERE a.EmployeeID = (SELECT EMP_ID FROM Employee WHERE EMP_CODE = 'FWGG000059')
GROUP BY a.ID, a.Branch, a.Period, a.EmployeeID
ORDER BY a.Period;

-- ============================================================
-- STEP 5: PaySlip table - salary processed?
-- ============================================================
SELECT 
    ps.ID,
    ps.Period,
    ps.EmployeeID,
    ps.BasicSalary,
    ps.BasicSalaryDays
FROM PaySlip ps
WHERE ps.EmployeeID = (SELECT EMP_ID FROM Employee WHERE EMP_CODE = 'FWGG000059')
ORDER BY ps.Period;

-- ============================================================
-- STEP 6: SalaryProcess table - was Process button clicked?
-- ============================================================
SELECT 
    ID,
    Branch,
    Period,
    EmployeeType,
    IsLocked,
    Remarks,
    LastUpdate
FROM SalaryProcess
ORDER BY LastUpdate DESC;

-- ============================================================
-- STEP 7: EmploymentDetailsHistory - transfer history
-- ============================================================
SELECT 
    edh.EMPPAY_HISTORY_ID,
    edh.EMPPAY_BRANCHCODE,
    edh.Emp_StartDate,
    edh.Emp_EndDate,
    CASE 
        WHEN edh.Emp_EndDate IS NULL THEN 'ACTIVE'
        ELSE 'CLOSED'
    END AS Status
FROM EmploymentDetailsHistory edh
WHERE edh.EMP_ID = (SELECT EMP_ID FROM Employee WHERE EMP_CODE = 'FWGG000059')
ORDER BY edh.Emp_StartDate;

-- ============================================================
-- STEP 8: Designation - must exist (view joins it)
-- ============================================================
SELECT 
    e.EMP_CODE,
    e.DesignationId,
    d.DesignationId,
    d.Desig_Name
FROM Employee e
LEFT JOIN Designation d ON d.DesignationId = e.DesignationId
WHERE e.EMP_CODE = 'FWGG000059';
