-- ============================================================
-- OBMS Indian Standard Seed Data
-- ============================================================
USE [obms];
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '=== Seeding Indian Standard Data ==='

-- 1. Seed Branch Master
PRINT 'Seeding BranchMaster...'
SET IDENTITY_INSERT BranchMaster ON;
INSERT INTO BranchMaster (ID, Code, Name, Address1, Address2, City, State, PostCode, Phone, Fax, BankName, BankBranch, BankAccount, PersonIncharge, Email, Description, ShortName, IsHeadQuarters, UbsCode, LastUpdate, LastUpdatedBy, ParentBranch, GSTIN, PANNumber, TANNumber, IndianState, GSTRegistrationStatus, IsGSTCompliant)
VALUES
(1, 'FWG0001-HYD', 'Hyderabad Head Office', 'Plot No 34, Jubilee Hills', 'Road No 36', 'Hyderabad', 'Telangana', '500033', '040-23456789', '040-23456790', 'SBI', 'Jubilee Hills', '34567890123', 'Rajesh Kumar', 'hyderabad@example.com', 'Main South HQ', 'HYD', 1, 'UBS001', GETDATE(), 'system', NULL, '36AABCU9603R1ZX', 'AABCU9603R', 'HYDR12345A', 'Telangana', 'Active', 1),
(2, 'FWG0002-MUM', 'Mumbai Branch', 'Andheri East', 'MIDC Area', 'Mumbai', 'Maharashtra', '400093', '022-28345678', '022-28345679', 'HDFC', 'Andheri East', '501002345678', 'Amit Sharma', 'mumbai@example.com', 'West Zone', 'MUM', 0, 'UBS002', GETDATE(), 'system', 'FWG0001-HYD', '27AABCU9603R1ZM', 'AABCU9603R', 'MUMR12345B', 'Maharashtra', 'Active', 1),
(3, 'FWG0003-BLR', 'Bengaluru Branch', 'Electronic City', 'Phase 1', 'Bengaluru', 'Karnataka', '560100', '080-45678901', '080-45678902', 'ICICI', 'Electronic City', '000101567890', 'Deepa Reddy', 'bengaluru@example.com', 'IT Hub', 'BLR', 0, 'UBS003', GETDATE(), 'system', 'FWG0001-HYD', '29AABCU9603R1ZQ', 'AABCU9603R', 'BLRR12345C', 'Karnataka', 'Active', 1),
(4, 'FWG0004-DEL', 'Delhi Branch', 'Connaught Place', 'Block A', 'New Delhi', 'Delhi', '110001', '011-23456700', '011-23456701', 'Axis Bank', 'Connaught Place', '910010023456', 'Vikram Singh', 'delhi@example.com', 'North Zone', 'DEL', 0, 'UBS004', GETDATE(), 'system', 'FWG0001-HYD', '07AABCU9603R1ZP', 'AABCU9603R', 'DELR12345D', 'Delhi', 'Active', 1),
(5, 'FWG0005-CHE', 'Chennai Branch', 'T Nagar', 'North Usman Road', 'Chennai', 'Tamil Nadu', '600017', '044-24356789', '044-24356790', 'PNB', 'T Nagar', '1234000100234567', 'Karthik N', 'chennai@example.com', 'South Hub 2', 'CHE', 0, 'UBS005', GETDATE(), 'system', 'FWG0001-HYD', '33AABCU9603R1ZR', 'AABCU9603R', 'CHER12345E', 'Tamil Nadu', 'Active', 1);
SET IDENTITY_INSERT BranchMaster OFF;

-- Need OBMSBranches entry for the branches to show up for user 'system' (and other logins)
INSERT INTO OBMSBranches (BranchCode, Name, IsAllowed, LastUpdatedBy, LastUpdatedDate)
SELECT Code, 'admin', 1, 'system', GETDATE() FROM BranchMaster;

-- 2. Seed Client Master (7 compliant + 3 non-compliant)
PRINT 'Seeding ClientMaster...'
SET IDENTITY_INSERT ClientMaster ON;
INSERT INTO ClientMaster (ID, Code, Name, Address1, Address2, City, State, PostCode, Phone, Fax, Email, Branch, Status, PersonIncharge, SuperClientCode, Shortname, IsClientHeadQuarters, CreatedDate, LastUpdatedDate, LastUpdatedBy, GSTIN, PANNumber, TANNumber, CINNumber, IndianState, PINCode, IsGSTCompliant, IsPANCompliant, IsTANCompliant, GSTRegistrationStatus, ComplianceCheckDate, ComplianceRemarks)
VALUES
-- Compliant Clients
(1, 'FwgC000001', 'Tech Mahindra Ltd', 'Hitech City', 'Madhapur', 'Hyderabad', 'Telangana', '500081', '040-456789', '040-456780', 'contact@techm.com', 'FWG0001-HYD', 'Active', 'Arun Kumar', 'FWGC001', 'TechM', 1, GETDATE(), GETDATE(), 'system', '36AABCT9603R1Z1', 'AABCT9603R', 'HYDT12345A', 'L72200TG1986PLC006226', 'Telangana', '500081', 1, 1, 1, 'Active', GETDATE(), 'Fully Compliant'),
(2, 'FwgC000002', 'Infosys Ltd', 'Electronic City', 'Phase 1', 'Bengaluru', 'Karnataka', '560100', '080-28520261', '080-28520362', 'contact@infosys.com', 'FWG0003-BLR', 'Active', 'Neha Gupta', 'FWGC002', 'Infy', 1, GETDATE(), GETDATE(), 'system', '29AAACI4331Q1Z1', 'AAACI4331Q', 'BLRI12345B', 'L85110KA1981PLC013115', 'Karnataka', '560100', 1, 1, 1, 'Active', GETDATE(), 'Fully Compliant'),
(3, 'FwgC000003', 'Reliance Retail', 'Navi Mumbai', 'Kopar Khairane', 'Mumbai', 'Maharashtra', '400709', '022-44700000', '022-44710000', 'contact@relianceretail.com', 'FWG0002-MUM', 'Active', 'Mukesh A', 'FWGC003', 'Reliance', 1, GETDATE(), GETDATE(), 'system', '27AAACR0919E1Z7', 'AAACR0919E', 'MUMR12345C', 'U01100MH1999PLC120563', 'Maharashtra', '400709', 1, 1, 1, 'Active', GETDATE(), 'Fully Compliant'),
(4, 'FwgC000004', 'TCS Ltd', 'Mount Road', 'T Nagar', 'Chennai', 'Tamil Nadu', '600002', '044-66160000', '044-66160001', 'contact@tcs.com', 'FWG0005-CHE', 'Active', 'Sridhar V', 'FWGC004', 'TCS', 1, GETDATE(), GETDATE(), 'system', '33AAACT7312K1ZG', 'AAACT7312K', 'CHET12345D', 'L22210MH1995PLC084781', 'Tamil Nadu', '600002', 1, 1, 1, 'Active', GETDATE(), 'Fully Compliant'),
(5, 'FwgC000005', 'HCL Technologies', 'Sector 3', 'Noida', 'New Delhi', 'Delhi', '110065', '011-4567890', '011-4567891', 'contact@hcl.com', 'FWG0004-DEL', 'Active', 'Vikas S', 'FWGC005', 'HCL', 1, GETDATE(), GETDATE(), 'system', '07AAACH1586Q1Z9', 'AAACH1586Q', 'DELH12345E', 'L74140DL1991PLC046369', 'Delhi', '110065', 1, 1, 1, 'Active', GETDATE(), 'Fully Compliant'),
(6, 'FwgC000006', 'Wipro Ltd', 'Madhapur', 'Hitech City', 'Hyderabad', 'Telangana', '500081', '040-123456', '040-123457', 'contact@wipro.com', 'FWG0001-HYD', 'Active', 'Priya K', 'FWGC006', 'Wipro', 0, GETDATE(), GETDATE(), 'system', '36AAACW1192M1ZC', 'AAACW1192M', 'HYDW12345F', 'L32102KA1945PLC020800', 'Telangana', '500081', 1, 1, 1, 'Active', GETDATE(), 'Fully Compliant'),
(7, 'FwgC000007', 'Cognizant Tech', 'Perungudi', 'OMR', 'Chennai', 'Tamil Nadu', '600096', '044-42210000', '044-42210001', 'contact@cognizant.com', 'FWG0005-CHE', 'Active', 'Anita R', 'FWGC007', 'CTS', 0, GETDATE(), GETDATE(), 'system', '33AAACC0826E1ZM', 'AAACC0826E', 'CHEC12345G', 'U72300TN1994PTC026590', 'Tamil Nadu', '600096', 1, 1, 1, 'Active', GETDATE(), 'Fully Compliant'),
-- Non-compliant Clients
(8, 'FwgC000008', 'Alpha Industries (NC)', 'BTM Layout', 'Stage 2', 'Bengaluru', 'Karnataka', '560076', '080-112233', '080-112234', 'alpha@example.com', 'FWG0003-BLR', 'Active', 'Ramesh Rao', 'FWGC008', 'Alpha', 1, GETDATE(), GETDATE(), 'system', '29AAACA1234A1Z1', 'AAACA1234A', 'BLRA12345H', NULL, 'Karnataka', '560076', 0, 1, 0, 'Cancelled', GETDATE(), 'GST Registration Cancelled. PT Defaults.'),
(9, 'FwgC000009', 'Beta Traders (NC)', 'Chandni Chowk', 'Near Red Fort', 'New Delhi', 'Delhi', '110006', '011-998877', '011-998876', 'beta@example.com', 'FWG0004-DEL', 'Active', 'Sanjay Dutt', 'FWGC009', 'Beta', 1, GETDATE(), GETDATE(), 'system', NULL, 'AAACB2345B', 'DELB12345I', NULL, 'Delhi', '110006', 0, 1, 1, 'Unregistered', GETDATE(), 'No valid GSTIN provided. ESI mismatch found.'),
(10, 'FwgC000010', 'Gamma Services (NC)', 'Bandra West', 'Linking Road', 'Mumbai', 'Maharashtra', '400050', '022-554433', '022-554432', 'gamma@example.com', 'FWG0002-MUM', 'Active', 'Pooja Bhatt', 'FWGC010', 'Gamma', 1, GETDATE(), GETDATE(), 'system', '27AAACG3456C1Z3', 'AAACG3456C', NULL, NULL, 'Maharashtra', '400050', 1, 1, 0, 'Active', GETDATE(), 'Missing TAN Registration. Overdue PF deposits.');
SET IDENTITY_INSERT ClientMaster OFF;

-- 3. Seed Salary Structure & Shift Times
PRINT 'Seeding SalaryStructure...'
SET IDENTITY_INSERT SalaryStructure ON;
INSERT INTO SalaryStructure (SalaryId, BranchCode, EmployeeType, EmployeeNationality, Name, GeneralDayRate, GeneralDayHours, GeneralDayOTRate, OffDayRate, OffDayOTRate, HolidayRate, HolidayOTRate, WorkingDays, WorkingHours, SalaryBand, TravelAllowance, Active, Status, LastUpdatedDate, EICC, NonStructure)
VALUES
(1, 'All', 'FT', 'I', 'Basic 15K', 500.0, 8.0, 100.0, 750.0, 150.0, 1000.0, 200.0, 26, 8, 15000, 0, 'Active', 'Active', GETDATE(), 0, 0),
(2, 'All', 'FT', 'I', 'Basic 25K', 833.33, 8.0, 150.0, 1250.0, 225.0, 1666.67, 300.0, 26, 8, 25000, 1500, 'Active', 'Active', GETDATE(), 0, 0),
(3, 'All', 'FT', 'I', 'Standard 45K', 1500.0, 8.0, 250.0, 2250.0, 375.0, 3000.0, 500.0, 26, 8, 45000, 3000, 'Active', 'Active', GETDATE(), 0, 0),
(4, 'All', 'FT', 'I', 'Manager 85K', 2833.33, 8.0, 0.0, 4250.0, 0.0, 5666.67, 0.0, 26, 8, 85000, 5000, 'Active', 'Active', GETDATE(), 0, 0);
SET IDENTITY_INSERT SalaryStructure OFF;


-- 4. Seed Employees
PRINT 'Seeding Employees...'

CREATE TABLE #EmpSeedData (
    EmpID INT, EmpCode VARCHAR(20), EmpName VARCHAR(100), Branch VARCHAR(20), Client VARCHAR(20), 
    State VARCHAR(50), PAN VARCHAR(10), Aadhaar VARCHAR(12), UAN VARCHAR(20), 
    ESI VARCHAR(17), SalaryBand INT, BasicRate DECIMAL(18,2)
);

INSERT INTO #EmpSeedData VALUES 
(1, 'EMP001', 'Rahul Dravid', 'FWG0003-BLR', 'FwgC000002', 'Karnataka', 'BLRPM1111A', '987654321011', '100000000001', '31000000000000001', 4, 85000),
(2, 'EMP002', 'Sachin Tendulkar', 'FWG0002-MUM', 'FwgC000003', 'Maharashtra', 'MUMPM2222B', '987654321022', '100000000002', '31000000000000002', 4, 85000),
(3, 'EMP003', 'VVS Laxman', 'FWG0001-HYD', 'FwgC000001', 'Telangana', 'HYDPM3333C', '987654321033', '100000000003', '31000000000000003', 3, 45000),
(4, 'EMP004', 'Virender Sehwag', 'FWG0004-DEL', 'FwgC000005', 'Delhi', 'DELPM4444D', '987654321044', '100000000004', '31000000000000004', 3, 45000),
(5, 'EMP005', 'MS Dhoni', 'FWG0005-CHE', 'FwgC000004', 'Tamil Nadu', 'CHEPM5555E', '987654321055', '100000000005', '31000000000000005', 4, 85000),
(6, 'EMP006', 'Virat Kohli', 'FWG0004-DEL', 'FwgC000009', 'Delhi', 'DELPM6666F', '987654321066', '100000000006', '31000000000000006', 3, 45000),
(7, 'EMP007', 'Rohit Sharma', 'FWG0002-MUM', 'FwgC000010', 'Maharashtra', 'MUMPM7777G', '987654321077', '100000000007', '31000000000000007', 3, 45000),
(8, 'EMP008', 'Jasprit Bumrah', 'FWG0001-HYD', 'FwgC000006', 'Telangana', 'HYDPM8888H', '987654321088', '100000000008', '31000000000000008', 2, 25000),
(9, 'EMP009', 'R Ashwin', 'FWG0005-CHE', 'FwgC000007', 'Tamil Nadu', 'CHEPM9999I', '987654321099', '100000000009', '31000000000000009', 2, 25000),
(10, 'EMP010', 'KL Rahul', 'FWG0003-BLR', 'FwgC000008', 'Karnataka', 'BLRPM1010J', '987654321100', '100000000010', '31000000000000010', 2, 25000),
(11, 'EMP011', 'Hardik Pandya', 'FWG0002-MUM', 'FwgC000003', 'Maharashtra', 'MUMPM1111K', '987654321111', '100000000011', '31000000000000011', 2, 25000),
(12, 'EMP012', 'Rishabh Pant', 'FWG0004-DEL', 'FwgC000005', 'Delhi', 'DELPM1212L', '987654321122', '100000000012', '31000000000000012', 2, 25000),
(13, 'EMP013', 'Shubman Gill', 'FWG0001-HYD', 'FwgC000001', 'Telangana', 'HYDPM1313M', '987654321133', '100000000013', '31000000000000013', 1, 15000),
(14, 'EMP014', 'Mohammed Shami', 'FWG0004-DEL', 'FwgC000009', 'Delhi', 'DELPM1414N', '987654321144', '100000000014', '31000000000000014', 1, 15000),
(15, 'EMP015', 'Mohammed Siraj', 'FWG0001-HYD', 'FwgC000001', 'Telangana', 'HYDPM1515O', '987654321155', '100000000015', '31000000000000015', 1, 15000),
(16, 'EMP016', 'Suryakumar Yadav', 'FWG0002-MUM', 'FwgC000003', 'Maharashtra', 'MUMPM1616P', '987654321166', '100000000016', '31000000000000016', 1, 15000),
(17, 'EMP017', 'Shreyas Iyer', 'FWG0002-MUM', 'FwgC000010', 'Maharashtra', 'MUMPM1717Q', '987654321177', '100000000017', '31000000000000017', 1, 15000),
(18, 'EMP018', 'Ravindra Jadeja', 'FWG0005-CHE', 'FwgC000004', 'Tamil Nadu', 'CHEPM1818R', '987654321188', '100000000018', '31000000000000018', 3, 45000),
(19, 'EMP019', 'Washington Sundar', 'FWG0005-CHE', 'FwgC000007', 'Tamil Nadu', 'CHEPM1919S', '987654321199', '100000000019', '31000000000000019', 2, 25000),
(20, 'EMP020', 'Mayank Agarwal', 'FWG0003-BLR', 'FwgC000002', 'Karnataka', 'BLRPM2020T', '987654321200', '100000000020', '31000000000000020', 1, 15000),
(21, 'EMP021', 'Manish Pandey', 'FWG0003-BLR', 'FwgC000008', 'Karnataka', 'BLRPM2121U', '987654321211', '100000000021', '31000000000000021', 1, 15000),
(22, 'EMP022', 'Bhuvneshwar Kumar', 'FWG0001-HYD', 'FwgC000006', 'Telangana', 'HYDPM2222V', '987654321222', '100000000022', '31000000000000022', 2, 25000),
(23, 'EMP023', 'Ishan Kishan', 'FWG0004-DEL', 'FwgC000009', 'Delhi', 'DELPM2323W', '987654321233', '100000000023', '31000000000000023', 1, 15000),
(24, 'EMP024', 'Prithvi Shaw', 'FWG0002-MUM', 'FwgC000010', 'Maharashtra', 'MUMPM2424X', '987654321244', '100000000024', '31000000000000024', 1, 15000),
(25, 'EMP025', 'T Natarajan', 'FWG0005-CHE', 'FwgC000007', 'Tamil Nadu', 'CHEPM2525Y', '987654321255', '100000000025', '31000000000000025', 1, 15000);

-- Insert into Employee
SET IDENTITY_INSERT Employee ON;
INSERT INTO Employee (
    EMP_ID, EMP_ROLE, EMP_CODE, EMP_NAME, EMP_ADDRESS1, EMP_ADDRESS2, EMP_POST_CODE, EMP_TOWN, EMP_STATE, 
    EMP_PHONE, EMP_HGH_EDU, EM_WORK_EXP, EMP_DATE_OF_BIRTH, EMP_IC_OLD, EMP_IC_NEW, EMP_IC_COLOR, 
    EMP_PASSPORT_NO, EMP_SEX, EMP_RACE, EMP_MARTIAL_STATUS, EMP_SP_WORK, EMP_CONTACT_ADDRESS1, 
    EMP_CONTACT_ADDRESS2, EMP_CONTACT_POST_CODE, EMP_CONTACT_TOWN, EMP_CONTACT_STATE, EMP_CONTACT_TELEPHONE, 
    EMP_BRANCH_CODE, HasTransfered, LASTUPDATE, LastUpdatedBy, EMP_MOBILEPHONE, EMP_CITIZEN, EMP_CHECKLIST, 
    EMP_CLIENT, KDNVetting,
    Aadhaar, PAN, UAN_Number, ESI_Number, BankAccountNumber, IFSC_Code, BankName, BankBranch, IndianState, 
    SalaryGroup, ProfessionalTax_State, EMP_BASIC_RATE
)
SELECT 
    EmpID, 'Employee', EmpCode, EmpName, 'Address Line 1', 'Address Line 2', '110000', City, S.State,
    '9876543210', 'Degree', '5 Years', '1990-01-01', '', '', 'Blue', 
    '', 'Male', 'Indian', 'Single', 0, 'Contact Addr 1', 
    'Contact Addr 2', '110000', City, S.State, '9876543210', 
    Branch, 0, GETDATE(), 'system', '9876543210', 1, 1, 
    Client, 0,
    Aadhaar, PAN, UAN, ESI, CAST(EmpID AS VARCHAR) + '0000001111', 
    CASE WHEN Branch LIKE '%HYD' THEN 'SBIN0001234'
         WHEN Branch LIKE '%MUM' THEN 'HDFC0001234'
         WHEN Branch LIKE '%BLR' THEN 'ICIC0001234'
         WHEN Branch LIKE '%DEL' THEN 'UTIB0001234'
         ELSE 'PUNB0001234' END,
    CASE WHEN Branch LIKE '%HYD' THEN 'State Bank of India'
         WHEN Branch LIKE '%MUM' THEN 'HDFC Bank'
         WHEN Branch LIKE '%BLR' THEN 'ICICI Bank'
         WHEN Branch LIKE '%DEL' THEN 'Axis Bank'
         ELSE 'Punjab National Bank' END,
    City, S.State, CAST(SalaryBand AS VARCHAR), S.State, BasicRate
FROM #EmpSeedData S
JOIN BranchMaster B ON S.Branch = B.Code;
SET IDENTITY_INSERT Employee OFF;

-- Insert into EmploymentDetails
SET IDENTITY_INSERT EmploymentDetails ON;
INSERT INTO EmploymentDetails (
    EMPPAY_ID, EMPPAY_CODE, EMPPAY_BRANCHCODE, EMPPAY_JOB_TITLE, EMPPAY_CATEGORY, 
    EMPPAY_DATE_JOINED, EMPPAY_DATE_CONFIRM, EMPPAY_BASIC_RATE, SALARYLAB, 
    ATTENDANCEALLOWANCE, NewStructureATTENDANCEALLOWANCE, SpecialAllowance, LASTUPDATE, LastUpdatedBy
)
SELECT 
    EmpID, EmpCode, Branch, 'Developer', 'Staff', 
    '2023-01-01', '2023-07-01', BasicRate, SalaryBand, 
    0, 0, 0, GETDATE(), 'system'
FROM #EmpSeedData;
SET IDENTITY_INSERT EmploymentDetails OFF;

-- Insert into EmployeeSalaryDetails
SET IDENTITY_INSERT EmployeeSalaryDetails ON;
INSERT INTO EmployeeSalaryDetails (
    EMPFL_ID, EMPFL_CODE, EMPFL_BRANCHCODE, EMPFL_BANK, EMPFL_BK_ACCNO, 
    EMPFL_TAX_NO, EMPFL_EPFNO, EMPFL_EPF8Pa, EMPFL_SOSCO_NO, EPFDETECT, 
    PAYMODE, SOCSODETECT, TMPGUARD, DETECTBYND55, LASTUPDATE, LastUpdatedBy, INCOMETAXDETECT
)
SELECT 
    EmpID, EmpCode, Branch, 
    CASE WHEN Branch LIKE '%HYD' THEN 'SBI'
         WHEN Branch LIKE '%MUM' THEN 'HDFC'
         WHEN Branch LIKE '%BLR' THEN 'ICICI'
         WHEN Branch LIKE '%DEL' THEN 'Axis'
         ELSE 'PNB' END, 
    CAST(EmpID AS VARCHAR) + '0000001111', 
    PAN, UAN, 0, ESI, 1, 
    'Bank', 1, 0, 0, GETDATE(), 'system', 
    CASE WHEN BasicRate > 60000 THEN 1 ELSE 0 END -- TDS deduction logic
FROM #EmpSeedData;
SET IDENTITY_INSERT EmployeeSalaryDetails OFF;

DROP TABLE #EmpSeedData;

PRINT '=== Script Execution Completed ==='
