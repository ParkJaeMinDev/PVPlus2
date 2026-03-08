using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class Layout
    {
        public string 상품코드 { get; set; }
        public string 담보코드 { get; set; }

        //데이터 구조가 offset일때
        public int Start { get; set; }
        public int Length { get; set; }

        //데이터 구조가 구분자로 되어있을 때
        public int Index { get; set; }

        public string FactorName { get; set; }
    }
}
