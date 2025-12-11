using System.Data.Common;

namespace Onboarding.Tool.Server.Factories.OnboardingSqlConnection;

public interface IOnboardingSqlConnection
{
    public DbConnection CreateOnboardingSqlConnection();
}
