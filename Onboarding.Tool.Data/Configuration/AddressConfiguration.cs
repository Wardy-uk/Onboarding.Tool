using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data.Configuration;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityColumn();
        builder.Property(a => a.Address1).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Address2).HasMaxLength(200);
        builder.Property(a => a.Address3).HasMaxLength(200);
        builder.Property(a => a.Town).HasMaxLength(200);
        builder.Property(a => a.PostCode1).IsRequired().HasMaxLength(10);
        builder.Property(a => a.PostCode2).IsRequired().HasMaxLength(10);
    }
}
