namespace PVPlus
{
    partial class SamplePVForm
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
            this.textBoxLine = new System.Windows.Forms.TextBox();
            this.lblSampleLine = new System.Windows.Forms.Label();
            this.textBoxSampleStatus = new System.Windows.Forms.TextBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnSampleInput = new System.Windows.Forms.Button();
            this.backgroundWorkerSample = new System.ComponentModel.BackgroundWorker();
            this.btnSampleOut = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxLine
            // 
            this.textBoxLine.Location = new System.Drawing.Point(70, 40);
            this.textBoxLine.Name = "textBoxLine";
            this.textBoxLine.Size = new System.Drawing.Size(625, 21);
            this.textBoxLine.TabIndex = 1;
            this.textBoxLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxLine_KeyDown);
            // 
            // lblSampleLine
            // 
            this.lblSampleLine.AutoSize = true;
            this.lblSampleLine.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSampleLine.Location = new System.Drawing.Point(27, 42);
            this.lblSampleLine.Name = "lblSampleLine";
            this.lblSampleLine.Size = new System.Drawing.Size(37, 13);
            this.lblSampleLine.TabIndex = 2;
            this.lblSampleLine.Text = "Line";
            // 
            // textBoxSampleStatus
            // 
            this.textBoxSampleStatus.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.textBoxSampleStatus.Location = new System.Drawing.Point(37, 108);
            this.textBoxSampleStatus.Multiline = true;
            this.textBoxSampleStatus.Name = "textBoxSampleStatus";
            this.textBoxSampleStatus.ReadOnly = true;
            this.textBoxSampleStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxSampleStatus.Size = new System.Drawing.Size(722, 114);
            this.textBoxSampleStatus.TabIndex = 3;
            this.textBoxSampleStatus.WordWrap = false;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(35, 89);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(53, 12);
            this.lblMessage.TabIndex = 4;
            this.lblMessage.Text = "확인결과";
            // 
            // btnSampleInput
            // 
            this.btnSampleInput.Location = new System.Drawing.Point(705, 40);
            this.btnSampleInput.Name = "btnSampleInput";
            this.btnSampleInput.Size = new System.Drawing.Size(54, 24);
            this.btnSampleInput.TabIndex = 7;
            this.btnSampleInput.Text = "입력";
            this.btnSampleInput.UseVisualStyleBackColor = true;
            this.btnSampleInput.Click += new System.EventHandler(this.btnSampleInput_Click);
            // 
            // backgroundWorkerSample
            // 
            this.backgroundWorkerSample.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerSample_DoWork);
            // 
            // btnSampleOut
            // 
            this.btnSampleOut.BackColor = System.Drawing.SystemColors.Info;
            this.btnSampleOut.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSampleOut.Location = new System.Drawing.Point(660, 76);
            this.btnSampleOut.Name = "btnSampleOut";
            this.btnSampleOut.Size = new System.Drawing.Size(96, 25);
            this.btnSampleOut.TabIndex = 8;
            this.btnSampleOut.Text = "샘플 상세내역";
            this.btnSampleOut.UseVisualStyleBackColor = false;
            this.btnSampleOut.Click += new System.EventHandler(this.btnSampleOut_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(37, 263);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 20;
            this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.Size = new System.Drawing.Size(719, 186);
            this.dataGridView1.TabIndex = 9;
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
            this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyDown);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(37, 237);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(719, 20);
            this.comboBox1.TabIndex = 10;
            // 
            // SamplePVForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 461);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnSampleOut);
            this.Controls.Add(this.btnSampleInput);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.textBoxSampleStatus);
            this.Controls.Add(this.lblSampleLine);
            this.Controls.Add(this.textBoxLine);
            this.MaximizeBox = false;
            this.Name = "SamplePVForm";
            this.Text = "Sample검증";
            this.Load += new System.EventHandler(this.SamplePVForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxLine;
        private System.Windows.Forms.Label lblSampleLine;
        private System.Windows.Forms.TextBox textBoxSampleStatus;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnSampleInput;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSample;
        private System.Windows.Forms.Button btnSampleOut;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}