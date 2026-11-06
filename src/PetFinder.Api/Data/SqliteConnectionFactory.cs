using System.Data;
using Microsoft.Data.Sqlite;

namespace PetFinder.Api.Data;

public sealed class SqliteConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
