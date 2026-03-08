using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVPlus.RULES
{
    public class CommutationTable
    {
        public const int MAXSIZE = 131; // 0~130
                           
        public CommutationTable()
        {
            n = (int)PV.variables["n"];
            i = (double)PV.variables["i"];
            v = (double)PV.variables["v"];
        }

        public double[] Rate_이율 { get; set; } = new double[MAXSIZE];
        public double[] Rate_할인율 { get; set; } = new double[MAXSIZE];
        public double[] Rate_할인율누계 { get; set; } = new double[MAXSIZE];
        public Dictionary<string, double[]> Rate_위험률 { get; set; } = new Dictionary<string, double[]>();
        public double[] Rate_해지율 { get; set; } = new double[MAXSIZE];

        public double[] Rate_유지자 { get; set; } = new double[MAXSIZE];
        public double[] Rate_납입자 { get; set; } = new double[MAXSIZE];
        public double[] Rate_납입자급부 { get; set; } = new double[MAXSIZE];
        public double[] Rate_납입면제자급부 { get; set; } = new double[MAXSIZE];

        public double[] Rate_k1 { get; set; } = new double[MAXSIZE];
        public double[] Rate_k2 { get; set; } = new double[MAXSIZE];
        public double[] Rate_k3 { get; set; } = new double[MAXSIZE];
        public double[] Rate_k4 { get; set; } = new double[MAXSIZE];
        public double[] Rate_k5 { get; set; } = new double[MAXSIZE];
        public double[] Rate_k6 { get; set; } = new double[MAXSIZE];
        public double[] Rate_k7 { get; set; } = new double[MAXSIZE];
        public double[] Rate_k8 { get; set; } = new double[MAXSIZE];
        public double[] Rate_k9 { get; set; } = new double[MAXSIZE];
        public double[] Rate_k10 { get; set; } = new double[MAXSIZE];

        public double[] Rate_r1 { get; set; } = new double[MAXSIZE];
        public double[] Rate_r2 { get; set; } = new double[MAXSIZE];
        public double[] Rate_r3 { get; set; } = new double[MAXSIZE];
        public double[] Rate_r4 { get; set; } = new double[MAXSIZE];
        public double[] Rate_r5 { get; set; } = new double[MAXSIZE];
        public double[] Rate_r6 { get; set; } = new double[MAXSIZE];
        public double[] Rate_r7 { get; set; } = new double[MAXSIZE];
        public double[] Rate_r8 { get; set; } = new double[MAXSIZE];
        public double[] Rate_r9 { get; set; } = new double[MAXSIZE];
        public double[] Rate_r10 { get; set; } = new double[MAXSIZE];

        public List<double[]> RateSegments_급부 { get; set; } = new List<double[]>();
        public List<double[]> RateSegments_유지자 { get; set; } = new List<double[]>();

        public double[] Lx_납입자 { get; set; } = new double[MAXSIZE];
        public double[] Lx_유지자 { get; set; } = new double[MAXSIZE];
        public double[] Lx_납입면제자 { get; set; } = new double[MAXSIZE];
        public double[] Dx_납입자 { get; set; } = new double[MAXSIZE];
        public double[] Dx_유지자 { get; set; } = new double[MAXSIZE];
        public double[] Nx_납입자 { get; set; } = new double[MAXSIZE];
        public double[] Nx_유지자 { get; set; } = new double[MAXSIZE];
        public double[] Cx_납입자급부 { get; set; } = new double[MAXSIZE];
        public double[] Cx_납입면제자급부 { get; set; } = new double[MAXSIZE];
        public double[] MxSegments_급부합계 { get; set; } = new double[MAXSIZE];
        public double[] Mx_납입자급부 { get; set; } = new double[MAXSIZE];
        public double[] Mx_납입면제자급부 { get; set; } = new double[MAXSIZE];
        public double[] Mx_급부 { get; set; } = new double[MAXSIZE];

        public List<double[]> LxSegments_유지자 { get; set; } = new List<double[]>();
        public List<double[]> CxSegments_급부 { get; set; } = new List<double[]>();
        public List<double[]> MxSegments_급부 { get; set; } = new List<double[]>();
                          
        protected int n;
        protected double i;
        protected double v;
        protected double[] vArr;

        public virtual double Pow(double[] arr, int t)
        {
            double res = 1;

            for(int i = 0; i < t; i++)
            {
                res = res * arr[i];
            }

            return res;
        }
        public virtual double[] GetLx(double[] Rate)
        {
            //유지자기수
            double[] Lx = new double[MAXSIZE];

            Lx[0] = 100000.0;

            for (int t = 0; t < n; t++)
            {
                Lx[t + 1] = Lx[t] * Rate[t];
            }

            return Lx;
        }
        public virtual double[] GetDx(double[] Lx)
        {
            double[] Dx = new double[MAXSIZE];

            for (int t = 0; t <= n; t++)
            {
                Dx[t] = Lx[t] * Rate_할인율누계[t];
            }

            return Dx;
        }
        public virtual double[] GetNx(double[] Dx)
        {
            double[] Nx = new double[MAXSIZE];

            for (int t = n; t >= 0; t--)
            {
                Nx[t] = Nx[t + 1] + Dx[t];
            }

            return Nx;
        }
        public virtual double[] GetCx(double[] Lx, double[] Rate)
        {
            double[] Cx = new double[MAXSIZE];

            for (int t = 0; t < n; t++)
            {
                Cx[t] = Lx[t] * Rate[t] * Rate_할인율누계[t] * Math.Pow(Rate_할인율[t], 0.5);
            }

            return Cx;
        }
        public virtual double[] GetMx(double[] Cx)
        {
            double[] Mx = new double[MAXSIZE];

            for (int t = n; t >= 0; t--)
            {
                Mx[t] = Mx[t + 1] + Cx[t];
            }

            return Mx;
        }
    }
}
