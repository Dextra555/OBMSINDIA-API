using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Models.DTO
{
    public class AttendanceDetailsDto
    {
        public int ID { get; set; }
        public int AttendanceID { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string? Client { get; set; }
        public DateTime? TimeStart { get; set; }
        public DateTime? TimeEnd { get; set; }
        public string? OTClient { get; set; }
        public DateTime? OTTimeStart { get; set; }
        public DateTime? OTTimeEnd { get; set; }
        public string? Type { get; set; } // Accept string for API
        public DateTime LastUpdate { get; set; }
        public string? LastUpdatedBy { get; set; }

        // Convert to domain model
        public AttendanceDetails ToDomainModel()
        {
            return new AttendanceDetails
            {
                ID = this.ID,
                AttendanceID = this.AttendanceID,
                AttendanceDate = this.AttendanceDate,
                Client = this.Client,
                TimeStart = this.TimeStart,
                TimeEnd = this.TimeEnd,
                OTClient = this.OTClient,
                OTTimeStart = this.OTTimeStart,
                OTTimeEnd = this.OTTimeEnd,
                Type = ConvertWorkTypeToInt(this.Type),
                LastUpdate = this.LastUpdate,
                LastUpdatedBy = this.LastUpdatedBy
            };
        }

        private int ConvertWorkTypeToInt(string? typeName)
        {
            return typeName switch
            {
                "General Working" => 1,
                "Off Day" => 2,
                "Annual Leave" => 8,
                "Medical Leave" => 9,
                "Maternity Leave" => 10,
                "Paternity Leave" => 11,
                "Hospitalization Leave" => 12,
                "Rest Day" => 13,
                "Unpaid Leave" => 14,
                "Marriage Leave" => 17,
                _ => 1 // Default to General Working
            };
        }
    }
}
