using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;
using OBMS.WebAPI.Repositories.Interface;

namespace OBMS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotationController : Controller
    {
        private readonly OBMSDbContext _oBMSDbContext;
        private readonly IQuotationRepository _quotationRepository;

        public QuotationController(IQuotationRepository quotationRepository, OBMSDbContext oBMSDbContext)
        {
            _quotationRepository = quotationRepository;
            _oBMSDbContext = oBMSDbContext;
        }

        [HttpGet]
        [Route("GetQuotationMaster")]
        public async Task<ActionResult<Object>> GetQuotationMasterList(string userID)
        {
            try
            {
                var objList = await _quotationRepository.GetQuotationMasterList(userID);
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetClientsQuotationByBranchID")]
        public async Task<ActionResult<Object>> GetClientsByBranchID(string branchID)
        {
            try
            {
                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                var objList = _quotationRepository.GetQuotations(branchID);

                dictResult.Add("quotations", objList);

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
        [Route("SaveAndUpdateQuotation")]
        public async Task<ActionResult> SaveAndUpdateQuotation(QuotationRequestDto quotationRequestDto)
        {
            try
            {
                var quotation = new Quotation();
                if (quotationRequestDto.ID != 0)
                {
                    quotation = _oBMSDbContext.Quotations.Where(x => x.ID == quotationRequestDto.ID).FirstOrDefault();
                }

                quotation.QuotationDate = quotationRequestDto.QuotationDate;
                quotation.QuotationEndDate = quotationRequestDto.QuotationEndDate;
                quotation.Branch = quotationRequestDto.Branch;
                quotation.Client = quotationRequestDto.Client;
                quotation.IsValid = quotationRequestDto.IsValid;
                quotation.Note = quotationRequestDto.Note;
                quotation.LASTUPDATE = DateTime.Now;

                if (quotationRequestDto.ID == 0)
                {
                    quotation.AddedDate = DateTime.Now;
                }
                else if (quotationRequestDto.AddedDate.HasValue)
                {
                    quotation.AddedDate = quotationRequestDto.AddedDate.Value;
                }

                await _quotationRepository.SaveAndUpdateQuotation(quotation);

                if (quotationRequestDto.quotationDetails != null)
                {
                    foreach (QuotationDetailsRequestDto detail in quotationRequestDto.quotationDetails)
                    {
                        var quotationDetail = new QuotationDetails();
                        if (detail.ID != 0)
                        {
                            quotationDetail = _oBMSDbContext.QuotationDetails.Where(x => x.ID == detail.ID).FirstOrDefault();
                        }

                        quotationDetail.ID = detail.ID;
                        quotationDetail.QuotationID = quotation.ID;
                        quotationDetail.QuotationDate = quotation.QuotationDate;
                        quotationDetail.Client = quotation.Client;
                        quotationDetail.Branch = quotation.Branch;
                        quotationDetail.Description = detail.Description;
                        quotationDetail.NoOfGuards = detail.NoOfGuards;
                        quotationDetail.PerDay = detail.PerDay;
                        quotationDetail.PerMonth = detail.PerMonth;
                        quotationDetail.Rate = detail.Rate;
                        quotationDetail.NoOfHours = detail.NoOfHours;
                        quotationDetail.NoOfDays = detail.NoOfDays;
                        quotationDetail.FollowCalender = detail.FollowCalender;
                        quotationDetail.MonthTotal = Math.Round(detail.MonthTotal, 2);
                        quotationDetail.HasDiscount = detail.HasDiscount;
                        quotationDetail.DiscountAmount = detail.DiscountAmount;
                        quotationDetail.DiscountHour = detail.DiscountHour;
                        quotationDetail.IsTaxable = detail.IsTaxable;
                        quotationDetail.TaxAmount = detail.TaxAmount;
                        quotationDetail.Category = detail.Category;
                        quotationDetail.Reason = detail.Reason;
                        quotationDetail.LASTUPDATE = DateTime.Now;

                        quotationDetail.Basic = detail.Basic;
                        quotationDetail.DA = detail.DA;
                        quotationDetail.Leaves = (int)detail.Leaves;
                        quotationDetail.LeavesPercentage = detail.LeavesPercentage;
                        quotationDetail.Allowance = detail.Allowance;
                        quotationDetail.Bonus = detail.Bonus;
                        quotationDetail.BonusPercentage = detail.BonusPercentage;
                        quotationDetail.NFH = detail.NFH;
                        quotationDetail.PF = detail.PF;
                        quotationDetail.PFPercentage = detail.PFPercentage;
                        quotationDetail.ESI = detail.ESI;
                        quotationDetail.ESIPercentage = detail.ESIPercentage;
                        quotationDetail.Uniform = detail.Uniform;
                        quotationDetail.ServiceFee = detail.ServiceFee;
                        quotationDetail.HRA = detail.HRA;
                        quotationDetail.HRAPercentage = detail.HRAPercentage;
                        quotationDetail.ProfessionalTax = detail.ProfessionalTax;
                        quotationDetail.RelieverCharges = detail.RelieverCharges;
                        quotationDetail.RelieverChargesPercentage = detail.RelieverChargesPercentage;
                        quotationDetail.Others = detail.Others;
                        quotationDetail.OthersPercentage = detail.OthersPercentage;
                        quotationDetail.AdministrationCharges = detail.AdministrationCharges;
                        quotationDetail.AdministrationChargesPercentage = detail.AdministrationChargesPercentage;
                        quotationDetail.ManagementFee = detail.ManagementFee;
                        quotationDetail.ManagementFeePercentage = detail.ManagementFeePercentage;
                        quotationDetail.SubTotal = detail.SubTotal;
                        quotationDetail.TotalPlusStatutory = detail.TotalPlusStatutory;
                        quotationDetail.TotalDirectCost = detail.TotalDirectCost;
                        quotationDetail.MonthlyChargedCost = detail.MonthlyChargedCost;

                        await _quotationRepository.SaveAndUpdateQuotationDetails(quotationDetail);
                    }
                }

                return Ok(new { Success = "Success", QuotationID = quotation.ID });

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        [Route("SaveAndUpdateQuotationDetails")]
        public async Task<ActionResult> SaveAndUpdateQuotationDetails(QuotationDetails quotationDetails)
        {
            try
            {
                // Add logging for debugging
                Console.WriteLine($"Saving QuotationDetails: ID={quotationDetails.ID}, QuotationID={quotationDetails.QuotationID}");
                Console.WriteLine($"Description: {quotationDetails.Description}, Category: {quotationDetails.Category}");
                Console.WriteLine($"NoOfGuards: {quotationDetails.NoOfGuards}, Rate: {quotationDetails.Rate}");
                Console.WriteLine($"Client: {quotationDetails.Client}, Branch: {quotationDetails.Branch}");
                Console.WriteLine($"QuotationDate: {quotationDetails.QuotationDate}");

                // Validate required fields
                if (quotationDetails.QuotationID == 0)
                {
                    Console.WriteLine("ERROR: QuotationID is 0");
                    return BadRequest("QuotationID is required");
                }

                if (string.IsNullOrEmpty(quotationDetails.Client))
                {
                    Console.WriteLine("ERROR: Client is null or empty");
                    return BadRequest("Client is required");
                }

                if (string.IsNullOrEmpty(quotationDetails.Branch))
                {
                    Console.WriteLine("ERROR: Branch is null or empty");
                    return BadRequest("Branch is required");
                }

                if (string.IsNullOrEmpty(quotationDetails.Description))
                {
                    Console.WriteLine("ERROR: Description is null or empty");
                    return BadRequest("Description is required");
                }

                quotationDetails.LASTUPDATE = DateTime.Now;

                await _quotationRepository.SaveAndUpdateQuotationDetails(quotationDetails);

                Console.WriteLine($"Successfully saved QuotationDetails with ID: {quotationDetails.ID}");
                return Ok(new { Success = "Success", QuotationDetailID = quotationDetails.ID });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving QuotationDetails: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteQuotationDetailById")]
        public async Task<ActionResult<HttpResponseMessage>> DeleteQuotationDetailById(int Id)
        {
            try
            {
                var data = _oBMSDbContext.QuotationDetails.Where(x => x.ID == Id).FirstOrDefault();

                if (data != null)
                {
                    _oBMSDbContext.QuotationDetails.Remove(data);
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

      [HttpDelete("DeleteQuotationById")]
public async Task<IActionResult> DeleteQuotationById(int Id)
{
    try
    {
        // Get quotation
        var quotation = await _oBMSDbContext.Quotations
                            .FirstOrDefaultAsync(x => x.ID == Id);

        if (quotation == null)
        {
            return NotFound(new { Message = "Quotation not found" });
        }

        // Get related details
        var details = await _oBMSDbContext.QuotationDetails
                            .Where(x => x.QuotationID == Id)
                            .ToListAsync();

        if (details.Any())
        {
            _oBMSDbContext.QuotationDetails.RemoveRange(details);
        }

        // Remove quotation
        _oBMSDbContext.Quotations.Remove(quotation);

        // Save all changes together
        await _oBMSDbContext.SaveChangesAsync();

        return Ok(new { Message = "Deleted successfully" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            Message = "Error occurred",
            Error = ex.Message   
        });
    }
}

        [HttpGet]
        [Route("GetQuotationByID")]
        public async Task<ActionResult<Object>> GetQuotationByID(int quotationID)
        {
            try
            {
                var objList = await _quotationRepository.GetQuotationByID(quotationID);
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetQuotations")]
        public async Task<ActionResult<Object>> GetQuotations(string userID, string branchId, bool load)
        {
            try
            {
                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                var objList = await _quotationRepository.GetQuotations(branchId);

                dictResult.Add("quotations", objList);

                if (load)
                {
                    var branchs = await _oBMSDbContext.BranchMasters
                        .Join(_oBMSDbContext.OBMSBranches,
                            branchmaster => branchmaster.Code,
                            branches => branches.BranchCode,
                            (branchmaster, branches) => new { branchmaster, branches })
                        .Where(joinResult => joinResult.branches.Name == userID && joinResult.branches.IsAllowed == true)
                        .Select(joinResult => joinResult.branchmaster)
                        .ToListAsync();

                    dictResult.Add("branchList", branchs);
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
                var objList = await _quotationRepository.CheckClientStatus(branchId, clientId, status);
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetQuotationsDiscountReport")]
        public async Task<ActionResult<Object>> GetQuotationsDiscountReport(string branchId, string clientId, DateTime startdate, DateTime endDate)
        {
            try
            {
                var objList = await _quotationRepository.GetQuotationsDiscountReport(branchId, clientId, startdate, endDate);
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetQuotationsDiscountReportMaster")]
        public async Task<ActionResult<Object>> GetQuotationsDiscountReportMaster(string userID)
        {
            try
            {
                var objList = await _quotationRepository.GetQuotationMasterList(userID);
                return Ok(objList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
