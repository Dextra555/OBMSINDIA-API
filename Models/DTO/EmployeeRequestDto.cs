using System.ComponentModel.DataAnnotations;

using System.Data.SqlTypes;



namespace OBMS.WebAPI.Models.DTO

{

    public class EmployeeRequestDto

    {



        public int EMP_ID { get; set; }



        public string EMP_ROLE { get; set; }



        public string EMP_CODE { get; set; }



        public string EMP_NAME { get; set; }



        public string EMP_ADDRESS1 { get; set; }



        public string EMP_ADDRESS2 { get; set; }



        public string EMP_POST_CODE { get; set; }



        public string EMP_TOWN { get; set; }



        public string EMP_STATE { get; set; }



        public string EMP_NATIONAL { get; set; }



        public string EMP_PHONE { get; set; }



        public string EMP_HGH_EDU { get; set; }



        public string EM_WORK_EXP { get; set; }



        public DateTime? EMP_DATE_OF_BIRTH { get; set; }



        public string EMP_IC_OLD { get; set; }



        public string EMP_IC_NEW { get; set; }



        public string EMP_IC_COLOR { get; set; }



        public string EMP_PASSPORT_NO { get; set; }



        public string EMP_SEX { get; set; }



        public string EMP_RACE { get; set; }



        public string EMP_MARTIAL_STATUS { get; set; }



        public string EMP_SPOUSE_NAME { get; set; }



        public string? EMP_FATHER_NAME { get; set; }



        public string EMP_SP_IC { get; set; }



        public int EMP_NO_CHILD { get; set; }



        public bool EMP_SP_WORK { get; set; }



        public string EMP_PER_NAME_CONTACT { get; set; }



        public string EMP_CONTACT_ADDRESS1 { get; set; }



        public string EMP_CONTACT_ADDRESS2 { get; set; }



        public string EMP_CONTACT_POST_CODE { get; set; }



        public string EMP_CONTACT_TOWN { get; set; }



        public string EMP_CONTACT_STATE { get; set; }



        public string EMP_CONTACT_TELEPHONE { get; set; }



        public string EMP_BRANCH_CODE { get; set; }



        public string OldBranch { get; set; }



        public DateTime? TransferDate { get; set; }



        public bool HasTransfered { get; set; }



        public DateTime LASTUPDATE { get; set; }



        public string LastUpdatedBy { get; set; }



        public string EMP_MOBILEPHONE { get; set; }



        public int EMP_CITIZEN { get; set; }



        public int EMP_CHECKLIST { get; set; }



        public string EMP_CLIENT { get; set; }



        public string? NewSalaryStructure { get; set; }



        public bool KDNVetting { get; set; }



        public string? SalaryStructure1000_3h { get; set; }





        public int EMPPAY_ID { get; set; }

        public string EMPPAY_JOB_TITLE { get; set; }

        public string EMPPAY_CATEGORY { get; set; }

        public DateTime? EMPPAY_DATE_JOINED { get; set; }

        public DateTime? EMPPAY_DATE_CONFIRM { get; set; }

        public DateTime? EMPPAY_DATE_PROMOTION { get; set; }

        public DateTime? EMPPAY_DATE_RESIGNED { get; set; }

        public double EMPPAY_BASIC_RATE { get; set; }

        public decimal SALARYLAB { get; set; }

        public decimal? ATTENDANCEALLOWANCE { get; set; }

        public decimal NewStructureATTENDANCEALLOWANCE { get; set; }

        public decimal SpecialAllowance { get; set; }

        public decimal? AttendanceAllowanceWorkingDays { get; set; }

        public string? AttendanceAllowanceFollowCalendar { get; set; } = "N";





        public int EMPFL_ID { get; set; }

        public string? EMPFL_BANK { get; set; }

        public string? EMPFL_BK_ACCNO { get; set; }

        public string? EMPFL_TAX_NO { get; set; }

        public string? EMPFL_EPFNO { get; set; }

        public bool? EMPFL_EPF8Pa { get; set; }

        public string? EMPFL_SOSCO_NO { get; set; }

        public bool EPFDETECT { get; set; }

        public string PAYMODE { get; set; }

        public bool SOCSODETECT { get; set; }

        public bool TMPGUARD { get; set; }

        public bool DETECTBYND55 { get; set; }

        public bool? INCOMETAXDETECT { get; set; }

        public string? EMP_SP_TEL_NO { get; set; }

        // Indian Compliance Fields
        public string? AadhaarNumber { get; set; }

        public string? PANNumber { get; set; }

        public string? PFAccountNumber { get; set; }

        public string? ESINumber { get; set; }

        public string? SalaryGroup { get; set; }

        public string? SpousePAN { get; set; }

        public string? SpouseAadhaar { get; set; }

        public string? IndianState { get; set; }

        public string? BankAccountNumber { get; set; }

        public string? BankIFSC { get; set; }

        public string? BankName { get; set; }

        public string? UPIId { get; set; }

        // Missing Department and Designation properties
        public int? DepartmentId { get; set; }
        public int? DesignationId { get; set; }

        // CB (Cost Breakdown) Properties - Simplified
        public decimal? CB_Basic { get; set; }
        public decimal? CB_DA { get; set; }
        public decimal? CB_HRA { get; set; }
        public decimal? CB_HRAPercentage { get; set; }
        public decimal? CB_Leaves { get; set; }
        public decimal? CB_LeavesPercentage { get; set; }
        public decimal? CB_OtherAllowances { get; set; }
        public decimal? CB_NH { get; set; }
        public decimal? CB_NHPercentage { get; set; }
        public decimal? CB_AdvanceStatutoryBonus { get; set; }
        public decimal? CB_AdvanceStatutoryBonusPercentage { get; set; }
        public decimal? CB_SubTotal { get; set; }

        // ── Branch Transfer fields ──────────────────────────────────────
        /// <summary>True when the user changed the branch in edit mode.</summary>
        public bool IsBranchChanged { get; set; }

        /// <summary>
        /// Effective start date at the new branch.
        /// Required when IsBranchChanged = true.
        /// Stored as Emp_StartDate on the new EmployeeHistory row.
        /// </summary>
        public DateTime? BranchStartDate { get; set; }

    }

}

