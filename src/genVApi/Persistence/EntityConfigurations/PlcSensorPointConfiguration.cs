using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class PlcSensorPointConfiguration : IEntityTypeConfiguration<PlcSensorPoint>
{
    public void Configure(EntityTypeBuilder<PlcSensorPoint> builder)
    {
        builder.ToTable("PlcSensorPoints").HasKey(x => x.Id);

        builder.Property(x => x.SensorCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DeviceName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DevicePrefix).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DataLabel).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ModbusHost).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ModbusPort).IsRequired();
        builder.Property(x => x.SlaveId).IsRequired();
        builder.Property(x => x.RegisterAddress).IsRequired();
        builder.Property(x => x.RegisterLength).IsRequired();
        builder.Property(x => x.ScaleDivisor).IsRequired();
        builder.Property(x => x.PollIntervalSeconds).IsRequired();

        builder.HasIndex(x => x.SensorCode).IsUnique();

        builder.HasMany(x => x.Readings).WithOne(r => r.SensorPoint).HasForeignKey(r => r.SensorPointId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
