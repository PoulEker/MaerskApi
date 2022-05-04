namespace MaerskApi.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string VoyageCode { get; set; }
        public decimal Price { get; set; }
        public CurrencyType CurrencyCode { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}