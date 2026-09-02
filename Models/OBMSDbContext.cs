using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Controllers;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;

namespace OBMS.WebAPI.Models;

public partial class OBMSDbContext : DbContext
{
    public OBMSDbContext(DbContextOptions<OBMSDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<AdvanceRepayment> AdvanceRepayments { get; set; }
    public virtual DbSet<Obmsuser> Obmsusers { get; set; }
    public virtual DbSet<BranchMaster> BranchMasters { get; set; }
    public virtual DbSet<ClientMaster> ClientMasters { get; set; }
    public virtual DbSet<ShiftTimeMaster> ShiftTimeMasters { get; set; }
    public virtual DbSet<SIP> SIPs { get; set; }
    public virtual DbSet<IncomeTax> IncomeTaxs { get; set; }
    public virtual DbSet<EPF> EPFs { get; set; }
    public virtual DbSet<SOCSO> SOCSOs { get; set; }
    public virtual DbSet<LeaveSystem> LeaveSystems { get; set; }
    public virtual DbSet<BankList> BankLists { get; set; }
    public virtual DbSet<SalaryStructure> SalaryStructures { get; set; }
    public virtual DbSet<SalaryStructure> SalaryStructure { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<EmploymentDetails> EmploymentDetails { get; set; }
    public virtual DbSet<EmployeeSalaryDetails> EmployeeSalaryDetails { get; set; }
    public virtual DbSet<Agreement> Agreements { get; set; }
    public virtual DbSet<AgreementDetails> AgreementDetails { get; set; }
    public virtual DbSet<Quotation> Quotations { get; set; }
    public virtual DbSet<QuotationDetails> QuotationDetails { get; set; }
    public virtual DbSet<TerminatedAgreement> TerminatedAgreements { get; set; }
    public virtual DbSet<ClientInvoice> ClientInvoices { get; set; }
    public virtual DbSet<InvoiceDetails> InvoiceDetails { get; set; }
    public virtual DbSet<ClientInvoiceDetail> ClientInvoiceDetails { get; set; }
    public virtual DbSet<EmployeeHistory> EmployeeHistories { get; set; }
    public DbSet<EmployeeItemIssue> EmployeeItemIssues { get; set; }
    public virtual DbSet<SalaryAdvance> SalaryAdvances { get; set; }
    public DbSet<InventoryCategory> InventoryCategories { get; set; }
    public DbSet<ItemMaster> ItemMasters { get; set; }
    public DbSet<PaySlip> PaySlips { get; set; }
    public DbSet<MiscTrans> MiscTrans { get; set; }
    public DbSet<AssetMaster> AssetMasters { get; set; }
    public DbSet<Supplier> suppliers { get; set; }
    public virtual DbSet<Supplier> Suppliers { get; set; }
    public virtual DbSet<BankMaster> BankMasters { get; set; }
    public virtual DbSet<ChequeMaster> ChequeMasters { get; set; }
    public virtual DbSet<SalaryProcess> SalaryProcess { get; set; }
    public virtual DbSet<Attendance> Attendances { get; set; }
    public virtual DbSet<Department> Departments { get; set; }
    public virtual DbSet<Designation> Designations { get; set; }
    public virtual DbSet<AttendanceDetails> AttendanceDetails { get; set; }
    public virtual DbSet<Receipts> Receipts { get; set; }
    public virtual DbSet<OBMSPermissions> OBMSPermissions { get; set; }
    public virtual DbSet<ScreenList> ScreenLists { get; set; }
    public DbSet<Recipient> Recipients { get; set; }
    public virtual DbSet<OBMSBanks> OBMSBanks { get; set; }
    public virtual DbSet<OBMSBranches> OBMSBranches { get; set; }
    public DbSet<QueryResult> QueryResults { get; set; }
    public DbSet<CheckEmployeeInfoResult> CheckEmployeeInfoResult { get; set; }
    public DbSet<ClientInvoiceNoResult> ClientInvoiceNoResult { get; set; }
    public DbSet<LeaveClassResults> LeaveClassResults { get; set; }
    public DbSet<CreditorInvoice> CreditorInvoices { get; set; }
    public DbSet<CreditorInvoiceDetails> CreditorInvoiceDetails { get; set; }
    public DbSet<ClientMasterResult> ClientMasterResults { get; set; }
    public DbSet<LastprocessedDates> LastprocessedDates { get; set; }
    public DbSet<StockIssues> StockIssues { get; set; }
    public DbSet<StockIssueDetail> StockIssueDetails { get; set; }    
    public DbSet<BranchPayment> BranchPayments { get; set; }
    public DbSet<BranchPaymentDetails> BranchPaymentDetails { get; set; }
    public DbSet<BranchPaymentRow> BranchPaymentRows { get; set; }
    public DbSet<CreditorInvoicePaymentRow> CreditorInvoicePaymentRows { get; set; }
    public DbSet<OtherPayment> OtherPayments { get; set; }
    public DbSet<BranchPaymentForBranch> BranchPaymentForBranchs { get; set; }
    public DbSet<PayToView> PayToViews { get; set; }
    public DbSet<SupplierInvoiceView> SupplierInvoiceViews { get; set; }    
    public DbSet<ReceiptDetail> ReceiptDetails { get;set; }
    public DbSet<ReceiptClientInvoiceView> ReceiptClientInvoiceViews { get; set; }
    public DbSet<ClientReceiptDetailsResult> ClientReceiptDetailsResults { get; set; }
    public DbSet<KKDNExcelListView> KKDNExcelListViews { get; set; }
    public DbSet<AccountGLReportDto> AccountGLReportDtos { get; set; }
    public DbSet<ChequeStatus> ChequeStatus { get; set; }
    public DbSet<ClientLegalDemandAction> ClientLegalDemandActions { get; set; }
    public DbSet<InvoiceDetailRow> InvoiceDetailRows { get; set; }
    public DbSet<ProfessionalTaxConfiguration> ProfessionalTaxConfigurations { get; set; }
    public DbSet<PFConfiguration> PFConfigurations { get; set; }
    public DbSet<ESIConfiguration> ESIConfigurations { get; set; }
    public DbSet<ESIReasonCode> ESIReasonCodes { get; set; }
    public DbSet<TDSSlabConfiguration> TDSSlabConfigurations { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }
    public DbSet<TDSReportResult> TDSReportResults { get; set; }
    public DbSet<BranchPaymentsDto> BranchPaymentsDtos { get; set; }
    public DbSet<ClientAttendancePeriod> ClientAttendancePeriods { get; set; }
    public virtual DbSet<CreditNote> CreditNotes { get; set; }
    public virtual DbSet<DebitNote> DebitNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BranchPaymentForBranch>().ToTable("BranchPaymentsForBranch");

        modelBuilder.Entity<PayToView>().ToTable("PayToView");

        modelBuilder.Entity<SupplierInvoiceView>().ToTable("SupplierInvoiceView");

        modelBuilder.Entity<ReceiptClientInvoiceView>().ToTable("ReceiptClientInvoiceView");

        modelBuilder.Entity<KKDNExcelListView>().ToTable("KKDNExcelListView");

        modelBuilder.Entity<AccountGLReportDto>().HasNoKey().ToView(null);

        modelBuilder.Entity<ChequeStatus>().Property(c => c.Status).HasColumnName("ChequeStatus");
        modelBuilder.Entity<InvoiceDetailRow>().HasNoKey().ToView(null);
        modelBuilder.Entity<TDSReportResult>().HasNoKey().ToView(null);

        // Configure Employee CB (Commercial Breakdown) columns - Simplified
        modelBuilder.Entity<Employee>().Property(e => e.CB_Basic).HasColumnName("CB_Basic");
        modelBuilder.Entity<Employee>().Property(e => e.CB_DA).HasColumnName("CB_DA");
        modelBuilder.Entity<Employee>().Property(e => e.CB_HRA).HasColumnName("CB_HRA");
        modelBuilder.Entity<Employee>().Property(e => e.CB_HRAPercentage).HasColumnName("CB_HRAPercentage");
        modelBuilder.Entity<Employee>().Property(e => e.CB_Leaves).HasColumnName("CB_Leaves");
        modelBuilder.Entity<Employee>().Property(e => e.CB_LeavesPercentage).HasColumnName("CB_LeavesPercentage");
        modelBuilder.Entity<Employee>().Property(e => e.CB_OtherAllowances).HasColumnName("CB_OtherAllowances");
        modelBuilder.Entity<Employee>().Property(e => e.CB_NH).HasColumnName("CB_NH");
        modelBuilder.Entity<Employee>().Property(e => e.CB_NHPercentage).HasColumnName("CB_NHPercentage");
        modelBuilder.Entity<Employee>().Property(e => e.CB_SubTotal).HasColumnName("CB_SubTotal");

        modelBuilder.Entity<BranchPaymentRow>().HasNoKey().ToView(null);
        modelBuilder.Entity<CreditorInvoicePaymentRow>().HasNoKey().ToView(null);
        modelBuilder.Entity<AgreementDetails>(entity =>entity.Property(e => e.Rate).HasColumnType("decimal(18,4)"));
        modelBuilder.Entity<ClientInvoiceDetail>().Property(p => p.Rate).HasColumnType("decimal(18,4)");

        // SIP decimal configurations
        modelBuilder.Entity<SIP>().Property(s => s.SIP_from).HasPrecision(18, 8);
        modelBuilder.Entity<SIP>().Property(s => s.SIP_to).HasPrecision(18, 8);
        modelBuilder.Entity<SIP>().Property(s => s.SIP_worker).HasPrecision(18, 8);
        modelBuilder.Entity<SIP>().Property(s => s.SIP_total).HasPrecision(18, 8);

        // SOCSO decimal configurations
        modelBuilder.Entity<SOCSO>().Property(s => s.socso_from).HasPrecision(18, 8);
        modelBuilder.Entity<SOCSO>().Property(s => s.socso_to).HasPrecision(18, 8);
        modelBuilder.Entity<SOCSO>().Property(s => s.socso_employer).HasPrecision(18, 8);
        modelBuilder.Entity<SOCSO>().Property(s => s.socso_worker).HasPrecision(18, 8);
        modelBuilder.Entity<SOCSO>().Property(s => s.socso_total).HasPrecision(18, 8);
        modelBuilder.Entity<SOCSO>().Property(s => s.socso_50year).HasPrecision(18, 8);
        modelBuilder.Entity<SOCSO>().Property(s => s.socso_foreigner).HasPrecision(18, 8);

        // SalaryAdvance decimal configurations
        modelBuilder.Entity<SalaryAdvance>().Property(s => s.Amount).HasPrecision(18, 8);

        // StockIssueDetail decimal configurations
        modelBuilder.Entity<StockIssueDetail>().Property(s => s.CostPerUnit).HasPrecision(18, 8);

        // Supplier decimal configurations
        modelBuilder.Entity<Supplier>().Property(s => s.CreditLimit).HasPrecision(18, 8);

        // OtherPayment decimal configurations (if using LINQ-to-Objects)
        // Note: OtherPayment appears to be a view/DTO, may need validation in actual entity

        // Configure WorkPlace property to handle NULL values by converting to empty string
        modelBuilder.Entity<Quotation>().Property(e => e.WorkPlace).HasDefaultValue("")
            .HasConversion(v => v ?? "", v => string.IsNullOrEmpty(v) ? "" : v);
        modelBuilder.Entity<Agreement>().Property(e => e.WorkPlace).HasDefaultValue("")
            .HasConversion(v => v ?? "", v => string.IsNullOrEmpty(v) ? "" : v);

    }    
}
