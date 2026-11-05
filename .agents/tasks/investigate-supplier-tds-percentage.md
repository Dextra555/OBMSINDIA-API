# Investigation: Supplier Page — Is TDS Checkbox & TDS Percentage Field

**Task:** When "Is TDS" is unchecked → Percentage field must be hidden and not required.  
When "Is TDS" is checked → Percentage field must be shown, enabled, and required.

---

## Summary Answer

**The frontend already has conditional show/hide wired correctly** via `*ngIf="isTDSChecked"` on the TDS Details row, and the getter `isTDSChecked` is already implemented in the TS component. **The one missing piece is that `Validators.required` is never added to `TDSPercentage` when `IsTDS` is checked.** The backend and database already fully support `IsTDS` + `TDSPercentage`. Only one small change is needed in the TS component.

---

## 1. File Paths

| File | Path |
|------|------|
| HTML template | `d:\India Live\obms-ui\src\app\modules\inventory\supplier-master\new-supplier\new-supplier.component.html` |
| TS component | `d:\India Live\obms-ui\src\app\modules\inventory\supplier-master\new-supplier\new-supplier.component.ts` |
| Domain model | `d:\India Live\obms-api\Models\Domain\Supplier.cs` |
| DTO | `d:\India Live\obms-api\Models\DTO\SupplierRequestDto.cs` |
| API controller | `d:\India Live\obms-api\Controllers\InventoryController.cs` (route: `POST Inventory/SaveAndUpdateSupplier`) |
| DB migration | `d:\India Live\obms-api\Database\Supplier_IsTDS_TDSPercentage_Migration.sql` |

---

## 2. Current Form Field Names

| UI Label | Angular form control name | DB column |
|----------|--------------------------|-----------|
| Is TDS (checkbox) | `IsTDS` | `Suppliers.IsTDS` (BIT NOT NULL DEFAULT 0) |
| Percentage (dropdown) | `TDSPercentage` | `Suppliers.TDSPercentage` (DECIMAL(5,2) NULL) |

---

## 3. What Already Works (No Change Needed)

### HTML — conditional show/hide is already correct

In `new-supplier.component.html` (lines ~157–171):

```html
<div class="row" *ngIf="isTDSChecked">
    <div class="col-12 col-sm-12 col-md-12 col-lg-12 wlt-c-furg-profile">
        <h2><span>TDS Details</span></h2>
    </div>
    <div class="col-12 col-sm-12 col-md-4 col-lg-4">
        <mat-form-field appearance="outline">
            <mat-label>Percentage</mat-label>
            <mat-select formControlName="TDSPercentage">
                <mat-option *ngFor="let t of tdsMasterList" [value]="t.TDSPercentage">
                    {{ t.TDSName }} — {{ t.TDSPercentage }}%
                </mat-option>
            </mat-select>
        </mat-form-field>
    </div>
</div>
```

The getter in the TS component is also already correct:

```typescript
get isTDSChecked(): boolean {
    return this.frm.get('IsTDS')?.value === true;
}
```

So hiding/showing the section when the checkbox is toggled **already works**.

### Backend — fully ready

- `Supplier` domain model has `IsTDS: bool` and `TDSPercentage: decimal?` ✅  
- `SupplierRequestDto` has both fields ✅  
- `InventoryController.SaveAndUpdateSupplier` maps `dto.IsTDS → obj.IsTDS` and `dto.TDSPercentage → obj.TDSPercentage` ✅  
- Database migration adds `IsTDS BIT NOT NULL DEFAULT 0` and `TDSPercentage DECIMAL(5,2) NULL` ✅

---

## 4. The Gap — Missing `Validators.required` on TDSPercentage

### Current state in `new-supplier.component.ts`

Form control at construction:
```typescript
IsTDS: [false],
TDSPercentage: [null]   // ← no validator at all, not even conditionally
```

In `ngOnInit`, there is a `valueChanges` subscription for `IsTax` that adds/removes `Validators.required` on `GSTNo` and `PanNo`. **There is NO equivalent subscription for `IsTDS` / `TDSPercentage`.**

So right now:
- When IsTDS is checked, the Percentage dropdown appears (correct) but is **not required** — the form submits without a value.
- When IsTDS is unchecked, the row disappears from the DOM (correct) but the form control still holds whatever value was last set — it is **never cleared**.

### Reference pattern — client-master uses `onTdsApplicableChange()`

`new-client-master.component.ts` solves the same problem using a `(change)` event handler:

```typescript
onTdsApplicableChange(isApplicable: boolean): void {
    const percentageControl = this.clientForm.get('TDSPercentage');
    if (isApplicable) {
        percentageControl?.setValidators([Validators.required]);
    } else {
        percentageControl?.clearValidators();
        percentageControl?.setValue(null);
    }
    percentageControl?.updateValueAndValidity();
}
```

And in the HTML the checkbox calls it:
```html
<mat-checkbox formControlName="TDSApplicable"
    (change)="onTdsApplicableChange($event.checked)">
```

The supplier component uses the `IsTax` → `valueChanges` pattern instead, which is equally valid.

---

## 5. Recommended Changes (Frontend Only)

### Change 1 — TS component: add a `valueChanges` subscription for `IsTDS`

In `new-supplier.component.ts`, inside `ngOnInit()`, right after the existing `IsTax` valueChanges block, add:

```typescript
// When IsTDS toggles, add/remove Validators.required for TDSPercentage
this.frm.get('IsTDS')?.valueChanges.subscribe((checked: boolean) => {
    const tdsPercentageControl = this.frm.get('TDSPercentage');
    if (checked) {
        tdsPercentageControl?.setValidators([Validators.required]);
    } else {
        tdsPercentageControl?.clearValidators();
        tdsPercentageControl?.setValue(null);   // clear the value when unchecked
    }
    tdsPercentageControl?.updateValueAndValidity();
});
```

This matches exactly the pattern already used for `IsTax` in the same component.

### Change 2 — HTML template: add `<mat-error>` for TDSPercentage

In `new-supplier.component.html`, inside the TDS Details row, add an error hint to the `mat-form-field` for Percentage:

```html
<mat-form-field appearance="outline">
    <mat-label>Percentage</mat-label>
    <mat-select formControlName="TDSPercentage">
        <mat-option *ngFor="let t of tdsMasterList" [value]="t.TDSPercentage">
            {{ t.TDSName }} — {{ t.TDSPercentage }}%
        </mat-option>
    </mat-select>
    <mat-error *ngIf="frm.get('TDSPercentage')?.hasError('required')">
        TDS Percentage is required
    </mat-error>
</mat-form-field>
```

---

## 6. Backend Changes Needed

**None.** The API controller, DTO, domain model, and database all already handle `IsTDS` and `TDSPercentage` correctly. The backend will naturally receive `TDSPercentage: null` when the user unchecks "Is TDS" (because the form control is cleared) and store `NULL` in the DB, which is the correct behavior.

---

## Summary of Changes

| # | File | Change |
|---|------|--------|
| 1 | `new-supplier.component.ts` | Add `valueChanges` subscription on `IsTDS` that calls `setValidators([Validators.required])` when checked and `clearValidators()` + `setValue(null)` when unchecked |
| 2 | `new-supplier.component.html` | Add `<mat-error>` inside the TDSPercentage `mat-form-field` to surface the required-validation message |

Both changes follow the pattern already used in the same project (`IsTax` in new-supplier.component.ts and `TDSApplicable` in new-client-master.component.ts).
