using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenAIONDPS
{
    public partial class FavoriteMemberListUnit : UserControl
    {
        private a _MainForm = null;

        public FavoriteMemberListUnit()
        {
            InitializeComponent();

            foreach (Control _Control in this.Controls)
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

        public void SetMainForm(a _MainForm)
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

                foreach (Control _Control in this.Controls)
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
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            foreach (Control _Control in this.Controls)
            {
                if (_Control.GetType().Name.Equals("TextBox"))
                {
                    TextBox _TextBox = (TextBox)_Control;
                    this.SavePropertyMemberName(int.Parse((string)_TextBox.Tag) , _TextBox.Text);
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
            foreach (Control _Control in this.Controls)
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
            }

            Properties.Settings.Default.Save();
        }
    }
}
