-- Fix SQL Ambiguity Issues - Version 2
-- Remove default constraints first, then drop duplicate columns from Attendance table

-- Drop default constraint for AttendanceDate if it exists
DECLARE @constraint_name NVARCHAR(128)
SELECT @constraint_name = d.name 
FROM sys.default_constraints d
JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
WHERE d.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'AttendanceDate'

IF @constraint_name IS NOT NULL
BEGIN
    PRINT 'Dropping default constraint for AttendanceDate: ' + @constraint_name
    EXEC('ALTER TABLE Attendance DROP CONSTRAINT ' + @constraint_name)
END

-- Drop default constraint for TimeStart if it exists
SELECT @constraint_name = d.name 
FROM sys.default_constraints d
JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
WHERE d.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'TimeStart'

IF @constraint_name IS NOT NULL
BEGIN
    PRINT 'Dropping default constraint for TimeStart: ' + @constraint_name
    EXEC('ALTER TABLE Attendance DROP CONSTRAINT ' + @constraint_name)
END

-- Drop default constraint for TimeEnd if it exists
SELECT @constraint_name = d.name 
FROM sys.default_constraints d
JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
WHERE d.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'TimeEnd'

IF @constraint_name IS NOT NULL
BEGIN
    PRINT 'Dropping default constraint for TimeEnd: ' + @constraint_name
    EXEC('ALTER TABLE Attendance DROP CONSTRAINT ' + @constraint_name)
END

-- Drop default constraint for Status if it exists
SELECT @constraint_name = d.name 
FROM sys.default_constraints d
JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
WHERE d.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'Status'

IF @constraint_name IS NOT NULL
BEGIN
    PRINT 'Dropping default constraint for Status: ' + @constraint_name
    EXEC('ALTER TABLE Attendance DROP CONSTRAINT ' + @constraint_name)
END

-- Now drop the columns
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
