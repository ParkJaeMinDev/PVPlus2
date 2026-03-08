using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVPlus.UI.LTFHelper
{
    public partial class LTFHelperGroupOpt : Form
    {
        public LTFHelperGroupOpt()
        {
            InitializeComponent();
            LoopForm = new LTFHelperGroupOpt_Loop();
        }

        public LTFHelperGroupOpt_Loop LoopForm;

        private void LTFHelperMultOpt_Load(object sender, EventArgs e)
        {
            textBoxGroupName.Size = new Size(420,42);
            textBoxGroupName.Multiline = true;
            textBoxGroupName.WordWrap = true;

            textBoxWriteLine.Size = new Size(420, 42);
            textBoxWriteLine.Multiline = true;
            textBoxWriteLine.WordWrap = true;

            LoopForm.dataGridViewLoop.AllowUserToAddRows = false;
            LoopForm.dataGridViewLoop.AllowUserToResizeRows = false;
            LoopForm.dataGridViewLoop.RowHeadersVisible = true;
            LoopForm.dataGridViewLoop.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;


            for (int i = 0; i< 30; i++)
            {
                LoopForm.dataGridViewLoop.Columns.Add(i.ToString(), $"No. {i}");
                LoopForm.dataGridViewLoop.Columns[i].Width = 120;
                LoopForm.dataGridViewLoop.Columns[i].Visible = false;
                LoopForm.dataGridViewLoop.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            for(int j = 0; j < 1000; j++)
            {
                LoopForm.dataGridViewLoop.Rows.Add(new DataGridViewRow());
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
