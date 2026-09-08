namespace OBMS.WebAPI.Models.DTO
{
    public class EmployeeLeaveBalanceDto
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
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
