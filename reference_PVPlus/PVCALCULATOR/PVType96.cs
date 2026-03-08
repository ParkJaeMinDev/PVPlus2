using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType96 : PVType11
    {
        //한화손보 간병치매보험 연장형(중증치매등 납입면제 사유 미발생시 보험기간이 연장됨)
        //급부대상자는 연령기준나이 미만일 경우 유지자, 이상일 경우 납입자로 계산 (F6: 연령기준나이, S6: 사고미발생 0/발생 1)

        public int 연장기준나이 { get; set; }

        public PVType96(LineInfo line) : base(line)
        {
            연장기준나이 = (int)variables["F6"];
        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double NP = 0;
            double payCnt = mm(freq);
            int n1 = 연장기준나이 - (int)variables["Age"];    //연장기준나이까지 보험기간

            if (freq == 99)
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n]) / c.Dx_납입자[0];
            }
            else
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n]) / (payCnt * NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m));

            }

            return NP;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double payCnt = mm(freq);
            double NNx_유지자 = NNx(c.Nx_유지자, c.Dx_유지자, freq, t, m);

            int n1 = 연장기준나이 - (int)variables["Age"];    //연장기준나이까지 보험기간
            bool Is사고발생 = (int)variables["S6"] == 1;

            double NPA = (c.Mx_급부[0] - c.Mx_급부[n1]) / (payCnt * NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m));
            double NPB = (c.Mx_급부[n1] - c.Mx_급부[n]) / (payCnt * NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m));

            double V = 0;

            //Rate_k1: 보험기간 연장 대상자
            if (freq == 99)
            {
                V = (c.Mx_급부[t] - c.Mx_급부[n]) / c.Dx_유지자[t];
            }
            else
            {
                double VA = 0;
                double VB = 0;

                if (t < m)
                {
                    VA = (c.Mx_급부[t] - c.Mx_급부[n1] - NPA * NNx_유지자) / c.Dx_유지자[t];
                    VB = (c.Mx_급부[n1] - c.Mx_급부[n] - NPB * NNx_유지자) / c.GetDx(c.Rate_k1)[t];
                }
                else if (t < n1)
                {
                    VA = (t < n1) ? (c.Mx_급부[t] - c.Mx_급부[n1]) / c.Dx_유지자[t] : 0;
                    VB = (t < n) ? (c.Mx_급부[n1] - c.Mx_급부[n]) / c.GetDx(c.Rate_k1)[t] : 0;
                }
                else
                {
                    VA = 0;
                    VB = (c.Mx_급부[t] - c.Mx_급부[n]) / c.Dx_유지자[t];
                }

                if (Is사고발생) V = VA;
                else V = VA + VB;
            }

            return V;
        }
    }
}
