# Employee Master - Join Date மற்றும் Branch History Tracking

## மாற்றங்கள் சுருக்கம் (Summary of Changes)

### 1. Employee Table-ல் புதிய Field

**JoinDate** field-ஐ Employee table-ல் சேர்த்துள்ளோம்:
- இந்த field-ல் employee company-யில் join செய்த original date store ஆகும்
- Form-ல் இருந்து EMPPAY_DATE_JOINED value இதில் save ஆகும்

### 2. EmployeeHistory Table-ல் Date Tracking

EmployeeHistory table-ல் ஏற்கனவே இருக்கும் fields:
- **Emp_StartDate**: Employee ஒரு குறிப்பிட்ட branch-ல் start செய்த date
- **Emp_EndDate**: Employee அந்த branch-ல் முடித்த date (NULL என்றால் இன்னும் அந்த branch-ல் active)
- **JoinDate**: Company-யில் join செய்த original date (reference-க்காக)

### 3. Unique Error Fix

**Problem:** Save செய்யும் போது duplicate active records (Emp_EndDate = NULL) create ஆகி unique constraint error வரும்

**Solution:** 
- Regular employee update போது existing active record-ஐ update செய்யும் (new record create செய்யாது)
- Branch change போது மட்டும் old record close பண்ணி new record create செய்யும்
- Safety check: Multiple active records-ஐ automatic-ஆ close செய்யும்

---

## எப்படி வேலை செய்யும் (How It Works)

### புதிய Employee Create செய்யும் போது (New Employee Creation)

1. Form-ல் **Join Date** field add செய்யவும் (புதிய field)
2. User Join Date enter செய்யும் (e.g., 2024-01-15)
3. System automatically செய்யும்:
   - Form-ல் இருந்து வரும் JoinDate-ஐ Employee table-ல் store பண்ணும்
   - **இதே JoinDate value-ஐ EmployeeHistory.Emp_StartDate-ஆ save செய்யும்** ⭐
   - EmployeeHistory table-ல் முதல் record create செய்யும்:
     * **Emp_StartDate = Form JoinDate** (முக்கியம்!)
     * Emp_EndDate = NULL (currently active)
     * EMP_BRANCH_CODE = initial branch
     * JoinDate = Form JoinDate (reference-க்காக)
   - Safety check: Duplicate active record இல்லை என்று confirm செய்யும்

**Form-ல் இருந்து வரும் data:**
```javascript
{
  JoinDate: "2024-01-15",           // ⭐ புதிய field - இதுதான் முக்கியம்!
  EMPPAY_DATE_JOINED: "2024-01-15"  // Employment details date (optional)
}
```

**Database-ல் save ஆகும் data:**
```
Employee Table:
  JoinDate = 2024-01-15  ← Form-ல் இருந்து

EmployeeHistory Table (1st Record):
  Emp_StartDate = 2024-01-15  ← Form JoinDate-ல் இருந்து! ⭐
  Emp_EndDate = NULL
  EMP_BRANCH_CODE = Branch A
  JoinDate = 2024-01-15
```

**Priority Order (Fallback logic):**
1. **First Priority:** Form JoinDate field (employee.JoinDate) ⭐
2. **Second Priority:** EMPPAY_DATE_JOINED (employment.EMPPAY_DATE_JOINED)
3. **Last Resort:** Current date (DateTime.Now)

### இது முக்கியம்! ⚠️
- Form-ல் JoinDate field கட்டாயம் add செய்யணும்
- அந்த value-தான் EmployeeHistory.Emp_StartDate-ஆ save ஆகும்
- EMPPAY_DATE_JOINED-ஐ மட்டும் rely பண்ண கூடாது

---

### Employee-ஐ Regular Update செய்யும் போது (No Branch Change)

1. Employee details-ஐ edit செய்யும் (name, address, phone, etc.)
2. **JoinDate-ஐ edit செய்யலாம்** ⭐
3. Branch மாற்றாமல் save செய்யும்
4. System automatically செய்யும்:
   - Existing active EmployeeHistory record-ஐ update செய்யும்
   - **JoinDate மாற்றினால், Emp_StartDate-ஐயும் update செய்யும்!** ⭐
   - **New record create செய்யாது** (duplicate தவிர்க்க)
   - Emp_EndDate மாறாது (still NULL)

**உதாரணம் 1: Name மட்டும் மாற்றுதல்:**
```
Employee address மாத்துறாங்க ஆனா JoinDate மாறல:

EmployeeHistory Table (Same Record - Updated):
  EMP_ADDRESS1 = புதிய address    ← மாற்றம்
  Emp_StartDate = 2024-01-15      ← மாறல (same)
  Emp_EndDate = NULL              ← மாறல
  EMP_BRANCH_CODE = Branch A      ← மாறல
```

**உதாரணம் 2: JoinDate மாற்றுதல்:**
```
Employee-ன் JoinDate-ஐ தவறாக enter பண்ணிட்டாங்க, edit செய்து correct பண்றாங்க:

Before Edit:
  Employee.JoinDate = 2024-01-15
  EmployeeHistory.Emp_StartDate = 2024-01-15

After Edit (JoinDate changed to 2024-01-20):
  Employee.JoinDate = 2024-01-20  ← மாற்றம்
  EmployeeHistory.Emp_StartDate = 2024-01-20  ← Auto-updated! ⭐
  Emp_EndDate = NULL (still active)
  No new record created! ✅
```

---

### Employee-ஐ Edit செய்து Branch Change செய்யும் போது

1. Edit form-ல் branch மாற்றவும்
2. **Effective Start Date** (BranchStartDate) கொடுக்கவும் - இது புதிய branch-ல் start செய்யும் date
3. System automatically செய்யும்:
   - **எல்லா** open active records-ஐயும் close செய்யும் (safety measure)
   - பழைய record-ன் Emp_EndDate = New StartDate - 1 day
   - புதிய EmployeeHistory record create செய்யும்:
     * Emp_StartDate = Effective Start Date
     * Emp_EndDate = NULL (new active record)
     * EMP_BRANCH_CODE = new branch

**உதாரணம் (Example):**
```
Employee Branch A-ல் இருந்து Branch B-க்கு 2024-06-01 அன்று transfer:

EmployeeHistory Record 1 (முந்தைய - Updated):
  Emp_StartDate = 2024-01-15
  Emp_EndDate = 2024-05-31       ← மூடப்பட்டது (Closed)
  EMP_BRANCH_CODE = Branch A

EmployeeHistory Record 2 (புதிய - New):
  Emp_StartDate = 2024-06-01     ← புதிய start date
  Emp_EndDate = NULL             ← இப்போது active
  EMP_BRANCH_CODE = Branch B
  JoinDate = 2024-01-15          ← மூல join date மாறாது
```

---

## Database Migration

### Step 1: JoinDate Column Migration
**Migration file:** `Employee_JoinDate_Migration.sql`

இந்த SQL script run செய்ய வேண்டும்:
1. Employee table-ல் JoinDate column add செய்யும்
2. EmployeeHistory table-ல் JoinDate column add செய்யும்
3. Existing employees-க்கு EmploymentDetails-ல் இருந்து data populate செய்யும்

```sql
-- SQL Server Management Studio-ல் open செய்து execute செய்யவும்
```

### Step 2: Cleanup Duplicate Active Records
**Migration file:** `EmployeeHistory_CleanupDuplicates.sql`

இந்த SQL script run செய்ய வேண்டும் (unique error வந்தால்):
1. Multiple active records உள்ள employees-ஐ identify செய்யும்
2. மிக சமீபத்திய record-ஐ தவிர மற்ற எல்லாவற்றையும் close செய்யும்
3. Verification செய்து final state show செய்யும்

```sql
-- இது one-time cleanup script
-- Duplicate records இருந்தால் மட்டும் run செய்யவும்
```

---

## முக்கியமான Points (Important Points)

### புதிய Employee Create செய்யும் போது
✅ Join Date கட்டாயம் கொடுக்க வேண்டும்
✅ System automatically Employee table மற்றும் EmployeeHistory table-ல் store செய்யும்
✅ Duplicate check செய்து safe-ஆ create செய்யும்

### Regular Update செய்யும் போது (Branch Change இல்லாமல்)
✅ Existing active record update ஆகும்
✅ New record create ஆகாது (duplicate தவிர்க்க)
✅ Emp_StartDate மற்றும் Emp_EndDate மாறாது

### Branch Change செய்யும் போது
✅ Effective Start Date கொடுக்க வேண்டும்
✅ Old branch record automatically close ஆகும் (Emp_EndDate set ஆகும்)
✅ New branch record create ஆகும்
✅ Original join date மாறாது (preserved)
✅ Multiple active records இருந்தால் எல்லாம் close ஆகும் (safety)

### Unique Error Prevention (முக்கியம்!)
✅ ஒரு employee-க்கு ஒரு நேரத்தில் ஒரு active record மட்டும்
✅ Regular update போது new record create செய்யாது
✅ Branch change போது மட்டும் new record create செய்யும்
✅ Legacy duplicate records automatic-ஆ cleanup ஆகும்

### Benefits (பயன்கள்)
✅ Employee எப்போது company-யில் join செய்தார் என்று தெரியும்
✅ Employee எந்த எந்த branch-ல் எப்போது வேலை செய்தார் என்று complete history தெரியும்
✅ குறிப்பிட்ட period-ல் எந்த employee எந்த branch-ல் இருந்தார் என்று query செய்யலாம்
✅ Duplicate active records வராது (unique error fix)
✅ Reports generate செய்ய easy

---

## Code Changes (மாற்றங்கள்)

### 1. New Employee Creation
```csharp
if (isNewEmployee)
{
    // Use JoinDate from form (employee.JoinDate) as Emp_StartDate ⭐
    DateTime startDate;
    
    if (employee.JoinDate.HasValue)
    {
        // Priority 1: Use JoinDate from form
        startDate = employee.JoinDate.Value.Date;
    }
    else if (employment.EMPPAY_DATE_JOINED.HasValue)
    {
        // Priority 2: Fallback to EMPPAY_DATE_JOINED
        startDate = employment.EMPPAY_DATE_JOINED.Value.Date;
    }
    else
    {
        // Priority 3: Last resort - current date
        startDate = DateTime.Now.Date;
    }
    
    // Safety check: No duplicate active record
    var existingActive = await _oBMSDbContext.EmployeeHistories
        .Where(h => h.EMP_ID == employee.EMP_ID && h.Emp_EndDate == null)
        .FirstOrDefaultAsync();
    
    if (existingActive == null)
    {
        await InsertEmployeeHistoryRow(
            empStartDate: startDate,  // ← Form JoinDate இங்க use ஆகும்!
            empEndDate: null
        );
    }
}
```

### 2. Controller - JoinDate Mapping
```csharp
// Form-ல் இருந்து JoinDate-ஐ Employee table-ல் save
if (employeeRequestDto.JoinDate.HasValue)
{
    employee.JoinDate = employeeRequestDto.JoinDate.Value;
}
else if (employeeRequestDto.EMPPAY_DATE_JOINED.HasValue)
{
    employee.JoinDate = employeeRequestDto.EMPPAY_DATE_JOINED.Value;
}
else if (employeeRequestDto.EMP_ID == 0)
{
    employee.JoinDate = DateTime.Now;
}
```

### 2. Regular Update (No Branch Change)
```csharp
else  // Regular update, no branch change
{
    var activeHistory = await _oBMSDbContext.EmployeeHistories
        .Where(h => h.EMP_ID == employee.EMP_ID && h.Emp_EndDate == null)
        .FirstOrDefaultAsync();
    
    if (activeHistory != null)
    {
        // Update existing record instead of creating new one
        await UpdateEmployeeHistoryRow(activeHistory, ...);
    }
}
```

### 3. Branch Change
```csharp
else if (isBranchChanged && branchStartDate.HasValue)
{
    // Close ALL open records (safety)
    var openHistories = await _oBMSDbContext.EmployeeHistories
        .Where(h => h.EMP_ID == employee.EMP_ID && h.Emp_EndDate == null)
        .ToListAsync();
    
    foreach (var openHistory in openHistories)
    {
        openHistory.Emp_EndDate = newStartDate.AddDays(-1);
    }
    
    // Create new active record
    await InsertEmployeeHistoryRow(...);
}
```

---

## UI Changes Required (தேவையான UI மாற்றங்கள்)

### New Employee Form
**கட்டாய fields (Required):**
- **Join Date** field - Employee company-யில் join செய்த date ⭐⭐⭐
  - **இது மிக முக்கியம்!** இந்த value-தான் EmployeeHistory.Emp_StartDate-ஆ save ஆகும்
  - Date picker component பயன்படுத்தவும்
  - Field name: `JoinDate` (EmployeeRequestDto-ல்)
  - Default value: சில நேரங்களில் EMPPAY_DATE_JOINED-உடன் sync செய்யலாம்

**Form structure:**
```typescript
// Employee Form Model
{
  JoinDate: Date,              // ⭐ Primary - இதுதான் Emp_StartDate-ஆ save ஆகும்!
  EMPPAY_DATE_JOINED: Date,    // Secondary - Fallback only
  // ... other fields
}
```

**Validation:**
- **JoinDate கட்டாயம் இருக்க வேண்டும்** (required)
- Join date future date-ஆ இருக்கக்கூடாது
- Join date reasonable range-ல் இருக்க வேண்டும் (e.g., last 50 years)
- Warning: JoinDate இல்லாவிட்டால் EMPPAY_DATE_JOINED use ஆகும் (less preferred)

### Edit Employee Form

**Regular Update (Branch மாற்றாமல்):**
- Employee details மட்டும் edit செய்யலாம் (name, address, phone, etc.)
- JoinDate edit செய்ய முடியாது (display only)
- Branch change checkbox unchecked இருக்கும்
- Effective Start Date field hide/disabled இருக்கும்

**Branch Transfer (Branch மாற்றும் போது):**
- New Branch dropdown select செய்யவும்
- **Effective Start Date** field mandatory-ஆ show செய்யவும்
  - Field name: `BranchStartDate` (EmployeeRequestDto-ல்)
  - புதிய branch-ல் start செய்யும் date
- Set: `IsBranchChanged = true`

**Validation:**
- Effective Start Date > Previous branch start date
- Effective Start Date <= Current date (future date allow செய்யலாம் if needed)
- Branch change confirmation alert காட்டலாம்

### Form Example (Angular/TypeScript)

```typescript
// New Employee Form
employeeForm = new FormGroup({
  JoinDate: new FormControl(null, Validators.required),
  EMPPAY_DATE_JOINED: new FormControl(null),
  // ... other controls
});

// Edit Employee Form
onBranchChange() {
  const branchChanged = this.employeeForm.get('EMP_BRANCH_CODE').value !== 
                        this.originalBranchCode;
  
  if (branchChanged) {
    this.showEffectiveStartDate = true;
    this.employeeForm.get('BranchStartDate').setValidators(Validators.required);
    this.employeeForm.get('IsBranchChanged').setValue(true);
  } else {
    this.showEffectiveStartDate = false;
    this.employeeForm.get('BranchStartDate').clearValidators();
    this.employeeForm.get('IsBranchChanged').setValue(false);
  }
}

// Save function
saveEmployee() {
  const formData = this.employeeForm.value;
  
  // Ensure JoinDate is populated
  if (!formData.JoinDate && formData.EMPPAY_DATE_JOINED) {
    formData.JoinDate = formData.EMPPAY_DATE_JOINED;
  }
  
  this.employeeService.saveEmployee(formData).subscribe(
    response => {
      if (response.Success === 'Success') {
        this.showSuccessMessage('Employee saved successfully');
      } else {
        this.showErrorMessage(response.Message);
      }
    },
    error => this.handleError(error)
  );
}
```

### HTML Template Example

```html
<!-- New Employee Form - Join Date ⭐ IMPORTANT! -->
<div class="form-group">
  <label>Join Date <span class="required">*</span></label>
  <input type="date" 
         formControlName="JoinDate" 
         class="form-control"
         [max]="today"
         required>
  <small class="text-info">
    <strong>Important:</strong> This date will be used as branch start date in history
  </small>
  <small class="text-danger" *ngIf="employeeForm.get('JoinDate').errors?.required">
    Join Date is required and will be saved as Emp_StartDate
  </small>
</div>

<!-- Optional: Employment Join Date (can be same as JoinDate) -->
<div class="form-group">
  <label>Employment Join Date</label>
  <input type="date" 
         formControlName="EMPPAY_DATE_JOINED" 
         class="form-control"
         [max]="today">
  <small class="text-muted">
    Usually same as Join Date. Used for employment records.
  </small>
</div>

<!-- Edit Employee Form - Branch Transfer -->
<div class="form-group">
  <label>Branch</label>
  <select formControlName="EMP_BRANCH_CODE" 
          class="form-control"
          (change)="onBranchChange()">
    <option *ngFor="let branch of branches" [value]="branch.Code">
      {{ branch.Name }}
    </option>
  </select>
</div>

<!-- Show only when branch changes -->
<div class="form-group" *ngIf="showEffectiveStartDate">
  <label>Effective Start Date (New Branch) <span class="required">*</span></label>
  <input type="date" 
         formControlName="BranchStartDate" 
         class="form-control"
         [min]="minEffectiveDate"
         required>
  <small class="text-info">
    Enter the date when employee starts at the new branch
  </small>
  <small class="text-danger" *ngIf="employeeForm.get('BranchStartDate').errors?.required">
    Effective Start Date is required when changing branch
  </small>
</div>
```

---

## Files மாற்றப்பட்டுள்ளது (Modified Files)

### Backend Files
1. `Models/Domain/Employee.cs` - JoinDate field added
2. `Models/Domain/EmployeeHistory.cs` - JoinDate field added
3. `Models/DTO/EmployeeRequestDto.cs` - JoinDate field added
4. `Controllers/EmployeeController.cs` - JoinDate mapping logic added
5. `Repositories/Implementation/EmployeeRepository.cs` - Updated logic:
   - InsertEmployeeHistoryRow (with duplicate check)
   - UpdateEmployeeHistoryRow (new method for regular updates)
   - saveAndUpdateEmployee (enhanced with 3 scenarios)

### Database Files
1. `Database/Employee_JoinDate_Migration.sql` - JoinDate column migration
2. `Database/EmployeeHistory_CleanupDuplicates.sql` - Cleanup script (NEW!)
3. `Database/EmployeeHistory_JoinDate_Documentation.md` - Complete documentation
4. `Database/README_Tamil.md` - Tamil explanation (updated)

---

## Testing செய்ய வேண்டியவை (Testing Checklist)

### Database Setup
- [ ] Employee_JoinDate_Migration.sql run செய்யவும்
- [ ] EmployeeHistory_CleanupDuplicates.sql run செய்யவும் (duplicate records இருந்தால்)
- [ ] Verify: ஒவ்வொரு employee-க்கும் ஒரு active record மட்டும் இருக்கா check செய்யவும்

### New Employee Testing
- [ ] புதிய employee create செய்து JoinDate check செய்யவும்
- [ ] EmployeeHistory table-ல் first record create ஆகிறதா check செய்யவும்
- [ ] Duplicate active record create ஆகலையா verify செய்யவும்

### Regular Update Testing
- [ ] Employee details (name, address) மாத்தவும் (branch மாத்தாம)
- [ ] Existing active record update ஆகிறதா check செய்யவும்
- [ ] New record create ஆகலையா verify செய்யவும்
- [ ] Total history records count அதே எண்ணிக்கையில் இருக்கா check செய்யவும்

### Branch Transfer Testing
- [ ] Branch change செய்து Effective Start Date கொடுக்கவும்
- [ ] Old record close ஆகிறதா (Emp_EndDate set) check செய்யவும்
- [ ] New record புதிய branch-ல் create ஆகிறதா check செய்யவும்
- [ ] Original JoinDate மாறாமல் இருக்கிறதா check செய்யவும்
- [ ] Verify: ஒரு active record மட்டும் இருக்கா check செய்யவும்

### Multiple Transfer Testing
- [ ] Multiple transfers செய்யவும் (3-4 times)
- [ ] Correct history maintain ஆகிறதா check செய்யவும்
- [ ] எல்லா closed records-க்கும் proper Emp_EndDate உள்ளதா check செய்யவும்
- [ ] மிக சமீபத்திய record மட்டும் active-ஆ உள்ளதா check செய்யவும்

### Error Testing
- [ ] Unique error வருதா என்று test செய்யவும் (should not happen now!)
- [ ] Multiple active records create ஆகுதா என்று check செய்யவும் (should be prevented)

---

## Troubleshooting (பிரச்சனை தீர்வு)

### Problem: Save செய்யும் போது unique constraint error

**Solution 1:** Cleanup script run செய்யவும்
```sql
-- Run: EmployeeHistory_CleanupDuplicates.sql
```

**Solution 2:** Manual check செய்யவும்
```sql
-- Check for duplicate active records
SELECT EMP_ID, EMP_CODE, COUNT(*) 
FROM EmployeeHistory 
WHERE Emp_EndDate IS NULL 
GROUP BY EMP_ID, EMP_CODE 
HAVING COUNT(*) > 1;
```

### Problem: JoinDate NULL-ஆ இருக்கு

**Solution:** Migration run செய்யவும்
```sql
-- Run: Employee_JoinDate_Migration.sql
```

### Problem: Multiple active records create ஆகுது

**Solution:** Latest code pull பண்ணி rebuild செய்யவும்
- Updated EmployeeRepository.cs இருக்கா check செய்யவும்
- UpdateEmployeeHistoryRow method உள்ளதா verify செய்யவும்

### Problem: Regular update போது new record create ஆகுது

**Solution:** IsBranchChanged flag correct-ஆ set ஆகுதா check செய்யவும்
- Branch மாறாம update செய்யும் போது IsBranchChanged = false இருக்கணும்

---

## Support

Questions இருந்தால் documentation file-ஐ பார்க்கவும்:
- `EmployeeHistory_JoinDate_Documentation.md` - Complete English documentation
- SQL queries மற்றும் examples அதில் உள்ளது

---

## Change Log

| Date | Change | Details |
|------|--------|---------|
| 2026-09-02 | Initial implementation | JoinDate field, Emp_StartDate/Emp_EndDate tracking |
| 2026-09-02 | Unique error fix | Added UpdateEmployeeHistoryRow method, enhanced duplicate prevention |

---

**Implementation Date:** 2026-09-02  
**Version:** 1.1 (Unique Error Fix)
**Status:** ✅ Ready for Testing
