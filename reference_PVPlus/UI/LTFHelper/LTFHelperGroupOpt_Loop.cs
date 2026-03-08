using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVPlus.UI.LTFHelper
{
    public partial class LTFHelperGroupOpt_Loop : Form
    {
        public LTFHelperGroupOpt_Loop()
        {
            InitializeComponent();
        }

        public void PasteClipboard(DataGridView myDataGridView)
        {
            DataObject o = (DataObject)Clipboard.GetDataObject();

            try
            {
                if (o.GetDataPresent(DataFormats.Text))
                {
                    int curRowIdx = myDataGridView.CurrentCell.RowIndex;
                    int curColIdx = myDataGridView.CurrentCell.ColumnIndex;


                    string[] pastedRows = Regex.Split(o.GetText().TrimEnd("\r\n".ToCharArray()), "\r\n");

                    for (int i = 0; i < pastedRows.Length; i++)
                    {
                        if (myDataGridView.Rows.Count <= i + curRowIdx)
                        {
                            myDataGridView.Rows.Add(new DataGridViewRow());
                        }
                    }

                    for (int i = 0; i < pastedRows.Length; i++)
                    {
                        string[] pastedRowCells = pastedRows[i].Split(new char[] { '\t' });

                        for (int j = 0; j < pastedRowCells.Length; j++)
                        {
                            if (myDataGridView.Columns.Count > j + curColIdx)
                            {
                                myDataGridView.Rows[i + curRowIdx].Cells[j + curColIdx].Value = pastedRowCells[j];
                            }
                        }
                    }

                   // o.SetData(null);

                }
            }
            catch
            {
                //o.SetData(null);
            }

            
        }

        private void LTFHelperMultOpt_Loop_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Control && e.KeyCode == Keys.V)
            {
                PasteClipboard(dataGridViewLoop);
            }
            if (e.KeyCode == Keys.Delete)
            {
                foreach(DataGridViewCell s in dataGridViewLoop.SelectedCells)
                {
                    s.Value = null;
                }
            }
        }
    }
}
