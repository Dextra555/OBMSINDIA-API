using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("Suppliers")]
    public class Supplier
    {
        [Key]
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

        [JsonPropertyName("GSTNo")]
        public string? GSTNo { get; set; }

        [JsonPropertyName("PanNo")]
        public string? PanNo { get; set; }

        public decimal? GSTPercentage { get; set; }
        public bool IsTDS { get; set; }
        public decimal? TDSPercentage { get; set; }
    }
}