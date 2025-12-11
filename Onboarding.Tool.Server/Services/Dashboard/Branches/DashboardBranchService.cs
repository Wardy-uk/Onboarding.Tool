using Microsoft.EntityFrameworkCore;
using Onboarding.Tool.Data;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Model.Dashboard.Branches;
using InstanceModel = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Services.Dashboard.Branches;

public class DashboardBranchService : IDashboardBranchService
{
    private readonly AppDbContext _context;
    private readonly IOnboardingDataService _onboardingDataService;

    public DashboardBranchService(AppDbContext context, IOnboardingDataService onboardingDataService)
    {
        _context = context;
        _onboardingDataService = onboardingDataService;
    }

    public async Task<Branch?> CreateBranchAsync(int instanceId, ImportBranch createdBranch)
    {
        if (createdBranch.IsDefault)
        {
            await _context.Branches
                .Where(b => b.InstanceId == instanceId && b.IsDefault)
                .ExecuteUpdateAsync(b => b.SetProperty(branch => branch.IsDefault, false));
        }

        Branch branch = new()
        {
            IsDefault = createdBranch.IsDefault,
            Name = createdBranch.Name,
            SalesEmail = createdBranch.SalesEmail,
            SalesPhone = createdBranch.SalesPhone,
            LettingsEmail = createdBranch.LettingsEmail,
            LettingsPhone = createdBranch.LettingsPhone,
            Address = new()
            {
                Address1 = createdBranch.Address1,
                Address2 = createdBranch.Address2,
                Address3 = createdBranch.Address3,
                Town = createdBranch.Town,
                PostCode1 = createdBranch.PostCode1,
                PostCode2 = createdBranch.PostCode2
            },
            InstanceId = instanceId
        };

        _context.Set<Branch>().Add(branch);
        await _context.SaveChangesAsync();

        return branch;
    }

    public async Task<Branch?> UpdateBranchAsync(SaveBranch branch, InstanceModel instance)
    {
        Branch? existingBranch = await GetBranchAsync(branch.Id, instance.Id);

        if (existingBranch == null)
            return null;

        await _onboardingDataService.UpdateBranchDefaultStatusAsync(branch, instance);
        await UpdateBranchProperties(existingBranch, branch);

        return existingBranch;
    }

    public async Task<bool> DeleteBranchAsync(int branchId, int instanceId)
    {
        Branch? branch = await GetBranchAsync(branchId, instanceId);

        if (branch == null)
            return false;

        _context.Branches.Remove(branch);
        await _context.SaveChangesAsync();
            
        return true;
    }

    public async Task<List<Branch>> ImportBranchesAsync(int instanceId, List<ImportBranch> importBranches)
    {
        List<Branch> existingBranches = await _context.Branches
                            .Include(b => b.Address)
                            .Where(b => b.InstanceId == instanceId)
                            .ToListAsync();

        List<Branch> updatedBranches = new();

        foreach (ImportBranch importBranch in importBranches)
        {
            Branch? existingBranch = existingBranches.FirstOrDefault(b =>
                string.Equals(b.Name, importBranch.Name, StringComparison.OrdinalIgnoreCase));

            if (existingBranch != null)
            {
                UpdateBranchFromImport(existingBranch, importBranch);
                updatedBranches.Add(existingBranch);
            }
            else
            {
                var newBranch = CreateBranchFromImport(importBranch, instanceId);
                _context.Branches.Add(newBranch);
                updatedBranches.Add(newBranch);
            }
        }

        await _context.SaveChangesAsync();

        return updatedBranches;
    }

    private static Branch CreateNewBranch(int instanceId) => new()
    {
        Name = "New Branch",
        IsDefault = false,
        SalesEmail = string.Empty,
        SalesPhone = string.Empty,
        LettingsEmail = string.Empty,
        LettingsPhone = string.Empty,
        InstanceId = instanceId
    };

    private async Task<Branch?> GetBranchAsync(int branchId, int instanceId)
    {
        return await _context.Branches
            .Include(b => b.Address)
            .FirstOrDefaultAsync(b => b.Id == branchId && b.InstanceId == instanceId);
    }

    private async Task UpdateBranchProperties(Branch branch, SaveBranch saveBranch)
    {
        branch.IsDefault = saveBranch.IsDefault;
        branch.Name = saveBranch.Name;
        branch.SalesEmail = saveBranch.SalesEmail;
        branch.SalesPhone = saveBranch.SalesPhone;
        branch.LettingsEmail = saveBranch.LettingsEmail;
        branch.LettingsPhone = saveBranch.LettingsPhone;

        if (saveBranch.Address != null)
        {
            if (branch.Address != null)
            {
                UpdateAddressProperties(branch.Address, saveBranch.Address);
            }
            else
            {
                branch.Address = CreateNewAddress(saveBranch.Address);
                _context.Add(branch.Address);
            }
        }
        else if (branch.Address != null)
        {
            _context.Address.Remove(branch.Address);
            branch.Address = null;
        }

        await _context.SaveChangesAsync();
    }

    private static void UpdateAddressProperties(Address address, Address addressModel)
    {
        address.Address1 = addressModel.Address1;
        address.Address2 = addressModel.Address2;
        address.Address3 = addressModel.Address3;
        address.Town = addressModel.Town;
        address.PostCode1 = addressModel.PostCode1;
        address.PostCode2 = addressModel.PostCode2;
    }

    private static Address CreateNewAddress(Address addressModel) => new()
    {
        Address1 = addressModel.Address1,
        Address2 = addressModel.Address2,
        Address3 = addressModel.Address3,
        Town = addressModel.Town,
        PostCode1 = addressModel.PostCode1,
        PostCode2 = addressModel.PostCode2
    };

    private static async Task DeleteBranchAndAddressAsync(AppDbContext context, Branch branch)
    {
        if (branch.Address != null)
            context.Address.Remove(branch.Address);

        context.Branches.Remove(branch);
        await context.SaveChangesAsync();
    }

    private static Branch CreateBranchFromImport(ImportBranch importBranch, int instanceId)
    {
        return new Branch
        {
            IsDefault = importBranch.IsDefault,
            Name = importBranch.Name,
            SalesEmail = importBranch.SalesEmail,
            SalesPhone = importBranch.SalesPhone,
            LettingsEmail = importBranch.LettingsEmail,
            LettingsPhone = importBranch.LettingsPhone,
            Address = new Address
            {
                Address1 = importBranch.Address1,
                Address2 = importBranch.Address2,
                Address3 = importBranch.Address3,
                Town = importBranch.Town,
                PostCode1 = importBranch.PostCode1,
                PostCode2 = importBranch.PostCode2
            },
            InstanceId = instanceId
        };
    }

    private static void UpdateBranchFromImport(Branch existingBranch, ImportBranch importBranch)
    {
        existingBranch.SalesEmail = importBranch.SalesEmail;
        existingBranch.SalesPhone = importBranch.SalesPhone;
        existingBranch.LettingsEmail = importBranch.LettingsEmail;
        existingBranch.LettingsPhone = importBranch.LettingsPhone;

        if (existingBranch.Address == null)
        {
            existingBranch.Address = new Address()
            {
                Address1 = importBranch.Address1,
                Address2 = importBranch.Address2,
                Address3 = importBranch.Address3,
                Town = importBranch.Town,
                PostCode1 = importBranch.PostCode1,
                PostCode2 = importBranch.PostCode2
            };
        }
        else
        {
            existingBranch.Address.Address1 = importBranch.Address1;
            existingBranch.Address.Address2 = importBranch.Address2;
            existingBranch.Address.Address3 = importBranch.Address3;
            existingBranch.Address.Town = importBranch.Town;
            existingBranch.Address.PostCode1 = importBranch.PostCode1;
            existingBranch.Address.PostCode2 = importBranch.PostCode2;
        }
    }
}
