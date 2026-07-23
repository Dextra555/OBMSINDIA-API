namespace OBMS.WebAPI.Services
{
    public class CurrencyService : ICurrencyService
    {
        // Exchange rate: 1 MYR = 18.00 INR (approximate, should be updated regularly)
        private const decimal EXCHANGE_RATE = 18.00m;

        public decimal ConvertMYRToINR(decimal amountInMYR)
        {
            return Math.Round(amountInMYR * EXCHANGE_RATE, 2);
        }

        public decimal ConvertINRToMYR(decimal amountInINR)
        {
            return Math.Round(amountInINR / EXCHANGE_RATE, 2);
        }

        public string FormatINR(decimal amount)
        {
            return $"₹{amount:N2}";
        }

        public string FormatMYR(decimal amount)
        {
            return $"RM{amount:N2}";
        }

        public decimal GetExchangeRate()
        {
            return EXCHANGE_RATE;
        }
    }
}
