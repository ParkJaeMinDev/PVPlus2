using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    //한화 2세대 실손(통합 3종 등에 사용), 순보험료를 Round 처리 후 준비금 및 영업보험료 산출
    public class PVType17 : PVCalculator
    {
        public PVType17(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double NP = 0;

            foreach (double[] Mx in c.MxSegments_급부)
            {
                NP += helper.RoundA(GetNPSegment(n, m, t, freq, Mx));
            }

            return NP;
        }

        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            double GP = 0;

            foreach (double[] Mx in c.MxSegments_급부)
            {
                double NP = GetNPSegment(n, m, t, freq, Mx);
                GP += helper.RoundA(GetGPSegment(n, m, t, freq, NP));
            }

            return GP;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double V = 0;

            foreach (double[] Mx in c.MxSegments_급부)
            {
                double NP = GetNPSegment(n, m, t, freq, Mx);
                V += helper.RoundA(GetVSegment(n, m, t, freq, Mx, NP));
            }

            return V;
        }


        public double GetNPSegment(int n, int m, int t, int freq, double[] Mx_급부)
        {
            double NP = 0;
            double payCnt = mm(freq);
            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            if (freq == 99)
            {
                NP = (Mx_급부[0] - Mx_급부[n]) / c.Dx_납입자[0];
            }
            else
            {
                NP = (Mx_급부[0] - Mx_급부[n]) / (payCnt * NNx);
            }

            return NP;
        }

        public double GetGPSegment(int n, int m, int t, int freq, double NP)
        {
            double 분자 = 0;
            double 분모 = 1.0;

            double payCnt = mm(freq);
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            if (freq == 99)
            {
                분자 = NP;
                분모 = 1 - ex.Alpha_P - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce;
            }
            else
            {
                분자 = NP + (ex.Alpha_S + NP * ex.Alpha_Statutory) / (payCnt * APV) + (ex.Beta_S / payCnt);
                분모 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce);
            }

            return 분자 / 분모;
        }

        public double GetVSegment(int n, int m, int t, int freq, double[] Mx_급부, double NP)
        {
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double 분자 = 0;
            double 분모 = 1.0;

            double 분자Out = Mx_급부[t] - Mx_급부[n];
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
