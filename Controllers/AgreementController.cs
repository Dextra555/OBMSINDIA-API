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
                bool createNewRow = false;  // true = insert new row, false = update existing

                if (agreementRequestDto.ID != 0)
                {
                    var existingAgreement = _oBMSDbContext.Agreements
                        .Where(x => x.ID == agreementRequestDto.ID)
                        .FirstOrDefault();

                    if (existingAgreement != null)
                    {
                        var existingDetails = _oBMSDbContext.AgreementDetails
                            .Where(x => x.AgreementID == agreementRequestDto.ID)
                            .ToList();

                        // Only a unique-key change (Branch/Client/WorkPlace/Date month)
                        // → keep the old record untouched and insert a brand-new row.
                        // WorkPlace change → new row for new site, old row kept visible.
                        // Date (month) change → new version row, old row kept visible.
                        // Same key → edit the existing row in place.
                        if (AgreementChanged(existingAgreement, agreementRequestDto, existingDetails))
                        {
                            createNewRow = true;
                            agreement = new Agreement();
                        }
                        else
                        {
                            // Same unique key → update the existing row in place.
                            // Remove any detail rows that were deleted in the UI so stale
                            // rows do not remain attached to the agreement.
                            agreement = existingAgreement;
                            var incomingDetailIds = agreementRequestDto.agreementDetails?
                                .Select(d => d.ID)
                                .ToList() ?? new List<int>();
                            var staleDetails = existingDetails
                                .Where(d => !incomingDetailIds.Contains(d.ID))
                                .ToList();
                            if (staleDetails.Count > 0)
                            {
                                _oBMSDbContext.AgreementDetails.RemoveRange(staleDetails);
                            }
                        }
                    }
                }

                // Duplicate check: same Branch + Client + WorkPlace + Month = duplicate.
                // Different WorkPlace = allowed (different site).
                // Different month same WorkPlace = allowed (new version, handled by createNewRow).
                // Exclude the original agreement ID when editing so a new version is never
                // blocked by its own old row.
                int duplicateExcludeId = agreementRequestDto.ID;

                bool isDuplicate = await _agreementRepository.CheckDuplicateAgreement(
                    agreementRequestDto.Branch,
                    agreementRequestDto.Client,
                    agreementRequestDto.AgreementDate,
                    duplicateExcludeId,
                    agreementRequestDto.WorkPlace
                );

                if (isDuplicate)
                {
                    return BadRequest(new
                    {
                        Success = "Duplicate",
                        Message = $"An agreement already exists for this Branch, Client and Work Place '{agreementRequestDto.WorkPlace}' in {agreementRequestDto.AgreementDate:MMMM yyyy}. Please edit that existing agreement instead of creating a duplicate."
                    });
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

                if (agreementRequestDto.ID == 0 || createNewRow)
                {
                    agreement.AddedDate = DateTime.Now;
                }
                else if (agreementRequestDto.AddedDate.HasValue)
                {
                    agreement.AddedDate = agreementRequestDto.AddedDate.Value;
                }

                // When creating a new row (WorkPlace/Date change), the old record is KEPT
                // valid and visible in the list. New versions are added as extra rows,
                // so the list shows every version for a Workplace in the order created.

                await _agreementRepository.saveAndUpdateAgreement(agreement);

                if (agreementRequestDto.agreementDetails != null)
                {
                    foreach (AgreementDetailsRequestDto detail in agreementRequestDto.agreementDetails)
                    {
                        var agreementDetail = new AgreementDetails();

                        // If creating a new row (WorkPlace/Date changed), all details go as new inserts
                        if (!createNewRow && detail.ID != 0)
                        {
                            agreementDetail = _oBMSDbContext.AgreementDetails.Where(x => x.ID == detail.ID).FirstOrDefault();
                        }

                        // When creating new row, reset ID so EF Core inserts a new detail row
                        agreementDetail.ID = createNewRow ? 0 : detail.ID;
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

                // After deleting the latest row, reactivate the previous history row
                // for the same Branch+Client+WorkPlace (the one just before this one).
                // This brings the previous version back to the list.
                var prevAgreement = await _oBMSDbContext.Agreements
                    .Where(a =>
                        a.Branch == agreement.Branch &&
                        a.Client == agreement.Client &&
                        a.WorkPlace == agreement.WorkPlace &&
                        a.IsValid == false)
                    .OrderByDescending(a => a.ID)
                    .FirstOrDefaultAsync();

                if (prevAgreement != null)
                {
                    prevAgreement.IsValid = true;
                    prevAgreement.LASTUPDATE = DateTime.Now;
                    _oBMSDbContext.Agreements.Update(prevAgreement);
                    await _oBMSDbContext.SaveChangesAsync();
                }

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

        [HttpGet]
        [Route("CheckDuplicateAgreement")]
        public async Task<ActionResult<Object>> CheckDuplicateAgreement(string branch, string client, DateTime agreementDate, int excludeId = 0, string workPlace = null)
        {
            try
            {
                var isDuplicate = await _agreementRepository.CheckDuplicateAgreement(branch, client, agreementDate, excludeId, workPlace);
                return Ok(new { IsDuplicate = isDuplicate });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error occurred", Error = ex.Message });
            }
        }

        // Returns true ONLY when the agreement unique key changes:
        // Branch, Client, Work Place, the AgreementDate month/year, or the AgreementEndDate.
        // Key change → create a brand-new history row; the old row stays as history.
        // Every other edit (rates, note, guards...) keeps the same key, so the existing
        // row is simply updated in place.
        private bool AgreementChanged(Agreement existing, AgreementRequestDto incoming, List<AgreementDetails> existingDetails)
        {
            if (!string.Equals(existing.Branch ?? "", incoming.Branch ?? "", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(existing.Client ?? "", incoming.Client ?? "", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(existing.WorkPlace ?? "", incoming.WorkPlace ?? "", StringComparison.OrdinalIgnoreCase) ||
                !SameMonth(existing.AgreementDate, incoming.AgreementDate) ||
                !SameDay(existing.AgreementEndDate, incoming.AgreementEndDate))
            {
                return true;
            }

            return false;
        }

        private static bool SameMonth(DateTime a, DateTime b)
        {
            return a.Year == b.Year && a.Month == b.Month;
        }

        private static bool SameDay(DateTime a, DateTime b)
        {
            return a.Year == b.Year && a.Month == b.Month && a.Day == b.Day;
        }
    }
}
