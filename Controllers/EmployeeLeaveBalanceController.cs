using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.DTO;

namespace OBMS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeLeaveBalanceController : ControllerBase
    {
        private readonly OBMSDbContext _oBMSDbContext;

        public EmployeeLeaveBalanceController(OBMSDbContext oBMSDbContext)
        {
            _oBMSDbContext = oBMSDbContext;
        }

        [HttpGet("GetLeaveBalance")]
        public async Task<ActionResult<EmployeeLeaveBalanceDto?>> GetLeaveBalance(int employeeID, DateTime period)
        {
            var record = await _oBMSDbContext.EmployeeLeaveBalances
                .Where(x => x.EmployeeID == employeeID && x.Period.Year == period.Year && x.Period.Month == period.Month)
                .FirstOrDefaultAsync();

            if (record == null)
                return Ok((EmployeeLeaveBalanceDto?)null);

            return Ok(ToDto(record));
        }

        [HttpPost("SaveLeaveBalance")]
        public async Task<ActionResult<EmployeeLeaveBalanceDto>> SaveLeaveBalance([FromBody] EmployeeLeaveBalanceDto dto)
        {
            if (dto == null || dto.EmployeeID <= 0)
                return BadRequest();

            var existing = await _oBMSDbContext.EmployeeLeaveBalances
                .Where(x => x.EmployeeID == dto.EmployeeID && x.Period.Year == dto.Period.Year && x.Period.Month == dto.Period.Month)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                existing = new Models.Domain.EmployeeLeaveBalance
                {
                    EmployeeID = dto.EmployeeID,
                    Period = new DateTime(dto.Period.Year, dto.Period.Month, 1),
                    AnnualLeaveTaken = dto.AnnualLeaveTaken,
                    AnnualLeaveAvailable = dto.AnnualLeaveAvailable,
                    MedicalLeaveTaken = dto.MedicalLeaveTaken,
                    MedicalLeaveAvailable = dto.MedicalLeaveAvailable,
                    MaternityLeaveTaken = dto.MaternityLeaveTaken,
                    MaternityLeaveAvailable = dto.MaternityLeaveAvailable,
                    PaternityLeaveTaken = dto.PaternityLeaveTaken,
                    PaternityLeaveAvailable = dto.PaternityLeaveAvailable,
                    HospitalizationLeaveTaken = dto.HospitalizationLeaveTaken,
                    HospitalizationLeaveAvailable = dto.HospitalizationLeaveAvailable,
                    LastUpdate = DateTime.Now,
                    LastUpdatedBy = dto.LastUpdatedBy
                };
                _oBMSDbContext.EmployeeLeaveBalances.Add(existing);
            }
            else
            {
                existing.AnnualLeaveTaken = dto.AnnualLeaveTaken;
                existing.AnnualLeaveAvailable = dto.AnnualLeaveAvailable;
                existing.MedicalLeaveTaken = dto.MedicalLeaveTaken;
                existing.MedicalLeaveAvailable = dto.MedicalLeaveAvailable;
                existing.MaternityLeaveTaken = dto.MaternityLeaveTaken;
                existing.MaternityLeaveAvailable = dto.MaternityLeaveAvailable;
                existing.PaternityLeaveTaken = dto.PaternityLeaveTaken;
                existing.PaternityLeaveAvailable = dto.PaternityLeaveAvailable;
                existing.HospitalizationLeaveTaken = dto.HospitalizationLeaveTaken;
                existing.HospitalizationLeaveAvailable = dto.HospitalizationLeaveAvailable;
                existing.LastUpdate = DateTime.Now;
                existing.LastUpdatedBy = dto.LastUpdatedBy;
            }

            await _oBMSDbContext.SaveChangesAsync();

            var saved = await _oBMSDbContext.EmployeeLeaveBalances
                .Where(x => x.EmployeeID == dto.EmployeeID && x.Period.Year == dto.Period.Year && x.Period.Month == dto.Period.Month)
                .FirstOrDefaultAsync();

            return Ok(ToDto(saved!));
        }

        private EmployeeLeaveBalanceDto ToDto(Models.Domain.EmployeeLeaveBalance record)
        {
            return new EmployeeLeaveBalanceDto
            {
                ID = record.ID,
                EmployeeID = record.EmployeeID,
                Period = record.Period,
                AnnualLeaveTaken = record.AnnualLeaveTaken,
                AnnualLeaveAvailable = record.AnnualLeaveAvailable,
                MedicalLeaveTaken = record.MedicalLeaveTaken,
                MedicalLeaveAvailable = record.MedicalLeaveAvailable,
                MaternityLeaveTaken = record.MaternityLeaveTaken,
                MaternityLeaveAvailable = record.MaternityLeaveAvailable,
                PaternityLeaveTaken = record.PaternityLeaveTaken,
                PaternityLeaveAvailable = record.PaternityLeaveAvailable,
                HospitalizationLeaveTaken = record.HospitalizationLeaveTaken,
                HospitalizationLeaveAvailable = record.HospitalizationLeaveAvailable,
                LastUpdate = record.LastUpdate,
                LastUpdatedBy = record.LastUpdatedBy
            };
        }
    }
}
