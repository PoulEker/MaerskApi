using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using MaerskApi.Models;

namespace MaerskApi.Models
{
    public class MaerskContext : DbContext
    {
        private Func<object, object> p;

        public MaerskContext(DbContextOptions<MaerskContext> options)
            : base(options) { }

        public MaerskContext(Func<object, object> p)
        {
            this.p = p;
        }

        public DbSet<CurrencyRate> CurrencyRates { get; set; } = null!;
        public DbSet<Models.Booking> Booking { get; set; }
    }

}
