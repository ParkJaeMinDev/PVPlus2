namespace PVPlus2.Models;

public class Product
{
    public string 상품코드 { get; set; } = string.Empty;
    public string 판매시기 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public double 예정이율 { get; set; }
    public double 평균공시이율 { get; set; }
    public int 판매채널 { get; set; }
}