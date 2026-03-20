using System;
using PVPlus2.Models;
using PVPlus2.Services;
using Xunit;
using Xunit.Sdk;

namespace PVPlus2.Test;

public class ExpressionCastTests
{
    [Fact]
    public void Cast_Returns_Expected_Long_Value()
    {
        var fromIntLiteral = ExpressionCompiler.CompileLong("cast(1, int)");
        var fromDoubleLiteralInt = ExpressionCompiler.CompileLong("cast(1.9, int)");
        var fromDoubleLiteralLong = ExpressionCompiler.CompileLong("cast(1.9, long)");
        var fromContext = ExpressionCompiler.CompileLong("cast(x, int)");
        var context = ExpressionTestHelper.CreateCompilerContext(3.7d, 2d);

        Assert.Equal(1L, fromIntLiteral(context));
        Assert.Equal(1L, fromDoubleLiteralInt(context));
        Assert.Equal(1L, fromDoubleLiteralLong(context));
        Assert.Equal(3L, fromContext(context));
    }

    [Fact]
    public void Cast_Returns_Expected_Double_Value()
    {
        var fromLongLiteral = ExpressionCompiler.CompileDouble("cast(1, double)");
        var nestedCast = ExpressionCompiler.CompileDouble("cast(cast(1.9, int), double)");
        var float64Alias = ExpressionCompiler.CompileDouble("cast(1, float64)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        ExpressionTestHelper.AssertDoubleEqual(1d, fromLongLiteral(context), "Unexpected result for cast(1, double).");
        ExpressionTestHelper.AssertDoubleEqual(1d, nestedCast(context), "Unexpected result for nested numeric cast.");
        ExpressionTestHelper.AssertDoubleEqual(1d, float64Alias(context), "Unexpected result for cast(1, float64).");
    }

    [Fact]
    public void Cast_Returns_Expected_Bool_And_String_Value()
    {
        var boolExpression = ExpressionCompiler.CompileBool("cast(True, bool)");
        var booleanAliasExpression = ExpressionCompiler.CompileBool("cast(True, boolean)");
        var stringExpression = ExpressionCompiler.CompileString("cast(\"hello\", string)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        Assert.True(boolExpression(context));
        Assert.True(booleanAliasExpression(context));
        Assert.Equal("hello", stringExpression(context));
    }

    [Fact]
    public void Cast_Supports_Integer_Aliases()
    {
        var int32Expression = ExpressionCompiler.CompileLong("cast(1, int32)");
        var int64Expression = ExpressionCompiler.CompileLong("cast(1, int64)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        Assert.Equal(1L, int32Expression(context));
        Assert.Equal(1L, int64Expression(context));
    }

    [Fact]
    public void Cast_Integrates_With_If_Expression()
    {
        var expression = ExpressionCompiler.CompileLong("if(x > 0, cast(x, int), cast(y, int))");

        Assert.Equal(3L, expression(ExpressionTestHelper.CreateCompilerContext(3.7d, 9.9d)));
        Assert.Equal(9L, expression(ExpressionTestHelper.CreateCompilerContext(-3.7d, 9.9d)));
    }

    [Fact]
    public void Invalid_Cast_Arity_Throws_Expected_FormatException()
    {
        AssertCastFailure(
            "cast(x)",
            ExpressionCompiler.CompileLong,
            typeof(FormatException),
            "cast(value, type) 형식이어야 합니다.");

        AssertCastFailure(
            "cast(x, int, double)",
            ExpressionCompiler.CompileLong,
            typeof(FormatException),
            "cast(value, type) 형식이어야 합니다.");
    }

    [Fact]
    public void Invalid_Cast_Type_Argument_Shape_Throws_Expected_FormatException()
    {
        AssertCastFailure(
            "cast(x, \"int\")",
            ExpressionCompiler.CompileLong,
            typeof(FormatException),
            "cast의 두 번째 인수는 타입명 identifier여야 합니다.");

        AssertCastFailure(
            "cast(x, x + 1)",
            ExpressionCompiler.CompileLong,
            typeof(FormatException),
            "cast의 두 번째 인수는 타입명 identifier여야 합니다.");
    }

    [Fact]
    public void Invalid_Cast_Target_Type_Token_Throws_Expected_NotSupportedException()
    {
        AssertCastFailure(
            "cast(x, float)",
            ExpressionCompiler.CompileLong,
            typeof(NotSupportedException),
            "float/single은 지원하지 않습니다. double을 사용하세요.");

        AssertCastFailure(
            "cast(x, single)",
            ExpressionCompiler.CompileLong,
            typeof(NotSupportedException),
            "float/single은 지원하지 않습니다. double을 사용하세요.");

        AssertCastFailure(
            "cast(x, foo)",
            ExpressionCompiler.CompileLong,
            typeof(NotSupportedException),
            "cast에서 지원하지 않는 타입입니다:");
    }

    [Fact]
    public void Invalid_Cast_Conversion_Throws_Expected_NotSupportedException()
    {
        AssertCastFailure(
            "cast(\"1\", int)",
            ExpressionCompiler.CompileLong,
            typeof(NotSupportedException),
            "지원하지 않는 cast 변환입니다.");

        AssertCastFailure(
            "cast(True, int)",
            ExpressionCompiler.CompileLong,
            typeof(NotSupportedException),
            "지원하지 않는 cast 변환입니다.");

        AssertCastFailure(
            "cast(x, string)",
            ExpressionCompiler.CompileString,
            typeof(NotSupportedException),
            "지원하지 않는 cast 변환입니다.");
    }

    private static void AssertCastFailure<T>(
        string expression,
        Func<string, Func<CommutationTable, T>> compiler,
        Type expectedExceptionType,
        string expectedMessageFragment)
    {
        Func<CommutationTable, T>? compiled = null;
        Exception? actualException = null;

        try
        {
            compiled = compiler(expression);
        }
        catch (Exception ex)
        {
            actualException = ex;
        }

        if (actualException is null && compiled is not null)
        {
            try
            {
                _ = compiled(ExpressionTestHelper.CreateCompilerContext(3.7d, 9.9d));
            }
            catch (Exception ex)
            {
                actualException = ex;
            }
        }

        if (actualException is null)
        {
            throw new XunitException(
                $"Expected '{expression}' to fail with {expectedExceptionType.Name}, but compilation and evaluation both succeeded.");
        }

        Assert.Equal(expectedExceptionType, actualException.GetType());
        Assert.Contains(expectedMessageFragment, actualException.Message);
    }
}
