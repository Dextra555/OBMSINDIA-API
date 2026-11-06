-- =====================================================
-- FIX: Re-calculate TDS for Employee 374 (FWGS000376)
--      Sep-2026 and Oct-2026 PaySlip rows
--
-- IMPORTANT: Review before running.
--            Run in SSMS against [obmsindia_new].
--
-- Why:
--   Sep-2026: TDS = 0 (processed before Phase 2 fix)
--   Oct-2026: TDS = 31215.38 (wrong — Sep YTD was 0)
--
-- Correct values (Gross 4,43,005 | Joining Sep-2026 | FY 2026-27):
--   Annual = 4,43,005 × 7  = 31,01,035
--   Std Ded              =     75,000
--   Taxable              = 30,26,035
--   Slab Tax + 4% Cess   =  5,07,322.92
--   ÷ 7 months           =     72,474.70  per month
--
-- Sep-2026: YTD = 0         → Monthly TDS = 5,07,322.92 ÷ 7 = 72,474.70
-- Oct-2026: YTD = 72,474.70 → Balance = 4,34,848.22 ÷ 6   = 72,474.70
-- =====================================================

USE obmsindia_new;
GO

BEGIN TRANSACTION;

-- ── Sep 2026 ──────────────────────────────────────────
-- Gross was 3,73,000 in old row; correct gross is 4,43,005
-- TDS for Sep (first month, remaining 7 months) = 72,474.70

UPDATE PaySlip
SET 
    BasicSalary    = 395000.00,
    GrossPay       = 443005.00,
    TDS            = 72474.70,
    TotalDeduction = 72474.70,   -- PF=0, ESI=0, PTax=0 for this employee
    NetPay         = 443005.00 - 72474.70,   -- = 370530.30
    LastUpdate     = GETDATE(),
    LastUpdatedBy  = 'TDS_FIX_PHASE2'
WHERE ID = 22733  -- Sep-2026 PaySlip row
  AND EmployeeID = 374;

PRINT 'Sep-2026 PaySlip updated. Rows: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- ── Oct 2026 ──────────────────────────────────────────
-- YTD after Sep = 72,474.70
-- Remaining months (Oct→Mar) = 6
-- Balance = 5,07,322.92 - 72,474.70 = 4,34,848.22
-- Monthly TDS = 4,34,848.22 / 6 = 72,474.70

UPDATE PaySlip
SET 
    TDS            = 72474.70,
    TotalDeduction = 72474.70,   -- PF=0, ESI=0, PTax=0
    NetPay         = 443005.00 - 72474.70,   -- = 370530.30
    LastUpdate     = GETDATE(),
    LastUpdatedBy  = 'TDS_FIX_PHASE2'
WHERE ID = 22752  -- Oct-2026 PaySlip row
  AND EmployeeID = 374;

PRINT 'Oct-2026 PaySlip updated. Rows: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- ── Verify ────────────────────────────────────────────
SELECT 
    p.ID,
    FORMAT(p.Period,'MMM-yyyy') AS Period,
    p.GrossPay,
    p.TDS,
    p.TotalDeduction,
    p.NetPay,
    CASE WHEN p.TDS = 72474.70 THEN '✓ CORRECT' ELSE '✗ WRONG' END AS Status
FROM PaySlip p
WHERE p.EmployeeID = 374
ORDER BY p.Period;

-- ── If result looks correct → COMMIT ─────────────────
-- COMMIT TRANSACTION;

-- ── If anything wrong → ROLLBACK ─────────────────────
ROLLBACK TRANSACTION;

-- After verifying above SELECT result:
-- 1. If correct  → Remove ROLLBACK, uncomment COMMIT, run again
-- 2. If wrong    → Just rollback (already done above)
