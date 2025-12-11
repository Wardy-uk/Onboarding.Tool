using System.Data.Common;
using System.Data.SqlClient;

namespace Onboarding.Tool.Server.Factories.InstanceSqlConnection;

public class InstanceSqlConnectionFactory : IInstanceSqlConnectionFactory
{
    private readonly IConfigDatabaseService _configService;

    public InstanceSqlConnectionFactory(IConfigDatabaseService configService)
    {
        _configService = configService;
    }

    public async Task<DbConnection> CreateInstanceConnectionAsync(string instance)
    {
        string connectionString = await _configService.GetInstanceConnectionStringAsync(instance);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Unable to obtain connection string for {instance}.");
        }

        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();

        return connection;
    }
}
