using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface IGSTCalculationService
    {
        GSTCalculationResult CalculateGST(decimal amount, string hsnCode, string supplierStateCode, string recipientStateCode, DateTime invoiceDate);
        GSTCalculationResult CalculateGSTByStateNames(decimal amount, string hsnCode, string supplierState, string recipientState, DateTime invoiceDate);
        List<GSTRate> GetApplicableGSTRates(DateTime invoiceDate);
    }

    public class GSTCalculationResult
    {
        public decimal Amount { get; set; }
        public string HSNCode { get; set; }
        public string SupplierState { get; set; }
        public string RecipientState { get; set; }
        public decimal GSTRate { get; set; }
        public bool IsIntraState { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TotalGST { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? GSTDescription { get; set; }
    }

    public class GSTRate
    {
        public decimal Rate { get; set; }
        public string HSNCode { get; set; }
        public string? Description { get; set; }
    }
}
