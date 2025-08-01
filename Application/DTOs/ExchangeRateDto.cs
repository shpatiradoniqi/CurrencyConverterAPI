namespace CurrencyConverterAPI.Application.DTOs
{
    public class ExchangeRateDto
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
