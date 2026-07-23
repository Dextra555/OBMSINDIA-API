using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Repositories.Interface;

namespace OBMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IPayrollRepository _payrollRepository;

        public ClientController(IPayrollRepository payrollRepository)
        {
            _payrollRepository = payrollRepository;
        }

        [HttpGet]
        [Route("GetClientByCode")]
        public async Task<ActionResult<ClientMaster>> GetClientByCode(string clientCode)
        {
            try
            {
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code is required");
                }

                var client = await _payrollRepository.GetClientByCode(clientCode);

                if (client == null)
                {
                    return NotFound($"Client with code {clientCode} not found");
                }

                return Ok(client);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetClientsByBranch")]
        public async Task<ActionResult<List<ClientMaster>>> GetClientsByBranch(string branchCode)
        {
            try
            {
                if (string.IsNullOrEmpty(branchCode))
                {
                    return BadRequest("Branch code is required");
                }

                var clients = await _payrollRepository.GetClientsByBranch(branchCode);

                if (clients == null || !clients.Any())
                {
                    return NotFound($"No clients found for branch {branchCode}");
                }

                return Ok(clients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
