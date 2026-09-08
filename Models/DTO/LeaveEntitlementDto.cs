namespace OBMS.WebAPI.Models.DTO
{
    public class LeaveEntitlementDto
    {
        public int LE_ID { get; set; }
        public int AnnualLeave { get; set; }
        public int MedicalLeave { get; set; }
        public int MaternityLeave { get; set; }
        public int PaternityLeave { get; set; }
        public int HospitalizationLeave { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? LastUpdatedBy { get; set; }
    }
}
