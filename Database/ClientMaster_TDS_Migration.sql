-- Add client-level TDS settings for receipt auto-calculation.
IF COL_LENGTH('dbo.ClientMaster', 'TDSApplicable') IS NULL
BEGIN
    ALTER TABLE dbo.ClientMaster
    ADD TDSApplicable BIT NOT NULL
        CONSTRAINT DF_ClientMaster_TDSApplicable DEFAULT (0);
END;
GO

IF COL_LENGTH('dbo.ClientMaster', 'TDSPercentage') IS NULL
BEGIN
    ALTER TABLE dbo.ClientMaster
    ADD TDSPercentage DECIMAL(5,2) NOT NULL
        CONSTRAINT DF_ClientMaster_TDSPercentage DEFAULT (0);
END;
GO
