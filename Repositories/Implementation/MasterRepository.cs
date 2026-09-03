using Azure;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Primitives;

using OBMS.WebAPI.Models;

using OBMS.WebAPI.Models.Domain;

using OBMS.WebAPI.Models.DTO;

using OBMS.WebAPI.Repositories.Interface;

using OBMS.WebAPI.Utility;

using SkiaSharp;

using System.ComponentModel.Design;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace OBMS.WebAPI.Repositories.Implementation

{

    public class MasterRepository : IMasterRepository

    {

        private readonly OBMSDbContext _oBMSDbContext;

        private readonly IRegisterRepository _registerRepository;



        public MasterRepository(IRegisterRepository registerRepository, OBMSDbContext oBMSDbContext)

        {

            _oBMSDbContext = oBMSDbContext;

            _registerRepository = registerRepository;

        }





        //Branch Master Module logic methods

        #region Branch Master Module



        public async Task<List<BranchMaster>> GetBranchMasterAllList()

        {

            var result = _oBMSDbContext.BranchMasters.ToList();

            return new List<BranchMaster>(result);



        }

        public async Task<List<BranchMaster>> GetBranchListByUserName(string userName)

        {

            bool isSuperAdmin = string.Equals(userName?.Trim(), "superadmin", StringComparison.OrdinalIgnoreCase);

            List<BranchMaster> result;

            if (isSuperAdmin)
            {
                result = await _oBMSDbContext.BranchMasters
                    .Select(x => new BranchMaster
                    {
                        ID = x.ID,
                        Code = x.Code,
                        Name = x.Name,
                        Address1 = x.Address1,
                        Address2 = x.Address2,
                        PostCode = x.PostCode,
                        City = x.City,
                        State = x.State,
                        Phone = x.Phone,
                        Fax = x.Fax,
                        BankName = x.BankName,
                        BankBranch = x.BankBranch,
                        BankAccount = x.BankAccount,
                        PersonIncharge = x.PersonIncharge,
                        Email = x.Email,
                        Description = x.Description,
                        ShortName = x.ShortName,
                        IsHeadQuarters = x.IsHeadQuarters,
                        UbsCode = x.UbsCode,
                        LastUpdate = x.LastUpdate,
                        LastUpdatedBy = x.LastUpdatedBy,
                        ParentBranch = x.ParentBranch
                    })
                    .Distinct()
                    .ToListAsync();

                return result;
            }

            result = await _oBMSDbContext.BranchMasters
                .Join(_oBMSDbContext.OBMSBranches,
                    branchmaster => branchmaster.Code,
                    branches => branches.BranchCode,
                    (branchmaster, branches) => new { branchmaster, branches })
                .Where(joinResult => joinResult.branches.Name == userName && joinResult.branches.IsAllowed == true)
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
                .Distinct()
                .ToListAsync();

            return result;

        }





        public async Task<List<BranchMaster>> GetBranchMasterList(string branchCode)

        {

            

                if (branchCode != null && branchCode != "null")

                {

                    var result = _oBMSDbContext.BranchMasters.Where(x => x.Code == branchCode).OrderBy(x => x.Name).ToList();

                    return new List<BranchMaster>(result);

                }

                else

                {

                    var result = _oBMSDbContext.BranchMasters.OrderBy(x => x.Name).ToList();

                    return new List<BranchMaster>(result);

                }



            



            return new List<BranchMaster>(null);



        }



        public async Task<BranchMaster> saveAndUpdateBranchMaster(BranchMaster branchMaster)

        {

            // Store original code for lookup

            string originalCode = branchMaster.Code;

            

            var existingBranchDetails = await _oBMSDbContext.BranchMasters.Where(x => x.Code == originalCode).SingleOrDefaultAsync();

            if (existingBranchDetails != null)

            {   

                // For updates, keep the original code unchanged

                branchMaster.Code = originalCode;

                

                existingBranchDetails.UbsCode = branchMaster.UbsCode;

                existingBranchDetails.Name = branchMaster.Name;

                existingBranchDetails.ShortName = branchMaster.ShortName;

                existingBranchDetails.PersonIncharge = branchMaster.PersonIncharge;

                existingBranchDetails.Address1 = branchMaster.Address1;

                existingBranchDetails.Address2 = branchMaster.Address2;

                existingBranchDetails.PostCode = branchMaster.PostCode;

                existingBranchDetails.City = branchMaster.City;

                existingBranchDetails.State = branchMaster.State;

                existingBranchDetails.Phone = branchMaster.Phone;

                existingBranchDetails.Fax = branchMaster.Fax;

                existingBranchDetails.Email = branchMaster.Email;

                existingBranchDetails.BankName = branchMaster.BankName;

                existingBranchDetails.BankBranch = branchMaster.BankBranch;

                existingBranchDetails.BankAccount = branchMaster.BankAccount;

                

                // Handle ParentBranch

                existingBranchDetails.ParentBranch = branchMaster.ParentBranch;

                

                existingBranchDetails.LastUpdatedBy = branchMaster.LastUpdatedBy;

                existingBranchDetails.LastUpdate = branchMaster.LastUpdate;

                existingBranchDetails.Description = branchMaster.Description;

                existingBranchDetails.IsHeadQuarters = branchMaster.IsHeadQuarters;

                

                // Update the code as well

                existingBranchDetails.Code = branchMaster.Code;

                

                _oBMSDbContext.Update(existingBranchDetails);

                await _oBMSDbContext.SaveChangesAsync();

                

                if (branchMaster != null)

                {

                    var obmsBranches = new List<OBMSBranches>()

                    {

                        new OBMSBranches

                        {

                            Name = branchMaster.LastUpdatedBy,

                            BranchCode = branchMaster.Code,

                            IsAllowed = true,

                            LastUpdatedBy = branchMaster.LastUpdatedBy,

                            LastUpdatedDate = DateTime.Now

                        }

                    };

                    await _registerRepository.SaveAndUpdateObmsBranches(obmsBranches);

                    

                }

                return existingBranchDetails;

            }

            else

            {

                // For new branches, generate the final code

                if (!string.IsNullOrEmpty(branchMaster.ShortName))

                {

                    branchMaster.Code = branchMaster.Code + "-" + branchMaster.ShortName;

                }

                

                // For new branches, ensure ParentBranch reference is valid

                if (!string.IsNullOrEmpty(branchMaster.ParentBranch))

                {

                    var parentExists = await _oBMSDbContext.BranchMasters

                        .AnyAsync(x => x.Code == branchMaster.ParentBranch);

                    

                    if (!parentExists)

                    {

                        throw new Exception($"Parent branch with code '{branchMaster.ParentBranch}' does not exist.");

                    }

                }

                

                _oBMSDbContext.Add(branchMaster);

                await _oBMSDbContext.SaveChangesAsync();

                

                if (branchMaster != null)

                {

                    var obmsBranches = new List<OBMSBranches>()

                    {

                        new OBMSBranches

                        {                               

                            Name = branchMaster.LastUpdatedBy,

                            BranchCode = branchMaster.Code,

                            IsAllowed = false,

                            LastUpdatedBy = branchMaster.LastUpdatedBy,

                            LastUpdatedDate = DateTime.Now

                        }

                    };

                    await _registerRepository.SaveAndUpdateObmsBranches(obmsBranches);

                    

                }

                return branchMaster;

            }

        }



        public async Task<(bool Success, BranchMaster? Branch, string Message)> DeleteBranchMasterByCode(string code)

        {

            try

            {

                var existingBranch = _oBMSDbContext.BranchMasters.Where(m => m.Code == code).SingleOrDefault();

                if (existingBranch != null)

                {

                    // Check if branch has child branches

                    var childBranches = _oBMSDbContext.BranchMasters.Where(b => b.ParentBranch == code).ToList();

                    if (childBranches.Any())

                    {

                        return (false, null, "Cannot delete branch with child branches");

                    }



                    // Remove user assignments from OBMSBranches before deleting branch

                    var obmsBranches = _oBMSDbContext.OBMSBranches.Where(b => b.BranchCode == code).ToList();

                    if (obmsBranches.Any())

                    {

                        _oBMSDbContext.OBMSBranches.RemoveRange(obmsBranches);

                        await _oBMSDbContext.SaveChangesAsync();

                    }



                    // Check if branch has clients

                    var clients = _oBMSDbContext.ClientMasters.Where(c => c.Branch == code).ToList();

                    if (clients.Any())

                    {

                        return (false, null, "Cannot delete branch with associated clients");

                    }



                    _oBMSDbContext.BranchMasters.Remove(existingBranch);

                    await _oBMSDbContext.SaveChangesAsync();

                    

                    return (true, existingBranch, "Successfully deleted branch details");

                }

                return (false, null, "Branch not found");

            }

            catch (Exception ex)

            {

                return (false, null, $"An error occurred while deleting the branch: {ex.Message}");

            }

        }

        #endregion



        //Client Master Module logic methods

        #region Client Master Module

        public async Task<List<ClientMaster>> GetClientMsterList(string clientCode, string status)

        {

           if (clientCode != null && clientCode != "null")

            {

                var result = _oBMSDbContext.ClientMasters.Where(x => x.Code == clientCode && x.Status == status).OrderBy(x => x.Name).ToList();

                return new List<ClientMaster>(result);

            }

            else

            {

                var result = _oBMSDbContext.ClientMasters.Where(x => x.Status == status).OrderBy(x => x.Name).ToList();

                return new List<ClientMaster>(result);

            }

        }

        public async Task<List<ClientMaster>> GetClientMsterListByStatus(string status)

        {

                var result = _oBMSDbContext.ClientMasters.Where(x => x.Status == status).OrderBy(x => x.Name).ToList();

                return new List<ClientMaster>(result);

        }



        public async Task<List<ClientMaster>> GetClientMsterListByBranch(string branchCode)

        {

            var result = _oBMSDbContext.ClientMasters.Where(x => x.Branch == branchCode).OrderBy(x => x.Name).ToList();

            return new List<ClientMaster>(result);

        }

        public async Task<ClientMaster> saveAndUpdateClientMaster(ClientMaster clientMaster)

        {

            DateTime targetDate = new DateTime(1990, 1, 1, 0, 0, 0);

            var existingClientDetails = await _oBMSDbContext.ClientMasters.Where(x => x.Code == clientMaster.Code).SingleOrDefaultAsync();

            if (existingClientDetails != null)

            {

                existingClientDetails.Code = clientMaster.Code;

                existingClientDetails.Name = clientMaster.Name;

                existingClientDetails.Address1 = clientMaster.Address1;

                existingClientDetails.Address2 = clientMaster.Address2;

                existingClientDetails.PostCode = clientMaster.PostCode;

                existingClientDetails.City = clientMaster.City;

                existingClientDetails.State = clientMaster.State;

                existingClientDetails.Phone = clientMaster.Phone;

                existingClientDetails.Fax = clientMaster.Fax;

                existingClientDetails.Email = clientMaster.Email;

                existingClientDetails.Branch = clientMaster.Branch;

                existingClientDetails.Shortname = clientMaster.Shortname;

                existingClientDetails.PersonIncharge = clientMaster.PersonIncharge;

                existingClientDetails.Status = clientMaster.Status;

                existingClientDetails.SuperClientCode = clientMaster.SuperClientCode;

                existingClientDetails.LastUpdatedBy = clientMaster.LastUpdatedBy;

                existingClientDetails.LastUpdatedDate = DateTime.Now;

                existingClientDetails.AgreementStart = clientMaster.AgreementStart;

                existingClientDetails.AgreementEnd = clientMaster.AgreementEnd == targetDate ? null : clientMaster.AgreementEnd;

                existingClientDetails.IsClientHeadQuarters = clientMaster.IsClientHeadQuarters;

                

                // Update Indian Compliance Fields

                existingClientDetails.GSTIN = string.IsNullOrWhiteSpace(clientMaster.GSTIN) ? null : clientMaster.GSTIN.Trim().ToUpper();

                existingClientDetails.PANNumber = string.IsNullOrWhiteSpace(clientMaster.PANNumber) ? null : clientMaster.PANNumber.Trim().ToUpper();

                existingClientDetails.TANNumber = string.IsNullOrWhiteSpace(clientMaster.TANNumber) ? null : clientMaster.TANNumber.Trim().ToUpper();

                existingClientDetails.CINNumber = string.IsNullOrWhiteSpace(clientMaster.CINNumber) ? null : clientMaster.CINNumber.Trim().ToUpper();

                existingClientDetails.GSTRegistrationStatus = clientMaster.GSTRegistrationStatus;

                existingClientDetails.IndianState = clientMaster.IndianState;

                existingClientDetails.PINCode = string.IsNullOrWhiteSpace(clientMaster.PINCode) ? null : clientMaster.PINCode.Trim();

                

                // Update Shipping Address Fields

                existingClientDetails.ShippingAddress1 = clientMaster.ShippingAddress1;

                existingClientDetails.ShippingAddress2 = clientMaster.ShippingAddress2;

                existingClientDetails.ShippingCity = clientMaster.ShippingCity;

                existingClientDetails.ShippingState = clientMaster.ShippingState;

                existingClientDetails.ShippingPINCode = string.IsNullOrWhiteSpace(clientMaster.ShippingPINCode) ? null : clientMaster.ShippingPINCode.Trim();

                

                // Update Billing Address Fields

                existingClientDetails.BillingAddress1 = clientMaster.BillingAddress1;

                existingClientDetails.BillingAddress2 = clientMaster.BillingAddress2;

                existingClientDetails.BillingCity = clientMaster.BillingCity;

                existingClientDetails.BillingState = clientMaster.BillingState;

                existingClientDetails.BillingPINCode = string.IsNullOrWhiteSpace(clientMaster.BillingPINCode) ? null : clientMaster.BillingPINCode.Trim();

                

                // Update simplified compliance field

                existingClientDetails.ClientComplianceStatus = clientMaster.ClientComplianceStatus ?? "non_compliance_client";



                _oBMSDbContext.Update(existingClientDetails);

                await _oBMSDbContext.SaveChangesAsync();

                return existingClientDetails;

            }

            else

            {

                clientMaster.LastUpdatedDate = clientMaster.LastUpdatedDate == targetDate ? null : clientMaster.LastUpdatedDate;

                clientMaster.AgreementEnd = clientMaster.AgreementEnd == targetDate ? null : clientMaster.AgreementEnd;

                _oBMSDbContext.Add(clientMaster);               

                await _oBMSDbContext.SaveChangesAsync();

                return clientMaster;

            }

        }



        public async Task<ActionResult<ClientMaster>> DeleteClientMasterByCode(string code)

        {

            var existingClient = _oBMSDbContext.ClientMasters.Where(m => m.Code == code).SingleOrDefault();

            if (existingClient != null)

            {

                _oBMSDbContext.ClientMasters.Remove(existingClient);

                await _oBMSDbContext.SaveChangesAsync();

            }

            return new ActionResult<ClientMaster>(existingClient);

        }



        public async Task<List<ClientMaster>> GetClientMsterListByComplianceStatus(string branchCode, string status)

        {

            var query = _oBMSDbContext.ClientMasters.AsQueryable();

            

            if (!string.IsNullOrEmpty(branchCode) && branchCode != "null")

            {

                query = query.Where(x => x.Branch == branchCode);

            }



            // Use the new simplified compliance status

            if (status.ToLower() == "compliant")

            {

                query = query.Where(x => x.ClientComplianceStatus == "compliance_client");

            }

            else

            {

                query = query.Where(x => x.ClientComplianceStatus == "non_compliance_client");

            }



            return await query.OrderBy(x => x.Name).ToListAsync();

        }



        public async Task<object> GetComplianceSummary(string branchCode)

        {

            var query = _oBMSDbContext.ClientMasters.AsQueryable();

            

            if (!string.IsNullOrEmpty(branchCode) && branchCode != "null")

            {

                query = query.Where(x => x.Branch == branchCode);

            }



            var totalClients = await query.CountAsync();

            var compliantClients = await query.CountAsync(x => x.ClientComplianceStatus == "compliance_client");

            var nonCompliantClients = await query.CountAsync(x => x.ClientComplianceStatus == "non_compliance_client");



            return new 

            {

                Total = totalClients,

                Compliant = compliantClients,

                NonCompliant = nonCompliantClients

            };

        }



        public string GetNClientMasterCode()

        {

            var result = _oBMSDbContext.ClientMasters

                .AsEnumerable()

                .Select(j => j.ID)

                .DefaultIfEmpty(0) // Handle the case where there are no valid integer codes

                .Max();

            var newClientCode = result + 1;



            return newClientCode.ToString("FwgC000000");

        }

        #endregion



        //shift time master module logic methods

        #region Shift Time Master

        public async Task<List<ShiftTimeMaster>> GetShiftTimeMasterList()

        {

            var result = await _oBMSDbContext.ShiftTimeMasters.ToListAsync();

            return new List<ShiftTimeMaster>(result);

        }



        public async Task<ShiftTimeMaster> saveAndUpdateShiftTimeMaster(ShiftTimeMaster shiftTimeMaster)

        {

            var existingShiftDetails = await _oBMSDbContext.ShiftTimeMasters.Where(x => x.ShiftType == shiftTimeMaster.ShiftType).SingleOrDefaultAsync();

            if (existingShiftDetails != null)

            {

                existingShiftDetails.ShiftType = shiftTimeMaster.ShiftType;

                existingShiftDetails.ShiftFrom = shiftTimeMaster.ShiftFrom;

                existingShiftDetails.ShiftTo = shiftTimeMaster.ShiftTo;

                existingShiftDetails.LastUpdate = shiftTimeMaster.LastUpdate;

                existingShiftDetails.LastUpdatedBy = shiftTimeMaster.LastUpdatedBy;

                _oBMSDbContext.Update(existingShiftDetails);

                await _oBMSDbContext.SaveChangesAsync();

                return existingShiftDetails;

            }

            else

            {

                _oBMSDbContext.Add(shiftTimeMaster);

                await _oBMSDbContext.SaveChangesAsync();

                return shiftTimeMaster;

            }



            throw new NotImplementedException();

        }



        public async Task<ActionResult<ShiftTimeMaster>> DeleteShiftTimeMasterById(int Id)

        {

            var existingShift = _oBMSDbContext.ShiftTimeMasters.Where(m => m.ID == Id).SingleOrDefault();

            if (existingShift != null)

            {

                _oBMSDbContext.ShiftTimeMasters.Remove(existingShift);

                await _oBMSDbContext.SaveChangesAsync();

            }

            return new ActionResult<ShiftTimeMaster>(existingShift);

        }

        #endregion



        #region SIP Region

        public async Task<List<SIP>> GetSIPMasterList(int Id)

        {

            if (Id > 0)

            {

                var result = await _oBMSDbContext.SIPs.Where(x => x.SIP_id == Id).ToListAsync();

                return new List<SIP>(result);

            }

            else

            {

                var result = await _oBMSDbContext.SIPs.ToListAsync();

                return new List<SIP>(result);

            }

        }



        public async Task<SIP> saveAndUpdateSIPMaster(SIP sipMaster)

        {

            var existingSIPDetails = await _oBMSDbContext.SIPs.Where(x => x.SIP_id == sipMaster.SIP_id).SingleOrDefaultAsync();

            if (existingSIPDetails != null)

            {

                existingSIPDetails.SIP_from = sipMaster.SIP_from;

                existingSIPDetails.SIP_to = sipMaster.SIP_to;

                existingSIPDetails.SIP_worker = sipMaster.SIP_worker;

                existingSIPDetails.SIP_boss = sipMaster.SIP_boss;

                existingSIPDetails.SIP_total = sipMaster.SIP_total;

                existingSIPDetails.LastUpdate = sipMaster.LastUpdate;

                existingSIPDetails.LastUpdatedBy = sipMaster.LastUpdatedBy;

                _oBMSDbContext.Update(existingSIPDetails);

                await _oBMSDbContext.SaveChangesAsync();

                return existingSIPDetails;

            }

            else

            {

                _oBMSDbContext.Add(sipMaster);

                await _oBMSDbContext.SaveChangesAsync();

                return sipMaster;

            }

            throw new NotImplementedException();

        }



        public async Task<ActionResult<SIP>> DeleteSIPMasterById(int Id)

        {

            var existingSIP = _oBMSDbContext.SIPs.Where(m => m.SIP_id == Id).SingleOrDefault();

            if (existingSIP != null)

            {

                _oBMSDbContext.SIPs.Remove(existingSIP);

                await _oBMSDbContext.SaveChangesAsync();

            }

            return new ActionResult<SIP>(existingSIP);

        }



        #endregion



        #region EPF Region        



        public async Task<List<EPF>> GetEPFMasterList(int Id)

        {

            if (Id > 0)

            {

                var result = await _oBMSDbContext.EPFs.Where(x => x.epf_id == Id).ToListAsync();

                return result.Where(x => x != null).ToList();

                //return new List<EPF>(result);

            }

            else

            {

                var result = await _oBMSDbContext.EPFs.ToListAsync();

                return result.Where(x => x != null).ToList();

                //return new List<EPF>(result);

            }

        }



        public async Task<EPF> saveAndUpdateEPFMaster(EPF epfMaster)

        {

            var existingEPFDetails = await _oBMSDbContext.EPFs.Where(x => x.epf_id == epfMaster.epf_id).SingleOrDefaultAsync();

            if (existingEPFDetails != null)

            {

                existingEPFDetails.epf_from = epfMaster.epf_from;

                existingEPFDetails.epf_to = epfMaster.epf_to;

                existingEPFDetails.epf_worker = epfMaster.epf_worker;

                existingEPFDetails.epf_boss = epfMaster.epf_boss;

                existingEPFDetails.epf_worker55 = epfMaster.epf_worker55;

                existingEPFDetails.epf_worker8Pa = epfMaster.epf_worker8Pa;

                existingEPFDetails.epf_boss55 = epfMaster.epf_boss55;

                existingEPFDetails.LastUpdate = epfMaster.LastUpdate;

                existingEPFDetails.LastUpdatedBy = epfMaster.LastUpdatedBy;

                _oBMSDbContext.Update(existingEPFDetails);

                await _oBMSDbContext.SaveChangesAsync();

                return existingEPFDetails;

            }

            else

            {

                _oBMSDbContext.Add(epfMaster);

                await _oBMSDbContext.SaveChangesAsync();

                return epfMaster;

            }

        }



        public async Task<ActionResult<EPF>> DeleteEPFMasterById(int Id)

        {

            var existingEPF = _oBMSDbContext.EPFs.Where(m => m.epf_id == Id).SingleOrDefault();

            if (existingEPF != null)

            {

                _oBMSDbContext.EPFs.Remove(existingEPF);

                await _oBMSDbContext.SaveChangesAsync();

            }

            return new ActionResult<EPF>(existingEPF);

        }



        #endregion



        #region Leave System Region

        public async Task<List<LeaveSystem>> GetLeaveMasterList(int Id)

        {

            if (Id > 0)

            {

                var result = await _oBMSDbContext.LeaveSystems.Where(x => x.LS_ID == Id).ToListAsync();

                return new List<LeaveSystem>(result);

            }

            else

            {

                var result = await _oBMSDbContext.LeaveSystems.ToListAsync();

                return new List<LeaveSystem>(result);

            }

        }



        public async Task<LeaveSystem> saveAndUpdateLeaveMaster(LeaveSystem leaveMaster)

        {

            var existingLeaveDetails = await _oBMSDbContext.LeaveSystems.Where(x => x.LS_ID == leaveMaster.LS_ID).SingleOrDefaultAsync();

            if (existingLeaveDetails != null)

            {

                existingLeaveDetails.al0to1 = leaveMaster.al0to1;

                existingLeaveDetails.AL1to2 = leaveMaster.AL1to2;

                existingLeaveDetails.AL2to5 = leaveMaster.AL2to5;

                existingLeaveDetails.AL6 = leaveMaster.AL6;

                existingLeaveDetails.ml0to2 = leaveMaster.ml0to2;

                existingLeaveDetails.ml2to5 = leaveMaster.ml2to5;

                existingLeaveDetails.ML6 = leaveMaster.ML6;

                existingLeaveDetails.HL = leaveMaster.HL;

                existingLeaveDetails.MtnyL = leaveMaster.MtnyL;

                existingLeaveDetails.PtnyL = leaveMaster.PtnyL;

                existingLeaveDetails.LASTUPDATE = leaveMaster.LASTUPDATE;

                existingLeaveDetails.LastUpdatedBy = leaveMaster.LastUpdatedBy;

                _oBMSDbContext.Update(existingLeaveDetails);

                await _oBMSDbContext.SaveChangesAsync();

                return existingLeaveDetails;

            }

            else

            {

                _oBMSDbContext.Add(leaveMaster);

                await _oBMSDbContext.SaveChangesAsync();

                return leaveMaster;

            }

        }



        public async Task<ActionResult<LeaveSystem>> DeleteLeaveMasterById(int Id)

        {

            var existingLeave = _oBMSDbContext.LeaveSystems.Where(m => m.LS_ID == Id).SingleOrDefault();

            if (existingLeave != null)

            {

                _oBMSDbContext.LeaveSystems.Remove(existingLeave);

                await _oBMSDbContext.SaveChangesAsync();

            }

            return new ActionResult<LeaveSystem>(existingLeave);

        }

        #endregion



        #region Salary Structure Region

        public async Task<List<SalaryStructure>> GetSalaryMasterList(int salaryId, string status)

        {

            if (salaryId > 0)

            {

                var result = await _oBMSDbContext.SalaryStructures.Where(x => x.SalaryId == salaryId && x.Status == status).ToListAsync();

                return new List<SalaryStructure>(result);

            }

            else

            {

                var result = await _oBMSDbContext.SalaryStructures.Where(x => x.Status == status).ToListAsync();

                return new List<SalaryStructure>(result);

            }

        }

        public async Task<List<SalaryStructure>> GetSalaryListByStatus(string activeStatus)

        {

            var result = await _oBMSDbContext.SalaryStructures.Where(x => x.Status == activeStatus).ToListAsync();

            return new List<SalaryStructure>(result);

            

        }

        public async Task<SalaryStructure> saveAndUpdateSalaryMaster(SalaryStructure salaryStructure)

        {

            var existingSalaryStructure = _oBMSDbContext.SalaryStructures

                .FirstOrDefault(s => s.SalaryId == salaryStructure.SalaryId);



            if (existingSalaryStructure != null)

            {

                // Update the properties of the existing entity with the new values

                existingSalaryStructure.BranchCode = salaryStructure.BranchCode;

                existingSalaryStructure.EmployeeType = salaryStructure.EmployeeType;

                existingSalaryStructure.EmployeeNationality = salaryStructure.EmployeeNationality;

                existingSalaryStructure.Name = salaryStructure.Name;

                existingSalaryStructure.GeneralDayRate = salaryStructure.GeneralDayRate;

                existingSalaryStructure.GeneralDayHours = salaryStructure.GeneralDayHours;

                existingSalaryStructure.GeneralDayOTRate = salaryStructure.GeneralDayOTRate;

                existingSalaryStructure.OffDayRate = salaryStructure.OffDayRate;

                existingSalaryStructure.OffDayOTRate = salaryStructure.OffDayOTRate;

                existingSalaryStructure.HolidayRate = salaryStructure.HolidayRate;

                existingSalaryStructure.HolidayOTRate = salaryStructure.HolidayOTRate;

                existingSalaryStructure.WorkingDays = salaryStructure.WorkingDays;

                existingSalaryStructure.WorkingHours = salaryStructure.WorkingHours;

                existingSalaryStructure.SalaryBand = salaryStructure.SalaryBand;

                existingSalaryStructure.TravelAllowance = salaryStructure.TravelAllowance;

                existingSalaryStructure.Status = salaryStructure.Status;

                existingSalaryStructure.EICC = salaryStructure.EICC;

                existingSalaryStructure.NonStructure = salaryStructure.NonStructure;

                existingSalaryStructure.Active = salaryStructure.Active;

                existingSalaryStructure.LastUpdatedDate = DateTime.Now;

                existingSalaryStructure.LastUpdatedBy = salaryStructure.LastUpdatedBy;



                // Update the changes to the database

                _oBMSDbContext.Update(existingSalaryStructure);

                await _oBMSDbContext.SaveChangesAsync();

                return existingSalaryStructure;



            }

            else

            {

                // Save the changes to the database

                _oBMSDbContext.SalaryStructures.Add(salaryStructure);

                _oBMSDbContext.SaveChanges();

                return salaryStructure;

            }

            throw new NotImplementedException();

        }

        public async Task<ActionResult<SalaryStructure>> DeleteSalaryMasterById(int salaryId)

        {

            var existingSalary = _oBMSDbContext.SalaryStructures.Where(m => m.SalaryId == salaryId).SingleOrDefault();

            if (existingSalary != null)

            {

                _oBMSDbContext.SalaryStructures.Remove(existingSalary);

                await _oBMSDbContext.SaveChangesAsync();

            }

            return new ActionResult<SalaryStructure>(existingSalary);

        }

        #endregion



        #region SOCSO Module Region

        public async Task<List<SOCSO>> GetSOCSOMasterList(int socsoId)

        {

            if (socsoId > 0)

            {

                var result = await _oBMSDbContext.SOCSOs.Where(x => x.socso_id == socsoId).ToListAsync();

                return new List<SOCSO>(result);

            }

            else

            {

                var result = await _oBMSDbContext.SOCSOs.ToListAsync();

                return new List<SOCSO>(result);

            }

        }

        public async Task<SOCSO> saveAndUpdateSOCSOMaster(SOCSO socsoMaster)

        {

            // Find the SOCSO record you want to update



            var socsoToUpdate = _oBMSDbContext.SOCSOs

               .FirstOrDefault(s => s.socso_id == socsoMaster.socso_id);



            if (socsoToUpdate != null)

            {

                // Update the properties

                socsoToUpdate.socso_id = socsoMaster.socso_id;

                socsoToUpdate.socso_from = socsoMaster.socso_from;

                socsoToUpdate.socso_to = socsoMaster.socso_to;

                socsoToUpdate.socso_employer = socsoMaster.socso_employer;

                socsoToUpdate.socso_worker = socsoMaster.socso_worker;

                socsoToUpdate.socso_50year = socsoMaster.socso_50year;

                socsoToUpdate.socso_foreigner = socsoMaster.socso_foreigner;

                socsoToUpdate.LASTUPDATE = DateTime.Now;

                socsoToUpdate.LastUpdatedBy = socsoMaster.LastUpdatedBy;



                // Save the changes to the database

                _oBMSDbContext.Update(socsoToUpdate);

                await _oBMSDbContext.SaveChangesAsync();

                return socsoToUpdate;

            }

            // Handle the case where the record with the given socsoId is not found

            else

            {

                _oBMSDbContext.SOCSOs.Add(socsoMaster);

                _oBMSDbContext.SaveChanges();

                return socsoMaster;

            }

        }

        public async Task<ActionResult<SOCSO>> DeleteSOCSOMasterById(int socsoId)

        {

            var existingSosco = _oBMSDbContext.SOCSOs.Where(m => m.socso_id == socsoId).SingleOrDefault();

            if (existingSosco != null)

            {

                _oBMSDbContext.SOCSOs.Remove(existingSosco);

                await _oBMSDbContext.SaveChangesAsync();

            }

            return new ActionResult<SOCSO>(existingSosco);

        }

        #endregion

        public async Task<List<IncomeTax>> GetIncomeTaxMasterList(int Id)

        {

            if (Id > 0)

            {

                var result = await _oBMSDbContext.IncomeTaxs.Where(x => x.IT_ID == Id).ToListAsync();

                return new List<IncomeTax>(result);

            }

            else

            {

                var result = await _oBMSDbContext.IncomeTaxs.ToListAsync();

                return new List<IncomeTax>(result);

            }

        }



        public async Task<IncomeTax> saveAndUpdateIncomeTaxMaster(IncomeTax incomeTax)

        {

            var incomeToUpdate = _oBMSDbContext.IncomeTaxs

              .FirstOrDefault(s => s.IT_ID == incomeTax.IT_ID);



            if (incomeToUpdate != null)

            {

                // Update the properties with the new values

                incomeToUpdate.IT_SAL_FROM = incomeTax.IT_SAL_FROM;

                incomeToUpdate.IT_SAL_TO = incomeTax.IT_SAL_TO;

                incomeToUpdate.IT_CATEGORY1 = incomeTax.IT_CATEGORY1;

                incomeToUpdate.K_2 = incomeTax.K_2;

                incomeToUpdate.KA1_2 = incomeTax.KA1_2;

                incomeToUpdate.KA2_2 = incomeTax.KA2_2;

                incomeToUpdate.KA3_2 = incomeTax.KA3_2;

                incomeToUpdate.KA4_2 = incomeTax.KA4_2;

                incomeToUpdate.KA5_2 = incomeTax.KA5_2;

                incomeToUpdate.KA6_2 = incomeTax.KA6_2;

                incomeToUpdate.KA7_2 = incomeTax.KA7_2;

                incomeToUpdate.KA8_2 = incomeTax.KA8_2;

                incomeToUpdate.KA9_2 = incomeTax.KA9_2;

                incomeToUpdate.KA10_2 = incomeTax.KA10_2;

                incomeToUpdate.K_3 = incomeTax.K_3;

                incomeToUpdate.KA1_3 = incomeTax.KA1_3;

                incomeToUpdate.KA2_3 = incomeTax.KA2_3;

                incomeToUpdate.KA3_3 = incomeTax.KA3_3;

                incomeToUpdate.KA4_3 = incomeTax.KA4_3;

                incomeToUpdate.KA5_3 = incomeTax.KA5_3;

                incomeToUpdate.KA6_3 = incomeTax.KA6_3;

                incomeToUpdate.KA7_3 = incomeTax.KA7_3;

                incomeToUpdate.KA8_3 = incomeTax.KA8_3;

                incomeToUpdate.KA9_3 = incomeTax.KA9_3;

                incomeToUpdate.KA10_3 = incomeTax.KA10_3;

                incomeToUpdate.LASTUPDATE = incomeTax.LASTUPDATE;

                incomeToUpdate.LastUpdatedBy = incomeTax.LastUpdatedBy;



                // Save the changes to the database

                _oBMSDbContext.Update(incomeToUpdate);

                await _oBMSDbContext.SaveChangesAsync();

                return incomeToUpdate;

            }

            else

            {

                _oBMSDbContext.IncomeTaxs.Add(incomeTax);

                _oBMSDbContext.SaveChanges();

                return incomeTax;

            }

        }



        public async Task<ActionResult<IncomeTax>> DeleteIncomeTaxMasterById(int Id)

        {

            var existingIncomeTax = _oBMSDbContext.IncomeTaxs.Where(m => m.IT_ID == Id).SingleOrDefault();

            if (existingIncomeTax != null)

            {

                _oBMSDbContext.IncomeTaxs.Remove(existingIncomeTax);

                await _oBMSDbContext.SaveChangesAsync();

            }

            return new ActionResult<IncomeTax>(existingIncomeTax);

        }



        public string GetBranchMasterCode()

        {

            var result = _oBMSDbContext.BranchMasters

                .AsEnumerable()

                .Select(j => j.ID)

                .DefaultIfEmpty(0) // Handle the case where there are no valid integer codes

                .Max();

            var newClientCode = result + 1;



            return newClientCode.ToString("FWG000");

        }



        #region Master Reports

        public async Task<List<KKDNExcelListView>> GetListWithBlankRowAsync(string branch, string kdnVetting)

        {

            var query = _oBMSDbContext.KKDNExcelListViews.AsQueryable();



            query = query.Where(x => x.HasTransfer == false);



            if (!string.IsNullOrEmpty(kdnVetting))

            {

                bool bKDNVetting = kdnVetting.Equals("YES", StringComparison.OrdinalIgnoreCase);

                query = query.Where(x => x.KDNVetting == bKDNVetting);

            }



            if (!string.IsNullOrEmpty(branch) && branch != "0")

            {

                query = query.Where(x => x.BranchCode == branch);

            }



            query = query.OrderBy(x => x.Name);



            return await query.ToListAsync();

        }

        #endregion



        string ReplaceAfterHyphen(string code, string newText)

        {

            if (code.Contains("-"))

            {

                // Find the index of the hyphen

                int hyphenIndex = code.IndexOf('-');

                // Return the part before the hyphen and concatenate the new text

                return code.Substring(0, hyphenIndex + 1) + newText;

            }

            else

            {

                // If no hyphen is found, return the original code

                return code;

            }

        }

    }

}

