using Npgsql;
using SqlPractice.DbConnections;

namespace SqlPractice.Benchmarks;

public class SearchingBenchmark
{
    private readonly NpgsqlConnection _connection;
    private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=TestDb";
    public SearchingBenchmark()
    {
        var connectionInstance = new AppNpgsqlConnection(ConnectionString);
        this._connection = connectionInstance.CreateConnection();
    }

    public async Task SearchByDefault()
    {
        
    }
    
    public async Task SearchByIndexCreation()
    {
        
    }
}