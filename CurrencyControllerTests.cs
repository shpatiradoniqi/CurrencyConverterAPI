using Moq;
using Microsoft.Extensions.Logging;
using CurrencyConverterAPI.Application.DTOs;
using CurrencyConverterAPI.Application.Interfaces;
using CurrencyConverterAPI.API.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterAPITest
{
    public class CurrencyControllerTests
    {
        private readonly Mock<IExchangeRateService> _mockExchangeRateService;
        private readonly Mock<ILogger<CurrencyController>> _mockLogger;
        private readonly CurrencyController _controller;

        public CurrencyControllerTests()
        {
            _mockExchangeRateService = new Mock<IExchangeRateService>();
            _mockLogger = new Mock<ILogger<CurrencyController>>();
            _controller = new CurrencyController(_mockExchangeRateService.Object, _mockLogger.Object);
        }


        [Fact]
        public async Task GetLatest_ShouldReturnOk_WithValidData()
        {
            // Arrange
            var baseCurrency = "USD";
            var exchangeRateDto = new ExchangeRateDto
            {
                BaseCurrency = "USD",
                Date = DateTime.Now,
                Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
            };
            _mockExchangeRateService.Setup(s => s.GetLatestRatesAsync(baseCurrency)).ReturnsAsync(exchangeRateDto);

            // Act
            var result = await _controller.GetLatest(baseCurrency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ExchangeRateDto>(okResult.Value);
            Assert.Equal(baseCurrency, returnValue.BaseCurrency);
        }

        [Fact]
        public async Task Convert_ShouldReturnOk_WithValidRequest()
        {
            // Arrange
            var request = new ConvertCurrencyRequest { FromCurrency = "USD", ToCurrency = "EUR", Amount = 100 };
            var convertedAmount = 85m;
            _mockExchangeRateService.Setup(s => s.ConvertCurrencyAsync(request)).ReturnsAsync(convertedAmount);

            // Act
            var result = await _controller.Convert(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(convertedAmount, okResult.Value);
        }

        [Fact]
        public async Task Convert_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var request = new ConvertCurrencyRequest { FromCurrency = "USD", ToCurrency = "EUR", Amount = 100 };
            var errorMessage = "Invalid currency";
            _mockExchangeRateService.Setup(s => s.ConvertCurrencyAsync(request)).ThrowsAsync(new ArgumentException(errorMessage));

            // Act
            var result = await _controller.Convert(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task GetHistory_ShouldReturnOk_WithValidData()
        {
            // Arrange
            var baseCurrency = "USD";
            var startDate = DateTime.Now.AddDays(-7);
            var endDate = DateTime.Now;
            var historicalRates = new List<ExchangeRateDto>
            {
                new ExchangeRateDto { BaseCurrency = "USD", Date = DateTime.Now.AddDays(-1), Rates = new Dictionary<string, decimal> { { "EUR", 0.86m } } }
            };
            _mockExchangeRateService.Setup(s => s.GetHistoricalRatesAsync(baseCurrency, startDate, endDate)).ReturnsAsync(historicalRates);

            // Act
            var result = await _controller.GetHistory(baseCurrency, startDate, endDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ExchangeRateDto>>(okResult.Value);
            Assert.NotEmpty(returnValue);
        }
    }
}