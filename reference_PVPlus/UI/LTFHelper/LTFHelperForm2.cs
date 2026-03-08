using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Flee.PublicTypes;
using PVPlus.UI.LTFHelper;
using System.Diagnostics.Eventing.Reader;
using System.Security.Policy;
using System.Text.RegularExpressions;
using PVPlus.RULES;
using System.Security.Permissions;
using System.Data.Common;

namespace PVPlus.UI
{
    public partial class LTFHelperForm2 : Form
    {
        public LTFHelperForm2()
        {
            InitializeComponent();
        }

        //members
        public LTFReader LTFHelper;

        public ExpressionContext context;

        public string Line { get; set; }

        public List<string[]> JoinLines { get; set; }

        public char Delimiter { get; set; }

        public HashSet<string> Set { get; set; }

        public Dictionary<string, string> Dictionary1 { get; set; }

        public Dictionary<string, string> Dictionary2 { get; set; }

        public Dictionary<string, string> Dictionary3 { get; set; }

        public Dictionary<string, IDynamicExpression> ExprDict { get; set; }


        //
        public Color[] ColorSet;

        public TextBox[] TextboxSet;

        public int CurRow = -1;

        public string[] positions;


        //subforms
        public LTFHelperSplitOpt SplitOptForm;

        public LTFHelperFilterOpt FilterOptForm;

        public LTFHelperSortOpt SortOptForm;

        public LTFHelperSelectOpt SelectOptForm;

        public LTFHelperDistinctOptForm DistinctOptForm;

        public LTFHelperGroupOpt MultOptForm;

        public LTFHelperCountOpt CntOptForm;



        private void LTFHelperForm2_Load(object sender, EventArgs e)
        {
            Helper.f = this;

            dataGridView1.AllowDrop = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.RowHeadersVisible = true;

            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.RowTemplate.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView1.RowHeadersWidth = 45;
            dataGridView1.TopLeftHeaderCell.Value = "No.";

            dataGridView1.DragEnter += new DragEventHandler(dataGridView1_DragEnter);
            dataGridView1.DragDrop += new DragEventHandler(dataGridView1_DragDrop);
            dataGridView1.SelectionChanged += new EventHandler(textboxColor_TextChanged);
            dataGridView1.RowsAdded += new DataGridViewRowsAddedEventHandler(dataGridView1_RowAddedOrRemoved);
            dataGridView1.RowsRemoved += new DataGridViewRowsRemovedEventHandler(dataGridView1_RowAddedOrRemoved);

            textBoxYello.TextChanged += new EventHandler(textboxColor_TextChanged);
            textBoxOrange.TextChanged += new EventHandler(textboxColor_TextChanged);
            textBoxRed.TextChanged += new EventHandler(textboxColor_TextChanged);
            textBoxViolet.TextChanged += new EventHandler(textboxColor_TextChanged);
            textBoxBlue.TextChanged += new EventHandler(textboxColor_TextChanged);
            textBoxGreen.TextChanged += new EventHandler(textboxColor_TextChanged);
            comboBox1.SelectedIndexChanged += new EventHandler(textboxColor_TextChanged);

            ColorSet = new Color[] { Color.Yellow, Color.Orange, Color.Red, Color.Violet, Color.Blue, Color.Green };
            TextboxSet = new TextBox[] { textBoxYello, textBoxOrange, textBoxRed, textBoxViolet, textBoxBlue, textBoxGreen };

            context = new ExpressionContext();
            context.Imports.AddType(typeof(Helper));
            context.Variables["a1"] = "";
            context.Variables["a2"] = "";
            context.Variables["a3"] = "";
            context.Variables["a4"] = "";
            context.Variables["a5"] = "";
            context.Variables["a6"] = "";

            SplitOptForm = new LTFHelperSplitOpt();
            FilterOptForm = new LTFHelperFilterOpt();
            SortOptForm = new LTFHelperSortOpt();
            SelectOptForm = new LTFHelperSelectOpt();
            DistinctOptForm = new LTFHelperDistinctOptForm();
            MultOptForm = new LTFHelperGroupOpt();
            CntOptForm = new LTFHelperCountOpt();

            dataGridView1.Columns.Add("col1", "Path");
            dataGridView1.Columns.Add("col2", "GroupBy");
            dataGridView1.Columns[0].Width = 468;
            dataGridView1.Columns[1].Width = 240;
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[0].ReadOnly = true;

            comboBox1.FlatStyle = FlatStyle.System;
            comboBox1.SelectedIndex = 0;

            richTextBoxDict1.KeyDown += new KeyEventHandler(richTextBox_KeyDown);
            richTextBoxDict2.KeyDown += new KeyEventHandler(richTextBox_KeyDown);
            richTextBoxDict3.KeyDown += new KeyEventHandler(richTextBox_KeyDown);
            richTextBoxSet.KeyDown += new KeyEventHandler(richTextBox_KeyDown);

            MultOptForm.button1.Click += new EventHandler(buttonLoop_Click);
            MultOptForm.LoopForm.buttonExistCheck.Click += new EventHandler(buttonExistCheck_Click);
        }

        public void ChangeForm(GroupBox gb, Form f)
        {
            if (f == null)
            {
                gb.Controls.Clear();
                Refresh();
                return;
            }

            f.TopLevel = false;
            //no border if needed
            f.FormBorderStyle = FormBorderStyle.None;
            f.AutoScaleMode = AutoScaleMode.Dpi;

            gb.Controls.Remove(gb.Controls.OfType<Form>().FirstOrDefault());
            gb.Controls.Add(f);
            f.Dock = DockStyle.Bottom;
            f.Show();
            Refresh();
        }

        private string GetDelimiterStr(string line)
        {
            foreach (char c in line)
            {
                if (c == '\t')
                {
                    return "tab";
                }
                else if (c == '|' || c == ',' || c == ';' || c == '@')
                {
                    return c.ToString();
                }
            }

            return "";
        }

        private char GetDelimiter(string delimiterStr)
        {
            switch (delimiterStr)
            {
                case "tab": return '\t';
                case "|": return '|';
                case ",": return ',';
                case ";": return ';';
                case "@": return '@';
                default: return '\0';
            }
        }

        private Dictionary<int, List<string[]>> GetLoopKeyDict()
        {
            Dictionary<int, List<string[]>> LoopKeysDict = new Dictionary<int, List<string[]>>();

            int primaryLoopKeysCount = 0;

            for (int i = 0; i < MultOptForm.LoopForm.dataGridViewLoop.Rows.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(MultOptForm.LoopForm.dataGridViewLoop.Rows[i].Cells[0].Value?.ToString()))
                {
                    primaryLoopKeysCount++;
                }
                else
                {
                    break;
                }
            }


            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                List<string[]> LoopKeys = new List<string[]>();

                for (int j = 0; j < primaryLoopKeysCount; j++)
                {
                    var val = MultOptForm.LoopForm.dataGridViewLoop.Rows[j].Cells[i].Value;

                    if (!string.IsNullOrWhiteSpace(val?.ToString()))
                    {
                        string[] loopKey = val.ToString().Trim().Split(',');
                        LoopKeys.Add(loopKey);
                    }
                    else
                    {
                        string[] loopKey = new string[1] { "Not Found" };
                        LoopKeys.Add(loopKey);
                    }
                }

                LoopKeysDict[i] = LoopKeys;
            }

            return LoopKeysDict;
        }

        public void SetConfigures()
        {
            this.positions = TextboxSet.Select(x => x.Text).ToArray();
            this.Delimiter = GetDelimiter(textBoxDelimiter.Text);

            try
            {
                this.Dictionary1 = richTextBoxDict1.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split(',')).ToDictionary(y1 => y1[0], y2 => y2[1]);
                this.Dictionary2 = richTextBoxDict2.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split(',')).ToDictionary(y1 => y1[0], y2 => y2[1]);
                this.Dictionary3 = richTextBoxDict3.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split(',')).ToDictionary(y1 => y1[0], y2 => y2[1]);
                this.Set = new HashSet<string>(richTextBoxSet.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Distinct());
            }
            catch
            {
                throw new Exception("Set 또는 Dict 입력 형식이 올바르지 않습니다. 중복 값, 오탈자 등을 확인 해 주세요.");
            }

        }

        public void SetVariables(string line)
        {
            int i = 1;
            if (line == "") return;

            foreach (string position in positions)
            {
                if (position != "")
                {
                    string value = "";
                    string mode = position.Contains(",") ? "Offset" : "Delimiter";

                    int pos = (mode == "Offset") ? 0 : int.Parse(position);
                    int start = (mode == "Offset") ? int.Parse(position.Split(',')[0]) : 0;
                    int end = (mode == "Offset") ? int.Parse(position.Split(',')[1]) : 0;

                    if (mode == "Delimiter") value = line.Split(Delimiter)[pos];
                    else if (mode == "Offset") value = line.Substring(start, end);

                    context.Variables["a" + i++] = value;
                }
                else
                {
                    context.Variables["a" + i++] = "";
                }
            }
        }

        public object GetValue(IDynamicExpression expr, string line)
        {
            try
            {
                this.Line = line;
                SetVariables(line);
                return expr.Evaluate();
            }
            catch
            {
                throw new Exception($"수식계산에 실패하였습니다: {line}");
            }
        }

        public object GetValue(IDynamicExpression expr, List<string[]> lines)
        {
            this.JoinLines = lines;
            return expr.Evaluate();
        }

        public object GetValueOrDefault(IDynamicExpression expr, string line, object defaultVal)
        {
            try
            {
                SetVariables(line);
                return expr.Evaluate();
            }
            catch
            {
                return defaultVal;
            }
        }

        public IDynamicExpression GetExpr(string s)
        {
            if (ExprDict == null) ExprDict = new Dictionary<string, IDynamicExpression>();

            if (!ExprDict.ContainsKey(s))
            {
                ExprDict[s] = context.CompileDynamic(s);
            }

            return ExprDict[s];
        }

        public IDynamicExpression Compile(string exprStr)
        {
            try
            {
                return context.CompileDynamic(exprStr);
            }
            catch
            {
                throw new Exception($"수식 오류 발생: {exprStr}");
            }
        }

        public double Sum(int n, string exprStr)
        {
            exprStr = exprStr.Trim();
            IDynamicExpression expr = GetExpr(exprStr);
            double sumVal = 0;

            foreach (string item in JoinLines[n])
            {
                this.Line = item;
                SetVariables(item);

                try
                {
                    if (string.IsNullOrWhiteSpace(expr.Evaluate().ToString())) continue;
                    sumVal += Convert.ToDouble(expr.Evaluate().ToString().Trim());
                }
                catch
                {
                    throw new Exception($"{expr.Evaluate().ToString()}을 double 타입으로 변환 할 수 없습니다.");
                }
            }

            return sumVal;
        }

        public string First(int n, string exprStr = null)
        {
            if (exprStr == null) return JoinLines[n].FirstOrDefault() ?? "Not Found";

            exprStr = exprStr.Trim();
            IDynamicExpression expr = GetExpr(exprStr);
            string item = JoinLines[n].First();

            this.Line = item;
            SetVariables(item);

            object val = expr.Evaluate();

            return val.ToString();
        }

        public string Last(int n, string exprStr = null)
        {
            if (exprStr == null) return JoinLines[n].LastOrDefault() ?? "Not Found";

            exprStr = exprStr.Trim();
            IDynamicExpression expr = GetExpr(exprStr);
            string item = JoinLines[n].Last();

            this.Line = item;
            SetVariables(item);

            object val = expr.Evaluate();

            return val.ToString();
        }

        public string All(int n)
        {
            return string.Join("_", JoinLines[n]);
        }

        public int Count(int n)
        {
            return JoinLines[n].Count();
        }

        public string Key()
        {
            IDynamicExpression exprKey = GetExpr(dataGridView1.Rows[0].Cells[1].Value.ToString());
            string key = GetValue(exprKey, JoinLines[0].FirstOrDefault()).ToString();

            return key;
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy | DragDropEffects.Scroll;
            }
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] strFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string strFile in strFiles.OrderBy(x => x))
                {
                    dataGridView1.Rows.Add(strFile);
                }

                dataGridView1.CurrentCell = dataGridView1[0, dataGridView1.Rows.Count - 1];
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow.Index != CurRow)
                {
                    richTextBoxSamples.Clear();
                    CurRow = dataGridView1.CurrentRow.Index;

                    using (StreamReader sr = new StreamReader(dataGridView1.CurrentRow.Cells[0].Value.ToString()))
                    {
                        for (int i = 0; i < 200; i++)
                        {
                            if (sr.EndOfStream) break;

                            string line = sr.ReadLine();
                            if (i == 0) textBoxDelimiter.Text = GetDelimiterStr(line);
                            richTextBoxSamples.AppendText(line + "\n");
                        }
                    }
                }

                if (dataGridView1.CurrentCell.ColumnIndex == 1)
                {
                    dataGridView1_CellEndEdit(sender, null);
                }
            }
            catch { }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (e.Button == MouseButtons.Right)
                {
                    contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
                }
            }
            catch
            {

            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int row = dataGridView1.CurrentCell.RowIndex;
            int column = dataGridView1.CurrentCell.ColumnIndex;
            if (column != 1) return;

            try
            {
                richTextSampeKey.Clear();
                SetConfigures();

                string keyExprStr = dataGridView1[column, row].Value.ToString();
                IDynamicExpression expr = context.CompileDynamic(keyExprStr);

                foreach (string line in richTextBoxSamples.Text.Split('\n'))
                {
                    string newLine = GetValue(expr, line).ToString();
                    richTextSampeKey.AppendText(newLine + "\n");
                }
            }
            catch
            {

            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "삭제")
            {
                if (dataGridView1.CurrentRow.Index < dataGridView1.Rows.Count)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
                }
            }
            else if (e.ClickedItem.Text == "전체삭제")
            {
                dataGridView1.Rows.Clear();
                richTextBoxSamples.Clear();
            }
            else if (e.ClickedItem.Text == "Help")
            {

            }
        }

        private void dataGridView1_RowAddedOrRemoved(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index);
            }
        }

        private void textboxColor_TextChanged(object sender, EventArgs e)
        {
            richTextBoxSamples.SelectAll();
            richTextBoxSamples.SelectionBackColor = richTextBoxSamples.BackColor;
            List<string> textList = richTextBoxSamples.Text.Split('\n').ToList();
            int colorIdx = 0;

            List<Tuple<Color, string>> tuples = new List<Tuple<Color, string>>();

            foreach (TextBox textbox in TextboxSet)
            {
                Color backColor = ColorSet[colorIdx++];
                if (textbox.Text == "") continue;

                try
                {
                    int positionByDelimiter = 0;
                    int[] positionsByLength = new int[2];

                    if (textbox.Text.Contains(","))
                    {
                        positionsByLength = textbox.Text.Split(',').Select(x => int.Parse(x)).ToArray();
                    }
                    else
                    {
                        positionByDelimiter = int.Parse(textbox.Text);
                    }

                    char delimiter = GetDelimiter(textBoxDelimiter.Text);
                    int lineIdx = 0;
                    int start = 0;

                    foreach (string line in textList)
                    {

                        string[] arr = line.Split(delimiter);
                        int offset = 0;
                        int length = 0;

                        if (positionsByLength[1] == 0)
                        {
                            //By delimiter
                            offset = arr.Take(positionByDelimiter).Sum(x => x.Length + 1);
                            length = arr[positionByDelimiter].Length;
                        }
                        else
                        {
                            //By offest
                            offset = positionsByLength[0];
                            length = positionsByLength[1];
                        }

                        if (offset + length <= line.Length)
                        {
                            richTextBoxSamples.SelectionStart = start + offset;
                            richTextBoxSamples.SelectionLength = length;
                            richTextBoxSamples.SelectionBackColor = backColor;
                        }

                        tuples.Add(new Tuple<Color, string>(backColor, richTextBoxSamples.SelectedRtf));

                        start += line.Length + 1;
                        lineIdx++;
                    }

                    
                }

                catch { }

                
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Helper.f = this;
                string path = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                LTFHelper = new LTFReader(path, x => x);
                ButtonCancel.Visible = true;

                SetConfigures();

                if (comboBox1.SelectedItem.ToString() == "Split")
                {
                    IDynamicExpression expr = Compile(SplitOptForm.GetExpression());

                    Task.Run(() =>
                    {
                        try
                        {
                            this.Invoke(new Action(() => timer1.Enabled = true));
                            LTFHelper.Split(x => GetValue(expr, x).ToString());
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = "Split 완료"));
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = ex.Message));
                        }

                    });
                }

                if (comboBox1.SelectedItem.ToString() == "Filter")
                {
                    IDynamicExpression expr = Compile(FilterOptForm.GetExpression());

                    Task.Run(() =>
                    {
                        try
                        {
                            this.Invoke(new Action(() => timer1.Enabled = true));
                            LTFHelper.Filter(x => (bool)GetValue(expr, x));
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = "Filter 완료"));
                        }
                        catch (Exception ex)
                        {
                            timer1.Enabled = false;
                            textBoxProgress.Invoke(new Action(() => textBoxProgress.Text = ex.Message));
                        }
                    });
                }

                if (comboBox1.SelectedItem.ToString() == "Sort")
                {
                    IDynamicExpression[] exprs = SortOptForm.GetExpressions()
                        .Where(x => x.ToString() != "")
                        .Skip(1)
                        .Select(x => Compile(x))
                        .ToArray();

                    Func<List<string>, List<string>> orderFunc = list =>
                    {
                        if (exprs.Any())
                        {
                            var query = list.OrderBy(y => GetValue(exprs.First(), y));

                            foreach (IDynamicExpression expr in exprs.Skip(1))
                            {
                                if (expr == null) continue;
                                query.ThenBy(y => GetValue(expr, y));
                            }

                            return query.ToList();
                        }
                        else
                        {
                            return list;
                        }
                    };

                    IDynamicExpression exprPrimary = Compile(SortOptForm.GetExpressions()[0]);
                    LTFHelper = new LTFReader(path, x => GetValue(exprPrimary, x).ToString());

                    Task.Run(() =>
                    {
                        try
                        {
                            this.Invoke(new Action(() => timer1.Enabled = true));
                            this.Invoke(new Action(() => labelProgress.Text = "Index생성중"));
                            this.Invoke(new Action(() => LTFHelper.GenerateIndexTable()));
                            this.Invoke(new Action(() => LTFHelper.LoadIndexTable()));
                            this.Invoke(new Action(() => LTFHelper.Sort(x => orderFunc(x))));
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = "Sort 완료"));
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = ex.Message));
                        }

                    });
                }

                if (comboBox1.SelectedItem.ToString() == "Select")
                {
                    IDynamicExpression expr = Compile(SelectOptForm.GetExpression());
                    Task.Run(() =>
                    {
                        try
                        {
                            this.Invoke(new Action(() => timer1.Enabled = true));
                            LTFHelper.Select(x => GetValue(expr, x).ToString());
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = "Select 완료"));
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = ex.Message));
                        }
                    });
                }

                if (comboBox1.SelectedItem.ToString() == "Distinct")
                {
                    IDynamicExpression expr = Compile(DistinctOptForm.GetExpression());
                    Task.Run(() =>
                    {
                        try
                        {
                            this.Invoke(new Action(() => timer1.Enabled = true));
                            LTFHelper.Distinct(x => GetValue(expr, x).ToString());
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = "Distinct 완료"));
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = ex.Message));
                        }
                    });
                }

                if (comboBox1.SelectedItem.ToString() == "Group")
                {
                    if (MultOptForm.textBoxGroupName.Text == "") throw new Exception("GroupName 입력이 필요합니다");
                    if (MultOptForm.textBoxWriteLine.Text == "") throw new Exception("WriteLine 입력이 필요합니다");

                    IDynamicExpression exprGroupName = Compile(MultOptForm.textBoxGroupName.Text);
                    IDynamicExpression exprWriteLine = Compile(MultOptForm.textBoxWriteLine.Text);

                    Func<List<string[]>, object> GetGroupName = x => GetValue(exprGroupName, x);
                    Func<List<string[]>, object> GetItem = x => GetValue(exprWriteLine, x);

                    
                    STFReader JoinSTFReader = new STFReader(dataGridView1.Rows[0].Cells[0].Value.ToString(), null);

                    timer1.Enabled = true;

                    Task.Run(() => {
                        try
                        {
                            JoinSTFReader.Delete("Grouped");

                            Dictionary<int, List<string[]>> LoopKeysDict = GetLoopKeyDict();
                            List<string[]> primaryLoopKeys = LoopKeysDict[0];


                            for (int i = 0; i < primaryLoopKeys.Count(); i++)
                            {
                                List<TFReader> readers = new List<TFReader>();

                                for (int j = 0; j < dataGridView1.Rows.Count; j++)
                                {
                                    IDynamicExpression exprKey = GetExpr(dataGridView1.Rows[j].Cells[1].Value.ToString());
                                    Func<string, string> GetKey = x => GetValue(exprKey, x).ToString();

                                    FileInfo rootPathFi = new FileInfo(dataGridView1.Rows[j].Cells[0].Value.ToString());
                                    List<string> filePaths = LoopKeysDict[j][i].Select(x => $"{rootPathFi.Directory}\\Splited\\{rootPathFi.Name.Replace(rootPathFi.Extension, "")}\\{x}{rootPathFi.Extension}").ToList();

                                    STFReader stf = new STFReader(filePaths, GetKey);
                                    readers.Add(stf);
                                }

                                JoinSTFReader.Group(readers, GetGroupName, GetItem);
                                this.Invoke(new Action(() => textBoxProgress.Text = $"Loop Progress: {i}/{primaryLoopKeys.Count()}"));
                            }

                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = "Grouped 완료"));
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = ex.Message));
                        }
                    });
                }

                if (comboBox1.SelectedItem.ToString() == "Count")
                {
                    IDynamicExpression expr = Compile(CntOptForm.GetExpression());

                    Task.Run(() =>
                    {
                        try
                        {
                            this.Invoke(new Action(() => timer1.Enabled = true));
                            LTFHelper.Counter(x => GetValue(expr, x).ToString());
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = "Count 완료"));
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() => timer1.Enabled = false));
                            this.Invoke(new Action(() => textBoxProgress.Text = ex.Message));
                        }

                    });
                }

            }
            catch (Exception ex)
            {
                timer1.Enabled = false;
                textBoxProgress.Invoke(new Action(() => textBoxProgress.Text = ex.Message));
            }
        }

        private void buttonLoop_Click(object sender, EventArgs e)
        {
            MultOptForm.LoopForm.Show();

            for (int i = 0; i < 30; i++)
            {
                if(i < dataGridView1.Rows.Count)
                {
                    MultOptForm.LoopForm.dataGridViewLoop.Columns[i].Visible = true;
                }
                else
                {
                    MultOptForm.LoopForm.dataGridViewLoop.Columns[i].Visible = false;

                }

            }
        }

        private void buttonExistCheck_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < MultOptForm.LoopForm.dataGridViewLoop.RowCount; i++)
                {
                    for (int j = 0; j < dataGridView1.RowCount; j++)
                    {
                        if (string.IsNullOrWhiteSpace(MultOptForm.LoopForm.dataGridViewLoop.Rows[i].Cells[j].Value?.ToString()))
                        {
                            MultOptForm.LoopForm.dataGridViewLoop.Rows[i].Cells[j].Style.BackColor = Color.White;
                            continue;
                        }

                        string[] loopItems = MultOptForm.LoopForm.dataGridViewLoop.Rows[i].Cells[j].Value.ToString().Split(',').Select(x => x.Trim()).ToArray();
                        List<bool> loopFileExist = new List<bool>();

                        for (int k = 0; k < loopItems.Count(); k++)
                        {
                            FileInfo rootFi = new FileInfo(dataGridView1.Rows[j].Cells[0].Value.ToString());
                            FileInfo loopFi = new FileInfo($"{rootFi.Directory}\\Splited\\{rootFi.Name.Replace(rootFi.Extension, "")}\\{loopItems[k]}{rootFi.Extension}");

                            if (loopFi.Exists) loopFileExist.Add(true);
                            else loopFileExist.Add(false);
                        }

                        if (loopFileExist.Any(x => x == false)) MultOptForm.LoopForm.dataGridViewLoop.Rows[i].Cells[j].Style.BackColor = Color.Red;
                        else MultOptForm.LoopForm.dataGridViewLoop.Rows[i].Cells[j].Style.BackColor = Color.LightGreen;
                    }
                }
            }
            catch { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Group") return;
            if (LTFHelper != null) textBoxProgress.Invoke(new Action(() => textBoxProgress.Text = (LTFHelper.Progress * 100).ToString("F2") + "%"));
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (LTFHelper != null) this.Invoke(new Action(() => LTFHelper.IsCancel = true));
            if (LTFHelper != null) this.Invoke(new Action(() => textBoxProgress.Text = "Cancel"));
        }

        private void richTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                ((RichTextBox)sender).Paste(DataFormats.GetFormat(DataFormats.Text));

                var f = ((RichTextBox)sender).Font;
                int s = ((RichTextBox)sender).SelectionStart;

                ((RichTextBox)sender).SelectAll();
                ((RichTextBox)sender).SelectionFont = f;
                ((RichTextBox)sender).SelectionStart = s;
                ((RichTextBox)sender).SelectionLength = 0;

                e.Handled = true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Split")
            {
                ChangeForm(groupBox1, SplitOptForm);
            }
            else if (comboBox1.SelectedItem.ToString() == "Filter")
            {
                ChangeForm(groupBox1, FilterOptForm);
            }
            else if (comboBox1.SelectedItem.ToString() == "Sort")
            {
                ChangeForm(groupBox1, SortOptForm);
            }
            else if (comboBox1.SelectedItem.ToString() == "Select")
            {
                ChangeForm(groupBox1, SelectOptForm);
            }
            else if (comboBox1.SelectedItem.ToString() == "Distinct")
            {
                ChangeForm(groupBox1, DistinctOptForm);
            }
            else if (comboBox1.SelectedItem.ToString() == "Group")
            {
                ChangeForm(groupBox1, MultOptForm);
            }
            else if (comboBox1.SelectedItem.ToString() == "Count")
            {
                ChangeForm(groupBox1, CntOptForm);
            }
        }

        private void comboBoxPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Split")
            {
                if (comboBoxPreset.SelectedItem.ToString() == "Hanwha") textBoxYello.Text = "10,8";
                if (comboBoxPreset.SelectedItem.ToString() == "Hyundai") textBoxYello.Text = "20,4";
                if (comboBoxPreset.SelectedItem.ToString() == "Samsung") textBoxYello.Text = "1";
                if (comboBoxPreset.SelectedItem.ToString() == "Hana") textBoxYello.Text = "0";
            }

            if (comboBox1.SelectedItem.ToString() == "Group")
            {
                if (comboBoxPreset.SelectedItem.ToString() == "Hanwha")
                {
                    textBoxYello.Text = "10,8";
                    textBoxOrange.Text = "28,56";
                    textBoxRed.Text = "94,33";
                    textBoxViolet.Text = "147,3";
                    textBoxBlue.Text = "196,15";

                    MultOptForm.textBoxGroupName.Text = "If(Count(1) = 0, \"MatchFailed\", If(Sum(0, \"a5\") <= Sum(1, \"a5\"), \"True\",\"False\"))";
                    MultOptForm.textBoxWriteLine.Text = "Join(\"\\t\", Key(), Count(0), Count(1), Sum(0, \"a5\"), Sum(1, \"a5\"), First(0), First(1))";

                    foreach (var item in dataGridView1.Rows)
                    {
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            dataGridView1.Rows[i].Cells[1].Value = "a1+a2+a3+a4";
                        }
                    }
                }
                if (comboBoxPreset.SelectedItem.ToString() == "Heungkuk")
                {
                    textBoxBlue.Text = "24";

                    MultOptForm.textBoxGroupName.Text = "If(Count(1) = 0, \"MatchFailed\", If(Sum(0, \"a5\") <= Sum(1, \"a5\"), \"True\",\"False\"))";
                    MultOptForm.textBoxWriteLine.Text = "Join(\"\\t\", Key(), Count(0), Count(1), Sum(0, \"a5\"), Sum(1, \"a5\"), First(0), First(1))";

                    foreach (var item in dataGridView1.Rows)
                    {
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            dataGridView1.Rows[i].Cells[1].Value = "Join(\"|\",sub(3),sub(4),sub(5),sub(6),sub(7),sub(8),sub(9),sub(10),sub(11),sub(12),sub(13),sub(14),sub(15),sub(16),sub(17),sub(18),sub(19),sub(20))";
                        }
                    }
                }
                if (comboBoxPreset.SelectedItem.ToString() == "Hana")
                {
                    textBoxBlue.Text = "12";

                    MultOptForm.textBoxGroupName.Text = "If(Count(1) = 0, \"MatchFailed\", If(Sum(0, \"a5\") <= Sum(1, \"a5\"), \"True\",\"False\"))";
                    MultOptForm.textBoxWriteLine.Text = "Join(\"\\t\", Key(), Count(0), Count(1), Sum(0, \"a5\"), Sum(1, \"a5\"), First(0), First(1))";

                    foreach (var item in dataGridView1.Rows)
                    {
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            dataGridView1.Rows[i].Cells[1].Value = "Join(\"|\",sub(1),sub(2),sub(3),sub(4),sub(5),sub(6),sub(7),sub(8),sub(9),sub(10),sub(11))";
                        }
                    }
                }
            }
        }
    }

    public class DataGridView2 : DataGridView
    {

    }

    public static class Helper
    {
        public static LTFHelperForm2 f { get; set; }

        public static int ToInt(object s)
        {
            return Convert.ToInt32(s);
        }

        public static double ToDouble(object s)
        {
            return Convert.ToDouble(s);
        }

        public static string ToString(object s)
        {
            return s.ToString();
        }

        public static bool SetContains(string s)
        {
            return f.Set.Contains(s);
        }

        public static bool ContainsKeyDict1(string s)
        {
            return f.Dictionary1.ContainsKey(s);
        }

        public static bool ContainsKeyDict2(string s)
        {
            return f.Dictionary1.ContainsKey(s);
        }

        public static bool ContainsKeyDict3(string s)
        {
            return f.Dictionary1.ContainsKey(s);
        }

        public static string Dict1(string s1)
        {
            if (f.Dictionary1.ContainsKey(s1)) return f.Dictionary1[s1];
            else return "NotMatch";
        }

        public static string Dict2(string s1)
        {
            if (f.Dictionary2.ContainsKey(s1)) return f.Dictionary2[s1];
            else return "NotMatch";
        }

        public static string Dict3(string s1)
        {
            if (f.Dictionary3.ContainsKey(s1)) return f.Dictionary3[s1];
            else return "NotMatch";
        }

        public static string Dict1(string s1, string s2)
        {
            if (f.Dictionary1.ContainsKey(s1)) return f.Dictionary1[s1];
            else return s2;
        }

        public static string Dict2(string s1, string s2)
        {
            if (f.Dictionary2.ContainsKey(s1)) return f.Dictionary2[s1];
            else return s2;
        }

        public static string Dict3(string s1, string s2)
        {
            if (f.Dictionary3.ContainsKey(s1)) return f.Dictionary3[s1];
            else return s2;
        }


        public static string Sub(int a)
        {
            return f.Line.Split(f.Delimiter)[a];
        }

        public static string Sub(int a, int b)
        {
            return f.Line.Substring(a, b);
        }

        public static string Join(string seperator, params object[] values)
        {
            return string.Join(seperator, values);
        }

        public static double Sum(int n, string exprStr)
        {
            return f.Sum(n, exprStr);
        }

        public static int Count(int n)
        {
            return f.Count(n);
        }

        public static string Key()
        {
            return f.Key();
        }

        public static string First(int n)
        {
            return f.First(n, null);
        }

        public static string Last(int n)
        {
            return f.First(n, null);
        }

        public static string All(int n)
        {
            return f.All(n);
        }

    }
}
