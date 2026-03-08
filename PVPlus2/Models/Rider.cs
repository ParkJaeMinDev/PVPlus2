namespace PVPlus2.Models;

public class Rider
{
    public string 상품코드 { get; set; } = string.Empty;
    public string 담보코드 { get; set; } = string.Empty;
    public string 담보명 { get; set; } = string.Empty;

    public string PV_Type { get; set; } = string.Empty;
    public string 가입금액Expr { get; set; } = string.Empty;
    public string 납입자Expr { get; set; } = string.Empty;
    public string 유지자Expr { get; set; } = string.Empty;

    public List<string> 급부Exprs { get; set; } = [];
    public List<string> 탈퇴Exprs { get; set; } = [];

    public Dictionary<string, string> RateKeyByRateVariable { get; set; } = [];

    public string wExpr { get; set; } = string.Empty;

    public string 납입자급부Expr { get; set; } = string.Empty;
    public string 납입면제자급부Expr { get; set; } = string.Empty;

    public string r1Expr { get; set; } = string.Empty;
    public string r2Expr { get; set; } = string.Empty;
    public string r3Expr { get; set; } = string.Empty;
    public string r4Expr { get; set; } = string.Empty;
    public string r5Expr { get; set; } = string.Empty;
    public string r6Expr { get; set; } = string.Empty;
    public string r7Expr { get; set; } = string.Empty;
    public string r8Expr { get; set; } = string.Empty;
    public string r9Expr { get; set; } = string.Empty;
    public string r10Expr { get; set; } = string.Empty;

    public string k1Expr { get; set; } = string.Empty;
    public string k2Expr { get; set; } = string.Empty;
    public string k3Expr { get; set; } = string.Empty;
    public string k4Expr { get; set; } = string.Empty;
    public string k5Expr { get; set; } = string.Empty;
    public string k6Expr { get; set; } = string.Empty;
    public string k7Expr { get; set; } = string.Empty;
    public string k8Expr { get; set; } = string.Empty;
    public string k9Expr { get; set; } = string.Empty;
    public string k10Expr { get; set; } = string.Empty;

    public string S_Type { get; set; } = string.Empty;
    public string MinSKey { get; set; } = string.Empty;
}
