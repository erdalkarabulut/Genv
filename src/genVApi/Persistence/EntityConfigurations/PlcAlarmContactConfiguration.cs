using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class PlcAlarmContactConfiguration : IEntityTypeConfiguration<PlcAlarmContact>
{
    public void Configure(EntityTypeBuilder<PlcAlarmContact> builder)
    {
        builder.ToTable("PlcAlarmContacts").HasKey(x => x.Id);

        builder.Property(x => x.DevicePrefix).HasMaxLength(32);
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256);

        builder.HasQueryFilter(x => !x.DeletedDate.HasValue);
    }
}
