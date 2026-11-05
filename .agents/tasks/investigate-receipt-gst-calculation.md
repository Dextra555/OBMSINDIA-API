# Investigation Report: GST % from Client Master → Receipt Page

**Date:** 2025  
**Status:** Complete  
**Issue Summary:** Receipt page shows GST % = 18 instead of the 12% stored on the selected client's master record (e.g., client Anejas, code FwgC000002).

---

## 1. Summary Answer

The plumbing from Client Master → DB → API → Receipt page is **mostly correct but has one conditional bug** in `applyClientTds()`. The GST % is never applied to Receipt's `TaxPercentage` form field unless `TDSApplicable` is also `true` on the client record. The "18" shown is stale data from a previously saved/edited receipt (the old receipt was saved with 18% and that value is reloaded when editing), **not** a hardcoded default. For a brand-new receipt, the field starts blank/0 and only gets populated via `applyClientTds()` — which silently skips setting it when `TDSApplicable` is false.

Additionally, the calculation formula for GST in non-adjustment mode is **tax-inclusive back-calculation** (extracts GST from the gross amount), but the user expects a **tax-exclusive add-on** (Amount × 12% added on top). This is a separate logic issue.

Also confirmed: the **"Supplier/Client TDS Slab" link under Finance menu** navigates correctly to `/master/tds-master` (the `TdsMasterComponent`) and that route is properly registered — this is **not broken**, but it jumps out of the Finance section to a Master screen, which may feel unexpected.

---

## 2. Evidence by Question

### Q1: Does Client Master save GSTPercentage to DB? Which endpoint and column?

**Yes, it saves correctly.**

- **DB column:** `ClientMaster.GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0` — added by migration script at `d:\India Live\obms-api\Database\ClientMaster_GSTPercentage_Migration.sql`
- **Domain model:** `d:\India Live\obms-api\Models\Domain\ClientMaster.cs` line ~99: `[Column(TypeName = "decimal(5,2)")] public decimal GSTPercentage { get; set; }`
- **DTO:** `d:\India Live\obms-api\Models\DTO\ClientMasterRequestDto.cs` line ~96: `public decimal GSTPercentage { get; set; }`
- **Save endpoint:** `POST /api/Master/SaveAndUpdateClientMaster` (`MasterController.cs` ~line 602):
  ```csharp
  GSTPercentage = (clientMasterRequestDto.TDSApplicable && clientMasterRequestDto.GSTApplicable)
      ? clientMasterRequestDto.GSTPercentage : 0,
  ```
  ⚠️ **Note:** GSTPercentage is only persisted as non-zero when **both** TDSApplicable AND GSTApplicable are `true`. If either is false, it stores 0.
- **Angular save:** `new-client-master.component.ts` savebuttonClick() (~line 436) applies the same guard before calling the API.

**Conclusion:** If client Anejas has `TDSApplicable = true` AND `GSTApplicable = true`, then `GSTPercentage = 12` is saved correctly in DB. If `TDSApplicable = false`, it stores 0.

---

### Q2: When client is selected on Receipt page, does it fetch client details including GSTPercentage?

**Yes — but only indirectly through the branch client list.**

- Receipt page calls `branchChange()` which calls `service.GetClientByBranch(branch)`:
  - **Angular service:** `finance.service.ts` line ~267 → `GET /api/Finance/GetClientByBranch?branch=...`
  - **API endpoint:** `FinanceController.cs` line ~5058:
    ```csharp
    return await _oBMSDbContext.ClientMasters
        .Where(x => x.Status != "Inactive" && x.Branch == branch)
        .ToListAsync();
    ```
  - This returns the **full ClientMaster entity**, which **includes `GSTApplicable` and `GSTPercentage`** because those fields are mapped on `ClientMaster`.

- When a client is then selected in the dropdown, `clientChange(value)` finds the matching client in the already-loaded `clientList` and calls `applyClientTds(client)`. **No separate API call is made per client selection** — the data is already available from `GetClientByBranch`.

**Conclusion:** `GSTPercentage` does travel from the API to the Angular `clientList` array. The issue is in how it is (or isn't) applied.

---

### Q3: How is the GST field on Receipt calculated? Is it hardcoded (18) or uses client data?

**It uses client data — but the `applyClientTds()` method has a conditional bug that may prevent it from being set.**

The `TaxPercentage` form field drives GST calculation in `calculate()`:

```typescript
// new-receipt.component.ts calculate()
this.taxAmount = this.taxPercentage * this.chequeAmount / (100 + this.taxPercentage);
```

This is a **tax-inclusive back-calculation**: it extracts GST already embedded in the receipt amount.  
For Amount=20000 with GST=12%, this gives:  `12 * 20000 / 112 = 2142.86` — NOT `20000 * 12% = 2400`.

The `amountWithGST` (Amount + GST Total field) is:
```typescript
// new-receipt.component.ts calculate() ~line 2000
const baseAmt = this.chequeAmount || this.cashAmount || this.ibgAmount;
this.amountWithGST = (baseAmt + this.taxAmount).toFixed(2);
```

For Amount=20000, taxPercentage=12:  `20000 + 2142.86 = 22142.86` (not the expected 22400).

**The "18" shown is not hardcoded** in the new receipt component. It comes from a previously saved receipt's `TaxPercentage` field being loaded in edit mode via `getReceiptData()` which patches `TaxPercentage: data.receipt.TaxPercentage`. That earlier receipt was saved with 18%.

---

### Q4: Where exactly in new-receipt.component.ts is the GST value set?

The GST `TaxPercentage` is set in **one place**: `applyClientTds()` method, called from both `clientChange()` and `branchChange()`:

```typescript
// new-receipt.component.ts ~line 1535-1543
private applyClientTds(client: any): void {
    const isTdsApplicable = client?.TDSApplicable === true || client?.TDSApplicable === 1;
    const tdsPercentage = isTdsApplicable ? Number(client.TDSPercentage) || 0 : 0;
    const isGstApplicable = isTdsApplicable && (client?.GSTApplicable === true || client?.GSTApplicable === 1);
    const gstPercentage = isGstApplicable ? Number(client.GSTPercentage) || 0 : 0;

    this.frm.get('HQPercentage')?.setValue(tdsPercentage);
    this.frm.get('TaxPercentage')?.setValue(gstPercentage > 0 ? gstPercentage : this.frm.get('TaxPercentage')?.value);
    this.calculate();
}
```

**Bug 1 — Double guard on TDSApplicable:** `isGstApplicable` is `isTdsApplicable && GSTApplicable`. If the client has `TDSApplicable = false`, then `isGstApplicable` is always `false` regardless of `GSTApplicable`, and `gstPercentage` stays 0. GST is a charge on services independent of TDS — the two flags should not be coupled this way on the receipt page.

**Bug 2 — Fallback keeps stale value:** When `gstPercentage === 0`, the line sets `TaxPercentage` to `this.frm.get('TaxPercentage')?.value` — i.e., whatever was already there. This means a stale value from a previously edited receipt is preserved.

---

### Q5: What code change is needed?

**Fix 1: `applyClientTds()` — decouple GST from TDS guard**

Current (buggy):
```typescript
const isGstApplicable = isTdsApplicable && (client?.GSTApplicable === true || client?.GSTApplicable === 1);
const gstPercentage = isGstApplicable ? Number(client.GSTPercentage) || 0 : 0;
```

Fixed:
```typescript
const isGstApplicable = client?.GSTApplicable === true || client?.GSTApplicable === 1;
const gstPercentage = isGstApplicable ? Number(client.GSTPercentage) || 0 : 0;
```

**Fix 2: Always overwrite TaxPercentage (don't preserve stale value)**

Current:
```typescript
this.frm.get('TaxPercentage')?.setValue(gstPercentage > 0 ? gstPercentage : this.frm.get('TaxPercentage')?.value);
```

Fixed:
```typescript
this.frm.get('TaxPercentage')?.setValue(gstPercentage);
```

**Fix 3: Change GST calculation from tax-inclusive back-calculation to tax-exclusive add-on**

The user's expected formula: `Amount 20000 + 12% = 22400`  
i.e., `GST = Amount × rate/100`, `Total = Amount + GST`

Current (inclusive back-calculation in `calculate()`):
```typescript
this.taxAmount = this.taxPercentage * this.chequeAmount / (100 + this.taxPercentage);
```

Fix for Amount + GST Total display — change `amountWithGST` calc OR change the formula:
```typescript
// Tax-exclusive add-on formula
this.taxAmount = this.chequeAmount * this.taxPercentage / 100;
// amountWithGST = chequeAmount + taxAmount = 20000 + 2400 = 22400 ✓
```

⚠️ **Note:** This is a breaking change to existing saved receipts which were calculated with the inclusive method. The team should decide which formula is canonical. If the old receipts used the inclusive method, changing to exclusive changes how new receipts are compared to old ones.

---

### Q6: Was GSTPercentage column added to DB per the migration SQL?

**Migration script exists but must be run manually.**

File: `d:\India Live\obms-api\Database\ClientMaster_GSTPercentage_Migration.sql`

Script content:
- Adds `GSTApplicable BIT NOT NULL DEFAULT 0` to `ClientMaster` if not present
- Adds `GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0` to `ClientMaster` if not present
- Uses `IF NOT EXISTS` guards so it's safe to re-run

**This migration must have been run on the live DB for the client save to work.** If it was not run, the `SaveAndUpdateClientMaster` API would throw a column-not-found error. Since the app is reportedly running, the columns are likely present. To confirm, run:

```sql
SELECT COLUMN_NAME, DATA_TYPE, COLUMN_DEFAULT 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ClientMaster' 
AND COLUMN_NAME IN ('GSTApplicable', 'GSTPercentage');
```

---

## 3. Root Cause Summary

| # | Bug | Location | Effect |
|---|-----|----------|--------|
| 1 | `isGstApplicable` requires `TDSApplicable = true` | `new-receipt.component.ts` `applyClientTds()` ~line 1537 | GST % is never applied to a client where TDS is not applicable |
| 2 | Stale TaxPercentage preserved when `gstPercentage === 0` | `new-receipt.component.ts` `applyClientTds()` ~line 1541 | Old receipt's 18% carries over to new receipt |
| 3 | GST calculated as tax-inclusive (back-calculation) not tax-exclusive (add-on) | `new-receipt.component.ts` `calculate()` | 20000 + 12% gives 22142 instead of expected 22400 |

---

## 4. Sidebar Navigation Issue

**"Supplier/Client TDS Slab" under Finance menu** — this link at `sidebar.component.html` line 162 navigates to `/master/tds-master`, which is the `TdsMasterComponent`. The route is registered in `admin-layout-routing.module.ts` line 436. **The navigation itself works correctly** — it just opens a Master-section page from within the Finance menu section. This may be intentional (quick access). It is **not** causing a broken page or redirect to logic page.

---

## 5. Recommended Fixes (Do Not Implement Here)

### File: `d:\India Live\obms-ui\src\app\modules\finance\receipts\new-receipt\new-receipt.component.ts`

**Change 1** — Fix `applyClientTds()` to decouple GST from TDS flag and always reset TaxPercentage:

```typescript
private applyClientTds(client: any): void {
    const isTdsApplicable = client?.TDSApplicable === true || client?.TDSApplicable === 1;
    const tdsPercentage = isTdsApplicable ? Number(client.TDSPercentage) || 0 : 0;

    // GST is independent of TDS — check GSTApplicable directly
    const isGstApplicable = client?.GSTApplicable === true || client?.GSTApplicable === 1;
    const gstPercentage = isGstApplicable ? Number(client.GSTPercentage) || 0 : 0;

    this.frm.get('HQPercentage')?.setValue(tdsPercentage);
    this.frm.get('TaxPercentage')?.setValue(gstPercentage);  // always overwrite
    this.calculate();
}
```

**Change 2** — Fix GST tax formula in `calculate()` for non-adjustment mode (all three receipt types):

```typescript
// Replace the back-calculation lines for Cheque, Cash, IBG:
// Before: this.taxAmount = this.taxPercentage * this.chequeAmount / (100 + this.taxPercentage);
// After (tax-exclusive add-on):
this.taxAmount = this.chequeAmount * this.taxPercentage / 100;
// Same change for cashAmount and ibgAmount branches
```

With these two changes, selecting client Anejas (GSTApplicable=true, GSTPercentage=12) on a new receipt with Amount=20000:
- `TaxPercentage` gets set to 12
- `taxAmount` = 20000 × 12/100 = 2400
- `amountWithGST` = 20000 + 2400 = **22400** ✓

---

## 6. Files Involved

| File | Role |
|------|------|
| `obms-ui/.../new-receipt.component.ts` | **Main fix location** — `applyClientTds()` and `calculate()` |
| `obms-ui/.../new-client-master.component.ts` | Client Master save — saves GSTApplicable/GSTPercentage correctly |
| `obms-api/Controllers/FinanceController.cs` | `GetClientByBranch` — returns full ClientMaster including GSTPercentage |
| `obms-api/Controllers/MasterController.cs` | `SaveAndUpdateClientMaster` — saves GSTPercentage with TDS+GST guard |
| `obms-api/Models/Domain/ClientMaster.cs` | Domain model — GSTPercentage field |
| `obms-api/Models/DTO/ClientMasterRequestDto.cs` | DTO — GSTPercentage field |
| `obms-api/Database/ClientMaster_GSTPercentage_Migration.sql` | DB migration — adds columns |
