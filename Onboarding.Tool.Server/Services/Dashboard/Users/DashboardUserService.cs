using Onboarding.Tool.Data;
using Onboarding.Tool.Model.Dashboard.Users;
using InstanceModel = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using User = Onboarding.Tool.Model.Data.Users;

namespace Onboarding.Tool.Server.Services.Dashboard.Users;

public class DashboardUserService : IDashboardUserService
{
    private readonly IOnboardingDataService _onboardingDataService;
    private readonly AppDbContext _context;

    public DashboardUserService(IOnboardingDataService onboardingDataService, AppDbContext context)
    {
        _onboardingDataService = onboardingDataService;
        _context = context;
    }

    public async Task<List<User>> ImportUsersAsync(List<ImportUser> users, InstanceModel instance)
    {
        IEnumerable<string> existingUsers = await _onboardingDataService.GetSetupUserEmailsAsync(instance);
        List<User> newUsers = CreateUsersFromImport(users, instance.Id, existingUsers);

        if (newUsers.Count > 0)
        {
            _context.Users.AddRange(newUsers);
            await _context.SaveChangesAsync();
        }

        return newUsers;
    }

    public async Task<User?> CreateUserAsync(ImportUser user, InstanceModel instance)
    {
        bool doesUserExist = await _onboardingDataService.DoesUserExistAsync(user, instance);

        if (doesUserExist)
            return null;

        User createdUser = new()
        {
            Username = user.Email,
            InstanceId = instance.Id
        };

        _context.Users.Add(createdUser);
        await _context.SaveChangesAsync();

        return createdUser;
    }

    public async Task<User?> UpdateUser(int userId, ImportUser newDetails, InstanceModel instance)
    {
        bool doesUserExist = await _onboardingDataService.DoesUserExistAsync(newDetails, instance);

        if (doesUserExist)
            return null;

        User? existingUser = _context.Users.Where(u => u.Id == userId && u.InstanceId == instance.Id).FirstOrDefault();
        if (existingUser == null)
            return null;

        existingUser.Username = newDetails.Email;
        await _context.SaveChangesAsync();

        return existingUser;
    }

    public async Task<bool> DeleteUser(int userId, InstanceModel instance)
    {
        User? existingUser = _context.Users.Where(u => u.Id == userId && u.InstanceId == instance.Id).FirstOrDefault();
        
        if (existingUser == null)
            return false;

        _context.Users.Remove(existingUser);
        await _context.SaveChangesAsync();

        return true;
    }

    private static List<User> CreateUsersFromImport(
                List<ImportUser> importUsers,
                int instanceId,
                IEnumerable<string> existingEmails)
    {
        return importUsers.Where(importUser => !string.IsNullOrWhiteSpace(importUser.Email) &&!existingEmails.Contains(importUser.Email))
            .Select(importUser => new User
            {
                Username = importUser.Email,
                InstanceId = instanceId
            }).ToList();
    }
}
