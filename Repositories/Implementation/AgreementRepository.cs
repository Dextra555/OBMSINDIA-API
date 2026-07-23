using BoldReports.Processing.ObjectModels;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Repositories.Interface;

namespace OBMS.WebAPI.Repositories.Implementation
{
    public class AgreementRepository : IAgreementRepository
    {
        private readonly OBMSDbContext _oBMSDbContext;

        public AgreementRepository(OBMSDbContext oBMSDbContext)
        {
            _oBMSDbContext = oBMSDbContext;

        }

        public async Task<Dictionary<string, Object>> GetAgreementMasterList(string userID)
        {

            var results = new Dictionary<string, Object>();


            var branchs = await _oBMSDbContext.BranchMasters
                .Join(_oBMSDbContext.OBMSBranches,
                    branchmaster => branchmaster.Code,
                    branches => branches.BranchCode,
                    (branchmaster, branches) => new { branchmaster, branches })
                .Where(joinResult => joinResult.branches.Name == userID && joinResult.branches.IsAllowed == true)
                .Select(joinResult => new BranchMaster
                {
                    ID = joinResult.branchmaster.ID,
                    Code = joinResult.branchmaster.Code,
                    Name = joinResult.branchmaster.Name,
                    Address1 = joinResult.branchmaster.Address1,
                    Address2 = joinResult.branchmaster.Address2,
                    PostCode = joinResult.branchmaster.PostCode,
                    City = joinResult.branchmaster.City,
                    State = joinResult.branchmaster.State,
                    Phone = joinResult.branchmaster.Phone,
                    Fax = joinResult.branchmaster.Fax,
                    BankName = joinResult.branchmaster.BankName,
                    BankBranch = joinResult.branchmaster.BankBranch,
                    BankAccount = joinResult.branchmaster.BankAccount,
                    PersonIncharge = joinResult.branchmaster.PersonIncharge,
                    Email = joinResult.branchmaster.Email,
                    Description = joinResult.branchmaster.Description,
                    ShortName = joinResult.branchmaster.ShortName,
                    IsHeadQuarters = joinResult.branchmaster.IsHeadQuarters,
                    UbsCode = joinResult.branchmaster.UbsCode,
                    LastUpdate = joinResult.branchmaster.LastUpdate,
                    LastUpdatedBy = joinResult.branchmaster.LastUpdatedBy,
                    ParentBranch = joinResult.branchmaster.ParentBranch
                })
                .ToListAsync();

            var clientList = _oBMSDbContext.ClientMasters?.ToList();

            results.Add("branchList", branchs);
            results.Add("clientList", clientList);

            return results;
        }

        public async Task<Agreement> saveAndUpdateAgreement(Agreement agreement)
        {
            // Ensure WorkPlace is not NULL
            if (agreement.WorkPlace == null)
            {
                agreement.WorkPlace = "";
            }

            if (agreement.ID == 0)
            {
                _oBMSDbContext.Agreements.Add(agreement);
            }
            else
            {
                _oBMSDbContext.Agreements.Update(agreement);
            }

            await _oBMSDbContext.SaveChangesAsync();


            //throw new NotImplementedException();
            return agreement;
        }

        public async Task<AgreementDetails> saveAndUpdateAgreementDetails(AgreementDetails agreementDetails)
        {
            if (agreementDetails.ID == 0)
            {
                _oBMSDbContext.AgreementDetails.Add(agreementDetails);
            }
            else
            {
                _oBMSDbContext.AgreementDetails.Update(agreementDetails);
            }

            await _oBMSDbContext.SaveChangesAsync();


            //throw new NotImplementedException();
            return agreementDetails;
        }

        public async Task<Dictionary<string, Object>> GetAgreementByID(int id)
        {
            var results = new Dictionary<string, Object>();

            var agreement = await _oBMSDbContext.Agreements.Where(x => x.ID == id).FirstOrDefaultAsync();
            var agreementDetails = await _oBMSDbContext.AgreementDetails.Where(x => x.AgreementID == id).ToListAsync();

            results.Add("agreement", agreement);
            results.Add("agreementDetails", agreementDetails);

            var query = await (from invoice in _oBMSDbContext.ClientInvoices
                              where invoice.IsDeleted == "N" && invoice.AgreementID == id
                              select new
                              {
                                  InvoiceDate = invoice.InvoiceDate != null ? invoice.InvoiceDate : DateTime.Parse("1753-01-01")
                              }).ToListAsync();

            results.Add("finalInvoiceDate", query);

            return results;
        }

        public async Task<List<Object>> GetAgreements(string branchId)
        {

            if (branchId == "0")
            {

                var query = from t in _oBMSDbContext.Agreements
                            join client in _oBMSDbContext.ClientMasters on t.Client equals client.Code
                            join branch in _oBMSDbContext.BranchMasters on
                            t.Branch equals branch.Code
                            where t.AgreementDate == _oBMSDbContext.Agreements
                                                         .Where(a => a.Client == t.Client && a.Branch == t.Branch)
                                                         .Max(a => a.AgreementDate)
                            select new
                            {
                                ID = t.ID,
                                BranchName = branch.Name,
                                ClientName = client.Name,
                                WorkPlace = t.WorkPlace,
                                AgreementDate = t.AgreementDate,
                                AddedDate = t.AddedDate,
                                QuotationID = t.QuotationID,
                                HasInvoices = _oBMSDbContext.ClientInvoices.Any(i => i.AgreementID == t.ID && i.IsDeleted == "N"),
                                Status = t.IsValid == false ? "Cancelled" : 
                                         _oBMSDbContext.ClientInvoices.Any(i => i.AgreementID == t.ID && i.IsDeleted == "N") ? "Closed" : "Open"
                            } into result
                            orderby result.AddedDate descending, result.ID descending
                            select result;
                return new List<Object>(query);
            }
            else
            {

                var query2 = from t in _oBMSDbContext.Agreements
                             join clientMaster in _oBMSDbContext.ClientMasters
                             on new { Client = t.Client, Branch = t.Branch } equals new { Client = clientMaster.Code, Branch = clientMaster.Branch }
                             join client in _oBMSDbContext.ClientMasters on t.Client equals client.Code
                             join branch in _oBMSDbContext.BranchMasters on
                             t.Branch equals branch.Code
                             where t.AgreementDate == _oBMSDbContext.Agreements
                                                                  .Where(a => a.Client == t.Client && a.Branch == t.Branch)
                                                                  .Max(a => a.AgreementDate)
                                && t.Branch == branchId
                                && !_oBMSDbContext.TerminatedAgreements
                                                  .Where(terminated => terminated.Branch == branchId)
                                                  .Select(terminated => terminated.Client)
                                                  .Contains(t.Client)
                             select new
                             {
                                 ID = t.ID,
                                 BranchName = branch.Name,
                                 ClientName = client.Name,
                                 WorkPlace = t.WorkPlace,
                                 AgreementDate = t.AgreementDate,
                                 AddedDate = t.AddedDate,
                                 QuotationID = t.QuotationID,
                                 HasInvoices = _oBMSDbContext.ClientInvoices.Any(i => i.AgreementID == t.ID && i.IsDeleted == "N"),
                                 Status = t.IsValid == false ? "Cancelled" : 
                                          _oBMSDbContext.ClientInvoices.Any(i => i.AgreementID == t.ID && i.IsDeleted == "N") ? "Closed" : "Open"
                             } into result
                             orderby result.AddedDate descending, result.ID descending
                             select result;
                return new List<Object>(query2);

            }

            //var query1 = from agreement in _oBMSDbContext.Agreements
            //             join client in _oBMSDbContext.ClientMasters on agreement.Client equals client.Code
            //             join branch in _oBMSDbContext.BranchMasters on
            //             agreement.Branch equals branch.Code
            //             select new
            //             {
            //                 ID = agreement.ID,
            //                 WorkPlace = agreement.WorkPlace,
            //                 BranchName = branch.Name,
            //                 ClientName = client.Name,
            //                 AgreementDate = agreement.AgreementDate
            //             };
            //return new List<Object>(query);
        }

        public async Task<Object> CheckClientStatus(string branchId, string clientId, string status)
        {


            var data = _oBMSDbContext.ClientMasters.Where(x => x.Branch == branchId).Where(x => x.Code == clientId).Where(x => x.Status == status).ToList();


            return data.Count();
        }

        public async Task<Object> GetFinalInvoiceDate(int agreementId)
        {

            var query = from invoice in _oBMSDbContext.ClientInvoices
                        where invoice.IsDeleted == "N" && invoice.AgreementID == agreementId
                        select new
                        {
                            InvoiceDate = invoice.InvoiceDate != null ? invoice.InvoiceDate : DateTime.Parse("1753-01-01")
                        };

            return query;

        }

        public async Task<List<Object>> GetAgreementsDiscountReport(string branchId, string clientId, DateTime startdate, DateTime endDate)
        {
            var query = from A in _oBMSDbContext.Agreements
                        join B in _oBMSDbContext.AgreementDetails on A.ID equals B.AgreementID
                        join D in _oBMSDbContext.ClientMasters on new { A.Client, A.Branch } equals new { Client = D.Code, Branch = D.Branch }
                        where A.Branch == branchId
                              && A.Client == clientId
                              && (A.IsValid == true || A.IsValid == null)
                              && B.AgreementDate >= startdate
                              && B.AgreementDate <= endDate
                              && (!string.IsNullOrEmpty(B.Category) && !string.IsNullOrEmpty(B.Reason))
                        group new { D.Name, B.AgreementDate, B.Description, B.Category, B.Reason } by new { D.Name, B.AgreementDate, B.Description, B.Category, B.Reason } into grouped
                        orderby grouped.Key.Name, grouped.Key.AgreementDate
                        select new
                        {
                            grouped.Key.Name,
                            grouped.Key.AgreementDate,
                            grouped.Key.Description,
                            grouped.Key.Category,
                            grouped.Key.Reason
                        };


            return new List<object>(query);
            //throw new NotImplementedException();
        }


        public async Task<Dictionary<string, Object>> GetAgreementListByBranchId(string branchId, string clientId, DateTime terminationDate)
        {
            var results = new Dictionary<string, Object>();

            var latestAgreements = _oBMSDbContext.Agreements
          .Join(
              _oBMSDbContext.ClientMasters,
              agreement => new { agreement.Client, agreement.Branch },
              clientMaster => new { Client = clientMaster.Code, Branch = clientMaster.Branch },
              (agreement, clientMaster) => new { Agreement = agreement, ClientMaster = clientMaster }
          )
          .Where(result =>
              result.Agreement.AgreementDate == _oBMSDbContext.Agreements
                  .Where(inner => inner.Client == result.Agreement.Client && inner.Branch == result.Agreement.Branch)
                  .Max(inner => inner.AgreementDate)
              && result.Agreement.Branch == branchId
              && !_oBMSDbContext.TerminatedAgreements
                  .Any(terminated => terminated.Client == result.Agreement.Client && terminated.Branch == branchId)
          )
          .OrderBy(result => result.ClientMaster.Name)
          .Select(result => new
          {
              result.Agreement.ID,
              result.Agreement.Branch,
              ClientName = result.ClientMaster.Name,
              result.Agreement.AgreementDate,
              result.Agreement.AddedDate,
              result.Agreement.Client
          })
          .ToList();

            var errorTxt = "";
            int numberOfInvoices = 0;
            foreach (var agreement in latestAgreements)
            {
                if (clientId == agreement.Client)
                {


                    numberOfInvoices = _oBMSDbContext.ClientInvoices
     .Where(ci => ci.AgreementID == agreement.ID && ci.IsDeleted == "N")
     .Count();
                    if (numberOfInvoices > 0)
                    {
                        var maxInvoiceDate = _oBMSDbContext.ClientInvoices
         .Where(ci => ci.AgreementID == agreement.ID && ci.IsDeleted == "N")
         .Select(ci => (DateTime?)ci.InvoiceDate)
         .Max();
                        DateTime invoiceDate = maxInvoiceDate ?? DateTime.MinValue;

                        if(invoiceDate > terminationDate)
                        {
                            errorTxt = "Invoice has been created for this client until " + invoiceDate.ToString("dd-MMM-yyyy") + ". Termination date cannot be less than or equal to the last invoice date.";
                        }
                    }
                }
            }



            


            results.Add("agreements", new List<object>(latestAgreements));

            results.Add("message", errorTxt);

            return results;

        }

        public async Task<TerminatedAgreement> SaveAndUpdateAgreementTermination(TerminatedAgreement terminatedAgreement)
        {
            var clientMaster = _oBMSDbContext.ClientMasters.Where(x => x.Code == terminatedAgreement.Client).FirstOrDefault();
            clientMaster.Status = "Inactive";
            _oBMSDbContext.ClientMasters.Update(clientMaster);

            if (terminatedAgreement.ID == 0)
            {
                _oBMSDbContext.TerminatedAgreements.Add(terminatedAgreement);
            }
            else
            {
                _oBMSDbContext.TerminatedAgreements.Update(terminatedAgreement);
            }

            await _oBMSDbContext.SaveChangesAsync();


            //throw new NotImplementedException();
            return terminatedAgreement;
        }


        public async Task<Dictionary<string, Object>> GetAgreementTerminationByID(int id)
        {

            var results = new Dictionary<string, Object>();


            var agreement = _oBMSDbContext.TerminatedAgreements.Where(x => x.ID == id).FirstOrDefault();

            
            results.Add("terminatedAgreement", agreement);
           

            return results;
        }

        public async Task<bool> CancelAgreement(int agreementId)
        {
            var agreement = await _oBMSDbContext.Agreements.FirstOrDefaultAsync(x => x.ID == agreementId);
            if (agreement == null) return false;

            // 1. If linked to a quotation, re-open it
            if (agreement.QuotationID.HasValue && agreement.QuotationID.Value > 0)
            {
                var quotation = await _oBMSDbContext.Quotations.FirstOrDefaultAsync(x => x.ID == agreement.QuotationID.Value);
                if (quotation != null)
                {
                    quotation.Status = "Open";
                    _oBMSDbContext.Quotations.Update(quotation);
                }
            }

            // 2. Update agreement status to cancelled (not delete)
            agreement.IsValid = false;
            agreement.LASTUPDATE = DateTime.Now;
            _oBMSDbContext.Agreements.Update(agreement);

            await _oBMSDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetAgreementsPostedThisMonth()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var count = await _oBMSDbContext.Agreements
                .Where(a => a.AgreementDate >= startOfMonth && a.AgreementDate <= endOfMonth)
                .CountAsync();

            return count;
        }
    }
}
