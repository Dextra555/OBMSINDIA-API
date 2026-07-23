-- Simple Migration Script: Update Client Compliance Schema
-- Purpose: Replace individual compliance fields with simplified ClientComplianceStatus field
-- Date: 2025-04-09

-- Step 1: Add the new column
PRINT 'Step 1: Adding ClientComplianceStatus column...';
ALTER TABLE ClientMaster 
ADD ClientComplianceStatus NVARCHAR(50) NULL;

-- Step 2: Set default values for existing records
PRINT 'Step 2: Setting default values...';
UPDATE ClientMaster 
SET ClientComplianceStatus = 'non_compliance_client' 
WHERE ClientComplianceStatus IS NULL;

-- Step 3: Drop old compliance columns
PRINT 'Step 3: Removing old compliance columns...';

-- Drop IsGSTCompliant if exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IsGSTCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsGSTCompliant;
    PRINT 'Dropped IsGSTCompliant';
END

-- Drop IsPANCompliant if exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IsPANCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsPANCompliant;
    PRINT 'Dropped IsPANCompliant';
END

-- Drop IsTANCompliant if exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IsTANCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsTANCompliant;
    PRINT 'Dropped IsTANCompliant';
END

-- Drop IsCINCompliant if exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IsCINCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsCINCompliant;
    PRINT 'Dropped IsCINCompliant';
END

-- Drop ComplianceCheckDate if exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'ComplianceCheckDate')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN ComplianceCheckDate;
    PRINT 'Dropped ComplianceCheckDate';
END

-- Drop ComplianceRemarks if exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'ComplianceRemarks')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN ComplianceRemarks;
    PRINT 'Dropped ComplianceRemarks';
END

-- Step 4: Add constraint for valid values
PRINT 'Step 4: Adding constraint...';
ALTER TABLE ClientMaster
ADD CONSTRAINT CK_ClientMaster_ClientComplianceStatus 
CHECK (ClientComplianceStatus IN ('compliance_client', 'non_compliance_client'));

-- Step 5: Create index
PRINT 'Step 5: Creating index...';
CREATE INDEX IX_ClientMaster_ClientComplianceStatus 
ON ClientMaster(ClientComplianceStatus);

-- Step 6: Verification
PRINT '=== Migration Complete ===';
PRINT 'Verifying changes...';

-- Show new column
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'ClientComplianceStatus';

-- Show sample data
SELECT TOP 3 ID, Code, Name, ClientComplianceStatus
FROM ClientMaster
ORDER BY ID;

PRINT 'Migration completed successfully!';
