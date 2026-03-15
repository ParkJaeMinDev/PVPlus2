using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PVPlus2.Models;
using PVPlus2.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PVPlus2.ViewModels;

public partial class TestViewModel : ObservableObject
{
    [ObservableProperty]
    private string _inputText = string.Join(Environment.NewLine, "  3 + 3  ", "1*3", "1+1", "1*3", "2+3", "3+3");

    [ObservableProperty]
    private string _outputText = string.Empty;

    [ObservableProperty]
    private int _arrayLength = 1000;
    private readonly Dictionary<string, Func<ExpressionContext, double>> _compiledNumericExpressions = new();
    private readonly Dictionary<string, Func<ExpressionContext, bool>> _compiledBoolExpressions = new();

    private const double ChecksumTolerance = 1e-9;

    private sealed record InvokeBenchmarkResult(
        double AverageLoopMicroseconds,
        double AverageCallNanoseconds,
        double Checksum);

    private sealed record BoolInvokeBenchmarkResult(
        double AverageLoopMicroseconds,
        double AverageCallNanoseconds,
        long TrueCountChecksum);

    private sealed record ChecksumComparisonResult(
        double CompilerChecksum,
        double NativeChecksum,
        double AbsoluteDifference,
        bool IsMatch);

    private sealed record BoolChecksumComparisonResult(
        long CompilerTrueCount,
        long NativeTrueCount,
        long AbsoluteDifference,
        bool IsMatch);

    private sealed record ExpectedErrorValidationResult(
        bool IsExpectedErrorObserved,
        string Outcome);

    [RelayCommand]
    private void RunTestParlot()
    {
        OutputText = "hello world";
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

        try
        {
            var numericExpressions = CreateVariableExpressions();
            var invalidNumericExpressions = CreateInvalidNumericExpressions();
            var boolExpressions = CreateBoolExpressions();
            var invalidBoolExpressions = CreateInvalidBoolExpressions();

            var allExpressions = new string[
                numericExpressions.Length
                + invalidNumericExpressions.Length
                + boolExpressions.Length
                + invalidBoolExpressions.Length];
            var offset = 0;
            Array.Copy(numericExpressions, 0, allExpressions, offset, numericExpressions.Length);
            offset += numericExpressions.Length;
            Array.Copy(invalidNumericExpressions, 0, allExpressions, offset, invalidNumericExpressions.Length);
            offset += invalidNumericExpressions.Length;
            Array.Copy(boolExpressions, 0, allExpressions, offset, boolExpressions.Length);
            offset += boolExpressions.Length;
            Array.Copy(invalidBoolExpressions, 0, allExpressions, offset, invalidBoolExpressions.Length);

            InputText = string.Join(Environment.NewLine, allExpressions);

            var sb = new StringBuilder();
            sb.AppendLine($"[TotalTest {DateTime.Now:HH:mm:ss.fff}]");
            sb.AppendLine("Numeric + Bool Edge Case Test");
            sb.AppendLine($"Valid Numeric Expressions: {numericExpressions.Length}");
            sb.AppendLine($"Invalid Numeric Expressions: {invalidNumericExpressions.Length}");
            sb.AppendLine($"Valid Bool Expressions: {boolExpressions.Length}");
            sb.AppendLine($"Invalid Bool Expressions: {invalidBoolExpressions.Length}");
            sb.AppendLine($"Total Expressions: {allExpressions.Length}");
            sb.AppendLine($"Array Length: {ArrayLength}");
            sb.AppendLine("Compile Benchmark: disabled (compile once)");
            sb.AppendLine();

            CompileExpressions(
                numericExpressions,
                ExpressionCompiler.CompileDouble,
                _compiledNumericExpressions);

            CompileExpressions(
                boolExpressions,
                ExpressionCompiler.CompileBool,
                _compiledBoolExpressions);

            var random = new Random(20260312);
            var xValues = CreateRandomArray(random, ArrayLength);
            var yValues = CreateRandomArray(random, ArrayLength);
            var nativeNumericExpressions = CreateNativeExpressions();
            var nativeBoolExpressions = CreateNativeBoolExpressions();

            var compilerNumericInvokeResults = BenchmarkInvocation(
                _compiledNumericExpressions,
                xValues,
                yValues);

            var nativeNumericInvokeResults = BenchmarkNativeInvocation(
                numericExpressions,
                nativeNumericExpressions,
                xValues,
                yValues);

            var numericChecksumResults = CompareChecksums(
                numericExpressions,
                compilerNumericInvokeResults,
                nativeNumericInvokeResults);

            var compilerBoolInvokeResults = BenchmarkBoolInvocation(
                _compiledBoolExpressions,
                xValues,
                yValues);

            var nativeBoolInvokeResults = BenchmarkNativeBoolInvocation(
                boolExpressions,
                nativeBoolExpressions,
                xValues,
                yValues);

            var boolChecksumResults = CompareBoolChecksums(
                boolExpressions,
                compilerBoolInvokeResults,
                nativeBoolInvokeResults);

            var invalidNumericResults = ValidateExpectedErrors(
                invalidNumericExpressions,
                ExpressionCompiler.CompileDouble);

            var invalidBoolResults = ValidateExpectedErrors(
                invalidBoolExpressions,
                ExpressionCompiler.CompileBool);

            sb.AppendLine("숫자 수식 | ExpressionCompiler 1회 평균 평가 시간(ns) | Native C# 1회 평균 평가 시간(ns)");
            sb.AppendLine(new string('-', 140));
            foreach (var expression in numericExpressions)
            {
                sb.AppendLine(
                    $"{expression} | {compilerNumericInvokeResults[expression].AverageCallNanoseconds:F3} | {nativeNumericInvokeResults[expression].AverageCallNanoseconds:F3}");
            }

            sb.AppendLine();
            sb.AppendLine("숫자 수식 | ExpressionCompiler Checksum | Native C# Checksum | Abs Diff | Match");
            sb.AppendLine(new string('-', 140));
            foreach (var expression in numericExpressions)
            {
                var checksum = numericChecksumResults[expression];
                sb.AppendLine(
                    $"{expression} | {checksum.CompilerChecksum:R} | {checksum.NativeChecksum:R} | {checksum.AbsoluteDifference:R} | {checksum.IsMatch}");
            }

            sb.AppendLine();
            sb.AppendLine("Bool 수식 | ExpressionCompiler 1회 평균 평가 시간(ns) | Native C# 1회 평균 평가 시간(ns)");
            sb.AppendLine(new string('-', 140));
            foreach (var expression in boolExpressions)
            {
                sb.AppendLine(
                    $"{expression} | {compilerBoolInvokeResults[expression].AverageCallNanoseconds:F3} | {nativeBoolInvokeResults[expression].AverageCallNanoseconds:F3}");
            }

            sb.AppendLine();
            sb.AppendLine("Bool 수식 | ExpressionCompiler TrueCount | Native C# TrueCount | Abs Diff | Match");
            sb.AppendLine(new string('-', 140));
            foreach (var expression in boolExpressions)
            {
                var checksum = boolChecksumResults[expression];
                sb.AppendLine(
                    $"{expression} | {checksum.CompilerTrueCount} | {checksum.NativeTrueCount} | {checksum.AbsoluteDifference} | {checksum.IsMatch}");
            }

            sb.AppendLine();
            sb.AppendLine("오류 예상 Numeric | Error Observed | Outcome");
            sb.AppendLine(new string('-', 140));
            foreach (var expression in invalidNumericExpressions)
            {
                var result = invalidNumericResults[expression];
                sb.AppendLine(
                    $"{expression} | {result.IsExpectedErrorObserved} | {result.Outcome}");
            }

            sb.AppendLine();
            sb.AppendLine("오류 예상 Boolean | Error Observed | Outcome");
            sb.AppendLine(new string('-', 140));
            foreach (var expression in invalidBoolExpressions)
            {
                var result = invalidBoolResults[expression];
                sb.AppendLine(
                    $"{expression} | {result.IsExpectedErrorObserved} | {result.Outcome}");
            }

            OutputText = sb.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            OutputText = FormatExceptionText(ex);
        }
    }

    private static void CompileExpressions<TDelegate>(
        IReadOnlyList<string> expressions,
        Func<string, TDelegate> compileFunc,
        Dictionary<string, TDelegate> target)
        where TDelegate : Delegate
    {
        target.Clear();

        foreach (var expression in expressions)
        {
            target[expression] = compileFunc(expression);
        }
    }

    private Dictionary<string, InvokeBenchmarkResult> BenchmarkInvocation(
        IReadOnlyDictionary<string, Func<ExpressionContext, double>> compiledExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, InvokeBenchmarkResult>();
        var operationCountPerLoop = xValues.Length;
        var repeatCount = 10;

        foreach (var pair in compiledExpressions)
        {
            var expression = pair.Key;
            var compiled = pair.Value;
            var context = new ExpressionContext();

            double blackhole = 0.0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < repeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    context.x = xValues[i];
                    context.y = yValues[i];
                    blackhole += compiled(context);
                }
            }
            sw.Stop();

            results[expression] = new InvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / repeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (repeatCount * operationCountPerLoop),
                blackhole);
        }

        return results;
    }

    private static Dictionary<string, ExpectedErrorValidationResult> ValidateExpectedErrors<T>(
        IReadOnlyList<string> expressions,
        Func<string, Func<ExpressionContext, T>> compileFunc)
    {
        var results = new Dictionary<string, ExpectedErrorValidationResult>();
        var context = new ExpressionContext
        {
            x = 12.5,
            y = 4.25
        };

        foreach (var expression in expressions)
        {
            try
            {
                var compiled = compileFunc(expression);
                _ = compiled(context);

                results[expression] = new ExpectedErrorValidationResult(
                    false,
                    "Unexpected success");
            }
            catch (Exception ex)
            {
                results[expression] = new ExpectedErrorValidationResult(
                    true,
                    $"{ex.GetType().Name}: {ex.Message}");
            }
        }

        return results;
    }

    private Dictionary<string, BoolInvokeBenchmarkResult> BenchmarkBoolInvocation(
        IReadOnlyDictionary<string, Func<ExpressionContext, bool>> compiledExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, BoolInvokeBenchmarkResult>();
        var operationCountPerLoop = xValues.Length;
        var repeatCount = 10;

        foreach (var pair in compiledExpressions)
        {
            var expression = pair.Key;
            var compiled = pair.Value;
            var context = new ExpressionContext();

            long trueCount = 0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < repeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    context.x = xValues[i];
                    context.y = yValues[i];

                    if (compiled(context))
                    {
                        trueCount++;
                    }
                }
            }
            sw.Stop();

            results[expression] = new BoolInvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / repeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (repeatCount * operationCountPerLoop),
                trueCount);
        }

        return results;
    }

    private Dictionary<string, InvokeBenchmarkResult> BenchmarkNativeInvocation(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, Func<double, double, double>> nativeExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, InvokeBenchmarkResult>();
        var operationCountPerLoop = xValues.Length;
        var repeatCount = 10;

        foreach (var expression in expressions)
        {
            var native = nativeExpressions[expression];
            double blackhole = 0.0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < repeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    blackhole += native(xValues[i], yValues[i]);
                }
            }
            sw.Stop();

            results[expression] = new InvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / repeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (repeatCount * operationCountPerLoop),
                blackhole);
        }

        return results;
    }

    private Dictionary<string, BoolInvokeBenchmarkResult> BenchmarkNativeBoolInvocation(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, Func<double, double, bool>> nativeExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, BoolInvokeBenchmarkResult>();
        var operationCountPerLoop = xValues.Length;
        var repeatCount = 10;

        foreach (var expression in expressions)
        {
            var native = nativeExpressions[expression];
            long trueCount = 0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < repeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    if (native(xValues[i], yValues[i]))
                    {
                        trueCount++;
                    }
                }
            }
            sw.Stop();

            results[expression] = new BoolInvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / repeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (repeatCount * operationCountPerLoop),
                trueCount);
        }

        return results;
    }

    private static Dictionary<string, ChecksumComparisonResult> CompareChecksums(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, InvokeBenchmarkResult> compilerResults,
        IReadOnlyDictionary<string, InvokeBenchmarkResult> nativeResults)
    {
        var results = new Dictionary<string, ChecksumComparisonResult>();

        foreach (var expression in expressions)
        {
            var compilerChecksum = compilerResults[expression].Checksum;
            var nativeChecksum = nativeResults[expression].Checksum;
            double absoluteDifference;
            bool isMatch;

            if (double.IsNaN(compilerChecksum) && double.IsNaN(nativeChecksum))
            {
                absoluteDifference = 0.0;
                isMatch = true;
            }
            else if (double.IsPositiveInfinity(compilerChecksum) && double.IsPositiveInfinity(nativeChecksum))
            {
                absoluteDifference = 0.0;
                isMatch = true;
            }
            else if (double.IsNegativeInfinity(compilerChecksum) && double.IsNegativeInfinity(nativeChecksum))
            {
                absoluteDifference = 0.0;
                isMatch = true;
            }
            else
            {
                absoluteDifference = Math.Abs(compilerChecksum - nativeChecksum);
                var tolerance = ChecksumTolerance * Math.Max(1.0, Math.Abs(nativeChecksum));
                isMatch = absoluteDifference <= tolerance;
            }

            results[expression] = new ChecksumComparisonResult(
                compilerChecksum,
                nativeChecksum,
                absoluteDifference,
                isMatch);
        }

        return results;
    }

    private static Dictionary<string, BoolChecksumComparisonResult> CompareBoolChecksums(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, BoolInvokeBenchmarkResult> compilerResults,
        IReadOnlyDictionary<string, BoolInvokeBenchmarkResult> nativeResults)
    {
        var results = new Dictionary<string, BoolChecksumComparisonResult>();

        foreach (var expression in expressions)
        {
            var compilerTrueCount = compilerResults[expression].TrueCountChecksum;
            var nativeTrueCount = nativeResults[expression].TrueCountChecksum;
            var absoluteDifference = Math.Abs(compilerTrueCount - nativeTrueCount);

            results[expression] = new BoolChecksumComparisonResult(
                compilerTrueCount,
                nativeTrueCount,
                absoluteDifference,
                absoluteDifference == 0);
        }

        return results;
    }

    private static Dictionary<string, Func<double, double, double>> CreateNativeExpressions()
    {
        return new Dictionary<string, Func<double, double, double>>
        {
            ["1 + 2 * 3"] = static (x, y) => 1 + 2 * 3,
            ["2 ^ 3 ^ 2"] = static (x, y) => Math.Pow(2, Math.Pow(3, 2)),
            ["(1 + 2) * (3 - 1)"] = static (x, y) => (1 + 2) * (3 - 1),
            ["10 / 4"] = static (x, y) => 10.0 / 4.0,
            ["-1 + 3"] = static (x, y) => -1 + 3,
            ["-2 ^ 2"] = static (x, y) => Math.Pow(-2, 2),
            ["0 / 0"] = static (x, y) => 0.0 / 0.0,
            ["10 + 20 * 3"] = static (x, y) => 10 + 20 * 3,
            ["10.5 / 2"] = static (x, y) => 10.5 / 2,
            ["-5 + 10"] = static (x, y) => -5 + 10,
            ["10 % 3"] = static (x, y) => 10 % 3,
            ["(1 + 2) * 3"] = static (x, y) => (1 + 2) * 3,
            [".5 * 2"] = static (x, y) => 0.5 * 2
        };
    }

    private static Dictionary<string, Func<double, double, bool>> CreateNativeBoolExpressions()
    {
        return new Dictionary<string, Func<double, double, bool>>
        {
            ["1 <> 2"] = static (x, y) => 1 != 2,
            ["NOT (1 == 2)"] = static (x, y) => !(1 == 2),
            ["TRUE AND NOT FALSE"] = static (x, y) => true && !false,
            ["1 + 2 == 3 AND 4 > 3"] = static (x, y) => 1 + 2 == 3 && 4 > 3,
            ["1 + 2 == 3.0"] = static (x, y) => 1 + 2 == 3.0,
            ["+True"] = static (x, y) => true,
            ["True"] = static (x, y) => true,
            ["False"] = static (x, y) => false,
            ["10 == 10.0"] = static (x, y) => 10 == 10.0,
            ["1 != 2"] = static (x, y) => 1 != 2,
            ["1 > 0 AND 2 < 5"] = static (x, y) => 1 > 0 && 2 < 5
        };
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

    private static string FormatExceptionText(Exception ex)
    {
        var sb = new StringBuilder();
        var current = ex;
        var depth = 0;

        while (current != null)
        {
            if (depth == 0)
            {
                sb.AppendLine("Benchmark failed.");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine($"Inner Exception {depth}:");
            }

            sb.AppendLine($"{current.GetType().FullName}: {current.Message}");

            if (!string.IsNullOrWhiteSpace(current.StackTrace))
            {
                sb.AppendLine();
                sb.AppendLine(current.StackTrace);
            }

            current = current.InnerException;
            depth++;
        }

        return sb.ToString().TrimEnd();
    }

    private static string[] CreateVariableExpressions()
    {
        return
        [
            "1 + 2 * 3",
            "2 ^ 3 ^ 2",
            "(1 + 2) * (3 - 1)",
            "10 / 4",
            "-1 + 3",
            "-2 ^ 2",
            "0 / 0",
            "10 + 20 * 3",
            "10.5 / 2",
            "-5 + 10",
            "10 % 3",
            "(1 + 2) * 3",
            ".5 * 2"
        ];
    }

    private static string[] CreateBoolExpressions()
    {
        return
        [
            "1 <> 2",
            "NOT (1 == 2)",
            "TRUE AND NOT FALSE",
            "1 + 2 == 3 AND 4 > 3",
            "1 + 2 == 3.0",
            "+True",
            "True",
            "False",
            "10 == 10.0",
            "1 != 2",
            "1 > 0 AND 2 < 5"
        ];
    }

    private static string[] CreateInvalidNumericExpressions()
    {
        return
        [
            "1e10",
            "1.2.3",
            "1 % 0",
            "1e-5",
            "1,000 + 2",
            "--1",
            "()",
            "1 / 0"
        ];
    }

    private static string[] CreateInvalidBoolExpressions()
    {
        return
        [
            "1 < 2 < 3",
            "-True",
            "True == 1",
            "NOT 1",
            "True > False",
            "1 AND 2"
        ];
    }

}
