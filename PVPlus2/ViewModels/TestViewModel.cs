using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PVPlus2.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PVPlus2.ViewModels;

public partial class TestViewModel : ObservableObject
{
    [ObservableProperty]
    private string _inputText = string.Join(Environment.NewLine, "3+3", "1*3", "1+1", "1*3", "2+3", "3+3");

    [ObservableProperty]
    private string _outputText = string.Empty;

    [ObservableProperty]
    private int _arrayLength = 1000;

    private readonly ExpressionCompiler _expressionCompiler = new();
    private readonly Dictionary<string, Func<double, double, double>> _compiledExpressions = new();
    private readonly Dictionary<string, Func<double[], double[], int, double>> _compiledWithLengthExpressions = new();

    private double _lastBlackhole;

    private sealed record CompileBenchmarkResult(double AverageCompileMicroseconds);

    private sealed record InvokeBenchmarkResult(
        double AverageLoopMicroseconds,
        double AverageCallNanoseconds,
        double Checksum);

    [RelayCommand]
    private void RunTestParlot()
    {
        var sb = new StringBuilder();

        var stopwatchWarmup = Stopwatch.StartNew();
        _ = _expressionCompiler.CompileTest("0");
        stopwatchWarmup.Stop();

        var compileMicroseconds123 = stopwatchWarmup.ElapsedTicks * 1_000_000.0 / Stopwatch.Frequency;
        sb.AppendLine($"Compile: {compileMicroseconds123:F3} us");

        _compiledExpressions.Clear();

        using var reader = new StringReader(InputText);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            var expressionText = line.Trim();

            if (string.IsNullOrWhiteSpace(expressionText))
            {
                continue;
            }

            stopwatchWarmup.Restart();
            var compiled = _expressionCompiler.CompileTest(expressionText);
            stopwatchWarmup.Stop();

            _compiledExpressions[expressionText] = compiled;

            var result = compiled(0, 0);
            var compileMicroseconds = stopwatchWarmup.ElapsedTicks * 1_000_000.0 / Stopwatch.Frequency;

            sb.AppendLine($"{expressionText} | Compile: {compileMicroseconds:F3} us | Result: {result}");
        }

        if (!string.IsNullOrWhiteSpace(OutputText))
        {
            OutputText += Environment.NewLine + Environment.NewLine;
        }

        OutputText += $"[{DateTime.Now:HH:mm:ss.fff}]" + Environment.NewLine;
        OutputText += sb.ToString().TrimEnd();
    }

    [RelayCommand]
    private void TotalTest()
    {
        OutputText = string.Empty;

        if (ArrayLength <= 0)
        {
            OutputText = "ArrayLength must be greater than 0.";
            return;
        }

        var scalarExpressions = CreateScalarExpressions();
        var variableExpressions = CreateVariableExpressions();

        var allExpressions = new string[scalarExpressions.Length + variableExpressions.Length];
        Array.Copy(scalarExpressions, allExpressions, scalarExpressions.Length);
        Array.Copy(variableExpressions, 0, allExpressions, scalarExpressions.Length, variableExpressions.Length);

        InputText = string.Join(Environment.NewLine, allExpressions);

        var sb = new StringBuilder();
        sb.AppendLine($"[TotalTest {DateTime.Now:HH:mm:ss.fff}]");
        sb.AppendLine($"Scalar Expressions: {scalarExpressions.Length}");
        sb.AppendLine($"Variable Expressions: {variableExpressions.Length}");
        sb.AppendLine($"Total Expressions: {allExpressions.Length}");
        sb.AppendLine($"Array Length: {ArrayLength}");
        sb.AppendLine();

        var compileResults = BenchmarkCompiler(
            allExpressions,
            _expressionCompiler.CompileTest,
            _compiledExpressions);

        var compileWithLengthResults = BenchmarkCompiler(
            allExpressions,
            _expressionCompiler.CompileWithLengthTest,
            _compiledWithLengthExpressions);

        var random = new Random(20260312);
        var xValues = CreateRandomArray(random, ArrayLength);
        var yValues = CreateRandomArray(random, ArrayLength);

        var invokeResults = BenchmarkInvocation(_compiledExpressions, xValues, yValues);
        var invokeWithLengthResults = BenchmarkInvocationWithLength(_compiledWithLengthExpressions, xValues, yValues);

        sb.AppendLine("수식 | 일반컴파일 평균 1회 컴파일 비용(us) | for문포함 컴파일 평균 1회 컴파일 비용(us)");
        sb.AppendLine(new string('-', 120));
        foreach (var expression in allExpressions)
        {
            sb.AppendLine(
                $"{expression} | {compileResults[expression].AverageCompileMicroseconds:F3} | {compileWithLengthResults[expression].AverageCompileMicroseconds:F3}");
        }

        sb.AppendLine();
        sb.AppendLine($"수식 | 일반 컴파일 1회 평균 평가 시간(ns) | for문포함 컴파일 1회 평균 평가 시간(ns)");
        sb.AppendLine(new string('-', 120));
        foreach (var expression in allExpressions)
        {
            sb.AppendLine(
                $"{expression} | {invokeResults[expression].AverageCallNanoseconds:F3} | {invokeWithLengthResults[expression].AverageCallNanoseconds:F3}");
        }

        OutputText = sb.ToString().TrimEnd();
    }

    private Dictionary<string, CompileBenchmarkResult> BenchmarkCompiler<TDelegate>(
        IReadOnlyList<string> expressions,
        Func<string, TDelegate> compileFunc,
        Dictionary<string, TDelegate> target)
        where TDelegate : Delegate
    {
        var results = new Dictionary<string, CompileBenchmarkResult>();

        target.Clear();

        foreach (var expression in expressions)
        {
            _ = compileFunc(expression);
        }

        foreach (var expression in expressions)
        {
            TDelegate compiled = null!;

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 100; i++)
            {
                compiled = compileFunc(expression);
            }
            sw.Stop();

            target[expression] = compiled;
            results[expression] = new CompileBenchmarkResult(ToMicroseconds(sw.ElapsedTicks) / 100.0);
        }

        return results;
    }

    private Dictionary<string, InvokeBenchmarkResult> BenchmarkInvocation(
        IReadOnlyDictionary<string, Func<double, double, double>> compiledExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, InvokeBenchmarkResult>();
        var operationCountPerLoop = xValues.Length;
        var repeatCount = 1000;

        foreach (var pair in compiledExpressions)
        {
            var expression = pair.Key;
            var compiled = pair.Value;

            double blackhole = 0.0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < repeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    blackhole += compiled(xValues[i], yValues[i]);
                }
            }
            sw.Stop();

            _lastBlackhole = blackhole;

            results[expression] = new InvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / repeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (repeatCount * operationCountPerLoop),
                blackhole);
        }

        return results;
    }

    private Dictionary<string, InvokeBenchmarkResult> BenchmarkInvocationWithLength(
        IReadOnlyDictionary<string, Func<double[], double[], int, double>> compiledExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, InvokeBenchmarkResult>();
        var operationCountPerLoop = xValues.Length;
        var repeatCount = 1000;

        foreach (var pair in compiledExpressions)
        {
            var expression = pair.Key;
            var compiled = pair.Value;

            double blackhole = 0.0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < repeatCount; repeat++)
            {
                blackhole += compiled(xValues, yValues, operationCountPerLoop);
            }
            sw.Stop();

            _lastBlackhole = blackhole;

            results[expression] = new InvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / repeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (repeatCount * operationCountPerLoop),
                blackhole);
        }

        return results;
    }

    private static double[] CreateRandomArray(Random random, int length)
    {
        var values = new double[length];

        for (var i = 0; i < length; i++)
        {
            values[i] = random.NextDouble() * 100.0;
        }

        return values;
    }

    private static double ToMicroseconds(long ticks)
    {
        return ticks * 1_000_000.0 / Stopwatch.Frequency;
    }

    private static double ToNanoseconds(long ticks)
    {
        return ticks * 1_000_000_000.0 / Stopwatch.Frequency;
    }

    private static string[] CreateScalarExpressions()
    {
        return
        [
            "1+1",
            "2*3",
            "10/2+7",
            "(3+4)*5",
            "12-3*2+8",
            "(18/3)+(7*4)-5",
            "((2+3)*4)-(6/2)+9",
            "1+2+3+4+5+6+7+8",
            "2*3*4*5/6+7-8",
            "(1+2)*(3+4)-(5+6)",
            "100/5+40/2-3*7+9",
            "((8+2)*(3+7))/5+11",
            "1+2+3+4+5+6+7+8+9+10",
            "(2*3)+(4*5)+(6*7)+(8*9)",
            "((1+2+3+4+5)*6-7+8)/3",
            "12+34-56+78-90+12-34+56-78+90",
            "((3*5)+(7*11)-(13/2)+(17*19))/4",
            "1*2+3*4+5*6+7*8+9*10+11*12+13*14",
            "(1+3+5+7+9+11+13+15+17+19)*(2+4+6)/7",
            "1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17+18+19+20+21+22+23+24+25+26+27+28+29+30+31+32"
        ];
    }

    private static string[] CreateVariableExpressions()
    {
        return
        [
            "x+y",
            "x*2+y",
            "x+y*3",
            "(x+y)*2",
            "x*x+y",
            "x+y+y",
            "(x*3)-(y/2)",
            "(x+y)*(x-y)",
            "x*x+y*y",
            "(x+2)*(y+3)",
            "(x*4)+(y*5)-6",
            "((x+y)/2)+(x*3)-(y*4)",
            "(x*x)+(2*x*y)+(y*y)",
            "(x+1)+(y+2)+(x+3)+(y+4)",
            "((x*2)+(y*3)+(x*4)+(y*5))/2",
            "(x+y+x+y+x+y+x+y+x+y)",
            "((x*10)-(y*5)+(x*4)-(y*2)+(x+y))/3",
            "(x*x*x)/(y+1)",
            "((x+y)*(x+y)*(x-y))/5",
            "(x+y)+(x*2)+(y*3)+(x*4)+(y*5)+(x*6)+(y*7)+(x*8)+(y*9)+(x*10)+(y*11)+(x*12)+(y*13)+(x*14)+(y*15)+(x*16)+(y*17)+(x*18)+(y*19)+(x*20)"
        ];
    }
}
