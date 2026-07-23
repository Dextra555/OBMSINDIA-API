-- Add additional CB columns and Bank Account to vwTimeSheet view
USE [obmsdev_backup]
GO

-- Drop existing view
IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vwTimeSheet')
BEGIN
    DROP VIEW vwTimeSheet
    PRINT 'Dropped existing vwTimeSheet view'
END
GO

-- Create vwTimeSheet view with added CB columns from Employee table
CREATE VIEW vwTimeSheet
AS
SELECT DISTINCT TOP 100 PERCENT
    EmployeeSalaryDetails.EMPFL_BK_ACCNO, 
    EmployeeSalaryDetails.EMPFL_EPFNO,    
    EmployeeSalaryDetails.EMPFL_SOSCO_NO, 
    EmploymentDetails.EMPPAY_DATE_JOINED, 
    Employee.EMP_NAME, 
    Employee.EMP_DATE_OF_BIRTH, 
    Employee.EMP_IC_OLD,         
    Employee.EMP_IC_NEW, 
    Employee.EMP_PASSPORT_NO, 
    PaySlip.Period,              
    BranchMaster.Code, 
    Employee.EMP_CODE,
    PaySlip.BasicSalary,                   
    PaySlip.OverTimeSalary, 
    PaySlip.OffDaySalary, 
    PaySlip.OffDayOverTimeSalary, 
    PaySlip.HolidaySalary, 
    PaySlip.HolidayOverTimeSalary, 
    PaySlip.Shift2Salary, 
    PaySlip.SpecialAllowance,
    PaySlip.AttendanceAllowance, 
    PaySlip.EPFDeductionAmount,                                 
    PaySlip.SOCSODeductionAmount, 
    PaySlip.EPFEmployerContribution, 
    PaySlip.SOCSOEmployerContribution,                 
    PaySlip.DailyAdvanceRecovery, 
    PaySlip.MonthlyAdvanceRecovery, 
    PaySlip.SpecialAdvanceRecovery,                     
    PaySlip.UniformIssueRecovery, 
    PaySlip.Bonus,                                
    PaySlip.LoanRecovery, 
    PaySlip.MiscDeduction,
    EmploymentDetails.EMPPAY_BASIC_RATE,                                 
    PaySlip.BasicSalaryDays, 
    PaySlip.BasicSalaryRate, 
    PaySlip.OverTimeSalaryHours,                                    
    PaySlip.OverTimeSalaryRate, 
    PaySlip.OffDaySalaryDays, 
    PaySlip.OffDaySalaryRate,                                   
    PaySlip.OffDayOverTimeSalaryHours, 
    PaySlip.OffDayOverTimeSalaryRate, 
    PaySlip.HolidaySalaryDays,                   
    PaySlip.HolidaySalaryRate, 
    PaySlip.HolidayOverTimeSalaryHours, 
    PaySlip.HolidayOverTimeSalaryRate,                 
    PaySlip.ReAllowance, 
    Employee.EMP_ROLE,
    PaySlip.EmployeeID, 
    PaySlip.ReAllowanceDays,                              
    PaySlip.MiscAmount, 
    PaySlip.IncomeTaxDeduction,
    TableClient.Client,
    -- CB columns from Employee table
    Employee.CB_Basic,
    Employee.CB_DA,
    Employee.CB_HRA,
    Employee.CB_Leaves,
    Employee.CB_OtherAllowances,
    Employee.CB_NH,
    Employee.CB_SubTotal,
    Employee.CB_PF,
    Employee.CB_ESI,
    Employee.CB_Advance,
    Employee.PF_AccountNumber,
    Employee.ESI_Number,
    Employee.BankAccountNumber
FROM (EmployeeSalaryDetails INNER JOIN 
    (EmploymentDetails INNER JOIN        
    (PaySlip INNER JOIN Employee ON PaySlip.EmployeeID=Employee.EMP_ID)         
    ON EmploymentDetails.EMPPAY_CODE=Employee.EMP_CODE) 
    ON EmployeeSalaryDetails.EMPFL_CODE=Employee.EMP_CODE)        
INNER JOIN BranchMaster ON Employee.EMP_BRANCH_CODE=BranchMaster.Code       
INNER JOIN Attendance ON Attendance.EmployeeID=Employee.EMP_ID              
INNER JOIN AttendanceDetails ON AttendanceDetails.AttendanceID=Attendance.ID
INNER JOIN (
    SELECT EmployeeID,Client,Branch,Attendance.Period                           
    FROM Attendance INNER JOIN AttendanceDetails ON AttendanceDetails.AttendanceID = Attendance.ID         
    UNION
    SELECT EmployeeID,OTClient as Client,Branch,Attendance.Period               
    FROM Attendance INNER JOIN AttendanceDetails ON AttendanceDetails.AttendanceID = Attendance.ID         
) TableClient ON TableClient.EmployeeID = Employee.EMP_ID AND TableClient.Period = PaySlip.Period                   
ORDER BY Employee.EMP_NAME
GO

PRINT 'vwTimeSheet view created successfully with additional CB columns'
GO

-- Verify the view was created
SELECT COUNT(*) AS ViewRowCount FROM vwTimeSheet
GO
