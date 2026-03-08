using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType101 : PVCalculator
    {
        // 현대 해상 집중납입형
        public PVType101(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double NP = 0;
            double payCnt = mm(freq);
            double NNx1 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, 5);
            double NNx2 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 5, m);

            if ((int)variables["F8"] == 1) t = 5;

            double b = 1.0;
            if (t >= 5) b = 0.25;

            if (freq == 99)
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n]) / c.Dx_납입자[0];
            }
            else
            {
                NP = b * (c.Mx_급부[0] - c.Mx_급부[n]) / (payCnt * (NNx1 + 0.25 * NNx2));
            }

            return NP;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {

            double 순보험료 = Get순보험료(n, m, 0, freq);
            double payCnt = mm(freq);
            double NNx_납입자1 = NNx(c.Nx_납입자, c.Dx_납입자, freq, Math.Min(t, 5), 5);
            double NNx_납입자2 = NNx(c.Nx_납입자, c.Dx_납입자, freq, Math.Max(t, 5), m);

            double 분자 = 0;
            double 분모 = 1.0;

            double b = 0.25;

            double 분자Out = c.Mx_급부[t] - c.Mx_급부[n];
            double 분자In = (t <= m) ? 순보험료 * payCnt * (NNx_납입자1 + b * NNx_납입자2) : 0;

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

        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            double 분자 = 0;
            double 분모 = 1.0;
            t = 0;
            ;
            if ((int)variables["F8"] == 1) t = 5;

            double payCnt = mm(freq);
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double 연납P20 = Get순보험료(n, Math.Min(n, 20), t, 12);

            double NP = Get순보험료(n, m, t, freq);
            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            if (freq == 99)
            {
                분자 = NP;
                분모 = 1 - ex.Alpha_P - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce;
            }
            else
            {
                분자 = NP + (ex.Alpha_S + 연납P20 * ex.Alpha_Statutory) / (payCnt * APV) + (ex.Beta_S / payCnt);
                분모 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce);
            }

            return 분자 / 분모;
        }
    }
}
