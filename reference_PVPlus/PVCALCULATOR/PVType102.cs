using PVPlus.RULES;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType102 : PVType11
    {
        public PVType102(LineInfo line) : base(line)
        {
            c = FindLowestPremiumGrade();
        }

        public CommutationTable FindLowestPremiumGrade()
        {
            int grade = (int)variables["F6"];
            int age = (int)variables["Age"];

            int n = (int)variables["n"];
            int m = (int)variables["m"];
            int t = (int)variables["t"];
            int freq = (int)variables["freq"];

            List<(double, CommutationTable)> c_grades = new List<(double, CommutationTable)>();

            //디버깅용 grades2
            //List<(double, CommutationTable)> c_grades2 = new List<(double, CommutationTable)>();

            for (int i = 0; i <= grade; i++)
            {
                CommutationTable c_grade = MakeCommutationTable();
                c = c_grade;

                double GP = base.Get영업보험료(n, m, t, freq);
                c_grades.Add((GP, c_grade));

                //디버깅용 준비금
                //double V = base.Get준비금(n, m, 1, 12);
                //c_grades2.Add((V, c_grade));

                variables["Age"] = (int)variables["Age"] - 1;
            }

            variables["Age"] = age; //원래 나이로 복구

            //영업보험료가 가장 낮은 등급 선택
            return c_grades.OrderBy(x => x.Item1).First().Item2;
        }
    }
}
