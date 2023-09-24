using BenchmarkDotNet.Attributes;
using SqlPractice.DbConnections;

namespace SqlPractice.Benchmarks;

public class InsertingBenchmark
{
    private readonly string _connectionString = "";
    private readonly AppNpgsqlConnection _appNpgsqlConnection;

    public InsertingBenchmark()
    {
        _appNpgsqlConnection = new AppNpgsqlConnection(_connectionString);
    }

    [Benchmark]
    public async Task InsertInOrder()
    {
        var connection = _appNpgsqlConnection.CreateConnection();
        var transaction = connection.BeginTransaction();
    }
}