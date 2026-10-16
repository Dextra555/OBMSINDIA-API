using System.Data.SqlClient;

namespace OBMS.WebAPI.BusinessObjects
{
    public class RbiBankSalaryExport
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
        public string Narration { get; set; } = "";  // e.g. "July Salary" — month name + Salary

        static RbiBankSalaryExport()
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

        public static List<RbiBankSalaryExport> GetRbiBankSalaryExportData(string dtSalaryPeriod, string branch, string employeeType)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    // Parse the period date to derive month name for Narration field
                    DateTime selectedDate;
                    if (!DateTime.TryParse(dtSalaryPeriod, out selectedDate))
                    {
                        selectedDate = DateTime.Now;
                    }

                    string whereClause = " WHERE PaySlip.Period = @SalaryPeriod " +
                                        " AND (ESD.PAYMODE = 'Bank' OR (ESD.PAYMODE IS NULL AND Employee.BankAccountNumber IS NOT NULL AND Employee.BankAccountNumber <> '')) ";
                    
                    if (!string.IsNullOrEmpty(branch) && branch != "ALL")
                    {
                        whereClause += " AND ISNULL(ESD.EMPFL_BRANCHCODE, Employee.EMP_BRANCH_CODE) = @Branch ";
                    }
                    
                    if (!string.IsNullOrEmpty(employeeType) && employeeType != "ALL")
                    {
                        whereClause += " AND Employee.EMP_ROLE = @EmployeeType ";
                    }

                    cmd.CommandText = @"SELECT
                        Employee.EMP_CODE as BeneficiaryCode,
                        ISNULL(NULLIF(ESD.EMPFL_BK_ACCNO, ''), Employee.BankAccountNumber) as BeneficiaryAccountNumber,
                        Employee.EMP_NAME as BeneficiaryName,
                        PaySlip.NetPay + ISNULL(MiscAmt.TotalMiscAmount, 0) as TransactionAmount,
                        Employee.EMP_ID as CustomerReferenceNumber,
                        ISNULL(ESD.EMPFL_BRANCHCODE, Employee.EMP_BRANCH_CODE) as PayerAddress1,
                        ISNULL(NULLIF(Employee.IFSC_Code, ''), ISNULL(NULLIF(Employee.BankIFSC, ''), '')) as IFSCCode,
                        ISNULL(NULLIF(Employee.BankName, ''), BankMaster.Name) as BeneficiaryBankName,
                        ISNULL(Employee.BankBranch, '') as BeneficiaryBankBranchName,
                        ISNULL(BankMaster.Email, '') as BeneficiaryEmailId,
                        ISNULL(BankMaster.BankAccount, '') as PayerAccountNo,
                        ISNULL(BankMaster.Name, '') as PayerName,
                        ISNULL(BankMaster.Address1, '') as PayerAddress2,
                        ISNULL(BankMaster.Address2, '') as PayerAddress3,
                        ISNULL(BankMaster.City, '') as PayerAddress4,
                        ISNULL(BankMaster.State, '') as PayerAddress5,
                        ISNULL(BankMaster.PostCode, '') as PayerAddress6
                        FROM PaySlip
                        INNER JOIN Employee ON PaySlip.EmployeeID = Employee.EMP_ID
                        LEFT JOIN EmployeeSalaryDetails ESD ON ESD.EMPFL_CODE = Employee.EMP_CODE
                        LEFT JOIN BranchMaster BankMaster ON Employee.EMP_BRANCH_CODE = BankMaster.Code
                        LEFT JOIN (
                            SELECT EmployeeID, SUM(Amount) AS TotalMiscAmount
                            FROM MiscTrans
                            WHERE YEAR(TransDate) = YEAR(@SalaryPeriod)
                              AND MONTH(TransDate) = MONTH(@SalaryPeriod)
                            GROUP BY EmployeeID
                        ) MiscAmt ON MiscAmt.EmployeeID = Employee.EMP_ID "
                        + whereClause +
                        " ORDER BY Employee.EMP_NAME ";

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
                            List<RbiBankSalaryExport> exportData = new List<RbiBankSalaryExport>();
                            
                            while (dr.Read())
                            {
                                var item = new RbiBankSalaryExport
                                {
                                    FieldType = "N",  // N = NEFT — portal max 1 char (N/R)
                                    TransactionType = "SALARY",
                                    BeneficiaryCode = dr["BeneficiaryCode"]?.ToString() ?? "",
                                    BeneficiaryAccountNumber = dr["BeneficiaryAccountNumber"]?.ToString() ?? "",
                                    TransactionAmount = FormatTransactionAmount(decimal.Parse(dr["TransactionAmount"]?.ToString() ?? "0")),
                                    BeneficiaryName = dr["BeneficiaryName"]?.ToString() ?? "",
                                    CustomerReferenceNumber = dr["CustomerReferenceNumber"]?.ToString() ?? "",
                                    PayerAccountNo = dr["PayerAccountNo"]?.ToString() ?? "",
                                    PayerName = dr["PayerName"]?.ToString() ?? "",
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
                                    Narration = "Salary " + selectedDate.ToString("MMMM")  // e.g. "Salary July"
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

        public static string GenerateCsvExport(List<RbiBankSalaryExport> exportData)
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

        public static byte[] GenerateCsvExportBytes(List<RbiBankSalaryExport> exportData)
        {
            string csvContent = GenerateCsvExport(exportData);
            return System.Text.Encoding.UTF8.GetBytes(csvContent);
        }

        // -----------------------------------------------------------------------
        // Indian Bank Net Banking portal — text file format (Salary)
        // -----------------------------------------------------------------------
        // 28 comma-separated fields per record, no header row, CRLF, UTF-8 no BOM.
        // Matches Excel formula: =B6&","&C6&","&...&","&AC6  (columns B to AC = 28 fields)
        //
        // Sample: N,FREWA,924010028747491,10000,Palani,,,,,,,,,,Advance September,,,,,,,,,,21/09/2026,,UTIB0000676,,,singam@fwg.my
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

        private static string FormatTxtAmount(string amount)
        {
            if (decimal.TryParse(amount, out decimal d))
                return Math.Truncate(d).ToString("0");
            return amount;
        }

        /// <summary>
        /// Sanitises the "Print Location" field (field 9 — Beneficiary Bank Name) for the
        /// Indian Bank portal.  The portal rejects special characters and enforces a 20-char
        /// maximum, so we strip everything except letters, digits, and spaces, then truncate.
        /// </summary>
        private static string SanitizePrintLocation(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // Remove any character that is not a letter, digit, or space
            var clean = System.Text.RegularExpressions.Regex.Replace(value, @"[^A-Za-z0-9 ]", "");

            // Collapse multiple spaces and trim
            clean = System.Text.RegularExpressions.Regex.Replace(clean, @" {2,}", " ").Trim();

            // Portal max length is 20 characters
            return clean.Length > 20 ? clean.Substring(0, 20).TrimEnd() : clean;
        }

        /// <summary>
        /// Generates the Indian Bank portal upload text file for Salary/Advance payments.
        /// 28 comma-separated fields per record, no header row, CRLF, UTF-8 no BOM.
        /// Matches Excel formula columns B to AC:
        ///   1=N, 2=FREWA, 3=CreditAcct, 4=Amount, 5=BenefName,
        ///   6-13=empty(8), 14=Narration, 15-22=empty(8), 23=Date(DD/MM/YYYY),
        ///   24=empty, 25=IFSCCode, 26-27=empty, 28=Email
        /// </summary>
        public static string GenerateTxtExport(List<RbiBankSalaryExport> exportData)
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
                    // 14 (O) — Narration (e.g. "Salary September")
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

        public static byte[] GenerateTxtExportBytes(List<RbiBankSalaryExport> exportData)
        {
            string txtContent = GenerateTxtExport(exportData);
            return new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                       .GetBytes(txtContent);
        }
    }
}
