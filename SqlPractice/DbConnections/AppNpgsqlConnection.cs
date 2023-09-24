using System.Data;
using Npgsql;

namespace SqlPractice.DbConnections;

public class AppNpgsqlConnection
{
    private readonly NpgsqlConnection _connection;
    private readonly string _connectionString;
    public AppNpgsqlConnection(string connectionString)
    {
        _connectionString = connectionString;
        _connection = new NpgsqlConnection(connectionString);
    }
    public NpgsqlConnection GetConnection()
    {
        return _connection;
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}