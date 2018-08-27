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
        StreamWriter DebugLogFileStreamWriter = null;
        private string DebugLogFileName = "Debug.log";
        private bool IsDebug = false;

        private string OwnName = "自分";
        private bool IsRunning = false;
        private bool StopFlag = true;
        private Thread CalculateThread = null;

        private Dictionary<string, SkillUnit> SkillUnitList = new Dictionary<string, SkillUnit>();
        private Dictionary<string, MemberUnit> MemberNameMemberUnitList = new Dictionary<string, MemberUnit>();
        private Dictionary<AION.JobType, int> JobTypeNumberOfMemberList = new Dictionary<AION.JobType, int>();

        private Dictionary<string, AION.Skill> SkillList = new Dictionary<string, AION.Skill>();

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
            this.SkillList = AION.GetSkillList();
            this.InitSkillUnit();
            this.FavoriteMemberList.SetMainForm(this);
            this.Member01.SetMemberName(this.OwnName);

            this.AlwaysOnTopCheckBox.Checked = Properties.Settings.Default.AlwaysOnTop;
            this.TopMost = Properties.Settings.Default.AlwaysOnTop;
        }

        private void InitSkillUnit()
        {
            foreach (AION.Skill _Skill in this.SkillList.Values)
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

            try
            {
                if (this.DebugCheckBox.Checked)
                {
                    this.DebugLogFileStreamWriter = new StreamWriter(this.ApplicationDirectory + DebugLogFileName, false, Encoding.GetEncoding("shift_jis"));
                    this.IsDebug = true;
                }
            }
            catch
            {
            }

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

            try
            {
                if (this.IsDebug && this.DebugLogFileStreamWriter != null)
                {
                    this.DebugLogFileStreamWriter.Flush();
                    this.DebugLogFileStreamWriter.Close();
                }

            }
            catch
            {
            }

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
        private static readonly Regex ChatLogLineRegex = new Regex(@"^(20[0-9][0-9]\.[0-9][0-9]\.[0-9][0-9]\s[0-9][0-9]:[0-9][0-9]:[0-9][0-9])\s:\s(.*。)", RegexOptions.Compiled);

        /// <summary>
        /// クリティカルヒットのパターン
        /// </summary>
        private static readonly Regex ChatLogCriticalHitRegex = new Regex(@"^クリティカルヒット！(.*)$", RegexOptions.Compiled);

        /// <summary>
        ///  通常攻撃のダメージのパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogSimpleDamageRegex = new Regex(@"^(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled);

        // 通常攻撃のダメージのパターン(他人)は計測開始時に取得

        // 通常攻撃のダメージのパターン(サモン)は計測開始時に取得

        /// <summary>
        /// クリティカルヒットのダメージのパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogCriticalHitDamageRegex = new Regex(@"^(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)の致命的なダメージを与えました。", RegexOptions.Compiled);

        /// <summary>
        /// スキルダメージのパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogSkillDamageRegex = new Regex(@"^(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled);

        // スキルダメージのパターン(他人)は計測開始時に取得

        // スキルダメージのパターン(サモン)は計測開始時に取得

        /// <summary>
        /// ドットスキルの成功のパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogSkillDotRegex = new Regex(@"^(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)(にダメージを与え続けました。|が出血状態になりました。)", RegexOptions.Compiled);

        /// <summary>
        /// ドットスキルの成功のパターン(他人)
        /// </summary>
        private static readonly Regex ChatLogCharacterSkillDotRegex = new Regex(@"^(?<SourceName>.+)が使用した(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)(はダメージを受け続けました。|は出血状態になりました。)", RegexOptions.Compiled);
            
        // ドットスキルのダメージのパターンは計測開始時に取得

        /// <summary>
        /// サモンスキル(攻撃対象固定)のパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogSkillSummon2Regex = new Regex(@"^サモン：(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)を攻撃する(?<SkillName2>.+)を召喚しました。", RegexOptions.Compiled);

        /// <summary>
        /// サモンスキル(攻撃対象固定)のパターン(他人)
        /// </summary>
        private static readonly Regex ChatLogCharacterSkillSummon2Regex = new Regex(@"^(?<SourceName>.+)が使用したサモン：(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)を攻撃する(?<SkillName2>.+)を召喚しました。", RegexOptions.Compiled);

        // サモンスキル(攻撃対象固定)のダメージのパターンは計測開始時に取得

        /// <summary>
        /// デバフダメージスキルのダメージのパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogSkillDebuffDamage1Regex = new Regex(@"^(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)が(?<Damage>[0-9,]+)のダメージを受け、.+が解除されました。", RegexOptions.Compiled);
        private static readonly Regex ChatLogSkillDebuffDamage2Regex = new Regex(@"^(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与え、(?<SkillName2>.+)効果が生じました。", RegexOptions.Compiled);

        /// <summary>
        /// デバフダメージスキルのダメージのパターン(他人)
        /// </summary>
        private static readonly Regex ChatLogCharacterSkillDebuffDamage1Regex = new Regex(@"^(?<SourceName>.+)が使用した(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)が(?<Damage>[0-9,]+)のダメージを受け、.+が解除されました。", RegexOptions.Compiled);
        private static readonly Regex ChatLogCharacterSkillDebuffDamage2Regex = new Regex(@"^(?<SourceName>.+)が使用した(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージ与え、(?<SkillName2>.+)効果を得ました。", RegexOptions.Compiled);

        // ディレイダメージスキルのパターン(自分)は計測開始時に取得

        // ディレイダメージスキルのパターン(他人)は計測開始時に取得

        // ディレイダメージスキルのダメージのパターンは計測開始時に取得

        // エフェクトダメージスキルのダメージのパターンは計測開始時に取得

        /// <summary>
        /// 反射のダメージのパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogReflectDamageRegex = new Regex(@"^攻撃を反射し、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled);

        /// <summary>
        /// 反射のダメージのパターン(他人)
        /// </summary>
        private static readonly Regex ChatLogReflectDamageCharacterRegex = new Regex(@"^(?<SourceName>.+)が攻撃を反射し、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled);

        /// <summary>
        /// 反射のダメージのパターン(ディシプリン エネルギー)
        /// </summary>
        private static readonly Regex ChatLogReflectDamagDisciplineEnergyRegex = new Regex(@"^ディシプリン\sエネルギーが攻撃を反射し、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled);

        // 回避/抵抗した攻撃のパターン(自分)リスト

        // 回避/抵抗された攻撃のパターン(自分)リスト

        // 回避/抵抗した/された攻撃のパターン(他人)リスト

        /// <summary>
        /// 回避/抵抗のパターン(他人)(その他排除用)
        /// </summary>
        private static readonly Regex ChatLogCharacterOthersEvasionResistanceRegex = new Regex("^(?<SourceName>.+)が(?<TargetName>.+)の(?<SkillName>.+)(を回避|に抵抗)しました。", RegexOptions.Compiled);

        /// <summary>
        /// 回復のパターン
        /// </summary>
        private static readonly Regex ChatLogHealRegex = new Regex(".*回復しました。$", RegexOptions.Compiled);

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
            LinkedList<Regex> ChatLogCharacterSimpleDamageRegexList = this.GetChatLogCharacterSimpleDamageRegexList();

            // 通常攻撃のダメージのパターン(サモン)
            LinkedList<Regex> ChatLogSummonSimpleDamageRegexList = this.GetChatLogSummonSimpleDamageRegexList();

            // スキルダメージのパターン(他人)
            LinkedList<Regex> ChatLogCharacterSkillDamageRegexList = this.GetChatLogCharacterSkillDamageRegexList();

            // スキルのダメージのパターン(サモン)
            LinkedList<Regex> ChatLogSummonSkillDamageRegexList = this.GetChatLogSummonSkillDamageRegexList();

            // ドットスキルのダメージのパターン
            LinkedList<Regex> ChatLogSkillDotDamageRegexList = this.GetChatLogSkillDotDamageRegexList();

            // サモンスキル(攻撃対象固定)のダメージのパターン
            LinkedList<Regex> ChatLogSkillSummon2DamageRegexList = this.GetChatLogSkillSummon2DamageRegexList();

            // ディレイダメージスキルのパターン(自分)
            LinkedList<Regex> ChatLogSkillDelayDamageRegexList = this.GetChatLogSkillDelayDamageRegexList();

            // ディレイダメージスキルのパターン(他人)
            LinkedList<Regex> ChatLogCharacterSkillDelayDamageRegexList = this.GetChatLogCharacterSkillDelayDamageRegexList();

            // ディレイダメージスキルのダメージのパターン
            LinkedList<Regex> ChatLogSkillDelayDamageDamageRegexList = this.GetChatLogSkillDelayDamageDamageRegexList();

            // エフェクトダメージスキルのダメージのパターン
            LinkedList<Regex> ChatLogSkillEffectDamageDamageRegexList = this.GetChatLogSkillEffectDamageDamageRegexList();

            // 回避/抵抗した攻撃のパターン(自分)リスト
            LinkedList<Regex> ChatLogEvadeResistRegexList = this.GetChatLogEvadeRegistRegexList();

            // 回避/抵抗された攻撃のパターン(自分)リスト
            LinkedList<Regex> ChatLogEvadedResistedRegexList = this.GetChatLogEvadedRegistedRegexList();

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


                            //
                            // 回復のパターンは対象外
                            //
                            Match ChatLogHealMatch = ChatLogHealRegex.Match(LogTextWithoutTime);
                            if (ChatLogHealMatch.Success)
                            {
                                continue;
                            }

                            //
                            // クリティカルヒット！
                            //
                            Match ChatLogCriticalHitMatch = ChatLogCriticalHitRegex.Match(LogTextWithoutTime);
                            if (ChatLogCriticalHitMatch.Success)
                            {
                                ChatLogActionData.CriticalHit = true;
                                LogTextWithoutTime = ChatLogCriticalHitMatch.Groups[1].Value;
                            }


                            //
                            // ドットスキルの成功
                            // "^(?<SkillName>.+)の効果により、(?<TargetName>.+)(にダメージを与え続けました。|が出血状態になりました。)
                            // "^(?<SourceName>.+)が使用した(?<SkillName>.+)の効果により、(?<TargetName>.+)(はダメージを受け続けました。|は出血状態になりました。)
                            //
                            Match ChatLogCharacterSkillDotMatch = ChatLogCharacterSkillDotRegex.Match(LogTextWithoutTime);
                            Match ChatLogSkillDotMatch = ChatLogSkillDotRegex.Match(LogTextWithoutTime);
                            if (ChatLogCharacterSkillDotMatch.Success || ChatLogSkillDotMatch.Success)
                            {
                                Match _Match = null;
                                if (ChatLogCharacterSkillDotMatch.Success)
                                {
                                    _Match = ChatLogCharacterSkillDotMatch;
                                    ChatLogActionData.SourceName = _Match.Groups["SourceName"].Value;
                                }
                                else
                                {
                                    _Match = ChatLogSkillDotMatch;
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


                            // 
                            // 反射のダメージ
                            // "^ディシプリン\sエネルギーが攻撃を反射し、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            // "^(?<SourceName>.+)が攻撃を反射し、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            // "^攻撃を反射し、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            //
                            Match ChatLogReflectDamagDisciplineEnergyMatch = ChatLogReflectDamagDisciplineEnergyRegex.Match(LogTextWithoutTime);
                            Match ChatLogReflectDamageCharacterMatch = ChatLogReflectDamageCharacterRegex.Match(LogTextWithoutTime);
                            Match ChatLogReflectDamageMatch = ChatLogReflectDamageRegex.Match(LogTextWithoutTime);
                            if (ChatLogReflectDamagDisciplineEnergyMatch.Success || ChatLogReflectDamageCharacterMatch.Success || ChatLogReflectDamageMatch.Success)
                            {
                                Match _Match = null;
                                if (ChatLogReflectDamagDisciplineEnergyMatch.Success)
                                {
                                    ChatLogActionData.SourceName = "ディシプリン エネルギー";
                                    ChatLogActionData.SkillName = "ディシプリン エネルギー";
                                    _Match = ChatLogReflectDamageCharacterMatch;
                                }
                                else if (ChatLogReflectDamageCharacterMatch.Success)
                                {
                                    ChatLogActionData.SourceName = ChatLogReflectDamageCharacterMatch.Groups["SourceName"].Value;
                                    _Match = ChatLogReflectDamageCharacterMatch;
                                }
                                else
                                {
                                    ChatLogActionData.SourceName = this.OwnName;
                                    _Match = ChatLogReflectDamageMatch;
                                }

                                ChatLogActionData.TargetName = _Match.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(_Match.Groups["Damage"].Value.Replace(",", ""));

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }


                            //
                            // デバフダメージスキルのダメージ
                            // "^(?<SkillName>.+)の効果により、(?<TargetName>.+)が(?<Damage>[0-9,]+)のダメージを受け、.+が解除されました。"
                            // "^(?<SkillName>.+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与え、(?<SkillName2>.+)効果が生じました。"
                            // "^(?<SourceName>.+)が使用した(?<SkillName>.+)の効果により、(?<TargetName>.+)が(?<Damage>[0-9,]+)のダメージを受け、.+が解除されました。"
                            // "^(?<SourceName>.+)が使用した(?<SkillName>.+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージ与え、(?<SkillName2>.+)効果を得ました。"
                            //
                            Match ChatLogCharacterSkillDebuffDamage1Match = ChatLogCharacterSkillDebuffDamage1Regex.Match(LogTextWithoutTime);
                            Match ChatLogCharacterSkillDebuffDamage2Match = ChatLogCharacterSkillDebuffDamage2Regex.Match(LogTextWithoutTime);
                            Match ChatLogSkillDebuffDamage1Match = ChatLogSkillDebuffDamage1Regex.Match(LogTextWithoutTime);
                            Match ChatLogSkillDebuffDamage2Match = ChatLogSkillDebuffDamage2Regex.Match(LogTextWithoutTime);
                            if (ChatLogCharacterSkillDebuffDamage1Match.Success ||
                                ChatLogCharacterSkillDebuffDamage2Match.Success ||
                                ChatLogSkillDebuffDamage1Match.Success ||
                                ChatLogSkillDebuffDamage2Match.Success)
                            {
                                Match _Match = null;
                                if (ChatLogCharacterSkillDebuffDamage1Match.Success || ChatLogCharacterSkillDebuffDamage2Match.Success)
                                {
                                    if (ChatLogCharacterSkillDebuffDamage1Match.Success)
                                    {
                                        _Match = ChatLogCharacterSkillDebuffDamage1Match;
                                    }
                                    else
                                    {
                                        _Match = ChatLogCharacterSkillDebuffDamage2Match;
                                    }
                                    ChatLogActionData.SourceName = _Match.Groups["SourceName"].Value;
                                }
                                else
                                {
                                    if (ChatLogSkillDebuffDamage1Match.Success)
                                    {
                                        _Match = ChatLogSkillDebuffDamage1Match;
                                    }
                                    else
                                    {
                                        _Match = ChatLogSkillDebuffDamage2Match;
                                    }
                                    ChatLogActionData.SourceName = this.OwnName;
                                }

                                ChatLogActionData.SkillName = _Match.Groups["SkillName"].Value;
                                ChatLogActionData.TargetName = _Match.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(_Match.Groups["Damage"].Value.Replace(",", ""));

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }


                            //
                            // エフェクトダメージスキルのダメージ
                            // "^(?<TargetName>.+)は(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")の効果により、(?<Damage>[0-9,]+)のダメージを受けました。"
                            //
                            bool ChatLogSkillEffectDamageDamageMatchFlag = false;
                            foreach (Regex ChatLogSkillEffectDamageDamageRegex in ChatLogSkillEffectDamageDamageRegexList)
                            {
                                Match ChatLogSkillEffectDamageDamageMatch = ChatLogSkillEffectDamageDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogSkillEffectDamageDamageMatch.Success)
                                {
                                    ChatLogSkillEffectDamageDamageMatchFlag = true;
                                    ChatLogActionData.SkillName = ChatLogSkillEffectDamageDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = ChatLogSkillEffectDamageDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(ChatLogSkillEffectDamageDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);
                                }
                            }
                            if (ChatLogSkillEffectDamageDamageMatchFlag)
                            {
                                continue;
                            }


                            //
                            // ディレイダメージスキルのダメージ
                            // "^(?<TargetName>.+)は(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<Damage>[0-9,]+)のダメージを受けました。"
                            //
                            bool ChatLogSkillDelayDamageDamageMatchFlag = false;
                            foreach (Regex ChatLogSkillDelayDamageRegex in ChatLogSkillDelayDamageDamageRegexList)
                            {
                                Match ChatLogSkillDelayDamageMatch = ChatLogSkillDelayDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogSkillDelayDamageMatch.Success)
                                {
                                    ChatLogSkillDelayDamageDamageMatchFlag = true;
                                    ChatLogActionData.TargetName = ChatLogSkillDelayDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.SkillName = ChatLogSkillDelayDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.Damage = long.Parse(ChatLogSkillDelayDamageMatch.Groups["Damage"].Value.Replace(",", ""));

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
                            if (ChatLogSkillDelayDamageDamageMatchFlag)
                            {
                                continue;
                            }


                            //
                            // ディレイダメージスキル(他人)
                            // "^(?<SourceName>.+)は(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果を与えました。"
                            // "^(?<SourceName>.+)が使用した(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果が生じました。"
                            //
                            bool ChatLogCharacterSkillDelayDamageMatchFlag = false;
                            foreach (Regex ChatLogCharacterSkillDelayDamageRegex in ChatLogCharacterSkillDelayDamageRegexList)
                            {
                                Match ChatLogCharacterSkillDelayMatch = ChatLogCharacterSkillDelayDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogCharacterSkillDelayMatch.Success)
                                {
                                    ChatLogCharacterSkillDelayDamageMatchFlag = true;
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
                            if (ChatLogCharacterSkillDelayDamageMatchFlag)
                            {
                                continue;
                            }


                            //
                            // ディレイダメージスキル(自分)
                            // "^(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")を使い、(?<TargetName>.+)が(?<SkillName2>.+)効果を受けました。"
                            // "^(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果が生じました。"
                            //
                            bool ChatLogSkillDelayDamageMatchFlag = false;
                            foreach (Regex ChatLogSkillDelayDamageRegex in ChatLogSkillDelayDamageRegexList)
                            {
                                Match ChatLogSkillDelayMatch = ChatLogSkillDelayDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogSkillDelayMatch.Success)
                                {
                                    ChatLogSkillDelayDamageMatchFlag = true;
                                    ChatLogActionData.SourceName = this.OwnName;
                                    ChatLogActionData.SkillName = ChatLogSkillDelayMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = ChatLogSkillDelayMatch.Groups["TargetName"].Value;

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
                            if (ChatLogSkillDelayDamageMatchFlag)
                            {
                                continue;
                            }


                            //
                            // サモン(攻撃対象固定)のダメージ
                            // "^(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")が使用した(?<SkillName2>" + _Skill.Value.Name.Replace(" ", "\\s") + ")(\\sエフェクト|)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            //
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


                            //
                            // ドットスキルのダメージ
                            // "^(?<TargetName>.+)は(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<Damage>[0-9,]+)のダメージを受けました。"
                            //
                            bool ChatLogSkillDotDamageMatchFlag = false;
                            foreach (Regex ChatLogSkillDotDamageRegex in ChatLogSkillDotDamageRegexList)
                            {
                                Match ChatLogSkillDotDamageMatch = ChatLogSkillDotDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogSkillDotDamageMatch.Success)
                                {
                                    ChatLogSkillDotDamageMatchFlag = true;
                                    ChatLogActionData.SkillName = ChatLogSkillDotDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = ChatLogSkillDotDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(ChatLogSkillDotDamageMatch.Groups["Damage"].Value.Replace(",", ""));

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
                            if (ChatLogSkillDotDamageMatchFlag)
                            {
                                continue;
                            }


                            //
                            // スキルのダメージ(サモン)
                            // "^(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")が使用した(?<SkillName2>.+)(\\sエフェクト|)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            //
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


                            //
                            // スキルのダメージ(他人)
                            // "^(?<SourceName>" + MemberName + ")が使用した(?<SkillName>.+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            // "^(?<SourceName>.+)が使用した(?<SkillName>.+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            //
                            bool ChatLogCharacterSkillDamageMatchFlag = false;
                            foreach (Regex ChatLogCharacterSkillDamageRegex in ChatLogCharacterSkillDamageRegexList)
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


                            //
                            // スキルのダメージ(自分)
                            // "^(?<SkillName>.+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            //
                            Match ChatLogSkillDamageMatch = ChatLogSkillDamageRegex.Match(LogTextWithoutTime);
                            if (ChatLogSkillDamageMatch.Success)
                            {
                                ChatLogActionData.SourceName = this.OwnName;
                                ChatLogActionData.SkillName = ChatLogSkillDamageMatch.Groups["SkillName"].Value;
                                ChatLogActionData.TargetName = ChatLogSkillDamageMatch.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(ChatLogSkillDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }


                            //
                            // 通常攻撃のダメージ(サモン)
                            // "^(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")が(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            //
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

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                    break;
                                }
                            }
                            if (ChatLogSummonSimpleDamageMatchFlag)
                            {
                                continue;
                            }


                            //
                            // クリティカルヒットのダメージ(自分)
                            // @"^(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)の致命的なダメージを与えました。"
                            //
                            Match ChatLogCriticalHitDamageMatch = ChatLogCriticalHitDamageRegex.Match(LogTextWithoutTime);
                            if (ChatLogCriticalHitDamageMatch.Success)
                            {
                                ChatLogActionData.SourceName = this.OwnName;
                                ChatLogActionData.SkillName = ChatLogCriticalHitDamageMatch.Groups["SkillName"].Value;
                                ChatLogActionData.TargetName = ChatLogCriticalHitDamageMatch.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(ChatLogCriticalHitDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }


                            //
                            // 通常攻撃のダメージ(他人)
                            // "^(?<SourceName>" + MemberName + ")が(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            // "^(?<SourceName>.+)が(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            //
                            bool ChatLogCharacterSimpleDamageMatchFlag = false;
                            foreach (Regex ChatLogCharacterSimpleDamageRegex in ChatLogCharacterSimpleDamageRegexList)
                            {
                                Match ChatLogCharacterSimpleDamageMatch = ChatLogCharacterSimpleDamageRegex.Match(LogTextWithoutTime);
                                if (ChatLogCharacterSimpleDamageMatch.Success)
                                {
                                    ChatLogCharacterSimpleDamageMatchFlag = true;
                                    ChatLogActionData.SourceName = ChatLogCharacterSimpleDamageMatch.Groups["SourceName"].Value;
                                    ChatLogActionData.SkillName = ChatLogCharacterSimpleDamageMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = ChatLogCharacterSimpleDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(ChatLogCharacterSimpleDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                    break;
                                }
                            }
                            if (ChatLogCharacterSimpleDamageMatchFlag)
                            {
                                continue;
                            }


                            //
                            // 自分 通常攻撃のダメージ
                            // "^(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)のダメージを与えました。"
                            //
                            Match ChatLogSimpleDamageMatch = ChatLogSimpleDamageRegex.Match(LogTextWithoutTime);
                            if (ChatLogSimpleDamageMatch.Success)
                            {
                                ChatLogActionData.SourceName = this.OwnName;
                                ChatLogActionData.SkillName = ChatLogSimpleDamageMatch.Groups["SkillName"].Value;
                                ChatLogActionData.TargetName = ChatLogSimpleDamageMatch.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(ChatLogSimpleDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                continue;
                            }


                            ///
                            /// 回避/抵抗(他人)
                            /// "^(?<SourceName>" + MemberName + ")が(?<TargetName>.+)の(?<SkillName>.+)(を回避|に抵抗)しました。"
                            /// "^(?<SourceName>.+)が(?<TargetName>" + MemberName + ")の(?<SkillName>.+)(を回避|に抵抗)しました。"
                            ///
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


                            ///
                            /// 回避/抵抗(他人)(その他排除用)
                            /// "^(?<SourceName>.+)が(?<TargetName>.+)の(?<SkillName>.+)(を回避|に抵抗)しました。"
                            ///
                            Match ChatLogCharacterOthersEvasionResistanceMatch = ChatLogCharacterOthersEvasionResistanceRegex.Match(LogTextWithoutTime);
                            if (ChatLogCharacterOthersEvasionResistanceMatch.Success)
                            {
                                continue;
                            }


                            ///
                            /// 回避/抵抗された攻撃(自分)
                            /// "^(?<SourceName>.+)が(?<SkillName>[^の]+)(を回避|に抵抗)しました。"
                            ///
                            bool ChatLogEvadedResistedMatchFlag = false;
                            foreach (Regex ChatLogEvadedResistedRegex in ChatLogEvadedResistedRegexList)
                            {
                                Match ChatLogEvadedResistedMatch = ChatLogEvadedResistedRegex.Match(LogTextWithoutTime);
                                if (ChatLogEvadedResistedMatch.Success)
                                {
                                    ChatLogEvadedResistedMatchFlag = true;
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

                                    break;
                                }
                            }
                            if (ChatLogEvadedResistedMatchFlag)
                            {
                                continue;
                            }


                            ///
                            /// 回避/抵抗した攻撃(自分)
                            /// "^(?<TargetName>.+)の(?<SkillName>[^の]+)(を回避|に抵抗)しました。"
                            ///
                            bool ChatLogEvadeResistMatchFlag = false;
                            foreach (Regex ChatLogEvadeResistRegex in ChatLogEvadeResistRegexList)
                            {
                                Match ChatLogEvadeResistMatch = ChatLogEvadeResistRegex.Match(LogTextWithoutTime);
                                if (ChatLogEvadeResistMatch.Success)
                                {
                                    ChatLogEvadeResistMatchFlag = true;
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

                                    break;
                                }
                            }
                            if (ChatLogEvadeResistMatchFlag)
                            {
                                continue;
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.StackTrace);
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
        private LinkedList<Regex> GetChatLogCharacterSimpleDamageRegexList()
        {
            LinkedList<Regex> ChatLogCharacterSimpleDamageRegexList = new LinkedList<Regex>();

            foreach (string MemberName in this.MemberNameMemberUnitList.Keys)
            {
                if (!String.IsNullOrEmpty(MemberName) && !MemberName.Equals(this.OwnName))
                {
                    ChatLogCharacterSimpleDamageRegexList.AddLast(new Regex("^(?<SourceName>" + MemberName + ")が(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));
                }
            }

            ChatLogCharacterSimpleDamageRegexList.AddLast(new Regex("^(?<SourceName>.+)が(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));

            return ChatLogCharacterSimpleDamageRegexList;
        }

        /// <summary>
        /// スキルダメージのパターン(他人)リストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogCharacterSkillDamageRegexList()
        {
            LinkedList<Regex> ChatLogCharacterSkillDamageRegexList = new LinkedList<Regex>();

            foreach (string MemberName in this.MemberNameMemberUnitList.Keys)
            {
                if (!String.IsNullOrEmpty(MemberName) && !MemberName.Equals(this.OwnName))
                {

                    ChatLogCharacterSkillDamageRegexList.AddLast(new Regex("^(?<SourceName>" + MemberName + @")が使用した(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));
                }
            }

            ChatLogCharacterSkillDamageRegexList.AddLast(new Regex(@"^(?<SourceName>.+)が使用した(?<SkillName>[\p{IsKatakana}\s]+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));

            return ChatLogCharacterSkillDamageRegexList;
        }

        /// <summary>
        /// ドットスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogSkillDotDamageRegexList()
        {
            LinkedList<Regex> ChatLogSkillDotDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in this.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.Dot))
                {
                    ChatLogSkillDotDamageRegexList.AddLast(new Regex("^(?<TargetName>.+)(は|が)(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")の効果により、(?<Damage>[0-9,]+)の(出血|)ダメージを受けました。", RegexOptions.Compiled));
                }
            }

            return ChatLogSkillDotDamageRegexList;
        }

        /// <summary>
        /// 通常攻撃のダメージのパターン(サモン)リストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogSummonSimpleDamageRegexList()
        {
            LinkedList<Regex> ChatLogSummonSimpleDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in this.SkillList.Values)
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

            foreach (AION.Skill _Skill in this.SkillList.Values)
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

            foreach (AION.Skill _Skill in this.SkillList.Values)
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
        private LinkedList<Regex> GetChatLogSkillDelayDamageRegexList()
        {
            LinkedList<Regex> ChatLogSkillDelayDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in this.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    if (_Skill.Job == AION.JobType.Melody)
                    {
                        ChatLogSkillDelayDamageRegexList.AddLast(new Regex("^(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")を使い、(?<TargetName>.+)が(?<SkillName2>.+)効果を受けました。", RegexOptions.Compiled));
                    }
                    else
                    {
                        ChatLogSkillDelayDamageRegexList.AddLast(new Regex("^(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果が生じました。", RegexOptions.Compiled));
                    }
                }

            }

            return ChatLogSkillDelayDamageRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのパターン(他人)のパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogCharacterSkillDelayDamageRegexList()
        {
            LinkedList<Regex> ChatLogCharacterSkillDelayDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in this.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    if (_Skill.Job == AION.JobType.Melody)
                    {
                        ChatLogCharacterSkillDelayDamageRegexList.AddLast(new Regex("^(?<SourceName>.+)は(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果を与えました。", RegexOptions.Compiled));
                    }
                    else
                    {
                        ChatLogCharacterSkillDelayDamageRegexList.AddLast(new Regex("^(?<SourceName>.+)が使用した(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果が生じました。", RegexOptions.Compiled));
                    }
                }
            }

            return ChatLogCharacterSkillDelayDamageRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogSkillDelayDamageDamageRegexList()
        {
            LinkedList<Regex> ChatLogSkillDelayDamageDamageRegexList = new LinkedList<Regex>();

            foreach (KeyValuePair<string, AION.Skill> _Skill in this.SkillList)
            {
                if (_Skill.Value.SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    ChatLogSkillDelayDamageDamageRegexList.AddLast(new Regex("^(?<TargetName>.+)は(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")(\\sエフェクト|)の効果により、(?<Damage>[0-9,]+)のダメージを受けました。", RegexOptions.Compiled));
                }
            }

            return ChatLogSkillDelayDamageDamageRegexList;
        }

        /// <summary>
        /// エフェクトダメージスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogSkillEffectDamageDamageRegexList()
        {
            LinkedList<Regex> ChatLogSkillEffectDamageDamageRegexList = new LinkedList<Regex>();

            foreach (AION.Skill _Skill in this.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(AION.SkillType.EffectDamage))
                {
                    ChatLogSkillEffectDamageDamageRegexList.AddLast(new Regex("^(?<TargetName>.+)は(?<SkillName>" + _Skill.Name.Replace(" ", "\\s") + ")の効果により、(?<Damage>[0-9,]+)のダメージを受けました。", RegexOptions.Compiled));
                }
            }

            return ChatLogSkillEffectDamageDamageRegexList;
        }

        /// <summary>
        /// 回避/抵抗した攻撃のパターン(自分)
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogEvadeRegistRegexList()
        {
            LinkedList<Regex> ChatLogEvadeRegexList = new LinkedList<Regex>();

            foreach (string MemberName in this.MemberNameMemberUnitList.Keys)
            {
                ChatLogEvadeRegexList.AddLast(new Regex("^(?<TargetName>.+)の(?<SkillName>[^の]+)(を回避|に抵抗)しました。", RegexOptions.Compiled));
            }

            return ChatLogEvadeRegexList;
        }

        /// <summary>
        /// 回避/抵抗された攻撃のパターン(自分)
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetChatLogEvadedRegistedRegexList()
        {
            LinkedList<Regex> ChatLogEvadedRegistedRegex = new LinkedList<Regex>();

            foreach (string MemberName in this.MemberNameMemberUnitList.Keys)
            {
                ChatLogEvadedRegistedRegex.AddLast(new Regex("^(?<SourceName>.+)が(?<SkillName>[^の]+)(を回避|に抵抗)しました。", RegexOptions.Compiled));
            }

            return ChatLogEvadedRegistedRegex;
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
        /// スキルがドットスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        private bool CheckSkillDot(string SkillName)
        {
            try
            {
                if (SkillList.ContainsKey(SkillName) && SkillList[SkillName].SkillType.Equals(AION.SkillType.Dot))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// スキルがエフェクトダメージスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        private bool CheckSkillEffectDamage(string SkillName)
        {
            try
            {
                if (SkillList.ContainsKey(SkillName) && SkillList[SkillName].SkillType.Equals(AION.SkillType.EffectDamage))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// スキルがサモンスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        private bool CheckSkillSummon(string SkillName)
        {
            try
            {
                if (SkillList.ContainsKey(SkillName) && SkillList[SkillName].SkillType.Equals(AION.SkillType.Summon))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// スキルがディレイダメージスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        private bool CheckSkillDelayDamage(string SkillName)
        {
            try
            {
                if (SkillList.ContainsKey(SkillName) && SkillList[SkillName].SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 攻撃のアップデート
        /// </summary>
        /// <param name="ChatLogActionData"></param>
        public void UpdateDamageData(ActionData ChatLogActionData)
        {
            bool UpdateTotalDamageFlag = false;

            if (this.LimitedTargetCheckBox.Checked && !String.IsNullOrEmpty(this.LimitedTargetNameComboBox.Text))
            {
                if (!ChatLogActionData.TargetName.Equals(this.LimitedTargetNameComboBox.Text))
                {
                    return;
                }
            }

            // エフェクトダメージスキルのダメージ
            if (this.CheckSkillEffectDamage(ChatLogActionData.SkillName))
            {
                AION.JobType Job = this.SkillList[ChatLogActionData.SkillName].Job;

                if (this.EnableJobRadioButton.Checked && this.JobTypeNumberOfMemberList[Job] == 1 && this.JobTypeNumberOfMemberList[AION.JobType.None] == 0)
                {
                    // メンバーのダメージを更新
                    foreach (MemberUnit _MemberUnit in this.MemberNameMemberUnitList.Values)
                    {
                        if (_MemberUnit.GetJob() == Job)
                        {
                            this.UpdateTotalDamage(ChatLogActionData.Damage);
                            _MemberUnit.AddDamage(ChatLogActionData.Damage, ChatLogActionData.CriticalHit, ChatLogActionData.Time);
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
            else if (this.SkillUnitList.ContainsKey(ChatLogActionData.SourceName) && this.CheckSkillSummon(ChatLogActionData.SourceName))
            {
                AION.JobType Job = this.SkillList[ChatLogActionData.SourceName].Job;

                if (this.EnableJobRadioButton.Checked && this.JobTypeNumberOfMemberList[Job] == 1 && this.JobTypeNumberOfMemberList[AION.JobType.None] == 0)
                {
                    // メンバーのダメージを更新
                    foreach (MemberUnit _MemberUnit in this.MemberNameMemberUnitList.Values)
                    {
                        if (_MemberUnit.GetJob() == Job)
                        {
                            this.UpdateTotalDamage(ChatLogActionData.Damage);
                            _MemberUnit.AddDamage(ChatLogActionData.Damage, ChatLogActionData.CriticalHit, ChatLogActionData.Time);
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
                this.MemberNameMemberUnitList[ChatLogActionData.SourceName].AddDamage(ChatLogActionData.Damage, ChatLogActionData.CriticalHit, ChatLogActionData.Time);
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

                if (this.IsDebug && this.DebugLogFileStreamWriter != null)
                {
                    this.DebugLogFileStreamWriter.WriteLine(ChatLogActionData.LogText);
                }
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

            if (this.IsDebug && this.DebugLogFileStreamWriter != null)
            {
                this.DebugLogFileStreamWriter.WriteLine(ChatLogActionData.LogText);
            }
        }

        /// <summary>
        /// 抵抗のアップデート
        /// </summary>
        /// <param name="SourceName"></param>
        /// <param name="TargetName"></param>
        public void UpdateResistance(ActionData ChatLogActionData)
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

            if (this.IsDebug && this.DebugLogFileStreamWriter != null)
            {
                this.DebugLogFileStreamWriter.WriteLine(ChatLogActionData.LogText);
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

                try
                {
                    if (this.DebugCheckBox.Checked)
                    {
                        this.DebugLogFileStreamWriter = new StreamWriter(this.ApplicationDirectory + DebugLogFileName, false, Encoding.GetEncoding("shift_jis"));
                        this.IsDebug = true;
                    }
                }
                catch
                {
                }

                this.CalculateThread = new Thread(new ThreadStart(Calculate));
                this.CalculateThread.Start();
            }
        }

        /// <summary>
        /// ログファイルから測定終了
        /// </summary>
        public void CalcFromLogEnd()
        {
            try
            {
                if (this.IsDebug && this.DebugLogFileStreamWriter != null)
                {
                    this.DebugLogFileStreamWriter.Flush();
                    this.DebugLogFileStreamWriter.Close();
                }

            }
            catch
            {
            }

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
    }
}
