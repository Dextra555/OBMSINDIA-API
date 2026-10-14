-- Migration: Add Period column to Agreement table
-- Run this script once on the database before deploying the updated API.

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Agreement' AND COLUMN_NAME = 'Period'
)
BEGIN
    ALTER TABLE [dbo].[Agreement]
    ADD [Period] NVARCHAR(50) NULL;

    PRINT 'Column Period added to Agreement table.';
END
ELSE
BEGIN
    PRINT 'Column Period already exists in Agreement table. No changes made.';
END
