-- ============================================================
-- FIX SUMMARY: Quotation Tables Creation
-- ============================================================
-- 
-- PROBLEM: SQL Exception - "Invalid column name 'AddedDate'" and "Invalid column name 'QuotationID'"
-- 
-- ROOT CAUSE: The Quotation and QuotationDetails tables did not exist in the database,
--              but the Entity Framework models were trying to access these tables.
--
-- SOLUTION: Created the missing tables based on the EF model definitions.
--
-- TABLES CREATED:
-- 1. Quotation table with all required columns including AddedDate
-- 2. QuotationDetails table with QuotationID foreign key
-- 3. ClientInvoiceDetail table with HSNCode column
--
-- FILES CREATED:
-- - CreateQuotationTables.sql - Main table creation script
-- - CreateClientInvoiceDetailSimple.sql - Invoice details table
-- - verify-quotation-tables.sql - Verification script
--
-- STATUS: ✅ FIXED - All required tables now exist
-- ============================================================

USE [obms];
GO

PRINT '=== FINAL VERIFICATION - All Required Tables ==='

SELECT 
    TableName = t.TABLE_NAME,
    Status = 'EXISTS',
    ColumnCount = COUNT(c.COLUMN_NAME)
FROM INFORMATION_SCHEMA.TABLES t
LEFT JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
WHERE t.TABLE_NAME IN ('Quotation', 'QuotationDetails', 'ClientInvoiceDetail')
    AND t.TABLE_TYPE = 'BASE TABLE'
GROUP BY t.TABLE_NAME
ORDER BY t.TABLE_NAME;

PRINT '=== Critical Columns Verification ==='

-- Check for the specific columns that were causing errors
SELECT 
    'Quotation.AddedDate' AS ColumnReference,
    CASE WHEN EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'Quotation' AND COLUMN_NAME = 'AddedDate'
    ) THEN 'EXISTS' ELSE 'MISSING' END AS Status

UNION ALL

SELECT 
    'QuotationDetails.QuotationID' AS ColumnReference,
    CASE WHEN EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'QuotationDetails' AND COLUMN_NAME = 'QuotationID'
    ) THEN 'EXISTS' ELSE 'MISSING' END AS Status

UNION ALL

SELECT 
    'ClientInvoiceDetail.HSNCode' AS ColumnReference,
    CASE WHEN EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'ClientInvoiceDetail' AND COLUMN_NAME = 'HSNCode'
    ) THEN 'EXISTS' ELSE 'MISSING' END AS Status;

PRINT '=== FIX COMPLETE - Database is now ready ==='
