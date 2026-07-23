using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.DTO
{
    public class GSTInvoiceDto
    {
        public int InvoiceId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string InvoiceNumber { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        [StringLength(15)]
        public string SupplierGSTIN { get; set; }

        [Required]
        [StringLength(15)]
        public string RecipientGSTIN { get; set; }

        [Required]
        [StringLength(50)]
        public string SupplierState { get; set; }

        [Required]
        [StringLength(50)]
        public string RecipientState { get; set; }

        [Required]
        [StringLength(8)]
        public string HSNCode { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemDescription { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxableAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal GSTRate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CGSTAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SGSTAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal IGSTAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalGSTAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(20)]
        public string PaymentMode { get; set; }

        public bool IsReverseCharge { get; set; }

        public string? Remarks { get; set; }
    }
}
