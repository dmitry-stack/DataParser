using Microsoft.EntityFrameworkCore;

namespace ProcessingApp.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Record> Records { get; set; } = null!;
}
