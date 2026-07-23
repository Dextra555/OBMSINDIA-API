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
            // No decimal if amount is in whole number
            return amount == Math.Truncate(amount) ? amount.ToString("0") : amount.ToString("F2");
        }

        public static List<RbiBankSalaryExport> GetRbiBankSalaryExportData(string dtSalaryPeriod, string branch, string employeeType)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    string whereClause = " WHERE PaySlip.Period = @SalaryPeriod ";
                    
                    if (!string.IsNullOrEmpty(branch) && branch != "ALL")
                    {
                        whereClause += " AND EmployeeSalaryDetails.EMPFL_BRANCHCODE = @Branch ";
                    }
                    
                    if (!string.IsNullOrEmpty(employeeType) && employeeType != "ALL")
                    {
                        whereClause += " AND Employee.EMP_ROLE = @EmployeeType ";
                    }

                    cmd.CommandText = @"SELECT
                        Employee.EMP_CODE as BeneficiaryCode,
                        EmployeeSalaryDetails.EMPFL_BK_ACCNO as BeneficiaryAccountNumber,
                        Employee.EMP_NAME as BeneficiaryName,
                        PaySlip.BasicSalary + PaySlip.OverTimeSalary + PaySlip.OffDaySalary + PaySlip.OffDayOverTimeSalary +
                         PaySlip.HolidaySalary + PaySlip.HolidayOverTimeSalary + PaySlip.Shift2Salary +
                         PaySlip.AttendanceAllowance + PaySlip.ReAllowance + PaySlip.MiscAmount -
                         (PaySlip.EPFDeductionAmount + PaySlip.SOCSODeductionAmount + PaySlip.DailyAdvanceRecovery +
                          PaySlip.MonthlyAdvanceRecovery + PaySlip.UniformIssueRecovery + PaySlip.LoanRecovery + PaySlip.MiscDeduction) as TransactionAmount,
                        Employee.EMP_ID as CustomerReferenceNumber,
                        EmployeeSalaryDetails.EMPFL_BRANCHCODE as PayerAddress1,
                        Employee.IFSC_Code as IFSCCode,
                        ISNULL(Employee.BankName, BankMaster.Name) as BeneficiaryBankName,
                        ISNULL(Employee.BankBranch, '') as BeneficiaryBankBranchName,
                        ISNULL(BankMaster.Email, '') as BeneficiaryEmailId,
                        ISNULL(BankMaster.BankAccount, '') as PayerAccountNo,
                        ISNULL(BankMaster.Name, '') as PayerName,
                        ISNULL(BankMaster.Address1, '') as PayerAddress2,
                        ISNULL(BankMaster.Address2, '') as PayerAddress3,
                        ISNULL(BankMaster.City, '') as PayerAddress4,
                        ISNULL(BankMaster.State, '') as PayerAddress5,
                        ISNULL(BankMaster.PostCode, '') as PayerAddress6
                        FROM   (EmployeeSalaryDetails EmployeeSalaryDetails
                        INNER JOIN (PaySlip PaySlip INNER JOIN Employee Employee ON PaySlip.EmployeeID=Employee.EMP_ID)
                        ON EmployeeSalaryDetails.EMPFL_CODE=Employee.EMP_CODE)
                        INNER JOIN BranchMaster BankMaster ON Employee.EMP_BRANCH_CODE=BankMaster.Code "
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
                                    FieldType = "1",  // 1-NEFT
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
                                    ValueDate = DateTime.Now.ToString("dd-MM-yyyy"),
                                    IFSCCode = dr["IFSCCode"]?.ToString() ?? "",
                                    BeneficiaryBankName = dr["BeneficiaryBankName"]?.ToString() ?? "",
                                    BeneficiaryBankBranchName = dr["BeneficiaryBankBranchName"]?.ToString() ?? "",
                                    BeneficiaryEmailId = dr["BeneficiaryEmailId"]?.ToString() ?? ""
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
    }
}
