using Flee.PublicTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class ExpenseRule
    {
        public string 상품코드 { get; set; }
        public string 담보코드 { get; set; }
        public IGenericExpression<bool> 조건1 { get; set; }
        public IGenericExpression<bool> 조건2 { get; set; }
        public IGenericExpression<bool> 조건3 { get; set; }
        public IGenericExpression<bool> 조건4 { get; set; }

        public IGenericExpression<double> Alpha_S_Expr { get; set; }
        public IGenericExpression<double> Alpha_P_Expr { get; set; }
        public IGenericExpression<double> Alpha2_P_Expr { get; set; }
        public IGenericExpression<double> Alpha_Statutory_Expr { get; set; }
        public IGenericExpression<double> Beta_S_Expr { get; set; }
        public IGenericExpression<double> Beta_P_Expr { get; set; }
        public IGenericExpression<double> Betaprime_S_Expr { get; set; }
        public IGenericExpression<double> Betaprime_P_Expr { get; set; }
        public IGenericExpression<double> Gamma_Expr { get; set; }
        public IGenericExpression<double> Ce_Expr { get; set; }

        public IGenericExpression<double> Refund_Rate_S_Expr { get; set; }
        public IGenericExpression<double> Refund_Rate_P_Expr { get; set; }

        public IGenericExpression<double> 가변1_Expr { get; set; }
        public IGenericExpression<double> 가변2_Expr { get; set; }
    }
}
