using BoldReports.Processing.ObjectModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;
using OBMS.WebAPI.Repositories.Implementation;
using OBMS.WebAPI.Repositories.Interface;

namespace OBMS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgreementController : Controller
    {
        private readonly OBMSDbContext _oBMSDbContext;
        private readonly IAgreementRepository _agreementRepository;


        public AgreementController(IAgreementRepository agreementRepository, OBMSDbContext oBMSDbContext)
        {
            _agreementRepository = agreementRepository;
            _oBMSDbContext = oBMSDbContext;
        }

        [HttpGet]
        [Route("GetAgreementMaster")]
        public async Task<ActionResult<Object>> GetAgreementMasterList(string userID)
        {
            try
            {
                var objList = await _agreementRepository.GetAgreementMasterList(userID);
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        [Route("GetClientsAgreementByBranchID")]
        public async Task<ActionResult<Object>> GetClientsByBranchID(string branchID)
        {

            try
            {



                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                var objList = await _agreementRepository.GetAgreements(branchID);

                dictResult.Add("agreements", objList);

                var objList1 = _oBMSDbContext.ClientMasters.Where(x => x.Branch == branchID).Where(x => x.Status == "Active").OrderBy(x => x.Name).ToList();

                dictResult.Add("clients", objList1);

                return Ok(dictResult);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetClientsOnlyByBranchID")]
        public async Task<ActionResult<Object>> GetClientsOnlyByBranchID(string branchID)
        {



            try
            {
                var objList = _oBMSDbContext.ClientMasters.Where(x => x.Branch == branchID && (x.Status == "Active" || x.Status == "A"))
                    .OrderBy(x => x.Name)
                    .ToList();
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        [Route("GetClientsAllStatusByBranchID")]
        public async Task<ActionResult<Object>> GetClientsAllStatusByBranchID(string branchID)
        {
            try
            {
                var objList = _oBMSDbContext.ClientMasters.Where(x => x.Branch == branchID).OrderBy(x => x.Name).ToList();
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [HttpPost]
        [Route("SaveAndUpdateAgreement")]
        public async Task<ActionResult> SaveAndUpdateAgreement(AgreementRequestDto agreementRequestDto)
        {
            try
            {
                var agreement = new Agreement();
                if (agreementRequestDto.ID != 0)
                {
                    agreement = _oBMSDbContext.Agreements.Where(x => x.ID == agreementRequestDto.ID).FirstOrDefault();
                }



                agreement.AgreementDate = agreementRequestDto.AgreementDate;
                agreement.Branch = agreementRequestDto.Branch;
                agreement.Client = agreementRequestDto.Client;
                agreement.WorkPlace = agreementRequestDto.WorkPlace;
                agreement.IsValid = agreementRequestDto.IsValid;
                agreement.Note = agreementRequestDto.Note;
                agreement.LASTUPDATE = DateTime.Now;
                agreement.AgreementEndDate = agreementRequestDto.AgreementEndDate;
                agreement.QuotationID = agreementRequestDto.QuotationID;
                
                if (agreementRequestDto.ID == 0)
                {
                    agreement.AddedDate = DateTime.Now;
                }
                else if (agreementRequestDto.AddedDate.HasValue)
                {
                    agreement.AddedDate = agreementRequestDto.AddedDate.Value;
                }


                await _agreementRepository.saveAndUpdateAgreement(agreement);

                if (agreementRequestDto.agreementDetails != null)
                {
                    foreach (AgreementDetailsRequestDto detail in agreementRequestDto.agreementDetails)
                    {
                        var agreementDetail = new AgreementDetails();
                        if (detail.ID != 0)
                        {
                            agreementDetail = _oBMSDbContext.AgreementDetails.Where(x => x.ID == detail.ID).FirstOrDefault();
                        }

                        agreementDetail.ID = detail.ID;
                        agreementDetail.AgreementID = agreement.ID;
                        agreementDetail.AgreementDate = agreement.AgreementDate;
                        agreementDetail.Client = agreement.Client;
                        agreementDetail.Branch = agreement.Branch;
                        agreementDetail.Description = detail.Description;
                        agreementDetail.NoOfGuards = detail.NoOfGuards;
                        agreementDetail.PerDay = detail.PerDay;
                        agreementDetail.PerMonth = detail.PerMonth;
                        agreementDetail.Rate = detail.Rate;
                        agreementDetail.NoOfHours = detail.NoOfHours;
                        agreementDetail.NoOfDays = detail.NoOfDays;
                        agreementDetail.FollowCalender = detail.FollowCalender;
                        agreementDetail.MonthTotal = Math.Round(detail.MonthTotal, 2);
                        agreementDetail.HasDiscount = detail.HasDiscount;
                        agreementDetail.DiscountAmount = detail.DiscountAmount;
                        agreementDetail.DiscountHour = detail.DiscountHour;
                        agreementDetail.IsTaxable = detail.IsTaxable;
                        agreementDetail.TaxAmount = detail.TaxAmount;
                        agreementDetail.Category = detail.Category;
                        agreementDetail.Reason = detail.Reason;
                        agreementDetail.LASTUPDATE = DateTime.Now;

                        agreementDetail.Basic = detail.Basic;
                        agreementDetail.DA = detail.DA;
                        agreementDetail.Leaves = (int)detail.Leaves;
                        agreementDetail.LeavesPercentage = detail.LeavesPercentage;
                        agreementDetail.Allowance = detail.Allowance;
                        agreementDetail.Bonus = detail.Bonus;
                        agreementDetail.BonusPercentage = detail.BonusPercentage;
                        agreementDetail.NFH = detail.NFH;
                        agreementDetail.PF = detail.PF;
                        agreementDetail.PFPercentage = detail.PFPercentage;
                        agreementDetail.ESI = detail.ESI;
                        agreementDetail.ESIPercentage = detail.ESIPercentage;
                        agreementDetail.Uniform = detail.Uniform;
                        agreementDetail.ServiceFee = detail.ServiceFee;
                        agreementDetail.HRA = detail.HRA;
                        agreementDetail.HRAPercentage = detail.HRAPercentage;
                        agreementDetail.ProfessionalTax = detail.ProfessionalTax;
                        agreementDetail.RelieverCharges = detail.RelieverCharges;
                        agreementDetail.RelieverChargesPercentage = detail.RelieverChargesPercentage;
                        agreementDetail.Others = detail.Others;
                        agreementDetail.OthersPercentage = detail.OthersPercentage;
                        agreementDetail.AdministrationCharges = detail.AdministrationCharges;
                        agreementDetail.AdministrationChargesPercentage = detail.AdministrationChargesPercentage;
                        agreementDetail.ManagementFee = detail.ManagementFee;
                        agreementDetail.ManagementFeePercentage = detail.ManagementFeePercentage;
                        agreementDetail.SubTotal = detail.SubTotal;
                        agreementDetail.TotalPlusStatutory = detail.TotalPlusStatutory;
                        agreementDetail.TotalDirectCost = detail.TotalDirectCost;
                        agreementDetail.MonthlyChargedCost = detail.MonthlyChargedCost;

                        await _agreementRepository.saveAndUpdateAgreementDetails(agreementDetail);
                    }
                }

                return Ok(new { Success = "Success", AgreementID = agreement.ID });

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpDelete("DeleteAgreementDetailById")]
        public async Task<ActionResult<HttpResponseMessage>> DeleteAgreementDetailById(int Id)
        {
            try
            {
                var data = _oBMSDbContext.AgreementDetails.Where(x => x.ID == Id).FirstOrDefault();


                if (data != null)
                {
                    _oBMSDbContext.AgreementDetails.Remove(data);
                    await _oBMSDbContext.SaveChangesAsync();
                }

                Dictionary<string, object> dictResult = new Dictionary<string, object>();
                dictResult.Add("Success", "Success");


                return Ok(dictResult);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("GetAgreementByID")]
        public async Task<ActionResult<Object>> GetAgreementByID(int agreementID)
        {
            try
            {
                var objList = await _agreementRepository.GetAgreementByID(agreementID);
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [HttpGet]
        [Route("GetAgreements")]
        public async Task<ActionResult<Object>> GetAgreements(string userID, string branchId, bool load)
        {
            try
            {



                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                var objList = await _agreementRepository.GetAgreements(branchId);

                dictResult.Add("agreements", objList);

                if (load)
                {
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

                    dictResult.Add("branches", branchs);
                }


                return Ok(dictResult);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        [Route("CheckClientStatus")]
        public async Task<ActionResult<Object>> CheckClientStatus(string branchId, string clientId, string status)
        {
            try
            {

                var dictResult = await _agreementRepository.CheckClientStatus(branchId, clientId, status);


                return Ok(dictResult);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [HttpGet]
        [Route("GetFinalInvoiceDate")]
        public async Task<ActionResult<Object>> GetFinalInvoiceDate(int agreementId)
        {
            try
            {

                var dictResult = await _agreementRepository.GetFinalInvoiceDate(agreementId);


                return Ok(dictResult);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [HttpGet]
        [Route("GetAgreementsDiscountReportMaster")]
        public async Task<ActionResult<Object>> GetAgreementsDiscountReportMaster(string userID)
        {
            try
            {
                var branches = await _oBMSDbContext.BranchMasters
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

                return Ok(branches);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        [Route("GetAgreementsDiscountReport")]
        public async Task<ActionResult<Object>> GetAgreementsDiscountReport(string branchId, string clientId, DateTime startDate, DateTime endDate)
        {
            try
            {

                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                var objList = await _agreementRepository.GetAgreementsDiscountReport(branchId, clientId, startDate, endDate);

                dictResult.Add("agreements", objList);



                dictResult.Add("branches", _oBMSDbContext.BranchMasters.ToList());



                return Ok(dictResult);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        [Route("GetAgreementTerminationList")]
        public async Task<ActionResult<Object>> GeAgreementTerminationList(string branchId,string userID)
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



            results.Add("branches", branchs);



            if (branchId == "0")
            {


                var query = from terminatedAgreement in _oBMSDbContext.TerminatedAgreements
                            join clientMaster in _oBMSDbContext.ClientMasters
                            on new { terminatedAgreement.Client, terminatedAgreement.Branch } equals new { Client = clientMaster.Code, clientMaster.Branch }
                            select new
                            {
                                terminatedAgreement.ID,
                                terminatedAgreement.Branch,
                                terminatedAgreement.Client,
                                ClientName = clientMaster.Name,
                                terminatedAgreement.Reason,
                                terminatedAgreement.TerminationDate,
                                terminatedAgreement.Note,
                                terminatedAgreement.LASTUPDATE
                            };

                results.Add("agreementTermination", query);
            }
            else
            {
                var query = from terminatedAgreement in _oBMSDbContext.TerminatedAgreements
                            join clientMaster in _oBMSDbContext.ClientMasters
                            on new { terminatedAgreement.Client, terminatedAgreement.Branch } equals new { Client = clientMaster.Code, clientMaster.Branch }
                            where terminatedAgreement.Branch == branchId
                            select new
                            {
                                terminatedAgreement.ID,
                                terminatedAgreement.Branch,
                                terminatedAgreement.Client,
                                ClientName = clientMaster.Name,
                                terminatedAgreement.Reason,
                                terminatedAgreement.TerminationDate,
                                terminatedAgreement.Note,
                                terminatedAgreement.LASTUPDATE
                            };

                results.Add("agreementTermination", query);
            }

            //return new List<object>(query);
            //throw new NotImplementedException();

            return Ok(results); ;
        }

        [HttpGet]
        [Route("GetAgreementListByBranchId")]
        public async Task<ActionResult<Object>> GetAgreementListByBranchId(string branchId, string clientId, DateTime terminationDate)
        {
            try
            {

                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                var objList = await _agreementRepository.GetAgreementListByBranchId(branchId, clientId, terminationDate);

                //dictResult.Add("agreements", objList);

                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost]
        [Route("SaveAndUpdateAgreementTermination")]
        public async Task<ActionResult> SaveAndUpdateAgreementTermination(TerminatedAgreementRequestDto terminatedAgreementRequestDto)
        {
            try
            {
                var terminatedAgreement = new TerminatedAgreement();
                if (terminatedAgreementRequestDto.ID != 0)
                {
                    terminatedAgreement = _oBMSDbContext.TerminatedAgreements.Where(x => x.ID == terminatedAgreementRequestDto.ID).FirstOrDefault();
                }
                terminatedAgreement.ID = terminatedAgreementRequestDto.ID;
                terminatedAgreement.Branch = terminatedAgreementRequestDto.Branch;
                terminatedAgreement.Client = terminatedAgreementRequestDto.Client;
                terminatedAgreement.Reason = terminatedAgreementRequestDto.Reason;
                terminatedAgreement.TerminationDate = terminatedAgreementRequestDto.TerminationDate;
                terminatedAgreement.Note = terminatedAgreementRequestDto.Note;
                terminatedAgreement.LASTUPDATE = DateTime.Now;
                terminatedAgreement.LastUpdatedBy = terminatedAgreementRequestDto.LastUpdatedBy;



                await _agreementRepository.SaveAndUpdateAgreementTermination(terminatedAgreement);

                return Ok(new { Success = "Success", TerminationID = terminatedAgreement.ID });

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("GetAgreementTerminationByID")]
        public async Task<ActionResult<Object>> GetAgreementTerminationByID(int id)
        {
            try
            {
                var objList = await _agreementRepository.GetAgreementTerminationByID(id);
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpDelete("DeleteAgreementById")]
        public async Task<IActionResult> DeleteAgreementById(int Id)
        {
            try
            {
                var agreement = await _oBMSDbContext.Agreements.FirstOrDefaultAsync(x => x.ID == Id);
                
                if (agreement == null)
                {
                    return NotFound(new { Message = "Agreement not found" });
                }

                var details = await _oBMSDbContext.AgreementDetails.Where(x => x.AgreementID == Id).ToListAsync();
                
                if (details.Any())
                {
                    _oBMSDbContext.AgreementDetails.RemoveRange(details);
                }

                _oBMSDbContext.Agreements.Remove(agreement);
                await _oBMSDbContext.SaveChangesAsync();

                return Ok(new { Message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error occurred", Error = ex.Message });
            }
        }

        [HttpPost("CancelAgreement")]
        public async Task<ActionResult> CancelAgreement([FromQuery] int agreementId)
        {
            try
            {
                var result = await _agreementRepository.CancelAgreement(agreementId);
                
                if (!result)
                {
                    return NotFound(new { Message = "Agreement not found" });
                }

                return Ok(new { Message = "Agreement cancelled successfully and quotation reopened" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error occurred", Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("SaveAndUpdateAgreementDetails")]
        public async Task<ActionResult> SaveAndUpdateAgreementDetails(AgreementDetails agreementDetails)
        {
            try
            {
                // Add logging for debugging
                Console.WriteLine($"Saving AgreementDetails: ID={agreementDetails.ID}, AgreementID={agreementDetails.AgreementID}");
                Console.WriteLine($"Description: {agreementDetails.Description}, Category: {agreementDetails.Category}");
                Console.WriteLine($"NoOfGuards: {agreementDetails.NoOfGuards}, Rate: {agreementDetails.Rate}");
                Console.WriteLine($"Client: {agreementDetails.Client}, Branch: {agreementDetails.Branch}");
                Console.WriteLine($"AgreementDate: {agreementDetails.AgreementDate}");

                // Validate required fields
                if (agreementDetails.AgreementID == 0)
                {
                    Console.WriteLine("ERROR: AgreementID is 0");
                    return BadRequest("AgreementID is required");
                }

                if (string.IsNullOrEmpty(agreementDetails.Client))
                {
                    Console.WriteLine("ERROR: Client is null or empty");
                    return BadRequest("Client is required");
                }

                if (string.IsNullOrEmpty(agreementDetails.Branch))
                {
                    Console.WriteLine("ERROR: Branch is null or empty");
                    return BadRequest("Branch is required");
                }

                if (string.IsNullOrEmpty(agreementDetails.Description))
                {
                    Console.WriteLine("ERROR: Description is null or empty");
                    return BadRequest("Description is required");
                }

                agreementDetails.LASTUPDATE = DateTime.Now;

                await _agreementRepository.saveAndUpdateAgreementDetails(agreementDetails);

                Console.WriteLine($"Successfully saved AgreementDetails with ID: {agreementDetails.ID}");
                return Ok(new { Success = "Success", AgreementDetailID = agreementDetails.ID });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving AgreementDetails: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetAgreementsPostedThisMonth")]
        public async Task<ActionResult<int>> GetAgreementsPostedThisMonth()
        {
            try
            {
                var count = await _agreementRepository.GetAgreementsPostedThisMonth();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error occurred", Error = ex.Message });
            }
        }
    }
}
