using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType2 : PVCalculator
    {
        //SA 만기환급형, RefundRate_S: 환급율
        public PVType2(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double NP = 0;
            double payCnt = mm(freq);
            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            if (freq == 99)
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n] + ex.Refund_Rate_S * c.Dx_유지자[n]) / c.Dx_납입자[0];
            }
            else
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n] + ex.Refund_Rate_S * c.Dx_유지자[n]) / (payCnt * NNx);
            }

            return NP;
        }

        public override double Get위험보험료(int n, int m, int t, int freq)
        {
            double NP = 0;
            double payCnt = mm(freq);
            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            if (freq == 99)
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n]) / c.Dx_납입자[0];
            }
            else
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n]) / (payCnt * NNx);
            }

            return NP;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double NP = Get순보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double 분자 = 0;
            double 분모 = 1.0;

            double 분자Out = c.Mx_급부[t] - c.Mx_급부[n] + ex.Refund_Rate_S * c.Dx_유지자[n];
            double 분자In = (m > 0 && t <= m) ? NP * payCnt * NNx_납입자 : 0;

            if (freq == 99)
            {
                분자 = 분자Out;
                분모 = c.Dx_유지자[t];
            }
            else
            {
                분자 = 분자Out - 분자In;
                분모 = c.Dx_유지자[t];
            }

            return 분자 / 분모;
        }
    }
}
