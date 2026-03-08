namespace PVPlus.UI.LTFHelper
{
    partial class LTFHelperGroupOpt
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
            this.textBoxGroupName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxWriteLine = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxGroupName
            // 
            this.textBoxGroupName.Location = new System.Drawing.Point(95, 28);
            this.textBoxGroupName.Name = "textBoxGroupName";
            this.textBoxGroupName.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxGroupName.Size = new System.Drawing.Size(450, 21);
            this.textBoxGroupName.TabIndex = 1;
            this.textBoxGroupName.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "SplitBy";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "WriteLine";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // textBoxWriteLine
            // 
            this.textBoxWriteLine.Location = new System.Drawing.Point(95, 80);
            this.textBoxWriteLine.Name = "textBoxWriteLine";
            this.textBoxWriteLine.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxWriteLine.Size = new System.Drawing.Size(450, 21);
            this.textBoxWriteLine.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(551, 80);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 43);
            this.button1.TabIndex = 5;
            this.button1.Text = "LoopBy";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // LTFHelperMultOpt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 135);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBoxWriteLine);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxGroupName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LTFHelperMultOpt";
            this.Text = "LTFHelperMult";
            this.Load += new System.EventHandler(this.LTFHelperMultOpt_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textBoxGroupName;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBoxWriteLine;
        public System.Windows.Forms.Button button1;
    }
}