# Investigation: PF Configuration — ₹25,000 Basic Salary Limit, Financial Year & Pension % in Salary Processing

## Summary (short answer to the user's question)

**Is the PF Configuration page (₹25,000 limit, Financial Year, Pension %) applied correctly during salary processing? — No, not fully.**

There are **two separate PF configuration systems** in this app, and they are not connected:

1. The **"PF Configuration" page** you are looking at (Master → pf-slab) writes to a new `PFConfiguration` table. This table is used by `PFCalculationService`, which feeds only the **payslip preview** and the **employee PF deduction** in the processing run.
2. The **actual stored payslip** PF amounts (employee EPF + employer contribution) come from a **legacy `EPF` slab lookup table** (Master → epf-slab), which the PF Configuration page does NOT touch.

On top of that, three concrete bugs/inconsistencies affect correctness:

- **Financial Year is a dead field.** It is saved to `PFConfiguration.Financial_Year` but **never used** in any lookup. Config rows are selected only by `Effective_Date` and `Is_Active`. Setting "2026-2027" has zero effect on which row is picked.
- **The ₹15,000 pension wage ceiling is NOT applied.** Pension (EPS) is computed as `8.33% × BasicSalaryLimit`. With ₹25,000 as the limit, pension becomes ₹2,082.50 instead of the statutory maximum ₹1,250. Likewise the employee PF becomes ₹3,000 instead of ₹1,800.
- **The Employer EPS Rate is only partly honored.** It is used in the payslip-preview path, but the **PF Statement report hardcodes 8.33%** and the ad-hoc `calculate-pf` endpoint ignores EPS entirely.

So the numbers you configured will not come out "correct" in the stored payslip or the PF statement, and the payslip preview will likely disagree with the processed payslip.

---

## Step-by-step analysis of how ₹25,000 / Financial Year / Pension % flows

### Step 1 — Where the configuration is stored (DB)

Table: **`PFConfiguration`** (new statutory config). Columns (`Models/Domain/PFConfiguration.cs`):

| Column | Type | Default | Your value |
|---|---|---|---|
| `Id` | int PK | — | — |
| `Employee_Rate` | decimal(5,2) | 12.00 | 12% |
| `Employer_Rate` | decimal(5,2) | 12.00 | (13% in seed) |
| `Employer_EPS_Rate` | decimal(5,2) | 8.33 | 8.33% (pension %) |
| `Basic_Salary_Limit` | decimal(10,2) | 15000.00 | **25000.00** |
| `Effective_Date` | datetime | now | (a date) |
| `Is_Active` | bit | 1 | 1 |
| `Created_Date` | datetime | now | — |
| `Financial_Year` | nvarchar(9) | "2024-2025" | **"2026-2027"** |

Seed script: `Scripts/Seed_PF_Configuration.sql` (note: it targets DB `obmsdev_backup`, while the controller's default connection string is `obmsuat` — confirm the right DB).

There is also a **legacy `EPF` table** (`Models/Domain/EPF.cs`) with columns `epf_id, epf_from, epf_to, epf_worker, epf_boss, epf_worker8Pa, epf_worker55, epf_boss55, ...`. This holds **precomputed PF amounts per salary slab** (e.g., a row for salary 14000–15000 with fixed `epf_worker`/`epf_boss` values).

### Step 2 — Backend endpoints that serve PF data

- **PF Configuration CRUD** — `Controllers/IndianStatutoryController.cs`, route `api/statutory/pf-configuration` (GET / POST / PUT / DELETE). Reads/writes the `PFConfiguration` table directly with raw SQL. `Financial_Year` is included in SELECT/INSERT/UPDATE.
- **Ad-hoc PF calc** — `api/statutory/calculate-pf` (POST) in the same controller. It runs `SELECT TOP 1 ... FROM PFConfiguration WHERE IsActive=1 ORDER BY EffectiveDate DESC` (no Financial Year filter, no date filter), then `taxable = min(total, pfLimit)`, `employeePF = taxable × EmployeeRate`, `employerPF = taxable × EmployerRate`. **EPS/pension is not computed here at all.**
- **Legacy EPF slab CRUD** — `Controllers/MasterController.cs` (`#region EPF Module`): `GetEPFMaster`, `SaveAndUpdateEPFMaster`, `DeleteEPFMasterById`. Serves the `EPF` lookup table. (This is the screen the processing run actually depends on.)

### Step 3 — Where PF / Pension is calculated during salary processing

The processing run is `BusinessObjects/SalaryProcess.cs` → `Process(...)`. It uses **two different sources** for PF:

**(a) Employee PF used in deductions (`dPF`)** — from `PFCalculationService.CalculatePFForPayslip(...)` (uses `PFConfiguration`):
```
total   = CB_Basic + CB_DA + CB_OtherAllowances
perDay  = total / calendarDays
pfWage  = perDay × BasicSalaryDays       (worked days)
capped  = Min(pfWage, BasicSalaryLimit)  ← ₹25,000 cap if you set it
PF      = Round(capped × EmployeeRate / 100, 0)   = Round(25000 × 12%) = ₹3,000
```
This `dPF` is added into `TotalDeduction` → `NetPay` (`SalaryProcess.cs` ~line 3489–3513, 3631). The capped wage is stored as `dPFWage` → PaySlip `PFWage` column.

**(b) Stored EPF employee/employer amounts (`dEPFEmployee`, `dEPFEmployer`)** — from the **legacy `EPF` slab table**, NOT from `PFConfiguration` (`SalaryProcess.cs` ~line 3105–3151):
```
select epf_worker, epf_boss from EPF where epf_from <= @BasicSalary and epf_to >= @BasicSalary
dEPFEmployee = epf_worker    → stored as PaySlip.EPFDeductionAmount
dEPFEmployer = epf_boss      → stored as PaySlip.EPFEmployerContribution
```
Age > 59 with `DETECTBYND55` flag → employer-only 4% of basic. **No age logic here for normal case.**

> This is the core issue: the stored payslip's EPF amounts come from the `EPF` slab table (salary-range lookup), so your `PFConfiguration` of ₹25,000 / 8.33% / 2026-2027 does **not** change what gets stored for EPF employee/employer. Those slab values were pre-entered in the EPF Slab screen.

### Step 4 — Pension (EPS) calculation in detail

`Services/PFCalculationService.cs` → `CalculatePFWithAge(...)`:
```
pfSalary     = Math.Min(TotalWages, pfConfig.BasicSalaryLimit)   ← capped at ₹25,000
PensionSalary = pfSalary
EPF  = pfSalary × EmployeeRate / 100          ← 12% of 25000 = 3000
EPS  = pfSalary × EmployerEPSRate / 100      ← 8.33% of 25000 = 2082.50
Balance = EPF − EPS                           ← 3.67% = 917.50
EmployerContribution = EPS + Balance
```
- **The statutory ₹15,000 pension wage ceiling is NOT applied separately.** EPS is computed on the same `pfSalary` that is capped at `BasicSalaryLimit`. So ₹25,000 produces pension = ₹2,082.50, which **exceeds the statutory EPS cap of ₹1,250/month** (8.33% of ₹15,000).
- The `EmployerEPSRate` from `PFConfiguration` IS used here.

`Controllers/ComplianceReportController.cs` (PF Statement / ECR report), two places:
- `CalculateEmployerPF(...)` (~line 321): `eps = Round(EligibleWages × 0.0833, 2)` — **hardcoded 8.33%**, ignores `Employer_EPS_Rate`.
- PF statement build (~line 965–974): `employerEPS = Round(storedPFWage × 0.0833, 2)` — **hardcoded 8.33%**, using the stored `PFWage` (which is capped at `BasicSalaryLimit`, i.e., ₹25,000). So the report also overstates pension when the limit is ₹25,000.

`api/statutory/calculate-pf` endpoint: does **not** compute EPS at all.

### Step 5 — How Financial Year affects which slab row is picked

In `PFCalculationService.GetActivePFConfiguration(date)`:
```csharp
_context.Set<PFConfiguration>().AsNoTracking()
    .Where(c => c.IsActive && c.EffectiveDate <= calculationDate)
    .OrderByDescending(c => c.EffectiveDate)
    .FirstOrDefault();
```
- **`Financial_Year` is NOT in the WHERE clause.** It is stored only for display. The active row is chosen purely by the **most recent `Effective_Date` that is ≤ the salary period date**.
- Consequence: your "2026-2027" row will be used **only if its `Effective_Date` is the latest one ≤ the processing date**. If you process a salary for a date **before** that row's Effective_Date, an older config row is used instead. If multiple active rows exist, the newest Effective_Date always wins regardless of its Financial Year label.

### Step 6 — Frontend PF Configuration form (`pf-slab`)

`obms-ui/src/app/modules/master/pf-slab/new-pf-slab/new-pf-slab.component.ts` + `.html`. Form fields:
- `basicSalaryLimit` (required) — the ₹25,000
- `financialYear` (required) — "2026-2027"
- `employeeContributionRate` (required) — 12
- `employerContributionRate` (required) — 13
- `employerEPSRate` — **NOT required** (no validator), the pension %
- `effectiveDate` (required)
- `isActive` (checkbox)

List view (`pf-slab.component.ts`) columns: `basicSalaryLimit, employeeContributionRate, employerContributionRate, financialYear, effectiveDate, isActive, action` — note **EPS rate is NOT shown in the list**.

Service: `obms-ui/src/app/service/indian-statutory.service.ts` → `getPFConfiguration/savePFConfiguration/updatePFConfiguration/deletePFConfiguration` → `api/statutory/pf-configuration`.

The **legacy EPF Slab editor** (`epf-slab/new-epf-slab`) edits `epf_from, epf_to, epf_worker, epf_boss, epf_worker55, epf_boss55, ...` via `MastermoduleService` → `MasterController` EPF endpoints. **This is the table the salary processing run reads for the stored payslip.**

### Step 7 — Payslip display

`PayrollRepository.GetPayslipData(...)` (payslip **preview**) uses `PFCalculationService.CalculatePF(basicSalary, hra, periodDate)` and returns:
- `providentFund = pfResult.EmployeeContribution` (employee EPF)
- `employerPF = pfResult.EmployerContribution` (EPS + Balance)
- `totalDeductions` includes `pfResult.EmployeeContribution`

Frontend payslip shows `providentFund` and `employerPF`; pension is **not shown as a separate line** in the standard payslip. The separate **PF Statement** report (`obms-ui/src/assets/report-templates/statements/pf-statement.html`) shows `EPS Wages`, `EPS Contribution Remitted`, `EPF Contribution Remitted` separately — but that report uses hardcoded 8.33% (see Step 4).

---

## Evidence — file / symbol citations

| Finding | Location |
|---|---|
| `PFConfiguration` table & columns | `Models/Domain/PFConfiguration.cs` |
| `EPF` legacy slab table & columns | `Models/Domain/EPF.cs` |
| PF config CRUD endpoints | `Controllers/IndianStatutoryController.cs` `#region PF Configuration` (route `api/statutory/pf-configuration`) |
| Ad-hoc calc ignores EPS & Financial Year | `IndianStatutoryController.cs` `CalculatePF` (route `calculate-pf`, ~line 1003) |
| PF calc service, EPS = pfSalary × EPSRate (no ₹15k pension ceiling) | `Services/PFCalculationService.cs` `CalculatePFWithAge` (~line 80–102) |
| Config lookup ignores `Financial_Year` | `Services/PFCalculationService.cs` `GetActivePFConfiguration` (~line 197–204) |
| `PFCalculationResult` model incl. EPS/PensionSalary | `Services/IPFCalculationService.cs` |
| Processing run employee PF (`dPF`) from `CalculatePFForPayslip` | `BusinessObjects/SalaryProcess.cs` ~line 3486–3513 |
| Processing run stored EPF from legacy `EPF` slab table | `BusinessObjects/SalaryProcess.cs` ~line 3105–3151 (`select epf_worker, epf_boss from EPF ...`) |
| Stored PaySlip columns: `EPFDeductionAmount`, `EPFEmployerContribution`, `PFWage`, `PF` | `BusinessObjects/SalaryProcess.cs` INSERT ~line 3693–3762 |
| Payslip preview uses `CalculatePF`, returns `providentFund`/`employerPF` | `Repositories/Implementation/PayrollRepository.cs` ~line 3684, 3760–3771 |
| PF Statement report hardcodes 8.33% on stored PFWage | `Controllers/ComplianceReportController.cs` ~line 965–974, `GetPFStatement` ~line 1936 |
| Frontend PF config form (₹25k, FY, EPS rate optional) | `obms-ui/src/app/modules/master/pf-slab/new-pf-slab/new-pf-slab.component.ts/.html` |
| Frontend EPF slab editor (legacy) | `obms-ui/src/app/modules/master/epf-slab/new-epf-slab/new-epf-slab.component.ts` |
| Frontend PF config API service | `obms-ui/src/app/service/indian-statutory.service.ts` |
| Seed script (targets `obmsdev_backup`) | `Scripts/Seed_PF_Configuration.sql` |

---

## Conclusions

1. **Two PF config systems, not unified.** `PFConfiguration` (new, your page) drives the payslip preview and the employee-PF deduction in the processing run. The stored payslip's EPF employee/employer amounts come from the legacy `EPF` slab table. Editing only the PF Configuration page will NOT change the EPF amounts written to `PaySlip`.
2. **Financial Year is effectively dead.** It is saved but never used in selection. Only `Effective_Date` + `Is_Active` decide which config applies. "2026-2027" by itself does nothing.
3. **₹25,000 as Basic Salary Limit overstates both PF and pension** because the statutory ₹15,000 ceilings (EPF wage ceiling and EPS pension wage ceiling) are not applied separately. With ₹25,000:
   - Employee PF cap = ₹3,000 (statutory ₹1,800)
   - EPS pension = ₹2,082.50 (statutory max ₹1,250)
   - Unless the organisation genuinely contributes PF on actual basic above the ceiling, ₹25,000 is almost certainly wrong; the statutory ceiling is ₹15,000.
4. **Employer EPS Rate is applied inconsistently**: used in `PFCalculationService` (preview path), hardcoded to 8.33% in the PF Statement report, and ignored in the `calculate-pf` endpoint.
5. **Payslip preview can disagree with the processed/stored payslip** because preview uses `PFConfiguration`-based math while stored EPF uses the `EPF` slab lookup.

---

## Recommendations (do NOT implement now — investigation only)

1. **Pick one source of truth for PF config.** Either retire the legacy `EPF` slab table and make `SalaryProcess.Process` derive `dEPFEmployee`/`dEPFEmployer` from `PFCalculationService` using `PFConfiguration`, or clearly document that the stored payslip EPF uses the EPF slab screen and the PF Configuration page only controls caps/rates for deductions and reporting. Currently the two silently diverge.
2. **Apply the ₹15,000 pension wage ceiling separately.** Introduce a distinct pension wage ceiling (e.g., a `Pension_Salary_Limit` column, default ₹15,000) and compute `EPS = min(pfSalary, pensionCeiling) × EPSRate / 100` (max ₹1,250). Do not reuse `BasicSalaryLimit` as the pension ceiling.
3. **Make Financial Year meaningful or remove it.** Either filter the active config by `Financial_Year` matching the salary period's FY (e.g., FY 2026-2027 covers 2026-04-01 to 2027-03-31), or drop the field and rely solely on `Effective_Date`. Today it misleads users into thinking it drives selection.
4. **Use `Employer_EPS_Rate` everywhere or fix it as statutory.** Align `ComplianceReportController` and the `calculate-pf` endpoint to the configured EPS rate, or explicitly document EPS as the statutory 8.33% and stop storing it as an editable field.
5. **Reconcile payslip preview vs processing.** The preview (`CalculatePF`, passing `hra` as `da`) and the processing run (`CalculatePFForPayslip`, prorated on `CB_Basic + CB_DA + CB_OtherAllowances`) can yield different employee PF for the same employee. Align the wage basis so preview and processed payslip match.
6. **Confirm the active DB.** The seed targets `obmsdev_backup` while the controller defaults to `obmsuat`; verify the `PFConfiguration` row with ₹25,000 is actually in the database the running API connects to.
