using System.ComponentModel.DataAnnotations;

namespace OBMS.WebAPI.Models.DTO
{
    public class QuotationRequestDto
    {
        public int ID { get; set; }
        public string Branch { get; set; }
        public string Client { get; set; }
        public DateTime QuotationDate { get; set; }
        public DateTime QuotationEndDate { get; set; }
        public string Note { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime LASTUPDATE { get; set; }
        public string? LastUpdatedBy { get; set; }
        public bool? IsValid { get; set; }
        public QuotationDetailsRequestDto[]? quotationDetails { get; set; }

    }
}
