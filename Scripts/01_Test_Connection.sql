-- Test Database Connection and Basic Operations
-- Purpose: Verify database connectivity and basic operations

PRINT '=== Database Connection Test ===';
PRINT 'Database: DB_NAME()';
PRINT 'Server: @@SERVERNAME';
PRINT 'User: SUSER_SNAME()';

-- Test basic query
PRINT '=== Basic Query Test ===';
SELECT COUNT(*) as TotalRecords FROM ClientMaster;

-- Show current columns in ClientMaster
PRINT '=== Current ClientMaster Columns ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClientMaster'
ORDER BY ORDINAL_POSITION;

-- Test if we can create a simple table
PRINT '=== Test Table Creation ===';
IF OBJECT_ID('TestTable', 'U') IS NOT NULL
    DROP TABLE TestTable;

CREATE TABLE TestTable (
    ID INT PRIMARY KEY,
    TestColumn NVARCHAR(50) NULL
);

INSERT INTO TestTable (ID, TestColumn) VALUES (1, 'Test Value');
SELECT * FROM TestTable;
DROP TABLE TestTable;

PRINT 'Connection test completed successfully!';
