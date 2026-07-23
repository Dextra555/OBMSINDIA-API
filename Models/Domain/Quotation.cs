using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("Quotation")]
    public class Quotation
    {
        [Key]
        public int ID { get; set; }
        public string Branch { get; set; }
        public string Client { get; set; }

        [StringLength(2000)]
        public string? WorkPlace { get; set; }
        public DateTime QuotationDate { get; set; } = DateTime.Now;
        public DateTime QuotationEndDate { get; set; } = DateTime.Now.AddMonths(1);

        [StringLength(4000)]
        public string? Note { get; set; }
        public DateTime? AddedDate { get; set; } = DateTime.Now;
        public DateTime LASTUPDATE { get; set; } = DateTime.Now;
        public string? LastUpdatedBy { get; set; }

        public bool? IsValid { get; set; }
        public string? Status { get; set; }
    }
}
