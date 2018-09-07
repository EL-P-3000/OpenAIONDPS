using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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

        /* メンバー＆スキル一覧 */
        private string OwnName = "自分";
        private string SimpleDamageName = "通常攻撃";
        private Dictionary<string, MemberUnit> MemberNameMemberUnitList = new Dictionary<string, MemberUnit>();
        private Dictionary<AION.JobType, int> JobTypeNumberOfMemberList = new Dictionary<AION.JobType, int>();
        private Dictionary<string, SkillUnit> SkillUnitList = new Dictionary<string, SkillUnit>();

        /* 時間計測 */
        private long TotalDamage = 0;
        private System.Timers.Timer CalcTimer = new System.Timers.Timer();
        private int CalcRemainingTime = 0;

        /* ログファイルからの計測 */
        private bool IsCalcLogFile = false;
        private string CalcLogFilePath = "";

        /* ホットキー */
        [DllImport("user32.dll")]
        extern static int RegisterHotKey(IntPtr hWnd, int id, int modKey, int key);
        [DllImport("user32.dll")]
        extern static int UnregisterHotKey(IntPtr hWnd, int id);
        private const int HOTKEY_ID = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int WM_HOTKEY = 0x0312;

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

            RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL, (int)Keys.F1);
        }

        private void InitSkillUnit()
        {
            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                SkillUnit _SkillUnit = new SkillUnit();
                _SkillUnit.SetJob(_Skill.Job);
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

            UnregisterHotKey(Handle, HOTKEY_ID);
        }

        private void ClearData(bool IsDeleteMemberName)
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

                    if (IsDeleteMemberName)
                    {
                        if (!String.IsNullOrEmpty(MemberName) && !MemberName.Equals(this.OwnName))
                        {
                            _MemberUnit.SetMemberName("");
                            _MemberUnit.SetJobType(AION.JobType.None);
                        }
                    }
                    else
                    {
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
            }

            foreach (Control _Control in this.SkillListFlowLayoutPanel.Controls)
            {
                if (_Control.GetType().Name.Equals("SkillUnit"))
                {
                    SkillUnit _SkillUnit = (SkillUnit)_Control;
                    _SkillUnit.Clear();
                }
            }

            this.SkillDamageListDataGridView.Rows.Clear();
        }

        private void OpenLogFileButton_Click(object sender, EventArgs e)
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
            this.OpenLogFileButton.Enabled = false;
            this.CalcFromLogFileButton.Enabled = false;
            this.FavoriteMemberButton.Enabled = false;
            this.IsRunning = true;
            this.StopFlag = false;

            this.ClearData(false);

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
            bool IsSuccess = false;
            int ExceptionCounter = 3;

            while (!IsSuccess)
            {
                try
                {
                    StreamWriter ChatLogFileStreamWriter = new StreamWriter(Properties.Settings.Default.ChatLogPath);
                    ChatLogFileStreamWriter.Write("");
                    ChatLogFileStreamWriter.Flush();
                    ChatLogFileStreamWriter.Close();
                    IsSuccess = true;
                }
                catch
                {
                    ExceptionCounter -= 1;
                    if (ExceptionCounter <= 0)
                    {
                        IsSuccess = true;
                    }
                    Thread.Sleep(50);
                }
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (this.CalcTimer.Enabled)
            {
                this.StopCalcTimer();
            }
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
            this.OpenLogFileButton.Enabled = true;
            this.CalcFromLogFileButton.Enabled = true;
            this.FavoriteMemberButton.Enabled = true;
        }

        protected override void WndProc(ref Message message)
        {

            base.WndProc(ref message);

            if (message.Msg == WM_HOTKEY)
            {
                if (((int)message.WParam) == HOTKEY_ID)
                {
                    if (this.IsRunning)
                    {
                        this.StopThread();
                    }
                    else
                    {
                        this.StartThread();
                    }
                }
            }
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

        private static readonly Regex AttackDamageToRegex = new Regex(AION.LogPattern.AttackDamageToPattern, RegexOptions.Compiled);

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
        /// ドットスキルの成功のパターン(自分)(メロディ)
        /// </summary>
        private static readonly Regex AttackSkillDotEffectMelodyWithoutSourceNameRegex = new Regex(AION.LogPattern.AttackSkillDotEffectMelodyWithoutSourceNamePattern, RegexOptions.Compiled);

        /// <summary>
        /// ドットスキルの成功のパターン(他人)
        /// </summary>
        private static readonly Regex AttackSkillDotEffectWithSourceNameRegex = new Regex(AION.LogPattern.AttackSkillDotEffectWithSourceNamePattern, RegexOptions.Compiled);

        /// <summary>
        /// ドットスキルの成功のパターン(他人)(メロディ)
        /// </summary>
        private static readonly Regex AttackSkillDotEffectMelodyWithSourceNameRegex = new Regex(AION.LogPattern.AttackSkillDotEffectMelodyWithSourceNamePattern, RegexOptions.Compiled);

        // ドットスキルのダメージのパターンは計測開始時に取得

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
        private static readonly Regex EvasionResistanceWithOthersRegex = new Regex(AION.LogPattern.EvasionResistanceWithOthersPattern, RegexOptions.Compiled);

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
            Delegate CalcFromLogEndDelegate = new Action(CalcFromLogFileEnd);
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
            LinkedList<Regex> AttackSimpleDamageWithSummonRegexList = this.GetAttackSimpleDamageWithSummonRegexList();

            // スキルダメージのパターン(他人)
            LinkedList<Regex> AttackSkillDamageWithSourceNameRegexList = this.GetAttackSkillDamageWithSourceNameRegexList();

            // スキルのダメージのパターン(サモン)
            LinkedList<Regex> AttackSkillDamageWithSummonRegexList = this.GetAttackSkillDamageWithSummonRegexList();

            // ドットスキルのダメージのパターン
            LinkedList<Regex> AttackSkillDotDamageRegexList = this.GetAttackSkillDotDamageRegexList();

            // スキル攻撃(スキル固定)のダメージのパターン(サモン)
            LinkedList<Regex> AttackSkillDamageFixedSkillWithSummonRegexList = this.GetAttackSkillDamageFixedSkillWithSummonRegexList();

            // ディレイダメージスキルのパターン(自分)
            LinkedList<Regex> AttackSkillDelayDamageWithoutSourceNameRegexList = this.GetAttackSkillDelayDamageWithoutSourceNameRegexList();

            // ディレイダメージスキルのパターン(他人)
            LinkedList<Regex> AttackSkillDelayDamageWithSourceNameRegexList = this.GetAttackSkillDelayDamageWithSourceNameRegexList();

            // ディレイダメージスキルのダメージのパターン
            LinkedList<Regex> AttackSkillDelayDamageDamageRegexList = this.GetAttackSkillDelayDamageDamageRegexList();

            // エフェクトダメージスキルのダメージのパターン
            LinkedList<Regex> AttackSkillEffectDamageDamageRegexList = this.GetAttackSkillEffectDamageDamageRegexList();

            // 回避/抵抗した/された攻撃のパターン(他人)リスト
            LinkedList<Regex> EvasionResistanceWithSourceNameRegexList = this.GetEvasionResistanceWithSourceNameRegexList();

            // ログファイルから計算の場合はログファイルを設定
            if (this.IsCalcLogFile)
            {
                LogFilePath = this.CalcLogFilePath;
            }

            using (FileStream ChatLogFileStream = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader ChatLogStreamReader = new StreamReader(ChatLogFileStream, Encoding.GetEncoding("Shift_JIS")))
                {
                    if (!this.IsCalcLogFile)
                    {
                        ChatLogStreamReader.ReadToEnd();
                    }

                    while (this.StopFlag == false)
                    {
                        try
                        {
                            // ログファイルから計算の場合はファイルの最後で終了
                            if (this.IsCalcLogFile && ChatLogStreamReader.EndOfStream == true)
                            {
                                break;
                            }

                            // ログファイルから計算の場合はイベント処理を挟む
                            if (this.IsCalcLogFile)
                            {
                                Application.DoEvents();
                            }

                            LogText = ChatLogStreamReader.ReadLine();
                            LogTextWithoutTime = "";

                            // ラインの取得
                            if (String.IsNullOrEmpty(LogText))
                            {
                                Application.DoEvents();
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
                                ChatLogActionData.IsCriticalHit = true;
                                LogTextWithoutTime = CriticalHitMatch.Groups[1].Value;
                            }

                            // "ダメージを与えました。"の判定 (負荷軽減のため)
                            Match AttackDamageToMatch = AttackDamageToRegex.Match(LogTextWithoutTime);
                            if (AttackDamageToMatch.Success)
                            {
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

                                // スキル攻撃(固定スキル)のダメージ(サモン)
                                bool AttackSkillDamageFixedSkillWithSummonMatchFlag = false;
                                foreach (Regex AttackSkillDamageFixedSkillWithSummonRegex in AttackSkillDamageFixedSkillWithSummonRegexList)
                                {
                                    Match AttackSkillDamageFixedSkillWithSummonMatch = AttackSkillDamageFixedSkillWithSummonRegex.Match(LogTextWithoutTime);
                                    if (AttackSkillDamageFixedSkillWithSummonMatch.Success)
                                    {
                                        AttackSkillDamageFixedSkillWithSummonMatchFlag = true;
                                        ChatLogActionData.SourceName = AttackSkillDamageFixedSkillWithSummonMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.SkillName = AttackSkillDamageFixedSkillWithSummonMatch.Groups["SkillName2"].Value;
                                        ChatLogActionData.TargetName = AttackSkillDamageFixedSkillWithSummonMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(AttackSkillDamageFixedSkillWithSummonMatch.Groups["Damage"].Value.Replace(",", ""));

                                        this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                        break;
                                    }
                                }
                                if (AttackSkillDamageFixedSkillWithSummonMatchFlag)
                                {
                                    continue;
                                }

                                // スキル攻撃のダメージ(サモン)
                                bool AttackSkillDamageWithSummonMatchFlag = false;
                                foreach (Regex AttackSkillDamageWithSummonRegex in AttackSkillDamageWithSummonRegexList)
                                {
                                    Match AttackSkillDamageWithSummonMatch = AttackSkillDamageWithSummonRegex.Match(LogTextWithoutTime);
                                    if (AttackSkillDamageWithSummonMatch.Success)
                                    {
                                        Debug.WriteLine(LogText);
                                        AttackSkillDamageWithSummonMatchFlag = true;
                                        ChatLogActionData.SourceName = AttackSkillDamageWithSummonMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.SkillName = AttackSkillDamageWithSummonMatch.Groups["SkillName2"].Value;
                                        ChatLogActionData.TargetName = AttackSkillDamageWithSummonMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(AttackSkillDamageWithSummonMatch.Groups["Damage"].Value.Replace(",", ""));

                                        this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                        break;
                                    }
                                }
                                if (AttackSkillDamageWithSummonMatchFlag)
                                {
                                    continue;
                                }

                                // スキル攻撃のダメージ(他人)
                                bool AttackSkillDamageWithSourceNameMatchFlag = false;
                                foreach (Regex AttackSkillDamageWithSourceNameRegex in AttackSkillDamageWithSourceNameRegexList)
                                {
                                    Match AttackSkillDamageWithSourceNameMatch = AttackSkillDamageWithSourceNameRegex.Match(LogTextWithoutTime);
                                    if (AttackSkillDamageWithSourceNameMatch.Success)
                                    {
                                        AttackSkillDamageWithSourceNameMatchFlag = true;
                                        ChatLogActionData.SourceName = AttackSkillDamageWithSourceNameMatch.Groups["SourceName"].Value;
                                        ChatLogActionData.SkillName = AttackSkillDamageWithSourceNameMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.TargetName = AttackSkillDamageWithSourceNameMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(AttackSkillDamageWithSourceNameMatch.Groups["Damage"].Value.Replace(",", ""));

                                        this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                        break;
                                    }
                                }
                                if (AttackSkillDamageWithSourceNameMatchFlag)
                                {
                                    continue;
                                }

                                // スキル攻撃のダメージ(自分)
                                Match AttackSkillDamageWithoutSourceNameMatch = AttackSkillDamageWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                if (AttackSkillDamageWithoutSourceNameMatch.Success)
                                {
                                    ChatLogActionData.SourceName = this.OwnName;
                                    ChatLogActionData.SkillName = AttackSkillDamageWithoutSourceNameMatch.Groups["SkillName"].Value;
                                    ChatLogActionData.TargetName = AttackSkillDamageWithoutSourceNameMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(AttackSkillDamageWithoutSourceNameMatch.Groups["Damage"].Value.Replace(",", ""));

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                    continue;
                                }

                                // 通常攻撃のダメージ(サモン)
                                bool AttackSimpleDamageWithSummonMatchFlag = false;
                                foreach (Regex AttackSimpleDamageWithSummonRegex in AttackSimpleDamageWithSummonRegexList)
                                {
                                    Match AttackSimpleDamageWithSummonMatch = AttackSimpleDamageWithSummonRegex.Match(LogTextWithoutTime);
                                    if (AttackSimpleDamageWithSummonMatch.Success)
                                    {
                                        Debug.WriteLine(LogText);
                                        AttackSimpleDamageWithSummonMatchFlag = true;
                                        ChatLogActionData.SourceName = AttackSimpleDamageWithSummonMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.TargetName = AttackSimpleDamageWithSummonMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(AttackSimpleDamageWithSummonMatch.Groups["Damage"].Value.Replace(",", ""));
                                        //ChatLogActionData.IsSkill = false;

                                        this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                        break;
                                    }
                                }
                                if (AttackSimpleDamageWithSummonMatchFlag)
                                {
                                    continue;
                                }

                                // 通常攻撃(クリティカルヒット)のダメージ(自分)
                                Match AttackCriticalHitDamageMatch = AttackCriticalHitDamageRegex.Match(LogTextWithoutTime);
                                if (AttackCriticalHitDamageMatch.Success)
                                {
                                    ChatLogActionData.SourceName = this.OwnName;
                                    ChatLogActionData.SkillName = this.SimpleDamageName;
                                    ChatLogActionData.TargetName = AttackCriticalHitDamageMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(AttackCriticalHitDamageMatch.Groups["Damage"].Value.Replace(",", ""));
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
                                        ChatLogActionData.SkillName = this.SimpleDamageName;
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

                                // 通常攻撃のダメージ(自分)
                                Match AttackSimpleDamageWithoutSourceNameMatch = AttackSimpleDamageWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                if (AttackSimpleDamageWithoutSourceNameMatch.Success)
                                {
                                    ChatLogActionData.SourceName = this.OwnName;
                                    ChatLogActionData.SkillName = this.SimpleDamageName;
                                    ChatLogActionData.TargetName = AttackSimpleDamageWithoutSourceNameMatch.Groups["TargetName"].Value;
                                    ChatLogActionData.Damage = long.Parse(AttackSimpleDamageWithoutSourceNameMatch.Groups["Damage"].Value.Replace(",", ""));
                                    ChatLogActionData.IsSkill = false;

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                    continue;
                                }
                            }
                            else
                            {
                                // ドットスキルの成功
                                Match AttackSkillDotEffectWithSourceNameMatch = AttackSkillDotEffectWithSourceNameRegex.Match(LogTextWithoutTime);
                                Match AttackSkillDotEffectMelodyWithSourceNameMatch = AttackSkillDotEffectMelodyWithSourceNameRegex.Match(LogTextWithoutTime);
                                Match AttackSkillDotEffectWithoutSourceNameMatch = AttackSkillDotEffectWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                Match AttackSkillDotEffectMelodyWithoutSourceNameMatch = AttackSkillDotEffectMelodyWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                if (AttackSkillDotEffectWithSourceNameMatch.Success || AttackSkillDotEffectMelodyWithSourceNameMatch.Success || AttackSkillDotEffectWithoutSourceNameMatch.Success || AttackSkillDotEffectMelodyWithoutSourceNameMatch.Success)
                                {
                                    Match _Match = null;
                                    if (AttackSkillDotEffectWithSourceNameMatch.Success)
                                    {
                                        _Match = AttackSkillDotEffectWithSourceNameMatch;
                                        ChatLogActionData.SourceName = _Match.Groups["SourceName"].Value;
                                    }
                                    else if (AttackSkillDotEffectMelodyWithSourceNameMatch.Success)
                                    {
                                        _Match = AttackSkillDotEffectMelodyWithSourceNameMatch;
                                        ChatLogActionData.SourceName = _Match.Groups["SourceName"].Value;
                                    }
                                    else if (AttackSkillDotEffectWithoutSourceNameMatch.Success)
                                    {
                                        _Match = AttackSkillDotEffectWithoutSourceNameMatch;
                                        ChatLogActionData.SourceName = this.OwnName;
                                    }
                                    else
                                    {
                                        _Match = AttackSkillDotEffectMelodyWithoutSourceNameMatch;
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

                                // 回避/抵抗(他人)
                                bool EvasionResistanceWithSourceNameMatchFlag = false;
                                foreach (Regex EvasionResistanceWithSourceNameRegex in EvasionResistanceWithSourceNameRegexList)
                                {
                                    Match EvasionResistanceWithSourceNameMatch = EvasionResistanceWithSourceNameRegex.Match(LogTextWithoutTime);
                                    if (EvasionResistanceWithSourceNameMatch.Success)
                                    {
                                        EvasionResistanceWithSourceNameMatchFlag = true;
                                        ChatLogActionData.SourceName = EvasionResistanceWithSourceNameMatch.Groups["SourceName"].Value;
                                        ChatLogActionData.TargetName = EvasionResistanceWithSourceNameMatch.Groups["TargetName"].Value;

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
                                if (EvasionResistanceWithSourceNameMatchFlag)
                                {
                                    continue;
                                }

                                // 回避/抵抗(他人)(その他排除用)
                                Match EvasionResistancePatternWithSourceNameMatch = EvasionResistanceWithOthersRegex.Match(LogTextWithoutTime);
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

                                    if (LogTextWithoutTime.IndexOf("攻撃") > 0)
                                    {
                                        ChatLogActionData.IsSkill = false;
                                    }

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
            AttackSimpleDamageWithSourceNameRegexList.AddLast(this.GetReplacedMemberNameRegexList(AION.LogPattern.AttackSimpleDamageWithSourceNameReplacedMemberNamePattern));
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
            AttackSkillDamageWithSourceNameRegexList.AddLast(this.GetReplacedMemberNameRegexList(AION.LogPattern.AttackSkillDamageWithSourceNameReplacedMemberNamePattern));
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
            AttackSkillDotDamageRegexList.AddLast(this.GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDotDamagePattern, AION.SkillType.Dot));
            return AttackSkillDotDamageRegexList;
        }

        /// <summary>
        /// 通常攻撃のダメージのパターン(サモン)リストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSimpleDamageWithSummonRegexList()
        {
            LinkedList<Regex> AttackSimpleDamageWithSummonPatternRegexList = new LinkedList<Regex>();
            AttackSimpleDamageWithSummonPatternRegexList.AddLast(this.GetReplacedSkillNameRegex(AION.LogPattern.AttackSimpleDamageWithSummonPattern, AION.SkillType.Summon));
            return AttackSimpleDamageWithSummonPatternRegexList;
        }

        /// <summary>
        /// スキルのダメージのパターン(サモン)リストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDamageWithSummonRegexList()
        {
            LinkedList<Regex> AttackSkillDamageWithSummonRegexList = new LinkedList<Regex>();
            AttackSkillDamageWithSummonRegexList.AddLast(this.GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDamageWithSummonPattern, AION.SkillType.Summon));
            return AttackSkillDamageWithSummonRegexList;
        }

        /// <summary>
        /// サモンスキル(攻撃対象固定)のダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDamageFixedSkillWithSummonRegexList()
        {
            LinkedList<Regex> AttackSkillDamageFixedSkillWithSummonRegexList = new LinkedList<Regex>();
            AttackSkillDamageFixedSkillWithSummonRegexList.AddLast(this.GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDamageFixedSkillWithSummonPattern, AION.SkillType.Summon));
            return AttackSkillDamageFixedSkillWithSummonRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのパターン(自分)のパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDelayDamageWithoutSourceNameRegexList()
        {
            LinkedList<Regex> AttackSkillDelayDamageWithoutSourceNameRegexList = new LinkedList<Regex>();
            AttackSkillDelayDamageWithoutSourceNameRegexList.AddLast(this.GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDelayDamageWithoutSourceNamePattern, AION.SkillType.DelayDamage));
            return AttackSkillDelayDamageWithoutSourceNameRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのパターン(他人)のパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDelayDamageWithSourceNameRegexList()
        {
            LinkedList<Regex> AttackSkillDelayDamageWithSourceNameRegexList = new LinkedList<Regex>();
            AttackSkillDelayDamageWithSourceNameRegexList.AddLast(this.GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDelayDamageWithSourceNamePattern, AION.SkillType.DelayDamage));
            return AttackSkillDelayDamageWithSourceNameRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillDelayDamageDamageRegexList()
        {
            LinkedList<Regex> AttackSkillDelayDamageDamageRegexList = new LinkedList<Regex>();
            AttackSkillDelayDamageDamageRegexList.AddLast(this.GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDelayDamageDamagePattern, AION.SkillType.DelayDamage));
            return AttackSkillDelayDamageDamageRegexList;
        }

        /// <summary>
        /// エフェクトダメージスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSkillEffectDamageDamageRegexList()
        {
            LinkedList<Regex> AttackSkillEffectDamageDamageRegexList = new LinkedList<Regex>();
            AttackSkillEffectDamageDamageRegexList.AddLast(this.GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillEffectDamageDamagePattern, AION.SkillType.EffectDamage));
            return AttackSkillEffectDamageDamageRegexList;
        }

        /// <summary>
        /// 回避/抵抗した/されたスキルのパターン(他人)
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetEvasionResistanceWithSourceNameRegexList()
        {
            LinkedList<Regex> EvasionResistanceWithSourceNameRegexList = new LinkedList<Regex>();
            EvasionResistanceWithSourceNameRegexList.AddLast(this.GetReplacedMemberNameRegexList(AION.LogPattern.EvasionResistanceWithSourceNamePattern));
            EvasionResistanceWithSourceNameRegexList.AddLast(this.GetReplacedMemberNameRegexList(AION.LogPattern.EvadedResistedWithTargetNamePattern));
            return EvasionResistanceWithSourceNameRegexList;
        }

        private Regex GetReplacedMemberNameRegexList(string LogPattern)
        {
            string MemberNameOrString = "";

            foreach (string MemberName in this.MemberNameMemberUnitList.Keys)
            {
                if (!String.IsNullOrEmpty(MemberName) && !MemberName.Equals(this.OwnName))
                {
                    if (String.IsNullOrEmpty(MemberNameOrString))
                    {
                        MemberNameOrString = MemberName;
                    }
                    else
                    {
                        MemberNameOrString += "|" + MemberName;
                    }
                }
            }

            return new Regex(LogPattern.Replace("[[[MemberName]]]", MemberNameOrString), RegexOptions.Compiled);
        }

        private Regex GetReplacedSkillNameRegex(string LogPattern, AION.SkillType SkillType)
        {
            string SkillNameOrString = "";

            foreach (AION.Skill _Skill in AION.SkillList.Values)
            {
                if (_Skill.SkillType.Equals(SkillType))
                {
                    if (String.IsNullOrEmpty(SkillNameOrString))
                    {
                        SkillNameOrString = _Skill.Name.Replace(" ", "\\s");
                    }
                    else
                    {
                        SkillNameOrString += "|" + _Skill.Name.Replace(" ", "\\s");
                    }
                }
            }

            return new Regex(LogPattern.Replace("[[[SkillName]]]", SkillNameOrString), RegexOptions.Compiled);
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
                if (AION.CheckSkillTypeEffectDamage(ChatLogActionData.SkillName))
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
                                _MemberUnit.AddDamage(ChatLogActionData);
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
                else if (this.SkillUnitList.ContainsKey(ChatLogActionData.SourceName) && AION.CheckSkillTypeSummon(ChatLogActionData.SourceName))
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
                                _MemberUnit.AddDamage(ChatLogActionData);
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
                    this.MemberNameMemberUnitList[ChatLogActionData.SourceName].AddDamage(ChatLogActionData);
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
                    this.MemberNameMemberUnitList[ChatLogActionData.SourceName].AddEvasion(true, ChatLogActionData.IsSkill, ChatLogActionData.Time);
                }
                else if (this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.TargetName))
                {
                    this.MemberNameMemberUnitList[ChatLogActionData.TargetName].AddEvasion(false, ChatLogActionData.IsSkill, ChatLogActionData.Time);
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
                    this.MemberNameMemberUnitList[ChatLogActionData.SourceName].AddResistance(true, ChatLogActionData.IsSkill, ChatLogActionData.Time);
                }
                else if (this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.TargetName))
                {
                    this.MemberNameMemberUnitList[ChatLogActionData.TargetName].AddResistance(false, ChatLogActionData.IsSkill, ChatLogActionData.Time);
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

        /* 計測時間 */

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
                this.StopThread();
                this.StopCalcTimer();
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
        /// 計測停止
        /// </summary>
        private void StopCalcTimer()
        {
            if (this.CalcTimer.Enabled)
            {
                this.CalcTimer.Stop();
                this.CalcRemainingTime = (int)this.CalcTimerMinutesNumericUpDown.Value * 60;
                this.CalcRemainingTimeLabel.Text = this.CalcRemainingTime.ToString();
            }
        }

        /* ログファイルからの計測 */

        /// <summary>
        /// ログファイルから計測イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalcFromLogFileButton_Click(object sender, EventArgs e)
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
                this.OpenLogFileButton.Enabled = false;
                this.CalcFromLogFileButton.Enabled = false;
                this.FavoriteMemberButton.Enabled = false;
                this.IsRunning = true;
                this.StopFlag = false;

                this.ClearData(false);

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
        public void CalcFromLogFileEnd()
        {
            this.CloseDebugLogFile();
            this.IsDebug = false;

            this.StopFlag = true;
            this.IsRunning = false;

            this.StartButton.Enabled = true;
            this.StopButton.Enabled = false;
            this.OpenLogFileButton.Enabled = true;
            this.CalcFromLogFileButton.Enabled = true;
            this.FavoriteMemberButton.Enabled = true;

            this.IsCalcLogFile = false;
        }

        /* その他 */

        private void SaveImageButton_Click(object sender, EventArgs e)
        {
            string SaveResultDirectory = Properties.Settings.Default.SaveResultDirectory;

            if (String.IsNullOrEmpty(SaveResultDirectory))
            {
                SaveResultDirectory = this.ApplicationDirectory;
                Properties.Settings.Default.SaveResultDirectory = this.ApplicationDirectory;
                Properties.Settings.Default.Save();
            }

            try
            {
                SaveFileDialog SkillListSaveFileDialog = new SaveFileDialog();
                SkillListSaveFileDialog.FileName = "DPS-Result-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png";
                SkillListSaveFileDialog.InitialDirectory = SaveResultDirectory;
                SkillListSaveFileDialog.Filter = "PNG(*.png)|*.png";
                SkillListSaveFileDialog.Title = "保存先を指定してください。";

                if (SkillListSaveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap SkillListBitmap = new Bitmap(this.Width, this.Height);
                    this.DrawToBitmap(SkillListBitmap, new Rectangle(0, 0, this.Width, this.Height));
                    SkillListBitmap.Save(SkillListSaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);

                    Properties.Settings.Default.SaveResultDirectory = Path.GetDirectoryName(SkillListSaveFileDialog.FileName) + "\\";
                    Properties.Settings.Default.Save();
                }
            }
            catch
            {
            }
        }

        private void CopyResultButton_Click(object sender, EventArgs e)
        {
            this.CopyResult(false);
        }

        private void CopyResultForSkypeButton_Click(object sender, EventArgs e)
        {
            this.CopyResult(true);
        }

        private void CopyResult(bool IsSkype)
        {
            string MemberResults = "";
            string SkillResults = "";

            if (this.MemberNameMemberUnitList != null && this.MemberNameMemberUnitList.Count > 0 && this.SkillUnitList != null && this.SkillUnitList.Count > 0)
            {
                foreach (MemberUnit _MemberUnit in this.MemberNameMemberUnitList.Values)
                {
                    string MemberResult = "";

                    if (IsSkype)
                    {
                        MemberResult = _MemberUnit.GetResultForSkype();
                    }
                    else
                    {
                        MemberResult = _MemberUnit.GetResult();
                    }

                    if (String.IsNullOrEmpty(MemberResult))
                    {
                        continue;
                    }

                    if (!String.IsNullOrEmpty(MemberResults))
                    {
                        MemberResults += Environment.NewLine;
                        MemberResults += Environment.NewLine;
                    }
                    MemberResults += MemberResult;
                }

                MemberResults += Environment.NewLine;
                MemberResults += Environment.NewLine;
                MemberResults += "■総ダメージ合計： " + this.TotalDamageLabel.Text;

                foreach (SkillUnit _SkillUnit in this.SkillUnitList.Values)
                {
                    string SkillResult = _SkillUnit.GetResult();

                    if (String.IsNullOrEmpty(SkillResult))
                    {
                        continue;
                    }

                    if (String.IsNullOrEmpty(SkillResults))
                    {
                        SkillResults += Environment.NewLine;
                        SkillResults += Environment.NewLine;
                        SkillResults += "■以下は使用者を特定できないスキルの一覧";
                    }

                    SkillResults += Environment.NewLine;
                    SkillResults += SkillResult;

                }
            }

            Clipboard.SetText(MemberResults + SkillResults);
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

        private void ClearButton_Click(object sender, EventArgs e)
        {
            this.ClearData(true);
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

        private void ProjectTopLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.ProjectTopLinkLabel.Text);
        }

        private void ProjectManualLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.ProjectManualLinkLabel.Text);
        }

        /* タブ関係 */

        private void MenuTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.MenuTabControl.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    if (this.IsRunning)
                    {
                        MessageBox.Show("計測を停止してから表示してください。", "注意");
                        this.MenuTabControl.SelectedIndex = 0;
                    }
                    else
                    {
                        this.SetSkillDamageList();
                    }
                    break;
                default:
                    return;
            }
        }

        /* スキル別ダメージ一覧 */

        private void SetSkillDamageList()
        {
            this.SkillDamageListDataGridView.Rows.Clear();

            if (this.MemberNameMemberUnitList != null && this.MemberNameMemberUnitList.Count >= 1)
            {
                foreach (MemberUnit _MemberUnit in MemberNameMemberUnitList.Values)
                {
                    Dictionary<string, AION.Skill> SkillList = _MemberUnit.GetSkillList();

                    foreach (AION.Skill _Skill in SkillList.Values)
                    {
                        this.SkillDamageListDataGridView.Rows.Add(
                            new string[] {
                                _MemberUnit.GetMemberName(),
                                _Skill.Name,
                                _Skill.Damage.ToString("#,0"),
                                _Skill.AttackNumber.ToString("#,0"),
                                _Skill.MaxDamage.ToString("#,0"),
                                _Skill.MinDamage.ToString("#,0"),
                                (_Skill.Damage / _Skill.AttackNumber).ToString("#,0"),
                            }
                        );
                    }
                }
            }
        }

        private void SaveSkillListImageButton_Click(object sender, EventArgs e)
        {
            string SaveResultDirectory = Properties.Settings.Default.SaveResultDirectory;

            if (String.IsNullOrEmpty(SaveResultDirectory))
            {
                SaveResultDirectory = this.ApplicationDirectory;
                Properties.Settings.Default.SaveResultDirectory = this.ApplicationDirectory;
                Properties.Settings.Default.Save();
            }

            try
            {
                SaveFileDialog SkillListSaveFileDialog = new SaveFileDialog();
                SkillListSaveFileDialog.FileName = "DPS-Result-Skill-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png";
                SkillListSaveFileDialog.InitialDirectory = SaveResultDirectory;
                SkillListSaveFileDialog.Filter = "PNG(*.png)|*.png";
                SkillListSaveFileDialog.Title = "保存先を指定してください。";

                if (SkillListSaveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.SkillDamageListDataGridView.CurrentCell = null;
                    int OriginalWidth = this.SkillDamageListDataGridView.Width;
                    int OriginalHeight = this.SkillDamageListDataGridView.Height;

                    int ScreenShotWidth = this.SkillDamageListDataGridView.Columns.GetColumnsWidth(DataGridViewElementStates.None);
                    int ScreenShotHeight = this.SkillDamageListDataGridView.Rows.GetRowsHeight(DataGridViewElementStates.None) + this.SkillDamageListDataGridView.Rows[0].Height;
                    this.SkillDamageListDataGridView.ScrollBars = ScrollBars.None;
                    this.SkillDamageListDataGridView.Width = ScreenShotWidth;
                    this.SkillDamageListDataGridView.Height = ScreenShotHeight;

                    Bitmap SkillListBitmap = new Bitmap(ScreenShotWidth, ScreenShotHeight);
                    this.SkillDamageListDataGridView.DrawToBitmap(SkillListBitmap, new Rectangle(0, 0, ScreenShotWidth, ScreenShotHeight));
                    SkillListBitmap.Save(SkillListSaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);

                    this.SkillDamageListDataGridView.Width = OriginalWidth;
                    this.SkillDamageListDataGridView.Height = OriginalHeight;
                    this.SkillDamageListDataGridView.ScrollBars = ScrollBars.Both;

                    Properties.Settings.Default.SaveResultDirectory = Path.GetDirectoryName(SkillListSaveFileDialog.FileName) + "\\";
                    Properties.Settings.Default.Save();
                }
            }
            catch
            {
            }
        }
    }
}
