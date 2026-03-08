namespace PVPlus2.Models;

public class Expense
{
    public string 상품코드 { get; set; } = string.Empty;
    public string 담보코드 { get; set; } = string.Empty;

    public string 조건1 { get; set; } = string.Empty;
    public string 조건2 { get; set; } = string.Empty;
    public string 조건3 { get; set; } = string.Empty;
    public string 조건4 { get; set; } = string.Empty;

    public string Alpha_S { get; set; } = string.Empty;
    public string Alpha_P { get; set; } = string.Empty;
    public string Alpha2_P { get; set; } = string.Empty;
    public string Alpha_Statutory { get; set; } = string.Empty;
    public string Beta_S { get; set; } = string.Empty;
    public string Beta_P { get; set; } = string.Empty;
    public string Betaprime_S { get; set; } = string.Empty;
    public string Betaprime_P { get; set; } = string.Empty;
    public string Gamma { get; set; } = string.Empty;
    public string Ce { get; set; } = string.Empty;

    public string Refund_Rate_S { get; set; } = string.Empty;
    public string Refund_Rate_P { get; set; } = string.Empty;

    public string 가변1 { get; set; } = string.Empty;
    public string 가변2 { get; set; } = string.Empty;
}
