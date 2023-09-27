using BenchmarkDotNet.Running;
using SqlPractice.Benchmarks;


var summary = BenchmarkRunner.Run(typeof(InsertingBenchmark));
Console.ReadKey();