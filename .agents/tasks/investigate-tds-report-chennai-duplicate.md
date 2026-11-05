# Investigation: TDS Report — Branch dropdown shows "Chennai" twice

## Summary answer

The Branch dropdown on the TDS Report page (`localhost:4200/#/report/finance/tds-report`) shows `FWG001-Chennai - Chennai` because the dropdown option template renders **both** the branch `Code` and the branch `Name`, and for the Chennai row the `Code` column itself already contains the name embedded in it (`FWG001-Chennai`) while the `Name` column is `Chennai`. Concatenating them with ` - ` produces the doubled "Chennai".

This is primarily a **frontend display issue**, not a data-corruption issue. The `Code` column of the Chennai branch legitimately stores the value `FWG001-Chennai` (name embedded in the code), and that same `Code` value is the canonical branch identifier stored across many transaction tables (`Receipts.Branch`, `CreditorInvoice.Branch`, `Employee.EMP_BRANCH_CODE`, `ClientMasters.Branch`, `OBMSBranches.BranchCode`). Changing the stored `Code` value in the database would break filtering of all historical records, so the correct fix is a one-line change to the dropdown's display label in the TDS Report template.

Note: the same doubling actually also affects the Coimbatore (`FWG006-Coimbatore - Coimbatore`) and Karnataka (`FWG007-Karnataka - Karnataka`) rows for the same reason; the user only flagged Chennai. Gujarat does not double because its `Code` suffix is an abbreviation (`FWG003-GUJ`) that differs from its `Name` (`Gujarat`).

---

## Evidence

### 1. Frontend dropdown template — the direct cause

File: `d:\India Live\obms-ui\src\app\modules\report\finance\tds-report\tds-report.component.html`
Line: **63** (inside the Branch `mat-select`)

```html
<mat-option value="">All Branches</mat-option>
<mat-option *ngFor="let d of branchList" value="{{d.Code}}">{{d.Code}} - {{d.Name}}</mat-option>
```

The option's *value* (sent to the backend as the branch filter) is `d.Code`. The option's *label* (what the user sees) is `{{d.Code}} - {{d.Name}}`. For a row where `Code = "FWG001-Chennai"` and `Name = "Chennai"`, the label renders as `FWG001-Chennai - Chennai`.

This is the only place that produces the visible text, so this is the precise location of the bug.

### 2. How branchList is populated

File: `d:\India Live\obms-ui\src\app\modules\report\finance\tds-report\tds-report.component.ts`
Lines: ~52-55 (inside `getUserAccessRights`)

```ts
this._masterService.GetBranchListByUserName(this.currentUser).subscribe((d: any) => {
  this.branchList = d;
});
```

Service file: `d:\India Live\obms-ui\src\app\service\mastermodule.service.ts`
Lines: 42-46

```ts
GetBranchListByUserName(userName: string): Observable<any> {
  return this.httpClient.get<BranchModel[]>(this.apiUrl + 'master/GetBranchListByUserName', {
    params: { userName: userName }
  })...
```

Model file: `d:\India Live\obms-ui\src\app\model\branchModel.ts`
Lines: 1-4 — `BranchModel` has distinct `Code!:string;` and `Name!:string;` fields. So `Code` and `Name` are two separate fields coming back from the API.

### 3. Backend endpoint — returns Code and Name straight from the table

Controller: `d:\India Live\obms-api\Controllers\MasterController.cs`
Lines: 112-126 — `[Route("GetBranchListByUserName")]` calls `_masterRepository.GetBranchListByUserName(userName)` and returns `Ok(branchList)`. No transformation of Code/Name.

Repository: `d:\India Live\obms-api\Repositories\Implementation\MasterRepository.cs`
Lines: 75-150 — `GetBranchListByUserName` reads from `BranchMasters` and projects `Code = x.Code` and `Name = x.Name` directly. There is **no** code that builds a "Code-Name" string or appends the name to the code. The doubling is therefore NOT introduced in the backend.

Domain model: `d:\India Live\obms-api\Models\Domain\BranchMaster.cs`
Lines: 7-15 — `BranchMaster` maps to table `BranchMaster` with separate `Code` and `Name` columns.

### 4. Why the stored Code contains the name (and why it must not be "fixed" in the DB)

The branch `Code` value selected in the dropdown (e.g., `FWG001-Chennai`) is passed as-is to the report APIs and used as an equality filter against transactional branch columns:

- `d:\India Live\obms-api\Controllers\FinanceController.cs` line ~6413 `GetTDSReport`: `AND r.Branch = '{branch}'` against the `Receipts` table.
- Same file line ~6470 `GetOutTDSReport`: `AND ci.Branch = '{branch}'` against `CreditorInvoice`.
- Same file line ~6509 `GetPayrollTDSReport`: `AND e.EMP_BRANCH_CODE = '{branch}'` against `Employee`.
- Same file line ~5060 `GetClientByBranch`: `ClientMasters...Where(x => x.Branch == branch)`.

This means the **full** code string `FWG001-Chennai` is the value actually stored in `Receipts.Branch`, `CreditorInvoice.Branch`, `Employee.EMP_BRANCH_CODE`, `ClientMasters.Branch`, and (per the repo join) `OBMSBranches.BranchCode`. Historical transaction rows already carry `FWG001-Chennai`. If the `BranchMaster.Code` were edited to just `FWG001`, the dropdown value would become `FWG001` and the equality filters would stop matching every existing `FWG001-Chennai` row — silently breaking the report and the client-by-branch lookup. A data fix would therefore require a coordinated cascading update across every referencing table, which is high-risk and out of scope for this display bug.

### 5. The doubling is a display-only artifact, confirmed by sibling reports

Across the `report` module, branch dropdowns use three different label conventions (value is always `d.Code`):

| Convention | Used in |
|---|---|
| `{{d.Name}}` (majority — no doubling) | pay-sheet, yearly-pay-sheet, general-ledger-report, bank-reconciliation-report, balance-sheet-report, accounting-profit-and-loss, bank-salary-statement-report, bank-salary-statement2-report, kkdn, client-master-report, branch-stock-ledger, epf-statement-report, etc. |
| `{{d.Code}}` (no doubling) | socso-statement-report, sip-statement-report, employee-list, branch-collections, branch-transactions, invoice-report, invoice-collection-status-report |
| `{{d.Code}} - {{d.Name}}` (the doubling pattern) | **tds-report** (line 63) and **gst-statement-report** (`gst-statement-report.component.html`, line ~59) |

Only the two reports using `{{d.Code}} - {{d.Name}}` exhibit the doubling, and only for branches whose `Code` suffix matches the `Name` (Chennai, Coimbatore, Karnataka). This confirms the bug is the label template, not the data.

---

## Conclusions

1. **Root cause (visible bug):** the TDS Report branch dropdown label is `{{d.Code}} - {{d.Name}}`. Because the Chennai row's `Code` is `FWG001-Chennai` (name embedded in the code) and its `Name` is `Chennai`, the rendered label is `FWG001-Chennai - Chennai`.
2. **Why not a pure data issue:** the embedded-name `Code` is the canonical branch identifier persisted across many transaction tables. Editing `BranchMaster.Code` would orphan all historical rows from the filters. The embedded-name code is an existing data-design characteristic of this system, shared by all branches; it is not corruption.
3. **Scope:** the same `{{d.Code}} - {{d.Name}}` label pattern exists in the GST statement report, which will show the identical doubling for Chennai/Coimbatore/Karnataka. Any fix should consider both for consistency.
4. The backend (`MasterController`/`MasterRepository`) is not at fault and needs no change.

## Recommendations (implement nothing)

Recommended fix — change only the display label, keep `d.Code` as the value so backend filtering is unaffected.

File: `d:\India Live\obms-ui\src\app\modules\report\finance\tds-report\tds-report.component.html`, line 63.

Change:
```html
<mat-option *ngFor="let d of branchList" value="{{d.Code}}">{{d.Code}} - {{d.Name}}</mat-option>
```
to (matches the majority convention used across the report module):
```html
<mat-option *ngFor="let d of branchList" value="{{d.Code}}">{{d.Name}}</mat-option>
```

This yields `Chennai`, `Gujarat`, `Coimbatore`, `Karnataka` — clean, no doubling — while the submitted form value remains `d.Code` (`FWG001-Chennai`), so all backend filters continue to match existing data.

If the code prefix must stay visible, the alternative is `{{d.Code}}` only (label = `FWG001-Chennai`), matching the invoice-report / branch-collections convention. Either option removes the doubling; `{{d.Name}}` is preferred for user-facing friendliness.

For consistency, apply the same label change to `d:\India Live\obms-ui\src\app\modules\report\payroll\gst-statement-report\gst-statement-report.component.html` (line ~59), which uses the identical `{{d.Code}} - {{d.Name}}` pattern.

No backend, database, or model changes are required.

## Items I could not verify (environment constraints)

- I did not query the live SQL database, so the exact stored value `FWG001-Chennai` for `BranchMaster.Code` is **inferred** from the rendered label (`{{d.Code}} - {{d.Name}}` → `FWG001-Chennai - Chennai` implies `Code = "FWG001-Chennai"` and `Name = "Chennai"`). The inference is deterministic given the template, but a direct `SELECT Code, Name FROM BranchMaster WHERE Code LIKE 'FWG001%'` query would confirm it.
- I did not run the Angular app to observe the dropdown live; the conclusion is based on static analysis of the template, service, controller, and repository.
