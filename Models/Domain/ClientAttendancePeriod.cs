using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    /// <summary>
    /// Per-client attendance period configuration.
    /// IsCustomPeriod = false  →  standard calendar month (backward compatible).
    /// IsCustomPeriod = true   →  PeriodStartDay of prev month to PeriodEndDay of reference month.
    ///   e.g. StartDay=25, EndDay=26, refMonth=Aug → 25-Jul to 26-Aug.
    /// </summary>
    [Table("ClientAttendancePeriod")]
    public class ClientAttendancePeriod
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        /// <summary>Day of month the cycle starts (1–28). Used only when IsCustomPeriod=true.</summary>
        public int PeriodStartDay { get; set; } = 1;

        /// <summary>Day of month the cycle ends (1–28). 0 = last day of reference month.</summary>
        public int PeriodEndDay { get; set; } = 0;

        /// <summary>false = standard calendar month. true = custom StartDay/EndDay.</summary>
        public bool IsCustomPeriod { get; set; } = false;

        public DateTime? LastUpdatedDate { get; set; }

        [MaxLength(100)]
        public string? LastUpdatedBy { get; set; }
    }
}
