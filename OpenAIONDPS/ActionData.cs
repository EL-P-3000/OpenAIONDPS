using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenAIONDPS
{
    public class ActionData
    {
        public string LogText = "";
        public string SourceName = "";
        public string SkillName = "";
        public string EffectSkillName = "";
        public string TargetName = "";
        public long Damage = 0;
        public long HealingAmount = 0;
        public bool IsSkill = true;
        public bool IsCriticalHit = false;
        public DateTime Time = DateTime.Now;
        public bool IsMCSkill = false;
    }
}
