using CurrencyConverterAPI.Application.DTOs;
using CurrencyConverterAPI.Application.Interfaces;
using CurrencyConverterAPI.Domain;
using CurrencyConverterAPI.Infrastructure.HelperClasses;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace CurrencyConverterAPI.Infrastructure.Services
{
    public class FrankfurterExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FrankfurterExchangeRateService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly string _baseUrl;
        private readonly IConfiguration _configuration;
       

        public FrankfurterExchangeRateService(HttpClient httpClient, IMemoryCache cache, ILogger<FrankfurterExchangeRateService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _retryPolicy = Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            _circuitBreakerPolicy = Policy.Handle<HttpRequestException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
            _baseUrl = _configuration["Frankfurter:BaseUrl"] ?? throw new ArgumentNullException("Frankfurter:BaseUrl not found");
        }

        public async Task<ExchangeRateDto> GetLatestRatesAsync(string baseCurrency)
        {
            string cacheKey = $"latest-{baseCurrency}";
            if (_cache.TryGetValue(cacheKey, out ExchangeRateDto cached))
                return cached;

            return await _retryPolicy.ExecuteAsync(async () =>
                await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var response = await _httpClient.GetFromJsonAsync<FrankfurterResponse>($"{_baseUrl}/latest?from={baseCurrency}");

                    var result = new ExchangeRateDto
                    {
                        BaseCurrency = response.Base,
                        Date = DateTime.Parse(response.Date),
                        Rates = response.Rates
                    };
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                    return result;
                })
            );
        }

        public async Task<decimal> ConvertCurrencyAsync(ConvertCurrencyRequest request)
        {
            if (CurrencyEnum.ForbiddenCurrencies.Contains(request.FromCurrency) ||
                CurrencyEnum.ForbiddenCurrencies.Contains(request.ToCurrency))
            {
                throw new ArgumentException("Forbidden currency detected.");
            }

            string url = $"{_baseUrl}/latest?amount={request.Amount}&from={request.FromCurrency}&to={request.ToCurrency}";

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var result = await _httpClient.GetFromJsonAsync<FrankfurterResponse>(url);
                    return result.Rates[request.ToCurrency];
                })
            );

            return response;
        }

        public async Task<IEnumerable<ExchangeRateDto>> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate)
        {
            string url = $"{_baseUrl}/{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency}";

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var data = await _httpClient.GetFromJsonAsync<FrankfurterResponseHistory>(url);
                    return data;
                })
            );

            return response.Rates.Select(rateByDate => new ExchangeRateDto
            {
                BaseCurrency = response.Base,
                Date = DateTime.Parse(rateByDate.Key),
                Rates = rateByDate.Value
            });
        }

    }

}
