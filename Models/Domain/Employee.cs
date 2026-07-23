using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OBMS.WebAPI.Models.DTO;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("Employee")]
    public class Employee
    {

        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EMP_ID { get; set; }

        //[StringLength(50)]
        public string EMP_ROLE { get; set; }

        //[StringLength(50)]
        public string EMP_CODE { get; set; }

        //[StringLength(50)]
        public string EMP_NAME { get; set; }

        //[StringLength(100)]
        public string EMP_ADDRESS1 { get; set; }

        //[StringLength(100)]
        public string EMP_ADDRESS2 { get; set; }

        //[StringLength(5)]
        public string EMP_POST_CODE { get; set; }

        //[StringLength(50)]
        public string EMP_TOWN { get; set; }

        //[StringLength(30)]
        public string EMP_STATE { get; set; }

        //[StringLength(50)]
        public string? EMP_NATIONAL { get; set; }

        //[StringLength(20)]
        public string EMP_PHONE { get; set; }

        //[StringLength(50)]
        public string EMP_HGH_EDU { get; set; }

        //[StringLength(50)]
        public string EM_WORK_EXP { get; set; }

        public DateTime? EMP_DATE_OF_BIRTH { get; set; }

        //[StringLength(20)]
        public string EMP_IC_OLD { get; set; }

        //[StringLength(20)]
        public string EMP_IC_NEW { get; set; }

        //[StringLength(25)]
        public string EMP_IC_COLOR { get; set; }

        //[StringLength(30)]
        public string EMP_PASSPORT_NO { get; set; }

        //[StringLength(50)]
        public string EMP_SEX { get; set; }

        [StringLength(25)]
        public string EMP_RACE { get; set; }

        //[StringLength(10)]
        public string EMP_MARTIAL_STATUS { get; set; }

        //[StringLength(50)]
        public string? EMP_SPOUSE_NAME { get; set; }

        [StringLength(50)]
        [Column("EMP_FATHER_NAME")]
        public string? EMP_FATHER_NAME { get; set; }

        //[StringLength(20)]
        public string? EMP_SP_IC { get; set; }

        public int? EMP_NO_CHILD { get; set; }

        public bool EMP_SP_WORK { get; set; }

        //[StringLength(50)]
        public string? EMP_PER_NAME_CONTACT { get; set; }

        //[StringLength(100)]
        public string EMP_CONTACT_ADDRESS1 { get; set; }

        //[StringLength(100)]
        public string EMP_CONTACT_ADDRESS2 { get; set; }

        //[StringLength(5)]
        public string EMP_CONTACT_POST_CODE { get; set; }

        //[StringLength(50)]
        public string EMP_CONTACT_TOWN { get; set; }

        //[StringLength(30)]
        public string EMP_CONTACT_STATE { get; set; }

        //[StringLength(20)]
        public string EMP_CONTACT_TELEPHONE { get; set; }

        //[StringLength(20)]
        public string EMP_BRANCH_CODE { get; set; }

        //[StringLength(20)]
        public string? OldBranch { get; set; }

        public DateTime? TransferDate { get; set; }

        public bool HasTransfered { get; set; }

        public DateTime LASTUPDATE { get; set; }

        //[StringLength(20)]
        public string? LastUpdatedBy { get; set; }

        //[StringLength(20)]
        public string EMP_MOBILEPHONE { get; set; }

        public int EMP_CITIZEN { get; set; }

        public int EMP_CHECKLIST { get; set; }

        //[StringLength(10)]
        public string? EMP_CLIENT { get; set; }

        //[StringLength(1)]
        public char? NewSalaryStructure { get; set; }

        public bool KDNVetting { get; set; }

        //[StringLength(1)]
        public char? SalaryStructure1000_3h { get; set; }

        // Indian Compliance Fields
        [StringLength(12)]
        [RegularExpression(@"^[2-9][0-9]{11}$", ErrorMessage = "Aadhaar number must be 12 digits starting with 2-9")]
        [Column("Aadhaar")]
        public string? AadhaarNumber { get; set; }

        [StringLength(10)]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "PAN number must be in format ABCDE1234F")]
        [Column("PAN")]
        public string? PANNumber { get; set; }

        [StringLength(26)]
        [Column("PF_AccountNumber")]
        public string? PFAccountNumber { get; set; }

        [StringLength(17)]
        [Column("ESI_Number")]
        public string? ESINumber { get; set; }

        [StringLength(20)]
        public string? SalaryGroup { get; set; }

        [StringLength(10)]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Spouse PAN must be in format ABCDE1234F")]
        public string? SpousePAN { get; set; }

        [StringLength(12)]
        [RegularExpression(@"^[2-9][0-9]{11}$", ErrorMessage = "Spouse Aadhaar must be 12 digits starting with 2-9")]
        public string? SpouseAadhaar { get; set; }

        [StringLength(50)]
        public string? IndianState { get; set; }

        [StringLength(34)]
        public string? BankAccountNumber { get; set; }

        [StringLength(11)]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "IFSC code must be in format ABCD0XXXXXX")]
        [Column("IFSC_Code")]
        public string? BankIFSC { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? UPIId { get; set; }

        // Additional database columns
        [StringLength(50)]
        [Column("BankBranch")]
        public string? BankBranch { get; set; }

        [StringLength(50)]
        [Column("ProfessionalTax_State")]
        public string? ProfessionalTaxState { get; set; }

        [StringLength(50)]
        [Column("UAN_Number")]
        public string? UANNumber { get; set; }

        // Missing property that PayrollRepository expects
        public decimal EMP_BASIC_RATE { get; set; }

        // Missing properties for Department and Designation
        public int? DepartmentId { get; set; }
        public int? DesignationId { get; set; }

        // Department and Designation Names (for display purposes, not mapped to database)
        [NotMapped]
        public string? DepartmentName { get; set; }
        [NotMapped]
        public string? DesignationName { get; set; }

        // Foreign key navigation properties
        public virtual Department? Department { get; set; }
        public virtual Designation? Designation { get; set; }

        // CB (Cost Breakdown) Properties - Simplified
        [Column("CB_Basic")]
        public decimal? CB_Basic { get; set; }
        [Column("CB_DA")]
        public decimal? CB_DA { get; set; }
        [Column("CB_HRA")]
        public decimal? CB_HRA { get; set; }
        [Column("CB_HRAPercentage")]
        public decimal? CB_HRAPercentage { get; set; }
        [Column("CB_Leaves")]
        public decimal? CB_Leaves { get; set; }
        [Column("CB_LeavesPercentage")]
        public decimal? CB_LeavesPercentage { get; set; }
        [Column("CB_OtherAllowances")]
        public decimal? CB_OtherAllowances { get; set; }
        [Column("CB_NH")]
        public decimal? CB_NH { get; set; }
        [Column("CB_NHPercentage")]
        public decimal? CB_NHPercentage { get; set; }
        [Column("CB_AdvanceStatutoryBonus")]
        public decimal? CB_AdvanceStatutoryBonus { get; set; }
        [Column("CB_AdvanceStatutoryBonusPercentage")]
        public decimal? CB_AdvanceStatutoryBonusPercentage { get; set; }
        [Column("CB_SubTotal")]
        public decimal? CB_SubTotal { get; set; }

    }
}
