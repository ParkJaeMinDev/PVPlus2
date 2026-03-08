namespace PVPlus.UI.LTFHelper
{
    partial class LTFHelperGroupOpt_Loop
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewLoop = new System.Windows.Forms.DataGridView();
            this.buttonExistCheck = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLoop)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridViewLoop.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLoop.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dataGridViewLoop.Location = new System.Drawing.Point(0, 50);
            this.dataGridViewLoop.Name = "dataGridView1";
            this.dataGridViewLoop.RowTemplate.Height = 23;
            this.dataGridViewLoop.Size = new System.Drawing.Size(550, 413);
            this.dataGridViewLoop.TabIndex = 0;
            this.dataGridViewLoop.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyDown);
            // 
            // buttonExistCheck
            // 
            this.buttonExistCheck.Location = new System.Drawing.Point(12, 12);
            this.buttonExistCheck.Name = "buttonExistCheck";
            this.buttonExistCheck.Size = new System.Drawing.Size(97, 32);
            this.buttonExistCheck.TabIndex = 1;
            this.buttonExistCheck.Text = "Exist?";
            this.buttonExistCheck.UseVisualStyleBackColor = true;
            // 
            // LTFHelperMultOpt_Loop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 463);
            this.Controls.Add(this.buttonExistCheck);
            this.Controls.Add(this.dataGridViewLoop);
            this.Name = "LTFHelperMultOpt_Loop";
            this.Text = "LTFHelperMultForm_Loop";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LTFHelperMultOpt_Loop_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLoop)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.DataGridView dataGridViewLoop;
        public System.Windows.Forms.Button buttonExistCheck;
    }
}