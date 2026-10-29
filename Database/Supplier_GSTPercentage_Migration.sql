-- Migration: Add GSTPercentage column to Suppliers table
-- Run this script once on the target database

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'GSTPercentage'
)
BEGIN
    ALTER TABLE Suppliers
    ADD GSTPercentage DECIMAL(5, 2) NULL;

    PRINT 'Column GSTPercentage added to Suppliers table.';
END
ELSE
BEGIN
    PRINT 'Column GSTPercentage already exists in Suppliers table.';
END
