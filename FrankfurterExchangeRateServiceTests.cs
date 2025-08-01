using CurrencyConverterAPI.Application.DTOs;
using CurrencyConverterAPI.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using Polly.CircuitBreaker;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace CurrencyConverterAPITest
{
    public class FrankfurterExchangeRateServiceTests
    {
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly Mock<ILogger<FrankfurterExchangeRateService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;

        public FrankfurterExchangeRateServiceTests()
        {
            _mockMemoryCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<FrankfurterExchangeRateService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            // Default setup for configuration
            _mockConfiguration.Setup(c => c["Frankfurter:BaseUrl"]).Returns("https://api.frankfurter.app");
        }

        private FrankfurterExchangeRateService CreateService()
        {
            // Service is created in each test to allow for different setups (e.g., config)
            return new FrankfurterExchangeRateService(_httpClient, _mockMemoryCache.Object, _mockLogger.Object, _mockConfiguration.Object);
        }

        private void SetupHttpMock(HttpStatusCode statusCode, string content)
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content),
                });
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenBaseUrlIsMissing()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["Frankfurter:BaseUrl"]).Returns((string)null);

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => CreateService());
            Assert.Contains("Frankfurter:BaseUrl not found", ex.Message);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldReturnCachedResult_WhenDataIsInCache()
        {
            // Arrange
            var baseCurrency = "USD";
            var cacheKey = $"latest-{baseCurrency}";
            var cachedRates = new ExchangeRateDto { BaseCurrency = "USD", Rates = new Dictionary<string, decimal> { { "JPY", 145m } } };

            // Mock IMemoryCache to return a value for our key
            object cachedValue = cachedRates;
            _mockMemoryCache.Setup(m => m.TryGetValue(cacheKey, out cachedValue)).Returns(true);

            var service = CreateService();

            // Act
            var result = await service.GetLatestRatesAsync(baseCurrency);

            // Assert
            Assert.Equal(cachedRates, result);

            // Verify that the HTTP client was NOT called, because data came from cache
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldRetry_OnHttpRequestException()
        {
            // Arrange
            var baseCurrency = "USD";

            // Setup the handler to throw an exception on every call
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Simulated network failure"));

            var service = CreateService();

            // Act & Assert
            // The call should fail after all retries
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetLatestRatesAsync(baseCurrency));

            // Verify that SendAsync was called 4 times (1 initial + 3 retries)
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(4), // As per the policy: WaitAndRetryAsync(3, ...)
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldBreakCircuit_AfterConsecutiveFailures()
        {
            // Arrange
            var baseCurrency = "USD";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Simulated network failure"));

            var service = CreateService();

            // Act & Assert
            // First two calls should fail and trip the circuit breaker
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetLatestRatesAsync(baseCurrency));
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetLatestRatesAsync(baseCurrency));

            // The third call should immediately fail with BrokenCircuitException without an HTTP call
            await Assert.ThrowsAsync<BrokenCircuitException>(() => service.GetLatestRatesAsync(baseCurrency));

            // Verify that SendAsync was called only twice.
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(2), // As per the policy: CircuitBreakerAsync(2, ...)
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldFetchFromApi_AndSetCacheOnSuccess()
        {
            // Arrange
            var baseCurrency = "USD";
            var cacheKey = $"latest-{baseCurrency}";
            var response = new { @base = "USD", date = "2025-08-01", rates = new Dictionary<string, decimal> { { "EUR", 0.91m } } };
            var jsonResponse = JsonSerializer.Serialize(response);
            SetupHttpMock(HttpStatusCode.OK, jsonResponse);

            // Mock TryGetValue to return false, indicating a cache miss
            object cachedValue = null;
            _mockMemoryCache.Setup(m => m.TryGetValue(cacheKey, out cachedValue)).Returns(false);

            // Mock the Set method on the cache
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            var service = CreateService();

            // Act
            var result = await service.GetLatestRatesAsync(baseCurrency);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(baseCurrency, result.BaseCurrency);

            // Verify that the cache's Set method was called
            _mockMemoryCache.Verify(m => m.CreateEntry(cacheKey), Times.Once());
        }
    }
}
