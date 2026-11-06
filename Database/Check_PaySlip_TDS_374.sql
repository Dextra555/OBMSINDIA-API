-- =====================================================
-- CHECK: PaySlip table — Employee 374 TDS saved?
-- Run this in SSMS against [obmsindia_new]
-- =====================================================

USE obmsindia_new;
GO

-- 1. PaySlip table rows for Employee 374
SELECT 
    p.ID                                AS PaySlipID,
    FORMAT(p.Period, 'MMM-yyyy')        AS Period,
    p.EmployeeID,
    p.BasicSalary,
    p.GrossPay,
    p.PF,
    p.ESI,
    p.PTax                              AS ProfessionalTax,
    p.TDS                               AS TDS_Saved,       -- ← THIS IS WHAT WE CHECK
    p.TotalDeduction,
    p.NetPay,
    FORMAT(p.LastUpdate,'dd-MMM-yyyy')  AS LastUpdated
FROM PaySlip p
WHERE p.EmployeeID = 374
ORDER BY p.Period DESC;

-- -------------------------------------------------------
-- EXPECTED:
--   Sep-2026 → TDS = 72474.70  (or 50634.70 if gross was 373005)
--   Oct-2026 → TDS = 72474.70
--
-- If TDS = 0.00 → Salary was processed BEFORE Phase 2 fix.
--   Solution: Re-process salary for Sep & Oct 2026.
-- -------------------------------------------------------

-- 2. Quick summary
SELECT 
    FORMAT(p.Period,'MMM-yyyy') AS Period,
    p.GrossPay,
    p.TDS,
    CASE 
        WHEN p.TDS > 0 THEN 'TDS SAVED ✓' 
        ELSE 'TDS = 0 — Need Re-process' 
    END AS Status
FROM PaySlip p
WHERE p.EmployeeID = 374
ORDER BY p.Period DESC;

-- 3. SalaryProcess lock status
SELECT 
    sp.ID,
    FORMAT(sp.Period,'MMM-yyyy') AS Period,
    sp.Branch,
    sp.EmployeeType,
    sp.IsLocked,
    sp.LastUpdatedBy,
    FORMAT(sp.LastUpdate,'dd-MMM-yyyy hh:mm tt') AS LastUpdate
FROM SalaryProcess sp
ORDER BY sp.Period DESC;
