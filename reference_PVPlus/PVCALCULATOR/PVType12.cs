using Flee.PublicTypes;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVPlus.PVCALCULATOR
{
    public class PVType12 : PVCalculator
    {
        public PVType12(LineInfo line) : base(line)
        {

        }

        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            double 분자 = 0;
            double 분모 = 1.0;

            double payCnt = mm(freq);
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPSTD = Get기준연납순보험료(n, m, t, 12);
            double NP = Get순보험료(n, m, t, freq);
            double BetaPPerGP = GetBeta보험료(n, m, t, freq);

            if (freq == 99)
            {
                분자 = NP;
                분모 = 1 - ex.Alpha_P - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce;
            }
            else
            {
                분자 = NP + (ex.Alpha_S + NPSTD * ex.Alpha_Statutory) / (payCnt * APV) + (ex.Beta_S / payCnt);
                분모 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce - payCnt * BetaPPerGP);
            }

            return 분자 / 분모;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double GP = Get영업보험료(n, m, t, freq);
            double BetaNP = GetBeta순보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double NNx_납후유지자 = NNx(c.Nx_유지자, c.Dx_유지자, 12, Math.Max(m, t), n);

            double 분자 = 0;
            double 분모 = 1.0;

            double 분자Out = (c.Mx_급부[t] - c.Mx_급부[n]) + GP * ex.Betaprime_P * NNx_납후유지자;
            double 분자In = (m > 0 && t <= m) ? BetaNP * payCnt * NNx_납입자 : 0;

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
            return Get순보험료(n, m, t, freq) + mm(freq) * Get영업보험료(n, m, t, freq) * GetBeta보험료(n, m, t, freq);
        }

        public virtual double GetBeta보험료(int n, int m, int t, int freq)
        {
            //납방별 영업보험료 1원당 적용되는 납후유지비에 의한 순보험료 증가액
            double betaPrime = ex.Betaprime_P;
            double payCnt = mm(freq);

            double 분자 = NNx(c.Nx_유지자, c.Dx_유지자, 12, m, n);
            double 분모 = payCnt * NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            return betaPrime * 분자 / 분모;
        }

    }

    public class PVType121 : PVType12
    {
        public PVType121(LineInfo line) : base(line)
        {

        }

        public override double Get기준연납순보험료(int n, int m, int t, int freq)
        {
            int S3 = (int)variables["S3"];
            int Min_n = Math.Min(n, 20);

            if (S3 > 0)
            {
                //삼성화재 기준연납순보험료 기수표계산
                //ex, 해지급부는 기존의 Min_n년납이 아닌 m년납 기준으로 산출됨
                string gKey = key.GetKeyWith(S3: 0, m: Min_n);
                Cals[gKey].c.Rate_납입자급부 = c.Rate_납입자급부;
                Cals[gKey].c.Rate_납입면제자급부 = c.Rate_납입자급부;
                Cals[gKey].ex = this.ex;

                CalculateCommutationColumns(Cals[gKey].c);

                return Cals[gKey].Get순보험료(n, Min_n, t, 12);
            }
            else
            {
                return Get순보험료(n, Min_n, t, 12);
            }
        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            int m_default = (int)variables["m"];
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double payCnt = mm(freq);

            double A = ex.Alpha_S / APV / payCnt;
            double B = 1 - ex.Beta_P - ex.Gamma - ex.Ce - ex.Alpha_P / APV - ex.Betaprime_P * base.NNx(c.Nx_유지자, c.Dx_유지자, 12, m, n) / base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double MW = m_default * payCnt * (c.Mx_납입자급부[0] - c.Mx_납입자급부[n] + c.Mx_납입면제자급부[0] - c.Mx_납입면제자급부[n]);

            double u = 1;
            double f = 1;

            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NP = (c.MxSegments_급부합계[0] - c.MxSegments_급부합계[n] + u * MW * A / B) / (payCnt * NNx - f * MW / B);

            return NP;
        }

        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            //기준연납순보험료 비례 보험료 제거버전(성능을 위해)
            double 분자 = 0;
            double 분모 = 1.0;

            double payCnt = mm(freq);
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NP = Get순보험료(n, m, t, freq);
            double BetaPPerGP = GetBeta보험료(n, m, t, freq);

            if (freq == 99)
            {
                분자 = NP;
                분모 = 1 - ex.Alpha_P - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce;
            }
            else
            {
                분자 = NP + ex.Alpha_S / (payCnt * APV) + (ex.Beta_S / payCnt);
                분모 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce - payCnt * BetaPPerGP);
            }

            return 분자 / 분모;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double GP = Get영업보험료(n, m, t, freq);
            double BetaNP = GetBeta순보험료(n, m, t, freq);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double NNx_납후유지자 = NNx(c.Nx_유지자, c.Dx_유지자, 12, Math.Max(m, t), n);

            double M1 = c.MxSegments_급부합계[t] - c.MxSegments_급부합계[n];
            double M2 = m * payCnt * GP * (c.Mx_납입자급부[t] - c.Mx_납입자급부[n] + c.Mx_납입면제자급부[t] - c.Mx_납입면제자급부[n]);
            double M3 = payCnt * GP * ex.Betaprime_P * NNx_납후유지자;

            double N = BetaNP * payCnt * NNx_납입자;
            double D = c.Dx_유지자[t];

            return (M1 + M2 + M3 - N) / D;
        }
    }
}
