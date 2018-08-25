using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenAIONDPS
{
    public partial class MemberUnit : UserControl
    {
        private static DateTime DefaultTime = new DateTime(0);
        private DateTime StartTime = DefaultTime;
        private DateTime EndTime = DefaultTime;
        private bool StartFlag = false;
        private long Damage = 0;
        private long MaxDamage = 0;
        private long MinDamage = 0;
        private long AttackNumber = 0;
        private long CriticalNumber = 0;
        private long DeadNumber = 0;

        public MemberUnit()
        {
            InitializeComponent();

            List<AION.JobSet> JobSetList = new List<AION.JobSet>();
            foreach (AION.JobType Job in Enum.GetValues(typeof(AION.JobType)))
            {
                JobSetList.Add(new AION.JobSet(Job, AION.GetJobName(Job)));
            }

            this.JobComboBox.Items.Clear();
            this.JobComboBox.DataSource = JobSetList;
            this.JobComboBox.DisplayMember = "Name";
            this.JobComboBox.ValueMember = "Type";
            this.JobComboBox.SelectedIndex = 0;
        }

        public void Clear()
        {
            this.StartTime = DefaultTime;
            this.EndTime = DefaultTime;
            this.StartFlag = false;
            this.Damage = 0;
            this.MaxDamage = 0;
            this.MinDamage = 0;
            this.AttackNumber = 0;
            this.CriticalNumber = 0;
            this.DeadNumber = 0;
            this.DamageLabel.Text = "0";
            this.MaxDamageLabel.Text = "0";
            this.MinDamageLabel.Text = "0";
            this.SecondLabel.Text = "0";
            this.AttackNumberLabel.Text = "0";
            this.DamageParSecondLabel.Text = "0";
            this.DamageParAttackNumberLabel.Text = "0";
            this.AttackNumberParSecondLabel.Text = "0";
            this.CriticalNumberLabel.Text = "0";
            this.CriticalNumberParAttackNumberLabel.Text = "0%";
            this.DamageParTotalDamageLabel.Text = "100%";
        }

        public bool IsStart()
        {
            return this.StartFlag;
        }

        public void SetMemberName(String CharacterName)
        {
            this.CharacterNameTextBox.Text = CharacterName;
        }

        public String GetMemberName()
        {
            return this.CharacterNameTextBox.Text;
        }

        public void SetJobType(AION.JobType Job)
        {
            this.JobComboBox.SelectedValue = Job;
        }

        public AION.JobType GetJob()
        {
            return (AION.JobType)this.JobComboBox.SelectedValue;
        }

        public void AddDamage(long Damage, bool IsCritical, DateTime Time)
        {
            if (!this.IsStart())
            {
                this.StartFlag = true;
                this.StartTime = Time;
            }
            this.EndTime = Time;

            this.UpdateDamage(Damage);
            if (IsCritical)
            {
                this.UpdateCriticalHit(); // 必ず UpdateAttackNumber() の前に呼ぶ。
            }
            this.UpdateSeconds();
            this.UpdateAttackNumber();
            this.UpdateMaxDamage(Damage);
            this.UpdateMinDamage(Damage);
            this.UpdateDamageParSecond();
            this.UpdateDamageParAttackNumber();
            this.UpdateAttackNumberParSecond();
        }

        private void UpdateDamage(long Damage)
        {
            this.Damage += Damage;
            this.DamageLabel.Text = this.Damage.ToString("#,0");
        }

        public void UpdateDamageParTotalDamage(long TotalDamage)
        {
            this.DamageParTotalDamageLabel.Text = ((long)(this.Damage * 100 / TotalDamage)).ToString() + "%";
        }


        private void UpdateSeconds()
        {
            long Ticks = this.EndTime.Ticks - this.StartTime.Ticks;
            this.SecondLabel.Text = Math.Ceiling((double)Ticks / 10000000).ToString("#,0");
        }

        private void UpdateAttackNumber()
        {
            this.AttackNumber += 1;
            this.AttackNumberLabel.Text = this.AttackNumber.ToString("#,0");
            if (this.CriticalNumber > 0)
            {
                this.CriticalNumberParAttackNumberLabel.Text = (((1.0 * this.CriticalNumber / this.AttackNumber)) * 100).ToString("F2") + "%";
            }
        }

        private void UpdateMaxDamage(long Damage)
        {
            if (this.MaxDamage == 0 || this.MaxDamage < Damage)
            {
                this.MaxDamage = Damage;
                this.MaxDamageLabel.Text = this.MaxDamage.ToString("#,0");
            }
        }

        private void UpdateMinDamage(long Damage)
        {
            if (this.MinDamage == 0 || this.MinDamage > Damage)
            {
                this.MinDamage = Damage;
                this.MinDamageLabel.Text = this.MinDamage.ToString("#,0");
            }
        }

        private void UpdateDamageParSecond()
        {
            long Ticks = this.EndTime.Ticks - this.StartTime.Ticks;

            if (Ticks <= 10000000)
            {
                this.DamageParSecondLabel.Text = (this.Damage).ToString("#,0");
            }
            else
            {
                double Second = Math.Ceiling((double)Ticks / 10000000);
                this.DamageParSecondLabel.Text = Math.Ceiling(this.Damage / Second).ToString("#,0");
            }
        }

        private void UpdateDamageParAttackNumber()
        {
            this.DamageParAttackNumberLabel.Text = Math.Ceiling((double)(this.Damage / this.AttackNumber)).ToString("#,0");
        }

        private void UpdateAttackNumberParSecond()
        {
            long Ticks = this.EndTime.Ticks - this.StartTime.Ticks;
            this.AttackNumberParSecondLabel.Text = Math.Round((double)(this.AttackNumber / (double)(Ticks / 10000000)), 2, MidpointRounding.AwayFromZero).ToString("F2");
        }

        private void UpdateCriticalHit()
        {
            this.CriticalNumber += 1;
            this.CriticalNumberLabel.Text = this.CriticalNumber.ToString("#,0");
        }
    }
}
