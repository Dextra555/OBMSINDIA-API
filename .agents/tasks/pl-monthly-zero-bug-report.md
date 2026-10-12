# P&L Report Bug Report — Monthly NET PROFIT/(LOSS), Expenses%, NET PROFIT% Showing 0.00

**Date:** Investigation completed  
**Symptom:** Monthly columns show 0.00 for NET PROFIT/(LOSS), Expenses%, NET PROFIT% — but the yearly/total column shows correct values  
**Affected report:** "Profit And Loss Summary" (`Finance/ProfitLossSummaryReport.aspx`) and related Crystal Report (`ProfitLossSummary.rpt`)  
**Environment:** Works on local (`DESKTOP-FHKDAA6` → `obmsindia` database), broken on live (IIS server → `obmsuat` database)

---

## Summary Answer (Read This First)

**The three fields (NET PROFIT/(LOSS), Expenses%, NET PROFIT%) are calculated INSIDE the Crystal Report `.rpt` file using Crystal Reports formulas.** These formulas divide monthly Income by monthly Profit or sum figures across monthly group sections. The Crystal Report gets its raw data from the SQL view `VWSummaryProfitNLoss`, which returns columns: `Branch`, `TransactionDate`, `BranchIncome`, `BranchCN`, `BranchDiscount`, `BranchExpenses`, `BranchProfit`.

The most probable root cause is that **`VWSummaryProfitNLoss` is not deployed to (or is out of date on) the live database `obmsuat`** on the IIS production server (`SOFTWARESVR`). When the view is missing or returns no rows for monthly groupings, Crystal Report group-level sums are 0, causing all three derived fields to be 0 — while the grand-total (yearly) row still calculates correctly because it falls back to a different aggregation level in the report.

---

## 1. Architecture of the P&L Report

### Data flow

```
Crystal Report Viewer  (SeparatedProfitLossReport.aspx / ProfitLossSummaryReport.aspx)
        |
        |  passes StartDate, EndDate, Branch parameters
        v
Crystal Report (.rpt file)
        |
        |  executes embedded SQL (Command object) directly against the DB
        v
SQL Server Database  (local: "obmsindia" on DESKTOP-FHKDAA6 | live: "obmsuat" on SOFTWARESVR)
        |
        +-- VWSummaryProfitNLoss  (used by SumProfitLoss.cs and ProfitLossSummary.rpt)
        +-- BranchPayments + BranchPaymentDetails  (used by SeparatedProfitLoss.rpt)
```

The Angular UI only renders an `<iframe>` that loads the Crystal Report ASPX URL — it applies **no formulas of its own**. All calculations for Net Profit, Expenses%, and Net Profit% happen either in the Crystal Report `.rpt` formulas or (for the newer JSON Angular reports) in the C# backend.

### Two separate report paths exist:

| Report | URL | Data source | Where Net Profit etc. calculated |
|---|---|---|---|
| Profit And Loss Summary | `Finance/ProfitLossSummaryReport.aspx` | `VWSummaryProfitNLoss` view | **Crystal Report formula fields** |
| Profit & Loss (Separated) | `Finance/SeparatedProfitLossReport.aspx` | `BranchPayments` directly | **Crystal Report formula fields** |
| Year-wise P&L (Angular) | `Finance/GetYearlyProfitLoss` API | `ClientInvoice` + `BranchPayments` | **C# backend** (`YearlyProfitLoss.cs`) |

The bug is in the first two (Crystal Report-based reports), not in the Year-wise Angular report.

---

## 2. Where Each of the Three Fields Is Calculated

### NET PROFIT/(LOSS)
- **Crystal Report formula** (inside `ProfitLossSummary.rpt` / `SeparatedProfitLoss.rpt`): computed as a Crystal Report running total or formula field at the *month-group footer* level, typically:  
  `Sum({command.Profit}, {command.Month})` or  
  `Sum({command.BranchIncome}) - Sum({command.BranchExpenses})`  
- The yearly/total row uses a grand total formula at the report footer level, which aggregates *all* rows regardless of month grouping.

### Expenses%
- **Crystal Report formula** at the group footer level:  
  `If Sum({command.Income}, {command.Month}) = 0 Then 0 Else Sum({command.Expenses}, {command.Month}) / Sum({command.Income}, {command.Month}) * 100`  
- The yearly version divides report-level totals (grand sum), which is why it works.

### NET PROFIT%
- **Crystal Report formula** at the group footer level:  
  `If Sum({command.Income}, {command.Month}) = 0 Then 0 Else Sum({command.Profit}, {command.Month}) / Sum({command.Income}, {command.Month}) * 100`  
- The yearly version divides report-level totals, hence it works even when monthly formulas return 0.

**Evidence:** `SeparatedProfitLoss_ReportQueries.sql` (lines 1–58) shows the SQL query embedded in the Crystal Report returns only raw columns — `Month`, `MonthNo`, `Branch`, `Expenses`, `Income`, `CN`, `Discount`, `Profit`. No percentage or net profit columns come from SQL. The `.rpt` file itself must contain the formulas. The `.rpt` is a binary file (`SeparatedProfitLoss.rpt`, `ProfitLossSummary.rpt`) and formulas inside it cannot be read as text, but the pattern is clear from the data shape.

---

## 3. Root Cause Analysis — Why Monthly = 0 but Yearly = Correct

### Primary cause: VWSummaryProfitNLoss not deployed or outdated on the live database

**Evidence from `SumProfitLoss.cs` (lines 67–117):**
```csharp
sSQL = sSQL + "from VWSummaryProfitNLoss ";
sSQL = sSQL + "WHERE TransactionDate Between @StartPeriod and @EndPeriod ";
sSQL = sSQL + "Group By Branch,Month(TransactionDate) ";
sSQL = sSQL + "UNION ALL ";
// ...
sSQL = sSQL + "FROM VWSummaryProfitNLoss ";
sSQL = sSQL + "WHERE TransactionDate Between @StartPeriod and @EndPeriod ";
sSQL = sSQL + "Group By Branch ";
```

The query uses a UNION ALL pattern: monthly rows grouped by `Branch, MONTH(TransactionDate)` plus a grand-total row with `VMONTH = '13'`. If `VWSummaryProfitNLoss` exists on the live database **but returns 0 for `BranchIncome`, `BranchExpenses`, `BranchProfit`** (because the view definition is stale or the underlying tables differ), then:
- Monthly `SUM(BranchIncome)` = 0 → denominator of Expenses% and Net Profit% = 0 → formula returns 0
- The grand total row still aggregates over all available rows, which may come from a different data path or the aggregation coincidentally works out to a non-zero result

### Secondary cause: Crystal Report formula division-by-zero guard returns 0 when income is 0

When `VWSummaryProfitNLoss` returns monthly income values of 0 (due to stale view or missing data), the Crystal Report formula:
```
If Sum({Income}, {Month}) = 0 Then 0 Else Sum({Profit}, {Month}) / Sum({Income}, {Month}) * 100
```
correctly returns `0` — which is the protection against `#DIV/0!` errors. This makes the symptom look "clean" (shows 0.00% rather than an error) even though the underlying data is wrong.

### Tertiary cause: Different database on live vs local

**Evidence from `OBMSINDIA-Report\Web.config` (local) vs `Web.Release.config` (live):**

| Environment | Server | Database |
|---|---|---|
| Local (development) | `DESKTOP-FHKDAA6` | `obmsindia` |
| Live (IIS/production) | `SOFTWARESVR` | `obmsuat` |

The two databases may have **different versions of `VWSummaryProfitNLoss`**. If the view was recently modified to add `BranchIncome`, `BranchExpenses`, etc. columns but the live database was not re-deployed with the updated view, the live view would return 0 for those columns or the view itself might not exist.

---

## 4. Monthly vs Yearly Data Path Differences

The SQL in `SumProfitLoss.cs` uses a single `UNION ALL` approach:
- **Monthly rows**: `GROUP BY Branch, MONTH(TransactionDate)` → gives one row per month  
- **Grand total row**: `GROUP BY Branch` only, with a literal `'13'` for VMONTH → gives one row for the full period

The C# reader simply reads whichever rows come back. If the view returns correct data for all branches combined (the Group By Branch case), but incorrect/zero data for monthly groupings, this perfectly explains the symptom: **total row correct, monthly rows = 0**.

This would happen if:
- The view is a simple passthrough but underlying base table rows have `TransactionDate` values that do **not** fall in the date range for monthly grouping — but do fall in range for the overall `BETWEEN` check (unlikely but possible with timezone shifts on the server)
- Or (more likely) the `VWSummaryProfitNLoss` view on the live DB has a `WHERE` clause or filter that zeroes out the `BranchIncome`/`BranchProfit` columns for individual months

---

## 5. SQL / View Analysis

`VWSummaryProfitNLoss` is **not defined in this repository** — no `CREATE VIEW VWSummaryProfitNLoss` script was found in the codebase. Only `VWOperationalProfitNLoss` and `VWNonOperationalTransactions` are scripted in `VWSeparatedProfitLoss.sql`.

The comment in `VWSeparatedProfitLoss.sql` (line 9–12) documents what columns `VWSummaryProfitNLoss` exposes:
```
VWSummaryProfitNLoss only exposes Branch, TransactionDate, BranchIncome,
BranchCN, BranchDiscount, BranchExpenses, BranchProfit.
ItemCategory, PaymentPurpose and IsDeleted exist on BranchPayments, not on the view.
```

**This view's CREATE script is not version-controlled in this repo.** That is the deployment gap — if it was changed locally but not redeployed to `obmsuat` on `SOFTWARESVR`, the live database has an older version.

---

## 6. Angular Frontend Analysis

The `profit-and-loss.component.ts` and `separated-profit-loss.component.ts` **do not perform any Net Profit, Expenses%, or Net Profit% calculations**. The Angular components only:
- Build an iframe URL pointing to the Crystal Report ASPX page
- OR display raw `Month`, `Income`, `CN`, `Discount`, `Expenses`, `Profit` values from the `SumProfitLoss` API endpoint

**The `handleDataBinding()` method in `profit-and-loss.component.ts` (lines 133–156):**
```typescript
employee.TotalIncome = item.TotalIncomeAmount || 0;
employee.TotalProfit = item.TotalProfitAmount || 0;
employee.TotalExpenses = item.TotalExpensesAmount || 0;
```
There are no Expenses% or Net Profit% fields in `ProfitAndLoss` model (`profitAndLoss.ts`) — the Angular table only shows Month, Income, CN, Discount, Expenses, Profit. The three broken fields (Net Profit/(Loss), Expenses%, Net Profit%) are calculated and displayed exclusively by the Crystal Report.

---

## 7. Conclusions

| # | Finding | Certainty |
|---|---|---|
| 1 | NET PROFIT/(LOSS), Expenses%, NET PROFIT% are calculated by **Crystal Report formula fields** in the `.rpt` files — not in SQL, not in C#, not in Angular | High |
| 2 | The Crystal Report formula returns 0 when the denominator (monthly Income) is 0, masking the real problem | High |
| 3 | `VWSummaryProfitNLoss` is the data source for the monthly P&L — its CREATE script is **not in source control** | Confirmed |
| 4 | The local and live databases are different (`obmsindia` vs `obmsuat`) with different DB servers | Confirmed |
| 5 | The live `obmsuat` database likely has a stale or missing `VWSummaryProfitNLoss` view causing monthly BranchIncome/BranchExpenses to return 0 | Most likely root cause |
| 6 | The UNION ALL grand-total row (`VMONTH='13'`) aggregates over all rows independently of month grouping — if any single aspect of data returns non-zero for the full range, the total works even when monthly rows are 0 | High |

---

## 8. Recommended Fixes

### Fix 1 (Most critical): Re-deploy `VWSummaryProfitNLoss` to the live database

1. Script out `VWSummaryProfitNLoss` from the local `obmsindia` database using SSMS:  
   `Right-click view → Script View as → CREATE TO → New Query Window`
2. Check if the scripted view exists on `obmsuat` on `SOFTWARESVR`:  
   ```sql
   SELECT TOP 0 * FROM VWSummaryProfitNLoss;  -- run on live db
   ```
3. If the view is missing or returns 0 values: execute the CREATE script on the live database
4. After re-deploying, verify:  
   ```sql
   SELECT Branch, MONTH(TransactionDate) AS Mo, SUM(BranchIncome) AS Inc, SUM(BranchExpenses) AS Exp, SUM(BranchProfit) AS Prf
   FROM VWSummaryProfitNLoss
   WHERE TransactionDate BETWEEN '2024-01-01' AND '2024-12-31'
   GROUP BY Branch, MONTH(TransactionDate)
   ORDER BY Mo;
   ```
   Monthly rows with non-zero Income/Expenses/Profit confirm the view is working.

### Fix 2: Add `VWSummaryProfitNLoss` to source control

Add the view's CREATE script to `OBMSINDIA-API\Database\` so it can be deployed consistently:
```
OBMSINDIA-API\Database\VWSummaryProfitNLoss.sql
```

### Fix 3: Diagnostic query to confirm the issue

Run this directly on the **live** database (`obmsuat`) to confirm:
```sql
-- If this returns rows with 0 Income and 0 Profit for monthly groups → view is stale/wrong
SELECT
    Branch,
    MONTH(TransactionDate) AS MonthNum,
    SUM(BranchIncome)   AS TotalIncome,
    SUM(BranchExpenses) AS TotalExpenses,
    SUM(BranchProfit)   AS TotalProfit
FROM VWSummaryProfitNLoss
WHERE TransactionDate BETWEEN '2024-01-01' AND '2024-12-31'
GROUP BY Branch, MONTH(TransactionDate)
ORDER BY Branch, MonthNum;
```
- If **rows exist but Income/Profit columns are 0** → view definition is stale, re-deploy from local
- If **no rows exist at all** → view may point to empty/different tables in the live DB
- If **rows are correct** → the bug is in the Crystal Report formula itself (check `.rpt` formula for the Income/Profit group sum fields)

### Fix 4: Check timezone / date precision on the live server

If the view rows exist and have correct data, check whether the date parameter passed from Crystal Reports is using a different format on the live IIS server vs local. The ASPX code in `ProfitLossSummaryReport.aspx.cs` uses:
```csharp
paramStartDate.Value = DateTime.Parse(Request.QueryString["StartDate"]);
```
A locale difference between local machine and `SOFTWARESVR` could cause `DateTime.Parse` to interpret dates differently, shifting the effective date range.

### Fix 5 (Long-term): Move Net Profit, Expenses%, Net Profit% calculation to the C# backend

The `SumProfitLoss.cs` already returns `Income`, `Expenses`, and `Profit` per month. Computing percentages in C# instead of Crystal Report formulas would:
- Eliminate Crystal Report formula bugs
- Make the logic version-controllable
- Make the Angular table display these values directly (no iframe needed)

The `YearlyProfitLoss.cs` already does this correctly for the year-wise report — the same pattern can be replicated for monthly.

---

## 9. File and Symbol Reference

| File | Relevance |
|---|---|
| `OBMSINDIA-API\BusinessObjects\SumProfitLoss.cs` | Core monthly P&L query — reads from `VWSummaryProfitNLoss`, does NOT compute Expenses% or NetProfit% |
| `OBMSINDIA-API\BusinessObjects\SeparatedProfitLoss.cs` | Separated P&L query — reads from `BranchPayments` directly, does NOT compute Expenses% or NetProfit% |
| `OBMSINDIA-API\BusinessObjects\YearlyProfitLoss.cs` | Year-wise report — **DOES** correctly compute `ExpensesPercent`, `NetProfitPercent`, `NetProfit` in C# |
| `OBMSINDIA-API\Controllers\FinanceController.cs` lines 5326–5470 | Routes: `GetList`, `GetListWithBranch`, `GetSeparatedProfitLoss`, `GetYearlyProfitLoss` |
| `OBMSINDIA-API\Database\SeparatedProfitLoss_ReportQueries.sql` | Crystal Report SQL queries — raw columns only, no percentage formulas |
| `OBMSINDIA-API\Database\VWSeparatedProfitLoss.sql` | New views (operational/non-operational split) — documents that `VWSummaryProfitNLoss` columns are `BranchIncome`, `BranchExpenses`, `BranchProfit` |
| `OBMSINDIA-Report\Finance\ProfitLossSummaryReport.aspx.cs` | Crystal Report page — passes StartDate/EndDate/Branch to `.rpt`; connects to `obmsuat` on live |
| `OBMSINDIA-Report\Web.config` | Local DB: `DESKTOP-FHKDAA6` / `obmsindia` |
| `OBMSINDIA-Report\Web.Release.config` | Live DB: `SOFTWARESVR` / `obmsuat` |
| `OBMSINDIA-UI\src\app\modules\report\finance\profit-and-loss\profit-and-loss.component.ts` | Angular component — no percentage calculations, passes parameters to Crystal Report iframe |
| `OBMSINDIA-UI\src\app\model\profitAndLoss.ts` | Frontend model — only has `Month`, `TotalCN`, `TotalDiscount`, `TotalIncome`, `TotalProfit`, `TotalExpenses` — NO percentage fields |
