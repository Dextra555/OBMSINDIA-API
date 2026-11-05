# Investigation: Branch Master — State Filter showing duplicate values

## Summary (answer first)

The Branch Master "State Filter" dropdown is populated **client-side** from each branch record's `State` field — there is **no backend endpoint that returns a distinct/canonical state list** for this page. De-duplication is done in the Angular component with `new Set()` using **strict string equality** (`branch-master.component.ts:215`). That de-dup does **not** trim or case-normalize the value before using it as the Set key; it only discards empty/whitespace-only strings. As a result it **cannot merge strings that differ only by trailing/leading spaces, internal double spaces, or letter case** (e.g. `"Tamil Nadu"` vs `"Tamil Nadu "` vs `"tamil nadu"`). The same strict `===` comparison is used in the filter, count, and display-name logic, so a selected state would also miss branches stored as a variant of that value.

I verified against the **local** DB (`OBMSINDIANEW`) that its `BranchMaster.State` data is clean — only 5 distinct values, no whitespace/case variants — so the local app does **not** reproduce the duplicate. The **production** frontend (`environment.prod.ts`) talks to a **remote** API (`http://124.217.236.37:64889/api/`) backed by a separate database I could not inspect. The de-dup feature has been committed and stable in the repo since ~April–July 2026, and the user can see the State Filter dropdown, so the deployed build contains it. Therefore the duplicates the user sees are caused by **variant `State` strings in the production `BranchMaster.State` column** that strict-equality `new Set()` cannot merge. (The system's own Professional-Tax code treating `"Tamil Nadu" / "TamilNadu" / "TN"` as equivalent — `MasterController` PT section — corroborates that such variants exist in the data.)

Root cause: **data + fragile de-dup**, not a missing SQL `DISTINCT` and not duplicate branch rows. Recommended fix is to normalize the state key (trim + collapse whitespace + case-insensitive) in the frontend de-dup and matching logic, plus a one-time cleanup of the production `BranchMaster.State` column. Nothing implemented — recommendations only.

---

## Evidence (with file / symbol citations)

### 1. Frontend: state dropdown is built from branch data, de-duped with strict-equality `Set`

`d:\India Live\obms-ui\src\app\modules\master\branch-master\branch-master.component.ts`

- `handleDataBinding()` stores the API result in `originalBranchData`, binds the table, then calls `loadAvailableStates()` (line 146).
- `loadAvailableStates()` (lines 213–217) — the de-dup:
  ```ts
  // line 215
  const states = [...new Set(this.originalBranchData.map((branch: any) => branch.State)
      .filter((state: string) => state && state.trim() !== ''))];
  this.availableStates = states.sort();
  ```
  - `new Set(...)` dedupes by **strict equality**. The `.filter(...)` only **excludes** empty / whitespace-only values (`state.trim() !== ''`); it does **not** trim or case-fold the value that becomes the Set key. So `"Tamil Nadu"` and `"Tamil Nadu "` remain two distinct Set entries → two identical-looking dropdown options.
- `applyStateFilter()` (lines 225–240) filters with `branch.State === this.selectedState` (line 233) — strict equality, so a variant never matches a different variant.
- `getStateFilterCount()` (lines 247–253) uses `branch.State === this.selectedState` (line 252) — same strict equality.
- `getStateDisplayName(state)` (lines 255–258) uses `branch.State === state` (line 257) — same strict equality; renders `${state} (${count})`.

So there are **four** strict-equality comparisons on the raw `State` string (lines 215, 233, 252, 257), all of which break on whitespace/case variants.

`d:\India Live\obms-ui\src\app\modules\master\branch-master\branch-master.component.html` — the dropdown iterates the de-duped list:
```html
<mat-label>State Filter</mat-label>
<mat-select (selectionChange)="onStateFilterChange($event)" [(value)]="selectedState">
  <mat-option value="">All States</mat-option>
  <mat-option *ngFor="let state of availableStates" [value]="state">
    {{ getStateDisplayName(state) }}
  </mat-option>
</mat-select>
```

### 2. No backend state-list endpoint for Branch Master

- `d:\India Live\obms-api\Controllers\MasterController.cs` — branch endpoints:
  - `GetBranchMasterAll()` (route `GetBranchMasterAll`, ~line 56) → `_masterRepository.GetBranchMasterAllList()` → returns the full branch list.
  - `GetBranchListByUserName(userName)` (route `GetBranchListByUserName`, ~line 113) → `_masterRepository.GetBranchListByUserName(userName)`.
  - There is **no** `GetState`/`GetStateList`/distinct-state endpoint. The only `GetStateList()` is a hardcoded helper in `d:\India Live\obms-api\Utility\Utility.cs` (lines 6–33, 23 Indian states) used by `InventoryRepository`/`EmployeeRepository` for *other* screens — **not** by Branch Master.
- `d:\India Live\obms-api\Repositories\Implementation\MasterRepository.cs`:
  - `GetBranchMasterAllList()` (lines 62–72): `_oBMSDbContext.BranchMasters.ToList();` — returns every branch row with its `State` value. No state aggregation.
  - `GetBranchListByUserName(userName)` (lines 75–163): superadmin path returns all branches with `.Distinct()`; non-superadmin path joins `BranchMasters`↔`OBMSBranches`, filters by user, projects to `BranchMaster`, and applies `.Distinct()` (line ~159). So the backend does **not** return duplicate branch rows; each branch carries one consistent `State` string.

### 3. `State` is an unconstrained free-text column

`d:\India Live\obms-api\Models\Domain\BranchMaster.cs`:
```csharp
public string State { get; set; }   // no [StringLength], no enum, no normalization
```
No constraints → inconsistent data entry is possible.

### 4. New branches are entered from a clean canonical dropdown (so NEW data is consistent; the problem is legacy/variant data)

`d:\India Live\obms-ui\src\app\modules\master\branch-master\new-branch\new-branch.component.ts` (lines 26–62): `statesList` is a hardcoded array of 36 properly-cased states; the form binds a `mat-select` whose `[value]="state.name"` (`new-branch.component.html` lines 101–108). So branches created through this form get canonical names — but data entered before this dropdown existed, via import, or with stray spaces remains inconsistent.

### 5. Local DB is clean — does not reproduce

Read-only query against `OBMSINDIANEW` (Windows-auth; no writes performed):
```
SELECT '[' + ISNULL(State,'') + ']' AS StateBracketed, LEN(State) AS StrLen, COUNT(*) AS BranchCount
FROM dbo.BranchMaster GROUP BY State ORDER BY State;
```
Result:
```
StateBracketed     | StrLen | BranchCount
[Andhra Pradesh]  | 14     | 1
[Gujarat]         | 7      | 1
[Karnataka]       | 9      | 1
[Tamil Nadu]      | 10     | 3
[Telangana]       | 9      | 1
```
Only 5 distinct values; brackets confirm no leading/trailing spaces; the 3 Tamil Nadu rows share the identical string. With this data the `new Set()` de-dup yields exactly 5 options → **no duplicate locally**. The clean local data comes from clean seeding (`d:\India Live\obms-api\Scripts\03_Seed_IndianData.sql` inserts canonical names); there is **no** state-cleanup/normalization migration in the repo.

### 6. Production points to a different DB I cannot inspect

`d:\India Live\obms-ui\src\environments\environment.prod.ts`:
```ts
baseUrl: 'http://124.217.236.37:64889/api/',
```
The local `appsettings.json` connects to `DESKTOP-FHKDAA6 / OBMSINDIANEW`; production uses the remote host above with its own database. I did not query the production API/DB (would hit a live system).

### 7. The de-dup feature is committed and old

`git log -S "loadAvailableStates"` on `branch-master.component.ts`: introduced by 2026-04-09 (`2a3efe0`), refined 2026-07-01 / 2026-07-23 (`a06e513`); file unchanged since. The HTML "State Filter" block is in `HEAD`. So a deployment made any time after mid-2026 contains the de-dup — consistent with the user being able to *see* the State Filter dropdown.

---

## Conclusions

- **Not** a missing SQL `DISTINCT`: there is no state-list query at all for this page; states are derived client-side.
- **Not** duplicate branch rows from the API: the repository applies `.Distinct()` (and `GetBranchMasterAllList` returns one row per branch).
- **It is** fragile client-side de-dup (strict-equality `Set`, no normalization) combined with variant `State` strings in the production data. Whitespace and case variants are the most likely; spelling variants (e.g. `Tamil Nadu` / `TamilNadu` / `TN`) are plausible given the PT code's equivalences.
- The local environment does not reproduce the bug because its `State` data is clean; the bug is environment-specific to production data.

Definitive confirmation (not performed — would hit production): run the same `GROUP BY State` query against the **production** `BranchMaster` table (or call the prod `GetBranchMasterAll` endpoint and inspect the `State` values). If two rows differ only by spaces/case/spelling while representing the same state, that confirms it.

---

## Recommendations (implement nothing — for the fix step)

### Primary fix — make the frontend de-dup and matching normalization-robust
File: `d:\India Live\obms-ui\src\app\modules\master\branch-master\branch-master.component.ts`

Add a small normalize helper (trim → collapse internal whitespace → lowercase) and use it as the Set key and in every comparison. Specifically touch the four strict-equality sites:

1. `loadAvailableStates()` (lines 213–217): key the `Set`/`Map` by the normalized value; keep a clean trimmed string for display, e.g.:
   ```ts
   const map = new Map<string, string>();
   for (const branch of this.originalBranchData) {
     const raw = (branch?.State ?? '').toString();
     const trimmed = raw.trim();
     if (!trimmed) continue;
     const key = trimmed.toLowerCase().replace(/\s+/g, ' ');
     if (!map.has(key)) map.set(key, trimmed);
   }
   this.availableStates = Array.from(map.values()).sort((a, b) => a.localeCompare(b));
   ```
2. `applyStateFilter()` (line 233): compare normalized `branch.State` to normalized `this.selectedState` instead of `===`.
3. `getStateFilterCount()` (line 252): same normalized comparison.
4. `getStateDisplayName(state)` (line 257): same normalized comparison.

This removes whitespace and case variants from the dropdown and makes selection match all variants of a state. (Spelling variants like `TamilNadu`/`TN` are not merged by normalization alone — handle those via the DB cleanup below, or an explicit mapping table if desired.)

### Secondary fix — one-time cleanup of production `State` data
Run against the **production** `BranchMaster` table (back up first):
```sql
-- remove leading/trailing spaces
UPDATE dbo.BranchMaster SET State = LTRIM(RTRIM(State)) WHERE State IS NOT NULL AND State <> LTRIM(RTRIM(State));
-- collapse internal double/multiple spaces (repeat until no rows affected)
UPDATE dbo.BranchMaster SET State = REPLACE(State, '  ', ' ') WHERE State LIKE '%  %';
-- normalize known spelling variants to canonical names, e.g.:
UPDATE dbo.BranchMaster SET State = 'Tamil Nadu' WHERE LOWER(LTRIM(RTRIM(State))) IN ('tamilnadu','tn');
-- (extend the mapping to match the canonical list in new-branch.component.ts statesList)
```
This guarantees a single canonical string per state regardless of how it was entered.

### Optional — single source of truth
Add a backend endpoint returning distinct canonical states (e.g. `SELECT DISTINCT LTRIM(RTRIM(State)) AS State FROM dbo.BranchMaster WHERE LTRIM(RTRIM(State)) <> '' ORDER BY State`) and have the frontend use it, so every consumer shares one de-duplicated list. (This would also help the TDS/GST branch-dropdown duplicates the user mentioned, but those are separate tasks.)

### Deployment
The frontend fix must be built and deployed to the production server (`124.217.236.37`); committing the change alone will not affect the running app.

### Verification (after fix)
- Build the UI (`ng build` / the project's build command) and confirm it compiles.
- On a data set containing a whitespace/case variant (e.g. insert a test branch with `State = 'Tamil Nadu '`) the State Filter dropdown should show `Tamil Nadu` exactly once, and selecting it should return all Tamil Nadu branches including the variant-spelled one.
- Re-run the `GROUP BY State` query on production after cleanup to confirm one row per canonical state.
