namespace OBMS.WebAPI.Models.DTO
{
    public class TDSMasterRequestDto
    {
        public int ID { get; set; }
        public string TDSName { get; set; } = null!;
        public decimal TDSPercentage { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
