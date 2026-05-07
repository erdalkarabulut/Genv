using System.Reflection;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence.Contexts;

public class BaseDbContext : DbContext
{
    protected IConfiguration Configuration { get; set; }
    public DbSet<EmailAuthenticator> EmailAuthenticators { get; set; }
    public DbSet<OperationClaim> OperationClaims { get; set; }
    public DbSet<OtpAuthenticator> OtpAuthenticators { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserOperationClaim> UserOperationClaims { get; set; }
    public DbSet<Bag> Bags { get; set; }
    public DbSet<BagCell> BagCells { get; set; }
    public DbSet<BagMovement> BagMovements { get; set; }
    public DbSet<Box> Boxes { get; set; }
    public DbSet<CollectionSession> CollectionSessions { get; set; }
    public DbSet<DliProduct> DliProducts { get; set; }
    public DbSet<Donor> Donors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Rack> Racks { get; set; }
    /// <summary>Raf üzeri slotlar (entity: <see cref="Slot"/>, tablo: RackSlots).</summary>
    public DbSet<Slot> RackSlots { get; set; }
    public DbSet<Tank> Tanks { get; set; }
    public DbSet<PlcSensorPoint> PlcSensorPoints { get; set; }
    public DbSet<PlcTelemetryReading> PlcTelemetryReadings { get; set; }
    public DbSet<PlcAlarmContact> PlcAlarmContacts { get; set; }
    public DbSet<PlcAlarmTemplate> PlcAlarmTemplates { get; set; }
    public DbSet<PlcSystemAlarm> PlcSystemAlarms { get; set; }
    public DbSet<ClinicalSettings> ClinicalSettings { get; set; }

    public BaseDbContext(DbContextOptions dbContextOptions, IConfiguration configuration)
        : base(dbContextOptions)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Design-time constructor for EF Core migrations.
    /// </summary>
    public BaseDbContext(DbContextOptions dbContextOptions, bool designTime)
        : base(dbContextOptions)
    {
        // Configuration won't be used at design time
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
