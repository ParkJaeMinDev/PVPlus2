using System.Globalization;
using Flee.PublicTypes;
using Xunit;
using Xunit.Sdk;
using CompilerContext = PVPlus2.Models.ExpressionContext;
using FleeContext = Flee.PublicTypes.ExpressionContext;

namespace PVPlus2.Test;

internal static class ExpressionTestHelper
{
    internal const double DoubleTolerance = 1e-9;

    internal static readonly (double X, double Y)[] InputCases =
    [
        (10d, 2d),
        (2d, 10d),
        (5d, 5d)
    ];

    internal static CompilerContext CreateCompilerContext(double x, double y)
    {
        return new CompilerContext
        {
            x = x,
            y = y
        };
    }

    internal static Func<double, double, double> CreateFleeDoubleEvaluator(string expression)
    {
        var context = CreateFleeContext();
        var compiled = context.CompileDynamic(expression);

        return (x, y) =>
        {
            SetFleeVariables(context, x, y);
            var value = compiled.Evaluate();
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        };
    }

    internal static Func<double, double, long> CreateFleeLongEvaluator(string expression)
    {
        var context = CreateFleeContext();
        var compiled = context.CompileDynamic(expression);

        return (x, y) =>
        {
            SetFleeVariables(context, x, y);
            var value = compiled.Evaluate();
            return Convert.ToInt64(value, CultureInfo.InvariantCulture);
        };
    }

    internal static Func<double, double, bool> CreateFleeBoolEvaluator(string expression)
    {
        var context = CreateFleeContext();
        var compiled = context.CompileDynamic(expression);

        return (x, y) =>
        {
            SetFleeVariables(context, x, y);
            var value = compiled.Evaluate();
            return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        };
    }

    internal static Func<double, double, string> CreateFleeStringEvaluator(string expression)
    {
        var context = CreateFleeContext();
        var compiled = context.CompileDynamic(expression);

        return (x, y) =>
        {
            SetFleeVariables(context, x, y);
            var value = compiled.Evaluate();
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        };
    }

    internal static FleeContext CreateFleeContext()
    {
        var context = new FleeContext();
        context.Variables["x"] = 0d;
        context.Variables["y"] = 0d;
        return context;
    }

    internal static void SetFleeVariables(FleeContext context, double x, double y)
    {
        context.Variables["x"] = x;
        context.Variables["y"] = y;
    }

    internal static void AssertCompilerExpressionFails<T>(
        string expression,
        Func<string, Func<CompilerContext, T>> compiler)
    {
        Func<CompilerContext, T>? compiled = null;
        var compileException = Record.Exception(() => compiled = compiler(expression));

        if (compileException is not null)
        {
            return;
        }

        var evaluationException = Record.Exception(() => compiled!(CreateCompilerContext(10d, 2d)));

        if (evaluationException is not null)
        {
            return;
        }

        throw new XunitException($"Expected compiler failure for '{expression}', but compilation and first evaluation both succeeded.");
    }

    internal static void AssertDoubleEqual(double expected, double actual, string message)
    {
        if (double.IsNaN(expected) && double.IsNaN(actual))
        {
            return;
        }

        if (double.IsPositiveInfinity(expected) && double.IsPositiveInfinity(actual))
        {
            return;
        }

        if (double.IsNegativeInfinity(expected) && double.IsNegativeInfinity(actual))
        {
            return;
        }

        if (double.IsFinite(expected) && double.IsFinite(actual))
        {
            var difference = Math.Abs(expected - actual);
            var tolerance = DoubleTolerance * Math.Max(1d, Math.Abs(expected));
            Assert.True(difference <= tolerance, $"{message} Expected {expected:R}, actual {actual:R}, difference {difference:R}, tolerance {tolerance:R}.");
            return;
        }

        Assert.True(false, $"{message} Expected {expected:R}, actual {actual:R}.");
    }
}
