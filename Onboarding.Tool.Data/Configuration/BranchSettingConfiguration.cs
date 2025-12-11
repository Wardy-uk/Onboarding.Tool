using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data.Configuration;

public class BranchSettingConfiguration : IEntityTypeConfiguration<BranchSetting>
{
    public void Configure(EntityTypeBuilder<BranchSetting> builder)
    {
        builder.HasKey(bs => bs.Id);
        builder.Property(bs => bs.Id).UseIdentityColumn();
        builder.Property(bs => bs.Setting).IsRequired().HasMaxLength(100);
        builder.Property(bs => bs.Value).IsRequired().HasMaxLength(100);

        builder.HasOne(bs => bs.Branch)
            .WithMany(b => b.Settings)
            .HasForeignKey(bs => bs.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bs => new { bs.Setting, bs.Value, bs.BranchId })
            .IsUnique();
    }
}
