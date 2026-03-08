using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType13 : PVType11
    {
        //betaPrime_S - 10년이내: β'_S, 10년이후: F6 
        public PVType13(LineInfo line) : base(line)
        {

        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double NPBeta = GetBeta순보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double NNx_납후유지자1 = NNx(c.Nx_유지자, c.Dx_유지자, 12, Math.Max(m, t), m + 10);
            double NNx_납후유지자2 = NNx(c.Nx_유지자, c.Dx_유지자, 12, Math.Max(m + 10, t), n);

            double 분자 = 0;
            double 분모 = 1.0;

            double 분자Out = (c.Mx_급부[t] - c.Mx_급부[n]) + ex.Betaprime_S * NNx_납후유지자1 + ex.가변1 * NNx_납후유지자2;
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

        public override double GetBeta보험료(int n, int m, int t, int 납입주기)
        {
            //SA 1원당 적용되는 납후유지비에 의한 순보험료 증가액
            double payCnt = mm(납입주기);

            double NNx_납후유지자1 = NNx(c.Nx_유지자, c.Dx_유지자, 12, m, m + 10);
            double NNx_납후유지자2 = NNx(c.Nx_유지자, c.Dx_유지자, 12, m + 10, n);

            double 분자 = NNx_납후유지자1 * ex.Betaprime_S + NNx_납후유지자2 * ex.가변1;
            double 분모 = (납입주기 == 99) ? 100000 : payCnt * NNx(c.Nx_납입자, c.Dx_납입자, 납입주기, 0, m);

            return 분자 / 분모;
        }
    }
}
