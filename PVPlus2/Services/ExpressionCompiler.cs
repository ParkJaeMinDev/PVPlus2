using FastExpressionCompiler;
using Parlot.Fluent;
using System;
using System.Linq.Expressions;
using static Parlot.Fluent.Parsers;

namespace PVPlus2.Services;

public class ExpressionCompiler
{
    private static readonly Parser<Expression> _parser = BuildParser();
    private static readonly Parser<FastExpressionCompiler.LightExpression.Expression> _lightParser = BuildParserLight();

    public Func<double> CompileTest(string text)
    {
        var body = _parser.Parse(text) ?? throw new FormatException($"잘못된 수식입니다: {text}");

        return Expression.Lambda<Func<double>>(body).Compile();
    }

    public Func<double> FastCompileTest(string text)
    {
        var body = _parser.Parse(text) ?? throw new FormatException($"잘못된 수식입니다: {text}");

        return Expression.Lambda<Func<double>>(body).CompileFast();
    }
    public Func<double> FastCompileLightTest(string text)
    {
        var body = _lightParser.Parse(text) ?? throw new FormatException($"잘못된 수식입니다: {text}");
        var lambda = FastExpressionCompiler.LightExpression.Expression.Lambda<Func<double>>(body);
        return FastExpressionCompiler.LightExpression.ExpressionCompiler
            .CompileFast<Func<double>>(lambda);
    }


    private static Parser<Expression> BuildParser()
    {
        var expr = Deferred<Expression>();

        var number = Terms.Decimal(NumberOptions.Any)
            .Then<Expression>(static d => Expression.Constant((double)d));

        var plus = Terms.Char('+');
        var minus = Terms.Char('-');
        var times = Terms.Char('*');
        var divide = Terms.Char('/');
        var lparen = Terms.Char('(');
        var rparen = Terms.Char(')');

        // primary = number | '(' expr ')'
        var primary = OneOf(
            number,
            Between(lparen, expr, rparen)
        );

        // unary = '-' unary | primary
        var unary = primary.Unary(
            (minus, static x => Expression.Negate(x))
        );

        // multiplicative = unary (('*' | '/') unary)*
        var multiplicative = unary.LeftAssociative(
            (times, static (a, b) => Expression.Multiply(a, b)),
            (divide, static (a, b) => Expression.Divide(a, b))
        );

        // additive = multiplicative (('+' | '-') multiplicative)*
        var additive = multiplicative.LeftAssociative(
            (plus, static (a, b) => Expression.Add(a, b)),
            (minus, static (a, b) => Expression.Subtract(a, b))
        );

        expr.Parser = additive;

        // 입력 전체를 끝까지 소비해야 성공
        return expr.Eof().Compile();
    }

    private static Parser<FastExpressionCompiler.LightExpression.Expression> BuildParserLight()
    {
        var expr = Deferred<FastExpressionCompiler.LightExpression.Expression>();

        var number = Terms.Decimal(NumberOptions.Any)
            .Then<FastExpressionCompiler.LightExpression.Expression>(
                static d => FastExpressionCompiler.LightExpression.Expression.Constant((double)d));

        var plus = Terms.Char('+');
        var minus = Terms.Char('-');
        var times = Terms.Char('*');
        var divide = Terms.Char('/');
        var lparen = Terms.Char('(');
        var rparen = Terms.Char(')');

        var primary = OneOf<FastExpressionCompiler.LightExpression.Expression>(
            number,
            Between(lparen, expr, rparen)
        );

        var unary = primary.Unary(
            (minus, static x => (FastExpressionCompiler.LightExpression.Expression)
                FastExpressionCompiler.LightExpression.Expression.Negate(x))
        );

        var multiplicative = unary.LeftAssociative(
            (times, static (a, b) => (FastExpressionCompiler.LightExpression.Expression)
                FastExpressionCompiler.LightExpression.Expression.Multiply(a, b)),
            (divide, static (a, b) => (FastExpressionCompiler.LightExpression.Expression)
                FastExpressionCompiler.LightExpression.Expression.Divide(a, b))
        );

        var additive = multiplicative.LeftAssociative(
            (plus, static (a, b) => (FastExpressionCompiler.LightExpression.Expression)
                FastExpressionCompiler.LightExpression.Expression.Add(a, b)),
            (minus, static (a, b) => (FastExpressionCompiler.LightExpression.Expression)
                FastExpressionCompiler.LightExpression.Expression.Subtract(a, b))
        );

        expr.Parser = additive;

        return expr.Eof().Compile();
    }

}
