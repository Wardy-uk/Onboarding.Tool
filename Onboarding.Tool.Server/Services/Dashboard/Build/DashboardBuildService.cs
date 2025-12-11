using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.Common;
using Onboarding.Tool.Data;
using Onboarding.Tool.Model.Dashboard.BuildConfigs;
using Onboarding.Tool.Model.Data;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Services.Dashboard.Build;

public class DashboardBuildService : IDashboardBuildService
{
    private readonly AppDbContext _context;

    public DashboardBuildService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PortalAccount?> AddPortalAccountAsync(Instance instance, string name)
    {
        PortalAccount? portalAccount = await _context.PortalAccount.Where(pa => pa.InstanceId == instance.Id && pa.PortalName == name).FirstOrDefaultAsync();
        if (portalAccount != null)
            return null;

        portalAccount = new()
        {
            PortalName = name,
            InstanceId = instance.Id
        };

        _context.PortalAccount.Add(portalAccount);
        await _context.SaveChangesAsync();

        return portalAccount;
    }

    public async Task<bool> DeletePortalAccountAsync(Instance instance, int portalId)
    {

        PortalAccount? portalAccount = await _context.PortalAccount.Where(pa => pa.Id == portalId && pa.InstanceId == instance.Id).FirstOrDefaultAsync();
        if (portalAccount == null)
            return false;

        _context.Remove(portalAccount);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<BranchBuildDistrict?> CreateBuildDistrictAsync(Instance instance, DistrictConfig districtConfig)
    {
        Branch? branch = await _context.Branches.Where(b => b.Id == districtConfig.BranchId && b.InstanceId == instance.Id).FirstAsync();
        if (branch == null)
            return null;

        BranchBuildDistrict? existingDistrict = await _context.BranchBuildDistricts
            .Include(bbd => bbd.Branch)
            .Where(bbd => bbd.Branch!.InstanceId == instance.Id && bbd.District == districtConfig.District)
            .FirstOrDefaultAsync();

        if (existingDistrict != null)
            return null;

        BranchBuildDistrict branchBuildDistrict = new()
        {
            District = districtConfig.District,
            AllSectors = districtConfig.AllSectors,
            BranchId = districtConfig.BranchId,
            Sectors = districtConfig.Sectors.Select(sector => new BranchBuildDistrictSector
            {
                Sector = sector
            }).ToList()
        };

        _context.BranchBuildDistricts.Add(branchBuildDistrict);
        await _context.SaveChangesAsync();

        return branchBuildDistrict;
    }

    public async Task<BranchBuildDistrict?> UpdateBuildDistrictAsync(Instance instance, int districtId, DistrictConfig districtConfig)
    {
        BranchBuildDistrict? existingDistrict = await _context.BranchBuildDistricts
                                    .Include(bbd => bbd.Branch)
                                    .Include(bbd => bbd.Sectors)
                                    .Where(bbd => bbd.Branch!.InstanceId == instance.Id && bbd.Id == districtConfig.DistrictId)
                                    .FirstOrDefaultAsync();

        if (existingDistrict == null)
            return null;


        existingDistrict.AllSectors = districtConfig.AllSectors;

        existingDistrict.Sectors ??= new List<BranchBuildDistrictSector>();

        HashSet<string> incomingSectors = new(districtConfig.Sectors);
        HashSet<string> existingSectors = new(existingDistrict.Sectors.Select(s => s.Sector));

        List<string> sectorsToAdd = incomingSectors.Except(existingSectors).ToList();
        foreach (string sector in sectorsToAdd)
        {
            existingDistrict.Sectors.Add(new BranchBuildDistrictSector { Sector = sector, District = existingDistrict });
        }

        List<BranchBuildDistrictSector> sectorsToRemove = existingDistrict.Sectors.Where(s => !incomingSectors.Contains(s.Sector)).ToList();
        foreach (BranchBuildDistrictSector sector in sectorsToRemove)
        {
            existingDistrict.Sectors.Remove(sector);
        }


        await _context.SaveChangesAsync();
        return existingDistrict;
    }

    public async Task<bool> DeleteBuildDistrictAsync(Instance instance, int districtId)
    {
        BranchBuildDistrict? district = await _context.BranchBuildDistricts
            .Include(bbd => bbd.Branch)
            .Where(bbd => bbd.Id == districtId && bbd.Branch!.InstanceId == instance.Id)
            .FirstAsync();

        if (district == null)
            return false;

        _context.BranchBuildDistricts.Remove(district);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<PortalAccount>> ImportPortalAccountsAsync(Instance instance, List<BuildPortalAccountImport> portals)
    {
        IEnumerable<PortalAccount> existingAccounts = await _context.Set<PortalAccount>().Where(i => i.InstanceId == instance.Id).ToListAsync();
        List<PortalAccount> newAccounts = new();

        foreach (BuildPortalAccountImport portal in portals)
        {
            if (existingAccounts.Where(p => p.PortalName == portal.Name).FirstOrDefault() != null)
                continue;

            PortalAccount account = new()
            {
                PortalName = portal.Name,
                InstanceId = instance.Id
            };

            newAccounts.Add(account);
        }

        _context.PortalAccount.AddRange(newAccounts);
        await _context.SaveChangesAsync();

        return newAccounts;
    }
}
