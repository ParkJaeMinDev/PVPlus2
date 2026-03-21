using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Flee.PublicTypes;
using PVPlus2.Services;
using System.Globalization;
using CompilerContext = PVPlus2.Models.CommutationTable;
using FleeContext = Flee.PublicTypes.ExpressionContext;

BenchmarkSwitcher
    .FromTypes(
    [
        typeof(ScalarCompileBenchmarks),
        typeof(ArrayCompileBenchmarks),
        typeof(ScalarEvaluationBenchmarks),
        typeof(ArrayEvaluationBenchmarks)
    ])
    .RunAllJoined(DefaultConfig.Instance, args);

[MemoryDiagnoser]
[ShortRunJob]
public class ScalarCompileBenchmarks
{
    [ParamsSource(nameof(Cases))]
    public ScalarCase Case { get; set; } = default!;

    public IEnumerable<ScalarCase> Cases => BenchmarkDataFactory.ScalarCases;

    private FleeContext _fleeContext = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _fleeContext = BenchmarkDataFactory.CreateScalarFleeContext();
    }

    [Benchmark(Baseline = true)]
    public object ExpressionCompiler_Compile()
    {
        return ExpressionCompiler.CompileLong(Case.Expression);
    }

    [Benchmark]
    public object Flee_Compile()
    {
        return _fleeContext.CompileDynamic(Case.Expression);
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ArrayCompileBenchmarks
{
    [ParamsSource(nameof(Cases))]
    public ArrayCase Case { get; set; } = default!;

    public IEnumerable<ArrayCase> Cases => BenchmarkDataFactory.ArrayCases;

    private FleeContext _fleeContext = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _fleeContext = BenchmarkDataFactory.CreateArrayFleeContext();
    }

    [Benchmark(Baseline = true)]
    public object ExpressionCompiler_Compile()
    {
        return ExpressionCompiler.CompileDoubleArrayInto(Case.Expression);
    }

    [Benchmark]
    public object Flee_Compile()
    {
        return _fleeContext.CompileDynamic(Case.Expression);
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ScalarEvaluationBenchmarks
{
    [ParamsSource(nameof(Cases))]
    public ScalarCase Case { get; set; } = default!;

    public IEnumerable<ScalarCase> Cases => BenchmarkDataFactory.ScalarCases;

    private CompilerContext _compilerContext = default!;
    private long[] _nValues = default!;
    private long[] _mValues = default!;
    private Func<CompilerContext, long> _compiler = default!;
    private FleeContext _fleeContext = default!;
    private IDynamicExpression _flee = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _compilerContext = BenchmarkDataFactory.CreateScalarContext();
        (_nValues, _mValues) = BenchmarkDataFactory.CreateScalarInputValues();

        _compiler = ExpressionCompiler.CompileLong(Case.Expression);
        _fleeContext = BenchmarkDataFactory.CreateScalarFleeContext();
        _flee = _fleeContext.CompileDynamic(Case.Expression);
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BenchmarkConstants.ScalarInputCount)]
    public long ExpressionCompiler_Evaluate()
    {
        var context = _compilerContext;
        var evaluator = _compiler;
        var nValues = _nValues;
        var mValues = _mValues;
        long total = 0;

        for (var i = 0; i < nValues.Length; i++)
        {
            context.n = nValues[i];
            context.m = mValues[i];
            total += evaluator(context);
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = BenchmarkConstants.ScalarInputCount)]
    public long Flee_Evaluate()
    {
        var context = _fleeContext;
        var variables = context.Variables;
        var evaluator = _flee;
        var nValues = _nValues;
        var mValues = _mValues;
        long total = 0;

        for (var i = 0; i < nValues.Length; i++)
        {
            variables["n"] = nValues[i];
            variables["m"] = mValues[i];
            total += BenchmarkDataFactory.ToLong(evaluator.Evaluate());
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = BenchmarkConstants.ScalarInputCount)]
    public long NativeCSharp_Evaluate()
    {
        var evaluator = Case.Native;
        var nValues = _nValues;
        var mValues = _mValues;
        long total = 0;

        for (var i = 0; i < nValues.Length; i++)
        {
            total += evaluator(nValues[i], mValues[i]);
        }

        return total;
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ArrayEvaluationBenchmarks
{
    [ParamsSource(nameof(Cases))]
    public ArrayCase Case { get; set; } = default!;

    public IEnumerable<ArrayCase> Cases => BenchmarkDataFactory.ArrayCases;

    private CompilerContext _context = default!;
    private Action<CompilerContext, double[]> _compiler = default!;
    private FleeContext _fleeContext = default!;
    private IDynamicExpression _flee = default!;
    private double[] _target = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _context = BenchmarkDataFactory.CreateArrayContext();
        _compiler = ExpressionCompiler.CompileDoubleArrayInto(Case.Expression);
        _fleeContext = BenchmarkDataFactory.CreateArrayFleeContext();
        _flee = _fleeContext.CompileDynamic(Case.Expression);
        _target = new double[BenchmarkConstants.ArrayOperationCount];
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BenchmarkConstants.ArrayOperationCount)]
    public double ExpressionCompiler_Evaluate()
    {
        _compiler(_context, _target);
        return BenchmarkDataFactory.Sum(_target, BenchmarkConstants.ArrayOperationCount);
    }

    [Benchmark(OperationsPerInvoke = BenchmarkConstants.ArrayOperationCount)]
    public double Flee_Evaluate()
    {
        var context = _context;
        var target = _target;
        var evaluator = _flee;
        var variables = _fleeContext.Variables;
        var q1 = context.q1;
        var q2 = context.q2;
        var r1 = context.r1;
        var r2 = context.r2;
        var k1 = context.k1;
        var k2 = context.k2;

        for (var i = 0; i < BenchmarkConstants.ArrayOperationCount; i++)
        {
            variables["q1"] = q1[i];
            variables["q2"] = q2[i];
            variables["r1"] = r1[i];
            variables["r2"] = r2[i];
            variables["k1"] = k1[i];
            variables["k2"] = k2[i];
            target[i] = BenchmarkDataFactory.ToDouble(evaluator.Evaluate());
        }

        return BenchmarkDataFactory.Sum(target, BenchmarkConstants.ArrayOperationCount);
    }

    [Benchmark(OperationsPerInvoke = BenchmarkConstants.ArrayOperationCount)]
    public double NativeCSharp_Evaluate()
    {
        var context = _context;
        var target = _target;
        var evaluator = Case.Native;

        for (var i = 0; i < BenchmarkConstants.ArrayOperationCount; i++)
        {
            target[i] = evaluator(context, i);
        }

        return BenchmarkDataFactory.Sum(target, BenchmarkConstants.ArrayOperationCount);
    }
}

public sealed record ScalarCase(string Label, string Expression, Func<long, long, long> Native)
{
    public override string ToString() => Label;
}

public sealed record ArrayCase(string Label, string Expression, Func<CompilerContext, int, double> Native)
{
    public override string ToString() => Label;
}

internal static class BenchmarkConstants
{
    internal const int ScalarInputCount = 2048;
    internal const int ArrayOperationCount = 128;
    internal const long ArrayN = 127;
    internal const int Seed = 12345;
    internal const long ScalarMinValue = 1;
    internal const long ScalarMaxExclusive = 1000;
}

internal static class BenchmarkDataFactory
{
    internal static IReadOnlyList<ScalarCase> ScalarCases { get; } =
    [
        new("S01 n + m", "n + m", static (n, m) => n + m),
        new("S02 n * m", "n * m", static (n, m) => n * m),
        new("S03 (n + 3) * (m + 5)", "(n + 3) * (m + 5)", static (n, m) => (n + 3) * (m + 5)),
        new(
            "S04 ((n + 7) * (m + 11)) - ((n % 5) * 3)",
            "((n + 7) * (m + 11)) - ((n % 5) * 3)",
            static (n, m) => ((n + 7) * (m + 11)) - ((n % 5) * 3)),
        new(
            "S05 ((n * 3) + (m * 5) + 17) % 97",
            "((n * 3) + (m * 5) + 17) % 97",
            static (n, m) => ((n * 3) + (m * 5) + 17) % 97),
        new(
            "S06 (n - m)^2 + ((n + m) % 11)",
            "((n - m) * (n - m)) + ((n + m) % 11)",
            static (n, m) => ((n - m) * (n - m)) + ((n + m) % 11)),
        new(
            "S07 ((n + 1) * (m + 2)) + ((n + 3) * (m + 4))",
            "((n + 1) * (m + 2)) + ((n + 3) * (m + 4))",
            static (n, m) => ((n + 1) * (m + 2)) + ((n + 3) * (m + 4))),
        new(
            "S08 if(n > m, (n - m) * 3, (m - n) * 5)",
            "if(n > m, (n - m) * 3, (m - n) * 5)",
            static (n, m) => n > m ? (n - m) * 3 : (m - n) * 5),
        new(
            "S09 ((n + m) * (n + 3) * (m + 5)) % 997",
            "((n + m) * (n + 3) * (m + 5)) % 997",
            static (n, m) => ((n + m) * (n + 3) * (m + 5)) % 997)
    ];

    internal static IReadOnlyList<ArrayCase> ArrayCases { get; } =
    [
        new("A01 q1 + q2", "q1 + q2", static (context, index) => context.q1[index] + context.q2[index]),
        new("A02 q1 * q2", "q1 * q2", static (context, index) => context.q1[index] * context.q2[index]),
        new(
            "A03 (q1 * r1) + (q2 * r2) - (k1 + k2)",
            "(q1 * r1) + (q2 * r2) - (k1 + k2)",
            static (context, index) => (context.q1[index] * context.r1[index]) + (context.q2[index] * context.r2[index]) - (context.k1[index] + context.k2[index]))
    ];

    internal static CompilerContext CreateScalarContext()
    {
        return new CompilerContext();
    }

    internal static (long[] NValues, long[] MValues) CreateScalarInputValues()
    {
        var random = new Random(BenchmarkConstants.Seed);
        var nValues = new long[BenchmarkConstants.ScalarInputCount];
        var mValues = new long[BenchmarkConstants.ScalarInputCount];

        for (var i = 0; i < BenchmarkConstants.ScalarInputCount; i++)
        {
            nValues[i] = random.NextInt64(BenchmarkConstants.ScalarMinValue, BenchmarkConstants.ScalarMaxExclusive);
            mValues[i] = random.NextInt64(BenchmarkConstants.ScalarMinValue, BenchmarkConstants.ScalarMaxExclusive);
        }

        return (nValues, mValues);
    }

    internal static CompilerContext CreateArrayContext()
    {
        var random = new Random(BenchmarkConstants.Seed);
        var context = new CompilerContext
        {
            n = BenchmarkConstants.ArrayN
        };

        FillRandom(random, context.q1);
        FillRandom(random, context.q2);
        FillRandom(random, context.r1);
        FillRandom(random, context.r2);
        FillRandom(random, context.k1);
        FillRandom(random, context.k2);

        return context;
    }

    internal static FleeContext CreateScalarFleeContext()
    {
        var context = new FleeContext();
        context.Variables["n"] = 0L;
        context.Variables["m"] = 0L;
        return context;
    }

    internal static FleeContext CreateArrayFleeContext()
    {
        var context = new FleeContext();
        context.Variables["q1"] = 0d;
        context.Variables["q2"] = 0d;
        context.Variables["r1"] = 0d;
        context.Variables["r2"] = 0d;
        context.Variables["k1"] = 0d;
        context.Variables["k2"] = 0d;
        return context;
    }

    internal static long ToLong(object value)
    {
        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    internal static double ToDouble(object value)
    {
        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    internal static double Sum(double[] values, int length)
    {
        double total = 0;

        for (var i = 0; i < length; i++)
        {
            total += values[i];
        }

        return total;
    }

    private static void FillRandom(Random random, double[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = random.NextDouble() * 100d;
        }
    }
}
