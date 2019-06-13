using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenAIONDPS
{
    public partial class SpeakerUnit : UserControl
    {
        public SpeakerUnit()
        {
            InitializeComponent();
        }

        public bool IsEnabled
        {
            set { this.EnableCheckBox.Checked = value; }
            get { return this.EnableCheckBox.Checked; }
        }

        public string PatternText
        {
            set { this.PatternTextBox.Text = value; }
            get { return this.PatternTextBox.Text; }
        }

        public string SpeakText
        {
            set { this.SpeakTextBox.Text = value; }
            get { return this.SpeakTextBox.Text; }
        }
    }
}
