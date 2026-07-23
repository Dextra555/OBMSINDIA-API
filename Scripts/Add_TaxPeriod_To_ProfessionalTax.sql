-- Add TaxPeriod column to ProfessionalTaxConfiguration table
-- This will track whether tax amounts are Monthly, SemiAnnual (6 months), or Annual

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProfessionalTaxConfiguration]') AND name = 'TaxPeriod')
BEGIN
    ALTER TABLE ProfessionalTaxConfiguration 
    ADD TaxPeriod NVARCHAR(20) NOT NULL DEFAULT 'Monthly';
    
    PRINT 'TaxPeriod column added to ProfessionalTaxConfiguration table successfully';
END
ELSE
BEGIN
    PRINT 'TaxPeriod column already exists in ProfessionalTaxConfiguration table';
END
