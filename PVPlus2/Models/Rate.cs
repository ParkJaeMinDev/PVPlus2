namespace PVPlus2.Models;

public class Rate
{
    public string 위험률명 { get; set; } = string.Empty;
    public string 적용년월 { get; set; } = string.Empty;
    public int 기간 { get; set; }

    public int? F1 { get; set; }
    public int? F2 { get; set; }
    public int? F3 { get; set; }
    public int? F4 { get; set; }
    public int? F5 { get; set; }
    public int? F6 { get; set; }
    public int? F7 { get; set; }
    public int? F8 { get; set; }
    public int? F9 { get; set; }

    public double[] RateArr { get; set; } = [];
}
