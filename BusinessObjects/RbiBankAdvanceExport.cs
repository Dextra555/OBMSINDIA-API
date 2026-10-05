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
            // No decimal if amount is in whole number
            return amount == Math.Truncate(amount) ? amount.ToString("0") : amount.ToString("F2");
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
                    
                    string whereClause = " WHERE sa.IsDeleted = 0 AND MONTH(sa.AdvanceDate) = " + selectedDate.Month + " AND YEAR(sa.AdvanceDate) = " + selectedDate.Year;
                    
                    if (!string.IsNullOrEmpty(branch) && branch != "ALL")
                    {
                        whereClause += " AND e.EMP_BRANCH_CODE = @Branch ";
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
                    "ISNULL(esd.EMPFL_BRANCHCODE, '') as PayerAddress1, " +
                    "ISNULL(NULLIF(e.IFSC_Code, ''), e.BankIFSC) as IFSCCode, " +
                    "ISNULL(e.BankName, '') as BeneficiaryBankName, " +
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
                    " GROUP BY e.EMP_CODE, esd.EMPFL_BK_ACCNO, e.BankAccountNumber, e.EMP_NAME, e.EMP_ID, esd.EMPFL_BRANCHCODE, e.IFSC_Code, e.BankIFSC, e.BankName, e.BankBranch, bm.Email, bm.BankAccount, bm.Name, bm.Code, bm.Address1, bm.Address2, bm.City, bm.State, bm.PostCode " +
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
                                    FieldType = "N",  // N = NEFT (RBI portal format)
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
                                    ValueDate = DateTime.Now.ToString("dd/MM/yyyy"),  // DD/MM/YYYY — RBI portal format
                                    IFSCCode = dr["IFSCCode"]?.ToString() ?? "",
                                    BeneficiaryBankName = dr["BeneficiaryBankName"]?.ToString() ?? "",
                                    BeneficiaryBankBranchName = dr["BeneficiaryBankBranchName"]?.ToString() ?? "",
                                    BeneficiaryEmailId = dr["BeneficiaryEmailId"]?.ToString() ?? "",
                                    Narration = selectedDate.ToString("MMMM") + " Advance"  // e.g. "August Advance"
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
        // RBI Bank portal upload — text file format
        // -----------------------------------------------------------------------
        // Exact 31-field layout derived from actual RBI portal sample file:
        //
        //  Pos  Value                  Source
        //  ---  ---------------------  ----------------------------------------
        //   1   N                      FieldType (N = NEFT)
        //   2   FREWA                  Fixed payer identifier (same for all records)
        //   3   6309920673             BeneficiaryAccountNumber
        //   4   3000                   TransactionAmount (whole number, no decimals)
        //   5   A L RAMASWAMY          BeneficiaryName
        //  6–13 (empty x8)             —
        //  14   July Advance           Narration (MMMM + " Advance")
        // 15–23 (empty x9)             —
        //  24   01/10/2026             ValueDate (DD/MM/YYYY)
        //  25   (empty)                —
        //  26   IDIB000E039            IFSCCode
        // 27–28 (empty x2)             —
        //  29   singam@fwg.my          BeneficiaryEmailId
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
        /// Formats amount as a whole number (no decimals) as seen in the RBI portal sample.
        /// e.g. 10000.00 → "10000"
        /// </summary>
        private static string FormatTxtAmount(string amount)
        {
            if (decimal.TryParse(amount, out decimal d))
                return Math.Truncate(d).ToString("0");
            return amount;
        }

        /// <summary>
        /// Generates the RBI Bank portal upload text file content.
        /// Produces exactly 31 comma-separated fields per record, no header row,
        /// CRLF line endings, UTF-8 no BOM — matches the actual RBI portal sample format.
        ///
        ///  Pos  Value                  Source
        ///  ---  ---------------------  ----------------------------------------
        ///   1   N                      FieldType (N = NEFT)
        ///   2   FREWA                  Fixed payer identifier (same for all records)
        ///   3   6309920673             BeneficiaryAccountNumber
        ///   4   3000                   TransactionAmount (whole number)
        ///   5   A L RAMASWAMY          BeneficiaryName
        ///  6–13 (empty x8)             —
        ///  14   July Advance           Narration (MMMM + " Advance")
        /// 15–23 (empty x9)             —
        ///  24   01/10/2026             ValueDate (DD/MM/YYYY)
        ///  25   (empty)                —
        ///  26   IDIB000E039            IFSCCode
        /// 27–28 (empty x2)             —
        ///  29   singam@fwg.my          BeneficiaryEmailId
        /// </summary>
        public static string GenerateTxtExport(List<RbiBankAdvanceExport> exportData)
        {
            var txt = new System.Text.StringBuilder();

            foreach (var item in exportData)
            {
                // Pos 2: Fixed value "FREWA" for all records (RBI portal payer identifier)
                string payerIdentifier = "FREWA";

                string[] fields =
                {
                    // 1  — FieldType (N = NEFT)
                    EscapeField(item.FieldType),
                    // 2  — PayerCode-PayerName (e.g. "FWG001-Chennai")
                    EscapeField(payerIdentifier),
                    // 3  — Beneficiary Account Number
                    EscapeField(item.BeneficiaryAccountNumber),
                    // 4  — Transaction Amount (whole number)
                    EscapeField(FormatTxtAmount(item.TransactionAmount)),
                    // 5  — Beneficiary Name
                    EscapeField(item.BeneficiaryName),
                    // 6–13 — 8 empty fields
                    "", "", "", "", "", "", "", "",
                    // 14 — Narration (e.g. "July Advance")
                    EscapeField(item.Narration),
                    // 15–22 — 8 empty fields
                    "", "", "", "", "", "", "", "",
                    // 23 — Value Date (DD/MM/YYYY)
                    EscapeField(item.ValueDate),
                    // 24 — empty
                    "",
                    // 25 — IFSC Code
                    EscapeField(item.IFSCCode),
                    // 26–27 — 2 empty fields
                    "", "",
                    // 28 — Fixed email for all records
                    "singam@fwg.my"
                };

                txt.Append(string.Join(",", fields));
                txt.Append("\r\n");
            }

            return txt.ToString();
        }

        /// <summary>
        /// Returns the RBI portal upload text file as a UTF-8 byte array (no BOM).
        /// </summary>
        public static byte[] GenerateTxtExportBytes(List<RbiBankAdvanceExport> exportData)
        {
            string txtContent = GenerateTxtExport(exportData);
            return new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                       .GetBytes(txtContent);
        }
    }
}
