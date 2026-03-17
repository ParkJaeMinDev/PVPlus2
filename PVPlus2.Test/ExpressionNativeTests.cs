using PVPlus2.Services;
using Xunit;

namespace PVPlus2.Test;

public class ExpressionNativeTests
{
    [Fact]
    public void CompilerOnlyDoubleExpressions_Match_Native()
    {
        foreach (var pair in ExpressionTestCases.CreateCompilerOnlyDoubleExpressions())
        {
            var compiler = ExpressionCompiler.CompileDouble(pair.Key);

            foreach (var input in ExpressionTestHelper.InputCases)
            {
                var compilerValue = compiler(ExpressionTestHelper.CreateCompilerContext(input.X, input.Y));
                var nativeValue = pair.Value(input.X, input.Y);

                ExpressionTestHelper.AssertDoubleEqual(nativeValue, compilerValue, $"Compiler mismatch for '{pair.Key}' at ({input.X}, {input.Y}).");
            }
        }
    }

    [Fact]
    public void CompilerOnlyLongExpressions_Match_Native()
    {
        foreach (var pair in ExpressionTestCases.CreateCompilerOnlyLongExpressions())
        {
            var compiler = ExpressionCompiler.CompileLong(pair.Key);

            foreach (var input in ExpressionTestHelper.InputCases)
            {
                var compilerValue = compiler(ExpressionTestHelper.CreateCompilerContext(input.X, input.Y));
                var nativeValue = pair.Value(input.X, input.Y);

                Assert.Equal(nativeValue, compilerValue);
            }
        }
    }

    [Fact]
    public void CompilerOnlyBoolExpressions_Match_Native()
    {
        foreach (var pair in ExpressionTestCases.CreateCompilerOnlyBoolExpressions())
        {
            var compiler = ExpressionCompiler.CompileBool(pair.Key);

            foreach (var input in ExpressionTestHelper.InputCases)
            {
                var compilerValue = compiler(ExpressionTestHelper.CreateCompilerContext(input.X, input.Y));
                var nativeValue = pair.Value(input.X, input.Y);

                Assert.Equal(nativeValue, compilerValue);
            }
        }
    }

    [Fact]
    public void CompilerOnlyStringExpressions_Match_Native()
    {
        foreach (var pair in ExpressionTestCases.CreateCompilerOnlyStringExpressions())
        {
            var compiler = ExpressionCompiler.CompileString(pair.Key);

            foreach (var input in ExpressionTestHelper.InputCases)
            {
                var compilerValue = compiler(ExpressionTestHelper.CreateCompilerContext(input.X, input.Y));
                var nativeValue = pair.Value(input.X, input.Y);

                Assert.Equal(nativeValue, compilerValue);
            }
        }
    }
}
