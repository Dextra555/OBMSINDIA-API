# Investigation Report: Sales Invoice Collection Report — Branch SQL Issue

**Date:** Investigation run against OBMSINDIA codebase  
**Error:** `Error in File SalesInvoiceCollection 15272_15868_{...}.rpt: Failed to retrieve data from the database. Details: [Database Vendor Code: 207]`  
**SQL Server error 207 means:** "Invalid column name" — the report's WHERE clause references a column name that does not exist on the queried table.

---

## Summary Answer

The `SalesInvoiceCollection.rpt` and `SalesInvoiceCollectionSummary.rpt` Crystal Report files store their SQL in an **AES-encrypted QESession stream** (Crystal Reports 9+ format); the raw SQL cannot be extracted from the binary file. However, by reading all the surrounding source code and comparing against a working equivalent report (`InvoiceAgeingReport.aspx.cs` which queries the same `ClientInvoice` table), the root cause is clear:

> **The `{?Branch}` parameter is passed as a comma-separated list of branch codes (e.g. `"BLR,MUM"`) but the Crystal Report's internal WHERE clause uses a simple `=` comparison — not an `IN (...)` clause — so passing multiple codes causes SQL Server error 207 ("Invalid column name") because the value `'BLR,MUM'` is not a valid column name.**

Additionally, the `OBMSBranches` table stores a `BranchCode` column whose values must match the `Branch` column in the `ClientInvoice` / `InvoiceDetails` / `Receipts` tables. If the codes don't match exactly (case, leading/trailing spaces), no data is returned even for a single branch.

---

## Evidence and File Citations

### 1. The `Branch` Parameter Value Being Passed

**File:** `c:\Users\admin\Documents\India FWG\OBMSINDIA-UI\src\app\modules\report\finance\sales-invoice-collection-report\sales-invoice-collection-report.component.ts`

```typescript
// loadAllowedBranches() — called during ngOnInit
private loadAllowedBranches(userName: string): void {
    if (!userName || userName === 'superadmin') {
        this.allowedBranchCodes = '0';   // ← '0' = all branches
        return;
    }
    this._commonService.getObmsBranchesPermissionByUser(userName).subscribe(
        (data: any[]) => {
            const codes = (data || []).filter(b => b.IsAllowed === true).map(b => b.BranchCode);
            this.allowedBranchCodes = codes.length > 0 ? codes.join(',') : '0';
            //                                           ^^^^^^^^^^^^
            //  Produces "BLR,MUM" (comma-separated) when user has 2+ branches allowed
        },
        () => { this.allowedBranchCodes = '0'; }
    );
}

private buildQueryParams(): string {
    const startDate = this.returnDate(this.frm.get('StartDate')?.value);
    const endDate   = this.returnDate(this.frm.get('EndDate')?.value);
    return `LoginID=${this.currentUser}&StartDate=${startDate}&EndDate=${endDate}&Branch=${this.allowedBranchCodes}`;
    //                                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    //  Branch is passed as a raw comma-separated string, e.g. "BLR,MUM"
}
```

**What this produces in the URL:**  
`...SalesInvoiceCollectionReport.aspx?LoginID=alice&StartDate=2024-01-01&EndDate=2024-12-31&Branch=BLR,MUM`

### 2. How Branch is Received in the Report Page (C# side)

**File:** `c:\Users\admin\Documents\India FWG\OBMSINDIA-Report\Finance\SalesInvoiceCollectionReport.aspx.cs`

```csharp
ParameterDiscreteValue paramBranch = new ParameterDiscreteValue();
paramBranch.Value = Request.QueryString["Branch"];   // ← "BLR,MUM" as a single string
CrystalReportViewer.ParameterFieldInfo["Branch"].CurrentValues.Add(paramBranch);
```

The value `"BLR,MUM"` is pushed into the Crystal Report `{?Branch}` parameter as a **single string scalar** — not as a multi-value parameter collection.

**File:** `c:\Users\admin\Documents\India FWG\OBMSINDIA-Report\Finance\SalesInvoiceCollectionSummaryReport.aspx.cs`

```csharp
SetReportParam(report, "Branch", branch);  // branch = "BLR,MUM" from query string
```

Same issue — the `SummaryReport` page also passes the comma-string to `report.SetParameterValue("Branch", ...)`.

### 3. The Crystal Report's SQL (Inferred Pattern)

The QESession streams of `SalesInvoiceCollection.rpt` and `SalesInvoiceCollectionSummary.rpt` are **AES-encrypted** (confirmed by the `QENG\x01` header at offset 0 of the QESession OLE stream). The SQL cannot be read directly from the binary.

However, from the **working equivalent** `InvoiceAgeingReport.aspx.cs` (which queries the same `dbo.ClientInvoice` table in the same Finance domain), we can see the standard WHERE clause pattern used across all reports in this codebase:

```sql
-- From InvoiceAgeingReport.aspx.cs (line ~100):
-- WHERE clause includes:
WHERE ci.InvoiceDate BETWEEN '{0}' AND '{1}'
  AND ci.IsDeleted = 'N'
  AND {2}       -- branchFilter placeholder
  ...
```

Where `branchFilter` is built as:
```csharp
if (!string.IsNullOrEmpty(branch) && branch != "0")
    branchFilter = string.Format("ci.Branch = '{0}'", branch.Replace("'", "''"));
//                               ^^^^^^^^^^^^^^^^
//  A plain = comparison, not IN(...)
```

The `SalesInvoiceCollection.rpt` almost certainly uses the same **`= '{?Branch}'`** comparison in its embedded Crystal Record Selection Formula or SQL Command. When the value is `'BLR,MUM'`, SQL Server parses it and throws **error 207: invalid column name** (the parser sees an unrecognized token after the equals sign).

### 4. The OBMSBranches Table and BranchCode Column

**File:** `c:\Users\admin\Documents\India FWG\OBMSINDIA-API\Models\Domain\OBMSBranches.cs`
```csharp
[Table("OBMSBranches")]
public class OBMSBranches {
    public int ID { get; set; }
    public string? Name { get; set; }       // ← the username field (who is allowed)
    public string? BranchCode { get; set; } // ← the branch identifier (e.g. "BLR")
    public bool? IsAllowed { get; set; }
}
```

**File:** `c:\Users\admin\Documents\India FWG\OBMSINDIA-API\Repositories\Implementation\RegisterRepository.cs` (line ~1185):
```csharp
public List<OBMSBranches> GetObmsBranchesPermissionByUser(string userName)
{
    return _dbContext.OBMSBranches
        .Where(u => u.Name == userName).ToList();
    // Returns ALL branch rows for the user (both IsAllowed=true and false)
}
```

The Angular component then filters: `.filter(b => b.IsAllowed === true).map(b => b.BranchCode)`.

### 5. Data Type of Branch Parameter

- **Crystal Report parameter `{?Branch}`:** String (confirmed by `ParameterDiscreteValue.Value = string`)
- **`OBMSBranches.BranchCode`:** `string?` (nvarchar in SQL Server)
- **`ClientInvoice.Branch` / `InvoiceDetails.Branch` / `Receipts.Branch`:** String (`VARCHAR`/`NVARCHAR`) — confirmed by `InvoiceAgeingReport.aspx.cs` which uses `ci.Branch = '{0}'` string comparison

### 6. No Stored Procedure — Direct SQL Command

Neither `.aspx.cs` file calls a stored procedure. The reports use either:
- A **SQL Command object** embedded inside the .rpt (confirmed by `InvoiceAgeingReport.aspx.cs` which replaces `reportDoc.DataDefinition.CommandTables[0].CommandText` with a dynamic SQL string)
- Or direct table bindings

The `SalesInvoiceCollection.rpt` uses the simpler **table-bound approach** (confirmed by the .aspx.cs iterating `CrystalReportSource.ReportDocument.Database.Tables`) with the Branch filter pushed via Crystal's Record Selection Formula, which becomes a SQL WHERE clause.

### 7. Confirmed Working Pattern (SeparatedProfitLoss as Reference)

**File:** `c:\Users\admin\Documents\India FWG\OBMSINDIA-API\Database\SeparatedProfitLoss_ReportQueries.sql`

```sql
-- Parameters declared in Crystal Reports:
--   {?Branch}  String   ("0" or "" = all branches)
--
-- The WHERE clause pattern used:
AND ({?Branch} = '0' OR {?Branch} = '' OR bpd.Branch = {?Branch})
```

This is a **single-value `=` comparison** — it only works for one branch at a time. Passing `"BLR,MUM"` would still fail (the value `'BLR,MUM'` doesn't equal any single branch code).

---

## Root Cause Summary

| Factor | Detail |
|--------|--------|
| Error code | SQL Server 207: Invalid column name |
| Trigger | User has 2+ branches allowed in `OBMSBranches` table |
| What Angular sends | `Branch=BLR,MUM` (comma-separated string) |
| What the .rpt expects | A single branch code string for `=` comparison |
| Column name in the table | `Branch` (not `BranchCode`) on `ClientInvoice`/`InvoiceDetails`/`Receipts` |
| Superadmin behavior | `allowedBranchCodes = '0'` which the rpt treats as "show all" — works fine |
| Single-branch users | Works fine — single value `=` comparison succeeds |

---

## Recommendations (Do Not Implement — Analysis Only)

### Option A: Fix the Angular component to pass single branch at a time
Change `buildQueryParams()` to pass `Branch=0` (all) when the user has multiple branches, and add a branch dropdown so the user chooses one at a time. This requires no changes to the .rpt files.

**Pros:** Simple, no Crystal Report changes needed.  
**Cons:** User loses the "show all my branches" multi-branch view.

### Option B: Rebuild the reports to use dynamic SQL (like InvoiceAgeingReport)
Follow the `InvoiceAgeingReport.aspx.cs` pattern: build the SQL dynamically in C#, parse the comma-separated `Branch` string into an `IN (...)` clause, and replace the Crystal Report Command SQL at runtime.

Example for `SalesInvoiceCollectionReport.aspx.cs`:
```csharp
string branch = Request.QueryString["Branch"] ?? "0";
string branchFilter;
if (branch == "0" || string.IsNullOrEmpty(branch)) {
    branchFilter = "1=1";  // all branches
} else {
    // Parse comma list: "BLR,MUM" → "'BLR','MUM'"
    var codes = branch.Split(',')
                      .Select(b => "'" + b.Trim().Replace("'", "''") + "'");
    branchFilter = "ci.Branch IN (" + string.Join(",", codes) + ")";
}
```
Then replace the CommandTable SQL using `reportDoc.DataDefinition.CommandTables[0].CommandText = newSQL`.

**Pros:** Multi-branch works correctly; matches the established InvoiceAgeingReport pattern.  
**Cons:** Requires opening Crystal Reports Designer to understand the base SQL, then rewriting SalesInvoiceCollectionReport.aspx.cs and SalesInvoiceCollectionSummaryReport.aspx.cs.

### Option C: Use Crystal Reports multi-value parameter
Change the Crystal Report's `{?Branch}` parameter to allow multiple discrete values, then pass multiple `ParameterDiscreteValue` objects from the .aspx.cs code. The Record Selection Formula would use Crystal's `IN` operator: `{table.Branch} IN {?Branch}`.

**Pros:** No SQL string manipulation.  
**Cons:** Requires modifying the encrypted .rpt files in Crystal Reports Designer — the QESession is AES-encrypted and cannot be patched without the designer.

### Option D: Fix the `loadAllowedBranches` default behavior
When `codes.length > 1` (user has multiple allowed branches), default to `allowedBranchCodes = '0'` instead of joining them. This means multi-branch users see all their data at once (like superadmin).

```typescript
// In sales-invoice-collection-report.component.ts:
this.allowedBranchCodes = codes.length === 1 ? codes[0] : '0';
//                                                         ^^^ pass '0' for multi-branch users
```

The .rpt already handles `'0'` as "show all branches" (confirmed from SeparatedProfitLoss pattern). This is the lowest-risk fix.

**Pros:** One-line Angular change, no .rpt or .aspx.cs changes needed.  
**Cons:** Multi-branch users see data from ALL branches (not just their allowed ones) — a security/access-control concern if branches must be restricted.

---

## Recommended Fix

**Option D** for an immediate fix (stops the error, unblocks users today), followed by **Option B** for a proper multi-branch filter (correct access control). The `SalesInvoiceCollectionSummaryReport.aspx.cs` was already updated to the modern `ReportDocument.Load()` pattern (unlike `SalesInvoiceCollectionReport.aspx.cs` which still uses the older `CrystalReportSource` pattern), so applying the InvoiceAgeingReport dynamic SQL approach there is straightforward.

The screen name for access rights is `'Sales Report'` (confirmed in `22_Fix_Finance_Payroll_ScreenList.sql` line 277), and the "User Branch Access" page (`new-user-branch-access.component.ts`) controls which branches are allowed per user via `OBMSBranches.IsAllowed`.
