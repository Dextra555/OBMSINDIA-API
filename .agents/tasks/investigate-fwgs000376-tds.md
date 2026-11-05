# Investigation: TDS not calculating for employee FWGS000376

**Status:** Root cause confirmed via live DB query.
**Date:** 2026-11-04

## Employee
- EMP_CODE: `FWGS000376`
- Name: Shabana Shaikh
- EMP_ID (PaySlip FK): `374`
- DOB: 1978-10-17 (age ~48 → basic exemption 2,50,000)
- Latest payslip (Sep 2026): GrossPay = 3,73,000, TDS = 0.00, TotalDeduction = 0.00

## Root cause

**`Employee.TDSApplicable = False` for FWGS000376.**

The TDS block in `SalaryProcess.cs` is gated by this single flag:

```csharp
dTDS = 0;
if (bTDSApplicable)   // <-- reads Employee.TDSApplicable (bit). False here => block skipped.
{
    decimal annualIncome = dGrossPay * 12;
    decimal basicExemption = 250000m;
    if (EmployeeAge >= 80) basicExemption = 500000m;
    else if (EmployeeAge >= 60) basicExemption = 300000m;
    ...
}
```

Because the flag is `False`, `dTDS` stays `0` and nothing is ever calculated,
regardless of salary. (Annual income here is ~44.7L, well above 2.5L exemption,
so once the flag is on, TDS will compute fine — the 6 active slabs are present.)

## Active payroll path
- `PayrollController.Process` calls `_salaryProcess.Process(...)` (legacy
  `SalaryProcess.cs`), NOT the newer `PayrollRepository` / `TDSCalculationService`.
- Both paths share the same `TDSApplicable` gate, so the cause is the same either way.

## TDS slab config (verified active, 6 rows)
| Min_Income | Max_Income | Tax_Rate |
|-----------:|-----------:|---------:|
| 0 | 300000 | 0% |
| 300001 | 600000 | 5% |
| 600001 | 900000 | 10% |
| 900001 | 1200000 | 15% |
| 1200001 | 1500000 | 20% |
| 1500001 | 99999999 | 30% |

## Fix (two options)
1. UI: Employee Master → edit FWGS000376 → "TDS Deduction" radio → **Yes** → Save.
2. SQL:
```sql
UPDATE Employee SET TDSApplicable = 1 WHERE EMP_CODE = 'FWGS000376';
```
Then reprocess the payroll period.

## Note (out of scope for TDS, worth flagging)
Sep 2026 payslip shows GrossPay 3,73,000 with TotalDeduction = 0.00 — no PF/ESI/PT/LWF
either. This employee currently has zero statutory deductions of any kind, which is
worth confirming is intended (e.g. non-structure / exempt setup) once TDS is enabled.
