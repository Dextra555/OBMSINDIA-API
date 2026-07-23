-- Debug Migration Script: Update Client Compliance Schema
-- Purpose: Step-by-step debugging and migration
-- Date: 2025-04-09

-- First, let's see what columns currently exist
PRINT '=== Current ClientMaster Columns ===';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClientMaster'
ORDER BY ORDINAL_POSITION;

-- Check if old compliance columns exist
PRINT '=== Checking for Old Compliance Columns ===';
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IsGSTCompliant')
    PRINT 'Found: IsGSTCompliant';
ELSE
    PRINT 'Not Found: IsGSTCompliant';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'ClientComplianceStatus')
    PRINT 'Found: ClientComplianceStatus';
ELSE
    PRINT 'Not Found: ClientComplianceStatus';

-- Try to add the new column with error handling
PRINT '=== Attempting to Add ClientComplianceStatus Column ===';
BEGIN TRY
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'ClientComplianceStatus')
    BEGIN
        ALTER TABLE ClientMaster ADD ClientComplianceStatus NVARCHAR(50) NULL;
        PRINT 'Successfully added ClientComplianceStatus column';
    END
    ELSE
    BEGIN
        PRINT 'ClientComplianceStatus column already exists';
    END
END TRY
BEGIN CATCH
    PRINT 'Error adding column: ' + ERROR_MESSAGE();
END CATCH

-- Verify the column was added
PRINT '=== Verifying Column Addition ===';
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'ClientComplianceStatus')
BEGIN
    PRINT 'SUCCESS: ClientComplianceStatus column exists';
    
    -- Show column details
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'ClientComplianceStatus';
    
    -- Update existing records
    PRINT 'Updating existing records with default value...';
    UPDATE ClientMaster 
    SET ClientComplianceStatus = 'non_compliance_client' 
    WHERE ClientComplianceStatus IS NULL;
    
    PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
END
ELSE
BEGIN
    PRINT 'FAILED: ClientComplianceStatus column was not added';
END

-- Show sample data
PRINT '=== Sample Data ===';
SELECT TOP 3 
    ID, 
    Code, 
    Name, 
    ClientComplianceStatus
FROM ClientMaster 
ORDER BY ID;
