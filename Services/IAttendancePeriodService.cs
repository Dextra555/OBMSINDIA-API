namespace OBMS.WebAPI.Services
{
    public class AttendancePeriodResult
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }
        /// <summary>Always last day of reference month — stored in Attendance.Period (DB backward compatible).</summary>
        public DateTime PeriodKey { get; set; }
        public bool     IsCustom  { get; set; }
        public int      TotalDays => (int)(EndDate - StartDate).TotalDays + 1;
    }

    public interface IAttendancePeriodService
    {
        Task<AttendancePeriodResult> GetAttendancePeriodAsync(string clientCode, int year, int month);
        AttendancePeriodResult GetAttendancePeriod(string clientCode, int year, int month);
        AttendancePeriodResult GetCalendarMonthPeriod(int year, int month);
    }
}
