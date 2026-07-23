-- Step 1: Add ClientComplianceStatus Column
-- Purpose: Add the new compliance status column

PRINT '=== Step 1: Adding ClientComplianceStatus Column ===';

-- Add the column
ALTER TABLE ClientMaster 
ADD ClientComplianceStatus NVARCHAR(50) NULL;

PRINT 'ClientComplianceStatus column added successfully';

-- Verify the column was added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'ClientComplianceStatus';

PRINT 'Step 1 completed successfully!';
