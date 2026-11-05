# Investigation: Add Client Filter to RBI Bank Salary Process

## Summary

The RBI Bank Salary Process page currently filters exported employees by **Salary Period + Branch + EmployeeType only** — there is no Client filter anywhere in the stack. Adding one is low-risk and well-supported by existing patterns:

- The UI already loads a branch list, and every sibling payroll/report screen loads a **Client dropdown off the selected branch** via `MastermoduleService.getClientMsterListByBranch(branchCode)` → `GET master/GetClientMsterListByBranch`. The RBI component already injects `MastermoduleService`, so no new dependency is needed.
- The backend export query joins `PaySlip → Employee`, and **`Employee.EMP_CLIENT` stores the employee's assigned client code** (FK to `ClientMaster.Code`). The current RBI query does NOT reference client, but the column is already there and is the same field used by the salary-processing engine (`SalaryProcess.cs` joins `ClientMaster cm ON cm.Code = Employee.EMP_CLIENT`).
- So the fix is: add a Client `<mat-select>` that loads after Branch selection, thread the selected client code through the 3 service calls, add a `client` parameter to the 3 controller methods + the repository + business object, and append `AND Employee.EMP_CLIENT = @Client` to the SQL `WHERE` clause when a specific client is chosen. The Excel/CSV/TXT export formats themselves need no change — they operate on the already-filtered list.

No stored procedure is involved; the query is inline SQL in `RbiBankSalaryExport.cs`. No `obms-reports-india` (Crystal `.aspx`) file participates — exports are generated API-side (CSV/TXT) and client-side (Excel via `xlsx`).

---

## Answers to the investigation questions

### 1. Exact file paths of the Angular component

- TS: `d:\India Live\obms-ui\src\app\modules\payroll\rbi-bank-salary-process\rbi-bank-salary-process.component.ts`
- HTML: `d:\India Live\obms-ui\src\app\modules\payroll\rbi-bank-salary-process\rbi-bank-salary-process.component.html`
- CSS: `d:\India Live\obms-ui\src\app\modules\payroll\rbi-bank-salary-process\rbi-bank-salary-process.component.css`
- Module: `d:\India Live\obms-ui\src\app\modules\payroll\rbi-bank-salary-process\rbi-bank-salary-process.module.ts`
- Frontend model: `d:\India Live\obms-ui\src\app\model\RbiBankSalaryExport.ts`

### 2. Parameters the component currently sends to the API

The form group defines four controls (`rbi-bank-salary-process.component.ts`, constructor):
```
Branch        // Validators.required  -> branch code, e.g. "MUM"
Period        // Validators.required  -> datepicker -> formatted to last day of month, e.g. "2025-07-31"
EmployeeType  // "ALL" default        -> "Guard" | "Staff" | "ALL"
exportOption  // "0" default         -> 0 = current page, 1 = all records (Excel paging only, NOT sent to API)
```

Only three values are sent to the API: `dtSalaryPeriod`, `branch`, `employeeType`. No client value is sent.

- `onSearchClick()` computes `dtPeriod` (last day of month) and calls `getRbiBankSalaryExportData(this.dtPeriod, branch, employeeType)`.
- `onExportCsvClick()` / `onExportTxtClick()` send `this.dtPeriod, branch, employeeType` directly (independent of the table data).

### 3. API endpoint + controller method signatures

Controller: `d:\India Live\obms-api\Controllers\PayrollController.cs`, class route `[Route("api/[controller]")]` → base `api/payroll`. `apiUrl = environment.baseUrl` in the service, and calls use `${this.apiUrl}payroll/...`.

Three endpoints (all `[HttpGet]`), all currently taking only `dtSalaryPeriod, branch, employeeType`:

- `GET api/payroll/GetRbiBankSalaryExport` → `GetRbiBankSalaryExport(string dtSalaryPeriod, string branch = "ALL", string employeeType = "ALL")` → returns `Ok(result)`.
- `GET api/payroll/GetRbiBankSalaryExportCsv` → same signature → returns `File(csvBytes, "text/csv", fileName)`.
- `GET api/payroll/GetRbiBankSalaryExportTxt` → same signature → returns `File(txtBytes, "text/plain", fileName)`.

Repository interface/impl: `IPayrollRepository.GetRbiBankSalaryExportData(string dtSalaryPeriod, string branch, string employeeType)` and `PayrollRepository.GetRbiBankSalaryExportData(...)` which simply delegates to `RbiBankSalaryExport.GetRbiBankSalaryExportData(dtSalaryPeriod, branch, employeeType)` (`PayrollRepository.cs` ~line 4209).

### 4. SQL query / stored procedure — does it support client filtering?

There is **no stored procedure**. The query is inline parameterised SQL in `RbiBankSalaryExport.GetRbiBankSalaryExportData(...)` (`obms-api\BusinessObjects\RbiBankSalaryExport.cs`).

Core query:
```sql
SELECT Employee.EMP_CODE as BeneficiaryCode,
       ISNULL(NULLIF(ESD.EMPFL_BK_ACCNO,''), Employee.BankAccountNumber) as BeneficiaryAccountNumber,
       Employee.EMP_NAME as BeneficiaryName,
       PaySlip.NetPay + ISNULL(MiscAmt.TotalMiscAmount,0) as TransactionAmount,
       Employee.EMP_ID as CustomerReferenceNumber,
       ...
FROM PaySlip
INNER JOIN Employee ON PaySlip.EmployeeID = Employee.EMP_ID
LEFT JOIN EmployeeSalaryDetails ESD ON ESD.EMPFL_CODE = Employee.EMP_CODE
LEFT JOIN BranchMaster BankMaster ON Employee.EMP_BRANCH_CODE = BankMaster.Code
LEFT JOIN ( ... MiscTrans aggregate ... ) MiscAmt ON MiscAmt.EmployeeID = Employee.EMP_ID
```

WHERE clause (built dynamically):
```sql
WHERE PaySlip.Period = @SalaryPeriod
  AND (ESD.PAYMODE = 'Bank' OR (ESD.PAYMODE IS NULL AND Employee.BankAccountNumber IS NOT NULL AND Employee.BankAccountNumber <> ''))
  [ AND ISNULL(ESD.EMPFL_BRANCHCODE, Employee.EMP_BRANCH_CODE) = @Branch ]   -- only if branch != ALL
  [ AND Employee.EMP_ROLE = @EmployeeType ]                                   -- only if employeeType != ALL
ORDER BY Employee.EMP_NAME
```

**It does NOT currently support client filtering** — there is no `ClientMaster` join and no `EMP_CLIENT` predicate. Parameters are added conditionally with `cmd.Parameters.AddWithValue` only when the value is non-empty/non-`ALL`. The existing conditional-parameter pattern is exactly the pattern a new `client` parameter should follow.

### 5. How Client master data is fetched in other pages

The dominant, consistent pattern across payroll/report screens: on Branch selection, call `MastermoduleService.getClientMsterListByBranch(branchCode)` and bind the result to a `clientModel: ClientModel[]` used by a Client `<mat-select>`. Evidence (all use the same call):

- Service: `mastermodule.service.ts` `getClientMsterListByBranch(branchCode)` → `GET master/GetClientMsterListByBranch?branchCode=<code>`.
- API: `MasterController.GetClientMsterListByBranch(string branchCode)` → `_masterRepository.GetClientMsterListByBranch(branchCode)`.
- Repo: `MasterRepository.GetClientMsterListByBranch(branchCode)` → `_oBMSDbContext.ClientMasters.Where(x => x.Branch == branchCode).OrderBy(x => x.Name)` — i.e. **clients are children of a branch** (`ClientMaster.Branch == branchCode`). This is why the client dropdown is loaded after branch selection.
- Consumers (representative): `professional-tax-statement-report`, `pf-statement-report`, `esi-statement-report`, `statutory-compliance-report`, `gst-statement-report`, `non-compliance-report`, `client-compliance-report`, `checkrole-report`, finance `debit-note`/`credit-note`, etc.

Typical TS handler:
```ts
onBranchSelectionChange(event: any) {
  const branchCode = this.frm.get('BranchCode')?.value;
  if (branchCode) this.getClientListByBranch(branchCode);
}
getClientListByBranch(branchCode: string) {
  this._masterService.getClientMsterListByBranch(branchCode).subscribe((data: ClientModel[]) => {
    this.clientModel = data;
  });
}
```

Typical HTML (from `professional-tax-statement-report.component.html`):
```html
<mat-form-field appearance="outline" class="w-100">
  <mat-label>Client (Optional)</mat-label>
  <mat-select formControlName="ClientCode">
    <mat-option value="">All Clients</mat-option>
    <mat-option *ngFor="let client of clientModel" [value]="client.Code">{{ client.Name }}</mat-option>
  </mat-select>
</mat-form-field>
```

`ClientModel` lives at `d:\India Live\obms-ui\src\app\model\clientModel.ts` (has `Code` and `Name`). The RBI component already injects `MastermoduleService` as `_masterService` and already calls `_masterService.GetBranchListByUserName(...)` for the branch list — so the wiring is already in place; only a new `clientModel` field + handler + `<mat-select>` are needed.

### 6. Text file export format (`RbiBankSalaryExport.cs`)

The TXT export (`GenerateTxtExport` / `GenerateTxtExportBytes`) produces the **Indian Bank Net Banking portal upload format**: 28 comma-separated fields per record, **no header row**, `CRLF` line endings, **UTF-8 without BOM**. Field map (Excel columns B–AC):

1 `N` (NEFT) · 2 `FREWA` (payment product code, hard-coded) · 3 BeneficiaryAccountNumber · 4 TransactionAmount (whole number, `FormatTxtAmount` truncates decimals) · 5 BeneficiaryName · 6–13 empty (8) · 14 Narration (`"Salary " + MonthName`) · 15–22 empty (8) · 23 ValueDate `DD/MM/YYYY` · 24 empty · 25 IFSCCode · 26–27 empty · 28 BeneficiaryEmailId (hard-coded `singam@fwg.my`).

`EscapeField` quotes fields containing comma/quote/newline. `SanitizePrintLocation` strips non-alphanumerics and truncates to 20 chars for the bank-name portal field. This format is purely a transform of the already-filtered `List<RbiBankSalaryExport>` — **it needs no change for client filtering**; it will naturally contain only the filtered employees once the query filters upstream.

There is also `GenerateCsvExport` (CSV with a header row of RBI-spec field names) and the Angular `exportToExcel` (client-side `xlsx`, with styled header/data-type/length/mandatory rows). Both likewise operate on the filtered dataset and need no structural change.

### 7. Branch ↔ Client relationship in the DB

`ClientMaster.Branch` holds the branch code (`MasterRepository.GetClientMsterListByBranch` filters `x.Branch == branchCode`), so **a Client belongs to a Branch**. The RBI screen already requires a Branch (form `Validators.required`), so loading clients off the selected branch is the correct and consistent approach. There is no "All Branches" option in the current RBI branch `<mat-select>`, so a branch is always chosen before clients load.

### 8. Do the existing table columns already link to client?

The visible table columns — Beneficiary Code (`Employee.EMP_CODE`), Account No, Transaction Amount (`PaySlip.NetPay + MiscAmount`), Beneficiary Name (`Employee.EMP_NAME`), IFSC, Bank Name — are employee-bank fields. **None of the displayed columns is the client**, but the underlying row's `Employee.EMP_ID` maps to `Employee.EMP_CLIENT`, so each exported row already implicitly belongs to a client. Filtering by `Employee.EMP_CLIENT` therefore directly narrows the exported employee set to the chosen client (e.g. "Anejas"), which is exactly what the user requested.

Confirmed entity fields:
- `d:\India Live\obms-api\Models\Domain\Employee.cs` line ~132: `public string? EMP_CLIENT { get; set; }` — client code, FK to `ClientMaster.Code`.
- `PaySlip.cs` has **no** Client column — so client filtering must go through the `Employee` join (already present in the query), not PaySlip.
- `SalaryProcess.cs` already joins `ClientMaster cm ON cm.Code = Employee.EMP_CLIENT`, confirming the canonical join key.

---

## Conclusions & recommended fix (implement nothing — recommendation only)

The change is additive and follows existing conventions end-to-end. Recommended implementation:

**Frontend — `rbi-bank-salary-process.component.ts` / `.html`**
1. Add `clientModel: ClientModel[] = [];` and a `Client` form control (default `""` = All Clients). Import `ClientModel` from `src/app/model/clientModel`.
2. In `onBranchSelectionChange` (already wired to the Branch `<mat-select>`), after computing the period, call `_masterService.getClientMsterListByBranch(branch)` and assign to `clientModel`; reset `Client` to `""` on branch change (matches sibling pattern).
3. In `onSearchClick`, `onExportCsvClick`, `onExportTxtClick`: read the `Client` control and pass it through.
4. Add the Client `<mat-select>` in the HTML (same row as Branch), with an `All Clients` empty option, following the `professional-tax-statement-report` markup.

**Frontend service — `payrollmodule.service.ts`**
5. Add a `client` parameter to `getRbiBankSalaryExport`, `getRbiBankSalaryExportCsv`, `getRbiBankSalaryExportTxt` and append `.set('client', encodeURIComponent(client ?? ''))` to each `HttpParams`. (Skip setting it when empty to keep `ALL` semantics, or send empty string — backend treats empty/`ALL` as no filter.)

**Backend controller — `PayrollController.cs`**
6. Add `string client = "ALL"` to the three method signatures (`GetRbiBankSalaryExport`, `GetRbiBankSalaryExportCsv`, `GetRbiBankSalaryExportTxt`) and pass it to `_payrollRepository.GetRbiBankSalaryExportData(...)`.

**Backend repository — `IPayrollRepository.cs` / `PayrollRepository.cs`**
7. Add `string client` to the interface method and the implementation, and forward it to `RbiBankSalaryExport.GetRbiBankSalaryExportData(...)`.

**Backend business object — `RbiBankSalaryExport.cs`**
8. Add `string client` to `GetRbiBankSalaryExportData(...)`. In the WHERE-clause builder, after the existing branch/employeeType blocks, add:
   ```csharp
   if (!string.IsNullOrEmpty(client) && client != "ALL")
   {
       whereClause += " AND Employee.EMP_CLIENT = @Client ";
   }
   ```
   and add the parameter conditionally (mirroring the existing branch/employeeType parameter pattern):
   ```csharp
   if (!string.IsNullOrEmpty(client) && client != "ALL")
   {
       cmd.Parameters.AddWithValue("@Client", client);
   }
   ```

No DB migration is required — `Employee.EMP_CLIENT` already exists. No report (`.aspx`) file is involved.

### Notes / edge cases to verify during implementation
- **Multi-client employees / attendance-driven client**: `SalaryProcess.cs` and `vwPaySheet` derive the *paying* client from `AttendanceDetails.Client` (per-day), so an employee can be paid by multiple clients in a month. `Employee.EMP_CLIENT` is the *master/home* client. For RBI Bank Salary export — which is one bank transfer per employee per month off `PaySlip.NetPay` — filtering by the master `Employee.EMP_CLIENT` is the correct, expected behaviour ("show me this client's employees"). If per-day client attribution is ever required instead, that would be a larger change using attendance; it is not needed for this request.
- **Sibling screen**: `GetRbiBankAdvanceExport` / `RbiBankAdvanceExport` (salary *advance* variant) has the identical signature and likely the same gap. Out of scope for this request, but flag it for a parallel fix if advances should also be client-filterable.
- The Excel export is client-side off the paged/filtered `dataSource`; the CSV/TXT exports re-query the API with the same parameters. Ensure the `Client` value is passed in **all three** code paths so Excel, CSV, and TXT stay consistent.
