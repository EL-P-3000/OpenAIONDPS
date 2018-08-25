using System;
using System.Windows.Forms;

namespace OpenAIONDPS
{
    public partial class SkillUnit : UserControl
    {
        private long Damage = 0;

        public SkillUnit()
        {
            InitializeComponent();
        }

        public void SetName(string Name)
        {
            this.NameLabel.Text = Name;
        }

        public string GetName()
        {
            return this.NameLabel.Text;
        }

        public void UpdateDamage(long Damage)
        {
            this.Damage += Damage;
            this.DamageLabel.Text = this.Damage.ToString("#,0");
        }

        public void Clear()
        {
            this.Damage = 0;
            this.DamageLabel.Text = "0";
        }
    }
}
