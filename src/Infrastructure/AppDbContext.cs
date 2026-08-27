using Microsoft.EntityFrameworkCore;

namespace ProcessingApp.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Domain.Record> Records { get; set; } = null!;

    }
}
