using System.ComponentModel.DataAnnotations;

namespace MaerskApi.Models
{
    public class CurrencyRate
    {
        public int Id { get; set; }
        public CurrencyType CurrencyRateSource { get; set; }
        public CurrencyType CurrencyRateTarget { get; set; }
        public decimal CurrencyRateSourcePrice { get; set; }
        public decimal CurrencyRateTargetPrice { get; set; }

    }
}
