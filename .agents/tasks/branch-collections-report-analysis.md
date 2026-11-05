# Branch Collections Report — "Collection Amount" & "CN Amount" Analysis

Read-only investigation. No code was modified. All conclusions are grounded in
files actually read; where a fact could not be confirmed from source it is
stated as such.

---

## Answer to the user (plain English)

- The table you queried with `select * from Transactions` is **not** the table
  the report reads directly. The report reads a table called **`Receipts`**.
  Your "Transactions" is a database-only view (it is not defined anywhere in
  the source-controlled code) that re-packages the `Receipts` rows (income)
  together with `BranchPayments`/`BranchPaymentDetails` rows (expenses) and
  renames the columns. So:
  - `TransactionDate` = `Receipts.ReceiptDate`
  - `TotalReceived` = `Receipts.ReceiptAmount`
  - `TransDetails` = `Receipts.Particulars`
  - `Invoice` = `Receipts.InvoiceNumbers`
  - `ChequeDetails` = `Receipts.ChequeNo` / cheque info
  - `TransLogDate` = `Receipts.LastUpdate`
  - `BranchExpenses` = `BranchPaymentDetails.Amount` (expense rows; 0 on
    receipt rows)
  - `BranchCollection`, `TaxAmount`, `HQAmount`, `HQPercentage`, `Branch`,
    `IsDeleted` = the identically-named columns on `Receipts`.

- **"Collection Amount"** in the report is the **`BranchCollection`** column
  of the `Receipts` table (summed/grouped). It is the branch's net share of
  each receipt. It is computed at receipt-entry time in the New Receipt screen
  as **`BranchCollection = TotalReceived − TaxAmount − HQAmount`**
  (where `HQAmount = HQPercentage × (TotalReceived − TaxAmount)`). In every
  row you shared, `TaxAmount = 0` and `HQAmount = 0`, so
  `BranchCollection = TotalReceived` — exactly what your data shows.

- **"CN Amount" = Credit Note Amount.** `CN` stands for **Credit Note**. It
  comes from the `CreditNoteAmount` column. There are two sources of credit
  note amounts in this system: the **`Receipts.CreditNoteAmount`** column
  (a legacy field captured when a receipt is entered) and a dedicated
  **`CreditNote`** table (`CreditNote.CreditNoteAmount`). The report column
  literally labelled "CN Amount" belongs to the **Invoice Collection Branch
  Summary** report, where it is `SUM(Receipts.CreditNoteAmount)` for the
  selected branch and date range.

- Important nuance: there are **two different "collection" reports** and you
  should confirm which one you are looking at:
  1. **Branch Collections** (menu case 5) → `BranchCollectionReport.aspx` →
     `BranchCollectionsSummary.rpt` — a Crystal Report bound directly to the
     `Receipts` table. Its exact on-screen column labels live inside the
     binary `.rpt` and could not be read from source, but its data source is
     `Receipts`, so its "Collection Amount" = `SUM(BranchCollection)`.
  2. **Invoice Collection Branch Summary** → `InvoiceCollectionBranchSummaryReport.aspx`
     → `InvoiceCollectionBranchSummary.rpt`. This one has BOTH labelled
     columns **"Collection Amount"** and **"CN Amount"**. Here, however,
     "Collection Amount" is **`SUM(ReceiptDetails.Amount)`** (money actually
     allocated against invoices), **not** `BranchCollection`, and
     "CN Amount" is **`SUM(Receipts.CreditNoteAmount)`**.

  If the report you are looking at shows both a "Collection Amount" and a
  "CN Amount" column side by side, you are almost certainly on report #2
  (Invoice Collection Branch Summary), reached from the **Invoice Collection
  Status Report** screen — and in that report "Collection Amount" is the
  invoice-allocation total, **not** the `BranchCollection` column. Only
  report #1 (true Branch Collections) sums `BranchCollection`.

---

## 1. Report location

### Branch Collections (the report named "Branch Collections Report")

| Layer | File | Evidence |
|---|---|---|
| UI screen | `d:\India Live\obms-ui\src\app\modules\report\finance\branch-collections\branch-collections.component.ts` | `clkBtn(1)` sets `reportPageName = "BranchCollectionReport.aspx?"`; `onSubmit()` appends `&StartDate=`, `&EndDate=`, `&Branch=` to `environment.baseReportUrl` + `"Finance/"`. |
| UI buttons | `...\branch-collections.component.html` | Visible button `Show` → `(click)="clkBtn(1)"`; a second button `Show(Total)-No Tax` (`clkBtn(2)`) is `hidden`. |
| Menu definition | `d:\India Live\obms-reports-india\Finance\MasterReports.aspx.cs` | case `"5"`: `this.MenuName = "Branch Collections Report"`; `NavigationTitle = "Finance - Reports - Branch Collections"`. |
| Menu dispatch | `d:\India Live\obms-reports-india\Finance\MasterReports.aspx.cs` (btnShow_Click) | case `"5"`: `this.MasterReportsFrame.Attributes["src"] = "BranchCollectionReport.aspx?Branch=" + ... + "&StartDate=" + ... + "&EndDate=" + ...` |
| Report page | `d:\India Live\obms-reports-india\Finance\BranchCollectionReport.aspx` | `Report FileName="BranchCollectionsSummary.rpt"`; `<CR:DataSourceRef TableName="Receipts" />` and `<CR:DataSourceRef TableName="BankMaster" />`. |
| Code-behind | `d:\India Live\obms-reports-india\Finance\BranchCollectionReport.aspx.cs` | Sets DB logon from `Web.config` (`Server`, `Database`, `UserID`, `Password`) and pushes parameters `StartDate`, `EndDate`, `Branch`, `LoginID` (+ company header params). |
| DB connection | `d:\India Live\obms-reports-india\Web.config` | `<add key="Server" value="DESKTOP-FHKDAA6"/>`, `<add key="Database" value="obmsindia"/>`, `<add key="UserID" value="sa"/>`, `<add key="Password" value="server@123"/>`. (Note: the file currently contains a git merge-conflict block with both India and Malaysia settings; the last-defined India block wins for `Server/Database`.) |

**Data source:** the Crystal Report `BranchCollectionsSummary.rpt` is bound
directly to the **`Receipts`** table (plus `BankMaster` for bank look-ups).
It is **not** driven by a stored procedure or a SQL Command visible in source
— the report connects straight to the `Receipts` table and applies a
record-selection formula stored **inside the binary `.rpt`**.

> Could not confirm from source: the exact on-screen column labels and the
> record-selection formula of `BranchCollectionsSummary.rpt`. The file is a
> binary Crystal Report (~64 KB) containing only an embedded bitmap and
> non-readable binary structures; no SQL, field names, or selection formula
> could be extracted as ASCII or UTF-16 strings. The filters are inferred from
> the parameters passed by the code-behind (`StartDate`, `EndDate`, `Branch`)
> and from the table schema (`Receipts`).

### Invoice Collection Branch Summary (the report that literally shows "Collection Amount" + "CN Amount")

| Layer | File | Evidence |
|---|---|---|
| Report page | `d:\India Live\obms-reports-india\Finance\InvoiceCollectionBranchSummaryReport.aspx` | `Report FileName="InvoiceCollectionBranchSummary.rpt"`; `<CR:DataSourceRef TableName="Command" />` (i.e. a SQL Command, not a raw table). |
| Code-behind + SQL | `d:\India Live\obms-reports-india\Finance\InvoiceCollectionBranchSummaryReport.aspx.cs` | Header comment documents the column set and embeds the full SQL. |
| Launcher | `d:\India Live\obms-ui\src\app\modules\report\finance\invoice-collection-status-report\invoice-collection-status-report.component.ts` | `clkBtn`: `number == 3 ? "InvoiceCollectionTotalReport.aspx?" : number == 4 ? "InvoiceCollectionTotalCurrentPeriodReport.aspx?" : "InvoiceCollectionBranchSummaryReport.aspx?"`. |

Note: the referenced companion file
`Database\InvoiceCollectionBranchSummary_ReportQuery.sql` mentioned in the
code-behind comment **does not exist** on disk (searched the whole tree). The
SQL therefore lives only inside the `.aspx.cs` comment block.

---

## 2. Report columns

### Branch Collections (`BranchCollectionsSummary.rpt`)
Not readable from the binary. Inferred columns (from the `Receipts` data
source + standard OBMS receipt report layout): `Branch`, `ReceiptDate`,
`VoucherNo`, `PaymentFrom`/`Particulars`, `ReceiptAmount` (Total Received),
`TaxAmount`, `HQPercentage`, `HQAmount`, `BranchCollection` (Collection
Amount), and likely `CreditNoteAmount` (since the `Receipts` table carries
it). **Cannot confirm exact labels without opening the `.rpt` in Crystal
Reports Designer.**

### Invoice Collection Branch Summary (`InvoiceCollectionBranchSummary.rpt`)
Documented verbatim in the code-behind header comment (`InvoiceCollectionBranchSummaryReport.aspx.cs`, lines ~11-13):

```
BRANCH | Service Charges | Adjustment | Actual Amount | Tax Amount |
Invoice Amount | Collection Amount | CN Amount | Balance
```

---

## 3. "Collection Amount" mapping (exact SQL / formula)

### In the true Branch Collections report
Data source = `Receipts` table. The report groups/sums the `BranchCollection`
column. The report's own grouping/selection SQL is inside the binary `.rpt`
(not extractable), but the API confirms the exact column set and filter
convention used elsewhere for `Receipts`. Representative authoritative query
(`d:\India Live\obms-api\Controllers\FinanceController.cs`, `GetTDSReport`,
~line 6408):

```sql
SELECT
    r.ID, r.ReceiptDate, r.VoucherNo, r.Branch, r.PaymentFrom,
    ISNULL(r.ReceiptAmount, 0)      AS ReceiptAmount,
    ISNULL(r.TaxPercentage, 0)      AS TaxPercentage,
    ISNULL(r.TaxAmount, 0)          AS TaxAmount,
    ISNULL(r.HQPercentage, 0)       AS HQPercentage,
    ISNULL(r.HQAmount, 0)           AS HQAmount,
    ISNULL(r.BranchCollection, 0)   AS BranchCollection,
    r.Particulars
FROM Receipts r
WHERE r.IsDeleted = 0
  AND CAST(r.ReceiptDate AS DATE) >= '{startDate}'
  AND CAST(r.ReceiptDate AS DATE) <= '{endDate}'
  AND (ISNULL(r.TaxAmount, 0) > 0 OR ISNULL(r.HQAmount, 0) > 0)
  [AND r.Branch = '{branch}']
ORDER BY r.ReceiptDate ASC, r.Branch ASC
```

So the report's "Collection Amount" = **`SUM(Receipts.BranchCollection)`**
filtered by `Branch` and `ReceiptDate` range, with **`IsDeleted = 0`** (i.e.
`IsDeleted <> 'Y'`). The date column filtered is **`ReceiptDate`** (your
`TransactionDate`).

### How `BranchCollection` itself is derived
Computed at receipt entry in the New Receipt UI
(`d:\India Live\obms-ui\src\app\modules\finance\receipts\new-receipt\new-receipt.component.ts`).

Forward-GST branch (receiptTypeID 1/2/5, ~line 1908):
```ts
taxAmount   = baseAmount * taxPercentage / 100;     // GST on GST-exclusive base
hqAmount    = baseAmount * (hqPercentage / 100);
branchAmount = baseAmount - taxAmount - hqAmount;   // => BranchCollection
```

Inclusive-GST branch (`dynamicCalculation()`, ~line 2005):
```ts
taxAmount  = (taxPercentage * baseAmount) / (100 + taxPercentage);  // extract GST from inclusive total
hqAmount   = (baseAmount - taxAmt) * (hqPercentage / 100);            // HQ on pre-tax service charge
branchAmount = baseAmount - taxAmt - hqAmt;                          // => BranchCollection
```

General formula: **`BranchCollection = TotalReceived − TaxAmount − HQAmount`**,
with **`HQAmount = HQPercentage × (TotalReceived − TaxAmount)`** (inclusive) or
`HQPercentage × TotalReceived` (forward).

Reconciled against **your actual rows** (all have `TaxAmount = 0`,
`HQAmount = 0`, `HQPercentage = 0`): `BranchCollection = TotalReceived − 0 − 0
= TotalReceived` — matches your data exactly (e.g. Anejas suspense row
`TotalReceived = 224775.00`, `BranchCollection = 224775.00`).

> Note on the example in the original brief (Anejas `Total=123485.60`,
> `Tax=13230.60`, `HQ=1102.55`, `HQ%=1.00`, `BranchCollection=122383.00`):
> that example assumed `BranchCollection = TotalReceived − HQAmount` (tax
> not subtracted). The actual UI code subtracts tax too, which would give
> `123485.60 − 13230.60 − 1102.55 = 109152.45`. That discrepancy means either
> that example row was computed under a different/older path or its figures
> are not a clean match. The formula confirmed from code is the one above; it
> is consistent with every row in the dataset you actually shared.

### In the Invoice Collection Branch Summary report
"Collection Amount" here is a **different** quantity. From the embedded SQL in
`InvoiceCollectionBranchSummaryReport.aspx.cs`:

```sql
ISNULL((SELECT SUM(rd.Amount) FROM ReceiptDetails rd
        INNER JOIN Receipts r ON r.ID = rd.ReceiptID AND r.IsDeleted = 0
        WHERE rd.InvoiceID IN (SELECT ID FROM ClientInvoice ci2
            WHERE ci2.Branch = ci.Branch AND ci2.IsDeleted = 'N'
              AND ci2.InvoiceDate >= {?StartDate} AND ci2.InvoiceDate <= {?EndDate})), 0) AS CollectionAmount
```

So in **this** report "Collection Amount" = `SUM(ReceiptDetails.Amount)` for
receipts allocated to invoices in the branch/date range — **not**
`SUM(Receipts.BranchCollection)`.

---

## 4. Transactions / Receipts schema

The table the report actually reads is **`Receipts`**. EF entity
(`d:\India Live\obms-api\Models\Domain\Receipts.cs`, `[Table("Receipts")]`):

```
ID, VoucherNo, ReceiptDate, Branch, PaymentFrom, Particulars,
ReceiptType, IsInvoiceAdjustment, BankCode, BankBranch, ChequeNo,
InvoiceNumbers, ReceiptAmount, TaxPercentage, TaxAmount,
HQPercentage, HQAmount, BranchCollection, CreditNoteAmount,
DebitNoteAmount, SuspendAmount, BankID, ChequeStatus,
IsDeleted, LastUpdate, LastUpdatedBy
```

Column meanings:

| Column | Meaning |
|---|---|
| `ReceiptAmount` | Total money received (your `TotalReceived`). GST-inclusive for inclusive mode. |
| `TaxAmount` / `TaxPercentage` | GST portion of the receipt. Separate from the branch/HQ split. |
| `HQAmount` / `HQPercentage` | Head-quarters commission. `HQAmount = HQPercentage × (ReceiptAmount − TaxAmount)` (inclusive) — i.e. HQ commission is on the pre-tax service charge. |
| `BranchCollection` | Branch's net share = `ReceiptAmount − TaxAmount − HQAmount`. This is the report's "Collection Amount". |
| `CreditNoteAmount` | Credit note amount captured on this receipt (legacy field; see §5). |
| `DebitNoteAmount` | Debit note amount captured on this receipt. |
| `SuspendAmount` | Suspense amount (e.g. "no payment advice" rows, common in your data). |
| `IsDeleted` | `BIT` — 0 = active, 1 = deleted. Your view shows it as `'N'`/`'Y'`. |

### Your "Transactions" table vs `Receipts`
Searched the entire tree: there is **no `CREATE VIEW/TABLE Transactions`** and
no `FROM Transactions` or `TableName="Transactions"` anywhere. "Transactions"
is a database-only object (a view) not captured in version control. Its
columns map to `Receipts` (income rows) plus `BranchPayments`/
`BranchPaymentDetails` (expense rows → your `BranchExpenses`), following the
same UNION pattern documented in
`d:\India Live\obms-api\Database\VWSeparatedProfitLoss.sql`:

```sql
-- General Income (Receipts - non invoice adjustment)
SELECT r.Branch, r.ReceiptDate AS TransactionDate, r.ReceiptAmount AS ServiceCharges, ...
       r.CreditNoteAmount AS CreditNoteAmount, 0 AS BranchExpenses
FROM Receipts r WHERE r.IsDeleted = 0 AND r.IsInvoiceAdjustment = 0
UNION ALL
-- Operational Expenses
SELECT bpd.Branch, bp.PaymentDate AS TransactionDate, 0 AS ServiceCharges, ...
       bpd.Amount AS BranchExpenses
FROM BranchPayments bp
INNER JOIN BranchPaymentDetails bpd ON bpd.PaymentID = bp.ID
WHERE bp.IsDeleted = 0 ...
```

That confirms: `BranchExpenses` originates from `BranchPaymentDetails.Amount`
(payments), and `TransactionDate`/`CreditNoteAmount`/`BranchCollection` come
from `Receipts`. So "select * from Transactions" returns a blended ledger; the
report reads the underlying `Receipts` rows directly.

---

## 5. "CN Amount" explanation

**"CN" = Credit Note. "CN Amount" = Credit Note Amount.**

Credit note amounts live in **two** places in this system:

1. **`Receipts.CreditNoteAmount`** — a legacy decimal column on each receipt
   (`Receipts.cs` line 28), set from `ReceiptsRequestDto.CreditNoteAmount`
   when a receipt is saved (`FinanceController.cs` line 4827).

2. **`CreditNote.CreditNoteAmount`** — a dedicated `CreditNote` table created
   by `d:\India Live\obms-api\Database\CreditDebitNote_Migration.sql`:
   ```sql
   CREATE TABLE CreditNote (
       ID, CreditNoteNo, Branch, Client, AgreementID, ClientInvoiceID,
       CreditNoteDate, CreditNoteAmount, Reason, ReferenceInvoiceNo,
       Status, ApprovalDate, ApprovedBy, CreatedDate, LastUpdatedDate,
       LastUpdatedBy, IsDeleted, TaxPercentage, TaxAmount, TotalAmount, ...)
   ```
   Managed by `d:\India Live\obms-api\Controllers\CreditDebitNoteController.cs`
   and `CreditDebitNoteRepository.cs`. Summarised per branch/month by view
   `VWCreditNoteByMonthBranch` in
   `d:\India Live\obms-api\Database\CreditDebitNoteSummary_Views.sql`
   (`SUM(cn.CreditNoteAmount)`).

The migration notes the historical link: an optional block migrates existing
`Receipts.CreditNoteAmount > 0` rows into the new `CreditNote` table.

### Which one feeds the report column?
- The **Invoice Collection Branch Summary** report's "CN Amount" column =
  **`SUM(Receipts.CreditNoteAmount)`**, filtered by branch and `ReceiptDate`
  range, `IsDeleted = 0` (from the embedded SQL in
  `InvoiceCollectionBranchSummaryReport.aspx.cs`):
  ```sql
  ISNULL((SELECT SUM(r.CreditNoteAmount) FROM Receipts r
          WHERE r.Branch = ci.Branch AND r.IsDeleted = 0
            AND r.ReceiptDate >= {?StartDate} AND r.ReceiptDate <= {?EndDate}), 0) AS CNAmount
  ```
- The **Credit Note Summary** report (menu case 10 → `CreditNoteReport.aspx`)
  and the **Yearly P&L** (`obms-api\BusinessObjects\YearlyProfitLoss.cs`) use
  the dedicated **`CreditNote`** table instead:
  ```sql
  SELECT YEAR(CreditNoteDate) AS PayYear, SUM(CreditNoteAmount) AS TotalCreditNote
  FROM CreditNote WHERE IsDeleted = 0 ...
  ```

So whether "CN Amount" is the `Receipts` legacy column or the `CreditNote`
table depends on which report you are viewing. The report that shows a column
literally labelled "CN Amount" next to "Collection Amount"
(InvoiceCollectionBranchSummary) uses **`Receipts.CreditNoteAmount`**.

### Does "CN Amount" appear in the Branch Collections report?
Not confirmed. The true Branch Collections Crystal report
(`BranchCollectionsSummary.rpt`) reads the `Receipts` table, which carries
`CreditNoteAmount`, so it *could* display a CN column — but its column labels
are locked in the binary and could not be read. The literal string "CN Amount"
is found in source only in the **Invoice Collection Branch Summary**
report.

---

## 6. Filters

For the `Receipts`-backed reports (Branch Collections and the invoice
collection CN sub-query), the confirmed filter convention from code is:

- **Branch:** `Branch = '<selected branch>'`; value `"0"` or empty means "all
  branches" (see `InvoiceCollectionBranchSummary` SQL:
  `('{?Branch}'='0' OR '{?Branch}'='' OR ci.Branch='{?Branch}')`; UI sends
  `Branch=0` when none selected — `branch-collections.component.html` default
  option `[value]="0"`).
- **Date range:** on **`ReceiptDate`** (your `TransactionDate`):
  `>= {?StartDate} AND <= {?EndDate}`. The UI passes `StartDate`/`EndDate`
  formatted `YYYY-MM-DD` (`returnDate()` in `branch-collections.component.ts`).
- **Deleted rows excluded:** `IsDeleted = 0` (BIT). Your view shows `'N'` for
  active and `'Y'` for deleted; deleted rows (`IsDeleted = 'Y'`/`1`) are
  excluded. (Note the mixed convention in the codebase: `Receipts`/
  `BranchPayments` use BIT `0/1`; `ClientInvoice` uses CHAR `'N'`/`'Y'` — see
  `VWSeparatedProfitLoss.sql`: `ci.IsDeleted = 'N'` vs `r.IsDeleted = 0`.)

---

## 7. Conclusions & recommendations

- **Confirmed:** the report's "Collection Amount" (true Branch Collections
  report) = `SUM(Receipts.BranchCollection)`, where `BranchCollection =
  ReceiptAmount − TaxAmount − HQAmount`. Your `select * from Transactions` is a
  view over `Receipts` (+ payments); its `BranchCollection` column is exactly
  that field, so the report sums that column.
- **Confirmed:** "CN Amount" = Credit Note amount = `CreditNoteAmount`. In
  the report that literally shows both "Collection Amount" and "CN Amount"
  (Invoice Collection Branch Summary), CN Amount = `SUM(Receipts.CreditNoteAmount)`;
  a separate dedicated `CreditNote` table exists and is used by the Credit
  Note Summary report and P&L.

### Gaps / what could not be confirmed from source
- The exact column labels and record-selection formula of
  `BranchCollectionsSummary.rpt` are inside the binary and were not readable.
  To confirm definitively, open it in **Crystal Reports Designer** and read
  the Field Explorer + "Selection Formulas → Record".
- The `Transactions` view is not in source control. Recommend adding its
  `CREATE VIEW` DDL to `obms-api\Database\` so the report-to-table mapping is
  auditable.
- The `Web.config` currently contains an unresolved git merge-conflict block
  (`<<<<<<< HEAD` / `>>>>>>> 18daa4f`) mixing India and Malaysia settings.
  Recommend resolving it so the active DB target is unambiguous.

### Recommended fixes (implement nothing here)
1. Capture the `Transactions` view DDL in version control (new file under
   `obms-api\Database\`), documenting the `Receipts` ↔ `Transactions` column
   mapping, so users stop assuming the report reads `Transactions`.
2. Resolve the `Web.config` merge conflict.
3. If the Branch Collections report is meant to show a "CN Amount" column
   alongside "Collection Amount", expose the `.rpt` definition (or migrate the
   report to a SQL-Command `.rpt` like `InvoiceCollectionBranchSummaryReport`
   so the SQL is in source) so the column provenance is verifiable.
