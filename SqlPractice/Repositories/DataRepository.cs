using Npgsql;
using NpgsqlTypes;
using SqlPractice.Models;

namespace SqlPractice.Repositories;

public class DataRepository
{
    public async Task<bool> AddressIsExist(NpgsqlConnection connection, Address address, int numberAddressTable)
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

    public async Task<int> GetAddressId(NpgsqlConnection connection, Address address, int numberAddressTable)
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

    public async Task InsertHuman(NpgsqlConnection connection, Human human, int addressId, int numberTable)
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

    public async Task<int> InsertAddress(NpgsqlConnection connection, Address address, int numberTable)
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

    public async Task<int> InsertIfNotExistAddress(NpgsqlConnection connection, Address address)
    {
        string query =
            $"select add_address3_Id(@{nameof(Address.CityName)},@{nameof(Address.StreetName)},@{nameof(Address.HouseNumber)})";
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue(nameof(Address.CityName),NpgsqlDbType.Text ,address.CityName);
        command.Parameters.AddWithValue(nameof(Address.StreetName),NpgsqlDbType.Text, address.StreetName);
        command.Parameters.AddWithValue(nameof(Address.HouseNumber),NpgsqlDbType.Integer, address.HouseNumber);
        var result = await command.ExecuteScalarAsync();

        return (int)result;
    }

    public async Task InsertHumanFunction(NpgsqlConnection connection, Human human, int addressId)
    {
        string query = $"select public.add_human3(" +
                       $"@{nameof(Human.Name)}," +
                       $"@{nameof(Human.Surname)}," +
                       $"@{nameof(Human.Age)}," +
                       $"@addressId)";
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue(nameof(Human.Name), NpgsqlDbType.Text, human.Name);
        command.Parameters.AddWithValue(nameof(Human.Surname), NpgsqlDbType.Text, human.Surname);
        command.Parameters.AddWithValue(nameof(Human.Age), NpgsqlDbType.Integer, human.Age);
        command.Parameters.AddWithValue("addressId", NpgsqlDbType.Integer, addressId);

        await command.ExecuteScalarAsync();
    }
}