# Employee Join Date and History Tracking

## Overview
This document explains how the employee join date is stored and how employee history is tracked when employees are created or transferred between branches.

---

## Database Schema Changes

### Employee Table
Added new column:
- **`JoinDate`** (DATETIME, nullable): Stores the original date when the employee joined the company

### EmployeeHistory Table
Already has these columns:
- **`Emp_StartDate`** (DATETIME, nullable): Start date at a specific branch
- **`Emp_EndDate`** (DATETIME, nullable): End date at a specific branch (NULL = currently active)
- **`JoinDate`** (DATETIME, nullable): Copy of the original join date from Employee table

---

## Data Flow

### 1. New Employee Creation

When a new employee is created:

```
Form Input:
- EMPPAY_DATE_JOINED (Join Date from form)

Storage:
1. Employee table:
   - JoinDate = EMPPAY_DATE_JOINED
   
2. EmploymentDetails table:
   - EMPPAY_DATE_JOINED = value from form
   
3. EmployeeHistory table (1st record):
   - Emp_StartDate = EMPPAY_DATE_JOINED (start at first branch)
   - Emp_EndDate = NULL (currently active)
   - JoinDate = EMPPAY_DATE_JOINED (for reference)
   - All other employee, employment, and salary details
```

**Example:**
```
Employee joins on 2024-01-15 at Branch A:

Employee:
  JoinDate = 2024-01-15

EmployeeHistory (Record 1):
  Emp_StartDate = 2024-01-15
  Emp_EndDate = NULL
  EMP_BRANCH_CODE = Branch A
  JoinDate = 2024-01-15
```

---

### 2. Employee Branch Transfer

When an employee is transferred to a new branch:

```
Form Input:
- BranchStartDate (Effective start date at new branch)
- IsBranchChanged = true

Process:
1. Find the currently active EmployeeHistory record (Emp_EndDate = NULL)
2. Set its Emp_EndDate = BranchStartDate - 1 day
3. Create a new EmployeeHistory record:
   - Emp_StartDate = BranchStartDate
   - Emp_EndDate = NULL (new active record)
   - EMP_BRANCH_CODE = new branch code
   - JoinDate = original join date from Employee table
   - All other current employee details
```

**Example:**
```
Employee transfers from Branch A to Branch B on 2024-06-01:

EmployeeHistory (Record 1 - UPDATED):
  Emp_StartDate = 2024-01-15
  Emp_EndDate = 2024-05-31       ← CLOSED
  EMP_BRANCH_CODE = Branch A
  JoinDate = 2024-01-15

EmployeeHistory (Record 2 - NEW):
  Emp_StartDate = 2024-06-01     ← NEW START DATE
  Emp_EndDate = NULL             ← CURRENTLY ACTIVE
  EMP_BRANCH_CODE = Branch B
  JoinDate = 2024-01-15          ← ORIGINAL JOIN DATE
```

---

### 3. Multiple Branch Transfers

An employee can have multiple EmployeeHistory records:

```
Transfer Timeline:
- 2024-01-15: Joins Branch A
- 2024-06-01: Transfers to Branch B
- 2024-12-01: Transfers to Branch C

EmployeeHistory Records:

Record 1:
  Emp_StartDate = 2024-01-15
  Emp_EndDate = 2024-05-31
  EMP_BRANCH_CODE = Branch A
  JoinDate = 2024-01-15

Record 2:
  Emp_StartDate = 2024-06-01
  Emp_EndDate = 2024-11-30
  EMP_BRANCH_CODE = Branch B
  JoinDate = 2024-01-15

Record 3:
  Emp_StartDate = 2024-12-01
  Emp_EndDate = NULL              ← CURRENTLY ACTIVE
  EMP_BRANCH_CODE = Branch C
  JoinDate = 2024-01-15
```

---

## Code Implementation

### Controller (EmployeeController.cs)

```csharp
// For new employees, store join date
if (employeeRequestDto.EMP_ID == 0)
{
    employee.JoinDate = employeeRequestDto.EMPPAY_DATE_JOINED ?? DateTime.Now;
}
else
{
    // For existing employees, preserve original JoinDate
    if (!employee.JoinDate.HasValue)
    {
        employee.JoinDate = employeeRequestDto.EMPPAY_DATE_JOINED ?? DateTime.Now;
    }
}

// Call repository with branch change parameters
await _employeeRepository.saveAndUpdateEmployee(
    employee, employment, salaryDetails,
    isBranchChanged: employeeRequestDto.IsBranchChanged,
    branchStartDate: employeeRequestDto.BranchStartDate);
```

### Repository (EmployeeRepository.cs)

```csharp
// New employee: Create first history record
if (isNewEmployee)
{
    DateTime startDate = employment.EMPPAY_DATE_JOINED.Date;
    await InsertEmployeeHistoryRow(employee, employment, salaryDetails,
        empStartDate: startDate,
        empEndDate: null);
}
// Branch change: Close old record and create new one
else if (isBranchChanged && branchStartDate.HasValue)
{
    DateTime newStartDate = branchStartDate.Value.Date;
    DateTime prevEndDate  = newStartDate.AddDays(-1);

    // Close previous active record
    var openHistory = await _oBMSDbContext.EmployeeHistories
        .Where(h => h.EMP_ID == employee.EMP_ID && h.Emp_EndDate == null)
        .OrderByDescending(h => h.EMP_HISTORY_ID)
        .FirstOrDefaultAsync();

    if (openHistory != null)
    {
        openHistory.Emp_EndDate = prevEndDate;
        _oBMSDbContext.EmployeeHistories.Update(openHistory);
        await _oBMSDbContext.SaveChangesAsync();
    }

    // Create new active record
    await InsertEmployeeHistoryRow(employee, employment, salaryDetails,
        empStartDate: newStartDate,
        empEndDate: null);
}
```

---

## SQL Queries

### Get Current Branch for an Employee
```sql
SELECT 
    eh.EMP_CODE,
    eh.EMP_NAME,
    eh.EMP_BRANCH_CODE AS CurrentBranch,
    eh.Emp_StartDate AS StartedAtBranchOn,
    e.JoinDate AS OriginalJoinDate
FROM EmployeeHistory eh
INNER JOIN Employee e ON eh.EMP_ID = e.EMP_ID
WHERE eh.EMP_ID = @EmployeeId
  AND eh.Emp_EndDate IS NULL;
```

### Get Complete Branch Transfer History
```sql
SELECT 
    eh.EMP_CODE,
    eh.EMP_NAME,
    eh.EMP_BRANCH_CODE AS Branch,
    eh.Emp_StartDate AS StartDate,
    ISNULL(eh.Emp_EndDate, GETDATE()) AS EndDate,
    DATEDIFF(DAY, eh.Emp_StartDate, ISNULL(eh.Emp_EndDate, GETDATE())) AS DaysAtBranch,
    CASE WHEN eh.Emp_EndDate IS NULL THEN 'Active' ELSE 'Closed' END AS Status,
    e.JoinDate AS OriginalJoinDate
FROM EmployeeHistory eh
INNER JOIN Employee e ON eh.EMP_ID = e.EMP_ID
WHERE eh.EMP_ID = @EmployeeId
ORDER BY eh.Emp_StartDate;
```

### Get Employees by Branch and Period
```sql
SELECT 
    eh.EMP_CODE,
    eh.EMP_NAME,
    eh.EMP_BRANCH_CODE,
    eh.Emp_StartDate,
    eh.Emp_EndDate,
    e.JoinDate
FROM EmployeeHistory eh
INNER JOIN Employee e ON eh.EMP_ID = e.EMP_ID
WHERE eh.EMP_BRANCH_CODE = @BranchCode
  AND eh.Emp_StartDate <= @PeriodEnd
  AND (eh.Emp_EndDate IS NULL OR eh.Emp_EndDate >= @PeriodStart)
ORDER BY eh.EMP_NAME;
```

---

## UI Form Requirements

### New Employee Form
Required fields:
- **Join Date** (EMPPAY_DATE_JOINED): The date employee joins the company
  - This will be stored in Employee.JoinDate
  - This will be used as Emp_StartDate in the first EmployeeHistory record

### Edit Employee Form (Branch Transfer)
Required fields when changing branch:
- **New Branch Code**: The new branch code
- **Effective Start Date** (BranchStartDate): Date employee starts at new branch
  - Cannot be before the previous branch's start date
  - Will close previous EmployeeHistory record with EndDate = StartDate - 1
  - Will create new EmployeeHistory record with StartDate = BranchStartDate

---

## Migration Notes

### Running the Migration
1. Execute: `Employee_JoinDate_Migration.sql`
2. This will:
   - Add JoinDate column to Employee table
   - Add JoinDate column to EmployeeHistory table
   - Populate JoinDate for existing employees from EmploymentDetails
   - Populate JoinDate for existing EmployeeHistory records

### Data Integrity Checks
After migration, verify:
```sql
-- Check employees without JoinDate
SELECT EMP_CODE, EMP_NAME 
FROM Employee 
WHERE JoinDate IS NULL;

-- Check history records without dates
SELECT EMP_CODE, EMP_NAME, EMP_HISTORY_ID
FROM EmployeeHistory
WHERE Emp_StartDate IS NULL;
```

---

## Benefits

1. **Original Join Date Preserved**: Employee.JoinDate always stores the original join date
2. **Complete Branch History**: EmployeeHistory tracks all branch transfers with date ranges
3. **Period-Based Queries**: Easy to query which employees were at which branch during a specific period
4. **Audit Trail**: Complete historical record of employee movements
5. **Reporting**: Can generate reports showing:
   - Employee tenure at company
   - Employee tenure at each branch
   - Branch transfer history
   - Employees active during specific periods

---

## Testing Scenarios

### Scenario 1: New Employee
1. Create employee with join date 2024-01-15
2. Verify Employee.JoinDate = 2024-01-15
3. Verify EmployeeHistory.Emp_StartDate = 2024-01-15
4. Verify EmployeeHistory.Emp_EndDate = NULL

### Scenario 2: Branch Transfer
1. Edit employee and change branch
2. Provide effective start date 2024-06-01
3. Verify old EmployeeHistory.Emp_EndDate = 2024-05-31
4. Verify new EmployeeHistory.Emp_StartDate = 2024-06-01
5. Verify new EmployeeHistory.Emp_EndDate = NULL
6. Verify Employee.JoinDate unchanged

### Scenario 3: Multiple Transfers
1. Transfer employee 3 times
2. Verify 3 EmployeeHistory records exist
3. Verify only last record has Emp_EndDate = NULL
4. Verify all records have same JoinDate
5. Verify no date gaps or overlaps

---

## Troubleshooting

### Issue: JoinDate is NULL
**Solution**: Run data migration to populate from EmploymentDetails

### Issue: Multiple active history records (Emp_EndDate = NULL)
**Solution**: Close old records and keep only the latest active

### Issue: Date gaps in history
**Solution**: Verify BranchStartDate calculations and adjust Emp_EndDate accordingly

---

## Change Log

| Date | Change | Author |
|------|--------|--------|
| 2026-09-02 | Initial implementation of JoinDate and Emp_StartDate/Emp_EndDate tracking | Development Team |
