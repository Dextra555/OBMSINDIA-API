using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using OBMS.WebAPI.Models.Domain;

using OBMS.WebAPI.Models.DTO;

using OBMS.WebAPI.Repositories.Implementation;

using OBMS.WebAPI.Repositories.Interface;

using System.Web.Services.Description;



namespace OBMS.WebAPI.Controllers

{

    [Route("api/[controller]")]

    [ApiController]

    public class MasterController : Controller

    {

        HttpResponseMessage response = new HttpResponseMessage();

        private readonly IMasterRepository _masterRepository;

        private readonly ISalaryProcess _iSalaryProcess;



        public MasterController(IMasterRepository masterRepository, ISalaryProcess iSalaryProcess)

        {

            _masterRepository = masterRepository;

            _iSalaryProcess = iSalaryProcess;

        }



        #region Branch Module Region

        [HttpGet]

        [Route("GetBranchMasterAll")]

        public async Task<ActionResult<BranchMasterRequestDto>> GetBranchMasterAll()

        {

            try

            {

                var branchList = await _masterRepository.GetBranchMasterAllList();

                return Ok(branchList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }

        [HttpGet]

        [Route("GetBranchMaster")]

        public async Task<ActionResult<BranchMasterRequestDto>> GetBranchMaster(string branchCode)

        {

            try

            {

                var branchList = await _masterRepository.GetBranchMasterList(branchCode);

                return Ok(branchList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }

        [HttpGet]

        [Route("GetBranchListByUserName")]

        public async Task<ActionResult<BranchMasterRequestDto>> GetBranchListByUserName(string userName)

        {

            try

            {

                var branchList = await _masterRepository.GetBranchListByUserName(userName);

                return Ok(branchList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }

        [HttpPost]

        [Route("SaveAndUpdateBranchMaster")]

        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateBranchMaster(BranchMasterRequestDto branchMasterRequestDto)

        {

            try

            {

                var branchDetails = new BranchMaster()

                {

                    ID = branchMasterRequestDto.ID,

                    Name = branchMasterRequestDto.Name,

                    Address1 = branchMasterRequestDto.Address1,

                    Address2 = branchMasterRequestDto.Address2,

                    BankAccount = branchMasterRequestDto.BankAccount,

                    BankBranch = branchMasterRequestDto.BankBranch,

                    BankName = branchMasterRequestDto.BankName,

                    City = branchMasterRequestDto.City,

                    Code = branchMasterRequestDto.Code,

                    Description = branchMasterRequestDto.Description,

                    Email = branchMasterRequestDto.Email,

                    Fax = branchMasterRequestDto.Fax,

                    IsHeadQuarters = branchMasterRequestDto.IsHeadQuarters,

                    LastUpdate = (branchMasterRequestDto.LastUpdate == null || branchMasterRequestDto.LastUpdate == DateTime.MinValue) ? DateTime.Now : branchMasterRequestDto.LastUpdate,

                    LastUpdatedBy = branchMasterRequestDto.LastUpdatedBy,

                    ParentBranch = branchMasterRequestDto.ParentBranch,

                    PersonIncharge = branchMasterRequestDto.PersonIncharge,

                    Phone = branchMasterRequestDto.Phone,

                    PostCode = branchMasterRequestDto.PostCode,

                    ShortName = branchMasterRequestDto.ShortName,

                    State = branchMasterRequestDto.State,

                    UbsCode = branchMasterRequestDto.UbsCode,

                };

                await _masterRepository.saveAndUpdateBranchMaster(branchDetails);



                

                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                dictResult.Add("Success", "Success");

                dictResult.Add("Branch", branchDetails);

                response.Headers.Add("Success", "Successfully save & update branch details.");



                return Ok(dictResult);

               

               

            }

            catch (Exception ex)

            {



                response.Headers.Add("Failure", "Branch save & update failed please check your key-in deatais....");

            }

            return response;

        }



        [HttpDelete("DeleteBranchMasterByCode")]

        public async Task<IActionResult> DeleteBranchMasterByCode(string code)

        {

            try

            {

                var result = await _masterRepository.DeleteBranchMasterByCode(code);

                if (!result.Success)

                {

                    return NotFound(new { Success = "Error", Message = result.Message });

                }

                return Ok(new { Success = "Success", Message = "Successfully deleted branch details" });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { Success = "Error", Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetBranchMasterCode")]

        public IActionResult GetBranchMasterCode()

        {

            try

            {

                string branchCode = _masterRepository.GetBranchMasterCode();

                if (branchCode != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("ClientCode", branchCode);

                    return Ok(dictResult);

                }

                return Ok(new { Success = "Error", Message = "Failed to generate branch code" });



            }

            catch (Exception ex)

            {

                return StatusCode(500, new { Success = "Error", Message = ex.Message });

            }



        }



        #endregion



        #region Client Module



        [HttpGet]

        [Route("GetClientMsterList")]

        public async Task<ActionResult<ClientMasterRequestDto>> GetClientMsterList(string clientCode, string status)

        {

            try

            {

                var clientList = await _masterRepository.GetClientMsterList(clientCode,status);

                return Ok(clientList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetClientMsterListByStatus")]

        public async Task<ActionResult<ClientMasterRequestDto>> GetClientMsterListByStatus(string status)

        {

            try

            {

                var clientList = await _masterRepository.GetClientMsterListByStatus(status);

                return Ok(clientList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetClientMsterListByBranch")]

        public async Task<ActionResult<ClientMasterRequestDto>> GetClientMsterListByBranch(string branchCode)

        {

            try

            {

                var clientList = await _masterRepository.GetClientMsterListByBranch(branchCode);

                return Ok(clientList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetClientMasterListByComplianceStatus")]

        public async Task<ActionResult<ClientMasterRequestDto>> GetClientMasterListByComplianceStatus(string branchCode, string status)

        {

            try

            {

                var clientList = await _masterRepository.GetClientMsterListByComplianceStatus(branchCode, status);

                return Ok(clientList);

            }

            catch (Exception ex)

            {

                throw;

            }

        }



        [HttpGet]

        [Route("GetComplianceSummary")]

        public async Task<ActionResult<object>> GetComplianceSummary(string branchCode)

        {

            try

            {

                var summary = await _masterRepository.GetComplianceSummary(branchCode);

                return Ok(summary);

            }

            catch (Exception ex)

            {

                throw;

            }

        }



        [HttpPost]

        [Route("SaveAndUpdateClientMaster")]

        public async Task<IActionResult> SaveAndUpdateClientMaster(ClientMasterRequestDto clientMasterRequestDto)

        {

            try

            {

                var clientDetails = new ClientMaster()

                {

                    ID = clientMasterRequestDto.ID,

                    Code = clientMasterRequestDto.Code,

                    Name = clientMasterRequestDto.Name,

                    Address1 = clientMasterRequestDto.Address1,

                    Address2 = clientMasterRequestDto.Address2,

                    Branch = clientMasterRequestDto.Branch,

                    Status = clientMasterRequestDto.Status,

                    SuperClientCode = clientMasterRequestDto.SuperClientCode,

                    PostCode = clientMasterRequestDto.PostCode,

                    City = clientMasterRequestDto.City,

                    State = clientMasterRequestDto.State,

                    Phone = clientMasterRequestDto.Phone,

                    Email = clientMasterRequestDto.Email,

                    Fax = clientMasterRequestDto.Fax,

                    IsClientHeadQuarters = clientMasterRequestDto.IsClientHeadQuarters,

                    PersonIncharge = clientMasterRequestDto.PersonIncharge,

                    Shortname = clientMasterRequestDto.Shortname,

                    //CreatedDate = clientMasterRequestDto.CreatedDate,

                    LastUpdatedBy = clientMasterRequestDto.LastUpdatedBy,

                    AgreementStart = clientMasterRequestDto.AgreementStart,

                    AgreementEnd = clientMasterRequestDto.AgreementEnd,

                    LastUpdatedDate = clientMasterRequestDto.LastUpdatedDate,

                    

                    // Indian Compliance Fields

                    GSTIN = string.IsNullOrWhiteSpace(clientMasterRequestDto.GSTIN) ? null : clientMasterRequestDto.GSTIN.Trim().ToUpper(),

                    PANNumber = string.IsNullOrWhiteSpace(clientMasterRequestDto.PANNumber) ? null : clientMasterRequestDto.PANNumber.Trim().ToUpper(),

                    TANNumber = string.IsNullOrWhiteSpace(clientMasterRequestDto.TANNumber) ? null : clientMasterRequestDto.TANNumber.Trim().ToUpper(),

                    CINNumber = string.IsNullOrWhiteSpace(clientMasterRequestDto.CINNumber) ? null : clientMasterRequestDto.CINNumber.Trim().ToUpper(),

                    GSTRegistrationStatus = clientMasterRequestDto.GSTRegistrationStatus,

                    IndianState = clientMasterRequestDto.IndianState,

                    PINCode = string.IsNullOrWhiteSpace(clientMasterRequestDto.PINCode) ? null : clientMasterRequestDto.PINCode.Trim(),

                    

                    // Shipping Address Fields

                    ShippingAddress1 = clientMasterRequestDto.ShippingAddress1,

                    ShippingAddress2 = clientMasterRequestDto.ShippingAddress2,

                    ShippingCity = clientMasterRequestDto.ShippingCity,

                    ShippingState = clientMasterRequestDto.ShippingState,

                    ShippingPINCode = string.IsNullOrWhiteSpace(clientMasterRequestDto.ShippingPINCode) ? null : clientMasterRequestDto.ShippingPINCode.Trim(),

                    

                    // Billing Address Fields

                    BillingAddress1 = clientMasterRequestDto.BillingAddress1,

                    BillingAddress2 = clientMasterRequestDto.BillingAddress2,

                    BillingCity = clientMasterRequestDto.BillingCity,

                    BillingState = clientMasterRequestDto.BillingState,

                    BillingPINCode = string.IsNullOrWhiteSpace(clientMasterRequestDto.BillingPINCode) ? null : clientMasterRequestDto.BillingPINCode.Trim(),



                    // Simplified Compliance Field

                    ClientComplianceStatus = clientMasterRequestDto.ClientComplianceStatus ?? "non_compliance_client"

                };

                var result = await _masterRepository.saveAndUpdateClientMaster(clientDetails);



                if (result != null)

                {

                    return Ok(new

                    {

                        Success = "Success",

                        Message = "Successfully saved & updated client details.",

                        Client = result

                    });

                }

                else

                {

                    return BadRequest(new

                    {

                        Success = "Error",

                        Message = "Client save & update failed. Please check your data."

                    });

                }

            }

            catch (Exception ex)

            {

                // Better error logging would go here

                return StatusCode(500, new

                {

                    Success = "Error",

                    Message = "An error occurred while saving client details: " + (ex.InnerException?.Message ?? ex.Message)

                });

            }



        }



        [HttpDelete("DeleteClientMasterByCode")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteClientMasterByCode(string code)

        {

            try

            {

                await _masterRepository.DeleteClientMasterByCode(code);

                return Ok(new { Success = "Success", Message = "Successfully deleted client details" });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { Success = "Error", Message = ex.Message });

            }

        }

        [HttpGet]

        [Route("GetNClientMasterCode")]

        public IActionResult GetNClientMasterCode()

        {

            try

            {

                string clientCode = _masterRepository.GetNClientMasterCode();

                if (clientCode != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("ClientCode", clientCode);

                    return Ok(dictResult);

                }

                return Ok(clientCode);



            }

            catch (Exception ex)

            {

                throw;

            }



        }

        #endregion



        #region Shift Timing Module



        [HttpGet]

        [Route("GetShiftMsterList")]

        public async Task<ActionResult<ShiftTimeMasterDto>> GetShiftMsterList()

        {

            try

            {

                var shiftList = await _masterRepository.GetShiftTimeMasterList();

                return Ok(shiftList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpPost]

        [Route("SaveAndUpdateShiftMaster")]

        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateShiftMaster(ShiftTimeMasterDto shiftTimeMasterDto)

        {

            try

            {

                var shiftDetails = new ShiftTimeMaster()

                {

                    ID = shiftTimeMasterDto.ID,

                    ShiftType = shiftTimeMasterDto.ShiftType,

                    ShiftFrom = shiftTimeMasterDto.ShiftFrom,

                    ShiftTo = shiftTimeMasterDto.ShiftTo,

                    LastUpdate = shiftTimeMasterDto.LastUpdate,

                    LastUpdatedBy = shiftTimeMasterDto.LastUpdatedBy

                };

                await _masterRepository.saveAndUpdateShiftTimeMaster(shiftDetails);



                if (shiftDetails != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("Shift", shiftDetails);

                    response.Headers.Add("Success", "Successfully save & update shift timing details.");



                    return Ok(dictResult);

                }

                else

                {

                    response.Headers.Add("Failure", "Shifi timing save & update failed please check your key-in deatais....");

                }

                return response;

            }

            catch (Exception ex)

            {



                throw;

            }



        }



        [HttpDelete("DeleteShiftMasterById")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteShiftMasterById(int Id)

        {

            try

            {

                await _masterRepository.DeleteShiftTimeMasterById(Id);

                response.Headers.Add("Success", "Successfully deleted shift timing detals");



            }

            catch (Exception ex)

            {

                throw;

            }

            return response;

        }



        #endregion



        #region EPF Module

        [HttpGet]

        [Route("GetEPFMaster")]

        public async Task<ActionResult<EPFRequestDto>> GetEPFMaster(int Id)

        {

            try

            {

                var sipList = await _masterRepository.GetEPFMasterList(Id);

                return Ok(sipList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpPost]

        [Route("SaveAndUpdateEPFMaster")]

        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateEPFMaster(EPFRequestDto ePFRequestDto)

        {

            try

            {

                var epfDetails = new EPF()

                {

                    epf_id = ePFRequestDto.epf_id,

                    epf_from = ePFRequestDto.epf_from,

                    epf_to = ePFRequestDto.epf_to,

                    epf_worker = ePFRequestDto.epf_worker,

                    epf_boss = ePFRequestDto.epf_boss,

                    epf_worker55 = ePFRequestDto.epf_worker55,

                    epf_worker8Pa = ePFRequestDto.epf_worker8Pa,

                    epf_boss55 = ePFRequestDto.epf_boss55,

                    LastUpdate = DateTime.Now,

                    LastUpdatedBy = ePFRequestDto.LastUpdatedBy,



                };

                await _masterRepository.saveAndUpdateEPFMaster(epfDetails);



                if (epfDetails != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("SIP", epfDetails);

                    response.Headers.Add("Success", "Successfully save & update EPF details.");



                    return Ok(dictResult);

                }

                else

                {

                    response.Headers.Add("Failure", "EPF Slab save & update failed please check your key-in deatais....");

                }

                return response;

            }

            catch (Exception ex)

            {



                throw;

            }



        }



        [HttpDelete("DeleteEPFMasterById")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteEPFMasterById(int Id)

        {

            try

            {

                await _masterRepository.DeleteEPFMasterById(Id);

                response.Headers.Add("Success", "Successfully deleted EPF details");



            }

            catch (Exception ex)

            {

                throw;

            }

            return response;

        }

        #endregion



        #region SIP Module

        [HttpGet]

        [Route("GetSIPMaster")]

        public async Task<ActionResult<SIPRequestDto>> GetSIPMaster(int Id)

        {

            try

            {

                var sipList = await _masterRepository.GetSIPMasterList(Id);

                return Ok(sipList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpPost]

        [Route("SaveAndUpdateSIPMaster")]

        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateSIPMaster(SIPRequestDto sIPRequestDto)

        {

            try

            {

                var sipDetails = new SIP()

                {

                    SIP_id = sIPRequestDto.SIP_id,

                    SIP_from = sIPRequestDto.SIP_from,

                    SIP_to = sIPRequestDto.SIP_to,

                    SIP_worker = sIPRequestDto.SIP_worker,

                    SIP_boss = sIPRequestDto.SIP_boss,

                    SIP_total = sIPRequestDto.SIP_total,

                    LastUpdate = sIPRequestDto.LastUpdate,

                    LastUpdatedBy = sIPRequestDto.LastUpdatedBy



                };

                await _masterRepository.saveAndUpdateSIPMaster(sipDetails);



                if (sipDetails != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("SIP", sipDetails);

                    response.Headers.Add("Success", "Successfully save & update SIP details.");



                    return Ok(dictResult);

                }

                else

                {

                    response.Headers.Add("Failure", "SIP save & update failed please check your key-in deatais....");

                }

                return response;

            }

            catch (Exception ex)

            {



                throw;

            }



        }



        [HttpDelete("DeleteSIPMasterById")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteSIPMasterById(int Id)

        {

            try

            {

                await _masterRepository.DeleteSIPMasterById(Id);

                response.Headers.Add("Success", "Successfully deleted SIP details");



            }

            catch (Exception ex)

            {

                throw;

            }

            return response;

        }

        #endregion



        #region Income Tax Module



        [HttpGet]

        [Route("GetIncomeTaxMaster")]

        public async Task<ActionResult<SIPRequestDto>> GetIncomeTaxMaster(int Id)

        {

            try

            {

                var incomeTaxList = await _masterRepository.GetIncomeTaxMasterList(Id);

                return Ok(incomeTaxList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }

        [HttpPost]

        [Route("saveAndUpdateIncomeTaxMaster")]

        public async Task<ActionResult<HttpResponseMessage>> saveAndUpdateIncomeTaxMaster(IncomeTaxRequestDto incomeTaxRequestDto)

        {

            var incomeTaxDeatils = new IncomeTax()

            {

                // Update the properties with the new values

                IT_ID = incomeTaxRequestDto.IT_ID,

                IT_SAL_FROM = incomeTaxRequestDto.IT_SAL_FROM,

                IT_SAL_TO = incomeTaxRequestDto.IT_SAL_TO,

                IT_CATEGORY1 = incomeTaxRequestDto.IT_CATEGORY1,

                K_2 = incomeTaxRequestDto.K_2,

                KA1_2 = incomeTaxRequestDto.KA1_2,

                KA2_2 = incomeTaxRequestDto.KA2_2,

                KA3_2 = incomeTaxRequestDto.KA3_2,

                KA4_2 = incomeTaxRequestDto.KA4_2,

                KA5_2 = incomeTaxRequestDto.KA5_2,

                KA6_2 = incomeTaxRequestDto.KA6_2,

                KA7_2 = incomeTaxRequestDto.KA7_2,

                KA8_2 = incomeTaxRequestDto.KA8_2,

                KA9_2 = incomeTaxRequestDto.KA9_2,

                KA10_2 = incomeTaxRequestDto.KA10_2,

                K_3 = incomeTaxRequestDto.K_3,

                KA1_3 = incomeTaxRequestDto.KA1_3,

                KA2_3 = incomeTaxRequestDto.KA2_3,

                KA3_3 = incomeTaxRequestDto.KA3_3,

                KA4_3 = incomeTaxRequestDto.KA4_3,

                KA5_3 = incomeTaxRequestDto.KA5_3,

                KA6_3 = incomeTaxRequestDto.KA6_3,

                KA7_3 = incomeTaxRequestDto.KA7_3,

                KA8_3 = incomeTaxRequestDto.KA8_3,

                KA9_3 = incomeTaxRequestDto.KA9_3,

                KA10_3 = incomeTaxRequestDto.KA10_3,

                LASTUPDATE = incomeTaxRequestDto.LASTUPDATE,

                LastUpdatedBy = incomeTaxRequestDto.LastUpdatedBy,

            };

            await _masterRepository.saveAndUpdateIncomeTaxMaster(incomeTaxDeatils);



            if (incomeTaxDeatils != null)

            {

                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                dictResult.Add("Success", "Success");

                dictResult.Add("IncomeTax", incomeTaxDeatils);

                response.Headers.Add("Success", "Successfully save & update Income Tax details.");



                return Ok(dictResult);

            }

            else

            {

                response.Headers.Add("Failure", "Income Tax save & update failed please check your key-in deatais....");

            }

            return response;



        }



        [HttpDelete("DeleteIncomeTaxMasterById")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteIncomeTaxMasterById(int Id)

        {

            try

            {

                await _masterRepository.DeleteIncomeTaxMasterById(Id);

                response.Headers.Add("Success", "Successfully deleted IncomeTax details");



            }

            catch (Exception ex)

            {

                throw;

            }

            return response;

        }

        #endregion



        #region Leave Module

        [HttpGet]

        [Route("GetLeaveMaster")]

        public async Task<ActionResult<LeaveSystemRequestDto>> GetLeaveMaster(int Id)

        {

            try

            {

                var leaveList = await _masterRepository.GetLeaveMasterList(Id);

                return Ok(leaveList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpPost]

        [Route("saveAndUpdateLeaveMaster")]

        public async Task<ActionResult<HttpResponseMessage>> saveAndUpdateLeaveMaster(LeaveSystemRequestDto leaveSystemRequestDto)

        {

            try

            {

                var leaveDetails = new LeaveSystem()

                {

                    LS_ID = leaveSystemRequestDto.LS_ID,

                    al0to1 = leaveSystemRequestDto.al0to1,

                    AL1to2 = leaveSystemRequestDto.AL1to2,

                    AL2to5 = leaveSystemRequestDto.AL2to5,

                    AL6 = leaveSystemRequestDto.AL6,

                    ml0to2 = leaveSystemRequestDto.ml0to2,

                    ml2to5 = leaveSystemRequestDto.ml2to5,

                    ML6 = leaveSystemRequestDto.ML6,

                    HL = leaveSystemRequestDto.HL,

                    MtnyL = leaveSystemRequestDto.MtnyL,

                    PtnyL = leaveSystemRequestDto.PtnyL,

                    LASTUPDATE = leaveSystemRequestDto.LASTUPDATE,

                    LastUpdatedBy = leaveSystemRequestDto.LastUpdatedBy,



                };

                await _masterRepository.saveAndUpdateLeaveMaster(leaveDetails);



                if (leaveDetails != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("Leave", leaveDetails);

                    response.Headers.Add("Success", "Successfully save & update Leave details.");



                    return Ok(dictResult);

                }

                else

                {

                    response.Headers.Add("Failure", "Leave save & update failed please check your key-in deatais....");

                }

                return response;

            }

            catch (Exception ex)

            {



                throw;

            }



        }



        [HttpDelete("DeleteLeaveMasterById")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteLeaveMasterById(int Id)

        {

            try

            {

                await _masterRepository.DeleteLeaveMasterById(Id);

                response.Headers.Add("Success", "Successfully deleted Leave details");



            }

            catch (Exception ex)

            {

                throw;

            }

            return response;

        }



        #endregion





        #region SOCSO Module





        [HttpGet]

        [Route("GetSOCSOMaster")]

        public async Task<ActionResult<SOCSO>> GetSOCSOMaster(int socsoId)

        {

            try

            {

                var socsoList = await _masterRepository.GetSOCSOMasterList(socsoId);

                return Ok(socsoList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpPost]

        [Route("SaveAndUpdateSOCSOMaster")]

        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateSOCSOMaster(SOCSORequestDto socsoMaster)

        {

            try

            {

                var socsoDetails = new SOCSO()

                {

                    socso_id = socsoMaster.socso_id,

                    socso_from = socsoMaster.socso_from,

                    socso_to = socsoMaster.socso_to,

                    socso_employer = socsoMaster.socso_employer,

                    socso_worker = socsoMaster.socso_worker,

                    socso_50year = socsoMaster.socso_50year,

                    socso_foreigner = socsoMaster.socso_foreigner,

                    LASTUPDATE = socsoMaster.LASTUPDATE,

                    LastUpdatedBy = socsoMaster.LastUpdatedBy,



                };

                await _masterRepository.saveAndUpdateSOCSOMaster(socsoDetails);



                if (socsoDetails != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("Leave", socsoDetails);

                    response.Headers.Add("Success", "Successfully save & update SOCSO details.");



                    return Ok(dictResult);

                }

                else

                {

                    response.Headers.Add("Failure", "SOCSO save & update failed please check your key-in deatais....");

                }

                return response;

            }

            catch (Exception ex)

            {



                throw;

            }



        }



        [HttpDelete("DeleteSOCSOMasterById")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteSOCSOMasterById(int socsoId)

        {

            try

            {

                await _masterRepository.DeleteSOCSOMasterById(socsoId);

                response.Headers.Add("Success", "Successfully deleted SOCSO details");



            }

            catch (Exception ex)

            {

                throw;

            }

            return response;

        }



        #endregion



        #region Salary Structure Module

        [HttpGet]

        [Route("GetSalaryMaster")]

        public async Task<ActionResult<SalaryStructureDto>> GetSalaryMasterList(int salaryId, string status)

        {

            try

            {

                var salaryList = await _masterRepository.GetSalaryMasterList(salaryId,status);

                return Ok(salaryList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }

        [HttpGet]

        [Route("GetSalaryListByStatus")]

        public async Task<ActionResult<SalaryStructureDto>> GetSalaryListByStatus(string activeStatus)

        {

            try

            {

                var salaryList = await _masterRepository.GetSalaryListByStatus(activeStatus);

                return Ok(salaryList);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpPost]

        [Route("saveAndUpdateSalaryMaster")]

        public async Task<ActionResult<HttpResponseMessage>> saveAndUpdateSalaryMaster(SalaryStructureDto salaryStructure)

        {

            try

            {

                var salaryDetails = new SalaryStructure()

                {

                    SalaryId = salaryStructure.SalaryId,

                    BranchCode = salaryStructure.BranchCode,

                    EmployeeType = salaryStructure.EmployeeType,

                    EmployeeNationality = salaryStructure.EmployeeNationality,

                    Name = salaryStructure.Name,

                    GeneralDayRate = salaryStructure.GeneralDayRate,

                    GeneralDayHours = (int)salaryStructure.WorkingHours,

                    GeneralDayOTRate = salaryStructure.GeneralDayOTRate,

                    OffDayRate = salaryStructure.OffDayRate,

                    OffDayOTRate = salaryStructure.OffDayOTRate,

                    HolidayRate = salaryStructure.HolidayRate,

                    HolidayOTRate = salaryStructure.HolidayOTRate,

                    WorkingDays = salaryStructure.WorkingDays,

                    WorkingHours = salaryStructure.WorkingHours,

                    SalaryBand = salaryStructure.SalaryBand,

                    TravelAllowance = salaryStructure.TravelAllowance,

                    Status = salaryStructure.Status,

                    EICC = salaryStructure.EICC,

                    NonStructure = salaryStructure.NonStructure,

                    Active = salaryStructure.Active,

                    LastUpdatedBy = salaryStructure.LastUpdatedBy,

                    LastUpdatedDate = DateTime.Now,



                };

                await _masterRepository.saveAndUpdateSalaryMaster(salaryDetails);



                if (salaryDetails != null)

                {

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();

                    dictResult.Add("Success", "Success");

                    dictResult.Add("Leave", salaryDetails);

                    response.Headers.Add("Success", "Successfully save & update Salary details.");



                    return Ok(dictResult);

                }

                else

                {

                    response.Headers.Add("Failure", "Branch save & update failed please check your key-in deatais....");

                }

                return response;

            }

            catch (Exception ex)

            {



                throw;

            }



        }



        [HttpDelete("DeleteSalaryMasterById")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteSalaryMasterById(int salaryId)

        {

            try

            {

                await _masterRepository.DeleteSalaryMasterById(salaryId);

                response.Headers.Add("Success", "Successfully deleted Salary details");



            }

            catch (Exception ex)

            {

                throw;

            }

            return response;

        }

        #endregion



        #region Master Reports

        [HttpGet]

        [Route("GetKKDNListView")]

        public async Task<IActionResult> GetKKDNListView(string branch,string kdnVetting)

        {

            try

            {

                var result = await _masterRepository.GetListWithBlankRowAsync(branch, kdnVetting);

                return Ok(result);

            }

            catch (Exception ex)

            {

                // Log the exception

                return StatusCode(500, "Internal server error: " + ex.Message);

            }

        }



        [HttpGet]

        [Route("GetKKDNList")]

        public async Task<IActionResult> GetKKDNList(string branch, string EmployeeType, DateTime dtDateJoinFrom, DateTime dtDateJoinTo, string kdnVetting)

        {

            try

            {

                var result = _iSalaryProcess.GetListWithBlankRow(branch, EmployeeType, dtDateJoinFrom, dtDateJoinTo, kdnVetting);

                return Ok(result);

            }

            catch (Exception ex)

            {

                // Log the exception

                return StatusCode(500, "Internal server error: " + ex.Message);

            }

        }



        #endregion



    }

}

