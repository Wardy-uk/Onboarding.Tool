using Onboarding.Tool.Model.Dashboard.Users;
using InstanceModel = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using User = Onboarding.Tool.Model.Data.Users;

namespace Onboarding.Tool.Server.Services.Dashboard.Users;

public interface IDashboardUserService
{
    public Task<User?> CreateUserAsync(ImportUser user, InstanceModel instance);

    public Task<List<User>> ImportUsersAsync(List<ImportUser> users, InstanceModel instance);

    public Task<User?> UpdateUser(int userId, ImportUser newDetails, InstanceModel instance);

    public Task<bool> DeleteUser(int userId, InstanceModel instance);
}
