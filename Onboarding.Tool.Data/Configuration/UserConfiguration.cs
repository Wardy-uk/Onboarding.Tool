using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).UseIdentityColumn();
        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);

        builder.HasOne(u => u.Instance)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => new { u.Username, u.InstanceId })
            .IsUnique();
    }
}
