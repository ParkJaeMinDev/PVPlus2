using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class ProductRule
    {
        public string 상품코드 { get; set; }
        public string 판매시기 { get; set; }
        public string 상품명 { get; set; }
        public double 예정이율 { get; set; }
        public double 평균공시이율 { get; set; }
        public int 판매채널 { get; set; }
    }
}
