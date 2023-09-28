using BenchmarkDotNet.Running;
using SqlPractice.Benchmarks;


var summary = BenchmarkRunner.Run(typeof(InsertingBenchmark));
// var benchmark = new InsertingBenchmark();
// await benchmark.InsertingViaBigInsertQueryString();
Console.ReadKey();