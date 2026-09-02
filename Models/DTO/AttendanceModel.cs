using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Models.DTO
{
    public class AttendanceModel
    {
        public Attendance? attendanceModel { get; set; }
        public List<AttendanceDetails>? attendanceDetails { get; set; }

        /// <summary>
        /// Client code used to resolve the custom attendance period.
        /// If null/empty, falls back to standard calendar month (backward compatible).
        /// </summary>
        public string? ClientCode { get; set; }
    }
}
