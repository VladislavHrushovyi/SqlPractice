using System.Text;
using BenchmarkDotNet.Attributes;
using Npgsql;
using SqlPractice.DbConnections;
using SqlPractice.Models;
using SqlPractice.Repositories;
using SqlPractice.Utils;

namespace SqlPractice.Benchmarks;

[MemoryDiagnoser]
public class InsertingBenchmark
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=TestDb";

    private readonly DataRepository _dataRepository = new();
    private readonly NpgsqlConnection _connection;

    private readonly IEnumerable<Human> _humans;

    public InsertingBenchmark()
    {
        var appNpgsqlConnection = new AppNpgsqlConnection(ConnectionString);
        _connection = appNpgsqlConnection.CreateConnection();
        _humans = StaticData.FormData();
    }

    [Benchmark(Baseline = true)]
    public async Task InsertingJustInsertQuery()
    {
        await _connection.OpenAsync();

        foreach (var human in _humans)
        {
            var isAddressExist = await _dataRepository.AddressIsExist(_connection, human.Address, 1);
            if (isAddressExist)
            {
                var addressId = await _dataRepository.GetAddressId(_connection, human.Address, 1);
                await _dataRepository.InsertHuman(_connection, human, addressId, 1);
            }
            else
            {
                var addressId = await _dataRepository.InsertAddress(_connection, human.Address, 1);
                await _dataRepository.InsertHuman(_connection, human, addressId, 1);
            }
        }

        await _connection.CloseAsync();
    }

    [Benchmark]
    public async Task InsertingViaTransaction()
    {
        await _connection.OpenAsync();
        await using var transaction = await _connection.BeginTransactionAsync();
        try
        {
            foreach (var human in _humans)
            {
                var isAddressExist = await _dataRepository.AddressIsExist(_connection, human.Address, 2);
                if (isAddressExist)
                {
                    var addressId = await _dataRepository.GetAddressId(_connection, human.Address, 2);
                    await _dataRepository.InsertHuman(_connection, human, addressId, 2);
                }
                else
                {
                    var addressId = await _dataRepository.InsertAddress(_connection, human.Address, 2);
                    await _dataRepository.InsertHuman(_connection, human, addressId, 2);
                }
            }

            await transaction.CommitAsync();
            await _connection.CloseAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await _connection.CloseAsync();
            Console.WriteLine(ex.Message);
        }
    }

    [Benchmark]
    public async Task InsertingViaProcedure()
    {
        await _connection.OpenAsync();

        foreach (var human in _humans)
        {
            var addressId = await _dataRepository.InsertIfNotExistAddress(_connection, human.Address);
            await _dataRepository.InsertHumanFunction(_connection, human, addressId);
        }

        await _connection.CloseAsync();
    }

    [Benchmark]
    public async Task InsertingViaBigInsertQueryString()
    {
                /*
                 WITH new_address AS (
                    SELECT id
                    FROM address4
                 WHERE
                city_name = 'test'
                AND street_name = 'test'
                AND house_number = 44
        ), inserted_address AS (
                    INSERT INTO address4(city_name, street_name, house_number)
                    SELECT 'test', 'test', 44
                    WHERE NOT EXISTS (SELECT 1 FROM new_address)
                    RETURNING id
                )
                    INSERT INTO people4(name, surname, age, address_id)
                    SELECT 'name2', 'surname2', 66, COALESCE((SELECT id FROM inserted_address), (SELECT id FROM new_address limit 1));	
                 */
        await _connection.OpenAsync();
        var queryBuilder = new StringBuilder();
        foreach (var human in _humans)
        {
            queryBuilder.Append("with new_address as (select id from address4 where ");
            queryBuilder.AppendFormat("city_name = '{0}' ", human.Address.CityName);
            queryBuilder.AppendFormat("and street_name = '{0}' ", human.Address.StreetName);
            queryBuilder.AppendFormat("and house_number = {0} ", human.Address.HouseNumber);
            queryBuilder.Append("), inserted_address as (insert into address4(city_name, street_name, house_number) ");
            queryBuilder.AppendFormat("select '{0}', '{1}', {2} ", human.Address.CityName, human.Address.StreetName, human.Address.HouseNumber);
            queryBuilder.Append("where not exists (select 1 from new_address) returning id) ");
            queryBuilder.Append("insert into people4(name, surname, age, address_id) ");
            queryBuilder.AppendFormat("select '{0}', '{1}', {2}, coalesce(", human.Name, human.Surname, human.Age);
            queryBuilder.Append("(select id from inserted_address), ");
            queryBuilder.Append("(select id from new_address limit 1)); ");
        }

        await using var command = new NpgsqlCommand(queryBuilder.ToString(), _connection);
        var result = await command.ExecuteScalarAsync();

        await _connection.CloseAsync();
    }

    [Benchmark]
    public async Task InsertingViaBigCallingFunctionString()
    {
        /*
         * with new_address as (
            select add_address5_id('wqdqwdq', 'wdwedwe', 44) as address_id
            ) select public.add_human5('1111', '1111', 22, new_address.address_id)
            from new_address
         */
        await _connection.OpenAsync();
        var queryBuilder = new StringBuilder();

        foreach (var human in _humans)
        {
            queryBuilder.Append("with new_address as ( ");
            queryBuilder.AppendFormat("select add_address5_id('{0}', '{1}', {2}) as address_id ) ", 
                human.Address.CityName, human.Address.StreetName, human.Address.HouseNumber);
            queryBuilder.AppendFormat("select add_human5('{0}', '{1}', {2}, new_address.address_id) ",
                human.Name, human.Surname, human.Age);
            queryBuilder.Append("from new_address;");
        }

        await using var command = new NpgsqlCommand(queryBuilder.ToString(), _connection);
        var result = await command.ExecuteScalarAsync();

        await _connection.CloseAsync();
    }
}