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

        // GST fields — kept for backward compatibility (Finance payments references GSTAmount)
        public decimal GSTPercentage { get; set; } = 0;
        public decimal GSTAmount { get; set; } = 0;

        // Transaction type: 'I' = Intra-state (CGST+SGST), 'E' = Inter-state (IGST)
        public string TransactionType { get; set; } = "I";

        // CGST / SGST — intra-state
        public decimal CGSTPercentage { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;
        public decimal SGSTPercentage { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;

        // IGST — inter-state
        public decimal IGSTPercentage { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;

        // TDS fields (header level — % entered, amount auto-calculated but editable)
        public decimal TDSPercentage { get; set; } = 0;
        public decimal TDSAmount { get; set; } = 0;
    }

}
