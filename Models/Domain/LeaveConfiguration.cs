using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("LeaveConfiguration")]
    public class LeaveConfiguration
    {
        [Key]
        public int ID { get; set; }

        public string LeaveType { get; set; } = string.Empty;

        public int Entitlement { get; set; }

        public string Country { get; set; } = "India";

        public DateTime LastUpdate { get; set; }

        public string? LastUpdatedBy { get; set; }
    }
}
