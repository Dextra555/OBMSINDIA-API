-- Update Karnataka and Andhra Pradesh Professional Tax Slabs
-- Based on user requirements:
-- 1. Salary/wage earners with monthly salary ₹25,000 and above: ₹200 per month
-- 2. Persons registered under Karnataka GST Act: ₹2,500 per annum

-- Clear existing Karnataka and Andhra Pradesh data
DELETE FROM ProfessionalTaxConfiguration WHERE State IN ('Karnataka', 'Andhra Pradesh');

-- Karnataka Professional Tax Slabs (Updated)
-- Based on: Salary ₹25,000 and above = ₹200 per month (₹1,200 for 6 months)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Karnataka', 0, 24999, 0, 'SemiAnnual', '2025-04-01', 'ADMIN'),
('Karnataka', 25000, NULL, 1200, 'SemiAnnual', '2025-04-01', 'ADMIN'),
('Karnataka', 0, NULL, 2500, 'Annual', '2025-04-01', 'ADMIN');

-- Andhra Pradesh Professional Tax Slabs (Updated)
-- Based on: Salary ₹25,000 and above = ₹200 per month
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Andhra Pradesh', 0, 24999, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Andhra Pradesh', 25000, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

PRINT 'Karnataka and Andhra Pradesh Professional Tax slabs updated successfully';
PRINT 'New Rates:';
PRINT '- Karnataka: Salary up to ₹24,999: ₹0 per 6 months';
PRINT '- Karnataka: Salary ₹25,000 and above: ₹1,200 per 6 months (₹200/month)';
PRINT '- Karnataka GST registered: ₹2,500 per annum';
PRINT '- Andhra Pradesh: Salary up to ₹24,999: ₹0 per month';
PRINT '- Andhra Pradesh: Salary ₹25,000 and above: ₹200 per month';
