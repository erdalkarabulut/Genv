using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class PlcTelemetryReadingConfiguration : IEntityTypeConfiguration<PlcTelemetryReading>
{
    public void Configure(EntityTypeBuilder<PlcTelemetryReading> builder)
    {
        builder.ToTable("PlcTelemetryReadings").HasKey(x => x.Id);

        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.ReadAtUtc).IsRequired();

        builder.HasIndex(x => new { x.SensorPointId, x.ReadAtUtc });
    }
}
