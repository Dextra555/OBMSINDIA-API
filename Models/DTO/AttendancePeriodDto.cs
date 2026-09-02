namespace OBMS.WebAPI.Models.DTO
{
    /// <summary>API response for a resolved attendance period.</summary>
    public class AttendancePeriodDto
    {
        public DateTime StartDate  { get; set; }
        public DateTime EndDate    { get; set; }
        public DateTime PeriodKey  { get; set; }
        public bool     IsCustom   { get; set; }
        public int      TotalDays  { get; set; }
        public string   Label      { get; set; } = string.Empty;
    }

    /// <summary>Request/response for client attendance period config CRUD.</summary>
    public class ClientAttendancePeriodConfigDto
    {
        public int    ID             { get; set; }
        public string ClientCode     { get; set; } = string.Empty;
        public int    PeriodStartDay { get; set; } = 1;
        public int    PeriodEndDay   { get; set; } = 0;
        public bool   IsCustomPeriod { get; set; } = false;
        public string? LastUpdatedBy { get; set; }
        public string? PreviewLabel  { get; set; }
    }
}
