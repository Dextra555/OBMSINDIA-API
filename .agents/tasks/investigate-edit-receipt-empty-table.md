# Investigation Report: Edit Receipt — Empty Invoice Table

## Summary Answer

**Root cause: Three compounding bugs.**

The primary bug is that `changeReceiptType()` is called during `getReceiptData()` and it **resets ChequeAmount / CashAmount / ibgAmount to 0** before the amount values are ever restored to the form. Because the component then calls `calculation()` on each invoice row, and `isCheckboxDisabled()` returns `true` when all amounts are 0, checkboxes stay disabled. More importantly, even though `invoiceList` is populated with the real invoice rows, the HTML table **hides every row whose `Balance === 0`**, and because `ibgAmount` was zeroed out, the `total_invoice_amount` calculation produces `0`, making `balance_amount = 0 - 0 = 0`, which further confuses displayed totals. In addition, two secondary bugs prevent any amount from being restored at all:

1. `ibgAmount`, `ChequeAmount`, and `CashAmount` are **commented out** in the `patchValue` call inside `getReceiptData()` — so even if the type-reset were fixed, the saved amounts are never written back into the form.
2. A **typo** (`BalancealanceAmount` instead of `BalanceAmount`) causes the balance field to always receive `undefined` from the API response.

The backend API itself **does** return the correct `details` array — the invoice rows exist in `data.receipt.details` — so the data source is not the problem. The issue is entirely in the frontend `getReceiptData` / `changeReceiptType` sequencing and the commented-out field mappings.

---

## Evidence

### 1. Backend returns correct data

**File:** `d:\India Live\obms-api\Repositories\Implementation\FinanceRepository.cs`, lines 365–423

```csharp
public async Task<ReceiptsRequestDto?> GetReceiptAsync(int id)
{
    var receipt = await _oBMSDbContext.Receipts
        .Where(r => !r.IsDeleted && r.ID == id)
        .FirstOrDefaultAsync();

    var details = await (from d in _oBMSDbContext.ReceiptDetails
                         join c in _oBMSDbContext.ClientInvoices
                             on d.InvoiceID equals c.ID
                         where d.ReceiptID == id
                         select new ReceiptDetailRequestDto
                         {
                             ID = d.ID,
                             ReceiptID = d.ReceiptID,
                             InvoiceID = d.InvoiceID,
                             PaidAmount = d.Amount,
                             InvoiceNo = c.InvoiceNo,
                             InvoiceAmount = c.ServiceCharges - c.Discount + c.TaxAmount,
                             ServiceAmount = c.ServiceCharges - c.Discount,
                             TaxAmount = c.TaxAmount,
                             InvoiceDate = c.InvoiceDate
                         }).ToArrayAsync();

    var receiptDto = new ReceiptsRequestDto { ..., details = details };
    return receiptDto;
}
```

The DTO (`ReceiptsRequestDto`) includes `ReceiptAmount`, `ReceiptType`, `IsInvoiceAdjustment`, and `details[]`. All are properly populated. **The API is not the bug.**

**Note:** `ReceiptsRequestDto` does NOT contain a `TotalInvoiceAmount`, `BalanceAmount`, or `client` property — these are computed fields that exist only in the frontend. References to them on the API response always return `undefined`.

---

### 2. Bug #1 — `changeReceiptType()` unconditionally zeroes out all amount fields

**File:** `d:\India Live\obms-ui\src\app\modules\finance\receipts\new-receipt\new-receipt.component.ts`, line ~1631

```ts
changeReceiptType(value: string): void {
    this.receiptTypeID = value;
    const amount = this.frm.get("ReceiptAmount")?.value || 0;  // reads ReceiptAmount

    this.frm.patchValue({
        ChequeAmount: 0,
        CashAmount: 0,
        ibgAmount: 0           // ← ALL amounts reset to 0
    });

    // For IBG (type 5):
    } else if (value?.toString() === '5') {
        this.frm.patchValue({ ibgAmount: amount });  // sets ibgAmount = ReceiptAmount
    }
    this.calculation();
}
```

This is called at line ~754 inside `getReceiptData()`:

```ts
this.changeReceiptType(data.receipt.ReceiptType);  // <-- called here
```

At this point in the execution sequence, `ReceiptAmount` has been patched correctly from `data.receipt.ReceiptAmount`. So for an IBG receipt, `changeReceiptType('5')` should read `ReceiptAmount` and assign it to `ibgAmount`. **This part would actually work correctly IF the patchValue for ReceiptAmount had been applied first.**

However: the form `patchValue` block on lines ~640–705 does set `ReceiptAmount: data.receipt.ReceiptAmount`. So `ibgAmount` should be set from `ReceiptAmount`. **But see Bug #2 — the specific amount fields are commented out.**

---

### 3. Bug #2 — Amount fields are commented out in `patchValue`

**File:** `new-receipt.component.ts`, lines ~654–660

```ts
this.frm.patchValue({
    ...
    ReceiptAmount: data.receipt.ReceiptAmount,
    // ibgAmount: data.receipt.ReceiptAmount,    // ← COMMENTED OUT
    // ChequeAmount: data.receipt.ChequeAmount,  // ← COMMENTED OUT
    // CashAmount: data.receipt.CashAmount,      // ← COMMENTED OUT
    ...
});
```

The developer commented out the direct assignment. The intent was to rely on `changeReceiptType()` to distribute `ReceiptAmount` into the correct type-specific field. This mechanism would work only if `changeReceiptType` reads `ReceiptAmount` from the form — and it does — but it reads it *before* the patchValue executes because JavaScript's `patchValue` is synchronous. Wait — actually `patchValue` IS synchronous, so `ReceiptAmount` IS set in the form before `changeReceiptType` is called on line 754.

Let me refine: the real problem is the **sequence of calls** in `getReceiptData()`:

```ts
// Step A: patchValue sets ReceiptAmount = 256144 (correct)
this.frm.patchValue({ ReceiptAmount: data.receipt.ReceiptAmount, ... });

// Step B: changeReceiptType reads ReceiptAmount (=256144), sets ibgAmount=256144 ✓
this.changeReceiptType(data.receipt.ReceiptType);   // ibgAmount = 256144 now

// Step C: branchChange triggers an async HTTP call to GetClientByBranch
this.branchChange(data.receipt.Branch);
```

Inside `branchChange()`:

```ts
branchChange(value: any) {
    this.service.GetClientByBranch(value).subscribe((d: any) => {
        this.clientList = d;
        // finds the client, calls applyClientTds → frm.patchValue for HQPercentage, TaxPercentage
        const client = this.clientList.find(x => x.Name === this.frm.get('PaymentFrom')?.value);
        if (client) { this.applyClientTds(client); }

        // Then patchValue the client code
        this.frm.patchValue({ client: code });
    });
}
```

After `branchChange` completes asynchronously, `frm.patchValue({ client: code })` fires. The `client` form control is set to a code string. But then — **this is the critical point** — the component also separately calls:

```ts
// Step D: tries to set client again from receipt.client
this.frm.patchValue({ client: data.receipt.client });  // data.receipt.client is UNDEFINED
```

`data.receipt.client` does not exist in `ReceiptsRequestDto` (the DTO has no `client` property). So this sets `frm.client = undefined/null`, overwriting whatever `branchChange` had just set.

Since `client` is null/empty after this, the HTML condition `*ngIf="frm.get('client')?.value"` that wraps the invoice table is **false**, hiding the entire table even if `showInvoiceTable` is true and `rows.controls` has data.

---

### 4. Bug #3 — Typo: `BalancealanceAmount` instead of `BalanceAmount`

**File:** `new-receipt.component.ts`, line ~668

```ts
balance_amount: data.receipt.BalancealanceAmount,   // ← typo, always undefined
```

`ReceiptsRequestDto` has no property named `BalancealanceAmount`. The correct property is also not in the DTO since `BalanceAmount` is a computed UI value, but this still causes `balance_amount` to be set to `undefined` (shows as empty or NaN).

---

### 5. HTML table gating conditions (two separate guards)

**File:** `new-receipt.component.html`

**Guard 1 — outer `*ngIf`:**
```html
<ng-container *ngIf="isAdjustment && showInvoiceTable">
  <div class="col-12 col-sm-12" *ngIf="frm.get('client')?.value">
```

The table only renders if ALL THREE are true:
- `isAdjustment` is truthy
- `showInvoiceTable` is true
- `frm.get('client')?.value` is non-empty

Because `data.receipt.client` is undefined (Bug #2 detail above), the `client` form value gets cleared, so `frm.get('client')?.value` is falsy → table is hidden.

**Guard 2 — row `[hidden]`:**
```html
<tr mat-row *matRowDef="let row; columns: displayedColumns;"
    [hidden]="row.value.Balance === 0 || row.value.Balance === '0' || +row.value.Balance === 0">
```

Balance is calculated in the component as:
```ts
d['Balance'] = Number(d['ServiceAmount']) - Number(d['PaidAmount'])
```

Where `PaidAmount = d.Amount` from `ReceiptDetails`. If the invoice was fully paid in this receipt, `Balance = 0` and the row is hidden. This is intentional design, but it means for a fully-paid receipt **all rows would be hidden** even if the table itself were visible. For receipt ID 141 with Amount=256144, this depends on whether the linked invoices' `ServiceAmount == d.Amount`.

---

### 6. `isCheckboxDisabled()` behaviour

**File:** `new-receipt.component.ts`, lines ~963–980

```ts
isCheckboxDisabled(): boolean {
    const ibgAmount = +this.frm.get('ibgAmount')?.value || 0;
    const chequeAmount = +this.frm.get('ChequeAmount')?.value || 0;
    return ibgAmount === 0 && chequeAmount === 0;
}
```

This disables checkboxes when both amounts are zero. After `changeReceiptType` runs, `ibgAmount` should hold the ReceiptAmount — so checkboxes should be enabled. But if the `client` form value is null and the table is hidden (Guard 1 above), this never even renders.

---

## Root Cause Chain (Ordered)

| # | Where | What | Effect |
|---|-------|------|--------|
| 1 | `ReceiptsRequestDto` | No `client` property | `data.receipt.client` is always `undefined` |
| 2 | `getReceiptData()` line ~780 | `this.frm.patchValue({ client: data.receipt.client })` | Sets `client` form field to `undefined`/`null` |
| 3 | HTML `*ngIf="frm.get('client')?.value"` | `client` is null | Invoice table `<div>` is hidden entirely — **primary visible symptom** |
| 4 | `getReceiptData()` patchValue block | `ibgAmount`, `ChequeAmount`, `CashAmount` are commented out | `changeReceiptType` still recovers `ibgAmount` from `ReceiptAmount`, so Amount field shows 0 only if `ReceiptAmount` wasn't set. Actually `ReceiptAmount` IS set, so `ibgAmount` should be non-zero — but the table still doesn't show because of Bug 2 |
| 5 | `BalancealanceAmount` typo | `balance_amount` always `undefined` | Balance Amount field shows stale/empty value |

---

## Conclusions

**The invoice table is empty because `frm.get('client')?.value` is falsy when editing.** The `client` code is set by `branchChange()` asynchronously, but then immediately overwritten with `undefined` by `this.frm.patchValue({ client: data.receipt.client })` since `ReceiptsRequestDto` has no `client` property.

The secondary symptom — Amount field showing `0` — could occur if `ReceiptAmount` is set in the form after `changeReceiptType()` is called, but based on code order it appears `ReceiptAmount` is set in the earlier `patchValue` block (synchronous), so `ibgAmount` should actually receive the correct value. The `Amount = 0` shown in the screenshot suggests either `ReceiptAmount` isn't being read correctly in `changeReceiptType`, or the UI is displaying `ibgAmount` which got wiped. Further runtime debugging would confirm.

---

## Recommended Fixes

### Fix 1 — Add `client` to the backend DTO and populate it (primary fix)

**File:** `d:\India Live\obms-api\Models\DTO\ReceiptsRequestDto.cs`

Add the property:
```csharp
public string? client { get; set; }
```

**File:** `d:\India Live\obms-api\Repositories\Implementation\FinanceRepository.cs`

In `GetReceiptAsync`, derive the client code from `PaymentFrom`:
```csharp
// After building receiptDto, look up the client code
var clientCode = await _oBMSDbContext.Clients
    .Where(c => c.Name == receipt.PaymentFrom && c.Branch == receipt.Branch)
    .Select(c => c.Code)
    .FirstOrDefaultAsync();

receiptDto.client = clientCode;
```

*(Adjust table/column names to match your schema.)*

### Fix 2 — Remove the bad `client` overwrite in `getReceiptData` (or rely on the fix above)

**File:** `new-receipt.component.ts`, line ~780

The line:
```ts
this.frm.patchValue({ client: data.receipt.client });
```
...should be removed **if** `branchChange` already sets the client correctly, or kept once Fix 1 ensures `data.receipt.client` holds the real code. Either way, the `branchChange` async callback sets `client` from the branch's client list matched by `PaymentFrom`, which should work — but it races with this explicit overwrite. The safest approach is to remove this line and let `branchChange` handle it.

### Fix 3 — Restore the commented-out amount fields (or validate `changeReceiptType` covers it)

**File:** `new-receipt.component.ts`, lines ~654–660

Either uncomment these:
```ts
ibgAmount: data.receipt.ReceiptAmount,   // for IBG type
// ChequeAmount: data.receipt.ReceiptAmount,  // for Cheque type
// CashAmount: data.receipt.ReceiptAmount,    // for Cash type
```
Or, better, refactor `changeReceiptType` to accept and apply the amount explicitly instead of reading `ReceiptAmount` from the form:

```ts
changeReceiptType(value: string, savedAmount?: number): void {
    ...
    const amount = savedAmount ?? (this.frm.get("ReceiptAmount")?.value || 0);
    ...
}
```
Then call it as: `this.changeReceiptType(data.receipt.ReceiptType.toString(), data.receipt.ReceiptAmount)`.

### Fix 4 — Fix the `BalancealanceAmount` typo

**File:** `new-receipt.component.ts`, line ~668

```ts
// Before:
balance_amount: data.receipt.BalancealanceAmount,

// After:
balance_amount: data.receipt.BalanceAmount,   // or remove — it's a computed field
```

Note: `BalanceAmount` is also not in `ReceiptsRequestDto`. Since it's a derived UI value (total_invoice_amount − receipt_amount), it should not be loaded from the API at all. Remove the line and let `calculation()` recompute it.

### Fix 5 — Ensure `showInvoiceTable = true` guard is sufficient

The current guard:
```ts
if (data.receipt.IsInvoiceAdjustment && Array.isArray(data.receipt.details) && data.receipt.details.length > 0) {
    this.showInvoiceTable = true;
}
```
This is correct logic. Once the `client` field problem is resolved (Fixes 1–2), this guard plus `isAdjustment` will correctly show the table.

---

## Summary of Bugs vs. Root Cause

| Bug | Type | Primary? |
|-----|------|----------|
| `data.receipt.client` is always `undefined` (no DTO property) → overwrites `client` form field with null → `*ngIf="frm.get('client')?.value"` hides table | Frontend + Backend mismatch | **YES — this is the root cause of the empty table** |
| `ibgAmount`/`ChequeAmount`/`CashAmount` commented out in patchValue | Frontend | Secondary — causes Amount field to show 0 |
| `BalancealanceAmount` typo | Frontend | Minor — only affects Balance Amount display |
