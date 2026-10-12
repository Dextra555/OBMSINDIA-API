# ESI Statement Report: Exclude ESI Deduction = No Employees

Employees whose Employee Master had "ESI Deduction = No" (`SOCSODETECT = false`) were
appearing in the ESI Statement Report with a blank IP Number. The fix adds a server-side
filter in `GetESIStatement` that drops any employee not flagged as ESI-eligible before
the report rows are formatted. No UI changes are required — the Angular component renders
whatever the API returns. The build is clean (0 errors, 0 warnings) and the change is
committed and published.

**Watch for:** An employee with no `EmployeeSalaryDetails` row at all would be silently
excluded from the report, even if they should be included (possible, no blocking concern
for the reported bug, but worth awareness). No test coverage was added.

**Verdict**: APPROVED

---

## High-level view

The filter inserts a two-step lookup between `GetPayrollRows()` and the ESI row
formatter: collect distinct employee codes from the raw rows, query
`EmployeeSalaryDetails` for those codes where `SOCSODETECT == true`, then discard any
row not in the eligible set. The join key (`empId = EMP_CODE` in payroll rows,
`EMPFL_CODE` in `EmployeeSalaryDetails`) is the canonical join used throughout the
codebase — confirmed in `PayrollRepository.cs`, `EmployeeRepository.cs`, and
`UtilityMain.cs`. The field type is `bool` (non-nullable), so no NULL edge case applies.

The filter is applied only inside `GetESIStatement`. Other callers of `GetPayrollRows`
(PF statement, payslip, salary register) are unaffected.

An employee who exists in payroll but has no `EmployeeSalaryDetails` row would be
excluded — the lookup produces no `EMPFL_CODE` match, so `esiEligibleCodes` never
contains them. In a healthy dataset this shouldn't occur, but the behavior is implicit
rather than logged or handled explicitly.

---

<details>
<summary>Issues (1)</summary>

1. **Silent exclusion for missing EmployeeSalaryDetails** — An employee present in
   payroll but missing a `EmployeeSalaryDetails` row will be silently dropped from the
   ESI report. This is not a bug for the reported case, but if it happens it will be
   invisible to the operator. Consider logging a warning when `esiEligibleCodes` is
   notably smaller than `empCodes`, or verify that the data model guarantees every
   employee has a salary-details row (the `EmployeeRepository` save path does create one,
   but legacy / migrated data may not).

</details>

---

<details>
<summary>Details</summary>

### Scope of change — no regression surface

`GetESIStatement` is the only endpoint modified. Every other caller of `GetPayrollRows`
(PF statement, salary register, payslip) is untouched. `SOCSODETECT` is already used in
existing SQL in `UtilityMain.cs` for the legacy ESI payslip report, so the field
semantics are not new.

### Silent exclusion edge case

The filter is an inner-style lookup: if `EmployeeSalaryDetails` has no row for an
employee code, that employee is absent from `esiEligibleCodes` and dropped from the
report with no log entry. For the reported bug this is the desired outcome when
`SOCSODETECT = false`. The risk is that a data gap (employee created without a
salary-details row) produces a silent omission rather than an obvious error.

### Test coverage

`GetESIStatement` has no automated test coverage. Manual verification that an employee
with `SOCSODETECT = false` no longer appears in the report output is the only current
confirmation path.

</details>

---

<details>
<summary>Files changed</summary>

| File | Change |
|---|---|
| `Controllers/ComplianceReportController.cs` | Added 5-line `SOCSODETECT` filter block inside `GetESIStatement`, lines 2146–2152 |
| `publish/OBMS.WebAPI.dll` | Rebuilt binary |
| `publish/OBMS.WebAPI.exe` | Rebuilt binary |
| `publish/OBMS.WebAPI.pdb` | Rebuilt debug symbols |

Commit: `d389bfd — fix: exclude SOCSODETECT=false employees from ESI Statement Report`

</details>
