using Flee.PublicTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class VarChg
    {
        public string 상품코드 { get; set; }
        public string 담보코드 { get; set; }
        public string 변수명 { get; set; }
        public IDynamicExpression 값 { get; set; }
    }
}
