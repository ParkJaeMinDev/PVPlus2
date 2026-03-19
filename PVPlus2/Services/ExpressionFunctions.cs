using PVPlus2.Models;

namespace PVPlus2.Services;

internal static class ExpressionFunctions
{
    public static long test(long a) => a;

    public static double test(double a) => a;

    public static double Min(params double[] values) => values.Min();

    public static double Max(params double[] values) => values.Max();

    public static long Abs(long value) => Math.Abs(value);

    public static double Abs(double value) => Math.Abs(value);

    public static long Floor(long value) => value;

    public static double Floor(double value) => Math.Floor(value);

    public static long Ceiling(long value) => value;

    public static double Ceiling(double value) => Math.Ceiling(value);

    public static long Round(long value) => value;

    public static double Round(double value) => Math.Round(value, MidpointRounding.AwayFromZero);

    public static long Round(long value, long digits) => value;

    public static double Round(double value, long digits) => Math.Round(value, checked((int)digits), MidpointRounding.AwayFromZero);

    public static double Pow(long x, long y) => Math.Pow(x, y);

    public static double Pow(double x, double y) => Math.Pow(x, y);

    public static double Pow(long x, double y) => Math.Pow(x, y);

    public static double Pow(double x, long y) => Math.Pow(x, y);

    public static double Sqrt(long value) => Math.Sqrt(value);

    public static double Sqrt(double value) => Math.Sqrt(value);

    public static long Truncate(long value) => value;

    public static double Truncate(double value) => Math.Truncate(value);

    public static long Sign(long value) => Math.Sign(value);

    public static long Sign(double value) => Math.Sign(value);

    public static bool ProductNameContains(ExpressionContext context, string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return (context.상품명 ?? string.Empty).Contains(s, StringComparison.Ordinal);
    }

    public static bool RiderNameContains(ExpressionContext context, string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return (context.담보명 ?? string.Empty).Contains(s, StringComparison.Ordinal);
    }
}
