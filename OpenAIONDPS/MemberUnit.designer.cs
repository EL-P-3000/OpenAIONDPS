namespace OpenAIONDPS
{
    partial class MemberUnit
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

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.CharacterNameTextBox = new System.Windows.Forms.TextBox();
            this.DamageLabel = new System.Windows.Forms.Label();
            this.MaxDamageLabel = new System.Windows.Forms.Label();
            this.MinDamageLabel = new System.Windows.Forms.Label();
            this.DamageParSecondLabel = new System.Windows.Forms.Label();
            this.SecondLabel = new System.Windows.Forms.Label();
            this.SlashLabel = new System.Windows.Forms.Label();
            this.AttackNumberLabel = new System.Windows.Forms.Label();
            this.JobComboBox = new System.Windows.Forms.ComboBox();
            this.DamageParAttackNumberLabel = new System.Windows.Forms.Label();
            this.AttackNumberParSecondLabel = new System.Windows.Forms.Label();
            this.DamageParTotalDamageLabel = new System.Windows.Forms.Label();
            this.CriticalNumberParAttackNumberLabel = new System.Windows.Forms.Label();
            this.CriticalNumberLabel = new System.Windows.Forms.Label();
            this.EvadeAttackNumberLabel = new System.Windows.Forms.Label();
            this.EvadedAttackNumberLabel = new System.Windows.Forms.Label();
            this.ResistedAttackNumberLabel = new System.Windows.Forms.Label();
            this.ResistAttackNumberLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CharacterNameTextBox
            // 
            this.CharacterNameTextBox.Location = new System.Drawing.Point(0, 3);
            this.CharacterNameTextBox.Name = "CharacterNameTextBox";
            this.CharacterNameTextBox.Size = new System.Drawing.Size(132, 19);
            this.CharacterNameTextBox.TabIndex = 0;
            // 
            // DamageLabel
            // 
            this.DamageLabel.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.DamageLabel.Location = new System.Drawing.Point(217, 6);
            this.DamageLabel.Name = "DamageLabel";
            this.DamageLabel.Size = new System.Drawing.Size(86, 12);
            this.DamageLabel.TabIndex = 1;
            this.DamageLabel.Text = "0";
            this.DamageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MaxDamageLabel
            // 
            this.MaxDamageLabel.Location = new System.Drawing.Point(487, 1);
            this.MaxDamageLabel.Name = "MaxDamageLabel";
            this.MaxDamageLabel.Size = new System.Drawing.Size(70, 12);
            this.MaxDamageLabel.TabIndex = 2;
            this.MaxDamageLabel.Text = "0";
            this.MaxDamageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MinDamageLabel
            // 
            this.MinDamageLabel.Location = new System.Drawing.Point(487, 13);
            this.MinDamageLabel.Name = "MinDamageLabel";
            this.MinDamageLabel.Size = new System.Drawing.Size(70, 12);
            this.MinDamageLabel.TabIndex = 3;
            this.MinDamageLabel.Text = "0";
            this.MinDamageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DamageParSecondLabel
            // 
            this.DamageParSecondLabel.Location = new System.Drawing.Point(411, 1);
            this.DamageParSecondLabel.Name = "DamageParSecondLabel";
            this.DamageParSecondLabel.Size = new System.Drawing.Size(70, 12);
            this.DamageParSecondLabel.TabIndex = 4;
            this.DamageParSecondLabel.Text = "0";
            this.DamageParSecondLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SecondLabel
            // 
            this.SecondLabel.Location = new System.Drawing.Point(370, 1);
            this.SecondLabel.Name = "SecondLabel";
            this.SecondLabel.Size = new System.Drawing.Size(35, 12);
            this.SecondLabel.TabIndex = 5;
            this.SecondLabel.Text = "0";
            this.SecondLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SlashLabel
            // 
            this.SlashLabel.Location = new System.Drawing.Point(348, 0);
            this.SlashLabel.Name = "SlashLabel";
            this.SlashLabel.Size = new System.Drawing.Size(16, 26);
            this.SlashLabel.TabIndex = 6;
            this.SlashLabel.Text = "/";
            this.SlashLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AttackNumberLabel
            // 
            this.AttackNumberLabel.Location = new System.Drawing.Point(370, 13);
            this.AttackNumberLabel.Name = "AttackNumberLabel";
            this.AttackNumberLabel.Size = new System.Drawing.Size(35, 12);
            this.AttackNumberLabel.TabIndex = 7;
            this.AttackNumberLabel.Text = "0";
            this.AttackNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // JobComboBox
            // 
            this.JobComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.JobComboBox.FormattingEnabled = true;
            this.JobComboBox.Items.AddRange(new object[] {
            "未指定",
            "ソード",
            "シールド",
            "シャドウ",
            "ボウ",
            "スペル",
            "スピリット",
            "キュア",
            "チャント",
            "バレット",
            "ギア",
            "メロディ"});
            this.JobComboBox.Location = new System.Drawing.Point(138, 3);
            this.JobComboBox.Name = "JobComboBox";
            this.JobComboBox.Size = new System.Drawing.Size(65, 20);
            this.JobComboBox.TabIndex = 9;
            // 
            // DamageParAttackNumberLabel
            // 
            this.DamageParAttackNumberLabel.Location = new System.Drawing.Point(411, 13);
            this.DamageParAttackNumberLabel.Name = "DamageParAttackNumberLabel";
            this.DamageParAttackNumberLabel.Size = new System.Drawing.Size(70, 12);
            this.DamageParAttackNumberLabel.TabIndex = 11;
            this.DamageParAttackNumberLabel.Text = "0";
            this.DamageParAttackNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AttackNumberParSecondLabel
            // 
            this.AttackNumberParSecondLabel.Location = new System.Drawing.Point(639, 6);
            this.AttackNumberParSecondLabel.Name = "AttackNumberParSecondLabel";
            this.AttackNumberParSecondLabel.Size = new System.Drawing.Size(70, 12);
            this.AttackNumberParSecondLabel.TabIndex = 12;
            this.AttackNumberParSecondLabel.Text = "0";
            this.AttackNumberParSecondLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DamageParTotalDamageLabel
            // 
            this.DamageParTotalDamageLabel.Location = new System.Drawing.Point(309, 0);
            this.DamageParTotalDamageLabel.Name = "DamageParTotalDamageLabel";
            this.DamageParTotalDamageLabel.Size = new System.Drawing.Size(33, 27);
            this.DamageParTotalDamageLabel.TabIndex = 13;
            this.DamageParTotalDamageLabel.Text = "0%";
            this.DamageParTotalDamageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CriticalNumberParAttackNumberLabel
            // 
            this.CriticalNumberParAttackNumberLabel.Location = new System.Drawing.Point(563, 13);
            this.CriticalNumberParAttackNumberLabel.Name = "CriticalNumberParAttackNumberLabel";
            this.CriticalNumberParAttackNumberLabel.Size = new System.Drawing.Size(70, 12);
            this.CriticalNumberParAttackNumberLabel.TabIndex = 15;
            this.CriticalNumberParAttackNumberLabel.Text = "0%";
            this.CriticalNumberParAttackNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CriticalNumberLabel
            // 
            this.CriticalNumberLabel.Location = new System.Drawing.Point(563, 1);
            this.CriticalNumberLabel.Name = "CriticalNumberLabel";
            this.CriticalNumberLabel.Size = new System.Drawing.Size(70, 12);
            this.CriticalNumberLabel.TabIndex = 14;
            this.CriticalNumberLabel.Text = "0";
            this.CriticalNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // EvadeAttackNumberLabel
            // 
            this.EvadeAttackNumberLabel.Location = new System.Drawing.Point(715, 1);
            this.EvadeAttackNumberLabel.Name = "EvadeAttackNumberLabel";
            this.EvadeAttackNumberLabel.Size = new System.Drawing.Size(70, 12);
            this.EvadeAttackNumberLabel.TabIndex = 16;
            this.EvadeAttackNumberLabel.Text = "0";
            this.EvadeAttackNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // EvadedAttackNumberLabel
            // 
            this.EvadedAttackNumberLabel.Location = new System.Drawing.Point(715, 13);
            this.EvadedAttackNumberLabel.Name = "EvadedAttackNumberLabel";
            this.EvadedAttackNumberLabel.Size = new System.Drawing.Size(70, 12);
            this.EvadedAttackNumberLabel.TabIndex = 17;
            this.EvadedAttackNumberLabel.Text = "0";
            this.EvadedAttackNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ResistedAttackNumberLabel
            // 
            this.ResistedAttackNumberLabel.Location = new System.Drawing.Point(791, 13);
            this.ResistedAttackNumberLabel.Name = "ResistedAttackNumberLabel";
            this.ResistedAttackNumberLabel.Size = new System.Drawing.Size(70, 12);
            this.ResistedAttackNumberLabel.TabIndex = 19;
            this.ResistedAttackNumberLabel.Text = "0";
            this.ResistedAttackNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ResistAttackNumberLabel
            // 
            this.ResistAttackNumberLabel.Location = new System.Drawing.Point(791, 1);
            this.ResistAttackNumberLabel.Name = "ResistAttackNumberLabel";
            this.ResistAttackNumberLabel.Size = new System.Drawing.Size(70, 12);
            this.ResistAttackNumberLabel.TabIndex = 18;
            this.ResistAttackNumberLabel.Text = "0";
            this.ResistAttackNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MemberUnit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ResistedAttackNumberLabel);
            this.Controls.Add(this.ResistAttackNumberLabel);
            this.Controls.Add(this.EvadedAttackNumberLabel);
            this.Controls.Add(this.EvadeAttackNumberLabel);
            this.Controls.Add(this.CriticalNumberParAttackNumberLabel);
            this.Controls.Add(this.CriticalNumberLabel);
            this.Controls.Add(this.DamageParTotalDamageLabel);
            this.Controls.Add(this.AttackNumberParSecondLabel);
            this.Controls.Add(this.DamageParAttackNumberLabel);
            this.Controls.Add(this.JobComboBox);
            this.Controls.Add(this.AttackNumberLabel);
            this.Controls.Add(this.SlashLabel);
            this.Controls.Add(this.SecondLabel);
            this.Controls.Add(this.DamageParSecondLabel);
            this.Controls.Add(this.MinDamageLabel);
            this.Controls.Add(this.MaxDamageLabel);
            this.Controls.Add(this.DamageLabel);
            this.Controls.Add(this.CharacterNameTextBox);
            this.Name = "MemberUnit";
            this.Size = new System.Drawing.Size(869, 26);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox CharacterNameTextBox;
        private System.Windows.Forms.Label DamageLabel;
        private System.Windows.Forms.Label MaxDamageLabel;
        private System.Windows.Forms.Label MinDamageLabel;
        private System.Windows.Forms.Label DamageParSecondLabel;
        private System.Windows.Forms.Label SecondLabel;
        private System.Windows.Forms.Label SlashLabel;
        private System.Windows.Forms.Label AttackNumberLabel;
        private System.Windows.Forms.ComboBox JobComboBox;
        private System.Windows.Forms.Label DamageParAttackNumberLabel;
        private System.Windows.Forms.Label AttackNumberParSecondLabel;
        private System.Windows.Forms.Label DamageParTotalDamageLabel;
        private System.Windows.Forms.Label CriticalNumberParAttackNumberLabel;
        private System.Windows.Forms.Label CriticalNumberLabel;
        private System.Windows.Forms.Label EvadeAttackNumberLabel;
        private System.Windows.Forms.Label EvadedAttackNumberLabel;
        private System.Windows.Forms.Label ResistedAttackNumberLabel;
        private System.Windows.Forms.Label ResistAttackNumberLabel;
    }
}
