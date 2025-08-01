namespace CurrencyConverterAPI.Application.DTOs
{
    public class ConvertCurrencyRequest
    {
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
