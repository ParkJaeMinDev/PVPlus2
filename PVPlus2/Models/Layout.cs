namespace PVPlus2.Models;

public class Layout
{
    public string 상품코드 { get; set; } = string.Empty;
    public string 담보코드 { get; set; } = string.Empty;
    public int Start { get; set; }
    public int Length { get; set; }
    public int Index { get; set; }
    public string FactorName { get; set; } = string.Empty;
}
