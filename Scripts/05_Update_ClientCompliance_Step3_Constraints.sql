-- Step 3: Add Constraints and Indexes
-- Purpose: Add validation constraints and performance indexes

PRINT '=== Step 3: Adding Constraints and Indexes ===';

-- Set proper options
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET NUMERIC_ROUNDABORT OFF;

-- Add constraint for valid values
PRINT 'Adding constraint for ClientComplianceStatus values...';
ALTER TABLE ClientMaster
ADD CONSTRAINT CK_ClientMaster_ClientComplianceStatus 
CHECK (ClientComplianceStatus IN ('compliance_client', 'non_compliance_client'));

PRINT 'Successfully added CK_ClientMaster_ClientComplianceStatus constraint';

-- Create index for performance
PRINT 'Creating index for ClientComplianceStatus...';
CREATE INDEX IX_ClientMaster_ClientComplianceStatus 
ON ClientMaster(ClientComplianceStatus);

PRINT 'Successfully created IX_ClientMaster_ClientComplianceStatus index';

-- Verification
PRINT '=== Final Verification ===';
PRINT 'Current ClientMaster columns (compliance related):';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClientMaster' 
    AND (COLUMN_NAME LIKE '%Compliance%' OR COLUMN_NAME IN ('ID', 'Code', 'Name'))
ORDER BY ORDINAL_POSITION;

PRINT 'Sample data with new compliance status:';
SELECT TOP 5 
    ID, 
    Code, 
    Name, 
    ClientComplianceStatus,
    Status
FROM ClientMaster 
ORDER BY ID;

PRINT 'Compliance status distribution:';
SELECT 
    ClientComplianceStatus,
    COUNT(*) as Count,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM ClientMaster) AS DECIMAL(10,2)) as Percentage
FROM ClientMaster 
GROUP BY ClientComplianceStatus;

PRINT '=== Migration Completed Successfully ===';
PRINT 'Summary of changes:';
PRINT '1. Added ClientComplianceStatus column (NVARCHAR(50))';
PRINT '2. Updated 17 existing records with default compliance status';
PRINT '3. Removed 6 old compliance columns (IsGSTCompliant, IsPANCompliant, IsTANCompliant, IsCINCompliant, ComplianceCheckDate, ComplianceRemarks)';
PRINT '4. Added validation constraint for compliance status values';
PRINT '5. Created performance index on ClientComplianceStatus';
PRINT '6. All old compliance tracking replaced with simplified approach';
