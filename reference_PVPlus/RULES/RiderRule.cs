using Flee.PublicTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class RiderRule
    {
        public string 상품코드 { get; set; }
        public string 담보코드 { get; set; }
        public string 담보명 { get; set; }

        public IGenericExpression<int> PV_Type { get; set; }
        public IGenericExpression<double> 가입금액Expr { get; set; }
        public IGenericExpression<double> 납입자Expr { get; set; }
        public IGenericExpression<double> 유지자Expr { get; set; }

        public List<IGenericExpression<double>> 급부Exprs { get; set; }
        public List<IGenericExpression<double>> 탈퇴Exprs { get; set; }

        public Dictionary<string, string> RateKeyByRateVariable { get; set; }

        public IGenericExpression<double> wExpr { get; set; }

        public IGenericExpression<double> 납입자급부Expr { get; set; }
        public IGenericExpression<double> 납입면제자급부Expr { get; set; }

        public IGenericExpression<double> r1Expr { get; set; }
        public IGenericExpression<double> r2Expr { get; set; }
        public IGenericExpression<double> r3Expr { get; set; }
        public IGenericExpression<double> r4Expr { get; set; }
        public IGenericExpression<double> r5Expr { get; set; }
        public IGenericExpression<double> r6Expr { get; set; }
        public IGenericExpression<double> r7Expr { get; set; }
        public IGenericExpression<double> r8Expr { get; set; }
        public IGenericExpression<double> r9Expr { get; set; }
        public IGenericExpression<double> r10Expr { get; set; }

        public IGenericExpression<double> k1Expr { get; set; }
        public IGenericExpression<double> k2Expr { get; set; }
        public IGenericExpression<double> k3Expr { get; set; }
        public IGenericExpression<double> k4Expr { get; set; }
        public IGenericExpression<double> k5Expr { get; set; }
        public IGenericExpression<double> k6Expr { get; set; }
        public IGenericExpression<double> k7Expr { get; set; }
        public IGenericExpression<double> k8Expr { get; set; }
        public IGenericExpression<double> k9Expr { get; set; }
        public IGenericExpression<double> k10Expr { get; set; }

        public IGenericExpression<int> S_Type { get; set; }
        public string MinSKey { get; set; }
    }

}
