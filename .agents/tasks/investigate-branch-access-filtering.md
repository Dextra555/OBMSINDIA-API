# Branch Access Filtering — Investigation Report

## Summary Answer

The backend infrastructure for branch-access filtering is fully in place and working correctly for most pages. The problem is **four specific components that call the wrong service method** to populate their branch dropdowns:

| Page | Wrong call | Right call |
|---|---|---|
| Employee Transfer | `EmployeeTransferService.getEmployees()` → `Master/GetBranchMasterAll` (all branches, no filter) | `MastermoduleService.GetBranchListByUserName(currentUser)` |
| Search Employee History | `EmployeeService.getEmployees(currentUser)` works, but only the `branchList` property inside the result is used — and the Employees endpoint itself does filter by user. This one is **partially correct but only pre-selects the first branch**; the dropdown doesn't refresh per-user because the first branch's history loads automatically. |  (see detail below) |
| New Receipt | `FinanceService.GetReceiptMaster(currentUser)` — the branch query inside this method **does NOT filter by `IsAllowed`**; it joins OBMSBranches on Name only, showing branches where `IsAllowed=false` too. | Fix `GetReceiptMaster` to add `&& obmsBranches.IsAllowed == true` |
| Bulk Attendance Upload | `PayrollModuleService.getBranchList()` → `master/GetBranchMasterAll` (unfiltered, no user parameter) | `MastermoduleService.GetBranchListByUserName(currentUser)` |

---

## Evidence

### 1. How User Branch Access is saved and stored

**Component:** `d:\India Live\obms-ui\src\app\modules\administration\user-branch-access\new-user-branch-access\new-user-branch-access.component.ts`

The `NewUserBranchAccessComponent` loads all branches from `master/GetBranchMasterAll`, then loads the user's current permissions from `register/GetObmsBranchesPermissionByUser?userName=X`. It renders a checkbox per branch. On save, it calls `register/SaveAndUpdateObmsBranches` with the full list (one entry per branch, `IsAllowed` true/false).

**Backend save:** `RegisterController.cs` → `RegisterRepository.cs` `SaveAndUpdateObmsBranches()` — upserts one row per branch in the `OBMSBranches` table with `IsAllowed` set to the checkbox state.

**Backend read (permission check):** `RegisterRepository.cs` `GetObmsBranchesPermissionByUser()`:
```csharp
return _dbContext.OBMSBranches.Where(u => u.Name == userName).ToList();
```
This returns ALL rows for the user including `IsAllowed=false` ones. That is intentional — the new-user-branch-access form needs to render all branches with their current checkbox state.

### 2. The correct filter endpoint — GetBranchListByUserName

**Service:** `mastermodule.service.ts` line 42 → `master/GetBranchListByUserName?userName=X`

**Backend:** `MasterController.cs` line 113 → `MasterRepository.cs` `GetBranchListByUserName()`:
```csharp
result = await _oBMSDbContext.BranchMasters
    .Join(_oBMSDbContext.OBMSBranches, ...)
    .Where(joinResult => joinResult.branches.Name == userName 
        && joinResult.branches.IsAllowed == true)  // ← filters correctly
    .Select(...)
    .ToListAsync();
```
Special case: if `userName == "superadmin"`, returns all branches without a join filter. `admin` is NOT handled as a special case in this method — an `admin` user gets branch-filtered results too, so admin users need branch access assigned to them in `OBMSBranches`.

This method is already correctly used by: salary-processing, payroll reports (pay-sheet, payslip, PF, ESI, PT, SOCSO, SIP, netpay, monthly-advance, pay-history, non-compliance), voucher-filter-report, new-attendance, rbi-bank-salary-process, rbi-bank-advance-salary-process, GL data list, quotations, agreements, statutory-compliance-report, yearly-pay-sheet. **These pages work correctly.**

### 3. Login — what is stored in sessionStorage

**File:** `d:\India Live\obms-ui\src\app\modules\user\login\login.component.ts`

On successful login, `successLogin()` stores:
- `sessionStorage.setItem('username', response.Users.Name)` — the username string
- `sessionStorage.setItem('userrole', response.Users.IsAdmin)` — role
- `sessionStorage.setItem('currentUser', JSON.stringify(data))` — full user object

No branch list is pre-fetched or cached at login time. Every page independently fetches its branch list on init. This is the correct approach, but it means each page must call the user-filtered endpoint.

---

## Root Cause Analysis Per Page

### Employee Transfer — `d:\India Live\obms-ui\src\app\modules\master\employee-transfer\employee-transfer.component.ts`

```typescript
// constructor — called before ngOnInit, before currentUser is set
service.getEmployees().subscribe((d: any) => {
  this.branchList = d;      // ← d is List<BranchMaster> with ALL branches
  this.branchTOList = d;
});
```

**Problem:** `EmployeeTransferService.getEmployees()` calls `Master/GetBranchMasterAll` with no user parameter. This returns every branch unconditionally. The call is also in the constructor, before `currentUser` is read from sessionStorage.

**Fix needed:** Replace the `service.getEmployees()` call in the constructor with `this._masterService.GetBranchListByUserName(this.currentUser)` called from `ngOnInit()` (after `currentUser` is set from sessionStorage).

### Search Employee History — `d:\India Live\obms-ui\src\app\modules\master\employee-history\employee-history.component.ts`

```typescript
forkJoin({
  branchResult: this.service.getEmployees(this.currentUser)  // Employee/Employees?name=currentUser
}).subscribe({
  next: ({ branchResult }) => {
    this.branchList = branchResult['branchList'];  // ← filtered list from Employee controller
    const selectedBranch = this.branchList[0]?.branchCode;  // ← uses Code not branchCode
    this._employeeTransfer.getEmployeeHistoryDetails(selectedBranch).subscribe(...)
  }
});
```

**Problem 1:** `EmployeeController.GetEmployees()` does filter branches by `IsAllowed=true` for non-superadmin users, so `branchList` contains only allowed branches — **this part is correct**. However, `branchList` items returned from the API have property `Code`, not `branchCode`, so `this.branchList[0]?.branchCode` will be `undefined`, and `getEmployeeHistoryDetails(undefined)` is called with no branch, which may return all history.

**Problem 2:** The branch dropdown in the template also shows `this.branchList` which is correctly filtered. But the `branchList[0]?.branchCode` property name bug means the initial load shows everything.

**Fix needed:** Change `this.branchList[0]?.branchCode` to `this.branchList[0]?.Code` so the initial history load is scoped to the user's first allowed branch.

Additionally, the branch dropdown `frm.get('BranchCode')` is never pre-populated with the first branch — the template dropdown starts blank and the user must manually change it to trigger `branchChange()`. Consider also calling `this.frm.patchValue({ BranchCode: selectedBranch })` after loading.

### New Receipt — `d:\India Live\obms-ui\src\app\modules\finance\receipts\new-receipt\new-receipt.component.ts`

```typescript
return this.service.GetReceiptMaster(this.currentUser).pipe(
  tap((d: any) => {
    this.branchList = (d['branchList'] || []).sort(...);  // ← filtered by GetReceiptMaster
    this.bankList = d['bankList'];
    this.bankInList = d['banks'];
  }),
  ...
);
```

**Backend** `FinanceRepository.GetReceiptMaster()`:
```csharp
var branchs = await (from branchMaster in _oBMSDbContext.BranchMasters
                     join obmsBranches in _oBMSDbContext.OBMSBranches
                     on branchMaster.Code equals obmsBranches.BranchCode
                     where obmsBranches.Name == userID   // ← no IsAllowed check!
                     orderby branchMaster.Name
                     select new { branchMaster.Code, branchMaster.Name }).ToListAsync();
```

**Problem:** The join filters by `Name == userID` but **does not check `IsAllowed == true`**. So every branch the user was ever added to (including ones where IsAllowed was set to false) appears in the Receipt dropdown. Compare with `GetPaymentMaster` in the same file which DOES handle `isSuperAdmin` but also lacks the `IsAllowed` check.

**Fix needed:** In `FinanceRepository.cs`, add `&& obmsBranches.IsAllowed == true` to the WHERE clause in `GetReceiptMaster`. This is a backend fix.

### Bulk Attendance Upload — `d:\India Live\obms-ui\src\app\modules\payroll\attendance\bulk-attendance-upload\bulk-attendance-upload.component.ts`

```typescript
// line ~70
currentUser: string = 'admin'; // TODO: Get from auth service  ← hardcoded!

private loadBranches(): void {
  this.payrollService.getBranchList().subscribe({  // → master/GetBranchMasterAll
    next: (branches: any[]) => {
      this.branches = branches;  // ← all branches, unfiltered
    }
  });
}
```

**Two problems:**
1. `currentUser` is hardcoded to `'admin'` — it never reads from `sessionStorage.getItem('username')`.
2. `PayrollModuleService.getBranchList()` calls `master/GetBranchMasterAll` — no user parameter, returns all branches.

Why Mumbai is missing: The problem description says all branches show except Mumbai is missing from Bulk Attendance Upload. This is because `GetBranchMasterAll` actually returns all branches — Mumbai is missing because the Mumbai branch may have an issue in the `BranchMasters` table itself (inactive, deleted, etc.), or it was added after a filtered snapshot. This is a separate data issue unrelated to branch-access filtering.

**Fix needed:**
1. Read `currentUser` from `sessionStorage.getItem('username')` in `ngOnInit()`.
2. Change `loadBranches()` to call `this._masterService.GetBranchListByUserName(this.currentUser)` (inject `MastermoduleService`).

---

## What is Working Correctly

- **User Branch Access save/load:** The `new-user-branch-access` component correctly loads all branches with per-user checkbox state, and saves correctly via `SaveAndUpdateObmsBranches`. ✓
- **Backend filter logic:** `GetBranchListByUserName` in `MasterRepository.cs` correctly joins `OBMSBranches` and filters by `IsAllowed=true`. ✓
- **superadmin bypass:** All filtered methods correctly return all branches for superadmin. ✓
- **Most payroll/report pages:** Already use `GetBranchListByUserName`. ✓
- **Invoices, Agreements, Quotations, Inventory:** Use `GetBranchListByUserName` or equivalent filtered queries. ✓

---

## Recommended Fixes (Summary)

### Fix 1 — Employee Transfer (Frontend)
**File:** `d:\India Live\obms-ui\src\app\modules\master\employee-transfer\employee-transfer.component.ts`

Move branch loading out of constructor, into `ngOnInit()` after setting `currentUser`, and replace:
```typescript
// Remove from constructor:
service.getEmployees().subscribe(...)

// Add to ngOnInit after currentUser is set:
this._masterService.GetBranchListByUserName(this.currentUser).subscribe((d: any) => {
  this.branchList = d;
  this.branchTOList = d;
});
```
Also inject `MastermoduleService` if not already present (it is already injected: `private _masterService: MastermoduleService`).

### Fix 2 — Search Employee History (Frontend)
**File:** `d:\India Live\obms-ui\src\app\modules\master\employee-history\employee-history.component.ts`

Change property name from `branchCode` to `Code`:
```typescript
// Line ~80: fix property name
const selectedBranch = this.branchList[0]?.Code;  // was: branchList[0]?.branchCode
```
Optionally pre-populate the dropdown:
```typescript
this.frm.patchValue({ BranchCode: selectedBranch });
```

### Fix 3 — New Receipt (Backend)
**File:** `d:\India Live\obms-api\Repositories\Implementation\FinanceRepository.cs`

Add `IsAllowed` filter to `GetReceiptMaster`:
```csharp
where obmsBranches.Name == userID && obmsBranches.IsAllowed == true
```
Also handle superadmin (return all branches) as `GetPaymentMaster` does.

### Fix 4 — Bulk Attendance Upload (Frontend)
**File:** `d:\India Live\obms-ui\src\app\modules\payroll\attendance\bulk-attendance-upload\bulk-attendance-upload.component.ts`

1. Remove hardcoded `currentUser: string = 'admin'` and read from sessionStorage in `ngOnInit()`.
2. Inject `MastermoduleService` and change `loadBranches()` to call `GetBranchListByUserName`.

### Fix 5 — Mumbai missing from Bulk Attendance Upload
Verify the `BranchMasters` table has an active Mumbai entry (`Code=FWG009`). If it does exist, `GetBranchMasterAll` should return it. If it is absent after Fix 4 (when using filtered endpoint), then Mumbai's `OBMSBranches.IsAllowed` may be false for users — meaning it needs to be checked in User Branch Access for each affected user.

---

## Key Files Reference

| File | Role |
|---|---|
| `obms-ui/.../administration/user-branch-access/new-user-branch-access/new-user-branch-access.component.ts` | Admin UI to assign branches to users |
| `obms-ui/.../service/mastermodule.service.ts` | `GetBranchListByUserName()` — the correct filtered method |
| `obms-ui/.../service/employee-transfer.service.ts` | `getEmployees()` — calls unfiltered `GetBranchMasterAll` |
| `obms-ui/.../service/payrollmodule.service.ts` | `getBranchList()` — calls unfiltered `GetBranchMasterAll` |
| `obms-ui/.../modules/master/employee-transfer/employee-transfer.component.ts` | Bug: constructor branch load, no user filter |
| `obms-ui/.../modules/master/employee-history/employee-history.component.ts` | Bug: wrong property name `branchCode` vs `Code` |
| `obms-ui/.../modules/finance/receipts/new-receipt/new-receipt.component.ts` | Uses `GetReceiptMaster(currentUser)` — backend bug |
| `obms-ui/.../modules/payroll/attendance/bulk-attendance-upload/bulk-attendance-upload.component.ts` | Bug: hardcoded `admin`, unfiltered endpoint |
| `obms-api/Repositories/Implementation/MasterRepository.cs` | Correct `GetBranchListByUserName` implementation |
| `obms-api/Repositories/Implementation/FinanceRepository.cs` | `GetReceiptMaster` missing `IsAllowed==true` filter |
| `obms-api/Controllers/MasterController.cs` | `GetBranchMasterAll` (unfiltered) and `GetBranchListByUserName` (filtered) |
| `obms-api/Controllers/RegisterController.cs` | `GetObmsBranchesPermissionByUser`, `SaveAndUpdateObmsBranches` |
