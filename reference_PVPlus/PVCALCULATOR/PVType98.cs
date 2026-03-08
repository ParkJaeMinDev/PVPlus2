using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PVPlus.RULES;

namespace PVPlus.PVCALCULATOR
{
    public class PVType98 : PVCalculator
    {
        double[] Rate_State = new double[CommutationTable.MAXSIZE];
        double[] Lx_State3 = new double[CommutationTable.MAXSIZE];
        double[] Lx_State1 = new double[CommutationTable.MAXSIZE];
        double[] Dx_State1 = new double[CommutationTable.MAXSIZE];
        double[] Nx_State1 = new double[CommutationTable.MAXSIZE];

        public PVType98(LineInfo line) : base(line)
        {
            Rate_State = c.Rate_k6;

            //l'''
            for (int i = 0; i < CommutationTable.MAXSIZE; i++)
            {
                if (i == 0)
                {
                    Lx_State3[i] = 100000;
                }
                else
                {
                    Lx_State3[i] = Lx_State3[i - 1] * (c.Rate_납입자[i - 1] - Rate_State[i - 1]);
                }
            }

            //l'
            for (int i = 0; i < CommutationTable.MAXSIZE; i++)
            {
                if (i == 0)
                {
                    Lx_State1[i] = 100000;
                }
                else
                {
                    Lx_State1[i] = c.Lx_납입자[i] - Lx_State3[i - 1] * Rate_State[i - 1];
                }
            }

            //납입자 변환
            Dx_State1 = c.GetDx(Lx_State1);
            Nx_State1 = c.GetNx(Dx_State1);

            c.Lx_납입자 = Lx_State1;
            c.Dx_납입자 = Dx_State1;
            c.Nx_납입자 = Nx_State1;

        }
    }
}
