using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PVPlus2.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PVPlus2.ViewModels;

public partial class TestViewModel : ObservableObject
{
    [ObservableProperty]
    private string _inputText = string.Join(Environment.NewLine, "3+3","1*3", "1+1", "1*3", "2+3", "3+3");

    [ObservableProperty]
    private string _outputText = string.Empty;

    private readonly ExpressionCompiler _expressionCompiler = new();
    private readonly Dictionary<string, Func<double>> _compiledExpressions = new();

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

            var result = compiled();

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
    private void RunTestFastCompile()
    {
        var sb = new StringBuilder();

        var stopwatchWarmup = Stopwatch.StartNew();
        _ = _expressionCompiler.FastCompileTest("0");
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
            var compiled = _expressionCompiler.FastCompileTest(expressionText);
            stopwatchWarmup.Stop();

            _compiledExpressions[expressionText] = compiled;

            var result = compiled();

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
    private void RunTestFastCompileWithLightExpress()
    {
        var sb = new StringBuilder();

        var stopwatchWarmup = Stopwatch.StartNew();
        _ = _expressionCompiler.FastCompileLightTest("0");
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
            var compiled = _expressionCompiler.FastCompileLightTest(expressionText);
            stopwatchWarmup.Stop();

            _compiledExpressions[expressionText] = compiled;

            var result = compiled();

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

}
