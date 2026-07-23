-- Insert sample Professional Tax Configuration data
-- Clear existing data first
DELETE FROM ProfessionalTaxConfiguration WHERE State IN ('Maharashtra', 'Karnataka', 'Tamil Nadu', 'Delhi', 'Gujarat')

-- Maharashtra Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy) VALUES
('Maharashtra', 0, 7500, 0, '2024-04-01', 'ADMIN'),
('Maharashtra', 7500.01, 10000, 175, '2024-04-01', 'ADMIN'),
('Maharashtra', 10000.01, 15000, 200, '2024-04-01', 'ADMIN'),
('Maharashtra', 15000.01, 20000, 300, '2024-04-01', 'ADMIN'),
('Maharashtra', 20000.01, NULL, 400, '2024-04-01', 'ADMIN')

-- Karnataka Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy) VALUES
('Karnataka', 0, 14999, 0, '2024-04-01', 'ADMIN'),
('Karnataka', 15000, 19999, 150, '2024-04-01', 'ADMIN'),
('Karnataka', 20000, NULL, 200, '2024-04-01', 'ADMIN')

-- Tamil Nadu Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy) VALUES
('Tamil Nadu', 0, 3500, 0, '2024-04-01', 'ADMIN'),
('Tamil Nadu', 3501, 5000, 22.50, '2024-04-01', 'ADMIN'),
('Tamil Nadu', 5001, 7500, 52.50, '2024-04-01', 'ADMIN'),
('Tamil Nadu', 7501, 10000, 115, '2024-04-01', 'ADMIN'),
('Tamil Nadu', 10001, 12500, 171, '2024-04-01', 'ADMIN'),
('Tamil Nadu', 12501, 15000, 208.50, '2024-04-01', 'ADMIN'),
('Tamil Nadu', 15001, 20000, 236, '2024-04-01', 'ADMIN'),
('Tamil Nadu', 20001, 25000, 289, '2024-04-01', 'ADMIN'),
('Tamil Nadu', 25001, 30000, 335, '2024-04-01', 'ADMIN'),
('Tamil Nadu', 30001, NULL, 413.50, '2024-04-01', 'ADMIN')

-- Delhi Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy) VALUES
('Delhi', 0, 11000, 0, '2024-04-01', 'ADMIN'),
('Delhi', 11001, 18000, 200, '2024-04-01', 'ADMIN'),
('Delhi', 18001, NULL, 250, '2024-04-01', 'ADMIN')

-- Gujarat Professional Tax Slabs
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy) VALUES
('Gujarat', 0, 5999, 0, '2024-04-01', 'ADMIN'),
('Gujarat', 6000, 8999, 80, '2024-04-01', 'ADMIN'),
('Gujarat', 9000, 11999, 150, '2024-04-01', 'ADMIN'),
('Gujarat', 12000, 14999, 200, '2024-04-01', 'ADMIN'),
('Gujarat', 15000, 17999, 250, '2024-04-01', 'ADMIN'),
('Gujarat', 18000, 19999, 300, '2024-04-01', 'ADMIN'),
('Gujarat', 20000, NULL, 400, '2024-04-01', 'ADMIN')

-- Insert sample PF Configuration (without CreatedBy column)
DELETE FROM PFConfiguration
INSERT INTO PFConfiguration (Employee_Rate, Employer_Rate, Employer_EPS_Rate, Basic_Salary_Limit, Effective_Date, Financial_Year) VALUES
(12.00, 12.00, 8.33, 15000.00, '2024-04-01', '2024-2025')

-- Insert sample ESI Configuration (without CreatedBy column)
DELETE FROM ESIConfiguration
INSERT INTO ESIConfiguration (Employee_Rate, Employer_Rate, Basic_Salary_Limit, Effective_Date, Financial_Year) VALUES
(0.75, 3.25, 21000.00, '2024-04-01', '2024-2025')

-- Insert sample TDS Configuration
DELETE FROM TDSSlabConfiguration
INSERT INTO TDSSlabConfiguration (Financial_Year, Min_Income, Max_Income, Tax_Rate, Education_Cess_Rate, Higher_Education_Cess_Rate) VALUES
('2024-2025', 0, 250000, 0, 0, 0),
('2024-2025', 250001, 500000, 5, 2, 1),
('2024-2025', 500001, 750000, 10, 2, 1),
('2024-2025', 750001, 1000000, 15, 2, 1),
('2024-2025', 1000001, 1250000, 20, 2, 1),
('2024-2025', 1250001, 1500000, 25, 2, 1),
('2024-2025', 1500001, NULL, 30, 2, 1)

PRINT 'Sample professional tax data inserted successfully'
