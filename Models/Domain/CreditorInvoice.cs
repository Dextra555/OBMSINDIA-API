using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("CreditorInvoice")]
    public class CreditorInvoice
    {
        [Key]
        public int ID { get; set; }
        public string Branch { get; set; }
        public int Supplier { get; set; }
        public int? RecID { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceType { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime LASTUPDATE { get; set; }
        public string LastUpdatedBy { get; set; }
        public int? ItemCategory { get; set; }
        public bool IsDeleted { get; set; }

        // GST fields (header level — % entered, amount auto-calculated but editable)
        public decimal GSTPercentage { get; set; } = 0;
        public decimal GSTAmount { get; set; } = 0;

        // TDS fields (header level — % entered, amount auto-calculated but editable)
        public decimal TDSPercentage { get; set; } = 0;
        public decimal TDSAmount { get; set; } = 0;
    }

}
