-- Update Andhra Pradesh Professional Tax Slabs with Correct Values
-- Based on user requirements:
-- 1. Upto 15000: ₹0.00
-- 2. Between 15001 To 20000: ₹150.00
-- 3. Above 20001: ₹200.00

-- Clear existing Andhra Pradesh data
DELETE FROM ProfessionalTaxConfiguration WHERE State = 'Andhra Pradesh';

-- Andhra Pradesh Professional Tax Slabs (Corrected)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Andhra Pradesh', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Andhra Pradesh', 15001, 20000, 150, 'Monthly', '2025-04-01', 'ADMIN'),
('Andhra Pradesh', 20001, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

PRINT 'Andhra Pradesh Professional Tax slabs updated successfully';
PRINT 'New Rates:';
PRINT '- Andhra Pradesh: Salary up to ₹15,000: ₹0 per month';
PRINT '- Andhra Pradesh: Salary ₹15,001 to ₹20,000: ₹150 per month';
PRINT '- Andhra Pradesh: Salary above ₹20,001: ₹200 per month';
