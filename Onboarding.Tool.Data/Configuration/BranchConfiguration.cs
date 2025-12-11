using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data.Configuration;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).UseIdentityColumn();
        builder.Property(b => b.IsDefault).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.SalesEmail).HasMaxLength(200);
        builder.Property(b => b.SalesPhone).HasMaxLength(50);
        builder.Property(b => b.LettingsEmail).HasMaxLength(200);
        builder.Property(b => b.LettingsPhone).HasMaxLength(50);

        builder.HasOne(b => b.Address)
            .WithOne()
            .HasForeignKey<Branch>(b => b.AddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(b => b.Settings)
            .WithOne(bs => bs.Branch)
            .HasForeignKey(bs => bs.BranchId);
    }
}
