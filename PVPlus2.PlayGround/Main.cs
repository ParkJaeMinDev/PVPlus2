using System.Diagnostics;
using PVPlus2.Models;

namespace PVPlus2.PlayGround;

internal static class Program
{
    private const int ScenarioN = 30;
    private const int TotalCallCount = 100_000;
    private const int BatchSize = 100;
    private const int BatchCount = TotalCallCount / BatchSize;
    private static readonly double TimestampToNanoseconds = 1_000_000_000.0 / Stopwatch.Frequency;
    private static readonly string WorkspaceDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    private static readonly string OutputPath = Path.Combine(WorkspaceDirectory, "GetCx_jit_trace_100_calls_codex.txt");
    private static double _sink;

    private static void Main()
    {
        int size = (int)CommutationTable.MAXSIZE;

        CommutationTable table = new()
        {
            n = ScenarioN,
            Rate_이율 = CreateInterestRates(size),
        };

        InitializeDiscountArrays(table);

        double[] rate = CreateRateInput(size);
        double[] survivalRates = CreateSurvivalRates(size);
        double[] lx = CreateLx(table, survivalRates, size);

        string[] lines = new string[BatchCount];

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        for (int batchIndex = 0; batchIndex < BatchCount; batchIndex++)
        {
            long start = Stopwatch.GetTimestamp();

            for (int i = 0; i < BatchSize; i++)
            {
                double[] cx = table.GetCx(lx, rate);
                _sink += ComputeChecksum(cx, ScenarioN);
                GC.KeepAlive(cx);
            }

            long elapsedTicks = Stopwatch.GetTimestamp() - start;
            double totalNanoseconds = elapsedTicks * TimestampToNanoseconds;
            double nanosecondsPerCall = totalNanoseconds / BatchSize;
            int startCall = (batchIndex * BatchSize) + 1;
            int endCall = startCall + BatchSize - 1;

            lines[batchIndex] =
                $"{batchIndex + 1},{startCall},{endCall},{elapsedTicks},{totalNanoseconds:F3},{nanosecondsPerCall:F3}";
        }

        File.WriteAllLines(OutputPath, lines);

        Console.WriteLine("GetCx JIT Trace (100-call batches)");
        Console.WriteLine($"n                 : {ScenarioN}");
        Console.WriteLine($"total calls       : {TotalCallCount:N0}");
        Console.WriteLine($"batch size        : {BatchSize:N0}");
        Console.WriteLine($"batch count       : {BatchCount:N0}");
        Console.WriteLine($"timer frequency   : {Stopwatch.Frequency:N0} ticks/sec");
        Console.WriteLine($"output            : {OutputPath}");
        Console.WriteLine($"checksum sink     : {_sink:R}");
    }

    private static double[] CreateInterestRates(int size)
    {
        double[] interestRates = new double[size];

        for (int i = 0; i < interestRates.Length; i++)
        {
            interestRates[i] = 0.0200 + ((i % 10) * 0.0010);
        }

        return interestRates;
    }

    private static void InitializeDiscountArrays(CommutationTable table)
    {
        for (int i = 0; i < table.Rate_할인율.Length; i++)
        {
            table.Rate_할인율[i] = 1.0 / (1.0 + table.Rate_이율[i]);
        }

        table.FillPrefixProducts(table.Rate_할인율, table.Rate_할인율누계);
        table.FillPrefixProducts_Cx(table.Rate_할인율, table.Rate_할인율누계_Cx);
    }

    private static double[] CreateRateInput(int size)
    {
        double[] rate = new double[size];

        for (int i = 0; i < rate.Length; i++)
        {
            rate[i] = 0.0010 + ((i % 9) * 0.0001);
        }

        return rate;
    }

    private static double[] CreateSurvivalRates(int size)
    {
        double[] survivalRates = new double[size];

        for (int i = 0; i < survivalRates.Length; i++)
        {
            survivalRates[i] = 0.9950 + ((i % 10) * 0.0001);
        }

        return survivalRates;
    }

    private static double[] CreateLx(CommutationTable table, double[] survivalRates, int size)
    {
        double[] lx = new double[size];
        table.FillLx(survivalRates, lx);
        return lx;
    }

    private static double ComputeChecksum(double[] cx, int count)
    {
        return cx[0] + cx[count / 2] + cx[count - 1];
    }
}
