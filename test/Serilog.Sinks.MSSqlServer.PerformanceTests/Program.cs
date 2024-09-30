using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Running;

namespace Serilog.Sinks.MSSqlServer.PerformanceTests;

public class Program
{
    public static void Main(string[] args)
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .WithOptions(ConfigOptions.DontOverwriteResults)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddExporter(JsonExporter.Default)
            .AddExporter(CsvExporter.Default)
            .WithArtifactsPath(@"./BenchmarkDotNet.Artifacts/results");

        BenchmarkRunner.Run<PipelineBenchmark>(config);
    }
}
