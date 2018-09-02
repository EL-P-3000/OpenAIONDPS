namespace OpenAIONDPS
{
    partial class SkillUnit
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
            this.NameLabel = new System.Windows.Forms.Label();
            this.DamageLabel = new System.Windows.Forms.Label();
            this.JobLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // NameLabel
            // 
            this.NameLabel.Location = new System.Drawing.Point(68, 2);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(250, 13);
            this.NameLabel.TabIndex = 0;
            this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DamageLabel
            // 
            this.DamageLabel.Location = new System.Drawing.Point(318, 2);
            this.DamageLabel.Name = "DamageLabel";
            this.DamageLabel.Size = new System.Drawing.Size(50, 13);
            this.DamageLabel.TabIndex = 1;
            this.DamageLabel.Text = "0";
            this.DamageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // JobLabel
            // 
            this.JobLabel.Location = new System.Drawing.Point(1, 2);
            this.JobLabel.Name = "JobLabel";
            this.JobLabel.Size = new System.Drawing.Size(50, 13);
            this.JobLabel.TabIndex = 2;
            this.JobLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SkillUnit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.JobLabel);
            this.Controls.Add(this.DamageLabel);
            this.Controls.Add(this.NameLabel);
            this.Name = "SkillUnit";
            this.Size = new System.Drawing.Size(400, 16);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Label DamageLabel;
        private System.Windows.Forms.Label JobLabel;
    }
}
