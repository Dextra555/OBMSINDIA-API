-- Update Karnataka Professional Tax Slabs with Correct Values
-- Based on user requirements:
-- 1. Upto 24,999: ₹0.00
-- 2. 25,000 and Above: ₹200.00

-- Clear existing Karnataka data
DELETE FROM ProfessionalTaxConfiguration WHERE State = 'Karnataka';

-- Karnataka Professional Tax Slabs (Corrected)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Karnataka', 0, 24999, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Karnataka', 25000, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

PRINT 'Karnataka Professional Tax slabs updated successfully';
PRINT 'New Rates:';
PRINT '- Karnataka: Salary up to ₹24,999: ₹0 per month';
PRINT '- Karnataka: Salary ₹25,000 and above: ₹200 per month';
