-- Insert new Professional Tax Configuration data from the provided table
-- Data parsed from the image showing various Indian states PT rates

-- Andhra Pradesh
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(1, 'Andhra Pradesh', 15000, 0, '2023-04-01', 1, GETDATE()),
(1, 'Andhra Pradesh', 20000, 150, '2023-04-01', 1, GETDATE()),
(1, 'Andhra Pradesh', 999999999, 200, '2023-04-01', 1, GETDATE());

-- Assam
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(2, 'Assam', 10000, 0, '2023-04-01', 1, GETDATE()),
(2, 'Assam', 25000, 150, '2023-04-01', 1, GETDATE()),
(2, 'Assam', 999999999, 200, '2023-04-01', 1, GETDATE());

-- Bihar
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(3, 'Bihar', 25000, 0, '2023-04-01', 1, GETDATE()),
(3, 'Bihar', 50000, 100, '2023-04-01', 1, GETDATE()),
(3, 'Bihar', 75000, 150, '2023-04-01', 1, GETDATE()),
(3, 'Bihar', 100000, 200, '2023-04-01', 1, GETDATE()),
(3, 'Bihar', 999999999, 250, '2023-04-01', 1, GETDATE());

-- Chhattisgarh
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(4, 'Chhattisgarh', 7500, 0, '2023-04-01', 1, GETDATE()),
(4, 'Chhattisgarh', 10000, 100, '2023-04-01', 1, GETDATE()),
(4, 'Chhattisgarh', 999999999, 150, '2023-04-01', 1, GETDATE());

-- Delhi
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(7, 'Delhi', 15000, 0, '2023-04-01', 1, GETDATE()),
(7, 'Delhi', 25000, 200, '2023-04-01', 1, GETDATE()),
(7, 'Delhi', 999999999, 250, '2023-04-01', 1, GETDATE());

-- Goa
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(8, 'Goa', 2500, 0, '2023-04-01', 1, GETDATE()),
(8, 'Goa', 5000, 25, '2023-04-01', 1, GETDATE()),
(8, 'Goa', 7500, 50, '2023-04-01', 1, GETDATE()),
(8, 'Goa', 10000, 75, '2023-04-01', 1, GETDATE()),
(8, 'Goa', 12500, 100, '2023-04-01', 1, GETDATE()),
(8, 'Goa', 15000, 125, '2023-04-01', 1, GETDATE()),
(8, 'Goa', 17500, 150, '2023-04-01', 1, GETDATE()),
(8, 'Goa', 20000, 175, '2023-04-01', 1, GETDATE()),
(8, 'Goa', 22500, 200, '2023-04-01', 1, GETDATE()),
(8, 'Goa', 999999999, 250, '2023-04-01', 1, GETDATE());

-- Gujarat
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(9, 'Gujarat', 5999, 0, '2023-04-01', 1, GETDATE()),
(9, 'Gujarat', 8999, 80, '2023-04-01', 1, GETDATE()),
(9, 'Gujarat', 11999, 150, '2023-04-01', 1, GETDATE()),
(9, 'Gujarat', 14999, 200, '2023-04-01', 1, GETDATE()),
(9, 'Gujarat', 17999, 250, '2023-04-01', 1, GETDATE()),
(9, 'Gujarat', 19999, 300, '2023-04-01', 1, GETDATE()),
(9, 'Gujarat', 999999999, 400, '2023-04-01', 1, GETDATE());

-- Haryana
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(10, 'Haryana', 9000, 0, '2023-04-01', 1, GETDATE()),
(10, 'Haryana', 15000, 150, '2023-04-01', 1, GETDATE()),
(10, 'Haryana', 25000, 200, '2023-04-01', 1, GETDATE()),
(10, 'Haryana', 999999999, 300, '2023-04-01', 1, GETDATE());

-- Himachal Pradesh
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(11, 'Himachal Pradesh', 12500, 0, '2023-04-01', 1, GETDATE()),
(11, 'Himachal Pradesh', 25000, 200, '2023-04-01', 1, GETDATE()),
(11, 'Himachal Pradesh', 999999999, 300, '2023-04-01', 1, GETDATE());

-- Jammu & Kashmir
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(12, 'Jammu & Kashmir', 7500, 0, '2023-04-01', 1, GETDATE()),
(12, 'Jammu & Kashmir', 10000, 100, '2023-04-01', 1, GETDATE()),
(12, 'Jammu & Kashmir', 12500, 125, '2023-04-01', 1, GETDATE()),
(12, 'Jammu & Kashmir', 15000, 150, '2023-04-01', 1, GETDATE()),
(12, 'Jammu & Kashmir', 17500, 175, '2023-04-01', 1, GETDATE()),
(12, 'Jammu & Kashmir', 20000, 200, '2023-04-01', 1, GETDATE()),
(12, 'Jammu & Kashmir', 22500, 225, '2023-04-01', 1, GETDATE()),
(12, 'Jammu & Kashmir', 25000, 250, '2023-04-01', 1, GETDATE()),
(12, 'Jammu & Kashmir', 999999999, 300, '2023-04-01', 1, GETDATE());

-- Jharkhand
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(13, 'Jharkhand', 10000, 0, '2023-04-01', 1, GETDATE()),
(13, 'Jharkhand', 15000, 100, '2023-04-01', 1, GETDATE()),
(13, 'Jharkhand', 25000, 150, '2023-04-01', 1, GETDATE()),
(13, 'Jharkhand', 999999999, 200, '2023-04-01', 1, GETDATE());

-- Karnataka
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(14, 'Karnataka', 14999, 0, '2023-04-01', 1, GETDATE()),
(14, 'Karnataka', 19999, 150, '2023-04-01', 1, GETDATE()),
(14, 'Karnataka', 999999999, 200, '2023-04-01', 1, GETDATE());

-- Kerala
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(15, 'Kerala', 11500, 0, '2023-04-01', 1, GETDATE()),
(15, 'Kerala', 17499, 80, '2023-04-01', 1, GETDATE()),
(15, 'Kerala', 20999, 120, '2023-04-01', 1, GETDATE()),
(15, 'Kerala', 999999999, 208, '2023-04-01', 1, GETDATE());

-- Madhya Pradesh
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(16, 'Madhya Pradesh', 7500, 0, '2023-04-01', 1, GETDATE()),
(16, 'Madhya Pradesh', 10000, 100, '2023-04-01', 1, GETDATE()),
(16, 'Madhya Pradesh', 999999999, 150, '2023-04-01', 1, GETDATE());

-- Maharashtra
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(17, 'Maharashtra', 7500, 0, '2023-04-01', 1, GETDATE()),
(17, 'Maharashtra', 10000, 175, '2023-04-01', 1, GETDATE()),
(17, 'Maharashtra', 999999999, 300, '2023-04-01', 1, GETDATE());

-- Meghalaya
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(18, 'Meghalaya', 16000, 0, '2023-04-01', 1, GETDATE()),
(18, 'Meghalaya', 999999999, 200, '2023-04-01', 1, GETDATE());

-- Odisha
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(19, 'Odisha', 8000, 0, '2023-04-01', 1, GETDATE()),
(19, 'Odisha', 12000, 100, '2023-04-01', 1, GETDATE()),
(19, 'Odisha', 16000, 150, '2023-04-01', 1, GETDATE()),
(19, 'Odisha', 20000, 200, '2023-04-01', 1, GETDATE()),
(19, 'Odisha', 999999999, 250, '2023-04-01', 1, GETDATE());

-- Puducherry
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(20, 'Puducherry', 7500, 0, '2023-04-01', 1, GETDATE()),
(20, 'Puducherry', 10000, 50, '2023-04-01', 1, GETDATE()),
(20, 'Puducherry', 12500, 75, '2023-04-01', 1, GETDATE()),
(20, 'Puducherry', 15000, 100, '2023-04-01', 1, GETDATE()),
(20, 'Puducherry', 17500, 125, '2023-04-01', 1, GETDATE()),
(20, 'Puducherry', 20000, 150, '2023-04-01', 1, GETDATE()),
(20, 'Puducherry', 22500, 175, '2023-04-01', 1, GETDATE()),
(20, 'Puducherry', 25000, 200, '2023-04-01', 1, GETDATE()),
(20, 'Puducherry', 999999999, 250, '2023-04-01', 1, GETDATE());

-- Punjab
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(21, 'Punjab', 12500, 0, '2023-04-01', 1, GETDATE()),
(21, 'Punjab', 999999999, 200, '2023-04-01', 1, GETDATE());

-- Rajasthan
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(22, 'Rajasthan', 7500, 0, '2023-04-01', 1, GETDATE()),
(22, 'Rajasthan', 10000, 100, '2023-04-01', 1, GETDATE()),
(22, 'Rajasthan', 15000, 150, '2023-04-01', 1, GETDATE()),
(22, 'Rajasthan', 20000, 200, '2023-04-01', 1, GETDATE()),
(22, 'Rajasthan', 999999999, 300, '2023-04-01', 1, GETDATE());

-- Sikkim
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(23, 'Sikkim', 10000, 0, '2023-04-01', 1, GETDATE()),
(23, 'Sikkim', 15000, 100, '2023-04-01', 1, GETDATE()),
(23, 'Sikkim', 20000, 200, '2023-04-01', 1, GETDATE()),
(23, 'Sikkim', 30000, 300, '2023-04-01', 1, GETDATE()),
(23, 'Sikkim', 999999999, 400, '2023-04-01', 1, GETDATE());

-- Tamil Nadu
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(24, 'Tamil Nadu', 21000, 0, '2023-04-01', 1, GETDATE()),
(24, 'Tamil Nadu', 30000, 100, '2023-04-01', 1, GETDATE()),
(24, 'Tamil Nadu', 45000, 275, '2023-04-01', 1, GETDATE()),
(24, 'Tamil Nadu', 60000, 400, '2023-04-01', 1, GETDATE()),
(24, 'Tamil Nadu', 75000, 525, '2023-04-01', 1, GETDATE()),
(24, 'Tamil Nadu', 999999999, 1095, '2023-04-01', 1, GETDATE());

-- Telangana
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(25, 'Telangana', 15000, 0, '2023-04-01', 1, GETDATE()),
(25, 'Telangana', 20000, 150, '2023-04-01', 1, GETDATE()),
(25, 'Telangana', 999999999, 200, '2023-04-01', 1, GETDATE());

-- Tripura
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(26, 'Tripura', 12000, 0, '2023-04-01', 1, GETDATE()),
(26, 'Tripura', 15000, 100, '2023-04-01', 1, GETDATE()),
(26, 'Tripura', 20000, 150, '2023-04-01', 1, GETDATE()),
(26, 'Tripura', 25000, 200, '2023-04-01', 1, GETDATE()),
(26, 'Tripura', 999999999, 300, '2023-04-01', 1, GETDATE());

-- Uttar Pradesh
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(27, 'Uttar Pradesh', 7500, 0, '2023-04-01', 1, GETDATE()),
(27, 'Uttar Pradesh', 10000, 100, '2023-04-01', 1, GETDATE()),
(27, 'Uttar Pradesh', 12500, 150, '2023-04-01', 1, GETDATE()),
(27, 'Uttar Pradesh', 15000, 200, '2023-04-01', 1, GETDATE()),
(27, 'Uttar Pradesh', 999999999, 300, '2023-04-01', 1, GETDATE());

-- Uttarakhand
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(28, 'Uttarakhand', 9000, 0, '2023-04-01', 1, GETDATE()),
(28, 'Uttarakhand', 10000, 100, '2023-04-01', 1, GETDATE()),
(28, 'Uttarakhand', 15000, 150, '2023-04-01', 1, GETDATE()),
(28, 'Uttarakhand', 999999999, 200, '2023-04-01', 1, GETDATE());

-- West Bengal
INSERT INTO ProfessionalTaxConfiguration (State_Code, State_Name, Monthly_Salary_Limit, Professional_Tax_Amount, Effective_Date, Is_Active, Created_Date) VALUES
(29, 'West Bengal', 10000, 0, '2023-04-01', 1, GETDATE()),
(29, 'West Bengal', 15000, 110, '2023-04-01', 1, GETDATE()),
(29, 'West Bengal', 20000, 130, '2023-04-01', 1, GETDATE()),
(29, 'West Bengal', 25000, 150, '2023-04-01', 1, GETDATE()),
(29, 'West Bengal', 30000, 170, '2023-04-01', 1, GETDATE()),
(29, 'West Bengal', 40000, 210, '2023-04-01', 1, GETDATE()),
(29, 'West Bengal', 50000, 250, '2023-04-01', 1, GETDATE()),
(29, 'West Bengal', 999999999, 290, '2023-04-01', 1, GETDATE());

PRINT 'New professional tax configuration data inserted successfully'
