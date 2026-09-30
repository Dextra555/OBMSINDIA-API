using System.Data.SqlClient;
using OBMS.WebAPI.Models.DTO;

namespace OBMS.WebAPI.BusinessObjects
{
    /// <summary>
    /// Calculates a year-wise Profit &amp; Loss report by aggregating the same underlying
    /// monthly records that the existing VWSummaryProfitNLoss view and BranchPayments
    /// table already provide.  The existing month-wise queries are NOT touched.
    ///
    /// SALES source:
    ///   - Sales (BranchIncome)  : VWSummaryProfitNLoss.BranchIncome
    ///   - Debit Note            : DebitNote table  (DebitNoteAmount, grouped by YEAR(DebitNoteDate))
    ///   - Credit Note           : CreditNote table (CreditNoteAmount, grouped by YEAR(CreditNoteDate))
    ///   - Total Sales           : Sales + DebitNote − CreditNote
    ///
    /// EXPENSE source:
    ///   Operational BranchPayments + BranchPaymentDetails
    ///   (same filter as existing SeparatedProfitLoss — excludes Contra/BU/Transfer/Internal/Adjustment)
    ///
    /// OTHERS source:
    ///   Non-operational BranchPayments (the rows excluded from Expenses above)
    ///   grouped by InventoryCategory.Name (SST, ACCRUAL2, LOAN REPAYMENT, etc.)
    /// </summary>
    public static class YearlyProfitLoss
    {
        private static readonly IConfiguration _configuration;

        static YearlyProfitLoss()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .Build();
        }

        // ── Non-operational exclusion (mirrors SeparatedProfitLoss filter) ──────
        // Also excludes rows whose InventoryCategory.Cat = 'O' (Trade Type = Other).
        private const string ExcludeNonOperational =
            "AND (bp.ItemCategory NOT LIKE '%Contra%'    OR bp.ItemCategory IS NULL) " +
            "AND (bp.ItemCategory NOT LIKE '%BU%'        OR bp.ItemCategory IS NULL) " +
            "AND (bp.ItemCategory NOT LIKE '%Transfer%'  OR bp.ItemCategory IS NULL) " +
            "AND (CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) NOT LIKE '%Contra%'     OR bp.PaymentPurpose IS NULL) " +
            "AND (CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) NOT LIKE '%Transfer%'   OR bp.PaymentPurpose IS NULL) " +
            "AND (CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) NOT LIKE '%Internal%'   OR bp.PaymentPurpose IS NULL) " +
            "AND (CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) NOT LIKE '%Adjustment%' OR bp.PaymentPurpose IS NULL) " +
            // Exclude Trade Type = Other (Cat = 'O') from operational expenses
            "AND ISNULL((SELECT TOP 1 ic3.Cat FROM InventoryCategory ic3 " +
            "            WHERE ic3.ID = TRY_CAST(bp.ItemCategory AS INT)), '') <> 'O' ";

        // ── Exclude Purchase-type categories from Expenses ────────────────────
        // Category Master: Cat = 'P' means Purchase (should NOT appear in P&L Expenses).
        //                  Cat = 'U' means Expenses/Utility (should appear in P&L Expenses).
        // When ItemCategory is not a valid integer (no InventoryCategory join),
        // we allow it through — only rows explicitly marked Cat='P' are excluded.
        private const string ExcludePurchaseCategories =
            "AND ISNULL((SELECT TOP 1 ic2.Cat FROM InventoryCategory ic2 " +
            "            WHERE ic2.ID = TRY_CAST(bp.ItemCategory AS INT)), 'U') <> 'P' ";

        // ── Include ONLY non-operational (Others) rows ────────────────────────
        // Also includes any row whose InventoryCategory.Cat = 'O' (Trade Type = Other).
        private const string IncludeNonOperational =
            "AND ( " +
            "    bp.ItemCategory LIKE '%Contra%' " +
            "    OR bp.ItemCategory LIKE '%BU%' " +
            "    OR bp.ItemCategory LIKE '%Transfer%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Contra%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Transfer%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Internal%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Adjustment%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%SST%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Accrual%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Loan%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Reimbursement%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%FD Placement%' " +
            "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%FWG Global%' " +
            "    OR ISNULL(ic.Name, '') LIKE '%SST%' " +
            "    OR ISNULL(ic.Name, '') LIKE '%Accrual%' " +
            "    OR ISNULL(ic.Name, '') LIKE '%Loan%' " +
            "    OR ISNULL(ic.Name, '') LIKE '%Reimbursement%' " +
            "    OR ISNULL(ic.Name, '') LIKE '%FD%' " +
            "    OR ISNULL(ic.Name, '') LIKE '%FWG%' " +
            // Include any category explicitly marked as Trade Type = Other (Cat = 'O')
            "    OR ISNULL((SELECT TOP 1 ic3.Cat FROM InventoryCategory ic3 " +
            "               WHERE ic3.ID = TRY_CAST(bp.ItemCategory AS INT)), '') = 'O' " +
            ") ";

        /// <summary>
        /// Returns the full year-wise P&amp;L response for all branches combined,
        /// covering fromYear through toYear inclusive.
        /// </summary>
        public static YearlyProfitLossResponseDto GetYearlyReport(int fromYear, int toYear)
        {
            var years = Enumerable.Range(fromYear, toYear - fromYear + 1).ToList();

            var startDate = new DateTime(fromYear, 1, 1);
            var endDate   = new DateTime(toYear, 12, 31);

            var summaryRows    = QuerySummary(startDate, endDate, years);
            var expenseDetails = QueryExpenseDetails(startDate, endDate);
            var othersDetails  = QueryOthersDetails(startDate, endDate);

            return new YearlyProfitLossResponseDto
            {
                Summary        = summaryRows,
                ExpenseDetails = expenseDetails,
                OthersDetails  = othersDetails,
                Years          = years,
                FromYear       = fromYear,
                ToYear         = toYear
            };
        }

        // ════════════════════════════════════════════════════════════════════════
        // QUERY 1 — Year-wise summary
        //   Sales    : VWSummaryProfitNLoss.BranchIncome  (all branches, year-wise)
        //   DebitNote: DebitNote table (DebitNoteAmount, year of DebitNoteDate)
        //   CreditNote: CreditNote table (CreditNoteAmount, year of CreditNoteDate)
        //   TotalSales = Sales + DebitNote − CreditNote
        //   Expenses  : BranchPayments + BranchPaymentDetails (operational only)
        //   NetProfit = TotalSales − Expenses
        // ════════════════════════════════════════════════════════════════════════

        private static List<YearlyProfitLossDto> QuerySummary(
            DateTime startDate, DateTime endDate, List<int> years)
        {
            // ── 1a. Sales (BranchIncome) from VWSummaryProfitNLoss ─────────────
            const string sqlSales =
                "SELECT " +
                "  YEAR(TransactionDate)  AS PayYear, " +
                "  SUM(BranchIncome)      AS TotalSales " +
                "FROM VWSummaryProfitNLoss " +
                "WHERE TransactionDate BETWEEN @StartDate AND @EndDate " +
                "GROUP BY YEAR(TransactionDate) " +
                "ORDER BY PayYear ";

            var salesMap = new Dictionary<int, decimal>();

            using (var cmd = new SqlCommand { CommandText = sqlSales })
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate",   endDate);

                using var sda = new SQLDataAccess(_configuration);
                using var dr  = sda.RetrieveData(cmd);
                while (dr.Read())
                {
                    int yr = dr.GetInt32(dr.GetOrdinal("PayYear"));
                    salesMap[yr] = dr.GetDecimal(dr.GetOrdinal("TotalSales"));
                }
            }

            // ── 1b. Debit Notes from DebitNote table ───────────────────────────
            const string sqlDebitNote =
                "SELECT " +
                "  YEAR(DebitNoteDate)    AS PayYear, " +
                "  SUM(DebitNoteAmount)   AS TotalDebitNote " +
                "FROM DebitNote " +
                "WHERE IsDeleted = 0 " +
                "AND DebitNoteDate BETWEEN @StartDate AND @EndDate " +
                "GROUP BY YEAR(DebitNoteDate) " +
                "ORDER BY PayYear ";

            var debitNoteMap = new Dictionary<int, decimal>();

            using (var cmd = new SqlCommand { CommandText = sqlDebitNote })
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate",   endDate);

                using var sda = new SQLDataAccess(_configuration);
                using var dr  = sda.RetrieveData(cmd);
                while (dr.Read())
                {
                    int yr = dr.GetInt32(dr.GetOrdinal("PayYear"));
                    debitNoteMap[yr] = dr.GetDecimal(dr.GetOrdinal("TotalDebitNote"));
                }
            }

            // ── 1c. Credit Notes from CreditNote table ─────────────────────────
            const string sqlCreditNote =
                "SELECT " +
                "  YEAR(CreditNoteDate)   AS PayYear, " +
                "  SUM(CreditNoteAmount)  AS TotalCreditNote " +
                "FROM CreditNote " +
                "WHERE IsDeleted = 0 " +
                "AND CreditNoteDate BETWEEN @StartDate AND @EndDate " +
                "GROUP BY YEAR(CreditNoteDate) " +
                "ORDER BY PayYear ";

            var creditNoteMap = new Dictionary<int, decimal>();

            using (var cmd = new SqlCommand { CommandText = sqlCreditNote })
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate",   endDate);

                using var sda = new SQLDataAccess(_configuration);
                using var dr  = sda.RetrieveData(cmd);
                while (dr.Read())
                {
                    int yr = dr.GetInt32(dr.GetOrdinal("PayYear"));
                    creditNoteMap[yr] = dr.GetDecimal(dr.GetOrdinal("TotalCreditNote"));
                }
            }

            // ── 1d. Expenses from BranchPayments (operational only, excluding Purchase-type categories) ──
            string sqlExpenses =
                "SELECT " +
                "  YEAR(bp.PaymentDate)  AS PayYear, " +
                "  SUM(bpd.Amount)       AS TotalExpenses " +
                "FROM BranchPayments bp " +
                "INNER JOIN BranchPaymentDetails bpd ON bpd.PaymentID = bp.ID " +
                "LEFT  JOIN InventoryCategory ic      ON ic.ID = TRY_CAST(bp.ItemCategory AS INT) " +
                "WHERE bp.IsDeleted = 0 " +
                "AND ISNULL(bpd.IsDeleted, 0) = 0 " +
                "AND bp.PaymentDate BETWEEN @StartDate AND @EndDate " +
                ExcludeNonOperational +
                ExcludePurchaseCategories +
                "GROUP BY YEAR(bp.PaymentDate) " +
                "ORDER BY PayYear ";

            var expensesMap = new Dictionary<int, decimal>();

            using (var cmd = new SqlCommand { CommandText = sqlExpenses })
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate",   endDate);

                using var sda = new SQLDataAccess(_configuration);
                using var dr  = sda.RetrieveData(cmd);
                while (dr.Read())
                {
                    int yr = dr.GetInt32(dr.GetOrdinal("PayYear"));
                    expensesMap[yr] = dr.GetDecimal(dr.GetOrdinal("TotalExpenses"));
                }
            }

            // ── Build ordered per-year rows ────────────────────────────────────
            var result       = new List<YearlyProfitLossDto>();
            decimal grandSales      = 0m;
            decimal grandDebitNote  = 0m;
            decimal grandCreditNote = 0m;
            decimal grandTotalSales = 0m;
            decimal grandExpenses   = 0m;

            foreach (int y in years)
            {
                decimal sales      = salesMap.TryGetValue(y, out var s)  ? s  : 0m;
                decimal debitNote  = debitNoteMap.TryGetValue(y, out var dn) ? dn : 0m;
                decimal creditNote = creditNoteMap.TryGetValue(y, out var cn) ? cn : 0m;
                decimal totalSales = sales + debitNote - creditNote;
                decimal expenses   = expensesMap.TryGetValue(y, out var e) ? e  : 0m;
                decimal netProfit  = totalSales - expenses;
                decimal expPct     = totalSales != 0 ? Math.Round(expenses  / totalSales * 100m, 2) : 0m;
                decimal netPct     = totalSales != 0 ? Math.Round(netProfit / totalSales * 100m, 2) : 0m;

                result.Add(new YearlyProfitLossDto
                {
                    YearLabel        = y.ToString(),
                    Sales            = sales,
                    DebitNote        = debitNote,
                    CreditNote       = creditNote,
                    TotalSales       = totalSales,
                    TotalExpenses    = expenses,
                    NetProfit        = netProfit,
                    ExpensesPercent  = expPct,
                    NetProfitPercent = netPct
                });

                grandSales      += sales;
                grandDebitNote  += debitNote;
                grandCreditNote += creditNote;
                grandTotalSales += totalSales;
                grandExpenses   += expenses;
            }

            // Grand total row
            decimal grandNet    = grandTotalSales - grandExpenses;
            decimal grandExpPct = grandTotalSales != 0 ? Math.Round(grandExpenses / grandTotalSales * 100m, 2) : 0m;
            decimal grandNetPct = grandTotalSales != 0 ? Math.Round(grandNet      / grandTotalSales * 100m, 2) : 0m;

            result.Add(new YearlyProfitLossDto
            {
                YearLabel        = "Total",
                Sales            = grandSales,
                DebitNote        = grandDebitNote,
                CreditNote       = grandCreditNote,
                TotalSales       = grandTotalSales,
                TotalExpenses    = grandExpenses,
                NetProfit        = grandNet,
                ExpensesPercent  = grandExpPct,
                NetProfitPercent = grandNetPct
            });

            return result;
        }

        // ════════════════════════════════════════════════════════════════════════
        // QUERY 2 — Expense category detail per year (operational expenses only)
        // ════════════════════════════════════════════════════════════════════════

        private static List<YearlyExpenseDetailDto> QueryExpenseDetails(
            DateTime startDate, DateTime endDate)
        {
            string sSQL =
                "SELECT " +
                "  YEAR(bp.PaymentDate)                                       AS PayYear, " +
                "  ISNULL(ic.Name, CAST(bp.ItemCategory AS NVARCHAR(100)))    AS CategoryName, " +
                "  SUM(bpd.Amount)                                            AS Amount " +
                "FROM BranchPayments bp " +
                "INNER JOIN BranchPaymentDetails bpd ON bpd.PaymentID = bp.ID " +
                "LEFT  JOIN InventoryCategory ic      ON ic.ID = TRY_CAST(bp.ItemCategory AS INT) " +
                "WHERE bp.IsDeleted = 0 " +
                "AND ISNULL(bpd.IsDeleted, 0) = 0 " +
                "AND bp.PaymentDate BETWEEN @StartDate AND @EndDate " +
                ExcludeNonOperational +
                ExcludePurchaseCategories +
                "GROUP BY YEAR(bp.PaymentDate), " +
                "  ISNULL(ic.Name, CAST(bp.ItemCategory AS NVARCHAR(100))) " +
                "ORDER BY CategoryName, PayYear ";

            var result = new List<YearlyExpenseDetailDto>();

            using var cmd = new SqlCommand { CommandText = sSQL };
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate",   endDate);

            using var sda = new SQLDataAccess(_configuration);
            using var dr  = sda.RetrieveData(cmd);

            while (dr.Read())
            {
                result.Add(new YearlyExpenseDetailDto
                {
                    Year     = dr.GetInt32(dr.GetOrdinal("PayYear")),
                    Category = dr.GetString(dr.GetOrdinal("CategoryName")),
                    Amount   = dr.GetDecimal(dr.GetOrdinal("Amount"))
                });
            }

            return result;
        }

        // ════════════════════════════════════════════════════════════════════════
        // QUERY 3 — OTHERS section: non-operational transactions per year
        //
        // These are the entries that are EXCLUDED from operational expenses,
        // matching the Excel "OTHERS" section:
        //   SST | ACCRUAL2 | LOAN REPAYMENT | FWG GLOBAL - LOAN |
        //   LOAN - DATUK A. CHANDRAKUMANAN | REIMBURSEMENT | LOAN - AURA | FD PLACEMENT
        //
        // Source : BranchPayments where the row matches the non-operational filter
        //          OR where PaymentPurpose/InventoryCategory.Name matches known Others labels.
        //          We fetch ALL non-operational rows and let the front-end display them;
        //          this mirrors the Excel which shows whatever categories exist in the data.
        // ════════════════════════════════════════════════════════════════════════

        private static List<YearlyOthersDetailDto> QueryOthersDetails(
            DateTime startDate, DateTime endDate)
        {
            // We select ALL non-operational BranchPayment rows grouped by year and
            // category name.  "Non-operational" means it matches the exclusion filter
            // that is applied to operational expenses (Contra/BU/Transfer/Internal/Adjustment)
            // OR it is one of the known Others labels from the Excel.
            //
            // Strategy: instead of trying to replicate a complex OR filter in one pass,
            // we fetch all BranchPayments rows that were EXCLUDED from expenses (i.e. NOT
            // matching the operational ExcludeNonOperational fragment) and group them.
            // In other words, rows where the NON-OPERATIONAL include condition fires.

            string sSQL =
                "SELECT " +
                "  YEAR(bp.PaymentDate)                                       AS PayYear, " +
                "  ISNULL(ic.Name, CAST(ISNULL(bp.PaymentPurpose, bp.ItemCategory) AS NVARCHAR(200))) AS CategoryName, " +
                "  SUM(bpd.Amount)                                            AS Amount " +
                "FROM BranchPayments bp " +
                "INNER JOIN BranchPaymentDetails bpd ON bpd.PaymentID = bp.ID " +
                "LEFT  JOIN InventoryCategory ic      ON ic.ID = TRY_CAST(bp.ItemCategory AS INT) " +
                "WHERE bp.IsDeleted = 0 " +
                "AND ISNULL(bpd.IsDeleted, 0) = 0 " +
                "AND bp.PaymentDate BETWEEN @StartDate AND @EndDate " +
                "AND ( " +
                "    bp.ItemCategory LIKE '%Contra%' " +
                "    OR bp.ItemCategory LIKE '%BU%' " +
                "    OR bp.ItemCategory LIKE '%Transfer%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Contra%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Transfer%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Internal%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Adjustment%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%SST%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Accrual%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Loan%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%Reimbursement%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%FD Placement%' " +
                "    OR CAST(bp.PaymentPurpose AS NVARCHAR(MAX)) LIKE '%FWG Global%' " +
                "    OR ISNULL(ic.Name, '') LIKE '%SST%' " +
                "    OR ISNULL(ic.Name, '') LIKE '%Accrual%' " +
                "    OR ISNULL(ic.Name, '') LIKE '%Loan%' " +
                "    OR ISNULL(ic.Name, '') LIKE '%Reimbursement%' " +
                "    OR ISNULL(ic.Name, '') LIKE '%FD%' " +
                "    OR ISNULL(ic.Name, '') LIKE '%FWG%' " +
                // Include any category explicitly marked as Trade Type = Other (Cat = 'O')
                "    OR ISNULL(ic.Cat, '') = 'O' " +
                ") " +
                "GROUP BY YEAR(bp.PaymentDate), " +
                "  ISNULL(ic.Name, CAST(ISNULL(bp.PaymentPurpose, bp.ItemCategory) AS NVARCHAR(200))) " +
                "ORDER BY CategoryName, PayYear ";

            var result = new List<YearlyOthersDetailDto>();

            using var cmd = new SqlCommand { CommandText = sSQL };
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate",   endDate);

            using var sda = new SQLDataAccess(_configuration);
            using var dr  = sda.RetrieveData(cmd);

            while (dr.Read())
            {
                result.Add(new YearlyOthersDetailDto
                {
                    Year     = dr.GetInt32(dr.GetOrdinal("PayYear")),
                    Category = dr.GetString(dr.GetOrdinal("CategoryName")),
                    Amount   = dr.GetDecimal(dr.GetOrdinal("Amount"))
                });
            }

            return result;
        }
    }
}
