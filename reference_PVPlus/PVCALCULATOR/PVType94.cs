using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType94 : PVCalculator
    {
        //삼성 납입면제 확장형, 납입면제율 k1에 적용
        public PVType94(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double payCnt = mm(freq);
            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            int 갱신보험기간 = (int)variables["S7"];
            int 갱신종료보험기간 = n;
            double v = (double)variables["v"];

            double[] Cx_납입면제자급부 = Enumerable.Range(0, CommutationTable.MAXSIZE).Select(j => c.Lx_납입자[j] * c.Rate_k1[j] * Math.Pow(v, j + 0.5)).ToArray();
            double[] Mx_납입면제자급부 = c.GetMx(Cx_납입면제자급부);

            double NP1 = (c.Mx_급부[0] - c.Mx_급부[갱신보험기간]) / (payCnt * NNx);
            double NP2 = (c.Mx_급부[갱신보험기간] - c.Mx_급부[갱신종료보험기간]) / (Math.Pow(v, 0.5) * payCnt * NNx) * Enumerable.Range(0, 갱신보험기간).Select(j => Cx_납입면제자급부[j] / c.Dx_유지자[j]).Sum();

            return NP1 + NP2;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double NP = Get순보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            int 갱신보험기간 = (int)variables["S7"];
            int 갱신종료보험기간 = n;
            double v = (double)variables["v"];

            double[] Cx_납입면제자급부 = Enumerable.Range(0, CommutationTable.MAXSIZE).Select(j => c.Lx_납입자[j] * c.Rate_k1[j] * Math.Pow(v, j + 0.5)).ToArray();
            double[] Mx_납입면제자급부 = c.GetMx(Cx_납입면제자급부);

            if (t >= 갱신보험기간) return 0;

            double V1 = (c.Mx_급부[t] - c.Mx_급부[갱신보험기간]) / c.Dx_유지자[t];
            double V2 = NP * (c.Nx_납입자[t] - c.Nx_납입자[m]) / c.Dx_납입자[t];
            double V3 = (c.Mx_급부[갱신보험기간] - c.Mx_급부[갱신종료보험기간]) / (Math.Pow(v, 0.5) * c.Dx_납입자[t]) * Enumerable.Range(0, 갱신보험기간 - t).Select(j => Cx_납입면제자급부[t + j] / c.Dx_유지자[t + j]).Sum();

            return V1 - V2 + V3;
        }
    }
}
