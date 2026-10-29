-- ============================================================
-- Migration: Add GSTApplicable & GSTPercentage columns to ClientMaster
-- Run this script once on the database before restarting API
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'GSTApplicable'
)
BEGIN
    ALTER TABLE ClientMaster
    ADD GSTApplicable BIT NOT NULL DEFAULT 0;

    PRINT 'GSTApplicable column added to ClientMaster table.';
END
ELSE
BEGIN
    PRINT 'GSTApplicable column already exists.';
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'GSTPercentage'
)
BEGIN
    ALTER TABLE ClientMaster
    ADD GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;

    PRINT 'GSTPercentage column added to ClientMaster table.';
END
ELSE
BEGIN
    PRINT 'GSTPercentage column already exists.';
END
