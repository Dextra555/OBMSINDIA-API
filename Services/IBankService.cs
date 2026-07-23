using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface IBankService
    {
        Task<List<IndianBanks>> GetActiveBanksAsync();
        Task<IndianBanks?> GetBankByCodeAsync(string bankCode);
        Task<IndianBanks?> GetBankByIFSCAsync(string ifscCode);
        Task<bool> ValidateIFSCAsync(string ifscCode);
        Task<List<string>> GetBankNamesAsync();
    }
}
