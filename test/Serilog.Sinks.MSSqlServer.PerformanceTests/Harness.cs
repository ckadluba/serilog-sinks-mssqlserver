using BenchmarkDotNet.Configs;
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
            .WithOptions(ConfigOptions.DontOverwriteResults);

        BenchmarkRunner.Run<PipelineBenchmark>(config);
    }
}
