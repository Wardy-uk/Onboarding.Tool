using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Data.Configuration;

public class InstanceSetupStepConfiguration : IEntityTypeConfiguration<InstanceSetupStep>
{
    public void Configure(EntityTypeBuilder<InstanceSetupStep> builder)
    {
        builder.HasKey(iss => iss.Id);
        builder.Property(iss => iss.Id).UseIdentityColumn();
        builder.Property(iss => iss.SetupStep).IsRequired().HasMaxLength(100);
        builder.Property(iss => iss.Status)
            .HasConversion<int>()
            .HasDefaultValue(TemplateConfirmationState.NotCreated)
            .IsRequired();

        builder.HasOne(iss => iss.Instance)
            .WithMany(i => i.SetupSteps)
            .HasForeignKey(u => u.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(iss => new { iss.SetupStep, iss.InstanceId })
            .IsUnique();
    }
}
