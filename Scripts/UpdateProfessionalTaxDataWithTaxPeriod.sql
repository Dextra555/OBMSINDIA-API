-- Update Professional Tax Data with TaxPeriod field
-- This script updates existing PT data to include proper TaxPeriod values

-- Clear existing data to avoid duplicates
DELETE FROM ProfessionalTaxConfiguration;

-- Tamil Nadu Professional Tax Slabs (SemiAnnual - 6 months)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Tamil Nadu', 0, 21000, 0, 'SemiAnnual', '2024-04-01', 'ADMIN'),
('Tamil Nadu', 21000.01, 30000, 100, 'SemiAnnual', '2024-04-01', 'ADMIN'),
('Tamil Nadu', 30000.01, 45000, 235, 'SemiAnnual', '2024-04-01', 'ADMIN'),
('Tamil Nadu', 45000.01, 60000, 510, 'SemiAnnual', '2024-04-01', 'ADMIN'),
('Tamil Nadu', 60000.01, 75000, 760, 'SemiAnnual', '2024-04-01', 'ADMIN'),
('Tamil Nadu', 75000.01, NULL, 1095, 'SemiAnnual', '2024-04-01', 'ADMIN');

-- Karnataka Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Karnataka', 0, 15000, 0, 'Monthly', '2024-04-01', 'ADMIN'),
('Karnataka', 15000.01, NULL, 200, 'Monthly', '2024-04-01', 'ADMIN');

-- Maharashtra Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Maharashtra', 0, 7500, 0, 'Monthly', '2024-04-01', 'ADMIN'),
('Maharashtra', 7500.01, 10000, 175, 'Monthly', '2024-04-01', 'ADMIN'),
('Maharashtra', 10000.01, NULL, 200, 'Monthly', '2024-04-01', 'ADMIN');

-- Telangana Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Telangana', 0, 15000, 0, 'Monthly', '2024-04-01', 'ADMIN'),
('Telangana', 15000.01, NULL, 200, 'Monthly', '2024-04-01', 'ADMIN');

-- Andhra Pradesh Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Andhra Pradesh', 0, 15000, 0, 'Monthly', '2024-04-01', 'ADMIN'),
('Andhra Pradesh', 15000.01, NULL, 200, 'Monthly', '2024-04-01', 'ADMIN');

-- Gujarat Professional Tax Slabs (Annual - Max ₹2,400/year)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Gujarat', 0, NULL, 200, 'Annual', '2024-04-01', 'ADMIN');

-- West Bengal Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('West Bengal', 0, 10000, 110, 'Monthly', '2024-04-01', 'ADMIN'),
('West Bengal', 10000.01, 15000, 130, 'Monthly', '2024-04-01', 'ADMIN'),
('West Bengal', 15000.01, 25000, 150, 'Monthly', '2024-04-01', 'ADMIN'),
('West Bengal', 25000.01, 40000, 180, 'Monthly', '2024-04-01', 'ADMIN'),
('West Bengal', 40000.01, NULL, 200, 'Monthly', '2024-04-01', 'ADMIN');

-- Kerala Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Kerala', 0, 15000, 0, 'Monthly', '2024-04-01', 'ADMIN'),
('Kerala', 15000.01, 20000, 80, 'Monthly', '2024-04-01', 'ADMIN'),
('Kerala', 20000.01, 30000, 150, 'Monthly', '2024-04-01', 'ADMIN'),
('Kerala', 30000.01, NULL, 200, 'Monthly', '2024-04-01', 'ADMIN');

-- Delhi Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Delhi', 0, 15000, 0, 'Monthly', '2024-04-01', 'ADMIN'),
('Delhi', 15000.01, NULL, 200, 'Monthly', '2024-04-01', 'ADMIN');

PRINT 'Professional Tax Configuration data updated with TaxPeriod field';
PRINT 'Tamil Nadu: SemiAnnual (6 months) - amounts will be divided by 6 for monthly calculation';
PRINT 'Gujarat: Annual - amounts will be divided by 12 for monthly calculation';
PRINT 'Other states: Monthly - no division needed';
