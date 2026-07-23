-- ============================================================
-- OBMS Attendance Seed Data (April 2024 - Dec 2024)
-- ============================================================
USE [obms];
GO

PRINT '=== Seeding Attendance Data (FY 2024-25) ==='

-- Generate monthly header for each employee
DECLARE @startDate DATE = '2024-04-01';
DECLARE @endDate DATE = '2024-12-01';
DECLARE @currentDate DATE = @startDate;

WHILE @currentDate <= @endDate
BEGIN
    INSERT INTO Attendance (Period, Branch, EmployeeID, Shift2Type, Shift2Rate, AllowanceDeduction, SpecialAllowanceDeduction, Bonus, LastUpdate, LastUpdatedBy, AttendanceDate)
    SELECT 
        @currentDate, 
        EMP_BRANCH_CODE, 
        EMP_ID, 
        0, 0, 0, 0, 0, GETDATE(), 'system', @currentDate
    FROM Employee;

    SET @currentDate = DATEADD(MONTH, 1, @currentDate);
END

-- Generate daily details (Assuming 26 working days)
DECLARE @currentMonthStart DATE = @startDate;
DECLARE @currentMonthEnd DATE; 
DECLARE @iterDate DATE;

WHILE @currentMonthStart <= @endDate
BEGIN
    SET @currentMonthEnd = EOMONTH(@currentMonthStart);
    SET @iterDate = @currentMonthStart;

    WHILE @iterDate <= @currentMonthEnd
    BEGIN
        -- Skip Sundays (Type = 2) for standard working
        DECLARE @dayType INT;
        IF DATEPART(dw, @iterDate) = 1 -- Sunday
            SET @dayType = 2; -- Rest Day
        ELSE
            SET @dayType = 1; -- Normal Working Day

        INSERT INTO AttendanceDetails (AttendanceID, AttendanceDate, Client, TimeStart, TimeEnd, OTClient, Type, LastUpdate, LastUpdatedBy)
        SELECT 
            A.ID, 
            @iterDate, 
            E.EMP_CLIENT, 
            CASE WHEN @dayType = 1 THEN '09:00:00' ELSE NULL END, 
            CASE WHEN @dayType = 1 THEN '18:00:00' ELSE NULL END, 
            NULL, 
            @dayType, 
            GETDATE(), 'system'
        FROM Attendance A
        JOIN Employee E ON A.EmployeeID = E.EMP_ID
        WHERE A.Period = @currentMonthStart;

        SET @iterDate = DATEADD(DAY, 1, @iterDate);
    END

    SET @currentMonthStart = DATEADD(MONTH, 1, @currentMonthStart);
END

-- Add some OT and Leaves for realism
UPDATE AttendanceDetails
SET Type = 8 -- Annual Leave
WHERE AttendanceDate IN ('2024-05-15', '2024-08-14', '2024-10-22') AND AttendanceID % 5 = 1 AND Type = 1

UPDATE AttendanceDetails
SET Type = 10 -- Medical Leave  
WHERE AttendanceDate = '2024-06-12' AND AttendanceID % 4 = 1 AND Type = 1

UPDATE AttendanceDetails
SET OTTimeStart = '18:00:00', OTTimeEnd = '20:00:00'
WHERE AttendanceDate IN ('2024-07-10', '2024-09-18') AND AttendanceID % 3 = 1 AND Type = 1

PRINT '=== Attendance Seeding Completed ==='
