-- ============================================================
-- FIX: vwPaySheet — Crystal Report PF columns correction
-- 
-- Problem:
--   Crystal Report PaySlip.rpt and PaySheet.rpt used
--   {vwPaySheet.EPFDeductionAmount} and {vwPaySheet.EPFEmployerContribution}
--   which contained 0 (old Malaysia EPF style, unused for Indian payroll).
--   Crystal Report formulas computed:
--     Employee PF  = MiscAmount (DA) × 12%  → 15,000 × 12% = 1,800  ❌
--     Employer PF  = MiscAmount (DA) × 13.2% → 15,000 × 13.2% = 1,980 ❌
--
-- Fix:
--   EPFDeductionAmount  ← PaySlip.PF  (already correctly saved by SalaryProcess)
--   EPFEmployerContribution ← PaySlip.PF × (EmployerRate / EmployeeRate)
--                              from active PFConfiguration row
--     e.g. 3,000 × (13% / 12%) = 3,250  ✅
--
-- After this fix:
--   Employee PF = 3,000  ✅  (Basic+DA = 25,000 × 12%)
--   Employer PF = 3,250  ✅  (Basic+DA = 25,000 × 13%)
-- ============================================================

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

    -- FIX: Employee PF — was PaySlip.EPFDeductionAmount (always 0 for Indian payroll)
    -- Now returns PaySlip.PF which is correctly calculated by SalaryProcess
    -- using PFConfiguration (BasicSalaryLimit=25000, EmployeeRate=12%)
    PaySlip.PF                          AS EPFDeductionAmount,

    PaySlip.SOCSODeductionAmount, 

    -- FIX: Employer PF — was PaySlip.EPFEmployerContribution (always 0 for Indian payroll)
    -- Now computed as: Employee PF × (EmployerRate / EmployeeRate) from active PFConfiguration
    -- e.g. 3,000 × (13% / 12%) = 3,250
    CAST(
        PaySlip.PF * (
            SELECT TOP 1
                CAST(pfc.Employer_Rate AS DECIMAL(10,4))
                / NULLIF(CAST(pfc.Employee_Rate AS DECIMAL(10,4)), 0)
            FROM PFConfiguration pfc
            WHERE pfc.Is_Active = 1
              AND pfc.Effective_Date <= PaySlip.Period
            ORDER BY pfc.Effective_Date DESC
        )
    AS DECIMAL(10,2))                   AS EPFEmployerContribution,

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
    EmployeeSalaryDetails.INCOMETAXDETECT,
    PaySlip.PTax,
    PaySlip.EarnedSalary,
    PaySlip.PerDaySalary,
    PaySlip.PF,
    PaySlip.PFWage,
    PaySlip.ESI,
    PaySlip.ESIWage,
    PaySlip.GrossPay,
    PaySlip.LWF,
    PaySlip.TotalDeduction,
    PaySlip.NetPay,
    PaySlip.TDS,
    Employee.TDSApplicable,
    -- PeriodTotalDays: actual period days based on work site client custom period config
    CASE 
        WHEN cap.IsCustomPeriod = 1 THEN
            DATEDIFF(DAY,
                DATEFROMPARTS(
                    CASE WHEN MONTH(PaySlip.Period) = 1 THEN YEAR(PaySlip.Period) - 1 ELSE YEAR(PaySlip.Period) END,
                    CASE WHEN MONTH(PaySlip.Period) = 1 THEN 12 ELSE MONTH(PaySlip.Period) - 1 END,
                    cap.PeriodStartDay
                ),
                CASE 
                    WHEN cap.PeriodEndDay = 0 
                    THEN EOMONTH(DATEFROMPARTS(YEAR(PaySlip.Period), MONTH(PaySlip.Period), 1))
                    ELSE DATEFROMPARTS(YEAR(PaySlip.Period), MONTH(PaySlip.Period), cap.PeriodEndDay)
                END
            ) + 1
        ELSE
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
    INNER JOIN Attendance
        ON Attendance.EmployeeID = Employee.EMP_ID
        AND MONTH(Attendance.Period) = MONTH(PaySlip.Period)
        AND YEAR(Attendance.Period)  = YEAR(PaySlip.Period)
    INNER JOIN BranchMaster 
        ON BranchMaster.Code = Attendance.Branch 
    INNER JOIN SalaryStructure 
        ON SalaryStructure.SalaryId = EmploymentDetails.SALARYLAB 
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
    LEFT JOIN ClientMaster cm_actual 
        ON cm_actual.Name = TableClient.Client
    LEFT JOIN ClientAttendancePeriod cap 
        ON cap.ClientCode = cm_actual.Code;
