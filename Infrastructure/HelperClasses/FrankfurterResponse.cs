﻿namespace CurrencyConverterAPI.Infrastructure.HelperClasses
{
    public class FrankfurterResponse
    {
        public string Base { get; set; }
        public string Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
