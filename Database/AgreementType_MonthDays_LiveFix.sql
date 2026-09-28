-- ============================================================
-- FILE   : AgreementType_MonthDays_LiveFix.sql
-- PURPOSE: Live DB-ல AgreementType column NOT NULL issue fix
--          மற்றும் MonthDays column இல்லாவிட்டால் add பண்ணும்
-- DATABASE: OBMSINDIAUАТ.dbo.AgreementDetails
-- DATE   : 2026-09-24
-- ============================================================

-- ✅ STEP 1: AgreementType column இருக்கா என்று check பண்ணு,
--            இல்லாவிட்டால் add பண்ணு (NULL allowed, default 'N')
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AgreementDetails'
      AND COLUMN_NAME = 'AgreementType'
)
BEGIN
    ALTER TABLE dbo.AgreementDetails
    ADD AgreementType NVARCHAR(200) NULL DEFAULT 'N';

    PRINT 'AgreementType column added successfully.';
END
ELSE
BEGIN
    PRINT 'AgreementType column already exists.';
END
GO

-- ✅ STEP 2: AgreementType column NOT NULL-ஆ இருந்தா NULL allow-ஆக மாற்று
--            (Local-ல NULL ok, Live-ல NOT NULL — இதுதான் error காரணம்)
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AgreementDetails'
      AND COLUMN_NAME = 'AgreementType'
      AND IS_NULLABLE = 'NO'
)
BEGIN
    -- Existing NULL rows-க்கு default value set பண்ணு
    UPDATE dbo.AgreementDetails
    SET AgreementType = 'N'
    WHERE AgreementType IS NULL;

    -- Column-ஐ nullable-ஆக மாற்று
    ALTER TABLE dbo.AgreementDetails
    ALTER COLUMN AgreementType NVARCHAR(200) NULL;

    PRINT 'AgreementType column changed to NULL allowed.';
END
ELSE
BEGIN
    PRINT 'AgreementType column is already nullable — no change needed.';
END
GO

-- ✅ STEP 3: MonthDays column இருக்கா என்று check பண்ணு,
--            இல்லாவிட்டால் add பண்ணு
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AgreementDetails'
      AND COLUMN_NAME = 'MonthDays'
)
BEGIN
    ALTER TABLE dbo.AgreementDetails
    ADD MonthDays INT NOT NULL DEFAULT 0;

    PRINT 'MonthDays column added successfully.';
END
ELSE
BEGIN
    PRINT 'MonthDays column already exists.';
END
GO

-- ✅ STEP 4: Final verification — column status பார்க்கலாம்
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AgreementDetails'
  AND COLUMN_NAME IN ('AgreementType', 'MonthDays')
ORDER BY COLUMN_NAME;
GO

-- ============================================================
-- EXPECTED OUTPUT:
-- COLUMN_NAME    | DATA_TYPE | IS_NULLABLE | COLUMN_DEFAULT
-- AgreementType  | nvarchar  | YES         | (N'N') or NULL
-- MonthDays      | int       | NO          | ((0))
-- ============================================================
