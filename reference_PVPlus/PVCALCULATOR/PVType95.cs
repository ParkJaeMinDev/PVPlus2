using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType95 : PVCalculator
    {
        //현대해상 납입면제후보장강화, 납입면제급부 k1, F6:0(납입상태), F6:1(납입면제상태)
        public PVType95(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double payCnt = mm(freq);
            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double v = (double)variables["v"];

            double NP1 = (c.Mx_급부[0] - c.Mx_급부[n]) / (payCnt * NNx);
            double NP2 = Enumerable.Range(0, m).Select(t_2 => c.Lx_납입자[t_2] * c.Rate_k1[t_2] * Math.Pow(v, t_2 + 0.5) * GetAverageAx(n, m, t_2, freq)).Sum() / (payCnt * NNx);

            return NP1 + NP2;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            bool Is납입면제 = (int)variables["F6"] == 1;
            bool Is완납 = t >= m;

            double NP = Get순보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double v = (double)variables["v"];

            double V = 0;

            if ((int)variables["S5"] > 0)
            {
                double[] Mx_급부 = c.MxSegments_급부합계;
                double[] Mx_납입자해지급부 = c.Mx_납입자급부;
                double[] Mx_납입면제자해지급부 = c.Mx_납입면제자급부;

                if (Is납입면제)
                {
                    return (2 * (Mx_급부[t] - Mx_급부[n]) + (Mx_납입자해지급부[t] - Mx_납입자해지급부[n]) + (Mx_납입면제자해지급부[t] - Mx_납입면제자해지급부[n])) / c.Dx_유지자[t];
                }
                if (Is완납)
                {
                    return ((Mx_급부[t] - Mx_급부[n]) * c.Dx_납입자[t] / c.Dx_유지자[t] + Mx_납입자해지급부[t] - Mx_납입자해지급부[n]) / c.Dx_납입자[t];
                }
                else
                {
                    double V11 = (Mx_급부[t] - Mx_급부[n]) * c.Dx_납입자[t] / c.Dx_유지자[t];
                    double V12 = Mx_납입자해지급부[t] - Mx_납입자해지급부[n];
                    double V13 = Enumerable.Range(t, n - t).Sum(j => c.Lx_유지자[j] * c.Rate_납입면제자급부[j] * Math.Pow(v, j + 0.5)) * c.Dx_납입자[t] / c.Dx_유지자[t];
                    double V14 = Enumerable.Range(t, n - t).Sum(j => c.Lx_납입자[j] * c.Rate_납입면제자급부[j] * Math.Pow(v, j + 0.5));

                    double V1 = (V11 + V12 + V13 - V14) / c.Dx_납입자[t];
                    double V2 = Enumerable.Range(t, m - t).Select(s => c.Lx_납입자[s] * c.Rate_k1[s] * Math.Pow(v, s + 0.5) * GetAverageAx(n, m, s, freq)).Sum() / c.Dx_납입자[t];
                    double V3 = payCnt * NP * NNx_납입자 / c.Dx_납입자[t];

                    V = V1 + V2 - V3;
                }
            }
            else
            {
                double[] Mx_급부 = c.Mx_급부;

                if (Is납입면제)
                {
                    return 2 * (Mx_급부[t] - Mx_급부[n]) / c.Dx_유지자[t];
                }
                if (Is완납)
                {
                    return (Mx_급부[t] - Mx_급부[n]) / c.Dx_유지자[t];
                }
                else
                {
                    double V1 = (c.Mx_급부[t] - c.Mx_급부[n]) / c.Dx_유지자[t];
                    double V2 = Enumerable.Range(t, m - t).Select(s => c.Lx_납입자[s] * c.Rate_k1[s] * Math.Pow(v, s + 0.5) * GetAverageAx(n, m, s, freq)).Sum() / c.Dx_납입자[t];
                    double V3 = payCnt * NP * NNx_납입자 / c.Dx_납입자[t];

                    V = V1 + V2 - V3;
                }
            }

            return V;
        }

        public double GetAverageAx(int n, int m, int t, int 납입주기)
        {
            double[] Mx_급부;

            if ((int)variables["S5"] > 0)
            {
                Mx_급부 = c.MxSegments_급부합계;
            }
            else
            {
                Mx_급부 = c.Mx_급부;
            }


            double A0 = (Mx_급부[t] - Mx_급부[n]) / c.Dx_유지자[t];
            double A1 = (Mx_급부[t + 1] - Mx_급부[n]) / c.Dx_유지자[t + 1];

            return (A0 + A1) / 2;
        }
    }
}
