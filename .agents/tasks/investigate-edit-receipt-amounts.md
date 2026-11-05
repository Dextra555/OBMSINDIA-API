# Investigation Report: Edit Receipt — Wrong GST%, Amount+GST Total, Balance Amount, TDS Amount, Branch Collection

**Date:** Investigation complete  
**File investigated:** `d:\India Live\obms-ui\src\app\modules\finance\receipts\new-receipt\new-receipt.component.ts`  
**Backend confirmed:** `d:\India Live\obms-api\Repositories\Implementation\FinanceRepository.cs` — `GetReceiptAsync(int id)`

---

## Summary Answer (Read This First)

The backend returns **all correct, saved values** for TaxPercentage, TaxAmount, HQPercentage, HQAmount, and BranchCollection. The bug is entirely on the frontend. After `patchValue()` correctly loads the saved receipt data, **two sequential function calls overwrite those values**:

1. **`changeReceiptType()` → `calculation()` → `calculate()`** — recalculates TaxAmount, HQAmount, BranchCollection from TaxPercentage and the receipt amount. This happens synchronously.
2. **`branchChange()` → async `GetClientByBranch()` → `applyClientTds()`** — overwrites `HQPercentage` with the client master's current TDSPercentage, overwrites `TaxPercentage` with the client master's GSTPercentage (if > 0), then calls `calculate()` again. This fires asynchronously and arrives **after** the initial patch, so it always wins.

For Receipt 141 the client master apparently has `GSTPercentage = 15.25` in its current record, so `applyClientTds()` sets `TaxPercentage = 15.25`, then `calculate()` back-calculates `TaxAmount = 15.25% × 256144 / (100 + 15.25) ≈ 33,873`, leading to the wrong `Amount + GST Total`, wrong `BranchCollection`, and a negative `balance_amount`.

**The balance_amount formula is also wrong for edit mode:**  
`balance_amount = total_invoice_amount − ibgAmount` (i.e., `ServiceAmount sum − cash paid`).  
For Receipt 141: ServiceAmount rows sum to 217071, ibgAmount = 256144 → `217071 − 256144 = −39073`. This is negative because `ibgAmount` (256144) includes GST/TDS components, but `total_invoice_amount` (217071) is the base service amount only. The formula needs to account for GST and TDS.

---

## Evidence

### 1. Backend — `GetReceiptAsync` correctly returns all saved values

**File:** `FinanceRepository.cs` lines ~368–420

```csharp
var receiptDto = new ReceiptsRequestDto
{
    ReceiptAmount  = receipt.ReceiptAmount,
    TaxPercentage  = receipt.TaxPercentage,   // ✅ from DB
    TaxAmount      = receipt.TaxAmount,        // ✅ from DB
    HQPercentage   = receipt.HQPercentage,     // ✅ from DB
    HQAmount       = receipt.HQAmount,         // ✅ from DB
    BranchCollection = receipt.BranchCollection, // ✅ from DB
    ...
};
```

**DTO:** `ReceiptsRequestDto.cs` confirms all six fields exist. The backend is not the problem.

---

### 2. `getReceiptData()` — correct initial `patchValue`, then immediately overwritten

**File:** `new-receipt.component.ts` lines ~576–746

Step-by-step call sequence after `patchValue()`:

```
patchValue({
  TaxPercentage: data.receipt.TaxPercentage,   // ✅ Set correctly from DB
  TaxAmount:     data.receipt.TaxAmount,        // ✅ Set correctly from DB
  HQPercentage:  data.receipt.HQPercentage,     // ✅ Set correctly from DB
  HQAmount:      data.receipt.HQAmount,         // ✅ Set correctly from DB
  BranchCollection: data.receipt.BranchCollection // ✅ Set correctly from DB
})

↓ Line ~742
this.changeReceiptType(data.receipt.ReceiptType)
  → this.calculation()              // reads ibgAmount (correct)
    → this.calculate()              // OVERWRITES TaxAmount, HQAmount, BranchCollection
                                    // using TaxPercentage (still correct at this point)

↓ Line ~743
this.changeAdjustment(data.receipt.IsInvoiceAdjustment)
  → this.calculation()              // OVERWRITES again (same result at this point)
    → this.calculate()

↓ Line ~746  [ASYNC — fires last]
this.branchChange(data.receipt.Branch)
  → GetClientByBranch(branch).subscribe(...)   // HTTP call, resolves after ~300ms
    → applyClientTds(client)
      → frm.HQPercentage = client.TDSPercentage  // ❌ OVERWRITES saved HQPercentage
      → frm.TaxPercentage = client.GSTPercentage  // ❌ OVERWRITES saved TaxPercentage
      → this.calculate()                          // ❌ OVERWRITES TaxAmount, HQAmount,
                                                  //    BranchCollection with wrong values
```

---

### 3. `applyClientTds()` — the definitive overwriter (lines ~1515–1523)

```typescript
private applyClientTds(client: any): void {
  const isTdsApplicable = client?.TDSApplicable === true || client?.TDSApplicable === 1;
  const tdsPercentage = isTdsApplicable ? Number(client.TDSPercentage) || 0 : 0;
  const isGstApplicable = isTdsApplicable && (client?.GSTApplicable === true || ...);
  const gstPercentage = isGstApplicable ? Number(client.GSTPercentage) || 0 : 0;

  this.frm.get('HQPercentage')?.setValue(tdsPercentage);           // ❌ overwrites DB value
  this.frm.get('TaxPercentage')?.setValue(gstPercentage > 0        // ❌ overwrites DB value
    ? gstPercentage
    : this.frm.get('TaxPercentage')?.value);
  this.calculate();                                                  // ❌ recalculates everything
}
```

`GetClientByBranch` returns the full `ClientMasters` entity including `TDSApplicable`, `TDSPercentage`, `GSTApplicable`, `GSTPercentage` (see `FinanceController.cs` line ~5063). These are the **current** client master values — not the values that were used when the receipt was originally saved.

---

### 4. `calculate()` — unconditionally recalculates all derived fields (lines ~1854–1960)

```typescript
calculate() {
  this.taxPercentage = Number(this.frm.get("TaxPercentage")?.value);
  this.hqPercentage  = Number(this.frm.get("HQPercentage")?.value);
  // ...
  if (this.isAdjustment) {
    this.taxAmount     = this.calculateSelectedInvoiceTax(receiptAmount);
    this.taxPercentage = (this.taxAmount / receiptAmount) * 100;
    this.frm.get("TaxPercentage")?.setValue(...)                   // ❌ overwrite
    this.hqAmount      = receiptAmount * (this.hqPercentage / 100);
    this.branchAmount  = receiptAmount + this.taxAmount - this.hqAmount;
  } else if (receiptTypeID === "5") {  // IBG
    this.taxAmount    = this.taxPercentage * this.ibgAmount / (100 + this.taxPercentage);
    this.hqAmount     = (this.ibgAmount - this.taxAmount) * (this.hqPercentage / 100);
    this.branchAmount = this.ibgAmount - this.taxAmount - this.hqAmount;
  }
  this.frm.get("TaxAmount")?.setValue(this.taxAmount.toFixed(2));  // ❌ overwrite
  this.frm.get("HQAmount")?.setValue(this.hqAmount.toFixed(2));    // ❌ overwrite
  this.frm.get("BranchCollection")?.setValue(Math.round(this.branchAmount).toString()); // ❌ overwrite
}
```

No guard for edit mode. Always overwrites.

---

### 5. `calculation()` — balance_amount formula is wrong for edit mode (line ~1806)

```typescript
this.frm.get("ReceiptAmount")?.setValue(cash);
const balanceAmount = (total - cash).toFixed(2);   // total = sum of ServiceAmount (base)
                                                    // cash  = ibgAmount (which includes GST+TDS)
```

`total_invoice_amount` = sum of base `ServiceAmount` (excludes GST).  
`ibgAmount` (cash) = the full receipt amount including GST content.  
So `balance_amount = 217071 − 256144 = −39073` for Receipt 141. This is structurally wrong: you are subtracting a GST-inclusive amount from a GST-exclusive total.

---

## Root Cause Summary

| Field | Where overwritten | Mechanism |
|---|---|---|
| `HQPercentage` (TDS%) | `applyClientTds()` | Sets to current client master TDSPercentage |
| `TaxPercentage` (GST%) | `applyClientTds()` | Sets to current client master GSTPercentage |
| `TaxAmount` | `calculate()` (called by `applyClientTds`, `changeReceiptType`, `changeAdjustment`) | Recalculated from TaxPercentage × ibgAmount |
| `HQAmount` | `calculate()` | Recalculated from HQPercentage × (ibgAmount − TaxAmount) |
| `BranchCollection` | `calculate()` | Recalculated from ibgAmount − TaxAmount − HQAmount |
| `balance_amount` | `calculation()` | Formula: `ServiceAmount_sum − ibgAmount` mixes GST-exclusive and GST-inclusive bases |

---

## Conclusions and Recommendations

### Fix 1 — Skip `applyClientTds()` in edit mode (HIGHEST PRIORITY)

`branchChange()` must not call `applyClientTds()` when loading an existing receipt (`this.receiptID > 0`). The client master percentages are only correct defaults for **new** receipts. For edit mode, the saved percentages from the DB are authoritative.

**In `branchChange()` (line ~1118):**
```typescript
branchChange(value: any) {
  this.service.GetClientByBranch(value).subscribe((d: any) => {
    this.clientList = d;
    let code = '';
    if (this.clientList && this.clientList.length > 0) {
      const client = this.clientList.find(
        (x: any) => x.Name === this.frm.get('PaymentFrom')?.value
      );
      code = client ? client.Code : '';
      if (client && this.receiptID === 0) {   // ← ADD THIS GUARD
        this.applyClientTds(client);
      }
    }
    this.frm.patchValue({ client: code });
  });
}
```

### Fix 2 — Restore saved values after `calculate()` in edit mode

Even with Fix 1, `changeReceiptType()` → `calculate()` and `changeAdjustment()` → `calculate()` still overwrite TaxAmount/HQAmount/BranchCollection synchronously. These calls happen **before** `branchChange()` fires so they run with the DB-loaded TaxPercentage/HQPercentage — which means their calculations may actually be consistent with the saved data **if** the saved TaxPercentage is still in the form. However, if `IsInvoiceAdjustment=true`, the `isAdjustment` branch in `calculate()` ignores the saved percentages entirely and recalculates from invoice rows.

For edit mode with `IsInvoiceAdjustment = true`, after calling `calculation()` and `calculate()`, re-patch the saved values:

```typescript
// In getReceiptData(), after changeReceiptType() and changeAdjustment():
if (this.receiptID > 0) {
  this.frm.patchValue({
    TaxPercentage:   data.receipt.TaxPercentage,
    TaxAmount:       data.receipt.TaxAmount,
    HQPercentage:    data.receipt.HQPercentage,
    HQAmount:        data.receipt.HQAmount,
    BranchCollection: data.receipt.BranchCollection,
  });
}
```

This restores the correct DB values that `changeReceiptType()` and `changeAdjustment()` may have clobbered.

### Fix 3 — Fix `balance_amount` formula for edit mode

The current formula (`total_invoice_amount − ibgAmount`) gives a negative result because `total_invoice_amount` is the sum of base `ServiceAmount` (GST-exclusive) while `ibgAmount` is the full receipt amount (GST-inclusive).

For edit mode the correct balance is:
```
balance_amount = total_invoice_amount − (ibgAmount − TaxAmount − HQAmount)
              = total_invoice_amount − BranchCollection
```
i.e., what remains of the base service amount after the branch collection portion of the receipt is accounted for.

In `calculation()` (line ~1806), change:
```typescript
// CURRENT (wrong for edit mode):
const balanceAmount = (total - cash).toFixed(2);

// CORRECT for edit mode:
if (this.receiptID > 0) {
  const branchCollection = Number(this.frm.get("BranchCollection")?.value) || 0;
  const balanceAmount = (total - branchCollection).toFixed(2);
  this.frm.get("balance_amount")?.setValue(balanceAmount);
} else {
  const balanceAmount = (total - cash).toFixed(2);
  this.frm.get("balance_amount")?.setValue(balanceAmount);
}
```

For Receipt 141: `total_invoice_amount(217071) − BranchCollection(290094 − TaxAmount − HQAmount)` — the exact value depends on the correct saved BranchCollection which Fix 1 and Fix 2 will restore first.

### Fix 4 — Guard `calculate()` isAdjustment branch for edit mode (optional, for correctness)

In `calculate()`, the `isAdjustment` branch unconditionally recalculates `TaxPercentage` from invoice rows. In edit mode, `TaxPercentage` was saved in the DB and should not be recalculated. Consider:

```typescript
if (this.isAdjustment && this.receiptID === 0) {
  // new receipt — derive TaxPercentage from invoices
  this.taxAmount = this.calculateSelectedInvoiceTax(receiptAmount);
  this.taxPercentage = ...
  this.frm.get("TaxPercentage")?.setValue(...);
  ...
} else if (this.isAdjustment && this.receiptID > 0) {
  // edit mode — use saved percentages, only recalculate amounts
  this.taxAmount = this.taxPercentage * receiptAmount / (100 + this.taxPercentage);
  this.hqAmount  = receiptAmount * (this.hqPercentage / 100);
  this.branchAmount = receiptAmount + this.taxAmount - this.hqAmount;
}
```

---

## Implementation Order

1. **Fix 1** (guard in `branchChange`) — this stops the async overwrite, the most impactful single change.
2. **Fix 2** (re-patch saved values after `changeReceiptType`/`changeAdjustment`) — stops the synchronous overwrite.
3. **Fix 3** (balance_amount formula) — fixes the negative balance display.
4. **Fix 4** (guard `calculate()` isAdjustment branch for edit) — optional cleanup, prevents edge-case drift.

Fixes 1 and 2 together will make GST%, TaxAmount, TDS Amount (HQAmount), and BranchCollection all display the saved DB values correctly. Fix 3 will make Balance Amount non-negative and meaningful.
