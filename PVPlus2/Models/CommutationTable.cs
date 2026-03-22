#pragma warning disable IDE1006
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace PVPlus2.Models;

public class CommutationTable
{
    // TEST용도
    public double x { get; set; }
    // TEST용도
    public double y { get; set; }
    public const long MAXSIZE = 131;
    public string Company { get; set; } = string.Empty;
    public string 상품코드 { get; set; } = string.Empty;
    public string 판매시기 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public double 예정이율 { get; set; }
    public double 평균공시이율 { get; set; }
    public long 판매채널 { get; set; }
    public long Channel { get; set; }
    public string 담보코드 { get; set; } = string.Empty;
    public string 담보명 { get; set; } = string.Empty;
    public long m { get; set; }
    public long n { get; set; }
    public double i { get; set; }
    public double ii { get; set; }
    public double v { get; set; }
    public double vv { get; set; }
    public long Age { get; set; }
    public long Freq { get; set; }
    public double Amount { get; set; }
    public long PV_Type { get; set; }
    public long S_Type { get; set; }
    public long Jong { get; set; }
    public long ElapseYear { get; set; }

    public long F1 { get; set; }
    public long F2 { get; set; }
    public long F3 { get; set; }
    public long F4 { get; set; }
    public long F5 { get; set; }
    public long F6 { get; set; }
    public long F7 { get; set; }
    public long F8 { get; set; }
    public long F9 { get; set; }
    public long F10 { get; set; }

    public long S1 { get; set; }
    public long S2 { get; set; }
    public long S3 { get; set; }
    public long S4 { get; set; }
    public long S5 { get; set; }
    public long S6 { get; set; }
    public long S7 { get; set; }
    public long S8 { get; set; }
    public long S9 { get; set; }
    public long S10 { get; set; }

    public double[] Rate_이율 { get; set; } = new double[MAXSIZE];
    public double[] Rate_할인율 { get; set; } = new double[MAXSIZE];
    public double[] Rate_할인율누계 { get; set; } = new double[MAXSIZE];
    public double[] Rate_할인율누계_Cx { get; set; } = new double[MAXSIZE];
    public double[] Rate_해지율 { get; set; } = new double[MAXSIZE];
    public double[] Rate_유지자 { get; set; } = new double[MAXSIZE];
    public double[] Rate_납입자 { get; set; } = new double[MAXSIZE];
    public double[] Rate_납입자급부 { get; set; } = new double[MAXSIZE];
    public double[] Rate_납입면제자급부 { get; set; } = new double[MAXSIZE];

    public double[] q1 { get; set; } = new double[MAXSIZE];
    public double[] q2 { get; set; } = new double[MAXSIZE];
    public double[] q3 { get; set; } = new double[MAXSIZE];
    public double[] q4 { get; set; } = new double[MAXSIZE];
    public double[] q5 { get; set; } = new double[MAXSIZE];
    public double[] q6 { get; set; } = new double[MAXSIZE];
    public double[] q7 { get; set; } = new double[MAXSIZE];
    public double[] q8 { get; set; } = new double[MAXSIZE];
    public double[] q9 { get; set; } = new double[MAXSIZE];
    public double[] q10 { get; set; } = new double[MAXSIZE];
    public double[] q11 { get; set; } = new double[MAXSIZE];
    public double[] q12 { get; set; } = new double[MAXSIZE];
    public double[] q13 { get; set; } = new double[MAXSIZE];
    public double[] q14 { get; set; } = new double[MAXSIZE];
    public double[] q15 { get; set; } = new double[MAXSIZE];
    public double[] q16 { get; set; } = new double[MAXSIZE];
    public double[] q17 { get; set; } = new double[MAXSIZE];
    public double[] q18 { get; set; } = new double[MAXSIZE];
    public double[] q19 { get; set; } = new double[MAXSIZE];
    public double[] q20 { get; set; } = new double[MAXSIZE];
    public double[] q21 { get; set; } = new double[MAXSIZE];
    public double[] q22 { get; set; } = new double[MAXSIZE];
    public double[] q23 { get; set; } = new double[MAXSIZE];
    public double[] q24 { get; set; } = new double[MAXSIZE];
    public double[] q25 { get; set; } = new double[MAXSIZE];
    public double[] q26 { get; set; } = new double[MAXSIZE];
    public double[] q27 { get; set; } = new double[MAXSIZE];
    public double[] q28 { get; set; } = new double[MAXSIZE];
    public double[] q29 { get; set; } = new double[MAXSIZE];
    public double[] q30 { get; set; } = new double[MAXSIZE];
    public double[] w { get; set; } = new double[MAXSIZE];

    public double[] k1 { get; set; } = new double[MAXSIZE];
    public double[] k2 { get; set; } = new double[MAXSIZE];
    public double[] k3 { get; set; } = new double[MAXSIZE];
    public double[] k4 { get; set; } = new double[MAXSIZE];
    public double[] k5 { get; set; } = new double[MAXSIZE];
    public double[] k6 { get; set; } = new double[MAXSIZE];
    public double[] k7 { get; set; } = new double[MAXSIZE];
    public double[] k8 { get; set; } = new double[MAXSIZE];
    public double[] k9 { get; set; } = new double[MAXSIZE];
    public double[] k10 { get; set; } = new double[MAXSIZE];

    public double[] r1 { get; set; } = new double[MAXSIZE];
    public double[] r2 { get; set; } = new double[MAXSIZE];
    public double[] r3 { get; set; } = new double[MAXSIZE];
    public double[] r4 { get; set; } = new double[MAXSIZE];
    public double[] r5 { get; set; } = new double[MAXSIZE];
    public double[] r6 { get; set; } = new double[MAXSIZE];
    public double[] r7 { get; set; } = new double[MAXSIZE];
    public double[] r8 { get; set; } = new double[MAXSIZE];
    public double[] r9 { get; set; } = new double[MAXSIZE];
    public double[] r10 { get; set; } = new double[MAXSIZE];

    public List<double[]> RateSegments_급부 { get; set; } = [];
    public List<double[]> RateSegments_유지자 { get; set; } = [];

    public double[] Lx_납입자 { get; set; } = new double[MAXSIZE];
    public double[] Lx_유지자 { get; set; } = new double[MAXSIZE];
    public double[] Lx_납입면제자 { get; set; } = new double[MAXSIZE];
    public double[] Dx_납입자 { get; set; } = new double[MAXSIZE];
    public double[] Dx_유지자 { get; set; } = new double[MAXSIZE];
    public double[] Nx_납입자 { get; set; } = new double[MAXSIZE];
    public double[] Nx_유지자 { get; set; } = new double[MAXSIZE];
    public double[] Cx_납입자급부 { get; set; } = new double[MAXSIZE];
    public double[] Cx_납입면제자급부 { get; set; } = new double[MAXSIZE];
    public double[] MxSegments_급부합계 { get; set; } = new double[MAXSIZE];
    public double[] Mx_납입자급부 { get; set; } = new double[MAXSIZE];
    public double[] Mx_납입면제자급부 { get; set; } = new double[MAXSIZE];
    public double[] Mx_급부 { get; set; } = new double[MAXSIZE];

    public List<double[]> LxSegments_유지자 { get; set; } = [];
    public List<double[]> CxSegments_급부 { get; set; } = [];
    public List<double[]> MxSegments_급부 { get; set; } = [];

    public string Substandard_Mode { get; set; } = string.Empty;
    public string TempStr1 { get; set; } = string.Empty;
    public string TempStr2 { get; set; } = string.Empty;
    public string TempStr3 { get; set; } = string.Empty;
    public string TempStr4 { get; set; } = string.Empty;
    public string TempStr5 { get; set; } = string.Empty;
    public string TempStr6 { get; set; } = string.Empty;
    public string TempStr7 { get; set; } = string.Empty;
    public string TempStr8 { get; set; } = string.Empty;
    public string TempStr9 { get; set; } = string.Empty;
    public string TempStr10 { get; set; } = string.Empty;
    public long TempInt1 { get; set; }
    public long TempInt2 { get; set; }
    public long TempInt3 { get; set; }
    public long TempInt4 { get; set; }
    public long TempInt5 { get; set; }
    public long TempInt6 { get; set; }
    public long TempInt7 { get; set; }
    public long TempInt8 { get; set; }
    public long TempInt9 { get; set; }
    public long TempInt10 { get; set; }
    public double TempDouble1 { get; set; }
    public double TempDouble2 { get; set; }
    public double TempDouble3 { get; set; }
    public double TempDouble4 { get; set; }
    public double TempDouble5 { get; set; }
    public double TempDouble6 { get; set; }
    public double TempDouble7 { get; set; }
    public double TempDouble8 { get; set; }
    public double TempDouble9 { get; set; }
    public double TempDouble10 { get; set; }

    public void FillPrefixProducts(double[] source, double[] target)
    {
        double acc = 1.0;

        for (int i = 0; i < target.Length; i++)
        {
            target[i] = acc;
            acc *= source[i];
        }
    }

    public void FillPrefixProducts_Cx(double[] source, double[] target)
    {
        double acc = 1.0;

        for (int i = 0; i < target.Length; i++)
        {
            target[i] = acc * Math.Sqrt(source[i]);
            acc *= source[i];
        }
    }

    public double[] GetLx(double[] Rate)
    {
        double[] Lx = new double[MAXSIZE];
        FillLx(Rate, Lx);
        return Lx;
    }

    public void FillLx(double[] Rate, double[] Lx)
    {
        double acc = 100000.0;
        Lx[0] = acc;

        for (int t = 0; t < n; t++)
        {
            acc *= Rate[t];
            Lx[t + 1] = acc;
        }
    }

    public double[] GetDx(double[] Lx)
    {
        double[] Dx = new double[MAXSIZE];

        FillDx(Lx, Dx);

        return Dx;
    }

    public void FillDx_Scalar(double[] Lx, double[] Dx)
    {
        int count = (int)n + 1;
        double[] rateDiscount = Rate_할인율누계;

        for (int t = 0; t < count; t++)
        {
            Dx[t] = Lx[t] * rateDiscount[t];
        }
    }

    public void FillDx(double[] Lx, double[] Dx)
    {
        if (!Avx.IsSupported)
        {
            FillDx_Scalar(Lx, Dx);
            return;
        }

        int count = (int)n + 1;

        ref double lxRef = ref MemoryMarshal.GetArrayDataReference(Lx);
        ref double dxRef = ref MemoryMarshal.GetArrayDataReference(Dx);
        ref double rateRef = ref MemoryMarshal.GetArrayDataReference(Rate_할인율누계);

        int simdCount = count & ~3; // 4 doubles = 256 bits, int simdCount = count - (count % 4) 의 비트연산 버전
        int t = 0;

        for (; t < simdCount; t += 4)
        {
            Vector256<double> lxVec = Vector256.LoadUnsafe(ref Unsafe.Add(ref lxRef, t));

            Vector256<double> rateVec = Vector256.LoadUnsafe(ref Unsafe.Add(ref rateRef, t));

            Vector256<double> dxVec = Avx.Multiply(lxVec, rateVec);
            dxVec.StoreUnsafe(ref Unsafe.Add(ref dxRef, t));
        }

        for (; t < count; t++)
        {
            Unsafe.Add(ref dxRef, t) = Unsafe.Add(ref lxRef, t) * Unsafe.Add(ref rateRef, t);
        }
    }

    public double[] GetNx(double[] Dx)
    {
        double[] Nx = new double[MAXSIZE];
        FillNx(Dx, Nx);
        return Nx;
    }

    public void FillNx(double[] Dx, double[] Nx)
    {
        int count = (int)n;
        double acc = 0.0;

        for (int t = count; t >= 0; t--)
        {
            acc += Dx[t];
            Nx[t] = acc;
        }
    }

    public double[] GetCx(double[] Lx, double[] Rate)
    {
        double[] Cx = new double[MAXSIZE];
        FillCx(Lx, Rate, Cx);
        return Cx;
    }

    public void FillCx_Scalar(double[] Lx, double[] Rate, double[] Cx)
    {
        for (int t = 0; t < n; t++)
        {
            Cx[t] = Lx[t] * Rate[t] * Rate_할인율누계_Cx[t];
        }
    }

    public void FillCx(double[] Lx, double[] Rate, double[] Cx)
    {
        if (!Avx.IsSupported)
        {
            FillCx_Scalar(Lx, Rate, Cx);
            return;
        }

        int count = (int)n;

        ref double lxRef = ref MemoryMarshal.GetArrayDataReference(Lx);
        ref double rateRef = ref MemoryMarshal.GetArrayDataReference(Rate);
        ref double cxRef = ref MemoryMarshal.GetArrayDataReference(Cx);
        ref double discountCxRef = ref MemoryMarshal.GetArrayDataReference(Rate_할인율누계_Cx);

        int simdCount = count & ~3; // 4 doubles = 256 bits
        int t = 0;

        for (; t < simdCount; t += 4)
        {
            Vector256<double> lxVec = Vector256.LoadUnsafe(ref Unsafe.Add(ref lxRef, t));
            Vector256<double> rateVec = Vector256.LoadUnsafe(ref Unsafe.Add(ref rateRef, t));
            Vector256<double> discountCxVec = Vector256.LoadUnsafe(ref Unsafe.Add(ref discountCxRef, t));

            Vector256<double> cxVec = Avx.Multiply(Avx.Multiply(lxVec, rateVec), discountCxVec);
            cxVec.StoreUnsafe(ref Unsafe.Add(ref cxRef, t));
        }

        for (; t < count; t++)
        {
            Unsafe.Add(ref cxRef, t) =
                Unsafe.Add(ref lxRef, t) *
                Unsafe.Add(ref rateRef, t) *
                Unsafe.Add(ref discountCxRef, t);
        }
    }

    public double[] GetMx(double[] Cx)
    {
        double[] Mx = new double[MAXSIZE];
        FillMx(Cx, Mx);
        return Mx;
    }

    public void FillMx(double[] Cx, double[] Mx)
    {
        int count = (int)n;
        double acc = 0.0;

        for (int t = count; t >= 0; t--)
        {
            acc += Cx[t];
            Mx[t] = acc;
        }
    }

}
#pragma warning restore IDE1006
