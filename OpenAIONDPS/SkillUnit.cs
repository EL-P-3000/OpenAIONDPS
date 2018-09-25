using System;
using System.Windows.Forms;

namespace OpenAIONDPS
{
    public partial class SkillUnit : UserControl
    {
        private bool IsStartWithCritical = false;
        private bool IsStartWithoutCritical = false;

        public SkillUnit()
        {
            InitializeComponent();
        }

        public long Damage { get; set; } = 0;

        public long AttackNumber { get; set; } = 0;

        public long MaxDamageWithCritical { get; set; } = 0;

        public long MinDamageWithCritical { get; set; } = 0;

        public long MaxDamageWithoutCritical { get; set; } = 0;

        public long MinDamageWithoutCritical { get; set; } = 0;

        public void SetJob(AION.JobType JobType)
        {
            this.JobLabel.Text = AION.GetJobName(JobType);
        }

        public string GetJobName()
        {
            return this.JobLabel.Text;
        }

        public void SetName(string Name)
        {
            this.NameLabel.Text = Name;
        }

        public string GetName()
        {
            return this.NameLabel.Text;
        }

        public void AddDamage(long Damage, bool IsCriticalHit)
        {
            this.UpdateDamage(Damage);
            this.UpdateAttackNumber();
            this.UpdateMaxDamage(Damage, IsCriticalHit);
            this.UpdateMinDamage(Damage, IsCriticalHit);

            if (IsCriticalHit)
            {
                this.IsStartWithCritical = true;
            }
            else
            {
                this.IsStartWithoutCritical = true;
            }
        }

        private void UpdateDamage(long Damage)
        {
            this.Damage += Damage;
            this.DamageLabel.Text = this.Damage.ToString("#,0");
        }

        private void UpdateAttackNumber()
        {
            this.AttackNumber += 1;
        }

        private void UpdateMaxDamage(long Damage, bool IsCriticalHit)
        {
            if (IsCriticalHit)
            {
                if (!this.IsStartWithCritical || this.MaxDamageWithCritical < Damage)
                {
                    this.MaxDamageWithCritical = Damage;
                }
            }
            else
            {
                if (!this.IsStartWithoutCritical || this.MaxDamageWithoutCritical < Damage)
                {
                    this.MaxDamageWithoutCritical = Damage;
                }
            }
        }

        private void UpdateMinDamage(long Damage, bool IsCriticalHit)
        {
            if (IsCriticalHit)
            {
                if (!this.IsStartWithCritical || this.MinDamageWithCritical > Damage)
                {
                    this.MinDamageWithCritical = Damage;
                }
            }
            else
            {
                if (!this.IsStartWithoutCritical || this.MinDamageWithoutCritical > Damage)
                {
                    this.MinDamageWithoutCritical = Damage;
                }
            }
        }

        public void Clear()
        {
            this.Damage = 0;
            this.AttackNumber = 0;
            this.MaxDamageWithCritical = 0;
            this.MinDamageWithCritical = 0;
            this.MaxDamageWithoutCritical = 0;
            this.MinDamageWithoutCritical = 0;
            this.IsStartWithCritical = false;
            this.IsStartWithoutCritical = false;
            this.DamageLabel.Text = "0";
        }

        public string GetResult()
        {
            string Result = "";

            try
            {
                if (String.IsNullOrEmpty(this.DamageLabel.Text) || this.DamageLabel.Text.Equals("0"))
                {
                    return Result;
                }

                Result += this.NameLabel.Text + "： " + this.DamageLabel.Text;
            }
            catch
            {
            }

            return Result;
        }
    }
}
