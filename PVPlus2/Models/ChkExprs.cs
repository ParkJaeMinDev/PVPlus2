namespace PVPlus2.Models;

public class ChkExprs
{
    public string 회사 { get; set; } = string.Empty;

    public string 조건1 { get; set; } = string.Empty;
    public string 조건2 { get; set; } = string.Empty;
    public string 조건3 { get; set; } = string.Empty;
    public string 조건4 { get; set; } = string.Empty;

    public string 산출항목 { get; set; } = string.Empty;
    public string 산출수식 { get; set; } = string.Empty;

    public double 값 { get; set; }
}
