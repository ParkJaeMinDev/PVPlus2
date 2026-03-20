using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PVPlus2.Models;
using PVPlus2.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PVPlus2.ViewModels;

public partial class TestViewModel : ObservableObject
{
    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private string _outputText = string.Empty;

    [ObservableProperty]
    private int _arrayLength = 1000;

    private readonly Dictionary<string, Func<CommutationTable, double>> _compiledDoubleExpressions = new();
    private readonly Dictionary<string, Func<CommutationTable, long>> _compiledLongExpressions = new();
    private readonly Dictionary<string, Func<CommutationTable, bool>> _compiledBoolExpressions = new();
    private readonly Dictionary<string, Func<CommutationTable, string>> _compiledStringExpressions = new();

    private const double ChecksumTolerance = 1e-9;
    private const int CompileRepeatCount = 100;
    private const int EvaluationRepeatCount = 10;
    private const ulong FnvOffsetBasis = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;

    private sealed record TimingBenchmarkResult(
        double AverageLoopMicroseconds,
        double AverageOperationNanoseconds);

    private sealed record DoubleInvokeBenchmarkResult(
        double AverageLoopMicroseconds,
        double AverageCallNanoseconds,
        double Checksum);

    private sealed record LongInvokeBenchmarkResult(
        double AverageLoopMicroseconds,
        double AverageCallNanoseconds,
        long Checksum);

    private sealed record BoolInvokeBenchmarkResult(
        double AverageLoopMicroseconds,
        double AverageCallNanoseconds,
        long TrueCountChecksum);

    private sealed record StringInvokeBenchmarkResult(
        double AverageLoopMicroseconds,
        double AverageCallNanoseconds,
        ulong HashChecksum);

    private sealed record DoubleChecksumComparisonResult(
        double CompilerChecksum,
        double NativeChecksum,
        double AbsoluteDifference,
        bool IsMatch);

    private sealed record LongChecksumComparisonResult(
        long CompilerChecksum,
        long NativeChecksum,
        long AbsoluteDifference,
        bool IsMatch);

    private sealed record BoolChecksumComparisonResult(
        long CompilerTrueCount,
        long NativeTrueCount,
        long AbsoluteDifference,
        bool IsMatch);

    private sealed record StringChecksumComparisonResult(
        ulong CompilerHash,
        ulong NativeHash,
        long MismatchCount,
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
            var nativeDoubleExpressions = CreateNativeDoubleExpressions();
            var nativeLongExpressions = CreateNativeLongExpressions();
            var nativeBoolExpressions = CreateNativeBoolExpressions();
            var nativeStringExpressions = CreateNativeStringExpressions();

            var validDoubleExpressions = nativeDoubleExpressions.Keys.ToArray();
            var validLongExpressions = nativeLongExpressions.Keys.ToArray();
            var validBoolExpressions = nativeBoolExpressions.Keys.ToArray();
            var validStringExpressions = nativeStringExpressions.Keys.ToArray();

            var invalidDoubleExpressions = CreateInvalidDoubleExpressions();
            var invalidLongExpressions = CreateInvalidLongExpressions();
            var invalidBoolExpressions = CreateInvalidBoolExpressions();
            var invalidStringExpressions = CreateInvalidStringExpressions();

            InputText = BuildInputText(
                validDoubleExpressions,
                invalidDoubleExpressions,
                validLongExpressions,
                invalidLongExpressions,
                validBoolExpressions,
                invalidBoolExpressions,
                validStringExpressions,
                invalidStringExpressions);

            var doubleCompileResults = BenchmarkCompilation(
                validDoubleExpressions,
                ExpressionCompiler.CompileDouble,
                _compiledDoubleExpressions);

            var longCompileResults = BenchmarkCompilation(
                validLongExpressions,
                ExpressionCompiler.CompileLong,
                _compiledLongExpressions);

            var boolCompileResults = BenchmarkCompilation(
                validBoolExpressions,
                ExpressionCompiler.CompileBool,
                _compiledBoolExpressions);

            var stringCompileResults = BenchmarkCompilation(
                validStringExpressions,
                ExpressionCompiler.CompileString,
                _compiledStringExpressions);

            var random = new Random(20260316);
            var xValues = CreateRandomArray(random, ArrayLength);
            var yValues = CreateRandomArray(random, ArrayLength);

            var compilerDoubleResults = BenchmarkCompiledDoubleInvocation(
                _compiledDoubleExpressions,
                xValues,
                yValues);

            var nativeDoubleResults = BenchmarkNativeDoubleInvocation(
                validDoubleExpressions,
                nativeDoubleExpressions,
                xValues,
                yValues);

            var doubleChecksumResults = CompareDoubleChecksums(
                validDoubleExpressions,
                compilerDoubleResults,
                nativeDoubleResults);

            var compilerLongResults = BenchmarkCompiledLongInvocation(
                _compiledLongExpressions,
                xValues,
                yValues);

            var nativeLongResults = BenchmarkNativeLongInvocation(
                validLongExpressions,
                nativeLongExpressions,
                xValues,
                yValues);

            var longChecksumResults = CompareLongChecksums(
                validLongExpressions,
                compilerLongResults,
                nativeLongResults);

            var compilerBoolResults = BenchmarkCompiledBoolInvocation(
                _compiledBoolExpressions,
                xValues,
                yValues);

            var nativeBoolResults = BenchmarkNativeBoolInvocation(
                validBoolExpressions,
                nativeBoolExpressions,
                xValues,
                yValues);

            var boolChecksumResults = CompareBoolChecksums(
                validBoolExpressions,
                compilerBoolResults,
                nativeBoolResults);

            var compilerStringResults = BenchmarkCompiledStringInvocation(
                _compiledStringExpressions,
                xValues,
                yValues);

            var nativeStringResults = BenchmarkNativeStringInvocation(
                validStringExpressions,
                nativeStringExpressions,
                xValues,
                yValues);

            var stringChecksumResults = CompareStringChecksums(
                validStringExpressions,
                _compiledStringExpressions,
                nativeStringExpressions,
                xValues,
                yValues);

            var invalidDoubleResults = ValidateExpectedErrors(
                invalidDoubleExpressions,
                ExpressionCompiler.CompileDouble);

            var invalidLongResults = ValidateExpectedErrors(
                invalidLongExpressions,
                ExpressionCompiler.CompileLong);

            var invalidBoolResults = ValidateExpectedErrors(
                invalidBoolExpressions,
                ExpressionCompiler.CompileBool);

            var invalidStringResults = ValidateExpectedErrors(
                invalidStringExpressions,
                ExpressionCompiler.CompileString);

            var sb = new StringBuilder();

            AppendSummary(
                sb,
                validDoubleExpressions.Length,
                invalidDoubleExpressions.Length,
                validLongExpressions.Length,
                invalidLongExpressions.Length,
                validBoolExpressions.Length,
                invalidBoolExpressions.Length,
                validStringExpressions.Length,
                invalidStringExpressions.Length);

            AppendCompileSection(sb, "Double Compile", validDoubleExpressions, doubleCompileResults);
            AppendEvaluationSection(
                sb,
                "Double Eval",
                validDoubleExpressions,
                compilerDoubleResults,
                nativeDoubleResults,
                static result => result.AverageLoopMicroseconds,
                static result => result.AverageCallNanoseconds,
                static result => result.AverageLoopMicroseconds,
                static result => result.AverageCallNanoseconds);
            AppendDoubleChecksumSection(sb, "Double Checksum", validDoubleExpressions, doubleChecksumResults);
            AppendErrorSection(sb, "Double Invalid", invalidDoubleExpressions, invalidDoubleResults);

            AppendCompileSection(sb, "Long Compile", validLongExpressions, longCompileResults);
            AppendEvaluationSection(
                sb,
                "Long Eval",
                validLongExpressions,
                compilerLongResults,
                nativeLongResults,
                static result => result.AverageLoopMicroseconds,
                static result => result.AverageCallNanoseconds,
                static result => result.AverageLoopMicroseconds,
                static result => result.AverageCallNanoseconds);
            AppendLongChecksumSection(sb, "Long Checksum", validLongExpressions, longChecksumResults);
            AppendErrorSection(sb, "Long Invalid", invalidLongExpressions, invalidLongResults);

            AppendCompileSection(sb, "Bool Compile", validBoolExpressions, boolCompileResults);
            AppendEvaluationSection(
                sb,
                "Bool Eval",
                validBoolExpressions,
                compilerBoolResults,
                nativeBoolResults,
                static result => result.AverageLoopMicroseconds,
                static result => result.AverageCallNanoseconds,
                static result => result.AverageLoopMicroseconds,
                static result => result.AverageCallNanoseconds);
            AppendBoolChecksumSection(sb, "Bool Checksum", validBoolExpressions, boolChecksumResults);
            AppendErrorSection(sb, "Bool Invalid", invalidBoolExpressions, invalidBoolResults);

            AppendCompileSection(sb, "String Compile", validStringExpressions, stringCompileResults);
            AppendEvaluationSection(
                sb,
                "String Eval",
                validStringExpressions,
                compilerStringResults,
                nativeStringResults,
                static result => result.AverageLoopMicroseconds,
                static result => result.AverageCallNanoseconds,
                static result => result.AverageLoopMicroseconds,
                static result => result.AverageCallNanoseconds);
            AppendStringChecksumSection(sb, "String Checksum", validStringExpressions, stringChecksumResults);
            AppendErrorSection(sb, "String Invalid", invalidStringExpressions, invalidStringResults);

            OutputText = sb.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            OutputText = FormatExceptionText(ex);
        }
    }

    private static Dictionary<string, TimingBenchmarkResult> BenchmarkCompilation<TDelegate>(
        IReadOnlyList<string> expressions,
        Func<string, TDelegate> compileFunc,
        Dictionary<string, TDelegate> target)
        where TDelegate : Delegate
    {
        target.Clear();

        var results = new Dictionary<string, TimingBenchmarkResult>(expressions.Count);

        foreach (var expression in expressions)
        {
            TDelegate? compiled = null;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < CompileRepeatCount; repeat++)
            {
                compiled = compileFunc(expression);
            }
            sw.Stop();

            target[expression] = compiled!;
            results[expression] = new TimingBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / CompileRepeatCount,
                ToNanoseconds(sw.ElapsedTicks) / CompileRepeatCount);
        }

        return results;
    }

    private static Dictionary<string, DoubleInvokeBenchmarkResult> BenchmarkCompiledDoubleInvocation(
        IReadOnlyDictionary<string, Func<CommutationTable, double>> compiledExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, DoubleInvokeBenchmarkResult>(compiledExpressions.Count);
        var operationCountPerLoop = xValues.Length;

        foreach (var pair in compiledExpressions)
        {
            var context = new CommutationTable();
            double checksum = 0.0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < EvaluationRepeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    context.x = xValues[i];
                    context.y = yValues[i];
                    checksum += pair.Value(context);
                }
            }
            sw.Stop();

            results[pair.Key] = new DoubleInvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / EvaluationRepeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (EvaluationRepeatCount * operationCountPerLoop),
                checksum);
        }

        return results;
    }

    private static Dictionary<string, DoubleInvokeBenchmarkResult> BenchmarkNativeDoubleInvocation(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, Func<double, double, double>> nativeExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, DoubleInvokeBenchmarkResult>(expressions.Count);
        var operationCountPerLoop = xValues.Length;

        foreach (var expression in expressions)
        {
            var native = nativeExpressions[expression];
            double checksum = 0.0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < EvaluationRepeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    checksum += native(xValues[i], yValues[i]);
                }
            }
            sw.Stop();

            results[expression] = new DoubleInvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / EvaluationRepeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (EvaluationRepeatCount * operationCountPerLoop),
                checksum);
        }

        return results;
    }

    private static Dictionary<string, LongInvokeBenchmarkResult> BenchmarkCompiledLongInvocation(
        IReadOnlyDictionary<string, Func<CommutationTable, long>> compiledExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, LongInvokeBenchmarkResult>(compiledExpressions.Count);
        var operationCountPerLoop = xValues.Length;

        foreach (var pair in compiledExpressions)
        {
            var context = new CommutationTable();
            long checksum = 0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < EvaluationRepeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    context.x = xValues[i];
                    context.y = yValues[i];
                    checksum += pair.Value(context);
                }
            }
            sw.Stop();

            results[pair.Key] = new LongInvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / EvaluationRepeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (EvaluationRepeatCount * operationCountPerLoop),
                checksum);
        }

        return results;
    }

    private static Dictionary<string, LongInvokeBenchmarkResult> BenchmarkNativeLongInvocation(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, Func<double, double, long>> nativeExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, LongInvokeBenchmarkResult>(expressions.Count);
        var operationCountPerLoop = xValues.Length;

        foreach (var expression in expressions)
        {
            var native = nativeExpressions[expression];
            long checksum = 0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < EvaluationRepeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    checksum += native(xValues[i], yValues[i]);
                }
            }
            sw.Stop();

            results[expression] = new LongInvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / EvaluationRepeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (EvaluationRepeatCount * operationCountPerLoop),
                checksum);
        }

        return results;
    }

    private static Dictionary<string, BoolInvokeBenchmarkResult> BenchmarkCompiledBoolInvocation(
        IReadOnlyDictionary<string, Func<CommutationTable, bool>> compiledExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, BoolInvokeBenchmarkResult>(compiledExpressions.Count);
        var operationCountPerLoop = xValues.Length;

        foreach (var pair in compiledExpressions)
        {
            var context = new CommutationTable();
            long trueCount = 0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < EvaluationRepeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    context.x = xValues[i];
                    context.y = yValues[i];

                    if (pair.Value(context))
                    {
                        trueCount++;
                    }
                }
            }
            sw.Stop();

            results[pair.Key] = new BoolInvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / EvaluationRepeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (EvaluationRepeatCount * operationCountPerLoop),
                trueCount);
        }

        return results;
    }

    private static Dictionary<string, BoolInvokeBenchmarkResult> BenchmarkNativeBoolInvocation(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, Func<double, double, bool>> nativeExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, BoolInvokeBenchmarkResult>(expressions.Count);
        var operationCountPerLoop = xValues.Length;

        foreach (var expression in expressions)
        {
            var native = nativeExpressions[expression];
            long trueCount = 0;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < EvaluationRepeatCount; repeat++)
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
                ToMicroseconds(sw.ElapsedTicks) / EvaluationRepeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (EvaluationRepeatCount * operationCountPerLoop),
                trueCount);
        }

        return results;
    }

    private static Dictionary<string, StringInvokeBenchmarkResult> BenchmarkCompiledStringInvocation(
        IReadOnlyDictionary<string, Func<CommutationTable, string>> compiledExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, StringInvokeBenchmarkResult>(compiledExpressions.Count);
        var operationCountPerLoop = xValues.Length;

        foreach (var pair in compiledExpressions)
        {
            var context = new CommutationTable();
            var hash = FnvOffsetBasis;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < EvaluationRepeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    context.x = xValues[i];
                    context.y = yValues[i];
                    hash = AppendStringHash(hash, pair.Value(context));
                }
            }
            sw.Stop();

            results[pair.Key] = new StringInvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / EvaluationRepeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (EvaluationRepeatCount * operationCountPerLoop),
                hash);
        }

        return results;
    }

    private static Dictionary<string, StringInvokeBenchmarkResult> BenchmarkNativeStringInvocation(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, Func<double, double, string>> nativeExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, StringInvokeBenchmarkResult>(expressions.Count);
        var operationCountPerLoop = xValues.Length;

        foreach (var expression in expressions)
        {
            var native = nativeExpressions[expression];
            var hash = FnvOffsetBasis;

            var sw = Stopwatch.StartNew();
            for (var repeat = 0; repeat < EvaluationRepeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    hash = AppendStringHash(hash, native(xValues[i], yValues[i]));
                }
            }
            sw.Stop();

            results[expression] = new StringInvokeBenchmarkResult(
                ToMicroseconds(sw.ElapsedTicks) / EvaluationRepeatCount,
                ToNanoseconds(sw.ElapsedTicks) / (EvaluationRepeatCount * operationCountPerLoop),
                hash);
        }

        return results;
    }

    private static Dictionary<string, DoubleChecksumComparisonResult> CompareDoubleChecksums(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, DoubleInvokeBenchmarkResult> compilerResults,
        IReadOnlyDictionary<string, DoubleInvokeBenchmarkResult> nativeResults)
    {
        var results = new Dictionary<string, DoubleChecksumComparisonResult>(expressions.Count);

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

            results[expression] = new DoubleChecksumComparisonResult(
                compilerChecksum,
                nativeChecksum,
                absoluteDifference,
                isMatch);
        }

        return results;
    }

    private static Dictionary<string, LongChecksumComparisonResult> CompareLongChecksums(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, LongInvokeBenchmarkResult> compilerResults,
        IReadOnlyDictionary<string, LongInvokeBenchmarkResult> nativeResults)
    {
        var results = new Dictionary<string, LongChecksumComparisonResult>(expressions.Count);

        foreach (var expression in expressions)
        {
            var compilerChecksum = compilerResults[expression].Checksum;
            var nativeChecksum = nativeResults[expression].Checksum;
            var absoluteDifference = Math.Abs(compilerChecksum - nativeChecksum);

            results[expression] = new LongChecksumComparisonResult(
                compilerChecksum,
                nativeChecksum,
                absoluteDifference,
                absoluteDifference == 0);
        }

        return results;
    }

    private static Dictionary<string, BoolChecksumComparisonResult> CompareBoolChecksums(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, BoolInvokeBenchmarkResult> compilerResults,
        IReadOnlyDictionary<string, BoolInvokeBenchmarkResult> nativeResults)
    {
        var results = new Dictionary<string, BoolChecksumComparisonResult>(expressions.Count);

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

    private static Dictionary<string, StringChecksumComparisonResult> CompareStringChecksums(
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, Func<CommutationTable, string>> compiledExpressions,
        IReadOnlyDictionary<string, Func<double, double, string>> nativeExpressions,
        double[] xValues,
        double[] yValues)
    {
        var results = new Dictionary<string, StringChecksumComparisonResult>(expressions.Count);
        var operationCountPerLoop = xValues.Length;

        foreach (var expression in expressions)
        {
            var compiled = compiledExpressions[expression];
            var native = nativeExpressions[expression];
            var context = new CommutationTable();
            var compilerHash = FnvOffsetBasis;
            var nativeHash = FnvOffsetBasis;
            long mismatchCount = 0;

            for (var repeat = 0; repeat < EvaluationRepeatCount; repeat++)
            {
                for (var i = 0; i < operationCountPerLoop; i++)
                {
                    context.x = xValues[i];
                    context.y = yValues[i];

                    var compilerValue = compiled(context);
                    var nativeValue = native(xValues[i], yValues[i]);

                    compilerHash = AppendStringHash(compilerHash, compilerValue);
                    nativeHash = AppendStringHash(nativeHash, nativeValue);

                    if (!string.Equals(compilerValue, nativeValue, StringComparison.Ordinal))
                    {
                        mismatchCount++;
                    }
                }
            }

            results[expression] = new StringChecksumComparisonResult(
                compilerHash,
                nativeHash,
                mismatchCount,
                mismatchCount == 0 && compilerHash == nativeHash);
        }

        return results;
    }

    private static Dictionary<string, ExpectedErrorValidationResult> ValidateExpectedErrors<T>(
        IReadOnlyList<string> expressions,
        Func<string, Func<CommutationTable, T>> compileFunc)
    {
        var results = new Dictionary<string, ExpectedErrorValidationResult>(expressions.Count);
        var context = new CommutationTable
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

    private void AppendSummary(
        StringBuilder sb,
        int validDoubleCount,
        int invalidDoubleCount,
        int validLongCount,
        int invalidLongCount,
        int validBoolCount,
        int invalidBoolCount,
        int validStringCount,
        int invalidStringCount)
    {
        var totalExpressions =
            validDoubleCount + invalidDoubleCount +
            validLongCount + invalidLongCount +
            validBoolCount + invalidBoolCount +
            validStringCount + invalidStringCount;

        sb.AppendLine($"[TotalTest {DateTime.Now:HH:mm:ss.fff}]");
        sb.AppendLine("Typed ExpressionCompiler regression benchmark");
        sb.AppendLine($"Valid Double: {validDoubleCount}");
        sb.AppendLine($"Invalid Double: {invalidDoubleCount}");
        sb.AppendLine($"Valid Long: {validLongCount}");
        sb.AppendLine($"Invalid Long: {invalidLongCount}");
        sb.AppendLine($"Valid Bool: {validBoolCount}");
        sb.AppendLine($"Invalid Bool: {invalidBoolCount}");
        sb.AppendLine($"Valid String: {validStringCount}");
        sb.AppendLine($"Invalid String: {invalidStringCount}");
        sb.AppendLine($"Total Expressions: {totalExpressions}");
        sb.AppendLine($"ArrayLength: {ArrayLength}");
        sb.AppendLine($"Compile Repeat Count: {CompileRepeatCount}");
        sb.AppendLine($"Eval Repeat Count: {EvaluationRepeatCount}");
        sb.AppendLine();
    }

    private static void AppendCompileSection(
        StringBuilder sb,
        string title,
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, TimingBenchmarkResult> results)
    {
        sb.AppendLine(title);
        sb.AppendLine("Expression | Avg Compile Loop (us) | Avg Compile (ns)");
        sb.AppendLine(new string('-', 140));

        foreach (var expression in expressions)
        {
            var result = results[expression];
            sb.AppendLine(
                $"{expression} | {result.AverageLoopMicroseconds:F3} | {result.AverageOperationNanoseconds:F3}");
        }

        sb.AppendLine();
    }

    private static void AppendEvaluationSection<TCompilerResult, TNativeResult>(
        StringBuilder sb,
        string title,
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, TCompilerResult> compilerResults,
        IReadOnlyDictionary<string, TNativeResult> nativeResults,
        Func<TCompilerResult, double> compilerLoopSelector,
        Func<TCompilerResult, double> compilerCallSelector,
        Func<TNativeResult, double> nativeLoopSelector,
        Func<TNativeResult, double> nativeCallSelector)
    {
        sb.AppendLine(title);
        sb.AppendLine("Expression | Compiler Loop (us) | Compiler Call (ns) | Native Loop (us) | Native Call (ns)");
        sb.AppendLine(new string('-', 140));

        foreach (var expression in expressions)
        {
            var compiler = compilerResults[expression];
            var native = nativeResults[expression];
            sb.AppendLine(
                $"{expression} | {compilerLoopSelector(compiler):F3} | {compilerCallSelector(compiler):F3} | {nativeLoopSelector(native):F3} | {nativeCallSelector(native):F3}");
        }

        sb.AppendLine();
    }

    private static void AppendDoubleChecksumSection(
        StringBuilder sb,
        string title,
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, DoubleChecksumComparisonResult> results)
    {
        sb.AppendLine(title);
        sb.AppendLine("Expression | Compiler Checksum | Native Checksum | Abs Diff | Match");
        sb.AppendLine(new string('-', 140));

        foreach (var expression in expressions)
        {
            var result = results[expression];
            sb.AppendLine(
                $"{expression} | {result.CompilerChecksum:R} | {result.NativeChecksum:R} | {result.AbsoluteDifference:R} | {result.IsMatch}");
        }

        sb.AppendLine();
    }

    private static void AppendLongChecksumSection(
        StringBuilder sb,
        string title,
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, LongChecksumComparisonResult> results)
    {
        sb.AppendLine(title);
        sb.AppendLine("Expression | Compiler Checksum | Native Checksum | Abs Diff | Match");
        sb.AppendLine(new string('-', 140));

        foreach (var expression in expressions)
        {
            var result = results[expression];
            sb.AppendLine(
                $"{expression} | {result.CompilerChecksum} | {result.NativeChecksum} | {result.AbsoluteDifference} | {result.IsMatch}");
        }

        sb.AppendLine();
    }

    private static void AppendBoolChecksumSection(
        StringBuilder sb,
        string title,
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, BoolChecksumComparisonResult> results)
    {
        sb.AppendLine(title);
        sb.AppendLine("Expression | Compiler TrueCount | Native TrueCount | Abs Diff | Match");
        sb.AppendLine(new string('-', 140));

        foreach (var expression in expressions)
        {
            var result = results[expression];
            sb.AppendLine(
                $"{expression} | {result.CompilerTrueCount} | {result.NativeTrueCount} | {result.AbsoluteDifference} | {result.IsMatch}");
        }

        sb.AppendLine();
    }

    private static void AppendStringChecksumSection(
        StringBuilder sb,
        string title,
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, StringChecksumComparisonResult> results)
    {
        sb.AppendLine(title);
        sb.AppendLine("Expression | Compiler Hash | Native Hash | Exact Mismatch Count | Match");
        sb.AppendLine(new string('-', 140));

        foreach (var expression in expressions)
        {
            var result = results[expression];
            sb.AppendLine(
                $"{expression} | 0x{result.CompilerHash:X16} | 0x{result.NativeHash:X16} | {result.MismatchCount} | {result.IsMatch}");
        }

        sb.AppendLine();
    }

    private static void AppendErrorSection(
        StringBuilder sb,
        string title,
        IReadOnlyList<string> expressions,
        IReadOnlyDictionary<string, ExpectedErrorValidationResult> results)
    {
        sb.AppendLine(title);
        sb.AppendLine("Expression | Error Observed | Outcome");
        sb.AppendLine(new string('-', 140));

        foreach (var expression in expressions)
        {
            var result = results[expression];
            sb.AppendLine($"{expression} | {result.IsExpectedErrorObserved} | {result.Outcome}");
        }

        sb.AppendLine();
    }

    private static string BuildInputText(
        IReadOnlyList<string> validDoubleExpressions,
        IReadOnlyList<string> invalidDoubleExpressions,
        IReadOnlyList<string> validLongExpressions,
        IReadOnlyList<string> invalidLongExpressions,
        IReadOnlyList<string> validBoolExpressions,
        IReadOnlyList<string> invalidBoolExpressions,
        IReadOnlyList<string> validStringExpressions,
        IReadOnlyList<string> invalidStringExpressions)
    {
        var sb = new StringBuilder();

        AppendExpressionGroup(sb, "[Valid Double]", validDoubleExpressions);
        AppendExpressionGroup(sb, "[Invalid Double]", invalidDoubleExpressions);
        AppendExpressionGroup(sb, "[Valid Long]", validLongExpressions);
        AppendExpressionGroup(sb, "[Invalid Long]", invalidLongExpressions);
        AppendExpressionGroup(sb, "[Valid Bool]", validBoolExpressions);
        AppendExpressionGroup(sb, "[Invalid Bool]", invalidBoolExpressions);
        AppendExpressionGroup(sb, "[Valid String]", validStringExpressions);
        AppendExpressionGroup(sb, "[Invalid String]", invalidStringExpressions);

        return sb.ToString().TrimEnd();
    }

    private static void AppendExpressionGroup(
        StringBuilder sb,
        string title,
        IReadOnlyList<string> expressions)
    {
        if (sb.Length > 0)
        {
            sb.AppendLine();
        }

        sb.AppendLine(title);
        foreach (var expression in expressions)
        {
            sb.AppendLine(expression);
        }
    }

    private static ulong AppendStringHash(ulong hash, string value)
    {
        foreach (var ch in value)
        {
            hash ^= ch;
            hash *= FnvPrime;
        }

        hash ^= 0xFFFFUL;
        hash *= FnvPrime;
        return hash;
    }

    private static Dictionary<string, Func<double, double, double>> CreateNativeDoubleExpressions()
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
            [".5 * 2"] = static (x, y) => 0.5 * 2,
            ["x + y"] = static (x, y) => x + y,
            ["x - y"] = static (x, y) => x - y,
            ["x * 2 + y / 2"] = static (x, y) => x * 2 + y / 2,
            ["(x + 10) / (y + 1)"] = static (x, y) => (x + 10) / (y + 1),
            ["x ^ 2"] = static (x, y) => Math.Pow(x, 2),
            ["(x + y) % 7"] = static (x, y) => (x + y) % 7,
            ["test(x) + 1"] = static (x, y) => x + 1
        };
    }

    private static Dictionary<string, Func<double, double, long>> CreateNativeLongExpressions()
    {
        return new Dictionary<string, Func<double, double, long>>
        {
            ["1 + 2"] = static (x, y) => 1 + 2,
            ["10 - 3"] = static (x, y) => 10 - 3,
            ["2 * 3 * 4"] = static (x, y) => 2 * 3 * 4,
            ["20 % 3"] = static (x, y) => 20 % 3,
            ["-5 + 10"] = static (x, y) => -5 + 10,
            ["((1 + 2) * 3) - 4"] = static (x, y) => ((1 + 2) * 3) - 4,
            ["100 - 50 + 25"] = static (x, y) => 100 - 50 + 25,
            ["7 % 5 + 2"] = static (x, y) => 7 % 5 + 2,
            ["3 * (4 + 5)"] = static (x, y) => 3 * (4 + 5),
            ["999 - 1"] = static (x, y) => 999 - 1,
            ["test(10)"] = static (x, y) => 10,
            ["test(10) + 5"] = static (x, y) => 10 + 5,
            ["100 % 10"] = static (x, y) => 100 % 10,
            ["50 - (3 * 7)"] = static (x, y) => 50 - (3 * 7),
            ["2 * 2 * 2 * 2"] = static (x, y) => 2 * 2 * 2 * 2,
            ["1 + 2 + 3 + 4 + 5"] = static (x, y) => 1 + 2 + 3 + 4 + 5,
            ["1000 % 97"] = static (x, y) => 1000 % 97,
            ["(20 - 3) * 2"] = static (x, y) => (20 - 3) * 2,
            ["+10"] = static (x, y) => 10,
            ["(3 + 7) % 4"] = static (x, y) => (3 + 7) % 4
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
            ["1 > 0 AND 2 < 5"] = static (x, y) => 1 > 0 && 2 < 5,
            ["x > y"] = static (x, y) => x > y,
            ["x >= y"] = static (x, y) => x >= y,
            ["x < y OR y < x"] = static (x, y) => x < y || y < x,
            ["NOT (x == y)"] = static (x, y) => !(x == y),
            ["\"a\" = \"a\""] = static (x, y) => "a" == "a",
            ["\"a\" <> \"b\""] = static (x, y) => "a" != "b",
            ["\"x=\" + 1 == \"x=1\""] = static (x, y) => "x=" + 1 == "x=1",
            ["\"A\" + True != \"AFalse\""] = static (x, y) => "A" + true != "AFalse",
            ["(x + y) > (x - y)"] = static (x, y) => (x + y) > (x - y)
        };
    }

    private static Dictionary<string, Func<double, double, string>> CreateNativeStringExpressions()
    {
        return new Dictionary<string, Func<double, double, string>>
        {
            ["\"hello\""] = static (x, y) => "hello",
            ["\"A\" + \"B\""] = static (x, y) => "AB",
            ["\"x=\" + x"] = static (x, y) => "x=" + x,
            ["\"y=\" + y"] = static (x, y) => "y=" + y,
            ["x + \"원\""] = static (x, y) => x + "원",
            ["\"sum=\" + (x + y)"] = static (x, y) => "sum=" + (x + y),
            ["\"flag=\" + True"] = static (x, y) => "flag=" + true,
            ["False + \" value\""] = static (x, y) => false + " value",
            ["\"mix:\" + x + \",\" + y"] = static (x, y) => "mix:" + x + "," + y,
            ["\"prefix-\" + 10"] = static (x, y) => "prefix-" + 10,
            ["10 + \"-suffix\""] = static (x, y) => 10 + "-suffix",
            ["\"pow=\" + (2 ^ 3)"] = static (x, y) => "pow=" + Math.Pow(2, 3),
            ["\"mod=\" + (10 % 3)"] = static (x, y) => "mod=" + (10 % 3),
            ["\"cmp=\" + (1 < 2)"] = static (x, y) => "cmp=" + (1 < 2),
            ["\"nested=\" + ((1 + 2) * 3)"] = static (x, y) => "nested=" + ((1 + 2) * 3),
            ["\"test=\" + test(10)"] = static (x, y) => "test=" + 10,
            ["\"testd=\" + test(10.5)"] = static (x, y) => "testd=" + 10.5,
            ["\"eq=\" + (\"a\" == \"a\")"] = static (x, y) => "eq=" + ("a" == "a"),
            ["\"xy=\" + x + y"] = static (x, y) => "xy=" + x + y,
            ["\"literal with space\""] = static (x, y) => "literal with space"
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

    private static string[] CreateInvalidDoubleExpressions()
    {
        return
        [
            "1e10",
            "1.2.3",
            "1 % 0",
            "1e-5",
            "1,000 + 2",
            "()",
            "True",
            "False",
            "\"abc\"",
            "\"a\" + \"b\"",
            "NOT 1",
            "1 < 2 < 3",
            "unknown(1)",
            "test(True)",
            "x AND y",
            "1 +",
            "(1 + 2",
            "\"a\" == \"a\"",
            "1 = 1",
            "\"a\" == 1"
        ];
    }

    private static string[] CreateInvalidLongExpressions()
    {
        return
        [
            "1e10",
            "1.2.3",
            "1 % 0",
            "1e-5",
            "1,000 + 2",
            "()",
            "True",
            "False",
            "\"abc\"",
            "\"a\" + \"b\"",
            "NOT 1",
            "1 < 2 < 3",
            "unknown(1)",
            "test(True)",
            "1 = 1",
            "\"a\" = \"a\"",
            "True AND False",
            "1 +",
            "(1 + 2",
            "-True"
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
            "1 AND 2",
            "\"a\"",
            "\"a\" + \"b\"",
            "1 + 2",
            "x + y",
            "unknown(1)",
            "test(True)",
            "1e10",
            "1.2.3",
            "()",
            "1 +",
            "(True",
            "\"a\" == 1",
            "1 == \"a\"",
            "\"a\" > \"b\""
        ];
    }

    private static string[] CreateInvalidStringExpressions()
    {
        return
        [
            "1 + 2",
            "True",
            "1 = 1",
            "\"a\" - \"b\"",
            "\"a\" * 3",
            "\"a\" / 2",
            "\"a\" % 2",
            "NOT \"a\"",
            "-\"a\"",
            "+\"a\"",
            "\"a\" > \"b\"",
            "\"a\" AND \"b\"",
            "unknown(1)",
            "test(True)",
            "1e10",
            "1.2.3",
            "()",
            "1 +",
            "\"a\" == 1",
            "1 == \"a\""
        ];
    }
}
