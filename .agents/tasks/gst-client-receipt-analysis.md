# GST Percentage Feature — Analysis Report

**Date:** Investigation completed  
**Scope:** Client Master GST Applicable / GST Percentage → Receipt page GST breakdown

---

## Summary Answer

The GST Applicable + GST Percentage feature is **largely implemented end-to-end** but has **two bugs** that prevent it from working correctly on the Receipt page:

1. **`GetClientByBranch` returns the full `ClientMaster` EF entity** (including `GSTApplicable` and `GSTPercentage`), so those values DO reach the Angular client list.  
2. **`applyClientTds()` in the Receipt component correctly reads `GSTApplicable` / `GSTPercentage`** and populates `TaxPercentage` in the form.  
3. **BUT `calculate()` treats GST as a back-calculated fraction of the receipt amount** (`taxAmount = taxPct * receiptAmt / (100 + taxPct)`), which is the wrong formula for "GST on top of base amount". There is no separate Base Amount + GST breakdown shown to the user — the UI only shows "Amount + GST Total" (a single read-only total field) with no line showing base vs. GST split.  
4. **The `Finance` sidebar has a "Supplier/Client TDS Slab" link that points to `routerLink="/master/tds-master"`** — a path that is correctly registered in the router. However, when the user is currently on the Finance section, clicking this link navigates away to the Master section's route **but the sidebar does not highlight Finance anymore and the section state stays on Finance**, causing apparent page-navigation confusion. This is the "Finance irukka click panna logic page pokuthu" issue reported.

---

## Evidence by File

### 1. Database Migration — `ClientMaster_GSTPercentage_Migration.sql`

```sql
ALTER TABLE ClientMaster ADD GSTApplicable BIT NOT NULL DEFAULT 0;
ALTER TABLE ClientMaster ADD GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0;
```

**Status: ✅ Migration script exists and is correct.** Both columns will be added idempotently (wrapped in `IF NOT EXISTS`). Migration must be run on the live DB before the feature works.

---

### 2. API Domain Model — `obms-api/Models/Domain/ClientMaster.cs`

```csharp
public bool GSTApplicable { get; set; }

[Column(TypeName = "decimal(5,2)")]
public decimal GSTPercentage { get; set; }
```

**Status: ✅ Both fields present** in the EF Domain model. Entity Framework will map them to the DB columns created by the migration.

---

### 3. API DTO — `obms-api/Models/DTO/ClientMasterRequestDto.cs`

```csharp
public bool GSTApplicable { get; set; }

[Range(0, 100)]
public decimal GSTPercentage { get; set; }
```

**Status: ✅ Both fields present** in the DTO with a range validator.

---

### 4. API Controller — `obms-api/Controllers/MasterController.cs` (line ~601)

```csharp
GSTApplicable = clientMasterRequestDto.GSTApplicable,
GSTPercentage = (clientMasterRequestDto.TDSApplicable && clientMasterRequestDto.GSTApplicable)
    ? clientMasterRequestDto.GSTPercentage : 0,
```

**Status: ✅ Save endpoint maps GST fields correctly.** `GSTPercentage` is only persisted when both `TDSApplicable` AND `GSTApplicable` are true — matching the UI's conditional display logic.

---

### 5. API Repository — `obms-api/Repositories/Implementation/MasterRepository.cs` (line ~639)

```csharp
existingClientDetails.GSTApplicable = clientMaster.GSTApplicable;
existingClientDetails.GSTPercentage = (clientMaster.TDSApplicable && clientMaster.GSTApplicable)
    ? clientMaster.GSTPercentage : 0;
```

**Status: ✅ Update path also maps GST fields correctly.**

---

### 6. Client Master HTML — `new-client-master.component.html`

```html
<!-- GST Applicable checkbox — shown only when TDS Applicable is checked -->
<div class="col-12 col-sm-12 col-md-4 col-lg-4 d-flex align-items-center"
    *ngIf="clientForm.get('TDSApplicable')?.value">
    <mat-checkbox formControlName="GSTApplicable"
        (change)="onGstApplicableChange($event.checked)">
        GST Applicable
    </mat-checkbox>
</div>
<!-- GST Percentage text box — shown only when both TDS & GST Applicable are checked -->
<div class="col-12 col-sm-12 col-md-4 col-lg-4"
    *ngIf="clientForm.get('TDSApplicable')?.value && clientForm.get('GSTApplicable')?.value">
    <mat-form-field appearance="outline" class="w-100">
        <mat-label>GST Percentage</mat-label>
        <input type="number" formControlName="GSTPercentage" matInput
            placeholder="e.g. 18" autocomplete="off" min="0" max="100">
    </mat-form-field>
</div>
```

**Status: ✅ Both UI controls exist and are correctly shown conditionally.** `GSTApplicable` checkbox appears only when `TDSApplicable` is checked. `GSTPercentage` input appears only when both are checked.

**⚠️ UX note:** The user must first check "TDS Applicable" to see the "GST Applicable" checkbox. If a client has GST but not TDS, they cannot enter a GST percentage at all. This may be intentional (GST is on top of TDS), but may confuse users who expect standalone GST.

---

### 7. Client Master TypeScript — `new-client-master.component.ts`

Form group definition:
```typescript
TDSApplicable: this.fb.control(false),
TDSPercentage: this.fb.control(null),
GSTApplicable: this.fb.control(false),
GSTPercentage: this.fb.control(null),
```

`onGstApplicableChange()`:
```typescript
onGstApplicableChange(isApplicable: boolean): void {
    const gstPercentageControl = this.clientForm.get('GSTPercentage');
    if (isApplicable) {
        gstPercentageControl?.setValidators([Validators.required, Validators.min(0), Validators.max(100)]);
    } else {
        gstPercentageControl?.clearValidators();
        gstPercentageControl?.setValue(null);
    }
    gstPercentageControl?.updateValueAndValidity();
}
```

`savebuttonClick()` save path:
```typescript
const gstApplicable = this.clientForm.get('GSTApplicable')?.value === true;
this.clientModel.GSTApplicable = gstApplicable;
this.clientModel.GSTPercentage = (this.clientModel.TDSApplicable && gstApplicable)
    ? Number(this.clientForm.get('GSTPercentage')?.value) || 0
    : 0;
```

`getClientMasterList()` (edit/load path):
```typescript
GSTApplicable: responseData[0].GSTApplicable ?? false,
GSTPercentage: responseData[0].GSTPercentage ?? null,
```
followed by:
```typescript
if (responseData[0].TDSApplicable === true) {
    this.onGstApplicableChange(responseData[0].GSTApplicable === true);
}
```

**Status: ✅ Form binding, validation, save, and load are all correctly wired.** The edit path properly restores both checkboxes and the percentage value.

---

### 8. Client Model TypeScript — `obms-ui/src/app/model/clientModel.ts`

```typescript
TDSApplicable: boolean;
TDSPercentage: number;
GSTApplicable: boolean;
GSTPercentage: number | null = null;
```

**Status: ✅ Angular model includes both fields.**

---

### 9. Finance API — `GetClientByBranch` endpoint (FinanceController.cs line ~5058)

```csharp
[Route("GetClientByBranch")]
public async Task<ActionResult<Object>> GetClientByBranch(string branch)
{
    return await _oBMSDbContext.ClientMasters
        .Where(x => x.Status != "Inactive" && x.Branch == branch)
        .ToListAsync();
}
```

**Status: ✅ Returns the full `ClientMaster` entity, which includes `GSTApplicable` and `GSTPercentage`.** These fields will be in the JSON response once the DB migration is run.

---

### 10. Receipt TypeScript — `applyClientTds()` method

```typescript
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

**Status: ✅ Method correctly reads `GSTApplicable` and `GSTPercentage` from the client object and populates `TaxPercentage` in the form.** This is called both in `clientChange()` and `branchChange()`.

---

### 11. Receipt TypeScript — `calculate()` GST formula (BUG)

```typescript
} else if (receiptTypeID.toString() === "1") {
    this.taxAmount = this.taxPercentage * this.chequeAmount / (100 + this.taxPercentage);
    this.hqAmount = (this.chequeAmount - this.taxAmount) * (this.hqPercentage / 100);
    this.branchAmount = this.chequeAmount - this.taxAmount - this.hqAmount;
}
```

**Status: ❌ BUG — Wrong GST formula.** The current formula `GST = taxPct × amount / (100 + taxPct)` is the **back-calculation formula** (for when GST is already included in the amount). This assumes the receipt amount entered is the **gross amount including GST**.

The correct interpretation for India (GST on top of service fee) should be:
- `baseAmount = receiptAmount` (what the client entered)  
- `gstAmount = baseAmount × (taxPct / 100)`  
- `totalWithGST = baseAmount + gstAmount`

But the current code does the reverse: it extracts GST out of the entered amount rather than adding it on top.

This makes `TaxAmount` wrong and `BranchCollection` (= receipt - tax - TDS) wrong as a consequence.

---

### 12. Receipt HTML — Amount breakdown display (MISSING)

The receipt HTML shows:
```html
<!-- Amount + GST Total replaces TaxAmount display -->
<div class="col-8 col-sm-8 col-md-8 col-lg-3">
    <mat-form-field appearance="outline" class="w-100">
        <mat-label>Amount + GST Total</mat-label>
        <input type="text" [value]="amountWithGST" matInput placeholder="" autocomplete="off" readonly>
    </mat-form-field>
</div>
```

**Status: ❌ MISSING — No breakdown.** There is no field showing:
- Base Amount (service amount before GST)
- GST Amount (the tax portion)
- Total = Base + GST

The user requirement is: "receipt page irukka click select panna etha client master atha client etha % podu irukka vatha show pannu amount iruthu GSt + kami kanu analysis pannu" — meaning the receipt page should show amount broken down into base + GST clearly.

Currently only a single "Amount + GST Total" field is shown, with no visible breakdown. `TaxAmount` is hidden:
```html
<!-- TaxAmount hidden — kept in form for save, not shown in UI -->
<div style="display:none">
    <input type="text" formControlName="TaxAmount" matInput>
```

---

### 13. TDS Slab Sidebar Navigation Bug (Finance section)

**Sidebar HTML (sidebar.component.html):**
```html
<!-- Under Finance submenu: -->
<li class="nav-item" routerLink="/master/tds-master">
    <a class="nav-link">Supplier/Client TDS Slab</a>
</li>
```

**Route in admin-layout-routing.module.ts:**
```
{ path: 'master/tds-master', component: TdsMasterComponent, canActivate: [AuthGuard] }
```

**Status: ❌ BUG — Wrong section navigation.** The Finance submenu contains a "Supplier/Client TDS Slab" link that points to `/master/tds-master`. This is a Master section route. When clicked from the Finance section context:
- The router navigates to `master/tds-master` ✅
- The sidebar `isSubmenuOpen('finance')` state collapses because the route belongs to Master, and `toggleSubmenu` uses route-based detection — the user appears to be taken out of Finance context entirely ❌
- This is the "Finance irukka click panna logic page pokuthu podu kodathu" (Finance page click → goes to another page, shouldn't) bug

The fix: The link should stay in Finance sidebar but route correctly. Either:
  - Keep the link pointing to `master/tds-master` but ensure the component itself doesn't have Finance-specific guards, OR  
  - Move TDS Master component into the Finance module routes (e.g., `finance/tds-master`)

---

## What Is Missing / Broken — Summary Table

| # | Location | Issue | Severity |
|---|----------|-------|----------|
| 1 | **DB** | `GSTApplicable`/`GSTPercentage` columns may not exist in live DB (migration not confirmed run) | 🔴 Blocker |
| 2 | **Receipt TS** `calculate()` | GST formula uses back-calculation (`×amt/(100+pct)`) instead of forward (`×amt/100`) | 🔴 Wrong calculation |
| 3 | **Receipt HTML** | No Base Amount + GST Amount breakdown display — user can't see the split | 🟡 Missing feature |
| 4 | **Sidebar** | Finance → "Supplier/Client TDS Slab" navigates to Master route, appears to leave Finance | 🟡 UX bug |
| 5 | **Client Master UX** | `GSTApplicable` checkbox only visible when `TDSApplicable` is checked — blocks standalone GST clients | 🟢 Possible design intent, but verify |

---

## Recommended Fixes

### Fix 1: Run DB Migration
Run `ClientMaster_GSTPercentage_Migration.sql` on the live database. This is a prerequisite for all other fixes.

### Fix 2: Fix GST calculation formula in Receipt (`calculate()`)

**File:** `obms-ui/src/app/modules/finance/receipts/new-receipt/new-receipt.component.ts`

Change the non-adjustment branch in `calculate()`:

```typescript
// CURRENT (wrong — back-calculates GST from gross):
this.taxAmount = this.taxPercentage * this.chequeAmount / (100 + this.taxPercentage);
this.branchAmount = this.chequeAmount - this.taxAmount - this.hqAmount;

// CORRECT (forward — GST is added on top of base amount):
this.taxAmount = this.chequeAmount * (this.taxPercentage / 100);
this.hqAmount = this.chequeAmount * (this.hqPercentage / 100);
this.branchAmount = this.chequeAmount + this.taxAmount - this.hqAmount;
// ReceiptAmount to save = chequeAmount + taxAmount (total including GST)
```

Apply the same fix for `cashAmount` and `ibgAmount` branches. Also update `amountWithGST` computation (currently correct: `baseAmt + taxAmount`).

### Fix 3: Show Base Amount + GST breakdown in Receipt HTML

**File:** `obms-ui/src/app/modules/finance/receipts/new-receipt/new-receipt.component.html`

Unhide `TaxAmount` and show it as "GST Amount". Add a "Base Amount" read-only display. Example:

```html
<!-- Replace the hidden TaxAmount div with visible breakdown: -->
<div class="col-4 col-sm-4 col-md-4 col-lg-2">
  <mat-form-field appearance="outline">
    <mat-label>Base Amount</mat-label>
    <input type="text" [value]="chequeAmount || cashAmount || ibgAmount | number:'1.2-2'" 
           matInput readonly>
  </mat-form-field>
</div>
<div class="col-4 col-sm-4 col-md-4 col-lg-2">
  <mat-form-field appearance="outline">
    <mat-label>GST Amount</mat-label>
    <input type="text" formControlName="TaxAmount" matInput readonly>
  </mat-form-field>
</div>
<!-- Keep "Amount + GST Total" field -->
```

In the TS, expose `baseAmount` as a computed property for the template.

### Fix 4: Fix Finance sidebar TDS Slab navigation

**File:** `obms-ui/src/app/components/sidebar/sidebar.component.html`

The Finance sidebar link:
```html
<li class="nav-item" routerLink="/master/tds-master">
    <a class="nav-link">Supplier/Client TDS Slab</a>
</li>
```

**Option A (simplest):** Keep the `routerLink="/master/tds-master"` but check if `toggleSubmenu` needs a route-match fix. Add `[routerLinkActiveOptions]="{exact:true}"` and ensure the sidebar state doesn't collapse Finance when navigating to a Master route from Finance.

**Option B (cleaner):** Add an alias route in the Finance routes:
```typescript
{ path: 'finance/tds-master', redirectTo: 'master/tds-master', pathMatch: 'full' }
```
and change the sidebar link to `routerLink="/finance/tds-master"`. This keeps the user appearing to stay in Finance context.

---

## What Is Fully Working (No Changes Needed)

- ✅ Client Master form: `GSTApplicable` checkbox + `GSTPercentage` input field
- ✅ Client Master save/update: correctly persists `GSTApplicable` and `GSTPercentage` to DB
- ✅ Client Master load/edit: correctly restores both fields from DB
- ✅ `GetClientByBranch` API: returns `GSTApplicable` and `GSTPercentage` in response
- ✅ Receipt `applyClientTds()`: reads client's GST fields and sets `TaxPercentage` in the form
- ✅ `amountWithGST` variable: computes base + tax total (correct formula: `baseAmt + taxAmount`)
- ✅ Receipt save: passes `TaxPercentage` and `TaxAmount` to the API
- ✅ All API models and DTOs: include `GSTApplicable` and `GSTPercentage`
