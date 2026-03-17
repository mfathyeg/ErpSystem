using System.Data;
using ErpSystem.Application.Abstractions.Data;
using Microsoft.Data.SqlClient;

namespace ErpSystem.Infrastructure.Data;

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
