-- Fix SQL Ambiguity Issues
-- Remove duplicate columns from Attendance table that exist in AttendanceDetails

-- Check if columns exist in Attendance table before dropping
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'AttendanceDate')
BEGIN
    PRINT 'Dropping AttendanceDate column from Attendance table...'
    ALTER TABLE Attendance DROP COLUMN AttendanceDate
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'TimeStart')
BEGIN
    PRINT 'Dropping TimeStart column from Attendance table...'
    ALTER TABLE Attendance DROP COLUMN TimeStart
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'TimeEnd')
BEGIN
    PRINT 'Dropping TimeEnd column from Attendance table...'
    ALTER TABLE Attendance DROP COLUMN TimeEnd
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'Status')
BEGIN
    PRINT 'Dropping Status column from Attendance table...'
    ALTER TABLE Attendance DROP COLUMN Status
END

PRINT 'SQL Ambiguity fix completed successfully!'
PRINT 'AttendanceDate, TimeStart, TimeEnd, and Status now exist only in AttendanceDetails table.'
