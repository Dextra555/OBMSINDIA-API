# Verification Note — ESI Statement Report Fix

## Files Changed

| File | Change |
|---|---|
| `Controllers/ComplianceReportController.cs` | Added SOCSODETECT filter block inside `GetESIStatement` method |
| `publish/OBMS.WebAPI.dll` | Rebuilt binary containing the filter |
| `publish/OBMS.WebAPI.exe` | Rebuilt binary containing the filter |
| `publish/OBMS.WebAPI.pdb` | Rebuilt debug symbols |

## Filter Logic

Inside `GetESIStatement`, immediately after `GetPayrollRows()` returns, the following
filter is applied before formatting rows for the response:

1. Extract all distinct `empId` values from the raw payroll rows.
2. Query `EmployeeSalaryDetails` for those employee codes where `SOCSODETECT == true`
   (i.e. ESI Deduction = Yes in Employee Master).
3. Discard any payroll row whose `empId` is not in the eligible set.

Employees with `SOCSODETECT == false` (ESI Deduction = No or blank) are therefore
never included in the ESI Statement response, resolving the blank IP Number issue.

## Build Result

```
dotnet build → Build succeeded in 3.9s — 0 Error(s), 0 Warning(s)
```

## Commit

```
[main d389bfd] fix: exclude SOCSODETECT=false employees from ESI Statement Report
 4 files changed, 9 insertions(+)
```
