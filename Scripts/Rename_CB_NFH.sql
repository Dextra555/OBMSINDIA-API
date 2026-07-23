-- Rename old CB_NFH column to CB_NFH_OLD to avoid confusion
USE obmsdev_backup;
GO

IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Employee' 
    AND COLUMN_NAME = 'CB_NFH'
)
BEGIN
    EXEC sp_rename 'Employee.CB_NFH', 'CB_NFH_OLD', 'COLUMN';
    PRINT 'Column CB_NFH renamed to CB_NFH_OLD successfully.';
END
ELSE
BEGIN
    PRINT 'Column CB_NFH does not exist - no action needed.';
END
GO
