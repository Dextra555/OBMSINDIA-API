using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;
using OBMS.WebAPI.Repositories.Interface;
using OBMS.WebAPI.Services;

namespace OBMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly OBMSDbContext _db;
        private readonly IAttendancePeriodService _attendancePeriodService;

        public ClientController(
            IPayrollRepository payrollRepository,
            OBMSDbContext db,
            IAttendancePeriodService attendancePeriodService)
        {
            _payrollRepository       = payrollRepository;
            _db                      = db;
            _attendancePeriodService = attendancePeriodService;
        }

        // ── Existing endpoints ─────────────────────────────────────────────

        [HttpGet("GetClientByCode")]
        public async Task<ActionResult<ClientMaster>> GetClientByCode(string clientCode)
        {
            try
            {
                if (string.IsNullOrEmpty(clientCode)) return BadRequest("Client code is required");
                var client = await _payrollRepository.GetClientByCode(clientCode);
                if (client == null) return NotFound($"Client with code {clientCode} not found");
                return Ok(client);
            }
            catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
        }

        [HttpGet("GetClientsByBranch")]
        public async Task<ActionResult<List<ClientMaster>>> GetClientsByBranch(string branchCode)
        {
            try
            {
                if (string.IsNullOrEmpty(branchCode)) return BadRequest("Branch code is required");
                var clients = await _payrollRepository.GetClientsByBranch(branchCode);
                if (clients == null || !clients.Any()) return NotFound($"No clients found for branch {branchCode}");
                return Ok(clients);
            }
            catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
        }

        // ── Attendance Period Config CRUD ──────────────────────────────────

        /// <summary>GET api/client/GetAttendancePeriodConfig?clientCode=</summary>
        [HttpGet("GetAttendancePeriodConfig")]
        public async Task<ActionResult<ClientAttendancePeriodConfigDto>> GetAttendancePeriodConfig(string clientCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientCode)) return BadRequest("clientCode is required");

                var config = await _db.ClientAttendancePeriods
                    .AsNoTracking().FirstOrDefaultAsync(c => c.ClientCode == clientCode);

                if (config == null)
                {
                    config = new ClientAttendancePeriod
                    {
                        ClientCode = clientCode, PeriodStartDay = 1,
                        PeriodEndDay = 0, IsCustomPeriod = false,
                        LastUpdatedDate = DateTime.Now, LastUpdatedBy = "AUTO"
                    };
                    _db.ClientAttendancePeriods.Add(config);
                    await _db.SaveChangesAsync();
                }

                var now    = DateTime.Today;
                var period = await _attendancePeriodService.GetAttendancePeriodAsync(clientCode, now.Year, now.Month);

                return Ok(new ClientAttendancePeriodConfigDto
                {
                    ID             = config.ID,
                    ClientCode     = config.ClientCode,
                    PeriodStartDay = config.PeriodStartDay,
                    PeriodEndDay   = config.PeriodEndDay,
                    IsCustomPeriod = config.IsCustomPeriod,
                    PreviewLabel   = $"{period.StartDate:dd-MMM-yyyy} to {period.EndDate:dd-MMM-yyyy}"
                });
            }
            catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
        }

        /// <summary>POST api/client/SaveAttendancePeriodConfig</summary>
        [HttpPost("SaveAttendancePeriodConfig")]
        public async Task<ActionResult<ClientAttendancePeriodConfigDto>> SaveAttendancePeriodConfig(
            [FromBody] ClientAttendancePeriodConfigDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.ClientCode)) return BadRequest("ClientCode is required");

                if (dto.IsCustomPeriod)
                {
                    if (dto.PeriodStartDay < 1 || dto.PeriodStartDay > 28)
                        return BadRequest("PeriodStartDay must be between 1 and 28");
                    if (dto.PeriodEndDay < 0 || dto.PeriodEndDay > 28)
                        return BadRequest("PeriodEndDay must be 0 (last day) or between 1 and 28");
                }

                var existing = await _db.ClientAttendancePeriods
                    .FirstOrDefaultAsync(c => c.ClientCode == dto.ClientCode);

                if (existing != null)
                {
                    existing.PeriodStartDay = dto.PeriodStartDay;
                    existing.PeriodEndDay   = dto.PeriodEndDay;
                    existing.IsCustomPeriod = dto.IsCustomPeriod;
                    existing.LastUpdatedDate = DateTime.Now;
                    existing.LastUpdatedBy   = dto.LastUpdatedBy ?? "SYSTEM";
                    _db.ClientAttendancePeriods.Update(existing);
                }
                else
                {
                    existing = new ClientAttendancePeriod
                    {
                        ClientCode = dto.ClientCode, PeriodStartDay = dto.PeriodStartDay,
                        PeriodEndDay = dto.PeriodEndDay, IsCustomPeriod = dto.IsCustomPeriod,
                        LastUpdatedDate = DateTime.Now, LastUpdatedBy = dto.LastUpdatedBy ?? "SYSTEM"
                    };
                    _db.ClientAttendancePeriods.Add(existing);
                }

                await _db.SaveChangesAsync();

                var now    = DateTime.Today;
                var period = await _attendancePeriodService.GetAttendancePeriodAsync(dto.ClientCode, now.Year, now.Month);
                dto.ID           = existing.ID;
                dto.PreviewLabel = $"{period.StartDate:dd-MMM-yyyy} to {period.EndDate:dd-MMM-yyyy}";
                return Ok(dto);
            }
            catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
        }

        /// <summary>GET api/client/GetAllAttendancePeriodConfigs</summary>
        [HttpGet("GetAllAttendancePeriodConfigs")]
        public async Task<ActionResult<List<ClientAttendancePeriodConfigDto>>> GetAllAttendancePeriodConfigs()
        {
            try
            {
                var configs = await _db.ClientAttendancePeriods.AsNoTracking()
                    .OrderBy(c => c.ClientCode).ToListAsync();

                var now    = DateTime.Today;
                var result = new List<ClientAttendancePeriodConfigDto>();

                foreach (var c in configs)
                {
                    var p = await _attendancePeriodService.GetAttendancePeriodAsync(c.ClientCode, now.Year, now.Month);
                    result.Add(new ClientAttendancePeriodConfigDto
                    {
                        ID = c.ID, ClientCode = c.ClientCode,
                        PeriodStartDay = c.PeriodStartDay, PeriodEndDay = c.PeriodEndDay,
                        IsCustomPeriod = c.IsCustomPeriod,
                        PreviewLabel = $"{p.StartDate:dd-MMM-yyyy} to {p.EndDate:dd-MMM-yyyy}"
                    });
                }
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
        }
    }
}
