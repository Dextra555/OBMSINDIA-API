-- Simple Professional Tax Data Insertion
-- Clear existing data
DELETE FROM ProfessionalTaxConfiguration WHERE State IN ('Tamil Nadu', 'Karnataka', 'Maharashtra', 'Telangana', 'Andhra Pradesh', 'Gujarat', 'West Bengal', 'Kerala', 'Madhya Pradesh', 'Odisha', 'Assam', 'Jharkhand', 'Chhattisgarh', 'Meghalaya', 'Tripura', 'Sikkim', 'Bihar');

-- Insert Tamil Nadu data
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Tamil Nadu', 0, 21000, 0, '2024-04-01', 'ADMIN', 'Up to ₹21,000 (6 months)'),
('Tamil Nadu', 21000.01, 30000, 100, '2024-04-01', 'ADMIN', '₹21,001 – ₹30,000'),
('Tamil Nadu', 30000.01, 45000, 235, '2024-04-01', 'ADMIN', '₹30,001 – ₹45,000'),
('Tamil Nadu', 45000.01, 60000, 510, '2024-04-01', 'ADMIN', '₹45,001 – ₹60,000'),
('Tamil Nadu', 60000.01, 75000, 760, '2024-04-01', 'ADMIN', '₹60,001 – ₹75,000'),
('Tamil Nadu', 75000.01, NULL, 1095, '2024-04-01', 'ADMIN', 'Above ₹75,000');

-- Insert Karnataka data
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Karnataka', 0, 15000, 0, '2024-04-01', 'ADMIN', 'Up to ₹15,000/month'),
('Karnataka', 15000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹15,000');

-- Insert Maharashtra data
INSERT INTO ProfessionalTaxConfiguration (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, CreatedBy, Notes) VALUES
('Maharashtra', 0, 7500, 0, '2024-04-01', 'ADMIN', 'Up to ₹7,500 (M)'),
('Maharashtra', 7500.01, 10000, 175, '2024-04-01', 'ADMIN', '₹7,501 – ₹10,000'),
('Maharashtra', 10000.01, NULL, 200, '2024-04-01', 'ADMIN', 'Above ₹10,000');

PRINT 'Professional Tax data inserted successfully!';
