using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;

namespace OBMS.WebAPI.Services
{
    /// <summary>
    /// Central attendance period calculator.
    ///
    /// IsCustomPeriod = false  →  StartDate=1st, EndDate=last day, PeriodKey=last day (calendar month)
    /// IsCustomPeriod = true   →  e.g. StartDay=25, EndDay=26, refMonth=Aug 2026
    ///   → StartDate = 25-Jul-2026, EndDate = 26-Aug-2026, PeriodKey = 31-Aug-2026
    /// </summary>
    public class AttendancePeriodService : IAttendancePeriodService
    {
        private readonly OBMSDbContext _db;
        public AttendancePeriodService(OBMSDbContext db) => _db = db;

        public async Task<AttendancePeriodResult> GetAttendancePeriodAsync(string clientCode, int year, int month)
        {
            if (string.IsNullOrWhiteSpace(clientCode))
                return CalendarMonth(year, month);

            var config = await _db.ClientAttendancePeriods
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientCode == clientCode);

            return Compute(config, year, month);
        }

        public AttendancePeriodResult GetAttendancePeriod(string clientCode, int year, int month)
        {
            if (string.IsNullOrWhiteSpace(clientCode))
                return CalendarMonth(year, month);

            var config = _db.ClientAttendancePeriods
                .AsNoTracking()
                .FirstOrDefault(c => c.ClientCode == clientCode);

            return Compute(config, year, month);
        }

        public AttendancePeriodResult GetCalendarMonthPeriod(int year, int month)
            => CalendarMonth(year, month);

        // ── Helpers ─────────────────────────────────────────────────────────

        private static AttendancePeriodResult Compute(
            Models.Domain.ClientAttendancePeriod? cfg, int year, int month)
        {
            if (cfg == null || !cfg.IsCustomPeriod)
                return CalendarMonth(year, month);

            return CustomPeriod(cfg.PeriodStartDay, cfg.PeriodEndDay, year, month);
        }

        private static AttendancePeriodResult CalendarMonth(int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end   = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return new AttendancePeriodResult { StartDate = start, EndDate = end, PeriodKey = end, IsCustom = false };
        }

        private static AttendancePeriodResult CustomPeriod(int startDay, int endDay, int refYear, int refMonth)
        {
            // Reference month end
            int refDays    = DateTime.DaysInMonth(refYear, refMonth);
            int clampedEnd = endDay == 0 ? refDays : Math.Min(endDay, refDays);
            var endDate    = new DateTime(refYear, refMonth, clampedEnd);
            var periodKey  = new DateTime(refYear, refMonth, refDays);

            // Previous month start
            int prevYear   = refMonth == 1 ? refYear - 1 : refYear;
            int prevMonth  = refMonth == 1 ? 12           : refMonth - 1;
            int prevDays   = DateTime.DaysInMonth(prevYear, prevMonth);
            var startDate  = new DateTime(prevYear, prevMonth, Math.Min(startDay, prevDays));

            return new AttendancePeriodResult
            {
                StartDate = startDate,
                EndDate   = endDate,
                PeriodKey = periodKey,
                IsCustom  = true
            };
        }
    }
}
