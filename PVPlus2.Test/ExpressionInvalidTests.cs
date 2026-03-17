using PVPlus2.Services;
using Xunit;

namespace PVPlus2.Test;

public class ExpressionInvalidTests
{
    [Fact]
    public void InvalidDoubleExpressions_Throw_In_Compiler()
    {
        foreach (var expression in ExpressionTestCases.CreateInvalidDoubleExpressions())
        {
            ExpressionTestHelper.AssertCompilerExpressionFails(expression, ExpressionCompiler.CompileDouble);
        }
    }

    [Fact]
    public void InvalidLongExpressions_Throw_In_Compiler()
    {
        foreach (var expression in ExpressionTestCases.CreateInvalidLongExpressions())
        {
            ExpressionTestHelper.AssertCompilerExpressionFails(expression, ExpressionCompiler.CompileLong);
        }
    }

    [Fact]
    public void InvalidBoolExpressions_Throw_In_Compiler()
    {
        foreach (var expression in ExpressionTestCases.CreateInvalidBoolExpressions())
        {
            ExpressionTestHelper.AssertCompilerExpressionFails(expression, ExpressionCompiler.CompileBool);
        }
    }

    [Fact]
    public void InvalidStringExpressions_Throw_In_Compiler()
    {
        foreach (var expression in ExpressionTestCases.CreateInvalidStringExpressions())
        {
            ExpressionTestHelper.AssertCompilerExpressionFails(expression, ExpressionCompiler.CompileString);
        }
    }
}
