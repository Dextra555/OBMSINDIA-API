ALTER VIEW [dbo].[vwPaySheet]
AS
SELECT DISTINCT   
    SalaryStructure.WorkingDays, 
    EmployeeSalaryDetails.epfdetect, 
    EmployeeSalaryDetails.SOCSODETECT, 
    Employee.CB_AdvanceStatutoryBonus, 
    EmploymentDetails.AttendanceAllowanceWorkingDays,   
    EmploymentDetails.AttendanceAllowanceFollowCalendar, 
    Employee.CB_Basic, 
    Employee.IndianState, 
    Employee.CB_DA, 
    Employee.CB_Leaves, 
    Employee.CB_HRA, 
    Employee.PF_AccountNumber, 
    Employee.CB_UniformCost,   
    Employee.CB_OtherAllowances, 
    Employee.CB_NH, 
    Employee.ESI_Number, 
    Employee.BankAccountNumber, 
    EmployeeSalaryDetails.EMPFL_BK_ACCNO, 
    EmployeeSalaryDetails.EMPFL_EPFNO,   
    EmployeeSalaryDetails.EMPFL_SOSCO_NO, 
    EmploymentDetails.EMPPAY_DATE_JOINED, 
    Employee.EMP_ID, 
    Employee.EMP_NAME, 
    ClientMaster.ClientComplianceStatus, 
    Employee.EMP_FATHER_NAME,   
    Employee.EMP_DATE_OF_BIRTH, 
    Employee.EMP_IC_OLD, 
    Employee.EMP_IC_NEW, 
    Employee.EMP_PASSPORT_NO, 
    PaySlip.Period, 
    BranchMaster.Code, 
    Employee.EMP_CODE, 
    PaySlip.BasicSalary,   
    EmploymentDetails.EMPPAY_BASIC_RATE, 
    Designation.Desig_Name AS Designation, 
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
    PaySlip.LoanRecovery, 
    PaySlip.MiscDeduction, 
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
    PaySlip.Shift2SalaryDaysHours, 
    PaySlip.Shift2SalaryRateType, 
    PaySlip.Shift2SalaryRate,   
    PaySlip.ReAllowance, 
    Employee.EMP_ROLE, 
    PaySlip.ReAllowanceDays, 
    PaySlip.MiscAmount, 
    PaySlip.Bonus, 
    PaySlip.IncomeTaxDeduction, 
    TableClient.Client,
    -- PeriodTotalDays: actual period days based on work site client custom period config
    -- e.g. Anejas (26 to 25): Aug 26 - Sep 25 = 31 days for Sep 2026
    -- Normal client: calendar month days (Sep = 30, Aug = 31, etc.)
    CASE 
        WHEN cap.IsCustomPeriod = 1 THEN
            DATEDIFF(DAY,
                -- Start: PeriodStartDay of PREVIOUS month
                DATEFROMPARTS(
                    CASE WHEN MONTH(PaySlip.Period) = 1 THEN YEAR(PaySlip.Period) - 1 ELSE YEAR(PaySlip.Period) END,
                    CASE WHEN MONTH(PaySlip.Period) = 1 THEN 12 ELSE MONTH(PaySlip.Period) - 1 END,
                    cap.PeriodStartDay
                ),
                -- End: PeriodEndDay of CURRENT month (0 = last day of month)
                CASE 
                    WHEN cap.PeriodEndDay = 0 
                    THEN EOMONTH(DATEFROMPARTS(YEAR(PaySlip.Period), MONTH(PaySlip.Period), 1))
                    ELSE DATEFROMPARTS(YEAR(PaySlip.Period), MONTH(PaySlip.Period), cap.PeriodEndDay)
                END
            ) + 1
        ELSE
            -- Default: calendar month total days
            DAY(EOMONTH(DATEFROMPARTS(YEAR(PaySlip.Period), MONTH(PaySlip.Period), 1)))
    END AS PeriodTotalDays

FROM PaySlip 
    LEFT JOIN Employee 
        ON PaySlip.EmployeeID = Employee.EMP_ID 
    LEFT JOIN ClientMaster 
        ON Employee.EMP_CLIENT = ClientMaster.Code 
    INNER JOIN EmploymentDetails 
        ON Employee.EMP_CODE = EmploymentDetails.EMPPAY_CODE 
    INNER JOIN EmployeeSalaryDetails 
        ON EmploymentDetails.EMPPAY_CODE = EmployeeSalaryDetails.EMPFL_CODE 
    INNER JOIN Designation 
        ON Employee.DesignationId = Designation.DesignationId 
    INNER JOIN BranchMaster 
        ON Employee.EMP_BRANCH_CODE = BranchMaster.Code 
    INNER JOIN Attendance 
        ON Attendance.EmployeeID = Employee.EMP_ID 
    INNER JOIN AttendanceDetails 
        ON AttendanceDetails.AttendanceID = Attendance.ID 
    INNER JOIN SalaryStructure 
        ON SalaryStructure.SalaryId = EmploymentDetails.SALARYLAB 
    -- Original TableClient subquery: gets client name per employee per period
    INNER JOIN (
        SELECT EmployeeID, Period, Client
        FROM (
            SELECT 
                Attendance.EmployeeID, 
                Attendance.Period, 
                COALESCE(NULLIF(AttendanceDetails.Client, ''), AttendanceDetails.OTClient) AS Client, 
                ROW_NUMBER() OVER (
                    PARTITION BY Attendance.EmployeeID, Attendance.Period
                    ORDER BY CASE WHEN AttendanceDetails.Client IS NOT NULL AND AttendanceDetails.Client <> '' THEN 0 ELSE 1 END
                ) AS rn
            FROM Attendance 
            INNER JOIN AttendanceDetails 
                ON AttendanceDetails.AttendanceID = Attendance.ID
        ) t
        WHERE rn = 1
    ) AS TableClient 
        ON TableClient.EmployeeID = Employee.EMP_ID 
        AND TableClient.Period = PaySlip.Period
    -- Get ClientCode from ClientMaster using the client NAME stored in TableClient.Client
    -- Then join ClientAttendancePeriod for custom period config
    LEFT JOIN ClientMaster cm_actual 
        ON cm_actual.Name = TableClient.Client
    LEFT JOIN ClientAttendancePeriod cap 
        ON cap.ClientCode = cm_actual.Code
GO
