# Implementation Plan: Exclude ESI Deduction = No Employees from ESI Statement Report

## Summary

**Bug:** Employees whose Employee Master has "ESI Deduction = No" are showing in the
ESI Statement Report with a blank IP Number (ESINumber).

**Root cause:** The `GetESIStatement` API endpoint fetches all employees with payslips
for the selected period via `GetPayrollRows()` and does not filter out employees whose
`SOCSODETECT = false` in `EmployeeSalaryDetails`. A fix was written as a local working-tree
change but was never committed or published — so the live binary still lacks the filter.

**Fix location:** C# API only. No Angular UI changes are required.

---

## Key Findings

### Field name for ESI Deduction
| Layer | Name | Type | Meaning |
|---|---|---|---|
| Database (EmployeeSalaryDetails table) | `SOCSODETECT` | `BIT NOT NULL DEFAULT 0` | `1` = ESI applicable, `0` = ESI not applicable |
| Domain model | `EmployeeSalaryDetails.SOCSODETECT` | `bool` | same |
| Angular form control | `SOCSODETECT` | radio: `"Yes"` / `"No"` / `""` | saved as `value == 'Yes'` → `true/false` |

- UI label: **"ESI Deduction"** with radio buttons Yes / No / None
- Save logic (new-employee.component.ts, line 2568):
  `data['SOCSODETECT'] = this.frm.get('SOCSODETECT')?.value == 'Yes'`
  → `"Yes"` → `true`, `"No"` or `""` → `false`
- The `EmployeeSalaryDetails` DbSet is registered in `OBMSDbContext` and is accessible
  via `_context.EmployeeSalaryDetails` in `ComplianceReportController`.

### Filter logic to apply
Exclude employees where `EmployeeSalaryDetails.SOCSODETECT == false`
(i.e. keep only those where `SOCSODETECT == true`).

---

## Fix: C# API

**File:** `c:\Users\admin\Documents\India FWG\OBMSINDIA-API\Controllers\ComplianceReportController.cs`

**Method:** `GetESIStatement` (starts at line 2123, `[HttpPost("GetESIStatement")]`)

**Current state:** A filter block already exists as an **uncommitted local change** in the
working tree (confirmed via `git diff HEAD~1`). It is NOT in the committed source or the
published binary. The filter code is:

```csharp
// Filter: exclude employees where ESI Deduction = No (SOCSODETECT = false)
var empCodes = rows.Select(r => (string)r.empId).Distinct().ToList();
var esiEligibleCodes = (await _context.EmployeeSalaryDetails
    .Where(sd => empCodes.Contains(sd.EMPFL_CODE) && sd.SOCSODETECT == true)
    .Select(sd => sd.EMPFL_CODE)
    .ToListAsync()).ToHashSet();
rows = rows.Where(r => esiEligibleCodes.Contains((string)r.empId)).ToList();
```

This logic is **correct**. It must be committed and published.

---

## Implementation Steps

- [ ] 1. **Verify the uncommitted filter block is present in the source file**

  Confirm lines ~2145–2152 in `Controllers/ComplianceReportController.cs` contain
  the `SOCSODETECT` filter exactly as shown above. If for any reason the block is
  missing (e.g. working-tree was reverted), re-add it at the location shown below.

  **Exact insertion point** — insert after the line:
  ```csharp
  var rows = await GetPayrollRows(branch, client, periodStart, periodEnd);
  ```
  …and before the line:
  ```csharp
  // Format ESI Statement data with required columns
  ```

  **Code to insert:**
  ```csharp
  // Filter: exclude employees where ESI Deduction = No (SOCSODETECT = false)
  var empCodes = rows.Select(r => (string)r.empId).Distinct().ToList();
  var esiEligibleCodes = (await _context.EmployeeSalaryDetails
      .Where(sd => empCodes.Contains(sd.EMPFL_CODE) && sd.SOCSODETECT == true)
      .Select(sd => sd.EMPFL_CODE)
      .ToListAsync()).ToHashSet();
  rows = rows.Where(r => esiEligibleCodes.Contains((string)r.empId)).ToList();
  ```

  Files to check/modify: `Controllers/ComplianceReportController.cs`

  Verify: `dotnet build` in `OBMSINDIA-API/` — must complete with 0 errors, 0 warnings.

- [ ] 2. **Build and publish the updated binary**

  Run from `c:\Users\admin\Documents\India FWG\OBMSINDIA-API\`:
  ```
  dotnet publish -c Release -o publish
  ```
  This overwrites `publish/OBMS.WebAPI.dll` and `publish/OBMS.WebAPI.exe` with the
  version that contains the filter.

  Files modified: `publish/OBMS.WebAPI.dll`, `publish/OBMS.WebAPI.exe`, `publish/OBMS.WebAPI.pdb`

  Verify: `dotnet publish` exits with "Build succeeded. 0 Error(s)".

- [ ] 3. **Commit the change**

  Stage and commit both the source change and the updated publish artifacts:
  ```
  git add Controllers/ComplianceReportController.cs
  git add publish/OBMS.WebAPI.dll publish/OBMS.WebAPI.exe publish/OBMS.WebAPI.pdb
  git commit -m "fix: exclude SOCSODETECT=false employees from ESI Statement Report"
  ```

  Verify: `git log --oneline -1` shows the new commit message.

---

## No Angular UI Changes Required

The Angular ESI Statement Report component
(`OBMSINDIA-UI/src/app/modules/report/payroll/esi-statement-report/esi-statement-report.component.ts`)
renders whatever rows the API returns. Once the API filters out `SOCSODETECT = false`
employees server-side, those employees will simply not appear in the response and the
UI will not show them. No UI changes are needed.

---

## Filter Logic Summary

| Condition | Action |
|---|---|
| `EmployeeSalaryDetails.SOCSODETECT == true` (ESI Deduction = Yes) | ✅ Include in report |
| `EmployeeSalaryDetails.SOCSODETECT == false` (ESI Deduction = No or None) | ❌ Exclude from report |

- Field: `SOCSODETECT` on table `EmployeeSalaryDetails`, joined via `EMPFL_CODE = EMP_CODE`
- This is a `BIT NOT NULL DEFAULT 0` column, so no NULL-handling is needed
- The `_context.EmployeeSalaryDetails` DbSet is available in `ComplianceReportController`
  (confirmed in `OBMSDbContext.cs`)

---

## Files to Modify

| File | Change |
|---|---|
| `OBMSINDIA-API/Controllers/ComplianceReportController.cs` | Ensure SOCSODETECT filter block is present in `GetESIStatement` method |
| `OBMSINDIA-API/publish/OBMS.WebAPI.dll` | Rebuild via `dotnet publish` |
| `OBMSINDIA-API/publish/OBMS.WebAPI.exe` | Rebuild via `dotnet publish` |
| `OBMSINDIA-API/publish/OBMS.WebAPI.pdb` | Rebuild via `dotnet publish` |

**No changes to stored procedures, Angular UI, or any other files.**
