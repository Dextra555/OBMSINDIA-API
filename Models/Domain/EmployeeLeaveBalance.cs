using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    /// <summary>
    /// Per-employee, per-period leave balance snapshot.
    /// Persists the taken/remaining values for Annual, Medical, Maternity,
    /// Paternity and Hospitalization leave so the UI can read from a table
    /// instead of computing the values on-the-fly.
    /// </summary>
    [Table("EmployeeLeaveBalance")]
    public class EmployeeLeaveBalance
    {
        [Key]
        public int ID { get; set; }

        public int EmployeeID { get; set; }

        /// <summary>Attendance period (month) this balance applies to.</summary>
        public DateTime Period { get; set; }

        public int AnnualLeaveTaken { get; set; }
        public int AnnualLeaveAvailable { get; set; }

        public int MedicalLeaveTaken { get; set; }
        public int MedicalLeaveAvailable { get; set; }

        public int MaternityLeaveTaken { get; set; }
        public int MaternityLeaveAvailable { get; set; }

        public int PaternityLeaveTaken { get; set; }
        public int PaternityLeaveAvailable { get; set; }

        public int HospitalizationLeaveTaken { get; set; }
        public int HospitalizationLeaveAvailable { get; set; }

        public DateTime LastUpdate { get; set; }
        public string? LastUpdatedBy { get; set; }
    }
}
