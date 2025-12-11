using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data.Configuration;

public class PortalAccountConfiguration : IEntityTypeConfiguration<PortalAccount>
{
    public void Configure(EntityTypeBuilder<PortalAccount> builder)
    {
        builder.HasKey(pa => pa.Id);
        builder.Property(pa => pa.Id).UseIdentityColumn();
        builder.Property(pa => pa.PortalName).IsRequired().HasMaxLength(100);

        builder.HasOne(pa => pa.Instance)
            .WithMany(i => i.PortalAccounts)
            .HasForeignKey(pa => pa.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pa => new { pa.PortalName, pa.InstanceId })
            .IsUnique();
    }
}
