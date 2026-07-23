-- Create ESI Reason Code table for ESIC portal compliance
-- This table stores standard ESIC reason codes for zero working days and employee exits

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ESIReasonCode')
BEGIN
    CREATE TABLE ESIReasonCode (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Reason_Code NVARCHAR(10) NOT NULL,
        Reason_Description NVARCHAR(100) NOT NULL,
        Is_Active BIT NOT NULL DEFAULT 1,
        Created_Date DATETIME NOT NULL DEFAULT GETDATE()
    );

    -- Add unique constraint on Reason_Code
    ALTER TABLE ESIReasonCode ADD CONSTRAINT UQ_ESIReasonCode_Reason_Code UNIQUE (Reason_Code);

    -- Add index for faster lookups
    CREATE INDEX IX_ESIReasonCode_Reason_Code ON ESIReasonCode(Reason_Code);
    CREATE INDEX IX_ESIReasonCode_Is_Active ON ESIReasonCode(Is_Active);

    PRINT 'ESIReasonCode table created successfully.';
END
ELSE
BEGIN
    PRINT 'ESIReasonCode table already exists.';
END

-- Insert standard ESIC reason codes
-- Based on ESIC portal standards for zero working days and employee exits

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'RES')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('RES', 'Resignation', 1);
    PRINT 'Inserted RES - Resignation';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'ABS')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('ABS', 'Absconding', 1);
    PRINT 'Inserted ABS - Absconding';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'DTH')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('DTH', 'Death', 1);
    PRINT 'Inserted DTH - Death';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'RET')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('RET', 'Retirement', 1);
    PRINT 'Inserted RET - Retirement';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'DIS')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('DIS', 'Dismissal', 1);
    PRINT 'Inserted DIS - Dismissal';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'LVE')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('LVE', 'Long Leave', 1);
    PRINT 'Inserted LVE - Long Leave';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'MAT')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('MAT', 'Maternity Leave', 1);
    PRINT 'Inserted MAT - Maternity Leave';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'PAT')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('PAT', 'Paternity Leave', 1);
    PRINT 'Inserted PAT - Paternity Leave';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'MED')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('MED', 'Medical Leave', 1);
    PRINT 'Inserted MED - Medical Leave';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'TRA')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('TRA', 'Transfer', 1);
    PRINT 'Inserted TRA - Transfer';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'NOC')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('NOC', 'No Contribution', 1);
    PRINT 'Inserted NOC - No Contribution';
END

IF NOT EXISTS (SELECT * FROM ESIReasonCode WHERE Reason_Code = 'OTH')
BEGIN
    INSERT INTO ESIReasonCode (Reason_Code, Reason_Description, Is_Active)
    VALUES ('OTH', 'Other Reasons', 1);
    PRINT 'Inserted OTH - Other Reasons';
END

-- Add ReasonCode and LastWorkingDay columns to Attendance table if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Attendance') AND name = 'ReasonCode')
BEGIN
    ALTER TABLE Attendance ADD ReasonCode NVARCHAR(10) NULL;
    PRINT 'Added ReasonCode column to Attendance table.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Attendance') AND name = 'LastWorkingDay')
BEGIN
    ALTER TABLE Attendance ADD LastWorkingDay DATETIME NULL;
    PRINT 'Added LastWorkingDay column to Attendance table.';
END

PRINT 'ESI Reason Codes setup completed successfully.';
