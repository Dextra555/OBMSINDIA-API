-- Migration: Add ToPeriod column to Agreement table
-- Run this script once on the database to enable From/To Period in Edit Agreements

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Agreement' AND COLUMN_NAME = 'ToPeriod'
)
BEGIN
    ALTER TABLE Agreement ADD ToPeriod NVARCHAR(50) NULL;
    PRINT 'ToPeriod column added successfully to Agreement table.';
END
ELSE
BEGIN
    PRINT 'ToPeriod column already exists in Agreement table.';
END
