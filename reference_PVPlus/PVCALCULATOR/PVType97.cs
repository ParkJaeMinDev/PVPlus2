using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType97 : PVCalculator
    {
        //한화 납입면제 고급형
        //F6:갱신보험기간(n1), n:갱신종료보험기간(n2)
        //S7:일반형(0), 납입면제 고급형(1)

        public PVType97(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            if (freq == 99) return base.Get순보험료(n, m, t, freq);
            if ((int)variables["S7"] == 0) return base.Get순보험료(n, m, t, freq);

            int n1 = (int)variables["F6"];
            int n2 = n;

            double v = (double)variables["v"];
            double payCnt = mm(freq);
            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            double[] Cx_납입면제자급부 = Enumerable.Range(0, CommutationTable.MAXSIZE).Select(j => c.Lx_납입자[j] * c.Rate_r1[j] * Math.Pow(v, j + 0.5)).ToArray();
            double[] Mx_납입면제자급부 = c.GetMx(Cx_납입면제자급부);
            double Ax = (c.Mx_급부[n1] - c.Mx_급부[n2]) / c.Dx_유지자[n1];

            double NP1 = (c.Mx_급부[0] - c.Mx_급부[n1]) / (payCnt * NNx);
            double NP2 = Ax * c.Dx_유지자[n1] * Enumerable.Range(0, n1).Select(j => Cx_납입면제자급부[j] / (c.Dx_유지자[j] + c.Dx_유지자[j + 1]) * (c.Lx_유지자[j] + c.Lx_유지자[j + 1]) / c.Lx_유지자[j]).Sum() / (payCnt * NNx);

            return NP1 + NP2;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            if ((int)variables["S7"] == 0) return base.Get준비금(n, m, t, freq);

            int n1 = (int)variables["F6"];
            int n2 = n;

            double NP = Get순보험료(n, m, t, freq);
            double v = (double)variables["v"];
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double[] Cx_납입면제자급부 = Enumerable.Range(0, CommutationTable.MAXSIZE).Select(j => c.Lx_납입자[j] * c.Rate_r1[j] * Math.Pow(v, j + 0.5)).ToArray();
            double[] Mx_납입면제자급부 = c.GetMx(Cx_납입면제자급부);
            double Ax = (c.Mx_급부[n1] - c.Mx_급부[n2]) / c.Dx_유지자[n1];

            if (t > n1) return 0;

            double V1 = (c.Mx_급부[t] - c.Mx_급부[n1]) / c.Dx_유지자[t];
            double V2 = NP * (c.Nx_납입자[t] - c.Nx_납입자[m]) / c.Dx_납입자[t];
            double V3 = Ax * c.Dx_유지자[n1] * Enumerable.Range(0, n1 - t).Select(j => Cx_납입면제자급부[t + j] / (c.Dx_유지자[t + j] + c.Dx_유지자[t + j + 1]) * (c.Lx_유지자[t + j] + c.Lx_유지자[t + j + 1]) / c.Lx_유지자[t + j]).Sum() / c.Dx_납입자[t];

            return V1 - V2 + V3;
        }
    }

    public class PVType971 : PVCalculator
    {
        //현대 납입면제 확장형
        //F6:갱신보험기간(n1), n:갱신종료보험기간(n2)
        //S7:일반형(0), 납입면제 고급형(1)
        //F9:납면 미발생(0), 납면 발생(1)
        //D3기수 추후수정필요

        public PVType971(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            if (freq == 99) return base.Get순보험료(n, m, t, freq);
            if ((int)variables["S7"] == 0) return base.Get순보험료(n, m, t, freq);


            int n1 = (int)variables["F6"];
            int n2 = n;

            double v = (double)variables["v"];
            double payCnt = mm(freq);
            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            double[] Cx_납입면제자급부 = Enumerable.Range(0, CommutationTable.MAXSIZE).Select(j => c.Lx_납입자[j] * c.Rate_r1[j] * Math.Pow(v, j + 0.5)).ToArray();
            double[] Mx_납입면제자급부 = c.GetMx(Cx_납입면제자급부);
            double Ax = (c.Mx_급부[n1] - c.Mx_급부[n2]) / c.Dx_유지자[n1];

            double NP1 = (c.Mx_급부[0] - c.Mx_급부[n1]) / (payCnt * NNx);
            double NP2 = Ax * c.Dx_유지자[n1] * Enumerable.Range(0, n1).Select(j => Cx_납입면제자급부[j] / (c.Dx_유지자[j] + c.Dx_유지자[j + 1]) * (c.Lx_유지자[j] + c.Lx_유지자[j + 1]) / c.Lx_유지자[j]).Sum() / (payCnt * NNx);

            return NP1 + NP2;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            if ((int)variables["S7"] == 0) return base.Get준비금(n, m, t, freq);

            int n1 = (int)variables["F6"];
            int n2 = n;

            double NP = Get순보험료(n, m, t, freq);
            double v = (double)variables["v"];
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double[] Cx_납입면제자급부 = Enumerable.Range(0, CommutationTable.MAXSIZE).Select(j => c.Lx_납입자[j] * c.Rate_r1[j] * Math.Pow(v, j + 0.5)).ToArray();
            double[] Mx_납입면제자급부 = c.GetMx(Cx_납입면제자급부);
            double Ax = (c.Mx_급부[n1] - c.Mx_급부[n2]) / c.Dx_유지자[n1];

            if ((int)variables["F9"] == 1)
            {
                double V4 = (c.Mx_급부[t] - c.Mx_급부[n1] + Ax * c.Dx_유지자[n1]) / c.Dx_유지자[t];
                double V5 = (c.Mx_급부[t] - c.Mx_급부[n2]) / c.Dx_유지자[t];
                if (t < n1) return V4;
                return V5;
            }


            double V1 = (c.Mx_급부[t] - c.Mx_급부[n1]) / c.Dx_유지자[t];
            double V2 = NP * payCnt * NNx_납입자 / c.Dx_납입자[t];
            double V3 = Ax * c.Dx_유지자[n1] * Enumerable.Range(0, n1 - t).Select(j => Cx_납입면제자급부[t + j] / (c.Dx_유지자[t + j] + c.Dx_유지자[t + j + 1]) * (c.Lx_유지자[t + j] + c.Lx_유지자[t + j + 1]) / c.Lx_유지자[t + j]).Sum() / c.Dx_납입자[t];


            if (t > n1) return 0;

            return V1 - V2 + V3;

        }
    }
}
