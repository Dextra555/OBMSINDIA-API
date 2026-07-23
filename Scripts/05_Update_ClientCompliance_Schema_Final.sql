-- Final Migration Script: Update Client Compliance Schema
-- Purpose: Replace individual compliance fields with simplified ClientComplianceStatus field
-- Date: 2025-04-09

PRINT '=== Starting Client Compliance Schema Migration ===';

-- Step 1: Add the new ClientComplianceStatus column
PRINT 'Step 1: Adding ClientComplianceStatus column...';
ALTER TABLE ClientMaster 
ADD ClientComplianceStatus NVARCHAR(50) NULL;

-- Step 2: Set default values for existing records based on old compliance logic
PRINT 'Step 2: Setting default values for existing records...';
UPDATE ClientMaster 
SET ClientComplianceStatus = 
    CASE 
        WHEN IsGSTCompliant = 1 AND IsPANCompliant = 1 AND IsTANCompliant = 1 AND IsCINCompliant = 1 
        THEN 'compliance_client'
        ELSE 'non_compliance_client'
    END
WHERE ClientComplianceStatus IS NULL;

PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records with compliance status';

-- Step 3: Drop old individual compliance columns
PRINT 'Step 3: Removing old compliance columns...';

ALTER TABLE ClientMaster DROP COLUMN IsGSTCompliant;
PRINT 'Dropped IsGSTCompliant';

ALTER TABLE ClientMaster DROP COLUMN IsPANCompliant;
PRINT 'Dropped IsPANCompliant';

ALTER TABLE ClientMaster DROP COLUMN IsTANCompliant;
PRINT 'Dropped IsTANCompliant';

ALTER TABLE ClientMaster DROP COLUMN IsCINCompliant;
PRINT 'Dropped IsCINCompliant';

ALTER TABLE ClientMaster DROP COLUMN ComplianceCheckDate;
PRINT 'Dropped ComplianceCheckDate';

ALTER TABLE ClientMaster DROP COLUMN ComplianceRemarks;
PRINT 'Dropped ComplianceRemarks';

-- Step 4: Add constraint for valid values
PRINT 'Step 4: Adding constraint for valid values...';
ALTER TABLE ClientMaster
ADD CONSTRAINT CK_ClientMaster_ClientComplianceStatus 
CHECK (ClientComplianceStatus IN ('compliance_client', 'non_compliance_client'));

-- Step 5: Create index for performance
PRINT 'Step 5: Creating index for performance...';
CREATE INDEX IX_ClientMaster_ClientComplianceStatus 
ON ClientMaster(ClientComplianceStatus);

-- Step 6: Verification
PRINT '=== Migration Verification ===';
PRINT 'New column details:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'ClientComplianceStatus';

PRINT 'Sample data with new compliance status:';
SELECT TOP 5 
    ID, 
    Code, 
    Name, 
    ClientComplianceStatus,
    Status
FROM ClientMaster 
ORDER BY ID;

PRINT 'Total records by compliance status:';
SELECT 
    ClientComplianceStatus,
    COUNT(*) as Count
FROM ClientMaster 
GROUP BY ClientComplianceStatus;

PRINT '=== Migration Completed Successfully ===';
