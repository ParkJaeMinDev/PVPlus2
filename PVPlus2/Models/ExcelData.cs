using System;
using System.Collections.Generic;
using System.Text;

namespace PVPlus2.Models
{
    internal class ExcelData
    {
        public string ExcelFilePath { get; set; } = string.Empty;
        public string DataFolderPath { get; set; } = string.Empty;
        public DateTime LoadedAt { get; set; }

        public Dictionary<string, Product> Product { get; } = [];
        public Dictionary<string, Rider> Rider { get; } = [];

        public Dictionary<string, List<Rate>> Rate { get; } = [];

        public Dictionary<string, List<Layout>> PLayout { get; } = [];
        public Dictionary<string, List<Layout>> VLayout { get; } = [];
        public Dictionary<string, List<Layout>> SLayout { get; } = [];

        public Dictionary<string, List<VarChg>> VarChg { get; } = [];
        public Dictionary<string, List<Expense>> Expense { get; } = [];
        public Dictionary<string, List<SInfo>> SInfo { get; } = [];
        public Dictionary<string, List<ChkExprs>> ChkExprs { get; } = [];
    }
}
