# Reports Analysis — obms-reports-india

## Technology & Framework

| Item | Detail |
|---|---|
| **Reporting Engine** | SAP Crystal Reports for Visual Studio 2013 (Runtime 13.0.4000.0) |
| **NuGet Package** | `CrystalDecisions.*` assemblies, all Version `13.0.4000.0`, PublicKeyToken `692fbea5521e1304` |
| **Report File Format** | `.rpt` (Crystal Reports native binary format) |
| **Host Technology** | ASP.NET Web Forms (Website project, no `.csproj` — uses `website.publishproj.xml`) |
| **Target Framework** | .NET Framework 4.8 |
| **Viewer Component** | `CrystalDecisions.Web.CrystalReportViewer` server control (`<CR:CrystalReportViewer>`) |
| **Image Handler** | `CrystalImageHandler.aspx` registered as HTTP handler for chart/image rendering |
| **Viewer JS** | `~/crystalreportviewers13/js/crviewer/crv.js` (local copy of CR viewer scripts) |
| **Logging** | `log4net` 2.0.12.0 |
| **Authentication** | Windows Authentication (`<authentication mode="Windows"/>`) |
| **Custom Errors** | Off (debug mode) |
| **Source Control** | Git — a merge conflict marker is present in `Web.config` (HEAD vs commit `3bb5196`) |

---

## Configuration (Web.config summary, credentials masked)

### appSettings Keys

| Key | Value / Note |
|---|---|
| `CompanyCode` | `FWG` |
| `CompanyName` | `FREIGHTWATCH G PATOMOTOH SECURITY SERVICES. SDN.BHD.` |
| `Registration` | `(199501027914)` |
| `Address1–State, Phone` | Company address fields passed as Crystal Reports parameters at render time |
| `Server` | **[REDACTED]** — local dev: `DESKTOP-FHKDAA6`; UAT (commented): `192.168.0.216,54422` |
| `Database` | **[REDACTED]** — local dev: `OBMSINDIANEW`; UAT: `obmsuat` |
| `UserID` | **[REDACTED]** |
| `Password` | **[REDACTED]** |
| `aspnet:MaxHttpCollectionKeys` | `4000` (large form collections for payroll) |
| `CompanyEPF` | `015117931` |
| `CompanyPIC / CompanyPICContact` | Contact person details |
| `CIMB_*` / `BSN_*` | Malaysian bank payment file generation keys |
| `SocsoCompanyCode` | `D4100019020Z` |
| `GSTStart6/End6`, `GSTStart0/End0`, `SSTStart6/End6`, `SSTStart8/End8` | Malaysian GST/SST date ranges for tax-period logic |

### Web.Release.config (Release transform)
- Overrides `Server`, `Database`, `UserID`, `Password` to point to `SOFTWARESVR` / `obmsuat` / **[REDACTED]** via `xdt:Transform="SetAttributes"`.
- `SourceWebVirtualPath` in `website.publishproj.xml` is `/CrystalReportsWebSite`.

### Publish Profiles (App_Data/PublishProfiles)
Six profiles (`FolderProfile` through `FolderProfile6`) — all `FileSystem` publish method targeting local/desktop folders on various developers' machines. No IIS-direct publish config detected.

### Notable Issues
- **Unresolved Git merge conflict** in `Web.config` around the `UserID` key (`<<<<<<< HEAD` / `>>>>>>>` markers remain in the file — will cause a parse error at startup).
- `aspnet:MaxHttpCollectionKeys` is duplicated twice in `Web.config`.

---

## Report Files Inventory

### Root Level (1 report)
| File | Description |
|---|---|
| `CrystalReport1.rpt` | Test/placeholder report (shown on `Default.aspx`) |
| `ResignedEmployeeReport.rpt` | Resigned employee listing |

### Accounting (5 reports)
| File | Paired ASPX |
|---|---|
| `BalanceSheet.rpt` | `BalanceSheetReport.aspx` |
| `BankReconciliation.rpt` | `BankReconciliationReport.aspx` |
| `GeneralLedger.rpt` | `GeneralLedgerReport.aspx` |
| `ProfitAndLost.rpt` | `ProfitAndLostReport.aspx` |
| `TrialBalance.rpt` | `TrialBalanceReport.aspx` |

### Finance (76 reports including versioned backups)
Key active reports (trimmed list — many date-versioned copies exist):

| File | Description |
|---|---|
| `TaxInvoiceReport.rpt` | Main tax invoice (uses `ClientInvoice`, `ClientInvoiceDetails`, `ClientMaster`) |
| `TaxInvoiceReportComputerGenerated.rpt` | Computer-generated variant |
| `InvoiceDetailsReport.rpt` | Invoice detail printout |
| `InvoiceCollectionStatus.rpt` / `InvoiceCollectionTotalStatus.rpt` | Collection dashboards |
| `MonthlyInvoiceStatus.rpt` (×4 variants) | Monthly invoice summaries |
| `PaymentVoucher.rpt` / `ReceiptVoucher.rpt` | Payment/receipt vouchers |
| `PaymentVoucherSummary.rpt` / `ReceiptVoucherSummary.rpt` | Summary variants |
| `CreditNote.rpt` / `DebitNote.rpt` | Credit/debit note documents |
| `ClientStatement.rpt` / `SupplierStatement.rpt` | Statement of accounts |
| `InvoiceAging.rpt` / `InvoiceAgingAllBranches.rpt` | Ageing analysis |
| `GSTReport.rpt` | GST (Malaysia) report |
| `BranchCollectionsSummary.rpt` / `BranchPaymentSummary.rpt` | Branch-level summaries |
| `SalesInvoiceCollection.rpt` / `SalesInvoiceCollectionSummary.rpt` | Sales collection reports |
| `ProfitLossSummary.rpt` | P&L summary |
| `TimeSheet.rpt` | Finance timesheet |
| `PaySlip.rpt` (Finance) | Finance-side pay slip |
| `EmployeeMonthlyLoanVoucher.rpt` | Loan voucher |
| `ChequeStatus.rpt` | Cheque tracking |
| `DeletedInvoiceStatus.rpt` | Audit trail for deleted invoices |
| `PaymentCategoryReport.rpt` | Payment category breakdown |
| `PaymentDueList.rpt` | Outstanding payments |
| `BranchTransactionsMgrSummary.rpt` / `BranchTransactionsSummary.rpt` | Branch transaction summaries |

> Many versioned backups exist (e.g. `TaxInvoiceReport_20150428.rpt`, `InvoiceDetailsReport_20180610.rpt` etc.) — these are historical snapshots, not active reports.

### Inventory (2 reports)
| File | Paired ASPX |
|---|---|
| `BranchStockLedger.rpt` | `BranchStockLedgerReport.aspx` |
| `HQStockLedger.rpt` | `HQStockLedger.aspx` |

### Master (3 reports)
| File | Paired ASPX |
|---|---|
| `BranchReport.rpt` | `BranchReport.aspx` |
| `ClientReport.rpt` | `ClientMasterReport.aspx` |
| `EmployeeReport.rpt` | `EmployeeList.aspx` |

### Payroll (70+ reports including versioned backups)
Key active reports:

| File | Description |
|---|---|
| `PaySlip.rpt` / `PaySlipBM.rpt` | Pay slip (English / Bahasa Malaysia variants) |
| `PaySlipNew.rpt`, `PaySlipII.rpt`, `PaySlipIII.rpt`, `PaySlipRBA.rpt` | Pay slip variants |
| `PaySlipGuardPart1.rpt` / `PaySlipGuardPart2.rpt` | Guard-specific pay slips |
| `PaySheet.rpt` / `PaySheet2.rpt` / `PaySheet3.rpt` / `PaySheet_4.rpt` | Pay sheet (salary summary) variants |
| `PaySheetGuardPart1.rpt` / `PaySheetGuardPart2.rpt` | Guard-specific pay sheets |
| `PaySheetNoEPF.rpt` / `PaySheetNoEPF1.rpt` | Pay sheets excluding EPF |
| `YPaySheet.rpt` / `YPaySheet_2.rpt` / `YPaySheet_3.rpt` | Yearly pay sheets |
| `EPFStatement.rpt` | EPF (Malaysia) contribution statement |
| `SOCSOStatement.rpt` | SOCSO contribution statement |
| `IncomeTaxForm.rpt` | Income tax (Form EA equivalent) |
| `BankSalaryStatement.rpt` (+ II, Guard variants) | Bank salary upload statements |
| `SalaryStatement.rpt` / `SalaryStatementII.rpt` / `SalaryStatementWithoutAttendance.rpt` | Salary statements |
| `SalarySummary.rpt` | Salary summary |
| `LeaveForm.rpt` / `LeaveRegister.rpt` / `LeaveRegisterDetail.rpt` | Leave management |
| `TimeSheet.rpt` / `TimeSheetII.rpt` / `TimeSheetIII.rpt` | Timesheets |
| `LoanLedger.rpt` / `LoanTotalOutStandingLedger.rpt` | Loan tracking |
| `EmployeeLoanVoucher.rpt` / `EmployeeAdvanceVoucher.rpt` / `EmployeeMaterialIssueVoucher.rpt` | Vouchers |
| `AdvanceSummary.rpt` / `DailyAdvanceReport.rpt` / `MonthlyAdvanceReport.rpt` | Advance reports |
| `BankAdvanceList.rpt` / `BankMonthlyAdvanceReport.rpt` / `CashMonthlyAdvanceReport.rpt` / `ChequeMonthlyAdvanceReport.rpt` | Advance disbursement by payment mode |
| `CheckRoleReport.rpt` / `BranchCheckRoleReport.rpt` | Role assignment verification |
| `MiscTransStatement.rpt` | Miscellaneous transactions |
| `PayHistory.rpt` | Employee pay history |
| `InvoiceAging.rpt` (Payroll) | Invoice ageing from payroll context |
| `EmployeeReport.rpt` (Payroll) | Payroll employee listing |

> Multiple date-versioned backups exist (e.g. `PaySlip_20180206.rpt`, `BankSalaryStatement_20180304.rpt`).

**Total active `.rpt` files (excluding obvious backups): ~130**  
**Total `.rpt` files including versioned backups: ~170**

---

## Report Access Pattern

### Architecture Overview
```
Angular UI (obms-ui)
    │
    │  Opens in <iframe> via SafeResourceUrl
    │  HTTP GET with query string parameters
    ▼
baseReportUrl  (e.g. http://localhost:8080/ or http://124.217.236.37:5333/)
    │
    │  IIS / IIS Express hosting obms-reports-india
    ▼
ASP.NET Web Forms (.aspx pages)
    │
    │  Page_Load: applies DB login, sets report parameters,
    │  sets record selection formula
    ▼
Crystal Reports Viewer (CrystalReportViewer server control)
    │  renders .rpt file embedded in browser
    ▼
SQL Server (direct OLEDB/ODBC connection from report server)
```

### URL Pattern
Reports are accessed as direct HTTP GET requests to ASPX pages with query string parameters:

```
{baseReportUrl}{Module}/{ReportPage}.aspx?LoginID={user}&Branch={branch}&[module-specific params]
```

**Examples:**
```
http://localhost:8080/Payroll/PaySlipReport.aspx?LoginID=admin&Branch=KL&Period=2024-06&Employee=0&Lang=E&EmployeeType=Guard
http://localhost:8080/Finance/TaxInvoiceReport.aspx?LoginID=admin&ID=1001,1002
http://localhost:8080/Finance/TaxInvoiceReportComputerGenerated.aspx?LoginID=admin&ID=1001
http://localhost:8080/Accounting/BalanceSheetReport.aspx?LoginID=admin&...
http://localhost:8080/Compliance/FormXVIIReport.aspx?LoginID=admin&Branch=KL&Client=ABC&Period=2024-06&...
```

### Common Query String Parameters
| Parameter | Used In | Purpose |
|---|---|---|
| `LoginID` | All reports | Logged-in username (passed to Crystal parameter) |
| `Branch` | Most reports | Branch code filter |
| `Period` | Payroll/Finance | Date period (YYYY-MM or YYYY-MM-DD) |
| `Employee` | Pay slip, salary | Employee code (0 = all) |
| `EmployeeType` | Payroll | Guard / Admin / etc. |
| `Lang` | Pay slip | `M` = Bahasa Malaysia, else English |
| `ID` | Finance (invoices) | Invoice ID(s), comma-separated |
| `Client` | Finance/Compliance | Client code filter |

### Module Folders → URL Segments
| Folder | URL Segment | Number of ASPX Pages |
|---|---|---|
| `Accounting/` | `Accounting/` | 5 |
| `Finance/` | `Finance/` | 51 |
| `Inventory/` | `Inventory/` | 2 |
| `Master/` | `Master/` | 3 |
| `Payroll/` | `Payroll/` | 47 |
| Root | (none) | 2 (`Default.aspx`, `EmployeeResignedList.aspx`) |

> **Note:** A `Compliance/` module is referenced in the Angular UI (`Compliance/FormXVIIReport.aspx`, etc.) but those ASPX pages are **not present** in the current `obms-reports-india` source tree. They are either in a separate deployment directory or are yet to be created.

### Angular UI Integration
- The Angular app stores `baseReportUrl` in `environment.ts` / `environment.prod.ts`.
- Report components set `url = environment.baseReportUrl + 'Module/Page.aspx?' + queryParams` then bind it to `<iframe [src]="urlSafe">` via `DomSanitizer.bypassSecurityTrustResourceUrl()`.
- The Crystal Reports viewer renders fully server-side inside the iframe; no client-side Crystal SDK is used.
- Newer Indian-compliance reports (`Compliance/Form*.aspx`) use a hybrid model: the Angular component fetches data from `obms-api` via `MastermoduleService.getComplianceReportData()`, renders HTML from local asset templates, and displays inline — no Crystal Reports involved.

### Configured Environments
| Environment | API URL | Report URL |
|---|---|---|
| Local dev | `http://localhost:5000/api/` | `http://localhost:8080/` |
| Remote dev | `http://124.217.236.37:5000/api/` | `http://124.217.236.37:5222/` |
| Production (active) | `http://124.217.236.37:64400/api/` | `http://124.217.236.37:5333/` |
| UAT | `http://175.139.216.185:8183/api/` | `http://175.139.216.185:8184/` |

---

## Database Connectivity

### Connection Method
Crystal Reports connects **directly to SQL Server** at render time. There is no connection pooling or ORM layer in the reports project. Each ASPX page's `Page_Load` applies login credentials to every table in the report (and all sub-reports) via `TableLogOnInfo`:

```csharp
TableLogOnInfo logon = new TableLogOnInfo();
logon.ConnectionInfo.ServerName   = ConfigurationManager.AppSettings["Server"];    // [REDACTED]
logon.ConnectionInfo.DatabaseName = ConfigurationManager.AppSettings["Database"];  // [REDACTED]
logon.ConnectionInfo.UserID       = ConfigurationManager.AppSettings["UserID"];    // [REDACTED]
logon.ConnectionInfo.Password     = ConfigurationManager.AppSettings["Password"];  // [REDACTED]
foreach (Table table in rpt.Database.Tables) { table.ApplyLogOnInfo(logon); }
// Sub-reports also iterated separately
foreach (ReportDocument sub in rpt.Subreports) { ApplyLogin(sub); }
```

### Key Points
- Connection credentials are stored in `<appSettings>` in plain text in `Web.config` (not encrypted).
- No `<connectionStrings>` section is used — all connection info flows through `appSettings`.
- The reports connect to the **same SQL Server database** as `obms-api` (same `Server` + `Database` keys).
- SQL Server authentication is used (UserID/Password), not Windows Integrated Security.
- The `Web.Release.config` transform swaps credentials to the UAT server (`SOFTWARESVR`/`obmsuat`) on publish.

### Shared Data Sources
There are **no SSRS shared data source (`.rds`) files** — each report embeds its own data source definition (table/view references declared in the `.aspx` `<CR:DataSourceRef>` elements). Database credentials are injected programmatically at runtime from `appSettings`.

**Known database objects referenced across reports:**
- `ClientInvoice`, `ClientInvoiceDetails`, `ClientMaster`
- `BranchMaster`, `Employee`, `EmployeeSalaryDetails`, `EmploymentDetails`, `Payslip`
- `vwPaySheet` (view — used in record selection formula for pay slip filtering)
- Various finance/accounting tables inferred from report names

---

## Files Inspected

| File | Purpose |
|---|---|
| `Web.config` | Primary configuration — framework, Crystal Reports assemblies, appSettings, DB credentials (masked) |
| `Web.Release.config` | Release transform — overrides DB connection to UAT server |
| `website.publishproj.xml` | MSBuild website publish project — target framework v4.8, virtual path `/CrystalReportsWebSite` |
| `App_Data/PublishProfiles/FolderProfile5.pubxml` | Representative publish profile (FileSystem to local desktop) |
| `Default.aspx` | Root landing page with Crystal Reports Viewer (test report) |
| `Payroll/PaySlipReport.aspx` | Pay slip viewer page — Crystal Reports Viewer binding, DataSource table references |
| `Payroll/PaySlipReport.aspx.cs` | Pay slip code-behind — full DB login injection, parameter injection, record selection formula |
| `Finance/TaxInvoiceReport.aspx` | Tax invoice viewer page |
| `Finance/TaxInvoiceReport.aspx.cs` | Tax invoice code-behind — DB login, parameter injection, multi-invoice selection formula |
| `obms-ui/src/environments/environment.ts` | Angular dev environment — `baseReportUrl` = `http://localhost:8080/` |
| `obms-ui/src/environments/environment.prod.ts` | Angular prod environment — `baseReportUrl` = `http://124.217.236.37:5333/` |
| `obms-ui/src/app/modules/report/payroll/client-compliance-report/client-compliance-report.component.ts` | Angular compliance report component — shows hybrid model (API data + HTML templates + legacy Crystal URLs) |
| `obms-ui/src/app/modules/report/payroll/pay-sheet/pay-sheet.component.ts` | Angular pay sheet component — iframe-based Crystal Reports call |
| `obms-ui/src/app/modules/report/payroll/payslip-report/payslip-report.component.ts` | Angular pay slip component — iframe-based Crystal Reports call |
