using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data.Configuration;

public class BranchBuildDistrictConfiguration : IEntityTypeConfiguration<BranchBuildDistrict>
{
    public void Configure(EntityTypeBuilder<BranchBuildDistrict> builder)
    {
        builder.HasKey(bd => bd.Id);
        builder.Property(bd => bd.Id).UseIdentityColumn();
        builder.Property(bd => bd.District).IsRequired().HasMaxLength(100);

        builder.HasOne(bd => bd.Branch)
            .WithMany(b => b.Districts)
            .HasForeignKey(bs => bs.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bs => new { bs.BranchId, bs.District })
            .IsUnique();
    }
}
