# Receipt GST/TDS Client Auto-Population — Analysis Report

## Summary Answer

The backend stores and returns `GSTPercentage`, `GSTApplicable`, `TDSPercentage`, and `TDSApplicable` correctly on every client record. The frontend `applyClientTds()` function in `new-receipt.component.ts` is also correctly written and does set these values when a client is selected. **However, there is one critical logic bug**: `GSTPercentage` is only applied to the `TaxPercentage` field when `gstPercentage > 0`. If the field resolves to 0 (because `GSTApplicable` is false, or the value is null/0), the form keeps its **previous** `TaxPercentage` value (e.g. 18 from a prior edit-mode receipt load) instead of resetting it to 0. A second, separate issue is that **`GSTPercentage` will always be stored as 0 in the database when `TDSApplicable` is false**, even if `GSTApplicable` is checked — the save guard in both the controller and repository enforces `GSTPercentage = 0` unless both `TDSApplicable AND GSTApplicable` are true.

---

## Evidence

### 1. Database Schema — ClientMaster table

File: `d:\India Live\obms-api\Database\ClientMaster_GSTPercentage_Migration.sql`

```sql
ALTER TABLE ClientMaster ADD GSTApplicable BIT NOT NULL DEFAULT 0;
ALTER TABLE ClientMaster ADD GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
```

The migration confirms both columns exist with proper types. `TDSApplicable` and `TDSPercentage` are original columns (predating this migration).

---

### 2. Domain Model — All four fields present

File: `d:\India Live\obms-api\Models\Domain\ClientMaster.cs`

```csharp
public bool TDSApplicable { get; set; }

[Column(TypeName = "decimal(5,2)")]
public decimal TDSPercentage { get; set; }

public bool GSTApplicable { get; set; }

[Column(TypeName = "decimal(5,2)")]
public decimal GSTPercentage { get; set; }
```

All four fields are mapped on the EF entity. The DTO (`ClientMasterRequestDto.cs`) mirrors them identically.

---

### 3. API Save Logic — Critical Gate: GSTPercentage requires TDSApplicable=true

File: `d:\India Live\obms-api\Controllers\MasterController.cs` (lines 597–603)  
File: `d:\India Live\obms-api\Repositories\Implementation\MasterRepository.cs` (lines 639–645)

**Both** the controller and the repository apply this guard on save:

```csharp
TDSApplicable  = clientMasterRequestDto.TDSApplicable,
TDSPercentage  = clientMasterRequestDto.TDSApplicable ? clientMasterRequestDto.TDSPercentage : 0,
GSTApplicable  = clientMasterRequestDto.GSTApplicable,
GSTPercentage  = (clientMasterRequestDto.TDSApplicable && clientMasterRequestDto.GSTApplicable)
                   ? clientMasterRequestDto.GSTPercentage : 0,
```

**Finding:** A client with `GSTApplicable = true` and `GSTPercentage = 12` but `TDSApplicable = false` will have `GSTPercentage = 0` stored in the database. This matches what the frontend save also enforces in `new-client-master.component.ts` (line 609):

```typescript
this.clientModel.GSTPercentage = (this.clientModel.TDSApplicable && gstApplicable)
  ? Number(this.clientForm.get('GSTPercentage')?.value) || 0
  : 0;
```

So the UI shows 12 in the form, but saves 0. **This means a client that has only GST checked (not TDS) will never have a non-zero GSTPercentage in the DB.**

---

### 4. GetClientByBranch API Endpoint — Returns full ClientMaster entity

File: `d:\India Live\obms-api\Controllers\FinanceController.cs` (line 5060)

```csharp
[HttpGet]
[Route("GetClientByBranch")]
public async Task<ActionResult<Object>> GetClientByBranch(string branch)
{
    return await _oBMSDbContext.ClientMasters
        .Where(x => x.Status != "Inactive" && x.Branch == branch)
        .ToListAsync();
}
```

**Finding:** This returns the **full `ClientMaster` entity** for all active clients in the branch. Since `GSTPercentage`, `GSTApplicable`, `TDSApplicable`, and `TDSPercentage` are all properties of the `ClientMaster` entity, they are included in the JSON response. No DTO projection is used, so nothing is stripped out.

---

### 5. New Receipt — Client Selection Flow

**Step 1: Branch change loads the client list**

File: `new-receipt.component.ts`, `branchChange()` method:

```typescript
branchChange(value: any) {
  this.service.GetClientByBranch(value).subscribe((d: any) => {
    this.clientList = d;
    // ...finds matching client by PaymentFrom name in edit mode
    if (client) {
      this.applyClientTds(client);
    }
    this.frm.patchValue({ client: code });
  });
}
```

After branch loads, the client list is stored in `this.clientList`. In edit mode, if `PaymentFrom` already matches a client name, `applyClientTds` is called immediately.

**Step 2: Client dropdown change**

```typescript
clientChange(value: any) {
  const client = this.clientList.find((x: any) => x.Code === value);
  this.applyClientTds(client);
  this.frm.patchValue({ PaymentFrom: client ? client.Name : '' });
  // ...fetches invoices
}
```

**Step 3: applyClientTds — the core logic**

```typescript
private applyClientTds(client: any): void {
  const isTdsApplicable = client?.TDSApplicable === true || client?.TDSApplicable === 1;
  const tdsPercentage = isTdsApplicable ? Number(client.TDSPercentage) || 0 : 0;
  const isGstApplicable = isTdsApplicable && (client?.GSTApplicable === true || client?.GSTApplicable === 1);
  const gstPercentage = isGstApplicable ? Number(client.GSTPercentage) || 0 : 0;

  this.frm.get('HQPercentage')?.setValue(tdsPercentage);    // TDS → HQPercentage field
  this.frm.get('TaxPercentage')?.setValue(
    gstPercentage > 0 ? gstPercentage : this.frm.get('TaxPercentage')?.value
  );
  this.calculate();
}
```

**Field mapping summary:**
| Receipt Form Field | Source |
|---|---|
| `HQPercentage` (TDS %) | `client.TDSPercentage` (only if `TDSApplicable=true`) |
| `TaxPercentage` (GST %) | `client.GSTPercentage` (only if both `TDSApplicable=true` AND `GSTApplicable=true`) |

---

### 6. GST Calculation Formula

File: `new-receipt.component.ts`, `calculate()` method (non-adjustment path):

```typescript
// For cheque (ReceiptType=1):
this.taxAmount = this.taxPercentage * this.chequeAmount / (100 + this.taxPercentage);
this.hqAmount  = (this.chequeAmount - this.taxAmount) * (this.hqPercentage / 100);
this.branchAmount = this.chequeAmount - this.taxAmount - this.hqAmount;
```

This is a **tax-inclusive back-calculation** (GST is already baked into the cheque amount). If the user types 110,000 as the cheque amount with 10% GST:
- `taxAmount = 10 * 110000 / 110 = 10,000`
- The 110,000 already includes GST.

There is also a separate display field:
```typescript
// Amount + GST Total display:
this.amountWithGST = (baseAmt + this.taxAmount).toFixed(2);
```

---

## Conclusions

### Bug 1 (Critical): GSTPercentage Stale Value on Client Switch

In `applyClientTds`:
```typescript
this.frm.get('TaxPercentage')?.setValue(
  gstPercentage > 0 ? gstPercentage : this.frm.get('TaxPercentage')?.value  // ← keeps stale value
);
```

When `gstPercentage` resolves to 0 (client has no GST, or `TDSApplicable=false`), the old `TaxPercentage` (e.g., 18 from a previously loaded receipt) stays in place. This explains the screenshot where the client master shows 12% but the receipt shows 18.

**Fix:**
```typescript
this.frm.get('TaxPercentage')?.setValue(gstPercentage);  // always reset to client's value (or 0)
```

---

### Bug 2 (Critical): GSTPercentage Always Saved as 0 When TDSApplicable=false

In `MasterController.cs` / `MasterRepository.cs` and also in the frontend `savebuttonClick()`:

```csharp
GSTPercentage = (clientMasterRequestDto.TDSApplicable && clientMasterRequestDto.GSTApplicable)
                  ? clientMasterRequestDto.GSTPercentage : 0,
```

A client that only has GST applicable (no TDS) can never persist a non-zero `GSTPercentage`. The intent appears to be that GST and TDS are always linked in this system, but the UI shows them as independent checkboxes. If users are setting GST=12% but TDS=unchecked, the value is lost on save.

**Fix Option A (recommended):** Store `GSTPercentage` independently of `TDSApplicable`:
```csharp
GSTApplicable  = clientMasterRequestDto.GSTApplicable,
GSTPercentage  = clientMasterRequestDto.GSTApplicable ? clientMasterRequestDto.GSTPercentage : 0,
```

**Fix Option B:** Keep the coupling but make the UI enforce it — hide or disable the GST checkbox unless TDS is also checked, and show a tooltip explaining why.

---

### Bug 3 (Minor): applyClientTds Called Twice in branchChange Edit Mode

In `branchChange()`, when the branch changes in edit mode:
1. `applyClientTds(client)` is called from `branchChange` (sets GST/TDS from client).
2. Then `getReceiptData()` calls `branchChange(data.receipt.Branch)` → triggers again.
3. `getReceiptData` then patches `TaxPercentage` from the saved receipt value.

The order means the receipt's saved `TaxPercentage` (e.g., 18) overwrites what `applyClientTds` just set. This is a secondary cause for the 18% showing up in edit mode.

---

## Recommended Fixes (Priority Order)

1. **Fix `applyClientTds` to always reset TaxPercentage** — change `gstPercentage > 0 ? gstPercentage : this.frm.get('TaxPercentage')?.value` to just `gstPercentage`.  
   File: `new-receipt.component.ts`

2. **Decouple GST save from TDSApplicable guard** — allow `GSTPercentage` to be stored when `GSTApplicable=true` regardless of `TDSApplicable`.  
   Files: `MasterController.cs`, `MasterRepository.cs`, `new-client-master.component.ts`

3. **Verify existing client data in DB** — any client saved before fix #2 will have `GSTPercentage = 0` even if the UI showed 12. Run a data audit: `SELECT Code, Name, TDSApplicable, GSTApplicable, GSTPercentage FROM ClientMaster WHERE GSTApplicable = 1 AND GSTPercentage = 0`.

4. **In edit mode, do not call `applyClientTds` on branch load** — since the receipt already has its persisted `TaxPercentage` and `HQPercentage`, calling `applyClientTds` during load is redundant and can corrupt the loaded values.
