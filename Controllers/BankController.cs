using Microsoft.AspNetCore.Mvc;
using OBMS.WebAPI.Services;

namespace OBMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankController : ControllerBase
    {
        private readonly IBankService _bankService;

        public BankController(IBankService bankService)
        {
            _bankService = bankService;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveBanks()
        {
            try
            {
                var banks = await _bankService.GetActiveBanksAsync();
                return Ok(banks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("names")]
        public async Task<IActionResult> GetBankNames()
        {
            try
            {
                var bankNames = await _bankService.GetBankNamesAsync();
                return Ok(bankNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("by-code/{bankCode}")]
        public async Task<IActionResult> GetBankByCode(string bankCode)
        {
            try
            {
                var bank = await _bankService.GetBankByCodeAsync(bankCode);
                if (bank == null)
                {
                    return NotFound($"Bank with code '{bankCode}' not found.");
                }
                return Ok(bank);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("by-ifsc/{ifscCode}")]
        public async Task<IActionResult> GetBankByIFSC(string ifscCode)
        {
            try
            {
                var bank = await _bankService.GetBankByIFSCAsync(ifscCode);
                if (bank == null)
                {
                    return NotFound($"Bank with IFSC code '{ifscCode}' not found.");
                }
                return Ok(bank);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("validate-ifsc/{ifscCode}")]
        public async Task<IActionResult> ValidateIFSC(string ifscCode)
        {
            try
            {
                var isValid = await _bankService.ValidateIFSCAsync(ifscCode);
                return Ok(new { isValid, ifscCode });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
