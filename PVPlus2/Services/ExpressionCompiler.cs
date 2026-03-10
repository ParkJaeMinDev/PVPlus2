using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace PVPlus2.Services;

public class ExpressionCompiler
{
    private static readonly Parser<double> _parser = BuildParser();

    public double Compile(string text)
    {
        return _parser.Parse(text);
    }

    public bool TryCompile(string text, out double value)
    {
        return _parser.TryParse(text, out value);
    }

    private static Parser<double> BuildParser()
    {
        var expression = Deferred<double>();

        var number = Terms.Decimal(NumberOptions.Any)
            .Then(static d => (double)d);

        var plus = Terms.Text("+");
        var minus = Terms.Text("-");
        var times = Terms.Text("*");
        var divide = Terms.Text("/");

        var lparen = Terms.Text("(");
        var rparen = Terms.Text(")");

        var primary = OneOf(
            number,
            Between(lparen, expression, rparen)
        );

        var multiplicative = primary.LeftAssociative(
            (times, static (a, b) => a * b),
            (divide, static (a, b) => a / b)
        );

        var additive = multiplicative.LeftAssociative(
            (plus, static (a, b) => a + b),
            (minus, static (a, b) => a - b)
        );

        expression.Parser = additive;

        return expression.Eof().Compile();
    }
}
