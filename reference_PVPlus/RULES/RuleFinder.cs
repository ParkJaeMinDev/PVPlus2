using Flee.PublicTypes;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class RuleFinder
    {
        public DataReader Reader { get; set; }

        public RuleFinder(DataReader reader)
        {
            Reader = reader;
            ReadAllData(reader);
            CheckRateKeyContains();
        }

        public void ReadAllData(DataReader reader)
        {
            try
            {
                productRule = DataReader.ProductRule;

                layouts = DataReader.Layouts;

                checkList = layouts.Where(x => x.상품코드 == "Check")
                    .Select(x => x.FactorName)
                    .Distinct().ToList();

                variableChangerByvariableChangerKey = DataReader.VarChgs.ToLookup(x => x.상품코드 + "|" + x.담보코드);

                riderRuleByRiderCode = DataReader.RiderRules.ToLookup(x => x.담보코드);

                exExprsByExpenseKey = DataReader.ExpenseRules.ToLookup(x => x.상품코드 + "|" + x.담보코드);

                rateRulesByRateKey = DataReader.RateRules.ToLookup(x => x.위험률명);

                sInfosByMinSKey = DataReader.SInfos.ToLookup(x => x.MinSKey);

                chkExprsByCheckItem = DataReader.ChkExprs.ToLookup(x => x.산출항목);
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception("인덱스가 배열 범위를 벗어 났습니다. Main시트의 Columns 값이 작게 설정되었거나 일부 셀의 LineLeaf(Alt+Enter)가 적절하게 변환되지 않았습니다.");
            }
        }

        public Layout FindRiderCodeLayout()
        {
            try
            {
                return layouts.First();
            }
            catch
            {
                throw new Exception("구분자 입력이 알맞은지 확인바랍니다.");
            }
        }
        public Dictionary<string, Layout> FindLayout(string productCode, string riderCode)
        {
            Dictionary<string, Layout> result = new Dictionary<string, Layout>();

            List<Layout> baseLayouts = layouts
                .Where(x => x.상품코드 == "Base")
                .ToList();

            List<Layout> checkLayouts = layouts
                .Where(x => x.상품코드 == "Check")
                .ToList();

            List<Layout> productLayouts = layouts
                .Where(x => x.상품코드 == productCode && x.담보코드 == "")
                .ToList();

            List<Layout> productAndRiderLayouts = layouts
                .Where(x => x.상품코드 == productCode && x.담보코드 == riderCode)
                .ToList();

            result["RiderCode"] = FindRiderCodeLayout();

            foreach (var s in baseLayouts)
            {
                result[s.FactorName] = s;
            }

            foreach (var s in checkLayouts)
            {
                result[s.FactorName] = s;
            }

            foreach (var s in productLayouts)
            {
                result[s.FactorName] = s;
            }

            foreach (var s in productAndRiderLayouts)
            {
                result[s.FactorName] = s;
            }

            return result;
        }
        public List<string> FindCheckList()
        {
            List<string> result = new List<string>();

            foreach(string s in checkList)
            {
                if (PV.variables[s].GetType() == typeof(double[]))
                {
                    int n = (int)PV.variables["n"];

                    for (int i = 0; i <= n; i++)
                    {
                        result.Add($"{s}[{i}]");
                    }
                }
                else
                {
                    result.Add(s);
                }
            }

            return result;
        }
        public List<VarChg> FindVariableChanger(string productCode, string riderCode)
        {
            List<VarChg> varchgs = new List<VarChg>();

            if (variableChangerByvariableChangerKey.Contains("Base" + "|"))
            {
                varchgs.AddRange(variableChangerByvariableChangerKey["Base" + "|"]);
            }

            if (variableChangerByvariableChangerKey.Contains(productCode + "|"))
            {
                varchgs.AddRange(variableChangerByvariableChangerKey[productCode + "|"]);
            }

            if (variableChangerByvariableChangerKey.Contains(productCode + "|" + riderCode))
            {
                varchgs.AddRange(variableChangerByvariableChangerKey[productCode + "|" + riderCode]);
            }

            return varchgs;
        }
        public ProductRule FindProductRule()
        {
            return productRule;
        }
        public RiderRule FindRiderRule(string riderCode)
        {
            if (!riderRuleByRiderCode.Contains(riderCode))
            {
                throw new Exception($"담보코드 {riderCode}가 Rider시트에 없습니다.");
            }

            return riderRuleByRiderCode[riderCode].First();
        }
        public Expense FindExpense(RiderRule riderRule, VariableCollection variables)
        {
            string productCode = riderRule.상품코드;
            string expenseRiderCode = riderRule.담보코드;

            List<string> keyCombinations = new List<string>()
            {
                productCode + "|" + expenseRiderCode,
                productCode + "|"
            };

            foreach (string keyCombination in keyCombinations)
            {
                if (exExprsByExpenseKey.Contains(keyCombination))
                {
                    List<ExpenseRule> exExprs = exExprsByExpenseKey[keyCombination].ToList();

                    foreach (ExpenseRule exExpr in exExprs)
                    {
                        if (!exExpr.조건1.Evaluate()) continue;
                        if (!exExpr.조건2.Evaluate()) continue;
                        if (!exExpr.조건3.Evaluate()) continue;
                        if (!exExpr.조건4.Evaluate()) continue;

                        return ToExpense(exExpr);
                    }
                }
            }

            throw new Exception($"조건과 일치하는 사업비를 찾을 수 없습니다. 담보코드: {riderRule.담보코드}");
        }
        public RateRule FindRateRule(string rateName, VariableCollection variables)
        {
            if (rateRulesByRateKey.Contains(rateName))
            {
                foreach(RateRule rateRule in rateRulesByRateKey[rateName])
                {
                    if (rateRule.F1 != null && (int)variables["F1"] != rateRule.F1) continue;
                    if (rateRule.F2 != null && (int)variables["F2"] != rateRule.F2) continue;
                    if (rateRule.F3 != null && (int)variables["F3"] != rateRule.F3) continue;
                    if (rateRule.F4 != null && (int)variables["F4"] != rateRule.F4) continue;
                    if (rateRule.F5 != null && (int)variables["F5"] != rateRule.F5) continue;
                    if (rateRule.F6 != null && (int)variables["F6"] != rateRule.F6) continue;
                    if (rateRule.F7 != null && (int)variables["F7"] != rateRule.F7) continue;
                    if (rateRule.F8 != null && (int)variables["F8"] != rateRule.F8) continue;
                    if (rateRule.F9 != null && (int)variables["F9"] != rateRule.F9) continue;

                    return rateRule;
                }
            }

            throw new Exception($"위험률Key를 찾을 수 없습니다 : {rateName} \r\n위험률Factors F1~F10 : {string.Join(" ", Enumerable.Range(1, 10).Select(i => variables["F" + i]))}");
        }
        public double FindMin_S(string MinSKey)
        {
            List<SInfo> sInfos = new List<SInfo>();

            if (sInfosByMinSKey.Contains(MinSKey))
            {
                sInfos = sInfosByMinSKey[MinSKey]
                   .Where(x => x.조건1.Evaluate() && x.조건2.Evaluate() && x.조건3.Evaluate())
                   .ToList();

                if (sInfos.Any()) return sInfos.Min(x => x.S);
            }

            return 0;
 
        }
        public ChkExprs FindChkExprs(string checkItem)
        {
            string companyName = Configure.CompanyRule.GetType().Name;

            List<ChkExprs> chkExprs1 = chkExprsByCheckItem[checkItem].Where(x => x.회사 == companyName).ToList();
            List<ChkExprs> chkExprs2 = chkExprsByCheckItem[checkItem].Where(x => x.회사 == "Base").ToList();

            foreach (ChkExprs chkExpr in chkExprs1.Union(chkExprs2))
            {
                bool 조건1 = chkExpr.조건1.Evaluate();
                bool 조건2 = chkExpr.조건2.Evaluate();
                bool 조건3 = chkExpr.조건3.Evaluate();
                bool 조건4 = chkExpr.조건4.Evaluate();

                if (조건1 && 조건2 && 조건3 && 조건4) return chkExpr;
            }

            throw new Exception($"조건과 일치하는 산출항목{checkItem}을 찾을 수 없습니다.");
        }

        public List<string> checkList;

        public ProductRule productRule;
        public List<Layout> layouts;
        public ILookup<string, VarChg> variableChangerByvariableChangerKey;
        public ILookup<string, RiderRule> riderRuleByRiderCode;
        public ILookup<string, ExpenseRule> exExprsByExpenseKey;
        public ILookup<string, RateRule> rateRulesByRateKey;
        public ILookup<string, ChkExprs> chkExprsByCheckItem;
        public ILookup<string, SInfo> sInfosByMinSKey;

        private Expense ToExpense(ExpenseRule exExpr)
        {
            Expense ex = new Expense();

            ex.상품코드 = exExpr.상품코드;
            ex.담보코드 = exExpr.담보코드;
            ex.조건1 = exExpr.조건1.ToString();
            ex.조건2 = exExpr.조건2.ToString();
            ex.조건3 = exExpr.조건3.ToString();
            ex.조건4 = exExpr.조건4.ToString();

            ex.Alpha_P = exExpr.Alpha_P_Expr.Evaluate();
            ex.Alpha2_P = exExpr.Alpha2_P_Expr.Evaluate();
            ex.Alpha_S = exExpr.Alpha_S_Expr.Evaluate();
            ex.Alpha_Statutory = exExpr.Alpha_Statutory_Expr.Evaluate();
            ex.Beta_P = exExpr.Beta_P_Expr.Evaluate();
            ex.Beta_S = exExpr.Beta_S_Expr.Evaluate();
            ex.Betaprime_P = exExpr.Betaprime_P_Expr.Evaluate();
            ex.Betaprime_S = exExpr.Betaprime_S_Expr.Evaluate();
            ex.Gamma = exExpr.Gamma_Expr.Evaluate();
            ex.Ce = exExpr.Ce_Expr.Evaluate();
            ex.Refund_Rate_S = exExpr.Refund_Rate_S_Expr.Evaluate();
            ex.Refund_Rate_P = exExpr.Refund_Rate_P_Expr.Evaluate();
            ex.가변1 = exExpr.가변1_Expr.Evaluate();
            ex.가변2 = exExpr.가변2_Expr.Evaluate();

            return ex;
        }

        private void CheckRateKeyContains()
        {
            foreach (var riderRules in riderRuleByRiderCode)
            {
                foreach (RiderRule riderRule in riderRules)
                {
                    List<string> rateKeyList = riderRule.RateKeyByRateVariable.Values.ToList();

                    foreach (string rateKey in rateKeyList)
                    {
                        if (!rateRulesByRateKey.Contains(rateKey))
                        {
                            throw new Exception($"담보코드 {riderRule.담보코드}의 위험률Key {rateKey}를 Rate에서 찾을 수 없습니다.");
                        }
                    }
                }
            }
        }
    }
}
