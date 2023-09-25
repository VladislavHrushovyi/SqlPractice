using BenchmarkDotNet.Attributes;
using Npgsql;
using SqlPractice.DbConnections;
using SqlPractice.Models;
using SqlPractice.Repositories;
using SqlPractice.Utils;

namespace SqlPractice.Benchmarks;

public class InsertingBenchmark
{
    private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=TestDb";
    private readonly DataRepository _dataRepository = new();
    private readonly NpgsqlConnection _connection1;
    private readonly NpgsqlConnection _connection2;
    private readonly NpgsqlConnection _connection3;

    private readonly List<Human> _humans;
    public InsertingBenchmark()
    {
        var appNpgsqlConnection = new AppNpgsqlConnection(ConnectionString);
        _connection1 = appNpgsqlConnection.CreateConnection();
        _connection2 = appNpgsqlConnection.CreateConnection();
        _connection3 = appNpgsqlConnection.CreateConnection();
        _humans = Enumerable.Range(1, 100).Select(_ => new Human()
        {
            Name = StaticData._names[Random.Shared.Next(0, StaticData._names.Count - 1)],
            Surname = StaticData._surnames[Random.Shared.Next(0, StaticData._surnames.Count - 1)],
            Age = Random.Shared.Next(10,75),
            Address = new Address()
            {
                CityName = StaticData._cities[Random.Shared.Next(0, StaticData._cities.Count - 1)],
                HouseNumber = Random.Shared.Next(1, 999),
                StreetName = StaticData._streets[Random.Shared.Next(0, StaticData._streets.Count - 1)]
            }
        }).ToList();
    }

    [Benchmark]
    public async Task InsertingJustInsertQuery()
    {
        var tempData = new Human()
        {
            Name = "Pitro",
            Surname = "Sur2",
            Age = 45,
            Address = new Address()
            {
                CityName = "Kyiv",
                StreetName = "street2",
                HouseNumber = 236
            }
        };
        _connection1.Open();

        var isAddressExist = await _dataRepository.AddressIsExist(_connection1, tempData.Address, 1);
        if (isAddressExist)
        {
            var addressId = await _dataRepository.GetAddressId(_connection1, tempData.Address, 1);
            await _dataRepository.InsertHuman(_connection1, tempData, addressId, 1);
        }
        else
        {
            var addressId = await _dataRepository.InsertAddress(_connection1, tempData.Address, 1);
            await _dataRepository.InsertHuman(_connection1, tempData, addressId, 1);
        }

    }

    public async Task InsertingViaTransaction()
    {
        
    }

    public async Task InsertingViaProcedure()
    {
        
    }
    
}