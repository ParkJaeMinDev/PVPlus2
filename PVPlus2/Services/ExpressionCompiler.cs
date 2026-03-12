using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Parlot.Fluent.Parsers;

namespace PVPlus2.Services;

public class ExpressionCompiler
{
    private static readonly ParameterExpression _xParameter = Expression.Parameter(typeof(double), "x");
    private static readonly ParameterExpression _yParameter = Expression.Parameter(typeof(double), "y");

    private static readonly ParameterExpression _xArrayParameter = Expression.Parameter(typeof(double[]), "xValues");
    private static readonly ParameterExpression _yArrayParameter = Expression.Parameter(typeof(double[]), "yValues");
    private static readonly ParameterExpression _lengthParameter = Expression.Parameter(typeof(int), "length");

    private static readonly ParameterExpression _loopIndexParameter = Expression.Variable(typeof(int), "i");
    private static readonly ParameterExpression _sumVariable = Expression.Variable(typeof(double), "sum");
    private static readonly LabelTarget _loopBreakLabel = Expression.Label("LoopBreak");

    private static readonly Parser<Expression> _parser = BuildParser();
    private static readonly Parser<Expression> _parserWithLength = BuildParserWithLength();

    private readonly Dictionary<string, Func<double, double, double>> _compiledCache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<double[], double[], int, double>> _compiledWithLengthCache = new(StringComparer.Ordinal);

    private readonly object _compiledCacheLock = new();
    private readonly object _compiledWithLengthCacheLock = new();

    public Func<double, double, double> CompileTest(string text)
    {
        var key = NormalizeExpressionText(text);

        lock (_compiledCacheLock)
        {
            if (_compiledCache.TryGetValue(key, out var cached))
            {
                return cached;
            }
        }

        var compiled = CompileTestUncached(key);

        lock (_compiledCacheLock)
        {
            if (_compiledCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            _compiledCache[key] = compiled;
            return compiled;
        }
    }

    public Func<double[], double[], int, double> CompileWithLengthTest(string text)
    {
        var key = NormalizeExpressionText(text);

        lock (_compiledWithLengthCacheLock)
        {
            if (_compiledWithLengthCache.TryGetValue(key, out var cached))
            {
                return cached;
            }
        }

        var compiled = CompileWithLengthTestUncached(key);

        lock (_compiledWithLengthCacheLock)
        {
            if (_compiledWithLengthCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            _compiledWithLengthCache[key] = compiled;
            return compiled;
        }
    }

    public Func<double, double, double> CompileTestUncached(string text)
    {
        var key = NormalizeExpressionText(text);
        var body = _parser.Parse(key) ?? throw new FormatException($"잘못된 수식입니다: {key}");

        return Expression.Lambda<Func<double, double, double>>(body, _xParameter, _yParameter).Compile();
    }

    public Func<double[], double[], int, double> CompileWithLengthTestUncached(string text)
    {
        var key = NormalizeExpressionText(text);
        var body = _parserWithLength.Parse(key) ?? throw new FormatException($"잘못된 수식입니다: {key}");

        return Expression.Lambda<Func<double[], double[], int, double>>(
            body,
            _xArrayParameter,
            _yArrayParameter,
            _lengthParameter
        ).Compile();
    }

    public void ClearCaches()
    {
        lock (_compiledCacheLock)
        {
            _compiledCache.Clear();
        }

        lock (_compiledWithLengthCacheLock)
        {
            _compiledWithLengthCache.Clear();
        }
    }

    private static string NormalizeExpressionText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FormatException("수식이 비어 있습니다.");
        }

        return text.Trim();
    }

    private static Parser<Expression> BuildParser()
    {
        var expr = Deferred<Expression>();

        var number = Terms.Decimal(NumberOptions.Any)
            .Then<Expression>(static d => Expression.Constant((double)d));

        var xVariable = Terms.Text("x")
            .Then<Expression>(static _ => _xParameter);

        var yVariable = Terms.Text("y")
            .Then<Expression>(static _ => _yParameter);

        var plus = Terms.Char('+');
        var minus = Terms.Char('-');
        var times = Terms.Char('*');
        var divide = Terms.Char('/');
        var lparen = Terms.Char('(');
        var rparen = Terms.Char(')');

        var primary = OneOf<Expression>(
            number,
            xVariable,
            yVariable,
            Between(lparen, expr, rparen)
        );

        var unary = primary.Unary(
            (minus, static x => (Expression)Expression.Negate(x))
        );

        var multiplicative = unary.LeftAssociative(
            (times, static (a, b) => (Expression)Expression.Multiply(a, b)),
            (divide, static (a, b) => (Expression)Expression.Divide(a, b))
        );

        var additive = multiplicative.LeftAssociative(
            (plus, static (a, b) => (Expression)Expression.Add(a, b)),
            (minus, static (a, b) => (Expression)Expression.Subtract(a, b))
        );

        expr.Parser = additive;

        return expr.Eof().Compile();
    }

    private static Parser<Expression> BuildParserWithLength()
    {
        var expr = Deferred<Expression>();

        var number = Terms.Decimal(NumberOptions.Any)
            .Then<Expression>(static d => Expression.Constant((double)d));

        var xVariable = Terms.Text("x")
            .Then<Expression>(_ => Expression.ArrayIndex(_xArrayParameter, _loopIndexParameter));

        var yVariable = Terms.Text("y")
            .Then<Expression>(_ => Expression.ArrayIndex(_yArrayParameter, _loopIndexParameter));

        var plus = Terms.Char('+');
        var minus = Terms.Char('-');
        var times = Terms.Char('*');
        var divide = Terms.Char('/');
        var lparen = Terms.Char('(');
        var rparen = Terms.Char(')');

        var primary = OneOf<Expression>(
            number,
            xVariable,
            yVariable,
            Between(lparen, expr, rparen)
        );

        var unary = primary.Unary(
            (minus, static x => (Expression)Expression.Negate(x))
        );

        var multiplicative = unary.LeftAssociative(
            (times, static (a, b) => (Expression)Expression.Multiply(a, b)),
            (divide, static (a, b) => (Expression)Expression.Divide(a, b))
        );

        var additive = multiplicative.LeftAssociative(
            (plus, static (a, b) => (Expression)Expression.Add(a, b)),
            (minus, static (a, b) => (Expression)Expression.Subtract(a, b))
        );

        expr.Parser = additive;

        return expr
            .Eof()
            .Then<Expression>(body =>
                Expression.Block(
                    new[] { _loopIndexParameter, _sumVariable },
                    Expression.Assign(_loopIndexParameter, Expression.Constant(0)),
                    Expression.Assign(_sumVariable, Expression.Constant(0.0)),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(_loopIndexParameter, _lengthParameter),
                            Expression.Block(
                                Expression.AddAssign(_sumVariable, body),
                                Expression.PostIncrementAssign(_loopIndexParameter)
                            ),
                            Expression.Break(_loopBreakLabel)
                        ),
                        _loopBreakLabel
                    ),
                    _sumVariable
                ))
            .Compile();
    }
}
