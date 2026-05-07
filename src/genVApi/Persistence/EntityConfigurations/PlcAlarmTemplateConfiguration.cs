using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class PlcAlarmTemplateConfiguration : IEntityTypeConfiguration<PlcAlarmTemplate>
{
    public void Configure(EntityTypeBuilder<PlcAlarmTemplate> builder)
    {
        builder.ToTable("PlcAlarmTemplates").HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SmsTemplate).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.EmailSubjectTemplate).HasMaxLength(200);
        builder.Property(x => x.EmailBodyTemplate).HasMaxLength(4000);
        builder.Property(x => x.DevicePrefix).HasMaxLength(32);
    }
}
