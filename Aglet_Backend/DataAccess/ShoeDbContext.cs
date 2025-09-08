using Aglet_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Aglet_Backend.DataAccess
{
    public class ShoeDbContext : DbContext
    {
        public ShoeDbContext(DbContextOptions<ShoeDbContext> options) : base(options) { }

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Shoe> Shoes { get; set; }
        public DbSet<StockTransmission> StockTransmissions { get; set; }
        public DbSet<PurchaseRecord> PurchaseRecords { get; set; }
    }
}
