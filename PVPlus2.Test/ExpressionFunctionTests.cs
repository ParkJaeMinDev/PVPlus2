using System;
using System.Reflection;
using PVPlus2.Models;
using PVPlus2.Services;
using Xunit;

namespace PVPlus2.Test;

public class ExpressionFunctionTests
{
    [Fact]
    public void Min_And_Max_Return_Expected_Values()
    {
        var minMany = ExpressionCompiler.CompileDouble("min(3, 1, 2)");
        var maxMany = ExpressionCompiler.CompileDouble("max(1, 5, 3, 2)");
        var minWithContext = ExpressionCompiler.CompileDouble("min(x, y, 10)");
        var maxAsLong = ExpressionCompiler.CompileLong("max(1, 2, 3)");
        var context = ExpressionTestHelper.CreateCompilerContext(7d, 2d);

        ExpressionTestHelper.AssertDoubleEqual(1d, minMany(context), "Unexpected result for min(3, 1, 2).");
        ExpressionTestHelper.AssertDoubleEqual(5d, maxMany(context), "Unexpected result for max(1, 5, 3, 2).");
        ExpressionTestHelper.AssertDoubleEqual(2d, minWithContext(context), "Unexpected result for min(x, y, 10).");
        Assert.Equal(3L, maxAsLong(context));
    }

    [Fact]
    public void Min_And_Max_Follow_Current_Params_Policy()
    {
        var minSingle = ExpressionCompiler.CompileDouble("min(1)");
        var maxSingle = ExpressionCompiler.CompileDouble("max(1)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        ExpressionTestHelper.AssertDoubleEqual(1d, minSingle(context), "Unexpected result for min(1).");
        ExpressionTestHelper.AssertDoubleEqual(1d, maxSingle(context), "Unexpected result for max(1).");

        ExpressionTestHelper.AssertCompilerExpressionFails("min()", ExpressionCompiler.CompileDouble);
        ExpressionTestHelper.AssertCompilerExpressionFails("max()", ExpressionCompiler.CompileDouble);
    }

    [Fact]
    public void Abs_And_Sign_Return_Expected_Values()
    {
        var absLong = ExpressionCompiler.CompileLong("abs(-5)");
        var absDouble = ExpressionCompiler.CompileDouble("abs(-2.5)");
        var signNegative = ExpressionCompiler.CompileLong("sign(-5)");
        var signZero = ExpressionCompiler.CompileLong("sign(0.0)");
        var signPositive = ExpressionCompiler.CompileLong("sign(x)");
        var context = ExpressionTestHelper.CreateCompilerContext(3.7d, 0d);

        Assert.Equal(5L, absLong(context));
        ExpressionTestHelper.AssertDoubleEqual(2.5d, absDouble(context), "Unexpected result for abs(-2.5).");
        Assert.Equal(-1L, signNegative(context));
        Assert.Equal(0L, signZero(context));
        Assert.Equal(1L, signPositive(context));
    }

    [Fact]
    public void Floor_Ceiling_And_Truncate_Return_Expected_Values()
    {
        var floorLong = ExpressionCompiler.CompileLong("floor(5)");
        var floorDouble = ExpressionCompiler.CompileDouble("floor(-2.1)");
        var ceilingLong = ExpressionCompiler.CompileLong("ceiling(5)");
        var ceilingDouble = ExpressionCompiler.CompileDouble("ceiling(-2.1)");
        var truncateLong = ExpressionCompiler.CompileLong("truncate(5)");
        var truncateDouble = ExpressionCompiler.CompileDouble("truncate(-2.9)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        Assert.Equal(5L, floorLong(context));
        ExpressionTestHelper.AssertDoubleEqual(-3d, floorDouble(context), "Unexpected result for floor(-2.1).");
        Assert.Equal(5L, ceilingLong(context));
        ExpressionTestHelper.AssertDoubleEqual(-2d, ceilingDouble(context), "Unexpected result for ceiling(-2.1).");
        Assert.Equal(5L, truncateLong(context));
        ExpressionTestHelper.AssertDoubleEqual(-2d, truncateDouble(context), "Unexpected result for truncate(-2.9).");
    }

    [Fact]
    public void Round_Uses_AwayFromZero_And_Digit_Precision()
    {
        var roundLong = ExpressionCompiler.CompileLong("round(5)");
        var roundPositive = ExpressionCompiler.CompileDouble("round(2.5)");
        var roundNegative = ExpressionCompiler.CompileDouble("round(-2.5)");
        var roundDigitsPositive = ExpressionCompiler.CompileDouble("round(1.25, 1)");
        var roundDigitsNegative = ExpressionCompiler.CompileDouble("round(-1.25, 1)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        Assert.Equal(5L, roundLong(context));
        ExpressionTestHelper.AssertDoubleEqual(3d, roundPositive(context), "Unexpected result for round(2.5).");
        ExpressionTestHelper.AssertDoubleEqual(-3d, roundNegative(context), "Unexpected result for round(-2.5).");
        ExpressionTestHelper.AssertDoubleEqual(1.3d, roundDigitsPositive(context), "Unexpected result for round(1.25, 1).");
        ExpressionTestHelper.AssertDoubleEqual(-1.3d, roundDigitsNegative(context), "Unexpected result for round(-1.25, 1).");
    }

    [Fact]
    public void Pow_And_Sqrt_Return_Expected_Values()
    {
        var powLongLong = ExpressionCompiler.CompileDouble("pow(2, 3)");
        var powMixed = ExpressionCompiler.CompileDouble("pow(9, 0.5)");
        var sqrtLong = ExpressionCompiler.CompileDouble("sqrt(9)");
        var sqrtDouble = ExpressionCompiler.CompileDouble("sqrt(2.25)");
        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);

        ExpressionTestHelper.AssertDoubleEqual(8d, powLongLong(context), "Unexpected result for pow(2, 3).");
        ExpressionTestHelper.AssertDoubleEqual(3d, powMixed(context), "Unexpected result for pow(9, 0.5).");
        ExpressionTestHelper.AssertDoubleEqual(3d, sqrtLong(context), "Unexpected result for sqrt(9).");
        ExpressionTestHelper.AssertDoubleEqual(1.5d, sqrtDouble(context), "Unexpected result for sqrt(2.25).");
    }

    [Fact]
    public void Product_And_Rider_Name_Functions_Return_Expected_Values()
    {
        var productMatch = ExpressionCompiler.CompileBool("ProductNameContains(\"종신\")");
        var productMiss = ExpressionCompiler.CompileBool("ProductNameContains(\"화재\")");
        var riderMatch = ExpressionCompiler.CompileBool("RiderNameContains(\"암\")");
        var combinedMatch = ExpressionCompiler.CompileBool("ProductNameContains(\"종신\") AND RiderNameContains(\"암\")");
        var lowerCaseFunctionName = ExpressionCompiler.CompileBool("productnamecontains(\"종신\")");

        var context = ExpressionTestHelper.CreateCompilerContext(10d, 2d);
        context.상품명 = "종신보험";
        context.담보명 = "암진단특약";

        Assert.True(productMatch(context));
        Assert.False(productMiss(context));
        Assert.True(riderMatch(context));
        Assert.True(combinedMatch(context));
        Assert.True(lowerCaseFunctionName(context));
    }

    [Fact]
    public void Product_And_Rider_Name_Functions_Reject_Invalid_Expression_Arguments()
    {
        ExpressionTestHelper.AssertCompilerExpressionFails("ProductNameContains(1)", ExpressionCompiler.CompileBool);
        ExpressionTestHelper.AssertCompilerExpressionFails("ProductNameContains()", ExpressionCompiler.CompileBool);
        ExpressionTestHelper.AssertCompilerExpressionFails("ProductNameContains(\"a\", \"b\")", ExpressionCompiler.CompileBool);
        ExpressionTestHelper.AssertCompilerExpressionFails("RiderNameContains(1)", ExpressionCompiler.CompileBool);
        ExpressionTestHelper.AssertCompilerExpressionFails("RiderNameContains()", ExpressionCompiler.CompileBool);
        ExpressionTestHelper.AssertCompilerExpressionFails("RiderNameContains(\"a\", \"b\")", ExpressionCompiler.CompileBool);
    }

    [Fact]
    public void Product_And_Rider_Name_Functions_Throw_ArgumentNullException_For_Null_Search_Term()
    {
        var productMethod = GetExpressionFunction("ProductNameContains");
        var riderMethod = GetExpressionFunction("RiderNameContains");
        var context = new ExpressionContext
        {
            상품명 = "종신보험",
            담보명 = "암진단특약"
        };

        var productException = Assert.Throws<TargetInvocationException>(() => productMethod.Invoke(null, [context, null]));
        var riderException = Assert.Throws<TargetInvocationException>(() => riderMethod.Invoke(null, [context, null]));

        Assert.IsType<ArgumentNullException>(productException.InnerException);
        Assert.IsType<ArgumentNullException>(riderException.InnerException);
    }

    [Fact]
    public void Invalid_Function_Arguments_Throw_In_Compiler()
    {
        ExpressionTestHelper.AssertCompilerExpressionFails("abs(True)", ExpressionCompiler.CompileLong);
        ExpressionTestHelper.AssertCompilerExpressionFails("min(True, 1)", ExpressionCompiler.CompileDouble);
        ExpressionTestHelper.AssertCompilerExpressionFails("sqrt(\"a\")", ExpressionCompiler.CompileDouble);
    }

    private static MethodInfo GetExpressionFunction(string methodName)
    {
        var expressionFunctionsType = typeof(ExpressionCompiler).Assembly.GetType("PVPlus2.Services.ExpressionFunctions", throwOnError: true)!;
        return expressionFunctionsType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"ExpressionFunctions method '{methodName}' was not found.");
    }
}
