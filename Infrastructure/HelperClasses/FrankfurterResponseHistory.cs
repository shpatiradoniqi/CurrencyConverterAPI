namespace CurrencyConverterAPI.Infrastructure.HelperClasses
{
    public class FrankfurterResponseHistory
    {
        public string Base { get; set; }
        public string Date { get; set; }
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
    }
}
