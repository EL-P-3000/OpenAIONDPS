using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace OpenAIONDPS
{
    public partial class MainForm : Form
    {
        private string ApplicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";

        /* デバッグ用 */
        StreamWriter DebugLogFileStreamWriter = null;
        TextWriter DebugLogFileTextWriter = null;
        private string DebugLogFileName = "Debug.log";
        private bool IsDebug = false;

        /* スレッド制御用 */
        private bool IsRunning = false;
        private bool StopFlag = true;
        private Thread CalculateThread = null;

        private string OwnName = "自分";

        private Dictionary<string, SkillUnit> SkillUnitList = new Dictionary<string, SkillUnit>();
        private Dictionary<string, MemberUnit> MemberNameMemberUnitList = new Dictionary<string, MemberUnit>();
        private Dictionary<AION.JobType, int> JobTypeNumberOfMemberList = new Dictionary<AION.JobType, int>();

        private LinkedList<MemberUnit> CureMemberUnitList = new LinkedList<MemberUnit>();

        private long TotalDamage = 0;

        private System.Timers.Timer CalcTimer = new System.Timers.Timer();

        private int CalcRemainingTime = 0;

        private bool IsCalcLogFile = false;
        private string CalcLogFilePath = "";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.InitSkillUnit();
            this.FavoriteMemberList.SetMainForm(this);
            this.Member01.SetMemberName(this.OwnName);

            this.AlwaysOnTopCheckBox.Checked = Properties.Settings.Default.AlwaysOnTop;
            this.TopMost = Properties.Settings.Default.AlwaysOnTop;
        }

        private void InitSkillUnit()
        {
            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                SkillUnit _SkillUnit = new SkillUnit();
                _SkillUnit.SetName(_Skill.Name);
                SkillUnitList.Add(_Skill.Name, _SkillUnit);

                if (_Skill.SkillType.Equals(AION.SkillType.Summon) ||
                    _Skill.SkillType.Equals(AION.SkillType.EffectDamage)
                    )
                {
                    this.SkillListFlowLayoutPanel.Controls.Add(_SkillUnit);
                }

            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.StopThread();
        }

        private void ClearData()
        {
            this.JobTypeNumberOfMemberList.Clear();
            foreach (AION.JobType Job in Enum.GetValues(typeof(AION.JobType)))
            {
                this.JobTypeNumberOfMemberList.Add(Job, 0);
            }

            this.MemberNameMemberUnitList.Clear();
            foreach (Control _Control in this.MemberGroupBox.Controls)
            {
                if (_Control.GetType().Name.Equals("MemberUnit"))
                {
                    MemberUnit _MemberUnit = (MemberUnit)_Control;
                    string MemberName = _MemberUnit.GetMemberName();
                    _MemberUnit.Clear();
                    if (!String.IsNullOrEmpty(MemberName) && !this.MemberNameMemberUnitList.ContainsKey(MemberName))
                    {
                        this.MemberNameMemberUnitList.Add(MemberName, _MemberUnit);
                        if (_MemberUnit.GetJob() != AION.JobType.None)
                        {
                            this.JobTypeNumberOfMemberList[_MemberUnit.GetJob()] += 1;
                        }
                    }
                }
            }

            foreach (Control _Control in this.SkillListFlowLayoutPanel.Controls)
            {
                if (_Control.GetType().Name.Equals("SkillUnit"))
                {
                    SkillUnit _SkillUnit = (SkillUnit)_Control;
                    _SkillUnit.Clear();
                }
            }
        }

        private void FileButton_Click(object sender, EventArgs e)
        {
            ShowFileOpenWindow();
        }

        private void ShowFileOpenWindow()
        {
            OpenFileDialog Dialog = new OpenFileDialog();

            Dialog.FileName = "Chat.log";
            Dialog.Filter = "ログファイル(Chat.log)|Chat.log";
            Dialog.Title = "ログファイルを選択してください";
            Dialog.RestoreDirectory = true;

            if (Dialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.ChatLogPath = Dialog.FileName;
                Properties.Settings.Default.InstallDirectory = Path.GetDirectoryName(Dialog.FileName) + "\\";
                Properties.Settings.Default.Save();
            }
        }

        public void InsertMember(string Name, AION.JobType Job)
        {
            if (String.IsNullOrEmpty(Name))
            {
                return;
            }

            foreach (Control _Control in this.MemberGroupBox.Controls)
            {
                if (_Control.GetType().Name.Equals("MemberUnit"))
                {
                    MemberUnit _MemberUnit = (MemberUnit)_Control;
                    _MemberUnit.Clear();
                    if (String.IsNullOrEmpty(_MemberUnit.GetMemberName()))
                    {
                        _MemberUnit.SetMemberName(Name);
                        _MemberUnit.SetJobType(Job);
                        break;
                    }
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartThread();
        }

        private void StartThread()
        {
            if (this.IsRunning == true)
            {
                MessageBox.Show("計測中です。", "エラー");
                return;
            }

            if (!File.Exists(Properties.Settings.Default.ChatLogPath))
            {
                MessageBox.Show("ログファイルを選択してください。", "エラー");
                return;
            }

            this.StartButton.Enabled = false;
            this.StopButton.Enabled = true;
            this.FavoriteMemberButton.Enabled = false;
            this.IsRunning = true;
            this.StopFlag = false;

            this.ClearData();

            // デバッグ
            this.OpenDebugLogFile();

            this.TotalDamage = 0;
            this.TotalDamageLabel.Text = "0";

            this.ClearChatLogFile();

            if (this.CalcTimeCheckBox.Checked)
            {
                this.CalcRemainingTime = (int)this.CalcTimerMinutesNumericUpDown.Value * 60;
                this.CalcRemainingTimeLabel.Text = this.CalcRemainingTime.ToString();
                this.CalcTimer = new System.Timers.Timer();
                this.CalcTimer.SynchronizingObject = this;
                this.CalcTimer.Interval = 1000;
                this.CalcTimer.Elapsed += new System.Timers.ElapsedEventHandler(CalcTimer_Elapsed);
            }

            this.CalculateThread = new Thread(new ThreadStart(Calculate));
            this.CalculateThread.Start();
        }

        private void ClearChatLogFile()
        {
            StreamWriter ChatLogFileStreamWriter = new StreamWriter(Properties.Settings.Default.ChatLogPath);
            ChatLogFileStreamWriter.Write("");
            ChatLogFileStreamWriter.Flush();
            ChatLogFileStreamWriter.Close();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            this.StopThread();
        }

        private void StopThread()
        {
            try
            {
                this.StopFlag = true;
                if (this.CalculateThread != null)
                {
                    this.CalculateThread.Join(2 * 1000);
                    this.CalculateThread = null;
                }
                this.IsRunning = false;
            }
            catch
            {
            }

            this.CloseDebugLogFile();
            this.IsDebug = false;

            this.StartButton.Enabled = true;
            this.StopButton.Enabled = false;
            this.FavoriteMemberButton.Enabled = true;
        }


        /* ログのパターン
         * 
         * 自分, 通常                    [ターゲット]に[ダメージ]のダメージを与えました。
         * 他人, 通常                    [他人]が[ターゲット]に[ダメージ]のダメージを与えました。
         * 
         * 自分, 通常, クリティカル      クリティカルヒット！[ターゲット]に[ダメージ]な致命的なダメージを与えました。
         * 他人, 通常, クリティカル    
         * 
         * 自分, スキル                  [スキル]の効果により、[ターゲット]に[ダメージ]のダメージを与えました。
         * 他人, スキル                  [他人]が使用した[スキル]の効果により、[ターゲット]に[ダメージ]のダメージを与えました。
         *
         * 自分, スキル, ダメージ＆効果  [スキル]により、[ターゲット]に[ダメージ]を与え、[エフェクトスキル]効果が生じました。 
         * 自分, スキル, ダメージ＆効果  
         * 他人, スキル, ダメージ＆効果  
         * 他人, スキル, ダメージ＆効果  [他人]が使用した[スキル]の効果により、[ターゲット]が[ダメージ]のダメージを受け、一部の魔法強化・弱化効果が解除されました。 
         * 
         * 自分, スキル, クリティカル    クリティカルヒット！[スキル]の効果により、[ターゲット]に[ダメージ]のダメージを与えました。
         * 他人, スキル, クリティカル    クリティカルヒット！[他人]が使用した[スキル]の効果により、[ターゲット]に[ダメージ]のダメージを与えました。
         * 
         * サモン, スキル                [サモン名]が使用した[サモン名]の効果により、[ターゲット]に[ダメージ]ダメージを与えました。
         * サモン, スキル                [サモン名]が使用した[サモン名] エフェクトの効果により、[ターゲット]に[ダメージ]のダメージを与えました。
         * 
         * 自分, 反射                    攻撃を反射し、[ターゲット]に[ダメージ]のダメージを与えました。
         * 他人, 反射                    [味方]が攻撃を反射し、[ターゲット]に[ダメージ]のダメージを与えました。
         * 
         * 自分, デバフ成功              [スキル]の効果により、[ターゲット]にダメージを与え続けました。
         * 他人, デバフ成功              [他人]が使用した[スキル]の効果により、[ターゲット]はダメージを受け続けました。
         * 
         */

        /// <summary>
        /// ログのパターン
        /// </summary>
        private static readonly Regex ChatLogLineRegex = new Regex(AION.LogPattern.LinePattern, RegexOptions.Compiled);

        /// <summary>
        /// クリティカルヒットのパターン
        /// </summary>
        private static readonly Regex CriticalHitRegex = new Regex(AION.LogPattern.AttackCriticalHitPattern, RegexOptions.Compiled);

        /// <summary>
        ///  通常攻撃のダメージのパターン(自分)
        /// </summary>
        private static readonly Regex AttackSimpleDamageWithoutSourceNameRegex = new Regex(AION.LogPattern.AttackSimpleDamageWithoutSourceNamePattern, RegexOptions.Compiled);

        // 通常攻撃のダメージのパターン(他人)は計測開始時に取得

        // 通常攻撃のダメージのパターン(サモン)は計測開始時に取得

        /// <summary>
        /// 通常攻撃(クリティカルヒット)のダメージのパターン(自分)
        /// </summary>
        private static readonly Regex AttackCriticalHitDamageRegex = new Regex(@"^(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)の致命的なダメージを与えました。", RegexOptions.Compiled);

        /// <summary>
        /// スキルダメージのパターン(自分)
        /// </summary>
        private static readonly Regex AttackSkillDamageWithoutSourceNameRegex = new Regex(AION.LogPattern.AttackSkillDamageWithoutSourceNamePattern, RegexOptions.Compiled);

        // スキルダメージのパターン(他人)は計測開始時に取得

        // スキルダメージのパターン(サモン)は計測開始時に取得

        /// <summary>
        /// ドットスキルの成功のパターン(自分)
        /// </summary>
        private static readonly Regex AttackSkillDotEffectWithoutSourceNameRegex = new Regex(AION.LogPattern.AttackSkillDotEffectWithoutSourceNamePattern, RegexOptions.Compiled);

        /// <summary>
        /// ドットスキルの成功のパターン(他人)
        /// </summary>
        private static readonly Regex AttackSkillDotEffectWithSourceNameRegex = new Regex(AION.LogPattern.AttackSkillDotEffectWithSourceNamePattern, RegexOptions.Compiled);
            
        // ドットスキルのダメージのパターンは計測開始時に取得

        /// <summary>
        /// サモンスキル(攻撃対象固定)のパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogSkillSummon2Regex = new Regex(@"^サモン：(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)を攻撃する(?<SkillName2>.+)を召喚しました。", RegexOptions.Compiled);

        /// <summary>
        /// サモンスキル(攻撃対象固定)のパターン(他人)
        /// </summary>
        private static readonly Regex ChatLogCharacterSkillSummon2Regex = new Regex(@"^(?<SourceName>.+)が使用したサモン：(?<SkillName>[\p{IsKatakana}：\s]+)の効果により、(?<TargetName>.+)を攻撃する(?<SkillName2>.+)を召喚しました。", RegexOptions.Compiled);

        // サモンスキル(攻撃対象固定)のダメージのパターンは計測開始時に取得

        /// <summary>
        /// デバフダメージスキルのダメージのパターン(自分)
        /// </summary>
        private static readonly Regex AttackSkillReleaseBuffDamageWithoutSourceNameRegex = new Regex(AION.LogPattern.AttackSkillReleaseBuffDamageWithoutSourceNamePattern, RegexOptions.Compiled);
        private static readonly Regex AttackSkillDebuffDamageWithoutSourceNameRegex = new Regex(AION.LogPattern.AttackSkillDebuffDamageWithoutSourceNamePattern, RegexOptions.Compiled);

        /// <summary>
        /// デバフダメージスキルのダメージのパターン(他人)
        /// </summary>
        private static readonly Regex AttackSkillReleaseBuffDamageWithSourceNameRegex = new Regex(AION.LogPattern.AttackSkillReleaseBuffDamageWithSourceNamePattern, RegexOptions.Compiled);
        private static readonly Regex AttackSkillDebuffDamageWithSourceNameRegex = new Regex(AION.LogPattern.AttackSkillDebuffDamageWithSourceNamePattern, RegexOptions.Compiled);

        // ディレイダメージスキルのパターン(自分)は計測開始時に取得

        // ディレイダメージスキルのパターン(他人)は計測開始時に取得

        // ディレイダメージスキルのダメージのパターンは計測開始時に取得

        // エフェクトダメージスキルのダメージのパターンは計測開始時に取得

        /// <summary>
        /// 反射のダメージのパターン(自分)
        /// </summary>
        private static readonly Regex AttackReflectionDamageWithoutSourceNameRegex = new Regex(AION.LogPattern.AttackReflectionDamageWithoutSourceNamePattern, RegexOptions.Compiled);

        /// <summary>
        /// 反射のダメージのパターン(他人)
        /// </summary>
        private static readonly Regex AttackReflectionDamageWithSourceNameRegex = new Regex(AION.LogPattern.AttackReflectionDamageWithSourceNamePattern, RegexOptions.Compiled);

        /// <summary>
        /// 反射のダメージのパターン(ディシプリン エネルギー)
        /// </summary>
        private static readonly Regex AttackReflectionDamageWithDisciplineEnergyRegex = new Regex(AION.LogPattern.AttackReflectionDamageWithDisciplineEnergyPattern, RegexOptions.Compiled);

        /// <summary>
        /// 回避/抵抗した攻撃のパターン(自分)リスト
        /// </summary>
        private static readonly Regex ChatLogEvadeResistRegex = new Regex(AION.LogPattern.EvasionResistanceWithoutSourceNamePattern, RegexOptions.Compiled);

        /// <summary>
        /// 回避/抵抗された攻撃のパターン(自分)リスト
        /// </summary>
        private static readonly Regex ChatLogEvadedResistedRegex = new Regex(AION.LogPattern.EvadedResistedWithoutTargetNamePattern, RegexOptions.Compiled);

        // 回避/抵抗した/された攻撃のパターン(他人)リスト

        /// <summary>
        /// 回避/抵抗のパターン(他人)(その他排除用)
        /// </summary>
        private static readonly Regex EvasionResistancePatternWithSourceNameRegex = new Regex(AION.LogPattern.EvasionResistanceWithSourceNamePattern, RegexOptions.Compiled);

        /// <summary>
        /// 回復のパターン
        /// </summary>
        private static readonly Regex HealCommonRegex = new Regex(AION.LogPattern.HealCommonPattern, RegexOptions.Compiled);

        /// <summary>
        /// 計測
        /// </summary>
        public void Calculate()
        {
            Delegate UpdateDataDelegate = new Action<ActionData>(UpdateDamageData);
            Delegate UpdateEvasionDelegate = new Action<ActionData>(UpdateEvasion);
            Delegate UpdateResistanceDelegate = new Action<ActionData>(UpdateResistance);
            Delegate CalcFromLogEndDelegate = new Action(CalcFromLogEnd);
            string LogFilePath = Properties.Settings.Default.ChatLogPath;
            string LogText = "";
            string LogTextWithoutTime = "";
            ActionData ChatLogActionData = null;

            // ターゲットのデバフリスト
            Dictionary<string, Dictionary<string, ActionData>> SkillDebuffTargetList = new Dictionary<string, Dictionary<string, ActionData>>();

            // ターゲットのディレイダメージスキルリスト
            Dictionary<string, Dictionary<string, LinkedList<ActionData>>> SkillDelayDamageList = new Dictionary<string, Dictionary<string, LinkedList<ActionData>>>();

            // 通常攻撃のダメージのパターン(他人)
            LinkedList<Regex> AttackSimpleDamageWithSourceNameRegexList = this.GetAttackSimpleDamageWithSourceNameRegexList();

            // 通常攻撃のダメージのパターン(サモン)
            LinkedList<Regex> ChatLogSummonSimpleDamageRegexList = this.GetChatLogSummonSimpleDamageRegexList();

            // スキルダメージのパターン(他人)
            LinkedList<Regex> AttackSkillDamageWithSourceNameRegexList = this.GetAttackSkillDamageWithSourceNameRegexList();

            // スキルのダメージのパターン(サモン)
            LinkedList<Regex> ChatLogSummonSkillDamageRegexList = this.GetChatLogSummonSkillDamageRegexList();

            // ドットスキルのダメージのパターン
            LinkedList<Regex> AttackSkillDotDamageRegexList = this.GetAttackSkillDotDamageRegexList();

            // サモンスキル(攻撃対象固定)のダメージのパターン
            LinkedList<Regex> ChatLogSkillSummon2DamageRegexList = this.GetChatLogSkillSummon2DamageRegexList();

            // ディレイダメージスキルのパターン(自分)
            LinkedList<Regex> AttackSkillDelayDamageWithoutSourceNameRegexList = this.GetAttackSkillDelayDamageWithoutSourceNameRegexList();

            // ディレイダメージスキルのパターン(他人)
            LinkedList<Regex> AttackSkillDelayDamageWithSourceNameRegexList = this.GetAttackSkillDelayDamageWithSourceNameRegexList();

            // ディレイダメージスキルのダメージのパターン
            LinkedList<Regex> AttackSkillDelayDamageDamageRegexList = this.GetAttackSkillDelayDamageDamageRegexList();

            // エフェクトダメージスキルのダメージのパターン
            LinkedList<Regex> AttackSkillEffectDamageDamageRegexList = this.GetAttackSkillEffectDamageDamageRegexList();

            // 回避/抵抗した/された攻撃のパターン(他人)リスト
            LinkedList<Regex> ChatLogCharacterEvasionResistanceRegexList = this.GetChatLogCharacterEvasionResistanceRegexList();

            // ログファイルから計算の場合はログファイルを設定
            if (this.IsCalcLogFile)
            {
                LogFilePath = this.CalcLogFilePath;
            }

            using (FileStream ChatLogFileStream = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader ChatLogStreamReader = new StreamReader(ChatLogFileStream, Encoding.GetEncoding("Shift_JIS")))
                {
                    while (this.StopFlag == false)
                    {
                        try
                        {
                            // ログファイルから計算の場合はファイルの最後で終了
                            if (this.IsCalcLogFile && ChatLogStreamReader.EndOfStream == true)
                            {
                                break;
                            }

                            LogText = ChatLogStreamReader.ReadLine();
                            LogTextWithoutTime = "";

                            // ラインの取得
                            if (String.IsNullOrEmpty(LogText))
                            {
                                continue;
                            }

                            // 正しいフォーマットのラインの取得
                            Match ChatLogLineMatch = ChatLogLineRegex.Match(LogText);
                            if (!ChatLogLineMatch.Success)
                            {
                                continue;
                            }

                            ChatLogActionData = new ActionData();
                            ChatLogActionData.LogText = LogText;
                            ChatLogActionData.SourceName = OwnName;

                            // ログファイルから計算の場合は時刻を取得
                            if (this.IsCalcLogFile)
                            {
                                ChatLogActionData.Time = DateTime.ParseExact(ChatLogLineMatch.Groups[1].Value, "yyyy.MM.dd HH:mm:ss", null);
                            }

                            // 時刻をラインから削除
                            LogTextWithoutTime = ChatLogLineMatch.Groups[2].Value;

                            // 回復のパターンは対象外
                            Match HealCommonMatch = HealCommonRegex.Match(LogTextWithoutTime);
                            if (HealCommonMatch.Success)
                            {
                                continue;
                            }

                            // クリティカルヒット！
                            Match CriticalHitMatch = CriticalHitRegex.Match(LogTextWithoutTime);
                            if (CriticalHitMatch.Success)
                            {
                                ChatLogActionData.CriticalHit = true;
                                LogTextWithoutTime = CriticalHitMatch.Groups[1].Value;
                            }

                            // ドットスキルの成功
                            Match AttackSkillDotEffectWithSourceNameMatch = AttackSkillDotEffectWithSourceNameRegex.Match(LogTextWithoutTime);
                            Match AttackSkillDotEffectWithoutSourceNameMatch = AttackSkillDotEffectWithoutSourceNameRegex.Match(LogTextWithoutTime);
                            if (AttackSkillDotEffectWithSourceNameMatch.Success || AttackSkillDotEffectWithoutSourceNameMatch.Success)
                            {
                                Match _Match = null;
                                if (AttackSkillDotEffectWithSourceNameMatch.Success)
                                {
                                    _Match = AttackSkillDotEffectWithSourceNameMatch;
                                    ChatLogActionData.SourceName = _Match.Groups["SourceName"].Value;
                                }
                                else
                                {
                                    _Match = AttackSkillDotEffectWithoutSourceNameMatch;
                                    ChatLogActionData.SourceName = this.OwnName;
                                }

                                ChatLogActionData.TargetName = _Match.Groups["TargetName"].Value;
                                ChatLogActionData.SkillName = _Match.Groups["SkillName"].Value;

                                // ターゲット存在のチェック
                                if (SkillDebuffTargetList.ContainsKey(ChatLogActionData.TargetName))
                                {
                                    // デバフ存在のチェック
                                    Dictionary<string, ActionData> DebuffSkillList = SkillDebuffTargetList[ChatLogActionData.TargetName];
                                    if (DebuffSkillList.ContainsKey(ChatLogActionData.SkillName))
                                    {
                                        DebuffSkillList.Remove(ChatLogActionData.SkillName);
                                    }

                                    DebuffSkillList.Add(ChatLogActionData.SkillName, ChatLogActionData);
                                }
                                else
                                {
                                    Dictionary<string, ActionData> DebuffSkillList = new Dictionary<string, ActionData>();
                                    DebuffSkillList.Add(ChatLogActionData.SkillName, ChatLogActionData);
                                    SkillDebuffTargetList.Add(ChatLogActionData.TargetName, DebuffSkillList);
                                }

                                continue;
                            }

                            // 反射のダメージ
                            Match AttackReflectionDamageWithDisciplineEnergyMatch = AttackReflectionDamageWithDisciplineEnergyRegex.Match(LogTextWithoutTime);
                            Match AttackReflectionDamageWithSourceNameMatch = AttackReflectionDamageWithSourceNameRegex.Match(LogTextWithoutTime);
                            Match AttackReflectionDamageWithoutSourceNameMatch = AttackReflectionDamageWithoutSourceNameRegex.Match(LogTextWithoutTime);
                            if (AttackReflectionDamageWithDisciplineEnergyMatch.Success || AttackReflectionDamageWithSourceNameMatch.Success || AttackReflectionDamageWithoutSourceNameMatch.Success)
                            {
                                Match _Match = null;
                                if (AttackReflectionDamageWithDisciplineEnergyMatch.Success)
                                {
                                    ChatLogActionData.SourceName = "ディシプリン エネルギー";
                                    ChatLogActionData.SkillName = "ディシプリン エネルギー";
                                    _Match = AttackReflectionDamageWithSourceNameMatch;
                                }
                                else if (AttackReflectionDamageWithSourceNameMatch.Success)
                                {
                                    ChatLogActionData.SourceName = AttackReflectionDamageWithSourceNameMatch.Groups["SourceName"].Value;
                                    _Match = AttackReflectionDamageWithSourceNameMatch;
                                }
                                else
                                {
                                    ChatLogActionData.SourceName = this.OwnName;
                                    _Match = AttackReflectionDamageWithoutSourceNameMatch;
                                }

                                ChatLogActionData.TargetName = _Match.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(_Match.Groups["Damage"].Value.Replace(",", ""));

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }

                            // デバフダメージスキルのダメージ
                            Match AttackSkillReleaseBuffDamageWithSourceNameMatch = AttackSkillReleaseBuffDamageWithSourceNameRegex.Match(LogTextWithoutTime);
                            Match AttackSkillDebuffDamageWithSourceNameMatch = AttackSkillDebuffDamageWithSourceNameRegex.Match(LogTextWithoutTime);
                            Match AttackSkillReleaseBuffDamageWithoutSourceNameMatch = AttackSkillReleaseBuffDamageWithoutSourceNameRegex.Match(LogTextWithoutTime);
                            Match AttackSkillDebuffDamageWithoutSourceNameMatch = AttackSkillDebuffDamageWithoutSourceNameRegex.Match(LogTextWithoutTime);
                            if (AttackSkillReleaseBuffDamageWithSourceNameMatch.Success ||
                                AttackSkillDebuffDamageWithSourceNameMatch.Success ||
                                AttackSkillReleaseBuffDamageWithoutSourceNameMatch.Success ||
                                AttackSkillDebuffDamageWithoutSourceNameMatch.Success)
                            {
                                Match _Match = null;
                                if (AttackSkillReleaseBuffDamageWithSourceNameMatch.Success || AttackSkillDebuffDamageWithSourceNameMatch.Success)
                                {
                                    if (AttackSkillReleaseBuffDamageWithSourceNameMatch.Success)
                                    {
                                        _Match = AttackSkillReleaseBuffDamageWithSourceNameMatch;
                                    }
                                    else
                                    {
                                        _Match = AttackSkillDebuffDamageWithSourceNameMatch;
                                    }
                                    ChatLogActionData.SourceName = _Match.Groups["SourceName"].Value;
                                }
                                else
                                {
                                    if (AttackSkillReleaseBuffDamageWithoutSourceNameMatch.Success)
                                    {
                                        _Match = AttackSkillReleaseBuffDamageWithoutSourceNameMatch;
                                    }
                                    else
                                    {
                                        _Match = AttackSkillDebuffDamageWithoutSourceNameMatch;
                                    }
                                    ChatLogActionData.SourceName = this.OwnName;
                                }

                                ChatLogActionData.SkillName = _Match.Groups["SkillName"].Value;
                                ChatLogActionData.TargetName = _Match.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(_Match.Groups["Damage"].Value.Replace(",", ""));

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }

                            // エフェクトダメージスキルのダメージ
                            bool AttackSkillEffectDamageDamageMatchFlag = false;
                            foreach (Regex AttackSkillEffectDamageDamageRegex in AttackSkillEffectDamageDamageRegexList)
                            {
                                Match ChatLogSkillEffectDamageDamageMatch = AttackSkillEffectDamageDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogSkillEffectDamageDamageMatch.Success)
                                {
                                    AttackSkillEffectDamageDamageMatchFlag = true;
                                    ChatLogActionData.SkillName = ChatLogSkillEffectDamageDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = ChatLogSkillEffectDamageDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(ChatLogSkillEffectDamageDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);
                                }
                            }
                            if (AttackSkillEffectDamageDamageMatchFlag)
                            {
                                continue;
                            }

                            // ディレイダメージスキルのダメージ
                            bool AttackSkillDelayDamageDamageMatchFlag = false;
                            foreach (Regex AttackSkillDelayDamageDamageRegex in AttackSkillDelayDamageDamageRegexList)
                            {
                                Match AttackSkillDelayDamageDamageMatch = AttackSkillDelayDamageDamageRegex.Match(LogTextWithoutTime);
                                if (AttackSkillDelayDamageDamageMatch.Success)
                                {
                                    AttackSkillDelayDamageDamageMatchFlag = true;
                                    ChatLogActionData.TargetName = AttackSkillDelayDamageDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.SkillName = AttackSkillDelayDamageDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.Damage = long.Parse(AttackSkillDelayDamageDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                    if (SkillDelayDamageList.ContainsKey(ChatLogActionData.TargetName))
                                    {
                                        Dictionary<string, LinkedList<ActionData>> SkillActionDataList = SkillDelayDamageList[ChatLogActionData.TargetName];
                                        if (SkillActionDataList.ContainsKey(ChatLogActionData.SkillName))
                                        {
                                            LinkedList<ActionData> ActionDataList = SkillActionDataList[ChatLogActionData.SkillName];
                                            LinkedList<ActionData> RemoveActionDataList = new LinkedList<ActionData>();

                                            foreach (ActionData ChatLogSkillDelayActionData in ActionDataList)
                                            {
                                                if (((ChatLogActionData.Time.Ticks - ChatLogSkillDelayActionData.Time.Ticks) / 10000000) < 10)
                                                {
                                                    ChatLogActionData.SourceName = ChatLogSkillDelayActionData.SourceName;
                                                    ActionDataList.Remove(ChatLogSkillDelayActionData);

                                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                                    break;
                                                }
                                                else
                                                {
                                                    RemoveActionDataList.AddLast(ChatLogSkillDelayActionData);
                                                }
                                            }

                                            foreach (ActionData RemoveActionData in RemoveActionDataList)
                                            {
                                                ActionDataList.Remove(RemoveActionData);
                                            }
                                        }
                                        else
                                        {
                                            // 使用者不明ダメージ
                                        }
                                    }
                                    else
                                    {
                                        // 使用者不明ダメージ
                                    }

                                    break;
                                }
                            }
                            if (AttackSkillDelayDamageDamageMatchFlag)
                            {
                                continue;
                            }

                            // ディレイダメージスキル(他人)
                            bool AttackSkillDelayDamageWithSourceNameMatchFlag = false;
                            foreach (Regex AttackSkillDelayDamageWithSourceNameRegex in AttackSkillDelayDamageWithSourceNameRegexList)
                            {
                                Match ChatLogCharacterSkillDelayMatch = AttackSkillDelayDamageWithSourceNameRegex.Match(LogTextWithoutTime);
                                if (ChatLogCharacterSkillDelayMatch.Success)
                                {
                                    AttackSkillDelayDamageWithSourceNameMatchFlag = true;
                                    ChatLogActionData.SourceName = ChatLogCharacterSkillDelayMatch.Groups["SourceName"].Value;
                                    ChatLogActionData.SkillName = ChatLogCharacterSkillDelayMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = ChatLogCharacterSkillDelayMatch.Groups["TargetName"].Value;

                                    if (!SkillDelayDamageList.ContainsKey(ChatLogActionData.TargetName))
                                    {
                                        Dictionary<string, LinkedList<ActionData>> _SkillActionDataList = new Dictionary<string, LinkedList<ActionData>>();
                                        SkillDelayDamageList.Add(ChatLogActionData.TargetName, _SkillActionDataList);
                                    }

                                    Dictionary<string, LinkedList<ActionData>> SkillActionDataList = SkillDelayDamageList[ChatLogActionData.TargetName];

                                    if (!SkillActionDataList.ContainsKey(ChatLogActionData.SkillName))
                                    {
                                        LinkedList<ActionData> TempSkillActionList = new LinkedList<ActionData>();
                                        SkillActionDataList.Add(ChatLogActionData.SkillName, TempSkillActionList);
                                    }

                                    LinkedList<ActionData> ActionList = SkillActionDataList[ChatLogActionData.SkillName];
                                    ActionList.AddLast(ChatLogActionData);

                                    break;
                                }
                            }
                            if (AttackSkillDelayDamageWithSourceNameMatchFlag)
                            {
                                continue;
                            }

                            // ディレイダメージスキル(自分)
                            bool AttackSkillDelayDamageWithoutSourceNameMatchFlag = false;
                            foreach (Regex AttackSkillDelayDamageWithoutSourceNameRegex in AttackSkillDelayDamageWithoutSourceNameRegexList)
                            {
                                Match AttackSkillDelayDamageWithoutSourceNameMatch = AttackSkillDelayDamageWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                if (AttackSkillDelayDamageWithoutSourceNameMatch.Success)
                                {
                                    AttackSkillDelayDamageWithoutSourceNameMatchFlag = true;
                                    ChatLogActionData.SourceName = this.OwnName;
                                    ChatLogActionData.SkillName = AttackSkillDelayDamageWithoutSourceNameMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = AttackSkillDelayDamageWithoutSourceNameMatch.Groups["TargetName"].Value;

                                    if (!SkillDelayDamageList.ContainsKey(ChatLogActionData.TargetName))
                                    {
                                        Dictionary<string, LinkedList<ActionData>> _SkillActionDataList = new Dictionary<string, LinkedList<ActionData>>();
                                        SkillDelayDamageList.Add(ChatLogActionData.TargetName, _SkillActionDataList);
                                    }

                                    Dictionary<string, LinkedList<ActionData>> SkillActionDataList = SkillDelayDamageList[ChatLogActionData.TargetName];

                                    if (!SkillActionDataList.ContainsKey(ChatLogActionData.SkillName))
                                    {
                                        LinkedList<ActionData> TempSkillActionList = new LinkedList<ActionData>();
                                        SkillActionDataList.Add(ChatLogActionData.SkillName, TempSkillActionList);
                                    }

                                    LinkedList<ActionData> ActionList = SkillActionDataList[ChatLogActionData.SkillName];
                                    ActionList.AddLast(ChatLogActionData);

                                    break;
                                }
                            }
                            if (AttackSkillDelayDamageWithoutSourceNameMatchFlag)
                            {
                                continue;
                            }

                            // サモン(攻撃対象固定)のダメージ
                            bool ChatLogSkillSummon2DamageMatchFlag = false;
                            foreach (Regex ChatLogSkillSummon2DamageRegex in ChatLogSkillSummon2DamageRegexList)
                            {
                                Match ChatLogSkillSummon2DamageMatch = ChatLogSkillSummon2DamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogSkillSummon2DamageMatch.Success)
                                {
                                    ChatLogSkillSummon2DamageMatchFlag = true;
                                    ChatLogActionData.SourceName = ChatLogSkillSummon2DamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.SkillName = ChatLogSkillSummon2DamageMatch.Groups["SkillName2"].Value;
                                    ChatLogActionData.TargetName = ChatLogSkillSummon2DamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(ChatLogSkillSummon2DamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                    break;
                                }
                            }
                            if (ChatLogSkillSummon2DamageMatchFlag)
                            {
                                continue;
                            }

                            // ドットスキルのダメージ
                            bool AttackSkillDotDamageMatchFlag = false;
                            foreach (Regex AttackSkillDotDamageRegex in AttackSkillDotDamageRegexList)
                            {
                                Match AttackSkillDotDamageMatch = AttackSkillDotDamageRegex.Match(LogTextWithoutTime);
                                if (AttackSkillDotDamageMatch.Success)
                                {
                                    AttackSkillDotDamageMatchFlag = true;
                                    ChatLogActionData.SkillName = AttackSkillDotDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = AttackSkillDotDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(AttackSkillDotDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                    if (SkillDebuffTargetList.ContainsKey(ChatLogActionData.TargetName))
                                    {
                                        Dictionary<string, ActionData> SkillDebuffList = SkillDebuffTargetList[ChatLogActionData.TargetName];
                                        if (SkillDebuffList.ContainsKey(ChatLogActionData.SkillName))
                                        {
                                            ChatLogActionData.SourceName = SkillDebuffList[ChatLogActionData.SkillName].SourceName;
                                            this.Invoke(UpdateDataDelegate, ChatLogActionData);
                                        }
                                    }
                                }
                            }
                            if (AttackSkillDotDamageMatchFlag)
                            {
                                continue;
                            }

                            // スキルのダメージ(サモン)
                            bool ChatLogSummonSkillDamageMatchFlag = false;
                            foreach (Regex ChatLogSummonSkillDamageRegex in ChatLogSummonSkillDamageRegexList)
                            {
                                Match ChatLogSummonSkillDamageMatch = ChatLogSummonSkillDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogSummonSkillDamageMatch.Success)
                                {
                                    Debug.WriteLine(LogText);
                                    ChatLogSummonSkillDamageMatchFlag = true;
                                    ChatLogActionData.SourceName = ChatLogSummonSkillDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.SkillName = ChatLogSummonSkillDamageMatch.Groups["SkillName2"].Value;
                                    ChatLogActionData.TargetName = ChatLogSummonSkillDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(ChatLogSummonSkillDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                    break;
                                }
                            }
                            if (ChatLogSummonSkillDamageMatchFlag)
                            {
                                continue;
                            }

                            // スキルのダメージ(他人)
                            bool ChatLogCharacterSkillDamageMatchFlag = false;
                            foreach (Regex ChatLogCharacterSkillDamageRegex in AttackSkillDamageWithSourceNameRegexList)
                            {
                                Match ChatLogCharacterSkillDamageMatch = ChatLogCharacterSkillDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogCharacterSkillDamageMatch.Success)
                                {
                                    ChatLogCharacterSkillDamageMatchFlag = true;
                                    ChatLogActionData.SourceName = ChatLogCharacterSkillDamageMatch.Groups["SourceName"].Value;
                                    ChatLogActionData.SkillName = ChatLogCharacterSkillDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = ChatLogCharacterSkillDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(ChatLogCharacterSkillDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                    break;
                                }
                            }
                            if (ChatLogCharacterSkillDamageMatchFlag)
                            {
                                continue;
                            }

                            // スキルのダメージ(自分)
                            Match ChatLogSkillDamageMatch = AttackSkillDamageWithoutSourceNameRegex.Match(LogTextWithoutTime);
                            if (ChatLogSkillDamageMatch.Success)
                            {
                                ChatLogActionData.SourceName = this.OwnName;
                                ChatLogActionData.SkillName = ChatLogSkillDamageMatch.Groups["SkillName"].Value;
                                ChatLogActionData.TargetName = ChatLogSkillDamageMatch.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(ChatLogSkillDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }

                            // 通常攻撃のダメージ(サモン)
                            bool ChatLogSummonSimpleDamageMatchFlag = false;
                            foreach (Regex ChatLogSummonSimpleDamageRegex in ChatLogSummonSimpleDamageRegexList)
                            {
                                Match ChatLogSummonSimpleDamageMatch = ChatLogSummonSimpleDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogSummonSimpleDamageMatch.Success)
                                {
                                    Debug.WriteLine(LogText);
                                    ChatLogSummonSimpleDamageMatchFlag = true;
                                    ChatLogActionData.SourceName = ChatLogSummonSimpleDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = ChatLogSummonSimpleDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(ChatLogSummonSimpleDamageMatch.Groups["Damage"].Value.Replace(",", ""));
                                    ChatLogActionData.IsSkill = false;

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                    break;
                                }
                            }
                            if (ChatLogSummonSimpleDamageMatchFlag)
                            {
                                continue;
                            }

                            // 通常攻撃(クリティカルヒット)のダメージ(自分)
                            Match ChatLogCriticalHitDamageMatch = AttackCriticalHitDamageRegex.Match(LogTextWithoutTime);
                            if (ChatLogCriticalHitDamageMatch.Success)
                            {
                                ChatLogActionData.SourceName = this.OwnName;
                                ChatLogActionData.SkillName = ChatLogCriticalHitDamageMatch.Groups["SkillName"].Value;
                                ChatLogActionData.TargetName = ChatLogCriticalHitDamageMatch.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(ChatLogCriticalHitDamageMatch.Groups["Damage"].Value.Replace(",", ""));
                                ChatLogActionData.IsSkill = false;

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }

                            // 通常攻撃のダメージ(他人)
                            bool AttackSimpleDamageWithSourceNameMatchFlag = false;
                            foreach (Regex AttackSimpleDamageWithSourceNameRegex in AttackSimpleDamageWithSourceNameRegexList)
                            {
                                Match AttackSimpleDamageWithSourceNameMatch = AttackSimpleDamageWithSourceNameRegex.Match(LogTextWithoutTime);
                                if (AttackSimpleDamageWithSourceNameMatch.Success)
                                {
                                    AttackSimpleDamageWithSourceNameMatchFlag = true;
                                    ChatLogActionData.SourceName = AttackSimpleDamageWithSourceNameMatch.Groups["SourceName"].Value;
                                    ChatLogActionData.SkillName = AttackSimpleDamageWithSourceNameMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = AttackSimpleDamageWithSourceNameMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(AttackSimpleDamageWithSourceNameMatch.Groups["Damage"].Value.Replace(",", ""));
                                    ChatLogActionData.IsSkill = false;

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                    break;
                                }
                            }
                            if (AttackSimpleDamageWithSourceNameMatchFlag)
                            {
                                continue;
                            }

                            // 自分 通常攻撃のダメージ
                            Match AttackSimpleDamageWithoutSourceNameMatch = AttackSimpleDamageWithoutSourceNameRegex.Match(LogTextWithoutTime);
                            if (AttackSimpleDamageWithoutSourceNameMatch.Success)
                            {
                                ChatLogActionData.SourceName = this.OwnName;
                                ChatLogActionData.SkillName = AttackSimpleDamageWithoutSourceNameMatch.Groups["SkillName"].Value;
                                ChatLogActionData.TargetName = AttackSimpleDamageWithoutSourceNameMatch.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(AttackSimpleDamageWithoutSourceNameMatch.Groups["Damage"].Value.Replace(",", ""));
                                ChatLogActionData.IsSkill = false;

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }

                            // 回避/抵抗(他人)
                            bool ChatLogCharacterEvasionResistanceMatchFlag = false;
                            foreach (Regex ChatLogCharacterEvasionResistanceRegex in ChatLogCharacterEvasionResistanceRegexList)
                            {
                                Match ChatLogCharacterEvasionResistanceMatch = ChatLogCharacterEvasionResistanceRegex.Match(LogTextWithoutTime);
                                if (ChatLogCharacterEvasionResistanceMatch.Success)
                                {
                                    ChatLogCharacterEvasionResistanceMatchFlag = true;
                                    ChatLogActionData.SourceName = ChatLogCharacterEvasionResistanceMatch.Groups["SourceName"].Value;
                                    ChatLogActionData.TargetName = ChatLogCharacterEvasionResistanceMatch.Groups["TargetName"].Value;

                                    if (MemberNameMemberUnitList.ContainsKey(ChatLogActionData.SourceName) || MemberNameMemberUnitList.ContainsKey(ChatLogActionData.TargetName))
                                    {
                                        if (LogTextWithoutTime.IndexOf("を回避しました。") > 0)
                                        {
                                            this.Invoke(UpdateEvasionDelegate, new object[] { ChatLogActionData });
                                        }
                                        else
                                        {
                                            this.Invoke(UpdateResistanceDelegate, new object[] { ChatLogActionData });
                                        }
                                    }

                                    break;
                                }
                            }
                            if (ChatLogCharacterEvasionResistanceMatchFlag)
                            {
                                continue;
                            }

                            // 回避/抵抗(他人)(その他排除用)
                            Match EvasionResistancePatternWithSourceNameMatch = EvasionResistancePatternWithSourceNameRegex.Match(LogTextWithoutTime);
                            if (EvasionResistancePatternWithSourceNameMatch.Success)
                            {
                                continue;
                            }

                            // 回避/抵抗された攻撃(自分)
                            Match ChatLogEvadedResistedMatch = ChatLogEvadedResistedRegex.Match(LogTextWithoutTime);
                            if (ChatLogEvadedResistedMatch.Success)
                            {
                                ChatLogActionData.SourceName = ChatLogEvadedResistedMatch.Groups["SourceName"].Value;
                                ChatLogActionData.TargetName = this.OwnName;

                                Debug.WriteLine(LogText);
                                if (LogTextWithoutTime.IndexOf("を回避しました。") > 0)
                                {
                                    this.Invoke(UpdateEvasionDelegate, new object[] { ChatLogActionData });
                                }
                                else
                                {
                                    this.Invoke(UpdateResistanceDelegate, new object[] { ChatLogActionData });
                                }

                                continue;
                            }

                            // 回避/抵抗した攻撃(自分)
                            Match ChatLogEvadeResistMatch = ChatLogEvadeResistRegex.Match(LogTextWithoutTime);
                            if (ChatLogEvadeResistMatch.Success)
                            {
                                ChatLogActionData.SourceName = this.OwnName;
                                ChatLogActionData.TargetName = ChatLogEvadeResistMatch.Groups["TargetName"].Value;

                                Debug.WriteLine(LogText);
                                if (LogTextWithoutTime.IndexOf("を回避しました。") > 0)
                                {
                                    this.Invoke(UpdateEvasionDelegate, new object[] { ChatLogActionData });
                                }
                                else
                                {
                                    this.Invoke(UpdateResistanceDelegate, new object[] { ChatLogActionData });
                                }

                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            this.PrintExceptionDebugMessage(ex);
                        }
                    }
                }
            }

            if (this.IsCalcLogFile)
            {
                this.Invoke(CalcFromLogEndDelegate);
            }

            this.IsRunning = false;
        }

        /// <summary>
        /// 通常攻撃のダメージのパターン(他人)リストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSimpleDamageWithSourceNameRegexList()
        {
            LinkedList<Regex> AttackSimpleDamageWithSourceNameRegexList = new LinkedList<Regex>();

            foreach (string MemberName in this.MemberNameMemberUnitList.Keys)
            {
                if (!String.IsNullOrEmpty(MemberName) && !MemberName.Equals(this.OwnName))
                {
                    AttackSimpleDamageWithSourceNameRegexList.AddLast(new Regex(AION.LogPattern.AttackSimpleDamageWithSourceNameReplacedMemberNamePattern.Replace("[[[MemberName]]]", MemberName), RegexOptions.Compiled));
                }
            }

            AttackSimpleDamageWithSourceNameRegexList.AddLast(new Regex(AION.LogPattern.AttackSimpleDamageWithSourceNamePattern, RegexOptions.Compiled));

            return AttackSimpleDamageWithSourceNameRegexList;
        }

        /// <summary>
        /// スキルダメージのパターン(他人)リストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDamageWithSourceNameRegexList()
        {
            LinkedList<Regex> AttackSkillDamageWithSourceNameRegexList = new LinkedList<Regex>();

            foreach (string MemberName in this.MemberNameMemberUnitList.Keys)
            {
                if (!String.IsNullOrEmpty(MemberName) && !MemberName.Equals(this.OwnName))
                {

                    AttackSkillDamageWithSourceNameRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillDamageWithSourceNameReplacedMemberNamePattern.Replace("[[[MemberName]]]", MemberName), RegexOptions.Compiled));
                }
            }

            AttackSkillDamageWithSourceNameRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillDamageWithSourceNamePattern, RegexOptions.Compiled));

            return AttackSkillDamageWithSourceNameRegexList;
        }

        /// <summary>
        /// ドットスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDotDamageRegexList()
        {
            LinkedList<Regex> AttackSkillDotDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.Dot))
                {
                    AttackSkillDotDamageRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillDotDamagePattern.Replace("[[[DotSkillName]]]", _Skill.Name), RegexOptions.Compiled));
                }
            }

            return AttackSkillDotDamageRegexList;
        }

        /// <summary>
        /// 通常攻撃のダメージのパターン(サモン)リストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogSummonSimpleDamageRegexList()
        {
            LinkedList<Regex> ChatLogSummonSimpleDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.Summon))
                {
                    ChatLogSummonSimpleDamageRegexList.AddLast(new Regex("^(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")が(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));
                }
            }

            return ChatLogSummonSimpleDamageRegexList;
        }

        /// <summary>
        /// スキルのダメージのパターン(サモン)リストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogSummonSkillDamageRegexList()
        {
            LinkedList<Regex> ChatLogSummonSkillDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.Summon))
                {
                    ChatLogSummonSkillDamageRegexList.AddLast(new Regex("^(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")が使用した(?<SkillName2>.+)(\\sエフェクト|)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));
                }
            }

            return ChatLogSummonSkillDamageRegexList;
        }

        /// <summary>
        /// サモンスキル(攻撃対象固定)のダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogSkillSummon2DamageRegexList()
        {
            LinkedList<Regex> ChatLogSkillSummon2DamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.Summon))
                {
                    ChatLogSkillSummon2DamageRegexList.AddLast(new Regex("^(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")が使用した(?<SkillName2>" + _Skill.Name.Replace(" ", "\\s") + ")(\\sエフェクト|)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));
                }
            }

            return ChatLogSkillSummon2DamageRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのパターン(自分)のパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDelayDamageWithoutSourceNameRegexList()
        {
            LinkedList<Regex> AttackSkillDelayDamageWithoutSourceNameRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    if (_Skill.Job == AION.JobType.Melody)
                    {
                        AttackSkillDelayDamageWithoutSourceNameRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillDelayDamageMelodyWithoutSourceNamePattern.Replace("[[[DelayDamageSkillName]]]", _Skill.Name), RegexOptions.Compiled));
                    }
                    else
                    {
                        AttackSkillDelayDamageWithoutSourceNameRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillDelayDamageWithoutSourceNamePattern.Replace("[[[DelayDamageSkillName]]]", _Skill.Name), RegexOptions.Compiled));
                    }
                }

            }

            return AttackSkillDelayDamageWithoutSourceNameRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのパターン(他人)のパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDelayDamageWithSourceNameRegexList()
        {
            LinkedList<Regex> AttackSkillDelayDamageWithSourceNameRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    if (_Skill.Job == AION.JobType.Melody)
                    {
                        AttackSkillDelayDamageWithSourceNameRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillDelayDamageMelodyWithSourceNamePattern.Replace("[[[DelayDamageSkillName]]]", _Skill.Name), RegexOptions.Compiled));
                    }
                    else
                    {
                        AttackSkillDelayDamageWithSourceNameRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillDelayDamageWithSourceNamePattern.Replace("[[[DelayDamageSkillName]]]", _Skill.Name), RegexOptions.Compiled));
                    }
                }
            }

            return AttackSkillDelayDamageWithSourceNameRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDelayDamageDamageRegexList()
        {
            LinkedList<Regex> AttackSkillDelayDamageDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    AttackSkillDelayDamageDamageRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillDelayDamageDamagePattern.Replace("[[[DelayDamageSkillName]]]", _Skill.Name), RegexOptions.Compiled));
                }
            }

            return AttackSkillDelayDamageDamageRegexList;
        }

        /// <summary>
        /// エフェクトダメージスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillEffectDamageDamageRegexList()
        {
            LinkedList<Regex> AttackSkillEffectDamageDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.EffectDamage))
                {
                    AttackSkillEffectDamageDamageRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillEffectDamageDamagePattern.Replace("[[[EffectDamageSkillName]]]", _Skill.Name), RegexOptions.Compiled));
                }
            }

            return AttackSkillEffectDamageDamageRegexList;
        }

        /// <summary>
        /// 回避/抵抗した/されたスキルのパターン(他人)
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogCharacterEvasionResistanceRegexList()
        {
            LinkedList<Regex> ChatLogCharacterEvasionResistanceRegexList = new LinkedList<Regex>();

            foreach (string MemberName in this.MemberNameMemberUnitList.Keys)
            {
                ChatLogCharacterEvasionResistanceRegexList.AddLast(new Regex("^(?<SourceName>" + MemberName + ")が(?<TargetName>.+)の(?<SkillName>.+)(を回避|に抵抗)しました。", RegexOptions.Compiled));
                ChatLogCharacterEvasionResistanceRegexList.AddLast(new Regex("^(?<SourceName>.+)が(?<TargetName>" + MemberName + ")の(?<SkillName>.+)(を回避|に抵抗)しました。", RegexOptions.Compiled));
            }

            return ChatLogCharacterEvasionResistanceRegexList;
        }

        /// <summary>
        /// 攻撃のアップデート
        /// </summary>
        /// <param name="ChatLogActionData"></param>
        public void UpdateDamageData(ActionData ChatLogActionData)
        {
            bool UpdateTotalDamageFlag = false;

            try
            {
                if (this.LimitedTargetCheckBox.Checked && !String.IsNullOrEmpty(this.LimitedTargetNameComboBox.Text))
                {
                    if (!ChatLogActionData.TargetName.Equals(this.LimitedTargetNameComboBox.Text))
                    {
                        return;
                    }
                }

                // エフェクトダメージスキルのダメージ
                if (AION.CheckSkillEffectDamage(ChatLogActionData.SkillName))
                {
                    AION.JobType Job = AION.SkillList[ChatLogActionData.SkillName].Job;

                    if (this.EnableJobRadioButton.Checked && this.JobTypeNumberOfMemberList[Job] == 1 && this.JobTypeNumberOfMemberList[AION.JobType.None] == 0)
                    {
                        // メンバーのダメージを更新
                        foreach (MemberUnit _MemberUnit in this.MemberNameMemberUnitList.Values)
                        {
                            if (_MemberUnit.GetJob() == Job)
                            {
                                this.UpdateTotalDamage(ChatLogActionData.Damage);
                                _MemberUnit.AddDamage(ChatLogActionData.Damage, ChatLogActionData.IsSkill, ChatLogActionData.CriticalHit, ChatLogActionData.Time);
                                UpdateTotalDamageFlag = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // スキル一覧のダメージを更新
                        this.UpdateTotalDamage(ChatLogActionData.Damage);
                        this.SkillUnitList[ChatLogActionData.SkillName].UpdateDamage(ChatLogActionData.Damage);
                        UpdateTotalDamageFlag = true;
                    }
                }
                // サモンスキルのダメージ
                else if (this.SkillUnitList.ContainsKey(ChatLogActionData.SourceName) && AION.CheckSkillSummon(ChatLogActionData.SourceName))
                {
                    AION.JobType Job = AION.SkillList[ChatLogActionData.SourceName].Job;

                    if (this.EnableJobRadioButton.Checked && this.JobTypeNumberOfMemberList[Job] == 1 && this.JobTypeNumberOfMemberList[AION.JobType.None] == 0)
                    {
                        // メンバーのダメージを更新
                        foreach (MemberUnit _MemberUnit in this.MemberNameMemberUnitList.Values)
                        {
                            if (_MemberUnit.GetJob() == Job)
                            {
                                this.UpdateTotalDamage(ChatLogActionData.Damage);
                                _MemberUnit.AddDamage(ChatLogActionData.Damage, ChatLogActionData.IsSkill, ChatLogActionData.CriticalHit, ChatLogActionData.Time);
                                UpdateTotalDamageFlag = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // スキル一覧のダメージを更新
                        this.UpdateTotalDamage(ChatLogActionData.Damage);
                        this.SkillUnitList[ChatLogActionData.SourceName].UpdateDamage(ChatLogActionData.Damage);
                        UpdateTotalDamageFlag = true;
                    }
                }
                // その他のダメージ
                else if (this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.SourceName))
                {
                    this.UpdateTotalDamage(ChatLogActionData.Damage);
                    this.MemberNameMemberUnitList[ChatLogActionData.SourceName].AddDamage(ChatLogActionData.Damage, ChatLogActionData.IsSkill, ChatLogActionData.CriticalHit, ChatLogActionData.Time);
                    UpdateTotalDamageFlag = true;
                }

                if (UpdateTotalDamageFlag)
                {
                    if (this.CalcTimeCheckBox.Checked && !this.CalcTimer.Enabled)
                    {
                        this.CalcTimer.Start();
                    }

                    foreach (MemberUnit _MemberUnit in this.MemberNameMemberUnitList.Values)
                    {
                        _MemberUnit.UpdateDamageParTotalDamage(this.TotalDamage);
                    }

                    if (this.IsDebug && this.DebugLogFileTextWriter != null)
                    {
                        this.DebugLogFileTextWriter.WriteLine(ChatLogActionData.LogText);
                    }
                }
            }
            catch (Exception ex)
            {
                this.PrintExceptionDebugMessage(ex);
            }
        }

        private void UpdateTotalDamage(long TotalDamage)
        {
            this.TotalDamage += TotalDamage;
            this.TotalDamageLabel.Text = this.TotalDamage.ToString("#,0");
        }

        /// <summary>
        /// 回避のアップデート
        /// </summary>
        /// <param name="SourceName"></param>
        /// <param name="TargetName"></param>
        public void UpdateEvasion(ActionData ChatLogActionData)
        {
            try
            {
                if (this.LimitedTargetCheckBox.Checked && !String.IsNullOrEmpty(this.LimitedTargetNameComboBox.Text))
                {
                    if (!ChatLogActionData.SourceName.Equals(this.LimitedTargetNameComboBox.Text) || !ChatLogActionData.TargetName.Equals(this.LimitedTargetNameComboBox.Text))
                    {
                        return;
                    }
                }

                if (this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.SourceName))
                {
                    this.MemberNameMemberUnitList[ChatLogActionData.SourceName].AddEvasion(true, ChatLogActionData.Time);
                }
                else if (this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.TargetName))
                {
                    this.MemberNameMemberUnitList[ChatLogActionData.TargetName].AddEvasion(false, ChatLogActionData.Time);
                }

                if (this.IsDebug && this.DebugLogFileTextWriter != null)
                {
                    this.DebugLogFileTextWriter.WriteLine(ChatLogActionData.LogText);
                }
            }
            catch (Exception ex)
            {
                this.PrintExceptionDebugMessage(ex);
            }
        }

        /// <summary>
        /// 抵抗のアップデート
        /// </summary>
        /// <param name="SourceName"></param>
        /// <param name="TargetName"></param>
        public void UpdateResistance(ActionData ChatLogActionData)
        {
            try
            {
                if (this.LimitedTargetCheckBox.Checked && !String.IsNullOrEmpty(this.LimitedTargetNameComboBox.Text))
                {
                    if (!ChatLogActionData.SourceName.Equals(this.LimitedTargetNameComboBox.Text) || !ChatLogActionData.TargetName.Equals(this.LimitedTargetNameComboBox.Text))
                    {
                        return;
                    }
                }

                if (this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.SourceName))
                {
                    this.MemberNameMemberUnitList[ChatLogActionData.SourceName].AddResistance(true, ChatLogActionData.Time);
                }
                else if (this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.TargetName))
                {
                    this.MemberNameMemberUnitList[ChatLogActionData.TargetName].AddResistance(false, ChatLogActionData.Time);
                }

                if (this.IsDebug && this.DebugLogFileTextWriter != null)
                {
                    this.DebugLogFileTextWriter.WriteLine(ChatLogActionData.LogText);
                }
            }
            catch (Exception ex)
            {
                this.PrintExceptionDebugMessage(ex);
            }
        }

        /// <summary>
        /// 計測時間の1秒間隔のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalcTimer_Elapsed(object sender, EventArgs e)
        {
            this.CalcRemainingTime -= 1;
            this.CalcRemainingTimeLabel.Text = this.CalcRemainingTime.ToString();

            if (this.CalcRemainingTime <= 0)
            {
                this.CalcTimer.Stop();
                this.StopThread();
            }
        }

        /// <summary>
        /// 計測時間の値変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalcTimerMinutesNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            this.CalcRemainingTime = (int)this.CalcTimerMinutesNumericUpDown.Value * 60;
            this.CalcRemainingTimeLabel.Text = this.CalcRemainingTime.ToString();
        }

        /// <summary>
        /// ログファイルから計測イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalcFromLogButton_Click(object sender, EventArgs e)
        {
            if (this.IsRunning == true)
            {
                MessageBox.Show("計測中です。", "エラー");
                return;
            }

            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.FileName = "*.log";
            Dialog.Filter = "ログファイル(*.log)|*.log";
            Dialog.Title = "ログファイルを選択してください";
            Dialog.RestoreDirectory = true;

            if (Dialog.ShowDialog() == DialogResult.OK)
            {
                this.CalcLogFilePath = Dialog.FileName;

                this.StartButton.Enabled = false;
                this.StopButton.Enabled = false;
                this.FavoriteMemberButton.Enabled = false;
                this.IsRunning = true;
                this.StopFlag = false;

                this.ClearData();

                this.TotalDamage = 0;
                this.TotalDamageLabel.Text = "0";
                this.IsCalcLogFile = true;

                // デバッグ
                this.OpenDebugLogFile();

                this.CalculateThread = new Thread(new ThreadStart(Calculate));
                this.CalculateThread.Start();
            }
        }

        /// <summary>
        /// ログファイルから測定終了
        /// </summary>
        public void CalcFromLogEnd()
        {
            this.CloseDebugLogFile();
            this.IsDebug = false;

            this.StopFlag = true;
            this.IsRunning = false;

            this.StartButton.Enabled = true;
            this.StopButton.Enabled = false;
            this.FavoriteMemberButton.Enabled = true;

            this.IsCalcLogFile = false;
        }

        /// <summary>
        /// 最前面表示イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.AlwaysOnTopCheckBox.Checked)
            {
                this.TopMost = true;
                Properties.Settings.Default.AlwaysOnTop = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                this.TopMost = false;
                Properties.Settings.Default.AlwaysOnTop = false;
                Properties.Settings.Default.Save();
            }

        }

        /// <summary>
        /// クライアントを64bitで起動イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AION64bitButton_Click(object sender, EventArgs e)
        {
            string BinRelativePath = "bin64\\aion.bin";
            string Arguments = "-ip:106.186.46.90 -np:106.186.66.51 -ingamebrowser -lbox -loginex -ncping -nosarefund -F2P -hidepromo -logincnt:3 -nobs -npsa -60f2p -dnpshop /SessKey:\"\" /CompanyID:\"14\" /ChannelGroupIndex:\" - 1\"";

            if (File.Exists(Properties.Settings.Default.InstallDirectory + BinRelativePath))
            {
                System.Diagnostics.Process.Start("cmd.exe", "/c start \"\" \"" + Properties.Settings.Default.InstallDirectory + BinRelativePath + "\" " + Arguments);
            }
            else
            {
                FolderBrowserDialog AIONFolder = new FolderBrowserDialog();
                AIONFolder.Description = @"AIONをインストールしたフォルダを選択してください。";
                AIONFolder.Description += Environment.NewLine + @"デフォルト: C:\Program Files (x86)\The Tower of AION\";

                if (AIONFolder.ShowDialog(this) == DialogResult.OK)
                {
                    Properties.Settings.Default.InstallDirectory = AIONFolder.SelectedPath + "\\";
                    Properties.Settings.Default.Save();

                    if (File.Exists(Properties.Settings.Default.InstallDirectory + BinRelativePath))
                    {
                        System.Diagnostics.Process.Start("cmd.exe", "/c start \"\" \"" + Properties.Settings.Default.InstallDirectory + BinRelativePath + "\" " + Arguments);
                    }
                    else
                    {
                        MessageBox.Show("ファイルが存在しません。", "エラー");
                    }
                }
            }
        }

        /// <summary>
        /// メンバー登録画面表示イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FavoriteMemberButton_Click(object sender, EventArgs e)
        {
            if (FavoriteMemberList.Visible)
            {
                FavoriteMemberList.Visible = false;
            }
            else
            {
                FavoriteMemberList.Visible = true;
            }
        }

        /* デバッグ関係 */

        /// <summary>
        /// デバッグ用ログファイルのオープン
        /// </summary>
        private void OpenDebugLogFile()
        {
            try
            {
                if (this.DebugCheckBox.Checked)
                {
                    this.DebugLogFileStreamWriter = new StreamWriter(this.ApplicationDirectory + this.DebugLogFileName, false, Encoding.GetEncoding("shift_jis"));
                    this.DebugLogFileTextWriter = TextWriter.Synchronized(this.DebugLogFileStreamWriter);
                    this.IsDebug = true;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// デバッグ用ログファイルのクローズ
        /// </summary>
        private void CloseDebugLogFile()
        {
            try
            {
                if (this.IsDebug && this.DebugLogFileTextWriter != null)
                {
                    this.DebugLogFileTextWriter.Flush();
                    this.DebugLogFileTextWriter.Close();
                    this.DebugLogFileTextWriter = null;
                }

            }
            catch
            {
            }

            try
            {
                if (this.IsDebug && this.DebugLogFileStreamWriter != null)
                {
                    this.DebugLogFileStreamWriter.Flush();
                    this.DebugLogFileStreamWriter.Close();
                    this.DebugLogFileStreamWriter = null;
                }

            }
            catch
            {
            }
        }

        private void PrintDebugMessage(string Message)
        {
            Debug.WriteLine(Message);
            if (this.IsDebug && this.DebugLogFileTextWriter != null)
            {
                this.DebugLogFileTextWriter.WriteLine(Message);
            }
        }

        private void PrintExceptionDebugMessage(Exception Ex)
        {
            if (this.IsDebug)
            {
                this.PrintDebugMessage(Ex.Message + Environment.NewLine + Ex.StackTrace);
            }
        }
    }
}
