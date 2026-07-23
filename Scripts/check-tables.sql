-- Check all tables in the database
SELECT TABLE_NAME, TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME

-- Check specifically for Quotation and QuotationDetails tables
PRINT 'Checking for Quotation tables:'
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('Quotation', 'QuotationDetails')
ORDER BY TABLE_NAME
