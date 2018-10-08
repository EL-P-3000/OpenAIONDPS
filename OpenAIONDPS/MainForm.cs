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
        private bool IsCalculationThreadRunning = false;
        private bool CalculationThreadStopFlag = true;
        private Thread CalculationThread = null;
        public class CalculationThreadSettings
        {
            public bool IsCalcLogFile { get; set; } = false;

            public string CalcLogFilePath { get; set; } = null;

            public bool StartCalcConditionChecked { get; set; } = false;

            public string StartCalcConditionText { get; set; } = null;

            public bool StopCalcConditionChecked { get; set; } = false;

            public string StopCalcConditionText { get; set; } = null;
        }

        /* メンバー＆スキル一覧 */
        private string OwnName = "自分";
        private string SimpleDamageName = "通常攻撃";
        private string ReflectDamageName = "反射";
        private Dictionary<string, MemberUnit> MemberNameMemberUnitList = new Dictionary<string, MemberUnit>();
        private Dictionary<AION.JobType, int> JobTypeNumberOfMemberList = new Dictionary<AION.JobType, int>();
        private Dictionary<string, SkillUnit> AttackSkillUnitList = new Dictionary<string, SkillUnit>();
        private Dictionary<string, SkillUnit> HealSkillUnitList = new Dictionary<string, SkillUnit>();

        /* 時間計測 */
        private long TotalDamage = 0;
        private System.Timers.Timer CalculationTimer = new System.Timers.Timer();
        private int CalculationRemainingTime = 0;

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

            this.AlwaysOnTopCheckBox.Checked = Registry.ReadAlwaysOnTop();
            this.TopMost = this.AlwaysOnTopCheckBox.Checked;

            RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL, (int)Keys.F1);
        }

        private void InitSkillUnit()
        {
            foreach (AION.AttackSkill _Skill in AION.AttackSkillList.Values)
            {
                if (_Skill.SkillType == AION.AttackSkillType.Summon || _Skill.SkillType == AION.AttackSkillType.EffectDamage)
                {
                    SkillUnit _SkillUnit = new SkillUnit();
                    _SkillUnit.SetJob(_Skill.Job);
                    _SkillUnit.SetName(_Skill.Name);

                    this.AttackSkillUnitList.Add(_Skill.Name, _SkillUnit);
                    this.SkillListFlowLayoutPanel.Controls.Add(_SkillUnit);
                }
            }

            foreach (AION.HealSkill _Skill in AION.HealSkillList.Values)
            {
                if (_Skill.SkillType == AION.HealSkillType.Summon || _Skill.SkillType == AION.HealSkillType.EffectHeal)
                {
                    SkillUnit _SkillUnit = new SkillUnit();
                    _SkillUnit.SetJob(_Skill.Job);
                    _SkillUnit.SetName(_Skill.Name);

                    this.HealSkillUnitList.Add(_Skill.Name, _SkillUnit);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopCalculationThread();
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

            foreach (SkillUnit _SkillUnit in this.AttackSkillUnitList.Values)
            {
                _SkillUnit.Clear();
            }

            foreach (SkillUnit _SkillUnit in this.HealSkillUnitList.Values)
            {
                _SkillUnit.Clear();
            }

            this.AttackSkillListDataGridView.Rows.Clear();

            this.HealSkillListDataGridView.Rows.Clear();
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
                Registry.WriteChatLogPath(Dialog.FileName);
                Registry.WriteInstallDirectory(Path.GetDirectoryName(Dialog.FileName) + "\\");
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
                    if (!String.IsNullOrEmpty(_MemberUnit.GetMemberName()) && _MemberUnit.GetMemberName().Equals(Name))
                    {
                        return;
                    }
                }
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

        private void CalculationStartButton_Click(object sender, EventArgs e)
        {
            this.StartCalculationThread(false);
        }

        private void StartCalculationThread(bool IsLogFile)
        {
            CalculationThreadSettings ThreadSettings = new CalculationThreadSettings();

            if (this.IsCalculationThreadRunning == true)
            {
                MessageBox.Show("計測中です。", "エラー");
                return;
            }

            if (!IsLogFile)
            {
                try
                {
                    if (!File.Exists(Registry.ReadChatLogPath()))
                    {
                        MessageBox.Show("ログファイルを選択してください。", "エラー");
                        return;
                    }
                }
                catch
                {
                }

                if (this.CalculationTimeCheckBox.Checked)
                {
                    this.CalculationRemainingTime = (int)this.CalculationTimerMinutesNumericUpDown.Value * 60;
                    this.CalculationRemainingTimeLabel.Text = this.CalculationRemainingTime.ToString();
                    this.CalculationTimer = new System.Timers.Timer();
                    this.CalculationTimer.Enabled = false;
                    this.CalculationTimer.SynchronizingObject = this;
                    this.CalculationTimer.Interval = 1000;
                    this.CalculationTimer.Elapsed += new System.Timers.ElapsedEventHandler(CalculationTimer_Elapsed);
                }
            }
            else
            {
                OpenFileDialog Dialog = new OpenFileDialog();
                Dialog.FileName = "*.log";
                Dialog.Filter = "ログファイル(*.log)|*.log";
                Dialog.Title = "ログファイルを選択してください";
                Dialog.RestoreDirectory = true;

                if (Dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                ThreadSettings.IsCalcLogFile = true;
                ThreadSettings.CalcLogFilePath = Dialog.FileName;
            }

            this.CalculationStartButton.Enabled = false;
            this.CalculationStopButton.Enabled = true;
            this.OpenLogFileButton.Enabled = false;
            this.CalcFromLogFileButton.Enabled = false;
            this.FavoriteMemberButton.Enabled = false;
            this.IsCalculationThreadRunning = true;
            this.CalculationThreadStopFlag = false;
            this.MenuTabControl.SelectedIndex = 0;
            this.FavoriteMemberList.Visible = false;

            this.ClearData(false);

            // デバッグ
            this.OpenDebugLogFile();

            this.TotalDamage = 0;
            this.TotalDamageLabel.Text = "0";

            this.ClearChatLogFile();

            ThreadSettings.StartCalcConditionChecked = this.StartCalcConditionCheckBox.Checked;
            ThreadSettings.StartCalcConditionText = this.StartCalcConditionComboBox.Text;
            ThreadSettings.StopCalcConditionChecked = this.StopCalcConditionCheckBox.Checked;
            ThreadSettings.StopCalcConditionText = this.StopCalcConditionComboBox.Text;

            this.CalculationThread = new Thread(new ParameterizedThreadStart(Calculate));
            this.CalculationThread.Start(ThreadSettings);
        }

        private void ClearChatLogFile()
        {
            bool IsSuccess = false;
            int ExceptionCounter = 3;

            while (!IsSuccess)
            {
                try
                {
                    StreamWriter ChatLogFileStreamWriter = new StreamWriter(Registry.ReadChatLogPath());
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

        private void CalculationStopButton_Click(object sender, EventArgs e)
        {
            if (this.CalculationTimer.Enabled)
            {
                this.StopCalculationTimer();
            }
            this.StopCalculationThread();
        }

        private void StopCalculationThread()
        {
            try
            {
                this.CalculationThreadStopFlag = true;
                if (this.CalculationThread != null)
                {
                    this.CalculationThread.Join(2 * 1000);
                    this.CalculationThread = null;
                }
                this.IsCalculationThreadRunning = false;
            }
            catch
            {
            }

            this.CloseDebugLogFile();
            this.IsDebug = false;

            this.CalculationStartButton.Enabled = true;
            this.CalculationStopButton.Enabled = false;
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
                    if (this.IsCalculationThreadRunning)
                    {
                        this.StopCalculationThread();
                    }
                    else
                    {
                        this.StartCalculationThread(false);
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

        /// <summary>
        /// 通常攻撃のダメージのパターン(サモン)
        /// </summary>
        private static readonly Regex AttackSimpleDamageWithSummonRegex = GetReplacedSkillNameRegex(AION.LogPattern.AttackSimpleDamageWithSummonPattern, AION.AttackSkillType.Summon);

        /// <summary>
        /// 通常攻撃(クリティカルヒット)のダメージのパターン(自分)
        /// </summary>
        private static readonly Regex AttackCriticalHitDamageRegex = new Regex(@"^(?<TargetName>[^、]+)に(?<Damage>[0-9,]+)の致命的なダメージを与えました。", RegexOptions.Compiled);

        /// <summary>
        /// スキルダメージのパターン(自分)
        /// </summary>
        private static readonly Regex AttackSkillDamageWithoutSourceNameRegex = new Regex(AION.LogPattern.AttackSkillDamageWithoutSourceNamePattern, RegexOptions.Compiled);

        // スキルダメージのパターン(他人)は計測開始時に取得

        /// <summary>
        /// スキルのダメージのパターン(サモン)
        /// </summary>
        private static readonly Regex AttackSkillDamageWithSummonRegex = GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDamageWithSummonPattern, AION.AttackSkillType.Summon);

        /// <summary>
        /// スキル攻撃(スキル固定)のダメージのパターン(サモン)
        /// </summary>
        private static readonly Regex AttackSkillDamageFixedSkillWithSummonRegex = GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDamageFixedSkillWithSummonPattern, AION.AttackSkillType.Summon);


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

        /// <summary>
        /// ドットスキルのダメージのパターン
        /// </summary>
        private static readonly Regex AttackSkillDotDamageRegex = GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDotDamagePattern, AION.AttackSkillType.Dot);

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

        /// <summary>
        /// ディレイダメージスキルのパターン(自分)
        /// </summary>
        private static readonly Regex AttackSkillDelayDamageWithoutSourceNameRegex = GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDelayDamageWithoutSourceNamePattern, AION.AttackSkillType.DelayDamage);

        /// <summary>
        /// ディレイダメージスキルのパターン(他人)
        /// </summary>
        private static readonly Regex AttackSkillDelayDamageWithSourceNameRegex = GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDelayDamageWithSourceNamePattern, AION.AttackSkillType.DelayDamage);

        /// <summary>
        /// ディレイダメージスキルのダメージのパターン
        /// </summary>
        private static readonly Regex AttackSkillDelayDamageDamageRegex = GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillDelayDamageDamagePattern, AION.AttackSkillType.DelayDamage);

        /// <summary>
        /// エフェクトダメージスキルのダメージのパターン
        /// </summary>
        private static readonly Regex AttackSkillEffectDamageDamageRegex = GetReplacedSkillNameRegex(AION.LogPattern.AttackSkillEffectDamageDamagePattern, AION.AttackSkillType.EffectDamage);

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

        // 持続回復
        private static readonly Regex HealSkillHotWithTargetNameRegex = new Regex(AION.LogPattern.HealSkillHotWithTargetNamePattern, RegexOptions.Compiled);
        private static readonly Regex HealSkillHotWithoutTargetNameRegex = new Regex(AION.LogPattern.HealSkillHotWithoutTargetNamePattern, RegexOptions.Compiled);

        // サモン
        private static readonly Regex HealSkillSummonWithoutTargetNameRegex = GetReplacedSkillNameRegex(AION.LogPattern.HealSkillSummonWithoutTargetNamePattern, AION.HealSkillType.Summon);
        private static readonly Regex HealSkillSummonWithTargetNameRegex = GetReplacedSkillNameRegex(AION.LogPattern.HealSkillSummonWithTargetNamePattern, AION.HealSkillType.Summon);

        // ディレイ回復
        private static readonly Regex HealSkillDelayHealSelfWithoutSourceNameRegex = new Regex(AION.LogPattern.HealSkillDelayHealSelfWithoutSourceNamePattern, RegexOptions.Compiled);
        private static readonly Regex HealSkillDelayHealWithoutSourceNameRegex = new Regex(AION.LogPattern.HealSkillDelayHealWithoutSourceNamePattern, RegexOptions.Compiled);
        private static readonly Regex HealSkillDelayHealSelfWithSourceNameRegex = new Regex(AION.LogPattern.HealSkillDelayHealSelfWithSourceNamePattern, RegexOptions.Compiled);
        private static readonly Regex HealSkillDelayHealWithSourceNameRegex = new Regex(AION.LogPattern.HealSkillDelayHealWithSourceNamePattern, RegexOptions.Compiled);

        // エフェクト回復
        private static readonly Regex HealSkillEffectWithoutTargetNameRegex = new Regex(AION.LogPattern.HealSkillEffectWithoutTargetNamePattern, RegexOptions.Compiled);
        private static readonly Regex HealSkillEffectWithTargetNameRegex = new Regex(AION.LogPattern.HealSkillEffectWithTargetNamePattern, RegexOptions.Compiled);

        // ポーション
        private static readonly Regex HealPotionWithoutSourceNameRegex = new Regex(AION.LogPattern.HealPotionWithoutSourceNamePattern, RegexOptions.Compiled);
        private static readonly Regex HealPotionWithoutSourceNameRegex2 = new Regex(AION.LogPattern.HealPotionWithoutSourceNamePattern2, RegexOptions.Compiled);
        private static readonly Regex HealPotionWithSourceNameRegex = new Regex(AION.LogPattern.HealPotionWithSourceNamePattern, RegexOptions.Compiled);

        /// <summary>
        /// 計測
        /// </summary>
        public void Calculate(object ThreadSettingsObject)
        {
            Delegate UpdateDamageDelegate = new Action<ActionData>(UpdateDamageData);
            Delegate UpdateEvasionDelegate = new Action<ActionData>(UpdateEvasion);
            Delegate UpdateResistanceDelegate = new Action<ActionData>(UpdateResistance);
            Delegate UpdateHealDelegate = new Action<ActionData>(UpdateHeal);
            Delegate CalcFromLogEndDelegate = new Action(StopCalculationFromLogFile);
            Delegate StopThreadDelegate = new Action(StopCalculationThread);
            string LogFilePath = Registry.ReadChatLogPath();
            string LogText = "";
            string LogTextWithoutTime = "";
            ActionData ChatLogActionData = null;
            ActionData PreviousHealChatLogActionData = null;
            bool IsStartCalcByStartCalcCondition = false;
            Regex StartCalcConditionRegex = null;
            Regex StopCalcConditionRegex = null;
            Match StartCalcConditionMatch = null;
            Match StopCalcConditionMatch = null;

            CalculationThreadSettings ThreadSettings = (CalculationThreadSettings)ThreadSettingsObject;

            // ターゲットのデバフリスト
            Dictionary<string, Dictionary<string, ActionData>> AttackSkillDebuffTargetList = new Dictionary<string, Dictionary<string, ActionData>>();

            // ターゲットのディレイダメージスキルリスト
            Dictionary<string, Dictionary<string, LinkedList<ActionData>>> AttackSkillDelayDamageTargetList = new Dictionary<string, Dictionary<string, LinkedList<ActionData>>>();

            // ヒールの持続回復スキルリスト
            // TargetName, SkillName, ActionData
            Dictionary<string, Dictionary<string, ActionData>> HealSkillContinuousTargetList = new Dictionary<string, Dictionary<string, ActionData>>();

            // 通常攻撃のダメージのパターン(他人)
            LinkedList<Regex> AttackSimpleDamageWithSourceNameRegexList = this.GetAttackSimpleDamageWithSourceNameRegexList();

            // スキルダメージのパターン(他人)
            LinkedList<Regex> AttackSkillDamageWithSourceNameRegexList = this.GetAttackSkillDamageWithSourceNameRegexList();

            // 回避/抵抗した/された攻撃のパターン(他人)リスト
            LinkedList<Regex> EvasionResistanceWithSourceNameRegexList = this.GetEvasionResistanceWithSourceNameRegexList();

            // 回復
            Regex HealSkillWithoutSourceNameRegex = new Regex(AION.LogPattern.HealSkillWithoutSourceNamePattern, RegexOptions.Compiled);
            Regex HealSkillNextLineWithoutSourceNameRegex = GetReplacedMemberNameRegex(AION.LogPattern.HealSkillNextLineWithoutSourceNamePattern);
            Regex HealSkillWithSourceNameRegex = GetReplacedMemberNameRegex(AION.LogPattern.HealSkillWithSourceNamePattern);
            Regex HealSkillNextLineSelfWithSourceNameRegex = GetReplacedMemberNameRegex(AION.LogPattern.HealSkillNextLineSelfWithSourceNamePattern);
            Regex HealSkillNextLineWithSourceNameRegex = GetReplacedMemberNameRegex(AION.LogPattern.HealSkillNextLineWithSourceNamePattern);

            // ログファイルから計算の場合はログファイルを設定
            if (ThreadSettings.IsCalcLogFile)
            {
                LogFilePath = ThreadSettings.CalcLogFilePath;
            }

            try
            {
                using (FileStream ChatLogFileStream = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader ChatLogStreamReader = new StreamReader(ChatLogFileStream, Encoding.GetEncoding("Shift_JIS")))
                    {
                        if (!ThreadSettings.IsCalcLogFile)
                        {
                            ChatLogStreamReader.ReadToEnd();
                        }

                        if (ThreadSettings.StartCalcConditionChecked && !String.IsNullOrEmpty(ThreadSettings.StartCalcConditionText))
                        {
                            StartCalcConditionRegex = new Regex("^" + ThreadSettings.StartCalcConditionText.Replace(" ", "\\s") + "$", RegexOptions.Compiled);
                        }

                        if (ThreadSettings.StopCalcConditionChecked && !String.IsNullOrEmpty(ThreadSettings.StopCalcConditionText))
                        {
                            StopCalcConditionRegex = new Regex("^" + ThreadSettings.StopCalcConditionText.Replace(" ", "\\s") + AION.LogPattern.StopCalcConditionPattern + "$", RegexOptions.Compiled);
                        }

                        while (this.CalculationThreadStopFlag == false)
                        {
                            try
                            {
                                // ログファイルから計算の場合はファイルの最後で終了
                                if (ThreadSettings.IsCalcLogFile && ChatLogStreamReader.EndOfStream == true)
                                {
                                    break;
                                }

                                // ログファイルから計算の場合はイベント処理を挟む
                                if (ThreadSettings.IsCalcLogFile)
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

                                // ログファイルから計算の場合は時刻を取得
                                if (ThreadSettings.IsCalcLogFile)
                                {
                                    ChatLogActionData.Time = DateTime.ParseExact(ChatLogLineMatch.Groups[1].Value, "yyyy.MM.dd HH:mm:ss", null);
                                }

                                // 時刻をラインから削除
                                LogTextWithoutTime = ChatLogLineMatch.Groups[2].Value;

                                // 計測開始条件
                                if (ThreadSettings.StartCalcConditionChecked && !IsStartCalcByStartCalcCondition && StartCalcConditionRegex != null)
                                {
                                    StartCalcConditionMatch = StartCalcConditionRegex.Match(LogTextWithoutTime);
                                    if (StartCalcConditionMatch.Success)
                                    {
                                        IsStartCalcByStartCalcCondition = true;
                                    }
                                    else
                                    {
                                        Application.DoEvents();
                                    }
                                    continue;
                                }

                                // 計測停止条件
                                if (ThreadSettings.StopCalcConditionChecked && StopCalcConditionRegex != null)
                                {
                                    StopCalcConditionMatch = StopCalcConditionRegex.Match(LogTextWithoutTime);
                                    if (StopCalcConditionMatch.Success)
                                    {
                                        this.Invoke(StopThreadDelegate, null);
                                        Application.DoEvents();
                                        break;
                                    }
                                }

                                // 前回復データの処理
                                if (PreviousHealChatLogActionData != null)
                                {
                                    bool IsNormalHeal = false;

                                    // エフェクト
                                    Match PreviousHealSkillNextLineSelfWithSourceNameMatch = HealSkillNextLineSelfWithSourceNameRegex.Match(LogTextWithoutTime);
                                    Match PreviousHealSkillNextLineWithSourceNameMatch = HealSkillNextLineWithSourceNameRegex.Match(LogTextWithoutTime);
                                    if ((PreviousHealSkillNextLineSelfWithSourceNameMatch.Success || PreviousHealSkillNextLineWithSourceNameMatch.Success) && LogTextWithoutTime.IndexOf(PreviousHealChatLogActionData.SkillName) > 0)
                                    {
                                        // 前回復データはヒールスキル
                                        PreviousHealChatLogActionData.SourceName = PreviousHealChatLogActionData.TargetName;
                                        this.Invoke(UpdateHealDelegate, PreviousHealChatLogActionData);

                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.Hot, HealSkillContinuousTargetList);
                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.AreaHot, HealSkillContinuousTargetList);
                                    }
                                    else
                                    {
                                        // 前回復データは持続回復／ディレイ回復
                                        if (HealSkillContinuousTargetList.ContainsKey(PreviousHealChatLogActionData.TargetName))
                                        {
                                            Dictionary<string, ActionData> SkillActionDataList = HealSkillContinuousTargetList[PreviousHealChatLogActionData.TargetName];
                                            if (SkillActionDataList.ContainsKey(PreviousHealChatLogActionData.SkillName))
                                            {
                                                ActionData BuffActionData = SkillActionDataList[PreviousHealChatLogActionData.SkillName];
                                                if ((PreviousHealChatLogActionData.Time.Ticks - BuffActionData.Time.Ticks) / 10000000 < 120)
                                                {
                                                    PreviousHealChatLogActionData.SourceName = BuffActionData.SourceName;
                                                    this.Invoke(UpdateHealDelegate, PreviousHealChatLogActionData);

                                                    if (PreviousHealChatLogActionData.SkillName.Equals("ピュリフィケーション ウェーブ") ||
                                                        PreviousHealChatLogActionData.SkillName.Equals("サルヴェーション ハンド") ||
                                                        PreviousHealChatLogActionData.SkillName.Equals("サルヴェーション スプレンダー")
                                                        )
                                                    {
                                                        SkillActionDataList.Remove(PreviousHealChatLogActionData.SkillName);
                                                    }
                                                }
                                                else
                                                {
                                                    // 基本的に到達することがないのでエラー処理が必要
                                                    SkillActionDataList.Remove(PreviousHealChatLogActionData.SkillName);
                                                }
                                            }
                                            else
                                            {
                                                IsNormalHeal = true;
                                            }
                                        }
                                        else
                                        {
                                            IsNormalHeal = true;
                                        }
                                    }

                                    if (IsNormalHeal)
                                    {
                                        PreviousHealChatLogActionData.SourceName = PreviousHealChatLogActionData.TargetName;
                                        this.Invoke(UpdateHealDelegate, PreviousHealChatLogActionData);
                                    }

                                    PreviousHealChatLogActionData = null;
                                }

                                // 回復
                                Match HealCommonMatch = HealCommonRegex.Match(LogTextWithoutTime);
                                if (HealCommonMatch.Success)
                                {
                                    // ポーション
                                    Match HealPotionWithoutSourceNameMatch = HealPotionWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                    Match HealPotionWithoutSourceNameMatch2 = HealPotionWithoutSourceNameRegex2.Match(LogTextWithoutTime);
                                    Match HealPotionWithSourceNameMatch = HealPotionWithSourceNameRegex.Match(LogTextWithoutTime);
                                    if (HealPotionWithoutSourceNameMatch.Success || HealPotionWithoutSourceNameMatch2.Success || HealPotionWithSourceNameMatch.Success)
                                    {
                                        Match _Match = null;
                                        if (HealPotionWithoutSourceNameMatch.Success)
                                        {
                                            ChatLogActionData.SourceName = this.OwnName;
                                            ChatLogActionData.TargetName = this.OwnName;
                                            _Match = HealPotionWithoutSourceNameMatch;
                                        }
                                        else if (HealPotionWithoutSourceNameMatch2.Success)
                                        {
                                            ChatLogActionData.SourceName = this.OwnName;
                                            ChatLogActionData.TargetName = this.OwnName;
                                            _Match = HealPotionWithoutSourceNameMatch2;
                                        }
                                        else
                                        {
                                            ChatLogActionData.SourceName = HealPotionWithSourceNameMatch.Groups["SourceName"].Value;
                                            ChatLogActionData.TargetName = HealPotionWithSourceNameMatch.Groups["SourceName"].Value;
                                            _Match = HealPotionWithSourceNameMatch;
                                        }
                                        ChatLogActionData.SkillName = "ポーション";
                                        ChatLogActionData.HealingAmount = long.Parse(_Match.Groups["HealingAmount"].Value.Replace(",", ""));

                                        this.Invoke(UpdateHealDelegate, ChatLogActionData);

                                        continue;
                                    }

                                    // エフェクト
                                    Match HealSkillEffectWithoutTargetNameMatch = HealSkillEffectWithoutTargetNameRegex.Match(LogTextWithoutTime);
                                    Match HealSkillEffectWithTargetNameMatch = HealSkillEffectWithTargetNameRegex.Match(LogTextWithoutTime);
                                    if (HealSkillEffectWithoutTargetNameMatch.Success || HealSkillEffectWithTargetNameMatch.Success)
                                    {
                                        Match _Match = null;
                                        if (HealSkillEffectWithoutTargetNameMatch.Success)
                                        {
                                            ChatLogActionData.TargetName = this.OwnName;
                                            _Match = HealSkillEffectWithoutTargetNameMatch;
                                        }
                                        else
                                        {
                                            ChatLogActionData.TargetName = HealSkillEffectWithTargetNameMatch.Groups["TargetName"].Value;
                                            _Match = HealSkillEffectWithTargetNameMatch;
                                        }
                                        ChatLogActionData.SkillName = _Match.Groups["SkillName"].Value;
                                        ChatLogActionData.HealingAmount = long.Parse(_Match.Groups["HealingAmount"].Value.Replace(",", ""));

                                        this.Invoke(UpdateHealDelegate, ChatLogActionData);

                                        continue;
                                    }

                                    // サモン
                                    Match HealSkillSummonWithTargetNameMatch = HealSkillSummonWithTargetNameRegex.Match(LogTextWithoutTime);
                                    Match HealSkillSummonWithoutTargetNameMatch = HealSkillSummonWithoutTargetNameRegex.Match(LogTextWithoutTime);
                                    if (HealSkillSummonWithTargetNameMatch.Success || HealSkillSummonWithoutTargetNameMatch.Success)
                                    {
                                        Match _Match = null;
                                        if (HealSkillSummonWithTargetNameMatch.Success)
                                        {
                                            ChatLogActionData.TargetName = HealSkillSummonWithTargetNameMatch.Groups["TargetName"].Value;
                                            _Match = HealSkillSummonWithTargetNameMatch;
                                        }
                                        else
                                        {
                                            ChatLogActionData.TargetName = this.OwnName;
                                            _Match = HealSkillSummonWithoutTargetNameMatch;
                                        }
                                        ChatLogActionData.SourceName = _Match.Groups["SkillName"].Value;
                                        ChatLogActionData.SkillName = _Match.Groups["SkillName"].Value;
                                        ChatLogActionData.HealingAmount = long.Parse(_Match.Groups["HealingAmount"].Value.Replace(",", ""));

                                        this.Invoke(UpdateHealDelegate, ChatLogActionData);

                                        continue;
                                    }

                                    // 持続回復／ディレイ回復処理(自分)
                                    bool HealSkillContinuousWithoutTargetNameFlag = false;
                                    Match HealSkillContinuousWithoutTargetNameMatch = HealSkillHotWithoutTargetNameRegex.Match(LogTextWithoutTime);
                                    if (HealSkillContinuousWithoutTargetNameMatch.Success)
                                    {
                                        ChatLogActionData.TargetName = this.OwnName;
                                        ChatLogActionData.SkillName = HealSkillContinuousWithoutTargetNameMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.HealingAmount = long.Parse(HealSkillContinuousWithoutTargetNameMatch.Groups["HealingAmount"].Value.Replace(",", ""));

                                        if (HealSkillContinuousTargetList.ContainsKey(ChatLogActionData.TargetName))
                                        {
                                            Dictionary<string, ActionData> SkillActionDataList = HealSkillContinuousTargetList[ChatLogActionData.TargetName];
                                            if (SkillActionDataList.ContainsKey(ChatLogActionData.SkillName))
                                            {
                                                ActionData BuffActionData = SkillActionDataList[ChatLogActionData.SkillName];
                                                if ((ChatLogActionData.Time.Ticks - BuffActionData.Time.Ticks) / 10000000 < 120)
                                                {
                                                    HealSkillContinuousWithoutTargetNameFlag = true;
                                                    ChatLogActionData.SourceName = BuffActionData.SourceName;
                                                    this.Invoke(UpdateHealDelegate, ChatLogActionData);

                                                    if (BuffActionData.SkillName.Equals("ピュリフィケーション ウェーブ") ||
                                                        BuffActionData.SkillName.Equals("サルヴェーション ハンド") ||
                                                        BuffActionData.SkillName.Equals("サルヴェーション スプレンダー")
                                                        )
                                                    {
                                                        SkillActionDataList.Remove(BuffActionData.SkillName);
                                                    }
                                                }
                                                else
                                                {
                                                    SkillActionDataList.Remove(ChatLogActionData.SkillName);
                                                }
                                            }
                                            else
                                            {
                                            }
                                        }
                                        else
                                        {
                                        }
                                    }
                                    if (HealSkillContinuousWithoutTargetNameFlag)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        ChatLogActionData = new ActionData();
                                        ChatLogActionData.LogText = LogText;

                                        // ログファイルから計算の場合は時刻を取得
                                        if (ThreadSettings.IsCalcLogFile)
                                        {
                                            ChatLogActionData.Time = DateTime.ParseExact(ChatLogLineMatch.Groups[1].Value, "yyyy.MM.dd HH:mm:ss", null);
                                        }
                                    }

                                    // 持続回復／ディレイ回復処理(他人)
                                    Match HealSkillContinuousWithTargetNameMatch = HealSkillHotWithTargetNameRegex.Match(LogTextWithoutTime);
                                    if (HealSkillContinuousWithTargetNameMatch.Success)
                                    {
                                        ChatLogActionData.TargetName = HealSkillContinuousWithTargetNameMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.SkillName = HealSkillContinuousWithTargetNameMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.HealingAmount = long.Parse(HealSkillContinuousWithTargetNameMatch.Groups["HealingAmount"].Value.Replace(",", ""));

                                        PreviousHealChatLogActionData = ChatLogActionData;

                                        continue;
                                    }

                                    // 自分が自分に回復スキルを使用
                                    Match HealSkillWithoutSourceNameMatch = HealSkillWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                    if (HealSkillWithoutSourceNameMatch.Success)
                                    {
                                        ChatLogActionData.SourceName = this.OwnName;
                                        ChatLogActionData.TargetName = this.OwnName;
                                        ChatLogActionData.SkillName = HealSkillWithoutSourceNameMatch.Groups["SkillName"].Value;

                                        if (LogTextWithoutTime.IndexOf("回復しました。") > 0)
                                        {
                                            ChatLogActionData.HealingAmount = long.Parse(HealSkillWithoutSourceNameMatch.Groups["HealingAmount"].Value.Replace(",", ""));
                                            this.Invoke(UpdateHealDelegate, ChatLogActionData);
                                        }

                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.Hot, HealSkillContinuousTargetList);
                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.AreaHot, HealSkillContinuousTargetList);

                                        continue;
                                    }

                                    // 自分が他人に回復スキルを使用
                                    Match HealSkillNextLineWithoutSourceNameMatch = HealSkillNextLineWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                    if (HealSkillNextLineWithoutSourceNameMatch.Success)
                                    {
                                        ChatLogActionData.SourceName = this.OwnName;
                                        ChatLogActionData.TargetName = HealSkillNextLineWithoutSourceNameMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.SkillName = HealSkillNextLineWithoutSourceNameMatch.Groups["SkillName"].Value;

                                        if (LogTextWithoutTime.IndexOf("回復しました。") > 0)
                                        {
                                            ChatLogActionData.HealingAmount = long.Parse(HealSkillNextLineWithoutSourceNameMatch.Groups["HealingAmount"].Value.Replace(",", ""));
                                            this.Invoke(UpdateHealDelegate, ChatLogActionData);
                                        }

                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.Hot, HealSkillContinuousTargetList);
                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.AreaHot, HealSkillContinuousTargetList);

                                        continue;
                                    }

                                    // 他人が本人に回復スキルを使用
                                    Match HealSkillWithSourceNameMatch = HealSkillWithSourceNameRegex.Match(LogTextWithoutTime);
                                    if (HealSkillWithSourceNameMatch.Success)
                                    {
                                        ChatLogActionData.SourceName = HealSkillWithSourceNameMatch.Groups["SourceName"].Value;
                                        ChatLogActionData.TargetName = HealSkillWithSourceNameMatch.Groups["SourceName"].Value;
                                        ChatLogActionData.SkillName = HealSkillWithSourceNameMatch.Groups["SkillName"].Value;

                                        if (LogTextWithoutTime.IndexOf("回復しました。") > 0)
                                        {
                                            ChatLogActionData.HealingAmount = long.Parse(HealSkillWithSourceNameMatch.Groups["HealingAmount"].Value.Replace(",", ""));
                                            this.Invoke(UpdateHealDelegate, ChatLogActionData);
                                        }

                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.Hot, HealSkillContinuousTargetList);
                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.AreaHot, HealSkillContinuousTargetList);

                                        continue;
                                    }

                                    // 他人が自分／他人に回復スキルを使用
                                    Match HealSkillNextLineSelfWithSourceNameMatch = HealSkillNextLineSelfWithSourceNameRegex.Match(LogTextWithoutTime);
                                    Match HealSkillNextLineWithSourceNameMatch = HealSkillNextLineWithSourceNameRegex.Match(LogTextWithoutTime);
                                    if (HealSkillNextLineSelfWithSourceNameMatch.Success || HealSkillNextLineWithSourceNameMatch.Success)
                                    {
                                        Match _Match = null;
                                        if (HealSkillNextLineSelfWithSourceNameMatch.Success)
                                        {
                                            ChatLogActionData.SourceName = HealSkillNextLineSelfWithSourceNameMatch.Groups["SourceName"].Value;
                                            ChatLogActionData.TargetName = this.OwnName;
                                            _Match = HealSkillNextLineSelfWithSourceNameMatch;
                                        }
                                        else
                                        {
                                            ChatLogActionData.SourceName = HealSkillNextLineWithSourceNameMatch.Groups["SourceName"].Value;
                                            ChatLogActionData.TargetName = HealSkillNextLineWithSourceNameMatch.Groups["TargetName"].Value;
                                            _Match = HealSkillNextLineWithSourceNameMatch;
                                        }
                                        ChatLogActionData.SkillName = _Match.Groups["SkillName"].Value;

                                        if (LogTextWithoutTime.IndexOf("回復しました。") > 0)
                                        {
                                            ChatLogActionData.HealingAmount = long.Parse(_Match.Groups["HealingAmount"].Value.Replace(",", ""));
                                            this.Invoke(UpdateHealDelegate, ChatLogActionData);
                                        }

                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.Hot, HealSkillContinuousTargetList);
                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.AreaHot, HealSkillContinuousTargetList);

                                        continue;
                                    }

                                    // ディレイ
                                    Match HealSkillDelayHealSelfWithoutSourceNameMatch = HealSkillDelayHealSelfWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                    Match HealSkillDelayHealWithoutSourceNameMatch = HealSkillDelayHealWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                    Match HealSkillDelayHealSelfWithSourceNameMatch = HealSkillDelayHealSelfWithSourceNameRegex.Match(LogTextWithoutTime);
                                    Match HealSkillDelayHealWithSourceNameMatch = HealSkillDelayHealWithSourceNameRegex.Match(LogTextWithoutTime);
                                    if (HealSkillDelayHealSelfWithoutSourceNameMatch.Success || HealSkillDelayHealWithoutSourceNameMatch.Success || HealSkillDelayHealSelfWithSourceNameMatch.Success || HealSkillDelayHealWithSourceNameMatch.Success)
                                    {
                                        Match _Match = null;
                                        if (HealSkillDelayHealSelfWithoutSourceNameMatch.Success)
                                        {
                                            ChatLogActionData.SourceName = this.OwnName;
                                            ChatLogActionData.TargetName = this.OwnName;
                                            _Match = HealSkillDelayHealSelfWithoutSourceNameMatch;
                                        }
                                        else if (HealSkillDelayHealWithoutSourceNameMatch.Success)
                                        {
                                            ChatLogActionData.SourceName = this.OwnName;
                                            ChatLogActionData.TargetName = HealSkillDelayHealWithoutSourceNameMatch.Groups["TargetName"].Value;
                                            _Match = HealSkillDelayHealWithoutSourceNameMatch;
                                        }
                                        else if (HealSkillDelayHealSelfWithSourceNameMatch.Success)
                                        {
                                            ChatLogActionData.SourceName = HealSkillDelayHealSelfWithSourceNameMatch.Groups["SourceName"].Value;
                                            ChatLogActionData.TargetName = HealSkillDelayHealSelfWithSourceNameMatch.Groups["SourceName"].Value;
                                            _Match = HealSkillDelayHealSelfWithSourceNameMatch;
                                        }
                                        else
                                        {
                                            ChatLogActionData.SourceName = HealSkillDelayHealWithSourceNameMatch.Groups["SourceName"].Value;
                                            ChatLogActionData.TargetName = HealSkillDelayHealWithSourceNameMatch.Groups["TargetName"].Value;
                                            _Match = HealSkillDelayHealWithSourceNameMatch;
                                        }
                                        ChatLogActionData.SkillName = _Match.Groups["SkillName"].Value;

                                        this.AddHealBuffList(ChatLogActionData, AION.HealSkillType.Delay, HealSkillContinuousTargetList);

                                        continue;
                                    }

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
                                            ChatLogActionData.SkillName = ReflectDamageName;
                                            _Match = AttackReflectionDamageWithSourceNameMatch;
                                        }
                                        else
                                        {
                                            ChatLogActionData.SourceName = this.OwnName;
                                            ChatLogActionData.SkillName = ReflectDamageName;
                                            _Match = AttackReflectionDamageWithoutSourceNameMatch;
                                        }

                                        ChatLogActionData.TargetName = _Match.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(_Match.Groups["Damage"].Value.Replace(",", ""));

                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

                                        continue;
                                    }

                                    // スキル攻撃(固定スキル)のダメージ(サモン)
                                    Match AttackSkillDamageFixedSkillWithSummonMatch = AttackSkillDamageFixedSkillWithSummonRegex.Match(LogTextWithoutTime);
                                    if (AttackSkillDamageFixedSkillWithSummonMatch.Success)
                                    {
                                        ChatLogActionData.SourceName = AttackSkillDamageFixedSkillWithSummonMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.SkillName = AttackSkillDamageFixedSkillWithSummonMatch.Groups["SkillName2"].Value;
                                        ChatLogActionData.TargetName = AttackSkillDamageFixedSkillWithSummonMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(AttackSkillDamageFixedSkillWithSummonMatch.Groups["Damage"].Value.Replace(",", ""));

                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

                                        continue;
                                    }

                                    // スキル攻撃のダメージ(サモン)
                                    Match AttackSkillDamageWithSummonMatch = AttackSkillDamageWithSummonRegex.Match(LogTextWithoutTime);
                                    if (AttackSkillDamageWithSummonMatch.Success)
                                    {
                                        ChatLogActionData.SourceName = AttackSkillDamageWithSummonMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.SkillName = AttackSkillDamageWithSummonMatch.Groups["SkillName2"].Value;
                                        ChatLogActionData.TargetName = AttackSkillDamageWithSummonMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(AttackSkillDamageWithSummonMatch.Groups["Damage"].Value.Replace(",", ""));

                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

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

                                            this.Invoke(UpdateDamageDelegate, ChatLogActionData);

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

                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

                                        continue;
                                    }

                                    // 通常攻撃のダメージ(サモン)
                                    Match AttackSimpleDamageWithSummonMatch = AttackSimpleDamageWithSummonRegex.Match(LogTextWithoutTime);
                                    if (AttackSimpleDamageWithSummonMatch.Success)
                                    {
                                        ChatLogActionData.SourceName = AttackSimpleDamageWithSummonMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.TargetName = AttackSimpleDamageWithSummonMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(AttackSimpleDamageWithSummonMatch.Groups["Damage"].Value.Replace(",", ""));

                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

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

                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

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

                                            this.Invoke(UpdateDamageDelegate, ChatLogActionData);

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

                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

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
                                        if (AttackSkillDebuffTargetList.ContainsKey(ChatLogActionData.TargetName))
                                        {
                                            // デバフ存在のチェック
                                            Dictionary<string, ActionData> DebuffSkillList = AttackSkillDebuffTargetList[ChatLogActionData.TargetName];
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
                                            AttackSkillDebuffTargetList.Add(ChatLogActionData.TargetName, DebuffSkillList);
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

                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

                                        continue;
                                    }

                                    // エフェクトダメージスキルのダメージ
                                    Match ChatLogSkillEffectDamageDamageMatch = AttackSkillEffectDamageDamageRegex.Match(LogTextWithoutTime);
                                    if (ChatLogSkillEffectDamageDamageMatch.Success)
                                    {
                                        ChatLogActionData.SkillName = ChatLogSkillEffectDamageDamageMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.TargetName = ChatLogSkillEffectDamageDamageMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(ChatLogSkillEffectDamageDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

                                        continue;
                                    }

                                    // ディレイダメージスキルのダメージ
                                    Match AttackSkillDelayDamageDamageMatch = AttackSkillDelayDamageDamageRegex.Match(LogTextWithoutTime);
                                    if (AttackSkillDelayDamageDamageMatch.Success)
                                    {
                                        ChatLogActionData.TargetName = AttackSkillDelayDamageDamageMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.SkillName = AttackSkillDelayDamageDamageMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.Damage = long.Parse(AttackSkillDelayDamageDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                        if (AttackSkillDelayDamageTargetList.ContainsKey(ChatLogActionData.TargetName))
                                        {
                                            Dictionary<string, LinkedList<ActionData>> SkillActionDataList = AttackSkillDelayDamageTargetList[ChatLogActionData.TargetName];
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

                                                        this.Invoke(UpdateDamageDelegate, ChatLogActionData);

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

                                        continue;
                                    }

                                    // ディレイダメージスキル(他人)
                                    Match ChatLogCharacterSkillDelayMatch = AttackSkillDelayDamageWithSourceNameRegex.Match(LogTextWithoutTime);
                                    if (ChatLogCharacterSkillDelayMatch.Success)
                                    {
                                        ChatLogActionData.SourceName = ChatLogCharacterSkillDelayMatch.Groups["SourceName"].Value;
                                        ChatLogActionData.SkillName = ChatLogCharacterSkillDelayMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.TargetName = ChatLogCharacterSkillDelayMatch.Groups["TargetName"].Value;

                                        if (!AttackSkillDelayDamageTargetList.ContainsKey(ChatLogActionData.TargetName))
                                        {
                                            Dictionary<string, LinkedList<ActionData>> _SkillActionDataList = new Dictionary<string, LinkedList<ActionData>>();
                                            AttackSkillDelayDamageTargetList.Add(ChatLogActionData.TargetName, _SkillActionDataList);
                                        }

                                        Dictionary<string, LinkedList<ActionData>> SkillActionDataList = AttackSkillDelayDamageTargetList[ChatLogActionData.TargetName];

                                        if (!SkillActionDataList.ContainsKey(ChatLogActionData.SkillName))
                                        {
                                            LinkedList<ActionData> TempSkillActionList = new LinkedList<ActionData>();
                                            SkillActionDataList.Add(ChatLogActionData.SkillName, TempSkillActionList);
                                        }

                                        LinkedList<ActionData> ActionList = SkillActionDataList[ChatLogActionData.SkillName];
                                        ActionList.AddLast(ChatLogActionData);

                                        continue;
                                    }

                                    // ディレイダメージスキル(自分)
                                    Match AttackSkillDelayDamageWithoutSourceNameMatch = AttackSkillDelayDamageWithoutSourceNameRegex.Match(LogTextWithoutTime);
                                    if (AttackSkillDelayDamageWithoutSourceNameMatch.Success)
                                    {
                                        ChatLogActionData.SourceName = this.OwnName;
                                        ChatLogActionData.SkillName = AttackSkillDelayDamageWithoutSourceNameMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.TargetName = AttackSkillDelayDamageWithoutSourceNameMatch.Groups["TargetName"].Value;

                                        if (!AttackSkillDelayDamageTargetList.ContainsKey(ChatLogActionData.TargetName))
                                        {
                                            Dictionary<string, LinkedList<ActionData>> _SkillActionDataList = new Dictionary<string, LinkedList<ActionData>>();
                                            AttackSkillDelayDamageTargetList.Add(ChatLogActionData.TargetName, _SkillActionDataList);
                                        }

                                        Dictionary<string, LinkedList<ActionData>> SkillActionDataList = AttackSkillDelayDamageTargetList[ChatLogActionData.TargetName];

                                        if (!SkillActionDataList.ContainsKey(ChatLogActionData.SkillName))
                                        {
                                            LinkedList<ActionData> TempSkillActionList = new LinkedList<ActionData>();
                                            SkillActionDataList.Add(ChatLogActionData.SkillName, TempSkillActionList);
                                        }

                                        LinkedList<ActionData> ActionList = SkillActionDataList[ChatLogActionData.SkillName];
                                        ActionList.AddLast(ChatLogActionData);

                                        continue;
                                    }

                                    // ドットスキルのダメージ
                                    Match AttackSkillDotDamageMatch = AttackSkillDotDamageRegex.Match(LogTextWithoutTime);
                                    if (AttackSkillDotDamageMatch.Success)
                                    {
                                        ChatLogActionData.SkillName = AttackSkillDotDamageMatch.Groups["SkillName"].Value;
                                        ChatLogActionData.TargetName = AttackSkillDotDamageMatch.Groups["TargetName"].Value;
                                        ChatLogActionData.Damage = long.Parse(AttackSkillDotDamageMatch.Groups["Damage"].Value.Replace(",", ""));

                                        if (AttackSkillDebuffTargetList.ContainsKey(ChatLogActionData.TargetName))
                                        {
                                            Dictionary<string, ActionData> SkillDebuffList = AttackSkillDebuffTargetList[ChatLogActionData.TargetName];
                                            if (SkillDebuffList.ContainsKey(ChatLogActionData.SkillName))
                                            {
                                                ChatLogActionData.SourceName = SkillDebuffList[ChatLogActionData.SkillName].SourceName;
                                                this.Invoke(UpdateDamageDelegate, ChatLogActionData);
                                            }
                                        }

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
            }
            catch (Exception ex2)
            {
                this.PrintExceptionDebugMessage(ex2);
                this.Invoke(StopThreadDelegate, null);
            }

            if (ThreadSettings.IsCalcLogFile)
            {
                this.Invoke(CalcFromLogEndDelegate);
            }

            this.IsCalculationThreadRunning = false;
        }

        private void AddHealBuffList(ActionData ChatLogActionData, AION.HealSkillType SkillType, Dictionary<string, Dictionary<string, ActionData>> HealSkillHotDelayTargetList)
        {
            if (AION.CheckHealSkillType(ChatLogActionData.SkillName, SkillType))
            {
                if (!HealSkillHotDelayTargetList.ContainsKey(ChatLogActionData.TargetName))
                {
                    HealSkillHotDelayTargetList.Add(ChatLogActionData.TargetName, new Dictionary<string, ActionData>());
                }

                Dictionary<string, ActionData> SkillActionDataList = HealSkillHotDelayTargetList[ChatLogActionData.TargetName];

                if (SkillActionDataList.ContainsKey(ChatLogActionData.SkillName))
                {
                    SkillActionDataList.Remove(ChatLogActionData.SkillName);
                }

                SkillActionDataList.Add(ChatLogActionData.SkillName, ChatLogActionData);
            }
        }

        /// <summary>
        /// 通常攻撃のダメージのパターン(他人)リストの取得
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetAttackSimpleDamageWithSourceNameRegexList()
        {
            LinkedList<Regex> AttackSimpleDamageWithSourceNameRegexList = new LinkedList<Regex>();
            AttackSimpleDamageWithSourceNameRegexList.AddLast(this.GetReplacedMemberNameRegex(AION.LogPattern.AttackSimpleDamageWithSourceNameReplacedMemberNamePattern));
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
            AttackSkillDamageWithSourceNameRegexList.AddLast(this.GetReplacedMemberNameRegex(AION.LogPattern.AttackSkillDamageWithSourceNameReplacedMemberNamePattern));
            AttackSkillDamageWithSourceNameRegexList.AddLast(new Regex(AION.LogPattern.AttackSkillDamageWithSourceNamePattern, RegexOptions.Compiled));
            return AttackSkillDamageWithSourceNameRegexList;
        }

        /// <summary>
        /// 回避/抵抗した/されたスキルのパターン(他人)
        /// </summary>
        /// <returns></returns>
        private LinkedList<Regex> GetEvasionResistanceWithSourceNameRegexList()
        {
            LinkedList<Regex> EvasionResistanceWithSourceNameRegexList = new LinkedList<Regex>();
            EvasionResistanceWithSourceNameRegexList.AddLast(this.GetReplacedMemberNameRegex(AION.LogPattern.EvasionResistanceWithSourceNamePattern));
            EvasionResistanceWithSourceNameRegexList.AddLast(this.GetReplacedMemberNameRegex(AION.LogPattern.EvadedResistedWithTargetNamePattern));
            return EvasionResistanceWithSourceNameRegexList;
        }

        private Regex GetReplacedMemberNameRegex(string LogPattern)
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

        private static Regex GetReplacedSkillNameRegex(string LogPattern, AION.AttackSkillType SkillType)
        {
            string SkillName = "";

            foreach (AION.AttackSkill _Skill in AION.AttackSkillList.Values)
            {
                if (_Skill.SkillType.Equals(SkillType))
                {
                    if (String.IsNullOrEmpty(SkillName))
                    {
                        SkillName = _Skill.Name.Replace(" ", "\\s");
                    }
                    else
                    {
                        SkillName += "|" + _Skill.Name.Replace(" ", "\\s");
                    }
                }
            }

            return new Regex(LogPattern.Replace("[[[SkillName]]]", SkillName), RegexOptions.Compiled);
        }

        private static Regex GetReplacedSkillNameRegex(string LogPattern, AION.HealSkillType SkillType)
        {
            string SkillName = "";

            foreach (AION.HealSkill _Skill in AION.HealSkillList.Values)
            {
                if (_Skill.SkillType.Equals(SkillType))
                {
                    if (String.IsNullOrEmpty(SkillName))
                    {
                        SkillName = _Skill.Name.Replace(" ", "\\s");
                    }
                    else
                    {
                        SkillName += "|" + _Skill.Name.Replace(" ", "\\s");
                    }
                }
            }

            return new Regex(LogPattern.Replace("[[[SkillName]]]", SkillName), RegexOptions.Compiled);
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
                if (AION.CheckAttackSkillTypeEffectDamage(ChatLogActionData.SkillName) || (this.AttackSkillUnitList.ContainsKey(ChatLogActionData.SourceName) && AION.CheckAttackSkillTypeSummon(ChatLogActionData.SourceName)))
                {
                    AION.JobType Job = AION.AttackSkillList[ChatLogActionData.SkillName].Job;

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
                        this.AttackSkillUnitList[ChatLogActionData.SkillName].AddDamage(ChatLogActionData.Damage, ChatLogActionData.IsCriticalHit);
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
                    if (this.CalculationTimeCheckBox.Checked && !this.CalculationTimer.Enabled)
                    {
                        this.CalculationTimer.Start();
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
                    if (!ChatLogActionData.SourceName.Equals(this.LimitedTargetNameComboBox.Text) && !ChatLogActionData.TargetName.Equals(this.LimitedTargetNameComboBox.Text))
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
                    if (!ChatLogActionData.SourceName.Equals(this.LimitedTargetNameComboBox.Text) && !ChatLogActionData.TargetName.Equals(this.LimitedTargetNameComboBox.Text))
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

        public void UpdateHeal(ActionData ChatLogActionData)
        {
            try
            {
                // チェック
                if (!String.IsNullOrEmpty(ChatLogActionData.SourceName) && AION.HealSkillList.ContainsKey(ChatLogActionData.SkillName) && this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.SourceName))
                {
                    if (AION.HealSkillList[ChatLogActionData.SkillName].Job != this.MemberNameMemberUnitList[ChatLogActionData.SourceName].GetJob())
                    {
                        this.PrintDebugMessage("Heal Error: " + ChatLogActionData.SourceName + ", " + ChatLogActionData.LogText);
                        return;
                    }
                }

                // エフェクト／サモン
                if (AION.CheckHealSkillType(ChatLogActionData.SkillName, AION.HealSkillType.EffectHeal) || AION.CheckHealSkillType(ChatLogActionData.SkillName, AION.HealSkillType.Summon))
                {
                    AION.JobType Job = AION.HealSkillList[ChatLogActionData.SkillName].Job;

                    if (this.EnableJobRadioButton.Checked && this.JobTypeNumberOfMemberList[Job] == 1 && this.JobTypeNumberOfMemberList[AION.JobType.None] == 0)
                    {
                        // メンバーのダメージを更新
                        foreach (MemberUnit _MemberUnit in this.MemberNameMemberUnitList.Values)
                        {
                            if (_MemberUnit.GetJob() == Job)
                            {
                                _MemberUnit.AddHeal(ChatLogActionData);
                                break;
                            }
                        }
                    }
                    else
                    {
                        // スキル一覧のダメージを更新
                        if (this.HealSkillUnitList.ContainsKey(ChatLogActionData.SkillName))
                        {
                            this.HealSkillUnitList[ChatLogActionData.SkillName].AddHeal(ChatLogActionData.HealingAmount);
                        }
                    }
                }
                else
                {
                    if (this.MemberNameMemberUnitList.ContainsKey(ChatLogActionData.SourceName))
                    {
                        this.MemberNameMemberUnitList[ChatLogActionData.SourceName].AddHeal(ChatLogActionData);
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

        /* 計測時間 */

        /// <summary>
        /// 計測時間の1秒間隔のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculationTimer_Elapsed(object sender, EventArgs e)
        {
            this.CalculationRemainingTime -= 1;
            this.CalculationRemainingTimeLabel.Text = this.CalculationRemainingTime.ToString();

            if (this.CalculationRemainingTime <= 0)
            {
                this.StopCalculationThread();
                this.StopCalculationTimer();
            }
        }

        /// <summary>
        /// 計測時間の値変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculationTimerMinutesNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            this.CalculationRemainingTime = (int)this.CalculationTimerMinutesNumericUpDown.Value * 60;
            this.CalculationRemainingTimeLabel.Text = this.CalculationRemainingTime.ToString();
        }

        /// <summary>
        /// 計測停止
        /// </summary>
        private void StopCalculationTimer()
        {
            if (this.CalculationTimer.Enabled)
            {
                this.CalculationTimer.Stop();
                this.CalculationRemainingTime = (int)this.CalculationTimerMinutesNumericUpDown.Value * 60;
                this.CalculationRemainingTimeLabel.Text = this.CalculationRemainingTime.ToString();
            }
        }

        /* ログファイルからの計測 */

        /// <summary>
        /// ログファイルから計測イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculationFromLogFileButton_Click(object sender, EventArgs e)
        {
            this.StartCalculationThread(true);
        }

        /// <summary>
        /// ログファイルから測定終了
        /// </summary>
        public void StopCalculationFromLogFile()
        {
            this.StopCalculationThread();
        }

        /* その他 */

        private void SaveImageButton_Click(object sender, EventArgs e)
        {
            string SaveResultDirectory = Registry.ReadSaveResultDirectory();

            if (String.IsNullOrEmpty(SaveResultDirectory))
            {
                SaveResultDirectory = this.ApplicationDirectory;
                Registry.WriteSaveResultDirectory(this.ApplicationDirectory);
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

                    Registry.WriteSaveResultDirectory(Path.GetDirectoryName(SkillListSaveFileDialog.FileName) + "\\");
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

            if (this.MemberNameMemberUnitList != null && this.MemberNameMemberUnitList.Count > 0 && this.AttackSkillUnitList != null && this.AttackSkillUnitList.Count > 0)
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

                foreach (SkillUnit _SkillUnit in this.AttackSkillUnitList.Values)
                {
                    string SkillResult = _SkillUnit.GetAttackResult();

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
                Registry.WriteAlwaysOnTop(true);
            }
            else
            {
                this.TopMost = false;
                Registry.WriteAlwaysOnTop(false);
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

        private void ProjectLatestVersionLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.ProjectLatestVersionLinkLabel.Text);
        }

        /* タブ関係 */

        private void MenuTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.MenuTabControl.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    if (this.IsCalculationThreadRunning)
                    {
                        MessageBox.Show("計測を停止してから表示してください。", "注意");
                        this.MenuTabControl.SelectedIndex = 0;
                    }
                    else
                    {
                        this.SetAttackSkillList();
                    }
                    break;
                case 2:
                    if (this.IsCalculationThreadRunning)
                    {
                        MessageBox.Show("計測を停止してから表示してください。", "注意");
                        this.MenuTabControl.SelectedIndex = 0;
                    }
                    else
                    {
                        this.SetHealSkillList();
                    }
                    break;
                default:
                    return;
            }
        }

        /* スキル別ダメージ一覧 */

        private void SetAttackSkillList()
        {
            this.AttackSkillListDataGridView.Rows.Clear();

            if (this.MemberNameMemberUnitList != null && this.MemberNameMemberUnitList.Count >= 1)
            {
                foreach (MemberUnit _MemberUnit in MemberNameMemberUnitList.Values)
                {
                    Dictionary<string, AION.AttackSkill> AttackSkillList = _MemberUnit.GetAttackSkillList();

                    foreach (AION.AttackSkill _Skill in AttackSkillList.Values)
                    {
                        this.AttackSkillListDataGridView.Rows.Add(
                            new string[] {
                                _MemberUnit.GetMemberName(),
                                _Skill.Name,
                                _Skill.Damage.ToString("#,0"),
                                _Skill.AttackNumber.ToString("#,0"),
                                _Skill.MaxDamageWithoutCritical.ToString("#,0"),
                                _Skill.MinDamageWithoutCritical.ToString("#,0"),
                                _Skill.MaxDamageWithCritical.ToString("#,0"),
                                _Skill.MinDamageWithCritical.ToString("#,0"),
                                (_Skill.Damage / _Skill.AttackNumber).ToString("#,0"),
                            }
                        );
                    }
                }
            }

            if (this.AttackSkillUnitList != null && this.AttackSkillUnitList.Count >= 0)
            {
                foreach (SkillUnit _SkillUnit in this.AttackSkillUnitList.Values)
                {
                    if (_SkillUnit.Damage > 0)
                    {
                        this.AttackSkillListDataGridView.Rows.Add(
                            new string[] {
                                _SkillUnit.GetJobName(),
                                _SkillUnit.GetName(),
                                _SkillUnit.Damage.ToString("#,0"),
                                _SkillUnit.AttackNumber.ToString("#,0"),
                                _SkillUnit.MaxDamageWithoutCritical.ToString("#,0"),
                                _SkillUnit.MinDamageWithoutCritical.ToString("#,0"),
                                _SkillUnit.MaxDamageWithCritical.ToString("#,0"),
                                _SkillUnit.MinDamageWithCritical.ToString("#,0"),
                                (_SkillUnit.Damage / _SkillUnit.AttackNumber).ToString("#,0"),
                            }
                        );
                    }
                }
            }
        }

        private void SaveAttackSkillListImageButton_Click(object sender, EventArgs e)
        {
            string SaveResultDirectory = Registry.ReadSaveResultDirectory();

            if (String.IsNullOrEmpty(SaveResultDirectory))
            {
                SaveResultDirectory = this.ApplicationDirectory;
                Registry.WriteSaveResultDirectory(this.ApplicationDirectory);
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
                    this.AttackSkillListDataGridView.CurrentCell = null;
                    int OriginalWidth = this.AttackSkillListDataGridView.Width;
                    int OriginalHeight = this.AttackSkillListDataGridView.Height;

                    int ScreenShotWidth = this.AttackSkillListDataGridView.Columns.GetColumnsWidth(DataGridViewElementStates.None);
                    int ScreenShotHeight = this.AttackSkillListDataGridView.Rows.GetRowsHeight(DataGridViewElementStates.None) + this.AttackSkillListDataGridView.Rows[0].Height;
                    this.AttackSkillListDataGridView.ScrollBars = ScrollBars.None;
                    this.AttackSkillListDataGridView.Width = ScreenShotWidth;
                    this.AttackSkillListDataGridView.Height = ScreenShotHeight;

                    Bitmap SkillListBitmap = new Bitmap(ScreenShotWidth, ScreenShotHeight);
                    this.AttackSkillListDataGridView.DrawToBitmap(SkillListBitmap, new Rectangle(0, 0, ScreenShotWidth, ScreenShotHeight));
                    SkillListBitmap.Save(SkillListSaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);

                    this.AttackSkillListDataGridView.Width = OriginalWidth;
                    this.AttackSkillListDataGridView.Height = OriginalHeight;
                    this.AttackSkillListDataGridView.ScrollBars = ScrollBars.Both;

                    Registry.WriteSaveResultDirectory(Path.GetDirectoryName(SkillListSaveFileDialog.FileName) + "\\");
                }
            }
            catch
            {
            }
        }

        /* スキル別ヒール一覧 */

        private void SetHealSkillList()
        {
            this.HealSkillListDataGridView.Rows.Clear();

            if (this.MemberNameMemberUnitList != null && this.MemberNameMemberUnitList.Count >= 1)
            {
                foreach (MemberUnit _MemberUnit in MemberNameMemberUnitList.Values)
                {
                    Dictionary<string, AION.HealSkill> HealSkillList = _MemberUnit.GetHealSkillList();

                    foreach (AION.HealSkill _Skill in HealSkillList.Values)
                    {
                        this.HealSkillListDataGridView.Rows.Add(
                            new string[] {
                                _MemberUnit.GetMemberName(),
                                _Skill.Name,
                                _Skill.HealingAmount.ToString("#,0"),
                            }
                        );
                    }
                }
            }

            if (this.HealSkillUnitList != null && this.HealSkillUnitList.Count >= 0)
            {
                foreach (SkillUnit _SkillUnit in this.HealSkillUnitList.Values)
                {
                    if (_SkillUnit.HealingAmount > 0)
                    {
                        this.HealSkillListDataGridView.Rows.Add(
                            new string[] {
                                _SkillUnit.GetJobName(),
                                _SkillUnit.GetName(),
                                _SkillUnit.HealingAmount.ToString("#,0"),
                            }
                        );
                    }
                }
            }
        }

        private void SaveHealSkillListImageButton_Click(object sender, EventArgs e)
        {
            string SaveResultDirectory = Registry.ReadSaveResultDirectory();

            if (String.IsNullOrEmpty(SaveResultDirectory))
            {
                SaveResultDirectory = this.ApplicationDirectory;
                Registry.WriteSaveResultDirectory(this.ApplicationDirectory);
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
                    this.HealSkillListDataGridView.CurrentCell = null;
                    int OriginalWidth = this.HealSkillListDataGridView.Width;
                    int OriginalHeight = this.HealSkillListDataGridView.Height;

                    int ScreenShotWidth = this.HealSkillListDataGridView.Columns.GetColumnsWidth(DataGridViewElementStates.None);
                    int ScreenShotHeight = this.HealSkillListDataGridView.Rows.GetRowsHeight(DataGridViewElementStates.None) + this.HealSkillListDataGridView.Rows[0].Height;
                    this.HealSkillListDataGridView.ScrollBars = ScrollBars.None;
                    this.HealSkillListDataGridView.Width = ScreenShotWidth;
                    this.HealSkillListDataGridView.Height = ScreenShotHeight;

                    Bitmap SkillListBitmap = new Bitmap(ScreenShotWidth, ScreenShotHeight);
                    this.HealSkillListDataGridView.DrawToBitmap(SkillListBitmap, new Rectangle(0, 0, ScreenShotWidth, ScreenShotHeight));
                    SkillListBitmap.Save(SkillListSaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);

                    this.HealSkillListDataGridView.Width = OriginalWidth;
                    this.HealSkillListDataGridView.Height = OriginalHeight;
                    this.HealSkillListDataGridView.ScrollBars = ScrollBars.Both;

                    Registry.WriteSaveResultDirectory(Path.GetDirectoryName(SkillListSaveFileDialog.FileName) + "\\");
                }
            }
            catch
            {
            }
        }
    }
}
