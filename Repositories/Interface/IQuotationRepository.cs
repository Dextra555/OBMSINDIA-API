using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Repositories.Interface
{
    public interface IQuotationRepository
    {
        Task<List<Object>> GetQuotations(string branchId);
        Task<List<Object>> GetQuotationMasterList(string userID);
        Task SaveAndUpdateQuotation(Quotation quotation);
        Task SaveAndUpdateQuotationDetails(QuotationDetails quotationDetails);
        Task<Object> GetQuotationByID(int quotationID);
        Task<Object> CheckClientStatus(string branchId, string clientId, string status);
        Task<List<Object>> GetQuotationsDiscountReport(string branchId, string clientId, DateTime startdate, DateTime endDate);
    }
}
