using BenchmarkDotNet.Running;
using SqlPractice.Benchmarks;
using SqlPractice.DbConnections;

var appContext = new AppNpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=TestDb");
appContext.DeleteAllTable();
appContext.InitTables();
//var summary = BenchmarkRunner.Run(typeof(InsertingBenchmark));
var summary = BenchmarkRunner.Run(typeof(SearchingBenchmark));
// var benchmark = new InsertingBenchmark();
// await benchmark.InsertingViaBigInsertQueryString();
// var benchmark = new SearchingBenchmark();
// await benchmark.SearchByIndexCreation();
Console.ReadKey();