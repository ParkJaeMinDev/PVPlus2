using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public interface ICompanyRule
    {
        // AdjustLine: 예를들어 한화의 경우 팩터가 아닐 경우 읽을 수 없는 특수문자 *을 사용하므로 0으로 치환
        string AdjustLine(string line);
    }

    public enum CompanyNames
    {
        Hanwha, Hyundai, Samsung, BaseCo, Hana, AIG, Heungkuk, MG, Post, KB, KB송부용
    }

    public class BaseCo : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line;
        }
    }

    public class Hyundai : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line;
        }
    }

    public class Hanwha : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line.Replace("*", "0");
        }
    }

    public class Hana : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line;
        }
    }

    public class KB : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line;
        }
    }

    public class KB송부용 : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line;
        }
    }

    public class AIG : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line;
        }
    }

    public class Samsung : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line;
        }
    }

    public class MG : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line + "          ";
        }
    }

    public class Post : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line;
        }
    }

    public class Heungkuk : ICompanyRule
    {
        public string AdjustLine(string line)
        {
            return line;
        }
    }

}
