using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using OBMS.WebAPI.Models.DTO;
using OBMS.WebAPI.Repositories.Interface;

namespace OBMS.WebAPI.BusinessObjects
{
    public class SalaryProcess : ISalaryProcess
    {
        private decimal EmployeeID = 0;
        private readonly IConfiguration _configuration;

        public SalaryProcess(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public List<KKDNExcelListDto> GetListWithBlankRow(string Branch, string EmployeeType, DateTime dtDateJoinFrom, DateTime dtDateJoinTo, string KDNVetting)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    string sQuery = string.Empty;
                    sQuery = " SELECT * FROM vwKKDN ";
                    sQuery = sQuery + " WHERE HasTransfer = 0 ";
                    int bKDNVetting = 0;
                    if (KDNVetting != "")
                    {
                        if (KDNVetting == "YES")
                        {
                            bKDNVetting = 1;
                        }
                        sQuery = sQuery + " And KDNVetting = @KDNVetting ";
                    }

                    if (Branch != "" && Branch != "0")
                    {
                        sQuery = sQuery + " And BranchCode = @Branch ";
                    }

                    //    sQuery = sQuery + " And DateJoin Between @DateJoinFrom and @DateJoinTo ";

                    sQuery = sQuery + " ORDER BY Name ";
                    cmd.CommandText = sQuery;
                    if (KDNVetting != "")
                    {
                        cmd.Parameters.AddWithValue("@KDNVetting", bKDNVetting);
                    }
                    if (Branch != "" && Branch != "0")
                    {
                        cmd.Parameters.AddWithValue("@Branch", Branch);
                    }
                    //cmd.Parameters.AddWithValue("@EmployeeType", EmployeeType);
                    //   cmd.Parameters.AddWithValue("@DateJoinFrom", dtDateJoinFrom);
                    //   cmd.Parameters.AddWithValue("@DateJoinTo", dtDateJoinTo);
                    using (SQLDataAccess sdaFactory = new SQLDataAccess(_configuration))
                    {
                        using (SqlDataReader dr = sdaFactory.RetrieveData(cmd))
                        {
                            List<KKDNExcelListDto> sKKDNExcelList = new List<KKDNExcelListDto>();

                            while (dr.Read())
                            {
                                string sNationlityValue;
                                if (dr.IsDBNull(dr.GetOrdinal("Nationality")))
                                {
                                    sNationlityValue = string.Empty;
                                }
                                else
                                {
                                    sNationlityValue = dr.GetString(dr.GetOrdinal("Nationality")).ToString();
                                }
                                string sCitizenValue;
                                if (dr.IsDBNull(dr.GetOrdinal("Citizen")))
                                {
                                    sCitizenValue = string.Empty;
                                }
                                else
                                {
                                    sCitizenValue = dr.GetString(dr.GetOrdinal("Citizen")).ToString();
                                }
                                string sEPFValue;
                                if (dr.IsDBNull(dr.GetOrdinal("EPF")))
                                {
                                    sEPFValue = string.Empty;
                                }
                                else
                                {
                                    sEPFValue = dr.GetString(dr.GetOrdinal("EPF")).ToString();
                                }
                                string sSOCSOValue;
                                if (dr.IsDBNull(dr.GetOrdinal("SOSCO")))
                                {
                                    sSOCSOValue = string.Empty;
                                }
                                else
                                {
                                    sSOCSOValue = dr.GetString(dr.GetOrdinal("SOSCO")).ToString();
                                }

                                string sKDNVetting;
                                if (dr.IsDBNull(dr.GetOrdinal("KDNVetting")))
                                {
                                    sKDNVetting = string.Empty;
                                }
                                else
                                {
                                    sKDNVetting = dr.GetBoolean(dr.GetOrdinal("KDNVetting")).ToString();
                                }

                                sKKDNExcelList.Add(
                                         new KKDNExcelListDto(
                                             dr.GetInt32(dr.GetOrdinal("EmpID")),
                                             dr.GetString(dr.GetOrdinal("BranchCode")),
                                             dr.GetString(dr.GetOrdinal("Name")),
                                             dr.GetString(dr.GetOrdinal("IC")),
                                             dr.GetDateTime(dr.GetOrdinal("DOB")),
                                             sNationlityValue.ToString(),
                                             sCitizenValue.ToString(),
                                             dr.GetString(dr.GetOrdinal("Race")),
                                             dr.GetString(dr.GetOrdinal("Gender")),
                                             dr.GetString(dr.GetOrdinal("Address")),
                                             dr.GetDateTime(dr.GetOrdinal("DateJoin")),
                                             dr.GetString(dr.GetOrdinal("JobTitle")),
                                             sEPFValue.ToString(),
                                             sSOCSOValue,
                                             Convert.ToBoolean(sKDNVetting),
                                             dr.GetBoolean(dr.GetOrdinal("HasTransfer"))
                                         )
                                     );
                            }
                            return sKKDNExcelList;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public string Process(string Branch, string EmployeeType, string Remarks, DateTime Period, bool LockProcess, string CurrentUser, string CompanyCode)
        {
            List<string> errorLog = new List<string>();
            try
            {
                // Reset EmployeeID at the beginning of each process
                EmployeeID = 0;

                //Get latest Version - amend on 4 Feb 2012
                decimal sSalaryProcessVersion = 0;
                //decimal sPaySlipVersion = 0;

                //sSalaryProcessVersion = Utility.GetPreviousSalaryProcessAuditVersion(Branch, EmployeeType, Period);
                //sPaySlipVersion = Utility.GetPreviousPayslipAuditVersion(Period);

                //End

                StringBuilder sbQuery = new StringBuilder();
                using (SQLDataAccess sdaAttendance = new SQLDataAccess(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        //cmd.CommandText = " SELECT Branch,EmployeeID FROM Attendance INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID WHERE Branch=@Branch AND EMP_ROLE = @EmployeeType AND Period=@AttendancePeriod";
                        //cmd.CommandText = " SELECT Branch,EmployeeID FROM Attendance INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID WHERE Branch=@Branch AND EMP_ROLE LIKE @EmployeeType AND Period=@AttendancePeriod and EMP_ID = '17241'";
                        cmd.CommandText = " SELECT DISTINCT Attendance.Branch, Attendance.EmployeeID FROM Attendance INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID WHERE Attendance.Branch=@Branch AND EMP_ROLE LIKE @EmployeeType AND MONTH(Attendance.Period)=MONTH(@AttendancePeriod) AND YEAR(Attendance.Period)=YEAR(@AttendancePeriod) order by Attendance.EmployeeID ";
                        cmd.Parameters.AddWithValue("@Branch", Branch);
                        cmd.Parameters.AddWithValue("@EmployeeType", "%" + EmployeeType + "%");
                        cmd.Parameters.AddWithValue("@AttendancePeriod", Period);

                        using (SqlDataReader dr = sdaAttendance.RetrieveData(cmd))
                        {
                            using (SQLDataAccess sdaPaySlip = new SQLDataAccess(_configuration))
                            {
                                SqlCommand cmdSalaryProcess = new SqlCommand();
                                SqlCommand cmdSalaryProcessAudit = new SqlCommand();
                                SQLDataAccess sdaSalaryProcess = new SQLDataAccess(_configuration);
                                //cmdSalaryProcess.CommandText = "SELECT ID,IsLocked FROM SalaryProcess WHERE Period = @Period AND Branch = @Branch AND EmployeeType = @EmployeeType";
                                cmdSalaryProcess.CommandText = "SELECT ID,IsLocked FROM SalaryProcess WHERE Period = @Period AND Branch = @Branch AND EmployeeType LIKE @EmployeeType";
                                cmdSalaryProcess.Parameters.AddWithValue("@Period", Period);
                                cmdSalaryProcess.Parameters.AddWithValue("@Branch", Branch);
                                cmdSalaryProcess.Parameters.AddWithValue("@EmployeeType", "%" + EmployeeType + "%");
                                SqlDataReader drSalaryProcess = sdaSalaryProcess.RetrieveData(cmdSalaryProcess);
                                bool bFoundRecord = false;
                                int ID = 0;
                                if (drSalaryProcess.HasRows)
                                {
                                    bFoundRecord = true;
                                    drSalaryProcess.Read();
                                    ID = drSalaryProcess.GetInt32(0);
                                    if (drSalaryProcess.GetBoolean(1))
                                        return "Salary Processing for the month is locked. It cannot be recomputed again.";
                                }
                                drSalaryProcess.Dispose();
                                cmdSalaryProcess.Dispose();
                                sdaSalaryProcess.Dispose();
                                cmdSalaryProcess = new SqlCommand();
                                if (bFoundRecord)
                                {
                                    cmdSalaryProcess.CommandText = "UPDATE SalaryProcess SET LastUpdate = @LastUpdate, LastUpdatedBy = @LastUpdatedBy ,IsLocked = @IsLocked,Remarks = @Remarks WHERE ID=@ID";
                                    cmdSalaryProcess.Parameters.AddWithValue("@ID", ID);
                                    cmdSalaryProcess.Parameters.AddWithValue("@IsLocked", LockProcess);
                                    cmdSalaryProcess.Parameters.AddWithValue("@Remarks", Remarks);
                                    cmdSalaryProcess.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                    cmdSalaryProcess.Parameters.AddWithValue("@LastUpdatedBy", CurrentUser);
                                }
                                else
                                {
                                    cmdSalaryProcess.CommandText = "INSERT INTO SalaryProcess (Branch,Period,EmployeeType,IsLocked,Remarks,LastUpdate,LastUpdatedBy) VALUES(@Branch,@Period,@EmployeeType,@IsLocked,@Remarks,@LASTUPDATE,@LastUpdatedBy)";
                                    cmdSalaryProcess.Parameters.AddWithValue("@Branch", Branch);
                                    cmdSalaryProcess.Parameters.AddWithValue("@Period", Period);
                                    cmdSalaryProcess.Parameters.AddWithValue("@EmployeeType", EmployeeType);
                                    cmdSalaryProcess.Parameters.AddWithValue("@IsLocked", LockProcess);
                                    cmdSalaryProcess.Parameters.AddWithValue("@Remarks", Remarks);
                                    cmdSalaryProcess.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                    cmdSalaryProcess.Parameters.AddWithValue("@LastUpdatedBy", CurrentUser);
                                }
                                //Insert into SalaryProcessAudit

                                cmdSalaryProcessAudit = new SqlCommand();
                                cmdSalaryProcessAudit.CommandText = "INSERT INTO SalaryProcessAudit (Branch,Period,EmployeeType,IsLocked,Remarks,LastUpdate,LastUpdatedBy,Version) VALUES(@Branch,@Period,@EmployeeType,@IsLocked,@Remarks,@LASTUPDATE,@LastUpdatedBy,@Version)";
                                cmdSalaryProcessAudit.Parameters.AddWithValue("@Branch", Branch);
                                cmdSalaryProcessAudit.Parameters.AddWithValue("@Period", Period);
                                cmdSalaryProcessAudit.Parameters.AddWithValue("@EmployeeType", EmployeeType);
                                cmdSalaryProcessAudit.Parameters.AddWithValue("@IsLocked", LockProcess);
                                cmdSalaryProcessAudit.Parameters.AddWithValue("@Remarks", Remarks);
                                cmdSalaryProcessAudit.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmdSalaryProcessAudit.Parameters.AddWithValue("@LastUpdatedBy", CurrentUser);
                                cmdSalaryProcessAudit.Parameters.AddWithValue("@Version", sSalaryProcessVersion);

                                //End Insert 
                                sdaPaySlip.StartTransaction();
                                sdaPaySlip.ExecuteSQL(cmdSalaryProcess);
                                sdaPaySlip.ExecuteSQL(cmdSalaryProcessAudit);

                                cmdSalaryProcess.Dispose();
                                cmdSalaryProcessAudit.Dispose();

                                while (dr.Read())
                                {
                                    EmployeeID = Convert.ToDecimal(dr["EmployeeID"]);
                                    SqlDataReader drEmployee = null;
                                    SQLDataAccess sdaEmployee = null;
                                    SqlDataReader drAttendance = null;
                                    SQLDataAccess sdaAttendanceList = null;
                                    SQLDataAccess sdaAdvance = null;

                                    try
                                    {
                                        using (SqlCommand cmdEmployee = new SqlCommand())
                                        {
                                            decimal dAttendanceAllowance = 0;
                                            decimal dSpecialAllowance = 0;
                                            decimal dAttendanceAllowanceDays = 0;
                                            string sAttendanceAllowanceFollowCalendar = string.Empty;
                                            decimal dReAllowance = 0;
                                            decimal dReAllowanceRate = 0;
                                            sbQuery = new StringBuilder();
                                            sbQuery.Append("SELECT EmployeeSalaryDetails.TMPGUARD, EMP_DATE_OF_BIRTH, emp_citizen, EMPPAY_DATE_JOINED, EMPPAY_DATE_RESIGNED, EMPFL_EPF8Pa, EPFDETECT, SOCSODETECT, DETECTBYND55, ");
                                            sbQuery.Append(" INCOMETAXDETECT, EmploymentDetails.EMPPAY_BASIC_RATE, EmploymentDetails.ATTENDANCEALLOWANCE AS AttendanceAllowance, EmploymentDetails.SpecialAllowance, EmploymentDetails.AttendanceAllowanceWorkingDays, ");
                                            sbQuery.Append(" EmploymentDetails.AttendanceAllowanceFollowCalendar, EmployeeSalaryDetails.PAYMODE, SalaryStructure.EmployeeNationality, SalaryStructure.WorkingHours, ");
                                            sbQuery.Append(" SalaryStructure.Name, SalaryStructure.TravelAllowance, SalaryStructure.GeneralDayRate, SalaryStructure.GeneralDayHours, ");
                                            sbQuery.Append(" SalaryStructure.WorkingDays, SalaryStructure.GeneralDayOTRate, SalaryStructure.OffDayRate, SalaryStructure.OffDayOTRate, ");
                                            sbQuery.Append(" SalaryStructure.HolidayRate, SalaryStructure.HolidayOTRate, ");
                                            sbQuery.Append(" SalaryStructure.SalaryBand, SalaryStructure.EICC, salarystructure.NonStructure, Employee.NewSalaryStructure, Employee.SalaryStructure1000_3h, Employee.EMP_PASSPORT_NO, ");
                                            sbQuery.Append(" Employee.CB_Basic, Employee.CB_DA, Employee.CB_HRA, Employee.CB_HRAPercentage, Employee.CB_OtherAllowances, Employee.CB_NH, Employee.CB_NHPercentage, Employee.CB_AdvanceStatutoryBonus, Employee.CB_SubTotal from Employee ");
                                            sbQuery.Append("LEFT JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE ");
                                            sbQuery.Append("LEFT JOIN EmployeeSalaryDetails ON EmployeeSalaryDetails.EMPFL_CODE = Employee.EMP_CODE ");
                                            sbQuery.Append("LEFT JOIN SalaryStructure ON SalaryStructure.SalaryID = EmploymentDetails.Salarylab ");
                                            sbQuery.Append("WHERE Employee.EMP_ID=@EmployeeID ");

                                            cmdEmployee.CommandText = sbQuery.ToString();
                                            cmdEmployee.Parameters.AddWithValue("@EmployeeID", EmployeeID);

                                            sdaEmployee = new SQLDataAccess(_configuration);
                                            drEmployee = sdaEmployee.RetrieveData(cmdEmployee);
                                            bool DeductEPF8Pa = false;
                                            bool DeductEPF = false;
                                            bool DeductSOCSO = false;
                                            bool DeductEPFBeyond55 = false;
                                            bool DeductIncomeTax = false; //IncomeTax
                                            decimal WorkingHours = 0;
                                            decimal GeneralDayHours = 0;
                                            int EmployeeAge = 0;
                                            int strCitizen = 0;
                                            decimal NoOfWorkingDays = 0;
                                            string SalaryPayMode = string.Empty;
                                            string EmployeeNationality = string.Empty;
                                            decimal dAttendanceDeduction = 0;
                                            decimal dSpecialAttendanceDeduction = 0;
                                            double dEmployeeBasicRate = 0;
                                            decimal dBonus = 0;
                                            int sSalaryStructure1000_3h = 0;
                                            string sEmpPassport = string.Empty;
                                            bool bNonStructure = false;
                                            bool bTmpGuard = false;
                                            // CB (Commercial Breakdown) values from Employee table - Simplified
                                            decimal dCB_Basic = 0;
                                            decimal dCB_DA = 0;
                                            decimal dCB_HRA = 0;
                                            decimal dCB_HRAPercentage = 0;
                                            decimal dCB_OtherAllowances = 0;
                                            decimal dCB_NH = 0;
                                            decimal dCB_NHPercentage = 0;
                                            decimal dCB_AdvanceStatutoryBonus = 0;
                                            decimal dCB_SubTotal = 0;

                                            if (drEmployee.Read())
                                            {
                                                // Check for missing data in joined tables
                                                if (drEmployee.IsDBNull(drEmployee.GetOrdinal("EMPPAY_DATE_JOINED")))
                                                {
                                                    throw new Exception("Missing data: EmploymentDetails record not found for this employee.");
                                                }
                                                if (drEmployee.IsDBNull(drEmployee.GetOrdinal("EPFDETECT")))
                                                {
                                                    throw new Exception("Missing data: EmployeeSalaryDetails record not found for this employee.");
                                                }
                                                if (drEmployee.IsDBNull(drEmployee.GetOrdinal("SalaryBand")))
                                                {
                                                    throw new Exception("Missing data: SalaryStructure record not found or not linked in EmploymentDetails.");
                                                }

                                                DateTime dob = drEmployee.IsDBNull(drEmployee.GetOrdinal("EMP_DATE_OF_BIRTH"))
                                                    ? new DateTime(1990, 1, 1)
                                                    : drEmployee.GetDateTime(drEmployee.GetOrdinal("EMP_DATE_OF_BIRTH"));
                                                EmployeeAge = Period.Year - dob.Year;
                                                if (Period.DayOfYear < dob.DayOfYear)
                                                {
                                                    EmployeeAge = EmployeeAge - 1;
                                                }
                                                WorkingHours = drEmployee.IsDBNull(drEmployee.GetOrdinal("WorkingHours")) ? 8 : drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingHours"));
                                                GeneralDayHours = drEmployee.IsDBNull(drEmployee.GetOrdinal("GeneralDayHours")) ? 8 : drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"));
                                                DeductEPF = drEmployee.IsDBNull(drEmployee.GetOrdinal("EPFDETECT")) ? false : drEmployee.GetBoolean(drEmployee.GetOrdinal("EPFDETECT"));
                                                DeductEPF8Pa = drEmployee.IsDBNull(drEmployee.GetOrdinal("EMPFL_EPF8Pa")) ? false : drEmployee.GetBoolean(drEmployee.GetOrdinal("EMPFL_EPF8Pa"));
                                                DeductSOCSO = drEmployee.IsDBNull(drEmployee.GetOrdinal("SOCSODETECT")) ? false : drEmployee.GetBoolean(drEmployee.GetOrdinal("SOCSODETECT"));
                                                DeductEPFBeyond55 = drEmployee.IsDBNull(drEmployee.GetOrdinal("DETECTBYND55")) ? false : drEmployee.GetBoolean(drEmployee.GetOrdinal("DETECTBYND55"));
                                                DeductIncomeTax = drEmployee.IsDBNull(drEmployee.GetOrdinal("INCOMETAXDETECT")) ? false : drEmployee.GetBoolean(drEmployee.GetOrdinal("INCOMETAXDETECT"));
                                                NoOfWorkingDays = drEmployee.IsDBNull(drEmployee.GetOrdinal("WorkingDays")) ? 26 : drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                dAttendanceAllowance = drEmployee.IsDBNull(drEmployee.GetOrdinal("AttendanceAllowance")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("AttendanceAllowance"));
                                                dSpecialAllowance = drEmployee.IsDBNull(drEmployee.GetOrdinal("SpecialAllowance")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("SpecialAllowance"));
                                                SalaryPayMode = drEmployee.IsDBNull(drEmployee.GetOrdinal("PAYMODE")) ? string.Empty : drEmployee.GetString(drEmployee.GetOrdinal("PAYMODE"));
                                                EmployeeNationality = drEmployee.IsDBNull(drEmployee.GetOrdinal("EmployeeNationality")) ? string.Empty : drEmployee.GetString(drEmployee.GetOrdinal("EmployeeNationality"));
                                                sAttendanceAllowanceFollowCalendar = drEmployee.IsDBNull(drEmployee.GetOrdinal("AttendanceAllowanceFollowCalendar")) ? "N" : drEmployee.GetString(drEmployee.GetOrdinal("AttendanceAllowanceFollowCalendar"));
                                                dAttendanceAllowanceDays = drEmployee.IsDBNull(drEmployee.GetOrdinal("AttendanceAllowanceWorkingDays")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("AttendanceAllowanceWorkingDays"));
                                                dEmployeeBasicRate = drEmployee.IsDBNull(drEmployee.GetOrdinal("EMPPAY_BASIC_RATE")) ? 0 : drEmployee.GetDouble(drEmployee.GetOrdinal("EMPPAY_BASIC_RATE"));
                                                strCitizen = drEmployee.IsDBNull(drEmployee.GetOrdinal("emp_citizen")) ? 0 : drEmployee.GetInt32(drEmployee.GetOrdinal("emp_citizen"));
                                                sEmpPassport = drEmployee.IsDBNull(drEmployee.GetOrdinal("EMP_PASSPORT_NO")) ? string.Empty : drEmployee.GetString(drEmployee.GetOrdinal("EMP_PASSPORT_NO"));
                                                bNonStructure = drEmployee.IsDBNull(drEmployee.GetOrdinal("NonStructure")) ? false : drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure"));
                                                bTmpGuard = drEmployee.IsDBNull(drEmployee.GetOrdinal("TMPGUARD")) ? false : drEmployee.GetBoolean(drEmployee.GetOrdinal("TMPGUARD"));
                                                // Read CB (Commercial Breakdown) values if available - Simplified
                                                dCB_Basic = drEmployee.IsDBNull(drEmployee.GetOrdinal("CB_Basic")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("CB_Basic"));
                                                dCB_DA = drEmployee.IsDBNull(drEmployee.GetOrdinal("CB_DA")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("CB_DA"));
                                                dCB_HRA = drEmployee.IsDBNull(drEmployee.GetOrdinal("CB_HRA")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("CB_HRA"));
                                                dCB_HRAPercentage = drEmployee.IsDBNull(drEmployee.GetOrdinal("CB_HRAPercentage")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("CB_HRAPercentage"));
                                                dCB_OtherAllowances = drEmployee.IsDBNull(drEmployee.GetOrdinal("CB_OtherAllowances")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("CB_OtherAllowances"));
                                                dCB_NH = drEmployee.IsDBNull(drEmployee.GetOrdinal("CB_NH")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("CB_NH"));
                                                dCB_NHPercentage = drEmployee.IsDBNull(drEmployee.GetOrdinal("CB_NHPercentage")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("CB_NHPercentage"));
                                                dCB_AdvanceStatutoryBonus = drEmployee.IsDBNull(drEmployee.GetOrdinal("CB_AdvanceStatutoryBonus")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("CB_AdvanceStatutoryBonus"));
                                                dCB_SubTotal = drEmployee.IsDBNull(drEmployee.GetOrdinal("CB_SubTotal")) ? 0 : drEmployee.GetDecimal(drEmployee.GetOrdinal("CB_SubTotal"));
                                                if (drEmployee["SalaryStructure1000_3h"] == DBNull.Value)
                                                {
                                                    sSalaryStructure1000_3h = 0;
                                                }
                                                else
                                                {
                                                    if (drEmployee.GetString(drEmployee.GetOrdinal("SalaryStructure1000_3h")) == "Y" || drEmployee.GetString(drEmployee.GetOrdinal("SalaryStructure1000_3h")) == "N")
                                                    {
                                                        sSalaryStructure1000_3h = 0;
                                                    }
                                                    else
                                                    {
                                                        sSalaryStructure1000_3h = Convert.ToInt32(drEmployee.GetString(drEmployee.GetOrdinal("SalaryStructure1000_3h")));
                                                    }
                                                }
                                                if (drEmployee["NewSalaryStructure"] == DBNull.Value)
                                                {
                                                    if (dEmployeeBasicRate > 0)
                                                    {
                                                        decimal workingDays = drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                        if (workingDays != 0)
                                                        {
                                                            dReAllowance = Math.Round((((decimal)drEmployee.GetDouble(drEmployee.GetOrdinal("EMPPAY_BASIC_RATE")) - drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / workingDays), 2);
                                                            dReAllowanceRate = Math.Round((((decimal)drEmployee.GetDouble(drEmployee.GetOrdinal("EMPPAY_BASIC_RATE")) - drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / workingDays), 2);
                                                        }
                                                        else
                                                        {
                                                            dReAllowance = 0;
                                                            dReAllowanceRate = 0;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (drEmployee.GetString(drEmployee.GetOrdinal("NewSalaryStructure")) == "N")
                                                    {
                                                        if (dEmployeeBasicRate > 0)
                                                        {
                                                            decimal workingDays = drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                            if (workingDays != 0)
                                                            {
                                                                dReAllowance = Math.Round((((decimal)drEmployee.GetDouble(drEmployee.GetOrdinal("EMPPAY_BASIC_RATE")) - drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / workingDays), 2);
                                                                dReAllowanceRate = Math.Round((((decimal)drEmployee.GetDouble(drEmployee.GetOrdinal("EMPPAY_BASIC_RATE")) - drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / workingDays), 2);
                                                            }
                                                            else
                                                            {
                                                                dReAllowance = 0;
                                                                dReAllowanceRate = 0;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        dReAllowance = 0;
                                                        dReAllowanceRate = 0;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                throw new Exception("No employee details found.");
                                            }

                                            //Allowance n Special Allowance
                                            sbQuery = new StringBuilder();
                                            sbQuery.Append("SELECT AllowanceDeduction,SpecialAllowanceDeduction FROM Attendance WHERE Attendance.EmployeeID=@EmployeeID AND  Month(Attendance.Period)=@PeriodMonth AND YEAR(Attendance.Period)=@PeriodYear ");
                                            SqlCommand cmdAllowanceDeduct = new SqlCommand();
                                            cmdAllowanceDeduct.CommandText = sbQuery.ToString();
                                            cmdAllowanceDeduct.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                            cmdAllowanceDeduct.Parameters.AddWithValue("@PeriodMonth", Period.Month);
                                            cmdAllowanceDeduct.Parameters.AddWithValue("@PeriodYear", Period.Year);
                                            SQLDataAccess sdaAllowanceDeduct = new SQLDataAccess(_configuration);
                                            SqlDataReader drAllowanceDeduct = sdaAllowanceDeduct.RetrieveData(cmdAllowanceDeduct);
                                            while (drAllowanceDeduct.Read())
                                            {
                                                if (EmployeeType == "Staff")
                                                {
                                                    //Normal Allowance
                                                    dAttendanceAllowance -= drAllowanceDeduct.GetDecimal(0);
                                                    dAttendanceDeduction = drAllowanceDeduct.GetDecimal(0);

                                                    //Special Allowance deduction
                                                    dSpecialAttendanceDeduction = drAllowanceDeduct.GetDecimal(1);
                                                    dSpecialAllowance = dSpecialAllowance - dSpecialAttendanceDeduction;

                                                    //Bonus
                                                    //dBonus = drAllowanceDeduct.GetDecimal(2);
                                                    //kean hong comment
                                                }
                                                else
                                                {
                                                    //Normal Allowance
                                                    dAttendanceDeduction = drAllowanceDeduct.GetDecimal(0);
                                                    dSpecialAttendanceDeduction = drAllowanceDeduct.GetDecimal(1);

                                                    //Special Allowance deduction
                                                    dSpecialAllowance = dSpecialAllowance - dSpecialAttendanceDeduction;

                                                    //Bonus
                                                    //dBonus = drAllowanceDeduct.GetDecimal(2);
                                                    //kean hong comment
                                                }
                                            }
                                            cmdAllowanceDeduct.Dispose();
                                            drAllowanceDeduct.Close();
                                            drAllowanceDeduct.Dispose();
                                            sdaAllowanceDeduct.Dispose();


                                            sbQuery = new StringBuilder();

                                            decimal OTGeneralDayHours = GeneralDayHours;
                                            //if (sSalaryStructure1000_3h != 0)
                                            //{
                                            OTGeneralDayHours = OTGeneralDayHours + Convert.ToDecimal(sSalaryStructure1000_3h);
                                            //}

                                            if (EmployeeNationality == "F")
                                            {
                                                sbQuery.Append("SELECT Type,  ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN DateDiff(Day,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd + 1)>1 THEN 1 ELSE DateDiff(Day,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd + 1) END),0) AS NormalDays, ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN DateDiff(hour,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd)>" + GeneralDayHours + " THEN " + GeneralDayHours + " ELSE DateDiff(hour,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd) END),0) AS NormalHours, ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN DateDiff(hour,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd)>" + OTGeneralDayHours + " THEN DateDiff(hour,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd)-" + OTGeneralDayHours + " ELSE 0 END),0) AS OTHours ");
                                                sbQuery.Append("FROM AttendanceDetails INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID ");
                                                sbQuery.Append("WHERE Month(AttendanceDetails.AttendanceDate)=@PeriodMonth AND YEAR(AttendanceDetails.AttendanceDate)=@PeriodYear ");
                                                sbQuery.Append("AND Attendance.EmployeeID=@EmployeeID ");
                                                sbQuery.Append("AND Type in (1,3,5) GROUP BY Type ");
                                                sbQuery.Append("UNION  ");
                                                sbQuery.Append("SELECT Type,   ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN WorkingDays + 1 >1 THEN 1 ELSE WorkingDays + 1 END),0) AS NormalDays,    ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN WorkingHours>" + GeneralDayHours + " THEN " + GeneralDayHours + " ELSE WorkingHours END),0) AS NormalHours,    ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN WorkingHours>" + OTGeneralDayHours + " THEN WorkingHours-" + OTGeneralDayHours + " ELSE 0 END),0) AS OTHours    ");
                                                sbQuery.Append("FROM AttendanceDetails INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID   ");
                                                sbQuery.Append("INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE  ");
                                                sbQuery.Append("LEFT JOIN SalaryStructure ON SalaryStructure.SalaryID = EmploymentDetails.Salarylab  ");
                                                sbQuery.Append("WHERE Month(AttendanceDetails.AttendanceDate)=@PeriodMonth ");
                                                sbQuery.Append("AND YEAR(AttendanceDetails.AttendanceDate)=@PeriodYear ");
                                                sbQuery.Append("AND Attendance.EmployeeID=@EmployeeID ");
                                                sbQuery.Append("AND Type in (2,4,6,7,8,9,10,11,12,13,14,15,16,17) GROUP BY Type ");
                                            }
                                            else
                                            {
                                                sbQuery.Append("SELECT Type,  ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN DateDiff(Day,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd + 1)>1 THEN 1 ELSE DateDiff(Day,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd + 1) END),0) AS NormalDays, ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN DateDiff(hour,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd)>" + GeneralDayHours + " THEN " + GeneralDayHours + " ELSE DateDiff(hour,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd) END),0) AS NormalHours, ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN DateDiff(hour,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd)>" + OTGeneralDayHours + " THEN DateDiff(hour,AttendanceDetails.TimeStart,AttendanceDetails.TimeEnd)-" + OTGeneralDayHours + " ELSE 0 END),0) AS OTHours ");
                                                sbQuery.Append("FROM AttendanceDetails INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID ");
                                                sbQuery.Append("WHERE Month(AttendanceDetails.AttendanceDate)=@PeriodMonth AND YEAR(AttendanceDetails.AttendanceDate)=@PeriodYear ");
                                                sbQuery.Append("AND Attendance.EmployeeID=@EmployeeID ");
                                                sbQuery.Append("AND Type in (1,3,5) GROUP BY Type ");
                                                sbQuery.Append("UNION  ");
                                                sbQuery.Append("SELECT Type,   ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN WorkingDays + 1 >1 THEN 1 ELSE WorkingDays + 1 END),0) AS NormalDays,    ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN WorkingHours>" + GeneralDayHours + " THEN " + GeneralDayHours + " ELSE WorkingHours END),0) AS NormalHours,    ");
                                                sbQuery.Append("ISNULL(SUM(CASE WHEN WorkingHours>" + OTGeneralDayHours + " THEN WorkingHours-" + OTGeneralDayHours + " ELSE 0 END),0) AS OTHours    ");
                                                sbQuery.Append("FROM AttendanceDetails INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID   ");
                                                sbQuery.Append("INNER JOIN Employee ON Employee.EMP_ID = Attendance.EmployeeID INNER JOIN EmploymentDetails ON EmploymentDetails.EMPPAY_CODE = Employee.EMP_CODE  ");
                                                sbQuery.Append("LEFT JOIN SalaryStructure ON SalaryStructure.SalaryID = EmploymentDetails.Salarylab  ");
                                                sbQuery.Append("WHERE Month(AttendanceDetails.AttendanceDate)=@PeriodMonth ");
                                                sbQuery.Append("AND YEAR(AttendanceDetails.AttendanceDate)=@PeriodYear ");
                                                sbQuery.Append("AND Attendance.EmployeeID=@EmployeeID ");
                                                sbQuery.Append("AND Type in (2,4,6,7,8,9,10,11,12,13,14,15,16,17) GROUP BY Type ");
                                            }

                                            SqlCommand cmdAttendance = new SqlCommand();
                                            cmdAttendance.CommandText = sbQuery.ToString();
                                            cmdAttendance.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                            cmdAttendance.Parameters.AddWithValue("@PeriodMonth", Period.Month);
                                            cmdAttendance.Parameters.AddWithValue("@PeriodYear", Period.Year);

                                            decimal BasicSalaryDays = 0;
                                            decimal WorkingDay = 0;
                                            decimal AllowanceDays = 0;
                                            decimal BasicSalaryRate = 0;
                                            decimal BasicSalary = 0;
                                            decimal dLumpSum_NonStructure = 0;
                                            decimal dSalaryBand = 0;
                                            decimal dTransferBasicBalance = 0;
                                            decimal OverTimeSalaryHours = 0;
                                            decimal OverTimeSalaryRate = 0;
                                            decimal OverTimeSalary = 0;
                                            decimal OffdaySalaryDays = 0;
                                            decimal OffdaySalaryRate = 0;
                                            decimal OffdaySalary = 0;
                                            decimal OffdayOTSalaryHours = 0;
                                            decimal OffdayOTSalaryRate = 0;
                                            decimal OffdayOTSalary = 0;
                                            decimal HolidayDays = 0;
                                            decimal HolidaySalaryDays = 0;
                                            decimal HolidaySalaryRate = 0;
                                            decimal HolidaySalaryRateForShift2 = 0;
                                            decimal HolidaySalary = 0;
                                            decimal HolidayOTSalaryHours = 0;
                                            decimal HolidayOTSalaryRate = 0;
                                            decimal HolidayOTSalary = 0;
                                            decimal UnPaidLeaveDay = 0;
                                            decimal AnnualLeaveDay = 0;
                                            decimal AbsentDay = 0;
                                            bool MPMbool = false;
                                            bool Normalbool = false;
                                            decimal UnPaidLeaveDayActual = 0;
                                            decimal AnnualLeaveDayActual = 0;
                                            decimal AbsentDayActual = 0;

                                            decimal MedicalLeaveDay = 0;
                                            decimal MedicalLeaveDayActual = 0;
                                            decimal MaternityLeaveDay = 0;
                                            decimal MaternityLeaveDayActual = 0;
                                            decimal PaternityLeaveDay = 0;
                                            decimal PaternityLeaveDayActual = 0;
                                            decimal HospitalizationLeaveDay = 0;
                                            decimal HospitalizationLeaveDayActual = 0;
                                            decimal SocsoDay = 0;
                                            decimal SocsoDayActual = 0;
                                            decimal HolidayDayActual = 0;
                                            decimal OnSEMIDeduction = 0;
                                            decimal AdditionalOT = 0;
                                            sdaAttendanceList = new SQLDataAccess(_configuration);
                                            drAttendance = sdaAttendanceList.RetrieveData(cmdAttendance);
                                            while (drAttendance.Read())
                                            {
                                                ////////if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                ////////{
                                                ////////    BasicSalary = (decimal)drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"));
                                                ////////    BasicSalaryDays = (decimal)drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                ////////    OverTimeSalary = (decimal)drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate"));
                                                ////////}
                                                if (EmployeeType == "Staff")
                                                {
                                                    if (drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) > 0)
                                                    {
                                                        switch (drAttendance.GetInt32(drAttendance.GetOrdinal("Type")))
                                                        {
                                                            case 1: //General Working
                                                                Normalbool = true;
                                                                if (drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) >= 26)
                                                                {
                                                                    BasicSalary = (decimal)drEmployee.GetDouble(drEmployee.GetOrdinal("EMPPAY_BASIC_RATE"));
                                                                    // Use actual NormalDays instead of WorkingDays to get exact day count
                                                                    BasicSalaryDays = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                    AllowanceDays = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                }
                                                                else
                                                                {
                                                                    if (Period.Month == 2 && drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) >= 24) //NormalHours --> NormalDays
                                                                    {
                                                                        BasicSalary = (decimal)drEmployee.GetDouble(drEmployee.GetOrdinal("EMPPAY_BASIC_RATE"));
                                                                        // Use actual NormalDays instead of WorkingDays to get exact day count
                                                                        BasicSalaryDays = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        AllowanceDays = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                    }
                                                                    else
                                                                    {
                                                                        BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")));
                                                                        BasicSalaryDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        AllowanceDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        Normalbool = false;
                                                                    }
                                                                }
                                                                BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                break;

                                                            case 2: // Off Day (rest day, not worked — counted for BasicSalaryDays)
                                                                // Track off days in AllowanceDays only — NOT in BasicSalaryDays
                                                                // BasicSalaryDays will add them back after hours-based recalculation
                                                                AllowanceDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                break;

                                                            case 3: // off day working
                                                                OffdaySalary = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"))) + (decimal)0.00000001, 2);
                                                                OffdaySalaryDays = Math.Round(OffdaySalary / Math.Round((drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")))), 2), 0);
                                                                OffdaySalaryRate = Math.Round((drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")))), 2);
                                                                OffdayOTSalary = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                OffdayOTSalaryHours = drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                OffdayOTSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                break;
                                                            case 4://Holiday
                                                                HolidayDays = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")));
                                                                HolidayDayActual = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                MPMbool = true;
                                                                break;
                                                            case 5: //Holiday Working
                                                                    //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                    //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    //AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                HolidaySalary = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"))) + (decimal)0.00000001, 2);
                                                                HolidaySalaryDays = Math.Round(HolidaySalary / Math.Round((drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")))), 2), 0);
                                                                HolidaySalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"))), 2);
                                                                HolidayOTSalary = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                HolidayOTSalaryHours = drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                HolidayOTSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                MPMbool = true;
                                                                break;
                                                            case 6: //UnPaid Leave
                                                                UnPaidLeaveDay = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")));
                                                                UnPaidLeaveDayActual = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                MPMbool = true;
                                                                break;

                                                            case 7: //Absent
                                                                AbsentDay = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")));
                                                                AbsentDayActual = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                MPMbool = true;
                                                                break;

                                                            case 8: //Annual Leave
                                                                AnnualLeaveDay = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")));
                                                                AnnualLeaveDayActual = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                MPMbool = true;
                                                                break;
                                                            case 9: //Medical leave
                                                                MedicalLeaveDay = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")));
                                                                MedicalLeaveDayActual = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                MPMbool = true;
                                                                break;
                                                            case 10: // Maternity Leave
                                                                MaternityLeaveDay = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")));
                                                                MaternityLeaveDayActual = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                MPMbool = true;
                                                                break;
                                                            case 11: // Paternity leave
                                                                PaternityLeaveDay = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")));
                                                                PaternityLeaveDayActual = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                MPMbool = true;
                                                                break;
                                                            case 12: // Hospitalization leave
                                                                HospitalizationLeaveDay = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")));
                                                                HospitalizationLeaveDayActual = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                MPMbool = true;
                                                                break;
                                                            case 13: // Paid By Socso
                                                                SocsoDay = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")));
                                                                SocsoDayActual = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                MPMbool = true;
                                                                break;
                                                        }
                                                    }
                                                }

                                                else //GUARD
                                                {
                                                    Boolean bIncompleteMonth = false;


                                                    if (drEmployee["EMPPAY_DATE_RESIGNED"] != DBNull.Value)
                                                    {
                                                        DateTime dtResignedDate = drEmployee.GetDateTime(drEmployee.GetOrdinal("EMPPAY_DATE_RESIGNED"));
                                                        if ((dtResignedDate.Month == Period.Month) && (dtResignedDate.Year == Period.Year) && (dtResignedDate.Day != Period.Day))
                                                        {
                                                            bIncompleteMonth = true;
                                                        }
                                                    }

                                                    if (drEmployee["EMPPAY_DATE_JOINED"] != DBNull.Value)
                                                    {
                                                        DateTime dtJoinedDate = drEmployee.GetDateTime(drEmployee.GetOrdinal("EMPPAY_DATE_JOINED"));
                                                        if ((dtJoinedDate.Month == Period.Month) && (dtJoinedDate.Year == Period.Year) && dtJoinedDate.Day != 1)
                                                        {
                                                            bIncompleteMonth = true;
                                                        }
                                                    }



                                                    sbQuery = new StringBuilder();
                                                    sbQuery.Append("SELECT Type,AttendanceDate,  ");
                                                    sbQuery.Append("ISNULL(SUM(CASE WHEN DateDiff(Day,TimeStart,TimeEnd + 1)>1 THEN 1 ELSE DateDiff(Day,TimeStart,TimeEnd + 1) END),0) AS NormalDays, ");
                                                    sbQuery.Append("ISNULL(SUM(CASE WHEN DateDiff(hour,TimeStart,TimeEnd)>" + GeneralDayHours + " THEN " + GeneralDayHours + " ELSE DateDiff(hour,TimeStart,TimeEnd) END),0) AS NormalHours, ");
                                                    sbQuery.Append("ISNULL(SUM(CASE WHEN DateDiff(hour,TimeStart,TimeEnd)>" + OTGeneralDayHours + " THEN DateDiff(hour,TimeStart,TimeEnd)-" + OTGeneralDayHours + " ELSE 0 END),0) AS OTHours ");
                                                    sbQuery.Append("FROM AttendanceDetails INNER JOIN Attendance ON Attendance.ID = AttendanceDetails.AttendanceID ");
                                                    sbQuery.Append("WHERE Month(AttendanceDetails.AttendanceDate)=@PeriodMonth AND YEAR(AttendanceDetails.AttendanceDate)=@PeriodYear ");
                                                    sbQuery.Append("AND Attendance.EmployeeID=@EmployeeID ");
                                                    if (CompanyCode.ToString().Trim() == "EASTWEST")
                                                    {
                                                        sbQuery.Append("AND Type in ('1') GROUP BY Type,AttendanceDate ");
                                                    }
                                                    else if (CompanyCode.ToString().Trim() == "FWG" && Branch == "PF059")
                                                    {
                                                        sbQuery.Append("AND Type in ('1','3','5') GROUP BY Type,AttendanceDate ");
                                                    }
                                                    else
                                                    {
                                                        sbQuery.Append(" GROUP BY Type,AttendanceDate ");
                                                    }
                                                    SqlCommand cmdAttendanceDetails = new SqlCommand();
                                                    cmdAttendanceDetails.CommandText = sbQuery.ToString();
                                                    cmdAttendanceDetails.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                                    cmdAttendanceDetails.Parameters.AddWithValue("@PeriodMonth", Period.Month);
                                                    cmdAttendanceDetails.Parameters.AddWithValue("@PeriodYear", Period.Year);

                                                    SQLDataAccess sdaAttendanceDetailsList = new SQLDataAccess(_configuration);
                                                    SqlDataReader drAttendanceDetails = sdaAttendanceDetailsList.RetrieveData(cmdAttendanceDetails);
                                                    decimal count1WeekTotal = 0;
                                                    decimal count2WeekTotal = 0;
                                                    decimal count3WeekTotal = 0;
                                                    decimal count4WeekTotal = 0;
                                                    decimal count5WeekTotal = 0;

                                                    while (drAttendanceDetails.Read())
                                                    {
                                                        DateTime GetDate = drAttendanceDetails.GetDateTime(drAttendanceDetails.GetOrdinal("AttendanceDate"));
                                                        if (GetDate.Day == 1 || GetDate.Day == 2 || GetDate.Day == 3 || GetDate.Day == 4 || GetDate.Day == 5 || GetDate.Day == 6 || GetDate.Day == 7)
                                                        {
                                                            //count2WeekTotal += (decimal)drAttendanceDetails.GetDecimal(drAttendanceDetails.GetOrdinal("NormalHours"));
                                                            count2WeekTotal += 8;
                                                        }
                                                        if (GetDate.Day == 8 || GetDate.Day == 9 || GetDate.Day == 10 || GetDate.Day == 11 || GetDate.Day == 12 || GetDate.Day == 13 || GetDate.Day == 14)
                                                        {
                                                            count3WeekTotal += 8;
                                                        }
                                                        if (GetDate.Day == 15 || GetDate.Day == 16 || GetDate.Day == 17 || GetDate.Day == 18 || GetDate.Day == 19 || GetDate.Day == 20 || GetDate.Day == 21)
                                                        {
                                                            count4WeekTotal += 8;
                                                        }
                                                        if (GetDate.Day == 22 || GetDate.Day == 23 || GetDate.Day == 24 || GetDate.Day == 25 || GetDate.Day == 26 || GetDate.Day == 27 || GetDate.Day == 28)
                                                        {
                                                            count5WeekTotal += 8;
                                                        }
                                                    }

                                                    drAttendanceDetails.Close();
                                                    drAttendanceDetails.Dispose();
                                                    cmdAttendanceDetails.Dispose();
                                                    sdaAttendanceDetailsList.Dispose();
                                                    //removed east west as per request On 6/6/2024
                                                    //if (CompanyCode.ToString().Trim() == "EASTWEST" || (CompanyCode.ToString().Trim() == "FWG" && Branch == "PF059"))
                                                    if (CompanyCode.ToString().Trim() == "FWG" && Branch == "PF059")
                                                    {
                                                        if (EmployeeNationality == "L" && bTmpGuard == false)
                                                        {
                                                            if (count1WeekTotal > 45)
                                                            {
                                                                //AdditionalOT = AdditionalOT + (count1WeekTotal - 45);
                                                                AdditionalOT = AdditionalOT + 3;
                                                            }
                                                            if (count2WeekTotal > 45)
                                                            {
                                                                AdditionalOT = AdditionalOT + 3;
                                                            }
                                                            if (count3WeekTotal > 45)
                                                            {
                                                                AdditionalOT = AdditionalOT + 3;
                                                            }
                                                            if (count4WeekTotal > 45)
                                                            {
                                                                AdditionalOT = AdditionalOT + 3;
                                                            }
                                                            if (count5WeekTotal > 45)
                                                            {
                                                                AdditionalOT = AdditionalOT + 3;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            AdditionalOT = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        AdditionalOT = 0;
                                                    }
                                                    if (drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) > 0)
                                                    {
                                                        switch (drAttendance.GetInt32(drAttendance.GetOrdinal("Type")))
                                                        {
                                                            case 1://General Working
                                                                if (EmployeeID == 24524)
                                                                {
                                                                    //decimal NormalHour = drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"));
                                                                    //decimal DayRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    //int dayhours = drEmployee.GetInt32(drEmployee.GetOrdinal("GeneralDayHours"));
                                                                    int DAY = 21;
                                                                }
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                {
                                                                    BasicSalary += Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"))), 2);
                                                                    BasicSalaryDays += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                    AllowanceDays += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                    OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                    OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                }
                                                                else
                                                                {
                                                                    if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == true)
                                                                    {
                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        //OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                        //OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 176)
                                                                        {
                                                                            //amend on 6 Apr 2022 to follow the normal working day instead of fix 22 days
                                                                            //amend on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC
                                                                            bIncompleteMonth = false;
                                                                            //BasicSalary += 22 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            //BasicSalaryDays += 22;
                                                                            BasicSalary += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryDays += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));

                                                                            //AllowanceDays += 22;

                                                                            //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("Normaldays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            //BasicSalaryDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("Normaldays"));
                                                                            AllowanceDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("Normaldays"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                            OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                            OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        }
                                                                        else
                                                                        {

                                                                            if (Period.Month == 2)
                                                                            {

                                                                                if (bIncompleteMonth == true)
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                    OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                                    OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                }
                                                                                else
                                                                                {
                                                                                    //amend on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC
                                                                                    //BasicSalary += 22 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    //BasicSalaryDays += 22;
                                                                                    BasicSalary += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryDays += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                                    //AllowanceDays += 22;

                                                                                    //BasicSalary += drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                    OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                                    OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);

                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (bIncompleteMonth == true)
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                    OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                                    OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                }
                                                                                else
                                                                                {
                                                                                    //amend on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC
                                                                                    //BasicSalary += 22 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    //BasicSalaryDays += 22;
                                                                                    BasicSalary += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryDays += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                                    //AllowanceDays += 22;
                                                                                    //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("Normaldays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    //BasicSalaryDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("Normaldays"));
                                                                                    AllowanceDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("Normaldays"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                    OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                                    OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                }
                                                                            }


                                                                        }

                                                                    }
                                                                    else
                                                                    {
                                                                        //1 MAR 2020 RBA
                                                                        if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("RBA"))
                                                                        {
                                                                            int MonthInDay = DateTime.DaysInMonth(Period.Year, Period.Month);
                                                                            if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 312)
                                                                            {
                                                                                BasicSalary = drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"));
                                                                                BasicSalaryDays = drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                                AllowanceDays = drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                                BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                //OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                //OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                                //OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                            }

                                                                        }
                                                                        //if (Branch.Substring(0,2) == "SW" && Period.Month == 2)

                                                                        if (Period.Month == 2)
                                                                        {
                                                                            if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                            {
                                                                                BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                WorkingDay = WorkingDay + BasicSalaryDays;
                                                                            }
                                                                            else if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                            {
                                                                                //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));


                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {
                                                                                        BasicSalaryDays += drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                                        BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25;
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (BasicSalaryRate / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += BasicSalaryRate * drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));


                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        BasicSalaryDays += drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                                        BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                                        BasicSalary += BasicSalaryRate * drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    //BasicSalary = 24 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")); ;
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                }

                                                                                AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));

                                                                                OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) + AdditionalOT;
                                                                                OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                //amend on Additional OT - previous additional OT calculation need to change and follow the request by Jaya on 27 Sept 2023 (6th day work is 5 normal working hours + 5 hours OT) 
                                                                                //East West follow old OT Calculation
                                                                                if (CompanyCode.ToString().Trim() == "EASTWEST")
                                                                                {
                                                                                    if (OverTimeSalaryRate != 0)
                                                                                    {
                                                                                        OverTimeSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"))) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                        OverTimeSalary = OverTimeSalary + ((OverTimeSalaryRate - (BasicSalaryRate / 8)) * AdditionalOT);
                                                                                    }
                                                                                }
                                                                                else //if (CompanyCode.ToString().Trim() == "FWG" && Branch == "PF059")
                                                                                {
                                                                                    OverTimeSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) + AdditionalOT) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                }

                                                                            }
                                                                            else
                                                                            {

                                                                                BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {
                                                                                        BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25;
                                                                                        BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (BasicSalaryRate / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        if (BasicSalaryDays == 25)
                                                                                        {
                                                                                            BasicSalary = drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"));
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                                        BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (BasicSalaryRate / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        if (BasicSalaryDays == 24)
                                                                                        {
                                                                                            BasicSalary = drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"));
                                                                                        }
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                }


                                                                                OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) + AdditionalOT;
                                                                                OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                //amend on Additional OT - previous additional OT calculation need to change and follow the request by Jaya on 27 Sept 2023 (6th day work is 5 normal working hours + 5 hours OT) 

                                                                                if (CompanyCode.ToString().Trim() == "EASTWEST")
                                                                                {
                                                                                    if (OverTimeSalaryRate != 0)
                                                                                    {
                                                                                        OverTimeSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"))) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                        OverTimeSalary = OverTimeSalary + ((OverTimeSalaryRate - (BasicSalaryRate / 8)) * AdditionalOT);
                                                                                    }
                                                                                }
                                                                                else //if (CompanyCode.ToString().Trim() == "FWG" && Branch == "PF059")
                                                                                {
                                                                                    OverTimeSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) + AdditionalOT) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                }
                                                                            }
                                                                            //BasicSalary += 22 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            //BasicSalaryDays += 22;
                                                                            //AllowanceDays += 22;
                                                                            //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            //OverTimeSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                            //OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                            //OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        }
                                                                        else
                                                                        {
                                                                            if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                            {
                                                                                BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                WorkingDay = WorkingDay + BasicSalaryDays;
                                                                            }
                                                                            else
                                                                            {
                                                                                BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                // Use NormalDays (actual days attended) instead of hours-based formula to avoid decimal days like 27.63
                                                                                BasicSalaryDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                                AllowanceDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                                BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));


                                                                                OverTimeSalaryHours += drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) + AdditionalOT;
                                                                                //OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                OverTimeSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                //amend on Additional OT - previous additional OT calculation need to change and follow the request by Jaya on 27 Sept 2023 (6th day work is 5 normal working hours + 5 hours OT) 
                                                                                //
                                                                                if (CompanyCode.ToString().Trim() == "EASTWEST")
                                                                                {
                                                                                    if (OverTimeSalaryRate != 0)
                                                                                    {
                                                                                        OverTimeSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"))) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                        OverTimeSalary = OverTimeSalary + ((OverTimeSalaryRate - (BasicSalaryRate / 8)) * AdditionalOT);
                                                                                    }
                                                                                }
                                                                                else //if (CompanyCode.ToString().Trim() == "FWG" && Branch == "PF059")
                                                                                {
                                                                                    OverTimeSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) + AdditionalOT) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                                }
                                                                            }

                                                                        }
                                                                    }
                                                                }

                                                                break;
                                                            case 2:// off day (week-off / rest day — no deduction, counted in BasicSalaryDays)
                                                                if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                {
                                                                    WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                }
                                                                // Off days are week-offs — add to BasicSalaryDays so no salary deduction
                                                                BasicSalaryDays += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                break;

                                                            case 3:// off day working
                                                                   //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                   //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == true)
                                                                {
                                                                    OffdaySalaryRate = Math.Round((drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26)), 2);
                                                                    OffdaySalary = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))) + (decimal)0.00000001, 2);

                                                                    OffdayOTSalary = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                    OffdayOTSalaryHours = drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                    OffdayOTSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);

                                                                }
                                                                else if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                {
                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                }

                                                                else
                                                                {
                                                                    if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                    {
                                                                        OffdaySalaryDays = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        OffdaySalaryDays = Math.Round(OffdaySalary / Math.Round((drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26)), 2), 0);
                                                                    }

                                                                    OffdaySalary = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))) + (decimal)0.00000001, 2);
                                                                    OffdaySalaryDays = Math.Round(OffdaySalary / Math.Round((drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")))), 2), 0);
                                                                    OffdaySalaryRate = Math.Round((drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")))), 2);
                                                                    OffdayOTSalary = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                    OffdayOTSalaryHours = drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                    OffdayOTSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("OffDayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                }
                                                                break;
                                                            case 4://Holiday
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                { }
                                                                else
                                                                {
                                                                    if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                    {
                                                                        if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        }
                                                                        else if (Period.Month == 2)
                                                                        {
                                                                            if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                            {
                                                                                //BasicSalary = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                                BasicSalaryDays = 24;
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    BasicSalary += (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary = 24 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")); ;
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //amend below on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC  
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        //BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        if (bIncompleteMonth == true)
                                                                        {
                                                                            BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                            BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        }
                                                                    }
                                                                }

                                                                break;
                                                            case 5://Holiday Working
                                                                if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == true)
                                                                {
                                                                    //HolidaySalary = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))) + (decimal)0.00000001, 2);
                                                                    //HolidaySalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22), 2);
                                                                    //HolidaySalaryDays = Math.Round(HolidaySalary / Math.Round((drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22)), 2), 0);
                                                                    HolidaySalaryDays = drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                    HolidaySalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate"));
                                                                    HolidaySalary = Math.Round(HolidaySalaryDays * HolidaySalaryRate, 2);

                                                                    HolidayOTSalary = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                    HolidayOTSalaryHours = drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                    HolidayOTSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 26 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);

                                                                    if (bIncompleteMonth == true)
                                                                    {
                                                                        BasicSalary += (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * HolidaySalaryDays);
                                                                        BasicSalaryDays = BasicSalaryDays + HolidaySalaryDays;
                                                                        AllowanceDays = AllowanceDays + HolidaySalaryDays;
                                                                        BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));

                                                                    }

                                                                    //BasicSalary += (HolidaySalaryDays * HolidaySalaryRate);
                                                                    //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    //amend below on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC  
                                                                    //BasicSalary += (HolidaySalaryDays * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"))));
                                                                    //BasicSalaryDays += HolidaySalaryDays;        
                                                                }

                                                                else if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                {
                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                }

                                                                else
                                                                {
                                                                    //if (Branch.Substring(0, 2) == "SW" && Period.Month == 2)
                                                                    //{
                                                                    //    if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                    //    {
                                                                    //        BasicSalary = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                    //        BasicSalaryDays = 24;
                                                                    //    }
                                                                    //    else
                                                                    //    {
                                                                    //        BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                    //        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    //    }

                                                                    //}
                                                                    //else
                                                                    //{
                                                                    //    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                    //    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    //}
                                                                    if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                    { }
                                                                    else
                                                                    {
                                                                        if (Branch.Substring(0, 2) == "SW" && Period.Month == 2 && EmployeeNationality == "L")
                                                                        {
                                                                            //BasicSalaryDays += 24;
                                                                            //BasicSalary += (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            if (BasicSalaryDays == 24)
                                                                            {
                                                                                BasicSalary = drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"));
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));

                                                                        }
                                                                    }
                                                                    HolidaySalary = Math.Round((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))) + (decimal)0.00000001, 2);
                                                                    if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                    {
                                                                        HolidaySalaryDays = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        HolidaySalaryDays = Math.Round(HolidaySalary / Math.Round((drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")))), 2), 0);
                                                                    }
                                                                    // AllowanceDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));

                                                                    HolidaySalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"))), 2);
                                                                    HolidayOTSalary = (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours")) * Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                    HolidayOTSalaryHours = drAttendance.GetDecimal(drAttendance.GetOrdinal("OTHours"));
                                                                    HolidayOTSalaryRate = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayOTRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                }
                                                                break;
                                                            case 6://unpaid Leave

                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                {
                                                                    OnSEMIDeduction = OnSEMIDeduction + ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")));
                                                                }

                                                                if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == true && bIncompleteMonth == false)
                                                                {

                                                                    BasicSalaryDays = BasicSalaryDays - drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                    BasicSalary = BasicSalary - (drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")));
                                                                }

                                                                //else
                                                                //{
                                                                //    if (Period.Month == 2)
                                                                //    {
                                                                //        if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                //        {
                                                                //            BasicSalaryDays = BasicSalaryDays - (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                //            if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                //            {
                                                                //                BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                //                BasicSalary = BasicSalaryDays * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24);
                                                                //            }
                                                                //            else
                                                                //            {
                                                                //                BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                //                BasicSalary = BasicSalaryDays * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                //            }
                                                                //        }
                                                                //        else
                                                                //        {
                                                                //            if (DateTime.IsLeapYear(Period.Year) == true)
                                                                //            {
                                                                //                BasicSalaryDays = BasicSalaryDays - (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                //                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                //                {
                                                                //                    BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25;
                                                                //                    BasicSalary = BasicSalary - ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25));
                                                                //                }
                                                                //                else
                                                                //                {
                                                                //                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                //                    BasicSalary = BasicSalaryDays * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                //                }
                                                                //            }
                                                                //            else
                                                                //            {
                                                                //                BasicSalaryDays = BasicSalaryDays - (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                //                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                //                {
                                                                //                    BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                //                    //BasicSalary = BasicSalaryDays * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24);
                                                                //                    BasicSalary = BasicSalary - ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24));
                                                                //                }
                                                                //                else
                                                                //                {
                                                                //                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                //                    BasicSalary = BasicSalaryDays * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                //                }
                                                                //            }
                                                                //        }


                                                                //    }
                                                                //}
                                                                break;
                                                            case 7://absent Leave
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                {
                                                                    //OnSEMIDeduction += OnSEMIDeduction + drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    OnSEMIDeduction = OnSEMIDeduction + ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")));
                                                                }

                                                                if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == true && bIncompleteMonth == false)
                                                                {

                                                                    BasicSalaryDays = BasicSalaryDays - drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                    BasicSalary = BasicSalary - (drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")));
                                                                }

                                                                //else
                                                                //{
                                                                //    if (Period.Month == 2)
                                                                //    {
                                                                //        if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                //        {
                                                                //            BasicSalaryDays = BasicSalaryDays - (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                //            if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                //            {
                                                                //                if (DateTime.IsLeapYear(Period.Year) == true)
                                                                //                {
                                                                //                    BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25;
                                                                //                    BasicSalary = BasicSalaryDays * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25);
                                                                //                }
                                                                //                else
                                                                //                {
                                                                //                    BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                //                    BasicSalary = BasicSalaryDays * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24);
                                                                //                }
                                                                //            }
                                                                //            else
                                                                //            {
                                                                //                BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                //                BasicSalary = BasicSalaryDays * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")); ;
                                                                //            }
                                                                //        }
                                                                //        else
                                                                //        {
                                                                //            BasicSalaryDays = BasicSalaryDays - (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                //            if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                //            {
                                                                //                if (DateTime.IsLeapYear(Period.Year) == true)
                                                                //                {
                                                                //                    BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25;
                                                                //                    //BasicSalary = BasicSalaryDays * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25);
                                                                //                    BasicSalary = BasicSalary - ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25));
                                                                //                }
                                                                //                else
                                                                //                {
                                                                //                    BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                //                    //BasicSalary = BasicSalaryDays * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24);
                                                                //                    BasicSalary = BasicSalary - ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * ((drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 25));
                                                                //                }
                                                                //            }
                                                                //            else
                                                                //            {
                                                                //                BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                //                BasicSalary = BasicSalaryDays * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")); ;
                                                                //            }
                                                                //        }
                                                                //    }
                                                                //}

                                                                break;
                                                            case 8://Annual Leave
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                { }
                                                                else
                                                                {
                                                                    if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                    {
                                                                        if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        }
                                                                        else if (Period.Month == 2)
                                                                        {
                                                                            if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                            {
                                                                                BasicSalaryDays = 24;
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    BasicSalary += (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary = 24 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")); ;
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));

                                                                                }
                                                                            }

                                                                        }

                                                                        else
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }
                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    }
                                                                    else
                                                                    {
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //amend below on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC  
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        //BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        if (bIncompleteMonth == true)
                                                                        {
                                                                            BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                            BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        }
                                                                    }
                                                                }

                                                                break;
                                                            case 9://Medical leave
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                { }
                                                                else
                                                                {
                                                                    if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                    {
                                                                        if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        }
                                                                        else if (Period.Month == 2)
                                                                        {
                                                                            if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                            {
                                                                                BasicSalaryDays = 24;
                                                                                //BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                                    BasicSalary = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalary = 24 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);

                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        //
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));


                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }
                                                                        //BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                        //BasicSalaryDays += ((int)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) / drEmployee.GetInt32(drEmployee.GetOrdinal("GeneralDayHours"));
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    }
                                                                    else
                                                                    {
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //amend below on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC  
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        //BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        if (BasicSalaryDays <= 0)
                                                                        {
                                                                            //BasicSalary += 22 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            //BasicSalaryDays += 22;
                                                                            BasicSalary += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryDays += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }

                                                                        if (bIncompleteMonth == true)
                                                                        {
                                                                            BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                            BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        }
                                                                    }
                                                                }

                                                                break;
                                                            case 10:// Maternity Leave
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                { }
                                                                else
                                                                {
                                                                    if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                    {
                                                                        if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        }
                                                                        else if (Period.Month == 2)
                                                                        {
                                                                            if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                            {
                                                                                //BasicSalary = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                                BasicSalaryDays = 24;
                                                                                //BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;

                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }
                                                                        //BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                        //BasicSalaryDays += ((int)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) / drEmployee.GetInt32(drEmployee.GetOrdinal("GeneralDayHours"));
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    }
                                                                    else
                                                                    {
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //amend below on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC  
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        //BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        if (BasicSalaryDays <= 0)
                                                                        {
                                                                            //BasicSalary += 22 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            //BasicSalaryDays += 22;
                                                                            BasicSalary += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryDays += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }

                                                                        if (bIncompleteMonth == true)
                                                                        {
                                                                            BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                            BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        }
                                                                    }
                                                                }

                                                                break;
                                                            case 11:// Paternity leave
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                { }
                                                                else
                                                                {
                                                                    if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                    {
                                                                        if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        }
                                                                        else if (Period.Month == 2)
                                                                        {
                                                                            if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                            {
                                                                                //BasicSalary = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                                BasicSalaryDays = 24;
                                                                                //BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }
                                                                        //BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                        //BasicSalaryDays += ((int)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) / drEmployee.GetInt32(drEmployee.GetOrdinal("GeneralDayHours"));
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    }
                                                                    else
                                                                    {
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //amend below on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC  
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        //BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        if (BasicSalaryDays <= 0)
                                                                        {
                                                                            //BasicSalary += 22 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            //BasicSalaryDays += 22;
                                                                            BasicSalary += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryDays += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }

                                                                        if (bIncompleteMonth == true)
                                                                        {
                                                                            BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                            BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        }
                                                                    }
                                                                }

                                                                break;
                                                            case 12:// Hospitalization leave
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                { }
                                                                else
                                                                {
                                                                    if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                    {
                                                                        if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        }
                                                                        else if (Period.Month == 2)
                                                                        {
                                                                            if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                            {
                                                                                //BasicSalary = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                                BasicSalaryDays = 24;
                                                                                //BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }
                                                                        //BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                        //BasicSalaryDays += ((int)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) / drEmployee.GetInt32(drEmployee.GetOrdinal("GeneralDayHours"));
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    }
                                                                    else
                                                                    {
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //amend below on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC  
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        //BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        if (BasicSalaryDays <= 0)
                                                                        {
                                                                            //BasicSalary += 22 * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            //BasicSalaryDays += 22;
                                                                            BasicSalary += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryDays += drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }
                                                                        if (bIncompleteMonth == true)
                                                                        {
                                                                            BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                            BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        }
                                                                    }
                                                                }

                                                                break;
                                                            case 14:// Non Schedule Day 
                                                                if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                {
                                                                    WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));


                                                                }
                                                                if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                {
                                                                    BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                    BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                }
                                                                else
                                                                {
                                                                    if (bIncompleteMonth == true)
                                                                    {
                                                                        BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                    }
                                                                }
                                                                break;
                                                            case 15:// Replacement Leave
                                                                if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                {
                                                                    WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));

                                                                }
                                                                if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                {
                                                                    BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                    BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                }
                                                                else
                                                                {
                                                                    if (bIncompleteMonth == true)
                                                                    {
                                                                        BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                    }
                                                                }
                                                                break;
                                                            case 16:// Compensanate Leave
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                { }
                                                                else
                                                                {
                                                                    if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                    {
                                                                        if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        }
                                                                        else if (Period.Month == 2)
                                                                        {
                                                                            if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                            {
                                                                                //BasicSalary = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                                BasicSalaryDays = 24;
                                                                                //BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }
                                                                        //BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                        //BasicSalaryDays += ((int)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) / drEmployee.GetInt32(drEmployee.GetOrdinal("GeneralDayHours"));
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    }
                                                                    else
                                                                    {
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //amend below on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC  
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        //BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));

                                                                        if (bIncompleteMonth == true)
                                                                        {
                                                                            BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                            BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        }

                                                                    }
                                                                }

                                                                break;
                                                            case 17:// Marriage Leave
                                                                if (drEmployee.GetString(drEmployee.GetOrdinal("NAME")).Contains("ONSEMI"))
                                                                { }
                                                                else
                                                                {
                                                                    if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                                    {
                                                                        if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            WorkingDay = WorkingDay + (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays"));
                                                                        }
                                                                        else if (Period.Month == 2)
                                                                        {
                                                                            if ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) >= 192)
                                                                            {
                                                                                //BasicSalary = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")));
                                                                                BasicSalaryDays = 24;
                                                                                //BasicSalaryRate = (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"))) / 24;
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (Branch.Substring(0, 2) == "SW" && EmployeeNationality == "L")
                                                                                {
                                                                                    if (DateTime.IsLeapYear(Period.Year) == true)
                                                                                    {

                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 25);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                        BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 24);
                                                                                        BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                                    BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                    BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                                }
                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            BasicSalary += (decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                            BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                            BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        }
                                                                        //BasicSalary += (decimal)(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")));
                                                                        //BasicSalaryDays += ((int)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours"))) / drEmployee.GetInt32(drEmployee.GetOrdinal("GeneralDayHours"));
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                    }
                                                                    else
                                                                    {
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand")) / 22 / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours"))), 2);
                                                                        //BasicSalaryDays += ((decimal)drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalHours")) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayHours")))) / drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));
                                                                        //amend below on 14 Aug 2023, requested by Uma from FWG to strict to 22 days for EICC  
                                                                        //BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                        //BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        //BasicSalaryRate = drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate"));

                                                                        if (bIncompleteMonth == true)
                                                                        {
                                                                            BasicSalary += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")) * drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")), 2);
                                                                            BasicSalaryDays += Math.Round(drAttendance.GetDecimal(drAttendance.GetOrdinal("NormalDays")), 2);
                                                                        }

                                                                    }
                                                                }

                                                                break;
                                                        }
                                                    }
                                                }
                                            }

                                            if (EmployeeType == "Guard" && drEmployee.GetBoolean(drEmployee.GetOrdinal("NonStructure")) == true)
                                            {
                                                if (BasicSalaryDays >= drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays")))
                                                {
                                                    dSalaryBand = drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"));
                                                    BasicSalary = drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays")) * BasicSalaryRate;
                                                    BasicSalaryDays = drEmployee.GetDecimal(drEmployee.GetOrdinal("WorkingDays"));
                                                }
                                                else
                                                {
                                                    dSalaryBand = drEmployee.GetDecimal(drEmployee.GetOrdinal("SalaryBand"));
                                                    dTransferBasicBalance = (decimal)(dSalaryBand - BasicSalary);
                                                    if (dTransferBasicBalance <= 1)
                                                    {
                                                        dTransferBasicBalance = 0;
                                                    }
                                                }

                                                if (WorkingDay >= 1)
                                                {

                                                    int days = DateTime.DaysInMonth(Period.Year, Period.Month);
                                                    int noOfAbsentDay = 0;
                                                    decimal dNonstructureDayRate = 0;
                                                    decimal dNonWorkingDeduction = 0;
                                                    decimal dLumpSumRate = 0;
                                                    noOfAbsentDay = (int)(days - WorkingDay);
                                                    dLumpSumRate = (decimal)drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate"));


                                                    dNonstructureDayRate = (decimal)((drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayOTRate")) + dSalaryBand) / 30);


                                                    if (days == 31)
                                                    {
                                                        dLumpSumRate = (decimal)dLumpSumRate + dNonstructureDayRate;
                                                    }


                                                    if (noOfAbsentDay > 0)
                                                    {
                                                        dNonWorkingDeduction = (decimal)(noOfAbsentDay * dNonstructureDayRate);
                                                        dLumpSum_NonStructure = (decimal)(dTransferBasicBalance + (dLumpSumRate - dNonWorkingDeduction));
                                                    }
                                                    else
                                                    {
                                                        dLumpSum_NonStructure = (decimal)dLumpSumRate;
                                                    }


                                                }
                                            }

                                            decimal strEmployeeID = 0;
                                            strEmployeeID = EmployeeID;
                                            decimal ReAllowanceDays = 0;
                                            decimal PaidLeave = 0;
                                            PaidLeave = MaternityLeaveDay + HospitalizationLeaveDay + SocsoDay;

                                            if (EmployeeType == "Staff")
                                            {
                                                // Off days (week-off/rest days) are tracked in AllowanceDays but NOT in BasicSalaryDays
                                                // OffDaysCount = difference between AllowanceDays and BasicSalaryDays
                                                decimal OffDaysCount = AllowanceDays - BasicSalaryDays;
                                                // BasicSalaryDays already excludes off days — use as-is for hours recalculation
                                                decimal BasicSalaryDaysWithoutOffDays = BasicSalaryDays;

                                                if (MPMbool)
                                                {
                                                    //Default basic salary Day to 26 if hours > 188(All month) or 176(Feb)
                                                    BasicSalaryDays = BasicSalaryDaysWithoutOffDays + AnnualLeaveDay + MedicalLeaveDay + PaternityLeaveDay + MaternityLeaveDay + HolidaySalaryDays + HolidayDays + UnPaidLeaveDay + AbsentDay + HospitalizationLeaveDay + SocsoDay;
                                                    AllowanceDays = BasicSalaryDays;

                                                    //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;
                                                    if (PaidLeave >= NoOfWorkingDays)
                                                    {
                                                        ReAllowanceDays = 0;
                                                        dReAllowance = 0;
                                                    }
                                                    else
                                                    {
                                                        BasicSalaryDays = BasicSalaryDays * 8;
                                                        if (BasicSalaryDays >= 188)
                                                        {
                                                            //Default Days
                                                            BasicSalaryDays = 26;
                                                            AllowanceDays = 26;

                                                            ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                            dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowance;
                                                            dReAllowance = Math.Round(dReAllowance, 1);
                                                        }
                                                        else
                                                        {
                                                            BasicSalaryDays = AllowanceDays;
                                                            //ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay - MedicalLeaveDay - PaternityLeaveDay - MaternityLeaveDay;
                                                            //dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay - PaternityLeaveDay - MaternityLeaveDay) * dReAllowance;
                                                            ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                            dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowance;
                                                            dReAllowance = Math.Round(dReAllowance, 1);
                                                        }

                                                    }

                                                    //Default basic salary Day to 26 if hours > 188(All month) or 176(Feb)
                                                    //BasicSalaryDays = BasicSalaryDays + AnnualLeaveDay + MedicalLeaveDay + PaternityLeaveDay + MaternityLeaveDay + HolidaySalaryDays + UnPaidLeaveDay + AbsentDay;
                                                    BasicSalaryDays = BasicSalaryDays * 8;

                                                    if (BasicSalaryDays >= 188)
                                                    {
                                                        //Default Days
                                                        BasicSalaryDays = 26;
                                                        AllowanceDays = 26;
                                                    }
                                                    else
                                                    {
                                                        if (Period.Month == 2 && BasicSalaryDays >= 176)
                                                        {
                                                            BasicSalaryDays = 26;
                                                            AllowanceDays = 26;
                                                            //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;
                                                        }
                                                        else
                                                        {
                                                            BasicSalaryDays = BasicSalaryDays / 8;
                                                            //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;

                                                            //cater for Month - FEB only
                                                            if (Period.Month == 2)
                                                            {
                                                                if (BasicSalaryDays >= 22)
                                                                {
                                                                    BasicSalaryDays = 26;
                                                                    AllowanceDays = 26;
                                                                    //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;
                                                                }
                                                                else
                                                                {
                                                                    if (BasicSalaryDays > 22 && BasicSalaryDays < 26)
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays + 4; //4th week
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    else if (BasicSalaryDays > 20 && BasicSalaryDays < 22)
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays + 3; // 3rd week
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    else if (BasicSalaryDays > 13 && BasicSalaryDays < 20)
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays + 2; //2nd week
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    else if (BasicSalaryDays > 6 && BasicSalaryDays < 13)
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays + 1; //1st week
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    else
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays;
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    //BasicSalaryDays = BasicSalaryDays + 4;
                                                                    //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay - SocsoDay;
                                                    BasicSalary = BasicSalaryDays * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")));
                                                }

                                                else//mpm is false //**not guard
                                                {
                                                    if (PaidLeave >= NoOfWorkingDays)
                                                    {
                                                        ReAllowanceDays = 0;
                                                        dReAllowance = 0;
                                                    }
                                                    else
                                                    {
                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowance;
                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                    }

                                                    //Default basic salary Day to 26 if hours > 188(All month) or 176(Feb)
                                                    // Exclude off days from hours-based recalculation (off days have 0 hours, not 8)
                                                    BasicSalaryDays = BasicSalaryDaysWithoutOffDays + AnnualLeaveDay + MedicalLeaveDay + PaternityLeaveDay + MaternityLeaveDay + HolidaySalaryDays + HolidayDays + UnPaidLeaveDay + AbsentDay + SocsoDay + HospitalizationLeaveDay;
                                                    BasicSalaryDays = BasicSalaryDays * 8;

                                                    if (BasicSalaryDays >= 188)
                                                    {
                                                        // Full month — use actual NormalDays from attendance (stored in BasicSalaryDaysWithoutOffDays)
                                                        BasicSalaryDays = BasicSalaryDaysWithoutOffDays;
                                                        AllowanceDays = BasicSalaryDaysWithoutOffDays;
                                                        //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;
                                                    }
                                                    else
                                                    {
                                                        if (Period.Month == 2 && BasicSalaryDays >= 176)
                                                        {
                                                            BasicSalaryDays = 26;
                                                            AllowanceDays = 26;
                                                            //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;
                                                        }
                                                        else
                                                        {
                                                            BasicSalaryDays = BasicSalaryDays / 8;
                                                            //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;

                                                            //cater for Month - FEB only
                                                            if (Period.Month == 2)
                                                            {
                                                                if (BasicSalaryDays >= 22)
                                                                {
                                                                    BasicSalaryDays = 26;
                                                                    AllowanceDays = 26;
                                                                    //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;
                                                                }
                                                                else
                                                                {
                                                                    if (BasicSalaryDays > 22 && BasicSalaryDays <= 26)
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays + 4; //4th week
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    else if (BasicSalaryDays > 20 && BasicSalaryDays <= 22)
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays + 3; // 3rd week
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    else if (BasicSalaryDays > 13 && BasicSalaryDays <= 20)
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays + 2; //2nd week
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    else if (BasicSalaryDays > 6 && BasicSalaryDays <= 13)
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays + 1; //1st week
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    else
                                                                    {
                                                                        BasicSalaryDays = BasicSalaryDays;
                                                                        AllowanceDays = BasicSalaryDays;
                                                                        ReAllowanceDays = AllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                                        dReAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dReAllowanceRate;
                                                                        dReAllowance = Math.Round(dReAllowance, 1);
                                                                    }
                                                                    //BasicSalaryDays = BasicSalaryDays + 4;
                                                                    //BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    BasicSalaryDays = BasicSalaryDays - UnPaidLeaveDay - AbsentDay - SocsoDay;
                                                    // Add back off days (week-offs) — they are not hours-based, no deduction needed
                                                    BasicSalaryDays = BasicSalaryDays + OffDaysCount;
                                                    BasicSalary = (BasicSalaryDays - OffDaysCount) * (drEmployee.GetDecimal(drEmployee.GetOrdinal("GeneralDayRate")));
                                                }
                                            }
                                            else
                                            {
                                                if (drEmployee.GetBoolean(drEmployee.GetOrdinal("EICC")) == false)
                                                {
                                                    ReAllowanceDays = AllowanceDays + OffdaySalaryDays + HolidaySalaryDays + HolidayDays;
                                                }
                                                else
                                                {
                                                    ReAllowanceDays = AllowanceDays + OffdaySalaryDays + HolidaySalaryDays + HolidayDays;
                                                    ReAllowanceDays = ReAllowanceDays - UnPaidLeaveDay - AbsentDay;
                                                }
                                                dReAllowance = (AllowanceDays + OffdaySalaryDays + HolidaySalaryDays + HolidayDays) * dReAllowance;
                                                dReAllowance = Math.Round(dReAllowance, 1);
                                            }

                                            if (EmployeeType == "Staff")
                                            {
                                                if (MPMbool)
                                                {
                                                    if (PaidLeave >= NoOfWorkingDays)
                                                    {
                                                        dAttendanceAllowance = 0;
                                                        BasicSalary = Math.Round(BasicSalary, 2);
                                                    }
                                                    else
                                                    {
                                                        if ((AllowanceDays - UnPaidLeaveDayActual - AbsentDayActual - PaternityLeaveDayActual - MaternityLeaveDayActual - SocsoDayActual - HospitalizationLeaveDayActual) < NoOfWorkingDays)
                                                        {
                                                            dAttendanceAllowance = (AllowanceDays - UnPaidLeaveDayActual - AbsentDayActual - PaternityLeaveDayActual - MaternityLeaveDayActual - SocsoDayActual - HospitalizationLeaveDayActual) * dAttendanceAllowance / NoOfWorkingDays;
                                                            dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2);
                                                        }
                                                        BasicSalary = Math.Round(BasicSalary, 2);
                                                    }
                                                }
                                                else
                                                {
                                                    if (PaidLeave >= NoOfWorkingDays)
                                                    {
                                                        dAttendanceAllowance = 0;
                                                        BasicSalary = Math.Round(BasicSalary, 2);
                                                    }
                                                    else
                                                    {
                                                        if ((AllowanceDays - UnPaidLeaveDay - AbsentDay) < NoOfWorkingDays)
                                                        {
                                                            dAttendanceAllowance = (AllowanceDays - UnPaidLeaveDay - AbsentDay) * dAttendanceAllowance / NoOfWorkingDays;
                                                            dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2);
                                                        }
                                                        BasicSalary = Math.Round(BasicSalary, 2);
                                                    }
                                                }
                                            }
                                            else//Guard
                                            {
                                                if (sAttendanceAllowanceFollowCalendar == "N" && (dAttendanceAllowanceDays == 0))
                                                {
                                                    if (PaidLeave >= NoOfWorkingDays)
                                                    {
                                                        dAttendanceAllowance = 0;
                                                        dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2) - dAttendanceDeduction;
                                                    }
                                                    else
                                                    {
                                                        dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2) - dAttendanceDeduction;
                                                    }
                                                }
                                                else
                                                {
                                                    if (sAttendanceAllowanceFollowCalendar == "Y")
                                                    {
                                                        int DaysInMonth = System.DateTime.DaysInMonth(Period.Year, Period.Month);

                                                        if (PaidLeave >= DaysInMonth)
                                                        {
                                                            dAttendanceAllowance = 0;
                                                            dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2) - dAttendanceDeduction;
                                                        }
                                                        else
                                                        {
                                                            if (AllowanceDays + OffdaySalaryDays + HolidaySalaryDays + HolidayDays < DaysInMonth)
                                                            {
                                                                dAttendanceAllowance = (AllowanceDays + OffdaySalaryDays + HolidaySalaryDays + HolidayDays) * (dAttendanceAllowance / DaysInMonth);
                                                                dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2) - dAttendanceDeduction;
                                                            }
                                                            else
                                                            {
                                                                dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2) - dAttendanceDeduction;
                                                            }
                                                        }

                                                    }
                                                    else
                                                    {
                                                        if (PaidLeave >= dAttendanceAllowanceDays)
                                                        {
                                                            dAttendanceAllowance = 0;
                                                            dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2) - dAttendanceDeduction;
                                                        }
                                                        else
                                                        {
                                                            if (AllowanceDays + OffdaySalaryDays + HolidaySalaryDays + HolidayDays < dAttendanceAllowanceDays)
                                                            {
                                                                dAttendanceAllowance = (AllowanceDays + OffdaySalaryDays + HolidaySalaryDays + HolidayDays) * (dAttendanceAllowance / dAttendanceAllowanceDays);
                                                                dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2) - dAttendanceDeduction;
                                                            }
                                                            else
                                                            {
                                                                dAttendanceAllowance = Math.Round(dAttendanceAllowance, 2) - dAttendanceDeduction;
                                                            }
                                                        }
                                                    }

                                                }


                                                if (Math.Round(BasicSalary, 2) == (decimal)(1499.99))
                                                {
                                                    BasicSalary = (decimal)(1500.00);
                                                }
                                                else
                                                {
                                                    BasicSalary = Math.Round(BasicSalary, 2, MidpointRounding.AwayFromZero);
                                                }
                                            }

                                            sbQuery = new StringBuilder();
                                            //sbQuery.Append("select attendance.shift2type,attendance.shift2rate,(case attendance.shift2type when 1 then count(attendanceid) else sum(DateDiff(hour,OTTimeStart,OTTimeEnd)) end),cast(((case attendance.shift2type when 1 then count(attendanceid) else sum(DateDiff(hour,OTTimeStart,OTTimeEnd)) end) * Shift2Rate) as NUMERIC(18,2))  ");
                                            //sbQuery.Append("from attendancedetails inner join attendance on attendance.id=AttendanceDetails.AttendanceID ");
                                            //sbQuery.Append("WHERE Month(AttendanceDetails.AttendanceDate)=@PeriodMonth AND YEAR(AttendanceDetails.AttendanceDate)=@PeriodYear AND Attendance.EmployeeID=@EmployeeID ");
                                            //sbQuery.Append("and OTTimeStart IS NOT NULL and OTTimeEnd IS NOT NULL ");
                                            //sbQuery.Append("group by attendanceid,shift2type,Shift2Rate ");

                                            sbQuery.Append("Select ");
                                            sbQuery.Append("Shift2Type,Shift2Rate,sum(NormalShift2) as NormalShift2, ");
                                            sbQuery.Append("Sum(HolidayShift2) as HolidayShift2,sum(TotalNormalAmount) as TotalNormalAmount ");
                                            sbQuery.Append("From ");
                                            sbQuery.Append("( ");
                                            sbQuery.Append("select attendance.shift2type,attendance.shift2rate, ");
                                            sbQuery.Append("count(DISTINCT CAST(AttendanceDetails.AttendanceDate as date)) as normalShift2, ");
                                            sbQuery.Append("(case attendanceDetails.Type when 5 then count(DISTINCT CAST(AttendanceDetails.AttendanceDate as date)) else 0 end) as HolidayShift2, ");
                                            sbQuery.Append("cast((count(DISTINCT CAST(AttendanceDetails.AttendanceDate as date)) * Shift2Rate) as NUMERIC(18,2)) as TotalNormalAmount ");
                                            sbQuery.Append("from attendancedetails inner join attendance on attendance.id=AttendanceDetails.AttendanceID  ");
                                            sbQuery.Append("WHERE  ");
                                            sbQuery.Append("Month(AttendanceDetails.AttendanceDate)=@PeriodMonth   ");
                                            sbQuery.Append("AND YEAR(AttendanceDetails.AttendanceDate)=@PeriodYear  ");
                                            sbQuery.Append("AND Attendance.EmployeeID=@EmployeeID  ");
                                            sbQuery.Append("and DATEPART(hour, OTTimeStart) > 0 and DATEPART(hour, OTTimeEnd) > 0  ");
                                            sbQuery.Append("group by shift2type,Shift2Rate,attendanceDetails.Type  ");
                                            sbQuery.Append(")ShiftII  ");
                                            sbQuery.Append("group by Shift2Type,Shift2Rate  ");

                                            SqlCommand cmdShift2Attendance = new SqlCommand();
                                            cmdShift2Attendance.CommandText = sbQuery.ToString();
                                            cmdShift2Attendance.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                            cmdShift2Attendance.Parameters.AddWithValue("@PeriodMonth", Period.Month);
                                            cmdShift2Attendance.Parameters.AddWithValue("@PeriodYear", Period.Year);
                                            SQLDataAccess sdaShift2AttendanceList = new SQLDataAccess(_configuration);
                                            SqlDataReader drShift2Attendance = sdaShift2AttendanceList.RetrieveData(cmdShift2Attendance);
                                            decimal Shift2type = 0;
                                            decimal Shift2rate = 0;
                                            decimal Shift2days = 0;
                                            decimal Shift2Holidays = 0;
                                            decimal Shift2Amount = 0;
                                            decimal Shift2HolidayAmount = 0;
                                            decimal Shift2NormalAmount = 0;
                                            while (drShift2Attendance.Read())
                                            {
                                                //Shift2type = drShift2Attendance.GetInt32(0);
                                                //Shift2rate = drShift2Attendance.GetDecimal(1);
                                                //Shift2days = drShift2Attendance.GetInt32(2);
                                                //Shift2Amount = drShift2Attendance.GetDecimal(3);
                                                //Shift2Amount = drShift2Attendance.GetDecimal(4); // Amount for normal days

                                                //Cater for Shift2 Holiday Rate base on Basic Salary Structure
                                                //HolidaySalaryRateForShift2 = Math.Round(drEmployee.GetDecimal(drEmployee.GetOrdinal("HolidayRate")), 2);

                                                Shift2type = drShift2Attendance.GetInt32(0);
                                                Shift2rate = drShift2Attendance.GetDecimal(1);
                                                Shift2days = drShift2Attendance.GetInt32(2);
                                                Shift2Holidays = drShift2Attendance.GetInt32(3);
                                                //Shift2HolidayAmount = Shift2Holidays * HolidaySalaryRateForShift2 * Shift2rate;
                                                //Shift2NormalAmount = (Shift2days - Shift2Holidays) * Shift2rate;
                                                Shift2NormalAmount = Shift2days * Shift2rate;
                                                Shift2Amount = Shift2NormalAmount + Shift2HolidayAmount;
                                                Shift2Amount = Math.Round(Shift2Amount, 2);


                                                //End
                                            }

                                            cmdShift2Attendance.Dispose();
                                            drShift2Attendance.Dispose();
                                            sdaShift2AttendanceList.Dispose();

                                            decimal dMiscEarnings = 0;
                                            decimal dMiscDeductions = 0;



                                            sbQuery = new StringBuilder();
                                            sbQuery.Append(" SELECT ISNULL(SUM(Amount),0) FROM MiscTrans WHERE ( Month(TransDate)= Month(@TransDate) AND Year(TransDate)= Year(@TransDate)) AND EmployeeID = @EmployeeID AND TransType = 1");
                                            SqlCommand cmdMiscEarnings = new SqlCommand();
                                            cmdMiscEarnings.CommandText = sbQuery.ToString();
                                            cmdMiscEarnings.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                            cmdMiscEarnings.Parameters.AddWithValue("@TransDate", Period);

                                            SQLDataAccess sdaMiscEarnings = new SQLDataAccess(_configuration);
                                            using (SqlDataReader drMiscEarnings = sdaMiscEarnings.RetrieveData(cmdMiscEarnings))
                                            {

                                                while (drMiscEarnings.Read())
                                                {
                                                    dMiscEarnings = drMiscEarnings.GetDecimal(0);
                                                }
                                                drMiscEarnings.Dispose();
                                            }

                                            cmdMiscEarnings.Dispose();
                                            sdaMiscEarnings.Dispose();

                                            dMiscEarnings = dMiscEarnings + dLumpSum_NonStructure;

                                            sbQuery = new StringBuilder();
                                            sbQuery.Append(" SELECT ISNULL(SUM(Amount),0) FROM MiscTrans WHERE ( Month(TransDate)= Month(@TransDate) AND Year(TransDate)= Year(@TransDate)) AND EmployeeID = @EmployeeID AND TransType = 2");
                                            SqlCommand cmdMiscDeductions = new SqlCommand();
                                            cmdMiscDeductions.CommandText = sbQuery.ToString();
                                            cmdMiscDeductions.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                            cmdMiscDeductions.Parameters.AddWithValue("@TransDate", Period);

                                            SQLDataAccess sdaMiscDeductions = new SQLDataAccess(_configuration);
                                            using (SqlDataReader drMiscDeductions = sdaMiscDeductions.RetrieveData(cmdMiscDeductions))
                                            {

                                                while (drMiscDeductions.Read())
                                                {
                                                    dMiscDeductions = drMiscDeductions.GetDecimal(0);
                                                }
                                                drMiscDeductions.Dispose();
                                            }

                                            cmdMiscDeductions.Dispose();
                                            sdaMiscDeductions.Dispose();
                                            dMiscDeductions = dMiscDeductions + OnSEMIDeduction;
                                            //sbQuery.Append(" SELECT SalaryAdvance.ID, SalaryAdvance.TransType, CAST((SalaryAdvance.Amount/SalaryAdvance.NoOfInstallments) AS Numeric(18,2)) AS Installment, ");
                                            //sbQuery.Append(" SalaryAdvance.Amount AS Advance,ISNULL(Sum(advancerepayment.Amount),0) as Payment ");
                                            //sbQuery.Append(" FROM SalaryAdvance  ");
                                            //sbQuery.Append(" LEFT OUTER JOIN  AdvanceRepayment ON AdvanceRepayment.AdvanceID = SalaryAdvance.ID ");
                                            //sbQuery.Append(" LEFT OUTER JOIN payslip on payslip.id=advancerepayment.payslipid  AND PaySlip.Period<@Period ");
                                            //sbQuery.Append(" WHERE SalaryAdvance.EmployeeID=@EmployeeID AND SalaryAdvance.AdvanceDate<@Period ");
                                            //sbQuery.Append(" group by SalaryAdvance.ID, SalaryAdvance.TransType, SalaryAdvance.Amount , SalaryAdvance.NoOfInstallments ");
                                            //sbQuery.Append(" having SalaryAdvance.Amount - ISNULL(SUM(AdvanceRepayment.Amount),0) >0");
                                            sbQuery = new StringBuilder();
                                            //sbQuery.Append(" SELECT SalaryAdvance.ID, SalaryAdvance.TransType, CAST((SalaryAdvance.Amount/SalaryAdvance.NoOfInstallments) AS Numeric(18,2)) AS Installment, ");
                                            //sbQuery.Append(" SalaryAdvance.Amount as Advance ");
                                            //sbQuery.Append(" FROM SalaryAdvance ");
                                            //sbQuery.Append(" WHERE SalaryAdvance.EmployeeID=@EmployeeID AND Month(SalaryAdvance.AdvanceDate)= Month(@Period) AND Year(SalaryAdvance.AdvanceDate)= Year(@Period) ");

                                            sbQuery.Append(" SELECT SalaryAdvance.ID, SalaryAdvance.TransType, CAST((SalaryAdvance.Amount/SalaryAdvance.NoOfInstallments) AS Numeric(18,2)) AS Installment, ");
                                            sbQuery.Append(" SalaryAdvance.Amount as Advance , ISNULL(Repayment.Payment,0) AS Payment ");
                                            sbQuery.Append(" FROM SalaryAdvance ");
                                            sbQuery.Append(" LEFT OUTER JOIN ( SELECT AdvanceRepayment.AdvanceID , ISNULL(SUM(AdvanceRepayment.Amount),0) as Payment ");
                                            sbQuery.Append(" FROM AdvanceRepayment ");
                                            sbQuery.Append(" INNER JOIN Payslip ON AdvanceRepayment.PaySlipID = PaySlip.ID ");
                                            sbQuery.Append(" INNER JOIN SalaryAdvance ON AdvanceRepayment.AdvanceID = SalaryAdvance.ID ");
                                            sbQuery.Append(" WHERE PaySlip.Period<>@Period AND SalaryAdvance.EmployeeID=@EmployeeID ");
                                            sbQuery.Append(" GROUP BY AdvanceRepayment.AdvanceID ) Repayment ON SalaryAdvance.ID = Repayment.AdvanceID  ");
                                            sbQuery.Append(" WHERE SalaryAdvance.EmployeeID=@EmployeeID AND SalaryAdvance.IsDeleted = 0 AND SalaryAdvance.AdvanceDate<= @Period AND SalaryAdvance.Amount-ISNULL(Repayment.Payment,0)>0 ");

                                            SqlCommand cmdAdvance = new SqlCommand();
                                            cmdAdvance.CommandText = sbQuery.ToString();
                                            cmdAdvance.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                            cmdAdvance.Parameters.AddWithValue("@Period", Period);

                                            decimal DailyAdvance = 0;
                                            decimal MonthlyAdvance = 0;
                                            decimal SpecialAdvance = 0;
                                            decimal Loan = 0;
                                            decimal UniformIssue = 0;

                                            sdaAdvance = new SQLDataAccess(_configuration);
                                            sdaAdvance.ExecuteSQL("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                                            using (SqlDataReader drAdvance = sdaAdvance.RetrieveData(cmdAdvance))
                                            {

                                                while (drAdvance.Read())
                                                {

                                                    if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) < drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")))
                                                    {
                                                        switch (drAdvance.GetInt32(drAdvance.GetOrdinal("TransType")))
                                                        {
                                                            case 1:
                                                                if (drEmployee["EMPPAY_DATE_RESIGNED"] != DBNull.Value)
                                                                {
                                                                    DateTime dtResignedDate = drEmployee.GetDateTime(drEmployee.GetOrdinal("EMPPAY_DATE_RESIGNED"));
                                                                    if ((dtResignedDate.Month == Period.Month) && (dtResignedDate.Year == Period.Year))
                                                                        DailyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    else
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                        {
                                                                            if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                            {
                                                                                DailyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                            }
                                                                            else
                                                                                DailyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                        }
                                                                        else
                                                                            DailyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                        {
                                                                            DailyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                        }
                                                                        else
                                                                            DailyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                    }
                                                                    else
                                                                        DailyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                }
                                                                break;
                                                            case 2:
                                                                if (drEmployee["EMPPAY_DATE_RESIGNED"] != DBNull.Value)
                                                                {
                                                                    DateTime dtResignedDate = drEmployee.GetDateTime(drEmployee.GetOrdinal("EMPPAY_DATE_RESIGNED"));
                                                                    if ((dtResignedDate.Month == Period.Month) && (dtResignedDate.Year == Period.Year))
                                                                        MonthlyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    else
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                        {
                                                                            if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                            {
                                                                                MonthlyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                            }
                                                                            else
                                                                                MonthlyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                        }
                                                                        else
                                                                            MonthlyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                        {
                                                                            MonthlyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                        }
                                                                        else
                                                                            MonthlyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                    }
                                                                    else
                                                                        MonthlyAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                }
                                                                break;
                                                            case 3:
                                                                if (drEmployee["EMPPAY_DATE_RESIGNED"] != DBNull.Value)
                                                                {
                                                                    DateTime dtResignedDate = drEmployee.GetDateTime(drEmployee.GetOrdinal("EMPPAY_DATE_RESIGNED"));
                                                                    if ((dtResignedDate.Month == Period.Month) && (dtResignedDate.Year == Period.Year))
                                                                        Loan += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    else
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                        {
                                                                            if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                            {
                                                                                Loan += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                            }
                                                                            else
                                                                                Loan += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                        }
                                                                        else
                                                                            Loan += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                        {
                                                                            Loan += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                        }
                                                                        else
                                                                            Loan += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                    }
                                                                    else
                                                                        Loan += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                }
                                                                break;
                                                            case 4:
                                                                if (drEmployee["EMPPAY_DATE_RESIGNED"] != DBNull.Value)
                                                                {
                                                                    DateTime dtResignedDate = drEmployee.GetDateTime(drEmployee.GetOrdinal("EMPPAY_DATE_RESIGNED"));
                                                                    if ((dtResignedDate.Month == Period.Month) && (dtResignedDate.Year == Period.Year))
                                                                        UniformIssue += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    else
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                        {
                                                                            if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                            {
                                                                                UniformIssue += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                            }
                                                                            else
                                                                                UniformIssue += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                        }
                                                                        else
                                                                            UniformIssue += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                        {
                                                                            UniformIssue += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                        }
                                                                        else
                                                                            UniformIssue += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                    }
                                                                    else
                                                                        UniformIssue += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                }
                                                                break;
                                                            case 5:
                                                                if (drEmployee["EMPPAY_DATE_RESIGNED"] != DBNull.Value)
                                                                {
                                                                    DateTime dtResignedDate = drEmployee.GetDateTime(drEmployee.GetOrdinal("EMPPAY_DATE_RESIGNED"));
                                                                    if ((dtResignedDate.Month == Period.Month) && (dtResignedDate.Year == Period.Year))
                                                                        SpecialAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    else
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                        {
                                                                            if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                            {
                                                                                SpecialAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                            }
                                                                            else
                                                                                SpecialAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                        }
                                                                        else
                                                                            SpecialAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) > drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")))
                                                                    {
                                                                        if (drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment")) < (decimal)0.1)
                                                                        {
                                                                            SpecialAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                        }
                                                                        else
                                                                            SpecialAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Installment"));
                                                                    }
                                                                    else
                                                                        SpecialAdvance += drAdvance.GetDecimal(drAdvance.GetOrdinal("Advance")) - drAdvance.GetDecimal(drAdvance.GetOrdinal("Payment"));
                                                                }
                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                            cmdAdvance.Dispose();
                                            sdaAdvance.Dispose();
                                            decimal dEPFEmployee = 0;
                                            decimal dEPFEmployer = 0;
                                            if (DeductEPF)
                                            {
                                                sbQuery = new StringBuilder();
                                                //if (DeductEPF8Pa) //8% is true
                                                //{
                                                sbQuery.Append("select epf_worker,epf_boss,epf_worker55,epf_boss55 from EPF  where (epf_from)<=@BasicSalary and (epf_to)>=@BasicSalary");
                                                //}
                                                //else //Default
                                                //{
                                                //    sbQuery.Append("select epf_worker7pa,epf_worker9pa,epf_boss,epf_worker55,epf_boss55 from EPF  where (epf_from)<=@BasicSalary and (epf_to)>=@BasicSalary");
                                                //}
                                                SqlCommand cmdEPF = new SqlCommand();
                                                cmdEPF.CommandText = sbQuery.ToString();
                                                //cmdEPF.Parameters.AddWithValue("@BasicSalary", BasicSalary.ToString("N2"));
                                                cmdEPF.Parameters.AddWithValue("@BasicSalary", double.Parse(BasicSalary.ToString("N2")));

                                                SQLDataAccess sdaEPF = new SQLDataAccess(_configuration);
                                                SqlDataReader drEPF = sdaEPF.RetrieveData(cmdEPF);

                                                while (drEPF.Read())
                                                {
                                                    if (EmployeeAge > 59)//55 -->59 [Started on NOV 2013]
                                                    {
                                                        if (DeductEPFBeyond55)
                                                        {
                                                            //dEPFEmployee = drEPF.GetDecimal(drEPF.GetOrdinal("epf_worker55"));
                                                            //dEPFEmployer = drEPF.GetDecimal(drEPF.GetOrdinal("epf_boss55"));
                                                            //1 Mar 2020 direct deduct 4% from Emplyee above 60
                                                            decimal EPFRate60 = 0.04M;
                                                            dEPFEmployee = (Decimal)0.00;
                                                            dEPFEmployer = BasicSalary * EPFRate60;
                                                            dEPFEmployer = Math.Round(dEPFEmployer);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ////if (DeductEPF8Pa) //8% is true
                                                        ////{
                                                        dEPFEmployee = drEPF.GetDecimal(drEPF.GetOrdinal("epf_worker"));
                                                        ////}
                                                        ////else //Default
                                                        ////{
                                                        ////dEPFEmployee = drEPF.GetDecimal(drEPF.GetOrdinal("epf_worker9pa"));
                                                        ////}
                                                        dEPFEmployer = drEPF.GetDecimal(drEPF.GetOrdinal("epf_boss"));
                                                    }
                                                }
                                                drEPF.Close();
                                                drEPF.Dispose();
                                                cmdEPF.Dispose();
                                                sdaEPF.Dispose();
                                            }
                                            decimal dSOCSOEmployee = 0;
                                            decimal dSOCSOEmployer = 0;
                                            decimal NetPay = 0;
                                            if (DeductSOCSO)
                                            {
                                                //if (WorkingHours == 8)
                                                //    NetPay = BasicSalary + OffdaySalary;
                                                //else
                                                //    NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;

                                                //2/11/2019 Aura to follow other company
                                                //if (CompanyCode.ToString().Trim() =="AURA")
                                                //{
                                                //    if (EmployeeType == "Staff")
                                                //    {
                                                //        NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;
                                                //        NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance;

                                                //    }
                                                //    else
                                                //    {
                                                //        //commented the code below as reported by Amuta that the salary processing for July is wrong on 2019 Aug 06
                                                //        //NetPay = BasicSalary + OverTimeSalary;
                                                //        //NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance;
                                                //        NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;
                                                //        NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance;

                                                //    }
                                                //}
                                                if (CompanyCode.ToString().Trim() == "FWG")
                                                {
                                                    if (EmployeeType == "Staff")
                                                    {
                                                        NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;
                                                        NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance;

                                                    }
                                                    else
                                                    {
                                                        if (sEmpPassport.ToString().Trim().Length > 3)
                                                        {
                                                            if (CompanyCode.ToString().Trim() == "FWG")
                                                            {
                                                                NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;
                                                                NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance + Shift2Amount;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //commented the code below as reported by Amuta that the salary processing for July is wrong on 2019 Aug 06
                                                            //NetPay = BasicSalary + OverTimeSalary;
                                                            //NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance;
                                                            //Version 1.0 Change the new payslip for Guard which to calculate the Socso by Basic only

                                                            NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;
                                                            NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance;

                                                            // 06/11/2019 rollback to previous version as Oct Salary having issue in Prod
                                                            //NetPay = BasicSalary;
                                                        }
                                                    }

                                                }
                                                else
                                                {
                                                    NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;
                                                    NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance;

                                                }

                                                sbQuery = new StringBuilder();
                                                sbQuery.Append("select top 1 socso_employer,socso_worker,socso_50year,socso_total,socso_foreigner from SOCSO  where socso_to >= @NetPay and socso_from <=@NetPay");
                                                SqlCommand cmdSOCSO = new SqlCommand();
                                                cmdSOCSO.CommandText = sbQuery.ToString();
                                                cmdSOCSO.Parameters.AddWithValue("@NetPay", NetPay);
                                                SQLDataAccess sdaSOCSO = new SQLDataAccess(_configuration);
                                                SqlDataReader drSOCSO = sdaSOCSO.RetrieveData(cmdSOCSO);

                                                while (drSOCSO.Read())
                                                {
                                                    if (EmployeeAge > 59) //55 -->59 [Started on Jan 2013]
                                                    {
                                                        dSOCSOEmployer = drSOCSO.GetDecimal(drSOCSO.GetOrdinal("socso_50year"));
                                                    }
                                                    else
                                                    {
                                                        dSOCSOEmployee = drSOCSO.GetDecimal(drSOCSO.GetOrdinal("socso_worker"));
                                                        dSOCSOEmployer = drSOCSO.GetDecimal(drSOCSO.GetOrdinal("socso_employer"));
                                                    }
                                                    if (sEmpPassport.ToString().Trim().Length > 3)
                                                    {
                                                        if (CompanyCode.ToString().Trim() == "FWG")
                                                        {
                                                            dSOCSOEmployer = drSOCSO.GetDecimal(drSOCSO.GetOrdinal("socso_foreigner"));
                                                        }
                                                    }
                                                }
                                                drSOCSO.Close();
                                                drSOCSO.Dispose();
                                                cmdSOCSO.Dispose();
                                                sdaSOCSO.Dispose();
                                            }
                                            else // added by Kean Hong on 08 Feb 2020 to include sosco for foreigner guard which paid by Employer only
                                            {
                                                if (sEmpPassport.ToString().Trim().Length > 3)
                                                {
                                                    if (CompanyCode.ToString().Trim() == "FWG")
                                                    {

                                                        //commented the code below as reported by Amuta that the salary processing for July is wrong on 2019 Aug 06
                                                        //NetPay = BasicSalary + OverTimeSalary;
                                                        //NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance;
                                                        //Version 1.0 Change the new payslip for Guard which to calculate the Socso by Basic only
                                                        //Version 1.1 change to include Shift2 Salary for foreigner as requested by Datin and Liza on Sept 2020
                                                        NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;
                                                        NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance + Shift2Amount;
                                                        // 06/11/2019 rollback to previous version as Oct Salary having issue in Prod
                                                        //NetPay = BasicSalary;
                                                    }
                                                    else
                                                    {
                                                        NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;
                                                        NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance;
                                                    }

                                                    sbQuery = new StringBuilder();
                                                    sbQuery.Append("select top 1 socso_employer,socso_worker,socso_50year,socso_total,socso_foreigner from SOCSO  where socso_to >= @NetPay and socso_from <=@NetPay");
                                                    SqlCommand cmdSOCSO = new SqlCommand();
                                                    cmdSOCSO.CommandText = sbQuery.ToString();
                                                    cmdSOCSO.Parameters.AddWithValue("@NetPay", NetPay);
                                                    SQLDataAccess sdaSOCSO = new SQLDataAccess(_configuration);
                                                    SqlDataReader drSOCSO = sdaSOCSO.RetrieveData(cmdSOCSO);

                                                    while (drSOCSO.Read())
                                                    {
                                                        if (CompanyCode.ToString().Trim() == "FWG")
                                                        {
                                                            dSOCSOEmployer = drSOCSO.GetDecimal(drSOCSO.GetOrdinal("socso_foreigner"));
                                                        }
                                                        else
                                                        {
                                                            if (EmployeeAge > 59) //55 -->59 [Started on Jan 2013]
                                                            {
                                                                dSOCSOEmployer = drSOCSO.GetDecimal(drSOCSO.GetOrdinal("socso_50year"));
                                                            }
                                                            else
                                                            {
                                                                dSOCSOEmployer = drSOCSO.GetDecimal(drSOCSO.GetOrdinal("socso_employer"));
                                                            }
                                                        }

                                                    }
                                                    drSOCSO.Close();
                                                    drSOCSO.Dispose();
                                                    cmdSOCSO.Dispose();
                                                    sdaSOCSO.Dispose();
                                                }
                                            }

                                            //SIP
                                            decimal dSIPEmployee = 0;
                                            decimal dSIPEmployer = 0;
                                            decimal dGrossIncomeSIP = 0;

                                            if (dSOCSOEmployee != 0)
                                            {
                                                if (CompanyCode.ToString().Trim() == "EASTWEST")
                                                {
                                                    dGrossIncomeSIP = BasicSalary + OverTimeSalary + OffdaySalary + OffdayOTSalary + HolidaySalary + HolidayOTSalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance + dMiscEarnings + Shift2Amount;
                                                }
                                                //2/11/2019 Aura to follow other company 
                                                //else if (CompanyCode.ToString().Trim() == "AURA")
                                                //{
                                                //    if (EmployeeType == "Staff")
                                                //    {
                                                //        dGrossIncomeSIP = BasicSalary + OverTimeSalary + OffdaySalary + OffdayOTSalary + HolidaySalary + HolidayOTSalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance + dMiscEarnings;
                                                //    }
                                                //    else
                                                //    {
                                                //        dGrossIncomeSIP = BasicSalary + OverTimeSalary + OffdaySalary + OffdayOTSalary + HolidaySalary + HolidayOTSalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance + dMiscEarnings;
                                                //    }

                                                //}
                                                else if (CompanyCode.ToString().Trim() == "FWG")
                                                {
                                                    if (EmployeeType == "Staff")
                                                    {
                                                        dGrossIncomeSIP = BasicSalary + OverTimeSalary + OffdaySalary + OffdayOTSalary + HolidaySalary + HolidayOTSalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance + dMiscEarnings;
                                                    }
                                                    else
                                                    {
                                                        //commented the code below as reported by Amuta that the salary processing for July is wrong on 2019 Aug 06
                                                        //dGrossIncomeSIP = BasicSalary + OverTimeSalary + HolidaySalary + HolidayOTSalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance;
                                                        //Version 1.0 Change the new payslip for Guard which to calculate the Socso by Basic only
                                                        dGrossIncomeSIP = BasicSalary + OverTimeSalary + OffdaySalary + OffdayOTSalary + HolidaySalary + HolidayOTSalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance + dMiscEarnings;
                                                        // 06/11/2019 rollback to previous version as Oct Salary having issue in Prod
                                                        //dGrossIncomeSIP = BasicSalary;

                                                    }
                                                }
                                                else
                                                {
                                                    dGrossIncomeSIP = BasicSalary + OverTimeSalary + OffdaySalary + OffdayOTSalary + HolidaySalary + HolidayOTSalary + dReAllowance + dAttendanceAllowance + dSpecialAllowance + dMiscEarnings;
                                                }
                                                if (strCitizen == 0 && EmployeeAge < 60)
                                                {
                                                    sbQuery = new StringBuilder();
                                                    sbQuery.Append("select * from SIP where (SIP_from)<=@GrossSalary and (SIP_to)>=@GrossSalary");
                                                    SqlCommand cmdSIP = new SqlCommand();
                                                    cmdSIP.CommandText = sbQuery.ToString();
                                                    cmdSIP.Parameters.AddWithValue("@GrossSalary", double.Parse(dGrossIncomeSIP.ToString("N2")));

                                                    SQLDataAccess sdaSIP = new SQLDataAccess(_configuration);
                                                    SqlDataReader drSIP = sdaSIP.RetrieveData(cmdSIP);

                                                    while (drSIP.Read())
                                                    {
                                                        dSIPEmployee = drSIP.GetDecimal(drSIP.GetOrdinal("SIP_worker"));
                                                        dSIPEmployer = drSIP.GetDecimal(drSIP.GetOrdinal("SIP_boss"));

                                                    }
                                                    drSIP.Close();
                                                    drSIP.Dispose();
                                                    cmdSIP.Dispose();
                                                    sdaSIP.Dispose();
                                                }
                                            }
                                            //IncomeTax - portion
                                            decimal dIncomeTaxEmployee = 0;
                                            decimal dIncomeTaxSalaryCalc = 0;
                                            if (DeductIncomeTax)
                                            {

                                                sbQuery = new StringBuilder();
                                                sbQuery.Append("select [IT_Category1] from IncomeTax  where [IT_SAL_TO] >= @BasicSalary and [IT_SAL_FROM] <=@BasicSalary");
                                                SqlCommand cmdIncomeTax = new SqlCommand();
                                                cmdIncomeTax.CommandText = sbQuery.ToString();
                                                //dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance
                                                dIncomeTaxSalaryCalc = BasicSalary + dAttendanceAllowance - dEPFEmployee;
                                                cmdIncomeTax.Parameters.AddWithValue("@BasicSalary", double.Parse(dIncomeTaxSalaryCalc.ToString("N2")));
                                                SQLDataAccess sdaIncomeTax = new SQLDataAccess(_configuration);
                                                SqlDataReader drIncomeTax = sdaIncomeTax.RetrieveData(cmdIncomeTax);

                                                while (drIncomeTax.Read())
                                                {
                                                    //dIncomeTaxEmployee = drIncomeTax.GetDecimal(drIncomeTax.GetOrdinal("IT_Category1"));
                                                    dIncomeTaxEmployee = decimal.Parse(drIncomeTax["IT_Category1"].ToString());
                                                }
                                                drIncomeTax.Close();
                                                drIncomeTax.Dispose();
                                                cmdIncomeTax.Dispose();
                                                sdaIncomeTax.Dispose();
                                            }

                                            SqlCommand cmdAdvanceRepay = new SqlCommand("DELETE FROM AdvanceRepayment WHERE PaySlipID = (SELECT ID FROM PaySlip WHERE Period=@Period AND EmployeeID=@EmployeeID)");
                                            cmdAdvanceRepay.Parameters.AddWithValue("@Period", Period);
                                            cmdAdvanceRepay.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                            sdaPaySlip.ExecuteSQL(cmdAdvanceRepay);
                                            cmdAdvanceRepay.Dispose();

                                            SqlCommand cmdPaySlipDel = new SqlCommand("DELETE FROM PaySlip WHERE Period=@Period AND EmployeeID=@EmployeeID");
                                            cmdPaySlipDel.Parameters.AddWithValue("@Period", Period);
                                            cmdPaySlipDel.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                            sdaPaySlip.ExecuteSQL(cmdPaySlipDel);
                                            cmdPaySlipDel.Dispose();

                                            //Commented by Kean Hong on 26 Apr 2020 to deduct the Gross Salary from Staff until it has been informed to remove 
                                            ////if (CompanyCode.ToString().Trim() == "FWG" && EmployeeType == "Staff")
                                            ////{
                                            ////    NetPay = BasicSalary + OffdaySalary + OverTimeSalary + OffdayOTSalary;
                                            ////    NetPay += HolidayOTSalary + HolidaySalary + dReAllowance + dAttendanceAllowance + dBonus + dSpecialAllowance;
                                            ////    if (EmployeeID == 17755 || EmployeeID == 4013 || EmployeeID == 20484)
                                            ////    {
                                            ////    }
                                            ////    else if (EmployeeID == 11487)
                                            ////    {
                                            ////        SpecialAdvance += (NetPay * 0.10M);
                                            ////        dMiscDeductions = dMiscDeductions + SpecialAdvance;
                                            ////    }
                                            ////    else
                                            ////    {
                                            ////        SpecialAdvance += (NetPay * 0.07M);
                                            ////        dMiscDeductions = dMiscDeductions + SpecialAdvance;
                                            ////    }

                                            ////}

                                            using (SqlCommand cmdPaySlip = new SqlCommand())
                                            {
                                                sbQuery = new StringBuilder();
                                                sbQuery.Append(" INSERT INTO PaySlip (Period, EmployeeID,BasicSalaryDays,BasicSalaryRate, BasicSalary, ");
                                                //sbQuery.Append(" INSERT INTO PaySlip (OverTimeSalaryHours,OverTimeSalaryRate,OverTimeSalary, OffDaySalaryDays, ");
                                                sbQuery.Append(" OverTimeSalaryHours,OverTimeSalaryRate,OverTimeSalary, OffDaySalaryDays, ");
                                                sbQuery.Append(" OffDaySalaryRate,OffDaySalary, OffDayOverTimeSalaryHours,OffDayOverTimeSalaryRate,OffDayOverTimeSalary, ");
                                                sbQuery.Append(" HolidaySalaryDays,HolidaySalaryRate,HolidaySalary, HolidayOverTimeSalaryHours,HolidayOverTimeSalaryRate, ");
                                                sbQuery.Append(" HolidayOverTimeSalary,Shift2SalaryDaysHours,Shift2SalaryRateType,Shift2SalaryRate,Shift2Salary, ");
                                                sbQuery.Append(" ReAllowanceDays,ReAllowance,AttendanceAllowance,SpecialAllowance, EPFDeductionAmount, SOCSODeductionAmount, ");
                                                sbQuery.Append(" EPFEmployerContribution, SOCSOEmployerContribution, ");
                                                sbQuery.Append(" DailyAdvanceRecovery, MonthlyAdvanceRecovery,SpecialAdvanceRecovery, UniformIssueRecovery, LoanRecovery,IncomeTaxDeduction, ");
                                                sbQuery.Append(" MiscAmount, MiscDeduction,Bonus,SalaryPayMode, ");
                                                sbQuery.Append(" LastUpdate,LastUpdatedBy,SIPEmployeeContribution,SIPEmployerContribution) VALUES (@Period, @EmployeeID, @BasicSalaryDays,@BasicSalaryRate,@BasicSalary, ");
                                                sbQuery.Append(" @OverTimeSalaryHours,@OverTimeSalaryRate,@OverTimeSalary,@OffDaySalaryDays,@OffDaySalaryRate,@OffDaySalary, ");
                                                sbQuery.Append(" @OffDayOverTimeSalaryHours,@OffDayOverTimeSalaryRate,@OffDayOverTimeSalary,@HolidaySalaryDays, ");
                                                sbQuery.Append(" @HolidaySalaryRate,@HolidaySalary,@HolidayOverTimeSalaryHours,@HolidayOverTimeSalaryRate, ");
                                                sbQuery.Append(" @HolidayOverTimeSalary,@Shift2SalaryDaysHours,@Shift2SalaryRateType,@Shift2SalaryRate,@Shift2Salary, ");
                                                sbQuery.Append(" @ReAllowanceDays,@ReAllowance,@AttendanceAllowance,@SpecialAllowance, @EPFDeductionAmount, @SOCSODeductionAmount, ");
                                                sbQuery.Append(" @EPFEmployerContribution, @SOCSOEmployerContribution, ");
                                                sbQuery.Append(" @DailyAdvanceRecovery, @MonthlyAdvanceRecovery,@SpecialAdvanceRecovery, @UniformIssueRecovery, @LoanRecovery, @IncomeTaxDeduction, ");
                                                sbQuery.Append(" @MiscAmount, @MiscDeduction,@Bonus, @SalaryPayMode, ");
                                                sbQuery.Append(" @LASTUPDATE,@LastUpdatedBy,@SIPEmployeeContribution,@SIPEmployerContribution); SELECT SCOPE_IDENTITY()");
                                                cmdPaySlip.CommandText = sbQuery.ToString();
                                                cmdPaySlip.Parameters.AddWithValue("@Period", Period);
                                                cmdPaySlip.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                                cmdPaySlip.Parameters.AddWithValue("@BasicSalaryDays", BasicSalaryDays);
                                                cmdPaySlip.Parameters.AddWithValue("@BasicSalaryRate", BasicSalaryRate);
                                                // Use CB_Basic only - no fallback
                                                cmdPaySlip.Parameters.AddWithValue("@BasicSalary", dCB_Basic);
                                                cmdPaySlip.Parameters.AddWithValue("@OverTimeSalaryHours", OverTimeSalaryHours);
                                                cmdPaySlip.Parameters.AddWithValue("@OverTimeSalaryRate", OverTimeSalaryRate);
                                                cmdPaySlip.Parameters.AddWithValue("@OverTimeSalary", OverTimeSalary);
                                                cmdPaySlip.Parameters.AddWithValue("@OffDaySalaryDays", OffdaySalaryDays);
                                                cmdPaySlip.Parameters.AddWithValue("@OffDaySalaryRate", OffdaySalaryRate);
                                                // Use CB_NH only for OffDaySalary - no fallback
                                                cmdPaySlip.Parameters.AddWithValue("@OffDaySalary", dCB_NH);
                                                cmdPaySlip.Parameters.AddWithValue("@OffDayOverTimeSalaryHours", OffdayOTSalaryHours);
                                                cmdPaySlip.Parameters.AddWithValue("@OffDayOverTimeSalaryRate", OffdayOTSalaryRate);
                                                cmdPaySlip.Parameters.AddWithValue("@OffDayOverTimeSalary", OffdayOTSalary);
                                                cmdPaySlip.Parameters.AddWithValue("@HolidaySalaryDays", HolidaySalaryDays);
                                                cmdPaySlip.Parameters.AddWithValue("@HolidaySalaryRate", HolidaySalaryRate);
                                                cmdPaySlip.Parameters.AddWithValue("@HolidaySalary", HolidaySalary);
                                                cmdPaySlip.Parameters.AddWithValue("@HolidayOverTimeSalaryHours", HolidayOTSalaryHours);
                                                cmdPaySlip.Parameters.AddWithValue("@HolidayOverTimeSalaryRate", HolidayOTSalaryRate);
                                                cmdPaySlip.Parameters.AddWithValue("@HolidayOverTimeSalary", HolidayOTSalary);
                                                cmdPaySlip.Parameters.AddWithValue("@Shift2SalaryDaysHours", Shift2days);
                                                cmdPaySlip.Parameters.AddWithValue("@Shift2SalaryRateType", Shift2type);
                                                cmdPaySlip.Parameters.AddWithValue("@Shift2SalaryRate", Shift2rate);
                                                cmdPaySlip.Parameters.AddWithValue("@Shift2Salary", Shift2Amount);
                                                cmdPaySlip.Parameters.AddWithValue("@ReAllowanceDays", ReAllowanceDays);
                                                // Removed CB_Leaves - set ReAllowance to 0
                                                cmdPaySlip.Parameters.AddWithValue("@ReAllowance", 0);
                                                // Use CB_HRA only for AttendanceAllowance - no fallback
                                                cmdPaySlip.Parameters.AddWithValue("@AttendanceAllowance", dCB_HRA);
                                                // Use CB_OtherAllowances for SpecialAllowance - no fallback
                                                cmdPaySlip.Parameters.AddWithValue("@SpecialAllowance", dCB_OtherAllowances);
                                                cmdPaySlip.Parameters.AddWithValue("@EPFDeductionAmount", dEPFEmployee);
                                                cmdPaySlip.Parameters.AddWithValue("@SOCSODeductionAmount", dSOCSOEmployee);
                                                cmdPaySlip.Parameters.AddWithValue("@EPFEmployerContribution", dEPFEmployer);
                                                cmdPaySlip.Parameters.AddWithValue("@SOCSOEmployerContribution", dSOCSOEmployer);
                                                // DailyAdvanceRecovery - use calculated value from SalaryAdvance
                                                cmdPaySlip.Parameters.AddWithValue("@DailyAdvanceRecovery", DailyAdvance);
                                                cmdPaySlip.Parameters.AddWithValue("@MonthlyAdvanceRecovery", MonthlyAdvance);
                                                cmdPaySlip.Parameters.AddWithValue("@SpecialAdvanceRecovery", SpecialAdvance);
                                                cmdPaySlip.Parameters.AddWithValue("@UniformIssueRecovery", UniformIssue);
                                                cmdPaySlip.Parameters.AddWithValue("@LoanRecovery", Loan);
                                                cmdPaySlip.Parameters.AddWithValue("@IncomeTaxDeduction", dIncomeTaxEmployee);
                                                // Use CB_DA only for MiscAmount (Dearness Allowance) - no fallback
                                                cmdPaySlip.Parameters.AddWithValue("@MiscAmount", dCB_DA);
                                                cmdPaySlip.Parameters.AddWithValue("@MiscDeduction", dMiscDeductions);
                                                // Use CB_AdvanceStatutoryBonus only - no fallback
                                                cmdPaySlip.Parameters.AddWithValue("@Bonus", dCB_AdvanceStatutoryBonus);
                                                cmdPaySlip.Parameters.AddWithValue("@SalaryPayMode", SalaryPayMode);
                                                cmdPaySlip.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                                cmdPaySlip.Parameters.AddWithValue("@LastUpdatedBy", CurrentUser);
                                                cmdPaySlip.Parameters.AddWithValue("@SIPEmployeeContribution", dSIPEmployee);
                                                cmdPaySlip.Parameters.AddWithValue("@SIPEmployerContribution", dSIPEmployer);
                                                SqlDataReader drTemp = sdaPaySlip.RetrieveData(cmdPaySlip);
                                                drTemp.Read();
                                                decimal dPaySlipID = drTemp.GetDecimal(0);
                                                drTemp.Close();
                                                sbQuery = new StringBuilder();
                                                sbQuery.Append("INSERT INTO AdvanceRepayment (AdvanceID,PaySlipID,Amount,LastUpdate) ");
                                                sbQuery.Append("SELECT ID,@PaySlipID, CASE WHEN Advance-Payment>Installment THEN Installment ELSE Advance-Payment END as CurrentPayment, @LastUpdate ");
                                                sbQuery.Append("FROM ");
                                                sbQuery.Append(" (SELECT SalaryAdvance.ID, CASE WHEN (MONTH(ISNULL(EmploymentDetails.EMPPAY_DATE_RESIGNED,CAST('2100-01-01' as DATETIME)))=MONTH(@Period) AND YEAR(ISNULL(EmploymentDetails.EMPPAY_DATE_RESIGNED,CAST('2100-01-01' as DATETIME)))=YEAR(@Period)) THEN SUM(SalaryAdvance.Amount)-ISNULL(SUM(paymentdetails.payment),0) ELSE CAST(SUM(SalaryAdvance.Amount/SalaryAdvance.NoOfInstallments) AS Numeric(18,2)) END AS Installment, ");
                                                sbQuery.Append(" SUM(SalaryAdvance.Amount) AS Advance,ISNULL(SUM(paymentdetails.payment),0) as Payment ");
                                                sbQuery.Append(" FROM SalaryAdvance  ");
                                                sbQuery.Append(" LEFT OUTER JOIN (select AdvanceID,Sum(advancerepayment.Amount) as Payment  ");
                                                sbQuery.Append(" from advancerepayment left outer join payslip on payslip.id=advancerepayment.payslipid where advancerepayment.advanceid in (select id  from SalaryAdvance where SalaryAdvance.EmployeeID=@EmployeeID AND SalaryAdvance.AdvanceDate<=@Period) and payslip.period<@Period group by advanceid) PaymentDetails  ON PaymentDetails.AdvanceID = SalaryAdvance.ID ");
                                                sbQuery.Append(" INNER JOIN Employee ON Employee.Emp_ID = @EmployeeID ");
                                                sbQuery.Append(" INNER JOIN EmploymentDetails ON EmploymentDetails.EmpPay_Code = Employee.Emp_Code ");
                                                sbQuery.Append(" WHERE SalaryAdvance.EmployeeID=@EmployeeID AND SalaryAdvance.AdvanceDate<=@Period AND SalaryAdvance.IsDeleted = 0 ");
                                                sbQuery.Append(" group by SalaryAdvance.ID, SalaryAdvance.TransType, EmploymentDetails.EMPPAY_DATE_RESIGNED ");
                                                sbQuery.Append(" having SUM(SalaryAdvance.Amount) - ISNULL(SUM(paymentdetails.payment),0) >0) PaymentDetail ");

                                                SqlCommand cmdRepayment = new SqlCommand();
                                                cmdRepayment.CommandText = sbQuery.ToString();
                                                cmdRepayment.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                                cmdRepayment.Parameters.AddWithValue("@PaySlipID", dPaySlipID);
                                                cmdRepayment.Parameters.AddWithValue("@Period", Period);
                                                cmdRepayment.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                                sdaPaySlip.ExecuteSQL(cmdRepayment);
                                            }

                                            //Insert into PayslipAudit
                                            sbQuery = new StringBuilder();
                                            sbQuery.Append(" INSERT INTO PaySlipAudit (Period, EmployeeID,BasicSalaryDays,BasicSalaryRate, BasicSalary, ");
                                            sbQuery.Append(" OverTimeSalaryHours,OverTimeSalaryRate,OverTimeSalary, OffDaySalaryDays, ");
                                            sbQuery.Append(" OffDaySalaryRate,OffDaySalary, OffDayOverTimeSalaryHours,OffDayOverTimeSalaryRate,OffDayOverTimeSalary, ");
                                            sbQuery.Append(" HolidaySalaryDays,HolidaySalaryRate,HolidaySalary, HolidayOverTimeSalaryHours,HolidayOverTimeSalaryRate, ");
                                            sbQuery.Append(" HolidayOverTimeSalary,Shift2SalaryDaysHours,Shift2SalaryRateType,Shift2SalaryRate,Shift2Salary, ");
                                            sbQuery.Append(" ReAllowanceDays,ReAllowance,AttendanceAllowance,SpecialAllowance, EPFDeductionAmount, SOCSODeductionAmount, ");
                                            sbQuery.Append(" EPFEmployerContribution, SOCSOEmployerContribution, ");
                                            sbQuery.Append(" DailyAdvanceRecovery, MonthlyAdvanceRecovery,SpecialAdvanceRecovery, UniformIssueRecovery, LoanRecovery,IncomeTaxDeduction, ");
                                            sbQuery.Append(" MiscAmount, MiscDeduction,Bonus,SalaryPayMode, ");
                                            sbQuery.Append(" LastUpdate,LastUpdatedBy,Version,SIPEmployeeContribution,SIPEmployerContribution) VALUES (@Period, @EmployeeID, @BasicSalaryDays,@BasicSalaryRate,@BasicSalary, ");
                                            sbQuery.Append(" @OverTimeSalaryHours,@OverTimeSalaryRate,@OverTimeSalary,@OffDaySalaryDays,@OffDaySalaryRate,@OffDaySalary, ");
                                            sbQuery.Append(" @OffDayOverTimeSalaryHours,@OffDayOverTimeSalaryRate,@OffDayOverTimeSalary,@HolidaySalaryDays, ");
                                            sbQuery.Append(" @HolidaySalaryRate,@HolidaySalary,@HolidayOverTimeSalaryHours,@HolidayOverTimeSalaryRate, ");
                                            sbQuery.Append(" @HolidayOverTimeSalary,@Shift2SalaryDaysHours,@Shift2SalaryRateType,@Shift2SalaryRate,@Shift2Salary, ");
                                            sbQuery.Append(" @ReAllowanceDays,@ReAllowance,@AttendanceAllowance,@SpecialAllowance, @EPFDeductionAmount, @SOCSODeductionAmount, ");
                                            sbQuery.Append(" @EPFEmployerContribution, @SOCSOEmployerContribution, ");
                                            sbQuery.Append(" @DailyAdvanceRecovery, @MonthlyAdvanceRecovery,@SpecialAdvanceRecovery, @UniformIssueRecovery, @LoanRecovery, @IncomeTaxDeduction, ");
                                            sbQuery.Append(" @MiscAmount, @MiscDeduction,@Bonus, @SalaryPayMode, ");
                                            sbQuery.Append(" @LASTUPDATE,@LastUpdatedBy,@Version,@SIPEmployeeContribution,@SIPEmployerContribution)");

                                            SqlCommand cmdPaySlipAudit = new SqlCommand();
                                            cmdPaySlipAudit.CommandText = sbQuery.ToString();
                                            cmdPaySlipAudit.Parameters.AddWithValue("@Period", Period);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@BasicSalaryDays", BasicSalaryDays);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@BasicSalaryRate", BasicSalaryRate);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@BasicSalary", BasicSalary);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@OverTimeSalaryHours", OverTimeSalaryHours);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@OverTimeSalaryRate", OverTimeSalaryRate);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@OverTimeSalary", OverTimeSalary);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@OffDaySalaryDays", OffdaySalaryDays);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@OffDaySalaryRate", OffdaySalaryRate);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@OffDaySalary", OffdaySalary);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@OffDayOverTimeSalaryHours", OffdayOTSalaryHours);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@OffDayOverTimeSalaryRate", OffdayOTSalaryRate);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@OffDayOverTimeSalary", OffdayOTSalary);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@HolidaySalaryDays", HolidaySalaryDays);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@HolidaySalaryRate", HolidaySalaryRate);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@HolidaySalary", HolidaySalary);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@HolidayOverTimeSalaryHours", HolidayOTSalaryHours);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@HolidayOverTimeSalaryRate", HolidayOTSalaryRate);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@HolidayOverTimeSalary", HolidayOTSalary);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@Shift2SalaryDaysHours", Shift2days);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@Shift2SalaryRateType", Shift2type);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@Shift2SalaryRate", Shift2rate);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@Shift2Salary", Shift2Amount);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@ReAllowanceDays", ReAllowanceDays);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@ReAllowance", dReAllowance);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@AttendanceAllowance", dAttendanceAllowance);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@SpecialAllowance", (dSpecialAllowance));
                                            cmdPaySlipAudit.Parameters.AddWithValue("@EPFDeductionAmount", dEPFEmployee);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@SOCSODeductionAmount", dSOCSOEmployee);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@EPFEmployerContribution", dEPFEmployer);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@SOCSOEmployerContribution", dSOCSOEmployer);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@DailyAdvanceRecovery", DailyAdvance);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@MonthlyAdvanceRecovery", MonthlyAdvance);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@SpecialAdvanceRecovery", SpecialAdvance);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@UniformIssueRecovery", UniformIssue);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@LoanRecovery", Loan);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@IncomeTaxDeduction", dIncomeTaxEmployee);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@MiscAmount", dMiscEarnings);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@MiscDeduction", dMiscDeductions);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@Bonus", dBonus);//dBonus
                                            cmdPaySlipAudit.Parameters.AddWithValue("@SalaryPayMode", SalaryPayMode);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@LastUpdatedBy", CurrentUser);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@Version", sSalaryProcessVersion);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@SIPEmployeeContribution", dSIPEmployee);
                                            cmdPaySlipAudit.Parameters.AddWithValue("@SIPEmployerContribution", dSIPEmployer);

                                            sdaPaySlip.ExecuteSQL(cmdPaySlipAudit);
                                            cmdPaySlipAudit.Dispose();
                                            //End Insert

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string detail = ex.Message;
                                        if (ex.StackTrace != null)
                                        {
                                            // Get just the first relevant line from stack trace
                                            var stackLines = ex.StackTrace.Split('\n');
                                            var firstLine = stackLines.FirstOrDefault(l => l.Contains("SalaryProcess.cs"))?.Trim() ?? stackLines.FirstOrDefault()?.Trim() ?? "";
                                            detail += $" | at: {firstLine}";
                                        }
                                        errorLog.Add($"EmployeeID: {EmployeeID} - {detail}");
                                    }
                                    finally
                                    {
                                        if (drEmployee != null) { try { drEmployee.Close(); drEmployee.Dispose(); } catch { } }
                                        if (sdaEmployee != null) { try { sdaEmployee.Dispose(); } catch { } }
                                        if (drAttendance != null) { try { drAttendance.Close(); drAttendance.Dispose(); } catch { } }
                                        if (sdaAttendanceList != null) { try { sdaAttendanceList.Dispose(); } catch { } }
                                        if (sdaAdvance != null) { try { sdaAdvance.Dispose(); } catch { } }
                                    }
                                }
                                sdaPaySlip.EndTransaction(true);
                                sdaPaySlip.Dispose();
                                if (errorLog.Count > 0)
                                {
                                    return "Processed with some errors. Please check the following employees:\n" + string.Join("\n", errorLog);
                                }
                                return string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                decimal EID = EmployeeID;
                string errorMessage = (errorLog != null && errorLog.Count > 0)
                    ? "Processed with errors. Last Error: " + ex.Message + " | History: " + string.Join("; ", errorLog)
                    : $"EmployeeID: {EID} Error on Salary Processing: {ex.Message}";

                // Log the full exception details for debugging
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner Exception: {ex.InnerException.Message}";
                }

                System.Diagnostics.Debug.WriteLine($"SalaryProcess Error: {errorMessage}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                throw new Exception(errorMessage);
            }
        }
    }
}
