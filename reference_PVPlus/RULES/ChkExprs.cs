using Flee.PublicTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class ChkExprs
    {
        public string 회사 { get; set; }
        public IGenericExpression<bool> 조건1 { get; set; }
        public IGenericExpression<bool> 조건2 { get; set; }
        public IGenericExpression<bool> 조건3 { get; set; }
        public IGenericExpression<bool> 조건4 { get; set; }

        public string 산출항목 { get; set; }
        public IGenericExpression<double> 산출수식 { get; set; }
        public double 값 { get; set; }
    }
}
