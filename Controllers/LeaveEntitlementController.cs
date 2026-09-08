using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;

namespace OBMS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveEntitlementController : ControllerBase
    {
        private readonly OBMSDbContext _oBMSDbContext;

        public LeaveEntitlementController(OBMSDbContext oBMSDbContext)
        {
            _oBMSDbContext = oBMSDbContext;
        }

        /// <summary>
        /// Returns the single LeaveEntitlement configuration row (LE_ID = 1).
        /// If no row exists yet, returns the system defaults without saving.
        /// </summary>
        [HttpGet("Get")]
        public async Task<ActionResult<LeaveEntitlementDto>> Get()
        {
            var record = await _oBMSDbContext.LeaveEntitlements
                .OrderBy(x => x.LE_ID)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                // LeaveSystem slab values (Malaysia flow) are the source of truth.
                return Ok(new LeaveEntitlementDto());
            }

            return Ok(ToDto(record));
        }

        /// <summary>
        /// Creates or updates the LeaveEntitlement configuration row.
        /// Only a single row is maintained (upsert on LE_ID = 1).
        /// </summary>
        [HttpPost("Save")]
        public async Task<ActionResult<LeaveEntitlementDto>> Save([FromBody] LeaveEntitlementDto dto)
        {
            if (dto == null)
                return BadRequest("Payload is required.");

            var existing = await _oBMSDbContext.LeaveEntitlements
                .OrderBy(x => x.LE_ID)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                existing = new LeaveEntitlement
                {
                    AnnualLeave          = dto.AnnualLeave,
                    MedicalLeave         = dto.MedicalLeave,
                    MaternityLeave       = dto.MaternityLeave,
                    PaternityLeave       = dto.PaternityLeave,
                    HospitalizationLeave = dto.HospitalizationLeave,
                    LastUpdate           = DateTime.Now,
                    LastUpdatedBy        = dto.LastUpdatedBy
                };
                _oBMSDbContext.LeaveEntitlements.Add(existing);
            }
            else
            {
                existing.AnnualLeave          = dto.AnnualLeave;
                existing.MedicalLeave         = dto.MedicalLeave;
                existing.MaternityLeave       = dto.MaternityLeave;
                existing.PaternityLeave       = dto.PaternityLeave;
                existing.HospitalizationLeave = dto.HospitalizationLeave;
                existing.LastUpdate           = DateTime.Now;
                existing.LastUpdatedBy        = dto.LastUpdatedBy;
            }

            await _oBMSDbContext.SaveChangesAsync();

            var saved = await _oBMSDbContext.LeaveEntitlements
                .OrderBy(x => x.LE_ID)
                .FirstOrDefaultAsync();

            return Ok(ToDto(saved!));
        }

        private static LeaveEntitlementDto ToDto(LeaveEntitlement record) => new LeaveEntitlementDto
        {
            LE_ID                = record.LE_ID,
            AnnualLeave          = record.AnnualLeave,
            MedicalLeave         = record.MedicalLeave,
            MaternityLeave       = record.MaternityLeave,
            PaternityLeave       = record.PaternityLeave,
            HospitalizationLeave = record.HospitalizationLeave,
            LastUpdate           = record.LastUpdate,
            LastUpdatedBy        = record.LastUpdatedBy
        };
    }
}
