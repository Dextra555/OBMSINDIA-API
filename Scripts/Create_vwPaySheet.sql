-- Create vwPaySheet view with correct join conditions
-- This view joins PaySlip with Employee and EmploymentDetails correctly
-- PaySlip.EmployeeID should join with Employee.EMP_ID (not EmploymentDetails.EMPPAY_ID)
-- Then Employee.EMP_CODE joins with EmploymentDetails.EMPPAY_CODE

USE [obmsdev_backup]
GO

IF OBJECT_ID('vwPaySheet', 'V') IS NOT NULL
    DROP VIEW vwPaySheet
GO

CREATE VIEW [dbo].[vwPaySheet]
AS
SELECT DISTINCT
    TOP 100 PERCENT
    PaySlip.ID AS PaySlipID,
    PaySlip.Period,
    PaySlip.EmployeeID,
    Employee.EMP_CODE,
    Employee.EMP_NAME,
    Employee.EMP_IC_OLD,
    Employee.EMP_IC_NEW,
    Employee.EMP_PASSPORT_NO,
    Employee.EMP_ROLE,
    BranchMaster.Code AS BranchCode,
    BranchMaster.Name AS BranchName,
    EmploymentDetails.EMPPAY_ID AS EmploymentDetailsID,
    EmploymentDetails.EMPPAY_BASIC_RATE,
    EmploymentDetails.ATTENDANCEALLOWANCE AS EmploymentAttendanceAllowance,
    EmploymentDetails.SpecialAllowance AS EmploymentSpecialAllowance,
    PaySlip.BasicSalaryDays,
    PaySlip.BasicSalaryRate,
    PaySlip.BasicSalary,
    PaySlip.OverTimeSalaryHours,
    PaySlip.OverTimeSalaryRate,
    PaySlip.OverTimeSalary,
    PaySlip.OffDaySalaryDays,
    PaySlip.OffDaySalaryRate,
    PaySlip.OffDaySalary,
    PaySlip.OffDayOverTimeSalaryHours,
    PaySlip.OffDayOverTimeSalaryRate,
    PaySlip.OffDayOverTimeSalary,
    PaySlip.HolidaySalaryDays,
    PaySlip.HolidaySalaryRate,
    PaySlip.HolidaySalary,
    PaySlip.HolidayOverTimeSalaryHours,
    PaySlip.HolidayOverTimeSalaryRate,
    PaySlip.HolidayOverTimeSalary,
    PaySlip.Shift2SalaryDaysHours,
    PaySlip.Shift2SalaryRateType,
    PaySlip.Shift2SalaryRate,
    PaySlip.Shift2Salary,
    PaySlip.ReAllowanceDays,
    PaySlip.ReAllowance,
    PaySlip.AttendanceAllowance,
    PaySlip.SpecialAllowance,
    PaySlip.EPFDeductionAmount,
    PaySlip.SOCSODeductionAmount,
    PaySlip.EPFEmployerContribution,
    PaySlip.SOCSOEmployerContribution,
    PaySlip.DailyAdvanceRecovery,
    PaySlip.SpecialAdvanceRecovery,
    PaySlip.MonthlyAdvanceRecovery,
    PaySlip.UniformIssueRecovery,
    PaySlip.LoanRecovery,
    PaySlip.IncomeTaxDeduction,
    PaySlip.MiscAmount,
    PaySlip.MiscDeduction,
    PaySlip.SalaryPayMode,
    PaySlip.Bonus,
    PaySlip.SIPEmployeeContribution,
    PaySlip.SIPEmployerContribution,
    PaySlip.LastUpdate,
    PaySlip.LastUpdatedBy,
    EmployeeSalaryDetails.EMPFL_BANK,
    EmployeeSalaryDetails.EMPFL_BK_ACCNO,
    EmployeeSalaryDetails.Paymode,
    SalaryProcess.IsLocked,
    SalaryProcess.Remarks
FROM
    PaySlip
    INNER JOIN Employee ON PaySlip.EmployeeID = Employee.EMP_ID
    LEFT JOIN EmploymentDetails ON Employee.EMP_CODE = EmploymentDetails.EMPPAY_CODE
    LEFT JOIN EmployeeSalaryDetails ON EmployeeSalaryDetails.EMPFL_CODE = Employee.EMP_CODE
    LEFT JOIN BranchMaster ON Employee.EMP_BRANCH_CODE = BranchMaster.Code
    LEFT JOIN SalaryProcess ON SalaryProcess.Period = PaySlip.Period 
        AND SalaryProcess.Branch = BranchMaster.Code 
        AND SalaryProcess.EmployeeType = Employee.EMP_ROLE
GO

PRINT 'vwPaySheet view created successfully.'
GO
