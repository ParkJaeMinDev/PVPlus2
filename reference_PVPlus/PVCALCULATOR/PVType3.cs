using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType3 : PVCalculator
    {
        public PVType3(LineInfo line) : base(line)
        {

        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double 순보험료 = Get순보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double 준비금 = 0;

            if (freq == 99)
            {
                준비금 = (c.Mx_급부[t] - c.Mx_급부[n]) / c.Dx_유지자[t];
            }
            else
            {
                // 급부별 지출현가
                for (int i = 0; i < c.MxSegments_급부.Count; i++)
                {
                    준비금 += (c.MxSegments_급부[i][t] - c.MxSegments_급부[i][n]) / c.GetDx(c.LxSegments_유지자[i])[t];
                }

                // 납입자, 납입면제자 급부 지출현가
                준비금 += (c.Mx_납입자급부[t] - c.Mx_납입자급부[n]) / c.Dx_유지자[t];
                준비금 += (c.Mx_납입면제자급부[t] - c.Mx_납입면제자급부[n]) / c.Dx_유지자[t];

                // 보험료 수입현가
                준비금 -= (t <= m) ? 순보험료 * payCnt * NNx_납입자 / c.Dx_유지자[t] : 0;
            }

            return 준비금;
        }
    }
}
