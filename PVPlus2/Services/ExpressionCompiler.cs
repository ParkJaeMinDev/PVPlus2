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

    private static readonly Parser<AstNode> _parser = BuildParser();

    private static readonly Dictionary<string, MethodInfo[]> _registeredFunctions =
        typeof(ExpressionFunctions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .GroupBy(static method => method.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static group => group.Key, static group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

    private static readonly MethodInfo _stringConcatMethod =
        typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(object), typeof(object) })!;

    public static Func<ExpressionContext, double> CompileDouble(string text)
    {
        var body = BindExpression(text);
        body = ConvertReturnExpression(body, typeof(double));

        return Expression.Lambda<Func<ExpressionContext, double>>(body, _contextParameter).Compile();
    }

    public static Func<ExpressionContext, long> CompileLong(string text)
    {
        var body = BindExpression(text);
        body = ConvertReturnExpression(body, typeof(long));

        return Expression.Lambda<Func<ExpressionContext, long>>(body, _contextParameter).Compile();
    }

    public static Func<ExpressionContext, bool> CompileBool(string text)
    {
        var body = BindExpression(text);
        body = ConvertReturnExpression(body, typeof(bool));

        return Expression.Lambda<Func<ExpressionContext, bool>>(body, _contextParameter).Compile();
    }

    public static Func<ExpressionContext, string> CompileString(string text)
    {
        var body = BindExpression(text);
        body = ConvertReturnExpression(body, typeof(string));

        return Expression.Lambda<Func<ExpressionContext, string>>(body, _contextParameter).Compile();
    }

    private static Expression BindExpression(string text)
    {
        var key = NormalizeExpressionText(text);
        var syntax = _parser.Parse(key) ?? throw new FormatException($"잘못된 수식입니다: {key}");
        return BindSyntax(syntax);
    }

    private static string NormalizeExpressionText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FormatException("수식이 비어 있습니다.");
        }

        return text.Trim();
    }

    private static Parser<AstNode> BuildParser()
    {
        var expr = Deferred<AstNode>();

        var number = Terms.Pattern(static c => IsNumberLiteralChar(c), 1)
            .Then<AstNode>(static text => CreateNumberLiteralNode(text));

        var stringLiteral = Terms.String(StringLiteralQuotes.Double)
            .Then<AstNode>(static text => new StringLiteralNode(text));

        var identifierName = Terms.Identifier(
                static c => IsIdentifierStartChar(c),
                static c => IsIdentifierChar(c))
            .Then(static text => text.ToString());

        var identifier = identifierName
            .Then<AstNode>(static name => new IdentifierNode(name));

        var trueLiteral = Terms.Keyword("True", caseInsensitive: true)
            .Then<AstNode>(_ => new BooleanLiteralNode(true));

        var falseLiteral = Terms.Keyword("False", caseInsensitive: true)
            .Then<AstNode>(_ => new BooleanLiteralNode(false));

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
            .Then<AstNode>(tuple => new FunctionCallNode(tuple.Item1, tuple.Item2.ToArray()));

        var primary = OneOf<AstNode>(
            functionCall,
            trueLiteral,
            falseLiteral,
            stringLiteral,
            number,
            identifier,
            Between(leftParenToken, expr, rightParenToken)
        );

        var unary = primary.Unary(
            (plusToken, static x => new UnaryNode(UnaryOperator.UnaryPlus, x)),
            (minusToken, static x => new UnaryNode(UnaryOperator.Negate, x))
        );

        var power = unary.RightAssociative(
            (powerToken, static (a, b) => new BinaryNode(BinaryOperator.Power, a, b))
        );

        var multiplicative = power.LeftAssociative(
            (multiplyToken, static (a, b) => new BinaryNode(BinaryOperator.Multiply, a, b)),
            (divideToken, static (a, b) => new BinaryNode(BinaryOperator.Divide, a, b)),
            (moduloToken, static (a, b) => new BinaryNode(BinaryOperator.Modulo, a, b))
        );

        var additive = multiplicative.LeftAssociative(
            (plusToken, static (a, b) => new BinaryNode(BinaryOperator.Add, a, b)),
            (minusToken, static (a, b) => new BinaryNode(BinaryOperator.Subtract, a, b))
        );

        var relational = additive.LeftAssociative(
            (greaterEqualToken, static (a, b) => new BinaryNode(BinaryOperator.GreaterThanOrEqual, a, b)),
            (lessEqualToken, static (a, b) => new BinaryNode(BinaryOperator.LessThanOrEqual, a, b)),
            (greaterToken, static (a, b) => new BinaryNode(BinaryOperator.GreaterThan, a, b)),
            (lessToken, static (a, b) => new BinaryNode(BinaryOperator.LessThan, a, b))
        );

        var equality = relational.LeftAssociative(
            (equalEqualToken, static (a, b) => new BinaryNode(BinaryOperator.Equal, a, b)),
            (notEqualToken, static (a, b) => new BinaryNode(BinaryOperator.NotEqual, a, b)),
            (angleNotEqualToken, static (a, b) => new BinaryNode(BinaryOperator.NotEqual, a, b)),
            (equalToken, static (a, b) => new BinaryNode(BinaryOperator.Equal, a, b))
        );

        var logicalNot = equality.Unary(
            (notToken, static x => new UnaryNode(UnaryOperator.Not, x))
        );

        var logicalAnd = logicalNot.LeftAssociative(
            (andToken, static (a, b) => new BinaryNode(BinaryOperator.And, a, b))
        );

        var logicalOr = logicalAnd.LeftAssociative(
            (orToken, static (a, b) => new BinaryNode(BinaryOperator.Or, a, b))
        );

        expr.Parser = logicalOr;

        return expr.Eof().Compile();
    }

    private static Expression BindSyntax(AstNode node)
    {
        return node switch
        {
            NumberLiteralNode literal => literal.Value switch
            {
                long longValue => Expression.Constant(longValue),
                double doubleValue => Expression.Constant(doubleValue),
                _ => throw new NotSupportedException($"지원하지 않는 숫자 literal 타입입니다: {literal.Value.GetType()}")
            },
            StringLiteralNode literal => Expression.Constant(MaterializeString(literal.Value), typeof(string)),
            BooleanLiteralNode literal => Expression.Constant(literal.Value),
            IdentifierNode identifier => CreatePropertyExpression(identifier.Name),
            FunctionCallNode functionCall => CreateFunctionCallExpression(
                functionCall.Name,
                functionCall.Arguments),
            UnaryNode unary => BindUnaryExpression(unary),
            BinaryNode binary => BindBinaryExpression(binary),
            _ => throw new NotSupportedException($"지원하지 않는 AST 노드입니다: {node.GetType()}")
        };
    }

    private static Expression BindUnaryExpression(UnaryNode node)
    {
        var operand = BindSyntax(node.Operand);

        return node.Operator switch
        {
            UnaryOperator.UnaryPlus => BuildUnaryPlusExpression(operand),
            UnaryOperator.Negate => BuildUnaryNegateExpression(operand),
            UnaryOperator.Not => Expression.Not(ConvertExpressionToBoolean(operand)),
            _ => throw new NotSupportedException($"지원하지 않는 단항 연산자입니다: {node.Operator}")
        };
    }

    private static Expression BindBinaryExpression(BinaryNode node)
    {
        var left = BindSyntax(node.Left);
        var right = BindSyntax(node.Right);

        return node.Operator switch
        {
            BinaryOperator.Add => BuildAddExpression(left, right),
            BinaryOperator.Subtract => BuildNumericBinaryExpression(left, right, Expression.Subtract),
            BinaryOperator.Multiply => BuildNumericBinaryExpression(left, right, Expression.Multiply),
            BinaryOperator.Divide => BuildDivideExpression(left, right),
            BinaryOperator.Modulo => BuildNumericBinaryExpression(left, right, Expression.Modulo),
            BinaryOperator.Power => BuildPowerExpression(left, right),
            BinaryOperator.Equal => BuildEqualityExpression(left, right),
            BinaryOperator.NotEqual => BuildNotEqualExpression(left, right),
            BinaryOperator.GreaterThan => BuildRelationalExpression(left, right, Expression.GreaterThan),
            BinaryOperator.GreaterThanOrEqual => BuildRelationalExpression(left, right, Expression.GreaterThanOrEqual),
            BinaryOperator.LessThan => BuildRelationalExpression(left, right, Expression.LessThan),
            BinaryOperator.LessThanOrEqual => BuildRelationalExpression(left, right, Expression.LessThanOrEqual),
            BinaryOperator.And => Expression.AndAlso(
                ConvertExpressionToBoolean(left),
                ConvertExpressionToBoolean(right)),
            BinaryOperator.Or => Expression.OrElse(
                ConvertExpressionToBoolean(left),
                ConvertExpressionToBoolean(right)),
            _ => throw new NotSupportedException($"지원하지 않는 이항 연산자입니다: {node.Operator}")
        };
    }

    private static Expression ConvertReturnExpression(Expression expression, Type targetType)
    {
        if (targetType == typeof(double))
        {
            if (expression.Type == typeof(double))
            {
                return expression;
            }

            if (expression.Type == typeof(long))
            {
                return Expression.Convert(expression, typeof(double));
            }

            throw new NotSupportedException($"double 반환 식이 아닙니다. 현재 타입: {expression.Type}");
        }

        if (targetType == typeof(long))
        {
            if (expression.Type == typeof(long))
            {
                return expression;
            }

            if (expression.Type == typeof(double))
            {
                return Expression.Convert(expression, typeof(long));
            }

            throw new NotSupportedException($"long 반환 식이 아닙니다. 현재 타입: {expression.Type}");
        }

        if (targetType == typeof(bool))
        {
            if (expression.Type != typeof(bool))
            {
                throw new NotSupportedException($"bool 반환 식이 아닙니다. 현재 타입: {expression.Type}");
            }

            return expression;
        }

        if (targetType == typeof(string))
        {
            if (expression.Type != typeof(string))
            {
                throw new NotSupportedException($"string 반환 식이 아닙니다. 현재 타입: {expression.Type}");
            }

            return expression;
        }

        throw new NotSupportedException($"지원하지 않는 반환 타입입니다: {targetType}");
    }

    private static Expression CreateFunctionCallExpression(string functionName, IReadOnlyList<AstNode> rawArguments)
    {
        if (TryBindSpecialFunction(functionName, rawArguments, out var specialExpression))
        {
            return specialExpression!;
        }

        var boundArguments = rawArguments.Select(BindSyntax).ToArray();
        return CreateReflectionFunctionCallExpression(functionName, boundArguments);
    }

    private static bool TryBindSpecialFunction(
        string functionName,
        IReadOnlyList<AstNode> rawArguments,
        out Expression? expression)
    {
        if (string.Equals(functionName, "if", StringComparison.OrdinalIgnoreCase))
        {
            expression = CreateIfExpression(rawArguments);
            return true;
        }

        if (string.Equals(functionName, "ifs", StringComparison.OrdinalIgnoreCase))
        {
            expression = CreateIfsExpression(rawArguments);
            return true;
        }

        if (string.Equals(functionName, "cast", StringComparison.OrdinalIgnoreCase))
        {
            expression = CreateCastExpression(rawArguments);
            return true;
        }

        expression = null;
        return false;
    }

    private static ConditionalExpression CreateIfExpression(IReadOnlyList<AstNode> arguments)
    {
        if (arguments.Count != 3)
        {
            throw new FormatException(
                $"if(condition, true_value, false_value) 형식이어야 합니다. 현재 인수 개수: {arguments.Count}");
        }

        var condition = ConvertExpressionToBoolean(BindSyntax(arguments[0]));
        var trueExpr = BindSyntax(arguments[1]);
        var falseExpr = BindSyntax(arguments[2]);

        (trueExpr, falseExpr) = UnifyBranchTypes(trueExpr, falseExpr);

        return Expression.Condition(condition, trueExpr, falseExpr);
    }

    private static Expression CreateIfsExpression(IReadOnlyList<AstNode> arguments)
    {
        if (arguments.Count < 3 || arguments.Count % 2 == 0)
        {
            throw new FormatException(
                $"ifs(condition1, val1, ..., conditionN, valN, default) 형식이어야 합니다. " +
                $"인수 개수는 홀수 3 이상이어야 합니다. 현재 인수 개수: {arguments.Count}");
        }

        var result = BindSyntax(arguments[^1]);

        for (var i = arguments.Count - 2; i >= 1; i -= 2)
        {
            var trueExpr = BindSyntax(arguments[i]);
            var condition = ConvertExpressionToBoolean(BindSyntax(arguments[i - 1]));

            (trueExpr, result) = UnifyBranchTypes(trueExpr, result);

            result = Expression.Condition(condition, trueExpr, result);
        }

        return result;
    }

    private static Expression CreateCastExpression(IReadOnlyList<AstNode> arguments)
    {
        if (arguments.Count != 2)
        {
            throw new FormatException(
                $"cast(value, type) 형식이어야 합니다. 현재 인수 개수: {arguments.Count}");
        }

        var valueExpression = BindSyntax(arguments[0]);
        var targetType = ResolveCastTargetType(arguments[1]);

        return ConvertCastExpression(valueExpression, targetType);
    }

    private static Type ResolveCastTargetType(AstNode rawTypeArgument)
    {
        if (rawTypeArgument is not IdentifierNode identifier)
        {
            throw new FormatException("cast의 두 번째 인수는 타입명 identifier여야 합니다.");
        }

        return identifier.Name.ToLowerInvariant() switch
        {
            "int" or "int32" => typeof(long),
            "long" or "int64" => typeof(long),
            "double" or "float64" => typeof(double),
            "bool" or "boolean" => typeof(bool),
            "string" => typeof(string),
            _ => throw new NotSupportedException($"cast에서 지원하지 않는 타입입니다: {identifier.Name}")
        };
    }

    private static Expression ConvertCastExpression(Expression valueExpression, Type targetType)
    {
        if (valueExpression.Type == targetType)
        {
            return valueExpression;
        }

        if (IsNumericType(valueExpression.Type) && IsNumericType(targetType))
        {
            return Expression.Convert(valueExpression, targetType);
        }

        throw new NotSupportedException(
            $"지원하지 않는 cast 변환입니다. source: {valueExpression.Type}, target: {targetType}");
    }

    private static (Expression TrueExpr, Expression FalseExpr) UnifyBranchTypes(
        Expression trueExpr,
        Expression falseExpr)
    {
        if (trueExpr.Type == falseExpr.Type)
        {
            return (trueExpr, falseExpr);
        }

        if (IsNumericType(trueExpr.Type) && IsNumericType(falseExpr.Type))
        {
            return (
                ConvertNumericExpressionToDouble(trueExpr),
                ConvertNumericExpressionToDouble(falseExpr)
            );
        }

        throw new NotSupportedException(
            $"if/ifs 브랜치 타입이 일치하지 않습니다. " +
            $"true 브랜치: {trueExpr.Type}, false 브랜치: {falseExpr.Type}");
    }

    private static MethodCallExpression CreateReflectionFunctionCallExpression(
        string functionName,
        Expression[] arguments)
    {
        if (!_registeredFunctions.TryGetValue(functionName, out var candidates))
        {
            throw new KeyNotFoundException($"Function '{functionName}' is not defined.");
        }

        MethodInfo? bestMethod = null;
        Expression[]? bestArguments = null;
        var bestScore = int.MaxValue;
        var bestIsAmbiguous = false;

        foreach (var candidate in candidates)
        {
            if (!TryConvertFunctionCallArguments(candidate, arguments, out var convertedArguments, out var score))
            {
                continue;
            }

            if (score < bestScore)
            {
                bestMethod = candidate;
                bestArguments = convertedArguments;
                bestScore = score;
                bestIsAmbiguous = false;
                continue;
            }

            if (score == bestScore)
            {
                bestIsAmbiguous = true;
            }
        }

        if (bestMethod == null || bestArguments == null)
        {
            throw new InvalidOperationException(
                $"No matching overload found for function '{functionName}' with {arguments.Length} argument(s).");
        }

        if (bestIsAmbiguous)
        {
            throw new InvalidOperationException($"Function '{functionName}' call is ambiguous.");
        }

        return Expression.Call(bestMethod, bestArguments);
    }

    private static bool TryConvertFunctionCallArguments(
        MethodInfo candidate,
        Expression[] arguments,
        out Expression[] convertedArguments,
        out int score)
    {
        var parameters = candidate.GetParameters();
        var isContextInjected = parameters.Length > 0
            && parameters[0].ParameterType == typeof(ExpressionContext);
        var isParams = parameters.Length > 0
            && parameters[^1].GetCustomAttribute<ParamArrayAttribute>() != null;

        // 명세 기준: context + params 조합은 이번 릴리스에서 지원하지 않는다.
        if (isContextInjected && isParams)
        {
            convertedArguments = null!;
            score = 0;
            return false;
        }

        if (isContextInjected)
        {
            return TryConvertContextInjectedFunctionArguments(arguments, parameters, out convertedArguments, out score);
        }

        if (isParams)
        {
            return TryConvertParamArrayFunctionArguments(arguments, parameters, out convertedArguments, out score);
        }

        return TryConvertFixedArityFunctionArguments(arguments, parameters, out convertedArguments, out score);
    }

    private static bool TryConvertFixedArityFunctionArguments(
        Expression[] arguments,
        ParameterInfo[] parameters,
        out Expression[] convertedArguments,
        out int score)
    {
        if (parameters.Length != arguments.Length)
        {
            convertedArguments = null!;
            score = 0;
            return false;
        }

        var result = new Expression[arguments.Length];
        var totalScore = 0;

        for (var i = 0; i < arguments.Length; i++)
        {
            if (!TryConvertFunctionArgument(
                    arguments[i],
                    parameters[i].ParameterType,
                    out var convertedArgument,
                    out var conversionScore))
            {
                convertedArguments = null!;
                score = 0;
                return false;
            }

            result[i] = convertedArgument;
            totalScore += conversionScore;
        }

        convertedArguments = result;
        score = totalScore;
        return true;
    }

    private static bool TryConvertContextInjectedFunctionArguments(
        Expression[] arguments,
        ParameterInfo[] parameters,
        out Expression[] convertedArguments,
        out int score)
    {
        var effectiveParameterCount = parameters.Length - 1;

        if (arguments.Length != effectiveParameterCount)
        {
            convertedArguments = null!;
            score = 0;
            return false;
        }

        var result = new Expression[parameters.Length];
        result[0] = _contextParameter;
        var totalScore = 0;

        for (var i = 1; i < parameters.Length; i++)
        {
            if (!TryConvertFunctionArgument(
                    arguments[i - 1],
                    parameters[i].ParameterType,
                    out var convertedArgument,
                    out var conversionScore))
            {
                convertedArguments = null!;
                score = 0;
                return false;
            }

            result[i] = convertedArgument;
            totalScore += conversionScore;
        }

        convertedArguments = result;
        score = totalScore;
        return true;
    }

    private static bool TryConvertParamArrayFunctionArguments(
        Expression[] arguments,
        ParameterInfo[] parameters,
        out Expression[] convertedArguments,
        out int score)
    {
        var fixedCount = parameters.Length - 1;

        if (arguments.Length < fixedCount)
        {
            convertedArguments = null!;
            score = 0;
            return false;
        }

        var elementType = parameters[^1].ParameterType.GetElementType()!;
        var result = new Expression[parameters.Length];
        var totalScore = 1; // params penalty

        for (var i = 0; i < fixedCount; i++)
        {
            if (!TryConvertFunctionArgument(
                    arguments[i],
                    parameters[i].ParameterType,
                    out var converted,
                    out var convScore))
            {
                convertedArguments = null!;
                score = 0;
                return false;
            }

            result[i] = converted;
            totalScore += convScore;
        }

        var paramsElements = new Expression[arguments.Length - fixedCount];

        for (var i = fixedCount; i < arguments.Length; i++)
        {
            if (!TryConvertFunctionArgument(
                    arguments[i],
                    elementType,
                    out var converted,
                    out var convScore))
            {
                convertedArguments = null!;
                score = 0;
                return false;
            }

            paramsElements[i - fixedCount] = converted;
            totalScore += convScore;
        }

        result[fixedCount] = Expression.NewArrayInit(elementType, paramsElements);
        convertedArguments = result;
        score = totalScore;
        return true;
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

        if (targetType.IsAssignableFrom(argument.Type) || targetType == typeof(object))
        {
            convertedArgument = Expression.Convert(argument, targetType);
            conversionScore = 2;
            return true;
        }

        convertedArgument = null!;
        conversionScore = 0;
        return false;
    }

    private static MemberExpression CreatePropertyExpression(string name)
    {
        var property = typeof(ExpressionContext).GetProperty(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
            ?? throw new KeyNotFoundException($"Property '{name}' is not defined on '{nameof(ExpressionContext)}'.");

        return Expression.Property(_contextParameter, property);
    }

    private static NumberLiteralNode CreateNumberLiteralNode(Parlot.TextSpan textSpan)
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

            return new NumberLiteralNode(doubleValue);
        }

        if (!long.TryParse(
            span,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out var longValue))
        {
            throw new FormatException($"잘못된 정수입니다: {new string(span)}");
        }

        return new NumberLiteralNode(longValue);
    }

    private static string MaterializeString(Parlot.TextSpan textSpan)
    {
        if (textSpan.Buffer is null || textSpan.Length == 0)
        {
            return string.Empty;
        }

        if (textSpan.Offset == 0 && textSpan.Length == textSpan.Buffer.Length)
        {
            return textSpan.Buffer;
        }

        return textSpan.Buffer.Substring(textSpan.Offset, textSpan.Length);
    }

    private static Expression BuildUnaryPlusExpression(Expression operand)
    {
        if (operand.Type == typeof(string))
        {
            throw new NotSupportedException("string에는 단항 + 를 사용할 수 없습니다.");
        }

        return operand;
    }

    private static UnaryExpression BuildUnaryNegateExpression(Expression operand)
    {
        if (!IsNumericType(operand.Type))
        {
            throw new NotSupportedException($"단항 - 는 숫자형에만 사용할 수 있습니다. 현재 타입: {operand.Type}");
        }

        return Expression.Negate(operand);
    }

    private static Expression BuildAddExpression(Expression left, Expression right)
    {
        if (left.Type == typeof(string) || right.Type == typeof(string))
        {
            return BuildStringConcatExpression(left, right);
        }

        return BuildNumericBinaryExpression(left, right, Expression.Add);
    }

    private static MethodCallExpression BuildStringConcatExpression(Expression left, Expression right)
    {
        return Expression.Call(
            _stringConcatMethod,
            ConvertExpressionToObject(left),
            ConvertExpressionToObject(right));
    }

    private static Expression ConvertExpressionToObject(Expression expression)
    {
        if (expression.Type == typeof(object))
        {
            return expression;
        }

        return Expression.Convert(expression, typeof(object));
    }

    private static BinaryExpression BuildNumericBinaryExpression(
        Expression left,
        Expression right,
        Func<Expression, Expression, BinaryExpression> operationFactory)
    {
        var operands = NormalizeNumericOperands(left, right);
        return operationFactory(operands.Left, operands.Right);
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

    private static BinaryExpression BuildPowerExpression(Expression left, Expression right)
    {
        return Expression.Power(
            ConvertNumericExpressionToDouble(left),
            ConvertNumericExpressionToDouble(right));
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
            $"지원하지 않는 숫자 연산입니다. 왼쪽 타입: {left.Type}, 오른쪽 타입: {right.Type}");
    }

    private static BinaryExpression BuildEqualityExpression(Expression left, Expression right)
    {
        if (left.Type == typeof(bool) && right.Type == typeof(bool))
        {
            return Expression.Equal(left, right);
        }

        if (left.Type == typeof(string) && right.Type == typeof(string))
        {
            return Expression.Equal(left, right);
        }

        if (left.Type == typeof(string) || right.Type == typeof(string))
        {
            throw new NotSupportedException(
                $"문자열 같음 비교는 좌우가 모두 string일 때만 지원합니다. 왼쪽 타입: {left.Type}, 오른쪽 타입: {right.Type}");
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

        if (left.Type == typeof(string) && right.Type == typeof(string))
        {
            return Expression.NotEqual(left, right);
        }

        if (left.Type == typeof(string) || right.Type == typeof(string))
        {
            throw new NotSupportedException(
                $"문자열 다름 비교는 좌우가 모두 string일 때만 지원합니다. 왼쪽 타입: {left.Type}, 오른쪽 타입: {right.Type}");
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

    private abstract record AstNode;

    private sealed record NumberLiteralNode(object Value) : AstNode;

    private sealed record StringLiteralNode(Parlot.TextSpan Value) : AstNode;

    private sealed record BooleanLiteralNode(bool Value) : AstNode;

    private sealed record IdentifierNode(string Name) : AstNode;

    private sealed record FunctionCallNode(string Name, IReadOnlyList<AstNode> Arguments) : AstNode;

    private sealed record UnaryNode(UnaryOperator Operator, AstNode Operand) : AstNode;

    private sealed record BinaryNode(BinaryOperator Operator, AstNode Left, AstNode Right) : AstNode;

    private enum UnaryOperator
    {
        UnaryPlus,
        Negate,
        Not
    }

    private enum BinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Power,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        And,
        Or
    }
}
