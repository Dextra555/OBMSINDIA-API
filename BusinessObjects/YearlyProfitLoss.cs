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

        // ── Non-operational exclusion ─────────────────────────────────────────
        // Only Cat='U' (Expenses) rows appear in Expenses section.
        // CONTRA is always excluded regardless of Trade Type.
        private const string ExcludeNonOperational =
            "AND ISNULL((SELECT TOP 1 ic3.Cat FROM InventoryCategory ic3 " +
            "            WHERE ic3.ID = TRY_CAST(bp.ItemCategory AS INT)), '') = 'U' " +
            // CONTRA must never appear in Expenses regardless of Trade Type
            "AND ISNULL((SELECT TOP 1 ic3.Name FROM InventoryCategory ic3 " +
            "            WHERE ic3.ID = TRY_CAST(bp.ItemCategory AS INT)), '') NOT LIKE '%Contra%' ";

        // ── Exclude Purchase-type categories from Expenses ────────────────────
        // Kept for safety — ExcludeNonOperational already restricts to Cat='U',
        // so Cat='P' rows are already excluded. This is a no-op in practice.
        private const string ExcludePurchaseCategories = "";

        // ── Include ONLY Others rows ──────────────────────────────────────────
        // Rows whose Category Master Trade Type = 'O' (Other) go to the OTHERS
        // section — EXCEPT 'CONTRA' which must never appear in any P&L section.
        private const string IncludeNonOperational =
            "AND ISNULL((SELECT TOP 1 ic3.Cat FROM InventoryCategory ic3 " +
            "            WHERE ic3.ID = TRY_CAST(bp.ItemCategory AS INT)), '') = 'O' " +
            // CONTRA must never appear anywhere in the P&L report
            "AND ISNULL((SELECT TOP 1 ic3.Name FROM InventoryCategory ic3 " +
            "            WHERE ic3.ID = TRY_CAST(bp.ItemCategory AS INT)), '') NOT LIKE '%Contra%' ";

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
                decimal sales      = salesMap.TryGetValue(y, out var s)   ? s  : 0m;
                decimal debitNote  = debitNoteMap.TryGetValue(y, out var dn) ? dn : 0m;
                decimal creditNote = creditNoteMap.TryGetValue(y, out var cn) ? cn : 0m;
                decimal expenses   = expensesMap.TryGetValue(y, out var e)   ? e  : 0m;

                // TOTAL SALES (display) = InvoiceSales + DebitNote - CreditNote
                decimal totalSales = sales + debitNote - creditNote;

                // NET PROFIT = InvoiceSales − Expenses  (matches Monthly P&L)
                decimal netProfit  = sales - expenses;

                // EXPENSES % = Expenses / TotalSales × 100
                decimal expPct = totalSales != 0 ? Math.Round(expenses / totalSales * 100m, 2) : 0m;

                // NET PROFIT %:
                //   Sales = 0          → 100%
                //   NP ≥ 0 (profit)    → NP / Sales × 100
                //   NP < 0 (loss)      → -(TotalSales / Expenses × 100)
                decimal netPct;
                if (sales == 0m)
                    netPct = 100m;
                else if (netProfit >= 0m)
                    netPct = Math.Round(netProfit / sales * 100m, 2);
                else
                    netPct = expenses != 0m
                        ? -Math.Round(totalSales / expenses * 100m, 2)
                        : 100m;

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
            decimal grandNet    = grandSales - grandExpenses;
            decimal grandExpPct = grandTotalSales != 0 ? Math.Round(grandExpenses / grandTotalSales * 100m, 2) : 0m;
            // NET PROFIT % Grand Total — same Crystal Report formula
            decimal grandNetPct;
            if (grandSales == 0m)
                grandNetPct = 100m;
            else if (grandNet >= 0m)
                grandNetPct = Math.Round(grandNet / grandSales * 100m, 2);
            else
                grandNetPct = grandExpenses != 0m
                    ? -Math.Round(grandTotalSales / grandExpenses * 100m, 2)
                    : 100m;

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
            // Fetch all rows whose Category Master Trade Type = 'O' (Other).
            // This is purely driven by the Cat field — no hardcoded name lists.
            // Admin changes in Category Master are reflected automatically.

            string sSQL =
                "SELECT " +
                "  YEAR(bp.PaymentDate)                                    AS PayYear, " +
                "  ISNULL(ic.Name, CAST(bp.ItemCategory AS NVARCHAR(100))) AS CategoryName, " +
                "  SUM(bpd.Amount)                                         AS Amount " +
                "FROM BranchPayments bp " +
                "INNER JOIN BranchPaymentDetails bpd ON bpd.PaymentID = bp.ID " +
                "LEFT  JOIN InventoryCategory ic      ON ic.ID = TRY_CAST(bp.ItemCategory AS INT) " +
                "WHERE bp.IsDeleted = 0 " +
                "AND ISNULL(bpd.IsDeleted, 0) = 0 " +
                "AND bp.PaymentDate BETWEEN @StartDate AND @EndDate " +
                IncludeNonOperational +
                "GROUP BY YEAR(bp.PaymentDate), " +
                "  ISNULL(ic.Name, CAST(bp.ItemCategory AS NVARCHAR(100))) " +
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
