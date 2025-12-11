using Microsoft.EntityFrameworkCore;
using Onboarding.Tool.Data.Configuration;
using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { 
    }

    public DbSet<Instance> Instances => Set<Instance>();
    public DbSet<InstanceSetting> InstanceSettings => Set<InstanceSetting>();
    public DbSet<InstanceSetupStep> InstanceSetupStep => Set<InstanceSetupStep>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<BranchSetting> BranchSetting => Set<BranchSetting>();
    public DbSet<Address> Address => Set<Address>();
    public DbSet<PortalAccount> PortalAccount => Set<PortalAccount>();
    public DbSet<Users> Users => Set<Users>();
    public DbSet<Image> Image => Set<Image>();
    public DbSet<BranchBuildDistrict> BranchBuildDistricts => Set<BranchBuildDistrict>();
    public DbSet<BranchBuildDistrictSector> BranchBuildDistrictSectors => Set<BranchBuildDistrictSector>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InstanceConfiguration());
        modelBuilder.ApplyConfiguration(new BranchConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new BranchSettingConfiguration());
        modelBuilder.ApplyConfiguration(new PortalAccountConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ImageConfiguration());
        modelBuilder.ApplyConfiguration(new BranchBuildDistrictConfiguration());
        modelBuilder.ApplyConfiguration(new BranchBuildDistrictSectorConfiguration());
        modelBuilder.ApplyConfiguration(new InstanceSettingConfiguration());
        modelBuilder.ApplyConfiguration(new InstanceSetupStepConfiguration());
    }
}
