using CurrencyConverterAPI.Application.DTOs;

namespace CurrencyConverterAPI.Application.Interfaces
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateDto> GetLatestRatesAsync(string baseCurrency);
        Task<decimal> ConvertCurrencyAsync(ConvertCurrencyRequest request);
        Task<IEnumerable<ExchangeRateDto>> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate);
    }
}
