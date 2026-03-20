using System;
using PVPlus2.Models;
using PVPlus2.Services;
using Xunit;

namespace PVPlus2.Test;

public class ExpressionArrayTests
{
    private static readonly int MaxSize = checked((int)CommutationTable.MAXSIZE);

    [Fact]
    public void CompileDoubleArrayInto_Writes_ElementWise_Arithmetic_Result_Up_To_N()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("Rate_이율 + k1 * 2");
        var context = CreateArrayContext(n: 5);
        var target = new double[MaxSize];

        expression(context, target);

        AssertComputedPrefixAndZeroTail(
            target,
            context.n,
            i => context.Rate_이율[i] + (context.k1[i] * 2d),
            "Unexpected element-wise arithmetic result.");
    }

    [Fact]
    public void CompileDoubleArrayInto_Mixes_Array_And_Scalar_Context_Value_Up_To_N()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("Rate_이율 * 예정이율");
        var context = CreateArrayContext(n: 5);
        var target = new double[MaxSize];

        expression(context, target);

        AssertComputedPrefixAndZeroTail(
            target,
            context.n,
            i => context.Rate_이율[i] * context.예정이율,
            "Unexpected array-scalar multiplication result.");
    }

    [Fact]
    public void CompileDoubleArrayInto_Supports_If_Per_Element_Up_To_N()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("if(Rate_이율 > 0.05, k1, 0)");
        var context = CreateArrayContext(n: 5);
        var target = new double[MaxSize];

        expression(context, target);

        AssertComputedPrefixAndZeroTail(
            target,
            context.n,
            i => context.Rate_이율[i] > 0.05d ? context.k1[i] : 0d,
            "Unexpected if(...) array result.");
    }

    [Fact]
    public void CompileDoubleArrayInto_Supports_Ifs_Per_Element_Up_To_N()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto(
            "ifs(Rate_이율 > 0.07, k1, Rate_이율 > 0.03, k2, 0)");
        var context = CreateArrayContext(n: 5);
        var target = new double[MaxSize];

        expression(context, target);

        AssertComputedPrefixAndZeroTail(
            target,
            context.n,
            i => context.Rate_이율[i] > 0.07d
                ? context.k1[i]
                : context.Rate_이율[i] > 0.03d
                    ? context.k2[i]
                    : 0d,
            "Unexpected ifs(...) array result.");
    }

    [Fact]
    public void CompileDoubleArrayAssignment_Writes_Into_Target_Property_Up_To_N()
    {
        var assignment = ExpressionCompiler.CompileDoubleArrayAssignment(
            "Rate_할인율",
            "Rate_이율 + 예정이율");
        var context = CreateArrayContext(n: 5);

        assignment(context);

        AssertComputedPrefixAndZeroTail(
            context.Rate_할인율,
            context.n,
            i => context.Rate_이율[i] + context.예정이율,
            "Unexpected array assignment result.");
    }

    [Fact]
    public void CompileDoubleArrayInto_Supports_Internal_T_Identifier()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("t");
        var context = CreateArrayContext(n: 5);
        var target = new double[MaxSize];

        expression(context, target);

        AssertComputedPrefixAndZeroTail(
            target,
            context.n,
            i => i,
            "Unexpected result for internal t identifier.");
    }

    [Fact]
    public void CompileDoubleArrayInto_Supports_T_In_Conditional()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("if(t < 2, 0.99, 0.98)");
        var context = CreateArrayContext(n: 5);
        var target = new double[MaxSize];

        expression(context, target);

        AssertComputedPrefixAndZeroTail(
            target,
            context.n,
            i => i < 2 ? 0.99d : 0.98d,
            "Unexpected result for conditional t expression.");
    }

    [Fact]
    public void CompileDoubleArrayInto_Computes_All_Elements_When_N_Is_MaxSizeMinusOne()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("1");
        var context = CreateArrayContext(n: CommutationTable.MAXSIZE - 1);
        var target = new double[MaxSize];

        expression(context, target);

        AssertComputedPrefixAndZeroTail(
            target,
            context.n,
            _ => 1d,
            "Unexpected result at MAXSIZE - 1.");
    }

    [Fact]
    public void CompileDoubleArrayInto_Throws_When_Target_Length_Is_Smaller_Than_N_Plus_1()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("Rate_이율");
        var context = CreateArrayContext(n: 10);
        var target = new double[10];

        var exception = Assert.Throws<ArgumentException>(() => expression(context, target));

        Assert.Contains("target length is smaller than n + 1.", exception.Message);
    }

    [Fact]
    public void CompileDoubleArrayInto_Throws_When_Source_Length_Is_Smaller_Than_N_Plus_1()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("Rate_이율 + 1");
        var context = CreateArrayContext(n: 10);
        var target = new double[MaxSize];

        context.Rate_이율 = new double[10];

        var exception = Assert.Throws<ArgumentException>(() => expression(context, target));

        Assert.Contains("source 'Rate_이율' length is smaller than n + 1.", exception.Message);
    }

    [Fact]
    public void CompileDoubleArrayInto_Throws_When_N_Is_Negative()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("1");
        var context = CreateArrayContext(n: -1);
        var target = new double[MaxSize];

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => expression(context, target));

        Assert.Contains("context.n must be between 0 and MAXSIZE - 1.", exception.Message);
    }

    [Fact]
    public void CompileDoubleArrayInto_Throws_When_N_Is_MaxSize()
    {
        var expression = ExpressionCompiler.CompileDoubleArrayInto("1");
        var context = CreateArrayContext(n: CommutationTable.MAXSIZE);
        var target = new double[MaxSize];

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => expression(context, target));

        Assert.Contains("context.n must be between 0 and MAXSIZE - 1.", exception.Message);
    }

    [Fact]
    public void CompileDoubleArrayAssignment_Rejects_NonArray_Target_Property()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            ExpressionCompiler.CompileDoubleArrayAssignment("예정이율", "Rate_이율"));

        Assert.Contains("is not double[]", exception.Message);
    }

    [Fact]
    public void CompileDoubleArrayInto_Rejects_Float_Cast_Alias()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            ExpressionCompiler.CompileDoubleArrayInto("cast(Rate_이율, float)"));

        Assert.Contains("float/single은 지원하지 않습니다. double을 사용하세요.", exception.Message);
    }

    [Fact]
    public void CompileDouble_Rejects_T_Outside_Array_Expressions()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => ExpressionCompiler.CompileDouble("t"));

        Assert.Contains("'t' is only valid inside array expressions.", exception.Message);
    }

    private static CommutationTable CreateArrayContext(long n)
    {
        var context = new CommutationTable
        {
            n = n,
            예정이율 = 1.5d,
            Rate_이율 = new double[MaxSize],
            Rate_할인율 = new double[MaxSize],
            k1 = new double[MaxSize],
            k2 = new double[MaxSize]
        };

        for (var i = 0; i < MaxSize; i++)
        {
            context.Rate_이율[i] = 0.001d * (i + 1);
            context.k1[i] = i + 10d;
            context.k2[i] = (i + 1) * 100d;
        }

        return context;
    }

    private static void AssertComputedPrefixAndZeroTail(
        double[] actual,
        long lastIndex,
        Func<int, double> expectedFactory,
        string message)
    {
        Assert.Equal(MaxSize, actual.Length);

        var maxComputedIndex = checked((int)lastIndex);

        for (var i = 0; i <= maxComputedIndex; i++)
        {
            ExpressionTestHelper.AssertDoubleEqual(
                expectedFactory(i),
                actual[i],
                $"{message} index={i}");
        }

        for (var i = maxComputedIndex + 1; i < actual.Length; i++)
        {
            ExpressionTestHelper.AssertDoubleEqual(
                0d,
                actual[i],
                $"{message} tail index={i}");
        }
    }
}
