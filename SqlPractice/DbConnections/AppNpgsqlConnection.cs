using System.Data;
using Npgsql;

namespace SqlPractice.DbConnections;

public class AppNpgsqlConnection
{
    private readonly IDbConnection _connection;
    private readonly string _connectionString;
    public AppNpgsqlConnection(string connectionString)
    {
        _connectionString = connectionString;
        _connection = new NpgsqlConnection(connectionString);
    }
    public IDbConnection GetConnection()
    {
        return _connection;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}