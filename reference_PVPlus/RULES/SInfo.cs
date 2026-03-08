using Flee.PublicTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class SInfo
    {
        public string MinSKey { get; set; }
        public string 상품코드 { get; set; }
        public string 담보코드 { get; set; }
        public IGenericExpression<bool> 조건1 { get; set; }
        public IGenericExpression<bool> 조건2 { get; set; }
        public IGenericExpression<bool> 조건3 { get; set; }

        public int Jong { get; set; }
        public int Freq { get; set; }
        public int Age { get; set; }
        public int n { get; set; }
        public int m { get; set; }

        public int F1 { get; set; }
        public int F2 { get; set; }
        public int F3 { get; set; }
        public int F4 { get; set; }
        public int F5 { get; set; }
        public int F6 { get; set; }
        public int F7 { get; set; }
        public int F8 { get; set; }
        public int F9 { get; set; }

        public int S1 { get; set; }
        public int S2 { get; set; }
        public int S3 { get; set; }
        public int S4 { get; set; }
        public int S5 { get; set; }
        public int S6 { get; set; }
        public int S7 { get; set; }
        public int S8 { get; set; }
        public int S9 { get; set; }

        public IGenericExpression<double> 위험보험료Expr { get; set; }
        public IGenericExpression<double> 정기위험보험료Expr { get; set; }
        public IGenericExpression<double> SExpr { get; set; }
        public string ErrorMessage { get; set; }

        public double 위험보험료 { get; set; }
        public double 정기위험보험료 { get; set; }
        public double S { get; set; }

        public override string ToString()
        {
            // Return a string representation of the SInfo object with tab -separated values
            return $"{MinSKey}\t{상품코드}\t{담보코드}\t{조건1.Text}\t{조건2.Text}\t{조건3.Text}\t{Jong}\t{Freq}\t{Age}\t{n}\t{m}\t" +
                   $"{F1}\t{F2}\t{F3}\t{F4}\t{F5}\t{F6}\t{F7}\t{F8}\t{F9}\t" +
                   $"{S1}\t{S2}\t{S3}\t{S4}\t{S5}\t{S6}\t{S7}\t{S8}\t{S9}\t" +
                   $"{위험보험료}\t{정기위험보험료}\t{S}\t{ErrorMessage}";            
        }
    }

}
