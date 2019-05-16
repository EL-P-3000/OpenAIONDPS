using System;
using System.Collections.Generic;

namespace OpenAIONDPS
{
    public static class AION
    {
        public class LogPattern
        {
            private const string SkillNamePattern                              = @"(?<SkillName>[\p{IsKatakana}：\s\(\)0-9]+)";
            private const string SkillNameEffectedPattern                      = @"(?<SkillName2>.+)";
            private const string SkillNameReplacedSkillNamePattern             = @"(?<SkillName>[[[SkillName]]])";
            private const string SkillNameReplacedSkillName2Pattern            = @"(?<SkillName2>[[[SkillName]]])";
            private const string SkillNameOrSimpleAttackPattern                = @"(?<SkillName>([\p{IsKatakana}：\s\(\)]+|攻撃))";
            private const string SkillNameGiganticSoldierPattern               = @"(?<SkillName>スウィープ|マグネティック\sディスチャージ|エネルギー\sディスチャージ)";
            private const string SkillNameCannonPattern                        = @"(?<SkillName>殺龍砲)";
            private const string SourceNamePattern                             = @"(?<SourceName>[^、]+)";
            private const string SourceNameReplacedMemberNamePattern           = @"(?<SourceName>[[[MemberName]]])";
            private const string TargetNamePattern                             = @"(?<TargetName>[^、]+)";
            private const string TargetNameReplacedMemberNamePattern           = @"(?<TargetName>[[[MemberName]]])";
            private const string DamagePattern                                 = @"(?<Damage>[0-9,]+)";
            private const string HealingAmountPattern                          = @"(?<HealingAmount>[0-9,]+)";
            private const string EXPPattern                                    = @"(?<EXP>[0-9,]+)";

            /* 共通 */

            /// <summary>
            /// 行のパターン
            /// </summary>
            public const string LinePattern = @"^(20[0-9][0-9]\.[0-9][0-9]\.[0-9][0-9]\s[0-9][0-9]:[0-9][0-9]:[0-9][0-9])\s:\s(.*)\s*$";

            /* 攻撃 */
            public const string AttackDamageToPattern = "ダメージを与えました。$";

            /// <summary>
            /// クリティカルヒットのパターン
            /// </summary>
            public const string AttackCriticalHitPattern = @"^クリティカルヒット！\s(.*)$";

            /* 通常攻撃 */

            /// <summary>
            ///  通常攻撃のダメージのパターン(自分)
            /// </summary>
            public const string AttackSimpleDamageWithoutSourceNamePattern = "^"+ TargetNamePattern + "に" + DamagePattern + "の(致命的な|)ダメージを与えました。";

            /// <summary>
            /// 通常攻撃のダメージのパターン(他人)(要SourceNameの置換)
            /// </summary>
            public const string AttackSimpleDamageWithSourceNameReplacedMemberNamePattern = "^" + SourceNameReplacedMemberNamePattern + "が" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// 通常攻撃のダメージのパターン(他人)
            /// </summary>
            public const string AttackSimpleDamageWithSourceNamePattern = "^" + SourceNamePattern + "が" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// 通常攻撃のダメージのパターン(サモン)
            /// </summary>
            public const string AttackSimpleDamageWithSummonPattern = "^" + SkillNameReplacedSkillNamePattern + "が" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /* スキル攻撃 */

            /// <summary>
            /// スキル攻撃のダメージのパターン(自分)
            /// </summary>
            public const string AttackSkillDamageWithoutSourceNamePattern = "^" + SkillNamePattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// スキル攻撃のダメージのパターン(他人)(要SourceNameの置換)
            /// </summary>
            public const string AttackSkillDamageWithSourceNameReplacedMemberNamePattern = "^" + SourceNameReplacedMemberNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// スキル攻撃のダメージのパターン(他人)
            /// </summary>
            public const string AttackSkillDamageWithSourceNamePattern = "^" + SourceNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// スキル攻撃のダメージのパターン(サモン)(要スキル名置換)
            /// </summary>
            public const string AttackSkillDamageWithSummonPattern = "^" + SkillNameReplacedSkillNamePattern + "が使用した" + SkillNameEffectedPattern + "(\\sエフェクト|)の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// スキル攻撃(固定スキル)のダメージのパターン(サモン)(要スキル名置換)
            /// </summary>
            public const string AttackSkillDamageFixedSkillWithSummonPattern = "^" + SkillNameReplacedSkillNamePattern + "が使用した" + SkillNameReplacedSkillName2Pattern + "(\\sエフェクト|)の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /* ドットスキル攻撃 */

            /// <summary>
            /// ドットスキル攻撃のエフェクトのパターン(自分)
            /// </summary>
            public const string AttackSkillDotEffectWithoutSourceNamePattern = "^" + SkillNamePattern + "の効果により、" + TargetNamePattern + "(にダメージを与え続けました。|が出血状態になりました。)";

            /// <summary>
            /// ドットスキル攻撃のエフェクトのパターン(自分)(メロディ)
            /// </summary>
            public const string AttackSkillDotEffectMelodyWithoutSourceNamePattern = "^" + SkillNamePattern + "を使い、" + TargetNamePattern + "が" + SkillNameEffectedPattern + "効果を受けました。";

            /// <summary>
            /// ドットスキル攻撃のエフェクトのパターン(他人)
            /// </summary>
            public const string AttackSkillDotEffectWithSourceNamePattern = "^" + SourceNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "(はダメージを受け続けました。|は出血状態になりました。)";

            /// <summary>
            /// ドットスキル攻撃のエフェクトのパターン(他人)(メロディ)
            /// </summary>
            public const string AttackSkillDotEffectMelodyWithSourceNamePattern = "^" + SourceNamePattern + "は" + SkillNamePattern + "の効果により、" + TargetNamePattern + "に" + SkillNameEffectedPattern + "効果を与えました。";

            /// <summary>
            /// ドットスキル攻撃のダメージのパターン
            /// </summary>
            public const string AttackSkillDotDamagePattern = "^" + TargetNamePattern + "(は|が)" + SkillNameReplacedSkillNamePattern + @"(\sエフェクト|)の効果により、" + DamagePattern + "の(出血|)ダメージを受けました。";

            /* バフ消去スキル攻撃 */

            public const string AttackSkillReleaseBuffDamageWithoutSourceNamePattern = "^" + SkillNamePattern + "の効果により、" + TargetNamePattern + "が" + DamagePattern + "のダメージを受け、.+が解除されました。";

            public const string AttackSkillReleaseBuffDamageWithSourceNamePattern = "^" + SourceNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "が" + DamagePattern + "のダメージを受け、.+が解除されました。";

            /* デバフスキル攻撃 */

            public const string AttackSkillDebuffDamageWithoutSourceNamePattern = "^" + SkillNamePattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与え、" + SkillNameEffectedPattern + "効果が生じました。";

            public const string AttackSkillDebuffDamageWithSourceNamePattern = "^" + SourceNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージ与え、" + SkillNameEffectedPattern + "効果を得ました。";

            /* ディレイダメージスキル攻撃 */

            /// <summary>
            /// ディレイダメージスキル攻撃のパターン(自分)
            /// </summary>
            public const string AttackSkillDelayDamageWithoutSourceNamePattern = "^" + SkillNameReplacedSkillNamePattern + "の効果により、" + TargetNamePattern + "に" + SkillNameEffectedPattern + "効果が生じました。";

            /// <summary>
            /// ディレイダメージスキル攻撃のパターン(他人)
            /// </summary>
            public const string AttackSkillDelayDamageWithSourceNamePattern = "^" + SourceNamePattern + "が使用した" + SkillNameReplacedSkillNamePattern + "の効果により、" + TargetNamePattern + "に" + SkillNameEffectedPattern + "効果が生じました。";

            /// <summary>
            /// ディレイダメージスキル攻撃のダメージのパターン
            /// </summary>
            public const string AttackSkillDelayDamageDamagePattern = "^" + TargetNamePattern + "は" + SkillNameReplacedSkillNamePattern + "の効果により、" + DamagePattern + "のダメージを受けました。";

            /* エフェクトダメージスキル攻撃 */

            /// <summary>
            /// エフェクトダメージスキル攻撃のダメージのパターン
            /// </summary>
            public const string AttackSkillEffectDamageDamagePattern = "^" + TargetNamePattern + "は" + SkillNameReplacedSkillNamePattern + "の効果により、" + DamagePattern + "のダメージを受けました。";

            /* 反射攻撃 */

            /// <summary>
            /// 反射攻撃のダメージのパターン(自分)
            /// </summary>
            public const string AttackReflectionDamageWithoutSourceNamePattern = "^攻撃を反射し、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// 反射攻撃のダメージのパターン(他人)
            /// </summary>
            public const string AttackReflectionDamageWithSourceNamePattern = "^" + SourceNamePattern + "が攻撃を反射し、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// 反射攻撃のダメージのパターン(ディシプリン エネルギー)
            /// </summary>
            public const string AttackReflectionDamageWithDisciplineEnergyPattern = @"^ディシプリン\sエネルギーが攻撃を反射し、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /* 巨神兵 */

            /// <summary>
            /// セネクタ巨神兵のダメージのパターン(自分)
            /// </summary>
            public const string AttackGiganticSoldierWithoutSourceNamePattern = SkillNameGiganticSoldierPattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// セネクタ巨神兵のダメージのパターン(他人)
            /// </summary>
            public const string AttackGiganticSoldierWithSourceNamePattern = "^" + SourceNamePattern + "は" + SkillNameGiganticSoldierPattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            ///  殺龍砲
            /// </summary>
            public const string AttackCannonPattern = @"^破壊されかかった(天族|魔族)\s殺龍砲が使用した" + SkillNameCannonPattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /* 回避/抵抗 */

            /// <summary>
            /// 回避/抵抗したパターン(自分)
            /// </summary>
            public const string EvasionResistanceWithoutSourceNamePattern = "^" + TargetNamePattern + "の" + SkillNameOrSimpleAttackPattern + "(を回避|に抵抗)しました。";

            /// <summary>
            /// 回避/抵抗されたパターン(自分)
            /// </summary>
            public const string EvadedResistedWithoutTargetNamePattern = "^" + SourceNamePattern + "が" + SkillNameOrSimpleAttackPattern + "(を回避|に抵抗)しました。";

            /// <summary>
            /// 回避/抵抗のパターン(他人)
            /// </summary>
            public const string EvasionResistanceWithOthersPattern = "^" + SourceNamePattern + "が" + TargetNamePattern + "の" + SkillNameOrSimpleAttackPattern + "(を回避|に抵抗)しました。";

            public const string EvasionResistanceWithSourceNamePattern = "^" + SourceNameReplacedMemberNamePattern + "が" + TargetNamePattern + "の" + SkillNameOrSimpleAttackPattern + "(を回避|に抵抗)しました。";
            public const string EvadedResistedWithTargetNamePattern = "^" + SourceNamePattern + "が" + TargetNameReplacedMemberNamePattern + "の" + SkillNameOrSimpleAttackPattern + "(を回避|に抵抗)しました。";

            /* 回復 */

            /// <summary>
            /// 回復の共通のパターン
            /// </summary>
            public const string HealCommonPattern = "(回復しました。|詠唱速度が変更されました。|持続的なHP回復効果を得ました。|条件付き回復状態になりました。)$";

            // 回復(自分→)
            public const string HealSkillWithoutSourceNamePattern = "^" + SkillNamePattern + "の効果により、(" + HealingAmountPattern + "のHPが回復しました。|詠唱速度が変更されました。|持続的なHP回復効果を得ました。|HPが" + HealingAmountPattern + "回復しました。)";
            public const string HealSkillNextLineWithoutSourceNamePattern = "^" + SkillNamePattern + "の効果により、" + TargetNamePattern + "(のHPが" + HealingAmountPattern + "回復しました。|の詠唱速度が変更されました。|[はが]持続的なHP回復効果を得ました。)";

            // 回復(他人→)
            public const string HealSkillWithSourceNamePattern = "^" + SourceNameReplacedMemberNamePattern + "は" + SkillNamePattern + "の効果により、(" + HealingAmountPattern + "のHPが回復しました。|詠唱速度が変更されました。|持続的なHP回復効果を得ました。)";
            public const string HealSkillNextLineSelfWithSourceNamePattern = "^" + SourceNameReplacedMemberNamePattern + "が使用した" + SkillNamePattern + "の効果により、(" + HealingAmountPattern + "のHPが回復しました。|詠唱速度が変更されました。|持続的なHP回復効果を得ました。)";
            public const string HealSkillNextLineWithSourceNamePattern = "^" + SourceNameReplacedMemberNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "(のHPが" + HealingAmountPattern + "回復しました。|の詠唱速度が変更されました。|は持続的なHP回復効果を得ました。)";

            // 持続回復／ディレイ回復
            public const string HealSkillHotWithoutTargetNamePattern = "^" + SkillNamePattern + "の効果により、HPが" + HealingAmountPattern + "回復しました。";
            public const string HealSkillHotWithTargetNamePattern = "^" + TargetNamePattern + "は" + SkillNamePattern + "の効果により、" + HealingAmountPattern + "のHPが回復しました。";

            // サモン
            public const string HealSkillSummonWithoutTargetNamePattern = "^" + SkillNameReplacedSkillNamePattern + "が使用した" + SkillNameReplacedSkillName2Pattern + "の効果により、" + HealingAmountPattern + "のHPが回復しました。";
            public const string HealSkillSummonWithTargetNamePattern = "^" + SkillNameReplacedSkillNamePattern + "が使用した" + SkillNameReplacedSkillName2Pattern + "の効果により、" + TargetNamePattern + "のHPが" + HealingAmountPattern + "回復しました。";

            // ディレイ
            public const string HealSkillDelayHealSelfWithoutSourceNamePattern = "^" + SkillNamePattern + "の効果により、条件付き回復効果を得ました。";
            public const string HealSkillDelayHealWithoutSourceNamePattern = "^" + SkillNamePattern + "の効果により、" + TargetNamePattern + "が条件付き回復状態になりました。";
            public const string HealSkillDelayHealSelfWithSourceNamePattern = "^" + SourceNamePattern + "は" + SkillNamePattern + "の効果により、条件付き回復状態になりました。";
            public const string HealSkillDelayHealWithSourceNamePattern = "^" + SourceNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "は条件付き回復状態になりました。";

            // エフェクト
            // ※他のパターンと被るため、一番上で判定
            public const string HealSkillEffectWithoutTargetNamePattern = "^" + SkillNamePattern + "\\sエフェクトの効果により、HPが" + HealingAmountPattern + "回復しました。";
            public const string HealSkillEffectWithTargetNamePattern = "^" + TargetNamePattern + "は" + SkillNamePattern + "\\sエフェクトの効果により、" + HealingAmountPattern + "のHPが回復しました。";

            // インビンサビリティ マントラ
            public const string HealSkillInvincibilityMantraWithoutTargetName = "^インビンサビリティ マントラの効果により、MPが" + HealingAmountPattern + "回復しました。";
            public const string HealSkillInvincibilityMantraWithTargetName = "^" + TargetNamePattern + "はインビンサビリティ マントラの効果により、MPが" + HealingAmountPattern + "回復しました。";

            // ポーション
            public const string HealPotionWithoutSourceNamePattern = "^HPが" + HealingAmountPattern + "回復しました。";
            public const string HealPotionWithoutSourceNamePattern2 = "^(回復|治癒)のポーションの効果により、HPが" + HealingAmountPattern + "回復しました。";
            public const string HealPotionWithSourceNamePattern = "^" + SourceNamePattern + "は(回復|治癒)のポーションの効果により、" + HealingAmountPattern + "のHPが回復しました。";

            /* バフ */

            /// <summary>
            /// ゲイル スペル(使用者(自分)→自分)
            /// </summary>
            public const string BuffSkillGaleOwnToOwn = "^ゲイル スペルの効果により、攻撃速度強化効果を得ました。";

            /// <summary>
            /// ゲイル スペル(使用者(自分)→他人)
            /// </summary>
            public const string BuffSkillGaleOwnToOther = "^ゲイル スペルの効果により、" + TargetNamePattern + "が攻撃速度強化効果を得ました。";

            /// <summary>
            /// ゲイル スペル(使用者(他人)→自分)
            /// </summary>
            public const string BuffSkillGaleOtherToOwn = "^" + SourceNamePattern + "が使用したゲイル スペルの効果により、攻撃速度強化効果を得ました。";

            /// <summary>
            /// ゲイル スペル(使用者(他人)→使用者(他人))
            /// </summary>
            public const string BuffSkillGaleOtherToOther = "^" + SourceNamePattern + "はゲイル スペルの効果により、攻撃速度強化状態になりました。";

            /// <summary>
            /// ゲイル スペル(使用者→他人)
            /// </summary>
            public const string BuffSkillGaleOtherToOthers = "^" + SourceNamePattern + "が使用したゲイル スペルの効果により、" + TargetNamePattern + "が攻撃速度強化状態になりました。";

            /// <summary>
            /// デストラクション ファンタジア(使用者(自分)→自分)
            /// </summary>
            public const string BuffSkillDestructionFantasiaOwnToOwn = "^ディストラクション ファンタジアの効果により、移動速度,攻撃速度強化効果を得ました。";

            /// <summary>
            /// デストラクション ファンタジア(使用者(自分)→他人)
            /// </summary>
            public const string BuffSkillDestructionFantasiaOwnToOther = "^ディストラクション ファンタジアの効果により、" + TargetNamePattern + "が移動速度,攻撃速度強化効果を得ました。";

            /// <summary>
            /// デストラクション ファンタジア(使用者→自分)
            /// </summary>
            public const string BuffSkillDestructionFantasiaOtherToOwn = "^" + SourceNamePattern + "が使用したディストラクション ファンタジアの効果により、移動速度,攻撃速度強化効果を得ました。";

            /// <summary>
            /// デストラクション ファンタジア(使用者(他人)→使用者(他人))
            /// </summary>
            public const string BuffSkillDestructionFantasiaOtherToOther = "^" + SourceNamePattern + "はディストラクション ファンタジアの効果により、移動速度,攻撃速度強化状態になりました。";

            /// <summary>
            /// デストラクション ファンタジア(使用者→他人)
            /// </summary>
            public const string BuffSkillDestructionFantasiaOtherToOthers = "^" + SourceNamePattern + "が使用したディストラクション ファンタジアの効果により、" + TargetNamePattern + "が移動速度,攻撃速度強化状態になりました。";

            // 計測停止条件
            public const string StopCalcConditionPattern = @"から経験値を" + EXPPattern + "獲得しました。.*";
        }

        public enum JobType { None, Sword, Shield, Shadow, Bow, Spell, Spirit, Cure, Chant, Bullet, Gia, Melody, Vivid };

        public enum AttackSkillType { Normal, DelayDamage, Dot, Summon, EffectDamage, Others };

        public enum HealSkillType { Normal, Area, AreaNoSource, Hot, AreaHot, Delay, Summon, EffectHeal, Others };

        public static string GetJobName(JobType Type)
        {
            switch (Type)
            {
                case JobType.Sword:
                    return "ソード";
                case JobType.Shield:
                    return "シールド";
                case JobType.Shadow:
                    return "シャドウ";
                case JobType.Bow:
                    return "ボウ";
                case JobType.Spell:
                    return "スペル";
                case JobType.Spirit:
                    return "スピリット";
                case JobType.Cure:
                    return "キュア";
                case JobType.Chant:
                    return "チャント";
                case JobType.Bullet:
                    return "バレット";
                case JobType.Gia:
                    return "ギア";
                case JobType.Melody:
                    return "メロディ";
                case JobType.Vivid:
                    return "ビビッド";
                case JobType.None:
                    return "－";
                default:
                    return "";
            }
        }

        public static JobType GetJobType(string Name)
        {
            if (String.IsNullOrEmpty(Name))
            {
                return JobType.None;
            }
            else if (Name.Equals("ソード"))
            {
                return JobType.Sword;
            }
            else if (Name.Equals("シールド"))
            {
                return JobType.Shield;
            }
            else if (Name.Equals("シャドウ"))
            {
                return JobType.Shadow;
            }
            else if (Name.Equals("ボウ"))
            {
                return JobType.Bow;
            }
            else if (Name.Equals("スペル"))
            {
                return JobType.Spell;
            }
            else if (Name.Equals("スピリット"))
            {
                return JobType.Spirit;
            }
            else if (Name.Equals("キュア"))
            {
                return JobType.Cure;
            }
            else if (Name.Equals("チャント"))
            {
                return JobType.Chant;
            }
            else if (Name.Equals("バレット"))
            {
                return JobType.Bullet;
            }
            else if (Name.Equals("ギア"))
            {
                return JobType.Gia;
            }
            else if (Name.Equals("メロディ"))
            {
                return JobType.Melody;
            }
            else if (Name.Equals("ビビッド"))
            {
                return JobType.Vivid;
            }
            else if (Name.Equals("－"))
            {
                return JobType.None;
            }
            else
            {
                return JobType.None;
            }
        }

        public class JobSet
        {
            private JobType _Type = JobType.None;
            private string _Name = "";

            public JobSet(JobType Type, string Name)
            {
                this._Type = Type;
                this._Name = Name;
            }

            public JobType Type
            {
                get
                {
                    return this._Type;
                }
            }

            public string Name
            {
                get
                {
                    return this._Name;
                }
            }

        }

        public class AttackSkill
        {
            public string Name = "";
            public JobType Job = JobType.None;
            public AttackSkillType SkillType = AttackSkillType.Others;
            private bool IsStartWithCritical = false;
            private bool IsStartWithoutCritical = false;

            public AttackSkill(string Name, JobType Job, AttackSkillType SkillType)
            {
                this.Name = Name;
                this.Job = Job;
                this.SkillType = SkillType;
            }

            public long Damage { get; set; } = 0;

            public long AttackNumber { get; set; } = 0;

            public long MaxDamageWithCritical { get; set; } = 0;

            public long MinDamageWithCritical { get; set; } = 0;

            public long MaxDamageWithoutCritical { get; set; } = 0;

            public long MinDamageWithoutCritical { get; set; } = 0;

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
        }

        public class HealSkill
        {
            public string Name = "";
            public JobType Job = JobType.None;
            public HealSkillType SkillType = HealSkillType.Others;
            private bool IsStart = false;

            public HealSkill(string Name, JobType Job, HealSkillType SkillType)
            {
                this.Name = Name;
                this.Job = Job;
                this.SkillType = SkillType;
            }

            public long HealingAmount { get; set; } = 0;

            public long HealingNumber { get; set; } = 0;

            public long MaxHealingAmount { get; set; } = 0;

            //public long MinHealingAmount { get; set; } = 0;

            public void AddHeal(long HealingAmount)
            {
                this.UpdateHealingAmount(HealingAmount);
                this.UpdateHealNumber();
                this.UpdateMaxHealingAmount(HealingAmount);
                //this.UpdateMinHealingAmount(HealingAmount);

                this.IsStart = true;
            }

            private void UpdateHealingAmount(long HealingAmount)
            {
                this.HealingAmount += HealingAmount;
            }

            private void UpdateHealNumber()
            {
                this.HealingNumber += 1;
            }

            private void UpdateMaxHealingAmount(long HealingAmount)
            {
                if (!this.IsStart || this.MaxHealingAmount < HealingAmount)
                {
                    this.MaxHealingAmount = HealingAmount;
                }
            }

            /*
            private void UpdateMinHealingAmount(long HealingAmount)
            {
                if (!this.IsStart || this.MinHealingAmount > HealingAmount)
                {
                    this.MinHealingAmount = HealingAmount;
                }
            }
            */
        }

        public static readonly Dictionary<string, AttackSkill> AttackSkillList = GetAttackSkillList();

        private static Dictionary<string, AttackSkill> GetAttackSkillList()
        {
            Dictionary<string, AttackSkill> _AttackSkillList = new Dictionary<string, AttackSkill>();
            string SkillName = "";

            /**************************************************************************************************************************************/
            /* シールド ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // エフェクトダメージ
            SkillName = "アップセット エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Shield, AttackSkillType.EffectDamage));


            /**************************************************************************************************************************************/
            /* ソード *****************************************************************************************************************************/
            /**************************************************************************************************************************************/


            /**************************************************************************************************************************************/
            /* シャドウ ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "エクターミネーション";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Shadow, AttackSkillType.Dot));
            SkillName = "デッドリー ポイズン サドン アタック";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Shadow, AttackSkillType.Dot));
            SkillName = "バック ダメージ";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Shadow, AttackSkillType.Dot));

            // エフェクトダメージ
            SkillName = "エクスプローシブ アプライ エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Shadow, AttackSkillType.EffectDamage));
            SkillName = "スティレット コミットメント効果";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Shadow, AttackSkillType.EffectDamage));
            SkillName = "ドラスティック ポイズン エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Shadow, AttackSkillType.EffectDamage));


            /**************************************************************************************************************************************/
            /* ボウ *******************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // エフェクトダメージ
            SkillName = "ヴェンジフル トラップ エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Bow, AttackSkillType.EffectDamage));
            SkillName = "ダスト トラップ エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Bow, AttackSkillType.EffectDamage));
            SkillName = "バンプ トラップ エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Bow, AttackSkillType.EffectDamage));
            /* 要調査 */
            SkillName = "バースト トラップ エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Bow, AttackSkillType.EffectDamage));
            SkillName = "クレアポイアンス トラップ エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Bow, AttackSkillType.EffectDamage));


            /**************************************************************************************************************************************/
            /* スペル *****************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ディレイダメージ
            SkillName = "ボルカニック ブローアウト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spell, AttackSkillType.DelayDamage));
            SkillName = "ボルカニック ビック イラプション";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spell, AttackSkillType.DelayDamage));
            SkillName = "エンハンスト ボルカニック ビック イラプション";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spell, AttackSkillType.DelayDamage));

            // ドット
            SkillName = "バーン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spell, AttackSkillType.Dot));
            SkillName = "ヒート サモン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spell, AttackSkillType.Dot));

            // サモン
            SkillName = "タイフーン サモン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spell, AttackSkillType.Summon));


            /**************************************************************************************************************************************/
            /* スピリット *************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "イロージョン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "ワイドエリア イロージョン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "アース チェーン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "マジック エクスプロージョン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "アンガー サイクロン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "カース クラウド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "サモン：ウインド エネルギー";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "サモン：サイクロン エネルギー";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "ヘル ペイン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "マジック エクスプロージョン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));
            SkillName = "マジック バックドラフト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Dot));

            // サモン
            SkillName = "ファイア スピリット";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Summon));
            SkillName = "ウォーター スピリット";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Summon));
            SkillName = "ウインド スピリット";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Summon));
            SkillName = "グラウンド スピリット";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Summon));
            SkillName = "タイフーン スピリット";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Spirit, AttackSkillType.Summon));


            /**************************************************************************************************************************************/
            /* キュア *****************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "パニッシュ";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Cure, AttackSkillType.Dot));
            SkillName = "アース アンガー";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Cure, AttackSkillType.Dot));
            SkillName = "ウイークン ブランド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Cure, AttackSkillType.Dot));
            SkillName = "ペイン サクセッシブ";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Cure, AttackSkillType.Dot));
            SkillName = "ディストラクション ボイス";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Cure, AttackSkillType.Dot));

            // サモン
            SkillName = "ディシプリン エネルギー";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Cure, AttackSkillType.Summon));
            SkillName = "ノーブル エネルギー";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Cure, AttackSkillType.Summon));


            /**************************************************************************************************************************************/
            /* チャント ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "マウンテン クラッシュ";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Chant, AttackSkillType.Dot));

            // エフェクトダメージ
            SkillName = "ウインド ブレス";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Chant, AttackSkillType.EffectDamage));
            SkillName = "アース プロミス エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Chant, AttackSkillType.EffectDamage));
            SkillName = "アース コミットメント エフェクト";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Chant, AttackSkillType.EffectDamage));


            /**************************************************************************************************************************************/
            /* バレット ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "ホーム ペネトレート";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Bullet, AttackSkillType.Dot));


            /**************************************************************************************************************************************/
            /* ギア *******************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ディレイダメージ
            SkillName = "チャージ ファイヤ";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Gia, AttackSkillType.DelayDamage));


            /**************************************************************************************************************************************/
            /* メロディ ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "ダメージ エコー";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Melody, AttackSkillType.Dot));
            SkillName = "シャープ フリカティブ";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Melody, AttackSkillType.Dot));
            SkillName = "モスキー ラプソディ";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Melody, AttackSkillType.Dot));


            /**************************************************************************************************************************************/
            /* ビビッド ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "タイム ボム";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.Dot));


            // エフェクトダメージ
            SkillName = "アンガー バインド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "ラス バインド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "インデュランス バインド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "タイム バインド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "グラビティ バインド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "タイムズ リストレイン";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "アニマ バインド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "ライトニング バインド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "スケッチャー ボム";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "グラビティ スケッチャー";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));
            SkillName = "ディシプリン バインド";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.Vivid, AttackSkillType.EffectDamage));


            /**************************************************************************************************************************************/
            /* エレスギガル ***********************************************************************************************************************/
            /**************************************************************************************************************************************/
            // 破壊されかかった(天族|魔族) 殺龍砲が使用した殺龍砲
            SkillName = "殺龍砲";
            _AttackSkillList.Add(SkillName, new AttackSkill(SkillName, JobType.None, AttackSkillType.EffectDamage));

            return _AttackSkillList;
        }

        public static readonly Dictionary<string, HealSkill> HealSkillList = GetHealSkillList();

        private static Dictionary<string, HealSkill> GetHealSkillList()
        {
            Dictionary<string, HealSkill> HealSkillList = new Dictionary<string, HealSkill>();
            string SkillName = "";

            /**************************************************************************************************************************************/
            /* キュア *****************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // エリア
            SkillName = "ヒール ウェーブズ";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Cure, HealSkillType.Area));
            // 持続
            SkillName = "リバース スプレンダー";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Cure, HealSkillType.Hot));
            // エリア＆持続
            SkillName = "リカバリー スプレンダー";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Cure, HealSkillType.AreaHot));
            SkillName = "ピュリフィケーション ウェーブ";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Cure, HealSkillType.AreaHot));
            SkillName = "アクウィット";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Cure, HealSkillType.AreaHot));
            // サモン
            SkillName = "ヒール エネルギー";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Cure, HealSkillType.Summon));
            // ディレイ
            SkillName = "サルヴェーション ハンド";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Cure, HealSkillType.Delay));
            SkillName = "サルヴェーション スプレンダー";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Cure, HealSkillType.Delay));


            /**************************************************************************************************************************************/
            /* チャント ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // 持続
            SkillName = "リカバリー スペル";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Chant, HealSkillType.Hot));
            // エリア＆持続
            SkillName = "ブロック カーテン";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Chant, HealSkillType.AreaHot));
            SkillName = "ライフ スペル";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Chant, HealSkillType.AreaHot));
            SkillName = "リカバリー プロテクト スペル";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Chant, HealSkillType.AreaHot));
            // エフェクト
            SkillName = "スプリント マントラ";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Chant, HealSkillType.EffectHeal));

            // アブソーブ スペル


            /**************************************************************************************************************************************/
            /* メロディ ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // 持続
            SkillName = "リバース メロディ";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Melody, HealSkillType.Hot));
            SkillName = "インプレッシブ メロディ";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Melody, HealSkillType.Hot));
            // エリア
            SkillName = "ソフト ハイハウリング";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Melody, HealSkillType.Area));
            SkillName = "スノウフラワー プロテクト チューン";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Melody, HealSkillType.Area));
            SkillName = "エグゾナレイティブ メロディ";
            HealSkillList.Add(SkillName, new HealSkill(SkillName, JobType.Melody, HealSkillType.Area));


            return HealSkillList;
        }

        /// <summary>
        /// スキルがドットスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public static bool CheckAttackSkillTypeDot(string Name)
        {
            return CheckAttackSkillType(Name, AttackSkillType.Dot);
        }

        /// <summary>
        /// スキルがエフェクトダメージスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public static bool CheckAttackSkillTypeEffectDamage(string Name)
        {
            return CheckAttackSkillType(Name, AttackSkillType.EffectDamage);
        }

        /// <summary>
        /// スキルがサモンスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public static bool CheckAttackSkillTypeSummon(string Name)
        {
            return CheckAttackSkillType(Name, AttackSkillType.Summon);
        }

        /// <summary>
        /// スキルがディレイダメージスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public static bool CheckAttackSkillTypeDelayDamage(string Name)
        {
            return CheckAttackSkillType(Name, AttackSkillType.DelayDamage);
        }

        private static bool CheckAttackSkillType(string Name, AttackSkillType Type)
        {
            try
            {
                if (AttackSkillList.ContainsKey(Name) && AttackSkillList[Name].SkillType.Equals(Type))
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        public static bool CheckHealSkillType(string Name, HealSkillType Type)
        {
            try
            {
                if (HealSkillList.ContainsKey(Name) && HealSkillList[Name].SkillType.Equals(Type))
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
