-- Migration Script: Update Client Compliance Schema (Fixed Version)
-- Purpose: Replace individual compliance fields with simplified ClientComplianceStatus field
-- Date: 2025-04-09
-- Author: System

-- Step 1: Add new ClientComplianceStatus column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'ClientComplianceStatus')
BEGIN
    PRINT 'Adding ClientComplianceStatus column...';
    ALTER TABLE ClientMaster 
    ADD ClientComplianceStatus NVARCHAR(50) NULL;
    
    -- Set default value for existing records
    PRINT 'Setting default values for existing records...';
    UPDATE ClientMaster 
    SET ClientComplianceStatus = 'non_compliance_client' 
    WHERE ClientComplianceStatus IS NULL;
    
    PRINT 'Successfully added ClientComplianceStatus column to ClientMaster table';
END
ELSE
BEGIN
    PRINT 'ClientComplianceStatus column already exists in ClientMaster table';
END

-- Step 2: Create constraint for ClientComplianceStatus values (only if column exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'ClientComplianceStatus')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'CK_ClientMaster_ClientComplianceStatus')
    BEGIN
        PRINT 'Adding constraint for ClientComplianceStatus values...';
        ALTER TABLE ClientMaster
        ADD CONSTRAINT CK_ClientMaster_ClientComplianceStatus 
        CHECK (ClientComplianceStatus IN ('compliance_client', 'non_compliance_client'));
        
        PRINT 'Successfully added constraint for ClientComplianceStatus values';
    END
    ELSE
    BEGIN
        PRINT 'ClientComplianceStatus constraint already exists';
    END
END

-- Step 3: Create index for performance (only if column exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'ClientComplianceStatus')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'IX_ClientMaster_ClientComplianceStatus')
    BEGIN
        PRINT 'Creating index for ClientComplianceStatus...';
        CREATE INDEX IX_ClientMaster_ClientComplianceStatus 
        ON ClientMaster(ClientComplianceStatus);
        
        PRINT 'Successfully created index for ClientComplianceStatus';
    END
    ELSE
    BEGIN
        PRINT 'ClientComplianceStatus index already exists';
    END
END

-- Step 4: Drop old individual compliance columns (if they exist)
PRINT 'Checking for old compliance columns to drop...';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'IsGSTCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsGSTCompliant;
    PRINT 'Dropped IsGSTCompliant column';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'IsPANCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsPANCompliant;
    PRINT 'Dropped IsPANCompliant column';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'IsTANCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsTANCompliant;
    PRINT 'Dropped IsTANCompliant column';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'IsCINCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsCINCompliant;
    PRINT 'Dropped IsCINCompliant column';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'ComplianceCheckDate')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN ComplianceCheckDate;
    PRINT 'Dropped ComplianceCheckDate column';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClientMaster' AND COLUMN_NAME = 'ComplianceRemarks')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN ComplianceRemarks;
    PRINT 'Dropped ComplianceRemarks column';
END

-- Step 5: Verify the changes
PRINT '=== Migration Summary ===';
PRINT 'Added: ClientComplianceStatus (NVARCHAR(50))';
PRINT 'Added: CK_ClientMaster_ClientComplianceStatus constraint';
PRINT 'Added: IX_ClientMaster_ClientComplianceStatus index';
PRINT 'Removed: IsGSTCompliant, IsPANCompliant, IsTANCompliant, IsCINCompliant';
PRINT 'Removed: ComplianceCheckDate, ComplianceRemarks';

-- Step 6: Show current schema
PRINT '=== Current ClientMaster Schema ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ClientMaster' 
    AND (COLUMN_NAME LIKE '%Compliance%' OR COLUMN_NAME IN ('ID', 'Code', 'Name'))
ORDER BY ORDINAL_POSITION;

-- Step 7: Select sample data to verify
PRINT '=== Sample Data Verification ===';
SELECT TOP 5 
    ID, 
    Code, 
    Name, 
    ClientComplianceStatus,
    Status
FROM ClientMaster 
ORDER BY ID;

PRINT 'Migration completed successfully!';
