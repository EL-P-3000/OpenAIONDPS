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
            
            this.InitiallizeComponent2(this.MemberGroup1GroupBox.Controls);
            this.InitiallizeComponent2(this.MemberGroup2GroupBox.Controls);
            this.InitiallizeComponent2(this.MemberGroup3GroupBox.Controls);
            this.InitiallizeComponent2(this.MemberGroup4GroupBox.Controls);
        }

        private void InitiallizeComponent2(ControlCollection _Controls)
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

        private void InsertButton_Click(object sender, EventArgs e)
        {
            string Name = "";
            AION.JobType JobType = AION.JobType.None;

            if (_MainForm != null)
            {
                int ID = int.Parse((string)((Button)sender).Tag);

                ControlCollection _Controls = null;
                if ((ID >= 1 && ID <= 11) || ID == 101)
                {
                    _Controls = this.MemberGroup1GroupBox.Controls;
                }
                else if ((ID >= 12 && ID <= 22) || ID == 102)
                {
                    _Controls = this.MemberGroup2GroupBox.Controls;
                }
                else if ((ID >= 23 && ID <= 33) || ID == 103)
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
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            this.Save(this.MemberGroup1GroupBox.Controls);
            this.Save(this.MemberGroup2GroupBox.Controls);
            this.Save(this.MemberGroup3GroupBox.Controls);
            this.Save(this.MemberGroup4GroupBox.Controls);
        }

        private void Save(ControlCollection Controls)
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

        private string GetRegistryMemberName(int ID)
        {
            try
            {
                return Registry.ReadValue<string>("MemberName" + ID.ToString("D3"));
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
                return Registry.ReadValue<string>("MemberJob" + ID.ToString("D3"));
            }
            catch
            {
                return "";
            }
        }

        private void SaveRegistryMemberName(int ID, string Name)
        {
            try
            {
                Registry.WriteValue("MemberName" + ID.ToString("D3"), Name);
            }
            catch
            {
            }
        }

        private void SaveRegistryMemberJob(int ID, string Job)
        {
            try
            {
                Registry.WriteValue("MemberJob" + ID.ToString("D3"), Job);
            }
            catch
            {
            }
        }
    }
}
