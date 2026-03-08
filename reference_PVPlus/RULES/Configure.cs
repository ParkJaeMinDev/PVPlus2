using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    //Configure 
    public static class Configure
    {
        public static TableType TableType;
        public static DirectoryInfo WorkingDI;
        public static FileInfo PVSTableInfo;
        public static ICompanyRule CompanyRule;
        public static string ProductCode;
        public static string SeperationType;
        public static char Delimiter = '\t';
        public static bool LimitExcessChecked;
        public static bool LineSummaryChecked;
    }

    //Tables 
    public enum TableType
    {
        //P, V, 표준해약공제액, S산출
        P, V, SRatio, StdAlpha
    }
}
