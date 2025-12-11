using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data.Configuration;

public class BranchBuildDistrictSectorConfiguration : IEntityTypeConfiguration<BranchBuildDistrictSector>
{
    public void Configure(EntityTypeBuilder<BranchBuildDistrictSector> builder)
    {
        builder.HasKey(bs => bs.Id);
        builder.Property(bs => bs.Id).UseIdentityColumn();
        builder.Property(bs => bs.Sector).IsRequired().HasMaxLength(100);

        builder.HasOne(bs => bs.District)
            .WithMany(bd => bd.Sectors)
            .HasForeignKey(bs => bs.DistrictId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bs => new { bs.DistrictId, bs.Sector })
            .IsUnique();
    }
}
