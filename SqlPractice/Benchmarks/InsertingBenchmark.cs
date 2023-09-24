using System.Data;
using BenchmarkDotNet.Attributes;
using Npgsql;
using SqlPractice.DbConnections;
using SqlPractice.Models;

namespace SqlPractice.Benchmarks;

public class InsertingBenchmark
{
    private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=TestDb";
    private readonly AppNpgsqlConnection _appNpgsqlConnection;
    private readonly NpgsqlConnection _connection1;
    private readonly NpgsqlConnection _connection2;
    private readonly NpgsqlConnection _connection3;

    private readonly List<string> _names = new() { "Vlad", "Joe", "Jack", "Pitro", "Volodka" };
    private readonly List<string> _surnames = new() { "sur1", "sur2", "sur3", "sur4", "sur4" };
    private readonly List<string> _cities = new() { "Kyiv", "Dnepr", "Odessa" };
    private readonly List<string> _streets = new() { "street1", "street2", "street3" };

    private readonly List<Human> _humans;
    public InsertingBenchmark()
    {
        _appNpgsqlConnection = new AppNpgsqlConnection(ConnectionString);
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

        var isAddressExist = await AddressIsExist(_connection1, tempData.Address, 1);
        if (isAddressExist)
        {
            var addressId = await GetAddressId(_connection1, tempData.Address, 1);
            await InsertHuman(_connection1, tempData, addressId, 1);
        }
        else
        {
            var addressId = await InsertAddress(_connection1, tempData.Address, 1);
            await InsertHuman(_connection1, tempData, addressId, 1);
        }

    }

    public async Task InsertingViaTransaction()
    {
        
    }

    public async Task InsertingViaProcedure()
    {
        
    }

    private async Task<bool> AddressIsExist(NpgsqlConnection connection, Address address, int numberAddressTable)
    {
        string query = $" select exists(" +
                       $"select 1 from address{numberAddressTable}" +
                       $" where city_name = @{nameof(Address.CityName)}" +
                       $" and street_name = @{nameof(Address.StreetName)}" +
                       $" and house_number = @{nameof(Address.HouseNumber)}" +
                       $")";
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue(nameof(Address.CityName), address.CityName);
        command.Parameters.AddWithValue(nameof(Address.StreetName), address.StreetName);
        command.Parameters.AddWithValue(nameof(Address.HouseNumber), address.HouseNumber);
        var result = await command.ExecuteScalarAsync();

        return (bool)result;
    }

    private async Task<int> GetAddressId(NpgsqlConnection connection, Address address, int numberAddressTable)
    {
        string query =
            $"select id from address{numberAddressTable} " +
            $" where city_name=@{nameof(Address.CityName)} " +
            $" and street_name=@{nameof(Address.StreetName)} " +
            $" and house_number=@{nameof(Address.HouseNumber)}" +
            $" limit 1 ";
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue(nameof(Address.CityName), address.CityName);
        command.Parameters.AddWithValue(nameof(Address.StreetName), address.StreetName);
        command.Parameters.AddWithValue(nameof(Address.HouseNumber), address.HouseNumber);
        
        var result = await command.ExecuteScalarAsync();

        return (int)result;
    }

    private async Task InsertHuman(NpgsqlConnection connection, Human human, int addressId, int numberTable)
    {
        string query = $"insert into people{numberTable} (name,surname,age,address_id)" +
                       $" values (@{nameof(Human.Name)}, @{nameof(Human.Surname)}, @{nameof(Human.Age)}, @AddressId)";
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue(nameof(Human.Name), human.Name);
        command.Parameters.AddWithValue(nameof(Human.Surname), human.Surname);
        command.Parameters.AddWithValue(nameof(Human.Age), human.Age);
        command.Parameters.AddWithValue("AddressId", addressId);

        await command.ExecuteScalarAsync();
    }

    private async Task<int> InsertAddress(NpgsqlConnection connection, Address address, int numberTable)
    {
        string query = $"insert into address{numberTable} (city_name, street_name, house_number) " +
                       $"values (@{nameof(Address.CityName)}, @{nameof(Address.StreetName)}, @{nameof(Address.HouseNumber)}) " +
                       $" returning id";
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue(nameof(Address.CityName), address.CityName);
        command.Parameters.AddWithValue(nameof(Address.StreetName), address.StreetName);
        command.Parameters.AddWithValue(nameof(Address.HouseNumber), address.HouseNumber);

        var result = await command.ExecuteScalarAsync();

        return (int)result;
    }
}