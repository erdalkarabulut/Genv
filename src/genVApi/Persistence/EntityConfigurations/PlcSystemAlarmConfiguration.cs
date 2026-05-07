using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class PlcSystemAlarmConfiguration : IEntityTypeConfiguration<PlcSystemAlarm>
{
    public void Configure(EntityTypeBuilder<PlcSystemAlarm> builder)
    {
        builder.ToTable("PlcSystemAlarms").HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.DevicePrefix).HasMaxLength(32);
        builder.Property(x => x.SensorCode).HasMaxLength(100);
        builder.Property(x => x.Message).HasMaxLength(500).IsRequired();
        builder.Property(x => x.RelatedDeviceAddress).HasMaxLength(200);

        builder.HasIndex(x => new { x.IsResolved, x.OccurredAtUtc });
        builder.HasIndex(x => x.SensorCode);
    }
}