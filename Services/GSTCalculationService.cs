using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;

namespace OBMS.WebAPI.Services
{
    public class GSTCalculationService : IGSTCalculationService
    {
        private readonly OBMSDbContext _context;

        public GSTCalculationService(OBMSDbContext context)
        {
            _context = context;
        }

        public GSTCalculationResult CalculateGST(decimal amount, string hsnCode, string supplierStateCode, string recipientStateCode, DateTime invoiceDate)
        {
            var result = new GSTCalculationResult
            {
                Amount = amount,
                HSNCode = hsnCode,
                SupplierState = supplierStateCode,
                RecipientState = recipientStateCode,
                InvoiceDate = invoiceDate
            };

            var gstConfig = GetApplicableGSTRate(hsnCode, invoiceDate);
            if (gstConfig == null)
            {
                return new GSTCalculationResult
                {
                    Amount = amount,
                    HSNCode = hsnCode,
                    SupplierState = supplierStateCode,
                    RecipientState = recipientStateCode,
                    GSTRate = 0,
                    IsIntraState = false,
                    CGST = 0,
                    SGST = 0,
                    IGST = 0,
                    TotalGST = 0,
                    TotalAmount = amount,
                    InvoiceDate = invoiceDate,
                    GSTDescription = "GST Not Applicable"
                };
            }

            result.GSTRate = gstConfig.GSTRate;
            result.GSTDescription = gstConfig.Description;
            result.IsIntraState = supplierStateCode == recipientStateCode;
            result.TotalGST = amount * gstConfig.GSTRate / 100;

            if (result.IsIntraState)
            {
                // Intra-state transaction: CGST + SGST
                result.CGST = result.TotalGST / 2;
                result.SGST = result.TotalGST / 2;
                result.IGST = 0;
            }
            else
            {
                // Inter-state transaction: IGST
                result.CGST = 0;
                result.SGST = 0;
                result.IGST = result.TotalGST;
            }

            result.TotalAmount = amount + result.TotalGST;

            return result;
        }

        public GSTCalculationResult CalculateGSTByStateNames(decimal amount, string hsnCode, string supplierState, string recipientState, DateTime invoiceDate)
        {
            var supplierStateCode = GetStateCodeByName(supplierState);
            var recipientStateCode = GetStateCodeByName(recipientState);
            
            return CalculateGST(amount, hsnCode, supplierStateCode, recipientStateCode, invoiceDate);
        }

        public List<GSTRate> GetApplicableGSTRates(DateTime invoiceDate)
        {
            return _context.Set<GSTConfiguration>()
                .Where(c => c.IsActive)
                .Select(c => new GSTRate
                {
                    Rate = c.GSTRate,
                    HSNCode = c.HSNCode,
                    Description = c.Description
                })
                .Distinct()
                .ToList();
        }

        private GSTConfiguration GetApplicableGSTRate(string hsnCode, DateTime invoiceDate)
        {
            return _context.Set<GSTConfiguration>()
                .Where(c => c.IsActive && 
                           c.HSNCode == hsnCode)
                .FirstOrDefault();
        }

        private string GetStateCodeByName(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
                return string.Empty;

            // For now, return empty string as IndianState table doesn't exist
            // TODO: Implement state code mapping when IndianState table is created
            return string.Empty;
        }

    }
}
