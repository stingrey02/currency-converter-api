using CurrencyConverterAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace CurrencyConverterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private const string FrankfurterBaseUrl = "https://api.frankfurter.dev/v1";
        private readonly HttpClient _httpClient;

        public CurrencyController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
        }

        // Convert currency
        // GET: api/currency/convert?from=USD&to=PHP&amount=10
        [HttpGet("convert")]
        public async Task<IActionResult> Convert(string from, string to, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || amount <= 0)
                return BadRequest("Please provide valid currencies and an amount greater than zero.");

            string fromCode = from.ToUpperInvariant();
            string toCode = to.ToUpperInvariant();
            string url = $"{FrankfurterBaseUrl}/latest?base={Uri.EscapeDataString(fromCode)}&symbols={Uri.EscapeDataString(toCode)}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<CurrencyResponse>(url);

                if (response == null || !response.Rates.TryGetValue(toCode, out decimal rate))
                    return BadRequest("Invalid currency.");

                decimal result = decimal.Round(amount * rate, 2);

                return Ok(new
                {
                    from = fromCode,
                    to = toCode,
                    amount,
                    rate,
                    result
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, "Currency service is unavailable right now.");
            }
        }

        // Get all rates
        // GET: api/currency/rates?from=USD
        [HttpGet("rates")]
        public async Task<IActionResult> GetRates(string from)
        {
            if (string.IsNullOrWhiteSpace(from))
                return BadRequest("Please provide a base currency.");

            string fromCode = from.ToUpperInvariant();
            string url = $"{FrankfurterBaseUrl}/latest?base={Uri.EscapeDataString(fromCode)}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<CurrencyResponse>(url);

                if (response == null)
                    return BadRequest("Currency service returned an empty response.");

                return Ok(response);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, "Currency service is unavailable right now.");
            }
        }

        // Get supported currencies
        // GET: api/currency/currencies
        [HttpGet("currencies")]
        public async Task<IActionResult> GetCurrencies()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
                    $"{FrankfurterBaseUrl}/currencies");

                if (response == null || response.Count == 0)
                    return BadRequest("Currency list is unavailable.");

                var currencies = response
                    .OrderBy(entry => entry.Value)
                    .Select(entry => new CurrencyOption
                    {
                        Code = entry.Key,
                        Name = entry.Value
                    });

                return Ok(currencies);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, "Currency service is unavailable right now.");
            }
        }
    }
}
