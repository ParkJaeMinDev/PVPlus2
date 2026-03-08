using Flee.PublicTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PVPlus.RULES
{
    public class DataReader
    {
        public ExpressionContext Context { get; private set; }

        public DataReader()
        {
            Context = new ExpressionContext();
            Context.Imports.AddType(typeof(Math));
            Context.Imports.AddType(typeof(helper));
            ReadOrUpdateAll();
        }

        public static ProductRule ProductRule = new ProductRule();
        public static List<RateRule> RateRules = new List<RateRule>();
        public static List<Layout> Layouts = new List<Layout>();
        public static List<VarChg> VarChgs = new List<VarChg>();
        public static List<ChkExprs> ChkExprs = new List<ChkExprs>();
        public static List<RiderRule> RiderRules = new List<RiderRule>();
        public static List<ExpenseRule> ExpenseRules = new List<ExpenseRule>();
        public static List<SInfo> SInfos = new List<SInfo>();

        public void ReadOrUpdateAll()
        {
            InitializeAllVariables();
            ReadProductRule();
            ReadRiderRules();
            ReadExpenses();
            ReadRateRules();
            ReadLayouts();
            ReadVariableChanger();
            ReadEvaluatedSInfos();
            ReadChkExprs();
        }

        private void ReadProductRule()
        {
            string path = Path.Combine(Configure.WorkingDI.FullName, "Product.txt");
            List<string[]> data = ToArrList(path);

            string[] arr = data
                .FirstOrDefault(x => x[0] == Configure.ProductCode);

            if (arr == null) throw new Exception($"ProductRule시트에서 상품코드({Configure.ProductCode})를 찾을 수 없습니다.");

            ProductRule rule = ToProductRule(arr);

            ProductRule = rule;
        }
        private void ReadRiderRules()
        {
            string path = Path.Combine(Configure.WorkingDI.FullName, "Rider.txt");

            RiderRules = ToArrList(path).Where(x => x[0] == Configure.ProductCode).Select(y => ToRiderRule(y)) .ToList();

            if (!RiderRules.Any()) throw new Exception($"RiderRule을 한 줄 이상 입력해 주세요({Configure.ProductCode})");

            //정기사망 기본추가
            string[] termArr = Enumerable.Range(0, 110).Select(x => "").ToArray();
            RiderRule rule = ToRiderRule(termArr);
            rule.상품코드 = Configure.ProductCode;
            rule.담보코드 = "정기사망";
            rule.담보명 = "정기사망";
            rule.PV_Type = CompileInt("1");
            rule.가입금액Expr = CompileDouble("1");
            rule.납입자Expr = CompileDouble("1-q1");
            rule.유지자Expr = CompileDouble("1-q1");
            rule.급부Exprs = new List<IGenericExpression<double>>() { CompileDouble("q1") };
            rule.탈퇴Exprs = new List<IGenericExpression<double>>() { CompileDouble("1-q1") };
            rule.RateKeyByRateVariable = new Dictionary<string, string>() { { "q1", "정기사망" } };
            RiderRules.Add(rule);
        }
        private void ReadExpenses()
        {
            string path = Path.Combine(Configure.WorkingDI.FullName, "Expense.txt");
            List<string[]> data = ToArrList(path);

            List<ExpenseRule> rules = new List<ExpenseRule>();

            foreach (var row in data)
            {
                if (row[0] != Configure.ProductCode)
                    continue;

                string[] riderCodes = row[1].Split(',');

                foreach (var riderCode in riderCodes)
                {
                    string[] newRow = (string[])row.Clone();
                    newRow[1] = riderCode.Trim(); 

                    rules.Add(ToExpenseExpression(newRow));
                }
            }

            ExpenseRules = rules;

            if (!ExpenseRules.Any())
                throw new Exception($"사업비를 한 줄 이상 입력해 주세요({Configure.ProductCode})");
        }
        private void ReadRateRules()
        {
            string path = Path.Combine(Configure.WorkingDI.FullName, "Rate.txt");

            RateRules = ToArrList(path).Select(x => ToRateRule(x)).ToList();

            //RiderRule.RateKeyByRateVariable의 Value 값을 순회하여 "*" 문자가 포함되어 있을 경우 해당 RateKey를 "*"로 분할하여 곱한 값을 새로운 RateRule로 추가
            //이는 RateRule에 원위험률과 조정계수가 각각 존재하여 원위험률*조정계수 위험률을 추가하기 위함
            //List<RateRule> additionalRateRules = new List<RateRule>();
            //foreach (var riderRule in RiderRules)
            //{
            //    foreach (var rateKey in riderRule.RateKeyByRateVariable.Values)
            //    {
            //        if (rateKey.Contains("*"))
            //        {
            //            string[] splitKeys = rateKey.Split('*').Select(x => x.Trim()).ToArray();
            //            foreach (var splitKey in splitKeys)
            //            {
            //                //기존 RateRules에서 "*"로 분할된 2개의 RateRule을 찾고 각각의 케이스에 대해 곱하여 새로운 RateRule 생성
            //                //곱하는 방식은 F1~F9이 모두 동일한 RateRule끼리만 곱함
            //                var baseRateRule = RateRules.FirstOrDefault(r => r.위험률명 == splitKey);
            //                var additionalRateRule = RateRules.FirstOrDefault(r => r.위험률명 == splitKey);
            //            }
            //        }
            //    }
            //}
            //RateRules.AddRange(additionalRateRules);

        }
        private void ReadLayouts()
        {
            string pathP = Path.Combine(Configure.WorkingDI.FullName, "LayoutP.txt");
            string pathV = Path.Combine(Configure.WorkingDI.FullName, "LayoutV.txt");
            string pathS = Path.Combine(Configure.WorkingDI.FullName, "LayoutS.txt");
            
            List<string[]> data = new List<string[]>();

            if (Configure.TableType == TableType.P) data = ToArrList(pathP);
            if (Configure.TableType == TableType.V) data = ToArrList(pathV);
            if (Configure.TableType == TableType.StdAlpha) data = ToArrList(pathS);

            Layouts = data
                .Where(x => x[0] == "RiderCode" || x[0] == "Check" || x[0] == "Base" || x[0] == Configure.ProductCode)
                .Where(x => x[5] != "")
                .Where(x => !(Configure.SeperationType == "1" && x[4] == ""))
                .Where(x => !(Configure.SeperationType == "2" && x[2] == ""))
                .Select(x => ToLayout(x))
                .ToList();
        }
        private void ReadVariableChanger()
        {
            string path = Path.Combine(Configure.WorkingDI.FullName, "VarChg.txt");

            VarChgs = ToArrList(path)
                .Where(x => x[0] == Configure.ProductCode || x[0] == "Base")
                .Select(x => ToVariableChanger(x))
                .ToList();
        } 
        private void ReadEvaluatedSInfos()
        {
            string path = Path.Combine(Configure.WorkingDI.FullName, "EvaluatedSInfo.txt");

            if (new FileInfo(path).Exists)
            {
                List<SInfo> evaluatedSInfo = ToArrList(path)
                    .Where(x => x[1] == Configure.ProductCode)
                    .Select(x => ToEvaluatedSInfo(x))
                    .ToList();

                SInfos = evaluatedSInfo;
            }
            else
            {
                SInfos = new List<SInfo>();
            }
        }
        private void ReadChkExprs()
        {
            string path = Path.Combine(Configure.WorkingDI.FullName, "ChkExprs.txt");

            List<ChkExprs> chkExprs = ToArrList(path)
                .Select(x => ToChkExprs(x))
                .ToList();

            ChkExprs = chkExprs;
        }
        public void ReadSInfos()
        {
            string path = Path.Combine(Configure.WorkingDI.FullName, "Sinfo.txt");

            SInfos = ToArrList(path).Where(x => x[1] == Configure.ProductCode).Select(y => ToSInfo(y)).ToList();
        }

        public void InitializeAllVariables()
        {
            InitializeFactorVariables();
            InitializeRateVariables();
            InitializeRuleVariables();
            InitializeCheckVariables();
        }
        public void InitializeFactorVariables()
        {
            //위험률 Factors
            Context.Variables["F1"] = 0;
            Context.Variables["F2"] = 0;
            Context.Variables["F3"] = 0;
            Context.Variables["F4"] = 0;
            Context.Variables["F5"] = 0;
            Context.Variables["F6"] = 0;
            Context.Variables["F7"] = 0;
            Context.Variables["F8"] = 0;
            Context.Variables["F9"] = 0;
            Context.Variables["F10"] = 0;

            //계산옵션 Factors
            Context.Variables["S1"] = 0;
            Context.Variables["S2"] = 0;
            Context.Variables["S3"] = 0;
            Context.Variables["S4"] = 0;
            Context.Variables["S5"] = 0;
            Context.Variables["S6"] = 0;
            Context.Variables["S7"] = 0;
            Context.Variables["S8"] = 0;
            Context.Variables["S9"] = 0;
            Context.Variables["S10"] = 0;

            //문자열 Factors
            Context.Variables["Str1"] = "";
            Context.Variables["Str2"] = "";
            Context.Variables["Str3"] = "";
            Context.Variables["Str4"] = "";
            Context.Variables["Str5"] = "";
            Context.Variables["Str6"] = "";
            Context.Variables["Str7"] = "";
            Context.Variables["Str8"] = "";
            Context.Variables["Str9"] = "";
            Context.Variables["Str10"] = "";

            //표준하체 할증 값 적용변수
            //norm:표준체, sub:할증체
            Context.Variables["Substandard_Mode"] = "None";

            //MP Factors
            Context.Variables["n"] = 0;
            Context.Variables["m"] = 0;
            Context.Variables["nAge"] = 0;
            Context.Variables["mAge"] = 0;
            Context.Variables["Age"] = 0;
            Context.Variables["Freq"] = 0;
            Context.Variables["Jong"] = 0;
            Context.Variables["ElapseYear"] = 0;

            Context.Variables["RiderCode"] = "";
            Context.Variables["Company"] = "";
            Context.Variables["Channel"] = 0;
            Context.Variables["PV_Type"] = 0;
            Context.Variables["S_Type"] = 0;

            Context.Variables["TempStr1"] = "";
            Context.Variables["TempStr2"] = "";
        }
        public void InitializeRateVariables()
        {
            for (int i = 1; i <= 30; i++)
            {
                Context.Variables["q" + i] = 0.0;
            }
            Context.Variables["w"] = 0.0;
            Context.Variables["r1"] = 0.0;
            Context.Variables["r2"] = 0.0;
            Context.Variables["r3"] = 0.0;
            Context.Variables["r4"] = 0.0;
            Context.Variables["r5"] = 0.0;
            Context.Variables["r6"] = 0.0;
            Context.Variables["r7"] = 0.0;
            Context.Variables["r8"] = 0.0;
            Context.Variables["r9"] = 0.0;
            Context.Variables["r10"] = 0.0;
            Context.Variables["k1"] = 0.0;
            Context.Variables["k2"] = 0.0;
            Context.Variables["k3"] = 0.0;
            Context.Variables["k4"] = 0.0;
            Context.Variables["k5"] = 0.0;
            Context.Variables["k6"] = 0.0;
            Context.Variables["k7"] = 0.0;
            Context.Variables["k8"] = 0.0;
            Context.Variables["k9"] = 0.0;
            Context.Variables["k10"] = 0.0;
        }
        public void InitializeRuleVariables()
        {
            Context.Variables["i"] = 0.0;
            Context.Variables["v"] = 0.0;

            Context.Variables["ii"] = 0.0;
            Context.Variables["vv"] = 0.0;

            Context.Variables["t"] = 0;
            Context.Variables["Amount"] = 0.0;
            Context.Variables["Min_S"] = 0.0;
        }
        public void InitializeCheckVariables()
        {
            Context.Variables["NP0"] = 0.0;
            Context.Variables["NP1"] = 0.0;
            Context.Variables["NP2"] = 0.0;
            Context.Variables["NP3"] = 0.0;
            Context.Variables["NP4"] = 0.0;
            Context.Variables["NP5"] = 0.0;
            Context.Variables["NP6"] = 0.0;

            Context.Variables["BETANP0"] = 0.0;
            Context.Variables["BETANP1"] = 0.0;
            Context.Variables["BETANP2"] = 0.0;
            Context.Variables["BETANP3"] = 0.0;
            Context.Variables["BETANP4"] = 0.0;
            Context.Variables["BETANP5"] = 0.0;
            Context.Variables["BETANP6"] = 0.0;

            Context.Variables["STDNP"] = 0.0;
            Context.Variables["AP"] = 0.0;

            Context.Variables["GP0"] = 0.0;
            Context.Variables["GP1"] = 0.0;
            Context.Variables["GP2"] = 0.0;
            Context.Variables["GP3"] = 0.0;
            Context.Variables["GP4"] = 0.0;
            Context.Variables["GP5"] = 0.0;
            Context.Variables["GP6"] = 0.0;

            Context.Variables["DCGP0"] = 0.0;
            Context.Variables["DCGP1"] = 0.0;
            Context.Variables["DCGP2"] = 0.0;
            Context.Variables["DCGP3"] = 0.0;
            Context.Variables["DCGP4"] = 0.0;
            Context.Variables["DCGP5"] = 0.0;
            Context.Variables["DCGP6"] = 0.0;

            Context.Variables["V0"] = 0.0;
            Context.Variables["V1"] = 0.0;
            Context.Variables["VWhole"] = new double[131];

            Context.Variables["W0"] = 0.0;
            Context.Variables["W1"] = 0.0;
            Context.Variables["WWhole"] = new double[131];

            Context.Variables["STDALPHA"] = 0.0;
            Context.Variables["ALPHA"] = 0.0;

            Context.Variables["TempCK0"] = 0.0;
            Context.Variables["TempCK1"] = 0.0;
            Context.Variables["TempCK2"] = 0.0;
            Context.Variables["TempCK3"] = 0.0;
            Context.Variables["TempCK4"] = 0.0;
            Context.Variables["TempCK5"] = 0.0;
            Context.Variables["TempCK6"] = 0.0;
            Context.Variables["TempCKA"] = 0.0;
            Context.Variables["TempCKB"] = 0.0;
            Context.Variables["TempCKC"] = 0.0;
        }

        public void GenerateEvaluatedSInfosText(List<SInfo> sList)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(Configure.WorkingDI.FullName, "EvaluatedSInfo.txt"), false, Encoding.UTF8))
            {
                sList.ForEach(x => sw.WriteLine(x.ToString()));
            }
        }

        private Dictionary<(Type, string), object> exprSet = new Dictionary<(Type, string), object>();

        public IGenericExpression<int> CompileInt(string exprStr)
        {
            if (string.IsNullOrWhiteSpace(exprStr)) exprStr = "0";

            if (exprSet.ContainsKey((typeof(int), exprStr)))
            {
                return (IGenericExpression<int>)exprSet[(typeof(int), exprStr)];
            }
            else
            {
                try
                {
                    exprSet[(typeof(int), exprStr)] = Context.CompileGeneric<int>(exprStr);
                    return (IGenericExpression<int>)exprSet[(typeof(int), exprStr)];
                }
                catch
                {
                    throw new Exception("적용 할 수 없는 수식 발견:" + exprStr);
                }
            }
        }
        public IGenericExpression<double> CompileDouble(string exprStr)
        {
            if (string.IsNullOrWhiteSpace(exprStr)) exprStr = "0";
            if (exprStr == "∞" || exprStr == "NaN") exprStr = "0";

            if (exprSet.ContainsKey((typeof(double), exprStr)))
            {
                return (IGenericExpression<double>)exprSet[(typeof(double), exprStr)];
            }
            else
            {
                try
                {
                    exprSet[(typeof(double), exprStr)] = Context.CompileGeneric<double>(exprStr);
                    return (IGenericExpression<double>)exprSet[(typeof(double), exprStr)];
                }
                catch
                {
                    throw new Exception("적용 할 수 없는 수식 발견:" + exprStr);
                }
            }
        }
        public IGenericExpression<string> CompileString(string exprStr)
        {
            if (string.IsNullOrWhiteSpace(exprStr)) exprStr = @""""""; //""

            if (exprSet.ContainsKey((typeof(string), exprStr)))
            {
                return (IGenericExpression<string>)exprSet[(typeof(string), exprStr)];
            }
            else
            {
                try
                {
                    exprSet[(typeof(string), exprStr)] = Context.CompileGeneric<string>(exprStr);
                    return (IGenericExpression<string>)exprSet[(typeof(string), exprStr)];
                }
                catch
                {
                    throw new Exception("적용 할 수 없는 수식 발견:" + exprStr);
                }
            }
        }
        public IGenericExpression<bool> CompileBool(string exprStr)
        {
            if (string.IsNullOrWhiteSpace(exprStr)) exprStr = "true";

            if (exprSet.ContainsKey((typeof(bool), exprStr)))
            {
                return (IGenericExpression<bool>)exprSet[(typeof(bool), exprStr)];
            }
            else
            {
                try
                {
                    exprSet[(typeof(bool), exprStr)] = Context.CompileGeneric<bool>(exprStr);
                    return (IGenericExpression<bool>)exprSet[(typeof(bool), exprStr)];
                }
                catch
                {
                    throw new Exception("적용 할 수 없는 수식 발견:" + exprStr);
                }
            }
        }

        public double ToDoubleOrDefault(string s, double defaultVal)
        {
            return double.TryParse(s, out double val) ? val : defaultVal;
        }
        public int ToIntOrDefault(string s, int defaultVal)
        {
            return int.TryParse(s, out int val) ? val : defaultVal;
        }
        public Nullable<int> ToNullableInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            return ToIntOrDefault(s, 0);
        }

        public List<string[]> ToArrList(string path)
        {
            List<string[]> result = new List<string[]>();

            using (StreamReader sr = new StreamReader(path, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    result.Add(sr.ReadLine().Split('\t'));
                }
            }

            return result;
        }
        private ProductRule ToProductRule(string[] arr)
        {
            ProductRule pr = new ProductRule();

            try
            {
                pr.상품코드 = arr[0];
                pr.판매시기 = arr[1];
                pr.상품명 = arr[2];
                pr.예정이율 = double.Parse(arr[3]);
                pr.평균공시이율 = ToDoubleOrDefault(arr[4], 0);
                pr.판매채널 = ToIntOrDefault(arr[5], 0);
            }
            catch
            {
                throw new Exception($"ProductRule 입력 형식이 잘 못 되었습니다. 상품코드: {arr[0]}");
            }

            return pr;
        }
        private RiderRule ToRiderRule(string[] arr)
        {
            RiderRule r = new RiderRule();

            r.상품코드 = arr[0];
            r.담보코드 = arr[1];
            r.담보명 = arr[2];
            r.PV_Type = CompileInt(arr[3]);
            r.가입금액Expr = CompileDouble(arr[4]);
            r.납입자Expr = CompileDouble(arr[5]);
            r.유지자Expr = CompileDouble(arr[6]);

            r.급부Exprs = new List<IGenericExpression<double>>();
            r.탈퇴Exprs = new List<IGenericExpression<double>>();

            for (int i = 0; i < 20; i++)
            {
                if (!string.IsNullOrWhiteSpace(arr[7 + i]))
                {
                    r.급부Exprs.Add(CompileDouble(arr[7 + i]));

                    if (!string.IsNullOrWhiteSpace(arr[27 + i]))
                    {
                        r.탈퇴Exprs.Add(CompileDouble(arr[27 + i]));
                    }
                    else
                    {
                        r.탈퇴Exprs.Add(CompileDouble(arr[6]));
                    }
                }
            }

            r.납입자급부Expr = CompileDouble(arr[47]);
            r.납입면제자급부Expr = CompileDouble(arr[48]);

            r.RateKeyByRateVariable = new Dictionary<string, string>();

            List<string> rateVariableList = new List<string>()
            {
                "q1","q2","q3","q4","q5","q6","q7","q8","q9","q10",
                "q11","q12","q13","q14","q15","q16","q17","q18","q19","q20",
                "q21","q22","q23","q24","q25","q26","q27","q28","q29","q30"
            };

            for (int i = 0; i < rateVariableList.Count(); i++)
            {
                string rateVariable = rateVariableList[i];
                string rateKey = arr[49 + i];

                if (rateKey == "")
                {
                    continue;
                }
                else
                {
                    r.RateKeyByRateVariable[rateVariable] = rateKey;
                }
            }

            r.wExpr = CompileDouble(arr[79]);

            r.r1Expr = CompileDouble(arr[80]);
            r.r2Expr = CompileDouble(arr[81]);
            r.r3Expr = CompileDouble(arr[82]);
            r.r4Expr = CompileDouble(arr[83]);
            r.r5Expr = CompileDouble(arr[84]);
            r.r6Expr = CompileDouble(arr[85]);
            r.r7Expr = CompileDouble(arr[86]);
            r.r8Expr = CompileDouble(arr[87]);
            r.r9Expr = CompileDouble(arr[88]);
            r.r10Expr = CompileDouble(arr[89]);

            r.k1Expr = CompileDouble(arr[90]);
            r.k2Expr = CompileDouble(arr[91]);
            r.k3Expr = CompileDouble(arr[92]);
            r.k4Expr = CompileDouble(arr[93]);
            r.k5Expr = CompileDouble(arr[94]);
            r.k6Expr = CompileDouble(arr[95]);
            r.k7Expr = CompileDouble(arr[96]);
            r.k8Expr = CompileDouble(arr[97]);
            r.k9Expr = CompileDouble(arr[98]);
            r.k10Expr = CompileDouble(arr[99]);

            r.S_Type = CompileInt(arr[100]);
            r.MinSKey = arr[101];

            return r;
        }
        private RateRule ToRateRule(string[] arr)
        {
            RateRule r = new RateRule();

            r.위험률명 = arr[0];
            r.적용년월 = arr[1];

            r.기간 = ToIntOrDefault(arr[2], 0);
            r.F1 = ToNullableInt(arr[3]);
            r.F2 = ToNullableInt(arr[4]);
            r.F3 = ToNullableInt(arr[5]);
            r.F4 = ToNullableInt(arr[6]);
            r.F5 = ToNullableInt(arr[7]);
            r.F6 = ToNullableInt(arr[8]);
            r.F7 = ToNullableInt(arr[9]);
            r.F8 = ToNullableInt(arr[10]);
            r.F9 = ToNullableInt(arr[11]);

            r.RateArr = new double[131];

            for (int i = 0; i <= 130; i++)
            {
                r.RateArr[i] = ToDoubleOrDefault(arr[12 + i], 0);
            }

            return r;
        }
        private ExpenseRule ToExpenseExpression(string[] arr)
        {
            ExpenseRule exExpr = new ExpenseRule();

            exExpr.상품코드 = arr[0];
            exExpr.담보코드 = arr[1];
            exExpr.조건1 = CompileBool(arr[2]);
            exExpr.조건2 = CompileBool(arr[3]);
            exExpr.조건3 = CompileBool(arr[4]);
            exExpr.조건4 = CompileBool(arr[5]);

            exExpr.Alpha_P_Expr = CompileDouble(arr[6]);
            exExpr.Alpha2_P_Expr = CompileDouble(arr[7]);
            exExpr.Alpha_S_Expr = CompileDouble(arr[8]);
            exExpr.Alpha_Statutory_Expr = CompileDouble(arr[9]);
            exExpr.Beta_P_Expr = CompileDouble(arr[10]);
            exExpr.Beta_S_Expr = CompileDouble(arr[11]);
            exExpr.Betaprime_P_Expr = CompileDouble(arr[12]);
            exExpr.Betaprime_S_Expr = CompileDouble(arr[13]);
            exExpr.Gamma_Expr = CompileDouble(arr[14]);
            exExpr.Ce_Expr = CompileDouble(arr[15]);
            exExpr.Refund_Rate_S_Expr = CompileDouble(arr[16]);
            exExpr.Refund_Rate_P_Expr = CompileDouble(arr[17]);
            exExpr.가변1_Expr = CompileDouble(arr[18]);
            exExpr.가변2_Expr = CompileDouble(arr[19]);

            return exExpr;
        }
        private Layout ToLayout(string[] arr)
        {
            Layout layout = new Layout();

            layout.상품코드 = arr[0];
            layout.담보코드 = arr[1];
            layout.Start = ToIntOrDefault(arr[2], 0);
            layout.Length = ToIntOrDefault(arr[3], 0);
            layout.Index = ToIntOrDefault(arr[4], 0);
            layout.FactorName = arr[5];

            if (!Context.Variables.ContainsKey(layout.FactorName))
            {
                throw new Exception("정의되지 않은 LayoutFactor - " + layout.FactorName);
            }

            return layout;
        }
        private VarChg ToVariableChanger(string[] arr)
        {
            VarChg vc = new VarChg();

            try
            {
                vc.상품코드 = arr[0];
                vc.담보코드 = arr[1];
                vc.변수명 = arr[2];
                vc.값 = Context.CompileDynamic(arr[3]);
            }
            catch
            {
                throw new Exception($"VarChg의 {arr[3]}의 값이 잘 못 입력되었습니다.");
            }

            return vc;
        }
        private SInfo ToSInfo(string[] arr)
        {
            SInfo s = new SInfo();
            s.MinSKey = arr[0];
            s.상품코드 = arr[1];
            s.담보코드 = arr[2];
            s.조건1 = CompileBool(arr[3]);
            s.조건2 = CompileBool(arr[4]);
            s.조건3 = CompileBool(arr[5]);
            s.Jong = ToIntOrDefault(arr[6], 0);
            s.Freq = ToIntOrDefault(arr[7], 1);
            s.Age = ToIntOrDefault(arr[8], 40);
            s.n = ToIntOrDefault(arr[9], 3);
            s.m = ToIntOrDefault(arr[10], 3);
            s.F1 = ToIntOrDefault(arr[11], 1);
            s.F2 = ToIntOrDefault(arr[12], 1);
            s.F3 = ToIntOrDefault(arr[13], 1);
            s.F4 = ToIntOrDefault(arr[14], 0);
            s.F5 = ToIntOrDefault(arr[15], 0);
            s.F6 = ToIntOrDefault(arr[16], 0);
            s.F7 = ToIntOrDefault(arr[17], 0);
            s.F8 = ToIntOrDefault(arr[18], 0);
            s.F9 = ToIntOrDefault(arr[19], 0);
            s.S1 = ToIntOrDefault(arr[20], 0);
            s.S2 = ToIntOrDefault(arr[21], 0);
            s.S3 = ToIntOrDefault(arr[22], 0);
            s.S4 = ToIntOrDefault(arr[23], 0);
            s.S5 = ToIntOrDefault(arr[24], 0);
            s.S6 = ToIntOrDefault(arr[25], 0);
            s.S7 = ToIntOrDefault(arr[26], 0);
            s.S8 = ToIntOrDefault(arr[27], 0);
            s.S9 = ToIntOrDefault(arr[28], 0);
            s.위험보험료Expr = CompileDouble(arr[29]);
            s.정기위험보험료Expr = CompileDouble(arr[30]);
            s.SExpr = CompileDouble(arr[31]);
            return s;
        }
        private SInfo ToEvaluatedSInfo(string[] arr)
        {
            SInfo s = new SInfo();
            s.MinSKey = arr[0];
            s.상품코드 = arr[1];
            s.담보코드 = arr[2];
            s.조건1 = CompileBool(arr[3]);
            s.조건2 = CompileBool(arr[4]);
            s.조건3 = CompileBool(arr[5]);
            s.Jong = ToIntOrDefault(arr[6], 0);
            s.Freq = ToIntOrDefault(arr[7], 1);
            s.Age = ToIntOrDefault(arr[8], 40);
            s.n = ToIntOrDefault(arr[9], 3);
            s.m = ToIntOrDefault(arr[10], 3);
            s.F1 = ToIntOrDefault(arr[11], 1);
            s.F2 = ToIntOrDefault(arr[12], 1);
            s.F3 = ToIntOrDefault(arr[13], 1);
            s.F4 = ToIntOrDefault(arr[14], 0);
            s.F5 = ToIntOrDefault(arr[15], 0);
            s.F6 = ToIntOrDefault(arr[16], 0);
            s.F7 = ToIntOrDefault(arr[17], 0);
            s.F8 = ToIntOrDefault(arr[18], 0);
            s.F9 = ToIntOrDefault(arr[19], 0);
            s.S1 = ToIntOrDefault(arr[20], 0);
            s.S2 = ToIntOrDefault(arr[21], 0);
            s.S3 = ToIntOrDefault(arr[22], 0);
            s.S4 = ToIntOrDefault(arr[23], 0);
            s.S5 = ToIntOrDefault(arr[24], 0);
            s.S6 = ToIntOrDefault(arr[25], 0);
            s.S7 = ToIntOrDefault(arr[26], 0);
            s.S8 = ToIntOrDefault(arr[27], 0);
            s.S9 = ToIntOrDefault(arr[28], 0);
            s.위험보험료 = ToDoubleOrDefault(arr[29], 0);
            s.정기위험보험료 = ToDoubleOrDefault(arr[30], 0);
            s.S = ToDoubleOrDefault(arr[31], 0);
            s.ErrorMessage = arr[32];
            return s;
        }
        private ChkExprs ToChkExprs(string[] arr)
        {
            ChkExprs chkExprs = new ChkExprs();

            chkExprs.회사 = arr[0];
            chkExprs.조건1 = CompileBool(arr[1]);
            chkExprs.조건2 = CompileBool(arr[2]);
            chkExprs.조건3 = CompileBool(arr[3]);
            chkExprs.조건4 = CompileBool(arr[4]);

            chkExprs.산출항목 = arr[5];
            chkExprs.산출수식 = CompileDouble(arr[6]);

            return chkExprs;
        }
    }
}
