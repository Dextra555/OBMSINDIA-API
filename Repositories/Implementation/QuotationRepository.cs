using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Repositories.Interface;

namespace OBMS.WebAPI.Repositories.Implementation
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly OBMSDbContext _oBMSDbContext;

        public QuotationRepository(OBMSDbContext oBMSDbContext)
        {
            _oBMSDbContext = oBMSDbContext;
        }

        public async Task<List<Object>> GetQuotations(string branchId)
        {
            if (branchId == "0")
            {
                var query = from t in _oBMSDbContext.Quotations
                            join client in _oBMSDbContext.ClientMasters on t.Client equals client.Code
                            join branch in _oBMSDbContext.BranchMasters on t.Branch equals branch.Code
                            where t.QuotationDate == _oBMSDbContext.Quotations
                                                         .Where(q => q.Client == t.Client && q.Branch == t.Branch)
                                                         .Max(q => q.QuotationDate)
                            select new
                            {
                                ID = t.ID,
                                BranchName = branch.Name,
                                ClientName = client.Name,
                                QuotationDate = t.QuotationDate,
                                AddedDate = t.AddedDate,
                                Status = t.Status
                            } into result
                            orderby result.AddedDate descending, result.ID descending
                            select result;
                return new List<Object>(query.ToList());
            }
            else
            {
                var query = from t in _oBMSDbContext.Quotations
                            join client in _oBMSDbContext.ClientMasters on t.Client equals client.Code
                            join branch in _oBMSDbContext.BranchMasters on t.Branch equals branch.Code
                            where t.QuotationDate == _oBMSDbContext.Quotations
                                                         .Where(q => q.Client == t.Client && q.Branch == t.Branch)
                                                         .Max(q => q.QuotationDate)
                                && t.Branch == branchId
                            select new
                            {
                                ID = t.ID,
                                BranchName = branch.Name,
                                ClientName = client.Name,
                                QuotationDate = t.QuotationDate,
                                AddedDate = t.AddedDate,
                                Status = t.Status
                            } into result
                            orderby result.AddedDate descending, result.ID descending
                            select result;
                return new List<Object>(query.ToList());
            }
        }

        public async Task<List<Object>> GetQuotationMasterList(string userID)
        {
            var query = from t in _oBMSDbContext.Quotations
                        join client in _oBMSDbContext.ClientMasters on t.Client equals client.Code
                        join branch in _oBMSDbContext.BranchMasters on t.Branch equals branch.Code
                        select new
                        {
                            ID = t.ID,
                            BranchName = branch.Name,
                            ClientName = client.Name,
                            QuotationDate = t.QuotationDate,
                            QuotationEndDate = t.QuotationEndDate,
                            Note = t.Note,
                            AddedDate = t.AddedDate,
                            IsValid = t.IsValid,
                            Status = t.Status
                        } into result
                        orderby result.AddedDate descending, result.ID descending
                        select result;
            return new List<Object>(query.ToList());
        }

        public async Task SaveAndUpdateQuotation(Quotation quotation)
        {
            // Ensure WorkPlace is not NULL
            if (quotation.WorkPlace == null)
            {
                quotation.WorkPlace = "";
            }

            if (quotation.ID == 0)
            {
                _oBMSDbContext.Quotations.Add(quotation);
            }
            else
            {
                _oBMSDbContext.Quotations.Update(quotation);
            }
            await _oBMSDbContext.SaveChangesAsync();
        }

        public async Task SaveAndUpdateQuotationDetails(QuotationDetails quotationDetails)
        {
            if (quotationDetails.ID == 0)
            {
                _oBMSDbContext.QuotationDetails.Add(quotationDetails);
            }
            else
            {
                _oBMSDbContext.QuotationDetails.Update(quotationDetails);
            }
            await _oBMSDbContext.SaveChangesAsync();
        }

        public async Task<Object> GetQuotationByID(int quotationID)
        {
            var quotation = await _oBMSDbContext.Quotations.FirstOrDefaultAsync(x => x.ID == quotationID);
            if (quotation != null)
            {
                var quotationDetails = await _oBMSDbContext.QuotationDetails.Where(x => x.QuotationID == quotationID).ToListAsync();
                return new
                {
                    quotation = quotation,
                    details = quotationDetails
                };
            }
            return null;
        }

        public async Task<Object> CheckClientStatus(string branchId, string clientId, string status)
        {
            var client = await _oBMSDbContext.ClientMasters.FirstOrDefaultAsync(x => x.Code == clientId && x.Branch == branchId && x.Status == status);
            return client != null ? true : false;
        }

        public async Task<List<Object>> GetQuotationsDiscountReport(string branchId, string clientId, DateTime startdate, DateTime endDate)
        {
            var query = from t in _oBMSDbContext.QuotationDetails
                        join q in _oBMSDbContext.Quotations on t.QuotationID equals q.ID
                        where q.Branch == branchId && q.Client == clientId && q.QuotationDate >= startdate && q.QuotationDate <= endDate && t.HasDiscount == true
                        select new
                        {
                            QuotationID = q.ID,
                            Description = t.Description,
                            DiscountAmount = t.DiscountAmount,
                            DiscountHour = t.DiscountHour,
                            QuotationDate = q.QuotationDate
                        };
            return new List<Object>(query.ToList());
        }
    }
}
