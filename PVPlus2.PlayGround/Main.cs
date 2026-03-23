using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PVPlus2.Models;
using PVPlus2.Services;

namespace PVPlus2.PlayGround;

internal static class Program
{
    private const int ScenarioN = 30;
    private const int WarmupIterationCount = 1_000;
    private const int MeasureIterationCount = 1_000;
    private static readonly double TimestampToNanoseconds = 1_000_000_000.0 / Stopwatch.Frequency;
    private static readonly string WorkspaceDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    private static readonly string OutputPath = Path.Combine(
        WorkspaceDirectory,
        "DU_arity_special_function_benchmark_codex.csv");

    private static double _sink;

    private sealed record BenchmarkDefinition(
        string Name,
        string DisplayText,
        CommutationTable Context,
        Func<BenchmarkExecutable> CreateExecutable);

    private sealed record BenchmarkExecutable(
        double CompileNanoseconds,
        Action<CommutationTable, double[]> Action);

    private sealed record BenchmarkResult(
        string Name,
        string DisplayText,
        double CompileNanoseconds,
        double TotalNanoseconds,
        double NanosecondsPerInvocation,
        double NanosecondsPerElement,
        double Checksum);

    private static void Main()
    {
        var benchmarks = CreateBenchmarkDefinitions()
            .Select(RunBenchmark)
            .ToArray();

        WriteResults(benchmarks);
        PrintResults(benchmarks);
    }

    private static BenchmarkResult RunBenchmark(BenchmarkDefinition definition)
    {
        var executable = definition.CreateExecutable();
        var target = new double[(int)CommutationTable.MAXSIZE];
        double localSink = 0.0;

        for (int i = 0; i < WarmupIterationCount; i++)
        {
            executable.Action(definition.Context, target);
            localSink += ConsumeTarget(target, definition.Context.n);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long measureStart = Stopwatch.GetTimestamp();

        for (int i = 0; i < MeasureIterationCount; i++)
        {
            executable.Action(definition.Context, target);
            localSink += ConsumeTarget(target, definition.Context.n);
        }

        long measureElapsedTicks = Stopwatch.GetTimestamp() - measureStart;
        double totalNanoseconds = measureElapsedTicks * TimestampToNanoseconds;
        double invocationCount = MeasureIterationCount;
        double elementCount = MeasureIterationCount * (definition.Context.n + 1);

        _sink += localSink;

        return new BenchmarkResult(
            definition.Name,
            definition.DisplayText,
            executable.CompileNanoseconds,
            totalNanoseconds,
            totalNanoseconds / invocationCount,
            totalNanoseconds / elementCount,
            localSink);
    }

    private static BenchmarkDefinition[] CreateBenchmarkDefinitions()
    {
        return
        [
            CreateExpressionBenchmark(
                "D.one.special",
                "D(0.5)",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "D.one.if",
                "if(S1 > 0 OR t >= 1, 1.0, 0.5)",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "D.two.special",
                "D(0.5, 0.75)",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "D.two.if",
                "if(S1 > 0 OR t >= 2, 1.0, if(t = 0, 0.5, 0.75))",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "D.three.special",
                "D(0.5, 0.75, 0.8)",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "D.three.if",
                "if(S1 > 0 OR t >= 3, 1.0, if(t = 0, 0.5, if(t = 1, 0.75, 0.8)))",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "U.one.special",
                "U(0.5)",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "U.one.if",
                "if(S1 > 0 OR Age < 15 OR t >= 1, 1.0, 0.5)",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "U.two.special",
                "U(0.5, 0.75)",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "U.two.if",
                "if(S1 > 0 OR Age < 15 OR t >= 2, 1.0, if(t = 0, 0.5, 0.75))",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "U.three.special",
                "U(0.5, 0.75, 0.8)",
                CreateContext(s1: 0, age: 30)),
            CreateExpressionBenchmark(
                "U.three.if",
                "if(S1 > 0 OR Age < 15 OR t >= 3, 1.0, if(t = 0, 0.5, if(t = 1, 0.75, 0.8)))",
                CreateContext(s1: 0, age: 30)),
        ];
    }

    private static BenchmarkDefinition CreateExpressionBenchmark(
        string name,
        string expressionText,
        CommutationTable context)
    {
        return new BenchmarkDefinition(
            name,
            expressionText,
            context,
            () =>
            {
                long compileStart = Stopwatch.GetTimestamp();
                var action = ExpressionCompiler.CompileDoubleArrayInto(expressionText);
                long compileElapsedTicks = Stopwatch.GetTimestamp() - compileStart;
                double compileNanoseconds = compileElapsedTicks * TimestampToNanoseconds;
                return new BenchmarkExecutable(compileNanoseconds, action);
            });
    }

    private static CommutationTable CreateContext(long s1, long age)
    {
        return new CommutationTable
        {
            n = ScenarioN,
            S1 = s1,
            Age = age
        };
    }

    private static double ConsumeTarget(double[] target, long n)
    {
        int last = (int)n;
        return target[0] + target[1] + target[2] + target[last];
    }

    private static void WriteResults(IEnumerable<BenchmarkResult> results)
    {
        var lines = new List<string>
        {
            "Name,Expression,CompileNs,TotalNs,NsPerInvocation,NsPerElement,Checksum"
        };

        lines.AddRange(results.Select(result =>
            string.Join(",",
                EscapeCsv(result.Name),
                EscapeCsv(result.DisplayText),
                result.CompileNanoseconds.ToString("F3"),
                result.TotalNanoseconds.ToString("F3"),
                result.NanosecondsPerInvocation.ToString("F3"),
                result.NanosecondsPerElement.ToString("F6"),
                result.Checksum.ToString("R"))));

        File.WriteAllLines(OutputPath, lines);
    }

    private static void PrintResults(IEnumerable<BenchmarkResult> results)
    {
        Console.WriteLine("D/U Arity Special Function Benchmark");
        Console.WriteLine($"n                   : {ScenarioN}");
        Console.WriteLine($"warmup iterations   : {WarmupIterationCount:N0}");
        Console.WriteLine($"measure iterations  : {MeasureIterationCount:N0}");
        Console.WriteLine($"timer frequency     : {Stopwatch.Frequency:N0} ticks/sec");
        Console.WriteLine($"output              : {OutputPath}");
        Console.WriteLine();

        foreach (var result in results)
        {
            Console.WriteLine(result.Name);
            Console.WriteLine($"  expr/call         : {result.DisplayText}");
            Console.WriteLine($"  compile ns        : {result.CompileNanoseconds:F3}");
            Console.WriteLine($"  total ns          : {result.TotalNanoseconds:F3}");
            Console.WriteLine($"  ns / invocation   : {result.NanosecondsPerInvocation:F3}");
            Console.WriteLine($"  ns / element      : {result.NanosecondsPerElement:F6}");
        }

        Console.WriteLine();
        Console.WriteLine($"checksum sink       : {_sink:R}");
    }

    private static string EscapeCsv(string value)
    {
        if (!value.Contains(',') && !value.Contains('"'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
