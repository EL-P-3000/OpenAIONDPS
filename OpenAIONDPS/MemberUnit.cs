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
        private long SkillAttackNumber = 0;
        private long SimpleAttackNumber = 0;
        private long SkillCriticalNumber = 0;
        private long SimpleCriticalNumber = 0;
        private long EvadeAttackNumber = 0;
        private long EvadedAttackNumber = 0;
        private long ResistAttackNumber = 0;
        private long ResistedAttackNumber = 0;

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
            this.SkillAttackNumber = 0;
            this.SimpleAttackNumber = 0;
            this.SkillCriticalNumber = 0;
            this.SimpleCriticalNumber = 0;
            this.EvadeAttackNumber = 0;
            this.EvadedAttackNumber = 0;
            this.ResistAttackNumber = 0;
            this.ResistedAttackNumber = 0;

            this.DamageLabel.Text = "0";
            this.MaxDamageLabel.Text = "0";
            this.MinDamageLabel.Text = "0";
            this.SecondLabel.Text = "0";
            this.AttackNumberLabel.Text = "0";
            this.DamageParSecondLabel.Text = "0";
            this.DamageParAttackNumberLabel.Text = "0";
            this.AttackNumberParSecondLabel.Text = "0";
            this.SkillAttackNumberLabel.Text = "0";
            this.SimpleAttackNumberLabel.Text = "0";
            this.SkillCriticalNumberLabel.Text = "0 (0%)";
            this.SimpleCriticalNumberLabel.Text = "0 (0%)";
            this.DamageParTotalDamageLabel.Text = "100%";
            this.EvadeAttackNumberLabel.Text = "0";
            this.EvadedAttackNumberLabel.Text = "0 (0%)";
            this.ResistAttackNumberLabel.Text = "0";
            this.ResistedAttackNumberLabel.Text = "0 (0%)";
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

        public void AddDamage(long Damage, bool IsSkill, bool IsCritical, DateTime Time)
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
                this.UpdateCriticalHit(IsSkill);
            }
            this.UpdateSeconds();
            this.UpdateAttackNumber(IsSkill);
            this.UpdateMaxDamage(Damage);
            this.UpdateMinDamage(Damage);
            this.UpdateDamageParSecond();
            this.UpdateDamageParAttackNumber();
            this.UpdateAttackNumberParSecond();

            this.UpdateCriticalHitLabel();
        }

        public void AddEvasion(bool IsSourceNameMember, bool IsSkill, DateTime Time)
        {
            // 回避した攻撃
            if (IsSourceNameMember)
            {
                this.UpdateEvadeAttackNumber();
            }
            // 回避された攻撃
            else
            {
                if (!this.IsStart())
                {
                    this.StartFlag = true;
                    this.StartTime = Time;
                }
                this.EndTime = Time;

                this.UpdateEvadedAttackNumber(IsSkill);
            }

        }

        public void AddResistance(bool IsSourceNameMember, bool IsSkill, DateTime Time)
        {
            // 回避した攻撃
            if (IsSourceNameMember)
            {
                this.UpdateResistAttackNumber();
            }
            // 回避された攻撃
            else
            {
                if (!this.IsStart())
                {
                    this.StartFlag = true;
                    this.StartTime = Time;
                }
                this.EndTime = Time;

                this.UpdateResistedAttackNumber(IsSkill);
            }

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

        private void UpdateAttackNumber(bool IsSkill)
        {
            this.AttackNumber += 1;
            this.AttackNumberLabel.Text = this.AttackNumber.ToString("#,0");

            if (IsSkill)
            {
                this.SkillAttackNumber += 1;
                this.SkillAttackNumberLabel.Text = this.SkillAttackNumber.ToString("#,0");
            }
            else
            {
                this.SimpleAttackNumber += 1;
                this.SimpleAttackNumberLabel.Text = this.SimpleAttackNumber.ToString("#,0");
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

        private void UpdateCriticalHit(bool IsSkill)
        {
            if (IsSkill)
            {
                this.SkillCriticalNumber += 1;
            }
            else
            {
                this.SimpleCriticalNumber += 1;
            }
        }

        private void UpdateCriticalHitLabel()
        {
            if (this.SkillCriticalNumber > 0)
            {
                this.SkillCriticalNumberLabel.Text = this.SkillCriticalNumber.ToString("#,0") + " (" + (1.0 * this.SkillCriticalNumber * 100/ this.AttackNumber).ToString("F2") + "%)";
            }

            if (this.SimpleCriticalNumber > 0)
            {
                this.SimpleCriticalNumberLabel.Text = this.SimpleCriticalNumber.ToString("#,0") + " (" + (1.0 * this.SimpleCriticalNumber * 100 / this.AttackNumber).ToString("F2") + "%)";
            }
        }

        private void UpdateEvadeAttackNumber()
        {
            this.EvadeAttackNumber += 1;
            this.EvadeAttackNumberLabel.Text = this.EvadeAttackNumber.ToString("#,0");
        }

        private void UpdateEvadedAttackNumber(bool IsSkill)
        {
            this.EvadedAttackNumber += 1;
            this.UpdateSeconds();
            this.UpdateAttackNumber(IsSkill);
            this.UpdateDamageParSecond();
            this.UpdateDamageParAttackNumber();
            this.UpdateAttackNumberParSecond();
            this.EvadedAttackNumberLabel.Text = this.EvadedAttackNumber.ToString("#,0") + " (" + Math.Round((1.0 * this.EvadedAttackNumber * 100 / this.AttackNumber), 0, MidpointRounding.AwayFromZero) + "%)";
        }

        private void UpdateResistAttackNumber()
        {
            this.ResistAttackNumber += 1;
            this.ResistAttackNumberLabel.Text = this.EvadeAttackNumber.ToString("#,0");
        }

        private void UpdateResistedAttackNumber(bool IsSkill)
        {
            this.ResistedAttackNumber += 1;
            this.UpdateSeconds();
            this.UpdateAttackNumber(IsSkill);
            this.UpdateDamageParSecond();
            this.UpdateDamageParAttackNumber();
            this.UpdateAttackNumberParSecond();
            this.ResistedAttackNumberLabel.Text = this.ResistedAttackNumber.ToString("#,0") + " (" + Math.Round((1.0 * this.ResistedAttackNumber * 100 / this.AttackNumber), 0, MidpointRounding.AwayFromZero) + "%)";
        }

        public string GetResult()
        {
            string Result = "";

            if (String.IsNullOrEmpty(this.CharacterNameTextBox.Text))
            {
                return Result;
            }

            Result += "■" + this.CharacterNameTextBox.Text + Environment.NewLine;
            Result += "総ダメージ：\t\t\t" + this.DamageLabel.Text + " (" + this.DamageParTotalDamageLabel.Text + ")" + Environment.NewLine; ;
            Result += "攻撃時間：\t\t\t" + this.SecondLabel.Text + Environment.NewLine;
            Result += "攻撃回数：\t\t\t" + this.AttackNumberLabel.Text + Environment.NewLine;
            Result += "秒間平均ダメージ：\t\t" + this.DamageParSecondLabel.Text + Environment.NewLine;
            Result += "回数平均ダメージ：\t\t" + this.DamageParAttackNumberLabel.Text + Environment.NewLine;
            Result += "最大ダメージ：\t\t\t" + this.MaxDamageLabel.Text + Environment.NewLine;
            Result += "最小ダメージ：\t\t\t" + this.MinDamageLabel.Text + Environment.NewLine;
            Result += "攻撃回数(スキル)：\t\t" + this.SkillAttackNumberLabel.Text + Environment.NewLine;
            Result += "攻撃回数(通常攻撃)：\t\t" + this.SimpleAttackNumberLabel.Text + Environment.NewLine;
            Result += "クリティカル回数(スキル)：\t" + this.SkillCriticalNumberLabel.Text + Environment.NewLine;
            Result += "クリティカル回数(通常攻撃)：\t" + this.SimpleCriticalNumberLabel.Text + Environment.NewLine;
            Result += "攻撃回数／攻撃時間：\t\t" + this.AttackNumberParSecondLabel.Text + Environment.NewLine;
            Result += "回避した回数：\t\t\t" + this.EvadeAttackNumberLabel.Text + Environment.NewLine;
            Result += "回避された回数：\t\t" + this.EvadedAttackNumberLabel.Text + Environment.NewLine;
            Result += "抵抗した回数：\t\t\t" + this.ResistAttackNumberLabel.Text + Environment.NewLine;
            Result += "抵抗された回数：\t\t" + this.ResistedAttackNumberLabel.Text;

            return Result;
        }

        public string GetResultForSkype()
        {
            string Result = "";

            if (String.IsNullOrEmpty(this.CharacterNameTextBox.Text))
            {
                return Result;
            }

            Result += "■" + this.CharacterNameTextBox.Text + Environment.NewLine;
            Result += "総ダメージ：\t\t\t" + this.DamageLabel.Text + " (" + this.DamageParTotalDamageLabel.Text + ")" + Environment.NewLine; ;
            Result += "攻撃時間：\t\t\t" + this.SecondLabel.Text + Environment.NewLine;
            Result += "攻撃回数：\t\t\t" + this.AttackNumberLabel.Text + Environment.NewLine;
            Result += "秒間平均ダメージ：\t\t\t" + this.DamageParSecondLabel.Text + Environment.NewLine;
            Result += "回数平均ダメージ：\t\t\t" + this.DamageParAttackNumberLabel.Text + Environment.NewLine;
            Result += "最大ダメージ：\t\t\t" + this.MaxDamageLabel.Text + Environment.NewLine;
            Result += "最小ダメージ：\t\t\t" + this.MinDamageLabel.Text + Environment.NewLine;
            Result += "攻撃回数(スキル)：\t\t\t" + this.SkillAttackNumberLabel.Text + Environment.NewLine;
            Result += "攻撃回数(通常攻撃)：\t\t" + this.SimpleAttackNumberLabel.Text + Environment.NewLine;
            Result += "クリティカル回数(スキル)：\t\t" + this.SkillCriticalNumberLabel.Text + Environment.NewLine;
            Result += "クリティカル回数(通常攻撃)：\t\t" + this.SimpleCriticalNumberLabel.Text + Environment.NewLine;
            Result += "攻撃回数／攻撃時間：\t\t" + this.AttackNumberParSecondLabel.Text + Environment.NewLine;
            Result += "回避した回数：\t\t\t" + this.EvadeAttackNumberLabel.Text + Environment.NewLine;
            Result += "回避された回数：\t\t\t" + this.EvadedAttackNumberLabel.Text + Environment.NewLine;
            Result += "抵抗した回数：\t\t\t" + this.ResistAttackNumberLabel.Text + Environment.NewLine;
            Result += "抵抗された回数：\t\t\t" + this.ResistedAttackNumberLabel.Text;

            return Result;
        }
    }
}
