using Flee.PublicTypes;
using PVPlus.PVCALCULATOR;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PVPlus
{
    public class PVResult
    {
        public PVCalculator cal;
        public VariableCollection variables;
        public List<string> checkList;

        public string OrgLine { get; set; }
        public string ResultType { get; set; }
        public Dictionary<string, double[]> Org_Cal_Diff = new Dictionary<string, double[]>();

        public PVResult(PVCalculator cal)
        {
            this.cal = cal;
            this.variables = cal.variables;
            this.checkList = PV.finder.FindCheckList();

            this.OrgLine = cal.line.OriginalLine;
            this.ResultType = "";
        }

        public virtual string GetHeadLine()
        {
            List<string> checkHeaderList = Org_Cal_Diff.Keys
                .Select(x => string.Join("\t", x + "_org", x + "_cal", x + "_diff"))
                .ToList();

            return "Line" + "\t" + string.Join("\t", checkHeaderList);
        }
        public virtual string GetLine()
        {
            List<string> checkResultList = Org_Cal_Diff.Values
            .Select(x => string.Join("\t", x))
            .ToList();

            return OrgLine + "\t" + string.Join("\t", checkResultList);
        }
        public virtual void SetResult()
        {
            foreach (string c in checkList)
            {
                double val_org = (double)variables[c];
                double val_cal = GetCheckItemValue(c);
                double val_diff = val_org - val_cal;

                if (double.IsNaN(val_cal)) val_diff = double.NaN;

                Org_Cal_Diff[c] = new double[3] { val_org, val_cal, val_diff };
            }

            ResultType = "정상";

            List<bool> Results = new List<bool>();

            foreach (var checkResult in Org_Cal_Diff)
            {
                double originalValue = checkResult.Value[0];
                double calculatedValue = checkResult.Value[1];
                double difference = checkResult.Value[2];

                bool result = true;

                if (Math.Abs(difference) >= 1 || double.IsNaN(difference)) result = false;   //차이가 1이상일 경우 -> 오차
                if (checkResult.Value[0] == 0) result = true;   //org값이 0일 경우 -> 정상
                if ((checkResult.Key == "GP6" || checkResult.Key == "GP0") && checkResult.Value[0] == 0) result = false; //월납영업보험료가 0일경우 -> 오차

                Results.Add(result);
            }

            if(Results.All(x => x == true)) ResultType = "정상";
            else ResultType = "오차";
        }

        public double GetCheckItemValue(string checkItem)
        {
            int n = (int)variables["n"];
            int m = (int)variables["m"];
            int t = (int)variables["ElapseYear"];
            int freq = (int)variables["Freq"];

            // VWhole, WWhole
            if (checkItem.Contains("Whole"))
            {
                string[] ss = checkItem.Split(new char[] { '[', ']' });
                t = int.Parse(ss[1]);
                return cal.Eval(ss[0], n, m, t, freq);
            }
            else
            {
                return cal.Eval(checkItem, n, m, t, freq);
            }
        }
    }

    public class PVResult_KB : PVResult
    {
        public PVResult_KB(PVCalculator cal) : base(cal)
        {

        }

        public override string GetHeadLine()
        {
            return null;
        }
        public override string GetLine()
        {
            List<string> items = new List<string>();
            string[] orgLineArr = OrgLine
                .Split(Configure.Delimiter)
                .Select(x => x.Trim())
                .ToArray();

            if (Configure.TableType == TableType.P)
            {
                return ToPString(orgLineArr);
            }
            else if (Configure.TableType == TableType.V)
            {
                return ToVString(orgLineArr);
            }

            throw new Exception("KB송부용은 P 또는 V 테이블만 체크 할 수 있습니다");
        }
        public override void SetResult()
        {
            base.SetResult();
        }

        private string ToPString(string[] orgLineArr)
        {
            List<string> items = new List<string>();

            items.Add(orgLineArr[0]);  //상품코드
            items.Add(orgLineArr[1]);  //담보코드
            items.Add(orgLineArr[2]);  //납기
            items.Add(orgLineArr[3]);  //보기
            items.Add(orgLineArr[4]);  //F1
            items.Add(orgLineArr[5]);  //연령
            items.Add(orgLineArr[6]);  //요율급수
            items.Add(orgLineArr[7]);  //요율키1
            items.Add(orgLineArr[8]);  //요율키2
            items.Add(orgLineArr[9]);  //요율키3
            items.Add(orgLineArr[10]);  //요율키4
            items.Add(orgLineArr[11]);  //요율키5
            items.Add(orgLineArr[12]);  //요율키6(표책구분)
            items.Add(orgLineArr[13]);  //산출기준금액


            items.Add(Org_Cal_Diff["GP6"][1].ToString());
            items.Add(Org_Cal_Diff["GP4"][1].ToString());
            items.Add(Org_Cal_Diff["GP3"][1].ToString());
            items.Add(Org_Cal_Diff["GP2"][1].ToString());
            items.Add(Org_Cal_Diff["GP1"][1].ToString());

            items.Add(orgLineArr[14]);  //영업보험료(월납)
            items.Add(orgLineArr[16]);  //영업보험료(3월납)
            items.Add(orgLineArr[17]);  //영업보험료(6월납)
            items.Add(orgLineArr[18]);  //영업보험료(연납)
            items.Add(orgLineArr[19]);  //영업보험료(일시납)

            items.Add("1"); //??

            items.Add(Org_Cal_Diff["GP6"][2].ToString());  //오차
            items.Add(Org_Cal_Diff["GP5"][2].ToString());
            items.Add(Org_Cal_Diff["GP4"][2].ToString());
            items.Add(Org_Cal_Diff["GP3"][2].ToString());
            items.Add(Org_Cal_Diff["GP2"][2].ToString());
            items.Add(Org_Cal_Diff["GP1"][2].ToString());

            return string.Join("|", items);
        }
        private string ToVString(string[] orgLineArr)
        {
            List<string> items = new List<string>();

            items.Add(orgLineArr[14]);  //경과년수
            items.Add(orgLineArr[0]);  //상품코드
            items.Add(orgLineArr[1]);  //담보코드
            items.Add(orgLineArr[2]);  //납기
            items.Add(orgLineArr[3]);  //보기
            items.Add(orgLineArr[4]);  //F1
            items.Add(orgLineArr[5]);  //연령
            items.Add(orgLineArr[6]);  //요율급수
            items.Add(orgLineArr[7]);  //x
            items.Add(orgLineArr[8]);  //요율키1
            items.Add(orgLineArr[9]);  //요율키2
            items.Add(orgLineArr[10]);  //요율키3
            items.Add(orgLineArr[11]);  //요율키4
            items.Add(orgLineArr[12]);  //요율키5
            items.Add(orgLineArr[13]);  //요율키6(표책구분)
            items.Add(orgLineArr[15]);  //산출기준금액

            items.Add(Org_Cal_Diff["NP6"][1].ToString());
            items.Add(Org_Cal_Diff["NP4"][1].ToString());
            items.Add(Org_Cal_Diff["NP3"][1].ToString());
            items.Add(Org_Cal_Diff["NP2"][1].ToString());
            items.Add(Org_Cal_Diff["V0"][1].ToString());
            items.Add(Org_Cal_Diff["V1"][1].ToString());

            items.Add(orgLineArr[22]);  //순보험료(월납)
            items.Add(orgLineArr[24]);  //순보험료(3월납)
            items.Add(orgLineArr[25]);  //순보험료(6월납)
            items.Add(orgLineArr[26]);  //순보험료(연납)
            items.Add(orgLineArr[28]);  //연시준비금
            items.Add(orgLineArr[29]);  //연말준비금

            items.Add("1"); //??

            items.Add(Org_Cal_Diff["NP6"][2].ToString());  //오차
            items.Add(Org_Cal_Diff["NP4"][2].ToString());
            items.Add(Org_Cal_Diff["NP3"][2].ToString());
            items.Add(Org_Cal_Diff["NP2"][2].ToString());
            items.Add(Org_Cal_Diff["V0"][2].ToString());
            items.Add(Org_Cal_Diff["V1"][2].ToString());

            return string.Join("|", items);
        }
    }

    public class PVResult_LimitExcessA : PVResult
    {
        public SInfo sInfo;
        public Dictionary<string, double> Dict = new Dictionary<string, double>();

        public PVResult_LimitExcessA(PVCalculator cal) : base(cal)
        {
            sInfo = cal.line.sInfo;
        }

        public override string GetHeadLine()
        {
            string line = $"MinSKey\t상품코드\t담보코드\t조건1\t조건2\t조건3\tJong\tFreq\tAge\tn\tm\tF1\tF2\tF3\tF4\tF5\tF6\tF7\tF8\tF9\tS1\tS2\tS3\tS4\tS5\tS6\tS7\tS8\tS9\t위험보험료\t정기보험위험보험료\tS\tErrorMessage";
            line += $"\tMin_S\t가입금액\t월납영업보험료\t기준연납순보험료\talpha_P\talpha_Statutory\talpha_S\t예정신계약비\t표준해약공제액\talphaExcessed\tresultType";

            return line ;
        }
        public override string GetLine()
        {
            int n = (int)variables["n"];
            int m = (int)variables["m"];
            int t = (int)variables["ElapseYear"];
            int freq = (int)variables["Freq"];

            string line = sInfo.ToString();

            line += $"\t{Dict["Min_S"]}\t{Dict["Amount"]}\t{Dict["GP"]}\t{Dict["STDNP"]}\t{Dict["alpha_P"]}\t{Dict["alpha_Statutory"]}\t{Dict["alpha_S"]}\t{Dict["ALPHA"]}\t{Dict["STDALPHA"]}\t{Dict["alphaExcessed"]}\t{ResultType}";

            return line;
        }
        public override void SetResult()
        {
            int n = (int)variables["n"];
            int m = (int)variables["m"];
            int t = (int)variables["ElapseYear"];
            int freq = (int)variables["Freq"];

            Dict["Min_S"] = cal.Min_S;
            Dict["Amount"] = cal.SA;
            Dict["GP"] = cal.Eval("GP6", n, m, t, 1);
            Dict["STDNP"] = cal.Eval("STDNP_UNIT", n, m, t, 12) * Dict["Amount"];
            Dict["alpha_P"] = cal.ex.Alpha_P;
            Dict["alpha_Statutory"] = cal.ex.Alpha_Statutory;
            Dict["alpha_S"] = cal.ex.Alpha_S;
            Dict["ALPHA"] = cal.Eval("ALPHA", n, m, t, 1);
            Dict["STDALPHA"] = cal.Eval("STDALPHA", n, m, t, 1);
            Dict["alphaExcessed"] = Dict["ALPHA"] - Dict["STDALPHA"];

            ResultType = Dict["alphaExcessed"] > 0 ? "한도초과" : "한도이하";
        }
    }

    public class PVResult_LimitExcessP : PVResult
    {
        public Dictionary<string, double> Dict = new Dictionary<string, double>();

        public PVResult_LimitExcessP(PVCalculator cal) : base(cal)
        {
        }

        public override string GetHeadLine()
        {
            return "Line\tFreq\tNP\tGP\tGP/NP\tExcessed";
        }
        public override string GetLine()
        {
            return $"{OrgLine}\t{Dict["Freq"]}\t{Dict["NP"]}\t{Dict["GP"]}\t{Dict["Ratio"]}\t{ResultType}";
        }
        public override void SetResult()
        {
            int n = (int)variables["n"];
            int m = (int)variables["m"];
            int t = (int)variables["ElapseYear"];
            int freq = (int)variables["Freq"];

            if (freq == 0) freq = 1;

            Dict["Freq"] = freq;
            Dict["GP"] = cal.Eval("GP0", n, m, t, freq);
            Dict["NP"] = cal.Eval("NP0", n, m, t, freq);
            Dict["Ratio"] = Dict["GP"] / Dict["NP"];

            ResultType = Dict["Ratio"] > 2 ? "한도초과" : "한도이하";
        }
    }

    public class PVResult_LimitExcessA2 : PVResult
    {
        public Dictionary<string, double> Dict = new Dictionary<string, double>();

        public PVResult_LimitExcessA2(PVCalculator cal) : base(cal)
        {

        }

        public override string GetHeadLine()
        {
            return "Line\tSRatio\tAlpha_Cal\tSTDAlpha_Cal\tExcessed";
        }
        public override string GetLine()
        {
            return $"{OrgLine}\t{Dict["Min_S"]}\t{Dict["ALPHA"]}\t{Dict["STDALPHA"]}\t{ResultType}";
        }
        public override void SetResult()
        {
            int n = (int)variables["n"];    
            int m = (int)variables["m"];
            int t = (int)variables["ElapseYear"];
            int freq = (int)variables["Freq"];

            if (freq == 0) freq = 1;

            Dict["Min_S"] = cal.Min_S;
            Dict["ALPHA"] = cal.Eval("ALPHA", n, m, t, freq);
            Dict["STDALPHA"] = cal.Eval("STDALPHA", n, m, t, freq);

            ResultType = Dict["ALPHA"] - Dict["STDALPHA"] > 0 ? "한도초과" : "한도이하";
        }
    }

    public class Sample
    {
        public string[,] Range = new string[1000, 300];
        public PVResult result { get; set; }

        private SubRange LineRange = new SubRange(0, 0, 1, 2);
        private SubRange 담보RuleRange = new SubRange(2, 0, 100, 2);
        private SubRange 위험률FactorRange = new SubRange(2, 3, 15, 2);
        private SubRange 계산FactorRange = new SubRange(2, 6, 20, 2);
        private SubRange 적용위험률Range = new SubRange(2, 9, 35, 2);
        private SubRange 적용사업비Range = new SubRange(2, 12, 25, 2);
        private SubRange 중간계산값Range = new SubRange(2, 15, 35, 2);
        private SubRange 산출결과Range = new SubRange(2, 18, 250, 4);
        private SubRange 기수표Range = new SubRange(2, 23, 990, 200);

        private string line;
        private VariableCollection variables;
        private LineInfo lineInfo;
        private PVCalculator pvCal;

        public Sample(string line)
        {
            this.line = line;
            this.variables = PV.variables;

            PV.reader.InitializeAllVariables();

            lineInfo = new LineInfo(line);
            pvCal = lineInfo.GetPVCalculator();
            result = lineInfo.CalculateLine();
        }

        public void WriteSample()
        {
            using (StreamWriter sw = new StreamWriter(Configure.WorkingDI.FullName + @"\Sample.txt", false))
            {
                for (int i = 0; i < Range.GetLength(0); i++)
                {
                    for (int j = 0; j < Range.GetLength(1); j++)
                    {
                        if (string.IsNullOrWhiteSpace(Range[i, j]))
                        {
                            sw.Write("");
                        }
                        else
                        {
                            sw.Write(Range[i, j]);
                        }

                        sw.Write("\t");
                    }

                    sw.Write("\r\n");
                }
            }
        }
        public void MakeSample()
        {
            FillLineRange();
            Fill담보RuleRange();
            Fill위험률FactorRange();
            Fill계산FactorRange();
            Fill적용위험률Range();
            Fill적용사업비Range();
            Fill중간계산값Range();
            Fill산출결과Range();

            List<string> pvKeys = PVCalculator.Cals.Keys.Reverse().ToList();
            pvKeys.ForEach(x => Fill기수표Range(기수표Range, PVCalculator.Cals[x], x));
            PasteRange(LineRange);
            PasteRange(담보RuleRange);
            PasteRange(위험률FactorRange);
            PasteRange(계산FactorRange);
            PasteRange(적용위험률Range);
            PasteRange(적용사업비Range);
            PasteRange(중간계산값Range);
            PasteRange(산출결과Range);
            PasteRange(기수표Range);

        }
        public void PasteRange(SubRange rng)
        {
            for (int i = 0; i < rng.RowSize; i++)
            {
                if (i >= rng.Items.Count()) break;

                for (int j = 0; j < rng.ColumnSize; j++)
                {
                    if (j >= rng.Items[i].Length) break;

                    Range[rng.RowPos + i, rng.ColumnPos + j] = rng.Items[i][j];
                }
            }
        }

        private void FillLineRange()
        {
            LineRange.Items.Add(new string[] { "Line", lineInfo.OriginalLine });
        }
        private void Fill담보RuleRange()
        {
            RiderRule riderRule = PV.finder.FindRiderRule(lineInfo.RiderCode);

            담보RuleRange.Items.Add(new string[] { "담보Rule", "" });
            담보RuleRange.Items.Add(new string[] { "상품코드", riderRule.상품코드 });
            담보RuleRange.Items.Add(new string[] { "담보코드", riderRule.담보코드 });
            담보RuleRange.Items.Add(new string[] { "담보명", riderRule.담보명 });
            담보RuleRange.Items.Add(new string[] { "종구분코드", variables["Jong"].ToString() });
            담보RuleRange.Items.Add(new string[] { "PV_Type", riderRule.PV_Type.ToString() });
            담보RuleRange.Items.Add(new string[] { "가입금액", riderRule.가입금액Expr.Evaluate().ToString() });
            담보RuleRange.Items.Add(new string[] { "납입자수식", riderRule.납입자Expr.ToString() });
            담보RuleRange.Items.Add(new string[] { "유지자수식", riderRule.유지자Expr.ToString() });

            for (int i = 0; i < riderRule.급부Exprs.Count(); i++)
            {
                담보RuleRange.Items.Add(new string[] { $"급부{i + 1}", riderRule.급부Exprs[i].ToString() });
            }

            for (int i = 0; i < riderRule.탈퇴Exprs.Count(); i++)
            {
                담보RuleRange.Items.Add(new string[] { $"탈퇴{i + 1}", riderRule.탈퇴Exprs[i].ToString() });
            }

            담보RuleRange.Items.Add(new string[] { "납입자급부", riderRule.납입자급부Expr.ToString() });
            담보RuleRange.Items.Add(new string[] { "납입면제자급부", riderRule.납입면제자급부Expr.ToString() });
            담보RuleRange.Items.Add(new string[] { "w", riderRule.wExpr.ToString() });

            if (riderRule.k1Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k1", riderRule.k1Expr.ToString() });
            if (riderRule.k2Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k2", riderRule.k2Expr.ToString() });
            if (riderRule.k3Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k3", riderRule.k3Expr.ToString() });
            if (riderRule.k4Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k4", riderRule.k4Expr.ToString() });
            if (riderRule.k5Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k5", riderRule.k5Expr.ToString() });
            if (riderRule.k6Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k6", riderRule.k6Expr.ToString() });
            if (riderRule.k7Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k7", riderRule.k7Expr.ToString() });
            if (riderRule.k8Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k8", riderRule.k8Expr.ToString() });
            if (riderRule.k9Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k9", riderRule.k9Expr.ToString() });
            if (riderRule.k10Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "k10", riderRule.k10Expr.ToString() });

            if (riderRule.r1Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r1", riderRule.r1Expr.ToString() });
            if (riderRule.r2Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r2", riderRule.r2Expr.ToString() });
            if (riderRule.r3Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r3", riderRule.r3Expr.ToString() });
            if (riderRule.r4Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r4", riderRule.r4Expr.ToString() });
            if (riderRule.r5Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r5", riderRule.r5Expr.ToString() });
            if (riderRule.r6Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r6", riderRule.r6Expr.ToString() });
            if (riderRule.r7Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r7", riderRule.r7Expr.ToString() });
            if (riderRule.r8Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r8", riderRule.r8Expr.ToString() });
            if (riderRule.r9Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r9", riderRule.r9Expr.ToString() });
            if (riderRule.r10Expr.ToString() != "0") 담보RuleRange.Items.Add(new string[] { "r10", riderRule.r10Expr.ToString() });

            담보RuleRange.Items.Add(new string[] { "S1", variables["S1"].ToString() });
            담보RuleRange.Items.Add(new string[] { "S2", variables["S2"].ToString() });
            담보RuleRange.Items.Add(new string[] { "S3", variables["S3"].ToString() });
            담보RuleRange.Items.Add(new string[] { "S4", variables["S4"].ToString() });
            담보RuleRange.Items.Add(new string[] { "S5", variables["S5"].ToString() });
            담보RuleRange.Items.Add(new string[] { "S6", variables["S6"].ToString() });
            담보RuleRange.Items.Add(new string[] { "S7", variables["S7"].ToString() });
            담보RuleRange.Items.Add(new string[] { "S8", variables["S8"].ToString() });
            담보RuleRange.Items.Add(new string[] { "S9", variables["S9"].ToString() });

            담보RuleRange.Items.Add(new string[] { "Min_SGroupKey", riderRule.MinSKey });
            담보RuleRange.Items.Add(new string[] { "TempStr1", variables["TempStr1"].ToString() });
            담보RuleRange.Items.Add(new string[] { "TempStr2", variables["TempStr2"].ToString() });
        }
        private void Fill위험률FactorRange()
        {
            위험률FactorRange.Items.Add(new string[] { "위험률Factor", "" });
            위험률FactorRange.Items.Add(new string[] { "성별(F1)", variables["F1"].ToString() });
            위험률FactorRange.Items.Add(new string[] { "급수(F2)", variables["F2"].ToString() });
            위험률FactorRange.Items.Add(new string[] { "운전(F3)", variables["F3"].ToString() });
            위험률FactorRange.Items.Add(new string[] { "금액 / 한도(F4)", variables["F4"].ToString() });
            위험률FactorRange.Items.Add(new string[] { "가입 / 사고연령(F5)", variables["F5"].ToString() });
            위험률FactorRange.Items.Add(new string[] { "가변1(F6)", variables["F6"].ToString() });
            위험률FactorRange.Items.Add(new string[] { "가변2(F7)", variables["F7"].ToString() });
            위험률FactorRange.Items.Add(new string[] { "가변3(F8)", variables["F8"].ToString() });
            위험률FactorRange.Items.Add(new string[] { "가변4(F9)", variables["F9"].ToString() });
        }
        private void Fill계산FactorRange()
        {
            계산FactorRange.Items.Add(new string[] { "계산Factor", "" });
            계산FactorRange.Items.Add(new string[] { "연령(Age)", variables["Age"].ToString() });
            계산FactorRange.Items.Add(new string[] { "보험기간(n)", variables["n"].ToString() });
            계산FactorRange.Items.Add(new string[] { "납입기간(m)", variables["m"].ToString() });
            계산FactorRange.Items.Add(new string[] { "납입주기코드(Freq)", variables["Freq"].ToString() });
            계산FactorRange.Items.Add(new string[] { "예정이율(i)", variables["i"].ToString() });
            계산FactorRange.Items.Add(new string[] { "평균공시이율(ii)", variables["ii"].ToString() });
            계산FactorRange.Items.Add(new string[] { "예정할인율(v)", variables["v"].ToString() });
            계산FactorRange.Items.Add(new string[] { "평균공시할인율(vv)", variables["vv"].ToString() });
            계산FactorRange.Items.Add(new string[] { "준비금경과년(ElapseYear)", variables["ElapseYear"].ToString() });
            계산FactorRange.Items.Add(new string[] { "S비율(Min_S)", pvCal.Min_S.ToString() });

        }
        private void Fill적용위험률Range()
        {
            RiderRule riderRule = PV.finder.FindRiderRule(lineInfo.RiderCode);

            적용위험률Range.Items.Add(new string[] { "적용위험률", "" });
            foreach (var s in riderRule.RateKeyByRateVariable)
            {
                적용위험률Range.Items.Add(new string[] { s.Key, s.Value });
            }
        }
        private void Fill적용사업비Range()
        {
            Expense ex = pvCal.ex;

            적용사업비Range.Items.Add(new string[] { "적용사업비", "" });
            적용사업비Range.Items.Add(new string[] { "α_P", ex.Alpha_P.ToString() });
            적용사업비Range.Items.Add(new string[] { "α2_P", ex.Alpha2_P.ToString() });
            적용사업비Range.Items.Add(new string[] { "α_S", ex.Alpha_S.ToString() });
            적용사업비Range.Items.Add(new string[] { "α_P20", ex.Alpha_Statutory.ToString() });
            적용사업비Range.Items.Add(new string[] { "β_P", ex.Beta_P.ToString() });
            적용사업비Range.Items.Add(new string[] { "β_S", ex.Beta_S.ToString() });
            적용사업비Range.Items.Add(new string[] { "β'_P", ex.Betaprime_P.ToString() });
            적용사업비Range.Items.Add(new string[] { "β'_S", ex.Betaprime_S.ToString() });
            적용사업비Range.Items.Add(new string[] { "γ", ex.Gamma.ToString() });
            적용사업비Range.Items.Add(new string[] { "ce", ex.Ce.ToString() });
            적용사업비Range.Items.Add(new string[] { "Refund_S", ex.Refund_Rate_S.ToString() });
            적용사업비Range.Items.Add(new string[] { "Refund_P", ex.Refund_Rate_P.ToString() });
            적용사업비Range.Items.Add(new string[] { "가변1", ex.가변1.ToString() });
            적용사업비Range.Items.Add(new string[] { "가변2", ex.가변2.ToString() });
        }
        private void Fill중간계산값Range()
        {
            CommutationTable c = pvCal.c;

            int n = (int)variables["n"];
            int m = (int)variables["m"];
            int age = (int)variables["Age"];
            int freq = (int)variables["Freq"];

            double NNx_연납 = c.Nx_납입자[0] - c.Nx_납입자[m];
            double NNx_월납 = c.Nx_납입자[0] - c.Nx_납입자[m] - 11.0 / 24.0 * (c.Dx_납입자[0] - c.Dx_납입자[m]);
            double APV_연납 = NNx_연납 / c.Dx_유지자[0];
            double APV_월납 = NNx_월납 / c.Dx_유지자[0];

            중간계산값Range.Items.Add(new string[] { "중간계산값", "" });
            중간계산값Range.Items.Add(new string[] { "가입금액", variables["Amount"].ToString() });
            중간계산값Range.Items.Add(new string[] { "M(x)-M(x+n)", (c.Mx_급부[0] - c.Mx_급부[n]).ToString() });
            중간계산값Range.Items.Add(new string[] { "D*(x)-D*(x+m)", (c.Dx_납입자[0] - c.Dx_납입자[m]).ToString() });
            중간계산값Range.Items.Add(new string[] { "N*⁽¹²⁾(x)-N*⁽¹²⁾(x+m)", (c.Nx_납입자[0] - c.Nx_납입자[m]).ToString() });
            중간계산값Range.Items.Add(new string[] { "ä⁽¹²⁾(x:m)", APV_연납.ToString() });
            중간계산값Range.Items.Add(new string[] { "N*⁽¹⁾(x)-N*⁽¹⁾(x+m)", NNx_월납.ToString() });
            중간계산값Range.Items.Add(new string[] { "ä⁽¹⁾(x:m)", APV_월납.ToString() });
            if (pvCal is PVType11)
            {
                중간계산값Range.Items.Add(new string[] { "N⁽¹²⁾(x+m)-N⁽¹²⁾(x+n)", (c.Nx_유지자[m] - c.Nx_유지자[n]).ToString() });
                중간계산값Range.Items.Add(new string[] { "연납베타순보험료", pvCal.GetBeta순보험료(n, m, 0, 12).ToString() });
                중간계산값Range.Items.Add(new string[] { "월납베타순보험료", pvCal.GetBeta순보험료(n, m, 0, 1).ToString() });
            }
            중간계산값Range.Items.Add(new string[] { "연납순보험료", pvCal.Get순보험료(n, m, 0, 12).ToString() });
            중간계산값Range.Items.Add(new string[] { "월납순보험료", pvCal.Get순보험료(n, m, 0, 1).ToString() });
            중간계산값Range.Items.Add(new string[] { "기준연납순보험료", pvCal.Get기준연납순보험료(n, m, 0, 12).ToString() });

            중간계산값Range.Items.Add(new string[] { "연납영업보험료", pvCal.Eval("GP_UNIT", n, m, 0, 12).ToString() });
            중간계산값Range.Items.Add(new string[] { "월납영업보험료", pvCal.Eval("GP_UNIT", n, m, 0, 1).ToString() });

            중간계산값Range.Items.Add(new string[] { "예정신계약비", pvCal.Eval("ALPHA_UNIT", n, m, 0 , freq).ToString() });
            중간계산값Range.Items.Add(new string[] { "표준해약공제액", pvCal.Eval("STDALPHA_UNIT", n, m, 0, freq).ToString() });


        }
        private void Fill산출결과Range()
        {
            Dictionary<string, double[]> dict = result.Org_Cal_Diff;

            산출결과Range.Items.Add(new string[] { "산출결과", "", "", "" });
            산출결과Range.Items.Add(new string[] { "산출변수", "테이블값", "산출값", "오차" });
            foreach (var s in dict)
            {
                산출결과Range.Items.Add(new string[] { s.Key, s.Value[0].ToString(), s.Value[1].ToString(), s.Value[2].ToString() });
            }
        }
        private void Fill기수표Range(SubRange 기수표Range, PVCalculator cal, string name)
        {
            Dictionary<string, string[]> 기수표Set = new Dictionary<string, string[]>();

            CommutationTable c = cal.c;
            int n = (int)cal.LocalVariables["n"];
            int m = (int)cal.LocalVariables["m"];
            int age = (int)cal.LocalVariables["Age"];
            int freq = (int)cal.LocalVariables["Freq"];
            IEnumerable<int> sequence = Enumerable.Range(0, n + 1);

            기수표Set.Add("t", sequence.Select(x => x.ToString()).ToArray());
            기수표Set.Add("Age", sequence.Select(x => (age + x).ToString()).ToArray());

            foreach (var s in cal.riderRule.RateKeyByRateVariable)
            {
                기수표Set.Add(s.Key, sequence.Select(x => c.Rate_위험률[s.Key][x].ToString()).ToArray());
            }

            기수표Set.Add("i", sequence.Select(x => c.Rate_이율[x].ToString()).ToArray());
            기수표Set.Add("v", sequence.Select(x => c.Rate_할인율[x].ToString()).ToArray());
            if (cal.riderRule.wExpr.ToString() != "0")
            {
                기수표Set.Add("w", sequence.Select(x => c.Rate_해지율[x].ToString()).ToArray());
            }

            if (cal.riderRule.r1Expr.ToString() != "0") 기수표Set.Add("r1", sequence.Select(x => c.Rate_r1[x].ToString()).ToArray());
            if (cal.riderRule.r2Expr.ToString() != "0") 기수표Set.Add("r2", sequence.Select(x => c.Rate_r2[x].ToString()).ToArray());
            if (cal.riderRule.r3Expr.ToString() != "0") 기수표Set.Add("r3", sequence.Select(x => c.Rate_r3[x].ToString()).ToArray());
            if (cal.riderRule.r4Expr.ToString() != "0") 기수표Set.Add("r4", sequence.Select(x => c.Rate_r4[x].ToString()).ToArray());
            if (cal.riderRule.r5Expr.ToString() != "0") 기수표Set.Add("r5", sequence.Select(x => c.Rate_r5[x].ToString()).ToArray());
            if (cal.riderRule.r6Expr.ToString() != "0") 기수표Set.Add("r6", sequence.Select(x => c.Rate_r6[x].ToString()).ToArray());
            if (cal.riderRule.r7Expr.ToString() != "0") 기수표Set.Add("r7", sequence.Select(x => c.Rate_r7[x].ToString()).ToArray());
            if (cal.riderRule.r8Expr.ToString() != "0") 기수표Set.Add("r8", sequence.Select(x => c.Rate_r8[x].ToString()).ToArray());
            if (cal.riderRule.r9Expr.ToString() != "0") 기수표Set.Add("r9", sequence.Select(x => c.Rate_r9[x].ToString()).ToArray());
            if (cal.riderRule.r10Expr.ToString() != "0") 기수표Set.Add("r10", sequence.Select(x => c.Rate_r10[x].ToString()).ToArray());

            if (cal.riderRule.k1Expr.ToString() != "0") 기수표Set.Add("k1", sequence.Select(x => c.Rate_k1[x].ToString()).ToArray());
            if (cal.riderRule.k2Expr.ToString() != "0") 기수표Set.Add("k2", sequence.Select(x => c.Rate_k2[x].ToString()).ToArray());
            if (cal.riderRule.k3Expr.ToString() != "0") 기수표Set.Add("k3", sequence.Select(x => c.Rate_k3[x].ToString()).ToArray());
            if (cal.riderRule.k4Expr.ToString() != "0") 기수표Set.Add("k4", sequence.Select(x => c.Rate_k4[x].ToString()).ToArray());
            if (cal.riderRule.k5Expr.ToString() != "0") 기수표Set.Add("k5", sequence.Select(x => c.Rate_k5[x].ToString()).ToArray());
            if (cal.riderRule.k6Expr.ToString() != "0") 기수표Set.Add("k6", sequence.Select(x => c.Rate_k6[x].ToString()).ToArray());
            if (cal.riderRule.k7Expr.ToString() != "0") 기수표Set.Add("k7", sequence.Select(x => c.Rate_k7[x].ToString()).ToArray());
            if (cal.riderRule.k8Expr.ToString() != "0") 기수표Set.Add("k8", sequence.Select(x => c.Rate_k8[x].ToString()).ToArray());
            if (cal.riderRule.k9Expr.ToString() != "0") 기수표Set.Add("k9", sequence.Select(x => c.Rate_k9[x].ToString()).ToArray());
            if (cal.riderRule.k10Expr.ToString() != "0") 기수표Set.Add("k10", sequence.Select(x => c.Rate_k10[x].ToString()).ToArray());

            기수표Set.Add("Rate_유지자", sequence.Select(x => c.Rate_유지자[x].ToString()).ToArray());
            기수표Set.Add("Lx_유지자", sequence.Select(x => c.Lx_유지자[x].ToString()).ToArray());
            기수표Set.Add("Dx_유지자", sequence.Select(x => c.Dx_유지자[x].ToString()).ToArray());
            기수표Set.Add("Nx_유지자", sequence.Select(x => c.Nx_유지자[x].ToString()).ToArray());

            기수표Set.Add("Rate_납입자", sequence.Select(x => c.Rate_납입자[x].ToString()).ToArray());
            기수표Set.Add("Lx_납입자", sequence.Select(x => c.Lx_납입자[x].ToString()).ToArray());
            기수표Set.Add("Dx_납입자", sequence.Select(x => c.Dx_납입자[x].ToString()).ToArray());
            기수표Set.Add("Nx_납입자", sequence.Select(x => c.Nx_납입자[x].ToString()).ToArray());

            for (int i = 0; i < c.RateSegments_급부.Count(); i++)
            {
                기수표Set.Add($"Rate_급부{i + 1}", sequence.Select(x => c.RateSegments_급부[i][x].ToString()).ToArray());
            }
            for (int i = 0; i < c.RateSegments_유지자.Count(); i++)
            {
                기수표Set.Add($"Rate_유지자{i + 1}", sequence.Select(x => c.RateSegments_유지자[i][x].ToString()).ToArray());
            }
            for (int i = 0; i < c.LxSegments_유지자.Count(); i++)
            {
                기수표Set.Add($"Lx_유지자{i + 1}", sequence.Select(x => c.LxSegments_유지자[i][x].ToString()).ToArray());
            }
            for (int i = 0; i < c.CxSegments_급부.Count(); i++)
            {
                기수표Set.Add($"Cx_급부{i + 1}", sequence.Select(x => c.CxSegments_급부[i][x].ToString()).ToArray());
            }
            for (int i = 0; i < c.MxSegments_급부.Count(); i++)
            {
                기수표Set.Add($"Mx_급부{i + 1}", sequence.Select(x => c.MxSegments_급부[i][x].ToString()).ToArray());
            }

            기수표Set.Add($"Mx_급부({1}~{c.RateSegments_유지자.Count()})합계", sequence.Select(x => c.MxSegments_급부합계[x].ToString()).ToArray());

            if ((int)variables["S5"] > 0)
            {
                string gKey = cal.key.GetKeyWith(S5: 0);
                PVCalculator cal2 = PVCalculator.Cals[gKey];

                기수표Set.Add("표준형V", sequence.Select(t => cal2.Eval("V2_UNIT", n, m, t, freq).ToString()).ToArray());
                기수표Set.Add("표준형W", sequence.Select(t => cal2.Get해약환급금(n, m, t, freq).ToString()).ToArray());
                기수표Set.Add("표준형Alpha", sequence.Select(t => cal2.Get예정신계약비(n, m, t, freq).ToString()).ToArray());
                기수표Set.Add("표준형STDAlpha", sequence.Select(t => cal2.Get표준해약공제액(n, m, t, freq).ToString()).ToArray());
                기수표Set.Add("표준형DAC", sequence.Select(t => cal2.Get미상각신계약비(n, m, t, freq).ToString()).ToArray());
            }

            if (cal.riderRule.납입자급부Expr.ToString() != "0")
            {
                기수표Set.Add("Rate_납입자급부", sequence.Select(x => c.Rate_납입자급부[x].ToString()).ToArray());
                기수표Set.Add("Cx_납입자급부", sequence.Select(x => c.Cx_납입자급부[x].ToString()).ToArray());
                기수표Set.Add("Mx_납입자급부", sequence.Select(x => c.Mx_납입자급부[x].ToString()).ToArray());
            }

            if (cal.riderRule.납입면제자급부Expr.ToString() != "0")
            {
                기수표Set.Add("Rate_납입면제자급부", sequence.Select(x => c.Rate_납입면제자급부[x].ToString()).ToArray());
                기수표Set.Add("Lx_납입면제자", sequence.Select(x => c.Lx_납입면제자[x].ToString()).ToArray());
                기수표Set.Add("Cx_납입면제자급부", sequence.Select(x => c.Cx_납입면제자급부[x].ToString()).ToArray());
                기수표Set.Add("Mx_납입면제자급부", sequence.Select(x => c.Mx_납입면제자급부[x].ToString()).ToArray());
            }

            기수표Set.Add("Mx_급부", sequence.Select(x => c.Mx_급부[x].ToString()).ToArray());
            기수표Set.Add("V", sequence.Select(t => cal.Eval("V_UNIT", n, m, t, freq).ToString()).ToArray());

            기수표Range.Items.Add(new string[] { name });
            기수표Range.Items.Add(기수표Set.Keys.ToArray());
            for (int i = 0; i < n + 1; i++)
            {
                string[] item = 기수표Set.Select(x => x.Value[i]).ToArray();
                기수표Range.Items.Add(item);
            }

            //마지막 줄 공백
            기수표Range.Items.Add(new string[] { "" });
        }

        public class SubRange
        {
            public int RowPos { get; set; }
            public int ColumnPos { get; set; }
            public int RowSize { get; set; }
            public int ColumnSize { get; set; }

            public List<string[]> Items { get; set; }

            public SubRange(int rowPos, int colPos, int rowSize, int colSize)
            {
                RowPos = rowPos;
                ColumnPos = colPos;
                RowSize = rowSize;
                ColumnSize = colSize;

                Items = new List<string[]>();
            }
        }
    }
}
