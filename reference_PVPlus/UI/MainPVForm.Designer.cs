namespace PVPlus
{
    partial class MainPVForm
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
            this.textBoxPPath = new System.Windows.Forms.TextBox();
            this.textBoxVPath = new System.Windows.Forms.TextBox();
            this.textBoxSPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxExcelPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxLineSummary = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxSample = new System.Windows.Forms.CheckBox();
            this.comboBoxCompany = new System.Windows.Forms.ComboBox();
            this.TextBoxDelimiter = new System.Windows.Forms.TextBox();
            this.checkBox구분자 = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxLimitCheck = new System.Windows.Forms.CheckBox();
            this.radioButtonS = new System.Windows.Forms.RadioButton();
            this.radioButtonV = new System.Windows.Forms.RadioButton();
            this.radioButtonP = new System.Windows.Forms.RadioButton();
            this.ExcessLimitChkBtn = new System.Windows.Forms.Button();
            this.textBoxProduct = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btn시작 = new System.Windows.Forms.Button();
            this.btnOpenExcel = new System.Windows.Forms.Button();
            this.btnOpenPTable = new System.Windows.Forms.Button();
            this.btnOpenVTable = new System.Windows.Forms.Button();
            this.btnOpenSTable = new System.Windows.Forms.Button();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.PVBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.button취소 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lblTime = new System.Windows.Forms.Label();
            this.btnEvaluateSInfos = new System.Windows.Forms.Button();
            this.labelFileOpen = new System.Windows.Forms.Label();
            this.comboBoxFileOpen = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxPPath
            // 
            this.textBoxPPath.Location = new System.Drawing.Point(180, 85);
            this.textBoxPPath.Name = "textBoxPPath";
            this.textBoxPPath.Size = new System.Drawing.Size(481, 21);
            this.textBoxPPath.TabIndex = 0;
            this.textBoxPPath.TextChanged += new System.EventHandler(this.textBoxPPath_TextChanged);
            // 
            // textBoxVPath
            // 
            this.textBoxVPath.Location = new System.Drawing.Point(180, 113);
            this.textBoxVPath.Name = "textBoxVPath";
            this.textBoxVPath.Size = new System.Drawing.Size(481, 21);
            this.textBoxVPath.TabIndex = 1;
            this.textBoxVPath.TextChanged += new System.EventHandler(this.textBoxVPath_TextChanged);
            // 
            // textBoxSPath
            // 
            this.textBoxSPath.Location = new System.Drawing.Point(180, 141);
            this.textBoxSPath.Name = "textBoxSPath";
            this.textBoxSPath.Size = new System.Drawing.Size(481, 21);
            this.textBoxSPath.TabIndex = 2;
            this.textBoxSPath.TextChanged += new System.EventHandler(this.textBoxSPath_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(54, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(13, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "P";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(54, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "V";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(54, 144);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "표준해약공제액";
            // 
            // textBoxExcelPath
            // 
            this.textBoxExcelPath.Location = new System.Drawing.Point(180, 36);
            this.textBoxExcelPath.Name = "textBoxExcelPath";
            this.textBoxExcelPath.Size = new System.Drawing.Size(481, 21);
            this.textBoxExcelPath.TabIndex = 6;
            this.textBoxExcelPath.TextChanged += new System.EventHandler(this.textBoxExcelPath_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(54, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "Excel파일경로";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxLineSummary);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.checkBoxSample);
            this.groupBox1.Controls.Add(this.comboBoxCompany);
            this.groupBox1.Controls.Add(this.TextBoxDelimiter);
            this.groupBox1.Controls.Add(this.checkBox구분자);
            this.groupBox1.Location = new System.Drawing.Point(284, 192);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(187, 135);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "설정";
            // 
            // checkBoxLineSummary
            // 
            this.checkBoxLineSummary.AutoSize = true;
            this.checkBoxLineSummary.Location = new System.Drawing.Point(93, 90);
            this.checkBoxLineSummary.Name = "checkBoxLineSummary";
            this.checkBoxLineSummary.Size = new System.Drawing.Size(76, 16);
            this.checkBoxLineSummary.TabIndex = 13;
            this.checkBoxLineSummary.Text = "라인 요약";
            this.checkBoxLineSummary.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "회사명";
            // 
            // checkBoxSample
            // 
            this.checkBoxSample.AutoSize = true;
            this.checkBoxSample.Location = new System.Drawing.Point(11, 90);
            this.checkBoxSample.Name = "checkBoxSample";
            this.checkBoxSample.Size = new System.Drawing.Size(72, 16);
            this.checkBoxSample.TabIndex = 12;
            this.checkBoxSample.Text = "간이검증";
            this.checkBoxSample.UseVisualStyleBackColor = true;
            // 
            // comboBoxCompany
            // 
            this.comboBoxCompany.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCompany.FormattingEnabled = true;
            this.comboBoxCompany.Location = new System.Drawing.Point(59, 29);
            this.comboBoxCompany.Name = "comboBoxCompany";
            this.comboBoxCompany.Size = new System.Drawing.Size(93, 20);
            this.comboBoxCompany.TabIndex = 10;
            this.comboBoxCompany.SelectedIndexChanged += new System.EventHandler(this.comboBoxCompany_SelectedIndexChanged);
            // 
            // TextBoxDelimiter
            // 
            this.TextBoxDelimiter.Location = new System.Drawing.Point(77, 58);
            this.TextBoxDelimiter.Name = "TextBoxDelimiter";
            this.TextBoxDelimiter.ReadOnly = true;
            this.TextBoxDelimiter.Size = new System.Drawing.Size(37, 21);
            this.TextBoxDelimiter.TabIndex = 9;
            // 
            // checkBox구분자
            // 
            this.checkBox구분자.AutoSize = true;
            this.checkBox구분자.Location = new System.Drawing.Point(11, 60);
            this.checkBox구분자.Name = "checkBox구분자";
            this.checkBox구분자.Size = new System.Drawing.Size(60, 16);
            this.checkBox구분자.TabIndex = 9;
            this.checkBox구분자.Text = "구분자";
            this.checkBox구분자.UseVisualStyleBackColor = true;
            this.checkBox구분자.CheckedChanged += new System.EventHandler(this.checkBox구분자_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxLimitCheck);
            this.groupBox2.Controls.Add(this.radioButtonS);
            this.groupBox2.Controls.Add(this.radioButtonV);
            this.groupBox2.Controls.Add(this.radioButtonP);
            this.groupBox2.Location = new System.Drawing.Point(490, 192);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(132, 135);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "테이블유형";
            // 
            // checkBoxLimitCheck
            // 
            this.checkBoxLimitCheck.AutoSize = true;
            this.checkBoxLimitCheck.Location = new System.Drawing.Point(15, 95);
            this.checkBoxLimitCheck.Name = "checkBoxLimitCheck";
            this.checkBoxLimitCheck.Size = new System.Drawing.Size(72, 16);
            this.checkBoxLimitCheck.TabIndex = 3;
            this.checkBoxLimitCheck.Text = "한도체크";
            this.checkBoxLimitCheck.UseVisualStyleBackColor = true;
            this.checkBoxLimitCheck.Visible = false;
            // 
            // radioButtonS
            // 
            this.radioButtonS.AutoSize = true;
            this.radioButtonS.Location = new System.Drawing.Point(15, 73);
            this.radioButtonS.Name = "radioButtonS";
            this.radioButtonS.Size = new System.Drawing.Size(107, 16);
            this.radioButtonS.TabIndex = 2;
            this.radioButtonS.TabStop = true;
            this.radioButtonS.Text = "표준해약공제액";
            this.radioButtonS.UseVisualStyleBackColor = true;
            this.radioButtonS.CheckedChanged += new System.EventHandler(this.radioButtonS_CheckedChanged);
            // 
            // radioButtonV
            // 
            this.radioButtonV.AutoSize = true;
            this.radioButtonV.Location = new System.Drawing.Point(15, 51);
            this.radioButtonV.Name = "radioButtonV";
            this.radioButtonV.Size = new System.Drawing.Size(31, 16);
            this.radioButtonV.TabIndex = 1;
            this.radioButtonV.TabStop = true;
            this.radioButtonV.Text = "V";
            this.radioButtonV.UseVisualStyleBackColor = true;
            // 
            // radioButtonP
            // 
            this.radioButtonP.AutoSize = true;
            this.radioButtonP.Location = new System.Drawing.Point(15, 29);
            this.radioButtonP.Name = "radioButtonP";
            this.radioButtonP.Size = new System.Drawing.Size(31, 16);
            this.radioButtonP.TabIndex = 0;
            this.radioButtonP.TabStop = true;
            this.radioButtonP.Text = "P";
            this.radioButtonP.UseVisualStyleBackColor = true;
            this.radioButtonP.CheckedChanged += new System.EventHandler(this.radioButtonP_CheckedChanged);
            // 
            // ExcessLimitChkBtn
            // 
            this.ExcessLimitChkBtn.BackColor = System.Drawing.SystemColors.Info;
            this.ExcessLimitChkBtn.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExcessLimitChkBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ExcessLimitChkBtn.Location = new System.Drawing.Point(16, 55);
            this.ExcessLimitChkBtn.Name = "ExcessLimitChkBtn";
            this.ExcessLimitChkBtn.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ExcessLimitChkBtn.Size = new System.Drawing.Size(70, 30);
            this.ExcessLimitChkBtn.TabIndex = 4;
            this.ExcessLimitChkBtn.Text = "신계약비";
            this.ExcessLimitChkBtn.UseVisualStyleBackColor = false;
            this.ExcessLimitChkBtn.Click += new System.EventHandler(this.ExcessLimitChkBtn_Click);
            // 
            // textBoxProduct
            // 
            this.textBoxProduct.Location = new System.Drawing.Point(56, 213);
            this.textBoxProduct.Name = "textBoxProduct";
            this.textBoxProduct.Size = new System.Drawing.Size(197, 21);
            this.textBoxProduct.TabIndex = 10;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.SystemColors.Menu;
            this.label7.Location = new System.Drawing.Point(54, 192);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 12;
            this.label7.Text = "상품코드";
            // 
            // btn시작
            // 
            this.btn시작.BackColor = System.Drawing.Color.LightYellow;
            this.btn시작.Location = new System.Drawing.Point(54, 242);
            this.btn시작.Name = "btn시작";
            this.btn시작.Size = new System.Drawing.Size(96, 47);
            this.btn시작.TabIndex = 13;
            this.btn시작.Text = "시작";
            this.btn시작.UseVisualStyleBackColor = false;
            this.btn시작.Click += new System.EventHandler(this.btn시작_Click);
            // 
            // btnOpenExcel
            // 
            this.btnOpenExcel.Location = new System.Drawing.Point(667, 36);
            this.btnOpenExcel.Margin = new System.Windows.Forms.Padding(1);
            this.btnOpenExcel.Name = "btnOpenExcel";
            this.btnOpenExcel.Size = new System.Drawing.Size(70, 23);
            this.btnOpenExcel.TabIndex = 14;
            this.btnOpenExcel.Text = "열기";
            this.btnOpenExcel.UseVisualStyleBackColor = true;
            this.btnOpenExcel.Click += new System.EventHandler(this.btnOpenExcelPath_Click);
            // 
            // btnOpenPTable
            // 
            this.btnOpenPTable.Location = new System.Drawing.Point(667, 84);
            this.btnOpenPTable.Margin = new System.Windows.Forms.Padding(1);
            this.btnOpenPTable.Name = "btnOpenPTable";
            this.btnOpenPTable.Size = new System.Drawing.Size(70, 23);
            this.btnOpenPTable.TabIndex = 15;
            this.btnOpenPTable.Text = "열기";
            this.btnOpenPTable.UseVisualStyleBackColor = true;
            this.btnOpenPTable.Click += new System.EventHandler(this.btnOpenPTable_Click);
            // 
            // btnOpenVTable
            // 
            this.btnOpenVTable.Location = new System.Drawing.Point(667, 112);
            this.btnOpenVTable.Margin = new System.Windows.Forms.Padding(1);
            this.btnOpenVTable.Name = "btnOpenVTable";
            this.btnOpenVTable.Size = new System.Drawing.Size(70, 23);
            this.btnOpenVTable.TabIndex = 16;
            this.btnOpenVTable.Text = "열기";
            this.btnOpenVTable.UseVisualStyleBackColor = true;
            this.btnOpenVTable.Click += new System.EventHandler(this.btnOpenVTable_Click);
            // 
            // btnOpenSTable
            // 
            this.btnOpenSTable.Location = new System.Drawing.Point(667, 140);
            this.btnOpenSTable.Margin = new System.Windows.Forms.Padding(1);
            this.btnOpenSTable.Name = "btnOpenSTable";
            this.btnOpenSTable.Size = new System.Drawing.Size(70, 23);
            this.btnOpenSTable.TabIndex = 17;
            this.btnOpenSTable.Text = "열기";
            this.btnOpenSTable.UseVisualStyleBackColor = true;
            this.btnOpenSTable.Click += new System.EventHandler(this.btnOpenSTable_Click);
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxStatus.Location = new System.Drawing.Point(56, 371);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxStatus.Size = new System.Drawing.Size(685, 89);
            this.textBoxStatus.TabIndex = 18;
            this.textBoxStatus.TextChanged += new System.EventHandler(this.textBoxStatus_TextChanged);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(55, 352);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(41, 12);
            this.lblStatus.TabIndex = 19;
            this.lblStatus.Text = "상태창";
            // 
            // PVBackgroundWorker
            // 
            this.PVBackgroundWorker.WorkerSupportsCancellation = true;
            this.PVBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.PVBackgroundWorker_DoWork);
            this.PVBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.PVBackgroundWorker_RunWorkerCompleted);
            // 
            // button취소
            // 
            this.button취소.Enabled = false;
            this.button취소.Location = new System.Drawing.Point(156, 242);
            this.button취소.Name = "button취소";
            this.button취소.Size = new System.Drawing.Size(97, 47);
            this.button취소.TabIndex = 20;
            this.button취소.Text = "취소";
            this.button취소.UseVisualStyleBackColor = true;
            this.button취소.Click += new System.EventHandler(this.btn취소_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(653, 351);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(41, 12);
            this.lblTime.TabIndex = 21;
            this.lblTime.Text = "타이머";
            // 
            // btnEvaluateSInfos
            // 
            this.btnEvaluateSInfos.BackColor = System.Drawing.SystemColors.Info;
            this.btnEvaluateSInfos.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEvaluateSInfos.ForeColor = System.Drawing.Color.Black;
            this.btnEvaluateSInfos.Location = new System.Drawing.Point(16, 20);
            this.btnEvaluateSInfos.Name = "btnEvaluateSInfos";
            this.btnEvaluateSInfos.Size = new System.Drawing.Size(70, 30);
            this.btnEvaluateSInfos.TabIndex = 23;
            this.btnEvaluateSInfos.Text = "S비율";
            this.btnEvaluateSInfos.UseVisualStyleBackColor = false;
            this.btnEvaluateSInfos.Click += new System.EventHandler(this.btnS산출_Click);
            // 
            // labelFileOpen
            // 
            this.labelFileOpen.AutoSize = true;
            this.labelFileOpen.BackColor = System.Drawing.SystemColors.Menu;
            this.labelFileOpen.Location = new System.Drawing.Point(54, 305);
            this.labelFileOpen.Name = "labelFileOpen";
            this.labelFileOpen.Size = new System.Drawing.Size(29, 12);
            this.labelFileOpen.TabIndex = 25;
            this.labelFileOpen.Text = "이동";
            // 
            // comboBoxFileOpen
            // 
            this.comboBoxFileOpen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFileOpen.FormattingEnabled = true;
            this.comboBoxFileOpen.Location = new System.Drawing.Point(113, 302);
            this.comboBoxFileOpen.Name = "comboBoxFileOpen";
            this.comboBoxFileOpen.Size = new System.Drawing.Size(140, 20);
            this.comboBoxFileOpen.TabIndex = 12;
            this.comboBoxFileOpen.SelectedIndexChanged += new System.EventHandler(this.comboBoxFileOpen_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ExcessLimitChkBtn);
            this.groupBox3.Controls.Add(this.btnEvaluateSInfos);
            this.groupBox3.Location = new System.Drawing.Point(635, 192);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(102, 134);
            this.groupBox3.TabIndex = 26;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "한도계산";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 12);
            this.label6.TabIndex = 27;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(6, 9);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(77, 12);
            this.linkLabel1.TabIndex = 28;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "업데이트링크";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(89, 9);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(77, 12);
            this.linkLabel2.TabIndex = 29;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "패치내역보기";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // MainPVForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(794, 501);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.comboBoxFileOpen);
            this.Controls.Add(this.labelFileOpen);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.button취소);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.btnOpenSTable);
            this.Controls.Add(this.btnOpenVTable);
            this.Controls.Add(this.btnOpenPTable);
            this.Controls.Add(this.btnOpenExcel);
            this.Controls.Add(this.btn시작);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxProduct);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxExcelPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSPath);
            this.Controls.Add(this.textBoxVPath);
            this.Controls.Add(this.textBoxPPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainPVForm";
            this.Text = "손해보험PV검증";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainPVForm_FormClosing);
            this.Load += new System.EventHandler(this.MainPVForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxPPath;
        private System.Windows.Forms.TextBox textBoxVPath;
        private System.Windows.Forms.TextBox textBoxSPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxExcelPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox구분자;
        private System.Windows.Forms.TextBox TextBoxDelimiter;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonS;
        private System.Windows.Forms.RadioButton radioButtonV;
        private System.Windows.Forms.RadioButton radioButtonP;
        private System.Windows.Forms.ComboBox comboBoxCompany;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxProduct;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btn시작;
        private System.Windows.Forms.Button btnOpenExcel;
        private System.Windows.Forms.Button btnOpenPTable;
        private System.Windows.Forms.Button btnOpenVTable;
        private System.Windows.Forms.Button btnOpenSTable;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.ComponentModel.BackgroundWorker PVBackgroundWorker;
        private System.Windows.Forms.Button button취소;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Button btnEvaluateSInfos;
        private System.Windows.Forms.CheckBox checkBoxLimitCheck;
        private System.Windows.Forms.Label labelFileOpen;
        private System.Windows.Forms.ComboBox comboBoxFileOpen;
        private System.Windows.Forms.Button ExcessLimitChkBtn;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox checkBoxSample;
        private System.Windows.Forms.CheckBox checkBoxLineSummary;
        private System.Windows.Forms.LinkLabel linkLabel2;
    }
}