using PVPlus.RULES;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType104 : PVType11
    {
        // 납입지원특약
        public PVType104(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double NP = 0;
            double payCnt = mm(freq);
            double APV = ax(c.Nx_유지자, c.Dx_유지자, freq, 0, m);

            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NNx_유지자 = NNx(c.Nx_유지자, c.Dx_유지자, freq, 0, m);
            double pi = (NNx_유지자 - NNx_납입자) / NNx_유지자;

            double 분모 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce);

            if (freq == 99)
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n]) / c.Dx_납입자[0];
            }
            else
            {
                return pi * 분모 / (분모 - pi);
            }

            return 0;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double NP = Get순보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double NNx_유지자 = NNx(c.Nx_유지자, c.Dx_유지자, freq, t, m);

            double V = 0;

            if (freq == 99)
            {

            }
            else
            {
                double 분자 = payCnt * (NNx_유지자 - NNx_납입자 - NP * NNx_납입자);
                double 분모 = c.Dx_유지자[t];
                V = 분자 / 분모;
            }

            return V;
        }
    }
}
