-- Step 2: Final Cleanup of Remaining Dependencies
-- Purpose: Remove remaining indexes and drop old compliance columns

PRINT '=== Step 2: Final Cleanup of Remaining Dependencies ===';

-- Set proper options
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET NUMERIC_ROUNDABORT OFF;

-- Drop remaining index on ComplianceCheckDate
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IX_ClientMaster_ComplianceCheckDate')
BEGIN
    DROP INDEX IX_ClientMaster_ComplianceCheckDate ON ClientMaster;
    PRINT 'Dropped index IX_ClientMaster_ComplianceCheckDate';
END

-- Drop remaining columns
ALTER TABLE ClientMaster DROP COLUMN ComplianceCheckDate;
PRINT 'Dropped ComplianceCheckDate';

ALTER TABLE ClientMaster DROP COLUMN ComplianceRemarks;
PRINT 'Dropped ComplianceRemarks';

PRINT 'Step 2 final cleanup completed successfully!';
