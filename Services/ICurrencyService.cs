namespace OBMS.WebAPI.Services
{
    public interface ICurrencyService
    {
        decimal ConvertMYRToINR(decimal amountInMYR);
        decimal ConvertINRToMYR(decimal amountInINR);
        string FormatINR(decimal amount);
        string FormatMYR(decimal amount);
        decimal GetExchangeRate();
    }
}
