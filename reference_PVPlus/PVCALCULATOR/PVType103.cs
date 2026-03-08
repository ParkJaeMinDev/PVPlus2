using PVPlus.RULES;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType103 : PVType11
    {
        // 납입지원특약
        public PVType103(LineInfo line) : base(line)
        {
        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double NP = 0;
            double payCnt = mm(freq);

            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NNx_유지자 = NNx(c.Nx_유지자, c.Dx_유지자, freq, 0, m);
            double pi = (NNx_유지자 - NNx_납입자) / NNx_유지자;

            if (freq == 99)
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n]) / c.Dx_납입자[0];
            }
            else
            {
                double 분자 = pi * (1 - ex.Alpha_S * c.Dx_유지자[0] / NNx_유지자 - ex.Beta_P - ex.Gamma - ex.Ce);
                double 분모 = (1 - ex.Alpha_S * c.Dx_유지자[0] / NNx_유지자 - ex.Beta_P - ex.Gamma - ex.Ce - pi);

                return 분자 / 분모;
            }

            return NP;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double NP = Get순보험료(n, m, t, freq);
            double GP = Get영업보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double NNx_유지자 = NNx(c.Nx_유지자, c.Dx_유지자, freq, t, m);

            double V = 0;

            if (freq == 99)
            {

            }
            else
            {
                double 준비금In = (1 + GP - NP) * payCnt * NNx_유지자 / c.Dx_유지자[t];
                double 준비금Out = (1 + GP) * payCnt * NNx_납입자 / c.Dx_납입자[t];
                V = 준비금In - 준비금Out;


            }

            return V;
        }
    }
}
