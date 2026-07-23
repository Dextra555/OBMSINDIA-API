-- Fix ServiceType table column names to match domain model
-- Domain model uses: LastUpdatedDate, LastUpdatedBy
-- Database currently has: UpdatedDate, UpdatedBy

USE obmsdev_backup;
GO

-- Check if columns exist with old names
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ServiceType' 
    AND COLUMN_NAME = 'UpdatedDate'
)
BEGIN
    -- Rename UpdatedDate to LastUpdatedDate
    EXEC sp_rename 'ServiceType.UpdatedDate', 'LastUpdatedDate', 'COLUMN';
    PRINT 'Renamed UpdatedDate to LastUpdatedDate';
END
ELSE
BEGIN
    PRINT 'Column UpdatedDate does not exist or already renamed';
END
GO

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ServiceType' 
    AND COLUMN_NAME = 'UpdatedBy'
)
BEGIN
    -- Rename UpdatedBy to LastUpdatedBy
    EXEC sp_rename 'ServiceType.UpdatedBy', 'LastUpdatedBy', 'COLUMN';
    PRINT 'Renamed UpdatedBy to LastUpdatedBy';
END
ELSE
BEGIN
    PRINT 'Column UpdatedBy does not exist or already renamed';
END
GO

-- Verify the changes
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ServiceType'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'ServiceType column name fix completed successfully';
GO
