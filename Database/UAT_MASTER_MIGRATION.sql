-- ============================================================
-- UAT MASTER MIGRATION SCRIPT
-- Run this once on UAT database to bring it up to date
-- Date: 2026-10-29
-- All statements are IDEMPOTENT - safe to run multiple times
-- ============================================================

PRINT '====== UAT Master Migration Started ======';
GO

-- ============================================================
-- 1. ClientMaster - GST Columns
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='ClientMaster' AND COLUMN_NAME='GSTApplicable')
BEGIN
    ALTER TABLE ClientMaster ADD GSTApplicable BIT NOT NULL DEFAULT 0;
    PRINT '1. GSTApplicable added to ClientMaster';
END
ELSE PRINT '1. ClientMaster.GSTApplicable already exists';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='ClientMaster' AND COLUMN_NAME='GSTPercentage')
BEGIN
    ALTER TABLE ClientMaster ADD GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT '   GSTPercentage added to ClientMaster';
END
GO

-- ============================================================
-- 2. ClientMaster - TDS Columns
-- ============================================================
IF COL_LENGTH('dbo.ClientMaster', 'TDSApplicable') IS NULL
BEGIN
    ALTER TABLE dbo.ClientMaster ADD TDSApplicable BIT NOT NULL CONSTRAINT DF_ClientMaster_TDSApplicable DEFAULT (0);
    PRINT '2. TDSApplicable added to ClientMaster';
END
ELSE PRINT '2. ClientMaster.TDSApplicable already exists';

IF COL_LENGTH('dbo.ClientMaster', 'TDSPercentage') IS NULL
BEGIN
    ALTER TABLE dbo.ClientMaster ADD TDSPercentage DECIMAL(5,2) NOT NULL CONSTRAINT DF_ClientMaster_TDSPercentage DEFAULT (0);
    PRINT '   TDSPercentage added to ClientMaster';
END
GO

-- ============================================================
-- 3. Suppliers - IsTax, GSTNo, PanNo Columns
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Suppliers' AND COLUMN_NAME='IsTax')
BEGIN
    ALTER TABLE Suppliers ADD IsTax BIT NOT NULL DEFAULT 0;
    PRINT '3. IsTax added to Suppliers';
END
ELSE PRINT '3. Suppliers.IsTax already exists';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Suppliers' AND COLUMN_NAME='GSTNo')
BEGIN
    ALTER TABLE Suppliers ADD GSTNo NVARCHAR(20) NULL;
    PRINT '   GSTNo added to Suppliers';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Suppliers' AND COLUMN_NAME='PanNo')
BEGIN
    ALTER TABLE Suppliers ADD PanNo NVARCHAR(20) NULL;
    PRINT '   PanNo added to Suppliers';
END
GO

-- ============================================================
-- 4. Suppliers - GSTPercentage Column
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Suppliers' AND COLUMN_NAME='GSTPercentage')
BEGIN
    ALTER TABLE Suppliers ADD GSTPercentage DECIMAL(5,2) NULL;
    PRINT '4. GSTPercentage added to Suppliers';
END
ELSE PRINT '4. Suppliers.GSTPercentage already exists';
GO

-- ============================================================
-- 5. Suppliers - IsTDS, TDSPercentage Columns
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Suppliers' AND COLUMN_NAME='IsTDS')
BEGIN
    ALTER TABLE Suppliers ADD IsTDS BIT NOT NULL DEFAULT 0;
    PRINT '5. IsTDS added to Suppliers';
END
ELSE PRINT '5. Suppliers.IsTDS already exists';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Suppliers' AND COLUMN_NAME='TDSPercentage')
BEGIN
    ALTER TABLE Suppliers ADD TDSPercentage DECIMAL(5,2) NULL;
    PRINT '   TDSPercentage added to Suppliers';
END
GO

-- ============================================================
-- 6. CreditorInvoice - GST & TDS Columns
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='GSTPercentage')
BEGIN
    ALTER TABLE CreditorInvoice ADD GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT '6. GSTPercentage added to CreditorInvoice';
END
ELSE PRINT '6. CreditorInvoice.GSTPercentage already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='GSTAmount')
BEGIN
    ALTER TABLE CreditorInvoice ADD GSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT '   GSTAmount added to CreditorInvoice';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='TDSPercentage')
BEGIN
    ALTER TABLE CreditorInvoice ADD TDSPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT '   TDSPercentage added to CreditorInvoice';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='TDSAmount')
BEGIN
    ALTER TABLE CreditorInvoice ADD TDSAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT '   TDSAmount added to CreditorInvoice';
END
GO

-- Drop old GSTApplicable / TDSApplicable from CreditorInvoice if exists
DECLARE @sql NVARCHAR(500);
SELECT @sql = 'ALTER TABLE CreditorInvoice DROP CONSTRAINT ' + dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
WHERE c.object_id=OBJECT_ID('CreditorInvoice') AND c.name='GSTApplicable';
IF @sql IS NOT NULL EXEC sp_executesql @sql;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='GSTApplicable')
BEGIN
    ALTER TABLE CreditorInvoice DROP COLUMN GSTApplicable;
    PRINT '   GSTApplicable dropped from CreditorInvoice';
END
GO

DECLARE @sql2 NVARCHAR(500);
SELECT @sql2 = 'ALTER TABLE CreditorInvoice DROP CONSTRAINT ' + dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
WHERE c.object_id=OBJECT_ID('CreditorInvoice') AND c.name='TDSApplicable';
IF @sql2 IS NOT NULL EXEC sp_executesql @sql2;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='TDSApplicable')
BEGIN
    ALTER TABLE CreditorInvoice DROP COLUMN TDSApplicable;
    PRINT '   TDSApplicable dropped from CreditorInvoice';
END
GO

-- ============================================================
-- 7. CreditorInvoice - CGST, SGST, IGST Columns
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='CGSTPercentage')
BEGIN
    ALTER TABLE CreditorInvoice ADD CGSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT '7. CGSTPercentage added to CreditorInvoice';
END
ELSE PRINT '7. CreditorInvoice CGST/SGST/IGST columns check:';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='CGSTAmount')
    ALTER TABLE CreditorInvoice ADD CGSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='SGSTPercentage')
    ALTER TABLE CreditorInvoice ADD SGSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='SGSTAmount')
    ALTER TABLE CreditorInvoice ADD SGSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='IGSTPercentage')
    ALTER TABLE CreditorInvoice ADD IGSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='IGSTAmount')
    ALTER TABLE CreditorInvoice ADD IGSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('CreditorInvoice') AND name='TransactionType')
    ALTER TABLE CreditorInvoice ADD TransactionType NVARCHAR(1) NOT NULL DEFAULT 'I';
PRINT '   CGST/SGST/IGST columns done';
GO

-- Backfill CGST/SGST from existing GSTAmount
UPDATE CreditorInvoice
SET CGSTPercentage=GSTPercentage/2, CGSTAmount=GSTAmount/2,
    SGSTPercentage=GSTPercentage/2, SGSTAmount=GSTAmount/2
WHERE GSTAmount > 0 AND CGSTAmount=0 AND SGSTAmount=0;
PRINT '   CGST/SGST backfill done';
GO

-- ============================================================
-- 8. Employee - TDSApplicable Column
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[dbo].[Employee]') AND name=N'TDSApplicable')
BEGIN
    ALTER TABLE [dbo].[Employee] ADD [TDSApplicable] BIT NOT NULL DEFAULT 0;
    PRINT '8. TDSApplicable added to Employee';
END
ELSE PRINT '8. Employee.TDSApplicable already exists';
GO

-- ============================================================
-- 9. PaySlip & PaySlipAudit - TDS Column
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[dbo].[PaySlip]') AND name=N'TDS')
BEGIN
    ALTER TABLE [dbo].[PaySlip] ADD [TDS] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT '9. TDS added to PaySlip';
END
ELSE PRINT '9. PaySlip.TDS already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[dbo].[PaySlipAudit]') AND name=N'TDS')
BEGIN
    ALTER TABLE [dbo].[PaySlipAudit] ADD [TDS] DECIMAL(10,2) NOT NULL DEFAULT 0;
    PRINT '   TDS added to PaySlipAudit';
END
GO

-- ============================================================
-- 10. CreditNote & DebitNote Tables
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='CreditNote')
BEGIN
    CREATE TABLE CreditNote (
        ID                  INT             PRIMARY KEY IDENTITY(1,1),
        CreditNoteNo        NVARCHAR(50)    NOT NULL,
        Branch              NVARCHAR(20)    NOT NULL,
        Client              NVARCHAR(50)    NOT NULL,
        AgreementID         INT             NULL,
        ClientInvoiceID     INT             NULL,
        CreditNoteDate      DATETIME        NOT NULL,
        CreditNoteAmount    DECIMAL(18,2)   NOT NULL,
        TaxPercentage       DECIMAL(18,2)   NOT NULL DEFAULT 0,
        TaxAmount           DECIMAL(18,2)   NOT NULL DEFAULT 0,
        TotalAmount         DECIMAL(18,2)   NOT NULL DEFAULT 0,
        Reason              NVARCHAR(1000)  NULL,
        ReferenceInvoiceNo  NVARCHAR(50)    NULL,
        Status              NVARCHAR(20)    NOT NULL DEFAULT 'Pending',
        ApprovalDate        DATETIME        NULL,
        ApprovedBy          NVARCHAR(50)    NULL,
        CreatedDate         DATETIME        NOT NULL DEFAULT GETDATE(),
        LastUpdatedDate     DATETIME        NULL,
        LastUpdatedBy       NVARCHAR(50)    NULL,
        IsDeleted           BIT             NOT NULL DEFAULT 0
    );
    CREATE UNIQUE INDEX UX_CreditNote_CreditNoteNo ON CreditNote(CreditNoteNo) WHERE IsDeleted=0;
    CREATE INDEX IX_CreditNote_Client_Branch ON CreditNote(Client, Branch);
    PRINT '10. CreditNote table created';
END
ELSE
BEGIN
    -- Add Tax columns if table exists but columns are missing
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CreditNote' AND COLUMN_NAME='TaxPercentage')
        ALTER TABLE CreditNote ADD TaxPercentage DECIMAL(18,2) NOT NULL DEFAULT 0;
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CreditNote' AND COLUMN_NAME='TaxAmount')
        ALTER TABLE CreditNote ADD TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CreditNote' AND COLUMN_NAME='TotalAmount')
        ALTER TABLE CreditNote ADD TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT '10. CreditNote already exists (columns verified)';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='DebitNote')
BEGIN
    CREATE TABLE DebitNote (
        ID                  INT             PRIMARY KEY IDENTITY(1,1),
        DebitNoteNo         NVARCHAR(50)    NOT NULL,
        Branch              NVARCHAR(20)    NOT NULL,
        Client              NVARCHAR(50)    NOT NULL,
        AgreementID         INT             NULL,
        ClientInvoiceID     INT             NULL,
        DebitNoteDate       DATETIME        NOT NULL,
        DebitNoteAmount     DECIMAL(18,2)   NOT NULL,
        TaxPercentage       DECIMAL(18,2)   NOT NULL DEFAULT 0,
        TaxAmount           DECIMAL(18,2)   NOT NULL DEFAULT 0,
        TotalAmount         DECIMAL(18,2)   NOT NULL DEFAULT 0,
        Reason              NVARCHAR(1000)  NULL,
        ReferenceInvoiceNo  NVARCHAR(50)    NULL,
        Status              NVARCHAR(20)    NOT NULL DEFAULT 'Pending',
        ApprovalDate        DATETIME        NULL,
        ApprovedBy          NVARCHAR(50)    NULL,
        DueDate             DATETIME        NULL,
        CreatedDate         DATETIME        NOT NULL DEFAULT GETDATE(),
        LastUpdatedDate     DATETIME        NULL,
        LastUpdatedBy       NVARCHAR(50)    NULL,
        IsDeleted           BIT             NOT NULL DEFAULT 0
    );
    CREATE UNIQUE INDEX UX_DebitNote_DebitNoteNo ON DebitNote(DebitNoteNo) WHERE IsDeleted=0;
    CREATE INDEX IX_DebitNote_Client_Branch ON DebitNote(Client, Branch);
    PRINT '    DebitNote table created';
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='DebitNote' AND COLUMN_NAME='TaxPercentage')
        ALTER TABLE DebitNote ADD TaxPercentage DECIMAL(18,2) NOT NULL DEFAULT 0;
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='DebitNote' AND COLUMN_NAME='TaxAmount')
        ALTER TABLE DebitNote ADD TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='DebitNote' AND COLUMN_NAME='TotalAmount')
        ALTER TABLE DebitNote ADD TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT '    DebitNote already exists (columns verified)';
END
GO

-- ScreenList entries for CreditNote/DebitNote
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName='CreditNote')
    INSERT INTO ScreenList(ScreenName, Category, LastUpdate, LastUpdatedBy) VALUES('CreditNote','Finance',GETDATE(),'MIGRATION');
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName='DebitNote')
    INSERT INTO ScreenList(ScreenName, Category, LastUpdate, LastUpdatedBy) VALUES('DebitNote','Finance',GETDATE(),'MIGRATION');
PRINT '    ScreenList entries done';
GO

-- ============================================================
-- 11. TDSMaster Table + Seed Data
-- ============================================================
IF OBJECT_ID(N'dbo.TDSMaster', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TDSMaster (
        ID            INT          IDENTITY(1,1) NOT NULL,
        TDSName       VARCHAR(100)               NOT NULL,
        TDSPercentage DECIMAL(5,2)               NOT NULL,
        IsActive      BIT          NOT NULL CONSTRAINT DF_TDSMaster_IsActive DEFAULT (1),
        CONSTRAINT PK_TDSMaster PRIMARY KEY CLUSTERED (ID ASC)
    );
    PRINT '11. TDSMaster table created';
END
ELSE PRINT '11. TDSMaster already exists';

IF NOT EXISTS (SELECT 1 FROM dbo.TDSMaster)
BEGIN
    INSERT INTO dbo.TDSMaster(TDSName, TDSPercentage, IsActive) VALUES
        (N'Contractor - Individual / HUF (Sec 194C)',  1.00, 1),
        (N'Contractor - Other Entities (Sec 194C)',    2.00, 1),
        (N'Commission / Brokerage (Sec 194H)',         2.00, 1),
        (N'Rent - Plant and Machinery (Sec 194I(b))',  2.00, 1),
        (N'Rent - Land and Building (Sec 194I(a))',   10.00, 1),
        (N'Professional Fees (Sec 194J)',             10.00, 1),
        (N'Technical Services (Sec 194J)',             2.00, 1),
        (N'Interest - Other (Sec 194A)',              10.00, 1),
        (N'Dividend (Sec 194)',                       10.00, 1),
        (N'Lottery Winnings (Sec 194B)',              30.00, 1);
    PRINT '    10 TDS seed records inserted';
END
ELSE PRINT '    TDSMaster already has data - skipped';
GO

-- ============================================================
-- FINAL VERIFICATION
-- ============================================================
PRINT '';
PRINT '====== Verification ======';

SELECT 'ClientMaster' AS TableName, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='ClientMaster' AND COLUMN_NAME IN ('GSTApplicable','GSTPercentage','TDSApplicable','TDSPercentage')

UNION ALL

SELECT 'Suppliers', COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='Suppliers' AND COLUMN_NAME IN ('IsTax','GSTNo','PanNo','GSTPercentage','IsTDS','TDSPercentage')

UNION ALL

SELECT 'CreditorInvoice', COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='CreditorInvoice' AND COLUMN_NAME IN ('GSTPercentage','GSTAmount','CGSTPercentage','CGSTAmount','SGSTPercentage','SGSTAmount','IGSTPercentage','IGSTAmount','TDSPercentage','TDSAmount','TransactionType')

UNION ALL

SELECT 'Employee', COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='Employee' AND COLUMN_NAME='TDSApplicable'

UNION ALL

SELECT 'PaySlip', COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='PaySlip' AND COLUMN_NAME='TDS'

ORDER BY TableName, COLUMN_NAME;

SELECT ID, TDSName, TDSPercentage, IsActive FROM dbo.TDSMaster ORDER BY ID;

PRINT '====== UAT Master Migration Completed ======';
GO
