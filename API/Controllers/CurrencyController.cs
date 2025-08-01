using CurrencyConverterAPI.Application.DTOs;
using CurrencyConverterAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterAPI.API.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(IExchangeRateService exchangeRateService, ILogger<CurrencyController> logger)
        {
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        [HttpGet("latest/{baseCurrency}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetLatest(string baseCurrency)
        {
            var rates = await _exchangeRateService.GetLatestRatesAsync(baseCurrency);
            return Ok(rates);
        }

        [HttpPost("convert")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Convert([FromBody] ConvertCurrencyRequest request)
        {
            try
            {
                var result = await _exchangeRateService.ConvertCurrencyAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("history")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHistory([FromQuery] string baseCurrency, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var history = await _exchangeRateService.GetHistoricalRatesAsync(baseCurrency, startDate, endDate);
            return Ok(history);
        }
    }
}
