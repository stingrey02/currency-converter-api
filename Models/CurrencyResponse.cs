using System.Collections.Generic;

namespace CurrencyConverterAPI.Models
{
    public class CurrencyResponse
    {
        public string Base { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;

        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}