namespace PVPlus.UI.TextModifier
{
    partial class HanwhaTextModifier
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
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PVPLUSExepnseTextBox = new System.Windows.Forms.TextBox();
            this.HanwhaExpenseTextBox = new System.Windows.Forms.TextBox();
            this.prdCodeTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.labelProgress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.HighlightText;
            this.button1.Location = new System.Drawing.Point(662, 7);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 30);
            this.button1.TabIndex = 4;
            this.button1.Text = "Convert";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "한화 사업비";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 265);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "PVPLUS 사업비";
            // 
            // PVPLUSExepnseTextBox
            // 
            this.PVPLUSExepnseTextBox.Location = new System.Drawing.Point(12, 289);
            this.PVPLUSExepnseTextBox.MaxLength = 1000000000;
            this.PVPLUSExepnseTextBox.Multiline = true;
            this.PVPLUSExepnseTextBox.Name = "PVPLUSExepnseTextBox";
            this.PVPLUSExepnseTextBox.ReadOnly = true;
            this.PVPLUSExepnseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.PVPLUSExepnseTextBox.Size = new System.Drawing.Size(750, 181);
            this.PVPLUSExepnseTextBox.TabIndex = 3;
            this.PVPLUSExepnseTextBox.WordWrap = false;
            // 
            // HanwhaExpenseTextBox
            // 
            this.HanwhaExpenseTextBox.Location = new System.Drawing.Point(12, 44);
            this.HanwhaExpenseTextBox.MaxLength = 1000000000;
            this.HanwhaExpenseTextBox.Multiline = true;
            this.HanwhaExpenseTextBox.Name = "HanwhaExpenseTextBox";
            this.HanwhaExpenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.HanwhaExpenseTextBox.Size = new System.Drawing.Size(750, 189);
            this.HanwhaExpenseTextBox.TabIndex = 2;
            this.HanwhaExpenseTextBox.WordWrap = false;
            // 
            // prdCodeTextBox
            // 
            this.prdCodeTextBox.Location = new System.Drawing.Point(540, 12);
            this.prdCodeTextBox.Name = "prdCodeTextBox";
            this.prdCodeTextBox.Size = new System.Drawing.Size(116, 21);
            this.prdCodeTextBox.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(481, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "상품코드";
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(219, 15);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(64, 12);
            this.labelProgress.TabIndex = 14;
            this.labelProgress.Text = "Progress: ";
            // 
            // HanwhaTextModifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 529);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.prdCodeTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.PVPLUSExepnseTextBox);
            this.Controls.Add(this.HanwhaExpenseTextBox);
            this.Name = "HanwhaTextModifier";
            this.Text = "HanwhaTextModifier";
            this.Load += new System.EventHandler(this.HanwhaTextModifier_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox PVPLUSExepnseTextBox;
        private System.Windows.Forms.TextBox HanwhaExpenseTextBox;
        private System.Windows.Forms.TextBox prdCodeTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelProgress;
    }
}