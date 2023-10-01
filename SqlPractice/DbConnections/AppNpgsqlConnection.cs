using System.Text;
using Npgsql;
using SqlPractice.Utils;

namespace SqlPractice.DbConnections;

public class AppNpgsqlConnection
{
    private readonly NpgsqlConnection _connection;
    private const int AmountTables = 7;
    private readonly string _connectionString;
    public AppNpgsqlConnection(string connectionString)
    {
        _connectionString = connectionString;
        _connection = new NpgsqlConnection(connectionString);
    }
    public NpgsqlConnection GetConnection()
    {
        return _connection;
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public void DeleteAllTable()
    {
        _connection.Open();
        var queryBuilder = new StringBuilder("drop table if exists ");
        for (int i = 0; i < AmountTables; i++)
        {
            queryBuilder.Append($"people{i + 1}, " +
                                $"address{i + 1}, ");
        }

        queryBuilder.Remove(queryBuilder.Length - 2, 1);
        queryBuilder.Append(';');
        using var command = new NpgsqlCommand(queryBuilder.ToString(), _connection);
        command.ExecuteScalar();
        _connection.Close();
    }

    public void InitTables()
    {
        _connection.Open();
        var queryBuilder = new StringBuilder();
        for (int i = 0; i < AmountTables; i++)
        {
            queryBuilder.Append(CreateQueryAddressTable(i + 1))
                .Append(CreateQueryPeopleTable(i + 1));
        }

        using var command = new NpgsqlCommand(queryBuilder.ToString(), _connection);
        command.ExecuteScalar();
        _connection.Close();
    }

    public void CreateNameSurnameIndex()
    {
        string query = "create index if not exists full_name on people7(name,surname);";
        _connection.Open();
        using var command = new NpgsqlCommand(query, _connection);
        command.ExecuteScalar();
        
        _connection.Close();
    }

    public void InitDataInTable(int numberOfTable)
    {
        _connection.Open();
        var data = StaticData.FormData().ToArray();
        var queryBuilder = new StringBuilder();

        for (int i = 0; i < 100; i++)
        {
            foreach (var human in data)
            {
                queryBuilder.Append("with new_address as ( ");
                queryBuilder.Append(
                    $"select add_address{numberOfTable}_id(" +
                    $"'{human.Address.CityName}', " +
                    $"'{human.Address.StreetName}', " +
                    $"{human.Address.HouseNumber}) as address_id ) ");
                queryBuilder.Append(
                    $"select add_human{numberOfTable}(" +
                    $"'{human.Name}', " +
                    $"'{human.Surname}', " +
                    $"{human.Age}, " +
                    $"new_address.address_id) ");
                queryBuilder.Append("from new_address;");
            }   
        }

        using var command = new NpgsqlCommand(queryBuilder.ToString(), _connection);
        var result = command.ExecuteScalar();

        _connection.Close();
    }

    private string CreateQueryAddressTable(int numberOfTable)
    {
        return $"""
                   CREATE TABLE public.address{numberOfTable}
               (
                   id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 ),
                   city_name character varying(50) NOT NULL,
                   street_name character varying(50) NOT NULL,
                   house_number integer NOT NULL,
                   PRIMARY KEY (id)
               );
               
               ALTER TABLE IF EXISTS public.address{numberOfTable}
                   OWNER to postgres;
               """;
    }

    private string CreateQueryPeopleTable(int numberOfTable)
    {
        return $"""
                 CREATE TABLE public.people{numberOfTable}
                 (
                     id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 ),
                     name character varying(50) NOT NULL,
                     surname character varying(50) NOT NULL,
                     age integer NOT NULL,
                     address_id integer NOT NULL,
                     PRIMARY KEY (id),
                     FOREIGN KEY (address_id)
                         REFERENCES public.address{numberOfTable} (id) MATCH SIMPLE
                         ON UPDATE NO ACTION
                         ON DELETE NO ACTION
                         NOT VALID
                 );

                 ALTER TABLE IF EXISTS public.people{numberOfTable}
                     OWNER to postgres;
                 """;
    }
}