namespace CurrencyConverterAPI.Domain
{
    public static class CurrencyEnum
    {
        public static readonly HashSet<string> ForbiddenCurrencies = new()
    {
        "TRY", "PLN", "THB", "MXN"
    };
    }
}
