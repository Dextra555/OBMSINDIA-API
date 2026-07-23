-- Step 2: Update Data and Drop Old Columns (Fixed)
-- Purpose: Update existing data and remove old compliance columns

PRINT '=== Step 2: Updating Data and Removing Old Columns ===';

-- Set proper options for the update
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET NUMERIC_ROUNDABORT OFF;

-- Update existing records with default values based on old compliance logic
PRINT 'Updating existing records with compliance status...';
UPDATE ClientMaster 
SET ClientComplianceStatus = 
    CASE 
        WHEN IsGSTCompliant = 1 AND IsPANCompliant = 1 AND IsTANCompliant = 1 AND IsCINCompliant = 1 
        THEN 'compliance_client'
        ELSE 'non_compliance_client'
    END
WHERE ClientComplianceStatus IS NULL;

PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records with compliance status';

-- Show current data before dropping columns
PRINT 'Current compliance status distribution:';
SELECT 
    ClientComplianceStatus,
    COUNT(*) as Count
FROM ClientMaster 
GROUP BY ClientComplianceStatus;

-- Drop old individual compliance columns
PRINT 'Dropping old compliance columns...';

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

PRINT 'Step 2 completed successfully!';
