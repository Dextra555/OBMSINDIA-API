-- ============================================================
-- Migration : Agreement WorkPlace Unique Key Enforcement
-- Purpose   : Enforce that only ONE active agreement exists per
--             Branch + Client + WorkPlace combination.
--
-- IMPORTANT : A DB-level unique index is NOT used here because
--             the history-tracking pattern keeps old rows active
--             (IsValid = true/null) when a date change creates a
--             new version row.  The unique constraint is enforced
--             at the API level (CheckDuplicateAgreement).
--
--             This script:
--               1. Reports any existing duplicates so they can be
--                  reviewed before deploying the new API code.
--               2. Provides a helper query to identify and cancel
--                  (IsValid = 0) older duplicate rows, keeping only
--                  the most-recent one per Branch+Client+WorkPlace.
--               3. Documents the expected unique key rules.
--
-- Run on    : OBMSINDIA database
-- Date      : 2026-09-12
-- ============================================================

-- ============================================================
-- STEP 1 : Check for existing duplicates
--          (same Branch + Client + WorkPlace, more than one active row)
-- ============================================================
SELECT
    Branch,
    Client,
    ISNULL(LTRIM(RTRIM(WorkPlace)), '') AS WorkPlace,
    COUNT(*)                            AS ActiveCount,
    MIN(ID)                             AS OldestID,
    MAX(ID)                             AS LatestID,
    MIN(AgreementDate)                  AS OldestDate,
    MAX(AgreementDate)                  AS LatestDate
FROM Agreement
WHERE (IsValid = 1 OR IsValid IS NULL)
GROUP BY
    Branch,
    Client,
    ISNULL(LTRIM(RTRIM(WorkPlace)), '')
HAVING COUNT(*) > 1
ORDER BY Branch, Client, WorkPlace;

-- ============================================================
-- STEP 2 : Cancel older duplicate rows
--          For each Branch+Client+WorkPlace group that has more
--          than one active row, keep the LATEST (highest ID) and
--          cancel (IsValid = 0) all older ones.
--
--          REVIEW the STEP 1 output BEFORE running this.
--          Wrap in a transaction and verify before committing.
-- ============================================================

BEGIN TRANSACTION;

UPDATE Agreement
SET
    IsValid     = 0,
    LASTUPDATE  = GETDATE()
WHERE ID IN (
    -- All active rows that are NOT the latest per Branch+Client+WorkPlace
    SELECT a.ID
    FROM Agreement a
    INNER JOIN (
        -- Latest ID per group
        SELECT
            Branch,
            Client,
            ISNULL(LTRIM(RTRIM(WorkPlace)), '') AS WorkPlace,
            MAX(ID)                             AS LatestID
        FROM Agreement
        WHERE (IsValid = 1 OR IsValid IS NULL)
        GROUP BY
            Branch,
            Client,
            ISNULL(LTRIM(RTRIM(WorkPlace)), '')
        HAVING COUNT(*) > 1          -- only groups with duplicates
    ) latest
        ON  a.Branch    = latest.Branch
        AND a.Client    = latest.Client
        AND ISNULL(LTRIM(RTRIM(a.WorkPlace)), '') = latest.WorkPlace
        AND a.ID       <> latest.LatestID          -- exclude the one we keep
    WHERE (a.IsValid = 1 OR a.IsValid IS NULL)
);

-- Verify: should return 0 rows after the update
SELECT
    Branch,
    Client,
    ISNULL(LTRIM(RTRIM(WorkPlace)), '') AS WorkPlace,
    COUNT(*) AS ActiveCount
FROM Agreement
WHERE (IsValid = 1 OR IsValid IS NULL)
GROUP BY
    Branch,
    Client,
    ISNULL(LTRIM(RTRIM(WorkPlace)), '')
HAVING COUNT(*) > 1;

-- If the above SELECT returns 0 rows → no duplicates remain → safe to commit.
-- COMMIT TRANSACTION;

-- If rows are returned → review and rollback.
-- ROLLBACK TRANSACTION;

-- ============================================================
-- STEP 3 : Unique key rule documentation (API enforced)
-- ============================================================
-- Rule   : Branch + Client + WorkPlace must be unique among
--          active agreements (IsValid = 1 OR IsValid IS NULL).
--
-- Enforced by: API → AgreementRepository.CheckDuplicateAgreement()
--              Returns HTTP 400 { Success: "Duplicate" } if violated.
--
-- WorkPlace change on existing agreement:
--   → BLOCKED by API (HTTP 400 { Success: "WorkPlaceChanged" }).
--   → User must create a new Agreement for the new WorkPlace.
--
-- Date change on existing agreement:
--   → Creates a new history row (same WorkPlace, new date, new ID).
--   → Old row remains in DB as history.
--   → List shows only latest row per Branch+Client+WorkPlace.
--
-- NOTE: A SQL filtered unique index is intentionally NOT created
--       because old history rows (same WorkPlace, older date) share
--       the same Branch+Client+WorkPlace key and would violate the
--       index even though they are superseded versions, not true
--       duplicates.
-- ============================================================
