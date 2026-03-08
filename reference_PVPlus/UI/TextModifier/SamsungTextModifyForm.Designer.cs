namespace PVPlus
{
    partial class SamsungTextModifierForm
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
            this.components = new System.ComponentModel.Container();
            this.labelSamP = new System.Windows.Forms.Label();
            this.labelSamV = new System.Windows.Forms.Label();
            this.labelSamT = new System.Windows.Forms.Label();
            this.labelSamE = new System.Windows.Forms.Label();
            this.textBoxPPath = new System.Windows.Forms.TextBox();
            this.textBoxVPath = new System.Windows.Forms.TextBox();
            this.textBoxTPath = new System.Windows.Forms.TextBox();
            this.textBoxEPath = new System.Windows.Forms.TextBox();
            this.buttonOpenPPath = new System.Windows.Forms.Button();
            this.buttonOpenVPath = new System.Windows.Forms.Button();
            this.buttonOpenTPath = new System.Windows.Forms.Button();
            this.buttonOpenEPath = new System.Windows.Forms.Button();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.labelProgress = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelSamP
            // 
            this.labelSamP.AutoSize = true;
            this.labelSamP.Location = new System.Drawing.Point(23, 49);
            this.labelSamP.Name = "labelSamP";
            this.labelSamP.Size = new System.Drawing.Size(13, 12);
            this.labelSamP.TabIndex = 0;
            this.labelSamP.Text = "P";
            // 
            // labelSamV
            // 
            this.labelSamV.AutoSize = true;
            this.labelSamV.Location = new System.Drawing.Point(23, 78);
            this.labelSamV.Name = "labelSamV";
            this.labelSamV.Size = new System.Drawing.Size(13, 12);
            this.labelSamV.TabIndex = 1;
            this.labelSamV.Text = "V";
            // 
            // labelSamT
            // 
            this.labelSamT.AutoSize = true;
            this.labelSamT.Location = new System.Drawing.Point(23, 105);
            this.labelSamT.Name = "labelSamT";
            this.labelSamT.Size = new System.Drawing.Size(13, 12);
            this.labelSamT.TabIndex = 2;
            this.labelSamT.Text = "T";
            // 
            // labelSamE
            // 
            this.labelSamE.AutoSize = true;
            this.labelSamE.Location = new System.Drawing.Point(23, 132);
            this.labelSamE.Name = "labelSamE";
            this.labelSamE.Size = new System.Drawing.Size(13, 12);
            this.labelSamE.TabIndex = 3;
            this.labelSamE.Text = "E";
            // 
            // textBoxPPath
            // 
            this.textBoxPPath.Location = new System.Drawing.Point(42, 46);
            this.textBoxPPath.Name = "textBoxPPath";
            this.textBoxPPath.Size = new System.Drawing.Size(633, 21);
            this.textBoxPPath.TabIndex = 4;
            // 
            // textBoxVPath
            // 
            this.textBoxVPath.Location = new System.Drawing.Point(42, 75);
            this.textBoxVPath.Name = "textBoxVPath";
            this.textBoxVPath.Size = new System.Drawing.Size(633, 21);
            this.textBoxVPath.TabIndex = 5;
            // 
            // textBoxTPath
            // 
            this.textBoxTPath.Location = new System.Drawing.Point(42, 102);
            this.textBoxTPath.Name = "textBoxTPath";
            this.textBoxTPath.Size = new System.Drawing.Size(633, 21);
            this.textBoxTPath.TabIndex = 6;
            // 
            // textBoxEPath
            // 
            this.textBoxEPath.Location = new System.Drawing.Point(42, 129);
            this.textBoxEPath.Name = "textBoxEPath";
            this.textBoxEPath.Size = new System.Drawing.Size(633, 21);
            this.textBoxEPath.TabIndex = 7;
            // 
            // buttonOpenPPath
            // 
            this.buttonOpenPPath.Location = new System.Drawing.Point(681, 45);
            this.buttonOpenPPath.Name = "buttonOpenPPath";
            this.buttonOpenPPath.Size = new System.Drawing.Size(50, 23);
            this.buttonOpenPPath.TabIndex = 8;
            this.buttonOpenPPath.Text = "열기";
            this.buttonOpenPPath.UseVisualStyleBackColor = true;
            this.buttonOpenPPath.Click += new System.EventHandler(this.buttonOpenPPath_Click);
            // 
            // buttonOpenVPath
            // 
            this.buttonOpenVPath.Location = new System.Drawing.Point(681, 73);
            this.buttonOpenVPath.Name = "buttonOpenVPath";
            this.buttonOpenVPath.Size = new System.Drawing.Size(50, 23);
            this.buttonOpenVPath.TabIndex = 9;
            this.buttonOpenVPath.Text = "열기";
            this.buttonOpenVPath.UseVisualStyleBackColor = true;
            this.buttonOpenVPath.Click += new System.EventHandler(this.buttonOpenVPath_Click);
            // 
            // buttonOpenTPath
            // 
            this.buttonOpenTPath.Location = new System.Drawing.Point(681, 100);
            this.buttonOpenTPath.Name = "buttonOpenTPath";
            this.buttonOpenTPath.Size = new System.Drawing.Size(50, 23);
            this.buttonOpenTPath.TabIndex = 10;
            this.buttonOpenTPath.Text = "열기";
            this.buttonOpenTPath.UseVisualStyleBackColor = true;
            this.buttonOpenTPath.Click += new System.EventHandler(this.buttonOpenTPath_Click);
            // 
            // buttonOpenEPath
            // 
            this.buttonOpenEPath.Location = new System.Drawing.Point(681, 128);
            this.buttonOpenEPath.Name = "buttonOpenEPath";
            this.buttonOpenEPath.Size = new System.Drawing.Size(50, 23);
            this.buttonOpenEPath.TabIndex = 11;
            this.buttonOpenEPath.Text = "열기";
            this.buttonOpenEPath.UseVisualStyleBackColor = true;
            this.buttonOpenEPath.Click += new System.EventHandler(this.buttonOpenEPath_Click);
            // 
            // buttonConvert
            // 
            this.buttonConvert.Location = new System.Drawing.Point(644, 161);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(86, 29);
            this.buttonConvert.TabIndex = 12;
            this.buttonConvert.Text = "Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(41, 168);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(37, 12);
            this.labelProgress.TabIndex = 13;
            this.labelProgress.Text = "진행: ";
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 271);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(569, 12);
            this.label1.TabIndex = 16;
            this.label1.Text = "P테이블과 각 테이블의 (              ) 위치 값이 일치할때 (P테이블 line + ; + V테이블 line)을 생성합니다.";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(165, 266);
            this.textBox1.Name = "textBox1";
            this.textBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textBox1.Size = new System.Drawing.Size(49, 21);
            this.textBox1.TabIndex = 17;
            this.textBox1.Text = "0,12";
            // 
            // SamsungTextModifierForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(756, 347);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.buttonConvert);
            this.Controls.Add(this.buttonOpenEPath);
            this.Controls.Add(this.buttonOpenTPath);
            this.Controls.Add(this.buttonOpenVPath);
            this.Controls.Add(this.buttonOpenPPath);
            this.Controls.Add(this.textBoxEPath);
            this.Controls.Add(this.textBoxTPath);
            this.Controls.Add(this.textBoxVPath);
            this.Controls.Add(this.textBoxPPath);
            this.Controls.Add(this.labelSamE);
            this.Controls.Add(this.labelSamT);
            this.Controls.Add(this.labelSamV);
            this.Controls.Add(this.labelSamP);
            this.Name = "SamsungTextModifierForm";
            this.Text = "SamsungTextModifierForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SamsungTextModifierForm_FormClosing);
            this.Load += new System.EventHandler(this.SamsungTextModifierForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSamP;
        private System.Windows.Forms.Label labelSamV;
        private System.Windows.Forms.Label labelSamT;
        private System.Windows.Forms.Label labelSamE;
        private System.Windows.Forms.TextBox textBoxPPath;
        private System.Windows.Forms.TextBox textBoxVPath;
        private System.Windows.Forms.TextBox textBoxTPath;
        private System.Windows.Forms.TextBox textBoxEPath;
        private System.Windows.Forms.Button buttonOpenPPath;
        private System.Windows.Forms.Button buttonOpenVPath;
        private System.Windows.Forms.Button buttonOpenTPath;
        private System.Windows.Forms.Button buttonOpenEPath;
        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
    }
}