-- ============================================================
-- Update Existing Designation Records with DepartmentId
-- ============================================================

USE [obms];
GO

PRINT '=== Updating Designation records with DepartmentId ==='

-- Update Designation records to link to HR department (DepartmentId = 1)
UPDATE Designation
SET DepartmentId = 1
WHERE DepartmentId IS NULL;

PRINT 'Designation records updated with DepartmentId = 1';

-- Verify the update
SELECT 
    d.DesignationId,
    d.Desig_Code,
    d.Desig_Name,
    d.DepartmentId,
    dept.Dept_Name AS DepartmentName
FROM Designation d
LEFT JOIN Department dept ON d.DepartmentId = dept.DepartmentId;

PRINT '=== Verification complete ==='
