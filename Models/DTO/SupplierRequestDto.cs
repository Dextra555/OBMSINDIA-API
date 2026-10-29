namespace OBMS.WebAPI.Models.DTO
{
    public class SupplierRequestDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public decimal CreditLimit { get; set; }
        public string Status { get; set; }
        public string ContactPerson { get; set; }
        public string Category { get; set; }
        public DateTime LASTUPDATE { get; set; }
        public string? LastUpdatedBy { get; set; }
        public bool IsTax { get; set; }
        public string? GSTNo { get; set; }
        public string? PanNo { get; set; }
        public decimal? GSTPercentage { get; set; }
        public bool IsTDS { get; set; }
        public decimal? TDSPercentage { get; set; }
    }
}
