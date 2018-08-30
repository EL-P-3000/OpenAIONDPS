using System;
using System.Collections.Generic;

namespace OpenAIONDPS
{
    public static class AION
    {
        public class LogPattern
        {
            private const string SkillNamePattern                              = @"(?<SkillName>[\p{IsKatakana}：\s]+)";
            private const string SkillNameEffectedPattern                      = @"(?<SkillName2>.+)";
            private const string SkillNameReplacedDotSkillNamePattern          = @"(?<SkillName>[[[DotSkillName]]])";
            private const string SkillNameReplacedSummonSkillNamePattern       = @"(?<SkillName>[[[SummonSkillName]]])";
            private const string SkillNameReplacedSummonSkillName2Pattern      = @"(?<SkillName2>[[[SummonSkillName]]])";
            private const string SkillNameReplacedDelayDamageSkillNamePattern  = @"(?<SkillName>[[[DelayDamageSkillName]]])";
            private const string SkillNameReplacedEffectDamageSkillNamePattern = @"(?<SkillName>[[[EffectDamageSkillName]]])";
            private const string SkillNameOrSimpleAttackPattern                = @"(?<SkillName>([\p{IsKatakana}：\s]+|攻撃))";
            private const string SourceNamePattern                             = @"(?<SourceName>[^、]+)";
            private const string SourceNameReplacedMemberNamePattern           = @"(?<SourceName>[[[MemberName]]])";
            private const string TargetNamePattern                             = @"(?<TargetName>[^、]+)";
            private const string TargetNameReplacedMemberNamePattern           = @"(?<TargetName>[[[MemberName]]])";
            private const string DamagePattern                                 = @"(?<Damage>[0-9,]+)";

            /* 共通 */

            /// <summary>
            /// 行のパターン
            /// </summary>
            public const string LinePattern = @"^(20[0-9][0-9]\.[0-9][0-9]\.[0-9][0-9]\s[0-9][0-9]:[0-9][0-9]:[0-9][0-9])\s:\s(.*。)";

            /// <summary>
            /// クリティカルヒットのパターン
            /// </summary>
            public const string AttackCriticalHitPattern = "^クリティカルヒット！(.*)$";

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
            public const string AttackSimpleDamageWithSummonPattern = "^" + SkillNameReplacedSummonSkillNamePattern + "が" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

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
            public const string AttackSkillDamageWithSourceNamePattern = @"^" + SourceNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// スキル攻撃のダメージのパターン(サモン)(要スキル名置換)
            /// </summary>
            public const string AttackSkillDamageWithSummonPattern = "^" + SkillNameReplacedSummonSkillNamePattern + "が使用した" + SkillNameEffectedPattern + "(\\sエフェクト|)の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// スキル攻撃(固定スキル)のダメージのパターン(サモン)(要スキル名置換)
            /// </summary>
            public const string AttackSkillDamageFixedSkillWithSummonPattern = "^" + SkillNameReplacedSummonSkillNamePattern + "が使用した" + SkillNameReplacedSummonSkillName2Pattern + "(\\sエフェクト|)の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /* ドットスキル攻撃 */

            /// <summary>
            /// ドットスキル攻撃のエフェクトのパターン(自分)
            /// </summary>
            public const string AttackSkillDotEffectWithoutSourceNamePattern = @"^" + SkillNamePattern + "の効果により、" + TargetNamePattern + "(にダメージを与え続けました。|が出血状態になりました。)";

            /// <summary>
            /// ドットスキル攻撃のエフェクトのパターン(他人)
            /// </summary>
            public const string AttackSkillDotEffectWithSourceNamePattern = @"^" + SourceNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "(はダメージを受け続けました。|は出血状態になりました。)";

            /// <summary>
            /// ドットスキル攻撃のダメージのパターン
            /// </summary>
            public const string AttackSkillDotDamagePattern = "^" + TargetNamePattern + "(は|が)" + SkillNameReplacedDotSkillNamePattern + "の効果により、" + DamagePattern + "の(出血|)ダメージを受けました。";

            /* バフ消去スキル攻撃 */

            public const string AttackSkillReleaseBuffDamageWithoutSourceNamePattern = @"^" + SkillNamePattern + "の効果により、" + TargetNamePattern + "が" + DamagePattern + "のダメージを受け、.+が解除されました。";

            public const string AttackSkillReleaseBuffDamageWithSourceNamePattern = @"^" + SourceNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "が" + DamagePattern + "のダメージを受け、.+が解除されました。";

            /* デバフスキル攻撃 */

            public const string AttackSkillDebuffDamageWithoutSourceNamePattern = @"^" + SkillNamePattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与え、" + SkillNameEffectedPattern + "効果が生じました。";

            public const string AttackSkillDebuffDamageWithSourceNamePattern = @"^" + SourceNamePattern + "が使用した" + SkillNamePattern + "の効果により、" + TargetNamePattern + "に" + DamagePattern + "のダメージ与え、" + SkillNameEffectedPattern + "効果を得ました。";

            /* ディレイダメージスキル攻撃 */

            /// <summary>
            /// ディレイダメージスキル攻撃のパターン(自分)
            /// </summary>
            public const string AttackSkillDelayDamageWithoutSourceNamePattern = "^" + SkillNameReplacedDelayDamageSkillNamePattern + "の効果により、" + TargetNamePattern + "に" + SkillNameEffectedPattern + "効果が生じました。";

            /// <summary>
            /// ディレイダメージスキル攻撃のパターン(自分)(メロディ)
            /// </summary>
            public const string AttackSkillDelayDamageMelodyWithoutSourceNamePattern = "^" + SkillNameReplacedDelayDamageSkillNamePattern + "を使い、" + TargetNamePattern + "が" + SkillNameEffectedPattern + "効果を受けました。";

            /// <summary>
            /// ディレイダメージスキル攻撃のパターン(他人)
            /// </summary>
            public const string AttackSkillDelayDamageWithSourceNamePattern = "^" + SourceNamePattern + "が使用した" + SkillNameReplacedDelayDamageSkillNamePattern + "の効果により、" + TargetNamePattern + "に" + SkillNameEffectedPattern + "効果が生じました。";

            /// <summary>
            /// ディレイダメージスキル攻撃のパターン(他人)(メロディ)
            /// </summary>
            public const string AttackSkillDelayDamageMelodyWithSourceNamePattern = "^" + SourceNamePattern + "は" + SkillNameReplacedDelayDamageSkillNamePattern + "の効果により、" + TargetNamePattern + "に" + SkillNameEffectedPattern + "効果を与えました。";

            /// <summary>
            /// ディレイダメージスキル攻撃のダメージのパターン
            /// </summary>
            public const string AttackSkillDelayDamageDamagePattern = "^" + TargetNamePattern + "は" + SkillNameReplacedDelayDamageSkillNamePattern + @"(\sエフェクト|)の効果により、" + DamagePattern + "のダメージを受けました。";

            /* エフェクトダメージスキル攻撃 */

            /// <summary>
            /// エフェクトダメージスキル攻撃のダメージのパターン
            /// </summary>
            public const string AttackSkillEffectDamageDamagePattern = "^" + TargetNamePattern + "は" + SkillNameReplacedEffectDamageSkillNamePattern + "の効果により、" + DamagePattern + "のダメージを受けました。";

            /* 反射攻撃 */

            /// <summary>
            /// 反射攻撃のダメージのパターン(自分)
            /// </summary>
            public const string AttackReflectionDamageWithoutSourceNamePattern = @"^攻撃を反射し、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// 反射攻撃のダメージのパターン(他人)
            /// </summary>
            public const string AttackReflectionDamageWithSourceNamePattern = @"^" + SourceNamePattern + "が攻撃を反射し、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

            /// <summary>
            /// 反射攻撃のダメージのパターン(ディシプリン エネルギー)
            /// </summary>
            public const string AttackReflectionDamageWithDisciplineEnergyPattern = @"^ディシプリン\sエネルギーが攻撃を反射し、" + TargetNamePattern + "に" + DamagePattern + "のダメージを与えました。";

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
            public const string HealCommonPattern = "回復しました。$";
        }

        public enum JobType { None, Sword, Shield, Shadow, Bow, Spell, Spirit, Cure, Chant, Bullet, Gia, Melody };

        public enum SkillType { Normal, DelayDamage, Dot, Summon, EffectDamage, Others };

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
                case JobType.None:
                    return "未選択";
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
            else if (Name.Equals("未選択"))
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

        public class Skill
        {
            public string Name = "";
            public JobType Job = JobType.None;
            public SkillType SkillType = SkillType.Others;

            public Skill(string Name, JobType Job, SkillType SkillType)
            {
                this.Name = Name;
                this.Job = Job;
                this.SkillType = SkillType;
            }
        }

        public static readonly Dictionary<string, Skill> SkillList = GetSkillList();

        private static Dictionary<string, Skill> GetSkillList()
        {
            Dictionary<string, Skill> SkillList = new Dictionary<string, Skill>();
            string SkillName = "";

            /**************************************************************************************************************************************/
            /* シールド ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // エフェクトダメージ
            SkillName = "アップセット エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Shield, SkillType.EffectDamage));


            /**************************************************************************************************************************************/
            /* ソード *****************************************************************************************************************************/
            /**************************************************************************************************************************************/


            /**************************************************************************************************************************************/
            /* シャドウ ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "エクターミネーション";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Shadow, SkillType.Dot));
            SkillName = "デッドリー ポイズン サドン アタック";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Shadow, SkillType.Dot));
            SkillName = "バック ダメージ";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Shadow, SkillType.Dot));

            // エフェクトダメージ
            SkillName = "エクスプローシブ アプライ エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Shadow, SkillType.EffectDamage));
            SkillName = "スティレット コミットメント アディショナル エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Shadow, SkillType.EffectDamage));
            SkillName = "ドラスティック ポイズン エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Shadow, SkillType.EffectDamage));


            /**************************************************************************************************************************************/
            /* ボウ *******************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // エフェクトダメージ
            SkillName = "ヴェンジフル トラップ エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Bow, SkillType.EffectDamage));
            SkillName = "ダスト トラップ エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Bow, SkillType.EffectDamage));
            SkillName = "バンプ トラップ エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Bow, SkillType.EffectDamage));
            /* 要調査 */
            SkillName = "バースト トラップ エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Bow, SkillType.EffectDamage));
            SkillName = "クレアポイアンス トラップ エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Bow, SkillType.EffectDamage));


            /**************************************************************************************************************************************/
            /* スペル *****************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ディレイダメージ
            SkillName = "ボルカニック ブローアウト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spell, SkillType.DelayDamage));
            SkillName = "ボルカニック ビック イラプション";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spell, SkillType.DelayDamage));

            // ドット
            SkillName = "バーン";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spell, SkillType.Dot));
            SkillName = "ヒート サモン";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spell, SkillType.Dot));

            // サモン
            SkillName = "タイフーン サモン";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spell, SkillType.Summon));


            /**************************************************************************************************************************************/
            /* スピリット *************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "イロージョン";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Dot));
            SkillName = "ワイドエリア イロージョン";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Dot));
            SkillName = "アース チェーン";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Dot));
            SkillName = "マジック エクスプロージョン";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Dot));
            SkillName = "アンガー サイクロン";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Dot));
            SkillName = "カース クラウド";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Dot));
            SkillName = "サモン：ウインド エネルギー";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Dot));
            SkillName = "サモン：サイクロン エネルギー";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Dot));

            // サモン
            SkillName = "ファイア スピリット";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Summon));
            SkillName = "ウォーター スピリット";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Summon));
            SkillName = "ウインド スピリット";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Summon));
            SkillName = "グラウンド スピリット";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Summon));
            SkillName = "タイフーン スピリット";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Spirit, SkillType.Summon));


            /**************************************************************************************************************************************/
            /* キュア *****************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "パニッシュ";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Cure, SkillType.Dot));
            SkillName = "アース アンガー";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Cure, SkillType.Dot));
            SkillName = "ウイークン ブランド";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Cure, SkillType.Dot));
            SkillName = "ペイン サクセッシブ";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Cure, SkillType.Dot));
            SkillName = "ディストラクション ボイス";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Cure, SkillType.Dot));

            // サモン
            SkillName = "ディシプリン エネルギー";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Cure, SkillType.Summon));
            SkillName = "ノーブル エネルギー";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Cure, SkillType.Summon));


            /**************************************************************************************************************************************/
            /* チャント ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "マウンテン クラッシュ";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Chant, SkillType.Dot));

            // エフェクトダメージ
            SkillName = "ウインド ブレス";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Chant, SkillType.EffectDamage));
            SkillName = "アース プロミス エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Chant, SkillType.EffectDamage));
            SkillName = "アース コミットメント エフェクト";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Chant, SkillType.EffectDamage));


            /**************************************************************************************************************************************/
            /* バレット ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ドット
            SkillName = "ホーム ペネトレート";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Bullet, SkillType.Dot));


            /**************************************************************************************************************************************/
            /* ギア *******************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ディレイダメージ
            SkillName = "チャージ ファイヤ";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Gia, SkillType.DelayDamage));


            /**************************************************************************************************************************************/
            /* メロディ ***************************************************************************************************************************/
            /**************************************************************************************************************************************/
            // ディレイダメージ
            SkillName = "ダメージ エコー";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Melody, SkillType.DelayDamage));
            SkillName = "シャープ フリカティブ";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Melody, SkillType.DelayDamage));

            // ドット
            SkillName = "モスキー ラプソディ";
            SkillList.Add(SkillName, new Skill(SkillName, JobType.Melody, SkillType.Dot));


            return SkillList;
        }

        /// <summary>
        /// スキルがドットスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public static bool CheckSkillTypeDot(string Name)
        {
            return CheckSkillType(Name, SkillType.Dot);
        }

        /// <summary>
        /// スキルがエフェクトダメージスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public static bool CheckSkillTypeEffectDamage(string Name)
        {
            return CheckSkillType(Name, SkillType.EffectDamage);
        }

        /// <summary>
        /// スキルがサモンスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public static bool CheckSkillTypeSummon(string Name)
        {
            return CheckSkillType(Name, SkillType.Summon);
        }

        /// <summary>
        /// スキルがディレイダメージスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public static bool CheckSkillTypeDelayDamage(string Name)
        {
            return CheckSkillType(Name, SkillType.DelayDamage);
        }

        private static bool CheckSkillType(string Name, SkillType Type)
        {
            try
            {
                if (SkillList.ContainsKey(Name) && SkillList[Name].SkillType.Equals(Type))
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
