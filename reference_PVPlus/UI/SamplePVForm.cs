using Flee.PublicTypes;
using PVPlus.PVCALCULATOR;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace PVPlus
{
    public partial class SamplePVForm : Form
    {
        public SamplePVForm(Form callingForm)
        {
            InitializeComponent();
            mainForm = callingForm as MainPVForm;
        }

        public void ReportExeption(Exception ex)
        {
            this.Invoke(new MethodInvoker(
               delegate ()
               {
                   textBoxSampleStatus.AppendText(ex.Message + '\n');
               }));
        }

        public void ReportMessage(string s)
        {
            this.Invoke(new MethodInvoker(
               delegate ()
               {
                   textBoxSampleStatus.AppendText(s + "\r\n");
               }));
        }

        public void ClearMessage()
        {
            this.Invoke(new MethodInvoker(
               delegate ()
               {
                   textBoxSampleStatus.Text = "";
               }));
        }

        public void LoadConfigureData()
        {
            textBoxLine.Text = Properties.Settings.Default.SampleLine;
        }

        public void SaveConfigureData()
        {
            Properties.Settings.Default.SampleLine = textBoxLine.Text;
        }

        private MainPVForm mainForm;

        AutoCompleteStringCollection auto;

        private void SamplePVForm_Load(object sender, EventArgs e)
        {
            auto = new AutoCompleteStringCollection();
            dataGridView1.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView1_EditingControlShowing);

            dataGridView1.Columns.Add("col1", "산출수식");
            dataGridView1.Columns.Add("col2", "값");
            dataGridView1.Columns.Add("col2", "기수표");

            dataGridView1.RowHeadersVisible = false;

            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.Columns[2].ReadOnly = true;

            dataGridView1.Columns[0].Width = 390;
            dataGridView1.Columns[1].Width = 146;
            dataGridView1.Columns[2].Width = 180;
        }

        private void btnSampleInput_Click(object sender, EventArgs e)
        {
            try
            {
                mainForm.SetConfigure();
                backgroundWorkerSample.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ReportExeption(ex);
            }
        }

        private void btnSampleOut_Click(object sender, EventArgs e)
        {
            try
            {
                mainForm.SetConfigure();

                Process.Start($@"{Configure.WorkingDI.FullName}\Sample.txt");
            }
            catch (Exception ex)
            {
                ReportExeption(ex);
            }
        }

        private void backgroundWorkerSample_DoWork(object sender, DoWorkEventArgs e)
        {
            new PV(mainForm).TestSample(textBoxLine.Text, this);

            this.Invoke(new Action(() =>
            {
                try
                {
                    string[] pvKeys = PVCalculator.Cals.Select(x => x.Key).Reverse().ToArray();
                    var cal = PVCalculator.Cals.LastOrDefault().Value;

                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(pvKeys);
                    comboBox1.SelectedItem = comboBox1.Items[0];

                    auto.Clear();
                    auto.AddRange(cal.variables.Keys.ToArray());
                    auto.AddRange(new string[] { "Eval(\"_\",n,m,t,freq)", "Eval(\"_\")" });
                    auto.AddRange(PV.finder.chkExprsByCheckItem.Select(x => x.Key).Select(y => $"Eval(\"{y}\",n,m,t,freq)").ToArray());
                    auto.AddRange(typeof(Expense).GetProperties().Where(x => x.PropertyType == typeof(double)).Select(x => $"Ex(\"{x.Name}\")").ToArray());
                }
                catch
                {

                }
            }));
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int row = dataGridView1.CurrentCell.RowIndex;
            int column = dataGridView1.CurrentCell.ColumnIndex;
            if (column != 0) return;

            try
            {
                string pvKey = comboBox1.SelectedItem.ToString();
                string item = dataGridView1.CurrentCell.Value?.ToString();

                if (string.IsNullOrWhiteSpace(item)) return;

                var cal = PVCalculator.Cals[pvKey];
                cal.LocalVariables.Keys.ToList().ForEach(x => helper.variables[x] = cal.LocalVariables[x]);
                helper.cal = cal;

                IDynamicExpression expr = PV.reader.Context.CompileDynamic(item);

                dataGridView1[column + 1, row].Value = expr.Evaluate();
                dataGridView1[column + 2, row].Value = pvKey;
            }
            catch (Exception ex)
            {
                dataGridView1[column + 1, row].Value = ex.Message;
                dataGridView1[column + 2, row].Value = "";
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                if (dataGridView1.CurrentRow.Index < dataGridView1.Rows.Count - 1)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
                }
            }
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            TextBox tb = e.Control as TextBox;

            if (tb != null)
            {
                tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                tb.AutoCompleteCustomSource = auto;
                tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            }
        }

        private void textBoxLine_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Control &&  e.KeyCode == Keys.A)
            {
                textBoxLine.SelectAll();
            }
        }
    }
}
