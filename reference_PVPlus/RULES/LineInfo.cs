using Flee.PublicTypes;
using PVPlus.PVCALCULATOR;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class LineInfo
    {
        public string OriginalLine { get; set; }
        public string AdjustedLine { get; set; }
        public string[] Arr { get; set; }
        public SInfo sInfo { get; set; }

        public string ProductCode { get; set; }
        public string RiderCode { get; set; }

        public Dictionary<string, Layout> layouts { get; set; }
        public VariableCollection variables;

        public LineInfo(string line)
        {
            variables = PV.variables;

            OriginalLine = line;
            AdjustedLine = Configure.CompanyRule.AdjustLine(line);
            if (Configure.SeperationType == "1") Arr = AdjustedLine.Split(Configure.Delimiter);

            ProductCode = Configure.ProductCode;
            RiderCode = GetLayoutItem(PV.finder.FindRiderCodeLayout()).Trim();

            layouts = PV.finder.FindLayout(ProductCode, RiderCode);

            helper.variables = variables;
            helper.lineInfo = this;
            PVCalculator.Cals = new Dictionary<string, PVCalculator>();

            SetVariables();
        }
        public LineInfo(SInfo sInfo)
        {
            variables = PV.variables;
            this.sInfo = sInfo;

            ProductCode = Configure.ProductCode;
            RiderCode = sInfo.담보코드;

            layouts = new Dictionary<string, Layout>();

            helper.variables = variables;
            helper.lineInfo = this;
            PVCalculator.Cals = new Dictionary<string, PVCalculator>();

            SetVariables();
        }

        public PVResult CalculateLine()
        {
            AddLineSummary();

            string CurMP = GetLayoutMP();
            string PreMP = helper.PreMP;

            if (CurMP == PreMP && layouts.Any())
            {
                helper.PreResult.OrgLine = OriginalLine;
                helper.PreResult.ResultType = "";
                return helper.PreResult;
            }

            PVCalculator pvCal = GetPVCalculator();
            PVResult pVSResultFactory = new PVResult(pvCal);

            helper.PreMP = CurMP;
            helper.PreResult = pVSResultFactory;

            return pVSResultFactory;
        }
        public void CalculateSInfo()
        {
            PVCalculator pvCal = GetPVCalculator();
            helper.cal = pvCal;

            sInfo.위험보험료 = sInfo.위험보험료Expr.Evaluate();
            sInfo.정기위험보험료 = sInfo.정기위험보험료Expr.Evaluate();
            sInfo.S = sInfo.SExpr.Evaluate();
        }

        public void SetVariables()
        {
            ProductRule productRule = PV.finder.FindProductRule();
            RiderRule riderRule = PV.finder.FindRiderRule(RiderCode);

            variables["i"] = productRule.예정이율;
            variables["ii"] = productRule.평균공시이율;

            if (AdjustedLine != null)
            {
                SetLayoutVariables();
                ChangeVariables();
            }
            if (sInfo != null)
            {
                variables["Jong"] = sInfo.Jong;
                variables["Age"] = sInfo.Age;
                variables["Freq"] = sInfo.Freq;

                variables["n"] = sInfo.n;
                variables["m"] = sInfo.m;

                variables["F1"] = sInfo.F1;
                variables["F2"] = sInfo.F2;
                variables["F3"] = sInfo.F3;
                variables["F4"] = sInfo.F4;
                variables["F5"] = sInfo.F5;
                variables["F6"] = sInfo.F6;
                variables["F7"] = sInfo.F7;
                variables["F8"] = sInfo.F8;
                variables["F9"] = sInfo.F9;

                variables["S1"] = sInfo.S1;
                variables["S2"] = sInfo.S2;
                variables["S3"] = sInfo.S3;
                variables["S4"] = sInfo.S4;
                variables["S5"] = sInfo.S5;
                variables["S6"] = sInfo.S6;
                variables["S7"] = sInfo.S7;
                variables["S8"] = sInfo.S8;
                variables["S9"] = sInfo.S9;
            }

            double Min_S = PV.finder.FindMin_S(riderRule.MinSKey);

            variables["v"] = 1 / (1 + (double)variables["i"]);
            variables["vv"] = 1 / (1 + (double)variables["ii"]);
            variables["Amount"] = riderRule.가입금액Expr.Evaluate();
            variables["PV_Type"] = riderRule.PV_Type.Evaluate();
            variables["S_Type"] = riderRule.S_Type.Evaluate();
            variables["Company"] = Configure.CompanyRule.GetType().Name;
            variables["Channel"] = productRule.판매채널;
        }
        public void SetLayoutVariables()
        {

            foreach (var s in layouts)
            {
                try
                {
                    if (variables.ContainsKey(s.Key))
                    {
                        Type variableType = variables[s.Key].GetType();

                        if (variableType == typeof(int))
                        {
                            variables[s.Key] = int.Parse(GetLayoutItem(s.Value));
                        }
                        else if (variableType == typeof(double))
                        {
                            variables[s.Key] = double.Parse(GetLayoutItem(s.Value));
                        }
                        else if (variableType == typeof(string))
                        {
                            variables[s.Key] = GetLayoutItem(s.Value).Trim();
                        }
                        else if (variableType == typeof(double[]))
                        {
                            int size = 130;
                            Layout tempLayout = new Layout();
                            tempLayout.Index = s.Value.Index;
                            tempLayout.Start = s.Value.Start;
                            tempLayout.Length = s.Value.Length;

                            for (int i = 0; i < size; i++)
                            {
                                variables[$"{s.Key}[{i}]"] = double.Parse(GetLayoutItem(tempLayout));

                                if (Configure.SeperationType == "1")
                                {
                                    tempLayout.Index += 1;
                                }
                                else
                                {
                                    tempLayout.Start += s.Value.Length;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    throw new Exception($"변수({s.Key})의 할당 값({GetLayoutItem(s.Value)})이 {variables[s.Key].GetType()} 타입으로 변환 될 수 없습니다.");
                }
            }
        }
        public void ChangeVariables()
        {
            List<VarChg> VarChgList = PV.finder.FindVariableChanger(ProductCode, RiderCode);

            foreach (VarChg s in VarChgList)
            {
                variables[s.변수명] = s.값.Evaluate();
            }
        }

        public string GetLayoutItem(Layout layout)
        {
            string item = null;

            if (Configure.SeperationType == "1")
            {
                if (layout.Index >= 0 && layout.Index < Arr.Length)
                {
                    item = Arr[layout.Index];
                }
                else
                {
                    item = "";
                    //throw new Exception($"인덱스 값이 구분자로 분할 된 라인의 최대 인덱스 값을 초과하였습니다. 구분자 또는 원본 라인을 확인 바랍니다.");
                }
            }
            else
            {
                if (layout.Start + layout.Length <= AdjustedLine.Length)
                {
                    item = AdjustedLine.Substring(layout.Start, layout.Length);
                }
                else
                {
                    //범위 초과부분 자동보정
                    item = "";
                }
            }

            if (string.IsNullOrWhiteSpace(item))
            {
                return "0";
            }

            return item;
        }
        public string GetLayoutMP()
        {
            string item = "";

            foreach (var s in layouts)
            {
                if(s.Value.상품코드 == "Check" || s.Value.FactorName == "ElapseYear")
                {
                    continue;
                }
                else
                {
                    item += GetLayoutItem(s.Value) + "|";
                }
            }

            return item;
        }
        public void AddLineSummary()
        {
            if (!Configure.LineSummaryChecked) return;
            if (PV.sr.BaseStream == null || PV.sw라인요약.BaseStream == null) return;

            LineSummary lineSummary = new LineSummary();

            lineSummary.RiderCode = RiderCode;
            lineSummary.Jong = (int)variables["Jong"];
            lineSummary.S1 = (int)variables["S1"];
            lineSummary.n = (int)variables["n"];
            lineSummary.m = (int)variables["m"];
            lineSummary.Age = (int)variables["Age"];
            PV.lineSummariesTemp.Add(lineSummary);

            while (PV.lineSummariesTemp.Count() % 100000 == 99999 || (PV.sr.EndOfStream && PV.lineSummariesTemp.Count() > 0))
            {
                string selectedRiderCode = PV.lineSummariesTemp.First().RiderCode;
                List<LineSummary> selectedLineSummaries = PV.lineSummariesTemp.Where(x => x.RiderCode == selectedRiderCode).ToList();

                var group1 = selectedLineSummaries.GroupBy(x => x.n);
                var group2 = selectedLineSummaries.GroupBy(x => x.Age + x.n);

                if (group1.Count() > group2.Count())
                {
                    //세만기 가정
                    foreach (var line in selectedLineSummaries)
                    {
                        line.MaturityAge = line.Age + line.n;
                        string key = string.Join("\t", line.RiderCode, line.Jong, line.S1, line.m, line.MaturityAge + "세");

                        if (!PV.lineSummaries.ContainsKey(key))
                        {
                            PV.lineSummaries.Add(key, line);
                        }

                        PV.lineSummaries[key].Count++;
                        PV.lineSummaries[key].Ages.Add(line.Age);
                    }
                }
                else
                {
                    //연만기 가정
                    foreach (var line in selectedLineSummaries)
                    {
                        string key = string.Join("\t", line.RiderCode, line.Jong, line.S1, line.m, line.n);

                        if (!PV.lineSummaries.ContainsKey(key))
                        {
                            PV.lineSummaries.Add(key, line);
                        }

                        PV.lineSummaries[key].Count++;
                        PV.lineSummaries[key].Ages.Add(line.Age);
                    }
                }

                PV.lineSummariesTemp.RemoveAll(x => x.RiderCode == selectedRiderCode);
            }
        }

        public PVCalculator GetPVCalculator()
        {
            int n = (int)variables["n"];
            int m = (int)variables["m"];
            int Min_n = Math.Min(n, 20);
            int standardType = (int)variables["S2"];
            int stdType = (int)variables["S3"];
            int csvType = (int)variables["S5"];
            int PV_Type = (int)variables["PV_Type"];
            string currentKey = new PVCalculatorKey().GetKey();

            //기수표 생성
            HashSet<int> Set1 = new HashSet<int>() { 0, stdType };
            HashSet<int> Set2 = new HashSet<int>() { 0, csvType };

            HashSet<(int, int)> Set = new HashSet<(int, int)>() { (0, 0), (0, csvType), (stdType, 0), (stdType, csvType) };
            //HashSet<(int, int)> Set = new HashSet<(int, int)>() { (stdType, 0), (0, 0), (0, csvType), (stdType, csvType) };

            foreach (var (S3, S5) in Set)
            {
                variables["S3"] = S3;
                variables["S5"] = S5;
                if (S3 == 0 && Set1.Count > 1) variables["m"] = Min_n;

                if (standardType > 0 && (string)variables["Substandard_Mode"] == "None")
                {
                    //위험률 1배 적용
                    variables["Substandard_Mode"] = "norm";
                    PVCalculator norm = CreatePVCalculatorInstance(PV_Type);

                    //위험률 k배 적용 S(k)
                    variables["Substandard_Mode"] = "sub";
                    PVCalculator sub = CreatePVCalculatorInstance(PV_Type);

                    variables["Substandard_Mode"] = "None";
                    CreatePVCalculatorInstance(sub, norm);
                }
                else
                {
                    CreatePVCalculatorInstance(PV_Type);
                }

                variables["S3"] = stdType;
                variables["S5"] = csvType;
                variables["m"] = m;
            }

            return PVCalculator.Cals[currentKey];
        }

        public PVCalculator GetPVCalculator(string otherRiderCode, Dictionary<string, object> otherVariables)
        {
            //helper클래스에서 다른 담보코드 및 변수의 값을 산출하는 메서드 구현시 사용
            string orgRiderCode = RiderCode;
            Dictionary<string, object> orgVariables = variables.Keys.ToDictionary(x => x, y => variables[y]);

            //RiderRule, Variables 변경
            if (otherRiderCode != null) RiderCode = otherRiderCode;

            //다른 담보의 변수를 끌어 올때 Ax메서드 등의 입력파라미터 뿐만 아니라 VarChg변수까지 적용
            //다른 담보의 Layout변수의 경우 상위담보의 Layout과 충돌가능성이 있어 제외하였음
            otherVariables.Keys.ToList().ForEach(x => variables[x] = otherVariables[x]);
            //ChangeVariables();

            PVCalculator cal = GetPVCalculator();

            //RiderRule, Variables 복원
            if (otherRiderCode != null) RiderCode = orgRiderCode;
            orgVariables.Keys.ToList().ForEach(x => variables[x] = orgVariables[x]);

            return cal;
        }

        private PVCalculator CreatePVCalculatorInstance(int PV_Type)
        {
            try
            {
                Type type = Type.GetType($"PVPlus.PVCALCULATOR.PVType{PV_Type}");
                PVCalculator cal = (PVCalculator)Activator.CreateInstance(type, this);
                return cal;
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    throw new Exception($"PVType{PV_Type}을 찾을 수 없습니다.");
                }
                if (ex.InnerException is InvalidCastException)
                {
                    throw new Exception("캐스트 변환에 실패하였습니다. 레이아웃에서 값을 가져오거나 VarChg시트에 잘못된 cast지정이 있는지 확인바랍니다. ex)int타입의 변수에 double타입의 변수를 넣는 경우...");
                }
                if (ex.InnerException is IndexOutOfRangeException)
                {
                    throw new Exception("인덱스가 배열 범위를 벗어났습니다. (Age+n) 값이 131을 초과하였는지 등 배열 인덱스 값 확인이 필요합니다.");
                }
                throw new Exception(ex.InnerException.Message);
            }
        }

        private PVCalculator CreatePVCalculatorInstance(PVCalculator sub, PVCalculator norm)
        {
            int standardType = (int)variables["S2"];

            PVCalculator cal;
            if (standardType == 1)
                cal = new PVSubstandard(sub, norm);
            else if (standardType == 2)
                cal = new PVSubStandardRound(sub, norm);
            else if (standardType == 3)
                cal = new PVSubstandardSimple(sub, norm);
            else if (standardType == 4)
                cal = new PVSubstandardHanhwaEmergency(sub, norm);
            else
                throw new Exception($"S2 변수의 값 {standardType}이 적절하지 않습니다.");

            return cal;
        }
    }

    public class LineSummary
    {
        public string RiderCode { get; set; }
        public int Jong { get; set; }
        public int S1 { get; set; }
        public int n { get; set; }
        public int MaturityAge { get; set; }
        public int m { get; set; }
        public int Age { get; set; }

        public List<int> Ages = new List<int>();
        public int Count { get; set; }

        public string AgeRange()
        {
            return ConvertSequenceToRangeString(Ages);
        }

        public int MidAge()
        {
            if (!Ages.Any()) return 0;

            // MaturityAge가 0이고 Ages에 40이 포함된 경우
            if (MaturityAge == 0 && Ages.Contains(40))
                return 40;

            var distinctAges = Ages.Distinct().OrderBy(x => x).ToList();
            if (distinctAges.Count == 1)
                return distinctAges[0];

            // 중간 인덱스 계산 (짝수 개수일 경우 두 중간값의 평균)
            int middleIndex = distinctAges.Count / 2;
            if (distinctAges.Count % 2 == 0)
            {
                double average = (distinctAges[middleIndex - 1] + distinctAges[middleIndex]) / 2.0;
                return (int)Math.Ceiling(average);

            }
            return distinctAges[middleIndex];
        }

        public double AgeCount()
        {
            if (Ages == null)
                return 0;

            return Ages.Distinct().Count();
        }

        public double AgeMult()
        {
            var ageCount = AgeCount();
            if (ageCount == 0)
                return 0;

            return Count / ageCount;
        }

        public string ConvertSequenceToRangeString(IEnumerable<int> numbers)
        {
            if (numbers == null || !numbers.Any())
                return string.Empty;

            // 열거형을 정렬된 리스트로 변환
            var sortedNumbers = numbers.OrderBy(x => x).Distinct().ToList();

            var result = new StringBuilder();
            int rangeStart = sortedNumbers[0];
            int previous = sortedNumbers[0];

            // 첫 번째 숫자 추가
            result.Append(rangeStart);

            for (int i = 1; i < sortedNumbers.Count; i++)
            {
                int current = sortedNumbers[i];

                // 연속되지 않은 숫자인 경우
                if (current != previous + 1)
                {
                    // 이전 범위 마무리
                    if (previous != rangeStart)
                    {
                        result.Append('~').Append(previous);
                    }
                    result.Append(',').Append(current);
                    rangeStart = current;
                }
                // 마지막 숫자인 경우
                else if (i == sortedNumbers.Count - 1)
                {
                    result.Append('~').Append(current);
                }

                previous = current;
            }

            return result.ToString();
        }
    }

    public class PVCalculatorKey
    {
        public string RiderCode { get; set; }
        public int Age { get; set; }
        public int n { get; set; }
        public int m { get; set; }

        public int S2 { get; set; }
        public int S3 { get; set; }
        public int S5 { get; set; }
        public int S6 { get; set; }

        public string Substandard_Mode { get; set; }

        public PVCalculatorKey()
        {
            RiderCode = PV.variables["RiderCode"].ToString();
            Age = (int)PV.variables["Age"];
            n = (int)PV.variables["n"];
            m = (int)PV.variables["m"];
            S2 = (int)PV.variables["S2"];
            S3 = (int)PV.variables["S3"];
            S5 = (int)PV.variables["S5"];
            S6 = (int)PV.variables["S6"];
            Substandard_Mode = (string)PV.variables["Substandard_Mode"];
        }

        public string GetKey()
        {
            return $"{RiderCode}|Age:{Age}|n:{n}|m:{m}|S2:{Substandard_Mode}|S3:{S3}|S5:{S5}";
        }

        public string GetKeyWith(string RiderCode = null, int? Age = null, int? n = null, int? m = null, int? S2 = null, int? S3 = null, int? S5 = null, int? S6 = null, string Substandard_Mode = null)
        {
            return $"{RiderCode ?? this.RiderCode}|Age:{Age ?? this.Age}|n:{n ?? this.n}|m:{m ?? this.m}|S2:{Substandard_Mode ?? this.Substandard_Mode}|S3:{S3 ?? this.S3}|S5:{S5 ?? this.S5}";
        }
    }
}
