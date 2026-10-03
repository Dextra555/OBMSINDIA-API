using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("PaySlip")]
    public class PaySlip
    {
        [Key]
        public int ID { get; set; }
        public DateTime Period { get; set; }
        public int EmployeeID { get; set; }
        public decimal? BasicSalaryDays { get; set; }
        public decimal BasicSalaryRate { get; set; }
        public decimal BasicSalary { get; set; }
        public int OverTimeSalaryHours { get; set; }
        public decimal OverTimeSalaryRate { get; set; }
        public decimal OverTimeSalary { get; set; }
        public decimal? OffDaySalaryDays { get; set; }
        public decimal OffDaySalaryRate { get; set; }
        public decimal OffDaySalary { get; set; }
        public int OffDayOverTimeSalaryHours { get; set; }
        public decimal OffDayOverTimeSalaryRate { get; set; }
        public decimal OffDayOverTimeSalary { get; set; }
        public decimal? HolidaySalaryDays { get; set; }
        public decimal HolidaySalaryRate { get; set; }
        public decimal HolidaySalary { get; set; }
        public int HolidayOverTimeSalaryHours { get; set; }
        public decimal HolidayOverTimeSalaryRate { get; set; }
        public decimal HolidayOverTimeSalary { get; set; }
        public decimal Shift2SalaryDaysHours { get; set; }
        public int Shift2SalaryRateType { get; set; }
        public decimal Shift2SalaryRate { get; set; }
        public decimal Shift2Salary { get; set; }
        public decimal? ReAllowanceDays { get; set; }
        public decimal ReAllowance { get; set; }
        public decimal AttendanceAllowance { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal EPFDeductionAmount { get; set; }
        public decimal SOCSODeductionAmount { get; set; }
        public decimal EPFEmployerContribution { get; set; }
        public decimal SOCSOEmployerContribution { get; set; }
        public decimal DailyAdvanceRecovery { get; set; }
        public decimal SpecialAdvanceRecovery { get; set; }
        public decimal MonthlyAdvanceRecovery { get; set; }
        public decimal? UniformIssueRecovery { get; set; }
        public decimal LoanRecovery { get; set; }
        public decimal IncomeTaxDeduction { get; set; }
        public decimal MiscAmount { get; set; }
        public decimal MiscDeduction { get; set; }
        public string SalaryPayMode { get; set; }
        public decimal Bonus { get; set; }
        public DateTime LastUpdate { get; set; }
        public string LastUpdatedBy { get; set; }
        public decimal? SIPEmployeeContribution { get; set; }
        public decimal? SIPEmployerContribution { get; set; }

        /// <summary>
        /// Professional Tax calculated from ProfessionalTaxConfiguration slabs
        /// during salary processing. Crystal Report reads this directly.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal PTax { get; set; } = 0;

        /// <summary>
        /// Per-day salary rate = (CB_Basic + CB_DA + CB_HRA + CB_OtherAllowances +
        /// CB_AdvanceStatutoryBonus + CB_Leaves + CB_NH) / CalendarDaysInMonth.
        /// Matches Crystal Report {@PerDaySalary} formula exactly.
        /// Stored with 4 decimal places for accuracy (e.g. 30000 / 31 = 967.7419).
        /// </summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal PerDaySalary { get; set; } = 0;

        /// <summary>
        /// Earned salary = BasicSalaryDays × PerDaySalary.
        /// Matches Crystal Report {@EarnedSalary} formula exactly.
        /// Used as the salary basis for PTax slab matching.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal EarnedSalary { get; set; } = 0;

        /// <summary>
        /// Employee PF contribution calculated from PFConfiguration during salary processing.
        /// Formula (matches Crystal Report):
        ///   pfWage = (CB_Basic + CB_DA + CB_OtherAllowances) / CalendarDays × WorkedDays
        ///   capped = Min(pfWage, BasicSalaryLimit)   [₹15,000]
        ///   PF     = Round(capped × EmployeeRate / 100, 0)   [12%]
        /// Eligibility: epfdetect=true AND (compliance_client OR Staff)
        /// Crystal Report reads {vwPaySheet.PF} directly.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal PF { get; set; } = 0;

        /// <summary>
        /// PF wage used for the PF calculation — after proration and before/after cap.
        /// Stored for audit/debugging. Crystal Report does not need this directly.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal PFWage { get; set; } = 0;

        /// <summary>
        /// Employee ESI contribution calculated from ESIConfiguration during salary processing.
        /// Formula (matches Crystal Report):
        ///   esiWage = EarnedSalary − CB_AdvanceStatutoryBonus
        ///   if esiWage &lt; BasicSalaryLimit (₹21,000)
        ///       ESI = Round(esiWage × EmployeeRate / 100, 0)   [0.75%]
        ///   else ESI = 0
        /// Eligibility: SOCSODETECT=true AND (compliance_client OR Staff)
        /// Crystal Report reads {vwPaySheet.ESI} directly.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal ESI { get; set; } = 0;

        /// <summary>
        /// ESI wage = EarnedSalary − CB_AdvanceStatutoryBonus.
        /// Stored for audit/debugging.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal ESIWage { get; set; } = 0;

        /// <summary>
        /// Full-month gross pay = CB_Basic + CB_DA + CB_HRA + CB_OtherAllowances +
        /// CB_AdvanceStatutoryBonus + CB_Leaves + CB_NH.
        /// Matches Crystal Report {@gross} formula exactly.
        /// Not prorated by attendance days — this is the full contracted month amount.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal GrossPay { get; set; } = 0;

        /// <summary>
        /// Labour Welfare Fund deduction.
        /// Matches Crystal Report LWF formula:
        ///   December → ₹30, all other months → ₹0.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal LWF { get; set; } = 0;

        /// <summary>
        /// Total deduction = PF + ESI + DailyAdvanceRecovery + MonthlyAdvanceRecovery +
        ///                   UniformIssueRecovery + LoanRecovery + MiscDeduction +
        ///                   MonthLeave (GrossPay - EarnedSalary) + PTax + LWF.
        /// Matches Crystal Report {@TotalDeduction} formula exactly.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalDeduction { get; set; } = 0;

        /// <summary>
        /// Net Pay = GrossPay - TotalDeduction.
        /// Matches Crystal Report NetPay formula exactly.
        /// Crystal Report reads {vwPaySheet.NetPay} directly.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal NetPay { get; set; } = 0;
    }
}
