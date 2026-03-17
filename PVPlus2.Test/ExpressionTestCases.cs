using System.Text;

namespace PVPlus2.Test;

internal static class ExpressionTestCases
{
    internal static IReadOnlyDictionary<string, Func<double, double, double>> CreateCommonDoubleExpressions()
    {
        return CreateCommonExpressionSet(CreateNativeDoubleExpressions(), CreateCommonDoubleExclusions());
    }

    internal static IReadOnlyDictionary<string, Func<double, double, double>> CreateCompilerOnlyDoubleExpressions()
    {
        return CreateCompilerOnlyExpressionSet(CreateNativeDoubleExpressions(), CreateCommonDoubleExclusions());
    }

    internal static IReadOnlyDictionary<string, Func<double, double, long>> CreateCommonLongExpressions()
    {
        return CreateCommonExpressionSet(CreateNativeLongExpressions(), CreateCommonLongExclusions());
    }

    internal static IReadOnlyDictionary<string, Func<double, double, long>> CreateCompilerOnlyLongExpressions()
    {
        return CreateCompilerOnlyExpressionSet(CreateNativeLongExpressions(), CreateCommonLongExclusions());
    }

    internal static IReadOnlyDictionary<string, Func<double, double, bool>> CreateCommonBoolExpressions()
    {
        return CreateCommonExpressionSet(CreateNativeBoolExpressions(), CreateCommonBoolExclusions());
    }

    internal static IReadOnlyDictionary<string, Func<double, double, bool>> CreateCompilerOnlyBoolExpressions()
    {
        return CreateCompilerOnlyExpressionSet(CreateNativeBoolExpressions(), CreateCommonBoolExclusions());
    }

    internal static IReadOnlyDictionary<string, Func<double, double, string>> CreateCommonStringExpressions()
    {
        return CreateCommonExpressionSet(CreateNativeStringExpressions(), CreateCommonStringExclusions());
    }

    internal static IReadOnlyDictionary<string, Func<double, double, string>> CreateCompilerOnlyStringExpressions()
    {
        return CreateCompilerOnlyExpressionSet(CreateNativeStringExpressions(), CreateCommonStringExclusions());
    }

    internal static IReadOnlyDictionary<string, TDelegate> CreateCommonExpressionSet<TDelegate>(
        IReadOnlyDictionary<string, TDelegate> source,
        HashSet<string> excludedOriginalExpressions)
    {
        var result = new Dictionary<string, TDelegate>(StringComparer.Ordinal);

        foreach (var pair in source)
        {
            if (excludedOriginalExpressions.Contains(pair.Key))
            {
                continue;
            }

            var canonical = CanonicalizeExpression(pair.Key);
            result.TryAdd(canonical, pair.Value);
        }

        return result;
    }

    internal static IReadOnlyDictionary<string, TDelegate> CreateCompilerOnlyExpressionSet<TDelegate>(
        IReadOnlyDictionary<string, TDelegate> source,
        HashSet<string> excludedOriginalExpressions)
    {
        var result = new Dictionary<string, TDelegate>(StringComparer.Ordinal);

        foreach (var pair in source)
        {
            var canonical = CanonicalizeExpression(pair.Key);
            if (excludedOriginalExpressions.Contains(pair.Key) || !string.Equals(pair.Key, canonical, StringComparison.Ordinal))
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }

    internal static string CanonicalizeExpression(string expression)
    {
        var builder = new StringBuilder(expression.Length);
        var inString = false;

        for (var i = 0; i < expression.Length; i++)
        {
            var current = expression[i];

            if (current == '"')
            {
                inString = !inString;
                builder.Append(current);
                continue;
            }

            if (!inString && i + 1 < expression.Length)
            {
                var next = expression[i + 1];

                if (current == '=' && next == '=')
                {
                    builder.Append('=');
                    i++;
                    continue;
                }

                if (current == '!' && next == '=')
                {
                    builder.Append("<>");
                    i++;
                    continue;
                }
            }

            builder.Append(current);
        }

        return builder.ToString();
    }

    internal static HashSet<string> CreateCommonDoubleExclusions()
    {
        return new HashSet<string>(StringComparer.Ordinal)
        {
            "10 / 4",
            "2 ^ 3 ^ 2",
            "-2 ^ 2",
            "0 / 0",
            "test(x) + 1"
        };
    }

    internal static HashSet<string> CreateCommonLongExclusions()
    {
        return new HashSet<string>(StringComparer.Ordinal)
        {
            "+10",
            "test(10)",
            "test(10) + 5"
        };
    }

    internal static HashSet<string> CreateCommonBoolExclusions()
    {
        return new HashSet<string>(StringComparer.Ordinal)
        {
            "+True"
        };
    }

    internal static HashSet<string> CreateCommonStringExclusions()
    {
        return new HashSet<string>(StringComparer.Ordinal)
        {
            "\"test=\" + test(10)",
            "\"testd=\" + test(10.5)"
        };
    }

    internal static Dictionary<string, Func<double, double, double>> CreateNativeDoubleExpressions()
    {
        return new Dictionary<string, Func<double, double, double>>(StringComparer.Ordinal)
        {
            ["1 + 2 * 3"] = static (x, y) => 1 + 2 * 3,
            ["2 ^ 3 ^ 2"] = static (x, y) => Math.Pow(2, Math.Pow(3, 2)),
            ["(1 + 2) * (3 - 1)"] = static (x, y) => (1 + 2) * (3 - 1),
            ["10 / 4"] = static (x, y) => 10.0 / 4.0,
            ["-1 + 3"] = static (x, y) => -1 + 3,
            ["-2 ^ 2"] = static (x, y) => Math.Pow(-2, 2),
            ["0 / 0"] = static (x, y) => 0.0 / 0.0,
            ["10 + 20 * 3"] = static (x, y) => 10 + 20 * 3,
            ["10.5 / 2"] = static (x, y) => 10.5 / 2,
            ["-5 + 10"] = static (x, y) => -5 + 10,
            ["10 % 3"] = static (x, y) => 10 % 3,
            ["(1 + 2) * 3"] = static (x, y) => (1 + 2) * 3,
            [".5 * 2"] = static (x, y) => 0.5 * 2,
            ["x + y"] = static (x, y) => x + y,
            ["x - y"] = static (x, y) => x - y,
            ["x * 2 + y / 2"] = static (x, y) => x * 2 + y / 2,
            ["(x + 10) / (y + 1)"] = static (x, y) => (x + 10) / (y + 1),
            ["x ^ 2"] = static (x, y) => Math.Pow(x, 2),
            ["(x + y) % 7"] = static (x, y) => (x + y) % 7,
            ["test(x) + 1"] = static (x, y) => x + 1
        };
    }

    internal static Dictionary<string, Func<double, double, long>> CreateNativeLongExpressions()
    {
        return new Dictionary<string, Func<double, double, long>>(StringComparer.Ordinal)
        {
            ["1 + 2"] = static (x, y) => 1 + 2,
            ["10 - 3"] = static (x, y) => 10 - 3,
            ["2 * 3 * 4"] = static (x, y) => 2 * 3 * 4,
            ["20 % 3"] = static (x, y) => 20 % 3,
            ["-5 + 10"] = static (x, y) => -5 + 10,
            ["((1 + 2) * 3) - 4"] = static (x, y) => ((1 + 2) * 3) - 4,
            ["100 - 50 + 25"] = static (x, y) => 100 - 50 + 25,
            ["7 % 5 + 2"] = static (x, y) => 7 % 5 + 2,
            ["3 * (4 + 5)"] = static (x, y) => 3 * (4 + 5),
            ["999 - 1"] = static (x, y) => 999 - 1,
            ["test(10)"] = static (x, y) => 10,
            ["test(10) + 5"] = static (x, y) => 10 + 5,
            ["100 % 10"] = static (x, y) => 100 % 10,
            ["50 - (3 * 7)"] = static (x, y) => 50 - (3 * 7),
            ["2 * 2 * 2 * 2"] = static (x, y) => 2 * 2 * 2 * 2,
            ["1 + 2 + 3 + 4 + 5"] = static (x, y) => 1 + 2 + 3 + 4 + 5,
            ["1000 % 97"] = static (x, y) => 1000 % 97,
            ["(20 - 3) * 2"] = static (x, y) => (20 - 3) * 2,
            ["+10"] = static (x, y) => 10,
            ["(3 + 7) % 4"] = static (x, y) => (3 + 7) % 4
        };
    }

    internal static Dictionary<string, Func<double, double, bool>> CreateNativeBoolExpressions()
    {
        return new Dictionary<string, Func<double, double, bool>>(StringComparer.Ordinal)
        {
            ["1 <> 2"] = static (x, y) => 1 != 2,
            ["NOT (1 == 2)"] = static (x, y) => !(1 == 2),
            ["TRUE AND NOT FALSE"] = static (x, y) => true && !false,
            ["1 + 2 == 3 AND 4 > 3"] = static (x, y) => 1 + 2 == 3 && 4 > 3,
            ["1 + 2 == 3.0"] = static (x, y) => 1 + 2 == 3.0,
            ["+True"] = static (x, y) => true,
            ["True"] = static (x, y) => true,
            ["False"] = static (x, y) => false,
            ["10 == 10.0"] = static (x, y) => 10 == 10.0,
            ["1 != 2"] = static (x, y) => 1 != 2,
            ["1 > 0 AND 2 < 5"] = static (x, y) => 1 > 0 && 2 < 5,
            ["x > y"] = static (x, y) => x > y,
            ["x >= y"] = static (x, y) => x >= y,
            ["x < y OR y < x"] = static (x, y) => x < y || y < x,
            ["NOT (x == y)"] = static (x, y) => !(x == y),
            ["\"a\" = \"a\""] = static (x, y) => "a" == "a",
            ["\"a\" <> \"b\""] = static (x, y) => "a" != "b",
            ["\"x=\" + 1 == \"x=1\""] = static (x, y) => "x=" + 1 == "x=1",
            ["\"A\" + True != \"AFalse\""] = static (x, y) => "A" + true != "AFalse",
            ["(x + y) > (x - y)"] = static (x, y) => (x + y) > (x - y)
        };
    }

    internal static Dictionary<string, Func<double, double, string>> CreateNativeStringExpressions()
    {
        return new Dictionary<string, Func<double, double, string>>(StringComparer.Ordinal)
        {
            ["\"hello\""] = static (x, y) => "hello",
            ["\"A\" + \"B\""] = static (x, y) => "AB",
            ["\"x=\" + x"] = static (x, y) => "x=" + x,
            ["\"y=\" + y"] = static (x, y) => "y=" + y,
            ["x + \"원\""] = static (x, y) => x + "원",
            ["\"sum=\" + (x + y)"] = static (x, y) => "sum=" + (x + y),
            ["\"flag=\" + True"] = static (x, y) => "flag=" + true,
            ["False + \" value\""] = static (x, y) => false + " value",
            ["\"mix:\" + x + \",\" + y"] = static (x, y) => "mix:" + x + "," + y,
            ["\"prefix-\" + 10"] = static (x, y) => "prefix-" + 10,
            ["10 + \"-suffix\""] = static (x, y) => 10 + "-suffix",
            ["\"pow=\" + (2 ^ 3)"] = static (x, y) => "pow=" + Math.Pow(2, 3),
            ["\"mod=\" + (10 % 3)"] = static (x, y) => "mod=" + (10 % 3),
            ["\"cmp=\" + (1 < 2)"] = static (x, y) => "cmp=" + (1 < 2),
            ["\"nested=\" + ((1 + 2) * 3)"] = static (x, y) => "nested=" + ((1 + 2) * 3),
            ["\"test=\" + test(10)"] = static (x, y) => "test=" + 10,
            ["\"testd=\" + test(10.5)"] = static (x, y) => "testd=" + 10.5,
            ["\"eq=\" + (\"a\" == \"a\")"] = static (x, y) => "eq=" + ("a" == "a"),
            ["\"xy=\" + x + y"] = static (x, y) => "xy=" + x + y,
            ["\"literal with space\""] = static (x, y) => "literal with space"
        };
    }

    internal static string[] CreateInvalidDoubleExpressions()
    {
        return
        [
            "1e10",
            "1.2.3",
            "1 % 0",
            "1e-5",
            "1,000 + 2",
            "()",
            "True",
            "False",
            "\"abc\"",
            "\"a\" + \"b\"",
            "NOT 1",
            "1 < 2 < 3",
            "unknown(1)",
            "test(True)",
            "x AND y",
            "1 +",
            "(1 + 2",
            "\"a\" == \"a\"",
            "1 = 1",
            "\"a\" == 1"
        ];
    }

    internal static string[] CreateInvalidLongExpressions()
    {
        return
        [
            "1e10",
            "1.2.3",
            "1 % 0",
            "1e-5",
            "1,000 + 2",
            "()",
            "True",
            "False",
            "\"abc\"",
            "\"a\" + \"b\"",
            "NOT 1",
            "1 < 2 < 3",
            "unknown(1)",
            "test(True)",
            "1 = 1",
            "\"a\" = \"a\"",
            "True AND False",
            "1 +",
            "(1 + 2",
            "-True"
        ];
    }

    internal static string[] CreateInvalidBoolExpressions()
    {
        return
        [
            "1 < 2 < 3",
            "-True",
            "True == 1",
            "NOT 1",
            "True > False",
            "1 AND 2",
            "\"a\"",
            "\"a\" + \"b\"",
            "1 + 2",
            "x + y",
            "unknown(1)",
            "test(True)",
            "1e10",
            "1.2.3",
            "()",
            "1 +",
            "(True",
            "\"a\" == 1",
            "1 == \"a\"",
            "\"a\" > \"b\""
        ];
    }

    internal static string[] CreateInvalidStringExpressions()
    {
        return
        [
            "1 + 2",
            "True",
            "1 = 1",
            "\"a\" - \"b\"",
            "\"a\" * 3",
            "\"a\" / 2",
            "\"a\" % 2",
            "NOT \"a\"",
            "-\"a\"",
            "+\"a\"",
            "\"a\" > \"b\"",
            "\"a\" AND \"b\"",
            "unknown(1)",
            "test(True)",
            "1e10",
            "1.2.3",
            "()",
            "1 +",
            "\"a\" == 1",
            "1 == \"a\""
        ];
    }
}
