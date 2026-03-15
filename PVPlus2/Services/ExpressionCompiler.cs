using Parlot.Fluent;
using PVPlus2.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Parlot.Fluent.Parsers;

namespace PVPlus2.Services;

public class ExpressionCompiler
{
    private static readonly ParameterExpression _contextParameter =
        Expression.Parameter(typeof(ExpressionContext), "context");

    private static readonly Parser<Expression> _parser =
        BuildParser(typeof(ExpressionContext), _contextParameter);

    private static readonly Dictionary<string, MethodInfo[]> _registeredFunctions =
        typeof(ExpressionFunctions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .GroupBy(static method => method.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static group => group.Key, static group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

    public static Func<ExpressionContext, double> CompileDouble(string text)
    {
        var key = NormalizeExpressionText(text);
        var body = _parser.Parse(key) ?? throw new FormatException($"잘못된 수식입니다: {key}");
        body = Expression.Convert(body, typeof(double));

        return Expression.Lambda<Func<ExpressionContext, double>>(body, _contextParameter).Compile();
    }

    public static Func<ExpressionContext, long> CompileLong(string text)
    {
        var key = NormalizeExpressionText(text);
        var body = _parser.Parse(key) ?? throw new FormatException($"잘못된 수식입니다: {key}");
        body = Expression.Convert(body, typeof(long));

        return Expression.Lambda<Func<ExpressionContext, long>>(body, _contextParameter).Compile();
    }

    public static Func<ExpressionContext, bool> CompileBool(string text)
    {
        var key = NormalizeExpressionText(text);
        var body = _parser.Parse(key) ?? throw new FormatException($"잘못된 수식입니다: {key}");

        if (body.Type != typeof(bool))
        {
            throw new NotSupportedException($"bool 반환 식이 아닙니다. 현재 타입: {body.Type}");
        }

        return Expression.Lambda<Func<ExpressionContext, bool>>(body, _contextParameter).Compile();
    }

    private static string NormalizeExpressionText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FormatException("수식이 비어 있습니다.");
        }

        return text.Trim();
    }

    private static Parser<Expression> BuildParser(Type contextType, ParameterExpression contextParameter)
    {
        var expr = Deferred<Expression>();

        var number = Terms.Pattern(static c => IsNumberLiteralChar(c), 1)
            .Then<Expression>(static text => CreateNumberLiteralExpression(text));

        var identifierName = Terms.Identifier(
                static c => IsIdentifierStartChar(c),
                static c => IsIdentifierChar(c))
            .Then(static text => text.ToString());

        var identifier = Terms.Identifier(
                static c => IsIdentifierStartChar(c),
                static c => IsIdentifierChar(c))
            .Then<Expression>(text => CreatePropertyExpression(text, contextType, contextParameter));

        var trueLiteral = Terms.Keyword("True", caseInsensitive: true)
            .Then<Expression>(_ => Expression.Constant(true));

        var falseLiteral = Terms.Keyword("False", caseInsensitive: true)
            .Then<Expression>(_ => Expression.Constant(false));

        var plusToken = Terms.Char('+');
        var minusToken = Terms.Char('-');
        var powerToken = Terms.Char('^');
        var multiplyToken = Terms.Char('*');
        var divideToken = Terms.Char('/');
        var moduloToken = Terms.Char('%');
        var commaToken = Terms.Char(',');
        var leftParenToken = Terms.Char('(');
        var rightParenToken = Terms.Char(')');

        var equalEqualToken = Terms.Text("==", caseInsensitive: false);
        var equalToken = Terms.Text("=", caseInsensitive: false);
        var notEqualToken = Terms.Text("!=", caseInsensitive: false);
        var angleNotEqualToken = Terms.Text("<>", caseInsensitive: false);
        var greaterEqualToken = Terms.Text(">=", caseInsensitive: false);
        var lessEqualToken = Terms.Text("<=", caseInsensitive: false);
        var greaterToken = Terms.Text(">", caseInsensitive: false);
        var lessToken = Terms.Text("<", caseInsensitive: false);

        var andToken = Terms.Keyword("AND", caseInsensitive: true);
        var orToken = Terms.Keyword("OR", caseInsensitive: true);
        var notToken = Terms.Keyword("NOT", caseInsensitive: true);

        var functionCall = identifierName
            .And(Between(leftParenToken, Separated(commaToken, expr), rightParenToken))
            .Then<Expression>(tuple => CreateFunctionCallExpression(tuple.Item1, tuple.Item2));

        var primary = OneOf<Expression>(
            functionCall,
            trueLiteral,
            falseLiteral,
            number,
            identifier,
            Between(leftParenToken, expr, rightParenToken)
        );

        var unary = primary.Unary(
            (plusToken, static x => x),
            (minusToken, static x => Expression.Negate(x))
        );

        var power = unary.RightAssociative(
            (powerToken, static (a, b) => Expression.Power(
                ConvertNumericExpressionToDouble(a),
                ConvertNumericExpressionToDouble(b)))
        );

        var multiplicative = power.LeftAssociative(
            (multiplyToken, static (a, b) => BuildNumericBinaryExpression(a, b, Expression.Multiply)),
            (divideToken, static (a, b) => BuildDivideExpression(a, b)),
            (moduloToken, static (a, b) => BuildNumericBinaryExpression(a, b, Expression.Modulo))
        );

        var additive = multiplicative.LeftAssociative(
            (plusToken, static (a, b) => BuildNumericBinaryExpression(a, b, Expression.Add)),
            (minusToken, static (a, b) => BuildNumericBinaryExpression(a, b, Expression.Subtract))
        );

        var relational = additive.LeftAssociative(
            (greaterEqualToken, static (a, b) => BuildRelationalExpression(a, b, Expression.GreaterThanOrEqual)),
            (lessEqualToken, static (a, b) => BuildRelationalExpression(a, b, Expression.LessThanOrEqual)),
            (greaterToken, static (a, b) => BuildRelationalExpression(a, b, Expression.GreaterThan)),
            (lessToken, static (a, b) => BuildRelationalExpression(a, b, Expression.LessThan))
        );

        var equality = relational.LeftAssociative(
            (equalEqualToken, static (a, b) => BuildEqualityExpression(a, b)),
            (notEqualToken, static (a, b) => BuildNotEqualExpression(a, b)),
            (angleNotEqualToken, static (a, b) => BuildNotEqualExpression(a, b)),
            (equalToken, static (a, b) => BuildEqualityExpression(a, b))
        );

        var logicalNot = equality.Unary(
            (notToken, static x => Expression.Not(ConvertExpressionToBoolean(x)))
        );

        var logicalAnd = logicalNot.LeftAssociative(
            (andToken, static (a, b) => Expression.AndAlso(
                ConvertExpressionToBoolean(a),
                ConvertExpressionToBoolean(b)))
        );

        var logicalOr = logicalAnd.LeftAssociative(
            (orToken, static (a, b) => Expression.OrElse(
                ConvertExpressionToBoolean(a),
                ConvertExpressionToBoolean(b)))
        );

        expr.Parser = logicalOr;

        return expr.Eof().Compile();
    }

    private static MethodCallExpression CreateFunctionCallExpression(string functionName, IReadOnlyList<Expression> arguments)
    {
        if (!_registeredFunctions.TryGetValue(functionName, out var candidates))
        {
            throw new KeyNotFoundException($"Function '{functionName}' is not defined.");
        }

        MethodInfo? bestMethod = null;
        Expression[]? bestArguments = null;
        var bestScore = int.MaxValue;

        foreach (var candidate in candidates)
        {
            var parameters = candidate.GetParameters();

            if (parameters.Length != arguments.Count)
            {
                continue;
            }

            var convertedArguments = new Expression[arguments.Count];
            var score = 0;
            var success = true;

            for (var i = 0; i < arguments.Count; i++)
            {
                if (!TryConvertFunctionArgument(
                        arguments[i],
                        parameters[i].ParameterType,
                        out var convertedArgument,
                        out var conversionScore))
                {
                    success = false;
                    break;
                }

                convertedArguments[i] = convertedArgument;
                score += conversionScore;
            }

            if (!success)
            {
                continue;
            }

            if (score < bestScore)
            {
                bestMethod = candidate;
                bestArguments = convertedArguments;
                bestScore = score;
                continue;
            }

            if (score == bestScore)
            {
                throw new InvalidOperationException($"Function '{functionName}' call is ambiguous.");
            }
        }

        if (bestMethod == null || bestArguments == null)
        {
            throw new InvalidOperationException(
                $"No matching overload found for function '{functionName}' with {arguments.Count} argument(s).");
        }

        return Expression.Call(bestMethod, bestArguments);
    }

    private static bool TryConvertFunctionArgument(
        Expression argument,
        Type targetType,
        out Expression convertedArgument,
        out int conversionScore)
    {
        if (argument.Type == targetType)
        {
            convertedArgument = argument;
            conversionScore = 0;
            return true;
        }

        if (argument.Type == typeof(long) && targetType == typeof(double))
        {
            convertedArgument = Expression.Convert(argument, typeof(double));
            conversionScore = 1;
            return true;
        }

        if (argument.Type == typeof(double) && targetType == typeof(long))
        {
            convertedArgument = Expression.Convert(argument, typeof(long));
            conversionScore = 1;
            return true;
        }

        convertedArgument = null!;
        conversionScore = 0;
        return false;
    }

    private static MemberExpression CreatePropertyExpression(
        Parlot.TextSpan textSpan,
        Type contextType,
        ParameterExpression contextParameter)
    {
        var name = textSpan.ToString();
        var property = contextType.GetProperty(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
            ?? throw new KeyNotFoundException($"Property '{name}' is not defined on '{contextType.Name}'.");

        if (property.PropertyType != typeof(double) && property.PropertyType != typeof(long))
        {
            throw new NotSupportedException($"현재는 long/double property만 지원합니다: {name}");
        }

        return Expression.Property(contextParameter, property);
    }

    private static ConstantExpression CreateNumberLiteralExpression(Parlot.TextSpan textSpan)
    {
        var span = textSpan.Span;

        if (span.IsEmpty)
        {
            throw new FormatException("숫자 리터럴이 비어 있습니다.");
        }

        if (span.IndexOf('.') >= 0)
        {
            if (!double.TryParse(
                span,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var doubleValue))
            {
                throw new FormatException($"잘못된 실수입니다: {new string(span)}");
            }

            return Expression.Constant(doubleValue);
        }

        if (!long.TryParse(
            span,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out var longValue))
        {
            throw new FormatException($"잘못된 정수입니다: {new string(span)}");
        }

        return Expression.Constant(longValue);
    }

    private static BinaryExpression BuildNumericBinaryExpression(
        Expression left,
        Expression right,
        Func<Expression, Expression, BinaryExpression> operationFactory)
    {
        var leftType = left.Type;
        var rightType = right.Type;

        if (leftType == typeof(long) && rightType == typeof(long))
        {
            return operationFactory(left, right);
        }

        if (leftType == typeof(double) && rightType == typeof(double))
        {
            return operationFactory(left, right);
        }

        if (leftType == typeof(long) && rightType == typeof(double))
        {
            return operationFactory(Expression.Convert(left, typeof(double)), right);
        }

        if (leftType == typeof(double) && rightType == typeof(long))
        {
            return operationFactory(left, Expression.Convert(right, typeof(double)));
        }

        throw new NotSupportedException(
            $"지원하지 않는 숫자 이항 연산입니다. 왼쪽 타입: {leftType}, 오른쪽 타입: {rightType}");
    }

    private static BinaryExpression BuildDivideExpression(Expression left, Expression right)
    {
        Expression leftOperand = left;
        Expression rightOperand = right;

        if (leftOperand.Type == typeof(long))
        {
            leftOperand = Expression.Convert(leftOperand, typeof(double));
        }

        if (rightOperand.Type == typeof(long))
        {
            rightOperand = Expression.Convert(rightOperand, typeof(double));
        }

        if (leftOperand.Type != typeof(double) || rightOperand.Type != typeof(double))
        {
            throw new NotSupportedException(
                $"지원하지 않는 나눗셈 타입입니다. 왼쪽 타입: {left.Type}, 오른쪽 타입: {right.Type}");
        }

        return Expression.Divide(leftOperand, rightOperand);
    }

    private static Expression ConvertNumericExpressionToDouble(Expression expression)
    {
        if (expression.Type == typeof(double))
        {
            return expression;
        }

        if (expression.Type == typeof(long))
        {
            return Expression.Convert(expression, typeof(double));
        }

        throw new NotSupportedException($"지원하지 않는 숫자 식 타입입니다: {expression.Type}");
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(long) || type == typeof(double);
    }

    private static Expression ConvertExpressionToBoolean(Expression expression)
    {
        if (expression.Type == typeof(bool))
        {
            return expression;
        }

        throw new NotSupportedException($"bool 식이 필요합니다. 현재 타입: {expression.Type}");
    }

    private static (Expression Left, Expression Right) NormalizeNumericOperands(Expression left, Expression right)
    {
        if (left.Type == typeof(long) && right.Type == typeof(long))
        {
            return (left, right);
        }

        if (left.Type == typeof(double) && right.Type == typeof(double))
        {
            return (left, right);
        }

        if (left.Type == typeof(long) && right.Type == typeof(double))
        {
            return (Expression.Convert(left, typeof(double)), right);
        }

        if (left.Type == typeof(double) && right.Type == typeof(long))
        {
            return (left, Expression.Convert(right, typeof(double)));
        }

        throw new NotSupportedException(
            $"지원하지 않는 숫자 비교입니다. 왼쪽 타입: {left.Type}, 오른쪽 타입: {right.Type}");
    }

    private static BinaryExpression BuildEqualityExpression(Expression left, Expression right)
    {
        if (left.Type == typeof(bool) && right.Type == typeof(bool))
        {
            return Expression.Equal(left, right);
        }

        if (IsNumericType(left.Type) && IsNumericType(right.Type))
        {
            var operands = NormalizeNumericOperands(left, right);
            return Expression.Equal(operands.Left, operands.Right);
        }

        throw new NotSupportedException(
            $"지원하지 않는 같음 비교입니다. 왼쪽 타입: {left.Type}, 오른쪽 타입: {right.Type}");
    }

    private static BinaryExpression BuildNotEqualExpression(Expression left, Expression right)
    {
        if (left.Type == typeof(bool) && right.Type == typeof(bool))
        {
            return Expression.NotEqual(left, right);
        }

        if (IsNumericType(left.Type) && IsNumericType(right.Type))
        {
            var operands = NormalizeNumericOperands(left, right);
            return Expression.NotEqual(operands.Left, operands.Right);
        }

        throw new NotSupportedException(
            $"지원하지 않는 다름 비교입니다. 왼쪽 타입: {left.Type}, 오른쪽 타입: {right.Type}");
    }

    private static BinaryExpression BuildRelationalExpression(
        Expression left,
        Expression right,
        Func<Expression, Expression, BinaryExpression> operationFactory)
    {
        if (!IsNumericType(left.Type) || !IsNumericType(right.Type))
        {
            throw new NotSupportedException(
                $"지원하지 않는 관계 비교입니다. 왼쪽 타입: {left.Type}, 오른쪽 타입: {right.Type}");
        }

        var operands = NormalizeNumericOperands(left, right);
        return operationFactory(operands.Left, operands.Right);
    }

    private static bool IsIdentifierStartChar(char c)
    {
        return char.IsLetter(c) || c == '_';
    }

    private static bool IsIdentifierChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    private static bool IsNumberLiteralChar(char c)
    {
        return char.IsDigit(c) || c == '.';
    }
}
