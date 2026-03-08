using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVPlus
{
    public partial class SamsungTextModifierForm : Form
    {
        private MainPVForm mainForm;

        private SamsungTableConverter converter;

        public SamsungTextModifierForm()
        {
            InitializeComponent();
        }

        public SamsungTextModifierForm(MainPVForm mainForm) : this()
        {
            this.mainForm = mainForm;
        }

        private void SamsungTextModifierForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonOpenPPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofdlg = new OpenFileDialog())
            {
                ofdlg.ShowDialog();

                if (ofdlg.FileName != "")
                {
                    textBoxPPath.Text = ofdlg.FileName;
                }
            }
        }

        private void buttonOpenVPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofdlg = new OpenFileDialog())
            {
                ofdlg.ShowDialog();

                if (ofdlg.FileName != "")
                {
                    textBoxVPath.Text = ofdlg.FileName;
                }
            }
        }

        private void buttonOpenTPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofdlg = new OpenFileDialog())
            {
                ofdlg.ShowDialog();

                if (ofdlg.FileName != "")
                {
                    textBoxTPath.Text = ofdlg.FileName;
                }
            }
        }

        private void buttonOpenEPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofdlg = new OpenFileDialog())
            {
                ofdlg.ShowDialog();

                if (ofdlg.FileName != "")
                {
                    textBoxEPath.Text = ofdlg.FileName;
                }
            }
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            string P = textBoxPPath.Text == "" ? null : textBoxPPath.Text;
            string V = textBoxVPath.Text == "" ? null : textBoxVPath.Text;
            string T = textBoxTPath.Text == "" ? null : textBoxTPath.Text;
            string E = textBoxEPath.Text == "" ? null : textBoxEPath.Text;

            try
            {
                converter = new SamsungTableConverter(P, V, T, E);
                converter.Start = int.Parse(textBox1.Text.Split(',')[0]);
                converter.Count = int.Parse(textBox1.Text.Split(',')[1]);
            }
            catch
            {
                labelProgress.Text = "오류 발생 substring 위치를 확인 해 주세요. ex - 0,8";
            }


            timer1.Enabled = true;
            Task t = Task.Run(() => converter.ConvertAll());
            t.ContinueWith(x => Invoke(new Action(() => timer1.Enabled = false)));
            t.ContinueWith(x => Invoke(new Action(() => labelProgress.Text = converter.ProgressMessage)));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelProgress.Invoke(new Action(() => labelProgress.Text = converter.ProgressMessage));
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            mainForm.Show();
            Hide();
        }

        private void SamsungTextModifierForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            mainForm.Show();
            mainForm.Close();
        }
    }

    class SamsungTableConverter
    {
        public string ProgressMessage { get; set; }

        public int Start { get; set; }
        public int Count { get; set; }
        public int Delimiter { get; set; }

        private FileInfo PPath;
        private FileInfo VPath;
        private FileInfo EPath;
        private FileInfo TPath;

        private FileInfo OutPPath;
        private FileInfo OutVPath;
        private FileInfo OutEPath;
        private FileInfo OutTPath;

        private FileInfo OutPErrPath;
        private FileInfo OutVErrPath;
        private FileInfo OutEErrPath;
        private FileInfo OutTErrPath;

        private long errCnt;
        private Dictionary<string, string> dict;

        public SamsungTableConverter(string P, string V, string E, string T)
        {
            PPath = (P != null) ? new FileInfo(P) : null;
            VPath = (V != null) ? new FileInfo(V) : null;
            EPath = (E != null) ? new FileInfo(E) : null;
            TPath = (T != null) ? new FileInfo(T) : null;

            OutPPath = (P != null) ? new FileInfo(PPath.DirectoryName + @"\Converted_" + PPath.Name) : null;
            OutVPath = (V != null) ? new FileInfo(VPath.DirectoryName + @"\Converted_" + VPath.Name) : null;
            OutEPath = (E != null) ? new FileInfo(EPath.DirectoryName + @"\Converted_" + EPath.Name) : null;
            OutTPath = (T != null) ? new FileInfo(TPath.DirectoryName + @"\Converted_" + TPath.Name) : null;

            OutPErrPath = (P != null) ? new FileInfo(PPath.DirectoryName + @"\Err_" + PPath.Name) : null;
            OutVErrPath = (V != null) ? new FileInfo(VPath.DirectoryName + @"\Err_" + VPath.Name) : null;
            OutEErrPath = (E != null) ? new FileInfo(EPath.DirectoryName + @"\Err_" + EPath.Name) : null;
            OutTErrPath = (T != null) ? new FileInfo(TPath.DirectoryName + @"\Err_" + TPath.Name) : null;

            if (!PPath.Exists)
            {
                throw new Exception("P테이블(기준테이블) 경로는 반드시 존재 하여야 합니다.");
            }
        }

        public void ConvertAll()
        {
            SetDict();
            ConvertP();
            if (VPath != null && VPath.Exists) ConvertV();
            if (TPath != null && TPath.Exists) ConvertT();
            if (EPath != null && EPath.Exists) ConvertE();

            if (OutPErrPath != null && OutPErrPath.Exists && OutPErrPath.Length == 0) OutPErrPath.Delete();
            if (OutVErrPath != null && OutVErrPath.Exists && OutVErrPath.Length == 0) OutVErrPath.Delete();
            if (OutTErrPath != null && OutTErrPath.Exists && OutTErrPath.Length == 0) OutTErrPath.Delete();
            if (OutEErrPath != null && OutEErrPath.Exists && OutEErrPath.Length == 0) OutEErrPath.Delete();

            ProgressMessage = $"Conversion completed. {"Err:" + errCnt}";
        }

        private void SetDict()
        {
            dict = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader(PPath.FullName))
            {
                ProgressMessage = $"Dictionary 생성 중...";

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string PID = line.Substring(Start, Count);
                    dict[PID] = line;
                }
            }
        }
        private void ConvertE()
        {
            using (StreamReader sr = new StreamReader(EPath.FullName))
            using (StreamWriter sw = new StreamWriter(OutEPath.FullName))
            using (StreamWriter swErr = new StreamWriter(OutEErrPath.FullName))
            {
                long cnt = 0;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string PID = line.Substring(Start, Count);

                    if (!dict.ContainsKey(PID))
                    {
                        errCnt++;
                        swErr.WriteLine(line);
                        continue;
                    }

                    string pLine = dict[PID];
                    string newLine = pLine + ";" +line;
                    sw.WriteLine(newLine);

                    cnt++;
                    ProgressMessage = $"Convert-E: {cnt} lines completed";
                }
            }
        }
        private void ConvertT()
        {
            using (StreamReader sr = new StreamReader(TPath.FullName))
            using (StreamWriter sw = new StreamWriter(OutTPath.FullName))
            using (StreamWriter swErr = new StreamWriter(OutTErrPath.FullName))
            {
                long cnt = 0;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string PID = line.Substring(Start, Count);

                    if (!dict.ContainsKey(PID))
                    {
                        errCnt++;
                        swErr.WriteLine(line);
                        continue;
                    }

                    string pLine = dict[PID];
                    string newLine = pLine + ";" + line;
                    sw.WriteLine(newLine);

                    cnt++;
                    ProgressMessage = $"Convert-T: {cnt} lines completed";
                }
            }
        }
        private void ConvertV()
        {
            using (StreamReader sr = new StreamReader(VPath.FullName))
            using (StreamWriter sw = new StreamWriter(OutVPath.FullName))
            using (StreamWriter swErr = new StreamWriter(OutVErrPath.FullName))
            {
                long cnt = 0;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string PID = line.Substring(Start, Count);

                    if (!dict.ContainsKey(PID))
                    {
                        errCnt++;
                        swErr.WriteLine(line);
                        continue;
                    }

                    string pLine = dict[PID];

                    string newLine = pLine + ";" + line;
                    sw.WriteLine(newLine);

                    cnt++;
                    ProgressMessage = $"Convert-V: {cnt} lines completed";
                }
            }
        }
        private void ConvertP()
        {
            using (StreamReader sr = new StreamReader(PPath.FullName))
            using (StreamWriter sw = new StreamWriter(OutPPath.FullName))
            using (StreamWriter swErr = new StreamWriter(OutPErrPath.FullName))
            {
                long cnt = 0;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    sw.WriteLine(line);

                    cnt++;
                    ProgressMessage = $"Convert-P: {cnt} lines completed";
                }
            }
        }
    }

}
