namespace OBMS.WebAPI.Models.DTO
{
    /// <summary>
    /// One row in the year-wise P&amp;L summary.
    /// YearLabel is the 4-digit year as a string (e.g. "2022"), or "Total" for the grand-total row.
    /// </summary>
    public class YearlyProfitLossDto
    {
        /// <summary>Year label: "2020", "2021", … "Total".</summary>
        public string YearLabel { get; set; } = string.Empty;

        // ── SALES section ──────────────────────────────────────────────────────

        /// <summary>Gross invoice sales / branch income for the year.</summary>
        public decimal Sales { get; set; }

        /// <summary>Debit note total for the year (adds to Sales).</summary>
        public decimal DebitNote { get; set; }

        /// <summary>Credit note total for the year (reduces Sales).</summary>
        public decimal CreditNote { get; set; }

        /// <summary>Total Sales = Sales + DebitNote − CreditNote.</summary>
        public decimal TotalSales { get; set; }

        // ── EXPENSES section ───────────────────────────────────────────────────

        /// <summary>Total operational expenses for the year.</summary>
        public decimal TotalExpenses { get; set; }

        // ── PROFIT section ─────────────────────────────────────────────────────

        /// <summary>Net Profit = TotalSales - TotalExpenses.</summary>
        public decimal NetProfit { get; set; }

        /// <summary>Expenses % = TotalExpenses / TotalSales * 100 (0 when TotalSales = 0).</summary>
        public decimal ExpensesPercent { get; set; }

        /// <summary>Net Profit % = NetProfit / TotalSales * 100 (0 when TotalSales = 0).</summary>
        public decimal NetProfitPercent { get; set; }
    }

    /// <summary>
    /// One expense-category row for the year-wise expense-detail section.
    /// </summary>
    public class YearlyExpenseDetailDto
    {
        /// <summary>Expense category name (e.g. "ACCOMMODATION", "AGENT FEE").</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>4-digit year.</summary>
        public int Year { get; set; }

        /// <summary>Total amount for this category in this year.</summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// One row in the OTHERS section (SST, Accrual, Loan Repayment, etc.).
    /// Category is the InventoryCategory name (or raw ItemCategory value when no match).
    /// </summary>
    public class YearlyOthersDetailDto
    {
        /// <summary>Others category name (e.g. "SST", "ACCRUAL2", "LOAN REPAYMENT").</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>4-digit year.</summary>
        public int Year { get; set; }

        /// <summary>Total amount for this Others category in this year.</summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Full response for the year-wise P&amp;L PDF endpoint.
    /// </summary>
    public class YearlyProfitLossResponseDto
    {
        /// <summary>Ordered list: one row per selected year, last row = grand total.</summary>
        public List<YearlyProfitLossDto> Summary { get; set; } = new();

        /// <summary>
        /// Flat list of (Category, Year, Amount) rows for ALL expense categories
        /// across all selected years.  The front-end groups these by category.
        /// </summary>
        public List<YearlyExpenseDetailDto> ExpenseDetails { get; set; } = new();

        /// <summary>
        /// Flat list of (Category, Year, Amount) rows for non-operational / Others
        /// categories (SST, Accrual, Loan Repayment, etc.) across all selected years.
        /// </summary>
        public List<YearlyOthersDetailDto> OthersDetails { get; set; } = new();

        /// <summary>Ordered list of selected years (e.g. [2021, 2022, 2023, 2024, 2025, 2026]).</summary>
        public List<int> Years { get; set; } = new();

        /// <summary>From year (inclusive).</summary>
        public int FromYear { get; set; }

        /// <summary>To year (inclusive).</summary>
        public int ToYear { get; set; }
    }
}
