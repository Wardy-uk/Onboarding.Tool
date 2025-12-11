using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Data.Configuration;

public class InstanceConfiguration : IEntityTypeConfiguration<Instance>
{
    public void Configure(EntityTypeBuilder<Instance> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.TemplatesConfirmed)
            .HasConversion<int>()
            .HasDefaultValue(TemplateConfirmationState.NotCreated)
            .IsRequired();
        builder.Property(i => i.DirectMailConfirmed)
            .HasConversion<int>()
            .HasDefaultValue(TemplateConfirmationState.NotCreated)
            .IsRequired();
        builder.Property(i => i.LetterheadConfirmed)
            .HasConversion<int>()
            .HasDefaultValue(TemplateConfirmationState.NotCreated)
            .IsRequired();

        builder.HasMany(i => i.Branches)
            .WithOne(b => b.Instance)
            .HasForeignKey(b => b.InstanceId);
    }
}
