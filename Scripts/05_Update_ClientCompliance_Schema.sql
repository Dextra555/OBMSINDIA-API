-- Migration Script: Update Client Compliance Schema
-- Purpose: Replace individual compliance fields with simplified ClientComplianceStatus field
-- Date: 2025-04-09
-- Author: System

-- Add new ClientComplianceStatus column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'ClientComplianceStatus')
BEGIN
    ALTER TABLE ClientMaster 
    ADD ClientComplianceStatus NVARCHAR(50) NULL;
    
    -- Set default value for existing records
    UPDATE ClientMaster 
    SET ClientComplianceStatus = 'non_compliance_client' 
    WHERE ClientComplianceStatus IS NULL;
    
    PRINT 'Successfully added ClientComplianceStatus column to ClientMaster table';
END
ELSE
BEGIN
    PRINT 'ClientComplianceStatus column already exists in ClientMaster table';
END

-- Create constraint for ClientComplianceStatus values
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'CK_ClientMaster_ClientComplianceStatus')
BEGIN
    ALTER TABLE ClientMaster
    ADD CONSTRAINT CK_ClientMaster_ClientComplianceStatus 
    CHECK (ClientComplianceStatus IN ('compliance_client', 'non_compliance_client'));
    
    PRINT 'Successfully added constraint for ClientComplianceStatus values';
END
ELSE
BEGIN
    PRINT 'ClientComplianceStatus constraint already exists';
END

-- Create index for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'IX_ClientMaster_ClientComplianceStatus')
BEGIN
    CREATE INDEX IX_ClientMaster_ClientComplianceStatus 
    ON ClientMaster(ClientComplianceStatus);
    
    PRINT 'Successfully created index for ClientComplianceStatus';
END
ELSE
BEGIN
    PRINT 'ClientComplianceStatus index already exists';
END

-- Drop old individual compliance columns (if they exist)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'IsGSTCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsGSTCompliant;
    PRINT 'Dropped IsGSTCompliant column';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'IsPANCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsPANCompliant;
    PRINT 'Dropped IsPANCompliant column';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'IsTANCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsTANCompliant;
    PRINT 'Dropped IsTANCompliant column';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'IsCINCompliant')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN IsCINCompliant;
    PRINT 'Dropped IsCINCompliant column';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'ComplianceCheckDate')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN ComplianceCheckDate;
    PRINT 'Dropped ComplianceCheckDate column';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ClientMaster]') AND name = N'ComplianceRemarks')
BEGIN
    ALTER TABLE ClientMaster DROP COLUMN ComplianceRemarks;
    PRINT 'Dropped ComplianceRemarks column';
END

-- Verify the changes
PRINT '=== Migration Summary ===';
PRINT 'Added: ClientComplianceStatus (NVARCHAR(50))';
PRINT 'Added: CK_ClientMaster_ClientComplianceStatus constraint';
PRINT 'Added: IX_ClientMaster_ClientComplianceStatus index';
PRINT 'Removed: IsGSTCompliant, IsPANCompliant, IsTANCompliant, IsCINCompliant';
PRINT 'Removed: ComplianceCheckDate, ComplianceRemarks';
PRINT 'Migration completed successfully!';

-- Select sample data to verify
SELECT TOP 5 
    ID, 
    Code, 
    Name, 
    ClientComplianceStatus,
    Status
FROM ClientMaster 
ORDER BY ID;
