-- Add ESI Wage Report columns to Attendance table
-- Run each column addition separately to avoid conflicts

PRINT 'Starting ESI Wage Report columns migration...'

-- Add TotalDays column
BEGIN TRY
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'TotalDays')
    BEGIN
        PRINT 'Adding TotalDays column to Attendance table...'
        ALTER TABLE Attendance ADD TotalDays INT NULL;
        PRINT 'TotalDays column added successfully.'
    END
    ELSE
    BEGIN
        PRINT 'TotalDays column already exists in Attendance table.'
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding TotalDays column: ' + ERROR_MESSAGE()
END CATCH

-- Add ReasonCode column
BEGIN TRY
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'ReasonCode')
    BEGIN
        PRINT 'Adding ReasonCode column to Attendance table...'
        ALTER TABLE Attendance ADD ReasonCode NVARCHAR(50) NULL;
        PRINT 'ReasonCode column added successfully.'
    END
    ELSE
    BEGIN
        PRINT 'ReasonCode column already exists in Attendance table.'
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding ReasonCode column: ' + ERROR_MESSAGE()
END CATCH

-- Add LastWorkingDay column
BEGIN TRY
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Attendance' AND COLUMN_NAME = 'LastWorkingDay')
    BEGIN
        PRINT 'Adding LastWorkingDay column to Attendance table...'
        ALTER TABLE Attendance ADD LastWorkingDay DATETIME NULL;
        PRINT 'LastWorkingDay column added successfully.'
    END
    ELSE
    BEGIN
        PRINT 'LastWorkingDay column already exists in Attendance table.'
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding LastWorkingDay column: ' + ERROR_MESSAGE()
END CATCH

PRINT 'ESI Wage Report columns migration completed.'
