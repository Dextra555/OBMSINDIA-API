-- =============================================================================
-- ESIConfiguration - CalculationBasis Column Migration
-- Purpose : ESI calculation-ku Gross Salary or Basic+DA basis select panna
--           option add pannurom.
-- Formula : Basic+DA selected-na → ESI = (Basic + DA) × 0.75 / 100
--           Gross selected-na    → ESI = Gross × 0.75 / 100  (existing)
-- Run on  : Live DB before deploying backend/frontend changes
-- =============================================================================

-- Step 1: CalculationBasis column add pannurom (default = 'Gross' — backward compat)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ESIConfiguration'
      AND COLUMN_NAME = 'Calculation_Basis'
)
BEGIN
    ALTER TABLE ESIConfiguration
        ADD Calculation_Basis NVARCHAR(10) NOT NULL
            CONSTRAINT DF_ESIConfiguration_CalculationBasis DEFAULT 'Gross';

    PRINT 'Column Calculation_Basis added successfully.';
END
ELSE
BEGIN
    PRINT 'Column Calculation_Basis already exists — skipping.';
END
GO

-- Step 2: Check constraint — only 'Gross' or 'BasicDA' allowed
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_NAME = 'ESIConfiguration'
      AND CONSTRAINT_NAME = 'CK_ESIConfiguration_CalculationBasis'
)
BEGIN
    ALTER TABLE ESIConfiguration
        ADD CONSTRAINT CK_ESIConfiguration_CalculationBasis
            CHECK (Calculation_Basis IN ('Gross', 'BasicDA'));

    PRINT 'Check constraint CK_ESIConfiguration_CalculationBasis added.';
END
GO

-- Step 3: Existing rows = 'Gross' (already defaulted above, but explicit update)
UPDATE ESIConfiguration
   SET Calculation_Basis = 'Gross'
 WHERE Calculation_Basis IS NULL OR Calculation_Basis = '';
GO

-- Step 4: Verify
SELECT Id,
       Gross_Salary_Limit,
       Employee_Rate,
       Employer_Rate,
       Effective_Date,
       Is_Active,
       Financial_Year,
       Calculation_Basis
  FROM ESIConfiguration
 ORDER BY Effective_Date DESC;
GO

-- =============================================================================
-- Values:
--   'Gross'   → ESI Wage = EarnedSalary − CB_AdvanceStatutoryBonus  (current)
--   'BasicDA' → ESI Wage = CB_Basic + CB_DA
-- Both compare against Gross_Salary_Limit (₹21,000) before applying rate.
-- =============================================================================
