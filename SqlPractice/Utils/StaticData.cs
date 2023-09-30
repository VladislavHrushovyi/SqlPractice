using SqlPractice.Models;

namespace SqlPractice.Utils;

public static class StaticData
{
    public static List<string> _names = new() { "Vlad", "Joe", "Jack", "Pitro", "Volodka" };
    public static List<string> _surnames = new() { "sur1", "sur2", "sur3", "sur4", "sur4" };
    public static List<string> _cities = new() { "Kyiv", "Dnepr", "Odessa" };
    public static List<string> _streets = new() { "street1", "street2", "street3" };

    public static IEnumerable<Human> FormData()
    {
        var rnd = new Random();
        return Enumerable.Range(1, 300).Select(_ => new Human()
        {
            Name = StaticData._names[Random.Shared.Next(0, StaticData._names.Count - 1)],
            Surname = StaticData._surnames[Random.Shared.Next(0, StaticData._surnames.Count - 1)],
            Age = Random.Shared.Next(10, 75),
            Address = new Address()
            {
                CityName = StaticData._cities[Random.Shared.Next(0, StaticData._cities.Count - 1)],
                HouseNumber = Random.Shared.Next(1, 999),
                StreetName = StaticData._streets[Random.Shared.Next(0, StaticData._streets.Count - 1)]
            }
        });
    }
}