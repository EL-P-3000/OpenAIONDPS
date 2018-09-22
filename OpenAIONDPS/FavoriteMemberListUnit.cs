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
                    _TextBox.Text = GetPropertyMemberName(int.Parse((string)_TextBox.Tag));
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
                    string AAA = GetPropertyMemberJob(int.Parse((string)_ComboBox.Tag));
                    _ComboBox.SelectedValue = AION.GetJobType(GetPropertyMemberJob(int.Parse((string)_ComboBox.Tag)));
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
                    this.SavePropertyMemberName(int.Parse((string)_TextBox.Tag), _TextBox.Text);
                }
                else if (_Control.GetType().Name.Equals("ComboBox"))
                {
                    ComboBox _ComboBox = (ComboBox)_Control;
                    this.SavePropertyMemberJob(int.Parse((string)_ComboBox.Tag), AION.GetJobName((AION.JobType)_ComboBox.SelectedValue));
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
                    this.SavePropertyMemberName(int.Parse((string)_TextBox.Tag), "");
                }
                else if (_Control.GetType().Name.Equals("ComboBox"))
                {
                    ComboBox _ComboBox = (ComboBox)_Control;
                    _ComboBox.SelectedValue = AION.JobType.None;
                    this.SavePropertyMemberJob(int.Parse((string)_ComboBox.Tag), AION.GetJobName(AION.JobType.None));
                }
            }
        }

        private string GetPropertyMemberName(int ID)
        {
            switch (ID)
            {
                case 1:
                    return Properties.Settings.Default.FavoriteMemberName01;
                case 2:
                    return Properties.Settings.Default.FavoriteMemberName02;
                case 3:
                    return Properties.Settings.Default.FavoriteMemberName03;
                case 4:
                    return Properties.Settings.Default.FavoriteMemberName04;
                case 5:
                    return Properties.Settings.Default.FavoriteMemberName05;
                case 6:
                    return Properties.Settings.Default.FavoriteMemberName06;
                case 7:
                    return Properties.Settings.Default.FavoriteMemberName07;
                case 8:
                    return Properties.Settings.Default.FavoriteMemberName08;
                case 9:
                    return Properties.Settings.Default.FavoriteMemberName09;
                case 10:
                    return Properties.Settings.Default.FavoriteMemberName10;
                case 11:
                    return Properties.Settings.Default.FavoriteMemberName11;
                case 12:
                    return Properties.Settings.Default.FavoriteMemberName12;
                case 13:
                    return Properties.Settings.Default.FavoriteMemberName13;
                case 14:
                    return Properties.Settings.Default.FavoriteMemberName14;
                case 15:
                    return Properties.Settings.Default.FavoriteMemberName15;
                case 16:
                    return Properties.Settings.Default.FavoriteMemberName16;
                case 17:
                    return Properties.Settings.Default.FavoriteMemberName17;
                case 18:
                    return Properties.Settings.Default.FavoriteMemberName18;
                case 19:
                    return Properties.Settings.Default.FavoriteMemberName19;
                case 20:
                    return Properties.Settings.Default.FavoriteMemberName20;
                case 21:
                    return Properties.Settings.Default.FavoriteMemberName21;
                case 22:
                    return Properties.Settings.Default.FavoriteMemberName22;
                case 23:
                    return Properties.Settings.Default.FavoriteMemberName23;
                case 24:
                    return Properties.Settings.Default.FavoriteMemberName24;
                case 25:
                    return Properties.Settings.Default.FavoriteMemberName25;
                case 26:
                    return Properties.Settings.Default.FavoriteMemberName26;
                case 27:
                    return Properties.Settings.Default.FavoriteMemberName27;
                case 28:
                    return Properties.Settings.Default.FavoriteMemberName28;
                case 29:
                    return Properties.Settings.Default.FavoriteMemberName29;
                case 30:
                    return Properties.Settings.Default.FavoriteMemberName30;
                case 31:
                    return Properties.Settings.Default.FavoriteMemberName31;
                case 32:
                    return Properties.Settings.Default.FavoriteMemberName32;
                case 33:
                    return Properties.Settings.Default.FavoriteMemberName33;
                case 34:
                    return Properties.Settings.Default.FavoriteMemberName34;
                case 35:
                    return Properties.Settings.Default.FavoriteMemberName35;
                case 36:
                    return Properties.Settings.Default.FavoriteMemberName36;
                case 37:
                    return Properties.Settings.Default.FavoriteMemberName37;
                case 38:
                    return Properties.Settings.Default.FavoriteMemberName38;
                case 39:
                    return Properties.Settings.Default.FavoriteMemberName39;
                case 40:
                    return Properties.Settings.Default.FavoriteMemberName40;
                case 41:
                    return Properties.Settings.Default.FavoriteMemberName41;
                case 42:
                    return Properties.Settings.Default.FavoriteMemberName42;
                case 43:
                    return Properties.Settings.Default.FavoriteMemberName43;
                case 44:
                    return Properties.Settings.Default.FavoriteMemberName44;
                case 45:
                    return Properties.Settings.Default.FavoriteMemberName45;
                case 46:
                    return Properties.Settings.Default.FavoriteMemberName46;
                case 47:
                    return Properties.Settings.Default.FavoriteMemberName47;
                case 48:
                    return Properties.Settings.Default.FavoriteMemberName48;
                default:
                    return "";
            }
        }

        private string GetPropertyMemberJob(int ID)
        {
            switch (ID)
            {
                case 1:
                    return Properties.Settings.Default.FavoriteMemberJob01;
                case 2:
                    return Properties.Settings.Default.FavoriteMemberJob02;
                case 3:
                    return Properties.Settings.Default.FavoriteMemberJob03;
                case 4:
                    return Properties.Settings.Default.FavoriteMemberJob04;
                case 5:
                    return Properties.Settings.Default.FavoriteMemberJob05;
                case 6:
                    return Properties.Settings.Default.FavoriteMemberJob06;
                case 7:
                    return Properties.Settings.Default.FavoriteMemberJob07;
                case 8:
                    return Properties.Settings.Default.FavoriteMemberJob08;
                case 9:
                    return Properties.Settings.Default.FavoriteMemberJob09;
                case 10:
                    return Properties.Settings.Default.FavoriteMemberJob10;
                case 11:
                    return Properties.Settings.Default.FavoriteMemberJob11;
                case 12:
                    return Properties.Settings.Default.FavoriteMemberJob12;
                case 13:
                    return Properties.Settings.Default.FavoriteMemberJob13;
                case 14:
                    return Properties.Settings.Default.FavoriteMemberJob14;
                case 15:
                    return Properties.Settings.Default.FavoriteMemberJob15;
                case 16:
                    return Properties.Settings.Default.FavoriteMemberJob16;
                case 17:
                    return Properties.Settings.Default.FavoriteMemberJob17;
                case 18:
                    return Properties.Settings.Default.FavoriteMemberJob18;
                case 19:
                    return Properties.Settings.Default.FavoriteMemberJob19;
                case 20:
                    return Properties.Settings.Default.FavoriteMemberJob20;
                case 21:
                    return Properties.Settings.Default.FavoriteMemberJob21;
                case 22:
                    return Properties.Settings.Default.FavoriteMemberJob22;
                case 23:
                    return Properties.Settings.Default.FavoriteMemberJob23;
                case 24:
                    return Properties.Settings.Default.FavoriteMemberJob24;
                case 25:
                    return Properties.Settings.Default.FavoriteMemberJob25;
                case 26:
                    return Properties.Settings.Default.FavoriteMemberJob26;
                case 27:
                    return Properties.Settings.Default.FavoriteMemberJob27;
                case 28:
                    return Properties.Settings.Default.FavoriteMemberJob28;
                case 29:
                    return Properties.Settings.Default.FavoriteMemberJob29;
                case 30:
                    return Properties.Settings.Default.FavoriteMemberJob30;
                case 31:
                    return Properties.Settings.Default.FavoriteMemberJob31;
                case 32:
                    return Properties.Settings.Default.FavoriteMemberJob32;
                case 33:
                    return Properties.Settings.Default.FavoriteMemberJob33;
                case 34:
                    return Properties.Settings.Default.FavoriteMemberJob34;
                case 35:
                    return Properties.Settings.Default.FavoriteMemberJob35;
                case 36:
                    return Properties.Settings.Default.FavoriteMemberJob36;
                case 37:
                    return Properties.Settings.Default.FavoriteMemberJob37;
                case 38:
                    return Properties.Settings.Default.FavoriteMemberJob38;
                case 39:
                    return Properties.Settings.Default.FavoriteMemberJob39;
                case 40:
                    return Properties.Settings.Default.FavoriteMemberJob40;
                case 41:
                    return Properties.Settings.Default.FavoriteMemberJob41;
                case 42:
                    return Properties.Settings.Default.FavoriteMemberJob42;
                case 43:
                    return Properties.Settings.Default.FavoriteMemberJob43;
                case 44:
                    return Properties.Settings.Default.FavoriteMemberJob44;
                case 45:
                    return Properties.Settings.Default.FavoriteMemberJob45;
                case 46:
                    return Properties.Settings.Default.FavoriteMemberJob46;
                case 47:
                    return Properties.Settings.Default.FavoriteMemberJob47;
                case 48:
                    return Properties.Settings.Default.FavoriteMemberJob48;
                default:
                    return "";
            }
        }

        private void SavePropertyMemberName(int ID, string Name)
        {
            switch (ID)
            {
                case 1:
                    Properties.Settings.Default.FavoriteMemberName01 = Name;
                    break;
                case 2:
                    Properties.Settings.Default.FavoriteMemberName02 = Name;
                    break;
                case 3:
                    Properties.Settings.Default.FavoriteMemberName03 = Name;
                    break;
                case 4:
                    Properties.Settings.Default.FavoriteMemberName04 = Name;
                    break;
                case 5:
                    Properties.Settings.Default.FavoriteMemberName05 = Name;
                    break;
                case 6:
                    Properties.Settings.Default.FavoriteMemberName06 = Name;
                    break;
                case 7:
                    Properties.Settings.Default.FavoriteMemberName07 = Name;
                    break;
                case 8:
                    Properties.Settings.Default.FavoriteMemberName08 = Name;
                    break;
                case 9:
                    Properties.Settings.Default.FavoriteMemberName09 = Name;
                    break;
                case 10:
                    Properties.Settings.Default.FavoriteMemberName10 = Name;
                    break;
                case 11:
                    Properties.Settings.Default.FavoriteMemberName11 = Name;
                    break;
                case 12:
                    Properties.Settings.Default.FavoriteMemberName12 = Name;
                    break;
                case 13:
                    Properties.Settings.Default.FavoriteMemberName13 = Name;
                    break;
                case 14:
                    Properties.Settings.Default.FavoriteMemberName14 = Name;
                    break;
                case 15:
                    Properties.Settings.Default.FavoriteMemberName15 = Name;
                    break;
                case 16:
                    Properties.Settings.Default.FavoriteMemberName16 = Name;
                    break;
                case 17:
                    Properties.Settings.Default.FavoriteMemberName17 = Name;
                    break;
                case 18:
                    Properties.Settings.Default.FavoriteMemberName18 = Name;
                    break;
                case 19:
                    Properties.Settings.Default.FavoriteMemberName19 = Name;
                    break;
                case 20:
                    Properties.Settings.Default.FavoriteMemberName20 = Name;
                    break;
                case 21:
                    Properties.Settings.Default.FavoriteMemberName21 = Name;
                    break;
                case 22:
                    Properties.Settings.Default.FavoriteMemberName22 = Name;
                    break;
                case 23:
                    Properties.Settings.Default.FavoriteMemberName23 = Name;
                    break;
                case 24:
                    Properties.Settings.Default.FavoriteMemberName24 = Name;
                    break;
                case 25:
                    Properties.Settings.Default.FavoriteMemberName25 = Name;
                    break;
                case 26:
                    Properties.Settings.Default.FavoriteMemberName26 = Name;
                    break;
                case 27:
                    Properties.Settings.Default.FavoriteMemberName27 = Name;
                    break;
                case 28:
                    Properties.Settings.Default.FavoriteMemberName28 = Name;
                    break;
                case 29:
                    Properties.Settings.Default.FavoriteMemberName29 = Name;
                    break;
                case 30:
                    Properties.Settings.Default.FavoriteMemberName30 = Name;
                    break;
                case 31:
                    Properties.Settings.Default.FavoriteMemberName31 = Name;
                    break;
                case 32:
                    Properties.Settings.Default.FavoriteMemberName32 = Name;
                    break;
                case 33:
                    Properties.Settings.Default.FavoriteMemberName33 = Name;
                    break;
                case 34:
                    Properties.Settings.Default.FavoriteMemberName34 = Name;
                    break;
                case 35:
                    Properties.Settings.Default.FavoriteMemberName35 = Name;
                    break;
                case 36:
                    Properties.Settings.Default.FavoriteMemberName36 = Name;
                    break;
                case 37:
                    Properties.Settings.Default.FavoriteMemberName37 = Name;
                    break;
                case 38:
                    Properties.Settings.Default.FavoriteMemberName38 = Name;
                    break;
                case 39:
                    Properties.Settings.Default.FavoriteMemberName39 = Name;
                    break;
                case 40:
                    Properties.Settings.Default.FavoriteMemberName40 = Name;
                    break;
                case 41:
                    Properties.Settings.Default.FavoriteMemberName41 = Name;
                    break;
                case 42:
                    Properties.Settings.Default.FavoriteMemberName42 = Name;
                    break;
                case 43:
                    Properties.Settings.Default.FavoriteMemberName43 = Name;
                    break;
                case 44:
                    Properties.Settings.Default.FavoriteMemberName44 = Name;
                    break;
                case 45:
                    Properties.Settings.Default.FavoriteMemberName45 = Name;
                    break;
                case 46:
                    Properties.Settings.Default.FavoriteMemberName46 = Name;
                    break;
                case 47:
                    Properties.Settings.Default.FavoriteMemberName47 = Name;
                    break;
                case 48:
                    Properties.Settings.Default.FavoriteMemberName48 = Name;
                    break;
            }

            Properties.Settings.Default.Save();
        }

        private void SavePropertyMemberJob(int ID, string Job)
        {
            switch (ID)
            {
                case 1:
                    Properties.Settings.Default.FavoriteMemberJob01 = Job;
                    break;
                case 2:
                    Properties.Settings.Default.FavoriteMemberJob02 = Job;
                    break;
                case 3:
                    Properties.Settings.Default.FavoriteMemberJob03 = Job;
                    break;
                case 4:
                    Properties.Settings.Default.FavoriteMemberJob04 = Job;
                    break;
                case 5:
                    Properties.Settings.Default.FavoriteMemberJob05 = Job;
                    break;
                case 6:
                    Properties.Settings.Default.FavoriteMemberJob06 = Job;
                    break;
                case 7:
                    Properties.Settings.Default.FavoriteMemberJob07 = Job;
                    break;
                case 8:
                    Properties.Settings.Default.FavoriteMemberJob08 = Job;
                    break;
                case 9:
                    Properties.Settings.Default.FavoriteMemberJob09 = Job;
                    break;
                case 10:
                    Properties.Settings.Default.FavoriteMemberJob10 = Job;
                    break;
                case 11:
                    Properties.Settings.Default.FavoriteMemberJob11 = Job;
                    break;
                case 12:
                    Properties.Settings.Default.FavoriteMemberJob12 = Job;
                    break;
                case 13:
                    Properties.Settings.Default.FavoriteMemberJob13 = Job;
                    break;
                case 14:
                    Properties.Settings.Default.FavoriteMemberJob14 = Job;
                    break;
                case 15:
                    Properties.Settings.Default.FavoriteMemberJob15 = Job;
                    break;
                case 16:
                    Properties.Settings.Default.FavoriteMemberJob16 = Job;
                    break;
                case 17:
                    Properties.Settings.Default.FavoriteMemberJob17 = Job;
                    break;
                case 18:
                    Properties.Settings.Default.FavoriteMemberJob18 = Job;
                    break;
                case 19:
                    Properties.Settings.Default.FavoriteMemberJob19 = Job;
                    break;
                case 20:
                    Properties.Settings.Default.FavoriteMemberJob20 = Job;
                    break;
                case 21:
                    Properties.Settings.Default.FavoriteMemberJob21 = Job;
                    break;
                case 22:
                    Properties.Settings.Default.FavoriteMemberJob22 = Job;
                    break;
                case 23:
                    Properties.Settings.Default.FavoriteMemberJob23 = Job;
                    break;
                case 24:
                    Properties.Settings.Default.FavoriteMemberJob24 = Job;
                    break;
                case 25:
                    Properties.Settings.Default.FavoriteMemberJob25 = Job;
                    break;
                case 26:
                    Properties.Settings.Default.FavoriteMemberJob26 = Job;
                    break;
                case 27:
                    Properties.Settings.Default.FavoriteMemberJob27 = Job;
                    break;
                case 28:
                    Properties.Settings.Default.FavoriteMemberJob28 = Job;
                    break;
                case 29:
                    Properties.Settings.Default.FavoriteMemberJob29 = Job;
                    break;
                case 30:
                    Properties.Settings.Default.FavoriteMemberJob30 = Job;
                    break;
                case 31:
                    Properties.Settings.Default.FavoriteMemberJob31 = Job;
                    break;
                case 32:
                    Properties.Settings.Default.FavoriteMemberJob32 = Job;
                    break;
                case 33:
                    Properties.Settings.Default.FavoriteMemberJob33 = Job;
                    break;
                case 34:
                    Properties.Settings.Default.FavoriteMemberJob34 = Job;
                    break;
                case 35:
                    Properties.Settings.Default.FavoriteMemberJob35 = Job;
                    break;
                case 36:
                    Properties.Settings.Default.FavoriteMemberJob36 = Job;
                    break;
                case 37:
                    Properties.Settings.Default.FavoriteMemberJob37 = Job;
                    break;
                case 38:
                    Properties.Settings.Default.FavoriteMemberJob38 = Job;
                    break;
                case 39:
                    Properties.Settings.Default.FavoriteMemberJob39 = Job;
                    break;
                case 40:
                    Properties.Settings.Default.FavoriteMemberJob40 = Job;
                    break;
                case 41:
                    Properties.Settings.Default.FavoriteMemberJob41 = Job;
                    break;
                case 42:
                    Properties.Settings.Default.FavoriteMemberJob42 = Job;
                    break;
                case 43:
                    Properties.Settings.Default.FavoriteMemberJob43 = Job;
                    break;
                case 44:
                    Properties.Settings.Default.FavoriteMemberJob44 = Job;
                    break;
                case 45:
                    Properties.Settings.Default.FavoriteMemberJob45 = Job;
                    break;
                case 46:
                    Properties.Settings.Default.FavoriteMemberJob46 = Job;
                    break;
                case 47:
                    Properties.Settings.Default.FavoriteMemberJob47 = Job;
                    break;
                case 48:
                    Properties.Settings.Default.FavoriteMemberJob48 = Job;
                    break;
            }

            Properties.Settings.Default.Save();
        }
    }
}
