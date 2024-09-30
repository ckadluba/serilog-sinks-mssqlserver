using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.PerformanceTests;

/// <summary>
/// Wrappers that make it easy to run benchmark suites through the <c>dotnet test</c> runner.
/// </summary>
/// <example>
/// <code>dotnet test -c Release --filter "FullyQualifiedName=Serilog.Sinks.MSSqlServer.PerformanceTests.Harness.Pipeline"</code>
/// </example>
public class Harness
{
    [Fact]
    public void Pipeline()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .WithOptions(ConfigOptions.DontOverwriteResults)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddExporter(JsonExporter.Default)
            .AddLogger(ConsoleLogger.Default); // Logging aktivieren

        BenchmarkRunner.Run<PipelineBenchmark>(config);
    }
}
