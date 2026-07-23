using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
namespace OBMS.WebAPI.BusinessObjects
{
    public class AgreementDetailFactory
    {
        private static readonly IConfiguration configuration;
        protected decimal dID;
        protected decimal dAgreementID;
        protected DateTime dtAgreementDate;
        protected string sClient;
        protected string sBranch;
        protected string sDescription;
        protected int iNoOfGuards;
        protected decimal dPerDay;
        protected decimal dPerMonth;
        protected decimal dRate;
        protected decimal dNoOfHours;
        protected decimal dNoOfDays;
        protected bool sFollowCalender;
        protected decimal dMonthTotal;
        protected bool sHasDiscount;
        protected decimal dDiscountAmount;
        protected int iDiscountHour;
        protected bool sIsTaxable;
        protected decimal dTaxAmount;
        protected DateTime dtLASTUPDATE;
        protected string sCategory;
        protected string sReason;

        public decimal ID
        {
            get
            {
                return dID;
            }
            set
            {
                dID = value;
            }
        }

        public decimal AgreementID
        {
            get
            {
                return dAgreementID;
            }
            set
            {
                dAgreementID = value;
            }
        }

        public DateTime AgreementDate
        {
            get
            {
                return dtAgreementDate;
            }
            set
            {
                dtAgreementDate = value;
            }
        }

        public string Client
        {
            get
            {
                return sClient;
            }
            set
            {
                sClient = value;
            }
        }

        public string Branch
        {
            get
            {
                return sBranch;
            }
            set
            {
                sBranch = value;
            }
        }

        public string Description
        {
            get
            {
                return sDescription;
            }
            set
            {
                sDescription = value;
            }
        }

        public int NoOfGuards
        {
            get
            {
                return iNoOfGuards;
            }
            set
            {
                iNoOfGuards = value;
            }
        }

        public decimal PerDay
        {
            get
            {
                return dPerDay;
            }
            set
            {
                dPerDay = value;
            }
        }

        public decimal PerMonth
        {
            get
            {
                return dPerMonth;
            }
            set
            {
                dPerMonth = value;
            }
        }

        public decimal Rate
        {
            get
            {
                return dRate;
            }
            set
            {
                dRate = value;
            }
        }

        public decimal NoOfHours
        {
            get
            {
                return dNoOfHours;
            }
            set
            {
                dNoOfHours = value;
            }
        }

        public decimal NoOfDays
        {
            get
            {
                return dNoOfDays;
            }
            set
            {
                dNoOfDays = value;
            }
        }

        public bool FollowCalender
        {
            get
            {
                return sFollowCalender;
            }
            set
            {
                sFollowCalender = value;
            }
        }
        public decimal MonthTotal
        {
            get
            {
                return dMonthTotal;
            }
            set
            {
                dMonthTotal = value;
            }
        }
        public bool HasDiscount
        {
            get
            {
                return sHasDiscount;
            }
            set
            {
                sHasDiscount = value;
            }
        }

        public decimal DiscountAmount
        {
            get
            {
                return dDiscountAmount;
            }
            set
            {
                dDiscountAmount = value;
            }
        }

        public int DiscountHour
        {
            get
            {
                return iDiscountHour;
            }
            set
            {
                iDiscountHour = value;
            }
        }

        public bool IsTaxable
        {
            get
            {
                return sIsTaxable;
            }
            set
            {
                sIsTaxable = value;
            }
        }

        public decimal TaxAmount
        {
            get
            {
                return dTaxAmount;
            }
            set
            {
                dTaxAmount = value;
            }
        }

        public DateTime LASTUPDATE
        {
            get
            {
                return dtLASTUPDATE;
            }
            set
            {
                dtLASTUPDATE = value;
            }
        }

        public string Category
        {
            get
            {
                return sCategory;
            }
            set
            {
                sCategory = value;
            }
        }

        public string Reason
        {
            get
            {
                return sReason;
            }
            set
            {
                sReason = value;
            }
        }

        public AgreementDetailFactory()
        {

        }
        static AgreementDetailFactory()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Base configuration file
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true) // Environment-specific file
                .Build();
        }

        public AgreementDetailFactory(decimal ID, decimal AgreementID, DateTime AgreementDate, string Client, string Branch, string Description, int NoOfGuards, decimal PerDay, decimal PerMonth, decimal Rate, decimal NoOfHours, decimal NoOfDays, bool FollowCalender, decimal MonthTotal, bool HasDiscount, decimal DiscountAmount, int DiscountHour, bool IsTaxable, decimal TaxAmount, DateTime LASTUPDATE, string Category, string Reason)
        {
            dID = ID;
            dAgreementID = AgreementID;
            dtAgreementDate = AgreementDate;
            sClient = Client;
            sBranch = Branch;
            sDescription = Description;
            iNoOfGuards = NoOfGuards;
            dPerDay = PerDay;
            dPerMonth = PerMonth;
            dRate = Rate;
            dNoOfHours = NoOfHours;
            dNoOfDays = NoOfDays;
            sFollowCalender = FollowCalender;
            dMonthTotal = MonthTotal;
            sHasDiscount = HasDiscount;
            dDiscountAmount = DiscountAmount;
            iDiscountHour = DiscountHour;
            sIsTaxable = IsTaxable;
            dTaxAmount = TaxAmount;
            dtLASTUPDATE = LASTUPDATE;
            sCategory = Category;
            sReason = Reason;

        }

        public static List<AgreementDetailFactory> GetList(decimal AgreementID)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = " SELECT ID,AgreementID,AgreementDate,Client,Branch,Description,NoOfGuards,PerDay,PerMonth,Rate,NoOfHours,NoOfDays,FollowCalender,MonthTotal,HasDiscount,DiscountAmount,DiscountHour,IsTaxable,TaxAmount,LASTUPDATE,Category,Reason FROM AgreementDetails with(nolock) WHERE AgreementID=@AgreementID ";
                    cmd.Parameters.AddWithValue("@AgreementID", AgreementID);
                    using (SQLDataAccess sdaFactory = new SQLDataAccess(configuration))
                    {
                        using (SqlDataReader dr = sdaFactory.RetrieveData(cmd))
                        {
                            List<AgreementDetailFactory> agreementdetailfactoryList = new List<AgreementDetailFactory>();
                            while (dr.Read())
                            {
                                agreementdetailfactoryList.Add(
                                    new AgreementDetailFactory(
                                    Convert.ToDecimal(dr["ID"]),
                                    Convert.ToDecimal(dr["AgreementID"]),
                                    dr.GetDateTime(dr.GetOrdinal("AgreementDate")),
                                    dr.GetString(dr.GetOrdinal("Client")),
                                    dr.GetString(dr.GetOrdinal("Branch")),
                                    dr.GetString(dr.GetOrdinal("Description")),
                                    dr.GetInt32(dr.GetOrdinal("NoOfGuards")),
                                    dr.GetDecimal(dr.GetOrdinal("PerDay")),
                                    dr.GetDecimal(dr.GetOrdinal("PerMonth")),
                                    dr.GetDecimal(dr.GetOrdinal("Rate")),
                                    dr.GetDecimal(dr.GetOrdinal("NoOfHours")),
                                    dr.GetDecimal(dr.GetOrdinal("NoOfDays")),
                                    dr.GetBoolean(dr.GetOrdinal("FollowCalender")),
                                    dr.GetDecimal(dr.GetOrdinal("MonthTotal")),
                                    dr.GetBoolean(dr.GetOrdinal("HasDiscount")),
                                    dr.GetDecimal(dr.GetOrdinal("DiscountAmount")),
                                    dr.GetInt32(dr.GetOrdinal("DiscountHour")),
                                    dr.GetBoolean(dr.GetOrdinal("IsTaxable")),
                                    dr.GetDecimal(dr.GetOrdinal("TaxAmount")),
                                    dr.GetDateTime(dr.GetOrdinal("LASTUPDATE")),
                                    dr.GetString(dr.GetOrdinal("Category")),
                                    dr.GetString(dr.GetOrdinal("Reason")))
                                );
                            }
                            return agreementdetailfactoryList;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Get(decimal ID)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = " SELECT ID,AgreementID,AgreementDate,Client,Branch,Description,NoOfGuards,PerDay,PerMonth,Rate,NoOfHours,NoOfDays,FollowCalender,MonthTotal,HasDiscount,DiscountAmount,DiscountHour,IsTaxable,TaxAmount,LASTUPDATE,Category,Reason FROM AgreementDetails with(nolock) WHERE  ID=@ID ";
                    cmd.Parameters.AddWithValue("@ID", ID);
                    using (SQLDataAccess sdaFactory = new SQLDataAccess(configuration))
                    {
                        using (SqlDataReader dr = sdaFactory.RetrieveData(cmd))
                        {
                            AgreementDetailFactory agreementdetailfactory = new AgreementDetailFactory();
                            while (dr.Read())
                            {
                                dID = dr.GetDecimal(dr.GetOrdinal("ID"));
                                dAgreementID = dr.GetDecimal(dr.GetOrdinal("AgreementID"));
                                dtAgreementDate = dr.GetDateTime(dr.GetOrdinal("AgreementDate"));
                                sClient = dr.GetString(dr.GetOrdinal("Client"));
                                sBranch = dr.GetString(dr.GetOrdinal("Branch"));
                                sDescription = dr.GetString(dr.GetOrdinal("Description"));
                                iNoOfGuards = dr.GetInt32(dr.GetOrdinal("NoOfGuards"));
                                dPerDay = dr.GetDecimal(dr.GetOrdinal("PerDay"));
                                dPerMonth = dr.GetDecimal(dr.GetOrdinal("PerMonth"));
                                dRate = dr.GetDecimal(dr.GetOrdinal("Rate"));
                                dNoOfHours = dr.GetDecimal(dr.GetOrdinal("NoOfHours"));
                                dNoOfDays = dr.GetDecimal(dr.GetOrdinal("NoOfDays"));
                                sFollowCalender = dr.GetBoolean(dr.GetOrdinal("FollowCalender"));
                                dMonthTotal = dr.GetDecimal(dr.GetOrdinal("MonthTotal"));
                                sHasDiscount = dr.GetBoolean(dr.GetOrdinal("HasDiscount"));
                                dDiscountAmount = dr.GetDecimal(dr.GetOrdinal("DiscountAmount"));
                                iDiscountHour = dr.GetInt32(dr.GetOrdinal("DiscountHour"));
                                sIsTaxable = dr.GetBoolean(dr.GetOrdinal("IsTaxable"));
                                dTaxAmount = dr.GetDecimal(dr.GetOrdinal("TaxAmount"));
                                dtLASTUPDATE = dr.GetDateTime(dr.GetOrdinal("LASTUPDATE"));
                                sCategory = dr.GetString(dr.GetOrdinal("Category"));
                                sReason = dr.GetString(dr.GetOrdinal("Reason"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public bool Add(SQLDataAccess sdaFactory, SqlTransaction Transaction, string CurrentUser)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = " INSERT INTO AgreementDetails ([AgreementID],[AgreementDate],[Client],[Branch],[Description],[NoOfGuards],[PerDay],[PerMonth],[Rate],[NoOfHours],[NoOfDays],[FollowCalender],[MonthTotal],[HasDiscount],[DiscountAmount],[DiscountHour],[IsTaxable],[TaxAmount],[LASTUPDATE],[LastUpdatedBy],[Category],[Reason]) VALUES (@AgreementID,@AgreementDate,@Client,@Branch,@Description,@NoOfGuards,@PerDay,@PerMonth,@Rate,@NoOfHours,@NoOfDays,@FollowCalender,@MonthTotal,@HasDiscount,@DiscountAmount,@DiscountHour,@IsTaxable,@TaxAmount,@LASTUPDATE,@LastUpdatedBy,@Category,@Reason)";
                    cmd.Parameters.AddWithValue("@AgreementID", dAgreementID);
                    cmd.Parameters.AddWithValue("@AgreementDate", dtAgreementDate);
                    cmd.Parameters.AddWithValue("@Client", sClient);
                    cmd.Parameters.AddWithValue("@Branch", sBranch);
                    cmd.Parameters.AddWithValue("@Description", sDescription);
                    cmd.Parameters.AddWithValue("@NoOfGuards", iNoOfGuards);
                    cmd.Parameters.AddWithValue("@PerDay", dPerDay);
                    cmd.Parameters.AddWithValue("@PerMonth", dPerMonth);
                    cmd.Parameters.AddWithValue("@Rate", dRate);
                    cmd.Parameters.AddWithValue("@NoOfHours", dNoOfHours);
                    cmd.Parameters.AddWithValue("@NoOfDays", dNoOfDays);
                    cmd.Parameters.AddWithValue("@FollowCalender", sFollowCalender);
                    cmd.Parameters.AddWithValue("@MonthTotal", dMonthTotal);
                    cmd.Parameters.AddWithValue("@HasDiscount", sHasDiscount);
                    cmd.Parameters.AddWithValue("@DiscountAmount", dDiscountAmount);
                    cmd.Parameters.AddWithValue("@DiscountHour", iDiscountHour);
                    cmd.Parameters.AddWithValue("@IsTaxable", sIsTaxable);
                    cmd.Parameters.AddWithValue("@TaxAmount", dTaxAmount);
                    cmd.Parameters.AddWithValue("@LASTUPDATE", dtLASTUPDATE);
                    cmd.Parameters.AddWithValue("@LastUpdatedBy", CurrentUser);
                    cmd.Parameters.AddWithValue("@Category", sCategory);
                    cmd.Parameters.AddWithValue("@Reason", sReason);
                    sdaFactory.StartTransaction(Transaction);
                    sdaFactory.ExecuteSQL(cmd);
                    if (Transaction == null)
                        sdaFactory.EndTransaction(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Save(SQLDataAccess sdaFactory, SqlTransaction Transaction, string CurrentUser)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = " UPDATE AgreementDetails SET [AgreementID] = @AgreementID,[AgreementDate] = @AgreementDate,[Client] = @Client,[Branch] = @Branch,[Description] = @Description,[NoOfGuards] = @NoOfGuards,[PerDay] = @PerDay,[PerMonth] = @PerMonth,[Rate] = @Rate,[NoOfHours] = @NoOfHours,[NoOfDays] = @NoOfDays,[FollowCalender] = @FollowCalender,[MonthTotal]=@MonthTotal[HasDiscount] = @HasDiscount,[DiscountAmount] = @DiscountAmount,[DiscountHour]=@DiscountHour,[IsTaxable] = @IsTaxable,[TaxAmount] = @TaxAmount,[LASTUPDATE] = @LASTUPDATE, [LastUpdatedBy]=@LastUpdatedBy, [Category]=@Category, [Reason] = @Reason WHERE [ID] = @ID";
                    cmd.Parameters.AddWithValue("@ID", dID);
                    cmd.Parameters.AddWithValue("@AgreementID", dAgreementID);
                    cmd.Parameters.AddWithValue("@AgreementDate", dtAgreementDate);
                    cmd.Parameters.AddWithValue("@Client", sClient);
                    cmd.Parameters.AddWithValue("@Branch", sBranch);
                    cmd.Parameters.AddWithValue("@Description", sDescription);
                    cmd.Parameters.AddWithValue("@NoOfGuards", iNoOfGuards);
                    cmd.Parameters.AddWithValue("@PerDay", dPerDay);
                    cmd.Parameters.AddWithValue("@PerMonth", dPerMonth);
                    cmd.Parameters.AddWithValue("@Rate", dRate);
                    cmd.Parameters.AddWithValue("@NoOfHours", dNoOfHours);
                    cmd.Parameters.AddWithValue("@NoOfDays", dNoOfDays);
                    cmd.Parameters.AddWithValue("@FollowCalender", sFollowCalender);
                    cmd.Parameters.AddWithValue("@MonthTotal", dMonthTotal);
                    cmd.Parameters.AddWithValue("@HasDiscount", sHasDiscount);
                    cmd.Parameters.AddWithValue("@DiscountAmount", dDiscountAmount);
                    cmd.Parameters.AddWithValue("@DiscountHour", iDiscountHour);
                    cmd.Parameters.AddWithValue("@IsTaxable", sIsTaxable);
                    cmd.Parameters.AddWithValue("@TaxAmount", dTaxAmount);
                    cmd.Parameters.AddWithValue("@LASTUPDATE", dtLASTUPDATE);
                    cmd.Parameters.AddWithValue("@LastUpdatedBy", CurrentUser);
                    cmd.Parameters.AddWithValue("@Category", sCategory);
                    cmd.Parameters.AddWithValue("@Reason", sReason);
                    sdaFactory.StartTransaction(Transaction);
                    sdaFactory.ExecuteSQL(cmd);
                    if (Transaction == null)
                        sdaFactory.EndTransaction(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Delete(SQLDataAccess sdaFactory, SqlTransaction Transaction)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = " DELETE FROM AgreementDetails WHERE   [ID] = @ID";
                    cmd.Parameters.AddWithValue("@ID", dID);
                    sdaFactory.StartTransaction(Transaction);
                    sdaFactory.ExecuteSQL(cmd);
                    if (Transaction == null)
                        sdaFactory.EndTransaction(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
