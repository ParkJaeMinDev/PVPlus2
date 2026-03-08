using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType93 : PVCalculator
    {
        //한화 납입면제율 산출형
        public PVType93(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double 납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double 유지자 = NNx(c.Nx_유지자, c.Dx_유지자, freq, 0, m);
            double 납입면제자 = 유지자 - 납입자;

            double 납면율 = 납입면제자 / 유지자;

            double Unit순보험료 = (1 - ex.Alpha_P - ex.Beta_P - ex.Gamma - ex.Ce) * 납면율 / (1 - ex.Alpha_P - ex.Beta_P - ex.Gamma - ex.Ce - 납면율);

            return Unit순보험료;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            return 0;
        }
    }

}
