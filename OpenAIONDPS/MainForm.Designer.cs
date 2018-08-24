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
            this.MemberGroupBox.SuspendLayout();
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
            this.SkillListFlowLayoutPanel.Location = new System.Drawing.Point(12, 505);
            this.SkillListFlowLayoutPanel.Name = "SkillListFlowLayoutPanel";
            this.SkillListFlowLayoutPanel.Size = new System.Drawing.Size(750, 179);
            this.SkillListFlowLayoutPanel.TabIndex = 15;
            // 
            // MemberGroupBox
            // 
            this.MemberGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.MemberGroupBox.Size = new System.Drawing.Size(750, 458);
            this.MemberGroupBox.TabIndex = 19;
            this.MemberGroupBox.TabStop = false;
            this.MemberGroupBox.Text = "メンバー";
            // 
            // FavoriteMemberList
            // 
            this.FavoriteMemberList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FavoriteMemberList.Location = new System.Drawing.Point(220, 15);
            this.FavoriteMemberList.Name = "FavoriteMemberList";
            this.FavoriteMemberList.Size = new System.Drawing.Size(522, 372);
            this.FavoriteMemberList.TabIndex = 25;
            this.FavoriteMemberList.Visible = false;
            // 
            // FavoriteMemberButton
            // 
            this.FavoriteMemberButton.Location = new System.Drawing.Point(53, 18);
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
            this.DisableJobRadioButton.Location = new System.Drawing.Point(86, 392);
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
            this.EnableJobRadioButton.Location = new System.Drawing.Point(8, 392);
            this.EnableJobRadioButton.Name = "EnableJobRadioButton";
            this.EnableJobRadioButton.Size = new System.Drawing.Size(72, 16);
            this.EnableJobRadioButton.TabIndex = 34;
            this.EnableJobRadioButton.Text = "ジョブ有効";
            this.EnableJobRadioButton.UseVisualStyleBackColor = true;
            // 
            // TotalDamageLabel
            // 
            this.TotalDamageLabel.Location = new System.Drawing.Point(218, 374);
            this.TotalDamageLabel.Name = "TotalDamageLabel";
            this.TotalDamageLabel.Size = new System.Drawing.Size(75, 13);
            this.TotalDamageLabel.TabIndex = 31;
            this.TotalDamageLabel.Text = "0";
            this.TotalDamageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(406, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 12);
            this.label1.TabIndex = 24;
            this.label1.Text = "ダメージ/秒";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(239, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 12);
            this.label2.TabIndex = 20;
            this.label2.Text = "総ダメージ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(371, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 21;
            this.label3.Text = "秒";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(359, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 22;
            this.label4.Text = "回数";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(394, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 12);
            this.label5.TabIndex = 23;
            this.label5.Text = "ダメージ/回数";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(476, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 12);
            this.label6.TabIndex = 25;
            this.label6.Text = "MAXダメージ";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(480, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 12);
            this.label7.TabIndex = 26;
            this.label7.Text = "MINダメージ";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 27;
            this.label8.Text = "名前";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(145, 23);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(30, 12);
            this.label9.TabIndex = 28;
            this.label9.Text = "ジョブ";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(652, 23);
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
            this.label11.Location = new System.Drawing.Point(6, 411);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(744, 44);
            this.label11.TabIndex = 36;
            this.label11.Text = "ジョブ有効の場合、誰の攻撃かを特定できない攻撃をジョブで特定します。空白以外の全員の職を設定し、かつジョブが重複していないジョブの場合は、職固有スキルによるダメー" +
    "ジをメンバーに加算します。同一ジョブが2人以上いる場合やジョブを未選択の場合は、下のスキル別にダメージが加算されます。ジョブ無効の場合は誰の攻撃かを特定できない" +
    "攻撃は下のスキル別にダメージが加算されます。";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(304, 23);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(29, 12);
            this.label15.TabIndex = 30;
            this.label15.Text = "割合";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(577, 15);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(44, 12);
            this.label16.TabIndex = 32;
            this.label16.Text = "クリ回数";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(577, 31);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(44, 12);
            this.label17.TabIndex = 33;
            this.label17.Text = "クリ確率";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Member01
            // 
            this.Member01.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member01.Location = new System.Drawing.Point(6, 46);
            this.Member01.Name = "Member01";
            this.Member01.Size = new System.Drawing.Size(721, 26);
            this.Member01.TabIndex = 1;
            // 
            // Member02
            // 
            this.Member02.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member02.Location = new System.Drawing.Point(6, 73);
            this.Member02.Name = "Member02";
            this.Member02.Size = new System.Drawing.Size(721, 26);
            this.Member02.TabIndex = 2;
            // 
            // Member03
            // 
            this.Member03.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member03.Location = new System.Drawing.Point(6, 100);
            this.Member03.Name = "Member03";
            this.Member03.Size = new System.Drawing.Size(721, 26);
            this.Member03.TabIndex = 3;
            // 
            // Member04
            // 
            this.Member04.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member04.Location = new System.Drawing.Point(6, 127);
            this.Member04.Name = "Member04";
            this.Member04.Size = new System.Drawing.Size(721, 26);
            this.Member04.TabIndex = 4;
            // 
            // Member05
            // 
            this.Member05.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member05.Location = new System.Drawing.Point(6, 154);
            this.Member05.Name = "Member05";
            this.Member05.Size = new System.Drawing.Size(721, 26);
            this.Member05.TabIndex = 5;
            // 
            // Member06
            // 
            this.Member06.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member06.Location = new System.Drawing.Point(6, 181);
            this.Member06.Name = "Member06";
            this.Member06.Size = new System.Drawing.Size(721, 26);
            this.Member06.TabIndex = 6;
            // 
            // Member07
            // 
            this.Member07.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member07.Location = new System.Drawing.Point(6, 208);
            this.Member07.Name = "Member07";
            this.Member07.Size = new System.Drawing.Size(721, 26);
            this.Member07.TabIndex = 7;
            // 
            // Member08
            // 
            this.Member08.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member08.Location = new System.Drawing.Point(6, 235);
            this.Member08.Name = "Member08";
            this.Member08.Size = new System.Drawing.Size(721, 26);
            this.Member08.TabIndex = 8;
            // 
            // Member09
            // 
            this.Member09.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member09.Location = new System.Drawing.Point(6, 262);
            this.Member09.Name = "Member09";
            this.Member09.Size = new System.Drawing.Size(721, 26);
            this.Member09.TabIndex = 9;
            // 
            // Member10
            // 
            this.Member10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member10.Location = new System.Drawing.Point(6, 289);
            this.Member10.Name = "Member10";
            this.Member10.Size = new System.Drawing.Size(721, 26);
            this.Member10.TabIndex = 10;
            // 
            // Member11
            // 
            this.Member11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member11.Location = new System.Drawing.Point(6, 316);
            this.Member11.Name = "Member11";
            this.Member11.Size = new System.Drawing.Size(721, 26);
            this.Member11.TabIndex = 11;
            // 
            // Member12
            // 
            this.Member12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Member12.Location = new System.Drawing.Point(6, 343);
            this.Member12.Name = "Member12";
            this.Member12.Size = new System.Drawing.Size(721, 26);
            this.Member12.TabIndex = 12;
            // 
            // AION64bitButton
            // 
            this.AION64bitButton.Location = new System.Drawing.Point(690, 12);
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
            this.DebugCheckBox.Location = new System.Drawing.Point(624, 16);
            this.DebugCheckBox.Name = "DebugCheckBox";
            this.DebugCheckBox.Size = new System.Drawing.Size(60, 16);
            this.DebugCheckBox.TabIndex = 25;
            this.DebugCheckBox.Text = "デバッグ";
            this.DebugCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 696);
            this.Controls.Add(this.DebugCheckBox);
            this.Controls.Add(this.AION64bitButton);
            this.Controls.Add(this.MemberGroupBox);
            this.Controls.Add(this.SkillListFlowLayoutPanel);
            this.Controls.Add(this.FileButton);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.Name = "MainForm";
            this.Text = "Open AION DPS 1.0.1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MemberGroupBox.ResumeLayout(false);
            this.MemberGroupBox.PerformLayout();
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
    }
}

