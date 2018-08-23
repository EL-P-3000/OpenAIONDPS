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
            this.Clear();
        }

        private void Clear()
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
                    _MemberUnit.Clear();
                    if (!String.IsNullOrEmpty(_MemberUnit.GetMemberName()))
                    {
                        this.MemberNameMemberUnitList.Add(_MemberUnit.GetMemberName(), _MemberUnit);
                        if (_MemberUnit.GetJob() != AION.JobType.None)
                        {
                            this.JobTypeNumberOfMemberList[_MemberUnit.GetJob()] += 1;
                        }
                    }
                }
            }

            try
            {
                if (this.DebugCheckBox.Checked)
                {
                    DebugLogFileStreamWriter = new StreamWriter(this.ApplicationDirectory + DebugLogFileName);
                    this.IsDebug = true;
                }
            }
            catch
            {
            }

            this.TotalDamage = 0;
            this.TotalDamageLabel.Text = "0";

            this.ClearChatLogFile();

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
            this.Clear();
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
         * 他人, スキル, ダメージ＆効果  
         * 
         * 自分, スキル, クリティカル    クリティカルヒット！[スキル]の効果により、[ターゲット]に[ダメージ]のダメージを与えました。
         * 他人, スキル, クリティカル    クリティカルヒット！[他人]が使用した[スキル]の効果により、[ターゲット]に[ダメージ]のダメージを与えました。
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
        private static readonly Regex ChatLogLineRegex = new Regex(@"^20[0-9][0-9]\.[0-9][0-9]\.[0-9][0-9]\s[0-9][0-9]:[0-9][0-9]:[0-9][0-9]\s:\s(.*。)", RegexOptions.Compiled);

        /// <summary>
        /// クリティカルヒットのパターン
        /// </summary>
        private static readonly Regex ChatLogCriticalHitRegex = new Regex(@"^クリティカルヒット！(.*)$", RegexOptions.Compiled);

        /// <summary>
        /// 通常ダメージのパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogSimpleDamageRegex = new Regex(@"^(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled);

        /// 通常ダメージのパターン((他人)は計測開始時に取得

        /// <summary>
        /// クリティカルヒットのダメージのパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogCriticalHitDamageRegex = new Regex(@"^(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)の致命的なダメージを与えました。", RegexOptions.Compiled);

        /// <summary>
        /// スキルダメージのパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogSkillDamageRegex = new Regex(@"^(?<SkillName>.+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled);

        /// スキルダメージのパターン(他人)は計測開始時に取得

        /// <summary>
        /// デバフダメージスキルのダメージのパターン
        /// </summary>
        private static readonly Regex ChatLogSkillDebuffDamageRegex = new Regex(@"^(?<SkillName>.+)が使用した(?<SkillName>.+)の効果により、(?<TargetName>.+)が(?<Damage>[0-9,]+)のダメージを受け、.+されました。", RegexOptions.Compiled);

        // ドットスキルのダメージのパターンは計測開始時に取得

        /* 未実装 */
        // サモンスキルのパターン

        /// <summary>
        /// サモンスキル(攻撃対象固定)のパターン(自分)
        /// </summary>
        private static readonly Regex ChatLogSkillSummon2Regex = new Regex(@"^サモン：(?<SkillName>.+)の効果により、(?<TargetName>.+)を攻撃する(?<SkillName2>.+)を召喚しました。", RegexOptions.Compiled);

        /// <summary>
        /// サモンスキル(攻撃対象固定)のパターン(他人)
        /// </summary>
        private static readonly Regex ChatLogCharacterSkillSummon2Regex = new Regex(@"^(?<SourceName>.+)が使用したサモン：(?<SkillName>.+)の効果により、(?<TargetName>.+)を攻撃する(?<SkillName2>.+)を召喚しました。", RegexOptions.Compiled);

        // サモンスキル(攻撃対象固定)のダメージのパターンは計測開始時に取得

        // ディレイダメージスキルのパターン(自分)は計測開始時に取得

        // ディレイダメージスキルのパターン(他人)は計測開始時に取得

        // ディレイダメージスキルのダメージのパターンは計測開始時に取得

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

        /// <summary>
        /// スレッド処理
        /// </summary>
        public void Calculate()
        {
            Delegate UpdateDataDelegate = new Action<ActionData>(UpdateData);
            string LogText = "";
            string LogTextWithoutTime = "";
            ActionData ChatLogActionData = null;

            // ターゲットのデバフリスト
            Dictionary<string, Dictionary<string, ActionData>> SkillDebuffTargetList = new Dictionary<string, Dictionary<string, ActionData>>();

            // ターゲットのディレイダメージスキルリスト
            Dictionary<string, Dictionary<string, LinkedList<ActionData>>> SkillDelayDamageList = new Dictionary<string, Dictionary<string, LinkedList<ActionData>>>();

            // 通常ダメージのパターン(他人)
            LinkedList<Regex> ChatLogCharacterSimpleDamageRegexList = this.GetChatLogCharacterSimpleDamageRegexList();

            // スキルダメージのパターン(他人)
            LinkedList<Regex> ChatLogCharacterSkillDamageRegexList = this.GetChatLogCharacterSkillDamageRegexList();

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

            // 次のラインがデバフかどうかのフラグ
            bool ChatLogNextDebuffFlag = false;

            // 前のChatLogActionData(ChatLogNextDebuffFlag=true用)
            ActionData ChatLogActionDataPrebious = null;

            using (FileStream ChatLogFileStream = new FileStream(Properties.Settings.Default.ChatLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader ChatLogStreamReader = new StreamReader(ChatLogFileStream, Encoding.GetEncoding("Shift_JIS")))
                {
                    while (this.StopFlag == false)
                    {
                        try
                        {
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

                            // 時刻をラインから削除
                            LogTextWithoutTime = ChatLogLineMatch.Groups[1].Value;

                            // クリティカルヒット！
                            Match ChatLogCriticalHitMatch = ChatLogCriticalHitRegex.Match(LogTextWithoutTime);
                            if (ChatLogCriticalHitMatch.Success)
                            {
                                ChatLogActionData.CriticalHit = true;
                                LogTextWithoutTime = ChatLogCriticalHitMatch.Groups[1].Value;
                            }


                            //
                            // デバフ
                            // "^" + ChatLogActionDataPrebious.SkillName + "の効果により、" + ChatLogActionDataPrebious.TargetName + "にダメージを与え続けました。"
                            // "^" + ChatLogActionDataPrebious.SourceName + "が使用した" + ChatLogActionDataPrebious.SkillName + "の効果により、" + ChatLogActionDataPrebious.TargetName + "はダメージを受け続けました。"
                            //
                            if (ChatLogNextDebuffFlag)
                            {
                                ChatLogNextDebuffFlag = false;

                                Regex ChatLogDebuffRegex = null;
                                if (ChatLogActionDataPrebious.SourceName.Equals(this.OwnName))
                                {
                                    ChatLogDebuffRegex = new Regex("^" + ChatLogActionDataPrebious.SkillName + "の効果により、" + ChatLogActionDataPrebious.TargetName + "にダメージを与え続けました。");
                                }
                                else
                                {
                                    ChatLogDebuffRegex = new Regex("^" + ChatLogActionDataPrebious.SourceName + "が使用した" + ChatLogActionDataPrebious.SkillName + "の効果により、" + ChatLogActionDataPrebious.TargetName + "はダメージを受け続けました。");
                                }

                                // デバフ成功のチェック
                                Match ChatLogDebuffMatch = ChatLogDebuffRegex.Match(LogTextWithoutTime);
                                if (ChatLogDebuffMatch.Success)
                                {
                                    // ターゲット存在のチェック
                                    if (SkillDebuffTargetList.ContainsKey(ChatLogActionDataPrebious.TargetName))
                                    {
                                        // デバフ存在のチェック
                                        Dictionary<string, ActionData> DebuffSkillList = SkillDebuffTargetList[ChatLogActionDataPrebious.TargetName];
                                        if (DebuffSkillList.ContainsKey(ChatLogActionDataPrebious.SkillName))
                                        {
                                            DebuffSkillList.Remove(ChatLogActionDataPrebious.SkillName);
                                        }

                                        DebuffSkillList.Add(ChatLogActionDataPrebious.SkillName, ChatLogActionDataPrebious);
                                    }
                                    else
                                    {
                                        Dictionary<string, ActionData> DebuffSkillList = new Dictionary<string, ActionData>();
                                        DebuffSkillList.Add(ChatLogActionDataPrebious.SkillName, ChatLogActionDataPrebious);
                                        SkillDebuffTargetList.Add(ChatLogActionDataPrebious.TargetName, DebuffSkillList);
                                    }

                                    ChatLogActionDataPrebious = null;

                                    continue;
                                }
                            }
                            ChatLogNextDebuffFlag = false;

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

                                            foreach (ActionData ChatLogSkillDelayActionData in ActionDataList)
                                            {
                                                if (((ChatLogActionData.Time.Ticks - ChatLogSkillDelayActionData.Time.Ticks) / 10000000) < 10)
                                                {
                                                    ChatLogActionData.SourceName = ChatLogSkillDelayActionData.SourceName;
                                                    ActionDataList.Remove(ChatLogSkillDelayActionData);

                                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);

                                                    break;
                                                }
                                                ActionDataList.Remove(ChatLogSkillDelayActionData);
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
                            // "^(?<SourceName>.+)が使用した(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果が生じました。"
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
                            // "(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果が生じました。"
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
                            // "^(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")が使用した(?<SkillName2>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。"
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
                            // デバフダメージスキルのダメージ
                            // "^(?<SkillName>.+)が使用した(?<SkillName>.+)の効果により、(?<TargetName>.+)が(?<Damage>[0-9,]+)のダメージを受け、.+されました。"
                            //
                            Match ChatLogSkillDebuffDamageMatch = ChatLogSkillDebuffDamageRegex.Match(LogTextWithoutTime);
                            if (ChatLogSkillDebuffDamageMatch.Success)
                            {
                                ChatLogActionData.SourceName = ChatLogSkillDebuffDamageMatch.Groups["SourceName"].Value;
                                ChatLogActionData.SkillName = ChatLogSkillDebuffDamageMatch.Groups["SkillName"].Value;
                                ChatLogActionData.TargetName = ChatLogSkillDebuffDamageMatch.Groups["TargetName"].Value;
                                ChatLogActionData.Damage = long.Parse(ChatLogSkillDebuffDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                this.Invoke(UpdateDataDelegate, ChatLogActionData);

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

                                    if (this.CheckSkillDot(ChatLogActionData.SkillName))
                                    {
                                        ChatLogNextDebuffFlag = true;
                                        ChatLogActionDataPrebious = ChatLogActionData;

                                        this.Invoke(UpdateDataDelegate, ChatLogActionData);
                                    }
                                    else
                                    {
                                        this.Invoke(UpdateDataDelegate, ChatLogActionData);
                                    }

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

                                if (this.CheckSkillDot(ChatLogActionData.SkillName))
                                {
                                    ChatLogNextDebuffFlag = true;
                                    ChatLogActionDataPrebious = ChatLogActionData;

                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);
                                }
                                else
                                {
                                    this.Invoke(UpdateDataDelegate, ChatLogActionData);
                                }

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
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.StackTrace);
                        }
                    }
                }
            }

            this.IsRunning = false;
        }

        // [味方]が[ターゲット]に[ダメージ]のダメージを与えました。
        public LinkedList<Regex> GetChatLogCharacterSimpleDamageRegexList()
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


        // [味方]が使用した[スキル]の効果により、[ターゲット]に[ダメージ]のダメージを与えました。
        public LinkedList<Regex> GetChatLogCharacterSkillDamageRegexList()
        {
            LinkedList<Regex> ChatLogCharacterSkillDamageRegexList = new LinkedList<Regex>();

            foreach (string MemberName in this.MemberNameMemberUnitList.Keys)
            {
                if (!String.IsNullOrEmpty(MemberName) && !MemberName.Equals(this.OwnName))
                {

                    ChatLogCharacterSkillDamageRegexList.AddLast(new Regex("^(?<SourceName>" + MemberName + ")が使用した(?<SkillName>.+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));
                }
            }

            ChatLogCharacterSkillDamageRegexList.AddLast(new Regex("^(?<SourceName>.+)が使用した(?<SkillName>.+)の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));

            return ChatLogCharacterSkillDamageRegexList;
        }

        /// <summary>
        /// ドットスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        public LinkedList<Regex> GetChatLogSkillDotDamageRegexList()
        {
            LinkedList<Regex> ChatLogSkillDotDamageRegexList = new LinkedList<Regex>();

            foreach (KeyValuePair<string, AION.Skill> _Skill in this.SkillList)
            {
                if (_Skill.Value.SkillType.Equals(AION.SkillType.Dot))
                {
                    ChatLogSkillDotDamageRegexList.AddLast(new Regex("^(?<TargetName>.+)は(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<Damage>[0-9,]+)のダメージを受けました。", RegexOptions.Compiled));
                }
            }

            return ChatLogSkillDotDamageRegexList;
        }

        /// <summary>
        /// サモンスキル(攻撃対象固定)のダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        public LinkedList<Regex> GetChatLogSkillSummon2DamageRegexList()
        {
            LinkedList<Regex> ChatLogSkillSummon2DamageRegexList = new LinkedList<Regex>();

            foreach (KeyValuePair<string, AION.Skill> _Skill in this.SkillList)
            {
                if (_Skill.Value.SkillType.Equals(AION.SkillType.Summon))
                {
                    ChatLogSkillSummon2DamageRegexList.AddLast(new Regex("^(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")が使用した(?<SkillName2>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<Damage>[0-9,]+)のダメージを与えました。", RegexOptions.Compiled));
                }
            }

            return ChatLogSkillSummon2DamageRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのパターン(自分)のパターンリストの取得
        /// </summary>
        /// <returns></returns>
        public LinkedList<Regex> GetChatLogSkillDelayDamageRegexList()
        {
            LinkedList<Regex> ChatLogSkillDelayDamageRegexList = new LinkedList<Regex>();

            foreach (KeyValuePair<string, AION.Skill> _Skill in this.SkillList)
            {
                if (_Skill.Value.SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    ChatLogSkillDelayDamageRegexList.AddLast(new Regex("^(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果が生じました。", RegexOptions.Compiled));
                }

            }

            return ChatLogSkillDelayDamageRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのパターン(他人)のパターンリストの取得
        /// </summary>
        /// <returns></returns>
        public LinkedList<Regex> GetChatLogCharacterSkillDelayDamageRegexList()
        {
            LinkedList<Regex> ChatLogCharacterSkillDelayDamageRegexList = new LinkedList<Regex>();

            foreach (KeyValuePair<string, AION.Skill> _Skill in this.SkillList)
            {
                if (_Skill.Value.SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    ChatLogCharacterSkillDelayDamageRegexList.AddLast(new Regex("^(?<SourceName>.+)が使用した(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<TargetName>.+)に(?<SkillName2>.+)効果が生じました。", RegexOptions.Compiled));
                }
            }

            return ChatLogCharacterSkillDelayDamageRegexList;
        }

        /// <summary>
        /// ディレイダメージスキルのダメージのパターンリストの取得
        /// </summary>
        /// <returns></returns>
        public LinkedList<Regex> GetChatLogSkillDelayDamageDamageRegexList()
        {
            LinkedList<Regex> ChatLogSkillDelayDamageDamageRegexList = new LinkedList<Regex>();

            foreach (KeyValuePair<string, AION.Skill> _Skill in this.SkillList)
            {
                if (_Skill.Value.SkillType.Equals(AION.SkillType.DelayDamage))
                {
                    ChatLogSkillDelayDamageDamageRegexList.AddLast(new Regex("^(?<TargetName>.+)は(?<SkillName>" + _Skill.Value.Name.Replace(" ", "\\s") + ")の効果により、(?<Damage>[0-9,]+)のダメージを受けました。", RegexOptions.Compiled));
                }
            }

            return ChatLogSkillDelayDamageDamageRegexList;
        }

        /// <summary>
        /// スキルがドットスキルかをチェック
        /// </summary>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public bool CheckSkillDot(string SkillName)
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
        public bool CheckSkillEffectDamage(string SkillName)
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
        public bool CheckSkillSummon(string SkillName)
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
        public bool CheckSkillDelayDamage(string SkillName)
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






        public void UpdateData(ActionData ChatLogActionData)
        {
            bool UpdateTotalDamageFlag = false;

            if (this.SkillUnitList.ContainsKey(ChatLogActionData.SourceName) &&
                (this.CheckSkillSummon(ChatLogActionData.SourceName) ||  this.CheckSkillEffectDamage(ChatLogActionData.SourceName))
                )
            {
                if (this.EnableJobRadioButton.Checked &&
                    this.JobTypeNumberOfMemberList[this.SkillList[ChatLogActionData.SourceName].Job] == 1 &&
                    this.JobTypeNumberOfMemberList[AION.JobType.None] == 0
                    )
                {
                    foreach (MemberUnit _MemberUnit in this.MemberNameMemberUnitList.Values)
                    {
                        if (_MemberUnit.GetJob() == this.SkillList[ChatLogActionData.SourceName].Job)
                        {
                            this.UpdateTotalDamage(ChatLogActionData.Damage);
                            _MemberUnit.AddDamage(ChatLogActionData.Damage, ChatLogActionData.CriticalHit);
                            UpdateTotalDamageFlag = true;
                            break;
                        }
                    }
                }
                else
                {
                    this.UpdateTotalDamage(ChatLogActionData.Damage);
                    this.SkillUnitList[ChatLogActionData.SourceName].UpdateDamage(ChatLogActionData.Damage);
                    UpdateTotalDamageFlag = true;
                }
            }
            else if (this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.SourceName))
            {
                this.UpdateTotalDamage(ChatLogActionData.Damage);
                this.MemberNameMemberUnitList[ChatLogActionData.SourceName].AddDamage(ChatLogActionData.Damage, ChatLogActionData.CriticalHit);
                UpdateTotalDamageFlag = true;
            }

            if (UpdateTotalDamageFlag)
            {
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

        public void UpdateTotalDamage(long TotalDamage)
        {
            this.TotalDamage += TotalDamage;
            this.TotalDamageLabel.Text = this.TotalDamage.ToString("#,0");
        }





        /// <summary>
        /// クライアントを64bitで起動
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
