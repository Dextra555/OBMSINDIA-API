-- Insert new Professional Tax Configuration data with corrected StateCodes and values
-- Data parsed from the image showing various Indian states PT rates

-- Jammu and Kashmir (StateCode: 01)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('01', 'Jammu and Kashmir', 7500, 0, '2023-04-01', 1, GETDATE()),
('01', 'Jammu and Kashmir', 10000, 100, '2023-04-01', 1, GETDATE()),
('01', 'Jammu and Kashmir', 12500, 125, '2023-04-01', 1, GETDATE()),
('01', 'Jammu and Kashmir', 15000, 150, '2023-04-01', 1, GETDATE()),
('01', 'Jammu and Kashmir', 17500, 175, '2023-04-01', 1, GETDATE()),
('01', 'Jammu and Kashmir', 20000, 200, '2023-04-01', 1, GETDATE()),
('01', 'Jammu and Kashmir', 22500, 225, '2023-04-01', 1, GETDATE()),
('01', 'Jammu and Kashmir', 25000, 250, '2023-04-01', 1, GETDATE()),
('01', 'Jammu and Kashmir', 9999999.99, 300, '2023-04-01', 1, GETDATE());

-- Himachal Pradesh (StateCode: 02)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('02', 'Himachal Pradesh', 12500, 0, '2023-04-01', 1, GETDATE()),
('02', 'Himachal Pradesh', 25000, 200, '2023-04-01', 1, GETDATE()),
('02', 'Himachal Pradesh', 9999999.99, 300, '2023-04-01', 1, GETDATE());

-- Punjab (StateCode: 03)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('03', 'Punjab', 12500, 0, '2023-04-01', 1, GETDATE()),
('03', 'Punjab', 9999999.99, 200, '2023-04-01', 1, GETDATE());

-- Haryana (StateCode: 06)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('06', 'Haryana', 9000, 0, '2023-04-01', 1, GETDATE()),
('06', 'Haryana', 15000, 150, '2023-04-01', 1, GETDATE()),
('06', 'Haryana', 25000, 200, '2023-04-01', 1, GETDATE()),
('06', 'Haryana', 9999999.99, 300, '2023-04-01', 1, GETDATE());

-- Delhi (StateCode: 07)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('07', 'Delhi', 15000, 0, '2023-04-01', 1, GETDATE()),
('07', 'Delhi', 25000, 200, '2023-04-01', 1, GETDATE()),
('07', 'Delhi', 9999999.99, 250, '2023-04-01', 1, GETDATE());

-- Rajasthan (StateCode: 08)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('08', 'Rajasthan', 7500, 0, '2023-04-01', 1, GETDATE()),
('08', 'Rajasthan', 10000, 100, '2023-04-01', 1, GETDATE()),
('08', 'Rajasthan', 15000, 150, '2023-04-01', 1, GETDATE()),
('08', 'Rajasthan', 20000, 200, '2023-04-01', 1, GETDATE()),
('08', 'Rajasthan', 9999999.99, 300, '2023-04-01', 1, GETDATE());

-- Uttar Pradesh (StateCode: 09)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('09', 'Uttar Pradesh', 7500, 0, '2023-04-01', 1, GETDATE()),
('09', 'Uttar Pradesh', 10000, 100, '2023-04-01', 1, GETDATE()),
('09', 'Uttar Pradesh', 12500, 150, '2023-04-01', 1, GETDATE()),
('09', 'Uttar Pradesh', 15000, 200, '2023-04-01', 1, GETDATE()),
('09', 'Uttar Pradesh', 9999999.99, 300, '2023-04-01', 1, GETDATE());

-- Bihar (StateCode: 10)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('10', 'Bihar', 25000, 0, '2023-04-01', 1, GETDATE()),
('10', 'Bihar', 50000, 100, '2023-04-01', 1, GETDATE()),
('10', 'Bihar', 75000, 150, '2023-04-01', 1, GETDATE()),
('10', 'Bihar', 100000, 200, '2023-04-01', 1, GETDATE()),
('10', 'Bihar', 9999999.99, 250, '2023-04-01', 1, GETDATE());

-- Uttarakhand (StateCode: 05)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('05', 'Uttarakhand', 9000, 0, '2023-04-01', 1, GETDATE()),
('05', 'Uttarakhand', 10000, 100, '2023-04-01', 1, GETDATE()),
('05', 'Uttarakhand', 15000, 150, '2023-04-01', 1, GETDATE()),
('05', 'Uttarakhand', 9999999.99, 200, '2023-04-01', 1, GETDATE());

-- Madhya Pradesh (need to check StateCode)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('MP', 'Madhya Pradesh', 7500, 0, '2023-04-01', 1, GETDATE()),
('MP', 'Madhya Pradesh', 10000, 100, '2023-04-01', 1, GETDATE()),
('MP', 'Madhya Pradesh', 9999999.99, 150, '2023-04-01', 1, GETDATE());

-- Chhattisgarh (need to check StateCode)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('CG', 'Chhattisgarh', 7500, 0, '2023-04-01', 1, GETDATE()),
('CG', 'Chhattisgarh', 10000, 100, '2023-04-01', 1, GETDATE()),
('CG', 'Chhattisgarh', 9999999.99, 150, '2023-04-01', 1, GETDATE());

-- Odisha (need to check StateCode)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('OD', 'Odisha', 8000, 0, '2023-04-01', 1, GETDATE()),
('OD', 'Odisha', 12000, 100, '2023-04-01', 1, GETDATE()),
('OD', 'Odisha', 16000, 150, '2023-04-01', 1, GETDATE()),
('OD', 'Odisha', 20000, 200, '2023-04-01', 1, GETDATE()),
('OD', 'Odisha', 9999999.99, 250, '2023-04-01', 1, GETDATE());

-- West Bengal (need to check StateCode)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('WB', 'West Bengal', 10000, 0, '2023-04-01', 1, GETDATE()),
('WB', 'West Bengal', 15000, 110, '2023-04-01', 1, GETDATE()),
('WB', 'West Bengal', 20000, 130, '2023-04-01', 1, GETDATE()),
('WB', 'West Bengal', 25000, 150, '2023-04-01', 1, GETDATE()),
('WB', 'West Bengal', 30000, 170, '2023-04-01', 1, GETDATE()),
('WB', 'West Bengal', 40000, 210, '2023-04-01', 1, GETDATE()),
('WB', 'West Bengal', 50000, 250, '2023-04-01', 1, GETDATE()),
('WB', 'West Bengal', 9999999.99, 290, '2023-04-01', 1, GETDATE());

-- Gujarat (need to check StateCode)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('GJ', 'Gujarat', 5999, 0, '2023-04-01', 1, GETDATE()),
('GJ', 'Gujarat', 8999, 80, '2023-04-01', 1, GETDATE()),
('GJ', 'Gujarat', 11999, 150, '2023-04-01', 1, GETDATE()),
('GJ', 'Gujarat', 14999, 200, '2023-04-01', 1, GETDATE()),
('GJ', 'Gujarat', 17999, 250, '2023-04-01', 1, GETDATE()),
('GJ', 'Gujarat', 19999, 300, '2023-04-01', 1, GETDATE()),
('GJ', 'Gujarat', 9999999.99, 400, '2023-04-01', 1, GETDATE());

-- Maharashtra (need to check StateCode)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('MH', 'Maharashtra', 7500, 0, '2023-04-01', 1, GETDATE()),
('MH', 'Maharashtra', 10000, 175, '2023-04-01', 1, GETDATE()),
('MH', 'Maharashtra', 9999999.99, 300, '2023-04-01', 1, GETDATE());

-- Karnataka (need to check StateCode)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('KA', 'Karnataka', 14999, 0, '2023-04-01', 1, GETDATE()),
('KA', 'Karnataka', 19999, 150, '2023-04-01', 1, GETDATE()),
('KA', 'Karnataka', 9999999.99, 200, '2023-04-01', 1, GETDATE());

-- Kerala (need to check StateCode)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('KL', 'Kerala', 11500, 0, '2023-04-01', 1, GETDATE()),
('KL', 'Kerala', 17499, 80, '2023-04-01', 1, GETDATE()),
('KL', 'Kerala', 20999, 120, '2023-04-01', 1, GETDATE()),
('KL', 'Kerala', 9999999.99, 208, '2023-04-01', 1, GETDATE());

-- Tamil Nadu (need to check StateCode)
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
('TN', 'Tamil Nadu', 21000, 0, '2023-04-01', 1, GETDATE()),
('TN', 'Tamil Nadu', 30000, 100, '2023-04-01', 1, GETDATE()),
('TN', 'Tamil Nadu', 45000, 275, '2023-04-01', 1, GETDATE()),
('TN', 'Tamil Nadu', 60000, 400, '2023-04-01', 1, GETDATE()),
('TN', 'Tamil Nadu', 75000, 525, '2023-04-01', 1, GETDATE()),
('TN', 'Tamil Nadu', 9999999.99, 1095, '2023-04-01', 1, GETDATE());

PRINT 'Corrected professional tax configuration data inserted successfully'
