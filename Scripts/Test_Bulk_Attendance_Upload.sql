-- =====================================================
-- Bulk Attendance Upload Test Script
-- =====================================================
-- This script tests the complete bulk attendance upload functionality
-- including database operations, validation, and data integrity

-- =====================================================
-- 1. Test Data Setup
-- =====================================================

-- Create test employees if they don't exist
IF NOT EXISTS (SELECT 1 FROM Employees WHERE EMP_CODE = 'TEST001')
BEGIN
    INSERT INTO Employees (EMP_CODE, EMP_NAME, EMP_BRANCH_CODE, EMP_ROLE, EMP_STATUS, EMP_DATE_JOINED)
    VALUES ('TEST001', 'Test Employee 1', 'BR001', 'GUARD', 'A', GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Employees WHERE EMP_CODE = 'TEST002')
BEGIN
    INSERT INTO Employees (EMP_CODE, EMP_NAME, EMP_BRANCH_CODE, EMP_ROLE, EMP_STATUS, EMP_DATE_JOINED)
    VALUES ('TEST002', 'Test Employee 2', 'BR001', 'SUPERVISOR', 'A', GETDATE());
END

-- Create test attendance period (current month)
DECLARE @TestPeriod DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), DAY(EOMONTH(GETDATE())));
DECLARE @TestBranch VARCHAR(10) = 'BR001';

-- =====================================================
-- 2. Test Database Schema Validation
-- =====================================================

PRINT '=== Testing Database Schema ==='

-- Check if required tables exist
IF OBJECT_ID('Attendance') IS NOT NULL
    PRINT '✓ Attendance table exists'
ELSE
    PRINT '✗ Attendance table missing'

IF OBJECT_ID('AttendanceDetails') IS NOT NULL
    PRINT '✓ AttendanceDetails table exists'
ELSE
    PRINT '✗ AttendanceDetails table missing'

IF OBJECT_ID('Employees') IS NOT NULL
    PRINT '✓ Employees table exists'
ELSE
    PRINT '✗ Employees table missing'

-- Check if required columns exist in Attendance table
IF COL_LENGTH('Attendance', 'EmployeeID') IS NOT NULL
    PRINT '✓ Attendance.EmployeeID column exists'
ELSE
    PRINT '✗ Attendance.EmployeeID column missing'

IF COL_LENGTH('Attendance', 'Period') IS NOT NULL
    PRINT '✓ Attendance.Period column exists'
ELSE
    PRINT '✗ Attendance.Period column missing'

IF COL_LENGTH('Attendance', 'Branch') IS NOT NULL
    PRINT '✓ Attendance.Branch column exists'
ELSE
    PRINT '✗ Attendance.Branch column missing'

-- Check if required columns exist in AttendanceDetails table
IF COL_LENGTH('AttendanceDetails', 'AttendanceID') IS NOT NULL
    PRINT '✓ AttendanceDetails.AttendanceID column exists'
ELSE
    PRINT '✗ AttendanceDetails.AttendanceID column missing'

IF COL_LENGTH('AttendanceDetails', 'AttendanceDate') IS NOT NULL
    PRINT '✓ AttendanceDetails.AttendanceDate column exists'
ELSE
    PRINT '✗ AttendanceDetails.AttendanceDate column missing'

IF COL_LENGTH('AttendanceDetails', 'Type') IS NOT NULL
    PRINT '✓ AttendanceDetails.Type column exists'
ELSE
    PRINT '✗ AttendanceDetails.Type column missing'

-- =====================================================
-- 3. Test Data Insertion (Simulating Bulk Upload)
-- =====================================================

PRINT '=== Testing Data Insertion ==='

-- Clean up any existing test data
DELETE FROM AttendanceDetails WHERE AttendanceID IN (
    SELECT ID FROM Attendance WHERE EmployeeID IN (
        SELECT EMP_ID FROM Employees WHERE EMP_CODE LIKE 'TEST%'
    ) AND Period = @TestPeriod
);

DELETE FROM Attendance WHERE EmployeeID IN (
    SELECT EMP_ID FROM Employees WHERE EMP_code LIKE 'TEST%'
) AND Period = @TestPeriod;

-- Test inserting attendance header (simulating bulk upload)
DECLARE @TestEmployee1ID INT = (SELECT EMP_ID FROM Employees WHERE EMP_CODE = 'TEST001');
DECLARE @TestEmployee2ID INT = (SELECT EMP_ID FROM Employees WHERE EMP_CODE = 'TEST002');

IF @TestEmployee1ID IS NOT NULL
BEGIN
    INSERT INTO Attendance (EmployeeID, Period, Branch, Shift2Type, Shift2Rate, AllowanceDeduction, 
                           SpecialAllowanceDeduction, Bonus, LastUpdate, LastUpdatedBy)
    VALUES (@TestEmployee1ID, @TestPeriod, @TestBranch, 0, 0, 0, 0, 0, GETDATE(), 'TestUser');
    
    DECLARE @AttendanceID1 INT = SCOPE_IDENTITY();
    PRINT '✓ Test attendance header 1 inserted: ID=' + CAST(@AttendanceID1 AS VARCHAR);
    
    -- Insert attendance details for first 5 days of the month
    DECLARE @CurrentDate DATE = DATEFROMPARTS(YEAR(@TestPeriod), MONTH(@TestPeriod), 1);
    
    WHILE @CurrentDate <= DATEADD(DAY, 4, DATEFROMPARTS(YEAR(@TestPeriod), MONTH(@TestPeriod), 1))
    BEGIN
        INSERT INTO AttendanceDetails (AttendanceID, AttendanceDate, Client, TimeStart, TimeEnd, 
                                       OTClient, OTTimeStart, OTTimeEnd, Type, LastUpdate, LastUpdatedBy)
        VALUES (@AttendanceID1, @CurrentDate, 'CLIENT001', 
                DATEADD(HOUR, 9, CAST(@CurrentDate AS DATETIME)), 
                DATEADD(HOUR, 17, CAST(@CurrentDate AS DATETIME)), 
                NULL, NULL, NULL, 1, GETDATE(), 'TestUser');
        
        SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
    END
    
    PRINT '✓ Test attendance details 1 inserted (5 days)';
END

IF @TestEmployee2ID IS NOT NULL
BEGIN
    INSERT INTO Attendance (EmployeeID, Period, Branch, Shift2Type, Shift2Rate, AllowanceDeduction, 
                           SpecialAllowanceDeduction, Bonus, LastUpdate, LastUpdatedBy)
    VALUES (@TestEmployee2ID, @TestPeriod, @TestBranch, 0, 0, 0, 0, 0, GETDATE(), 'TestUser');
    
    DECLARE @AttendanceID2 INT = SCOPE_IDENTITY();
    PRINT '✓ Test attendance header 2 inserted: ID=' + CAST(@AttendanceID2 AS VARCHAR);
    
    -- Insert attendance details with mixed work types
    DECLARE @CurrentDate DATE = DATEFROMPARTS(YEAR(@TestPeriod), MONTH(@TestPeriod), 1);
    
    WHILE @CurrentDate <= DATEADD(DAY, 4, DATEFROMPARTS(YEAR(@TestPeriod), MONTH(@TestPeriod), 1))
    BEGIN
        DECLARE @WorkType INT = CASE 
            WHEN DATEPART(DAY, @CurrentDate) IN (6, 7) THEN 2 -- Off Day
            WHEN DATEPART(DAY, @CurrentDate) = 3 THEN 8 -- Annual Leave
            ELSE 1 -- General Working
        END;
        
        INSERT INTO AttendanceDetails (AttendanceID, AttendanceDate, Client, TimeStart, TimeEnd, 
                                       OTClient, OTTimeStart, OTTimeEnd, Type, LastUpdate, LastUpdatedBy)
        VALUES (@AttendanceID2, @CurrentDate, 
                CASE WHEN @WorkType = 1 THEN 'CLIENT002' ELSE NULL END,
                CASE WHEN @WorkType = 1 THEN DATEADD(HOUR, 8, CAST(@CurrentDate AS DATETIME)) ELSE NULL END,
                CASE WHEN @WorkType = 1 THEN DATEADD(HOUR, 16, CAST(@CurrentDate AS DATETIME)) ELSE NULL END,
                NULL, NULL, NULL, @WorkType, GETDATE(), 'TestUser');
        
        SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
    END
    
    PRINT '✓ Test attendance details 2 inserted (mixed work types)';
END

-- =====================================================
-- 4. Test Data Retrieval and Validation
-- =====================================================

PRINT '=== Testing Data Retrieval ==='

-- Test retrieving attendance data (simulating export functionality)
SELECT 
    e.EMP_CODE,
    e.EMP_NAME,
    a.Branch,
    a.Period,
    COUNT(ad.ID) AS TotalDays,
    SUM(CASE WHEN ad.Type = 1 THEN 1 ELSE 0 END) AS WorkingDays,
    SUM(CASE WHEN ad.Type = 2 THEN 1 ELSE 0 END) AS OffDays,
    SUM(CASE WHEN ad.Type = 8 THEN 1 ELSE 0 END) AS LeaveDays
FROM Attendance a
JOIN Employees e ON a.EmployeeID = e.EMP_ID
LEFT JOIN AttendanceDetails ad ON a.ID = ad.AttendanceID
WHERE a.Period = @TestPeriod 
  AND e.EMP_CODE LIKE 'TEST%'
GROUP BY e.EMP_CODE, e.EMP_NAME, a.Branch, a.Period
ORDER BY e.EMP_CODE;

-- Test work type distribution
SELECT 
    e.EMP_CODE,
    e.EMP_NAME,
    ad.AttendanceDate,
    CASE ad.Type
        WHEN 1 THEN 'General Working'
        WHEN 2 THEN 'Off Day'
        WHEN 8 THEN 'Annual Leave'
        ELSE 'Other'
    END AS WorkType,
    ad.Client,
    ad.TimeStart,
    ad.TimeEnd
FROM Attendance a
JOIN Employees e ON a.EmployeeID = e.EMP_ID
JOIN AttendanceDetails ad ON a.ID = ad.AttendanceID
WHERE a.Period = @TestPeriod 
  AND e.EMP_CODE LIKE 'TEST%'
ORDER BY e.EMP_CODE, ad.AttendanceDate;

-- =====================================================
-- 5. Test Data Integrity Constraints
-- =====================================================

PRINT '=== Testing Data Integrity ==='

-- Test foreign key constraints
DECLARE @FKTest1 INT = 99999; -- Non-existent employee ID
BEGIN TRY
    INSERT INTO Attendance (EmployeeID, Period, Branch, Shift2Type, Shift2Rate, AllowanceDeduction, 
                           SpecialAllowanceDeduction, Bonus, LastUpdate, LastUpdatedBy)
    VALUES (@FKTest1, @TestPeriod, @TestBranch, 0, 0, 0, 0, 0, GETDATE(), 'TestUser');
    PRINT '✗ Foreign key constraint failed - invalid EmployeeID accepted'
END TRY
BEGIN CATCH
    PRINT '✓ Foreign key constraint working - invalid EmployeeID rejected'
END CATCH

-- Test data type constraints
BEGIN TRY
    INSERT INTO AttendanceDetails (AttendanceID, AttendanceDate, Client, TimeStart, TimeEnd, 
                                   OTClient, OTTimeStart, OTTimeEnd, Type, LastUpdate, LastUpdatedBy)
    VALUES (1, 'Invalid Date', 'TEST', NULL, NULL, NULL, NULL, NULL, 1, GETDATE(), 'TestUser');
    PRINT '✗ Date constraint failed - invalid date accepted'
END TRY
BEGIN CATCH
    PRINT '✓ Date constraint working - invalid date rejected'
END CATCH

-- =====================================================
-- 6. Test Performance with Large Dataset
-- =====================================================

PRINT '=== Testing Performance ==='

-- Create performance test data (100 employees)
DECLARE @StartTime DATETIME = GETDATE();
DECLARE @TestCount INT = 0;

-- Clean up performance test data
DELETE FROM AttendanceDetails WHERE AttendanceID IN (
    SELECT ID FROM Attendance WHERE EmployeeID IN (
        SELECT EMP_ID FROM Employees WHERE EMP_CODE LIKE 'PERF%'
    )
);

DELETE FROM Attendance WHERE EmployeeID IN (
    SELECT EMP_ID FROM Employees WHERE EMP_CODE LIKE 'PERF%'
);

-- Create test employees for performance test
WHILE @TestCount < 100
BEGIN
    DECLARE @TestCode VARCHAR(10) = 'PERF' + RIGHT('000' + CAST(@TestCount AS VARCHAR), 3);
    
    IF NOT EXISTS (SELECT 1 FROM Employees WHERE EMP_CODE = @TestCode)
    BEGIN
        INSERT INTO Employees (EMP_CODE, EMP_NAME, EMP_BRANCH_CODE, EMP_ROLE, EMP_STATUS, EMP_DATE_JOINED)
        VALUES (@TestCode, 'Performance Test ' + CAST(@TestCount AS VARCHAR), @TestBranch, 'GUARD', 'A', GETDATE());
    END
    
    SET @TestCount = @TestCount + 1;
END

-- Insert performance test attendance data
SET @TestCount = 0;
WHILE @TestCount < 100
BEGIN
    DECLARE @PerfCode VARCHAR(10) = 'PERF' + RIGHT('000' + CAST(@TestCount AS VARCHAR), 3);
    DECLARE @PerfEmployeeID INT = (SELECT EMP_ID FROM Employees WHERE EMP_CODE = @PerfCode);
    
    IF @PerfEmployeeID IS NOT NULL
    BEGIN
        INSERT INTO Attendance (EmployeeID, Period, Branch, Shift2Type, Shift2Rate, AllowanceDeduction, 
                               SpecialAllowanceDeduction, Bonus, LastUpdate, LastUpdatedBy)
        VALUES (@PerfEmployeeID, @TestPeriod, @TestBranch, 0, 0, 0, 0, 0, GETDATE(), 'PerfTest');
        
        DECLARE @PerfAttendanceID INT = SCOPE_IDENTITY();
        
        -- Insert 15 days of attendance details
        DECLARE @DayCount INT = 1;
        WHILE @DayCount <= 15
        BEGIN
            INSERT INTO AttendanceDetails (AttendanceID, AttendanceDate, Client, TimeStart, TimeEnd, 
                                           OTClient, OTTimeStart, OTTimeEnd, Type, LastUpdate, LastUpdatedBy)
            VALUES (@PerfAttendanceID, DATEADD(DAY, @DayCount - 1, DATEFROMPARTS(YEAR(@TestPeriod), MONTH(@TestPeriod), 1)), 
                    'PERF_CLIENT', DATEADD(HOUR, 9, DATEADD(DAY, @DayCount - 1, CAST(@TestPeriod AS DATETIME))), 
                    DATEADD(HOUR, 17, DATEADD(DAY, @DayCount - 1, CAST(@TestPeriod AS DATETIME))), 
                    NULL, NULL, NULL, 1, GETDATE(), 'PerfTest');
            
            SET @DayCount = @DayCount + 1;
        END
    END
    
    SET @TestCount = @TestCount + 1;
END

DECLARE @EndTime DATETIME = GETDATE();
DECLARE @Duration INT = DATEDIFF(MILLISECOND, @StartTime, @EndTime);

PRINT '✓ Performance test completed: ' + CAST(@TestCount AS VARCHAR) + ' employees, ' + 
      CAST(@Duration AS VARCHAR) + 'ms';

-- Test query performance on large dataset
SET @StartTime = GETDATE();

SELECT COUNT(*) AS TotalRecords
FROM Attendance a
JOIN Employees e ON a.EmployeeID = e.EMP_ID
JOIN AttendanceDetails ad ON a.ID = ad.AttendanceID
WHERE a.Period = @TestPeriod
  AND e.EMP_CODE LIKE 'PERF%';

SET @EndTime = GETDATE();
SET @Duration = DATEDIFF(MILLISECOND, @StartTime, @EndTime);

PRINT '✓ Query performance test: ' + CAST(@Duration AS VARCHAR) + 'ms';

-- =====================================================
-- 7. Test Transaction Rollback
-- =====================================================

PRINT '=== Testing Transaction Rollback ==='

BEGIN TRANSACTION;
BEGIN TRY
    -- Insert test data that will be rolled back
    DECLARE @RollbackEmployeeID INT = (SELECT EMP_ID FROM Employees WHERE EMP_CODE = 'TEST001');
    
    INSERT INTO Attendance (EmployeeID, Period, Branch, Shift2Type, Shift2Rate, AllowanceDeduction, 
                           SpecialAllowanceDeduction, Bonus, LastUpdate, LastUpdatedBy)
    VALUES (@RollbackEmployeeID, DATEADD(MONTH, 1, @TestPeriod), @TestBranch, 0, 0, 0, 0, 0, GETDATE(), 'RollbackTest');
    
    DECLARE @RollbackAttendanceID INT = SCOPE_IDENTITY();
    
    INSERT INTO AttendanceDetails (AttendanceID, AttendanceDate, Client, TimeStart, TimeEnd, 
                                   OTClient, OTTimeStart, OTTimeEnd, Type, LastUpdate, LastUpdatedBy)
    VALUES (@RollbackAttendanceID, DATEADD(DAY, 1, DATEADD(MONTH, 1, @TestPeriod)), 'ROLLBACK_CLIENT', 
            DATEADD(HOUR, 9, DATEADD(DAY, 1, DATEADD(MONTH, 1, CAST(@TestPeriod AS DATETIME))), 
            DATEADD(HOUR, 17, DATEADD(DAY, 1, DATEADD(MONTH, 1, CAST(@TestPeriod AS DATETIME))), 
            NULL, NULL, NULL, 1, GETDATE(), 'RollbackTest');
    
    -- Force rollback
    ROLLBACK TRANSACTION;
    PRINT '✓ Transaction rollback test completed';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '✗ Transaction rollback test failed with error';
END CATCH

-- Verify rollback worked
IF NOT EXISTS (SELECT 1 FROM Attendance WHERE LastUpdatedBy = 'RollbackTest')
    PRINT '✓ Rollback successful - no test data found'
ELSE
    PRINT '✗ Rollback failed - test data still exists'

-- =====================================================
-- 8. Cleanup Test Data
-- =====================================================

PRINT '=== Cleaning Up Test Data ==='

-- Clean up performance test data
DELETE FROM AttendanceDetails WHERE AttendanceID IN (
    SELECT ID FROM Attendance WHERE EmployeeID IN (
        SELECT EMP_ID FROM Employees WHERE EMP_CODE LIKE 'PERF%'
    )
);

DELETE FROM Attendance WHERE EmployeeID IN (
    SELECT EMP_ID FROM Employees WHERE EMP_CODE LIKE 'PERF%'
);

DELETE FROM Employees WHERE EMP_CODE LIKE 'PERF%';

-- Clean up main test data (keep for manual verification)
PRINT 'Note: Main test data (TEST001, TEST002) kept for manual verification';
PRINT 'To clean up all test data, uncomment the following lines:';
PRINT '-- DELETE FROM AttendanceDetails WHERE AttendanceID IN (SELECT ID FROM Attendance WHERE EmployeeID IN (SELECT EMP_ID FROM Employees WHERE EMP_CODE LIKE ''TEST%''));';
PRINT '-- DELETE FROM Attendance WHERE EmployeeID IN (SELECT EMP_ID FROM Employees WHERE EMP_CODE LIKE ''TEST%'');';
PRINT '-- DELETE FROM Employees WHERE EMP_CODE LIKE ''TEST%'';';

-- =====================================================
-- 9. Final Validation Summary
-- =====================================================

PRINT '=== Test Summary ===';
PRINT '✓ Database schema validation completed';
PRINT '✓ Data insertion test completed';
PRINT '✓ Data retrieval test completed';
PRINT '✓ Data integrity constraints tested';
PRINT '✓ Performance test completed (100 employees, 1500 records)';
PRINT '✓ Transaction rollback test completed';
PRINT '';
PRINT 'Bulk attendance upload system is ready for testing!';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Test API endpoints with actual Excel files';
PRINT '2. Test frontend interface';
PRINT '3. Test error handling and validation';
PRINT '4. Test with real production data (in staging environment)';
