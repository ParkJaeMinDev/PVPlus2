using System;
using System.Collections.Generic;

namespace PVPlus2.Models;

public class CommutationTable_old
{
    public const int MAXSIZE = 131; // index 0..130

    public int n { get; set; }
    public double i { get; set; }
    public double v { get; set; }

    public double[] Rate_이율 { get; set; } = new double[MAXSIZE];
    public double[] Rate_할인율 { get; set; } = new double[MAXSIZE];
    public double[] Rate_할인율누계 { get; set; } = new double[MAXSIZE];
    public Dictionary<string, double[]> Rate_위험률 { get; set; } = [];
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

    public List<double[]> RateSegments_급부 { get; set; } = [];
    public List<double[]> RateSegments_유지자 { get; set; } = [];

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

    public List<double[]> LxSegments_유지자 { get; set; } = [];
    public List<double[]> CxSegments_급부 { get; set; } = [];
    public List<double[]> MxSegments_급부 { get; set; } = [];

    public CommutationTable_old()
    {
    }

    public CommutationTable_old(int n, double i, double v)
    {
        this.n = n;
        this.i = i;
        this.v = v;
    }

    public double Pow(double[] values, int t)
    {
        double result = 1.0;

        for (int index = 0; index < t; index++)
        {
            result *= values[index];
        }

        return result;
    }

    public double[] GetLx(double[] rate)
    {
        var lx = new double[MAXSIZE];
        lx[0] = 100000.0;

        for (int t = 0; t < n; t++)
        {
            lx[t + 1] = lx[t] * rate[t];
        }

        return lx;
    }

    public double[] GetDx(double[] lx)
    {
        var dx = new double[MAXSIZE];

        for (int t = 0; t <= n; t++)
        {
            dx[t] = lx[t] * Rate_할인율누계[t];
        }

        return dx;
    }

    public double[] GetNx(double[] dx)
    {
        var nx = new double[MAXSIZE];
        double running = 0.0;

        for (int t = n; t >= 0; t--)
        {
            running += dx[t];
            nx[t] = running;
        }

        return nx;
    }

    public double[] GetCx(double[] lx, double[] rate)
    {
        var cx = new double[MAXSIZE];

        for (int t = 0; t < n; t++)
        {
            cx[t] = lx[t] * rate[t] * Rate_할인율누계[t] * Math.Sqrt(Rate_할인율[t]);
        }

        return cx;
    }

    public double[] GetMx(double[] cx)
    {
        var mx = new double[MAXSIZE];
        double running = 0.0;

        for (int t = n; t >= 0; t--)
        {
            running += cx[t];
            mx[t] = running;
        }

        return mx;
    }
}
