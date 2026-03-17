using PVPlus2.Services;
using Xunit;

namespace PVPlus2.Test;

public class ExpressionFleeNativeTests
{
    [Fact]
    public void CommonDoubleExpressions_Match_AllEngines()
    {
        foreach (var pair in ExpressionTestCases.CreateCommonDoubleExpressions())
        {
            var compiler = ExpressionCompiler.CompileDouble(pair.Key);
            var flee = ExpressionTestHelper.CreateFleeDoubleEvaluator(pair.Key);

            foreach (var input in ExpressionTestHelper.InputCases)
            {
                var compilerValue = compiler(ExpressionTestHelper.CreateCompilerContext(input.X, input.Y));
                var fleeValue = flee(input.X, input.Y);
                var nativeValue = pair.Value(input.X, input.Y);

                ExpressionTestHelper.AssertDoubleEqual(nativeValue, compilerValue, $"Compiler mismatch for '{pair.Key}' at ({input.X}, {input.Y}).");
                ExpressionTestHelper.AssertDoubleEqual(nativeValue, fleeValue, $"Flee mismatch for '{pair.Key}' at ({input.X}, {input.Y}).");
            }
        }
    }

    [Fact]
    public void CommonLongExpressions_Match_AllEngines()
    {
        foreach (var pair in ExpressionTestCases.CreateCommonLongExpressions())
        {
            var compiler = ExpressionCompiler.CompileLong(pair.Key);
            var flee = ExpressionTestHelper.CreateFleeLongEvaluator(pair.Key);

            foreach (var input in ExpressionTestHelper.InputCases)
            {
                var compilerValue = compiler(ExpressionTestHelper.CreateCompilerContext(input.X, input.Y));
                var fleeValue = flee(input.X, input.Y);
                var nativeValue = pair.Value(input.X, input.Y);

                Assert.Equal(nativeValue, compilerValue);
                Assert.Equal(nativeValue, fleeValue);
            }
        }
    }

    [Fact]
    public void CommonBoolExpressions_Match_AllEngines()
    {
        foreach (var pair in ExpressionTestCases.CreateCommonBoolExpressions())
        {
            var compiler = ExpressionCompiler.CompileBool(pair.Key);
            var flee = ExpressionTestHelper.CreateFleeBoolEvaluator(pair.Key);

            foreach (var input in ExpressionTestHelper.InputCases)
            {
                var compilerValue = compiler(ExpressionTestHelper.CreateCompilerContext(input.X, input.Y));
                var fleeValue = flee(input.X, input.Y);
                var nativeValue = pair.Value(input.X, input.Y);

                Assert.Equal(nativeValue, compilerValue);
                Assert.Equal(nativeValue, fleeValue);
            }
        }
    }

    [Fact]
    public void CommonStringExpressions_Match_AllEngines()
    {
        foreach (var pair in ExpressionTestCases.CreateCommonStringExpressions())
        {
            var compiler = ExpressionCompiler.CompileString(pair.Key);
            var flee = ExpressionTestHelper.CreateFleeStringEvaluator(pair.Key);

            foreach (var input in ExpressionTestHelper.InputCases)
            {
                var compilerValue = compiler(ExpressionTestHelper.CreateCompilerContext(input.X, input.Y));
                var fleeValue = flee(input.X, input.Y);
                var nativeValue = pair.Value(input.X, input.Y);

                Assert.Equal(nativeValue, compilerValue);
                Assert.Equal(nativeValue, fleeValue);
            }
        }
    }
}
