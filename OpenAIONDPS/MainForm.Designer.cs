namespace OpenAIONDPS
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.FileButton = new System.Windows.Forms.Button();
            this.SkillListFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.MemberGroupBox = new System.Windows.Forms.GroupBox();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.LimitedTargetNameComboBox = new System.Windows.Forms.ComboBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.LimitedTargetCheckBox = new System.Windows.Forms.CheckBox();
            this.FavoriteMemberList = new OpenAIONDPS.FavoriteMemberListUnit();
            this.FavoriteMemberButton = new System.Windows.Forms.Button();
            this.DisableJobRadioButton = new System.Windows.Forms.RadioButton();
            this.EnableJobRadioButton = new System.Windows.Forms.RadioButton();
            this.TotalDamageLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.Member01 = new OpenAIONDPS.MemberUnit();
            this.Member02 = new OpenAIONDPS.MemberUnit();
            this.Member03 = new OpenAIONDPS.MemberUnit();
            this.Member04 = new OpenAIONDPS.MemberUnit();
            this.Member05 = new OpenAIONDPS.MemberUnit();
            this.Member06 = new OpenAIONDPS.MemberUnit();
            this.Member07 = new OpenAIONDPS.MemberUnit();
            this.Member08 = new OpenAIONDPS.MemberUnit();
            this.Member09 = new OpenAIONDPS.MemberUnit();
            this.Member10 = new OpenAIONDPS.MemberUnit();
            this.Member11 = new OpenAIONDPS.MemberUnit();
            this.Member12 = new OpenAIONDPS.MemberUnit();
            this.AION64bitButton = new System.Windows.Forms.Button();
            this.DebugCheckBox = new System.Windows.Forms.CheckBox();
            this.CalcFromLogButton = new System.Windows.Forms.Button();
            this.AlwaysOnTopCheckBox = new System.Windows.Forms.CheckBox();
            this.CalcTimeCheckBox = new System.Windows.Forms.CheckBox();
            this.CalcTimerMinutesNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.CalcRemainingTimeLabel = new System.Windows.Forms.Label();
            this.MemberGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CalcTimerMinutesNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(93, 12);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(75, 23);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "開始";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(174, 12);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 23);
            this.StopButton.TabIndex = 1;
            this.StopButton.Text = "停止";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // FileButton
            // 
            this.FileButton.Location = new System.Drawing.Point(12, 12);
            this.FileButton.Name = "FileButton";
            this.FileButton.Size = new System.Drawing.Size(75, 23);
            this.FileButton.TabIndex = 2;
            this.FileButton.Text = "ログファイル";
            this.FileButton.UseVisualStyleBackColor = true;
            this.FileButton.Click += new System.EventHandler(this.FileButton_Click);
            // 
            // SkillListFlowLayoutPanel
            // 
            this.SkillListFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SkillListFlowLayoutPanel.AutoScroll = true;
            this.SkillListFlowLayoutPanel.Location = new System.Drawing.Point(12, 573);
            this.SkillListFlowLayoutPanel.Name = "SkillListFlowLayoutPanel";
            this.SkillListFlowLayoutPanel.Size = new System.Drawing.Size(890, 236);
            this.SkillListFlowLayoutPanel.TabIndex = 15;
            // 
            // MemberGroupBox
            // 
            this.MemberGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MemberGroupBox.Controls.Add(this.label25);
            this.MemberGroupBox.Controls.Add(this.label24);
            this.MemberGroupBox.Controls.Add(this.label23);
            this.MemberGroupBox.Controls.Add(this.label22);
            this.MemberGroupBox.Controls.Add(this.LimitedTargetNameComboBox);
            this.MemberGroupBox.Controls.Add(this.label21);
            this.MemberGroupBox.Controls.Add(this.label20);
            this.MemberGroupBox.Controls.Add(this.label18);
            this.MemberGroupBox.Controls.Add(this.label19);
            this.MemberGroupBox.Controls.Add(this.label14);
            this.MemberGroupBox.Controls.Add(this.label13);
            this.MemberGroupBox.Controls.Add(this.label12);
            this.MemberGroupBox.Controls.Add(this.LimitedTargetCheckBox);
            this.MemberGroupBox.Controls.Add(this.FavoriteMemberList);
            this.MemberGroupBox.Controls.Add(this.FavoriteMemberButton);
            this.MemberGroupBox.Controls.Add(this.DisableJobRadioButton);
            this.MemberGroupBox.Controls.Add(this.EnableJobRadioButton);
            this.MemberGroupBox.Controls.Add(this.TotalDamageLabel);
            this.MemberGroupBox.Controls.Add(this.label1);
            this.MemberGroupBox.Controls.Add(this.label2);
            this.MemberGroupBox.Controls.Add(this.label3);
            this.MemberGroupBox.Controls.Add(this.label4);
            this.MemberGroupBox.Controls.Add(this.label5);
            this.MemberGroupBox.Controls.Add(this.label6);
            this.MemberGroupBox.Controls.Add(this.label7);
            this.MemberGroupBox.Controls.Add(this.label8);
            this.MemberGroupBox.Controls.Add(this.label9);
            this.MemberGroupBox.Controls.Add(this.label10);
            this.MemberGroupBox.Controls.Add(this.label11);
            this.MemberGroupBox.Controls.Add(this.label15);
            this.MemberGroupBox.Controls.Add(this.label16);
            this.MemberGroupBox.Controls.Add(this.label17);
            this.MemberGroupBox.Controls.Add(this.Member01);
            this.MemberGroupBox.Controls.Add(this.Member02);
            this.MemberGroupBox.Controls.Add(this.Member03);
            this.MemberGroupBox.Controls.Add(this.Member04);
            this.MemberGroupBox.Controls.Add(this.Member05);
            this.MemberGroupBox.Controls.Add(this.Member06);
            this.MemberGroupBox.Controls.Add(this.Member07);
            this.MemberGroupBox.Controls.Add(this.Member08);
            this.MemberGroupBox.Controls.Add(this.Member09);
            this.MemberGroupBox.Controls.Add(this.Member10);
            this.MemberGroupBox.Controls.Add(this.Member11);
            this.MemberGroupBox.Controls.Add(this.Member12);
            this.MemberGroupBox.Location = new System.Drawing.Point(12, 41);
            this.MemberGroupBox.Name = "MemberGroupBox";
            this.MemberGroupBox.Size = new System.Drawing.Size(890, 526);
            this.MemberGroupBox.TabIndex = 19;
            this.MemberGroupBox.TabStop = false;
            this.MemberGroupBox.Text = "メンバー";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(444, 12);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(42, 12);
            this.label25.TabIndex = 50;
            this.label25.Text = "ダメージ";
            this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(380, 12);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(29, 12);
            this.label24.TabIndex = 49;
            this.label24.Text = "攻撃";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(520, 12);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(42, 12);
            this.label23.TabIndex = 48;
            this.label23.Text = "ダメージ";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(583, 12);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(55, 12);
            this.label22.TabIndex = 47;
            this.label22.Text = "クリティカル";
            this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LimitedTargetNameComboBox
            // 
            this.LimitedTargetNameComboBox.FormattingEnabled = true;
            this.LimitedTargetNameComboBox.Items.AddRange(new object[] {
            "ベリトラ",
            "理性を失ったプリギタ",
            "ネルガル",
            "呪われたオペル"});
            this.LimitedTargetNameComboBox.Location = new System.Drawing.Point(81, 500);
            this.LimitedTargetNameComboBox.Name = "LimitedTargetNameComboBox";
            this.LimitedTargetNameComboBox.Size = new System.Drawing.Size(203, 20);
            this.LimitedTargetNameComboBox.TabIndex = 46;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(761, 12);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(29, 12);
            this.label21.TabIndex = 45;
            this.label21.Text = "回避";
            this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(837, 12);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(29, 12);
            this.label20.TabIndex = 44;
            this.label20.Text = "抵抗";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(809, 43);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(57, 12);
            this.label18.TabIndex = 43;
            this.label18.Text = "された回数";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(819, 27);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(47, 12);
            this.label19.TabIndex = 42;
            this.label19.Text = "した回数";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 410);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(174, 12);
            this.label14.TabIndex = 41;
            this.label14.Text = "総ダメージ合計 (スキルリストを含む)";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(733, 43);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(57, 12);
            this.label13.TabIndex = 40;
            this.label13.Text = "された回数";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(743, 27);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(47, 12);
            this.label12.TabIndex = 39;
            this.label12.Text = "した回数";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LimitedTargetCheckBox
            // 
            this.LimitedTargetCheckBox.AutoSize = true;
            this.LimitedTargetCheckBox.Location = new System.Drawing.Point(3, 502);
            this.LimitedTargetCheckBox.Name = "LimitedTargetCheckBox";
            this.LimitedTargetCheckBox.Size = new System.Drawing.Size(72, 16);
            this.LimitedTargetCheckBox.TabIndex = 37;
            this.LimitedTargetCheckBox.Text = "対象限定";
            this.LimitedTargetCheckBox.UseVisualStyleBackColor = true;
            // 
            // FavoriteMemberList
            // 
            this.FavoriteMemberList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FavoriteMemberList.Location = new System.Drawing.Point(243, 58);
            this.FavoriteMemberList.Name = "FavoriteMemberList";
            this.FavoriteMemberList.Size = new System.Drawing.Size(522, 345);
            this.FavoriteMemberList.TabIndex = 25;
            this.FavoriteMemberList.Visible = false;
            // 
            // FavoriteMemberButton
            // 
            this.FavoriteMemberButton.Location = new System.Drawing.Point(53, 30);
            this.FavoriteMemberButton.Name = "FavoriteMemberButton";
            this.FavoriteMemberButton.Size = new System.Drawing.Size(75, 23);
            this.FavoriteMemberButton.TabIndex = 25;
            this.FavoriteMemberButton.Text = "登録";
            this.FavoriteMemberButton.UseVisualStyleBackColor = true;
            this.FavoriteMemberButton.Click += new System.EventHandler(this.FavoriteMemberButton_Click);
            // 
            // DisableJobRadioButton
            // 
            this.DisableJobRadioButton.AutoSize = true;
            this.DisableJobRadioButton.Checked = true;
            this.DisableJobRadioButton.Location = new System.Drawing.Point(86, 429);
            this.DisableJobRadioButton.Name = "DisableJobRadioButton";
            this.DisableJobRadioButton.Size = new System.Drawing.Size(72, 16);
            this.DisableJobRadioButton.TabIndex = 35;
            this.DisableJobRadioButton.TabStop = true;
            this.DisableJobRadioButton.Text = "ジョブ無効";
            this.DisableJobRadioButton.UseVisualStyleBackColor = true;
            // 
            // EnableJobRadioButton
            // 
            this.EnableJobRadioButton.AutoSize = true;
            this.EnableJobRadioButton.Location = new System.Drawing.Point(8, 429);
            this.EnableJobRadioButton.Name = "EnableJobRadioButton";
            this.EnableJobRadioButton.Size = new System.Drawing.Size(72, 16);
            this.EnableJobRadioButton.TabIndex = 34;
            this.EnableJobRadioButton.Text = "ジョブ有効";
            this.EnableJobRadioButton.UseVisualStyleBackColor = true;
            // 
            // TotalDamageLabel
            // 
            this.TotalDamageLabel.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.TotalDamageLabel.Location = new System.Drawing.Point(227, 410);
            this.TotalDamageLabel.Name = "TotalDamageLabel";
            this.TotalDamageLabel.Size = new System.Drawing.Size(82, 12);
            this.TotalDamageLabel.TabIndex = 31;
            this.TotalDamageLabel.Text = "0";
            this.TotalDamageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(433, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 24;
            this.label1.Text = "秒間平均";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(255, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 12);
            this.label2.TabIndex = 20;
            this.label2.Text = "総ダメージ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(392, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 21;
            this.label3.Text = "秒";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(380, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 22;
            this.label4.Text = "回数";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(433, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 23;
            this.label5.Text = "回数平均";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(533, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 25;
            this.label6.Text = "MAX";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(537, 43);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(25, 12);
            this.label7.TabIndex = 26;
            this.label7.Text = "MIN";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 27;
            this.label8.Text = "名前";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(145, 35);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(30, 12);
            this.label9.TabIndex = 28;
            this.label9.Text = "ジョブ";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(668, 27);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 12);
            this.label10.TabIndex = 29;
            this.label10.Text = "回数/秒";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.Location = new System.Drawing.Point(6, 452);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(874, 44);
            this.label11.TabIndex = 36;
            this.label11.Text = "ジョブ有効の場合、誰の攻撃かを特定できない攻撃をジョブで特定します。空白以外の全員の職を設定し、かつジョブが重複していないジョブの場合は、職固有スキルによるダメー" +
    "ジをメンバーに加算します。同一ジョブが2人以上いる場合やジョブを未選択の場合は、下のスキル別にダメージが加算されます。ジョブ無効の場合は誰の攻撃かを特定できない" +
    "攻撃は下のスキル別にダメージが加算されます。";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(322, 27);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(29, 12);
            this.label15.TabIndex = 30;
            this.label15.Text = "割合";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(604, 27);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(34, 12);
            this.label16.TabIndex = 32;
            this.label16.Text = "スキル";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(585, 43);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(53, 12);
            this.label17.TabIndex = 33;
            this.label17.Text = "通常攻撃";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Member01
            // 
            this.Member01.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member01.Location = new System.Drawing.Point(6, 58);
            this.Member01.Name = "Member01";
            this.Member01.Size = new System.Drawing.Size(874, 26);
            this.Member01.TabIndex = 1;
            // 
            // Member02
            // 
            this.Member02.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member02.Location = new System.Drawing.Point(6, 87);
            this.Member02.Name = "Member02";
            this.Member02.Size = new System.Drawing.Size(874, 26);
            this.Member02.TabIndex = 2;
            // 
            // Member03
            // 
            this.Member03.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member03.Location = new System.Drawing.Point(6, 116);
            this.Member03.Name = "Member03";
            this.Member03.Size = new System.Drawing.Size(874, 26);
            this.Member03.TabIndex = 3;
            // 
            // Member04
            // 
            this.Member04.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member04.Location = new System.Drawing.Point(6, 145);
            this.Member04.Name = "Member04";
            this.Member04.Size = new System.Drawing.Size(874, 26);
            this.Member04.TabIndex = 4;
            // 
            // Member05
            // 
            this.Member05.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member05.Location = new System.Drawing.Point(6, 174);
            this.Member05.Name = "Member05";
            this.Member05.Size = new System.Drawing.Size(874, 26);
            this.Member05.TabIndex = 5;
            // 
            // Member06
            // 
            this.Member06.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member06.Location = new System.Drawing.Point(6, 203);
            this.Member06.Name = "Member06";
            this.Member06.Size = new System.Drawing.Size(874, 26);
            this.Member06.TabIndex = 6;
            // 
            // Member07
            // 
            this.Member07.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member07.Location = new System.Drawing.Point(6, 232);
            this.Member07.Name = "Member07";
            this.Member07.Size = new System.Drawing.Size(874, 26);
            this.Member07.TabIndex = 7;
            // 
            // Member08
            // 
            this.Member08.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member08.Location = new System.Drawing.Point(6, 261);
            this.Member08.Name = "Member08";
            this.Member08.Size = new System.Drawing.Size(874, 26);
            this.Member08.TabIndex = 8;
            // 
            // Member09
            // 
            this.Member09.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member09.Location = new System.Drawing.Point(6, 290);
            this.Member09.Name = "Member09";
            this.Member09.Size = new System.Drawing.Size(874, 26);
            this.Member09.TabIndex = 9;
            // 
            // Member10
            // 
            this.Member10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member10.Location = new System.Drawing.Point(6, 319);
            this.Member10.Name = "Member10";
            this.Member10.Size = new System.Drawing.Size(874, 26);
            this.Member10.TabIndex = 10;
            // 
            // Member11
            // 
            this.Member11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member11.Location = new System.Drawing.Point(6, 348);
            this.Member11.Name = "Member11";
            this.Member11.Size = new System.Drawing.Size(874, 26);
            this.Member11.TabIndex = 11;
            // 
            // Member12
            // 
            this.Member12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member12.Location = new System.Drawing.Point(6, 377);
            this.Member12.Name = "Member12";
            this.Member12.Size = new System.Drawing.Size(874, 26);
            this.Member12.TabIndex = 12;
            // 
            // AION64bitButton
            // 
            this.AION64bitButton.Location = new System.Drawing.Point(827, 12);
            this.AION64bitButton.Name = "AION64bitButton";
            this.AION64bitButton.Size = new System.Drawing.Size(75, 23);
            this.AION64bitButton.TabIndex = 24;
            this.AION64bitButton.Text = "64bit起動";
            this.AION64bitButton.UseVisualStyleBackColor = true;
            this.AION64bitButton.Click += new System.EventHandler(this.AION64bitButton_Click);
            // 
            // DebugCheckBox
            // 
            this.DebugCheckBox.AutoSize = true;
            this.DebugCheckBox.Location = new System.Drawing.Point(707, 16);
            this.DebugCheckBox.Name = "DebugCheckBox";
            this.DebugCheckBox.Size = new System.Drawing.Size(114, 16);
            this.DebugCheckBox.TabIndex = 25;
            this.DebugCheckBox.Text = "デバッグ用ログ出力";
            this.DebugCheckBox.UseVisualStyleBackColor = true;
            // 
            // CalcFromLogButton
            // 
            this.CalcFromLogButton.Location = new System.Drawing.Point(255, 12);
            this.CalcFromLogButton.Name = "CalcFromLogButton";
            this.CalcFromLogButton.Size = new System.Drawing.Size(75, 23);
            this.CalcFromLogButton.TabIndex = 26;
            this.CalcFromLogButton.Text = "ログから計測";
            this.CalcFromLogButton.UseVisualStyleBackColor = true;
            this.CalcFromLogButton.Click += new System.EventHandler(this.CalcFromLogButton_Click);
            // 
            // AlwaysOnTopCheckBox
            // 
            this.AlwaysOnTopCheckBox.AutoSize = true;
            this.AlwaysOnTopCheckBox.Location = new System.Drawing.Point(617, 16);
            this.AlwaysOnTopCheckBox.Name = "AlwaysOnTopCheckBox";
            this.AlwaysOnTopCheckBox.Size = new System.Drawing.Size(84, 16);
            this.AlwaysOnTopCheckBox.TabIndex = 27;
            this.AlwaysOnTopCheckBox.Text = "最前面表示";
            this.AlwaysOnTopCheckBox.UseVisualStyleBackColor = true;
            this.AlwaysOnTopCheckBox.CheckedChanged += new System.EventHandler(this.AlwaysOnTopCheckBox_CheckedChanged);
            // 
            // CalcTimeCheckBox
            // 
            this.CalcTimeCheckBox.AutoSize = true;
            this.CalcTimeCheckBox.Location = new System.Drawing.Point(336, 16);
            this.CalcTimeCheckBox.Name = "CalcTimeCheckBox";
            this.CalcTimeCheckBox.Size = new System.Drawing.Size(92, 16);
            this.CalcTimeCheckBox.TabIndex = 28;
            this.CalcTimeCheckBox.Text = "計測時間(分)";
            this.CalcTimeCheckBox.UseVisualStyleBackColor = true;
            // 
            // CalcTimerMinutesNumericUpDown
            // 
            this.CalcTimerMinutesNumericUpDown.Location = new System.Drawing.Point(428, 13);
            this.CalcTimerMinutesNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.CalcTimerMinutesNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CalcTimerMinutesNumericUpDown.Name = "CalcTimerMinutesNumericUpDown";
            this.CalcTimerMinutesNumericUpDown.Size = new System.Drawing.Size(42, 19);
            this.CalcTimerMinutesNumericUpDown.TabIndex = 29;
            this.CalcTimerMinutesNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.CalcTimerMinutesNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.CalcTimerMinutesNumericUpDown.ValueChanged += new System.EventHandler(this.CalcTimerMinutesNumericUpDown_ValueChanged);
            // 
            // CalcRemainingTimeLabel
            // 
            this.CalcRemainingTimeLabel.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.CalcRemainingTimeLabel.ForeColor = System.Drawing.Color.Red;
            this.CalcRemainingTimeLabel.Location = new System.Drawing.Point(476, 12);
            this.CalcRemainingTimeLabel.Name = "CalcRemainingTimeLabel";
            this.CalcRemainingTimeLabel.Size = new System.Drawing.Size(45, 18);
            this.CalcRemainingTimeLabel.TabIndex = 30;
            this.CalcRemainingTimeLabel.Text = "300";
            this.CalcRemainingTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 821);
            this.Controls.Add(this.CalcRemainingTimeLabel);
            this.Controls.Add(this.CalcTimerMinutesNumericUpDown);
            this.Controls.Add(this.CalcTimeCheckBox);
            this.Controls.Add(this.AlwaysOnTopCheckBox);
            this.Controls.Add(this.CalcFromLogButton);
            this.Controls.Add(this.DebugCheckBox);
            this.Controls.Add(this.AION64bitButton);
            this.Controls.Add(this.MemberGroupBox);
            this.Controls.Add(this.SkillListFlowLayoutPanel);
            this.Controls.Add(this.FileButton);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.MaximumSize = new System.Drawing.Size(930, 860);
            this.Name = "MainForm";
            this.Text = "Open AION DPS 1.0.14";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MemberGroupBox.ResumeLayout(false);
            this.MemberGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CalcTimerMinutesNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button FileButton;
        private MemberUnit Member01;
        private MemberUnit Member02;
        private MemberUnit Member03;
        private MemberUnit Member04;
        private MemberUnit Member05;
        private MemberUnit Member06;
        private MemberUnit Member07;
        private MemberUnit Member08;
        private MemberUnit Member09;
        private MemberUnit Member10;
        private MemberUnit Member11;
        private MemberUnit Member12;
        private System.Windows.Forms.FlowLayoutPanel SkillListFlowLayoutPanel;
        private System.Windows.Forms.GroupBox MemberGroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label TotalDamageLabel;
        private System.Windows.Forms.Button AION64bitButton;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.RadioButton DisableJobRadioButton;
        private System.Windows.Forms.RadioButton EnableJobRadioButton;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button FavoriteMemberButton;
        private FavoriteMemberListUnit FavoriteMemberList;
        private System.Windows.Forms.CheckBox DebugCheckBox;
        private System.Windows.Forms.CheckBox LimitedTargetCheckBox;
        private System.Windows.Forms.Button CalcFromLogButton;
        private System.Windows.Forms.CheckBox AlwaysOnTopCheckBox;
        private System.Windows.Forms.CheckBox CalcTimeCheckBox;
        private System.Windows.Forms.NumericUpDown CalcTimerMinutesNumericUpDown;
        private System.Windows.Forms.Label CalcRemainingTimeLabel;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.ComboBox LimitedTargetNameComboBox;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
    }
}

