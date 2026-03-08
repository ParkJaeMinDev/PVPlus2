using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType16 : PVType11
    {
        //납입기간동안 가입금액비례 유지비 존재
        public PVType16(LineInfo line) : base(line)
        {

        }

        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            double 분자 = 0;
            double 분모 = 1.0;

            double payCnt = mm(freq);
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = GetBeta순보험료(n, m, t, freq);
            double NPSTD = Get기준연납순보험료(n, m, t, 12);

            double NNx_납입 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NNx_유지 = NNx(c.Nx_유지자, c.Dx_유지자, 12, 0, m);
            double NNx_납후 = NNx(c.Nx_유지자, c.Dx_유지자, 12, m, n);

            if (freq == 99)
            {
                분자 = NPBeta;
                분모 = 1 - ex.Alpha_P - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce;
            }
            else
            {
                double 분자1 = NPBeta + (ex.Alpha_S + NPSTD * ex.Alpha_Statutory) / (payCnt * APV);
                double 분모1 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce - ex.Betaprime_P * NNx_납후 / NNx_납입);
                double GP1 = 분자1 / 분모1;

                double 분자2 = NPBeta + (ex.Alpha_S + NPSTD * ex.Alpha_Statutory) / (payCnt * APV) + ex.Beta_S * NNx_유지 / NNx_납입 / payCnt;
                double 분모2 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.가변1 - ex.Gamma - ex.Ce - ex.Betaprime_P * NNx_납후 / NNx_납입);
                double GP2 = 분자2 / 분모2;

                double 유지비1 = GP1 * ex.Beta_P;
                double 유지비2 = GP2 * ex.가변1 + ex.Beta_S * NNx_유지 / NNx_납입 / payCnt;

                if (유지비1 <= 유지비2 || (int)variables["S1"] == 0) return GP1;
                else return GP2;

            }

            return 분자 / 분모;
        }
    }
}
