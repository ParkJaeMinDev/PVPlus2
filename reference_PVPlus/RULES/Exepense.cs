using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class Expense
    {
        public string 상품코드 { get; set; }
        public string 담보코드 { get; set; }
        public string 조건1 { get; set; }
        public string 조건2 { get; set; }
        public string 조건3 { get; set; }
        public string 조건4 { get; set; }

        public double Alpha_S { get; set; }
        public double Alpha_P { get; set; }
        public double Alpha2_P { get; set; }
        public double Alpha_Statutory { get; set; }
        public double Beta_S { get; set; }
        public double Beta_P { get; set; }
        public double Betaprime_S { get; set; }
        public double Betaprime_P { get; set; }
        public double Gamma { get; set; }
        public double Ce { get; set; }

        //사업비는 아니지만 보기,납기에 따라 적용되는 특성이 사업비와 같음
        public double Refund_Rate_S { get; set; }
        public double Refund_Rate_P { get; set; }

        //특이 상품 확장용도
        public double 가변1 { get; set; }
        public double 가변2 { get; set; }
    }
}
