using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PVPlus.PVCALCULATOR
{
    public class PVType11 : PVCalculator
    {
        public PVType11(LineInfo line) : base(line)
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

            if (freq == 99)
            {
                분자 = NPBeta;
                분모 = 1 - ex.Alpha_P - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce;
            }
            else
            {
                분자 = NPBeta + (ex.Alpha_S + NPSTD * ex.Alpha_Statutory) / (payCnt * APV) + (ex.Beta_S / payCnt);
                분모 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce);
            }

            return 분자 / 분모;

        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double NPBeta =  GetBeta순보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double NNx_납후유지자 = NNx(c.Nx_유지자, c.Dx_유지자, 12, Math.Max(m, t), n);

            double 분자 = 0;
            double 분모 = 1.0;

            double 분자Out = (c.Mx_급부[t] - c.Mx_급부[n]) + ex.Betaprime_S * NNx_납후유지자;
            double 분자In = (m > 0 && t <= m) ? NPBeta * payCnt * NNx_납입자 : 0;

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

        public override double GetBeta순보험료(int n, int m, int t, int freq)
        {
            return Get순보험료(n, m, t, freq) + GetBeta보험료(n, m, t, freq);
        }

        public virtual double GetBeta보험료(int n, int m, int t, int freq)
        {
            //SA 1원당 적용되는 납후유지비에 의한 순보험료 증가액

            double betaPrime = ex.Betaprime_S;
            double payCnt = mm(freq);

            double 분자 = NNx(c.Nx_유지자, c.Dx_유지자, 12, m, n);
            double 분모 = (freq == 99) ? 100000 : payCnt * NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            return betaPrime * 분자 / 분모;
        }
    }
}
