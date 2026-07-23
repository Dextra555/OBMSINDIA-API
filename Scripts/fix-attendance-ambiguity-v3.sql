-- Fix SQL Ambiguity Issues - Version 3
-- More robust constraint and column dropping

-- First, let's see what constraints exist
PRINT 'Checking existing constraints on Attendance table...'
SELECT 
    dc.name AS constraint_name,
    c.name AS column_name,
    dc.definition AS constraint_definition
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE dc.parent_object_id = OBJECT_ID('Attendance')
ORDER BY c.name

-- Drop constraints with proper error handling
BEGIN TRY
    DECLARE @sql NVARCHAR(MAX)
    
    -- Drop AttendanceDate constraint
    SELECT @sql = 'ALTER TABLE Attendance DROP CONSTRAINT ' + dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'AttendanceDate'
    
    IF @sql IS NOT NULL
    BEGIN
        PRINT 'Executing: ' + @sql
        EXEC sp_executesql @sql
    END
    
    SET @sql = NULL
    
    -- Drop TimeStart constraint
    SELECT @sql = 'ALTER TABLE Attendance DROP CONSTRAINT ' + dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'TimeStart'
    
    IF @sql IS NOT NULL
    BEGIN
        PRINT 'Executing: ' + @sql
        EXEC sp_executesql @sql
    END
    
    SET @sql = NULL
    
    -- Drop TimeEnd constraint
    SELECT @sql = 'ALTER TABLE Attendance DROP CONSTRAINT ' + dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'TimeEnd'
    
    IF @sql IS NOT NULL
    BEGIN
        PRINT 'Executing: ' + @sql
        EXEC sp_executesql @sql
    END
    
    SET @sql = NULL
    
    -- Drop Status constraint
    SELECT @sql = 'ALTER TABLE Attendance DROP CONSTRAINT ' + dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'Status'
    
    IF @sql IS NOT NULL
    BEGIN
        PRINT 'Executing: ' + @sql
        EXEC sp_executesql @sql
    END
    
END TRY
BEGIN CATCH
    PRINT 'Error dropping constraints: ' + ERROR_MESSAGE()
END CATCH

-- Now drop the columns
BEGIN TRY
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'AttendanceDate')
    BEGIN
        PRINT 'Dropping AttendanceDate column...'
        ALTER TABLE Attendance DROP COLUMN AttendanceDate
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'TimeStart')
    BEGIN
        PRINT 'Dropping TimeStart column...'
        ALTER TABLE Attendance DROP COLUMN TimeStart
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'TimeEnd')
    BEGIN
        PRINT 'Dropping TimeEnd column...'
        ALTER TABLE Attendance DROP COLUMN TimeEnd
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'Status')
    BEGIN
        PRINT 'Dropping Status column...'
        ALTER TABLE Attendance DROP COLUMN Status
    END
    
    PRINT 'SUCCESS: All conflicting columns removed from Attendance table!'
END TRY
BEGIN CATCH
    PRINT 'Error dropping columns: ' + ERROR_MESSAGE()
END CATCH

-- Verify the columns are gone
PRINT ''
PRINT 'Verification - Current columns in Attendance table:'
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Attendance'
ORDER BY ORDINAL_POSITION
