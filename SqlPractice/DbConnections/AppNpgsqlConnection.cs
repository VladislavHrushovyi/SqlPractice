using System.Data;
using Npgsql;

namespace SqlPractice.DbConnections;

public class AppNpgsqlConnection
{
    private readonly IDbConnection _connection;

    public AppNpgsqlConnection(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
    }
    public IDbConnection GetConnection()
    {
        return _connection;
    }

    public async Task InitDatabase()
    {
        
    }

    public async Task CreateTables()
    {
        
    }

    public async Task InitData()
    {
        
    }
}