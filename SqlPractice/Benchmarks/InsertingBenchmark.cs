using System.Data;
using BenchmarkDotNet.Attributes;
using SqlPractice.DbConnections;
using SqlPractice.Models;

namespace SqlPractice.Benchmarks;

public class InsertingBenchmark
{
    private readonly string _connectionString = "";
    private readonly AppNpgsqlConnection _appNpgsqlConnection;
    private readonly IDbConnection _connection1;
    private readonly IDbConnection _connection2;
    private readonly IDbConnection _connection3;

    private readonly List<string> _names = new() { "Vlad", "Joe", "Jack", "Pitro", "Volodka" };
    private readonly List<string> _surnames = new() { "sur1", "sur2", "sur3", "sur4", "sur4" };
    private readonly List<string> _cities = new() { "Kyiv", "Dnepr", "Odessa" };
    private readonly List<string> _streets = new() { "street1", "street2", "street3" };

    private readonly List<Human> _humans;
    public InsertingBenchmark()
    {
        _appNpgsqlConnection = new AppNpgsqlConnection(_connectionString);
        _connection1 = _appNpgsqlConnection.CreateConnection();
        _connection2 = _appNpgsqlConnection.CreateConnection();
        _connection3 = _appNpgsqlConnection.CreateConnection();
        _humans = Enumerable.Range(1, 100).Select(_ => new Human()
        {
            Name = _names[Random.Shared.Next(0, _names.Count - 1)],
            Surname = _surnames[Random.Shared.Next(0, _surnames.Count - 1)],
            Age = Random.Shared.Next(10,75),
            Address = new Address()
            {
                CityName = _cities[Random.Shared.Next(0, _cities.Count - 1)],
                HouseNumber = Random.Shared.Next(1, 999),
                StreetName = _streets[Random.Shared.Next(0, _streets.Count - 1)]
            }
        }).ToList();
    }

    [Benchmark]
    public async Task InsertingJustSelectQuery()
    {
        var connection = _appNpgsqlConnection.CreateConnection();
        var transaction = connection.BeginTransaction();
    }

    public async Task InsertingViaTransaction()
    {
        
    }

    public async Task InsertingViaProcedure()
    {
        
    }
}