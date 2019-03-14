using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenAIONDPS
{
    public partial class FavoriteMemberListUnit : UserControl
    {
        private MainForm _MainForm = null;

        public FavoriteMemberListUnit()
        {
            InitializeComponent();

            this.InitiallizeComponentOwnName(this.OwnNameListGroupBox.Controls);
            this.InitiallizeComponentMember(this.MemberGroup1GroupBox.Controls);
            this.InitiallizeComponentMember(this.MemberGroup2GroupBox.Controls);
            this.InitiallizeComponentMember(this.MemberGroup3GroupBox.Controls);
            this.InitiallizeComponentMember(this.MemberGroup4GroupBox.Controls);
        }

        private void InitiallizeComponentOwnName(ControlCollection _Controls)
        {
            foreach (Control _Control in _Controls)
            {
                if (_Control.GetType().Name.Equals("TextBox"))
                {
                    TextBox _TextBox = (TextBox)_Control;
                    _TextBox.Text = this.GetRegistryOwnName(int.Parse((string)_TextBox.Tag));
                }
            }
        }

        private void InitiallizeComponentMember(ControlCollection _Controls)
        {
            foreach (Control _Control in _Controls)
            {
                if (_Control.GetType().Name.Equals("Button"))
                {
                    if (((Button)_Control).Name.IndexOf("Insert") == 0)
                    {
                        ((Button)_Control).Click += new EventHandler(InsertButton_Click);
                    }
                }
                else if (_Control.GetType().Name.Equals("TextBox"))
                {
                    TextBox _TextBox = (TextBox)_Control;
                    _TextBox.Text = this.GetRegistryMemberName(int.Parse((string)_TextBox.Tag));
                }
                else if (_Control.GetType().Name.Equals("ComboBox"))
                {
                    List<AION.JobSet> JobSetList = new List<AION.JobSet>();
                    foreach (AION.JobType Job in Enum.GetValues(typeof(AION.JobType)))
                    {
                        JobSetList.Add(new AION.JobSet(Job, AION.GetJobName(Job)));
                    }

                    ComboBox _ComboBox = (ComboBox)_Control;
                    _ComboBox.Items.Clear();
                    _ComboBox.DataSource = JobSetList;
                    _ComboBox.DisplayMember = "Name";
                    _ComboBox.ValueMember = "Type";
                    try
                    {
                        _ComboBox.SelectedValue = AION.GetJobType(this.GetRegistryMemberJob(int.Parse((string)_ComboBox.Tag)));
                    }
                    catch
                    {
                        _ComboBox.SelectedValue = AION.JobType.None;
                    }
                }
            }
        }

        public void SetMainForm(MainForm _MainForm)
        {
            this._MainForm = _MainForm;
        }

        public LinkedList<string> GetOwnNameList()
        {
            LinkedList<string> OwnNameList = new LinkedList<string>();

            foreach (Control _Control in this.OwnNameListGroupBox.Controls)
            {
                if (_Control.GetType().Name.Equals("TextBox"))
                {
                    TextBox _TextBox = (TextBox)_Control;
                    OwnNameList.AddLast(_TextBox.Text);
                }
            }

            return OwnNameList;
        }

        private void InsertButton_Click(object sender, EventArgs e)
        {
            string Name = "";
            AION.JobType JobType = AION.JobType.None;

            if (_MainForm != null)
            {
                int ID = int.Parse((string)((Button)sender).Tag);

                ControlCollection _Controls = null;
                if ((ID >= 1 && ID <= 11) || (ID >= 49 && ID <= 54) || ID == 101)
                {
                    _Controls = this.MemberGroup1GroupBox.Controls;
                }
                else if ((ID >= 12 && ID <= 22) || (ID >= 55 && ID <= 60) || ID == 102)
                {
                    _Controls = this.MemberGroup2GroupBox.Controls;
                }
                else if ((ID >= 23 && ID <= 33) || (ID >= 61 && ID <= 66) || ID == 103)
                {
                    _Controls = this.MemberGroup3GroupBox.Controls;
                }
                else
                {
                    _Controls = this.MemberGroup4GroupBox.Controls;
                }

                if (ID < 100)
                {
                    foreach (Control _Control in _Controls)
                    {
                        if (_Control.GetType().Name.Equals("TextBox"))
                        {
                            if (int.Parse((string)((TextBox)_Control).Tag) == ID)
                            {
                                Name = ((TextBox)_Control).Text;
                            }
                        }
                        else if (_Control.GetType().Name.Equals("ComboBox"))
                        {
                            if (int.Parse((string)((ComboBox)_Control).Tag) == ID)
                            {
                                JobType = (AION.JobType)((ComboBox)_Control).SelectedValue;
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(Name))
                    {
                        _MainForm.InsertMember(Name, JobType);
                    }
                }
                else if (ID >= 101 && ID < 200)
                {
                    int MinID = 11 * (ID - 101) + 1;
                    int MaxID = MinID + 11;

                    for (int i = MinID; i <= MaxID; i++)
                    {
                        foreach (Control _Control in _Controls)
                        {
                            if (_Control.GetType().Name.Equals("TextBox"))
                            {
                                if (int.Parse((string)((TextBox)_Control).Tag) == i)
                                {
                                    Name = ((TextBox)_Control).Text;
                                }
                            }
                            else if (_Control.GetType().Name.Equals("ComboBox"))
                            {
                                if (int.Parse((string)((ComboBox)_Control).Tag) == i)
                                {
                                    JobType = (AION.JobType)((ComboBox)_Control).SelectedValue;
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(Name))
                        {
                            _MainForm.InsertMember(Name, JobType);
                        }
                    }

                    if (ID == 101)
                    {
                        MinID = 49;
                        MaxID = 54;
                    }
                    else if (ID == 102)
                    {
                        MinID = 55;
                        MaxID = 60;
                    }
                    else if (ID == 103)
                    {
                        MinID = 61;
                        MaxID = 66;
                    }

                    for (int i = MinID; i <= MaxID; i++)
                    {
                        foreach (Control _Control in _Controls)
                        {
                            if (_Control.GetType().Name.Equals("TextBox"))
                            {
                                if (int.Parse((string)((TextBox)_Control).Tag) == i)
                                {
                                    Name = ((TextBox)_Control).Text;
                                }
                            }
                            else if (_Control.GetType().Name.Equals("ComboBox"))
                            {
                                if (int.Parse((string)((ComboBox)_Control).Tag) == i)
                                {
                                    JobType = (AION.JobType)((ComboBox)_Control).SelectedValue;
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(Name))
                        {
                            _MainForm.InsertMember(Name, JobType);
                        }
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            this.SaveOwnName(this.OwnNameListGroupBox.Controls);
            this.SaveMember(this.MemberGroup1GroupBox.Controls);
            this.SaveMember(this.MemberGroup2GroupBox.Controls);
            this.SaveMember(this.MemberGroup3GroupBox.Controls);
            this.SaveMember(this.MemberGroup4GroupBox.Controls);
        }

        private void SaveOwnName(ControlCollection Controls)
        {
            foreach (Control _Control in Controls)
            {
                if (_Control.GetType().Name.Equals("TextBox"))
                {
                    TextBox _TextBox = (TextBox)_Control;
                    this.SaveRegistryOwnName(int.Parse((string)_TextBox.Tag), _TextBox.Text);
                }
            }
        }

        private void SaveMember(ControlCollection Controls)
        {
            foreach (Control _Control in Controls)
            {
                if (_Control.GetType().Name.Equals("TextBox"))
                {
                    TextBox _TextBox = (TextBox)_Control;
                    this.SaveRegistryMemberName(int.Parse((string)_TextBox.Tag), _TextBox.Text);
                }
                else if (_Control.GetType().Name.Equals("ComboBox"))
                {
                    ComboBox _ComboBox = (ComboBox)_Control;
                    this.SaveRegistryMemberJob(int.Parse((string)_ComboBox.Tag), AION.GetJobName((AION.JobType)_ComboBox.SelectedValue));
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            this.Clear(this.OwnNameListGroupBox.Controls);
            this.Clear(this.MemberGroup1GroupBox.Controls);
            this.Clear(this.MemberGroup2GroupBox.Controls);
            this.Clear(this.MemberGroup3GroupBox.Controls);
            this.Clear(this.MemberGroup4GroupBox.Controls);
        }

        private void Clear(ControlCollection Controls)
        {
            foreach (Control _Control in Controls)
            {
                if (_Control.GetType().Name.Equals("TextBox"))
                {
                    TextBox _TextBox = (TextBox)_Control;
                    _TextBox.Text = "";
                }
                else if (_Control.GetType().Name.Equals("ComboBox"))
                {
                    ComboBox _ComboBox = (ComboBox)_Control;
                    _ComboBox.SelectedValue = AION.JobType.None;
                }
            }

            Registry.ClearFavoriteMember();
        }

        private string GetRegistryOwnName(int ID)
        {
            try
            {
                return Registry.ReadOwnName(ID);
            }
            catch
            {
                return "";
            }
        }

        private string GetRegistryMemberName(int ID)
        {
            try
            {
                return Registry.ReadMemberName(ID);
            }
            catch
            {
                return "";
            }
        }

        private string GetRegistryMemberJob(int ID)
        {
            try
            {
                return Registry.ReadMemberJob(ID);
            }
            catch
            {
                return "";
            }
        }

        private void SaveRegistryOwnName(int ID, string Name)
        {
            try
            {
                Registry.WriteOwnName(ID, Name);
            }
            catch
            {
            }
        }

        private void SaveRegistryMemberName(int ID, string Name)
        {
            try
            {
                Registry.WriteMemberName(ID, Name);
            }
            catch
            {
            }
        }

        private void SaveRegistryMemberJob(int ID, string Job)
        {
            try
            {
                Registry.WriteMemberJob(ID, Job);
            }
            catch
            {
            }
        }
    }
}
