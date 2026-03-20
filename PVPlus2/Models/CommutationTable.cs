namespace PVPlus2.Models;

public class CommutationTable
{
    // TEST용도
    public double x { get; set; }
    // TEST용도
    public double y { get; set; }
    public string 상품코드 { get; set; }
    public string 판매시기 { get; set; }
    public string 상품명 { get; set; }
    public double 예정이율 { get; set; }
    public double 평균공시이율 { get; set; }
    public int 판매채널 { get; set; }
    public string 담보코드 { get; set; }
    public string 담보명 { get; set; }
    public const long MAXSIZE = 131;
    public long m { get; set; }
    public long n { get; set; }
    public double i { get; set; }
    public double v { get; set; }

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

    public double[] Rate_이율 { get; set; } = new double[MAXSIZE];
    public double[] Rate_할인율 { get; set; } = new double[MAXSIZE];
    public double[] Rate_할인율누계 { get; set; } = new double[MAXSIZE];
    public Dictionary<string, double[]> Rate_위험률 { get; set; } = [];
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
}
