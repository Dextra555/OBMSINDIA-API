-- Step 2: Remove Dependencies and Drop Old Columns
-- Purpose: Remove constraints, indexes, and old compliance columns

PRINT '=== Step 2: Removing Dependencies and Old Columns ===';

-- Set proper options
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET NUMERIC_ROUNDABORT OFF;

-- First, let's identify and drop constraints
PRINT 'Checking for constraints on old compliance columns...';

-- Drop default constraints if they exist
IF EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ClientMaster') AND COL_NAME(parent_object_id, parent_column_id) = 'IsGSTCompliant')
BEGIN
    DECLARE @constraint_name sysname;
    SELECT @constraint_name = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ClientMaster') AND COL_NAME(parent_object_id, parent_column_id) = 'IsGSTCompliant';
    EXEC('ALTER TABLE ClientMaster DROP CONSTRAINT ' + @constraint_name);
    PRINT 'Dropped default constraint for IsGSTCompliant';
END

IF EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ClientMaster') AND COL_NAME(parent_object_id, parent_column_id) = 'IsPANCompliant')
BEGIN
    SELECT @constraint_name = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ClientMaster') AND COL_NAME(parent_object_id, parent_column_id) = 'IsPANCompliant';
    EXEC('ALTER TABLE ClientMaster DROP CONSTRAINT ' + @constraint_name);
    PRINT 'Dropped default constraint for IsPANCompliant';
END

IF EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ClientMaster') AND COL_NAME(parent_object_id, parent_column_id) = 'IsTANCompliant')
BEGIN
    SELECT @constraint_name = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ClientMaster') AND COL_NAME(parent_object_id, parent_column_id) = 'IsTANCompliant';
    EXEC('ALTER TABLE ClientMaster DROP CONSTRAINT ' + @constraint_name);
    PRINT 'Dropped default constraint for IsTANCompliant';
END

IF EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ClientMaster') AND COL_NAME(parent_object_id, parent_column_id) = 'IsCINCompliant')
BEGIN
    SELECT @constraint_name = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ClientMaster') AND COL_NAME(parent_object_id, parent_column_id) = 'IsCINCompliant';
    EXEC('ALTER TABLE ClientMaster DROP CONSTRAINT ' + @constraint_name);
    PRINT 'Dropped default constraint for IsCINCompliant';
END

-- Drop indexes if they exist
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IX_ClientMaster_IsGSTCompliant')
BEGIN
    DROP INDEX IX_ClientMaster_IsGSTCompliant ON ClientMaster;
    PRINT 'Dropped index IX_ClientMaster_IsGSTCompliant';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IX_ClientMaster_IsPANCompliant')
BEGIN
    DROP INDEX IX_ClientMaster_IsPANCompliant ON ClientMaster;
    PRINT 'Dropped index IX_ClientMaster_IsPANCompliant';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IX_ClientMaster_IsTANCompliant')
BEGIN
    DROP INDEX IX_ClientMaster_IsTANCompliant ON ClientMaster;
    PRINT 'Dropped index IX_ClientMaster_IsTANCompliant';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('ClientMaster') AND name = 'IX_ClientMaster_IsCINCompliant')
BEGIN
    DROP INDEX IX_ClientMaster_IsCINCompliant ON ClientMaster;
    PRINT 'Dropped index IX_ClientMaster_IsCINCompliant';
END

-- Now drop the columns
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
