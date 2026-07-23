using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;

namespace OBMS.WebAPI.Services
{
    public class BankService : IBankService
    {
        private readonly OBMSDbContext _context;

        public BankService(OBMSDbContext context)
        {
            _context = context;
        }

        public async Task<List<IndianBanks>> GetActiveBanksAsync()
        {
            return await _context.Set<IndianBanks>()
                .Where(b => b.IsActive)
                .OrderBy(b => b.BankName)
                .ToListAsync();
        }

        public async Task<IndianBanks?> GetBankByCodeAsync(string bankCode)
        {
            return await _context.Set<IndianBanks>()
                .FirstOrDefaultAsync(b => b.BankCode == bankCode && b.IsActive);
        }

        public async Task<IndianBanks?> GetBankByIFSCAsync(string ifscCode)
        {
            return await _context.Set<IndianBanks>()
                .FirstOrDefaultAsync(b => b.IFSCCode == ifscCode && b.IsActive);
        }

        public async Task<bool> ValidateIFSCAsync(string ifscCode)
        {
            var bank = await GetBankByIFSCAsync(ifscCode);
            return bank != null;
        }

        public async Task<List<string>> GetBankNamesAsync()
        {
            return await _context.Set<IndianBanks>()
                .Where(b => b.IsActive)
                .Select(b => b.BankName)
                .OrderBy(name => name)
                .ToListAsync();
        }
    }
}
