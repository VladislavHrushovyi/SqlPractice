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
        _humans = Enumerable.Range(1, 300).Select(_ => new Human()
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

    [Benchmark(Baseline = true)]
    public async Task InsertingJustInsertQuery()
    {
        _connection1.Open();

        foreach (var human in _humans)
        {
            var isAddressExist = await _dataRepository.AddressIsExist(_connection1, human.Address, 1);
            if (isAddressExist)
            {
                var addressId = await _dataRepository.GetAddressId(_connection1, human.Address, 1);
                await _dataRepository.InsertHuman(_connection1, human, addressId, 1);
            }
            else
            {
                var addressId = await _dataRepository.InsertAddress(_connection1, human.Address, 1);
                await _dataRepository.InsertHuman(_connection1, human, addressId, 1);
            }
        }

        await _connection1.CloseAsync();
    }

    [Benchmark]
    public async Task InsertingViaTransaction()
    {
        await _connection2.OpenAsync();
        await using var transaction = await _connection2.BeginTransactionAsync();
        try
        {
            foreach (var human in _humans)
            {
                var isAddressExist = await _dataRepository.AddressIsExist(_connection2, human.Address, 2);
                if (isAddressExist)
                {
                    var addressId = await _dataRepository.GetAddressId(_connection2, human.Address, 2);
                    await _dataRepository.InsertHuman(_connection2, human, addressId, 2);
                }
                else
                {
                    var addressId = await _dataRepository.InsertAddress(_connection2, human.Address, 2);
                    await _dataRepository.InsertHuman(_connection2, human, addressId, 2);
                }
            }
            
            await transaction.CommitAsync();
            await _connection2.CloseAsync();
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync();
            await _connection2.CloseAsync();
            Console.WriteLine(ex.Message);
        }
    }

    [Benchmark]
    public async Task InsertingViaProcedure()
    {
        await _connection3.OpenAsync();

        foreach (var human in _humans)
        {
            var addressId = await _dataRepository.InsertIfNotExistAddress(_connection3, human.Address);
            await _dataRepository.InsertHumanFunction(_connection3, human, addressId);
        }

        await _connection3.CloseAsync();
    }
    
}