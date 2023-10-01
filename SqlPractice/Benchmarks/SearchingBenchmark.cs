using BenchmarkDotNet.Attributes;
using Npgsql;
using SqlPractice.DbConnections;

namespace SqlPractice.Benchmarks;

[MemoryDiagnoser]
public class SearchingBenchmark
{
    private readonly NpgsqlConnection _connection;
    private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=TestDb";
    public SearchingBenchmark()
    {
        var connectionInstance = new AppNpgsqlConnection(ConnectionString);
        this._connection = connectionInstance.CreateConnection();
    }
    
    [GlobalSetup]
    public void Setup()
    {
        var connectionInstance = new AppNpgsqlConnection(ConnectionString);
        connectionInstance.InitDataInTable(6);
        connectionInstance.InitDataInTable(7);
        connectionInstance.CreateNameSurnameIndex();
    }

    [Benchmark(Baseline = true)]
    public async Task SearchByDefault()
    {
        await _connection.OpenAsync();
        string query = "select * from people6 where name='Vlad' and surname='sur1'";
        await using var command = new NpgsqlCommand(query, _connection);

        var result = await command.ExecuteScalarAsync();
        await _connection.CloseAsync();
    }
    
    [Benchmark]
    public async Task SearchByIndexCreation()
    {
        await _connection.OpenAsync();
        string query = "select * from people7 where name='Vlad' and surname='sur1'";
        await using var command = new NpgsqlCommand(query, _connection);

        var result = await command.ExecuteScalarAsync();
        await _connection.CloseAsync();
    }
}