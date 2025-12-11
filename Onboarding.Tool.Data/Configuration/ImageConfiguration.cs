using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data.Configuration;

public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).UseIdentityColumn();
        builder.Property(i => i.Type).IsRequired();
        builder.Property(i => i.Name).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Height).HasDefaultValue(0);
        builder.Property(i => i.Width).HasDefaultValue(0);
        builder.Property(i => i.Data).HasColumnType("varbinary(max)");

        builder.HasOne(i => i.Instance)
            .WithMany(i => i.Images)
            .HasForeignKey(i => i.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => new { i.Type, i.InstanceId })
            .IsUnique();
    }
}
