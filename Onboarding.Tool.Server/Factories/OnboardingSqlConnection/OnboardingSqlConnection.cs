using System.Data.Common;
using System.Data.SqlClient;

namespace Onboarding.Tool.Server.Factories.OnboardingSqlConnection;

public class OnboardingSqlConnection : IOnboardingSqlConnection
{
    private readonly string _connectionString;

    public OnboardingSqlConnection(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string: DefaultConnection");
    }

    public DbConnection CreateOnboardingSqlConnection()
    {
        SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        return connection;
    }
}
