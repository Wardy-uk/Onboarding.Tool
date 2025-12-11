using System.Data.Common;

namespace Onboarding.Tool.Server.Factories.InstanceSqlConnection;

public interface IInstanceSqlConnectionFactory
{
    Task<DbConnection> CreateInstanceConnectionAsync(string instance);
}
