-- =============================================================================
-- PaySlip Backfill Script
-- Purpose : Calculate and fill PTax, PF, ESI, GrossPay, LWF,
--           EarnedSalary, PerDaySalary, PFWage, ESIWage,
--           TotalDeduction, NetPay for existing PaySlip records
--           that have 0 in these new columns.
--
-- Run AFTER: PTax_PaySlip_Migration.sql  (adds the columns)
-- Run AFTER: vwPaySheet rebuild
--
-- Safe to run multiple times — only updates rows where GrossPay = 0
--
-- Author : Kiro
-- Date   : 2026-10-02
-- =============================================================================

SET NOCOUNT ON;
PRINT '=== PaySlip Backfill Starting ===';
PRINT 'Processing period: ' + CONVERT(VARCHAR, GETDATE(), 120);
GO

-- =============================================================================
-- STEP 1 : Verify all required columns exist before proceeding
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaySlip') AND name = 'PTax')
   OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaySlip') AND name = 'PF')
   OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaySlip') AND name = 'ESI')
   OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaySlip') AND name = 'GrossPay')
   OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaySlip') AND name = 'NetPay')
BEGIN
    RAISERROR('ERROR: Required columns missing. Run PTax_PaySlip_Migration.sql first!', 16, 1);
    RETURN;
END
PRINT 'Step 1: All columns exist. Proceeding...';
GO

-- =============================================================================
-- STEP 2 : Show how many rows need backfill
-- =============================================================================
SELECT 
    YEAR(p.Period) AS [Year],
    MONTH(p.Period) AS [Month],
    COUNT(*) AS TotalRows,
    SUM(CASE WHEN p.GrossPay = 0 THEN 1 ELSE 0 END) AS NeedsBackfill
FROM PaySlip p
GROUP BY YEAR(p.Period), MONTH(p.Period)
ORDER BY [Year] DESC, [Month] DESC;
PRINT 'Step 2: Row count shown above. GrossPay=0 rows need backfill.';
GO

-- =============================================================================
-- STEP 3 : Main backfill UPDATE
--
-- Calculations match SalaryProcess.cs + Crystal Report formulas exactly:
--
--   CalendarDays   = days in that salary period month
--   CBGross        = CB_Basic + CB_DA + CB_HRA + CB_OtherAllowances +
--                    CB_AdvanceStatutoryBonus + CB_Leaves + CB_NH
--   PerDaySalary   = CBGross / CalendarDays
--   EarnedSalary   = BasicSalaryDays × PerDaySalary
--
--   GrossPay       = CBGross (full month, not prorated)
--   MonthLeave     = GrossPay - EarnedSalary
--
--   PFWage         = (CB_Basic + CB_DA + CB_OtherAllowances) / CalendarDays
--                    × BasicSalaryDays  → capped at PF BasicSalaryLimit
--   PF             = ROUND(PFWage × EmployeeRate / 100, 0)
--                    only if EPFDETECT=1 AND (compliance_client OR Staff)
--
--   ESIWage        = EarnedSalary - CB_AdvanceStatutoryBonus
--   ESI            = ROUND(ESIWage × EmployeeRate / 100, 0)
--                    only if ESIWage < BasicSalaryLimit AND SOCSODETECT=1
--                    AND (compliance_client OR Staff)
--
--   PTax           = from ProfessionalTaxConfiguration via slab match
--                    only if INCOMETAXDETECT=1
--
--   LWF            = 30 if December, else 0
--
--   TotalDeduction = PF + ESI + DailyAdvanceRecovery + MonthlyAdvanceRecovery
--                  + UniformIssueRecovery + LoanRecovery + MiscDeduction
--                  + MonthLeave + PTax + LWF
--
--   NetPay         = GrossPay - TotalDeduction
-- =============================================================================

UPDATE p
SET
    -- ── PerDaySalary ──────────────────────────────────────────────────────────
    p.PerDaySalary = ROUND(
        (
            ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_HRA, 0)
          + ISNULL(e.CB_OtherAllowances, 0) + ISNULL(e.CB_AdvanceStatutoryBonus, 0)
          + ISNULL(e.CB_Leaves, 0) + ISNULL(e.CB_NH, 0)
        )
        / NULLIF(DAY(EOMONTH(p.Period)), 0),
        4),

    -- ── EarnedSalary ──────────────────────────────────────────────────────────
    p.EarnedSalary = ROUND(
        ISNULL(p.BasicSalaryDays, 0)
        * (
            (
                ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_HRA, 0)
              + ISNULL(e.CB_OtherAllowances, 0) + ISNULL(e.CB_AdvanceStatutoryBonus, 0)
              + ISNULL(e.CB_Leaves, 0) + ISNULL(e.CB_NH, 0)
            )
            / NULLIF(DAY(EOMONTH(p.Period)), 0)
          ),
        2),

    -- ── GrossPay ──────────────────────────────────────────────────────────────
    p.GrossPay = ROUND(
        ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_HRA, 0)
      + ISNULL(e.CB_OtherAllowances, 0) + ISNULL(e.CB_AdvanceStatutoryBonus, 0)
      + ISNULL(e.CB_Leaves, 0) + ISNULL(e.CB_NH, 0),
        2),

    -- ── LWF ───────────────────────────────────────────────────────────────────
    p.LWF = CASE WHEN MONTH(p.Period) = 12 THEN 30.00 ELSE 0.00 END,

    -- ── PFWage + PF ───────────────────────────────────────────────────────────
    -- Eligibility: EPFDETECT=1 AND (compliance_client OR Staff)
    p.PFWage = CASE
        WHEN ISNULL(esd.EPFDETECT, 0) = 1
         AND (
             ISNULL(cm.ClientComplianceStatus, '') = 'compliance_client'
          OR ISNULL(e.EMP_ROLE, '') = 'Staff'
             )
        THEN
            -- Prorated PF wage (Basic + DA + OtherAllowances only)
            ROUND(
                CASE
                    WHEN ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_OtherAllowances, 0))
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                         2)
                         > pf.BasicSalaryLimit
                    THEN pf.BasicSalaryLimit
                    ELSE ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_OtherAllowances, 0))
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                         2)
                END,
            2)
        ELSE 0
    END,

    p.PF = CASE
        WHEN ISNULL(esd.EPFDETECT, 0) = 1
         AND (
             ISNULL(cm.ClientComplianceStatus, '') = 'compliance_client'
          OR ISNULL(e.EMP_ROLE, '') = 'Staff'
             )
        THEN
            ROUND(
                CASE
                    WHEN ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_OtherAllowances, 0))
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                         2)
                         > pf.BasicSalaryLimit
                    THEN pf.BasicSalaryLimit
                    ELSE ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_OtherAllowances, 0))
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                         2)
                END
                * pf.Employee_Rate / 100.0,
            0)
        ELSE 0
    END,

    -- ── ESIWage + ESI ─────────────────────────────────────────────────────────
    -- ESIWage = EarnedSalary - CB_AdvanceStatutoryBonus
    -- Eligibility: SOCSODETECT=1 AND ESIWage < BasicSalaryLimit AND (compliance_client OR Staff)
    p.ESIWage = CASE
        WHEN ISNULL(esd.SOCSODETECT, 0) = 1
         AND (
             ISNULL(cm.ClientComplianceStatus, '') = 'compliance_client'
          OR ISNULL(e.EMP_ROLE, '') = 'Staff'
             )
        THEN
            ROUND(
                CASE
                    WHEN ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (
                                ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_HRA, 0)
                              + ISNULL(e.CB_OtherAllowances, 0) + ISNULL(e.CB_AdvanceStatutoryBonus, 0)
                              + ISNULL(e.CB_Leaves, 0) + ISNULL(e.CB_NH, 0)
                            )
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                        2) - ISNULL(e.CB_AdvanceStatutoryBonus, 0) < 0
                    THEN 0
                    ELSE ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (
                                ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_HRA, 0)
                              + ISNULL(e.CB_OtherAllowances, 0) + ISNULL(e.CB_AdvanceStatutoryBonus, 0)
                              + ISNULL(e.CB_Leaves, 0) + ISNULL(e.CB_NH, 0)
                            )
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                        2) - ISNULL(e.CB_AdvanceStatutoryBonus, 0)
                END,
            2)
        ELSE 0
    END,

    p.ESI = CASE
        WHEN ISNULL(esd.SOCSODETECT, 0) = 1
         AND (
             ISNULL(cm.ClientComplianceStatus, '') = 'compliance_client'
          OR ISNULL(e.EMP_ROLE, '') = 'Staff'
             )
         AND (
                CASE
                    WHEN ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (
                                ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_HRA, 0)
                              + ISNULL(e.CB_OtherAllowances, 0) + ISNULL(e.CB_AdvanceStatutoryBonus, 0)
                              + ISNULL(e.CB_Leaves, 0) + ISNULL(e.CB_NH, 0)
                            )
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                        2) - ISNULL(e.CB_AdvanceStatutoryBonus, 0) < 0
                    THEN 0
                    ELSE ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (
                                ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_HRA, 0)
                              + ISNULL(e.CB_OtherAllowances, 0) + ISNULL(e.CB_AdvanceStatutoryBonus, 0)
                              + ISNULL(e.CB_Leaves, 0) + ISNULL(e.CB_NH, 0)
                            )
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                        2) - ISNULL(e.CB_AdvanceStatutoryBonus, 0)
                END
             ) < esi.BasicSalaryLimit
        THEN
            ROUND(
                CASE
                    WHEN ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (
                                ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_HRA, 0)
                              + ISNULL(e.CB_OtherAllowances, 0) + ISNULL(e.CB_AdvanceStatutoryBonus, 0)
                              + ISNULL(e.CB_Leaves, 0) + ISNULL(e.CB_NH, 0)
                            )
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                        2) - ISNULL(e.CB_AdvanceStatutoryBonus, 0) < 0
                    THEN 0
                    ELSE ROUND(
                            ISNULL(p.BasicSalaryDays, 0)
                            * (
                                ISNULL(e.CB_Basic, 0) + ISNULL(e.CB_DA, 0) + ISNULL(e.CB_HRA, 0)
                              + ISNULL(e.CB_OtherAllowances, 0) + ISNULL(e.CB_AdvanceStatutoryBonus, 0)
                              + ISNULL(e.CB_Leaves, 0) + ISNULL(e.CB_NH, 0)
                            )
                            / NULLIF(DAY(EOMONTH(p.Period)), 0),
                        2) - ISNULL(e.CB_AdvanceStatutoryBonus, 0)
                END
                * esi.Employee_Rate / 100.0,
            0)
        ELSE 0
    END,

    -- ── PTax ──────────────────────────────────────────────────────────────────
    -- Match slab from ProfessionalTaxConfiguration
    -- For SemiAnnual states (Tamil Nadu): match EarnedSalary × 6, then ÷ 6
    -- For Monthly states: match EarnedSalary directly
    p.PTax = CASE
        WHEN ISNULL(esd.INCOMETAXDETECT, 0) = 1
         AND ISNULL(e.IndianState, '') <> ''
        THEN
            ROUND(
                ISNULL(
                    (
                        SELECT TOP 1
                            CASE
                                WHEN ptc.TaxPeriod = 'SemiAnnual' THEN ptc.TaxAmount / 6.0
                                WHEN ptc.TaxPeriod = 'Annual'     THEN ptc.TaxAmount / 12.0
                                ELSE ptc.TaxAmount
                            END
                        FROM ProfessionalTaxConfiguration ptc
                        WHERE ptc.IsActive = 1
                          AND LOWER(ptc.State) = LOWER(ISNULL(e.IndianState, ''))
                          AND ptc.EffectiveDate <= p.Period
                          AND (
                              CASE
                                  WHEN ptc.TaxPeriod = 'SemiAnnual'
                                  THEN ROUND(
                                          ISNULL(p.BasicSalaryDays, 0)
                                          * (
                                              ISNULL(e.CB_Basic,0)+ISNULL(e.CB_DA,0)+ISNULL(e.CB_HRA,0)
                                             +ISNULL(e.CB_OtherAllowances,0)+ISNULL(e.CB_AdvanceStatutoryBonus,0)
                                             +ISNULL(e.CB_Leaves,0)+ISNULL(e.CB_NH,0)
                                          )
                                          / NULLIF(DAY(EOMONTH(p.Period)),0),
                                       2) * 6
                                  WHEN ptc.TaxPeriod = 'Annual'
                                  THEN ROUND(
                                          ISNULL(p.BasicSalaryDays, 0)
                                          * (
                                              ISNULL(e.CB_Basic,0)+ISNULL(e.CB_DA,0)+ISNULL(e.CB_HRA,0)
                                             +ISNULL(e.CB_OtherAllowances,0)+ISNULL(e.CB_AdvanceStatutoryBonus,0)
                                             +ISNULL(e.CB_Leaves,0)+ISNULL(e.CB_NH,0)
                                          )
                                          / NULLIF(DAY(EOMONTH(p.Period)),0),
                                       2) * 12
                                  ELSE ROUND(
                                          ISNULL(p.BasicSalaryDays, 0)
                                          * (
                                              ISNULL(e.CB_Basic,0)+ISNULL(e.CB_DA,0)+ISNULL(e.CB_HRA,0)
                                             +ISNULL(e.CB_OtherAllowances,0)+ISNULL(e.CB_AdvanceStatutoryBonus,0)
                                             +ISNULL(e.CB_Leaves,0)+ISNULL(e.CB_NH,0)
                                          )
                                          / NULLIF(DAY(EOMONTH(p.Period)),0),
                                       2)
                              END
                          ) >= ptc.MinSalary
                          AND (
                              ptc.MaxSalary IS NULL
                              OR (
                                  CASE
                                      WHEN ptc.TaxPeriod = 'SemiAnnual'
                                      THEN ROUND(
                                              ISNULL(p.BasicSalaryDays,0)
                                              * (
                                                  ISNULL(e.CB_Basic,0)+ISNULL(e.CB_DA,0)+ISNULL(e.CB_HRA,0)
                                                 +ISNULL(e.CB_OtherAllowances,0)+ISNULL(e.CB_AdvanceStatutoryBonus,0)
                                                 +ISNULL(e.CB_Leaves,0)+ISNULL(e.CB_NH,0)
                                              )
                                              / NULLIF(DAY(EOMONTH(p.Period)),0),
                                           2) * 6
                                      WHEN ptc.TaxPeriod = 'Annual'
                                      THEN ROUND(
                                              ISNULL(p.BasicSalaryDays,0)
                                              * (
                                                  ISNULL(e.CB_Basic,0)+ISNULL(e.CB_DA,0)+ISNULL(e.CB_HRA,0)
                                                 +ISNULL(e.CB_OtherAllowances,0)+ISNULL(e.CB_AdvanceStatutoryBonus,0)
                                                 +ISNULL(e.CB_Leaves,0)+ISNULL(e.CB_NH,0)
                                              )
                                              / NULLIF(DAY(EOMONTH(p.Period)),0),
                                           2) * 12
                                      ELSE ROUND(
                                              ISNULL(p.BasicSalaryDays,0)
                                              * (
                                                  ISNULL(e.CB_Basic,0)+ISNULL(e.CB_DA,0)+ISNULL(e.CB_HRA,0)
                                                 +ISNULL(e.CB_OtherAllowances,0)+ISNULL(e.CB_AdvanceStatutoryBonus,0)
                                                 +ISNULL(e.CB_Leaves,0)+ISNULL(e.CB_NH,0)
                                              )
                                              / NULLIF(DAY(EOMONTH(p.Period)),0),
                                           2)
                                  END
                              ) <= ptc.MaxSalary
                          )
                        ORDER BY ptc.EffectiveDate DESC
                    ),
                0),
            0)
        ELSE 0
    END

FROM PaySlip p
INNER JOIN Employee e
    ON e.EMP_ID = p.EmployeeID
INNER JOIN EmployeeSalaryDetails esd
    ON esd.EMPFL_CODE = e.EMP_CODE
LEFT JOIN ClientMaster cm
    ON cm.Code = e.EMP_CLIENT
-- Get active PF config for the period
CROSS APPLY (
    SELECT TOP 1
        pfc.Employee_Rate,
        pfc.Basic_Salary_Limit AS BasicSalaryLimit
    FROM PFConfiguration pfc
    WHERE pfc.Is_Active = 1
      AND pfc.Effective_Date <= p.Period
    ORDER BY pfc.Effective_Date DESC
) pf
-- Get active ESI config for the period
CROSS APPLY (
    SELECT TOP 1
        ec.Employee_Rate,
        ec.Basic_Salary_Limit AS BasicSalaryLimit
    FROM ESIConfiguration ec
    WHERE ec.Is_Active = 1
      AND ec.Effective_Date <= p.Period
    ORDER BY ec.Effective_Date DESC
) esi
-- Only update rows where GrossPay = 0 (not yet backfilled)
WHERE p.GrossPay = 0;

PRINT 'Step 3: PTax/PF/ESI/GrossPay/LWF/EarnedSalary/PerDaySalary updated.';
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' rows updated.';
GO

-- =============================================================================
-- STEP 4 : Update TotalDeduction + NetPay
--          (Run after Step 3 so all component values are filled)
-- =============================================================================
UPDATE p
SET
    p.TotalDeduction = ROUND(
        ISNULL(p.PF, 0)
      + ISNULL(p.ESI, 0)
      + ISNULL(p.DailyAdvanceRecovery, 0)
      + ISNULL(p.MonthlyAdvanceRecovery, 0)
      + ISNULL(p.UniformIssueRecovery, 0)
      + ISNULL(p.LoanRecovery, 0)
      + ISNULL(p.MiscDeduction, 0)
      + ISNULL(p.GrossPay - p.EarnedSalary, 0)  -- MonthLeave = GrossPay - EarnedSalary
      + ISNULL(p.PTax, 0)
      + ISNULL(p.LWF, 0),
    2),

    p.NetPay = ROUND(
        ISNULL(p.GrossPay, 0)
        - ROUND(
            ISNULL(p.PF, 0)
          + ISNULL(p.ESI, 0)
          + ISNULL(p.DailyAdvanceRecovery, 0)
          + ISNULL(p.MonthlyAdvanceRecovery, 0)
          + ISNULL(p.UniformIssueRecovery, 0)
          + ISNULL(p.LoanRecovery, 0)
          + ISNULL(p.MiscDeduction, 0)
          + ISNULL(p.GrossPay - p.EarnedSalary, 0)
          + ISNULL(p.PTax, 0)
          + ISNULL(p.LWF, 0),
          2),
    2)

FROM PaySlip p
WHERE p.TotalDeduction = 0
  AND p.GrossPay > 0;  -- only rows that were updated in Step 3

PRINT 'Step 4: TotalDeduction + NetPay updated.';
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' rows updated.';
GO

-- =============================================================================
-- STEP 5 : Verify results — spot check top 10 rows
-- =============================================================================
SELECT TOP 10
    e.EMP_NAME,
    e.IndianState,
    CONVERT(VARCHAR(7), p.Period, 120) AS Period,
    p.BasicSalaryDays,
    p.GrossPay,
    p.EarnedSalary,
    p.GrossPay - p.EarnedSalary AS MonthLeave,
    p.PTax,
    p.PF,
    p.ESI,
    p.LWF,
    p.TotalDeduction,
    p.NetPay
FROM PaySlip p
INNER JOIN Employee e ON e.EMP_ID = p.EmployeeID
ORDER BY p.Period DESC, e.EMP_NAME;

PRINT '=== Backfill Complete. Verify results above. ===';
GO
