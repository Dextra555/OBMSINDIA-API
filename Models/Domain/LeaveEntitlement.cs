using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    /// <summary>
    /// Stores India-specific fixed leave entitlement values.
    /// A single row (LE_ID = 1) acts as the system-wide default configuration.
    /// Leave values are now sourced from the Malaysia-style LeaveSystem slab table;
    /// this table is retained only for legacy reference.
    /// </summary>
    [Table("LeaveEntitlement")]
    public class LeaveEntitlement
    {
        [Key]
        public int LE_ID { get; set; }

        public int AnnualLeave { get; set; } = 0;

        public int MedicalLeave { get; set; } = 0;

        public int MaternityLeave { get; set; } = 0;

        public int PaternityLeave { get; set; } = 0;

        public int HospitalizationLeave { get; set; } = 0;

        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public string? LastUpdatedBy { get; set; }
    }
}
