using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using static System.Windows.Forms.LinkLabel;

namespace PVPlus.UI
{
    abstract public class TFReader
    {
        public double Progress { get; set; }
        public bool IsCancel { get; set; }
        public bool Exist { get; set; }
        public string FilePath { get; set; }
        public Func<string, string> GetPrimaryKey { get; set; }

        protected FileInfo _fi;
        protected DirectoryInfo _di;
        protected string _name;
        protected string _extension;
        protected char _delimiter;
        protected Dictionary<string, int> _layout;

        public virtual List<string> GetValueByLine(string key)
        {
            return new List<string>();
        }
        public virtual List<T> GetValueByClassList<T>(string key) where T : new()
        {
            return new List<T>();
        }
        public virtual List<string> GetKeyList()
        {
            return new List<string>();
        }

        public List<string> ReadAll()
        {
            List<string> result = new List<string>();

            using (StreamReader sr = new StreamReader(FilePath, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    result.Add(sr.ReadLine());
                }
            }

            return result;
        }
        public void Sample(string key)
        {
            DirectoryInfo di = CreatetDirectory("Samples");
            using (StreamWriter sw = new StreamWriter(Path.Combine(di.FullName, key + _extension), false, Encoding.UTF8))
            {
                GetValueByLine(key).ForEach(x => sw.WriteLine(x));
            }
        }
        public void Sample(List<string> keys)
        {
            string type = "Samples";

            DirectoryInfo di = CreatetDirectory("Samples");
            FileInfo fi = CreateFileInfo(type, di);

            using (StreamWriter sw = new StreamWriter(fi.FullName, false, Encoding.UTF8))
            {
                foreach (string s in keys)
                {
                    GetValueByLine(s).ForEach(x => sw.WriteLine(x));
                }
            }
        }
        public void Sort()
        {
            string type = "Sorted";

            DirectoryInfo di = CreatetDirectory("Sorted");
            FileInfo fi = CreateFileInfo(type, di);

            using (StreamWriter sw = new StreamWriter(fi.FullName, false, Encoding.UTF8))
            {
                List<string> sortedKeyList = GetKeyList().OrderBy(x => x).ToList();
                foreach (string key in sortedKeyList)
                {
                    List<string> values = GetValueByLine(key).ToList();
                    WriteLines(sw, values);

                    if (IsCancel) return;
                }
            }
        }
        public void Sort(Func<List<string>, List<string>> orderFunc)
        {
            string type = "Sorted";

            DirectoryInfo di = CreatetDirectory("Sorted");
            FileInfo fi = CreateFileInfo(type, di);

            using (StreamWriter sw = new StreamWriter(fi.FullName, false, Encoding.UTF8))
            {
                List<string> sortedKeyList = GetKeyList().OrderBy(x => x).ToList();
                foreach (string key in sortedKeyList)
                {
                    List<string> values = orderFunc(GetValueByLine(key));
                    WriteLines(sw, values);

                    if (IsCancel) return;
                }
            }
        }
        public void Group(List<TFReader> targets, Func<List<string[]>, object> GetGroupName, Func<List<string[]>, object> GetItem)
        {
            DirectoryInfo di = CreatetDirectory("Grouped");
            TFReader mainTarget = targets.First();

            List<object[]> list = new List<object[]>();

            foreach (string s in mainTarget.GetKeyList())
            {
                List<string[]> vals = targets.Select(x => x.GetValueByLine(s).ToArray()).ToList();

                object groupName = GetGroupName(vals);
                object resultLine = GetItem(vals);

                list.Add(new object[] { groupName, resultLine });
            }

            var groupCollection = list.GroupBy(x => x[0]);

            foreach (IGrouping<object, object> group in groupCollection)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(di.FullName, group.Key + _extension), true, Encoding.UTF8))
                {
                    foreach (object[] line in group)
                    {
                        sw.WriteLine(line[1].ToString());
                    }
                }

                if (IsCancel) return;
            }

        }

        public void LeftJoin(TFReader target, string seperator, Func<string, string> GetAddedLine)
        {
            string type = "LeftJoined";

            DirectoryInfo di = CreatetDirectory("Joined");
            FileInfo fi = CreateFileInfo(type, di);

            using (StreamWriter sw = new StreamWriter(fi.FullName, false, Encoding.UTF8))
            {
                foreach (string key in GetKeyList())
                {
                    List<string> list1 = GetValueByLine(key);
                    List<string> list2 = target.GetValueByLine(key);
                    List<string> joinedList = new List<string>();

                    foreach (string s in list1)
                    {
                        joinedList.Add(s + seperator + GetAddedLine(list2.FirstOrDefault()));
                    }

                    joinedList.ForEach(x => sw.WriteLine(x));
                }
            }
        }
        public void InnerJoin(TFReader target, string seperator, Func<string, string> GetAddedLine)
        {
            string type = "InnerJoined";

            DirectoryInfo di = CreatetDirectory("Joined");
            FileInfo fi = CreateFileInfo(type, di);

            using (StreamWriter sw = new StreamWriter(fi.FullName, false, Encoding.UTF8))
            {
                foreach (string key in GetKeyList())
                {
                    List<string> list1 = GetValueByLine(key);
                    List<string> list2 = target.GetValueByLine(key);
                    List<string> joinedList = new List<string>();

                    foreach (string s in list1)
                    {
                        if (list2.Any())
                        {
                            joinedList.Add(s + seperator + GetAddedLine(list2.First()));
                        }
                    }

                    joinedList.ForEach(x => sw.WriteLine(x));
                }
            }
        }
        public void Split(Func<string, string> GetItem)
        {
            DirectoryInfo di = CreatetDirectory("Splited");
            di.GetFiles().ToList().ForEach(x => x.Delete());

            using (StreamReader sr = new StreamReader(FilePath, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    List<string> lines = ReadLines(sr);
                    var groupCollection = lines.GroupBy(x => GetItem(x));

                    foreach (IGrouping<string, string> group in groupCollection)
                    {
                        using (StreamWriter sw = new StreamWriter(Path.Combine(di.FullName, group.Key + _extension), true, Encoding.UTF8))
                        {
                            foreach (string line in group)
                            {
                                sw.WriteLine(line);
                            }                        
                        }

                        if (IsCancel) return;
                    }
                }
            }

        }
        public void Filter(Func<string, bool> FilterFunc)
        {
            string type = "Filtered";

            DirectoryInfo di = CreatetDirectory(type);
            FileInfo fi = CreateFileInfo(type, di);

            using (StreamReader sr = new StreamReader(FilePath, Encoding.UTF8))
            using (StreamWriter sw = new StreamWriter(fi.FullName, false, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    List<string> lines = ReadLines(sr);
                    List<string> filteredLines = lines.Where(FilterFunc).ToList();
                    foreach (string line in filteredLines)
                    {
                        sw.WriteLine(line);
                    }

                    if (IsCancel) return;
                }
            }
        }
        public void Counter(Func<string, string> GetItem)
        {
            string type = "Counter";

            DirectoryInfo di = CreatetDirectory(type);
            FileInfo fi = CreateFileInfo(type, di);
            Dictionary<string, long> Counter = new Dictionary<string, long>();

            using (StreamReader sr = new StreamReader(FilePath, Encoding.UTF8))
            using (StreamWriter sw = new StreamWriter(fi.FullName, false, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    List<string> lines = ReadLines(sr);

                    foreach (string line in lines)
                    {
                        string item = GetItem(line);

                        if (!Counter.ContainsKey(item))
                        {
                            Counter[item] = 1;
                        }
                        else
                        {
                            Counter[item]++;
                        }
                    }

                    if (IsCancel) return;
                }

                foreach (var s in Counter)
                {
                    sw.WriteLine(s.Key + '\t' + s.Value);
                }
            }
        }
        public void Distinct(Func<string, string> GetItem)
        {
            string type = "Distinct";

            DirectoryInfo di = CreatetDirectory(type);
            FileInfo fi1 = CreateFileInfo(type, di);
            FileInfo fi2 = CreateFileInfo(type + "List", di);

            HashSet<string> distinctSet = new HashSet<string>();

            using (StreamReader sr = new StreamReader(FilePath, Encoding.UTF8))
            using (StreamWriter sw1 = new StreamWriter(fi1.FullName, false, Encoding.UTF8))
            using (StreamWriter sw2 = new StreamWriter(fi2.FullName, false, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    List<string> lines = ReadLines(sr).ToList();
                    List<string> lineList = new List<string>();
                    List<string> itemList = new List<string>();

                    foreach (string line in lines)
                    {
                        string item = GetItem(line);
                        if (!distinctSet.Contains(item))
                        {
                            distinctSet.Add(item);
                            lineList.Add(line);
                            itemList.Add(item);
                        }
                    }

                    WriteLines(sw1, lineList);
                    WriteLines(sw2, itemList);

                    lineList.Clear();
                    itemList.Clear();

                    if (IsCancel) return;
                }
            }
        }
        public void Select(Func<string, string> GetItem)
        {
            string type = "Select";

            DirectoryInfo di = CreatetDirectory(type);
            FileInfo fi = CreateFileInfo(type, di);

            using (StreamReader sr = new StreamReader(FilePath, Encoding.UTF8))
            using (StreamWriter sw = new StreamWriter(fi.FullName, false, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    List<string> lines = ReadLines(sr);
                    List<string> selectedLines = lines.Select(x => GetItem(x)).ToList();
                    foreach (string line in selectedLines)
                    {
                        sw.WriteLine(line);
                    }

                    if (IsCancel) return;
                }
            }
        }

        public void Delete(string type)
        {
            DirectoryInfo di = CreatetDirectory(type);
            di.GetFiles().ToList().ForEach(x => x.Delete());
        }
        protected DirectoryInfo CreatetDirectory(string type)
        {
            DirectoryInfo di1 = _di.CreateSubdirectory(type);
            if (!di1.Exists) di1.Create();

            DirectoryInfo di2 = di1.CreateSubdirectory(_name);
            if (!di2.Exists) di2.Create();

            return di2;
        }
        protected FileInfo CreateFileInfo(string type, DirectoryInfo di)
        {
            int index = 0;
            FileInfo fi = new FileInfo(Path.Combine(di.FullName, type + "_" + _name + _extension));
            while (fi.Exists)
            {
                index++;

                fi = new FileInfo(Path.Combine(di.FullName, type + index + "_" + _name + _extension));
            }

            return fi;
        }
        protected List<string> ReadLines(StreamReader sr)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < 50000; i++)
            {
                if (sr.EndOfStream) break;
                string line = sr.ReadLine();
                result.Add(line);
            }

            Progress = (double)sr.BaseStream.Position / (double)sr.BaseStream.Length;

            return result;
        }
        protected void WriteLines(StreamWriter sw, IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                sw.WriteLine(line);
            }
        }
    }

    public class LTFReader : TFReader
    {
        public Dictionary<string, string> Index { get; set; }
        public string IndexFilePath { get; set; }

        public LTFReader(string path, Func<string, string> GetPrimaryKey)
        {
            this.GetPrimaryKey = GetPrimaryKey;
            this.FilePath = path;
            _fi = new FileInfo(path);
            _name = Path.GetFileNameWithoutExtension(_fi.Name);
            _extension = Path.GetExtension(_fi.Extension);
            _di = _fi.Directory;
            if (_fi.Exists) this.Exist = true;

            DirectoryInfo di = CreatetDirectory("IDXTable");
            this.IndexFilePath = Path.Combine(di.FullName, "IDXTable_" + _name + _extension);
            this.Index = new Dictionary<string, string>();
        }
        public LTFReader(string path, Func<string, string> GetPrimaryKey, char delimiter, Dictionary<string, int> layout) : this(path, GetPrimaryKey)
        {
            // GetValueByClassList 메서드 사용시 해당 생성자로 초기화 필요
            _delimiter = delimiter;
            _layout = layout;
        }
        public LTFReader(string path, Func<string, string> GetPrimaryKey, char delimiter, Type type) : this(path, GetPrimaryKey)
        {
            int i = 0;
            Dictionary<string, int> layout = type.GetProperties().ToDictionary(X => X.Name, x => i++);

            // GetValueByClassList 메서드 사용시 해당 생성자로 초기화 필요
            _delimiter = delimiter;
            _layout = layout;
        }

        public void GenerateIndexTable()
        {
            if (!Exist) return;

            string type = "IDXTable";

            DirectoryInfo di = CreatetDirectory(type);
            di.GetFiles().ToList().ForEach(x => x.Delete());    //기존인덱스삭제

            Dictionary<string, List<long>> indexDic = new Dictionary<string, List<long>>();

            long totalLength = new FileInfo(FilePath).Length;   //파일크기
            int linePos = 0;    //현재 몇번째 줄을 읽고 있는지 표시
            long bytePos = 0;   //txt파일의 byte위치

            string preKey = "_Default";
            string currentKey = "_Default";
            int newLineLen = GetNewLineLen(FilePath);

            using (StreamReader sr = new StreamReader(FilePath, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    currentKey = GetPrimaryKey(line);

                    if (preKey != currentKey)
                    {
                        if (!indexDic.ContainsKey(currentKey))
                        {
                            indexDic.Add(currentKey, new List<long>());
                            indexDic[currentKey].Add(bytePos);
                        }
                        else
                        {
                            indexDic[currentKey].Add(bytePos);
                        }
                    }

                    preKey = currentKey;
                    int lineSize = Encoding.UTF8.GetByteCount(line);
                    bytePos += lineSize + newLineLen;
                    linePos++;

                    if (linePos % 100000 == 0)
                    {
                        Console.Write("\r{1} : {0:F2}% indexing completed", 100.0 * bytePos / totalLength, FilePath);
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter(IndexFilePath, false, Encoding.UTF8))
            {
                List<string> items = indexDic.Keys.OrderBy(x => x).ToList();
                items.ForEach(x => sw.WriteLine(x + "|" + string.Join(",", indexDic[x])));
            }

            Console.WriteLine("\r{0} : 100% indexing completed", FilePath);
        }
        public void LoadIndexTable()
        {
            if (!Exist) return;

            using (StreamReader sr = new StreamReader(IndexFilePath, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] arr = line.Split('|');
                    Index.Add(arr[0], arr[1]);
                }
            }
        }
        public int GetNewLineLen(string path)
        {
            //대부분은 개행라인으로 \r\n을 쓰므로 2를 리턴. 가끔 \n만 쓰는 곳도 있음.
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] buffer = new byte[100000];
                fs.Read(buffer, 0, 100000);
                string str = Encoding.UTF8.GetString(buffer);

                if (str.Contains('\r') && str.Contains('\n')) return 2;
                else return 1;
            }
        }

        public override List<string> GetValueByLine(string key)
        {
            if (!Index.ContainsKey(key)) return new List<string>();

            List<string> result = new List<string>();
            List<long> posList = Index[key].Split(',').Select(x => long.Parse(x)).ToList();

            using (StreamReader sr = new StreamReader(_fi.FullName, Encoding.UTF8))
            {
                foreach (long pos in posList)
                {
                    sr.DiscardBufferedData();
                    sr.BaseStream.Position = pos;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        if (key != GetPrimaryKey(line)) break;
                        result.Add(line);
                    }
                }
            }

            return result;
        }
        public override List<T> GetValueByClassList<T>(string key)
        {
            List<string[]> items = GetValueByLine(key).Select(x => x.Split(_delimiter)).ToList();
            return AutoMapper.ArrListToClassList<T>(items, _layout);
        }
        public override List<string> GetKeyList()
        {
            return Index.Keys.ToList();
        }
    }

    public class STFReader : TFReader
    {
        public Dictionary<string, List<string>> Dict { get; set; }

        public STFReader(string path, Func<string, string> GetKey)
        {
            this.GetPrimaryKey = GetKey;
            this.FilePath = path;
            _fi = new FileInfo(path);
            _name = Path.GetFileNameWithoutExtension(_fi.Name);
            _extension = Path.GetExtension(_fi.Extension);
            _di = _fi.Directory;
            if (_fi.Exists) this.Exist = true;

            Dict = new Dictionary<string, List<string>>();

            if (Exist && GetKey != null)
            {
                using (StreamReader sr = new StreamReader(FilePath, Encoding.UTF8))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string key = GetKey(line);

                        if (!Dict.ContainsKey(key))
                        {
                            Dict.Add(key, new List<string>());
                            Dict[key].Add(line);
                        }
                        else
                        {
                            Dict[key].Add(line);
                        }
                    }
                }
            }
        }
        public STFReader(IEnumerable<string> paths, Func<string, string> GetKey)
        {
            this.GetPrimaryKey = GetKey;
            this.FilePath = paths.First();
            _fi = new FileInfo(paths.First());
            _name = Path.GetFileNameWithoutExtension(_fi.Name);
            _extension = Path.GetExtension(_fi.Extension);
            _di = _fi.Directory;
            if (_fi.Exists) this.Exist = true;

            Dict = new Dictionary<string, List<string>>();

            if (Exist && GetKey != null)
            {
                foreach (string path in paths)
                {
                    if (!new FileInfo(path).Exists) { continue; }

                    using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            string key = GetKey(line);

                            if (!Dict.ContainsKey(key))
                            {
                                Dict.Add(key, new List<string>());
                                Dict[key].Add(line);
                            }
                            else
                            {
                                Dict[key].Add(line);
                            }
                        }
                    }
                }
            }
        }
        public STFReader(string path, Func<string, string> GetKey, char delimiter, Dictionary<string, int> layout) : this(path, GetKey)
        {
            _delimiter = delimiter;
            _layout = layout;
        }
        public STFReader(string path, Func<string, string> GetPrimaryKey, char delimiter, Type type) : this(path, GetPrimaryKey)
        {
            int i = 0;
            Dictionary<string, int> layout = type.GetProperties().ToDictionary(X => X.Name, x => i++);

            // GetValueByClassList 메서드 사용시 해당 생성자로 초기화 필요
            _delimiter = delimiter;
            _layout = layout;
        }

        public override List<string> GetValueByLine(string key)
        {
            if (Dict.ContainsKey(key))
            {
                return Dict[key];
            }
            else
            {
                return new List<string>();
            }
        }
        public override List<T> GetValueByClassList<T>(string key)
        {
            List<string[]> items = GetValueByLine(key).Select(x => x.Split(_delimiter)).ToList();
            return AutoMapper.ArrListToClassList<T>(items, _layout);
        }
        public override List<string> GetKeyList()
        {
            return Dict.Keys.ToList();
        }
    }

    public class TFWriter
    {
        public string Path { get; set; }

        public TFWriter(string path, Type headerType = null, bool Append = false)
        {
            FileInfo fi = new FileInfo(path);
            this.Path = fi.FullName;

            if (!Append)
            {
                //파일명 재설정. 파일1.dat (존재하면->) 파일2.dat (존재하면->) 파일3.dat ...
                FileInfo changedFi = new FileInfo(path);

                int cnt = 0;
                while (changedFi.Exists)
                {
                    cnt++;
                    string fullnameWithoutExtension = fi.FullName.Replace(fi.Extension, "") + cnt;
                    string extension = fi.Extension;
                    changedFi = new FileInfo(fullnameWithoutExtension + extension);
                }
                this.Path = changedFi.FullName;
            }

            if (headerType != null)
            {
                W(string.Join("\t", AutoMapper.GetPropertyNames(headerType)));
            }
        }

        public void W(string item)
        {
            using (StreamWriter sw = new StreamWriter(Path, true, Encoding.UTF8))
            {
                sw.WriteLine(item);
            }
        }

        public void W(List<string> items)
        {
            using (StreamWriter sw = new StreamWriter(Path, true, Encoding.UTF8))
            {
                items.ForEach(x => sw.WriteLine(x));
            }
        }

        public void W<T>(T item)
        {
            using (StreamWriter sw = new StreamWriter(Path, true, Encoding.UTF8))
            {
                sw.WriteLine(AutoMapper.ClassToString(item));
            }
        }

        public void W<T>(List<T> items)
        {
            using (StreamWriter sw = new StreamWriter(Path, true, Encoding.UTF8))
            {
                AutoMapper.ClassListToStringList(items).ForEach(x => sw.WriteLine(x));
            }
        }
    }

    public class AutoMapper
    {
        //Options
        protected static string DecimalTruncationFormat = "{0:f4}";
        protected static Func<string, DateTime> ToDateTimeFunc = DefaultToDateTime;
        //

        public static T ArrToClass<T>(string[] item, Dictionary<string, int> layout) where T : new()
        {
            T result = new T();

            foreach (PropertyInfo p in typeof(T).GetProperties())
            {
                string name = p.Name;
                Type type = p.PropertyType;

                if (layout.ContainsKey(name))
                {
                    int position = layout[name];

                    if (type == typeof(string))
                    {
                        string val = item[position];
                        p.SetValue(result, val, null);
                    }
                    else if (type == typeof(char))
                    {
                        char val = '\t';
                        char.TryParse(item[position], out val);
                        p.SetValue(result, val, null);
                    }
                    else if (type == typeof(bool))
                    {
                        bool val = false;
                        bool.TryParse(item[position], out val);
                        p.SetValue(result, val, null);
                    }
                    else if (type == typeof(double))
                    {
                        double val = 0;
                        double.TryParse(item[position], out val);
                        p.SetValue(result, val, null);
                    }
                    else if (type == typeof(int))
                    {
                        int val = 0;
                        int.TryParse(item[position], out val);
                        p.SetValue(result, val, null);
                    }
                    else if (type == typeof(DateTime))
                    {
                        DateTime val = ToDateTimeFunc(item[position]);
                        p.SetValue(result, val, null);
                    }
                    else
                    {
                        object val = Convert.ChangeType(item[position], type);
                        p.SetValue(result, val, null);
                    }
                }
            }

            return result;
        }

        public static List<T> ArrListToClassList<T>(List<string[]> items, Dictionary<string, int> layout) where T : new()
        {
            return items.Select(x => ArrToClass<T>(x, layout)).ToList();
        }

        public static string ClassToString<T>(T item, string seperator = "\t")
        {
            List<string> list = new List<string>();
            PropertyInfo[] ps = typeof(T).GetProperties().OrderBy(x => x.MetadataToken).ToArray();

            foreach (PropertyInfo p in ps)
            {
                Type type = p.PropertyType;

                if (type == typeof(double))
                {
                    list.Add(string.Format(DecimalTruncationFormat, p.GetValue(item, null)));
                }
                else if (type == typeof(DateTime))
                {
                    list.Add(((DateTime)p.GetValue(item, null)).ToShortDateString());
                }
                else
                {
                    object val = p.GetValue(item, null);
                    if (val != null)
                    {
                        list.Add(val.ToString());
                    }
                    else
                    {
                        list.Add("");
                    }
                }
            }

            return string.Join(seperator, list);
        }

        public static List<string> ClassListToStringList<T>(List<T> items, string seperator = "\t", bool containsHeader = true)
        {
            return items.Select(x => ClassToString(x, seperator)).ToList();
        }

        public static List<T> FileToClassList<T>(string filePath, char delimiter, Dictionary<string, int> layout = null) where T : new()
        {
            List<T> result = new List<T>();

            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                //layout null when firstline is a layout.
                if (layout == null)
                {
                    string[] firstLine = sr.ReadLine().Split(delimiter);
                    layout = Enumerable.Range(0, firstLine.Length).ToDictionary(i => firstLine[i], i => i);
                }

                while (!sr.EndOfStream)
                {
                    string[] item = sr.ReadLine().Split(delimiter);
                    result.Add(ArrToClass<T>(item, layout));
                }
            }

            return result;
        }

        public static List<string> GetPropertyNames(Type type)
        {
            return type.GetProperties().OrderBy(x => x.MetadataToken).Select(x => x.Name).ToList();
        }

        private static DateTime DefaultToDateTime(string s)
        {
            // (yyyy-mm-dd:h.m.s)
            if (s.Length > 10) return DefaultToDateTime(s.Substring(0, 10));
            // (yyyy-mm-dd), (yyyy.mm.dd), ...
            if (s.Length == 10) return new DateTime(int.Parse(s.Substring(0, 4)), int.Parse(s.Substring(5, 2)), int.Parse(s.Substring(8, 2)));
            // (yyyymmdd)
            if (s.Length == 8) return new DateTime(int.Parse(s.Substring(0, 4)), int.Parse(s.Substring(4, 2)), int.Parse(s.Substring(6, 2)));
            // (yyyymm)
            if (s.Length == 6) return new DateTime(int.Parse(s.Substring(0, 4)), int.Parse(s.Substring(4, 2)), 1);
            // default
            return new DateTime(0001, 1, 1);
        }
    }

    public class SubFunc
    {
        public static DateTime DateMax(DateTime a, DateTime b)
        {
            if (a >= b) return a;
            else return b;
        }

        public static DateTime DateMin(DateTime a, DateTime b)
        {
            if (a <= b) return a;
            else return b;
        }

        public static DateTime ToDateTime(string s)
        {
            // (yyyy-mm-dd:h.m.s)
            if (s.Length > 10) return ToDateTime(s.Substring(0, 10));
            // (yyyy-mm-dd), (yyyy.mm.dd), ...
            if (s.Length == 10) return new DateTime(int.Parse(s.Substring(0, 4)), int.Parse(s.Substring(5, 2)), int.Parse(s.Substring(8, 2)));
            // (yyyymmdd)
            if (s.Length == 8) return new DateTime(int.Parse(s.Substring(0, 4)), int.Parse(s.Substring(4, 2)), int.Parse(s.Substring(6, 2)));
            // (yyyymm)
            if (s.Length == 6) return new DateTime(int.Parse(s.Substring(0, 4)), int.Parse(s.Substring(4, 2)), 1);
            // default
            return new DateTime(0001, 1, 1);
        }

        public static double RoundUp(double val, int n)
        {
            return Math.Ceiling(val * Math.Pow(10, n)) / Math.Pow(10, n);
        }

        public static double RoundDown(double val, int n)
        {
            return Math.Floor(val * Math.Pow(10, n)) / Math.Pow(10, n);
        }

        public static void GenerateFileNames(string diPath)
        {
            DirectoryInfo di = new DirectoryInfo(diPath);
            List<FileInfo> fiList = di.GetFiles().ToList();
            using (StreamWriter sw = new StreamWriter(string.Format(@"{0}\{1}", di.FullName, "FileNames.txt"), false, Encoding.UTF8))
            {
                fiList.ForEach(x => sw.WriteLine(x.Name.Replace(x.Extension, "")));
            }
        }

        public static void FileSum(string dirPath, int skip = 0, string searchPattern = "*.txt")
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);

            FileInfo[] fis = di.GetFiles(searchPattern);
            string outPath = Path.Combine(di.FullName, "sum.txt");

            using (StreamWriter sw = new StreamWriter(outPath, false, Encoding.UTF8))
            {
                foreach (FileInfo fi in fis)
                {
                    using (StreamReader sr = new StreamReader(fi.FullName, Encoding.UTF8))
                    {
                        for (int i = 0; i < skip; i++)
                        {
                            sr.ReadLine();
                            if (sr.EndOfStream) break;
                        }

                        while (!sr.EndOfStream)
                        {
                            sw.WriteLine(sr.ReadLine());
                        }
                    }

                }
            }
        }
    }


    public class TextFileReader
    {
        private readonly string _filePath;
        private readonly Encoding _encoding;
        private readonly Dictionary<string, List<long>> _index;
        private readonly Func<string, string> _keySelector;

        public bool IsIndexed { get { return _index.Count > 0; } }
        public double Progress { get; private set; }
        public bool IsCanceled { get; set; }
        public int BufferSize { get; set; }

        public TextFileReader(string filePath, Func<string, string> keySelector)
        {
            _filePath = filePath;
            _encoding = Encoding.UTF8;
            _keySelector = keySelector;
            _index = new Dictionary<string, List<long>>();
            BufferSize = 50000;
        }

        public TextFileReader(string filePath, Func<string, string> keySelector, Encoding encoding)
            : this(filePath, keySelector)
        {
            _encoding = encoding;
        }

        public void CreateIndex()
        {
            _index.Clear();
            string previousKey = null;
            long position = 0;
            long fileSize = new FileInfo(_filePath).Length;

            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader reader = new StreamReader(fs, _encoding))
            {
                while (!reader.EndOfStream && !IsCanceled)
                {
                    string line = reader.ReadLine();
                    string currentKey = _keySelector(line);

                    if (previousKey != currentKey)
                    {
                        if (!_index.ContainsKey(currentKey))
                        {
                            _index[currentKey] = new List<long>();
                        }
                        _index[currentKey].Add(position);
                    }

                    previousKey = currentKey;
                    position = fs.Position;
                    Progress = (double)position / fileSize;
                }
            }
        }

        public void SaveIndex(string indexPath)
        {
            using (StreamWriter writer = new StreamWriter(indexPath, false, _encoding))
            {
                foreach (KeyValuePair<string, List<long>> pair in _index.OrderBy(x => x.Key))
                {
                    writer.WriteLine(string.Format("{0}|{1}",
                        pair.Key,
                        string.Join(",", pair.Value.ConvertAll(x => x.ToString()).ToArray())));
                }
            }
        }

        public void LoadIndex(string indexPath)
        {
            _index.Clear();
            using (StreamReader reader = new StreamReader(indexPath, _encoding))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        List<long> positions = parts[1]
                            .Split(',')
                            .Select(x => long.Parse(x))
                            .ToList();
                        _index[key] = positions;
                    }
                }
            }
        }

        public List<string> GetLines(string key)
        {
            List<string> results = new List<string>();

            if (!_index.ContainsKey(key))
                return results;

            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader reader = new StreamReader(fs, _encoding))
            {
                foreach (long position in _index[key])
                {
                    if (IsCanceled) break;

                    fs.Position = position;
                    string line = reader.ReadLine();

                    while (line != null && _keySelector(line) == key)
                    {
                        results.Add(line);
                        line = reader.ReadLine();
                        if (line == null) break;
                    }
                }
            }

            return results;
        }

        public void ProcessFile(Action<List<string>> processAction)
        {
            List<string> buffer = new List<string>();
            long fileSize = new FileInfo(_filePath).Length;

            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader reader = new StreamReader(fs, _encoding))
            {
                while (!reader.EndOfStream && !IsCanceled)
                {
                    buffer.Clear();
                    for (int i = 0; i < BufferSize && !reader.EndOfStream; i++)
                    {
                        buffer.Add(reader.ReadLine());
                    }

                    if (buffer.Count > 0)
                    {
                        processAction(buffer);
                    }

                    Progress = (double)fs.Position / fileSize;
                }
            }
        }

        public List<string> GetUniqueKeys()
        {
            return new List<string>(_index.Keys);
        }
    }

    public class TextFileWriter
    {
        private readonly string _filePath;
        private readonly Encoding _encoding;
        private readonly bool _append;

        public TextFileWriter(string filePath, bool append = false)
        {
            _filePath = filePath;
            _encoding = Encoding.UTF8;
            _append = append;
        }

        public TextFileWriter(string filePath, Encoding encoding, bool append = false)
            : this(filePath, append)
        {
            _encoding = encoding;
        }

        public void WriteLines(IEnumerable<string> lines)
        {
            using (StreamWriter writer = new StreamWriter(_filePath, _append, _encoding))
            {
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }
            }
        }

        public void WriteLine(string line)
        {
            using (StreamWriter writer = new StreamWriter(_filePath, _append, _encoding))
            {
                writer.WriteLine(line);
            }
        }
    }
}
