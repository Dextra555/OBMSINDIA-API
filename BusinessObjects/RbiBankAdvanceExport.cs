using System.Data.SqlClient;

namespace OBMS.WebAPI.BusinessObjects
{
    public class RbiBankAdvanceExport
    {
        private static readonly IConfiguration configuration;

        // RBI Specification Fields
        public string FieldType { get; set; } = "";  // 0-RTGS, 1-NEFT, 2-Fund Transfer, 3-Direct Debit
        public string TransactionType { get; set; } = "";
        public string BeneficiaryCode { get; set; } = "";
        public string BeneficiaryAccountNumber { get; set; } = "";
        public string TransactionAmount { get; set; } = "";
        public string BeneficiaryName { get; set; } = "";
        public string CustomerReferenceNumber { get; set; } = "";
        public string PayerAccountNo { get; set; } = "";
        public string PayerName { get; set; } = "";
        public string PayerCode { get; set; } = "";  // BranchMaster.Code — used as payer identifier in RBI txt upload
        public string PayerAddress1 { get; set; } = "";
        public string PayerAddress2 { get; set; } = "";
        public string PayerAddress3 { get; set; } = "";
        public string PayerAddress4 { get; set; } = "";
        public string PayerAddress5 { get; set; } = "";
        public string PayerAddress6 { get; set; } = "";
        public string PayerAddress7 { get; set; } = "";
        public string PayerAddress8 { get; set; } = "";
        public string PayerAddress9 { get; set; } = "";
        public string PayerAddress10 { get; set; } = "";
        public string ChargeBearer { get; set; } = "";

        // Additional properties for PayrollRepository compatibility
        public string PaymentDetails1 => PayerAddress1;
        public string PaymentDetails2 => PayerAddress2;
        public string PaymentDetails3 => PayerAddress3;
        public string PaymentDetails4 => PayerAddress4;
        public string PaymentDetails5 => PayerAddress5;
        public string PaymentDetails6 => PayerAddress6;
        public string PaymentDetails7 => PayerAddress7;
        public string PaymentDetails8 => PayerAddress8;
        public string ChargeMaster { get; set; } = "";
        public string ChequeDate { get; set; } = "";
        public string MICRNumber { get; set; } = "";
        public string FieldName { get; set; } = "";
        public string ValueDate { get; set; } = "";  // DD-MM-YYYY format
        public string IFSCCode { get; set; } = "";
        public string BeneficiaryBankName { get; set; } = "";
        public string BeneficiaryBankBranchName { get; set; } = "";
        public string BeneficiaryEmailId { get; set; } = "";
        public string Narration { get; set; } = "";  // e.g. "August Advance" — month name + Advance

        static RbiBankAdvanceExport()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .Build();
        }

        private static string FormatTransactionAmount(decimal amount)
        {
            // Round to nearest whole number for RBI bank export
            return Math.Round(amount, MidpointRounding.AwayFromZero).ToString("0");
        }

        public static List<RbiBankAdvanceExport> GetRbiBankAdvanceExportData(string dtSalaryPeriod, string branch, string employeeType)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    // Parse the date safely
                    DateTime selectedDate;
                    if (!DateTime.TryParse(dtSalaryPeriod, out selectedDate))
                    {
                        selectedDate = DateTime.Now; // Default to current date if parsing fails
                    }
                    
                    string whereClause = " WHERE sa.IsDeleted = 0 " +
                                        " AND MONTH(sa.AdvanceDate) = " + selectedDate.Month +
                                        " AND YEAR(sa.AdvanceDate) = " + selectedDate.Year +
                                        " AND (esd.PAYMODE = 'Bank' OR (esd.PAYMODE IS NULL AND e.BankAccountNumber IS NOT NULL AND e.BankAccountNumber <> '')) ";
                    
                    if (!string.IsNullOrEmpty(branch) && branch != "ALL")
                    {
                        whereClause += " AND ISNULL(esd.EMPFL_BRANCHCODE, e.EMP_BRANCH_CODE) = @Branch ";
                    }
                    
                    if (!string.IsNullOrEmpty(employeeType) && employeeType != "ALL")
                    {
                        whereClause += " AND e.EMP_ROLE = @EmployeeType ";
                    }

                    string query = "SELECT " +
                    "e.EMP_CODE as BeneficiaryCode, " +
                    "ISNULL(NULLIF(esd.EMPFL_BK_ACCNO, ''), e.BankAccountNumber) as BeneficiaryAccountNumber, " +
                    "e.EMP_NAME as BeneficiaryName, " +
                    "SUM(sa.Amount) as TransactionAmount, " +
                    "e.EMP_ID as CustomerReferenceNumber, " +
                    "ISNULL(esd.EMPFL_BRANCHCODE, e.EMP_BRANCH_CODE) as PayerAddress1, " +
                    "ISNULL(NULLIF(e.IFSC_Code, ''), ISNULL(NULLIF(e.BankIFSC, ''), '')) as IFSCCode, " +
                    "ISNULL(NULLIF(e.BankName, ''), '') as BeneficiaryBankName, " +
                    "ISNULL(e.BankBranch, '') as BeneficiaryBankBranchName, " +
                    "ISNULL(bm.Email, '') as BeneficiaryEmailId, " +
                    "ISNULL(bm.BankAccount, '') as PayerAccountNo, " +
                    "ISNULL(bm.Name, '') as PayerName, " +
                    "ISNULL(bm.Code, '') as PayerCode, " +
                    "ISNULL(bm.Address1, '') as PayerAddress2, " +
                    "ISNULL(bm.Address2, '') as PayerAddress3, " +
                    "ISNULL(bm.City, '') as PayerAddress4, " +
                    "ISNULL(bm.State, '') as PayerAddress5, " +
                    "ISNULL(bm.PostCode, '') as PayerAddress6 " +
                    "FROM SalaryAdvance sa " +
                    "INNER JOIN Employee e ON sa.EmployeeID = e.EMP_ID " +
                    "LEFT JOIN EmployeeSalaryDetails esd ON e.EMP_CODE = esd.EMPFL_CODE " +
                    "LEFT JOIN BranchMaster bm ON e.EMP_BRANCH_CODE = bm.Code " +
                    whereClause +
                    " GROUP BY e.EMP_CODE, esd.EMPFL_BK_ACCNO, e.BankAccountNumber, e.EMP_NAME, e.EMP_ID, esd.EMPFL_BRANCHCODE, e.EMP_BRANCH_CODE, e.IFSC_Code, e.BankIFSC, e.BankName, e.BankBranch, bm.Email, bm.BankAccount, bm.Name, bm.Code, bm.Address1, bm.Address2, bm.City, bm.State, bm.PostCode, esd.PAYMODE " +
                    "ORDER BY e.EMP_NAME ";
                    
                    cmd.CommandText = query;

                    cmd.Parameters.AddWithValue("@SalaryPeriod", dtSalaryPeriod);
                    
                    if (!string.IsNullOrEmpty(branch) && branch != "ALL")
                    {
                        cmd.Parameters.AddWithValue("@Branch", branch);
                    }
                    
                    if (!string.IsNullOrEmpty(employeeType) && employeeType != "ALL")
                    {
                        cmd.Parameters.AddWithValue("@EmployeeType", employeeType);
                    }

                    using (SQLDataAccess sdaFactory = new SQLDataAccess(configuration))
                    {
                        using (SqlDataReader dr = sdaFactory.RetrieveData(cmd))
                        {
                            List<RbiBankAdvanceExport> exportData = new List<RbiBankAdvanceExport>();
                            
                            while (dr.Read())
                            {
                                var item = new RbiBankAdvanceExport
                                {
                                    FieldType = "N",  // N = NEFT — portal max 1 char (N/R)
                                    TransactionType = "ADVANCE",
                                    BeneficiaryCode = dr["BeneficiaryCode"]?.ToString() ?? "",
                                    BeneficiaryAccountNumber = dr["BeneficiaryAccountNumber"]?.ToString() ?? "",
                                    TransactionAmount = FormatTransactionAmount(decimal.Parse(dr["TransactionAmount"]?.ToString() ?? "0")),
                                    BeneficiaryName = dr["BeneficiaryName"]?.ToString() ?? "",
                                    CustomerReferenceNumber = dr["CustomerReferenceNumber"]?.ToString() ?? "",
                                    PayerAccountNo = dr["PayerAccountNo"]?.ToString() ?? "",
                                    PayerName = dr["PayerName"]?.ToString() ?? "",
                                    PayerCode = dr["PayerCode"]?.ToString() ?? "",
                                    PayerAddress1 = dr["PayerAddress1"]?.ToString() ?? "",
                                    PayerAddress2 = dr["PayerAddress2"]?.ToString() ?? "",
                                    PayerAddress3 = dr["PayerAddress3"]?.ToString() ?? "",
                                    PayerAddress4 = dr["PayerAddress4"]?.ToString() ?? "",
                                    PayerAddress5 = dr["PayerAddress5"]?.ToString() ?? "",
                                    PayerAddress6 = dr["PayerAddress6"]?.ToString() ?? "",
                                    PayerAddress7 = "",
                                    PayerAddress8 = "",
                                    PayerAddress9 = "",
                                    PayerAddress10 = "",
                                    ChargeBearer = "OUR",
                                    ValueDate = DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),  // DD/MM/YYYY — forced slashes regardless of OS locale
                                    IFSCCode = dr["IFSCCode"]?.ToString() ?? "",
                                    BeneficiaryBankName = dr["BeneficiaryBankName"]?.ToString() ?? "",
                                    BeneficiaryBankBranchName = dr["BeneficiaryBankBranchName"]?.ToString() ?? "",
                                    BeneficiaryEmailId = "singam@fwg.my",
                                    Narration = "Advance " + selectedDate.ToString("MMMM")  // e.g. "Advance September"
                                };
                                
                                exportData.Add(item);
                            }
                            return exportData;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public static string GenerateCsvExport(List<RbiBankAdvanceExport> exportData)
        {
            // Generate CSV in exact format from image specification
            var csv = new System.Text.StringBuilder();

            // Header row (optional - remove if not needed)
            csv.AppendLine("FieldType,TransactionType,BeneficiaryCode,BeneficiaryAccountNumber,TransactionAmount,BeneficiaryName,CustomerReferenceNumber,PayerAccountNo,PayerName,PayerAddress1,PayerAddress2,PayerAddress3,PayerAddress4,PayerAddress5,PayerAddress6,PayerAddress7,PayerAddress8,PayerAddress9,PayerAddress10,ChargeBearer,ValueDate,IFSCCode,BeneficiaryBankName,BeneficiaryBankBranchName,BeneficiaryEmailId");

            // Data rows
            foreach (var item in exportData)
            {
                csv.AppendLine($"{item.FieldType},{item.TransactionType},{item.BeneficiaryCode},{item.BeneficiaryAccountNumber},{item.TransactionAmount},{item.BeneficiaryName},{item.CustomerReferenceNumber},{item.PayerAccountNo},{item.PayerName},{item.PayerAddress1},{item.PayerAddress2},{item.PayerAddress3},{item.PayerAddress4},{item.PayerAddress5},{item.PayerAddress6},{item.PayerAddress7},{item.PayerAddress8},{item.PayerAddress9},{item.PayerAddress10},{item.ChargeBearer},{item.ValueDate},{item.IFSCCode},{item.BeneficiaryBankName},{item.BeneficiaryBankBranchName},{item.BeneficiaryEmailId}");
            }

            return csv.ToString();
        }

        public static byte[] GenerateCsvExportBytes(List<RbiBankAdvanceExport> exportData)
        {
            string csvContent = GenerateCsvExport(exportData);
            return System.Text.Encoding.UTF8.GetBytes(csvContent);
        }

        // -----------------------------------------------------------------------
        // Indian Bank portal upload — text file format (Advance)
        // -----------------------------------------------------------------------
        // 28 comma-separated fields per record, no header row, CRLF, UTF-8 no BOM.
        // Matches Excel formula columns B to AC (same as Salary format):
        //   N,FREWA,{creditAcct},{Amount},{BenefName},,,,,,,,,{Narration},,,,,,,,,,{DD/MM/YYYY},,{IFSCCode},,,{Email}
        //
        //  Pos  Col  Value / Source
        //  ---  ---  ----------------------------------------
        //   1    B   N                          (Transaction Type — NEFT)
        //   2    C   FREWA                      (Payment Product code)
        //   3    D   BeneficiaryAccountNumber   (employee credit account)
        //   4    E   TransactionAmount          (whole number)
        //   5    F   BeneficiaryName            (employee name)
        //   6    G   (empty)
        //   7    H   (empty)
        //   8    I   (empty)
        //   9    J   (empty)
        //  10    K   (empty)
        //  11    L   (empty)
        //  12    M   (empty)
        //  13    N   (empty)
        //  14    O   Narration                  (e.g. "Advance September")
        //  15    P   (empty)
        //  16    Q   (empty)
        //  17    R   (empty)
        //  18    S   (empty)
        //  19    T   (empty)
        //  20    U   (empty)
        //  21    V   (empty)
        //  22    W   (empty)
        //  23    X   ValueDate (DD/MM/YYYY)     (Chq/Txn Date — MANDATORY)
        //  24    Y   (empty)
        //  25    Z   IFSCCode                   (employee IFSC code)
        //  26   AA   (empty)
        //  27   AB   (empty)
        //  28   AC   BeneficiaryEmailId
        // -----------------------------------------------------------------------

        /// <summary>
        /// Wraps a field in double-quotes when it contains a comma, double-quote,
        /// or newline, escaping any embedded double-quotes per RFC 4180.
        /// </summary>
        private static string EscapeField(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            bool needsQuoting = value.IndexOf(',') >= 0
                             || value.IndexOf('"') >= 0
                             || value.IndexOf('\n') >= 0
                             || value.IndexOf('\r') >= 0;

            if (!needsQuoting)
                return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>
        /// Formats amount as a whole number (no decimals).
        /// e.g. 10000.00 → "10000"
        /// </summary>
        private static string FormatTxtAmount(string amount)
        {
            if (decimal.TryParse(amount, out decimal d))
                return Math.Truncate(d).ToString("0");
            return amount;
        }

        /// <summary>
        /// Generates the Indian Bank portal upload text file for Advance payments.
        /// 28 comma-separated fields per record, no header row, CRLF, UTF-8 no BOM.
        /// Same format as Salary export — columns B to AC.
        /// </summary>
        public static string GenerateTxtExport(List<RbiBankAdvanceExport> exportData)
        {
            var txt = new System.Text.StringBuilder();

            foreach (var item in exportData)
            {
                string[] fields =
                {
                    // 1  (B) — Transaction Type
                    EscapeField(item.FieldType),
                    // 2  (C) — Payment Product code
                    "FREWA",
                    // 3  (D) — Credit Account (employee's bank account)
                    EscapeField(item.BeneficiaryAccountNumber),
                    // 4  (E) — Amount (whole number)
                    EscapeField(FormatTxtAmount(item.TransactionAmount)),
                    // 5  (F) — Beneficiary Name (employee name)
                    EscapeField(item.BeneficiaryName),
                    // 6–13 (G–N) — empty (8 fields)
                    "", "", "", "", "", "", "", "",
                    // 14 (O) — Narration (e.g. "Advance September")
                    EscapeField(item.Narration),
                    // 15–22 (P–W) — empty (8 fields)
                    "", "", "", "", "", "", "", "",
                    // 23 (X) — Chq/Txn Date (DD/MM/YYYY) — MANDATORY
                    EscapeField(item.ValueDate),
                    // 24 (Y) — empty
                    "",
                    // 25 (Z) — IFSC Code (employee's bank IFSC)
                    EscapeField(item.IFSCCode),
                    // 26–27 (AA–AB) — empty
                    "", "",
                    // 28 (AC) — Beneficiary Email
                    EscapeField(item.BeneficiaryEmailId)
                };

                txt.Append(string.Join(",", fields));
                txt.Append("\r\n");
            }

            return txt.ToString();
        }
        public static byte[] GenerateTxtExportBytes(List<RbiBankAdvanceExport> exportData)
        {
            string txtContent = GenerateTxtExport(exportData);
            return new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                       .GetBytes(txtContent);
        }
    }
}
