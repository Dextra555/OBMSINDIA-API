# RBI Bank Salary Process — Complete Analysis & Client Filter Implementation Plan

Generated: investigation step of workflow `wf_b4ec2ac5e687cc6e`

---

## EXECUTIVE SUMMARY

The RBI Bank Salary Process module spans **4 Angular files, 3 API controller actions, 1 repository interface + implementation, 1 business object (containing inline SQL), and 3 SQL tables** — no stored procedures are involved. Adding a Client Name dropdown filter requires changes in exactly **6 files** and approximately **40 lines of code**. The `Employee.EMP_CLIENT` column (already in the DB schema) is the correct join key. The `MastermoduleService.getClientMsterListByBranch()` service already exists and is already injected into the component. No database migration is required.

---

## 1. ANGULAR FRONTEND

### 1.1 Component files

| File | Full Path |
|------|-----------|
| Component TypeScript | `d:\India Live\obms-ui\src\app\modules\payroll\rbi-bank-salary-process\rbi-bank-salary-process.component.ts` |
| Component HTML | `d:\India Live\obms-ui\src\app\modules\payroll\rbi-bank-salary-process\rbi-bank-salary-process.component.html` |
| Component CSS | `d:\India Live\obms-ui\src\app\modules\payroll\rbi-bank-salary-process\rbi-bank-salary-process.component.css` |
| Module | `d:\India Live\obms-ui\src\app\modules\payroll\rbi-bank-salary-process\rbi-bank-salary-process.module.ts` |
| Frontend model | `d:\India Live\obms-ui\src\app\model\RbiBankSalaryExport.ts` |

### 1.2 Existing filters

The `rbiBankSalaryForm` FormGroup (defined in constructor) has four controls:
```
Branch        // Validators.required  — bound to <mat-select> loading from GetBranchListByUserName
Period        // Validators.required  — bound to <mat-datepicker>
EmployeeType  // default "ALL"        — bound to radio group (Guard | Staff | ALL)
exportOption  // default "0"          — 0 = current page, 1 = all records (Excel paging only)
```

**There is NO Client dropdown** anywhere in the component.

### 1.3 Search button → API call chain

`onSearchClick()` (TS line ~131):
1. Reads `Period` control → computes `dtPeriod` as the last day of the selected month (`formatDate(new Date(year, month+1, 0))`).
2. Reads `Branch` and `EmployeeType` controls.
3. Calls `getRbiBankSalaryExportData(dtPeriod, branch, employeeType)`.
4. That calls `_payrollService.getRbiBankSalaryExport(dtSalaryPeriod, branch, employeeType)`.

Service method (`payrollmodule.service.ts` line ~715):
```typescript
getRbiBankSalaryExport(dtSalaryPeriod: string, branch: string, employeeType: string): Observable<any[]> {
  const params = new HttpParams()
    .set('dtSalaryPeriod', encodeURIComponent(dtSalaryPeriod))
    .set('branch', encodeURIComponent(branch))
    .set('employeeType', encodeURIComponent(employeeType));
  return this.httpClient.get<any[]>(`${this.apiUrl}payroll/GetRbiBankSalaryExport`, { params })
    .pipe(catchError(this.errorHandle));
}
```
→ `GET {baseUrl}payroll/GetRbiBankSalaryExport?dtSalaryPeriod=…&branch=…&employeeType=…`

### 1.4 Export methods

| Button | Handler | Service method | API Endpoint |
|--------|---------|----------------|-------------|
| Export to Excel | `onExportClick()` | None (client-side `xlsx`) | No API call — uses `this.dataSource.filteredData` |
| Export to CSV | `onExportCsvClick()` | `getRbiBankSalaryExportCsv(dtPeriod, branch, employeeType)` | `GET payroll/GetRbiBankSalaryExportCsv` |
| Export to Text File | `onExportTxtClick()` | `getRbiBankSalaryExportTxt(dtPeriod, branch, employeeType)` | `GET payroll/GetRbiBankSalaryExportTxt` |

**Critical**: CSV and TXT exports re-query the API independently with the same three parameters. Excel operates on the already-loaded `dataSource.filteredData`. All three must receive the new `client` parameter to stay consistent.

### 1.5 Client dropdown — status

**Not present.** No `clientModel` array, no `Client` form control, no client-related service call in the component.

### 1.6 Branch population

In `getUserAccessRights()` (after access is confirmed):
```typescript
this._masterService.GetBranchListByUserName(this.currentUser).subscribe((d: any) => {
  this.branchList = d;
});
```
→ `GET master/GetBranchListByUserName?userName={currentUser}`  
Binds to `branchList: any[]`; the HTML `<mat-select>` uses `d.Code` / `d.Name`.

### 1.7 Existing ClientService / service method

`MastermoduleService` (`d:\India Live\obms-ui\src\app\service\mastermodule.service.ts`) already has:

```typescript
getClientMsterListByBranch(branchCode: string): Observable<any> {
  const params = { params: new HttpParams({ fromString: "?branchCode=" + branchCode }) };
  return this.httpClient.get<any[]>(this.apiUrl + 'master/GetClientMsterListByBranch', params)
    .pipe(catchError(this.errorHandle));
}
```

The RBI component already injects `MastermoduleService` as `_masterService`. No new service or import is needed — just a new method call.

---

## 2. .NET API BACKEND

### 2.1 Controller actions

File: `d:\India Live\obms-api\Controllers\PayrollController.cs`, class route `[Route("api/[controller]")]`

All three actions are `[HttpGet]` and currently accept only `dtSalaryPeriod, branch, employeeType`:

```csharp
// Line ~2926
[HttpGet, Route("GetRbiBankSalaryExport")]
public IActionResult GetRbiBankSalaryExport(
    string dtSalaryPeriod, string branch = "ALL", string employeeType = "ALL")
{
    var result = _payrollRepository.GetRbiBankSalaryExportData(dtSalaryPeriod, branch, employeeType);
    return Ok(result);
}

// Line ~2954
[HttpGet, Route("GetRbiBankSalaryExportCsv")]
public IActionResult GetRbiBankSalaryExportCsv(
    string dtSalaryPeriod, string branch = "ALL", string employeeType = "ALL")
{
    var exportData = _payrollRepository.GetRbiBankSalaryExportData(dtSalaryPeriod, branch, employeeType);
    var csvBytes = RbiBankSalaryExport.GenerateCsvExportBytes(exportData);
    var fileName = $"RBI_Bank_Salary_Export_{dtSalaryPeriod}_{branch}_{employeeType}.csv";
    return File(csvBytes, "text/csv", fileName);
}

// Line ~2990
[HttpGet, Route("GetRbiBankSalaryExportTxt")]
public IActionResult GetRbiBankSalaryExportTxt(
    string dtSalaryPeriod, string branch = "ALL", string employeeType = "ALL")
{
    var exportData = _payrollRepository.GetRbiBankSalaryExportData(dtSalaryPeriod, branch, employeeType);
    var txtBytes = RbiBankSalaryExport.GenerateTxtExportBytes(exportData);
    var fileName = $"RBI_Bank_Salary_Export_{dtSalaryPeriod}_{branch}_{employeeType}.txt";
    return File(txtBytes, "text/plain", fileName);
}
```

None of them accept a `client` parameter yet.

### 2.2 Repository/service called

All three controller actions call `_payrollRepository.GetRbiBankSalaryExportData(...)`, which is the `IPayrollRepository` interface. The `PayrollController` constructor injects `IPayrollRepository _payrollRepository`.

### 2.3 `RbiBankSalaryExport.cs`

Full path: `d:\India Live\obms-api\BusinessObjects\RbiBankSalaryExport.cs`

Key fields (all string properties):
- `FieldType`, `TransactionType`, `BeneficiaryCode`, `BeneficiaryAccountNumber`, `TransactionAmount`
- `BeneficiaryName`, `CustomerReferenceNumber`, `PayerAccountNo`, `PayerName`
- `PayerAddress1`–`PayerAddress10`, `ChargeBearer`, `ValueDate`
- `IFSCCode`, `BeneficiaryBankName`, `BeneficiaryBankBranchName`, `BeneficiaryEmailId`
- `Narration` (e.g. `"Salary July"`)

The static `GetRbiBankSalaryExportData(...)` method contains the inline parameterised SQL. Three static export methods: `GenerateCsvExportBytes`, `GenerateTxtExportBytes`, `GenerateTxtExport`.

---

## 3. REPOSITORY / DATA ACCESS

### 3.1 Interface

File: `d:\India Live\obms-api\Repositories\Interface\IPayrollRepository.cs` (line ~73):
```csharp
List<BusinessObjects.RbiBankSalaryExport> GetRbiBankSalaryExportData(
    string dtSalaryPeriod, string branch, string employeeType);
```

### 3.2 Implementation

File: `d:\India Live\obms-api\Repositories\Implementation\PayrollRepository.cs` (~line 4209):
```csharp
#region RBI Bank Salary Export
public List<RbiBankSalaryExport> GetRbiBankSalaryExportData(
    string dtSalaryPeriod, string branch, string employeeType)
{
    try
    {
        var businessData = RbiBankSalaryExport.GetRbiBankSalaryExportData(
            dtSalaryPeriod, branch, employeeType);
        return businessData;
    }
    catch (Exception) { throw; }
}
```

This is a pure pass-through to the Business Object. No logic lives here — changes to this method are purely signature updates.

### 3.3 Stored procedures

**None.** The query is entirely inline parameterised SQL inside `RbiBankSalaryExport.GetRbiBankSalaryExportData()` in the Business Object. No SP definition to find or modify.

### 3.4 Does existing query accept ClientId/ClientName?

**No.** The current WHERE clause only supports `SalaryPeriod`, `Branch`, and `EmployeeType`. There is no `ClientId`, `ClientCode`, or `EMP_CLIENT` predicate.

---

## 4. SQL / DATABASE

### 4.1 Tables involved

| Table | Role in Query |
|-------|---------------|
| `PaySlip` | Primary table — one row per employee per salary period; provides `Period`, `NetPay` |
| `Employee` | INNER JOINed via `PaySlip.EmployeeID = Employee.EMP_ID`; provides `EMP_CODE`, `EMP_NAME`, `EMP_BRANCH_CODE`, `EMP_ROLE`, `BankAccountNumber`, `IFSC_Code`, `BankName`, `BankBranch`, **`EMP_CLIENT`** |
| `EmployeeSalaryDetails` (alias `ESD`) | LEFT JOINed via `ESD.EMPFL_CODE = Employee.EMP_CODE`; provides `EMPFL_BK_ACCNO`, `EMPFL_BRANCHCODE`, `PAYMODE` |
| `BranchMaster` (alias `BankMaster`) | LEFT JOINed via `Employee.EMP_BRANCH_CODE = BankMaster.Code`; provides payer bank details |
| `MiscTrans` | Sub-query aggregate — LEFT JOINed; adds monthly misc amount to `TransactionAmount` |

### 4.2 Client–Employee relationship

The canonical relationship is `Employee.EMP_CLIENT` (a `string?`, max 10 chars) = `ClientMaster.Code`.

- `Employee.cs` line ~132: `public string? EMP_CLIENT { get; set; }` — stores the **client code** the employee is assigned to.
- `ClientMaster.cs`: `public string Code { get; set; }` and `public string Name { get; set; }` — the code is the FK target.
- Confirmed usage: `SalaryProcess.cs` joins `ClientMaster cm ON cm.Code = Employee.EMP_CLIENT` for salary calculation.
- Confirmed usage: `MasterRepository.GetClientMsterListByBranch` filters `ClientMaster.Branch == branchCode`, so clients are children of branches.

There is no `ClientEmployee` linking table — `EMP_CLIENT` on the `Employee` row is the direct FK. One employee → one client.

**Agreement table** (`Agreement.cs`) links `Branch + Client` codes to define service contracts, but it is not joined in the RBI salary query and is not the right table for this filter.

### 4.3 Full current WHERE clause (from `RbiBankSalaryExport.cs`)

```sql
WHERE PaySlip.Period = @SalaryPeriod
  AND (ESD.PAYMODE = 'Bank'
    OR (ESD.PAYMODE IS NULL
        AND Employee.BankAccountNumber IS NOT NULL
        AND Employee.BankAccountNumber <> ''))
  [ AND ISNULL(ESD.EMPFL_BRANCHCODE, Employee.EMP_BRANCH_CODE) = @Branch ]  -- added if branch != ALL
  [ AND Employee.EMP_ROLE = @EmployeeType ]                                  -- added if employeeType != ALL
ORDER BY Employee.EMP_NAME
```

The new client predicate to add:
```sql
[ AND Employee.EMP_CLIENT = @Client ]   -- added if client != ALL and client is non-empty
```

No additional JOIN is needed — `Employee` is already in the query.

---

## 5. CLIENT MASTER API

### 5.1 Backend endpoints

**MasterController** (`d:\India Live\obms-api\Controllers\MasterController.cs`):

| Route | Parameters | Returns |
|-------|-----------|---------|
| `GET master/GetClientMsterListByBranch` | `branchCode: string` | `List<ClientMaster>` filtered by `ClientMaster.Branch == branchCode` |
| `GET master/GetClientMsterListByStatus` | `status: string` | `List<ClientMaster>` filtered by status |
| `GET master/GetClientMsterList` | `clientCode, status` | Single client or all by status |

**Recommended endpoint for this feature:** `GET master/GetClientMsterListByBranch?branchCode={code}` — because the RBI screen already requires a Branch selection, and clients belong to branches.

**ClientController** (`d:\India Live\obms-api\Controllers\ClientController.cs`):

| Route | Parameters | Returns |
|-------|-----------|---------|
| `GET api/client/GetClientsByBranch` | `branchCode: string` | `List<ClientMaster>` |
| `GET api/client/GetClientByCode` | `clientCode: string` | `ClientMaster` |

Either works; `master/GetClientMsterListByBranch` is what all other payroll screens use.

### 5.2 Response shape (ClientMaster / ClientMasterRequestDto)

Key fields:
- `ID` (int) — database PK
- `Code` (string) — **the client identifier used everywhere as FK** (e.g. `"ANEJAS"`)
- `Name` (string) — **the human-readable name** (e.g. `"Anejas Security Services"`)
- `Branch` (string) — which branch this client belongs to
- `Status` (string) — `"Active"` / `"A"` / `"Inactive"`
- `Shortname` (string) — optional abbreviation

For the dropdown, bind `[value]="client.Code"` and display `{{ client.Name }}`.

### 5.3 Angular service

`MastermoduleService.getClientMsterListByBranch(branchCode)` in `mastermodule.service.ts`:
```typescript
getClientMsterListByBranch(branchCode: string): Observable<any> {
  const params = { params: new HttpParams({ fromString: "?branchCode=" + branchCode }) };
  return this.httpClient.get<any[]>(this.apiUrl + 'master/GetClientMsterListByBranch', params)
    .pipe(catchError(this.errorHandle));
}
```

Already injected in `RbiBankSalaryProcessComponent` as `_masterService`. No new service, no new import needed.

The `ClientModel` TypeScript class is at `d:\India Live\obms-ui\src\app\model\clientModel.ts`:
- `Code: string` — the value to send to the API
- `Name: string` — the label to display in the dropdown

---

## 6. MODELS / DTOs

### 6.1 `RbiBankSalaryExport` (C# Business Object)

File: `d:\India Live\obms-api\BusinessObjects\RbiBankSalaryExport.cs`

Full field list mapped from SQL query results:
- `BeneficiaryCode` ← `Employee.EMP_CODE`
- `BeneficiaryAccountNumber` ← `ISNULL(NULLIF(ESD.EMPFL_BK_ACCNO,''), Employee.BankAccountNumber)`
- `BeneficiaryName` ← `Employee.EMP_NAME`
- `TransactionAmount` ← `PaySlip.NetPay + ISNULL(MiscAmt.TotalMiscAmount, 0)` (rounded to integer)
- `CustomerReferenceNumber` ← `Employee.EMP_ID`
- `IFSCCode` ← `ISNULL(NULLIF(Employee.IFSC_Code,''), ISNULL(NULLIF(Employee.BankIFSC,''),''))`
- `BeneficiaryBankName` ← `ISNULL(NULLIF(Employee.BankName,''), BankMaster.Name)`
- `BeneficiaryBankBranchName` ← `ISNULL(Employee.BankBranch,'')`
- `BeneficiaryEmailId` ← hard-coded `singam@fwg.my`
- `PayerAccountNo` ← `ISNULL(BankMaster.BankAccount,'')`
- `PayerName` ← `ISNULL(BankMaster.Name,'')`
- `PayerAddress1` ← `ISNULL(ESD.EMPFL_BRANCHCODE, Employee.EMP_BRANCH_CODE)` (branch code)
- `PayerAddress2–6` ← `BankMaster.Address1`, `.Address2`, `.City`, `.State`, `.PostCode`
- `FieldType` ← hard-coded `"N"` (NEFT)
- `TransactionType` ← hard-coded `"SALARY"`
- `ChargeBearer` ← hard-coded `"OUR"`
- `ValueDate` ← `DateTime.Now.ToString("dd/MM/yyyy")`
- `Narration` ← `"Salary " + selectedDate.ToString("MMMM")` (e.g. `"Salary July"`)

### 6.2 `RbiBankSalaryExport` (TypeScript model)

File: `d:\India Live\obms-ui\src\app\model\RbiBankSalaryExport.ts`

Matches C# shape — same field names, all `string`. Uses `setExportData(data)` → `Object.assign(this, data)` to populate from the API JSON.

---

## 7. CONCLUSIONS & IMPLEMENTATION PLAN

### Files to change (6 files)

| # | File | Change |
|---|------|--------|
| 1 | `rbi-bank-salary-process.component.ts` | Add `clientModel`, `Client` form control, `onBranchSelectionChange` update, pass `client` to all 3 service calls |
| 2 | `rbi-bank-salary-process.component.html` | Add `<mat-select>` for Client after Branch field |
| 3 | `payrollmodule.service.ts` | Add `client` param to 3 service methods |
| 4 | `PayrollController.cs` | Add `string client = "ALL"` to 3 action signatures; pass to repository |
| 5 | `IPayrollRepository.cs` + `PayrollRepository.cs` (Implementation) | Add `string client` to interface signature and pass-through implementation |
| 6 | `RbiBankSalaryExport.cs` | Add `string client` param; append `AND Employee.EMP_CLIENT = @Client` to WHERE clause conditionally |

### No changes needed

- `rbi-bank-salary-process.component.css` — no styling changes
- `rbi-bank-salary-process.module.ts` — no new module imports needed
- Any `.aspx` report files — exports are API-side (CSV/TXT) and client-side (Excel); no Crystal Reports involved
- Any SQL migration scripts — `Employee.EMP_CLIENT` already exists in the DB schema

### Detailed change specifications

**File 1: `rbi-bank-salary-process.component.ts`**
```typescript
// Add import
import { ClientModel } from 'src/app/model/clientModel';

// Add property
clientModel: ClientModel[] = [];

// Add 'Client' to FormGroup in constructor
this.rbiBankSalaryForm = fb.group({
  Branch: ["", Validators.required],
  Period: ["", Validators.required],
  EmployeeType: ["ALL", Validators.required],
  Client: [""],   // NEW — default empty = All Clients
  exportOption: ['0'],
});

// Update onBranchSelectionChange to load clients
onBranchSelectionChange(event: any) {
  let dtPeriodDate = new Date(this.rbiBankSalaryForm.value.Period);
  this.dtPeriod = this.formatDate(
    new Date(dtPeriodDate.getFullYear(), dtPeriodDate.getMonth() + 1, 0)
  );
  // NEW: load clients for the selected branch
  const branch = this.rbiBankSalaryForm.get('Branch')?.value;
  if (branch) {
    this._masterService.getClientMsterListByBranch(branch).subscribe((data: ClientModel[]) => {
      this.clientModel = data;
      this.rbiBankSalaryForm.get('Client')?.setValue('');  // reset on branch change
    });
  }
}

// Update onSearchClick to pass client
onSearchClick() {
  const period = new Date(this.rbiBankSalaryForm.get('Period')?.value);
  this.dtPeriod = this.formatDate(
    new Date(period.getFullYear(), period.getMonth() + 1, 0)
  );
  const branch = this.rbiBankSalaryForm.get('Branch')?.value;
  const employeeType = this.rbiBankSalaryForm.get('EmployeeType')?.value;
  const client = this.rbiBankSalaryForm.get('Client')?.value || '';   // NEW
  if (this.dtPeriod !== '' && this.dtPeriod !== null) {
    this.getRbiBankSalaryExportData(this.dtPeriod, branch, employeeType, client);  // NEW param
  }
}

// Update getRbiBankSalaryExportData signature and service call
getRbiBankSalaryExportData(dtSalaryPeriod: string, branch: string, employeeType: string, client: string): void {
  this.showLoadingSpinner = true;
  this._payrollService.getRbiBankSalaryExport(dtSalaryPeriod, branch, employeeType, client).subscribe({ ... });
}

// Update onExportCsvClick and onExportTxtClick to pass client
const client = this.rbiBankSalaryForm.get('Client')?.value || '';
this._payrollService.getRbiBankSalaryExportCsv(this.dtPeriod, branch, employeeType, client).subscribe(...)
this._payrollService.getRbiBankSalaryExportTxt(this.dtPeriod, branch, employeeType, client).subscribe(...)
```

**File 2: `rbi-bank-salary-process.component.html`**
Add after the Branch `<mat-form-field>` (same row, `col-lg-4`):
```html
<div class="col-12 col-sm-12 col-md-4 col-lg-4">
  <mat-form-field appearance="outline">
    <mat-label>Client</mat-label>
    <mat-select formControlName="Client">
      <mat-option value="">All Clients</mat-option>
      <mat-option *ngFor="let c of clientModel" [value]="c.Code">{{ c.Name }}</mat-option>
    </mat-select>
  </mat-form-field>
</div>
```

**File 3: `payrollmodule.service.ts`** — add `client: string` to all 3 methods:
```typescript
getRbiBankSalaryExport(dtSalaryPeriod: string, branch: string, employeeType: string, client: string): Observable<any[]> {
  const params = new HttpParams()
    .set('dtSalaryPeriod', encodeURIComponent(dtSalaryPeriod))
    .set('branch', encodeURIComponent(branch))
    .set('employeeType', encodeURIComponent(employeeType))
    .set('client', encodeURIComponent(client ?? ''));   // NEW
  return this.httpClient.get<any[]>(`${this.apiUrl}payroll/GetRbiBankSalaryExport`, { params })
    .pipe(catchError(this.errorHandle));
}
// Same pattern for getRbiBankSalaryExportCsv and getRbiBankSalaryExportTxt
```

**File 4: `PayrollController.cs`** — add `string client = ""` to all 3 actions:
```csharp
public IActionResult GetRbiBankSalaryExport(
    string dtSalaryPeriod, string branch = "ALL", string employeeType = "ALL", string client = "")
{
    var result = _payrollRepository.GetRbiBankSalaryExportData(dtSalaryPeriod, branch, employeeType, client);
    return Ok(result);
}
// Same for GetRbiBankSalaryExportCsv and GetRbiBankSalaryExportTxt
```

**File 5: `IPayrollRepository.cs` + `PayrollRepository.cs` (Implementation)**
```csharp
// Interface
List<RbiBankSalaryExport> GetRbiBankSalaryExportData(
    string dtSalaryPeriod, string branch, string employeeType, string client);

// Implementation (pass-through)
public List<RbiBankSalaryExport> GetRbiBankSalaryExportData(
    string dtSalaryPeriod, string branch, string employeeType, string client)
{
    return RbiBankSalaryExport.GetRbiBankSalaryExportData(dtSalaryPeriod, branch, employeeType, client);
}
```

**File 6: `RbiBankSalaryExport.cs`**
```csharp
// Change method signature
public static List<RbiBankSalaryExport> GetRbiBankSalaryExportData(
    string dtSalaryPeriod, string branch, string employeeType, string client)

// Add to WHERE clause builder (after the existing employeeType block)
if (!string.IsNullOrEmpty(client) && client != "ALL")
{
    whereClause += " AND Employee.EMP_CLIENT = @Client ";
}

// Add parameter (after the existing employeeType parameter block)
if (!string.IsNullOrEmpty(client) && client != "ALL")
{
    cmd.Parameters.AddWithValue("@Client", client);
}
```

---

## 8. EDGE CASES

1. **Multi-client employees**: `Employee.EMP_CLIENT` is the employee's master/home client code. Payroll can theoretically split an employee's pay across multiple clients via `AttendanceDetails.Client`, but the RBI Bank Salary export uses `PaySlip.NetPay` (one row per employee), so `EMP_CLIENT` is the correct and expected filter key.

2. **Client dropdown ordering**: `GetClientMsterListByBranch` already `OrderBy(x => x.Name)` in the repository — no additional sorting needed.

3. **Branch reset**: When branch changes, `clientModel` should be refreshed and `Client` reset to `""` (All Clients) — this prevents stale client selections from a previous branch.

4. **Sibling screen**: `GetRbiBankAdvanceExport` in `PayrollController` + `RbiBankAdvanceExport.cs` has the identical gap. That screen (`rbi-bank-advance-salary-process`) has no client filter either. Out of scope for this task but should be addressed in parallel.

5. **CSV/TXT file name**: After adding `client`, optionally include it in the file name for clarity:  
   `RBI_Bank_Salary_Export_{dtSalaryPeriod}_{branch}_{client}_{employeeType}.csv`

6. **Backward compatibility**: Adding `client = ""` as a default parameter in the controller means existing callers (other consumers, if any) continue working without sending a client parameter.
