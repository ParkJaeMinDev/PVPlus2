using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType92 : PVCalculator
    {
        //현대 퍼펙트라이프 패키지보험 기납입보험료 환급형, Expnese 가변2: 환급률
        public PVType92(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double 분자 = 0;
            double 분모 = 1.0;

            double payCnt = mm(freq);
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            if (freq == 99)
            {
                분자 = c.Mx_급부[0] - c.Mx_급부[n];
                분모 = c.Dx_유지자[0] - ex.가변2 * c.Dx_유지자[n] / (1 - ex.Alpha_P - ex.Beta_P - ex.Ce);
            }
            else
            {
                분자 = c.Mx_급부[0] - c.Mx_급부[n] + ex.Alpha_S * m * ex.가변2 * c.Dx_유지자[n] / (1 - ex.Alpha_P / APV - ex.Beta_P - ex.Gamma - ex.Ce) / APV;
                분모 = payCnt * (NNx_납입자 - m * ex.가변2 * c.Dx_유지자[n] / (1 - ex.Alpha_P / APV - ex.Beta_P - ex.Gamma - ex.Ce));
            }

            return 분자 / 분모;
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
            double GP = Math.Round(Get영업보험료(n, m, t, freq) * SA) / SA;
            //double GP = Get영업보험료(n, m, t, freq);

            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double 분자 = 0;
            double 분모 = 1.0;

            if (freq == 99)
            {
                분자 = c.Mx_급부[t] - c.Mx_급부[n] + GP * ex.가변2 * c.Dx_유지자[n];
                분모 = c.Dx_유지자[t];
            }
            else
            {
                분자 = c.Mx_급부[t] - c.Mx_급부[n] + ex.가변2 * c.Dx_유지자[n] * payCnt * m * GP - NP * payCnt * NNx_납입자;
                분모 = c.Dx_유지자[t];
            }

            return 분자 / 분모;
        }
    }
}
