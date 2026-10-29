# User Branch Access — Investigation Report

## Summary (Quick Answers)

| Question | Answer |
|---|---|
| DB table for user–branch mapping | `OBMSBranches` (model: `OBMSBranches.cs`, EF DbSet: `OBMSBranches`) |
| `IsAllowed` field | `OBMSBranches.IsAllowed` (bool?) — flags whether the user may see that branch |
| User identity key | `OBMSBranches.Name` stores the **username** (string, matches `sessionStorage 'username'`) |
| Endpoint — allowed branches by user | `GET /master/GetBranchListByUserName?userName=X` — already filters `IsAllowed == true` |
| Endpoint — all branches (no filter) | `GET /master/GetBranchMasterAll` — no user filter |
| Separate "get allowed branches" endpoint? | **Yes — it already exists** (`GetBranchListByUserName`). Most components already call it. |
| How Angular stores username at login | `sessionStorage.setItem('username', ...)` + `DatasharingService.setUsername(...)` BehaviorSubject |
| Where filtering is incomplete / missing | `GetReceiptMaster` (Finance) misses `IsAllowed == true` check; `dashboard` uses `getBranchMasterList()` (no filter); `new-user-branch-access` admin page uses `getBranchMasterList()` by design (correct) |

---

## 1. Database / Domain Model

### `OBMSBranches` table

File: `d:\India Live\obms-api\Models\Domain\OBMSBranches.cs`

```csharp
[Table("OBMSBranches")]
public class OBMSBranches
{
    [Key] public int ID { get; set; }
    public string? Name { get; set; }       // ← stores the USERNAME
    public string? BranchCode { get; set; } // ← FK to BranchMaster.Code
    public bool? IsAllowed { get; set; }    // ← the access flag
    public DateTime LastUpdatedDate { get; set; }
    public string? LastUpdatedBy { get; set; }
}
```

**Key insight:** `Name` = username (not a person's full name). It is the join key to `OBMSUsers.Name`.  
There is one row per (user, branch) pair. `IsAllowed = true` means the user can access that branch.

### `OBMSUsers` table

File: `d:\India Live\obms-api\Models\Domain\Obmsuser.cs`

`OBMSUsers.Name` is the string username used throughout. There is no foreign-key constraint enforced at the EF layer between `OBMSBranches.Name` and `OBMSUsers.Name` — it is a logical relationship only.

---

## 2. Backend API — Branch Access Endpoints

### A. `GET /master/GetBranchListByUserName?userName=X`  ✅ Correct, filtered
File: `MasterController.cs` line 113 → `MasterRepository.GetBranchListByUserName()`

Logic (in `MasterRepository.cs` ~line 75–158):
- If `userName == "superadmin"` (case-insensitive) → returns **all** branches (no filter).
- Otherwise → joins `BranchMasters` with `OBMSBranches` on `Code = BranchCode`, filters  
  `WHERE OBMSBranches.Name == userName AND OBMSBranches.IsAllowed == true`.

This is the **correct, authoritative** endpoint for user-scoped branch lists.

### B. `GET /master/GetBranchMasterAll`  ⚠️ Unfiltered
Returns every row in `BranchMasters` with no user or `IsAllowed` check.  
Intended for admin/superadmin operations.

### C. `GET /Register/GetObmsBranchesPermissionByUser?userName=X`
File: `RegisterController.cs` line 614  
Returns the raw `OBMSBranches` rows for a user (ID, BranchCode, IsAllowed, etc.).  
Used by the **admin UI** (`new-user-branch-access`) to populate the permission editor grid — not for filtering branch dropdowns.

### D. `GET /Finance/GetReceiptMaster?userId=X`  ⚠️ Bug — missing `IsAllowed` check
File: `FinanceRepository.cs` line ~287

```csharp
var branchs = await (from branchMaster in _oBMSDbContext.BranchMasters
                     join obmsBranches in _oBMSDbContext.OBMSBranches
                     on branchMaster.Code equals obmsBranches.BranchCode
                     where obmsBranches.Name == userID   // ← missing: && obmsBranches.IsAllowed == true
                     orderby branchMaster.Name
                     select new { branchMaster.Code, branchMaster.Name }).ToListAsync();
```

Compare with the correct pattern in `GetInvoiceMaster()` (line ~31) which does include  
`&& joinResult.branches.IsAllowed == true`. The Receipts page gets branches from this endpoint (via `new-receipt.component.ts` → `service.GetReceiptMaster(currentUser)`) and will therefore show all OBMSBranches rows for the user regardless of the `IsAllowed` flag.

---

## 3. Login & Auth Flow

File: `login.component.ts`

1. User submits credentials → `CommonService.loginUser()` → `POST /Register/LoginUser`
2. API creates a JWT (claims: `ClaimTypes.Name = userName`; signed with key `"obms-authentication"`; expires 60 min).
3. API also returns the `OBMSUsers` object in the response body.
4. On success (`response.Success == 'Success'`), Angular:
   - `sessionStorage.setItem('username', response.Users.Name)` — the string username
   - `sessionStorage.setItem('userrole', response.Users.IsAdmin)` — boolean
   - `sessionStorage.setItem('currentUser', JSON.stringify(userObj))` — full user object (password cleared)
   - `DatasharingService.setUsername(response.Users.Name)` — BehaviorSubject for reactive consumers
   - `AuthService.setToken(response.token)` → `localStorage.setItem('auth_token', token)`

**Branch info is NOT stored at login.** Every component fetches its own branch list on `ngOnInit`, passing `currentUser` (from sessionStorage or DatasharingService) to `GetBranchListByUserName`.

---

## 4. Angular Frontend — Branch Dropdown Pattern

### Primary correct pattern (used by ~70 components)
```typescript
// ngOnInit
this.currentUser = sessionStorage.getItem('username')!;
// fallback for reactive
this._dataService.getUsername().subscribe(u => this.currentUser = u);

// inside getUserAccessRights or ngOnInit
this._masterService.GetBranchListByUserName(this.currentUser)
  .subscribe(data => {
    this.branchList = data.sort((a, b) => a.Code.localeCompare(b.Code));
  });
```
This goes to `GET /master/GetBranchListByUserName` which correctly filters by `IsAllowed == true`.

### Receipts page — uses GetReceiptMaster instead
`new-receipt.component.ts` line 881:
```typescript
return this.service.GetReceiptMaster(this.currentUser)
  .pipe(tap(d => { this.branchList = d['branchList']; ... }));
```
The backend query for `GetReceiptMaster` lacks the `IsAllowed` filter (see §2-D above).

### Dashboard — uses getBranchMasterList (no user filter)
`dashboard.component.ts` line 188:
```typescript
this._masterService.getBranchMasterList().subscribe(...); // GET /master/GetBranchMasterAll
```
This is used only for estimating total employee counts — not a user-facing branch picker. Low priority.

### Admin page (by design, no filter)
`new-user-branch-access.component.ts` line 130:  
Calls `getBranchMasterList()` (all branches) to show every branch so the admin can tick/untick `IsAllowed`.  
**This is correct** — you need to see all branches to grant access.

---

## 5. Where Branch Dropdowns Appear

The sidebar (`sidebar.component.html`) has no branch dropdown — it is a static navigation menu only.  
Branch selection happens inside individual module components (Invoice, Receipts, Payroll, Reports, etc.).

Summary of affected component categories:

| Category | Components | Pattern Used |
|---|---|---|
| Finance — Invoice, Batch Invoice, Credit/Debit Note | ~6 components | `GetBranchListByUserName` ✅ |
| Finance — Receipts list | `search-receipts.component.ts` | `GetBranchListByUserName` ✅ |
| Finance — New Receipt | `new-receipt.component.ts` | `GetReceiptMaster` ⚠️ (missing IsAllowed) |
| Finance — Payments, Legal | ~4 components | `GetBranchListByUserName` ✅ |
| Payroll — Attendance, Salary, Advances, Misc | ~9 components | `GetBranchListByUserName` ✅ |
| Reports — Finance | ~15 components | `GetBranchListByUserName` ✅ |
| Reports — Payroll | ~20 components | `GetBranchListByUserName` ✅ |
| Reports — Accounting | ~7 components | `GetBranchListByUserName` ✅ |
| Master — Branch / Client | ~4 components | Mixed (`getBranchMasterList` + `GetBranchListByUserName`) |
| Dashboard | `dashboard.component.ts` | `getBranchMasterList` (employee count estimate only) |
| Admin — User Branch Access | `new-user-branch-access.component.ts` | `getBranchMasterList` (by design, correct) |

---

## 6. Conclusions and Recommendations

### What is working correctly
The core filtering infrastructure is **already built and working**:
- `OBMSBranches` table with `IsAllowed` flag exists.
- `GetBranchListByUserName` API endpoint correctly filters by `IsAllowed == true` and special-cases `superadmin`.
- The vast majority of Angular components (~70+) already call `GetBranchListByUserName`.

### Bug: `GetReceiptMaster` missing `IsAllowed` filter

**File:** `d:\India Live\obms-api\Repositories\Implementation\FinanceRepository.cs` (~line 290)

**Fix:** Add `&& obmsBranches.IsAllowed == true` to the LINQ query:

```csharp
// Before:
where obmsBranches.Name == userID

// After:
where obmsBranches.Name == userID && obmsBranches.IsAllowed == true
```

Also add superadmin bypass for consistency with the pattern used everywhere else:
```csharp
bool isSuperAdmin = userID.Equals("superadmin", StringComparison.OrdinalIgnoreCase);
// ...
where isSuperAdmin || (obmsBranches.Name == userID && obmsBranches.IsAllowed == true)
```

This affects `new-receipt.component.ts` (New Receipt form) which populates its branch dropdown from this endpoint.

### Optional: Dashboard uses unfiltered branch list

`dashboard.component.ts` calls `getBranchMasterList()` (returns all branches) to estimate employee counts.  
This is not a user-facing branch picker so it is unlikely to cause a data leakage problem.  
If the dashboard should show stats only for the user's allowed branches, replace with `GetBranchListByUserName(currentUser)`.

### No structural changes needed

The schema, the filtering API, and the frontend service layer are all correct. Only the `GetReceiptMaster` backend query needs a one-line fix. No new endpoints, no Angular service changes, no auth changes are required.

---

## File Reference Index

| File | Role |
|---|---|
| `obms-api/Models/Domain/OBMSBranches.cs` | Domain model — user-branch access table |
| `obms-api/Models/Domain/Obmsuser.cs` | User model |
| `obms-api/Controllers/MasterController.cs` (line 113) | `GetBranchListByUserName` endpoint |
| `obms-api/Controllers/RegisterController.cs` (line 614) | `GetObmsBranchesPermissionByUser` (admin) |
| `obms-api/Controllers/FinanceController.cs` (line 4717) | `GetReceiptMaster` endpoint |
| `obms-api/Repositories/Implementation/MasterRepository.cs` (line 75) | `GetBranchListByUserName` impl — correct `IsAllowed` filter |
| `obms-api/Repositories/Implementation/FinanceRepository.cs` (line 287) | `GetReceiptMaster` impl — **missing `IsAllowed` filter** |
| `obms-ui/src/app/service/mastermodule.service.ts` | Angular service — `GetBranchListByUserName()` and `getBranchMasterList()` |
| `obms-ui/src/app/service/finance.service.ts` (line 103, 255) | Angular service — `GetBranchListByUserName()` and `GetReceiptMaster()` |
| `obms-ui/src/app/service/auth.service.ts` | JWT token storage in localStorage |
| `obms-ui/src/app/service/datasharing.service.ts` | BehaviorSubject for reactive username sharing |
| `obms-ui/src/app/modules/user/login/login.component.ts` | Login flow — sets sessionStorage + DatasharingService |
| `obms-ui/src/app/modules/administration/user-branch-access/` | Admin UI to edit user-branch permissions |
| `obms-ui/src/app/components/sidebar/sidebar.component.html` | Sidebar — static nav, no branch dropdown |
| `obms-ui/src/app/modules/finance/receipts/new-receipt/new-receipt.component.ts` | Uses `GetReceiptMaster` for branch list |
| `obms-ui/src/app/modules/finance/receipts/search-receipts/search-receipts.component.ts` | Uses `GetBranchListByUserName` — correct |
