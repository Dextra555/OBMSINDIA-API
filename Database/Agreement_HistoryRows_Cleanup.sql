-- ============================================================
-- Script    : Agreement History Rows Cleanup
-- Purpose   : For each Branch + Client + WorkPlace group,
--             keep ONLY the latest row (highest ID) active.
--             All older rows are set IsValid = 0 so they
--             disappear from the list screen.
--
-- Run STEP 1 first to preview what will be changed.
-- Then run STEP 2 inside a transaction and verify before COMMIT.
--
-- Run on    : OBMSINDIA database
-- ============================================================

-- ============================================================
-- STEP 1 : Preview — which rows will be deactivated?
-- ============================================================
SELECT
    a.ID,
    a.Branch,
    a.Client,
    ISNULL(LTRIM(RTRIM(a.WorkPlace)), '') AS WorkPlace,
    a.AgreementDate,
    a.IsValid,
    'Will be set IsValid=0 (old history row)' AS Action
FROM Agreement a
INNER JOIN (
    -- Latest active ID per Branch+Client+WorkPlace
    SELECT
        Branch,
        Client,
        ISNULL(LTRIM(RTRIM(WorkPlace)), '') AS WorkPlace,
        MAX(ID) AS LatestID
    FROM Agreement
    WHERE (IsValid = 1 OR IsValid IS NULL)
    GROUP BY Branch, Client, ISNULL(LTRIM(RTRIM(WorkPlace)), '')
) latest
    ON  a.Branch = latest.Branch
    AND a.Client = latest.Client
    AND ISNULL(LTRIM(RTRIM(a.WorkPlace)), '') = latest.WorkPlace
    AND a.ID <> latest.LatestID   -- not the latest → old history row
WHERE (a.IsValid = 1 OR a.IsValid IS NULL)
ORDER BY a.Branch, a.Client, a.WorkPlace, a.AgreementDate;

-- ============================================================
-- STEP 2 : Apply — deactivate all old history rows
-- ============================================================
BEGIN TRANSACTION;

UPDATE Agreement
SET
    IsValid    = 0,
    LASTUPDATE = GETDATE()
WHERE ID IN (
    SELECT a.ID
    FROM Agreement a
    INNER JOIN (
        SELECT
            Branch,
            Client,
            ISNULL(LTRIM(RTRIM(WorkPlace)), '') AS WorkPlace,
            MAX(ID) AS LatestID
        FROM Agreement
        WHERE (IsValid = 1 OR IsValid IS NULL)
        GROUP BY Branch, Client, ISNULL(LTRIM(RTRIM(WorkPlace)), '')
    ) latest
        ON  a.Branch = latest.Branch
        AND a.Client = latest.Client
        AND ISNULL(LTRIM(RTRIM(a.WorkPlace)), '') = latest.WorkPlace
        AND a.ID <> latest.LatestID
    WHERE (a.IsValid = 1 OR a.IsValid IS NULL)
);

-- Verify: each Branch+Client+WorkPlace should now have exactly 1 active row
SELECT
    Branch,
    Client,
    ISNULL(LTRIM(RTRIM(WorkPlace)), '') AS WorkPlace,
    COUNT(*) AS ActiveCount,
    MAX(ID)  AS ActiveID,
    MAX(AgreementDate) AS ActiveDate
FROM Agreement
WHERE (IsValid = 1 OR IsValid IS NULL)
GROUP BY Branch, Client, ISNULL(LTRIM(RTRIM(WorkPlace)), '')
ORDER BY Branch, Client, WorkPlace;

-- If ActiveCount = 1 for all rows → COMMIT
-- COMMIT TRANSACTION;

-- If any ActiveCount > 1 → ROLLBACK and investigate
-- ROLLBACK TRANSACTION;
