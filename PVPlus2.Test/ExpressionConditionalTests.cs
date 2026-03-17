using PVPlus2.Services;
using Xunit;

namespace PVPlus2.Test;

public class ExpressionConditionalTests
{
    [Fact]
    public void If_Returns_Expected_Long_Value()
    {
        var whenTrue = ExpressionCompiler.CompileLong("if(10 > 1, 5, 2)");
        var whenFalse = ExpressionCompiler.CompileLong("if(10 < 1, 5, 2)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        Assert.Equal(5L, whenTrue(context));
        Assert.Equal(2L, whenFalse(context));
    }

    [Fact]
    public void If_Promotes_Mixed_Numeric_Branches_To_Double()
    {
        var whenTrue = ExpressionCompiler.CompileDouble("if(True, 1, 2.5)");
        var whenFalse = ExpressionCompiler.CompileDouble("if(False, 1, 2.5)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        ExpressionTestHelper.AssertDoubleEqual(1d, whenTrue(context), "Unexpected result for numeric if(true, ...).");
        ExpressionTestHelper.AssertDoubleEqual(2.5d, whenFalse(context), "Unexpected result for numeric if(false, ...).");
    }

    [Fact]
    public void If_Returns_Expected_Bool_And_String_Value()
    {
        var boolExpression = ExpressionCompiler.CompileBool("if(False, True, False)");
        var stringExpression = ExpressionCompiler.CompileString("if(True, \"A\", \"B\")");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        Assert.False(boolExpression(context));
        Assert.Equal("A", stringExpression(context));
    }

    [Fact]
    public void If_ShortCircuits_Unused_Branch()
    {
        var whenTrue = ExpressionCompiler.CompileLong("if(True, 5, 1 % 0)");
        var whenFalse = ExpressionCompiler.CompileLong("if(False, 1 % 0, 5)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        Assert.Equal(5L, whenTrue(context));
        Assert.Equal(5L, whenFalse(context));
    }

    [Fact]
    public void Ifs_Returns_First_Matching_Value_Or_Default()
    {
        var expression = ExpressionCompiler.CompileString("ifs(x > 10, \"high\", x > 5, \"mid\", \"low\")");

        Assert.Equal("high", expression(ExpressionTestHelper.CreateCompilerContext(12d, 0d)));
        Assert.Equal("mid", expression(ExpressionTestHelper.CreateCompilerContext(7d, 0d)));
        Assert.Equal("low", expression(ExpressionTestHelper.CreateCompilerContext(2d, 0d)));
    }

    [Fact]
    public void Ifs_ShortCircuits_Remaining_Conditions_And_Values()
    {
        var shortCircuitDefault = ExpressionCompiler.CompileLong("ifs(True, 5, 1 % 0)");
        var shortCircuitEarlierValue = ExpressionCompiler.CompileLong("ifs(False, 1 % 0, True, 7, 9)");
        var shortCircuitLaterCondition = ExpressionCompiler.CompileLong("ifs(True, 5, 1 % 0 = 0, 7, 9)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        Assert.Equal(5L, shortCircuitDefault(context));
        Assert.Equal(7L, shortCircuitEarlierValue(context));
        Assert.Equal(5L, shortCircuitLaterCondition(context));
    }

    [Fact]
    public void Invalid_If_Expressions_Throw_In_Compiler()
    {
        string[] expressions =
        [
            "if(True, 1)",
            "if(True, 1, 2, 3)",
            "if(1, 2, 3)",
            "if(True, 1, \"x\")"
        ];

        foreach (var expression in expressions)
        {
            ExpressionTestHelper.AssertCompilerExpressionFails(expression, ExpressionCompiler.CompileLong);
        }
    }

    [Fact]
    public void Invalid_Ifs_Expressions_Throw_In_Compiler()
    {
        string[] expressions =
        [
            "ifs(True, 1)",
            "ifs(True, 1, False, 2)",
            "ifs(1, 2, 3)",
            "ifs(True, 1, False, \"x\", 0)"
        ];

        foreach (var expression in expressions)
        {
            ExpressionTestHelper.AssertCompilerExpressionFails(expression, ExpressionCompiler.CompileLong);
        }
    }
}
