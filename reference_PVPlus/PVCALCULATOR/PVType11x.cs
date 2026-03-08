using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    //ABL 납후유지비 상품. 납입면제자에 대한 납후유지비 .
    public class PVType111 : PVType1
    {
        public PVType111(LineInfo line) : base(line)
        {

        }

        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            double payCnt = mm(freq);
            double NNx_유지자0 = NNx(c.Nx_유지자, c.Dx_유지자, freq, 0, n);
            double NNx_납입자0 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = Get순보험료(n, m, t, freq) + ex.Betaprime_S / payCnt * (NNx_유지자0 - NNx_납입자0) / NNx_납입자0;
            double NPSTD = Get기준연납순보험료(n, m, t, 12);

            double 분자 = NPBeta + (ex.Alpha_S + NPSTD * ex.Alpha_Statutory) / (payCnt * APV) + (ex.Beta_S / payCnt);
            double 분모 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce);

            return 분자 / 분모;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double payCnt = mm(freq);
            double NNx_유지자 = NNx(c.Nx_유지자, c.Dx_유지자, freq, t, n);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double NNx_유지자0 = NNx(c.Nx_유지자, c.Dx_유지자, freq, 0, n);
            double NNx_납입자0 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = Get순보험료(n, m, t, freq) + ex.Betaprime_S / payCnt * (NNx_유지자0 - NNx_납입자0) / NNx_납입자0;

            double 분자Out = (c.Mx_급부[t] - c.Mx_급부[n]) + ex.Betaprime_S * (NNx_유지자 - NNx_납입자);
            double 분자In = (m > 0 && t <= m) ? NPBeta * payCnt * NNx_납입자 : 0;

            double 분자 = 분자Out - 분자In;
            double 분모 = c.Dx_유지자[t];

            return 분자 / 분모;
        }

        public override double GetBeta순보험료(int n, int m, int t, int freq)
        {
            double payCnt = mm(freq);
            double NNx_유지자0 = NNx(c.Nx_유지자, c.Dx_유지자, freq, 0, n);
            double NNx_납입자0 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = Get순보험료(n, m, t, freq) + ex.Betaprime_S / payCnt * (NNx_유지자0 - NNx_납입자0) / NNx_납입자0;

            return NPBeta;
        }
    }

    //라이나생명 납후유지비 상품
    public class PVType112 : PVType11
    {
        public PVType112(LineInfo line) : base(line)
        {

        }

        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            double payCnt = mm(freq);
            double NNx_유지자0 = NNx(c.Nx_유지자, c.Dx_유지자, 12, 0, n);
            double NNx_납입자0 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = Get순보험료(n, m, t, freq) + ex.Betaprime_S / payCnt * (NNx_유지자0 - NNx_납입자0) / NNx_납입자0;
            double NPSTD = Get기준연납순보험료(n, m, t, 12);

            double 분자 = NPBeta + (ex.Alpha_S + NPSTD * ex.Alpha_Statutory) / (payCnt * APV) + (ex.Beta_S / payCnt);
            double 분모 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce);

            return 분자 / 분모;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double payCnt = mm(freq);
            double NNx_유지자 = NNx(c.Nx_유지자, c.Dx_유지자, freq, t, n);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double NNx_유지자0 = NNx(c.Nx_유지자, c.Dx_유지자, freq, 0, n);
            double NNx_납입자0 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = Get순보험료(n, m, t, freq) + ex.Betaprime_S / payCnt * (NNx_유지자0 - NNx_납입자0) / NNx_납입자0;

            double 분자Out = (c.Mx_급부[t] - c.Mx_급부[n]) + ex.Betaprime_S * (NNx_유지자 - NNx_납입자);
            double 분자In = (m > 0 && t <= m) ? NPBeta * payCnt * NNx_납입자 : 0;

            double 분자 = 분자Out - 분자In;
            double 분모 = c.Dx_유지자[t];

            return 분자 / 분모;
        }

        public override double GetBeta순보험료(int n, int m, int t, int freq)
        {
            double payCnt = mm(freq);
            double NNx_유지자0 = NNx(c.Nx_유지자, c.Dx_유지자, freq, 0, n);
            double NNx_납입자0 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = Get순보험료(n, m, t, freq) + ex.Betaprime_S / payCnt * (NNx_유지자0 - NNx_납입자0) / NNx_납입자0;

            return NPBeta;
        }
    }

    public class PVType1121 : PVType11
    {
        const int MAXSIZE = 131;

        int n; //보험기간
        int m; //납입기간
        int x; //가입연령
        double v; //할인율
        double[] va = new double[MAXSIZE]; //할인율^(t-x) 
        double[] vam = new double[MAXSIZE]; //할인율^(t-x+0.5)     

        double[] q_i = new double[MAXSIZE];
        double[] q_j = new double[MAXSIZE];
        double[] q_k = new double[MAXSIZE];
        double[] q_death = new double[MAXSIZE]; //사망률

        double[] k1 = new double[MAXSIZE]; //(1-q_death/2)/(1-q_death), 사망률 보정계수
        double[] w = new double[MAXSIZE]; //해지율

        double B1 = 0;
        double B2 = 0;
        double B3 = 0;
        double DWait = 0;

        double DR1 = 0.8;
        double DR2 = 0.5;

        double[] lx_State0 = new double[MAXSIZE]; //(0,0,0): 건강상태
        double[] lx_State1 = new double[MAXSIZE]; //(1,0,0): t
        double[] lx_State2 = new double[MAXSIZE]; //(0,1,0): j
        double[] lx_State3 = new double[MAXSIZE]; //(0,0,1): k
        double[] lx_State4 = new double[MAXSIZE]; //(1,1,0): t,j
        double[] lx_State5 = new double[MAXSIZE]; //(1,0,1): t,k
        double[] lx_State6 = new double[MAXSIZE]; //(0,1,1): j,k
        double[] lx_State7 = new double[MAXSIZE]; //(1,1,1): t,j,k(탈퇴)

        double[] q_3rd = new double[MAXSIZE]; //최종 탈퇴율
        double[] q_A = new double[MAXSIZE]; //P(t 첫번째 발생)
        double[] q_AB = new double[MAXSIZE]; //P(t 첫번째 발생, j 두번째 발생)
        double[] q_ABC = new double[MAXSIZE]; //P(t 첫번째 발생, j 두번째 발생, k 세번째 발생)
        double[] q_B_A = new double[MAXSIZE]; //P(j 두번째 발생 | t 첫번째 발생)
        double[] q_C_AB = new double[MAXSIZE]; //P(k 세번째 발생 | t 첫번째 발생, j 두번째 발생)
        double[] q_B = new double[MAXSIZE]; //P(j 첫번째 발생), 경쟁관계 j,k 두가지만 존재
        double[] q_BC = new double[MAXSIZE]; //P(j 첫번째 발생, k 두번째 발생), 경쟁관계 j,k 두가지만 존재
        double[] q_C_B = new double[MAXSIZE]; //P(k 두번째 발생 | j 첫번째 발생), 경쟁관계 j,k 두가지만 존재
        double[] q_C = new double[MAXSIZE]; //P(k 첫번째 발생), 경쟁관계 k만 존재

        double[] lx_Step1 = new double[MAXSIZE]; //Step1: 유지자기수
        double[,] lx_Step2 = new double[MAXSIZE, MAXSIZE]; //Step2: 유지자기수
        double[,,] lx_Step3 = new double[MAXSIZE, MAXSIZE, MAXSIZE]; //Step3: 유지자기수

        double[] Dx_Step1 = new double[MAXSIZE];
        double[,] Dx_Step2 = new double[MAXSIZE, MAXSIZE];
        double[,,] Dx_Step3 = new double[MAXSIZE, MAXSIZE, MAXSIZE];

        double[] Cx_Step1 = new double[MAXSIZE];
        double[] Cx_Step2 = new double[MAXSIZE];
        double[] Cx_Step3 = new double[MAXSIZE];

        double[,] CCx_Step2 = new double[MAXSIZE, MAXSIZE];
        double[,,] CCx_Step3 = new double[MAXSIZE, MAXSIZE, MAXSIZE];

        double[] Mx_Step1 = new double[MAXSIZE];
        double[] Mx_Step2 = new double[MAXSIZE];
        double[] Mx_Step3 = new double[MAXSIZE];

        public PVType1121(LineInfo line) : base(line)
        {

        }

        public override CommutationTable MakeCommutationTable()
        {
            CommutationTable c = new CommutationTable();
            this.c = c;

            n = (int)variables["n"];
            m = (int)variables["m"];
            x = (int)variables["Age"];

            B1 = 0.5 * riderRule.r7Expr.Evaluate();
            B2 = riderRule.r8Expr.Evaluate();
            B3 = riderRule.r9Expr.Evaluate();
            DWait = riderRule.r10Expr.Evaluate();

            //r1,r2,r3를 미리 계산하여 가변위험률 q3rd 입력
            Dictionary<string, double[]> RateQx = FindQx(x, n, m);

            for (int t = 0; t <= n; t++)
            {
                variables["q1"] = RateQx["q1"][t];
                variables["q2"] = RateQx["q2"][t];
                variables["q3"] = RateQx["q3"][t];

                if (RateQx.ContainsKey("q21")) variables["q21"] = RateQx["q21"][t];
                if (RateQx.ContainsKey("q22")) variables["q22"] = RateQx["q22"][t];
                if (RateQx.ContainsKey("q23")) variables["q23"] = RateQx["q23"][t];

                c.Rate_r1[t] = riderRule.r1Expr.Evaluate();
                c.Rate_r2[t] = riderRule.r2Expr.Evaluate();
                c.Rate_r3[t] = riderRule.r3Expr.Evaluate();
            }

            SetRate(0);
            Setq3rd();

            //Rate Sheet에 가변위험률 등록
            PV.finder.FindRateRule("q3rd", variables).RateArr = q_3rd;

            c = base.MakeCommutationTable();

            // 계산의 편의를 위한 간단한 변수명의 필드값 설정
            v = c.Rate_할인율[0];
            va = c.Rate_할인율누계;
            vam = c.Rate_할인율누계.Select((rate, i) => rate * Math.Pow(c.Rate_할인율[i], 0.5)).ToArray();
            q_death = c.Rate_r4;
            k1 = c.Rate_k1;
            w = c.Rate_해지율;

            //Cx, Mx 기수표 재구성
            for (int i = 0; i < 6; i++)
            {
                SetRate(i);
                SetLx();
                SetDx();
                SetCx();

                c.LxSegments_유지자.Add(c.Lx_유지자);

                double[] CxSegment = new double[MAXSIZE];
                double[] MxSegment = new double[MAXSIZE];

                for (int t = 0; t < MAXSIZE; t++)
                {
                    CxSegment[t] = Cx_Step1[t] + Cx_Step2[t] + Cx_Step3[t];
                    MxSegment[t] = Mx_Step1[t] + Mx_Step2[t] + Mx_Step3[t];
                }

                c.CxSegments_급부.Add(CxSegment);
                c.MxSegments_급부.Add(MxSegment);
            }

            for (int t = 0; t <= n; t++)
            {
                c.MxSegments_급부합계[t] = c.MxSegments_급부[0][t] + c.MxSegments_급부[1][t] + c.MxSegments_급부[2][t] + c.MxSegments_급부[3][t] + c.MxSegments_급부[4][t] + c.MxSegments_급부[5][t];
                c.Mx_급부[t] = c.MxSegments_급부합계[t] + c.Mx_납입자급부[t] + c.Mx_납입면제자급부[t];
            }

            return c;
        }

        public void SetRate(int combinationNum)
        {
            //switch for 6 cases(i,j,k)
            switch (combinationNum)
            {
                case 0: //i,j,k
                    q_i = c.Rate_r1;
                    q_j = c.Rate_r2;
                    q_k = c.Rate_r3;
                    break;
                case 1: //i,k,j
                    q_i = c.Rate_r1;
                    q_j = c.Rate_r3;
                    q_k = c.Rate_r2;
                    break;
                case 2: //j,i,k
                    q_i = c.Rate_r2;
                    q_j = c.Rate_r1;
                    q_k = c.Rate_r3;
                    break;
                case 3: //j,k,i
                    q_i = c.Rate_r2;
                    q_j = c.Rate_r3;
                    q_k = c.Rate_r1;
                    break;
                case 4: //k,i,j
                    q_i = c.Rate_r3;
                    q_j = c.Rate_r1;
                    q_k = c.Rate_r2;
                    break;
                case 5: //k,j,i
                    q_i = c.Rate_r3;
                    q_j = c.Rate_r2;
                    q_k = c.Rate_r1;
                    break;
            }

            //UDD가정하에 t,j,k 순서에 따른 발생확률 계산
            for (int t = 0; t < n; t++)
            {
                q_A[t] = q_i[t] - q_i[t] * (q_j[t] + q_k[t]) / 2 + q_i[t] * q_j[t] * q_k[t] / 3;
                q_AB[t] = q_i[t] * q_j[t] / 2 - q_i[t] * q_j[t] * q_k[t] / 3;
                q_ABC[t] = q_i[t] * q_j[t] * q_k[t] / 6;
                q_B_A[t] = q_AB[t] / q_A[t];
                q_C_AB[t] = q_ABC[t] / q_AB[t];
                q_B[t] = q_j[t] - q_j[t] * q_k[t] / 2;
                q_BC[t] = q_j[t] * q_k[t] / 2;
                q_C_B[t] = q_BC[t] / q_B[t];
                q_C[t] = q_k[t];
            }
        }

        public void Setq3rd()
        {
            //초기상태
            lx_State0[0] = 100000;
            lx_State1[0] = 0;
            lx_State2[0] = 0;
            lx_State3[0] = 0;
            lx_State4[0] = 0;
            lx_State5[0] = 0;
            lx_State6[0] = 0;
            lx_State7[0] = 0;

            //전이행렬 적용
            for (int t = 1; t <= n; t++)
            {
                lx_State0[t] = lx_State0[t - 1] * (1 - q_i[t - 1]) * (1 - q_j[t - 1]) * (1 - q_k[t - 1]);
                lx_State1[t] = lx_State0[t - 1] * q_i[t - 1] * (1 - q_j[t - 1]) * (1 - q_k[t - 1]) + lx_State1[t - 1] * (1 - q_j[t - 1]) * (1 - q_k[t - 1]);
                lx_State2[t] = lx_State0[t - 1] * (1 - q_i[t - 1]) * q_j[t - 1] * (1 - q_k[t - 1]) + lx_State2[t - 1] * (1 - q_i[t - 1]) * (1 - q_k[t - 1]);
                lx_State3[t] = lx_State0[t - 1] * (1 - q_i[t - 1]) * (1 - q_j[t - 1]) * q_k[t - 1] + lx_State3[t - 1] * (1 - q_i[t - 1]) * (1 - q_j[t - 1]);
                lx_State4[t] = lx_State0[t - 1] * q_i[t - 1] * q_j[t - 1] * (1 - q_k[t - 1]) + lx_State1[t - 1] * q_j[t - 1] * (1 - q_k[t - 1]) + lx_State2[t - 1] * q_i[t - 1] * (1 - q_k[t - 1]) + lx_State4[t - 1] * (1 - q_k[t - 1]);
                lx_State5[t] = lx_State0[t - 1] * q_i[t - 1] * (1 - q_j[t - 1]) * q_k[t - 1] + lx_State1[t - 1] * (1 - q_j[t - 1]) * q_k[t - 1] + lx_State3[t - 1] * q_i[t - 1] * (1 - q_j[t - 1]) + lx_State5[t - 1] * (1 - q_j[t - 1]);
                lx_State6[t] = lx_State0[t - 1] * (1 - q_i[t - 1]) * q_j[t - 1] * q_k[t - 1] + lx_State2[t - 1] * (1 - q_i[t - 1]) * q_k[t - 1] + lx_State3[t - 1] * (1 - q_i[t - 1]) * q_j[t - 1] + lx_State6[t - 1] * (1 - q_i[t - 1]);
                lx_State7[t] = lx_State0[t - 1] * q_i[t - 1] * q_j[t - 1] * q_k[t - 1] + lx_State1[t - 1] * q_j[t - 1] * q_k[t - 1] + lx_State2[t - 1] * q_i[t - 1] * q_k[t - 1] + lx_State3[t - 1] * q_i[t - 1] * q_j[t - 1] + lx_State4[t - 1] * q_k[t - 1] + lx_State5[t - 1] * q_j[t - 1] + lx_State6[t - 1] * q_i[t - 1] + lx_State7[t - 1];
            }

            //lx_State0~6: 유지자, lx_State7: 최종탈퇴 기수를 사용하여 q_3rd 계산
            for (int t = 0; t <= n; t++)
            {
                q_3rd[t] = (lx_State7[t + 1] - lx_State7[t]) / (lx_State0[t] + lx_State1[t] + lx_State2[t] + lx_State3[t] + lx_State4[t] + lx_State5[t] + lx_State6[t]);
            }
        }

        public void SetLx()
        {
            //Step1
            lx_Step1[0] = 100000;
            for (int t = 0; t < n; t++)
            {
                double qijk = (1 - (1 - q_i[t]) * (1 - q_j[t]) * (1 - q_k[t])) * k1[t];

                lx_Step1[t + 1] = lx_Step1[t] * (1 - qijk) * (1 - w[t] * k1[t]) * (1 - q_death[t]);
            }

            //Step2
            for (int t = 0; t < n; t++)
            {
                lx_Step2[t, 0] = lx_Step1[t] * qA(t);

                for (int s = 0; s <= n - t; s++)
                {
                    double qjk = (1 - (1 - q_j[t + s]) * (1 - q_k[t + s])) * k1[t + s];

                    if (s == 0)
                    {
                        lx_Step2[t, s + 1] = lx_Step2[t, s] * q_i[t + s] * (1 - qjk) / qA(t + s) * (1 - w[t + s] * k1[t + s]) * (1 - q_death[t + s]);
                    }
                    else
                    {
                        lx_Step2[t, s + 1] = lx_Step2[t, s] * (1 - qjk) * (1 - w[t + s] * k1[t + s]) * (1 - q_death[t + s]);
                    }
                }
            }

            //Step3
            for (int t = 0; t <= n; t++)
            {
                for (int s = 0; s <= n - t; s++)
                {
                    lx_Step3[t, s, 0] = lx_Step2[t, s] * qB(t, s);

                    for (int r = 0; r <= n - t - s; r++)
                    {
                        double qk = q_k[t + s + r] * k1[t + s + r];

                        if (s == 0 && r == 0)
                        {
                            lx_Step3[t, s, r + 1] = lx_Step3[t, s, r] * q_i[t + s + r] * q_j[t + s + r] / 2 * (1 - qk) / (qA(t + s + r) * qB(t, s + r)) * (1 - w[t + s + r] * k1[t + s + r]) * (1 - q_death[t + s + r]);
                        }
                        else if (s > 0 && r == 0)
                        {
                            lx_Step3[t, s, r + 1] = lx_Step3[t, s, r] * q_j[t + s + r] * (1 - qk) / qB(t, s + r) * (1 - w[t + s + r] * k1[t + s + r]) * (1 - q_death[t + s + r]);
                        }
                        else
                        {
                            lx_Step3[t, s, r + 1] = lx_Step3[t, s, r] * (1 - qk) * (1 - w[t + s + r] * k1[t + s + r]) * (1 - q_death[t + s + r]);
                        }
                    }
                }
            }
        }

        public void SetDx()
        {
            //Step1
            for (int t = 0; t <= n; t++)
            {
                Dx_Step1[t] = lx_Step1[t] * va[t];
            }

            //Step2
            for (int t = 0; t <= n; t++)
            {
                for (int s = 0; s <= n - t; s++)
                {
                    Dx_Step2[t, s] = lx_Step2[t, s] * va[t + s];
                }
            }

            //Step3
            for (int t = 0; t <= n; t++)
            {
                for (int s = 0; s <= n - t; s++)
                {
                    for (int r = 0; r <= n - t - s; r++)
                    {
                        Dx_Step3[t, s, r] = lx_Step3[t, s, r] * va[t + s + r];
                    }
                }
            }
        }

        public void SetCx()
        {
            //Step1
            Cx_Step1 = new double[MAXSIZE];
            for (int t = 0; t < n; t++)
            {
                if (t == 0)
                {
                    Cx_Step1[t] = lx_Step1[t] * DWait * (1 - DR1) * B1 * qA(t) * vam[t] * (1 - w[t] / 2);
                }
                else if (t == 1)
                {
                    Cx_Step1[t] = lx_Step1[t] * (1 - DR2) * B1 * qA(t) * vam[t] * (1 - w[t] / 2);
                }
                else
                {
                    Cx_Step1[t] = lx_Step1[t] * B1 * qA(t) * vam[t] * (1 - w[t] / 2);
                }
            }

            Mx_Step1 = c.GetMx(Cx_Step1);


            //Step2
            Cx_Step2 = new double[MAXSIZE];
            for (int t = 0; t < n; t++)
            {
                for (int s = 0; s < n - t; s++)
                {
                    if (t + s == 0)
                    {
                        CCx_Step2[t, s] = lx_Step2[t, s] * DWait * (1 - DR1) * B2 * qB(t, s) * (1 - w[t + s] / 2) * vam[t + s];
                    }
                    else if (t + s == 1)
                    {
                        CCx_Step2[t, s] = lx_Step2[t, s] * (1 - DR2) * B2 * qB(t, s) * (1 - w[t + s] / 2) * vam[t + s];
                    }
                    else
                    {
                        CCx_Step2[t, s] = lx_Step2[t, s] * B2 * qB(t, s) * (1 - w[t + s] / 2) * vam[t + s];
                    }

                    //Cx_Step2[t + s] += lx_Step1[t] * qA(t) * vam[t] * CCx_Step2[t, s] / Dx_Step2[t, 0];
                    Cx_Step2[t + s] += lx_Step1[t] * qA(t) * CCx_Step2[t, s] / lx_Step2[t, 0];
                }
            }

            Mx_Step2 = c.GetMx(Cx_Step2);

            //Step3
            Cx_Step3 = new double[MAXSIZE];
            for (int t = 0; t < n; t++)
            {
                for (int s = 0; s < n - t; s++)
                {
                    for (int r = 0; r < n - t - s; r++)
                    {
                        if (t + s + r == 0)
                        {
                            CCx_Step3[t, s, r] = lx_Step3[t, s, r] * DWait * (1 - DR1) * B3 * qC(t, s, r) * (1 - w[t + s + r] / 2) * vam[t + s + r];
                        }
                        else if (t + s + r == 1)
                        {
                            CCx_Step3[t, s, r] = lx_Step3[t, s, r] * (1 - DR2) * B3 * qC(t, s, r) * (1 - w[t + s + r] / 2) * vam[t + s + r];
                        }
                        else
                        {
                            CCx_Step3[t, s, r] = lx_Step3[t, s, r] * B3 * qC(t, s, r) * (1 - w[t + s + r] / 2) * vam[t + s + r];
                        }

                        //Cx_Step3[t + s + r] += lx_Step1[t] * qA(t) * vam[t] * lx_Step2[t, s] * qB(t, s) * vam[t + s] * CCx_Step3[t, s, r] / Dx_Step3[t, s, 0] / Dx_Step2[t, 0];
                        Cx_Step3[t + s + r] += lx_Step1[t] * qA(t) * lx_Step2[t, s] * qB(t, s) * CCx_Step3[t, s, r] / lx_Step3[t, s, 0] / lx_Step2[t, 0];
                    }
                }
            }

            Mx_Step3 = c.GetMx(Cx_Step3);
        }

        public double qA(int t)
        {
            return q_A[t];
        }

        public double qB(int t, int s)
        {
            if (s == 0) return q_B_A[t + s];
            else return q_B[t + s];
        }

        public double qC(int t, int s, int r)
        {
            if (s == 0 && r == 0) return q_C_AB[t + s + r];
            else if (s > 0 && r == 0) return q_C_B[t + s + r];
            else return q_C[t + s + r];
        }
    }

    //KB생명 영업보험료식
    public class PVType113 : PVType1
    {
        public PVType113(LineInfo line) : base(line)
        {

        }

        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            double 분자 = 0;
            double 분모 = 1.0;

            double payCnt = mm(freq);
            double APV = ax(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NNx = base.NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NP = Get순보험료(n, m, t, freq);
            double NPSTD = Get기준연납순보험료(n, m, t, 12);

            if (freq == 99)
            {
                분자 = NP;
                분모 = 1 - ex.Alpha_P - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce;
            }
            else
            {
                분자 = c.Mx_급부[0] + ex.Beta_S * (c.Nx_유지자[0] - c.Nx_유지자[m]);
                분모 = payCnt * NNx * (1.0 - ex.Alpha_P * c.Dx_유지자[0] / NNx - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce);
            }

            return 분자 / 분모;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double BetaNP = (c.Mx_급부[0] + ex.Beta_S * (c.Nx_유지자[0] - c.Nx_유지자[m])) / NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double 분자 = 0;
            double 분모 = 1.0;

            double 분자Out = c.Mx_급부[t] - c.Mx_급부[n] + ex.Beta_S * (c.Nx_유지자[Math.Min(t, m)] - c.Nx_유지자[m]);
            double 분자In = (m > 0 && t <= m) ? BetaNP * NNx_납입자 : 0;

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

    public class PVType1131 : PVType1
    {
        public PVType1131(LineInfo line) : base(line)
        {

        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double BetaNP = (c.Mx_급부[0] + ex.Beta_S * (c.Nx_유지자[0] - c.Nx_유지자[m])) / NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double 분자 = 0;
            double 분모 = 1.0;

            double[] M_CAN = c.MxSegments_급부[0];
            double[] M_OTR = Enumerable.Range(0, 131).Select(x => c.MxSegments_급부.Skip(1).Sum(y => y[x])).ToArray();
            double[] Dx_A = Enumerable.Range(0, 131).Select(x => c.LxSegments_유지자[0][x] * c.Rate_할인율누계[x]).ToArray();
            double[] Nx_A = c.GetNx(Dx_A);

            double VA = 0;
            double VB = 0;
            double VC = 0;

            double NN_Inforce = (t < m) ? c.Nx_유지자[0] - c.Nx_유지자[m] : 0;
            double NN_Payment = (t < m) ? c.Nx_납입자[0] - c.Nx_납입자[m] : 0;
            double NNt_Inforce = (t < m) ? c.Nx_유지자[t] - c.Nx_유지자[m] : 0;
            double NNt_Payment = (t < m) ? c.Nx_납입자[t] - c.Nx_납입자[m] : 0;

            double P_CAN = M_CAN[0] / NN_Payment;
            double P_OTR = (M_OTR[0] + ex.Beta_S * NN_Inforce) / NN_Payment;

            if (t < m)
            {
                VA = (M_CAN[t] - P_CAN * NNt_Payment) / Dx_A[t];
                VB = (M_OTR[t] + ex.Beta_S * NNt_Inforce - P_OTR * NNt_Payment) / c.Dx_유지자[t];
            }
            else
            {
                VA = M_CAN[t] / Dx_A[t];
                VB = M_OTR[t] / c.Dx_유지자[t];
            }


            VC = M_OTR[t] / Dx_A[t];

            return VA + VB;
        }
    }

    //AIA 준비금 구분적립
    public class PVType114 : PVType1
    {
        public PVType114(LineInfo line) : base(line)
        {

        }

        public override double Get준비금(int n, int m, int t, int freq)
        {

            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double[] D1x = c.GetDx(c.LxSegments_유지자[0]);
            double[] M1x = c.MxSegments_급부[0];

            double[] D2x = c.Dx_유지자;
            double[] M2x = new double[131];

            for (int i = 0; i < 131; i++)
            {
                M2x[i] = c.Mx_급부[i] - M1x[i];
            }

            double 순보험료1 = (M1x[0] - M1x[n]) / (c.Nx_납입자[0] - c.Nx_납입자[m]);
            double 순보험료2 = (M2x[0] - M2x[n]) / (c.Nx_납입자[0] - c.Nx_납입자[m]);

            double 분자Out1 = M1x[t] - M1x[n];
            double 분자In1 = (m > 0 && t <= m) ? 순보험료1 * payCnt * NNx_납입자 : 0;

            double 분자Out2 = M2x[t] - M2x[n];
            double 분자In2 = (m > 0 && t <= m) ? 순보험료2 * payCnt * NNx_납입자 : 0;

            double V1 = (분자Out1 - 분자In1) / D1x[t];
            double V2 = (분자Out2 - 분자In2) / D2x[t];

            return V1 + V2;

        }


    }

    public class PVType1141 : PVType1
    {
        public PVType1141(LineInfo line) : base(line)
        {

        }

        public override double Get준비금(int n, int m, int t, int freq)
        {

            double payCnt = mm(freq);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);

            double[] D1x = c.GetDx(c.LxSegments_유지자[0]);
            double[] M1x = c.MxSegments_급부[0];

            double[] D2x = c.Dx_유지자;
            double[] M2x = new double[131];

            for (int i = 0; i < 131; i++)
            {
                M2x[i] = c.Mx_급부[i] - M1x[i];
            }

            double 순보험료1 = (M1x[0] - M1x[n]) / (c.Nx_납입자[0] - c.Nx_납입자[m]);
            double 순보험료2 = (M2x[0] - M2x[n]) / (c.Nx_납입자[0] - c.Nx_납입자[m]);

            double 분자Out1 = M1x[t] - M1x[n];
            double 분자In1 = (m > 0 && t <= m) ? 순보험료1 * payCnt * NNx_납입자 : 0;

            double 분자Out2 = M2x[t] - M2x[n];
            double 분자In2 = (m > 0 && t <= m) ? 순보험료2 * payCnt * NNx_납입자 : 0;

            double V1 = (분자Out1 - 분자In1) / D1x[t];
            double V2 = (분자Out2 - 분자In2) / D2x[t];

            return V2;

        }
    }

    //하나생명 납입면제특약, 월납
    public class PVType115 : PVCalculator
    {
        
        public PVType115(LineInfo line) : base(line)
        {

        }

        public override double Get순보험료(int n, int m, int t, int freq)
        {
            double NP = 0;
            double payCnt = mm(freq);

            double NNx_월납입자 = NNx(c.Nx_납입자, c.Dx_납입자, 1, 0, n);
            double NNx_월유지자 = NNx(c.Nx_유지자, c.Dx_유지자, 1, 0, n);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            if (freq == 99)
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n]) / c.Dx_납입자[0];
            }
            else
            {
                NP = (NNx_월유지자 - NNx_월납입자) / NNx_납입자 * (12.0 / payCnt);
            }

            return NP;
        }

        public override double Get기준연납순보험료(int n, int m, int t, int freq)
        {
            int Min_n = Math.Min(n, 20);
            double NP = 0;
            double payCnt = mm(freq);

            double NNx_월납입자 = NNx(c.Nx_납입자, c.Dx_납입자, 1, 0, n);
            double NNx_월유지자 = NNx(c.Nx_유지자, c.Dx_유지자, 1, 0, n);
            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);

            if (freq == 99)
            {
                NP = (c.Mx_급부[0] - c.Mx_급부[n]) / c.Dx_납입자[0];
            }
            else
            {
                NP = ((c.Nx_유지자[0] - c.Nx_납입자[0]) - (c.Nx_유지자[n] - c.Nx_납입자[n])) / (c.Nx_납입자[0] - c.Nx_납입자[Min_n]);
            }

            return NP;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {

            double 순보험료 = Get순보험료(n, m, t, 1);
            double payCnt = mm(freq);

            double NNx_납입자 = NNx(c.Nx_납입자, c.Dx_납입자, 1, t, n);
            double NNx_유지자 = NNx(c.Nx_유지자, c.Dx_유지자, 1, t, n);

            double 분자 = 0;
            double 분모 = 1.0;

            double 분자Out = NNx_유지자 - NNx_납입자;
            double 분자In = (m > 0 && t <= m) ? 순보험료 * NNx_납입자 : 0;

            if (freq == 99)
            {
                분자 = 분자Out;
                분모 = c.Dx_유지자[t];
            }
            else
            {
                분자 = (분자Out - 분자In);
                분모 = c.Dx_유지자[t];
            }

            return 12 * 분자 / 분모;
        }
    }
}