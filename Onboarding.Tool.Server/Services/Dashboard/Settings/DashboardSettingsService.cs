using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Onboarding.Tool.Data;
using Onboarding.Tool.Model.Dashboard.Settings;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Model.Settings;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Services.Dashboard.Settings;

public class DashboardSettingsService : IDashboardSettingsService
{
    private readonly AppDbContext _context;

    public DashboardSettingsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<InstanceSetting>?> ApplyInstanceSettingsAsync(Instance instance, Dictionary<string, string> instanceSettings)
    {
        Dictionary<string, string> populatedSettings = instanceSettings
            .Where(s => !string.IsNullOrWhiteSpace(s.Value))
            .ToDictionary(s => s.Key, s => s.Value);

        List<InstanceSetting> currentSettings = await _context.Set<InstanceSetting>()
            .Where(s => s.InstanceId == instance.Id)
            .ToListAsync();

        foreach (var kvp in populatedSettings)
        {
            InstanceSetting? setting = currentSettings.Where(c => c.Setting == kvp.Key).FirstOrDefault();
            if (setting == null)
            {
                InstanceSetting newSetting = new()
                {
                    Setting = kvp.Key,
                    Value = kvp.Value,
                    InstanceId = instance.Id
                };

                _context.InstanceSettings.Add(newSetting);
                currentSettings.Add(newSetting);
            }
            else
            {
                setting.Value = kvp.Value;
            }
        }

        HashSet<string> populatedSettingIds = populatedSettings.Select(s => s.Key).ToHashSet();

        List<InstanceSetting> settingsToDelete = currentSettings
            .Where(s => !populatedSettingIds.Contains(s.Setting) || string.IsNullOrWhiteSpace(s.Value))
            .ToList();

        _context.InstanceSettings.RemoveRange(settingsToDelete);

        await _context.SaveChangesAsync();
        return currentSettings;
    }
    public async Task<List<BranchSetting>?> ApplyBranchSettingsAsync(Instance instance, int branchId, Dictionary<string, string> branchSettings)
    {
        Branch? branch = await _context.Branches
            .Include(b => b.Settings)
            .Where(b => b.Id == branchId && b.InstanceId == instance.Id)
            .FirstOrDefaultAsync();

        if (branch == null)
            return null;

        Dictionary<string, string> populatedSettings = branchSettings
            .Where(s => !string.IsNullOrWhiteSpace(s.Value))
            .ToDictionary(s => s.Key, s => s.Value);

        List<BranchSetting> currentSettings = await _context.Set<BranchSetting>()
            .Where(s => s.BranchId == branch.Id)
            .ToListAsync();

        foreach (var kvp in populatedSettings)
        {
            BranchSetting? setting = currentSettings.Where(c => c.Setting == kvp.Key).FirstOrDefault();

            if (setting == null)
            {
                BranchSetting newSetting = new()
                {
                    Setting = kvp.Key,
                    Value = kvp.Value,
                    BranchId = branch.Id
                };

                _context.BranchSetting.Add(newSetting);
                currentSettings.Add(newSetting);
            }
            else
            {
                setting.Value = kvp.Value;
            }
        }

        HashSet<string> populatedSettingIds = populatedSettings.Select(s => s.Key).ToHashSet();

        List<BranchSetting> settingsToDelete = currentSettings
            .Where(s => !populatedSettingIds.Contains(s.Setting) || string.IsNullOrWhiteSpace(s.Value))
            .ToList();

        _context.BranchSetting.RemoveRange(settingsToDelete);

        await _context.SaveChangesAsync();
        return currentSettings;
    }

    public async Task<bool> ImportSettingsAsync(Instance instance, List<Dictionary<string, object>> importSettings)
    {
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            await _context.InstanceSettings
                .Where(isv => isv.InstanceId == instance.Id)
                .ExecuteDeleteAsync();

            var branchesWithSettings = await _context.Branches
                .Where(b => b.InstanceId == instance.Id)
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();

            List<int> branchIds = branchesWithSettings.Select(b => b.Id).ToList();
            if (branchIds.Count > 0)
            {
                await _context.BranchSetting
                    .Where(bs => branchIds.Contains(bs.BranchId))
                    .ExecuteDeleteAsync();
            }

            List<InstanceSetting> newInstanceSettings = new();
            List<BranchSetting> newBranchSettings = new();

            Dictionary<string, object>? instanceSettings =
                                           importSettings.First(c => c.ContainsKey("context") && c["context"]?.ToString()?.Contains("Default", StringComparison.OrdinalIgnoreCase) == true);

            foreach (var kvp in instanceSettings)
            {
                if (kvp.Key == "context" || string.IsNullOrEmpty(kvp.Value?.ToString()))
                    continue;

                newInstanceSettings.Add(new InstanceSetting
                {
                    Setting = kvp.Key,
                    Value = kvp.Value.ToString()!,
                    InstanceId = instance.Id
                });
            }

            Dictionary<string, int> branchLookup = branchesWithSettings.ToDictionary(b => b.Name, b => b.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var settings in importSettings)
            {
                if (!settings.ContainsKey("context"))
                    continue;

                string branchName = settings["context"].ToString()!;

                if (branchName.Equals("default", StringComparison.OrdinalIgnoreCase) ||
                    !branchLookup.TryGetValue(branchName, out int branchId))
                    continue;

                foreach (var kvp in settings)
                {
                    if (kvp.Key == "context" || string.IsNullOrEmpty(kvp.Value?.ToString()))
                        continue;

                    newBranchSettings.Add(new BranchSetting
                    {
                        Setting = kvp.Key,
                        Value = kvp.Value.ToString()!,
                        BranchId = branchId
                    });
                }
            }

            if (newInstanceSettings.Count > 0)
            {
                _context.InstanceSettings.AddRange(newInstanceSettings);
            }

            if (newBranchSettings.Count > 0)
            {
                _context.BranchSetting.AddRange(newBranchSettings);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return true;
    }

    public async Task<List<InstanceSetting>> GetSettingsAsync(Instance instance)
    {
        List<InstanceSetting> branchSettings = await _context.Set<InstanceSetting>().Where(s => s.InstanceId == instance.Id).ToListAsync();
        return branchSettings;
    }

    public async Task<List<BranchSetting>> GetBranchSettingsAsync(Instance instance, int branchId)
    {
        bool branchExists = await _context.Set<Branch>()
            .AnyAsync(b => b.InstanceId == instance.Id && b.Id == branchId);

        if (!branchExists)
            return new List<BranchSetting>();

        return await _context.Set<BranchSetting>()
            .Where(s => s.BranchId == branchId)
            .ToListAsync();
    }

    public async Task<List<SettingBranchDto>> GetBranchesAsync(Instance instance)
    {
        List<Branch> branches = await _context.Set<Branch>()
            .Where(b => b.InstanceId == instance.Id)
            .OrderByDescending(b => b.IsDefault)
            .ToListAsync();

        return branches.Select(b => new SettingBranchDto
        {
            Id = b.Id,
            Name = b.Name
        }).ToList();
    }
}
