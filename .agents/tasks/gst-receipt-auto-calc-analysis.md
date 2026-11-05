# GST Receipt Auto-Calculation — Investigation Report

**Date:** 2026-10-30  
**Scope:** Client Master GST fields → Receipt page auto-calculation  

---

## Summary Answer (read this first)

**The GST infrastructure is fully wired end-to-end — DB columns exist, backend saves/returns them, and the Receipt component already has `applyClientTds()` that reads `GSTApplicable` + `GSTPercentage` and sets `TaxPercentage` on the form.**

**However, there is one critical conditional bug:** `GSTApplicable` is only honoured when `TDSApplicable` is also `true`. If a client has GST without TDS, the GST percentage is silently zeroed out — both at save time (backend) and at receipt calculation time (frontend). This is the only gap that needs fixing.

---

## 1. DB Columns — What Exists

### Migration files confirm two columns on `ClientMaster`:

**File:** `d:\India Live\obms-api\Database\ClientMaster_TDS_Migration.sql`
- `TDSApplicable` — BIT, DEFAULT 0
- `TDSPercentage` — DECIMAL(5,2), DEFAULT 0

**File:** `d:\India Live\obms-api\Database\ClientMaster_GSTPercentage_Migration.sql`
- `GSTApplicable` — BIT, DEFAULT 0
- `GSTPercentage` — DECIMAL(5,2), DEFAULT 0

### Domain model confirms all four columns:

**File:** `d:\India Live\obms-api\Models\Domain\ClientMaster.cs` (lines 88–99)
```csharp
public bool TDSApplicable { get; set; }
public decimal TDSPercentage { get; set; }
public bool GSTApplicable { get; set; }

[Column(TypeName = "decimal(5,2)")]
public decimal GSTPercentage { get; set; }
```

**Answer to Q1:**
| Field | DB Column | Type |
|---|---|---|
| GST flag | `GSTApplicable` | BIT |
| GST rate | `GSTPercentage` | DECIMAL(5,2) |
| TDS flag | `TDSApplicable` | BIT |
| TDS rate | `TDSPercentage` | DECIMAL(5,2) |

---

## 2. Client Master — How GST is Saved

### Frontend form definition

**File:** `d:\India Live\obms-ui\src\app\modules\master\client-master\new-client\new-client-master.component.ts` (lines ~161–164)
```typescript
TDSApplicable: this.fb.control(false),
TDSPercentage: this.fb.control(null),
GSTApplicable: this.fb.control(false),
GSTPercentage: this.fb.control(null),
```

### Frontend save logic — the TDS dependency

**File:** `new-client-master.component.ts` — `savebuttonClick()` (lines ~377–387)
```typescript
this.clientModel.TDSApplicable = this.clientForm.get('TDSApplicable')?.value === true;
this.clientModel.TDSPercentage = this.clientModel.TDSApplicable
  ? Number(this.clientForm.get('TDSPercentage')?.value) || 0
  : 0;
const gstApplicable = this.clientForm.get('GSTApplicable')?.value === true;
this.clientModel.GSTApplicable = gstApplicable;
this.clientModel.GSTPercentage = (this.clientModel.TDSApplicable && gstApplicable)  // ← BUG HERE
  ? Number(this.clientForm.get('GSTPercentage')?.value) || 0
  : 0;
```

**⚠️ BUG:** `GSTPercentage` is only saved to the model when `TDSApplicable === true`. If TDS is off but GST is on, `GSTPercentage` is sent as `0`.

### Backend save — same TDS dependency

**File:** `d:\India Live\obms-api\Controllers\MasterController.cs` (lines 601–603)
```csharp
GSTApplicable = clientMasterRequestDto.GSTApplicable,
GSTPercentage = (clientMasterRequestDto.TDSApplicable && clientMasterRequestDto.GSTApplicable)
    ? clientMasterRequestDto.GSTPercentage : 0,
```

**File:** `d:\India Live\obms-api\Repositories\Implementation\MasterRepository.cs` (lines 643–645)
```csharp
existingClientDetails.GSTApplicable = clientMaster.GSTApplicable;
existingClientDetails.GSTPercentage = (clientMaster.TDSApplicable && clientMaster.GSTApplicable)
    ? clientMaster.GSTPercentage : 0;
```

**⚠️ BUG (confirmed in two backend files):** Same gate — `GSTPercentage` is stored as `0` unless `TDSApplicable` is also `true`.

### API endpoint for saving client

`POST /Master/SaveAndUpdateClientMaster`  
Frontend call: `this._masterService.saveAndUpdateClientMaster(this.clientModel)` in `savebuttonClick()`.

---

## 3. Receipt Page — Client Selection and GST Application

### API endpoint that provides client list to Receipt

**File:** `d:\India Live\obms-api\Controllers\FinanceController.cs` (lines 5057–5067)
```csharp
[Route("GetClientByBranch")]
public async Task<ActionResult<Object>> GetClientByBranch(string branch)
{
    return await _oBMSDbContext.ClientMasters
        .Where(x => x.Status != "Inactive" && x.Branch == branch)
        .ToListAsync();
}
```

This returns the full `ClientMaster` entity — **including `GSTApplicable` and `GSTPercentage`**. No projection, so all columns come through.

**Answer to Q2:** `GET /Finance/GetClientByBranch?branch={branchCode}` — called from `branchChange()` in the receipt component.

### Frontend — when branch is selected

**File:** `d:\India Live\obms-ui\src\app\modules\finance\receipts\new-receipt\new-receipt.component.ts` — `branchChange()` (lines ~1168–1197)
```typescript
branchChange(value: any) {
  this.service.GetClientByBranch(value).subscribe((d: any) => {
    this.clientList = d;
    ...
    if (client) {
      this.applyClientTds(client);   // ← called here on branch change
    }
    this.frm.patchValue({ client: code });
  })
}
```

### Frontend — when client is selected from dropdown

**File:** `new-receipt.component.ts` — `clientChange()` (lines ~1299–1301)
```typescript
clientChange(value: any) {
  const client = this.clientList.find((x: any) => x.Code === value);
  this.applyClientTds(client);      // ← called here on client change
  ...
}
```

### The `applyClientTds()` method — the heart of GST auto-calc

**File:** `new-receipt.component.ts` (lines ~1534–1544)
```typescript
private applyClientTds(client: any): void {
  const isTdsApplicable = client?.TDSApplicable === true || client?.TDSApplicable === 1;
  const tdsPercentage = isTdsApplicable ? Number(client.TDSPercentage) || 0 : 0;
  const isGstApplicable = isTdsApplicable &&              // ← BUG: GST gated on TDS
      (client?.GSTApplicable === true || client?.GSTApplicable === 1);
  const gstPercentage = isGstApplicable ? Number(client.GSTPercentage) || 0 : 0;

  this.frm.get('HQPercentage')?.setValue(tdsPercentage);
  this.frm.get('TaxPercentage')?.setValue(
    gstPercentage > 0 ? gstPercentage : this.frm.get('TaxPercentage')?.value
  );
  this.calculate();
}
```

**⚠️ BUG:** `isGstApplicable` requires `isTdsApplicable` to be `true`. A client with only GST (no TDS) will never have their GST % applied.

### The `calculate()` method — what TaxPercentage drives

**File:** `new-receipt.component.ts` — `calculate()` (lines ~1874–1990)

Once `TaxPercentage` is set, `calculate()` correctly derives:
```typescript
this.taxAmount = this.taxPercentage * this.chequeAmount / (100 + this.taxPercentage);
// (back-calculation — GST is extracted from the entered gross amount)
```

And finally:
```typescript
this.amountWithGST = (baseAmt + this.taxAmount).toFixed(2);  // line ~2003
```

**Answer to Q3 & Q4:**  
- When a client is selected, `applyClientTds()` IS called — the wiring is there.  
- GST % IS read from the client object and set into `TaxPercentage` — but only if `TDSApplicable` is also `true`.  
- The calculation logic in `calculate()` correctly uses `TaxPercentage` to derive `TaxAmount`.  
- **Current state:** GST auto-calc works only for clients with both TDS + GST enabled. Clients with GST only get 0% tax.

---

## 4. Full Flow Diagram

```
[Client Master Form]
  User checks GSTApplicable + enters GSTPercentage
  savebuttonClick() → GSTPercentage saved only if TDSApplicable==true  ← BUG
  POST /Master/SaveAndUpdateClientMaster
  → MasterController.cs line 603 → same gate  ← BUG
  → MasterRepository.cs line 645 → same gate  ← BUG
  → DB: ClientMaster.GSTApplicable, ClientMaster.GSTPercentage

[Receipt Page - new-receipt]
  User selects Branch
  → branchChange() → GET /Finance/GetClientByBranch → returns full ClientMaster rows
    (GSTApplicable + GSTPercentage ARE in the response — no gap here)
  User selects Client
  → clientChange() → applyClientTds(client)
    → isGstApplicable = isTdsApplicable && ...  ← BUG: needs TDS to be on
    → frm.TaxPercentage = gstPercentage (if non-zero)
    → calculate() → taxAmount = taxPercentage * amount / (100 + taxPercentage)
    → amountWithGST = baseAmount + taxAmount
```

---

## 5. Exact Fix Locations

### Fix 1 — Frontend: `savebuttonClick()` in Client Master

**File:** `d:\India Live\obms-ui\src\app\modules\master\client-master\new-client\new-client-master.component.ts`  
**Current (wrong):**
```typescript
this.clientModel.GSTPercentage = (this.clientModel.TDSApplicable && gstApplicable)
  ? Number(this.clientForm.get('GSTPercentage')?.value) || 0
  : 0;
```
**Fix:**
```typescript
this.clientModel.GSTPercentage = gstApplicable
  ? Number(this.clientForm.get('GSTPercentage')?.value) || 0
  : 0;
```

### Fix 2 — Backend: `MasterController.cs`

**File:** `d:\India Live\obms-api\Controllers\MasterController.cs` (line ~603)  
**Current:**
```csharp
GSTPercentage = (clientMasterRequestDto.TDSApplicable && clientMasterRequestDto.GSTApplicable)
    ? clientMasterRequestDto.GSTPercentage : 0,
```
**Fix:**
```csharp
GSTPercentage = clientMasterRequestDto.GSTApplicable
    ? clientMasterRequestDto.GSTPercentage : 0,
```

### Fix 3 — Backend: `MasterRepository.cs`

**File:** `d:\India Live\obms-api\Repositories\Implementation\MasterRepository.cs` (line ~645)  
**Current:**
```csharp
existingClientDetails.GSTPercentage = (clientMaster.TDSApplicable && clientMaster.GSTApplicable)
    ? clientMaster.GSTPercentage : 0;
```
**Fix:**
```csharp
existingClientDetails.GSTPercentage = clientMaster.GSTApplicable
    ? clientMaster.GSTPercentage : 0;
```

### Fix 4 — Frontend: `applyClientTds()` in Receipt component

**File:** `d:\India Live\obms-ui\src\app\modules\finance\receipts\new-receipt\new-receipt.component.ts` (lines ~1537–1538)  
**Current:**
```typescript
const isGstApplicable = isTdsApplicable &&
    (client?.GSTApplicable === true || client?.GSTApplicable === 1);
const gstPercentage = isGstApplicable ? Number(client.GSTPercentage) || 0 : 0;
```
**Fix:**
```typescript
const isGstApplicable = client?.GSTApplicable === true || client?.GSTApplicable === 1;
const gstPercentage = isGstApplicable ? Number(client.GSTPercentage) || 0 : 0;
```

---

## 6. Secondary Observation — `onGstApplicableChange()` validator gated on TDS

**File:** `new-client-master.component.ts` — `getClientMasterList()` edit-load (line ~338)
```typescript
this.onTdsApplicableChange(responseData[0].TDSApplicable === true);
// Restore GST Applicable state after patch — triggers validator setup for GSTPercentage
if (responseData[0].TDSApplicable === true) {   // ← GST validator only restored when TDS is on
  this.onGstApplicableChange(responseData[0].GSTApplicable === true);
}
```

This means when editing a client that has GST but no TDS, the `GSTPercentage` required validator is never applied, and the field may not be validated. After Fix 1–3 are applied this should also be corrected:

```typescript
this.onTdsApplicableChange(responseData[0].TDSApplicable === true);
this.onGstApplicableChange(responseData[0].GSTApplicable === true);  // always call
```

---

## 7. What the Amount Calculation Currently Does (Receipt)

When `TaxPercentage` is correctly populated (after fixes), the formula used is a **back-calculation** (GST-inclusive amount entry):

```
taxAmount = taxPercentage × receiptAmount / (100 + taxPercentage)
```

Example: Client has 18% GST, user enters ₹11,800 cheque amount.  
`taxAmount = 18 × 11800 / 118 = ₹1800` (correct — GST extracted from gross)

The `amountWithGST` field (`baseAmt + taxAmount`) shown on screen is used for display only and equals the entered cheque amount in this scheme.

---

## 8. Answers Summary

| Question | Answer |
|---|---|
| Q1: DB column names | `GSTApplicable` (BIT), `GSTPercentage` (DECIMAL 5,2) on table `ClientMaster` |
| Q2: API endpoint for client on receipt | `GET /Finance/GetClientByBranch?branch=X` — returns full ClientMaster rows including GST fields |
| Q3: GST % fetched on client select? | Yes — `applyClientTds()` called in both `branchChange()` and `clientChange()`, reads `GSTApplicable`/`GSTPercentage` |
| Q4: Current state | GST calc works only if `TDSApplicable` is also `true` — bug in 4 places |
| Q5: Files needing changes | See Fix 1–4 above — 3 backend files + 1 frontend file; no new API endpoints needed |

---

## 9. Recommendations

1. Apply **Fix 4** (frontend receipt `applyClientTds`) first — it is a 1-line change and immediately makes GST-only clients work for new receipts.
2. Apply **Fix 1** (client master save) + **Fix 3** (MasterRepository) + **Fix 2** (MasterController) to correctly persist GST for clients without TDS.
3. Apply the secondary `onGstApplicableChange` fix so edit-mode client loads restore the GST validator correctly.
4. No DB migration is needed — the columns already exist from `ClientMaster_GSTPercentage_Migration.sql`.
5. Re-save any existing clients that have `GSTApplicable=true` but `GSTPercentage=0` due to the old bug — they will need to re-enter their GST % after the fix is deployed.
