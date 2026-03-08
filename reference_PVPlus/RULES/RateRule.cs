using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class RateRule
    {
        public string 위험률명 { get; set; }
        public string 적용년월 { get; set; }
        public int 기간 { get; set; }

        //RateFactors
        public Nullable<int> F1 { get; set; }
        public Nullable<int> F2 { get; set; }
        public Nullable<int> F3 { get; set; }
        public Nullable<int> F4 { get; set; }
        public Nullable<int> F5 { get; set; }
        public Nullable<int> F6 { get; set; }
        public Nullable<int> F7 { get; set; }
        public Nullable<int> F8 { get; set; }
        public Nullable<int> F9 { get; set; }
        //

        public double[] RateArr { get; set; }
    }
}
