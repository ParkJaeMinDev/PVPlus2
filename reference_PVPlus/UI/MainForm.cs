
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;
using PVPlus.UI.TextModifier;

namespace PVPlus.UI
{
    public partial class MainForm : Form
    {
        public MainPVForm mainPVForm;
        public SamplePVForm samplePVForm;
        public LTFHelperForm2 ltfHelperForm2;      
        public TabPage AddFuncTab;
        public Size InitSize;

        //Extra
        public SamsungTextModifierForm samsungCvtForm;
        public HanwhaTextModifier hanwhaCvtForm;

        public MainForm()
        {
            InitializeComponent();

            // 폼의 최대화 버튼 활성화
            this.MaximizeBox = true;
        }

        public void AddForm(TabPage tp, Form f)
        {
            tp.Controls.Clear();  // 기존 컨트롤을 모두 제거

            if (f == null)
            {
                Refresh();
                return;
            }

            f.TopLevel = false;
            f.FormBorderStyle = FormBorderStyle.None;
            f.AutoScaleMode = AutoScaleMode.Dpi;

            tp.Controls.Add(f);
            f.Dock = DockStyle.Fill;
            f.Show();
            Refresh();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.TabPages[0].Text = "MainPV";
            tabControl1.TabPages[1].Text = "Sample";
            tabControl1.TabPages[2].Text = "LTFHelper";
            tabControl1.TabPages[3].Text = "추가기능";

            mainPVForm = new MainPVForm(this);
            samplePVForm = new SamplePVForm(mainPVForm);
            ltfHelperForm2 = new LTFHelperForm2();

            AddForm(tabControl1.SelectedTab, mainPVForm);
            AddFuncTab = tabControl1.TabPages[3];

            mainPVForm.LoadConfigureData();
            samplePVForm.LoadConfigureData();

            InitSize = new Size(Size.Width, Size.Height);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mainPVForm.SaveConfigureData();
            samplePVForm.SaveConfigureData();
            Properties.Settings.Default.Save();

            mainPVForm.Close();
            samplePVForm.Close();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Text == "LTFHelper")
            {
                this.Size = new Size(InitSize.Width, InitSize.Height + 95);
            }
            else
            {
                this.Size = InitSize;
            }

            if (tabControl1.SelectedTab.Text == "MainPV") AddForm(tabControl1.SelectedTab, mainPVForm);
            if (tabControl1.SelectedTab.Text == "Sample") AddForm(tabControl1.SelectedTab, samplePVForm);
            if (tabControl1.SelectedTab.Text == "LTFHelper") AddForm(tabControl1.SelectedTab, ltfHelperForm2);

        }

        public void SetVersionText(string text)
        {
            Text = text;
        }

        public void HideForm(bool status)
        {
            if(status)
            {
                tabControl1.Hide();              
            }
            else
            {
                tabControl1.Show();
            }         
        }
    }
}
