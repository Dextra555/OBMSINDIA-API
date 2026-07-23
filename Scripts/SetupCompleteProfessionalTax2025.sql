-- Complete Professional Tax Setup for All States (2025-26)
-- Tamil Nadu updated to Greater Chennai Corporation latest rates

-- Clear existing data
DELETE FROM ProfessionalTaxConfiguration;

-- Tamil Nadu Professional Tax Slabs (2025-26 Greater Chennai Corporation)
-- Half-yearly deduction (Sep and Mar) - 6-month periods
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Tamil Nadu', 0, 21000, 0, 'SemiAnnual', '2025-04-01', 'ADMIN'),
('Tamil Nadu', 21000.01, 30000, 180, 'SemiAnnual', '2025-04-01', 'ADMIN'),
('Tamil Nadu', 30000.01, 45000, 425, 'SemiAnnual', '2025-04-01', 'ADMIN'),
('Tamil Nadu', 45000.01, 60000, 930, 'SemiAnnual', '2025-04-01', 'ADMIN'),
('Tamil Nadu', 60000.01, 75000, 1025, 'SemiAnnual', '2025-04-01', 'ADMIN'),
('Tamil Nadu', 75000.01, NULL, 1250, 'SemiAnnual', '2025-04-01', 'ADMIN');

-- Karnataka Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Karnataka', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Karnataka', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Maharashtra Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Maharashtra', 0, 7500, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Maharashtra', 7500.01, 10000, 175, 'Monthly', '2025-04-01', 'ADMIN'),
('Maharashtra', 10000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Telangana Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Telangana', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Telangana', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Andhra Pradesh Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Andhra Pradesh', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Andhra Pradesh', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Gujarat Professional Tax Slabs (Annual - Max ₹2,400/year)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Gujarat', 0, NULL, 200, 'Annual', '2025-04-01', 'ADMIN');

-- West Bengal Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('West Bengal', 0, 10000, 110, 'Monthly', '2025-04-01', 'ADMIN'),
('West Bengal', 10000.01, 15000, 130, 'Monthly', '2025-04-01', 'ADMIN'),
('West Bengal', 15000.01, 25000, 150, 'Monthly', '2025-04-01', 'ADMIN'),
('West Bengal', 25000.01, 40000, 180, 'Monthly', '2025-04-01', 'ADMIN'),
('West Bengal', 40000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Kerala Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Kerala', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Kerala', 15000.01, 20000, 80, 'Monthly', '2025-04-01', 'ADMIN'),
('Kerala', 20000.01, 30000, 150, 'Monthly', '2025-04-01', 'ADMIN'),
('Kerala', 30000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Delhi Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Delhi', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Delhi', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Rajasthan Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Rajasthan', 0, 7500, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Rajasthan', 7500.01, 10000, 175, 'Monthly', '2025-04-01', 'ADMIN'),
('Rajasthan', 10000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Punjab Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Punjab', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Punjab', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Haryana Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Haryana', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Haryana', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Uttar Pradesh Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Uttar Pradesh', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Uttar Pradesh', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Madhya Pradesh Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Madhya Pradesh', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Madhya Pradesh', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Bihar Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Bihar', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Bihar', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Odisha Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Odisha', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Odisha', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Assam Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Assam', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Assam', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Jharkhand Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Jharkhand', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Jharkhand', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Chhattisgarh Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Chhattisgarh', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Chhattisgarh', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Uttarakhand Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Uttarakhand', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Uttarakhand', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Himachal Pradesh Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Himachal Pradesh', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Himachal Pradesh', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Jammu & Kashmir Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Jammu & Kashmir', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Jammu & Kashmir', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Goa Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Goa', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Goa', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Chandigarh Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Chandigarh', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Chandigarh', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

-- Puducherry Professional Tax Slabs (Monthly)
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, TaxPeriod, EffectiveDate, CreatedBy) VALUES
('Puducherry', 0, 15000, 0, 'Monthly', '2025-04-01', 'ADMIN'),
('Puducherry', 15000.01, NULL, 200, 'Monthly', '2025-04-01', 'ADMIN');

PRINT 'Professional Tax Configuration setup complete for all states';
PRINT 'Tamil Nadu updated to 2025-26 Greater Chennai Corporation rates';
PRINT 'Tamil Nadu PT: Half-yearly deduction (Sep and Mar) with 6-month periods';
PRINT 'Other states: Monthly PT calculation';
