# Frontend Analysis — obms-ui

## Framework & Dependencies

**Angular Version:** 13.3.x  
**Angular CLI:** 13.3.6  
**TypeScript:** 4.6.2  
**Node Memory Budget:** 4096 MB (configured in start/build scripts)  
**Location Strategy:** HashLocationStrategy (URLs use `#/` fragment)  

### Key Runtime Dependencies

| Package | Version | Purpose |
|---|---|---|
| @angular/core | ~13.3.0 | Framework core |
| @angular/material | ^13.3.9 | UI component library |
| @angular/cdk | ^13.3.9 | Component Dev Kit |
| @angular/forms | ~13.3.0 | Template & reactive forms |
| @angular/router | ~13.3.0 | SPA routing |
| bootstrap | ^4.0.0 | CSS grid & layout |
| font-awesome | ^4.7.0 | Icon set |
| animate.css | ^4.1.1 | CSS animations |
| apexcharts / ng-apexcharts | ^3.41.0 / ^1.7.0 | Charts |
| chart.js / ng2-charts | ^3.8.0 / ^3.0.11 | Chart rendering |
| jwt-decode | ^4.0.0 | JWT token parsing |
| crypto-js | ^4.2.0 | SHA-256 password hashing |
| sweetalert2 | ^11.7.32 | Modal/toast alerts |
| rxjs | ~7.5.0 | Reactive programming |
| exceljs | ^4.4.0 | Excel export |
| xlsx | ^0.18.5 | Excel read/write |
| jspdf + jspdf-autotable | ^4.2.1 / ^5.0.7 | PDF generation |
| html2canvas | ^1.4.1 | Screenshot to canvas |
| jquery | ^3.5.1 | DOM manipulation (legacy) |

### Dev Dependencies (build-time)

| Package | Version | Purpose |
|---|---|---|
| @boldreports/angular-reporting-components | ^5.3.9 | Bold Reports viewer (SSRS-style reports) |
| @boldreports/types | ^5.3.9 | Bold Reports type definitions |
| @angular-devkit/build-angular | ~13.3.6 | Build tooling |
| karma + jasmine | 6.3 / 4.0 | Unit testing |

---

## Project Structure

```
obms-ui/
├── src/
│   ├── app/
│   │   ├── app.component.ts              # Root component
│   │   ├── app.module.ts                 # Root module
│   │   ├── app-routing.module.ts         # Top-level router
│   │   │
│   │   ├── layout/
│   │   │   ├── admin-layout/             # Authenticated shell layout
│   │   │   │   ├── admin-layout.component.ts
│   │   │   │   ├── admin-layout.module.ts
│   │   │   │   └── admin-layout-routing.module.ts  # ALL protected routes
│   │   │   └── auth-layout/              # Unauthenticated shell layout
│   │   │       ├── auth-layout.component.ts
│   │   │       ├── auth-layout.module.ts
│   │   │       └── auth-layout-routing.module.ts   # login/register/changepassword
│   │   │
│   │   ├── components/                   # Shared UI components
│   │   │   ├── alert/
│   │   │   ├── dialog-confirmation/
│   │   │   ├── footer/
│   │   │   ├── header/
│   │   │   ├── message/
│   │   │   ├── sidebar/
│   │   │   ├── spinner/
│   │   │   └── components.module.ts
│   │   │
│   │   ├── model/                        # TypeScript interfaces/models (35+ files)
│   │   │
│   │   ├── modules/
│   │   │   ├── Accounting/               # GL Data List
│   │   │   ├── administration/           # Users, Banks, Cheques, Access Rights
│   │   │   ├── dashboard/                # Dashboard component
│   │   │   ├── finance/                  # Invoices, Payments, Receipts, etc.
│   │   │   ├── inventory/                # Assets, Items, Suppliers, Bills
│   │   │   ├── master/                   # Branch, Client, Employee, Slabs
│   │   │   ├── payroll/                  # Attendance, Salary, Advances, Loans
│   │   │   ├── quotation-and-agreement/  # Quotations & Agreements
│   │   │   ├── report/                   # All report screens by category
│   │   │   │   ├── accounting/
│   │   │   │   ├── finance/
│   │   │   │   ├── inventory/
│   │   │   │   ├── master/
│   │   │   │   └── payroll/
│   │   │   ├── shared.module.ts
│   │   │   └── user/                     # Login, Registration, Password Change
│   │   │
│   │   ├── service/                      # HTTP services, guards, interceptors
│   │   │
│   │   └── shared/
│   │       ├── pipes/currency.pipe.ts
│   │       ├── shared.module.ts
│   │       └── validators/
│   │           ├── custom-validators.ts
│   │           └── indian-validators.ts
│   │
│   ├── environments/
│   │   ├── environment.ts                # Dev config (localhost:5000)
│   │   └── environment.prod.ts           # Prod config (124.217.236.37:64889)
│   │
│   └── assets/
│
├── angular.json
├── package.json
└── tsconfig.json
```

---

## Routing Table (All Routes)

### Auth Routes (no guard — `AuthLayoutComponent`)

| Path | Component |
|---|---|
| `/login` | `LoginComponent` |
| `/register` | `RegistrationComponent` |
| `/changepassword` | `PasswordChangeComponent` |

### Protected Routes (all guarded by `AuthGuard` — `AdminLayoutComponent`)

#### Dashboard
| Path | Component |
|---|---|
| `/dashboard` | `DashboardComponent` |

#### Master Module
| Path | Component |
|---|---|
| `/master/branch-master` | `BranchMasterComponent` |
| `/master/branch-master/new-branch` | `NewBranchComponent` |
| `/master/client-master` | `ClientMasterComponent` |
| `/master/client-master/new-client` | `NewClientMasterComponent` |
| `/master/employee-master` | `EmployeeMasterComponent` |
| `/master/employee-master/new-employee` | `NewEmployeeComponent` |
| `/master/employee-master/edit-employee/:EMP_ID` | `NewEmployeeComponent` |
| `/master/employee-transfer` | `EmployeeTransferComponent` |
| `/master/employee-history` | `EmployeeHistoryComponent` |
| `/master/shift-timings` | `ShiftTimingsComponent` |
| `/master/shift-timings/new-shift-timing` | `NewShiftTimingComponent` |
| `/master/salary-slab` | `SalarySlabComponent` |
| `/master/salary-slab/new-salary-slab` | `NewSalarySlabComponent` |
| `/master/leave-slab` | `LeaveSlabComponent` |
| `/master/income-tax-slab` | `IncomeTaxSlabComponent` |
| `/master/income-tax-slab/new-income-tax-slab` | `NewIncomeTaxSlabComponent` |
| `/master/epf-slab` | `EpfSlabComponent` |
| `/master/epf-slab/new-epf-slab` | `NewEpfSlabComponent` |
| `/master/sip-slab` | `SipSlabComponent` |
| `/master/sip-slab/new-sip-slab` | `NewSipSlabComponent` |
| `/master/socso-slab` | `SocsoSlabComponent` |
| `/master/socso-slab/new-socso-slab` | `NewSocsoSlabComponent` |
| `/master/pf-slab` | `PfSlabComponent` (India) |
| `/master/pf-slab/new-pf-slab` | `NewPfSlabComponent` |
| `/master/esi-slab` | `EsiSlabComponent` (India) |
| `/master/esi-slab/new-esi-slab` | `NewEsiSlabComponent` |
| `/master/esi-slab/edit-esi-slab/:id` | `NewEsiSlabComponent` |
| `/master/tds-slab` | `TdsSlabComponent` (India) |
| `/master/gst-slab` | `GstSlabComponent` (India) |
| `/master/gst-slab/add` | `NewGstSlabComponent` |
| `/master/gst-slab/edit/:id` | `NewGstSlabComponent` |
| `/master/professional-tax-slab` | `ProfessionalTaxSlabComponent` (India) |
| `/master/service-type` | `ServiceTypeComponent` |
| `/master/service-type/new` | `NewServiceTypeComponent` |
| `/master/service-type/edit/:id` | `NewServiceTypeComponent` |
| `/master/service-type/details/:id` | `ViewServiceTypeComponent` |
| `/master/department-master` | `DepartmentMasterComponent` |
| `/master/designation-master` | `DesignationMasterComponent` |

#### Quotation & Agreement Module
| Path | Component |
|---|---|
| `/quotation-and-agreement/quotations` | `QuotationsComponent` |
| `/quotation-and-agreement/quotations/new-quotation` | `NewQuotationComponent` |
| `/quotation-and-agreement/quotations/edit-quotation/:ID` | `NewQuotationComponent` |
| `/quotation-and-agreement/agreements` | `AgreementsComponent` |
| `/quotation-and-agreement/agreements/new-agreement` | `NewAgreementComponent` |
| `/quotation-and-agreement/agreements/edit-agreement/:ID` | `NewAgreementComponent` |
| `/quotation-and-agreement/agreements-discount-report` | `AgreementsDiscountReportComponent` |
| `/quotation-and-agreement/agreement-termination` | `AgreementTerminationComponent` |
| `/quotation-and-agreement/agreement-termination/new-agreement-termination` | `NewAgreementTerminationComponent` |
| `/quotation-and-agreement/agreement-termination/edit-agreement-termination/:ID` | `NewAgreementTerminationComponent` |

#### Inventory Module
| Path | Component |
|---|---|
| `/inventory/asset-master` | `AssetMasterComponent` |
| `/inventory/asset-master/new-asset` | `NewAssetComponent` |
| `/inventory/asset-master/new-asset/:ID` | `NewAssetComponent` |
| `/inventory/category-master` | `CategoryMasterComponent` |
| `/inventory/category-master/new-category` | `NewCatergoryComponent` |
| `/inventory/category-master/new-category/:ID` | `NewCatergoryComponent` |
| `/inventory/item-master` | `ItemMasterComponent` |
| `/inventory/item-master/new-item` | `NewItemComponent` |
| `/inventory/item-master/new-item/:ID` | `NewItemComponent` |
| `/inventory/supplier-master` | `SupplierMasterComponent` |
| `/inventory/supplier-master/new-supplier` | `NewSupplierComponent` |
| `/inventory/supplier-master/new-supplier/:ID` | `NewSupplierComponent` |
| `/inventory/recepient-master` | `RecepientMasterComponent` |
| `/inventory/recepient-master/new-recepient` | `NewRecepientComponent` |
| `/inventory/recepient-master/new-recepient/:ID` | `NewRecepientComponent` |
| `/inventory/utility-bills` | `UtilityBillsComponent` |
| `/inventory/purchase-bills` | `PurchaseBillsComponent` |
| `/inventory/material-issue` | `MaterialIssueComponent` |
| `/finance/utility-search` | `SearchUtilityBillsComponent` |

#### Payroll Module
| Path | Component |
|---|---|
| `/payroll/employee-monthly-advance` | `EmployeemonthlyadvanceComponent` |
| `/payroll/new-employee-monthly-advance` | `NewEmployeeMonthlyAdvanceComponent` |
| `/payroll/new-employee-daily-advance` | `NewEmployeeDailyAdvanceComponent` |
| `/payroll/daily-advance-voucher-report` | `DailyAdvanceVoucherReportComponent` |
| `/payroll/new-employee-loan` | `NewEmployeeLoanComponent` |
| `/payroll/loan-voucher-report` | `LoanVoucherReportComponent` |
| `/payroll/new-employee-uniform-loan` | `NewEmployeeUniformLoanComponent` |
| `/payroll/uniform-loan-report` | `UniformLoanReportComponent` |
| `/payroll/voucher-filter-report` | `VoucherFilterReportComponent` |
| `/payroll/misc-transactions` | `MiscTransctionsComponent` |
| `/payroll/new-misc-transactions` | `NewMiscTransactionsComponent` |
| `/payroll/new-attendance` | `NewAttendanceComponent` |
| `/payroll/bulk-attendance-upload` | `BulkAttendanceUploadComponent` |
| `/payroll/attendance-display` | `AttendanceDisplayComponent` |
| `/payroll/salary-processing` | `SalaryProcessingComponent` |
| `/payroll/report/rbi-bank-salary-process` | `RbiBankSalaryProcessComponent` |
| `/payroll/report/rbi-bank-advance-salary-process` | `RbiBankAdvanceSalaryProcessComponent` |

#### Administration Module
| Path | Component |
|---|---|
| `/administration/user-registration` | `UserRegistrationComponent` |
| `/administration/bank-list` | `BankListComponent` |
| `/administration/new-bank-list` | `NewBankListComponent` |
| `/administration/bank-master` | `BankMasterComponent` |
| `/administration/new-bank-master` | `NewBankMasterComponent` |
| `/administration/cheque-master` | `ChequeMasterComponent` |
| `/administration/new-cheque-master` | `NewChequeMasterComponent` |
| `/administration/user-access-rights` | `UserAccessRightsComponent` |
| `/administration/new-user-access-rights` | `NewUserAccessRightsComponent` |
| `/administration/user-branch-access` | `UserBranchAccessComponent` |
| `/administration/new-user-branch-access` | `NewUserBranchAccessComponent` |
| `/administration/user-bank-access` | `UserBankAccessComponent` |
| `/administration/new-user-bank-access` | `NewUserBankAccessComponent` |

#### Finance Module
| Path | Component |
|---|---|
| `/finance/invoice` | `InvoiceComponent` |
| `/finance/batch-invoice` | `BatchInvoiceComponent` |
| `/finance/invoice-recycle-bin` | `InvoiceRecycleBinComponent` |
| `/finance/credit-note` | `CreditNoteComponent` |
| `/finance/debit-note` | `DebitNoteComponent` |
| `/finance/payments` | `PaymentsComponent` |
| `/finance/search-payments` | `SearchPaymentsComponent` |
| `/finance/category-search` | `CategorySearchComponent` |
| `/finance/payments-recycle-bin` | `PaymentsRecycleBinComponent` |
| `/finance/receipts` | `ReceiptsComponent` |
| `/finance/receipts/new-receipt` | `NewReceiptComponent` |
| `/finance/receipts/search-receipt` | `SearchReceiptsComponent` |
| `/finance/cheque-status` | `ChequeStatusComponent` |
| `/finance/cheque-master` | `FinanceChequeMasterComponent` |
| `/finance/legal-demand-action` | `LegalDemandActionComponent` |
| `/finance/legal-demand-action/new-legal-demand-action` | `NewLegalDemandActionComponent` |
| `/finance/legal-demand-action/new-legal-demand-action/:ID` | `NewLegalDemandActionComponent` |
| `/finance/invoice/print-invoice` | `PrintInvoiceComponent` |

#### Accounting Module
| Path | Component |
|---|---|
| `/accounting/acc-gl-data-list` | `AccGlDataListComponent` |

#### Master Reports
| Path | Component |
|---|---|
| `/report/:ReportName` | `ReportComponent` (generic Bold Reports viewer) |
| `/master/report/branch-report` | `BranchReportComponent` |
| `/master/report/employee-list` | `EmployeeListComponent` |
| `/master/report/client-master-report` | `ClientMasterReportComponent` |
| `/master/report/kkdn` | `KKDNComponent` |

#### Inventory Reports
| Path | Component |
|---|---|
| `/inventory/report/hq-stock-ledger` | `HQStockLedgerComponent` |
| `/inventory/report/branch-stock-ledger` | `BranchStockLedgerComponent` |

#### Payroll Reports
| Path | Component |
|---|---|
| `/report/payroll/monthly-advance-report` | `MonthlyAdvanceReportComponent` |
| `/report/payroll/pay-sheet-report` | `PaySheetComponent` |
| `/report/payroll/yearly-pay-sheet-report` | `YearlyPaySheetComponent` |
| `/report/payroll/pay-history-report` | `PayHistoryComponent` |
| `/report/payroll/payslip-report` | `PayslipReportComponent` |
| `/report/payroll/payslip2-report` | `Payslip2ReportComponent` |
| `/report/payroll/payslip3-report` | `Payslip3ReportComponent` |
| `/report/payroll/income-tax-report` | `IncomeTaxFormComponent` |
| `/report/payroll/leave-register-report` | `LeaveRegisterReportComponent` |
| `/report/payroll/leave-application-report` | `LeaveApplicationFormComponent` |
| `/report/payroll/netpay-report` | `NetpayReportsComponent` |
| `/report/payroll/misc-transaction-report` | `MiscTransactionReportComponent` |
| `/report/payroll/check-role-report` | `CheckroleReportComponent` |
| `/report/payroll/loan-ledger-report` | `LoanLedgerReportComponent` |
| `/report/payroll/epf-statement-report` | `EpfStatementReportComponent` |
| `/report/payroll/socso-statement-report` | `SocsoStatementReportComponent` |
| `/report/payroll/sip-statement-report` | `SipStatementReportComponent` |
| `/report/payroll/bank-salary-report` | `BankSalaryStatementReportComponent` |
| `/report/payroll/bank-salary2-report` | `BankSalaryStatement2ReportComponent` |
| `/report/payroll/list-summary-report` | `ListSummaryReportComponent` |
| `/report/payroll/client-compliance-report` | `ClientComplianceReportComponent` |
| `/report/payroll/esi-statement-report` | `EsiStatementReportComponent` (India) |
| `/report/payroll/professional-tax-statement-report` | `ProfessionalTaxStatementReportComponent` (India) |
| `/report/payroll/non-compliance-report` | `NonComplianceReportComponent` |
| `/report/payroll/pf-statement-report` | `PfStatementReportComponent` (India) |
| `/report/compliance/client-compliance-report` | `ClientComplianceReportComponent` (duplicate alias) |

#### Finance Reports
| Path | Component |
|---|---|
| `/report/finance/invoice-report` | `InvoiceReportComponent` |
| `/report/finance/print-indian-invoice` | `PrintIndianInvoiceComponent` |
| `/report/finance/payment-voucher-summary-report` | `PaymentVoucherSummaryComponent` |
| `/report/finance/receipt-voucher-summary-report` | `ReceiptVoucherSummaryComponent` |
| `/report/finance/branch-transactions-report` | `BranchTransactionsComponent` |
| `/report/finance/branch-collections-report` | `BranchCollectionsComponent` |
| `/report/finance/branch-payment-summary-report` | `BranchPaymentSummaryComponent` |
| `/report/finance/cheque-status-report` | `ChequeStatusReportComponent` |
| `/report/finance/credit-note-summary-report` | `CreditNoteSummaryComponent` |
| `/report/finance/credit-note-report` | `CreditNoteReportComponent` |
| `/report/finance/debit-note-report` | `DebitNoteReportComponent` |
| `/report/finance/profit-and-loss-report` | `ProfitAndLossComponent` |
| `/report/finance/separated-profit-loss-report` | `SeparatedProfitLossComponent` |
| `/report/finance/invoice-collection-status-report` | `InvoiceCollectionStatusReportComponent` |
| `/report/finance/monthly-invoice-status` | `MonthlyInvoiceStatusComponent` |
| `/report/finance/deleted-invoice-details` | `DeletedInvoiceDetailsComponent` |
| `/report/finance/sales-invoice-collection-report` | `SalesInvoiceCollectionReportComponent` |
| `/report/finance/sales-invoice-report` | `SalesInvoiceReportComponent` |
| `/report/finance/client-statement` | `ClientStatementComponent` |
| `/report/finance/supplier-statement` | `SupplierStatementComponent` |
| `/report/finance/invoice-ageing` | `InvoiceAgeingComponent` |
| `/report/finance/payment-due-list` | `PaymentDueListComponent` |
| `/report/finance/print-invoice-computer-generated` | `PrintInvoiceComputerGeneratedComponent` |
| `/report/finance/print-voucher-report` | `PrintVoucherReportComponent` |
| `/report/finance/receipt-voucher-report` | `ReceiptVoucherReportComponent` |
| `/report/finance/tds-report` | `TdsReportComponent` (India) |
| `/report/finance/gst-statement-report` | `GstStatementReportComponent` (India) |

#### Accounting Reports
| Path | Component |
|---|---|
| `/report/accounting/balance-sheet` | `BalanceSheetReportComponent` |
| `/report/accounting/trail-balance` | `TrailBalanceReportComponent` |
| `/report/accounting/accounting-profit-and-loss` | `AccountingProfitAndLossComponent` |
| `/report/accounting/profit-and-loss-without-contra` | `ProfitAndLossWithoutContraComponent` |
| `/report/accounting/pl-year-report` | `PlYearReportComponent` |
| `/report/accounting/bank-reconciliation` | `BankReconciliationReportComponent` |
| `/report/accounting/general-ledger` | `GeneralLedgerReportComponent` |

### Wildcards
- `''` → redirects to `login`
- `'**'` → redirects to `login`

---

## Modules & Components

### Root Module (`AppModule`)
- Imports: `BrowserModule`, `AppRoutingModule`, `BrowserAnimationsModule`, `HttpClientModule`, `FormsModule`, `ReactiveFormsModule`, `SharedModule`
- Providers: `CommonService`, `MastermoduleService`, `PayrollModuleService`, `HashLocationStrategy`, `AuthInterceptor` (HTTP_INTERCEPTORS)

### Layout Modules
- **AdminLayoutModule** — authenticated shell; declares all feature components via `admin-layout-routing.module.ts`
- **AuthLayoutModule** — unauthenticated shell; declares `LoginComponent`, `RegistrationComponent`, `PasswordChangeComponent`

### Feature Modules (under `modules/`)
| Module | Key Components |
|---|---|
| `dashboard` | `DashboardComponent` — live stats, report iframe embed |
| `user` | `LoginComponent`, `RegistrationComponent`, `PasswordChangeComponent` |
| `master` | Branch, Client, Employee, Transfer, Shift, Slabs (EPF, SIP, SOCSO, Salary, Income Tax, Leave, PF, ESI, TDS, GST, PT), Service Type, Department, Designation |
| `quotation-and-agreement` | Quotations, Agreements, Agreement Termination, Discount Report |
| `inventory` | Asset, Category, Item, Supplier, Recipient, Purchase Bills, Material Issue, Utility Bills |
| `payroll` | Attendance (new/bulk/display), Monthly Advance, Daily Advance, Loan, Uniform Loan, Misc Transactions, Salary Processing, RBI Bank Export |
| `finance` | Invoice (new/edit/print/batch/recycle), Payments (new/search/category/recycle), Receipts (new/search/print), Cheque Status, Cheque Master, Legal Demand, Credit Note, Debit Note |
| `administration` | User Registration, Bank List, Bank Master, Cheque Master, User Access Rights, User Branch Access, User Bank Access |
| `Accounting` | GL Data List |
| `report` | ~50 report components across master/payroll/finance/inventory/accounting sub-folders |

### Shared Components (`components/`)
- `AlertComponent` — message alerts
- `DialogConfirmationComponent` — reusable confirmation dialog (Angular Material)
- `FooterComponent` — page footer
- `HeaderComponent` — top navigation bar
- `MessageComponent` — toast/inline messages
- `SidebarComponent` — collapsible left nav
- `SpinnerComponent` — loading overlay

---

## HTTP Services & API Mappings

All services read `environment.baseUrl` (dev: `http://localhost:5000/api/`, prod: `http://124.217.236.37:64889/api/`).

### `CommonService` → Register controller (`Register/`)
| Method | HTTP | Endpoint |
|---|---|---|
| `addUser` | POST | `Register/UserRegistration` |
| `loginUser` | POST | `Register/LoginUser` |
| `changePassword` | POST | `Register/changepassword` |
| `getUsers` | GET | `register/GetObmsusers` |
| `getUserById` | GET | `register/GetObmsuserById` |
| `getUserByName` | GET | `register/GetObmsuserByName` |
| `deleteUser` | DELETE | `register/DeleteObmsuser` |
| `getBankList` | GET | `register/GetBankList` |
| `getUserBankList` | GET | `register/GetUserBankList` |
| `GetBankListById` | GET | `register/GetBankListById` |
| `saveAndUpdateBankDetails` | POST | `Register/saveAndUpdateBankDetails` |
| `deleteBank` | DELETE | `register/DeleteBank` |
| `getBankMasterList` | GET | `register/GetBankMasterList` |
| `getBankMasterById` | GET | `register/GetBankMasterById` |
| `saveAndUpdateBankMasterDetails` | POST | `Register/saveAndUpdateBankMasterDetails` |
| `deleteBankMaster` | DELETE | `register/DeleteBankMaster` |
| `getBankPrefixNoByBankID` | GET | `register/GetBankPrefixNoByBankID` |
| `getChequeList` | GET | `register/GetChequeList` |
| `getChequeListById` | GET | `register/GetChequeListById` |
| `saveAndUpdateChequeMasterDetails` | POST | `Register/saveAndUpdateChequeMasterDetails` |
| `DeleteChequeMaster` | DELETE | `register/DeleteChequeMaster` |
| `getScreensByCategory` | GET | `register/GetScreensByCategory` |
| `addScreen` | POST | `register/AddScreen` |
| `getPermissionsWithScreens` | GET | `register/GetPermissionsWithScreens` |
| `saveAndUpdateObmsPermissions` | POST | `register/SaveAndUpdateObmsPermissions` |
| `getObmsBranchesPermission` | GET | `register/GetObmsBranchesPermission` |
| `getObmsBranchesPermissionByUser` | GET | `register/GetObmsBranchesPermissionByUser` |
| `SaveAndUpdateObmsBranches` | POST | `register/SaveAndUpdateObmsBranches` |
| `getObmsBanksPermission` | GET | `register/GetObmsBanksPermission` |
| `saveAndUpdateObmsBanks` | POST | `register/SaveAndUpdateObmsBanks` |

### `MastermoduleService` → Master, Statutory, Finance, Dashboard controllers
| Method | HTTP | Endpoint |
|---|---|---|
| `getBranchMaster` | GET | `master/GetBranchMaster` |
| `getBranchMasterList` | GET | `master/GetBranchMasterAll` |
| `GetBranchListByUserName` | GET | `master/GetBranchListByUserName` |
| `GetBankListByUserName` | GET | `Register/GetBankListByUserId` |
| `saveAndUpdateBranchMaster` | POST | `master/saveAndUpdateBranchMaster` |
| `deleteBranchMasterByCode` | DELETE | `master/DeleteBranchMasterByCode` |
| `getBranchesByState` | POST | `master/branches-by-state` |
| `getAllBranchStates` | GET | `master/all-branch-states` |
| `getClienthMaster` | GET | `master/GetClientMsterList` |
| `getClientMsterListByStatus` | GET | `master/GetClientMsterListByStatus` |
| `getClientMsterListByBranch` | GET | `master/GetClientMsterListByBranch` |
| `getClientMasterListByComplianceStatus` | GET | `master/GetClientMasterListByComplianceStatus` |
| `saveAndUpdateClientMaster` | POST | `master/SaveAndUpdateClientMaster` |
| `deleteClientMasterByCode` | DELETE | `master/DeleteClientMasterByCode` |
| `getShifthMasterList` | GET | `master/GetShiftMsterList` |
| `saveAndUpdateShiftMaster` | POST | `master/SaveAndUpdateShiftMaster` |
| `getSIPhMaster` | GET | `master/GetSIPMaster` |
| `saveAndUpdateSIPMaster` | POST | `master/SaveAndUpdateSIPMaster` |
| `getEPFMaster` | GET | `master/GetEPFMaster` |
| `saveAndUpdateEPFMaster` | POST | `master/SaveAndUpdateEPFMaster` |
| `getIncomeTaxMaster` | GET | `master/GetIncomeTaxMaster` |
| `saveAndUpdateIncomeTaxMaster` | POST | `master/saveAndUpdateIncomeTaxMaster` |
| `getLeaveMaster` | GET | `master/GetLeaveMaster` |
| `saveAndUpdateLeaveMaster` | POST | `master/saveAndUpdateLeaveMaster` |
| `getSOCSOMaster` | GET | `master/GetSOCSOMaster` |
| `saveAndUpdateSOCSOMaster` | POST | `master/SaveAndUpdateSOCSOMaster` |
| `getSalaryMaster` | GET | `master/GetSalaryMaster` |
| `saveAndUpdateSalaryMaster` | POST | `master/saveAndUpdateSalaryMaster` |
| `getComplianceReportData` | POST | `ComplianceReport/GetComplianceReportData` |
| `getUserAccessRights` | GET | `register/GetUserAccessRights` |
| `getKKDNList` | GET | `master/GetKKDNList` |
| `getSuppliers` | GET | `Finance/GetSuppliers` |
| `getInventoryCategories` | GET | `Finance/GetInventoryCategories` |
| `getMonthlyInvoices` | GET | `Finance/monthly-invoices` |
| `getMonthlyInvoiceList` | GET | `Finance/GetMonthlyInvoiseList` |
| **Indian Statutory** | | |
| `getPFConfigurationList` | GET | `statutory/pf-configuration` |
| `saveAndUpdatePFConfiguration` | POST | `statutory/saveAndUpdatePFConfiguration` |
| `getESIConfigurationList` | GET | `statutory/esi-configuration` |
| `saveESIConfiguration` | POST | `statutory/esi-configuration` |
| `updateESIConfiguration` | PUT | `statutory/esi-configuration/:id` |
| `deleteESIConfigurationById` | DELETE | `statutory/esi-configuration/:id` |
| `getPTConfigurationList` | GET | `statutory/pt-configuration` |
| `saveAndUpdatePTConfiguration` | POST | `statutory/saveAndUpdatePTConfiguration` |
| `getTDSConfigurationList` | GET | `statutory/tds-configuration` |
| `saveAndUpdateTDSSConfiguration` | POST | `statutory/saveAndUpdateTDSSConfiguration` |
| `getGSTConfigurationList` | GET | `statutory/gst-configuration` |
| `saveAndUpdateGSTConfiguration` | POST | `statutory/saveAndUpdateGSTConfiguration` |
| `calculateGST` | POST | `statutory/calculate-gst` |
| `getServiceCategoryList` | GET | `master/service-categories` |
| `getStateMasterList` | GET | `master/state-master` |
| `getBillingCycleList` | GET | `master/billing-cycles` |
| `getInvoiceFormatList` | GET | `master/invoice-formats` |
| **Dashboard** | | |
| `getServerStatus` | GET | `dashboard/server-status` |
| `getAllEmployees` | GET | `Employee/Employees` |
| `getAgreementsPostedThisMonth` | GET | `Agreement/GetAgreementsPostedThisMonth` |
| `getYearlyProfitLoss` | GET | `Finance/GetYearlyProfitLoss` |

### `EmployeeService` → Employee controller
| Method | HTTP | Endpoint |
|---|---|---|
| `getEmployees` | GET | `Employee/Employees` |
| `getEmployeeMaster` | GET | `Employee/GetEmployeeMaster` |
| `getEmployeeById` | GET | `Employee/EmployeeById` |
| `getClientsFromBranchId` | GET | `Employee/GetClientsFromBranchId` |
| `getEmployeesByBranchId` | GET | `Employee/EmployeesByBranchId` |
| `saveEmployee` | POST | `Employee/SaveAndUpdateEmployee` |
| `updateEmployee` | PUT | `Employee/SaveAndUpdateEmployee` |
| `checkEmployeeInfo` | GET | `Employee/CheckEmployeeInfo` |
| `searchEmployees` | POST | `Employee/SearchEmployees` |
| `validateEmployeeField` | POST | `Employee/ValidateEmployeeField` |
| `getEmployeeByPAN` | GET | `Employee/GetEmployeeByPAN/:pan` |
| `getEmployeeByAadhaar` | GET | `Employee/GetEmployeeByAadhaar/:aadhaar` |
| `deleteEmployee` | DELETE | `Employee/DeleteEmployee/:id` |
| `previewExcelImport` | POST | `Employee/PreviewExcelImport` |
| `bulkImportEmployees` | POST | `Employee/BulkImportEmployees` |
| `downloadImportTemplate` | GET | `Employee/DownloadImportTemplate` |

### `PayrollModuleService` → Payroll controller
| Method | HTTP | Endpoint |
|---|---|---|
| `getEmployeeList` | GET | `payroll/GetEmployeeList` |
| `getEmployeeListByBranchCode` | GET | `payroll/GetEmployeeListByBranchCode` |
| `saveAndUpdateSalaryMonthlyAdvance` | POST | `payroll/SaveAndUpdateSalaryMonthlyAdvance` |
| `saveAndUpdateSalaryDailyAdvances` | POST | `payroll/SaveAndUpdateSalaryDailyAdvances` |
| `getSalaryAdvanceById` | GET | `payroll/GetSalaryAdvanceById` |
| `getEmployeeById` | GET | `payroll/GetEmployeeById` |
| `getPaySheetData` | GET | `payroll/GetPaySheetData` |
| `getMiscTransList` | GET | `payroll/GetMiscTrans` |
| `saveAndUpdateMiscTrans` | POST | `payroll/SaveAndUpdateMiscTrans` |
| `getDailyAdvanceList` | GET | `payroll/GetDailyAdvanceList` |
| `getClients` | GET | `payroll/GetClients` |
| `getClientsByBranchID` | GET | `payroll/GetClientsByBranchID` |
| `getEmployeeDetails` | GET | `payroll/GetEmployeeDetails` |
| `saveAndUpdateAttendance` | POST | `payroll/SaveAndUpdateAttendance` |
| `deleteAttendance` | DELETE | `payroll/DeleteAttendance` |
| `getAttendancePeriod` | GET | `payroll/GetAttendancePeriod` |
| `getClientAttendancePeriodConfig` | GET | `client/GetAttendancePeriodConfig` |
| `saveClientAttendancePeriodConfig` | POST | `client/SaveAttendancePeriodConfig` |
| `getList` | GET | `payroll/GetList` |
| `salaryProcess` | GET | `payroll/Process` |
| `getListWithBlankRow` | GET | `payroll/WithBlankRow` |
| `getListWithBlankRowWithBranch` | GET | `payroll/WithBlankRowByBranch` |
| `getEPFToExcel` | GET | `payroll/GetEPFToExcel` |
| `getRbiBankSalaryExport` | GET | `payroll/GetRbiBankSalaryExport` |
| `getRbiBankSalaryExportCsv` | GET | `payroll/GetRbiBankSalaryExportCsv` (Blob) |
| `getRbiBankSalaryExportTxt` | GET | `payroll/GetRbiBankSalaryExportTxt` (Blob) |
| `getRbiBankAdvanceExport` | GET | `payroll/GetRbiBankAdvanceExport` |
| `getSOCSOToExcel` | GET | `payroll/GetEmployeeSocsoList` |
| `getSIPToExcel` | GET | `payroll/GetEmployeeSIPList` |
| `getEPFToCIMBList` | GET | `payroll/GetEPFToCIMBList` |
| `getPayslipData` | GET | `payroll/GetPayslipData` |
| `getPayslip2Data` | GET | `payroll/GetPayslipData` (variant) |
| `getEmployeeAttendanceList` | GET | `payroll/GetEmployeeAttendanceList` |
| `getIsSalaryProcessDoneForCurrentPeriod` | GET | `payroll/IsSalaryProcessDoneForCurrentPeriod` |
| `getIsTemporaryEmployee` | GET | `payroll/IsTemporaryEmployee` |

### `FinanceService` → Finance, CreditDebitNote, PFReport, ESIWageReport controllers
| Method | HTTP | Endpoint |
|---|---|---|
| `getInvoiceMaster` | GET | `Finance/GetInvoiceMaster` |
| `getClients` | GET | `Finance/GetInvoiceClientByBranchAndInvoiceDate` |
| `getBatchInvoiceClients` | GET | `Finance/GetBatchInvoiceClientByBranchAndInvoiceDate` |
| `getAgreement` | GET | `Finance/GetAgreementAndDetailsByBranchInvoicePeriodAndClient` |
| `getClientInvoiceById` | GET | `Finance/GetInvoiceAndDetailsByInvoiceId` |
| `saveInvoice` | POST | `Finance/SaveAndUpdateInvoice` |
| `saveBatchInvoice` | POST | `Finance/SaveAndUpdateBatchInvoice` |
| `deleteInvoice` | GET | `Finance/DeleteInvoiceById` |
| `RestoreInvoices` | POST | `Finance/RestoreInvoices` |
| `GetPaymentMaster` | GET | `Finance/GetPaymentMaster` |
| `saveAndUpdatePayment` | POST | `Finance/SaveAndUpdatePayment` |
| `deletePayment` | POST | `Finance/delete` |
| `GetReceiptMaster` | GET | `Finance/GetReceiptMaster` |
| `SaveAndUpdateReceipt` | POST | `Finance/SaveAndUpdateReceipt` |
| `deleteReceipt` | POST | `Finance/receipt/delete` |
| `getChequeStatuses` | GET | `Finance/cheque-status` |
| `restoreChequeStatus` | POST | `Finance/restore-chequeStatus` |
| `getLegalDemandList` | GET | `Finance/GetLegalDemandList` |
| `saveOrUpdateLegalDemand` | POST | `Finance/saveOrUpdateLegalDemand` |
| `deleteLegalDemand` | DELETE | `Finance/DeleteLegalDemand/:id` |
| `getPFStatement` | POST | `PFReport/GetPFStatement` |
| `getESIWageReport` | POST | `ESIWageReport/GetESIWageReport` |
| `getTDSReport` | GET | `Finance/GetTDSReport` |
| `generateCreditNoteNo` | GET | `CreditDebitNote/GenerateCreditNoteNo` |
| `getCreditNotes` | GET | `CreditDebitNote/GetCreditNotes` |
| `saveCreditNote` | POST | `CreditDebitNote/SaveCreditNote` |
| `saveDebitNote` | POST | `CreditDebitNote/SaveDebitNote` |
| `generateClientStatement` | POST | `Finance/client-statement` |
| `executeSupplierReport` | POST | `Finance/supplier-report` |
| `getBranchPaymentList` | GET | `Finance/branch-payment-details` |
| `getCreditorInvoicePaymentList` | GET | `Finance/creditor-invoice-payment-details` |

### `InventoryService` → Inventory controller
| Method | HTTP | Endpoint |
|---|---|---|
| `saveCategory` / `getCategories` | POST/GET | `Inventory/SaveAndUpdateCategory` / `Inventory/GetInventoryCategories` |
| `saveItem` / `getItems` | POST/GET | `Inventory/SaveAndUpdateItem` / `Inventory/GetInventoryItems` |
| `saveAsset` / `getAssets` | POST/GET | `Inventory/SaveAndUpdateAssetMaster` / `Inventory/GetAssetMasters` |
| `saveSupplier` / `getSuppliers` | POST/GET | `Inventory/SaveAndUpdateSupplier` / `Inventory/GetSuppliers` |
| `saveRecipient` / `getRecipients` | POST/GET | `Inventory/SaveAndUpdateRecipient` / `Inventory/GetRecipients` |
| `getUtilityMaster` | GET | `Inventory/GetUtilityMasterList` |
| `saveUtility` | POST | `Inventory/SaveAndUpdateUtility` |
| `saveMaterial` | POST | `Inventory/SaveAndUpdateMaterial` |
| `GetMaterialMasterList` | GET | `Inventory/GetMaterialMasterList` |
| `getUtilitySearchList` | GET | `Inventory/GetUtilityBillSearchList` |
| `deleteItem` | POST | `Inventory/DeleteItem/:id` |
| `isCreditorInvoice` | GET | `Inventory/CreditorInvoice_CheckOnBranchPayments/:id` |

### `AccountingService` → Accounting controller
| Method | HTTP | Endpoint |
|---|---|---|
| `getList` | GET | `Accounting/GetList` |
| `getListWithType` | GET | `Accounting/GetListWithType` |
| `checkRecordExists` | GET | `Accounting/exists` |
| `addGLRecord` | POST | `Accounting/add` |
| `deleteGLRecord` | DELETE | `Accounting/delete` |

### `QuotationService` → Quotation controller
| Method | HTTP | Endpoint |
|---|---|---|
| `getClientsByBranchID` | GET | `Quotation/GetClientsQuotationByBranchID` |
| `saveAndUpdateQuotation` | POST | `Quotation/SaveAndUpdateQuotation` |
| `getQuotationByID` | GET | `Quotation/GetQuotationByID` |
| `getQuotations` | GET | `Quotation/GetQuotations` |
| `getQuotationsDiscountReport` | GET | `Quotation/GetQuotationsDiscountReport` |
| `deleteQuotationById` | DELETE | `Quotation/DeleteQuotationById` |

### `AgreementService` → Agreement controller
| Method | HTTP | Endpoint |
|---|---|---|
| `getAgreementMaster` | GET | `Agreement/GetAgreementMaster` |
| `getClientsByBranchID` | GET | `Agreement/GetClientsAgreementByBranchID` |
| `save` | POST | `Agreement/SaveAndUpdateAgreement` |
| `getAgreementById` | GET | `Agreement/GetAgreementByID` |
| `getAgreements` | GET | `Agreement/GetAgreements` |
| `SaveAndUpdateAgreementTermination` | POST | `Agreement/SaveAndUpdateAgreementTermination` |
| `DeleteAgreementById` | DELETE | `Agreement/DeleteAgreementById` |
| `CancelAgreement` | POST | `Agreement/CancelAgreement` |
| `checkDuplicateAgreement` | GET | `Agreement/CheckDuplicateAgreement` |

### `BankService` → Bank controller
| Method | HTTP | Endpoint |
|---|---|---|
| `getActiveBanks` | GET | `bank/active` |
| `getBankByCode` | GET | `bank/by-code/:bankCode` |
| `getBankByIFSC` | GET | `bank/by-ifsc/:ifscCode` |
| `validateIFSC` | GET | `bank/validate-ifsc/:ifscCode` |

### `IndianPayrollService` (standalone, hardcoded `localhost:5000/api/`)
- `calculatePayrollDeductions` → POST `indian-payroll/calculate-deductions`
- `generatePayslip` → GET `indian-payroll/generate-payslip/:id/:period`
- `processBulkPayroll` → POST `indian-payroll/bulk-process`
- `generateForm16` → GET `indian-payroll/form16/:id/:year` (Blob)
- `validateCompliance` → POST `indian-payroll/validate-compliance`

### Other Services (non-HTTP)
- `AuthService` — JWT token in `localStorage` (`auth_token`), 10-min inactivity auto-logout
- `DatasharingService` — BehaviorSubject for username, menu name, selected invoice sharing across components
- `SidebarService` — BehaviorSubject for sidebar submenu open/close state
- `IndianComplianceService` — mostly local validators; minimal HTTP calls to `indian-compliance/` controller
- `PdfExportService` — client-side PDF generation (jsPDF)

---

## Authentication & Authorization

### Login Flow
1. User submits credentials at `/login` via `LoginComponent`
2. Password is **SHA-256 hashed** (CryptoJS) client-side before sending
3. POST `Register/LoginUser` — server returns `{ Success, Failure, token, Users, Message }`
4. On success: JWT token stored in `localStorage` key `auth_token` via `AuthService.setToken()`
5. User info stored in `sessionStorage` keys: `username`, `userrole`, `currentUser` (JSON)
6. Router navigates to `/dashboard`

### Password Expiry Logic
- Passwords expire after 20 days from `LastUpdatedDate`
- If 1–7 days remain: warning dialog, user may skip or go to `/changepassword`
- If expired: user is forced to `/changepassword` before accessing the app

### Session Management
- `AuthService` listens to `mousemove`, `keydown`, `click`, `scroll` events
- Inactivity timeout: **10 minutes** (constant named `timeoutDuration = 10 * 60 * 1000`)
- On timeout: `AuthService.logout()` clears token and redirects to `/login`
- Manual logout sets `localStorage['manualLogout'] = 'true'` (no session-expired message on next login)

### HTTP Interceptor (`AuthInterceptor`)
- Attaches `Authorization: Bearer <token>` header to every outgoing request
- On `401` response: calls `AuthService.logout()` (forces re-login on expired/invalid token)
- Also resets the inactivity timer on each intercepted request

### Route Guard (`AuthGuard`)
- `canActivate()` checks `AuthService.isAuthenticated()` (truthy JWT in localStorage)
- Redirects to `/login` if not authenticated
- Applied to **all** routes in `admin-layout-routing.module.ts`
- No role-based guard is implemented at the route level; screen-level permission checks are done via `MastermoduleService.getUserAccessRights(userName, screenName)` returning `{ Read, Create, Update, Delete }` flags

### User Roles
Stored as `sessionStorage['userrole']`:
- `'1'` or `'2'` or `'true'` = Admin/Super Admin (checked in `DashboardComponent` for full access)
- `'superadmin'` username = bypasses permission checks
- Standard users: permission per screen fetched from `register/GetUserAccessRights`

---

## UI Libraries & Styling

| Library | Usage |
|---|---|
| **Angular Material 13** | Dialogs (`MatDialog`), datepickers, tables (`MatTable`), form fields, buttons, icons, tabs, stepper |
| **Bootstrap 4** | Grid system, utility classes, modal-less layout |
| **Font Awesome 4.7** | Icon set throughout navigation and buttons |
| **animate.css 4** | CSS entrance/exit animations |
| **SweetAlert2** | Success/error/warning toast and modal dialogs (replaces native `alert/confirm`) |
| **ApexCharts + ng-apexcharts** | Line, bar, area charts on dashboard |
| **Chart.js + ng2-charts** | Alternative charting library |
| **Bold Reports (SSRS-style)** | Embedded report viewer via `<iframe>` or the `@boldreports/angular-reporting-components` renderer; report server URL: `environment.baseReportUrl` |

### Global Styles (loaded in `angular.json`)
```
@angular/material/prebuilt-themes/indigo-pink.css   ← Material theme
bootstrap/dist/css/bootstrap.min.css
animate.css/animate.min.css
font-awesome/css/font-awesome.min.css
src/styles.css                                       ← Custom OBMS styles
@boldreports/javascript-reporting-controls/Content/material/bold.reports.all.min.css
```

---

## Key Screens per Module

### Dashboard
- System info card (company name, version, environment status)
- Live stats tiles: Total Guards, Total Staff, Active Clients, Pending Payments, Agreements This Month
- Finance iframe embed: `PaymentDueList.aspx` from Bold Reports server (shown only if user has read access)

### Master Module
- **Branch Master** — branch list/create/edit/delete with state filtering
- **Client Master** — client list with status/compliance status filtering, new client form with Indian fields (GSTIN, PAN, GST state)
- **Employee Master** — list with branch/type/status filter, Excel bulk import dialog, new/edit form with Indian compliance fields (Aadhaar, PAN, PF, ESI, TDS accounts)
- **Employee Transfer** — transfer employees between branches
- **Shift Timings** — shift schedule configuration
- **Salary Slabs** — dynamic salary structure builder
- **EPF / SIP / SOCSO Slabs** (Malaysia) — statutory contribution tables
- **PF / ESI / TDS / GST / Professional Tax Slabs** (India) — Indian statutory configuration
- **Service Type** — service category master
- **Department & Designation** — HR hierarchy masters

### Quotation & Agreement Module
- **Quotations** — client quotation list; new quotation with commercial breakdown dialog
- **Agreements** — service agreements converted from quotations; new/edit agreement
- **Agreement Termination** — record contract terminations with reason/date
- **Agreements Discount Report** — discount analysis across agreements

### Finance Module
- **Invoice** — generate invoices from agreements; edit/print/search; recycle bin
- **Batch Invoice** — bulk invoice generation for multiple clients
- **Credit Note / Debit Note** — adjustment notes against invoices
- **Payments** — record outgoing payments; search by category/date; recycle bin
- **Receipts** — record incoming payments from clients; search
- **Cheque Status** — update cheque clearance/return status
- **Cheque Master** — manage cheque books (Finance view)
- **Legal Demand Action** — track overdue client legal actions

### Inventory Module
- **Asset Master** — company asset register
- **Category Master** — inventory categories (Uniform, Equipment, etc.)
- **Item Master** — stock items catalog
- **Supplier Master** — vendor/supplier list with category
- **Recipient Master** — internal recipients for material issuance
- **Purchase Bills** — supplier invoice entry
- **Material Issue** — stock issue to branches/employees
- **Utility Bills** — electricity, water, and other utility invoices

### Payroll Module
- **Attendance** — enter, bulk upload (Excel), and display attendance by period/client
- **Employee Monthly Advance** — monthly salary advance entries
- **Employee Daily Advance** — daily advance entry with voucher report
- **Employee Loan** — loan disbursement and ledger report
- **Employee Uniform Loan** — uniform advance with report
- **Misc Transactions** — miscellaneous payroll adjustments
- **Salary Processing** — run/lock payroll for a branch/period/employee type
- **RBI Bank Salary/Advance Export** — generate CSV/TXT files in RBI NEFT format for SBI/HDFC/ICICI

### Administration Module
- **User Registration** — create/manage system users
- **Bank List** — map banks to users (access control)
- **Bank Master** — bank configuration
- **Cheque Master** — cheque book management
- **User Access Rights** — screen-level CRUD permission matrix per user
- **User Branch Access** — assign branch visibility per user
- **User Bank Access** — assign bank access per user

### Report Module (~50 reports)
- **Payroll**: Pay Sheet, Yearly Pay Sheet, Pay History, Payslip (3 formats), Income Tax, Leave Register, Loan Ledger, EPF/SOCSO/SIP/Bank Salary Statements, PF Statement, ESI Wage Statement, Professional Tax Statement, GST Statement, Misc Transaction Report, Compliance Reports
- **Finance**: Invoice Report, Sales Invoice Report, Payment/Receipt Voucher Summaries, Branch Collections/Transactions, Cheque Status, Credit/Debit Note Reports, P&L, Separated P&L, Client Statement, Supplier Statement, Invoice Ageing, TDS Report, Invoice Collection Status
- **Accounting**: Balance Sheet, Trial Balance, P&L (with/without contra), Year P&L, Bank Reconciliation, General Ledger
- **Master**: Employee List, Branch Report, Client Master Report, KKDN Report
- **Inventory**: HQ Stock Ledger, Branch Stock Ledger

---

## Files Inspected

| File | Purpose |
|---|---|
| `package.json` | Dependencies, scripts |
| `angular.json` | Build config, styles, scripts |
| `src/app/app-routing.module.ts` | Root routing |
| `src/app/app.module.ts` | Root module, providers |
| `src/app/layout/admin-layout/admin-layout-routing.module.ts` | All protected routes |
| `src/app/layout/auth-layout/auth-layout-routing.module.ts` | Auth routes |
| `src/app/service/auth.guard.ts` | Route guard |
| `src/app/service/auth.interceptor.ts` | JWT header + 401 handler |
| `src/app/service/auth.service.ts` | Token management, inactivity timer |
| `src/app/service/common.service.ts` | Register/Admin API calls |
| `src/app/service/mastermodule.service.ts` | Master/Statutory/Dashboard API calls |
| `src/app/service/payrollmodule.service.ts` | Payroll API calls |
| `src/app/service/employee.service.ts` | Employee CRUD API |
| `src/app/service/finance.service.ts` | Finance API calls |
| `src/app/service/inventory.service.ts` | Inventory API calls |
| `src/app/service/accounting.service.ts` | Accounting/GL API calls |
| `src/app/service/bank.service.ts` | Indian bank API calls |
| `src/app/service/indian-payroll.service.ts` | Indian payroll calculation service |
| `src/app/service/indian-compliance.service.ts` | Indian compliance validators |
| `src/app/service/datasharing.service.ts` | Cross-component state (BehaviorSubject) |
| `src/app/service/sidebar.service.ts` | Sidebar state |
| `src/app/modules/quotation-and-agreement/quotation.service.ts` | Quotation API calls |
| `src/app/modules/quotation-and-agreement/agreement.service.ts` | Agreement API calls |
| `src/app/modules/user/login/login.component.ts` | Login form, password hash, expiry check |
| `src/app/modules/dashboard/dashboard.component.ts` | Dashboard stats, iframe report |
| `src/environments/environment.ts` | Dev config (localhost:5000) |
| `src/environments/environment.prod.ts` | Prod config (124.217.236.37:64889) |
| Full recursive file listing of `src/app/` | Module/component structure |
