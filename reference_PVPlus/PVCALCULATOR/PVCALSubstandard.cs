using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PVPlus.RULES;

namespace PVPlus.PVCALCULATOR
{
    //Special PVTypes
    public class PVSubstandard : PVCalculator
    {
        public PVCalculator sub;
        public PVCalculator norm;

        public PVSubstandard(PVCalculator sub, PVCalculator norm) : base() 
        {
            this.sub = sub;
            this.norm = norm;

            line = norm.line;
            variables = norm.variables;

            key = new PVCalculatorKey();
            Cals[key.GetKey()] = this;
            c = norm.c;
            ex = norm.ex;
            LocalVariables = variables.ToDictionary(x => x.Key, y => y.Value);

            productRule = norm.productRule;
            riderRule = norm.riderRule;
            companyRule = Configure.CompanyRule;

            SA = (double)variables["Amount"];
            Min_S = 0;

            helper.cal = this;
        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            return sub.Get순보험료(n, m, t, freq) - norm.Get순보험료(n, m, t, freq);
        }
        public override double GetBeta순보험료(int n, int m, int t, int freq)
        {
            return sub.Get순보험료(n, m, t, freq) - norm.Get순보험료(n, m, t, freq);
        }
        public override double Get위험보험료(int n, int m, int t, int freq)
        {
            return sub.Get위험보험료(n, m, t, freq) - norm.Get위험보험료(n, m, t, freq);
        }
        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            return sub.Get순보험료(n, m, t, freq) - norm.Get순보험료(n, m, t, freq);
        }
        public override double Get준비금(int n, int m, int t, int freq)
        {
            return sub.Get준비금(n, m, t, freq) - norm.Get준비금(n, m, t, freq);
        }
    }

    public class PVSubStandardRound : PVSubstandard
    {
        //표준하체 Round(S배위험률 요율값) - Round(1배위험률 요율값) 
        public PVSubStandardRound(PVCalculator sub, PVCalculator norm) : base(sub, norm)
        {
        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            return helper.Round2(sub.Get순보험료(n, m, t, freq) * SA) / SA - helper.Round2(norm.Get순보험료(n, m, t, freq) * SA) / SA;
        }
        public override double GetBeta순보험료(int n, int m, int t, int freq)
        {
            return helper.Round2(sub.Get순보험료(n, m, t, freq) * SA) / SA - helper.Round2(norm.Get순보험료(n, m, t, freq) * SA) / SA;
        }
        public override double Get위험보험료(int n, int m, int t, int freq)
        {
            return helper.Round2(sub.Get위험보험료(n, m, t, freq) * SA) / SA - helper.Round2(norm.Get위험보험료(n, m, t, freq) * SA) / SA;
        }
        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            return helper.Round2(sub.Get순보험료(n, m, t, freq) * SA) / SA - helper.Round2(norm.Get순보험료(n, m, t, freq) * SA) / SA;
        }
        public override double Get준비금(int n, int m, int t, int freq)
        {
            return helper.Round2(sub.Get준비금(n, m, t, freq) * SA) / SA - helper.Round2(norm.Get준비금(n, m, t, freq) * SA) / SA;
        }
    }

    public class PVSubstandardSimple : PVSubstandard
    {
        //표준하체 Round(S배위험률 요율값)
        public PVSubstandardSimple(PVCalculator sub, PVCalculator norm) : base(sub, norm)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            return sub.Get순보험료(n, m, t, freq);
        }
        public override double GetBeta순보험료(int n, int m, int t, int freq)
        {
            return sub.Get순보험료(n, m, t, freq);
        }
        public override double Get위험보험료(int n, int m, int t, int freq)
        {
            return sub.Get위험보험료(n, m, t, freq);
        }
        public override double Get저축보험료(int n, int m, int t, int freq)
        {
            return sub.Get저축보험료(n, m, t, freq);
        }
        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            return sub.Get순보험료(n, m, t, freq);
        }
        public override double Get준비금(int n, int m, int t, int freq)
        {
            return sub.Get준비금(n, m, t, freq);
        }
    }

    public class PVSubstandardHanhwaEmergency : PVCalculator
    {
        public PVCalculator org; 
        public PVCalculator sub;
        public PVCalculator norm;

        public PVSubstandardHanhwaEmergency(PVCalculator sub, PVCalculator norm) : base()
        {
            this.sub = sub;
            this.norm = norm;

            line = norm.line;
            variables = norm.variables;
            c = this.norm.c;
            ex = this.norm.ex;

            productRule = norm.productRule;
            riderRule = norm.riderRule;
            companyRule = Configure.CompanyRule;

            SA = (double)variables["Amount"];
            Min_S = 0;

            
            int S2 = (int)variables["S2"];
            variables["S2"] = 0;

            // 납후유지비가 있는 베타순보험료를 산출하기 위함
            org = line.GetPVCalculator();

            //한화에서 표준하체라인에서 무해지형에서 일반형 V끌고 올때는 일반형 V산출시 납후유지비 반영
            //따라서 할증P = 300P + 100P(Beta) - 100P(일반형은 납후유지비 반영) 
            this.norm = line.GetPVCalculator();
            this.norm.ex = new Expense();

            variables["S2"] = S2; 

            helper.cal = this;
        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            return helper.RoundA(sub.Get순보험료(n, m, t, freq)) + (helper.RoundA(org.GetBeta순보험료(n, m, t, freq)) - helper.RoundA(norm.Get순보험료(n, m, t, freq)));
        }
        public override double GetBeta순보험료(int n, int m, int t, int freq)
        {
            return helper.RoundA(sub.Get순보험료(n, m, t, freq)) + (helper.RoundA(org.GetBeta순보험료(n, m, t, freq)) - helper.RoundA(norm.Get순보험료(n, m, t, freq)));
        }
        public override double Get위험보험료(int n, int m, int t, int freq)
        {
            return sub.Get위험보험료(n, m, t, freq) - norm.Get위험보험료(n, m, t, freq);
        }
        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            return helper.RoundA(sub.Get순보험료(n, m, t, freq)) + (helper.RoundA(org.GetBeta순보험료(n, m, t, freq)) - helper.RoundA(norm.Get순보험료(n, m, t, freq)));
        }
        public override double Get준비금(int n, int m, int t, int freq)
        {
            return helper.RoundA(sub.Get준비금(n, m, t, freq)) + (helper.RoundA(org.Get준비금(n, m, t, freq)) - helper.RoundA(norm.Get준비금(n, m, t, freq)));
        }
    }

}
