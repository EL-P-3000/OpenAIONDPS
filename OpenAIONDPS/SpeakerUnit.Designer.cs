namespace OpenAIONDPS
{
    partial class SpeakerUnit
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
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
            this.EnableCheckBox = new System.Windows.Forms.CheckBox();
            this.PatternTextBox = new System.Windows.Forms.TextBox();
            this.SpeakTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // EnableCheckBox
            // 
            this.EnableCheckBox.AutoSize = true;
            this.EnableCheckBox.Location = new System.Drawing.Point(3, 3);
            this.EnableCheckBox.Name = "EnableCheckBox";
            this.EnableCheckBox.Size = new System.Drawing.Size(15, 14);
            this.EnableCheckBox.TabIndex = 0;
            this.EnableCheckBox.UseVisualStyleBackColor = true;
            // 
            // PatternTextBox
            // 
            this.PatternTextBox.Location = new System.Drawing.Point(24, 0);
            this.PatternTextBox.Name = "PatternTextBox";
            this.PatternTextBox.Size = new System.Drawing.Size(583, 19);
            this.PatternTextBox.TabIndex = 1;
            // 
            // SpeakTextBox
            // 
            this.SpeakTextBox.Location = new System.Drawing.Point(613, 0);
            this.SpeakTextBox.Name = "SpeakTextBox";
            this.SpeakTextBox.Size = new System.Drawing.Size(471, 19);
            this.SpeakTextBox.TabIndex = 2;
            // 
            // SpeakerUnit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SpeakTextBox);
            this.Controls.Add(this.PatternTextBox);
            this.Controls.Add(this.EnableCheckBox);
            this.Name = "SpeakerUnit";
            this.Size = new System.Drawing.Size(1087, 21);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox EnableCheckBox;
        private System.Windows.Forms.TextBox PatternTextBox;
        private System.Windows.Forms.TextBox SpeakTextBox;
    }
}
