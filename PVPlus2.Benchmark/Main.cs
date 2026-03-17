using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Flee.PublicTypes;
using PVPlus2.Services;
using System.Globalization;
using CompilerContext = PVPlus2.Models.ExpressionContext;
using FleeContext = Flee.PublicTypes.ExpressionContext;

BenchmarkSwitcher.FromTypes(new[]
{
    typeof(ArithmeticBenchmarks),
    typeof(StringBenchmarks),
    typeof(IfBenchmarks),
    typeof(IfsBenchmarks)
}).Run(args);

[MemoryDiagnoser]
public class ArithmeticBenchmarks : XYBenchmarkBase
{
    private const int InputCount = BenchmarkConstants.InputCount;

    [ParamsSource(nameof(Cases))]
    public ArithmeticCase Case { get; set; } = default!;

    public IEnumerable<ArithmeticCase> Cases => BenchmarkCases.Arithmetic;

    private Func<CompilerContext, double> _compiler = default!;
    private FleeContext _fleeContext = default!;
    private IDynamicExpression _flee = default!;
    private Func<double, double, double> _native = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        InitializeInputs();

        _compiler = ExpressionCompiler.CompileDouble(Case.Expression);
        _fleeContext = BenchmarkHelpers.CreateFleeContext();
        _flee = _fleeContext.CompileDynamic(Case.Expression);
        _native = Case.Native;
    }

    [Benchmark(OperationsPerInvoke = InputCount)]
    public double Arithmetic_ExpressionCompiler()
    {
        var context = CompilerContext;
        var evaluator = _compiler;
        var xValues = XValues;
        var yValues = YValues;
        var total = 0d;

        for (var i = 0; i < xValues.Length; i++)
        {
            context.x = xValues[i];
            context.y = yValues[i];
            total += evaluator(context);
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = InputCount)]
    public double Arithmetic_Flee()
    {
        var context = _fleeContext;
        var evaluator = _flee;
        var xValues = XValues;
        var yValues = YValues;
        var total = 0d;

        for (var i = 0; i < xValues.Length; i++)
        {
            context.Variables["x"] = xValues[i];
            context.Variables["y"] = yValues[i];
            total += BenchmarkHelpers.ToDouble(evaluator.Evaluate());
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = InputCount)]
    public double Arithmetic_Native()
    {
        var evaluator = _native;
        var xValues = XValues;
        var yValues = YValues;
        var total = 0d;

        for (var i = 0; i < xValues.Length; i++)
        {
            total += evaluator(xValues[i], yValues[i]);
        }

        return total;
    }
}

[MemoryDiagnoser]
public class StringBenchmarks : XYBenchmarkBase
{
    private const int InputCount = BenchmarkConstants.InputCount;

    [ParamsSource(nameof(Cases))]
    public StringCase Case { get; set; } = default!;

    public IEnumerable<StringCase> Cases => BenchmarkCases.String;

    private Func<CompilerContext, string> _compiler = default!;
    private FleeContext _fleeContext = default!;
    private IDynamicExpression _flee = default!;
    private Func<double, double, string> _native = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        InitializeInputs();

        _compiler = ExpressionCompiler.CompileString(Case.Expression);
        _fleeContext = BenchmarkHelpers.CreateFleeContext();
        _flee = _fleeContext.CompileDynamic(Case.Expression);
        _native = Case.Native;
    }

    [Benchmark(OperationsPerInvoke = InputCount)]
    public int String_ExpressionCompiler()
    {
        var context = CompilerContext;
        var evaluator = _compiler;
        var xValues = XValues;
        var yValues = YValues;
        var totalLength = 0;

        if (Case.UsesVariables)
        {
            for (var i = 0; i < xValues.Length; i++)
            {
                context.x = xValues[i];
                context.y = yValues[i];
                totalLength += evaluator(context).Length;
            }

            return totalLength;
        }

        for (var i = 0; i < InputCount; i++)
        {
            totalLength += evaluator(context).Length;
        }

        return totalLength;
    }

    [Benchmark(OperationsPerInvoke = InputCount)]
    public int String_Flee()
    {
        var context = _fleeContext;
        var evaluator = _flee;
        var xValues = XValues;
        var yValues = YValues;
        var totalLength = 0;

        if (Case.UsesVariables)
        {
            for (var i = 0; i < xValues.Length; i++)
            {
                context.Variables["x"] = xValues[i];
                context.Variables["y"] = yValues[i];
                totalLength += BenchmarkHelpers.ToStringValue(evaluator.Evaluate()).Length;
            }

            return totalLength;
        }

        for (var i = 0; i < InputCount; i++)
        {
            totalLength += BenchmarkHelpers.ToStringValue(evaluator.Evaluate()).Length;
        }

        return totalLength;
    }

    [Benchmark(OperationsPerInvoke = InputCount)]
    public int String_Native()
    {
        var evaluator = _native;
        var xValues = XValues;
        var yValues = YValues;
        var totalLength = 0;

        if (Case.UsesVariables)
        {
            for (var i = 0; i < xValues.Length; i++)
            {
                totalLength += evaluator(xValues[i], yValues[i]).Length;
            }

            return totalLength;
        }

        for (var i = 0; i < InputCount; i++)
        {
            totalLength += evaluator(0d, 0d).Length;
        }

        return totalLength;
    }
}

[MemoryDiagnoser]
public class IfBenchmarks : XYBenchmarkBase
{
    private const int InputCount = BenchmarkConstants.InputCount;

    [ParamsSource(nameof(Cases))]
    public ConditionalCase Case { get; set; } = default!;

    public IEnumerable<ConditionalCase> Cases => BenchmarkCases.If;

    private Func<CompilerContext, double> _compiler = default!;
    private FleeContext _fleeContext = default!;
    private IDynamicExpression _flee = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        InitializeInputs();

        _compiler = ExpressionCompiler.CompileDouble(Case.Expression);
        _fleeContext = BenchmarkHelpers.CreateFleeContext();
        _flee = _fleeContext.CompileDynamic(Case.Expression);
    }

    [Benchmark(OperationsPerInvoke = InputCount)]
    public double If_ExpressionCompiler()
    {
        var context = CompilerContext;
        var evaluator = _compiler;
        var xValues = XValues;
        var yValues = YValues;
        var total = 0d;

        for (var i = 0; i < xValues.Length; i++)
        {
            context.x = xValues[i];
            context.y = yValues[i];
            total += evaluator(context);
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = InputCount)]
    public double If_Flee()
    {
        var context = _fleeContext;
        var evaluator = _flee;
        var xValues = XValues;
        var yValues = YValues;
        var total = 0d;

        for (var i = 0; i < xValues.Length; i++)
        {
            context.Variables["x"] = xValues[i];
            context.Variables["y"] = yValues[i];
            total += BenchmarkHelpers.ToDouble(evaluator.Evaluate());
        }

        return total;
    }
}

[MemoryDiagnoser]
public class IfsBenchmarks : XYBenchmarkBase
{
    private const int InputCount = BenchmarkConstants.InputCount;

    [ParamsSource(nameof(Cases))]
    public ConditionalCase Case { get; set; } = default!;

    public IEnumerable<ConditionalCase> Cases => BenchmarkCases.Ifs;

    private Func<CompilerContext, double> _compiler = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        InitializeInputs();
        _compiler = ExpressionCompiler.CompileDouble(Case.Expression);
    }

    [Benchmark(OperationsPerInvoke = InputCount)]
    public double Ifs_ExpressionCompiler()
    {
        var context = CompilerContext;
        var evaluator = _compiler;
        var xValues = XValues;
        var yValues = YValues;
        var total = 0d;

        for (var i = 0; i < xValues.Length; i++)
        {
            context.x = xValues[i];
            context.y = yValues[i];
            total += evaluator(context);
        }

        return total;
    }
}

public abstract class XYBenchmarkBase
{
    protected readonly CompilerContext CompilerContext = new();

    protected double[] XValues { get; private set; } = Array.Empty<double>();

    protected double[] YValues { get; private set; } = Array.Empty<double>();

    protected void InitializeInputs()
    {
        (XValues, YValues) = BenchmarkHelpers.CreateInputValues();
    }
}

public sealed record ArithmeticCase(string Label, string Expression, Func<double, double, double> Native)
{
    public override string ToString() => Label;
}

public sealed record StringCase(
    string Label,
    string Expression,
    Func<double, double, string> Native,
    bool UsesVariables)
{
    public override string ToString() => Label;
}

public sealed record ConditionalCase(string Label, string Expression)
{
    public override string ToString() => Label;
}

internal static class BenchmarkConstants
{
    internal const int InputCount = 8192;
    internal const int Seed = 12345;
}

internal static class BenchmarkHelpers
{
    internal static (double[] XValues, double[] YValues) CreateInputValues()
    {
        var random = new Random(BenchmarkConstants.Seed);
        var xValues = new double[BenchmarkConstants.InputCount];
        var yValues = new double[BenchmarkConstants.InputCount];

        for (var i = 0; i < BenchmarkConstants.InputCount; i++)
        {
            xValues[i] = random.NextDouble() * 1000d;
            yValues[i] = random.NextDouble() * 1000d;
        }

        return (xValues, yValues);
    }

    internal static FleeContext CreateFleeContext()
    {
        var context = new FleeContext();
        context.Variables["x"] = 0d;
        context.Variables["y"] = 0d;
        return context;
    }

    internal static double ToDouble(object value)
    {
        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    internal static string ToStringValue(object? value)
    {
        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }
}

internal static class BenchmarkCases
{
    private static readonly string Blue = "blue";
    private static readonly string Ocean = "ocean";
    private static readonly string ValuePrefix = "value:";
    private static readonly string TailSuffix = "-tail";
    private static readonly string KoreanPrefix = "문자열:";
    private static readonly string KoreanValue = "벤치";

    private static readonly int FortyTwo = 42;
    private static readonly int SeventySeven = 77;

    internal static IReadOnlyList<ArithmeticCase> Arithmetic { get; } =
    [
        new("A01 x + y - 3", "x + y - 3", static (x, y) => x + y - 3d),
        new("A02 x * 2 + y - 7", "x * 2 + y - 7", static (x, y) => x * 2d + y - 7d),
        new("A03 (x + y) * 0.5", "(x + y) * 0.5", static (x, y) => (x + y) * 0.5d),
        new("A04 (x + 5) % 7 + y", "(x + 5) % 7 + y", static (x, y) => (x + 5d) % 7d + y),
        new("A05 x ^ 2 + y", "x ^ 2 + y", static (x, y) => Math.Pow(x, 2d) + y),
        new("A06 (x + y) % 11", "(x + y) % 11", static (x, y) => (x + y) % 11d),
        new("A07 (x * y) % 13", "(x * y) % 13", static (x, y) => (x * y) % 13d),
        new("A08 (x - y) ^ 2", "(x - y) ^ 2", static (x, y) => Math.Pow(x - y, 2d)),
        new("A09 x ^ 2 + y ^ 2", "x ^ 2 + y ^ 2", static (x, y) => Math.Pow(x, 2d) + Math.Pow(y, 2d)),
        new("A10 ((x + 3) * (y + 5)) % 17", "((x + 3) * (y + 5)) % 17", static (x, y) => ((x + 3d) * (y + 5d)) % 17d)
    ];

    internal static IReadOnlyList<StringCase> String { get; } =
    [
        new("S01 blue + ocean", "\"blue\" + \"ocean\"", static (x, y) => Blue + Ocean, UsesVariables: false),
        new("S02 value + 42", "\"value:\" + 42", static (x, y) => ValuePrefix + FortyTwo, UsesVariables: false),
        new("S03 77 + tail", "77 + \"-tail\"", static (x, y) => SeventySeven + TailSuffix, UsesVariables: false),
        new("S04 korean + korean", "\"문자열:\" + \"벤치\"", static (x, y) => KoreanPrefix + KoreanValue, UsesVariables: false),
        new("S05 x label", "\"x=\" + x", static (x, y) => "x=" + x, UsesVariables: true),
        new("S06 sum label", "\"sum:\" + (x + y)", static (x, y) => "sum:" + (x + y), UsesVariables: true),
        new("S07 cmp label", "\"cmp:\" + (x > y)", static (x, y) => "cmp:" + (x > y), UsesVariables: true),
        new("S08 left/right label", "\"left=\" + x + \",right=\" + y", static (x, y) => "left=" + x + ",right=" + y, UsesVariables: true),
        new("S09 pow label", "\"pow:\" + (x ^ 2)", static (x, y) => "pow:" + Math.Pow(x, 2d), UsesVariables: true),
        new("S10 rem label", "\"rem:\" + ((x + y) % 5)", static (x, y) => "rem:" + ((x + y) % 5d), UsesVariables: true)
    ];

    internal static IReadOnlyList<ConditionalCase> If { get; } =
    [
        new("IF01 x > y", "if(x > y, x + y, x - y)"),
        new("IF02 x <= y", "if(x <= y, y - x + 1, x - y + 1)"),
        new("IF03 string eq const", "if(\"A\" = \"A\", x + 10, y + 10)"),
        new("IF04 string concat eq", "if(\"좌\" + \"표\" = \"좌표\", x ^ 2, y ^ 2)"),
        new("IF05 concat compare", "if(\"x=\" + 1 = \"x=1\", (x + y) % 7, (x * y) % 7)")
    ];

    internal static IReadOnlyList<ConditionalCase> Ifs { get; } =
    [
        new("IFS01 numeric tier", "ifs(x > y + 100, x + y - 3, x > y, x - y + 3, y - x + 1)"),
        new("IFS02 mixed numeric", "ifs((x + y) % 2 > 1, x ^ 2, x > y, (x + y) % 9, y ^ 2)"),
        new("IFS03 string eq", "ifs(\"A\" = \"B\", x + 1, \"한글\" = \"한글\", y + 2, x + y + 3)"),
        new("IFS04 range split", "ifs(x < 100, x + 5, x < 500, y + 7, x - y + 9)"),
        new("IFS05 concat compare", "ifs(\"x=\" + 1 = \"x=2\", x * 2, x > y + 10, (x * y) % 17, x + y + 11)")
    ];
}
