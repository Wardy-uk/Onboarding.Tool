using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data.Configuration;

public class InstanceSettingConfiguration : IEntityTypeConfiguration<InstanceSetting>
{
    public void Configure(EntityTypeBuilder<InstanceSetting> builder)
    {
        builder.HasKey(ins => ins.Id);
        builder.Property(ins => ins.Id).UseIdentityColumn();
        builder.Property(ins => ins.Setting).IsRequired().HasMaxLength(100);
        builder.Property(ins => ins.Value).IsRequired().HasMaxLength(100);

        builder.HasOne(ins => ins.Instance)
            .WithMany(i => i.Settings)
            .HasForeignKey(ins => ins.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ins => new { ins.Setting, ins.Value, ins.InstanceId })
            .IsUnique();
    }
}
