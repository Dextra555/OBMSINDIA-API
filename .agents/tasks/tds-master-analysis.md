# TDS Master – Architecture Investigation Report

**Date:** 2026-10-27  
**Scope:** Read-only analysis of `d:\India Live` (obms-api + obms-ui)  
**Purpose:** Understand existing patterns before implementing TDSMaster table and CRUD API

---

## 1. Summary Answer

**No `TDSMaster` table, model, or controller exists yet** – there are zero matches across the entire codebase for `TDSMaster`, `tds-master`, or `TdsMaster`.

However, the project already has **extensive TDS-related infrastructure** that must be understood before adding TDSMaster:

| Existing TDS artifact | What it is |
|---|---|
| `TDSSlabConfiguration` DB table | Income-tax bracket slabs (Min_Income / Max_Income / Tax_Rate) – used for **payroll TDS** (Section 192) |
| `TDSSlabConfiguration.cs` domain model | C# EF entity mapped to the table above |
| `TdsSlabConfiguration` in `OBMSDbContext` | Registered `DbSet` |
| `IndianStatutoryController` `/api/statutory/tds-configuration` | GET / POST / DELETE for payroll TDS slabs |
| `TdsSlabComponent` (UI) | List/add/edit screen at `/master/tds-slab` – already wired into routing and sidebar |
| `ClientMaster.TDSApplicable` + `ClientMaster.TDSPercentage` | Per-client TDS toggle (free-form %) for invoice receipts |
| `CreditorInvoice.TDSPercentage` + `TDSAmount` | Vendor-invoice TDS fields |

The requested **`TDSMaster`** is a **lookup / rate table** for vendor/contractor TDS categories (Section 194C, 194J, 194I, 194A, etc.). It is architecturally distinct from `TDSSlabConfiguration` (which is for employee income-tax slabs). They must coexist without collision.

---

## 2. Evidence

### 2.1 Database Conventions  
Source: `d:\India Live\obms-api\Database\`

All migrations use **defensive `IF NOT EXISTS` / `IF COL_LENGTH` guards**. Example from `Supplier_IsTax_GSTNo_PanNo_Migration.sql` and `ClientMaster_TDS_Migration.sql`:

```sql
IF COL_LENGTH('dbo.ClientMaster', 'TDSApplicable') IS NULL
BEGIN
    ALTER TABLE dbo.ClientMaster
    ADD TDSApplicable BIT NOT NULL CONSTRAINT DF_ClientMaster_TDSApplicable DEFAULT (0);
END;
GO
```

Column naming conventions observed:
- Identity PK → `ID` (uppercase, integer)  
- Status flag → `IsActive` (BIT, default 1)  
- Timestamps → `CreatedDate` / `LastUpdatedDate` (DATETIME)  
- Audit → `CreatedBy` / `LastUpdatedBy` (VARCHAR)
- Decimals → `DECIMAL(5,2)` for percentages, `DECIMAL(10,2)` / `DECIMAL(18,2)` for amounts

`TDSSlabConfiguration` uses `Is_Active` (underscore) because it was generated with column aliases — this is an older pattern. All newer tables (e.g. `ServiceType`, `ProfessionalTaxConfiguration`) use `IsActive` (PascalCase, no underscore). **New `TDSMaster` should follow the newer PascalCase convention.**

### 2.2 Domain Model Pattern  
Source: `d:\India Live\obms-api\Models\Domain\BranchMaster.cs`, `ServiceType.cs`, `TDSSlabConfiguration.cs`

Standard model structure:

```csharp
[Table("TDSMaster")]
public class TDSMaster
{
    [Key]
    public int ID { get; set; }           // INT, identity, PK — matches BranchMaster/ShiftTimeMaster
    
    [Required]
    [StringLength(100)]
    public string TDSName { get; set; }   // VARCHAR(100) NOT NULL
    
    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal TDSPercentage { get; set; }
    
    public bool IsActive { get; set; } = true;  // BIT default 1
}
```

`[Table("X")]` attribute is mandatory. `[Key]` marks the PK. `[Required]` + `[StringLength]` are used on strings. `[Column(TypeName = "decimal(5,2)")]` is used for precision types.

### 2.3 DbContext  
Source: `d:\India Live\obms-api\Models\OBMSDbContext.cs`

EF Core `OBMSDbContext` has a `DbSet<T>` entry for every table. The global `OnModelCreating` override sets `Precision(18)` / `Scale(8)` on all `decimal` properties by convention — but specific columns override this with `[Column(TypeName = "decimal(5,2)")]`.

New entity must be added:
```csharp
public DbSet<TDSMaster> TDSMasters { get; set; }
```

No EF migrations are used — the project uses **hand-written SQL migration scripts** in `Database\`. No `Migrations\` folder exists.

### 2.4 Repository Pattern  
Source: `d:\India Live\obms-api\Repositories\Interface\IMasterRepository.cs` + `Implementation\MasterRepository.cs`

Every master entity follows:
1. `IMasterRepository` declares the interface method signatures
2. `MasterRepository` implements them using `OBMSDbContext` (LINQ/EF)
3. No Dapper or raw SQL in `MasterRepository` — **pure EF Core LINQ**

For `IndianStatutoryController` (TDSSlabConfiguration), **raw ADO.NET `SqlConnection`/`SqlCommand`** is used instead of the repository pattern. This is an alternative used in that controller only.

**Recommendation:** Follow the `IMasterRepository` / `MasterRepository` EF approach for `TDSMaster`, as that is the dominant pattern in the codebase (BranchMaster, ClientMaster, ShiftMaster, EPFMaster, etc.).

### 2.5 Controller Pattern  
Source: `d:\India Live\obms-api\Controllers\MasterController.cs`

All master CRUD endpoints live in `MasterController` under `[Route("api/[controller]")]` → `api/master/...`.  
Standard endpoint signatures:
```csharp
[HttpGet]
[Route("GetTDSMasterList")]
public async Task<ActionResult<TDSMasterDto>> GetTDSMasterList()

[HttpPost]
[Route("SaveAndUpdateTDSMaster")]
public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateTDSMaster(TDSMasterRequestDto dto)

[HttpDelete("DeleteTDSMasterById")]
public async Task<IActionResult> DeleteTDSMasterById(int Id)
```

Return pattern for POST success: `Ok(new { Success = "Success", ... })`.  
Return pattern for POST failure: `StatusCode(500, new { Success = "Error", Message = ... })`.

### 2.6 DTO Pattern  
Source: `d:\India Live\obms-api\Models\DTO\ShiftTimeMasterDto.cs`, `BranchMasterRequestDto.cs`

DTOs are plain C# classes (no base class). Named `<Entity>RequestDto`. Properties mirror the domain model exactly for simple masters.

### 2.7 Program.cs / DI Registration  
Source: `d:\India Live\obms-api\Program.cs`

`IMasterRepository` → `MasterRepository` is already registered:
```csharp
builder.Services.AddScoped<IMasterRepository, MasterRepository>();
```
No additional DI changes needed for `TDSMaster` since it is added to the existing `MasterRepository`.

### 2.8 Routing  
- API: `[Route("api/[controller]")]` on `MasterController` → all TDSMaster endpoints will be at `api/master/...`
- UI: Routes registered in `admin-layout-routing.module.ts`; component declared in `shared.module.ts`
- Sidebar: `sidebar.component.html` at `d:\India Live\obms-ui\src\app\components\sidebar\sidebar.component.html`

### 2.9 Existing TDS in UI  
Source: `d:\India Live\obms-ui\src\app\modules\master\tds-slab\tds-slab.component.ts`

`TdsSlabComponent` exists for `TDSSlabConfiguration` (income tax slabs). It is declared in `shared.module.ts`, routed at `/master/tds-slab`, and already in the sidebar.

The new `TdsMasterComponent` will be a **separate component** at `/master/tds-master`. It must be:
1. Added to `shared.module.ts` declarations + exports  
2. Added to `admin-layout-routing.module.ts` imports + routes  
3. Added to `sidebar.component.html`  
4. Service methods added to `mastermodule.service.ts`  
5. TypeScript interface added to `indian-compliance.model.ts`

### 2.10 API Base URL  
- Dev (environment.ts): `http://localhost:5000/api/`  
- Prod (environment.prod.ts): `http://124.217.236.37:64400/api/`

### 2.11 TDS Rate Validation (Indian IT Act)  
Source: Requested validation per requirement #6

The 10 categories specified in the requirement map to the following sections of the Income Tax Act (FY 2024-25):

| Category | Section | Rate (no PAN) | Rate (with PAN) | Threshold |
|---|---|---|---|---|
| Contractor – Individual/HUF | 194C | 1% | 1% | ₹30,000/transaction or ₹1,00,000/year |
| Contractor – Other Entities | 194C | 2% | 2% | Same |
| Commission/Brokerage | 194H | 5% | 5% | ₹15,000/year |
| Rent – Plant & Machinery | 194I(b) | 2% | 2% | ₹2,40,000/year |
| Rent – Land & Building | 194I(a) | 10% | 10% | ₹2,40,000/year |
| Professional Fees | 194J | 10% | 10% | ₹30,000/year |
| Technical Services | 194J | 2% | 2% | ₹30,000/year |
| Interest (other than bank) | 194A | 10% | 10% | ₹40,000 (banks)/₹5,000 (others) |
| Dividend | 194 | 10% | 10% | ₹5,000/year |
| Lottery Winnings | 194B | 30% | 30% | ₹10,000/ticket |

**Key discrepancy:** The requirement specifies Commission/Brokerage at 2%, but the current statutory rate under Section 194H is **5%** (as of FY 2024-25). The implementer must NOT assume the rates in the requirements are production-correct. The SQL seed data should use the rates as specified by the user (which will be kept as a configurable lookup), with a comment noting they may need updating per applicable provisions.

**PAN-related rule:** If the payee does not furnish PAN, TDS is deducted at the **higher of**: (a) the applicable rate, or (b) 20% — per Section 206AA. This is a runtime calculation concern and does not affect the `TDSMaster` table structure.

---

## 3. Conclusions and Recommendations

### What to build

1. **SQL Migration** (`d:\India Live\obms-api\Database\TDSMaster_Migration.sql`)  
   - `CREATE TABLE TDSMaster` with `ID INT IDENTITY(1,1) PRIMARY KEY`, `TDSName VARCHAR(100) NOT NULL`, `TDSPercentage DECIMAL(5,2) NOT NULL`, `IsActive BIT NOT NULL DEFAULT 1`  
   - `IF OBJECT_ID` guard to prevent duplicate creation  
   - 10 seed `INSERT` rows using the rates from the user requirement (with code comment about Section 194H discrepancy on Commission)

2. **Domain model** (`d:\India Live\obms-api\Models\Domain\TDSMaster.cs`)  
   - `[Table("TDSMaster")]`, `[Key]` on `ID`, `[Required]` + `[StringLength(100)]` on `TDSName`, `[Column(TypeName = "decimal(5,2)")]` on `TDSPercentage`

3. **DTO** (`d:\India Live\obms-api\Models\DTO\TDSMasterRequestDto.cs`)  
   - Plain C# DTO mirroring the domain model

4. **DbContext** (`d:\India Live\obms-api\Models\OBMSDbContext.cs`)  
   - Add `public DbSet<TDSMaster> TDSMasters { get; set; }`

5. **Repository interface** (`d:\India Live\obms-api\Repositories\Interface\IMasterRepository.cs`)  
   - Add `GetTDSMasterList()`, `SaveAndUpdateTDSMaster(TDSMaster)`, `DeleteTDSMasterById(int)`

6. **Repository implementation** (`d:\India Live\obms-api\Repositories\Implementation\MasterRepository.cs`)  
   - EF LINQ implementation for the 3 methods above

7. **Controller** (`d:\India Live\obms-api\Controllers\MasterController.cs`)  
   - Add `GetTDSMasterList`, `SaveAndUpdateTDSMaster`, `DeleteTDSMasterById` endpoints in a new `#region TDS Master`

8. **UI TypeScript interface** (`d:\India Live\obms-ui\src\app\model\indian-compliance.model.ts`)  
   - Add `TDSMaster` interface with `id`, `tDSName`, `tDSPercentage`, `isActive`

9. **UI Service** (`d:\India Live\obms-ui\src\app\service\mastermodule.service.ts`)  
   - Add `getTDSMasterList()`, `saveAndUpdateTDSMaster()`, `deleteTDSMasterById()` calling `api/master/...`

10. **UI Component** (`d:\India Live\obms-ui\src\app\modules\master\tds-master\`)  
    - `tds-master.component.ts/html/css` — list view following `tds-slab.component.ts` pattern  
    - `new-tds-master/new-tds-master.component.ts/html/css` — add/edit form following `new-gst-slab` pattern

11. **Routing** (`d:\India Live\obms-ui\src\app\layout\admin-layout\admin-layout-routing.module.ts`)  
    - Add import + routes for `TdsMasterComponent` at `/master/tds-master` and `/master/tds-master/add`, `/master/tds-master/edit/:id`

12. **Module declaration** (`d:\India Live\obms-ui\src\app\modules\shared.module.ts`)  
    - Import and declare `TdsMasterComponent`, `NewTdsMasterComponent`; export both

13. **Sidebar** (`d:\India Live\obms-ui\src\app\components\sidebar\sidebar.component.html`)  
    - Add `<li class="nav-item" routerLink="/master/tds-master"><a class="nav-link">TDS Master</a></li>` after the existing TDS Slab entry

### What NOT to touch
- `TDSSlabConfiguration` table, model, controller, or UI — it is for payroll income-tax slabs and is entirely separate  
- `ClientMaster.TDSApplicable` / `ClientMaster.TDSPercentage` — client-level receipt TDS fields  
- `CreditorInvoice.TDSPercentage` / `TDSAmount` — vendor invoice TDS  
- Any GST, supplier, payroll, ESI, PF modules  
- The `IndianStatutoryController` — it owns `TDSSlabConfiguration`; `TDSMaster` belongs in `MasterController`

### Build / run commands
- API: `dotnet build` from `d:\India Live\obms-api\`; run with `dotnet run` on port 5000 (dev)  
- UI: `ng serve` from `d:\India Live\obms-ui\`; dev server at `http://localhost:4200`  
- DB: Execute migration SQL against `OBMSINDIANEW` database on `DESKTOP-FHKDAA6` (connection string in `appsettings.json`)  
- Test API: Swagger UI at `http://localhost:5000/swagger` (enabled unconditionally in `Program.cs`)

---

*Investigation complete. No code was modified.*
