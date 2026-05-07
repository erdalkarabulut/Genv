using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Persistence.Contexts;

namespace Persistence.Contexts;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BaseDbContext>
{
    public BaseDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BaseDbContext>();

        var connectionString = args.Length > 0 ? args[0] : "Host=localhost;Database=genvapi;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        // For design-time, we use a design-time specific constructor if available,
        // or pass null configuration (it won't be used at design time)
        return new BaseDbContext(optionsBuilder.Options, designTime: true);
    }
}